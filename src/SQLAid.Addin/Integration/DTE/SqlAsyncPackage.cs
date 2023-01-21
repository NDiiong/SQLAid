using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using SQLAid.Addin.Logging;
using System;
using System.ComponentModel.Design;
using System.IO;
using Task = System.Threading.Tasks.Task;

namespace SQLAid.Integration.DTE
{
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string)]
    public abstract class SqlAsyncPackage : Package
    {
        private readonly string _packageGuidString;

        public DTE2 Application { get; private set; }
        public IMenuCommandService MenuCommand { get; private set; }

        public SqlAsyncPackage(string packageGuidString)
        {
            _packageGuidString = packageGuidString;
        }

        protected override int QueryClose(out bool canClose)
        {
#if !DEBUG
            SetSkipLoading();
#endif
            return base.QueryClose(out canClose);
        }

        protected abstract Task InitializeAsync();

        protected override void Initialize()
        {
            base.Initialize();
            Logger.Initialize(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SqlAidAsyncPackage.NAME));
            ThreadHelper.JoinableTaskFactory.Run(InitializeAsync);
        }

        private void SetSkipLoading()
        {
            try
            {
                var registryKey = UserRegistryRoot.CreateSubKey(string.Format("Packages\\{{{0}}}", _packageGuidString));
                registryKey.SetValue("SkipLoading", 1, RegistryValueKind.DWord);
                registryKey.Close();
            }
            catch
            { }
        }

        public string GetExtensionInstallationDirectory()
        {
            var uri = new Uri(GetType().Assembly.CodeBase, UriKind.Absolute);
            return Path.GetDirectoryName(uri.LocalPath);
        }
    }
}