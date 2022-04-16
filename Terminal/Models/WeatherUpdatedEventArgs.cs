using System;

namespace Terminal.Models
{
    public class WeatherUpdatedEventArgs : EventArgs
    {
        public string CurrentTemperature { get; set; }
        public string TemperatureUnit { get; set; }
    }
}
