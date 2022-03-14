using System;

namespace Terminal.Models
{
    public class PedometerUpdatedEventArgs : EventArgs
    {
        /// <summary>
        ///     The recorded total step count.
        /// </summary>
        public int Steps { get; set; }

        /// <summary>
        ///     The calories burned.
        /// </summary>
        public int Calories { get; set; }

        /// <summary>
        ///     The last recorded speed.
        /// </summary>
        public int SpeedAverage { get; set; }

        /// <summary>
        ///     The moving distance travelled.
        /// </summary>
        public int Distance { get; set; }
    }
}
