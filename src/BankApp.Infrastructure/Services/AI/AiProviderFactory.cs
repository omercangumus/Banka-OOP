using System;
using System.IO;
using System.Text.Json;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Factory for creating AI providers based on configuration
    /// </summary>
    public static class AiProviderFactory
    {
        private static string? _cachedApiKey;
        
        /// <summary>
        /// Create the appropriate AI provider based on available API keys
        /// </summary>
        public static IAIProvider CreateProvider()
        {
            var groqKey = GetApiKey();
            if (!string.IsNullOrEmpty(groqKey))
            {
                var groqProvider = new GroqAiProvider(groqKey);
                if (groqProvider.IsAvailable)
                {
                    return groqProvider;
                }
            }
            
            // Fallback to offline provider
            return new OfflineAiProvider();
        }
        
        /// <summary>
        /// Get API key from config file or environment variable
        /// </summary>
        private static string? GetApiKey()
        {
            if (_cachedApiKey != null)
                return _cachedApiKey;
            
            // 1. Try environment variable first
            var envKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
            if (!string.IsNullOrEmpty(envKey) && envKey != "your-api-key-here")
            {
                _cachedApiKey = envKey;
                return _cachedApiKey;
            }
            
            // 2. Try appsettings.local.json in app directory
            try
            {
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                var configPath = Path.Combine(appDir, "appsettings.local.json");
                
                // Also check source directory during development
                if (!File.Exists(configPath))
                {
                    var devPath = Path.Combine(appDir, "..", "..", "..", "appsettings.local.json");
                    if (File.Exists(devPath))
                        configPath = devPath;
                }
                
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("AI", out var aiSection) &&
                        aiSection.TryGetProperty("GroqApiKey", out var keyElement))
                    {
                        var key = keyElement.GetString();
                        if (!string.IsNullOrEmpty(key) && key != "your-api-key-here")
                        {
                            _cachedApiKey = key;
                            return _cachedApiKey;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Config read error: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if online AI is available
        /// </summary>
        public static bool IsOnlineAvailable()
        {
            return !string.IsNullOrEmpty(GetApiKey());
        }
        
        /// <summary>
        /// Get status information for display
        /// </summary>
        public static (bool IsOnline, string ProviderName, string StatusText) GetStatus()
        {
            var provider = CreateProvider();
            bool isOnline = provider is GroqAiProvider && provider.IsAvailable;
            
            return (
                isOnline,
                provider.ProviderName,
                isOnline ? "Bağlı" : "Çevrimdışı"
            );
        }
    }
}
