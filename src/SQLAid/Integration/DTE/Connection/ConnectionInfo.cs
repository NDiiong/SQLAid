using Microsoft.SqlServer.Management.Smo.RegSvrEnum;

namespace SQLAid.Integration.DTE.Connection
{
    public class ConnectionInfo
    {
        public string ConnectionString { get; set; }
        public string ServerName { get; set; }
        public string Database { get; set; }
        public string ColorKey => $"{ServerName}/{Database}";
        public UIConnectionInfo ActiveConnectionInfo { get; set; }
    }
}