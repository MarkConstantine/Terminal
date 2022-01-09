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
        private int _steps = 0;

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
                OnPropertyChanged();
                OnPropertyChanged(nameof(WatchTime));
            }
        }

        public string WatchTime
        {
            get => _time.ToString(AmbientModeEnabled ? "hh\\:mm" : "hh\\:mm\\:ss");
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
            get => _steps;
            set
            {
                if (_steps == value) return;
                _steps = value;
                OnPropertyChanged();
            }
        }

        private void Service_PedometerUpdated(object sender, PedometerUpdatedEventArgs e)
        {
            Steps = e.Steps;
        }
    }
}
