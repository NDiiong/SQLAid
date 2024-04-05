using Microsoft.SqlServer.Management.UI.VSIntegration;
using Microsoft.SqlServer.Management.UI.VSIntegration.Editors;

namespace SQLAid.Integration.DTE
{
    public class HostContext : IHostContext
    {
        private readonly IFrameDocumentView _frameDocumentView;

        public HostContext(IFrameDocumentView frameDocumentView)
        {
            _frameDocumentView = frameDocumentView;
        }

        public IEditor GetCurrentEditor()
        {
            return new Editor(_frameDocumentView);
        }

        public IEditor GetNewEditor()
        {
            ServiceCache.ScriptFactory.CreateNewBlankScript(ScriptType.Sql);
            return GetCurrentEditor();
        }

        public IServerConnection CloneCurrentConnection(string database)
        {
            throw new System.NotImplementedException();
        }

        public string GetCurrentConnectionString()
        {
            throw new System.NotImplementedException();
        }
    }
}