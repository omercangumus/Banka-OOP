using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Globalization;
using System.Linq;

namespace BankApp.Infrastructure.Services
{
    public class StockService
    {
        private readonly HttpClient _http;
        private readonly Random _random;

        public StockService()
        {
            _http = new HttpClient();
            _random = new Random();
        }

        // Genişletilmiş BIST Listesi
        private readonly string[] _allStocks = new string[] 
        { 
            "THYAO", "GARAN", "ASELS", "AKBNK", "EREGL", "SASA", "HEKTS", 
            "SISE", "KCHOL", "FROTO", "TUPRS", "BIMAS", "YKBNK", "ISCTR", 
            "PETKM", "KONTR", "ODAS", "GUBRF", "EKGYO", "VESTL", "KOZAL",
            "TCELL", "SAHOL", "ENKAI", "ALARK", "PGSUS", "TOASO", "MGROS",
            "ASTOR", "EUPWR", "GESAN", "SMART"
        };

        public string[] GetStockList() => _allStocks;

        public async Task<List<StockCandle>> GetStockHistoryAsync(string symbol)
        {
            try
            {
                // Yahoo Finance CSV Linki
                string url = $"https://query1.finance.yahoo.com/v7/finance/download/{symbol}.IS?period1={DateTimeOffset.Now.AddYears(-1).ToUnixTimeSeconds()}&period2={DateTimeOffset.Now.ToUnixTimeSeconds()}&interval=1d&events=history";
                var csv = await _http.GetStringAsync(url);
                
                var list = new List<StockCandle>();
                var lines = csv.Split('\n').Skip(1);
                
                foreach (var line in lines)
                {
                    var p = line.Split(',');
                    if (p.Length < 6) continue;
                    if (DateTime.TryParse(p[0], out var date) && 
                        decimal.TryParse(p[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var open) &&
                        decimal.TryParse(p[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var high) &&
                        decimal.TryParse(p[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var low) &&
                        decimal.TryParse(p[4], NumberStyles.Any, CultureInfo.InvariantCulture, out var close))
                    {
                        list.Add(new StockCandle { Date = date, Open = open, High = high, Low = low, Close = close });
                    }
                }
                return list;
            }
            catch 
            {
                return GenerateDummy(symbol);
            }
        }

        public decimal GetLivePrice(string symbol, decimal lastClose)
        {
            // Simülasyon: Son kapanış fiyatına % -2 ... +2 arası değişim uygula
            double changePercent = (_random.NextDouble() * 4) - 2; // -2 ile +2
            decimal change = lastClose * (decimal)(changePercent / 100);
            return lastClose + change;
        }

        private List<StockCandle> GenerateDummy(string symbol)
        {
            var list = new List<StockCandle>();
            decimal price = 100; // Base price
            for(int i=0; i<50; i++)
            {
                decimal change = (decimal)(_random.NextDouble() * 4 - 2);
                list.Add(new StockCandle { 
                    Date = DateTime.Now.AddDays(-50+i), 
                    Open = price, 
                    Close = price+change, 
                    High = price+change+1, 
                    Low = price-1 
                });
                price += change;
            }
            return list;
        }
    }

    public class StockCandle { public DateTime Date; public decimal Open, High, Low, Close; }
}
