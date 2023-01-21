using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.ConnectionDlg;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using SQLAid.Integration.Exceptions;

namespace SQLAid.Integration.DTE.SqlConnection
{
    public class SqlConnectionWayTwo : ISqlConnection
    {
        public SqlConnectionInfo GetCurrentSqlConnection()
        {
            var connectionInfo = ServiceCache.ScriptFactory.CurrentlyActiveWndConnectionInfo;
            var databaseName = connectionInfo.UIConnectionInfo.AdvancedOptions["DATABASE"];

            if (string.IsNullOrEmpty(databaseName))
                throw new ConnectionInfoException("No database context");

            var connectionBase = UIConnectionInfoUtil.GetCoreConnectionInfo(connectionInfo.UIConnectionInfo);

            var sqlConnectionInfo = (SqlConnectionInfo)connectionBase;
            sqlConnectionInfo.DatabaseName = databaseName;

            return sqlConnectionInfo;
        }
    }
}