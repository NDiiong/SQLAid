using Microsoft.SqlServer.Management.Common;
using System.Data;

namespace SQLAid.Integration.DTE
{
    internal class HostServerConnection : IServerConnection
    {
        private ServerConnection _srvconn;

        public HostServerConnection(SqlConnectionInfo connectionInfo)
        {
            _srvconn = new ServerConnection(connectionInfo);
        }

        public object Connection => _srvconn;

        public void Connect()
        {
            _srvconn.Connect();
        }

        public void Disconnect()
        {
            _srvconn.Disconnect();
        }

        public void Dispose()
        {
            if (_srvconn.IsOpen)
                _srvconn.Disconnect();
        }

        public IDataReader ExecuteReader(string query)
        {
            return _srvconn.ExecuteReader(query);
        }
    }
}