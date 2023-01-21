using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration;

namespace SQLAid.Integration.DTE.SqlConnection
{
    public class SqlConnection : ISqlConnection
    {
        public SqlConnectionInfo GetCurrentSqlConnection()
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
    }
}