using System;
using Tizen.Location;

namespace Terminal.Services
{
    public class LocationService
    {
        private static LocationService _instance;
        public static LocationService Instance
        {
            get => _instance ?? (_instance = new LocationService());
        }

        private readonly Locator _locator;

        private LocationService()
        {
            _locator = new Locator(LocationType.Hybrid);
            _locator.Start();

            var appTerminatedService = AppTerminatedService.Instance;
            appTerminatedService.Terminated += AppTerminatedService_Terminated;
        }

        public Location GetLocation()
        {
            return _locator.GetLocation();
        }

        private void AppTerminatedService_Terminated(object sender, EventArgs e)
        {
            _locator.Stop();
            _locator.Dispose();
            Logger.Log($"{nameof(LocationService)} Terminated");
        }
    }
}
