using System;
using Terminal.Services;
using Terminal.Views;
using Terminal.ViewModels;
using Tizen.Applications;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer.Watchface;
using Xamarin.Forms;
using Tizen;

namespace Terminal
{
    class Program : FormsWatchface
    {
        private WatchView _watchView;
        private ClockViewModel _viewModel;

        protected override void OnCreate()
        {
            base.OnCreate();

            ElmSharp.Utility.AppendGlobalFontPath(DirectoryInfo.Resource);

            PrivilegeManager privilegeManager = PrivilegeManager.Instance;

            if (!privilegeManager.AllPermissionsGranted())
            {
                privilegeManager.PrivilegesChecked += OnPrivilegesChecked;
                privilegeManager.CheckAllPrivileges();
            }
            else
            {
                Load();
            }
        }

        protected override void OnTick(TimeEventArgs time)
        {
            base.OnTick(time);
            if (_viewModel != null)
            {
                _viewModel.Time = time.Time.UtcTimestamp;
                _viewModel.Battery = Battery.Percent;
            }
        }

        protected override void OnAmbientChanged(AmbientEventArgs mode)
        {
            base.OnAmbientChanged(mode);
            _viewModel.AmbientModeEnabled = mode.Enabled;
        }

        protected override void OnAmbientTick(TimeEventArgs time)
        {
            base.OnAmbientTick(time);
            _viewModel.Time = time.Time.UtcTimestamp + TimeSpan.FromMilliseconds(time.Time.Millisecond);
        }

        protected override void OnTerminate()
        {
            AppTerminatedService service = AppTerminatedService.Instance;
            service.Terminate();
            base.OnTerminate();
        }

        private void Load()
        {
            _viewModel = new ClockViewModel();
            _watchView = new WatchView
            {
                BindingContext = _viewModel
            };
            LoadWatchface(_watchView);
        }

        private void OnPrivilegesChecked(object sender, EventArgs args)
        {
            PrivilegeManager privilegeManager = PrivilegeManager.Instance;
            if (privilegeManager.AllPermissionsGranted())
            {
                privilegeManager.PrivilegesChecked -= OnPrivilegesChecked;
                Load();
            }
            else
            {
                Current.Exit();
            }
        }

        public static void Main(string[] args)
        {
            Logger.Log("PROGRAM START");
            var app = new Program();
            Forms.Init(app);
            FormsCircularUI.Init();
            app.Run(args);
        }
    }
}
