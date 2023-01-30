using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using Microsoft.VisualStudio.TextManager.Interop;
using SQLAid.Integration;
using SQLAid.Integration.DTE;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Commands.TextEditor.Highlighter
{
    public static class HighlightCommand
    {
        private static WindowEvents _windowEvents;
        private static IServiceProvider _serviceProvider;
        private static IWpfTextView _textView;
        private static IFrameDocumentView _frameDocumentView;

        static HighlightCommand()
        {
            _frameDocumentView = new FrameDocumentView();
        }

        public static async Task InitializeAsync(SqlAsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _serviceProvider = package;
            var dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
            _windowEvents = dte.Events.WindowEvents;
            _windowEvents.WindowCreated += _windowEvents_WindowCreated;
        }

        private static void _windowEvents_WindowCreated(EnvDTE.Window Window)
        {
            try
            {
                var vsTextManager = _serviceProvider.GetService(typeof(SVsTextManager)) as IVsTextManager;
                var componentModel = (IComponentModel)_serviceProvider.GetService(typeof(SComponentModel));
                var editor = componentModel.GetService<IVsEditorAdaptersFactoryService>();

                IVsTextView textViewCurrent;
                vsTextManager.GetActiveView(1, null, out textViewCurrent);
                _textView = editor.GetWpfTextView(textViewCurrent);

                AddAdornmentLayer(ref _textView, "SelectionHighlight");
                _textView.Selection.SelectionChanged += Selection_SelectionChanged;
                _textView.LayoutChanged += _textView_LayoutChanged;

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                throw;
            }
        }

        private static void _textView_LayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
        {
            var _layer = _textView.GetAdornmentLayer("SelectionHighlight");
            _layer.RemoveAllAdornments();
            ColorizeSelection();
        }

        private static void Selection_SelectionChanged(object sender, EventArgs e)
        {
            ColorizeSelection();
        }

        private static void ColorizeSelection()
        {
            var wpfTextViewLineCollection = _textView.TextViewLines;
            var textDocument = _frameDocumentView.GetTextDocument();
            var text = textDocument.CreateEditPoint().GetText(textDocument.EndPoint);
            var matchWholeWord = _textView.Selection.IsEmpty;
            var _highlightSpan = SelectionToHighlightSpan();

            var _highlight = _highlightSpan.HasValue ? _highlightSpan.Value.GetText() : null;

            if (_highlight != null)
            {
                var _layer = _textView.GetAdornmentLayer("SelectionHighlight");
                _layer.RemoveAllAdornments();
                var _search = new BackgroundSearch(_textView.TextSnapshot, _highlight, matchWholeWord, default);
                foreach (var item in _search.Matches)
                {
                    var markerGeometry = wpfTextViewLineCollection.GetMarkerGeometry(item);
                    if (markerGeometry != null)
                    {
                        var brush = new SolidColorBrush(Colors.Red);
                        brush.Freeze();

                        var solidColorBrush = new SolidColorBrush(Colors.AliceBlue);
                        solidColorBrush.Freeze();
                        var pen = new System.Windows.Media.Pen(solidColorBrush, 0.5);
                        pen.Freeze();

                        var geometryDrawing = new GeometryDrawing(brush, pen, markerGeometry);
                        geometryDrawing.Freeze();
                        var drawingImage = new DrawingImage(geometryDrawing);
                        drawingImage.Freeze();
                        var image = new System.Windows.Controls.Image();
                        image.Source = drawingImage;
                        Canvas.SetLeft(image, markerGeometry.Bounds.Left);
                        Canvas.SetTop(image, markerGeometry.Bounds.Top);
                        _layer.AddAdornment((AdornmentPositioningBehavior)2, (SnapshotSpan?)item, (object)null, (UIElement)image, (AdornmentRemovedCallback)null);
                    }
                }
                Console.WriteLine();
            }
        }

        private static SnapshotSpan? SelectionToHighlightSpan()
        {
            var selection = _textView.Selection.StreamSelectionSpan.SnapshotSpan;

            if ((selection.Length > 0) && (selection.Length < 128))
            {
                if (_textView.Selection.Mode == TextSelectionMode.Box)
                {
                    ITextViewLine line = _textView.GetTextViewLineContainingBufferPosition(_textView.Selection.ActivePoint.Position);
                    if (!line.ContainsBufferPosition(_textView.Selection.AnchorPoint.Position))
                        return null;
                }

                int end = selection.End;
                for (int i = selection.Start; (i < end); ++i)
                {
                    var c = selection.Snapshot[i];
                    if (!char.IsWhiteSpace(c))
                        return selection;
                }
            }

            return null;
        }

        private static void AddAdornmentLayer(ref IWpfTextView WPFView, string AdornmentLayerName)
        {
            object obj = null;
            IList list = null;
            Dictionary<string, int> dictionary = null;
            UIElementCollection uIElementCollection = null;
            var num = -1;
            Type type = null;
            Type type2 = null;
            UIElement uIElement = null;
            object obj2 = null;
            var num2 = 0;
            try
            {
                obj = GetBaseLayer(ref WPFView);
                list = GetBaseLayerElementsField(ref obj);
                dictionary = GetBaseLayerOrderedViewLayerDefinitionsField(ref obj);
                uIElementCollection = GetBaseLayerChildrenProperty(ref obj);
                if (!dictionary.ContainsKey(AdornmentLayerName))
                {
                    num = (dictionary[AdornmentLayerName] = GetNewAdornmentLayerID(ref dictionary));
                    type = GetAdornmentLayerType(ref obj);
                    type2 = GetUIElementDataType(ref obj);
                    uIElement = CreateAdornmentLayerInstance(type, WPFView, AdornmentLayerName);
                    obj2 = CreateUIElementDataInstance(type2, uIElement, AdornmentLayerName, num);
                    num2 = GetAdornmentLayerInsertIndex(type2, ref list, num);
                    uIElementCollection.Insert(num2, uIElement);
                    list.Insert(num2, obj2);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static UIElementCollection GetBaseLayerChildrenProperty(ref object BaseLayer)
        {
            UIElementCollection uIElementCollection = null;
            try
            {
                return (UIElementCollection)BaseLayer.GetType().GetProperty("Children", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(BaseLayer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static Type GetUIElementDataType(ref object BaseLayer)
        {
            Type type = null;
            try
            {
                return BaseLayer.GetType().Assembly.GetType("Microsoft.VisualStudio.Text.Editor.Implementation.ViewStack+UIElementData");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static int GetNewAdornmentLayerID(ref Dictionary<string, int> OrderedViewLayerDefinitionsField)
        {
            var num = -1;
            var i = -1;
            try
            {
                num = OrderedViewLayerDefinitionsField["Text"];
                for (i = OrderedViewLayerDefinitionsField["SelectionAndProvisionHighlight"]; OrderedViewLayerDefinitionsField.ContainsValue(i) && i <= num; i++)
                {
                }
                if (i > num && i < OrderedViewLayerDefinitionsField.Aggregate((KeyValuePair<string, int> l, KeyValuePair<string, int> r) => (l.Value > r.Value) ? l : r).Value && OrderedViewLayerDefinitionsField.ContainsValue(i))
                {
                    i = OrderedViewLayerDefinitionsField.Aggregate((KeyValuePair<string, int> l, KeyValuePair<string, int> r) => (l.Value > r.Value) ? l : r).Value + 1;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GC.ReRegisterForFinalize(num);
            }
            return i;
        }

        private static Dictionary<string, int> GetBaseLayerOrderedViewLayerDefinitionsField(ref object BaseLayer)
        {
            Dictionary<string, int> dictionary = null;
            try
            {
                return (Dictionary<string, int>)BaseLayer.GetType().GetField("_orderedViewLayerDefinitions", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(BaseLayer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static UIElement CreateAdornmentLayerInstance(Type AdornmentLayerType, IWpfTextView View, string AdornmentLayerName)
        {
            UIElement uIElement = null;
            try
            {
                return (UIElement)CreateInternalObjectInstance(AdornmentLayerType, View, AdornmentLayerName, false);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static IList GetBaseLayerElementsField(ref object BaseLayer)
        {
            IList list = null;
            try
            {
                return (IList)BaseLayer.GetType().GetField("_elements", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(BaseLayer);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object CreateUIElementDataInstance(Type UIElementDataType, UIElement AdornmentLayer, string AdornmentLayerName, int AdornmentLayerID)
        {
            object obj = null;
            try
            {
                return CreateInternalObjectInstance(UIElementDataType, AdornmentLayer, AdornmentLayerName, AdornmentLayerID);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object CreateInternalObjectInstance(Type ObjectType, object FirstValue, object SecondValue, object ThirdValue)
        {
            object obj = null;
            try
            {
                return ObjectType.InvokeMember(ObjectType.Name, BindingFlags.CreateInstance, null, null, new object[3] { FirstValue, SecondValue, ThirdValue });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static int GetAdornmentLayerInsertIndex(Type UIElementDataType, ref IList ElementsField, int AdornmentLayerID)
        {
            var i = 0;
            var num = -1;
            try
            {
                for (; i < ElementsField.Count; i++)
                {
                    num = (int)UIElementDataType.GetField("Rank", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(ElementsField[i]);
                    if (num > AdornmentLayerID)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                GC.ReRegisterForFinalize(num);
            }
            return i;
        }

        private static Type GetAdornmentLayerType(ref object BaseLayer)
        {
            Type type = null;
            try
            {
                return BaseLayer.GetType().Assembly.GetType("Microsoft.VisualStudio.Text.Editor.Implementation.AdornmentLayer");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static object GetBaseLayer(ref IWpfTextView WPFView)
        {
            object obj = null;
            try
            {
                return WPFView.GetType().GetField("_baseLayer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(WPFView);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}