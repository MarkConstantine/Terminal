using System;
using Terminal.Models;
using Terminal.Services;

namespace Terminal.ViewModels
{
    public class ClockViewModel : BaseViewModel
    {
        private DateTime _time;
        private int _battery = 100;
        private bool _ambientModeEnabled = false;
        private int _totalSteps = 0;
        private int _totalStepsAtLastReset = 0;
        private DateTime _lastResetTime = DateTime.MinValue;
        private string _weather = string.Empty;
        private string _bitcoinPrice = string.Empty;

        public ClockViewModel()
        {
            PedometerService.Instance.PedometerUpdated += Service_PedometerUpdated;
            WeatherService.Instance.WeatherUpdated += Service_WeatherUpdated;
            BitcoinService.Instance.PriceUpdated += Service_PriceUpdated;
        }

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
            }
        }

        public string WatchTime
        {
            get => _time.ToString(AmbientModeEnabled ? "hh\\:mm tt" : "hh\\:mm\\:ss tt");
        }

        public string Weather
        {
            get => _weather;
            set
            {
                if (_weather == value) return;
                _weather = value;
                OnPropertyChanged();
            }
        }

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

        public string BitcoinPrice
        {
            get => _bitcoinPrice;
            set
            {
                if (_bitcoinPrice == value) return;
                _bitcoinPrice = value;
                OnPropertyChanged();
            }
        }

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

        private void HandleStepReset(DateTime currentTime)
        {
            if (currentTime.Date == _lastResetTime.Date) return;
            _lastResetTime = currentTime;
            _totalStepsAtLastReset = _totalSteps;
            OnPropertyChanged(nameof(Steps));
        }
    }
}
