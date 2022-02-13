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
        private int _totalStepsAtMidnight = 0;

        public ClockViewModel()
        {
            PedometerService service = PedometerService.Instance;
            service.PedometerUpdated += Service_PedometerUpdated;
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                if (_time == value) return;
                _time = value;
                if (AtMidnight(_time.TimeOfDay))
                {
                    _totalStepsAtMidnight = _totalSteps;
                    OnPropertyChanged(nameof(Steps));
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(WatchTime));
            }
        }

        public string WatchTime
        {
            get => _time.ToString(AmbientModeEnabled ? "hh\\:mm tt" : "hh\\:mm\\:ss tt");
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
            get => _totalSteps - _totalStepsAtMidnight;
            set
            {
                if (_totalSteps == value) return;
                _totalSteps = value;
                OnPropertyChanged();
            }
        }

        private void Service_PedometerUpdated(object sender, PedometerUpdatedEventArgs e)
        {
            Steps = e.Steps;
        }

        private bool AtMidnight(TimeSpan timeOfDay)
        {
            var midnight = DateTime.Today.TimeOfDay;
            var diff = timeOfDay - midnight;
            return TimeSpan.Zero < diff && diff < TimeSpan.FromSeconds(5);
        }
    }
}
