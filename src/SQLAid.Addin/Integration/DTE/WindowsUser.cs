using System;
using System.Security.Principal;

namespace SQLAid.Integration.DTE
{
    public class WindowsUser : IWindowsUser
    {
        private WindowsIdentity _identity;

        public WindowsUser()
        {
            _identity = WindowsIdentity.GetCurrent();
        }

        public string Name => _identity.Name ?? String.Empty;
    }
}