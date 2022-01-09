using System;
using Terminal.Models;
using Tizen.Sensor;

namespace Terminal.Services
{
    public class PedometerService
    {
        private static PedometerService _instance;
        public static PedometerService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PedometerService();
                }
                return _instance;
            }
        }

        public event EventHandler<PedometerUpdatedEventArgs> PedometerUpdated;
        private readonly Pedometer _pedometer;

        private PedometerService()
        {
            _pedometer = new Pedometer();
            _pedometer.DataUpdated += Pedometer_DataUpdated;
            _pedometer.Start();
            PedometerUpdated?.Invoke(this, new PedometerUpdatedEventArgs
            {
                Calories = (int)_pedometer.CalorieBurned,
                Steps = (int)_pedometer.StepCount,
                SpeedAverage = (int)_pedometer.LastSpeed,
                Distance = (int)_pedometer.MovingDistance
            });

            var appTerminatedService = AppTerminatedService.Instance;
            appTerminatedService.Terminated += AppTerminatedService_Terminated;
        }

        private void Pedometer_DataUpdated(object sender, PedometerDataUpdatedEventArgs e)
        {
            PedometerUpdated?.Invoke(this, new PedometerUpdatedEventArgs
            {
                Calories = (int)e.CalorieBurned,
                Steps = (int)e.StepCount,
                SpeedAverage = (int)e.LastSpeed,
                Distance = (int)e.MovingDistance
            });
        }

        private void AppTerminatedService_Terminated(object sender, EventArgs e)
        {
            _pedometer.Stop();
            _pedometer.DataUpdated -= Pedometer_DataUpdated;
            _pedometer.Dispose();
        }
    }
}
