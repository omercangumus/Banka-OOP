using System;
using System.Collections.Generic;

namespace BankApp.UI.Services
{
    public class QuoteSnapshot
    {
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double ChangePercent { get; set; }
        public DateTime CachedAt { get; set; }
    }
    
    public static class QuoteCache
    {
        private static readonly Dictionary<string, QuoteSnapshot> _cache = new Dictionary<string, QuoteSnapshot>();
        private static readonly object _lock = new object();
        private const int CacheTTLSeconds = 90;
        
        public static void Set(string symbol, double price, double changePercent)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return;
            
            var key = NormalizeSymbol(symbol);
            lock (_lock)
            {
                _cache[key] = new QuoteSnapshot
                {
                    Symbol = key,
                    Price = price,
                    ChangePercent = changePercent,
                    CachedAt = DateTime.Now
                };
            }
        }
        
        public static QuoteSnapshot Get(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol)) return null;
            
            var key = NormalizeSymbol(symbol);
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var snapshot))
                {
                    var age = (DateTime.Now - snapshot.CachedAt).TotalSeconds;
                    if (age < CacheTTLSeconds)
                        return snapshot;
                }
                return null;
            }
        }
        
        private static string NormalizeSymbol(string symbol)
        {
            return symbol?.Trim().ToUpperInvariant() ?? string.Empty;
        }
        
        public static void Clear()
        {
            lock (_lock)
            {
                _cache.Clear();
            }
        }
    }
}
