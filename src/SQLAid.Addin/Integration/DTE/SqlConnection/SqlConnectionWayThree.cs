using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.ObjectExplorer;

namespace SQLAid.Integration.DTE.SqlConnection
{
    public class SqlConnectionWayThree : ISqlConnection
    {
        public SqlConnectionInfo GetCurrentSqlConnection()
        {
            var nodes = GetObjectExplorerSelectedNodes();

            if (nodes.Length > 0)
                return nodes[0].Connection as SqlConnectionInfo;

            return default;
        }

        private INodeInformation[] GetObjectExplorerSelectedNodes()
        {
            var objExplorer = GetObjectExplorer();
            objExplorer.GetSelectedNodes(out var _, out var nodes);
            return nodes;
        }

        private IObjectExplorerService GetObjectExplorer()
        {
            return (IObjectExplorerService)ServiceCache.ServiceProvider.GetService(typeof(IObjectExplorerService));
        }
    }
}