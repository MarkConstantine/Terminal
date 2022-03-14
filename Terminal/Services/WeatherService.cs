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
        private static HttpClient _client = new HttpClient();
        private static WeatherService _instance;
        public static WeatherService Instance
        {
            get => _instance ?? (_instance = new WeatherService());
        }

        public event EventHandler<WeatherUpdatedEventArgs> WeatherUpdated;
        private readonly LocationService _locationService;

        private WeatherService()
        {
            _locationService = LocationService.Instance;

            Task.Run(WeatherThread);
        }

        private async Task WeatherThread()
        {
            while (true)
            {
                try
                {
                    var location = _locationService.GetLocation();
                    await GetWeatherAsync(location);
                }
                catch (Exception ex)
                {
                    Log.Error(Constants.LogTag, $"Exception in {nameof(WeatherThread)}: {ex}");
                }

                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        private async Task GetWeatherAsync(Location location)
        {
            if (location == null)
            {
                Log.Warn(Constants.LogTag, $"{nameof(GetWeatherAsync)}: Location was null for weather request");
                return;
            }

            var uri = new Uri($"http://www.7timer.info/bin/api.pl?lon={location.Longitude}&lat={location.Latitude}&product=civillight&output=json");
            
            var httpResponse = await _client.GetAsync(uri);
            httpResponse.EnsureSuccessStatusCode();

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
            var forecast = JsonConvert.DeserializeObject<WeatherCivilLightResponse>(jsonResponse);

            var todaysForecast = forecast.DataSeries.First(d => d.Date == int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            var todaysTemperature = todaysForecast.Temperature;

            WeatherUpdated.Invoke(this, new WeatherUpdatedEventArgs
            {
                MinCelsius = todaysTemperature.Min,
                MaxCelsius = todaysTemperature.Max,
                MinFarenheit = CelsiusToFarenheit(todaysTemperature.Min),
                MaxFarenheit = CelsiusToFarenheit(todaysTemperature.Max)
            });
        }

        private double CelsiusToFarenheit(double celsius)
        {
            return celsius * 9 / 5 + 32;
        }

        private class WeatherCivilLightResponse
        {
            [JsonProperty("product")]
            public string Product { get; set; }

            [JsonProperty("init")]
            public string Init { get; set; }
            
            [JsonProperty("dataseries")]
            public IList<ForecastResponse> DataSeries { get; set; }
        }

        private class TemperatureMinMax
        {
            [JsonProperty("max")]
            public int Max { get; set; }

            [JsonProperty("min")]
            public int Min { get; set; }
        }

        private class ForecastResponse
        {
            [JsonProperty("date")]
            public int Date { get; set; }
            
            [JsonProperty("weather")]
            public string Weather { get; set; }

            [JsonProperty("temp2m")]
            public TemperatureMinMax Temperature { get; set; }

            [JsonProperty("wind10m_max")]
            public int WindSpeed { get; set; }
        }
    }
}
