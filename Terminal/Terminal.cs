using Terminal.ViewModels;
using Tizen.Applications;
using Tizen.Location;
using Tizen.System;
using Tizen.Wearable.CircularUI.Forms;
using Tizen.Wearable.CircularUI.Forms.Renderer.Watchface;
using Xamarin.Forms;

namespace Terminal
{
    class Program : FormsWatchface
    {
        private ClockViewModel _viewModel;
        private Locator _locator;


        protected override void OnCreate()
        {
            base.OnCreate();

            ElmSharp.Utility.AppendGlobalFontPath(DirectoryInfo.Resource);

            //_locator = new Locator(LocationType.Hybrid);
            //_locator.LocationChanged += LocationChangedHandler;
            //_locator.Interval = 60;
            //_locator.Start();

            var watchfaceApp = new TextWatchApplication();
            _viewModel = new ClockViewModel();
            watchfaceApp.BindingContext = _viewModel;
            LoadWatchface(watchfaceApp);
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

            _locator.Dispose();
            _locator = null;
        }

        private void LocationChangedHandler(object semder, LocationChangedEventArgs e)
        {
            _viewModel.Location = e.Location;
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
