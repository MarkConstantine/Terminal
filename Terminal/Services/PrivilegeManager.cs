using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Models;
using Tizen.Security;

namespace Terminal.Services
{
    public class PrivilegeManager
    {
        private const string PRIVILEGE_HEALTHINFO = "http://tizen.org/privilege/healthinfo";
        
        private static PrivilegeManager _instance;
        public static PrivilegeManager Instance
        {
            get => _instance ?? (_instance = new PrivilegeManager());
        }

        public event EventHandler PrivilegesChecked;
        private readonly IList<PrivilegeItem> _privilegeItems = new List<PrivilegeItem>()
        {
            new PrivilegeItem(PRIVILEGE_HEALTHINFO)
        };

        private PrivilegeManager()
        {
        }

        public void CheckAllPrivileges()
        {
            foreach (var item in _privilegeItems)
            {
                PrivilegeCheck(item.Privilege);
            }
        }

        public bool AllPermissionsGranted()
        {
            return _privilegeItems.All(item => item.Granted);
        }

        private bool AllPermissionsChecked()
        {
            return _privilegeItems.All(item => item.Checked);
        }

        private void AllPrivilegesChecked()
        {
            if (AllPermissionsChecked())
            {
                PrivilegesChecked?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PPM_RequestResponse(object sender, RequestResponseEventArgs e)
        {
            if (e.cause == CallCause.Answer)
            {
                switch (e.result)
                {
                    case RequestResult.AllowForever:
                        SetPermission(e.privilege, true);
                        break;
                    case RequestResult.DenyForever:
                    case RequestResult.DenyOnce:
                        SetPermission(e.privilege, false);
                        break;
                }
            }

            AllPrivilegesChecked();
        }

        private void PrivilegeCheck(string privilege)
        {
            switch (PrivacyPrivilegeManager.CheckPermission(privilege))
            {
                case CheckResult.Allow:
                    SetPermission(privilege, true);
                    break;
                case CheckResult.Deny:
                    SetPermission(privilege, false);
                    break;
                case CheckResult.Ask:
                    PrivacyPrivilegeManager.GetResponseContext(privilege)
                        .TryGetTarget(out PrivacyPrivilegeManager.ResponseContext context);

                    if (context != null)
                    {
                        context.ResponseFetched += PPM_RequestResponse;
                    }

                    PrivacyPrivilegeManager.RequestPermission(privilege);
                    PrivacyPrivilegeManager.GetResponseContext(privilege).TryGetTarget(out context);

                    break;
            }

            AllPrivilegesChecked();
        }

        private void SetPermission(string privilege, bool value)
        {
            _privilegeItems.First(p => p.Privilege == privilege).GrantPermission(value);
        }
    }
}
