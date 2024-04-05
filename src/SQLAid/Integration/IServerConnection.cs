using System.Data;

namespace SQLAid.Integration
{
    public interface IServerConnection
    {
        void Connect();

        void Disconnect();

        IDataReader ExecuteReader(string query);

        object Connection { get; }
    }
}