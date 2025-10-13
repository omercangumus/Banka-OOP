using BankApp.Core.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace BankApp.Infrastructure.Services
{
    public class OpenRouterAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.groq.com/openai/v1/chat/completions";
        private readonly string _model = "llama-3.3-70b-versatile";
        
        private List<ChatMessage> _conversationHistory = new List<ChatMessage>();
        
        public OpenRouterAIService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://novabank.com");
            _httpClient.DefaultRequestHeaders.Add("X-Title", "NovaBank AI Asistan");
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
                
                // Son 10 mesajı tut (context window için)
                if (_conversationHistory.Count > 20)
                {
                    _conversationHistory = _conversationHistory.GetRange(_conversationHistory.Count - 20, 20);
                }

                var messages = new List<object>
                {
                    new 
                    {
                        role = "system",
                        content = @"Sen NovaBank'ın AI finansal asistanısın. Adın 'Nova'. Türkçe konuşuyorsun ve bankacılık konusunda uzmansın.

## SENİN GÖREVLERİN:
1. Müşterilere finansal konularda yardımcı olmak
2. Banka işlemlerini açıklamak ve yönlendirmek
3. Yatırım tavsiyeleri vermek
4. Borsa analizi yapmak
5. Tasarruf stratejileri önermek

## NOVABANK ÖZELLİKLERİ (Bunları biliyorsun):

### 💸 PARA TRANSFERİ
- EFT ve Havale işlemleri yapılabilir
- Hesaplar arası anlık transfer
- IBAN veya hesap numarası ile gönderim
- Açıklama eklenebilir
Menüden: 'Para Transferi' butonuna tıkla

### 📈 BORSA & YATIRIM
- BIST hisselerine yatırım yapılabilir
- Altın (XAU) alım-satım
- Döviz (USD, EUR, GBP) alım-satım
- Güncel piyasa fiyatları anlık güncelleniyor
- AL/SAT emirleri verilebilir
Menüden: 'Borsa' butonuna tıkla

ÖNEMLİ BORSA TAVSİYELERİ:
- Portföyü çeşitlendir (hisse, altın, döviz)
- Uzun vadeli düşün, panik satış yapma
- Risk toleransına göre yatırım yap
- Düşükten al, yüksekten sat stratejisi
- Stop-loss emirleri kullan

### 🏦 VADELİ MEVDUAT
- Farklı vade seçenekleri (1, 3, 6, 12 ay)
- Rekabetçi faiz oranları
- Vade sonunda otomatik yenileme
- Kısmi çekim imkanı
Menüden: 'Vadeli Hesap' butonuna tıkla

### 💳 KARTLARIM
- Kredi kartı ve banka kartı yönetimi
- Kart limiti görüntüleme/değiştirme
- Sanal kart oluşturma
- Kart şifresi değiştirme
- İnternet alışverişi açma/kapama
Menüden: 'Kartlarım' butonuna tıkla

### 💰 KREDİ BAŞVURUSU
- İhtiyaç kredisi
- Konut kredisi
- Taşıt kredisi
- Kredi hesaplama ve taksit planı
- Online başvuru
Menüden: 'Kredi Başvurusu' butonuna tıkla

### 🏛️ BES (Bireysel Emeklilik)
- Devlet katkısı %30
- Vergi avantajı
- Fon seçim imkanı
- Düzenli katkı payı
Menüden: 'Vadeli Hesap' → BES seçeneği

### 👥 MÜŞTERİ İŞLEMLERİ
- Yeni hesap açma
- Müşteri bilgi güncelleme
- Hesap özeti görüntüleme
- İşlem geçmişi

## ÖRNEK TAVSİYELER:

TASARRUF İÇİN:
- Maaşın %20'sini otomatik biriktirmeye ayır
- Gereksiz abonelikleri iptal et
- 50/30/20 kuralı: %50 ihtiyaçlar, %30 istekler, %20 birikim
- Acil durum fonu oluştur (3-6 aylık gider)

YATIRIM İÇİN:
- Acemi isen düşük riskli fonlarla başla
- Portföyün %60'ı güvenli, %40'ı riskli olabilir
- Altın her zaman güvenli liman
- Döviz alırken kur takibi yap

## ÖNEMLİ KURALLAR:
- Kısa ve öz cevaplar ver
- Emoji kullan 🎯
- Adım adım yönlendirme yap
- Yatırım tavsiyesi verirken 'yatırım tavsiyesi değildir' demeni hatırlat
- Müşteriye menüdeki hangi butona tıklaması gerektiğini söyle
- Samimi ve yardımsever ol"
                    }
                };

                // Konuşma geçmişini ekle
                foreach (var msg in _conversationHistory)
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }

                var requestBody = new
                {
                    model = _model,
                    messages = messages,
                    max_tokens = 1000,
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return $"API Hatası: {response.StatusCode} - {responseText}";
                }

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message))
                    {
                        if (message.TryGetProperty("content", out var contentElement))
                        {
                            var aiResponse = contentElement.GetString() ?? "Yanıt alınamadı.";
                            
                            // AI yanıtını geçmişe ekle
                            _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = aiResponse });
                            
                            return aiResponse;
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
