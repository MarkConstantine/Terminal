using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Terminal.Models;
using Tizen;

namespace Terminal.Services
{
    public class BitcoinService
    {
        private static BitcoinService _instance;
        public static BitcoinService Instance
        {
            get => _instance ?? (_instance = new BitcoinService());
        }

        public event EventHandler<BitcoinPriceUpdatedEventArgs> PriceUpdated;
        private readonly HttpClient _client;

        public async Task UpdatePrice()
        {
            var price = await GetPrice();
            PriceUpdated.Invoke(this, new BitcoinPriceUpdatedEventArgs
            {
                Price = decimal.Parse(price.PriceUsd),
                ChangePercent24Hr = decimal.Parse(price.ChangePercent24Hr)
            });
        }

        private BitcoinService()
        {
            _client = new HttpClient();
            Task.Run(PriceThread);
        }

        private async Task PriceThread()
        {
            while (true)
            {
                try
                {
                    await UpdatePrice();
                    await Task.Delay(TimeSpan.FromHours(1));
                }
                catch (Exception ex)
                {
                    Log.Error(Constants.LogTag, $"Exception in {nameof(PriceThread)}: {ex}");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }

        private async Task<CoinCapAsset> GetPrice()
        {
            var requestUri = new Uri("https://api.coincap.io/v2/assets/bitcoin");

            var httpResponse = await _client.GetAsync(requestUri);
            httpResponse.EnsureSuccessStatusCode();

            var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

            var assetResponse = JsonConvert.DeserializeObject<CoinCapAssetResponse>(jsonResponse);
            Log.Info(Constants.LogTag, $"{requestUri} {httpResponse.StatusCode} [{assetResponse.Data.PriceUsd}, {assetResponse.Data.ChangePercent24Hr}%]");
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(httpResponse.Headers));
            Log.Debug(Constants.LogTag, JsonConvert.SerializeObject(assetResponse));

            return assetResponse.Data;
        }

        private class CoinCapAssetResponse
        {
            [JsonProperty("timestamp")]
            public ulong Timestamp { get; set; }

            [JsonProperty("data")]
            public CoinCapAsset Data { get; set; }
        }

        private class CoinCapAsset
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("rank")]
            public string Rank { get; set; }

            [JsonProperty("symbol")]
            public string Symbol { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("supply")]
            public string Supply { get; set; }

            [JsonProperty("maxSupply")]
            public string MaxSupply { get; set; }

            [JsonProperty("marketCapUsd")]
            public string MarketCapUsd { get; set; }

            [JsonProperty("volumeUsd24Hr")]
            public string VolumeUsd24Hr { get; set; }

            [JsonProperty("priceUsd")]
            public string PriceUsd { get; set; }

            [JsonProperty("changePercent24Hr")]
            public string ChangePercent24Hr { get; set; }

            [JsonProperty("vwap24Hr")]
            public string Vwap24Hr { get; set; }

            [JsonProperty("explorer")]
            public string Explorer { get; set; }
        }
    }
}
