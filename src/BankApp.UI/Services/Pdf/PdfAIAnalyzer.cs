using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BankApp.UI.Services.Pdf
{
    public static class PdfAIAnalyzer
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static string _apiKey;
        
        static PdfAIAnalyzer()
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.local.json", optional: true)
                    .Build();
                    
                _apiKey = config["Groq:ApiKey"] ?? Environment.GetEnvironmentVariable("GROQ_API_KEY") ?? "";
            }
            catch
            {
                _apiKey = "";
            }
        }
        
        public static async Task<(string analysis, string recommendation, string confidence)> GetAIAnalysisAsync(InvestmentAnalysisData data)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return GetFallbackAnalysis(data);
            }
            
            try
            {
                var prompt = $@"Sen bir finansal analistsin. Aşağıdaki hisse senedi verilerini analiz et ve KISA bir yatırım önerisi ver.

Sembol: {data.Symbol}
Son Fiyat: ${data.LastPrice:N2}
Değişim: {data.ChangePercent:N2}%
Açılış: ${data.Open:N2}
Yüksek: ${data.High:N2}
Düşük: ${data.Low:N2}
RSI: {data.RSI}
MACD: {data.MACD}

Lütfen şu formatta yanıt ver:
ANALIZ: (2-3 cümle teknik analiz)
ÖNERI: (BUY/SELL/HOLD)
GÜVEN: (High/Medium/Low)";

                var request = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = "Sen kısa ve öz yanıtlar veren bir finansal analistsin. Türkçe yanıt ver." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = 300,
                    temperature = 0.3
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
                
                var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    return GetFallbackAnalysis(data);
                }
                
                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);
                var aiResponse = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "";
                
                return ParseAIResponse(aiResponse, data);
            }
            catch
            {
                return GetFallbackAnalysis(data);
            }
        }
        
        private static (string analysis, string recommendation, string confidence) ParseAIResponse(string response, InvestmentAnalysisData data)
        {
            var analysis = "AI analizi mevcut değil.";
            var recommendation = "HOLD";
            var confidence = "Medium";
            
            try
            {
                var lines = response.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("ANALIZ:", StringComparison.OrdinalIgnoreCase))
                        analysis = line.Substring(7).Trim();
                    else if (line.StartsWith("ÖNERI:", StringComparison.OrdinalIgnoreCase) || line.StartsWith("ÖNERİ:", StringComparison.OrdinalIgnoreCase))
                        recommendation = line.Substring(6).Trim().ToUpper();
                    else if (line.StartsWith("GÜVEN:", StringComparison.OrdinalIgnoreCase))
                        confidence = line.Substring(6).Trim();
                }
                
                // Normalize recommendation
                if (recommendation.Contains("BUY") || recommendation.Contains("AL"))
                    recommendation = "BUY";
                else if (recommendation.Contains("SELL") || recommendation.Contains("SAT"))
                    recommendation = "SELL";
                else
                    recommendation = "HOLD";
            }
            catch
            {
                return GetFallbackAnalysis(data);
            }
            
            return (analysis, recommendation, confidence);
        }
        
        private static (string analysis, string recommendation, string confidence) GetFallbackAnalysis(InvestmentAnalysisData data)
        {
            string analysis;
            string recommendation;
            string confidence;
            
            if (data.ChangePercent > 2)
            {
                analysis = $"{data.Symbol} bugün güçlü bir yükseliş gösteriyor. Momentum pozitif görünüyor ancak aşırı alım bölgesine dikkat edilmeli.";
                recommendation = "HOLD";
                confidence = "Medium";
            }
            else if (data.ChangePercent < -2)
            {
                analysis = $"{data.Symbol} bugün satış baskısı altında. Destek seviyelerinin takibi önemli. Dipten alım fırsatı olabilir.";
                recommendation = "HOLD";
                confidence = "Low";
            }
            else
            {
                analysis = $"{data.Symbol} yatay bir seyir izliyor. Net bir trend sinyali yok. Bekle-gör stratejisi önerilir.";
                recommendation = "HOLD";
                confidence = "Medium";
            }
            
            return (analysis, recommendation, confidence);
        }
    }
}
