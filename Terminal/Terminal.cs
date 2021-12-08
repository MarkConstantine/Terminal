using Terminal.Views;
using Terminal.ViewModels;
using Tizen.Applications;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer.Watchface;
using Xamarin.Forms;

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

            _viewModel = new ClockViewModel();
            _watchView = new WatchView
            {
                BindingContext = _viewModel
            };
            LoadWatchface(_watchView);
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
        }

        protected override void OnAmbientTick(TimeEventArgs time)
        {
            base.OnAmbientTick(time);
        }

        protected override void OnTerminate()
        {
            base.OnTerminate();
        }


        static void Main(string[] args)
        {
            var app = new Program();
            Forms.Init(app);
            FormsCircularUI.Init();
            app.Run(args);
        }
    }
}
