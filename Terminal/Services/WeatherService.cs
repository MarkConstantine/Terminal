using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terminal.Models;
using Tizen.Location;
using Tizen;

namespace Terminal.Services
{
    public class WeatherService
    {
        private static WeatherService _instance;
        public static WeatherService Instance
        {
            get => _instance ?? (_instance = new WeatherService());
        }

        public event EventHandler<WeatherUpdatedEventArgs> WeatherUpdated;
        private readonly LocationService _locationService;
        private readonly HttpClient _client;

        private DateTime _updateTime;
        private Location _lastLocation;
        private List<WeatherGovForecastPeriod> _hourlyForecast;

        private WeatherService()
        {
            _locationService = LocationService.Instance;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("User-Agent", "(mark.constantine, MarkConstantConstantine@gmail.com)");
            Task.Run(WeatherThread);
        }

        private async Task WeatherThread()
        {
            while (true)
            {
                try
                {
                    await GetTemperatureAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(Constants.LogTag, $"Exception in {nameof(WeatherThread)}: {ex}");
                }

                await Task.Delay(TimeSpan.FromHours(1));
            }
        }

        private async Task<Uri> GetForecastUriAsync(Location location)
        {
            var requestUri = new Uri($"https://api.weather.gov/points/{location.Latitude},{location.Longitude}");
            
            var httpResponse = await _client.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var pointResponse = JsonConvert.DeserializeObject<WeatherGovPointResponse>(jsonResponse);

            var forecastUri = pointResponse.Properties.ForecastHourlyUri;
            Log.Info(Constants.LogTag, $"Forecast URI from weather.gov: {forecastUri}");
            return new Uri(forecastUri);
        }

        private async Task<List<WeatherGovForecastPeriod>> GetForecastAsync(Uri forecastUri)
        {
            var httpResponse = await _client.GetAsync(forecastUri);
            httpResponse.EnsureSuccessStatusCode();

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var forecastResponse = JsonConvert.DeserializeObject<WeatherGovForecastResponse>(jsonResponse);

            var periods = forecastResponse?.Properties?.Periods;
            Log.Info(Constants.LogTag, $"Received {periods.Count} forecast period(s) between {periods.First().StartTime} and {periods.Last().EndTime}");
            return periods;
        }

        private async Task GetTemperatureAsync()
        {
            // Cache forecast data to improve battery life.
            var now = DateTime.Now;
            var currentLocation = _locationService.GetLocation();

            if (_updateTime.Date != now.Date || _hourlyForecast == null || _lastLocation != currentLocation)
            {
                // To update cache.
                var forecastUri = await GetForecastUriAsync(currentLocation);
                _hourlyForecast = await GetForecastAsync(forecastUri);
                _lastLocation = currentLocation;
                _updateTime = DateTime.UtcNow;
            }

            // Use cache.
            var hourForecast = _hourlyForecast.Where(period => period.StartTime <= now && now <= period.EndTime).FirstOrDefault();
            if (hourForecast == null)
            {
                // Cache miss invalidate cache.
                _updateTime = DateTime.MinValue; 
            }
            else
            {
                WeatherUpdated.Invoke(this, new WeatherUpdatedEventArgs
                {
                    CurrentTemperature = hourForecast.Temperature,
                    TemperatureUnit = hourForecast.TemperatureUnit,
                });
            }
        }

        private class WeatherGovPointResponse
        {
            public WeatherGovPointProperties Properties { get; set; }
        }

        private class WeatherGovPointProperties
        {
            [JsonProperty("gridX")]
            public int GridX { get; set; }

            [JsonProperty("gridY")]
            public int GridY { get; set; }

            [JsonProperty("forecastHourly")]
            public string ForecastHourlyUri { get; set; }
        }

        private class WeatherGovForecastResponse
        {
            [JsonProperty("properties")]
            public WeatherGovForecastProperties Properties { get; set; }
        }

        private class WeatherGovForecastProperties
        {
            [JsonProperty("periods")]
            public List<WeatherGovForecastPeriod> Periods { get; set; }
        }

        private class WeatherGovForecastPeriod
        {
            [JsonProperty("number")]
            public int Number { get; set; }

            [JsonProperty("startTime")]
            public DateTime StartTime { get; set; }

            [JsonProperty("endTime")]
            public DateTime EndTime { get; set; }

            [JsonProperty("temperature")]
            public string Temperature { get; set; }

            [JsonProperty("temperatureUnit")]
            public string TemperatureUnit { get; set; }
        }
    }
}
