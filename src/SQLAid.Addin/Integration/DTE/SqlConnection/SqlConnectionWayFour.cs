using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;
using SQLAid.Addin.Extension;
using System.Data;
using System.Reflection;

namespace SQLAid.Integration.DTE.SqlConnection
{
    public class SqlConnectionWayFour : ISqlConnection
    {
        public SqlConnectionInfo GetCurrentSqlConnection()
        {
            var sqlScriptEditorControl = GetCurrentlyActiveFrameDocView();
            var connectionString = sqlScriptEditorControl.GetNonPublicField("m_connection").As<IDbConnection>().ConnectionString;

            var sqlConnection = new System.Data.SqlClient.SqlConnection(connectionString);
            var serverConnection = new ServerConnection(sqlConnection);
            return new SqlConnectionInfo(serverConnection, ConnectionType.Sql);
        }

        private SqlScriptEditorControl GetCurrentlyActiveFrameDocView()
        {
            const BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;

            var scriptFactor = ServiceCache.ScriptFactory;
            var monitorSelection = ServiceCache.VSMonitorSelection;

            var sqlScriptEditorControl = ServiceCache.ScriptFactory
                .GetType()
                .GetMethod("GetCurrentlyActiveFrameDocView", bindingFlags)
                .Invoke(scriptFactor, new object[] { monitorSelection, false, null });

            return sqlScriptEditorControl.As<SqlScriptEditorControl>();
        }
    }
}