using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// OpenRouter AI Provider - Multi-model AI service
    /// Supports various models like Claude, GPT, Llama, etc.
    /// </summary>
    public class OpenRouterAiProvider : IAIProvider
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _conversationHistory;
        private const string OPENROUTER_API_URL = "https://openrouter.ai/api/v1/chat/completions";
        private const string DEFAULT_MODEL = "anthropic/claude-3.5-sonnet";
        
        public bool IsAvailable => !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-api-key-here";
        public string ProviderName => "OpenRouter (Multi-Model)";
        
        public OpenRouterAiProvider(string? apiKey = null)
        {
            _apiKey = apiKey ?? "";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://novabank.com");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "NovaBank AI Assistant");
            _conversationHistory = new List<ChatMessage>();
        }
        
        public async Task<string> AskAsync(AiRequest request)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException("OpenRouter API key not configured");
            }
            
            try
            {
                // Build system prompt with enhanced financial knowledge
                var systemPrompt = BuildEnhancedSystemPrompt(request);
                
                // Add user message to history
                _conversationHistory.Add(new ChatMessage { Role = "user", Content = request.UserMessage });
                
                // Build messages array
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };
                
                // Add conversation history (last 10 messages to avoid token limits)
                var recentHistory = _conversationHistory.Count > 10 
                    ? _conversationHistory.GetRange(_conversationHistory.Count - 10, 10)
                    : _conversationHistory;
                
                foreach (var msg in recentHistory)
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }
                
                // Build request
                var requestBody = new
                {
                    model = DEFAULT_MODEL,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 4000,
                    response_format = new { type = "text" }
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(OPENROUTER_API_URL, content);
                response.EnsureSuccessStatusCode();
                
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
                
                var answer = responseObj.GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
                
                // Add AI response to history
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = answer });
                
                return answer;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"OpenRouter API error: {ex.Message}", ex);
            }
        }
        
        private string BuildEnhancedSystemPrompt(AiRequest request)
        {
            var prompt = new StringBuilder();
            prompt.AppendLine("Sen NovaBank için uzman bir finansal AI asistanısın. Türkçe konuşuyorsun.");
            prompt.AppendLine();
            prompt.AppendLine("UZMANLIK ALANLARIN:");
            prompt.AppendLine("🏦 Bankacılık ve Yatırım Danışmanlığı");
            prompt.AppendLine("📈 Teknik ve Temel Analiz");
            prompt.AppendLine("💰 Portföy Yönetimi");
            prompt.AppendLine("📊 Grafik Analizi ve Pattern Recognition");
            prompt.AppendLine("🔍 Risk Yönetimi");
            prompt.AppendLine("💡 Finansal Strateji Geliştirme");
            prompt.AppendLine();
            prompt.AppendLine("ÖZELLİKLER:");
            prompt.AppendLine("- Detaylı teknik analiz yapabilirsin (RSI, MACD, Bollinger, Fibonacci)");
            prompt.AppendLine("- Grafik formasyonlarını tanırsın (Double Top, Head & Shoulders, Triangle)");
            prompt.AppendLine("- Destek/direnç seviyelerini hesaplayabilirsin");
            prompt.AppendLine("- Risk/reward oranları belirleyebilirsin");
            prompt.AppendLine("- Portföy optimizasyonu önerebilirsin");
            prompt.AppendLine();
            
            if (request.Context != null)
            {
                prompt.AppendLine("MEVCUT BAĞLAM:");
                prompt.AppendLine($"Kullanıcı: {request.Context.Username}");
                prompt.AppendLine($"Portföy Değeri: ₺{request.Context.TotalPortfolioValue:N0}");
                
                if (request.Context.StockData?.Count > 0)
                {
                    prompt.AppendLine("HİSSE SENEDİ VERİLERİ:");
                    foreach (var stock in request.Context.StockData)
                    {
                        prompt.AppendLine($"- {stock.Symbol}: ₺{stock.CurrentPrice:N2} ({stock.ChangePercent:+0.##}%)");
                    }
                }
                
                if (!string.IsNullOrEmpty(request.Context.RecentTransactions))
                {
                    prompt.AppendLine($"SON İŞLEMLER: {request.Context.RecentTransactions}");
                }
            }
            
            prompt.AppendLine();
            prompt.AppendLine("YANIT FORMATI:");
            prompt.AppendLine("1. Analizi net ve yapılandırılmış yap");
            prompt.AppendLine("2. Emoji kullanarak görsel zenginlik kat");
            prompt.AppendLine("3. Sayısal verileri formatla (₺1.234.567)");
            prompt.AppendLine("4. Risk sevielerini belirt (Düşük/Orta/Yüksek)");
            prompt.AppendLine("5. Somut eylem adımları öner");
            prompt.AppendLine();
            prompt.AppendLine("ÖRNEK YANIT:");
            prompt.AppendLine("📊 **GARAN ANALİZİ**");
            prompt.AppendLine("🔹 **Fiyat:** ₺45.67 (+2.3%)");
            prompt.AppendLine("🔹 **Teknik Göstergeler:**");
            prompt.AppendLine("   - RSI: 65 (Aşırı Alım Bölgesine Yaklaş)");
            prompt.AppendLine("   - MACD: Bullish Cross (Alım Sinyali)");
            prompt.AppendLine("🔹 **Destek/Direnç:**");
            prompt.AppendLine("   - Destek: ₺42.50");
            prompt.AppendLine("   - Direnç: ₺48.00");
            prompt.AppendLine("🔹 **Öneri:** HOLD (Risk: Orta)");
            prompt.AppendLine("📝 Not: ₺48 üzerinde kalıcılıkta yeni alım düşünülebilir.");
            
            return prompt.ToString();
        }
        
        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }
    }
}
