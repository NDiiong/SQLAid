using Microsoft.SqlServer.Management.Common;

namespace SQLAid.Integration
{
    public interface ISqlConnection
    {
        SqlConnectionInfo GetCurrentSqlConnection();
    }
}