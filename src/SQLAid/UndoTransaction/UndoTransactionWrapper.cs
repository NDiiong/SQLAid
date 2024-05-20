using EnvDTE80;
using System;

namespace SQLAid.UndoTransaction
{
    public class UndoTransactionWrapper : IUndoTransaction
    {
        private readonly DTE2 _application;
        private readonly string _name;

        public UndoTransactionWrapper(DTE2 application, string name)
        {
            _application = application;
            _name = name;
        }

        public void Run(Action action)
        {
            var shouldCloseUndoContext = false;
            if (!_application.UndoContext.IsOpen)
            {
                _application.UndoContext.Open(_name);
                shouldCloseUndoContext = true;
            }

            try
            {
                action();
            }
            catch (Exception ex)
            {
                var message = $"{_name} was stopped";
                _application.StatusBar.Text = $"{message}.  See output window for more details.";

                if (shouldCloseUndoContext)
                {
                    _application.UndoContext.SetAborted();
                    shouldCloseUndoContext = false;
                }
            }
            finally
            {
                // Always close the undo transaction to prevent ongoing interference with the IDE.
                if (shouldCloseUndoContext)
                {
                    _application.UndoContext.Close();
                }
            }
        }

        public void Dispose()
        {
        }
    }
}