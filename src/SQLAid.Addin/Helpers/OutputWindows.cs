using EnvDTE;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace SQLAid.Helpers
{
    public static class OutputWindows
    {
        public static void Write(string message, string name = "SQLAid")
        {
            var dte = ServiceCache.ExtensibilityModel;
            var outputWindow = (OutputWindow)dte.Windows.Item(Constants.vsWindowKindOutput).Object;
            OutputWindowPane outputWindowPane;
            try
            {
                outputWindowPane = outputWindow.OutputWindowPanes.Item(name);
            }
            catch
            {
                outputWindowPane = outputWindow.OutputWindowPanes.Add(name);
            }

            outputWindow.Parent.AutoHides = false;
            outputWindow.Parent.Activate();
            outputWindowPane.Activate();
            outputWindowPane.OutputString(message + "\r\n");
        }
    }
}