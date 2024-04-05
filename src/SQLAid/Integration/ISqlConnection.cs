using SQLAid.Integration.DTE.Connection;

namespace SQLAid.Integration
{
    public interface ISqlConnection
    {
        ConnectionInfo GetCurrentSqlConnection();
    }
}