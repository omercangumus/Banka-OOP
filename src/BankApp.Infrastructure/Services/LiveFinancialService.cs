using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BankApp.Infrastructure.Services
{
    public class LiveFinancialService
    {
        private static readonly HttpClient _http = new HttpClient();
        
        // API Endpoints
        private const string COINGECKO_API = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin,ethereum,tether,solana,avalanche-2&vs_currencies=usd,try&include_24hr_change=true";
        private const string EXCHANGE_API = "https://api.exchangerate-api.com/v4/latest/USD"; // Base USD
        
        public async Task<LiveMarketData> GetMarketDataAsync()
        {
            var data = new LiveMarketData();

            try 
            {
                // 1. Fetch Crypto
                var cryptoJson = await _http.GetStringAsync(COINGECKO_API);
                var cryptoData = JsonSerializer.Deserialize<Dictionary<string, CoinGeckoItem>>(cryptoJson);

                if (cryptoData != null)
                {
                    data.Bitcoin = new AssetRate 
                    { 
                        PriceUSD = (decimal)cryptoData["bitcoin"].Usd, 
                        PriceTRY = (decimal)cryptoData["bitcoin"].Try,
                        Change24h = (decimal)cryptoData["bitcoin"].Change24h 
                    };
                    data.Ethereum = new AssetRate 
                    { 
                        PriceUSD = (decimal)cryptoData["ethereum"].Usd, 
                        PriceTRY = (decimal)cryptoData["ethereum"].Try,
                        Change24h = (decimal)cryptoData["ethereum"].Change24h 
                    };
                }

                // 2. Fetch Fiat (USD Base)
                var fiatJson = await _http.GetStringAsync(EXCHANGE_API);
                using (JsonDocument doc = JsonDocument.Parse(fiatJson))
                {
                    var rates = doc.RootElement.GetProperty("rates");
                    decimal usdTry = rates.GetProperty("TRY").GetDecimal();
                    decimal eurUsd = rates.GetProperty("EUR").GetDecimal(); // 1 USD = x EUR -> 1 EUR = 1/x USD

                    data.UsdTry = usdTry;
                    data.EurTry = usdTry / eurUsd; // Cross rate approximation
                    data.EurUsd = 1 / eurUsd;
                }

                // 3. Gold (Mocked real-ish calculation based on ounce)
                // Ounce ~ 2650 USD (Approx)
                decimal ounceUsd = 2650m; 
                data.GoldOunceUsd = ounceUsd;
                data.GoldGramTry = (ounceUsd * data.UsdTry) / 31.1035m;
            }
            catch (Exception ex)
            {
                // Fallback / Error handling
                Console.WriteLine($"Live Data Error: {ex.Message}");
            }

            return data;
        }
    }

    public class LiveMarketData
    {
        public decimal UsdTry { get; set; }
        public decimal EurTry { get; set; }
        public decimal EurUsd { get; set; }
        public decimal GoldGramTry { get; set; }
        public decimal GoldOunceUsd { get; set; }

        public AssetRate Bitcoin { get; set; } = new();
        public AssetRate Ethereum { get; set; } = new();
    }

    public class AssetRate
    {
        public decimal PriceUSD { get; set; }
        public decimal PriceTRY { get; set; }
        public decimal Change24h { get; set; }
    }

    // Helper for deserialization
    public class CoinGeckoItem
    {
        [JsonPropertyName("usd")]
        public double Usd { get; set; }
        
        [JsonPropertyName("try")]
        public double Try { get; set; }

        [JsonPropertyName("usd_24h_change")]
        public double Change24h { get; set; }
    }
}
