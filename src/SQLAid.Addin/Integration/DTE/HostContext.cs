using EnvDTE;
using EnvDTE80;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.ConnectionDlg;
using Microsoft.SqlServer.Management.UI.Grid;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;
using SQLAid.Integration.Exceptions;
using System;
using System.Data.SqlClient;
using System.Reflection;

namespace SQLAid.Integration.DTE
{
    public class HostContext : IHostContext
    {
        public DTE2 _app;

        public HostContext(DTE2 app)
        {
            _app = app;
        }

        public IEditor GetCurrentEditor()
        {
            var activeDocument = _app.ActiveDocument;
            if (activeDocument == null)
                return null;

            var document = (TextDocument)activeDocument.Object("");
            return new Editor(document);
        }

        public IEditor GetNewEditor()
        {
            ServiceCache.ScriptFactory.CreateNewBlankScript(ScriptType.Sql);
            return GetCurrentEditor();
        }

        public IResultGrid GetFocusedResultGrid()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            var scriptFactor = ServiceCache.ScriptFactory;
            var monitorSelection = ServiceCache.VSMonitorSelection;

            var editorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            var resultControlField = editorControl.GetType()
                .GetField("m_sqlResultsControl", bindingFlags);

            if (resultControlField == null)
                return null;

            var resultsControl = resultControlField.GetValue(editorControl);

            var resultsTabPage = resultsControl
                .GetType()
                .GetField("m_gridResultsPage", bindingFlags)
                .GetValue(resultsControl);

            var grid = (IGridControl)resultsTabPage
                .GetType()
                .BaseType
                .GetProperty("FocusedGrid", bindingFlags)
                .GetValue(resultsTabPage, null);

            if (grid == null)
                return null;

            return new ResultGrid(grid);
        }

        public IServerConnection CloneCurrentConnection(string database)
        {
            var connectionInfo = GetCurrentSqlConnectionInfo(database);
            return new HostServerConnection(connectionInfo);
        }

        public string GetCurrentConnectionString()
        {
            var connectionInfo = GetCurrentSqlConnectionInfo(null);
            return connectionInfo.ConnectionString;
        }

        public string GetCurrentConnectionString(UIConnectionInfo uIConnectionInfo)
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = uIConnectionInfo.ServerName,
                IntegratedSecurity = string.IsNullOrEmpty(uIConnectionInfo.Password),
                Password = uIConnectionInfo.Password,
                UserID = uIConnectionInfo.UserName,
                InitialCatalog = uIConnectionInfo.AdvancedOptions["DATABASE"] ?? "master",
                ApplicationName = SqlAidAsyncPackage.NAME
            };

            return builder.ToString();
        }

        private SqlConnectionInfo GetCurrentSqlConnectionInfo()
        {
            var connectionInfo = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo;
            var info = connectionInfo.UIConnectionInfo;
            var serverName = info.ServerName;
            var databaseName = info.AdvancedOptions["DATABASE"];
            var blnIsUsingIntegratedSecurity = (0 == info.AuthenticationType);
            var userName = info.UserName;
            var passWord = info.Password;

            var version = info.ServerVersion;
            var buildMajor = 0;
            var buildMinor = 0;
            var buildNumber = 0;
            if (null != version)
            {
                buildMajor = version.Major;
                buildMinor = version.Minor;
                buildNumber = version.BuildNumber;
            }

            if (null == serverName || null == databaseName)
                return null;

            return new SqlConnectionInfo(serverName, userName, passWord);
        }

        private SqlConnectionInfo GetCurrentSqlConnectionInfo(string databaseName)
        {
            var connectionInfo = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo;
            databaseName = databaseName ?? connectionInfo.UIConnectionInfo.AdvancedOptions["DATABASE"];

            if (String.IsNullOrEmpty(databaseName))
                throw new ConnectionInfoException("No database context");

            var connectionBase = UIConnectionInfoUtil.GetCoreConnectionInfo(connectionInfo.UIConnectionInfo);

            var sqlConnectionInfo = (SqlConnectionInfo)connectionBase;
            sqlConnectionInfo.DatabaseName = databaseName;

            return sqlConnectionInfo;
        }

        private SqlConnectionInfo GetCurrentSqlConnectionInfoWay3()
        {
            var nodes = GetObjectExplorerSelectedNodes();

            if (nodes.Length > 0)
                return nodes[0].Connection as SqlConnectionInfo;

            return default;
        }

        private INodeInformation[] GetObjectExplorerSelectedNodes()
        {
            var objExplorer = GetObjectExplorer();
            objExplorer.GetSelectedNodes(out var _, out var nodes);
            return nodes;
        }

        private IObjectExplorerService GetObjectExplorer()
        {
            return (IObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
        }
    }
}