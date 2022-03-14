using System;

namespace Terminal.Models
{
    public class WeatherUpdatedEventArgs : EventArgs
    {
        public double MinCelsius { get; set; }
        public double MaxCelsius { get; set; }
        public double MinFarenheit { get; set; }
        public double MaxFarenheit { get; set; }
    }
}
