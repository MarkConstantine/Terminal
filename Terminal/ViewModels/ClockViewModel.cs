using System;
using System.Threading.Tasks;
using Terminal.Models;
using Terminal.Services;
using Tizen;
using Xamarin.Forms;

namespace Terminal.ViewModels
{
    public class ClockViewModel : BaseViewModel
    {
        private PedometerService _pedometerService = PedometerService.Instance;
        private WeatherService _weatherService = WeatherService.Instance;
        private BitcoinService _bitcoinService = BitcoinService.Instance;

        public ClockViewModel()
        {
            _pedometerService.PedometerUpdated += Service_PedometerUpdated;
            _weatherService.WeatherUpdated += Service_WeatherUpdated;
            _bitcoinService.PriceUpdated += Service_PriceUpdated;
            RefreshCommand = new Command(
                execute: RefreshCommand_Execute,
                canExecute: RefreshCommand_CanExecute);
        }

        private DateTime _time;
        public DateTime Time
        {
            get => _time;
            set
            {
                if (_time == value) return;
                _time = value;
                HandleStepReset(value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(WatchTime));
                OnPropertyChanged(nameof(WatchDate));
            }
        }

        #region Ambient Mode
        private bool _ambientModeEnabled = false;
        public bool AmbientModeEnabled
        {
            get => _ambientModeEnabled;
            set
            {
                if (_ambientModeEnabled == value) return;
                _ambientModeEnabled = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Watch Time
        public string WatchTime
        {
            get => _time.ToString(AmbientModeEnabled ? "hh\\:mm tt" : "hh\\:mm\\:ss tt");
        }

        private bool _watchTimeVisible = true;
        public bool WatchTimeVisible
        {
            get => _watchTimeVisible;
            set
            {
                _watchTimeVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WatchTime));
            }
        }
        #endregion

        #region Watch Date
        public string WatchDate
        {
            get => _time.ToString("ddd MM/dd");
        }

        private bool _watchDateVisible = true;
        public bool WatchDateVisible
        {
            get => _watchDateVisible;
            set
            {
                _watchDateVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WatchDate));
            }
        }
        #endregion

        #region Battery
        private int _battery = 50;
        public int Battery
        {
            get => _battery;
            set
            {
                if (_battery == value) return;
                _battery = value;
                OnPropertyChanged();
            }
        }

        private bool _batteryVisible = true;
        public bool BatteryVisible
        {
            get => _batteryVisible;
            set
            {
                _batteryVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Battery));
            }
        }
        #endregion

        #region Steps
        private int _totalSteps = 0;
        private int _totalStepsAtLastReset = 0;
        public int Steps
        {
            get => _totalSteps - _totalStepsAtLastReset;
            set
            {
                if (_totalSteps == value) return;
                _totalSteps = value;
                OnPropertyChanged();
            }
        }

        private bool _stepsVisible = true;
        public bool StepsVisible
        {
            get => _stepsVisible;
            set
            {
                _stepsVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Steps));
            }
        }

        private DateTime _lastResetTime = DateTime.MinValue;
        private void HandleStepReset(DateTime currentTime)
        {
            if (currentTime.Date == _lastResetTime.Date) return;
            _lastResetTime = currentTime;
            _totalStepsAtLastReset = _totalSteps;
            OnPropertyChanged(nameof(Steps));
        }
        #endregion

        #region Weather
        private string _weather = string.Empty;
        public string Weather
        {
            get => _weather;
            set
            {
                if (_weather == value) return;
                _weather = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WeatherVisible));
            }
        }

        private bool _weatherVisible = true;
        public bool WeatherVisible
        {
            get => _weatherVisible && !string.IsNullOrEmpty(_weather);
            set
            {
                _weatherVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Weather));
            }
        }
        #endregion

        #region Bitcoin Price
        private string _bitcoinPrice = string.Empty;
        public string BitcoinPrice
        {
            get => _bitcoinPrice;
            set
            {
                if (_bitcoinPrice == value) return;
                _bitcoinPrice = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BitcoinPriceVisible));
            }
        }

        private bool _bitcoinPriceVisible = true;
        public bool BitcoinPriceVisible
        {
            get => _bitcoinPriceVisible && !string.IsNullOrEmpty(_bitcoinPrice);
            set
            {
                _bitcoinPriceVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BitcoinPrice));
            }
        }
        #endregion

        #region Refresh Button
        public Command RefreshCommand { private set; get; }

        private bool _currentlyRefreshing = false;
        public bool CurrentlyRefreshing
        {
            get => _currentlyRefreshing;
            set
            {
                _currentlyRefreshing = value;
                OnPropertyChanged();
                RefreshCommand.ChangeCanExecute();
            }
        }

        private async void RefreshCommand_Execute(object arg)
        {
            CurrentlyRefreshing = true;

            try
            {
                var weatherUpdateTask = _weatherService.UpdateWeather();
                var bitcoinUpdateTask = _bitcoinService.UpdatePrice();
                var loadingTask = SimulateSlowLoading();

                await Task.WhenAll(weatherUpdateTask, bitcoinUpdateTask, loadingTask);
            }
            catch (Exception ex)
            {
                Logger.Log(Constants.LogTag, $"Refresh Exception: {ex}");
            }

            CurrentlyRefreshing = false;
        }

        private bool RefreshCommand_CanExecute(object arg)
        {
            return !CurrentlyRefreshing;
        }

        private async Task SimulateSlowLoading()
        {
            WatchTimeVisible = false;
            WatchDateVisible = false;
            BatteryVisible = false;
            StepsVisible = false;
            WeatherVisible = false;
            BitcoinPriceVisible = false;

            WatchTimeVisible = true;
            await WaitDelay();

            WatchDateVisible = true;
            await WaitDelay();

            BatteryVisible = true;
            await WaitDelay();

            StepsVisible = true;
            await WaitDelay();

            WeatherVisible = true;
            await WaitDelay();

            BitcoinPriceVisible = true;
            await WaitDelay();
        }

        private Random _random = new Random();
        private Task WaitDelay(int minDelay = 50, int maxDelay = 500)
        {
            return Task.Delay(TimeSpan.FromMilliseconds(_random.Next(minDelay, maxDelay)));
        }
        #endregion

        private void Service_PedometerUpdated(object sender, PedometerUpdatedEventArgs e)
        {
            Steps = e.Steps;
        }

        private void Service_WeatherUpdated(object sender, WeatherUpdatedEventArgs e)
        {
            Weather = $"{e.CurrentTemperature}°{e.TemperatureUnit}";
        }

        private void Service_PriceUpdated(object sender, BitcoinPriceUpdatedEventArgs e)
        {
            BitcoinPrice = $"{e.Price:c}";
        }
    }
}
