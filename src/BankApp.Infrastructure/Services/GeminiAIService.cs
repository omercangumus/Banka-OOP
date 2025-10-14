using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    public class GeminiAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";
        
        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();

        public GeminiAIService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetFinancialAdviceAsync(int userId)
        {
            return await GetResponseAsync("Bana finansal öneriler ver. Tasarruf ve yatırım konusunda tavsiyeler sun.");
        }

        public async Task<string> GetResponseAsync(string query)
        {
            try
            {
                // Kullanıcı mesajını geçmişe ekle
                _conversationHistory.Add(new ChatMessage { Role = "user", Content = query });
                
                // Son 10 mesajı tut
                if (_conversationHistory.Count > 20)
                {
                    _conversationHistory = _conversationHistory.GetRange(_conversationHistory.Count - 20, 20);
                }

                // System instruction for function calling
                string systemInstruction = @"Sen NovaBank'ın yapay zeka asistanısın. Kullanıcıya yardımcı olmak için gerçek işlem yapabilirsin.

YAPABİLECEĞİN İŞLEMLER:
1. Para Transferi
2. Hesap Açma
3. Kredi Kartı Başvurusu

Eğer kullanıcı bu işlemlerden birini isterse, cevabını SADECE şu JSON formatında ver (başka açıklama ekleme):
{""action"": ""TRANSFER"", ""amount"": 1000, ""iban"": ""TR33..."", ""description"": ""Açıklama""}
{""action"": ""OPEN_ACCOUNT"", ""accountType"": ""Checking"", ""currency"": ""TRY""}
{""action"": ""CREDIT_CARD"", ""limit"": 5000}

Eğer işlem talebi değil, normal sohbet ise normal Türkçe cevap ver. JSON komutları haricinde teknik terim kullanma.

ÖRNEKLER:
Kullanıcı: ""100 TL transfer et TR123'e kira için""
Sen: {""action"": ""TRANSFER"", ""amount"": 100, ""iban"": ""TR123..."", ""description"": ""Kira""}

Kullanıcı: ""Dolar hesabı aç""
Sen: {""action"": ""OPEN_ACCOUNT"", ""accountType"": ""Checking"", ""currency"": ""USD""}

Kullanıcı: ""Merhaba nasılsın""
Sen: Merhaba! Ben NovaBank AI asistanıyım, size nasıl yardımcı olabilirim?

NOT: İşlem komutları için SADECE JSON dön, başka bir şey ekleme!";

                // Gemini API request body
                var contents = new List<object>();
                
                // Add system instruction
                contents.Add(new
                {
                    role = "user",
                    parts = new[] { new { text = systemInstruction } }
                });
                
                contents.Add(new
                {
                    role = "model",
                    parts = new[] { new { text = "Anladım, işlem komutları için sadece JSON, sohbet için normal Türkçe cevap vereceğim." } }
                });

                // Add conversation history
                foreach (var msg in _conversationHistory)
                {
                    contents.Add(new
                    {
                        role = msg.Role == "user" ? "user" : "model",
                        parts = new[] { new { text = msg.Content } }
                    });
                }

                var requestBody = new
                {
                    contents = contents,
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 1000
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}?key={_apiKey}", content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"API Hatası: {response.StatusCode} - {responseText}";
                }

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var contentObj))
                    {
                        if (contentObj.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            var firstPart = parts[0];
                            if (firstPart.TryGetProperty("text", out var textElement))
                            {
                                var aiResponse = textElement.GetString() ?? "Yanıt alınamadı.";
                                
                                // AI yanıtını geçmişe ekle
                                _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
                                
                                return aiResponse;
                            }
                        }
                    }
                }

                return "AI yanıtı işlenemedi.";
            }
            catch (HttpRequestException ex)
            {
                return $"Bağlantı hatası: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Hata: {ex.Message}";
            }
        }

        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }

        private class ChatMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
        }
    }
}
