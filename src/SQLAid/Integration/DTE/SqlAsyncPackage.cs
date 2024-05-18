using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.Win32;
using SQLAid.Logging;
using SQLAid.Options;
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

        public static DTE2 _application;

        public DTE2 Application
        {
            get
            {
                if (_application != null)
                    return _application;

                _application = GetGlobalService(typeof(EnvDTE.DTE)) as DTE2;
                return _application;
            }
        }

        private SQLAidOptions _options;

        public SQLAidOptions Options
        {
            get
            {
                if (_options != null)
                    return _options;

                _options = new SQLAidOptions();
                return _options;
            }
        }

        public IMenuCommandService MenuCommand { get; private set; }

        private string _extensionInstallationDirectory;

        public string ExtensionInstallationDirectory
        {
            get
            {
                if (!string.IsNullOrEmpty(_extensionInstallationDirectory))
                    return _extensionInstallationDirectory;

                var uri = new Uri(GetType().Assembly.CodeBase, UriKind.Absolute);
                _extensionInstallationDirectory = Path.GetDirectoryName(uri.LocalPath);
                return _extensionInstallationDirectory;
            }
        }

        protected SqlAsyncPackage(string packageGuidString)
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
            Logger.Initialize(Options.LocalData);
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
    }
}