using Microsoft.SqlServer.Management.Common;
using SQLAid.Addin.Extension;
using System.Data;

namespace SQLAid.Integration.DTE.Connection
{
    public class SqlConnectionWayFour : ISqlConnection
    {
        private readonly IFrameDocumentView _frameDocumentView;

        public SqlConnectionWayFour(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView;
        }

        public SqlConnectionInfo GetCurrentSqlConnection()
        {
            var sqlScriptEditorControl = _frameDocumentView.GetCurrentlyActiveFrameDocView();
            var dbConnection = sqlScriptEditorControl.GetField<IDbConnection>("m_connection");

            var sqlConnection = new System.Data.SqlClient.SqlConnection(dbConnection.ConnectionString);
            var serverConnection = new ServerConnection(sqlConnection);
            return new SqlConnectionInfo(serverConnection, ConnectionType.Sql);
        }
    }
}