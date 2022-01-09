using System;

namespace Terminal.Models
{
    public class PedometerUpdatedEventArgs : EventArgs
    {
        public int Calories { get; set; }
        public int Steps { get; set; }
        public int SpeedAverage { get; set; }
        public int Distance { get; set; }
    }
}
