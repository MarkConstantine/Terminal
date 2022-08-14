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

        public async Task UpdateWeather()
        {
            var hourForecast = await GetTemperatureAsync();
            WeatherUpdated.Invoke(this, new WeatherUpdatedEventArgs
            {
                CurrentTemperature = hourForecast.Temperature,
                TemperatureUnit = hourForecast.TemperatureUnit,
            });
            Log.Info(Constants.LogTag, $"Weather updated {hourForecast.Temperature}{hourForecast.TemperatureUnit}");
        }

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
                    await UpdateWeather();
                    await Task.Delay(TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    Log.Error(Constants.LogTag, $"Exception in {nameof(WeatherThread)}: {ex}");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private async Task<Uri> GetForecastUriAsync(Location location)
        {
            var requestUri = new Uri($"https://api.weather.gov/points/{location.Latitude},{location.Longitude}");

            var httpResponse = await _client.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            var pointResponse = JsonConvert.DeserializeObject<WeatherGovPointResponse>(jsonResponse);
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(httpResponse.Headers));
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(pointResponse));

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
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(httpResponse.Headers));
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(forecastResponse, Formatting.None));

            var periods = forecastResponse?.Properties?.Periods;
            Log.Info(Constants.LogTag, $"Received {periods.Count} forecast period(s) between {periods.First().StartTime} and {periods.Last().EndTime}");
            return periods;
        }

        private async Task<WeatherGovForecastPeriod> GetTemperatureAsync()
        {
            var currentLocation = _locationService.GetLocation();
            var forecastUri = await GetForecastUriAsync(currentLocation);
            var hourlyForecast = await GetForecastAsync(forecastUri);

            var now = DateTime.Now;
            return hourlyForecast.First(period => period.StartTime <= now && now <= period.EndTime);
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
