using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Models;
using Tizen.Security;

namespace Terminal.Services
{
    public class PrivilegeManager
    {
        private const string PRIVILEGE_ALARM_SET = "http://tizen.org/privilege/alarm.set";
        private const string PRIVILEGE_HEALTHINFO = "http://tizen.org/privilege/healthinfo";
        private const string PRIVILEGE_LOCATION = "http://tizen.org/privilege/location";
        private const string PRIVILEGE_INTERNET = "http://tizen.org/privilege/internet";
        private const string PRIVILEGE_NETWORK_GET = "http://tizen.org/privilege/network.get";

        private static PrivilegeManager _instance;
        public static PrivilegeManager Instance
        {
            get => _instance ?? (_instance = new PrivilegeManager());
        }

        public event EventHandler PrivilegesChecked;
        private readonly IList<PrivilegeItem> _privilegeItems = new List<PrivilegeItem>()
        {
            new PrivilegeItem(PRIVILEGE_ALARM_SET),
            new PrivilegeItem(PRIVILEGE_HEALTHINFO),
            new PrivilegeItem(PRIVILEGE_LOCATION),
            new PrivilegeItem(PRIVILEGE_INTERNET),
            new PrivilegeItem(PRIVILEGE_NETWORK_GET)
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
            var allGranted = _privilegeItems.All(item => item.Granted);
            Logger.Log($"All permissions granted: {allGranted}");
            return allGranted;
        }

        private bool AllPermissionsChecked()
        {
            var allChecked = _privilegeItems.All(item => item.Checked);
            Logger.Log($"All permissions checked: {allChecked}");
            return allChecked;
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
