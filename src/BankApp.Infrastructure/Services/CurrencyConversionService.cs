using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Dapper;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// S3 FIX: Tek kaynaklı para birimi dönüşüm servisi
    /// Tüm hesaplamalar TRY bazında yapılır
    /// </summary>
    public class CurrencyConversionService
    {
        private readonly DapperContext _context;
        
        // Default kur değerleri (config/DB'den okunabilir)
        private static decimal _usdTryRate = 34.50m;
        private static decimal _eurTryRate = 37.80m;
        private static DateTime _lastUpdate = DateTime.MinValue;
        
        public CurrencyConversionService(DapperContext context)
        {
            _context = context;
        }
        
        public CurrencyConversionService()
        {
            _context = new DapperContext();
        }
        
        /// <summary>
        /// USD/TRY kuru - güncel veya cache
        /// </summary>
        public static decimal UsdTryRate => _usdTryRate;
        
        /// <summary>
        /// EUR/TRY kuru
        /// </summary>
        public static decimal EurTryRate => _eurTryRate;
        
        /// <summary>
        /// USD tutarını TRY'ye çevirir
        /// </summary>
        public static decimal ConvertUsdToTry(decimal usdAmount)
        {
            var tryAmount = usdAmount * _usdTryRate;
            Debug.WriteLine($"[DATA] MoneyConvert src=USD amount={usdAmount:N2} usdTry={_usdTryRate:N2} try={tryAmount:N2}");
            return tryAmount;
        }
        
        /// <summary>
        /// EUR tutarını TRY'ye çevirir
        /// </summary>
        public static decimal ConvertEurToTry(decimal eurAmount)
        {
            var tryAmount = eurAmount * _eurTryRate;
            Debug.WriteLine($"[DATA] MoneyConvert src=EUR amount={eurAmount:N2} eurTry={_eurTryRate:N2} try={tryAmount:N2}");
            return tryAmount;
        }
        
        /// <summary>
        /// Herhangi bir para birimini TRY'ye çevirir
        /// </summary>
        public static decimal ConvertToTry(decimal amount, string currency)
        {
            currency = currency?.ToUpperInvariant() ?? "TRY";
            
            return currency switch
            {
                "USD" => ConvertUsdToTry(amount),
                "USDT" => ConvertUsdToTry(amount), // USDT ~ USD
                "EUR" => ConvertEurToTry(amount),
                "TRY" => amount,
                "TL" => amount,
                _ => amount // Bilinmeyen para birimi - direkt kullan
            };
        }
        
        /// <summary>
        /// Sembol bazında para birimini belirler
        /// </summary>
        public static string GetCurrencyForSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return "TRY";
            
            symbol = symbol.ToUpperInvariant();
            
            // Türk hisseleri (BIST)
            if (symbol.EndsWith(".IS") || symbol.EndsWith(".E"))
                return "TRY";
            
            // Kripto (genelde USD bazlı)
            if (symbol.Contains("BTC") || symbol.Contains("ETH") || 
                symbol.Contains("USDT") || symbol.Contains("BNB"))
                return "USD";
            
            // Amerikan hisseleri
            if (symbol == "AAPL" || symbol == "GOOGL" || symbol == "MSFT" || 
                symbol == "AMZN" || symbol == "TSLA" || symbol == "META" ||
                symbol == "NVDA" || symbol == "AMD" || symbol == "NFLX")
                return "USD";
            
            // Default: USD (çoğu uluslararası hisse)
            return "USD";
        }
        
        /// <summary>
        /// Sembol ve fiyat verildiğinde TRY karşılığını hesaplar
        /// </summary>
        public static decimal GetTryValue(string symbol, decimal priceInOriginalCurrency, decimal quantity)
        {
            var currency = GetCurrencyForSymbol(symbol);
            var totalOriginal = priceInOriginalCurrency * quantity;
            var totalTry = ConvertToTry(totalOriginal, currency);
            
            Debug.WriteLine($"[DATA] MoneyConvert symbol={symbol} qty={quantity:N2} price={priceInOriginalCurrency:N2} currency={currency} totalTry={totalTry:N2}");
            
            return totalTry;
        }
        
        /// <summary>
        /// Kurları günceller (API'den veya DB'den)
        /// </summary>
        public async Task RefreshRatesAsync()
        {
            try
            {
                // Önce DB'den kontrol et
                using var conn = _context.CreateConnection();
                
                var usdRate = await conn.QueryFirstOrDefaultAsync<decimal?>(
                    @"SELECT ""Rate"" FROM ""ExchangeRates"" WHERE ""FromCurrency"" = 'USD' AND ""ToCurrency"" = 'TRY' LIMIT 1");
                
                if (usdRate.HasValue && usdRate.Value > 0)
                {
                    _usdTryRate = usdRate.Value;
                    Debug.WriteLine($"[DATA] ExchangeRate USD/TRY={_usdTryRate:N2} (from DB)");
                }
                
                var eurRate = await conn.QueryFirstOrDefaultAsync<decimal?>(
                    @"SELECT ""Rate"" FROM ""ExchangeRates"" WHERE ""FromCurrency"" = 'EUR' AND ""ToCurrency"" = 'TRY' LIMIT 1");
                
                if (eurRate.HasValue && eurRate.Value > 0)
                {
                    _eurTryRate = eurRate.Value;
                    Debug.WriteLine($"[DATA] ExchangeRate EUR/TRY={_eurTryRate:N2} (from DB)");
                }
                
                _lastUpdate = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[WARN] ExchangeRate refresh failed: {ex.Message} - using defaults");
            }
        }
        
        /// <summary>
        /// Kurları manuel set eder (test/config için)
        /// </summary>
        public static void SetRates(decimal usdTry, decimal eurTry)
        {
            _usdTryRate = usdTry;
            _eurTryRate = eurTry;
            _lastUpdate = DateTime.UtcNow;
            Debug.WriteLine($"[DATA] ExchangeRates SET: USD/TRY={_usdTryRate:N2}, EUR/TRY={_eurTryRate:N2}");
        }
    }
}
