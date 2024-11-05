using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using System.Data.SqlClient;

namespace SQLAid.Integration.DTE.Connection
{
    public class SqlConnection : ISqlConnection
    {
        public ConnectionInfo GetCurrentSqlConnection()
        {
            UIConnectionInfo connection = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo.UIConnectionInfo;

            var databaseName = connection.AdvancedOptions["DATABASE"];
            if (string.IsNullOrEmpty(databaseName))
                databaseName = "master";

            var builder = new SqlConnectionStringBuilder
            {
                DataSource = connection.ServerName,
                IntegratedSecurity = string.IsNullOrEmpty(connection.Password),
                Password = connection.Password,
                UserID = connection.UserName,
                InitialCatalog = databaseName,
                ApplicationName = "SQLAid",
            };

            ConnectionInfo connectionInfo = new ConnectionInfo
            {
                ConnectionString = builder.ToString(),
                Database = databaseName,
                ServerName = connection.ServerName,
                ActiveConnectionInfo = connection
            };

            return connectionInfo;
        }
    }
}