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
            lock (_lock)
            {
                _cache[symbol] = new QuoteSnapshot
                {
                    Symbol = symbol,
                    Price = price,
                    ChangePercent = changePercent,
                    CachedAt = DateTime.Now
                };
            }
        }
        
        public static QuoteSnapshot Get(string symbol)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(symbol, out var snapshot))
                {
                    var age = (DateTime.Now - snapshot.CachedAt).TotalSeconds;
                    if (age < CacheTTLSeconds)
                        return snapshot;
                }
                return null;
            }
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
