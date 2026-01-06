using System;
using System.IO;
using System.Text.Json;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Feature Flag Service - Reads configuration from appsettings.json or environment
    /// </summary>
    public static class FeatureFlags
    {
        private static bool? _enableAITradingMode;
        
        /// <summary>
        /// AI Trading Mode - When enabled, AI can execute trade-related tools
        /// Default: OFF (false)
        /// </summary>
        public static bool EnableAITradingMode
        {
            get
            {
                if (!_enableAITradingMode.HasValue)
                {
                    _enableAITradingMode = LoadFeatureFlag("EnableAITradingMode", false);
                }
                return _enableAITradingMode.Value;
            }
        }
        
        /// <summary>
        /// Reload feature flags from config
        /// </summary>
        public static void Reload()
        {
            _enableAITradingMode = null;
        }
        
        private static bool LoadFeatureFlag(string flagName, bool defaultValue)
        {
            // 1. Check environment variable first
            var envValue = Environment.GetEnvironmentVariable($"NOVABANK_{flagName.ToUpper()}");
            if (!string.IsNullOrEmpty(envValue))
            {
                if (bool.TryParse(envValue, out bool envResult))
                    return envResult;
                if (envValue == "1") return true;
                if (envValue == "0") return false;
            }
            
            // 2. Check appsettings.json
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("Features", out var features))
                    {
                        if (features.TryGetProperty(flagName, out var flagValue))
                        {
                            if (flagValue.ValueKind == JsonValueKind.True) return true;
                            if (flagValue.ValueKind == JsonValueKind.False) return false;
                        }
                    }
                }
            }
            catch
            {
                // Config read error, use default
            }
            
            return defaultValue;
        }
    }
}
