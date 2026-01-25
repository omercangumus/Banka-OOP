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
            var openRouterKey = GetOpenRouterApiKey();
            if (!string.IsNullOrEmpty(openRouterKey))
            {
                var openRouterProvider = new OpenRouterAiProvider(openRouterKey);
                if (openRouterProvider.IsAvailable)
                {
                    return openRouterProvider;
                }
            }
            
            // Fallback to offline provider
            return new OfflineAiProvider();
        }
        
        /// <summary>
        /// Get OpenRouter API key from config file or environment variable
        /// </summary>
        private static string? GetOpenRouterApiKey()
        {
            if (_cachedApiKey != null)
                return _cachedApiKey;
            
            // 1. Try environment variable first
            var envKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
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
                        aiSection.TryGetProperty("OpenRouterApiKey", out var keyElement))
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
                System.Diagnostics.Debug.WriteLine($"[AI-FACTORY] Error loading OpenRouter API key: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Check if any API key is configured
        /// </summary>
        public static bool HasApiKey()
        {
            return !string.IsNullOrEmpty(GetOpenRouterApiKey());
        }
        
        /// <summary>
        /// Check if online AI is available (for backward compatibility)
        /// </summary>
        public static bool IsOnlineAvailable()
        {
            var provider = CreateProvider();
            return provider.IsAvailable && !(provider is OfflineAiProvider);
        }
        
        /// <summary>
        /// Get status information for display
        /// </summary>
        public static (bool IsOnline, string ProviderName, string StatusText) GetStatus()
        {
            var provider = CreateProvider();
            bool isOnline = provider is OpenRouterAiProvider && provider.IsAvailable;
            
            return (
                isOnline,
                provider.ProviderName,
                isOnline ? "Bağlı" : "Çevrimdışı"
            );
        }
    }
}
