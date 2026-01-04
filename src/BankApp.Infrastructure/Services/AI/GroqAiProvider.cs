using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Groq AI Provider - Uses Groq API for fast LLM inference
    /// API Key should be set via environment variable GROQ_API_KEY
    /// </summary>
    public class GroqAiProvider : IAIProvider
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly List<ChatMessage> _conversationHistory;
        private const string GROQ_API_URL = "https://api.groq.com/openai/v1/chat/completions";
        private const string MODEL = "llama-3.1-70b-versatile";
        
        public bool IsAvailable => !string.IsNullOrEmpty(_apiKey) && _apiKey != "your-api-key-here";
        public string ProviderName => "Groq (LLaMA 3.1)";
        
        public GroqAiProvider(string? apiKey = null)
        {
            _apiKey = apiKey ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _conversationHistory = new List<ChatMessage>();
        }
        
        public async Task<string> AskAsync(AiRequest request)
        {
            if (!IsAvailable)
            {
                throw new InvalidOperationException("Groq API key not configured");
            }
            
            try
            {
                // Build system prompt with context
                var systemPrompt = BuildSystemPrompt(request);
                
                // Add user message to history
                _conversationHistory.Add(new ChatMessage { Role = "user", Content = request.UserMessage });
                
                // Build messages array
                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };
                
                // Add conversation history (last 10 messages)
                var historyStart = Math.Max(0, _conversationHistory.Count - 10);
                for (int i = historyStart; i < _conversationHistory.Count; i++)
                {
                    messages.Add(new { role = _conversationHistory[i].Role, content = _conversationHistory[i].Content });
                }
                
                var requestBody = new
                {
                    model = MODEL,
                    messages = messages,
                    temperature = 0.7,
                    max_tokens = 1500
                };
                
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(GROQ_API_URL, content);
                var responseJson = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Groq API error: {response.StatusCode}");
                }
                
                using var doc = JsonDocument.Parse(responseJson);
                var assistantMessage = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "Yanıt alınamadı";
                
                // Add assistant response to history
                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = assistantMessage });
                
                return assistantMessage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Groq API Error: {ex.Message}");
                throw;
            }
        }
        
        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }
        
        private string BuildSystemPrompt(AiRequest request)
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("Sen NovaBank için bir yapay zeka finansal asistansın. Türkçe yanıt ver.");
            sb.AppendLine();
            sb.AppendLine("KURALLAR:");
            sb.AppendLine("1. Sadece verilen verilere dayanarak analiz yap");
            sb.AppendLine("2. Kesin alım/satım tavsiyeleri VERME, sadece bilgilendirici ol");
            sb.AppendLine("3. Risk uyarılarını her zaman ekle");
            sb.AppendLine("4. Kısa ve öz ol, gereksiz uzatma");
            sb.AppendLine("5. Emoji kullanarak okunabilirliği artır");
            sb.AppendLine();
            sb.AppendLine("ÇIKTI FORMATI:");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("[BAŞLIK]");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine("📋 ÖZET: (2-3 cümle)");
            sb.AppendLine();
            sb.AppendLine("📊 BULGULAR:");
            sb.AppendLine("• Madde 1");
            sb.AppendLine("• Madde 2");
            sb.AppendLine();
            sb.AppendLine("⚠️ RİSK/UYARILAR:");
            sb.AppendLine("• Risk 1");
            sb.AppendLine();
            sb.AppendLine("💡 ÖNERİLEN AKSİYONLAR:");
            sb.AppendLine("• İncele/izle dili kullan");
            sb.AppendLine();
            
            // Add context data
            var ctx = request.Context;
            sb.AppendLine("KULLANICI VERİLERİ:");
            sb.AppendLine($"- Kullanıcı: {ctx.Username}");
            sb.AppendLine($"- Net Varlık: ₺{ctx.NetWorth:N2}");
            sb.AppendLine($"- Toplam Bakiye: ₺{ctx.TotalBalance:N2}");
            sb.AppendLine($"- Kar/Zarar: ₺{ctx.TotalProfit:N2} ({ctx.ProfitPercent:+0.00;-0.00}%)");
            sb.AppendLine($"- Hesap Sayısı: {ctx.AccountCount}");
            sb.AppendLine($"- Son İşlem Sayısı: {ctx.RecentTransactionCount}");
            sb.AppendLine($"- Son Dönem Harcama: ₺{ctx.TotalSpending:N2}");
            
            if (!string.IsNullOrEmpty(ctx.SpendingByCategory))
            {
                sb.AppendLine($"- Harcama Dağılımı: {ctx.SpendingByCategory}");
            }
            
            // Stock data for market analysis
            if (!string.IsNullOrEmpty(ctx.StockSymbol))
            {
                sb.AppendLine();
                sb.AppendLine("HİSSE VERİLERİ:");
                sb.AppendLine($"- Sembol: {ctx.StockSymbol}");
                sb.AppendLine($"- Fiyat: ${ctx.StockPrice:N2}");
                sb.AppendLine($"- Değişim: {ctx.StockChangePercent:+0.00;-0.00}%");
                
                if (!string.IsNullOrEmpty(ctx.StockNews))
                {
                    sb.AppendLine($"- Son Haberler: {ctx.StockNews}");
                }
                
                sb.AppendLine();
                sb.AppendLine("Borsa analizi için ek format:");
                sb.AppendLine("🎯 DESTEK/DİRENÇ: Seviyeleri belirt");
                sb.AppendLine("📈 TREND: Yükseliş/Düşüş/Yatay");
                sb.AppendLine("📊 TEKNİK: RSI/MACD yorumu (genel)");
            }
            
            if (!string.IsNullOrEmpty(request.Topic))
            {
                sb.AppendLine();
                sb.AppendLine($"KONU: {request.Topic}");
            }
            
            return sb.ToString();
        }
        
        private class ChatMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }
    }
}
