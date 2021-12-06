using Tizen.Location;
using System;

namespace Terminal.ViewModels
{
    public class ClockViewModel : BaseViewModel
    {
        private DateTime _time;
        private uint _steps = 5_000;
        private int _battery = 100;
        private double _celsius = 50;
        private Location _location = null;

        public DateTime Time
        {
            get => _time;
            set
            {
                if (_time == value) return;
                _time = value;
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

        public uint Steps
        {
            get => _steps;
            set
            {
                if (_steps == value) return;
                _steps = value;
                OnPropertyChanged();
            }
        }

        public Location Location
        {
            get => _location;
            set
            {
                if (_location == Location) return;
                _location = value;
                OnPropertyChanged();
            }
        }

        public string Weather
        {
            get => $"{_celsius * 1.8 + 32.0}°F {_celsius}°C";
        }
    }
}
