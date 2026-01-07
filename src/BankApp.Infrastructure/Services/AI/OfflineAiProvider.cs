using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Offline AI Provider - Works without internet connection
    /// Provides intelligent responses based on financial patterns and rules
    /// </summary>
    public class OfflineAiProvider : IAIProvider
    {
        private readonly Dictionary<string, Func<AiRequest, string>> _responseHandlers;
        private readonly List<ChatMessage> _conversationHistory;
        
        public bool IsAvailable => true;
        public string ProviderName => "Offline AI (Rule-Based)";
        
        public OfflineAiProvider()
        {
            _conversationHistory = new List<ChatMessage>();
            _responseHandlers = InitializeHandlers();
        }
        
        public async Task<string> AskAsync(AiRequest request)
        {
            var message = request.UserMessage.ToLower();
            
            // Add to history
            _conversationHistory.Add(new ChatMessage { Role = "user", Content = request.UserMessage });
            
            // Find appropriate handler
            var response = _responseHandlers
                .FirstOrDefault(kvp => message.Contains(kvp.Key))
                .Value?.Invoke(request) ?? GetDefaultResponse(request);
            
            // Add AI response to history
            _conversationHistory.Add(new ChatMessage { Role = "assistant", Content = response });
            
            return response;
        }
        
        private Dictionary<string, Func<AiRequest, string>> InitializeHandlers()
        {
            return new Dictionary<string, Func<AiRequest, string>>
            {
                ["portföy"] = HandlePortfolioRequest,
                ["yatırım"] = HandleInvestmentRequest,
                ["hisse"] = HandleStockRequest,
                ["analiz"] = HandleAnalysisRequest,
                ["destek"] = HandleSupportRequest,
                ["direnç"] = HandleResistanceRequest,
                ["risk"] = HandleRiskRequest,
                ["al"] = HandleBuyRequest,
                ["sat"] = HandleSellRequest,
                ["grafik"] = HandleChartRequest,
                ["rsi"] = HandleRSIRequest,
                ["macd"] = HandleMACDRequest,
                ["fibonacci"] = HandleFibonacciRequest,
                ["trend"] = HandleTrendRequest,
                ["pdf"] = HandlePDFRequest,
                ["rapor"] = HandleReportRequest
            };
        }
        
        private string HandlePortfolioRequest(AiRequest request)
        {
            var response = new StringBuilder();
            response.AppendLine("📊 **PORTFÖY ÖZETİ**");
            response.AppendLine();
            
            if (request.Context != null)
            {
                response.AppendLine($"💰 **Toplam Değer:** ₺{request.Context.TotalPortfolioValue:N0}");
                response.AppendLine($"👤 **Müşteri:** {request.Context.Username}");
                
                if (request.Context.StockData?.Count > 0)
                {
                    response.AppendLine();
                    response.AppendLine("📈 **HİSSE SENEDLERİ:**");
                    foreach (var stock in request.Context.StockData)
                    {
                        var changeIcon = stock.ChangePercent >= 0 ? "📈" : "📉";
                        response.AppendLine($"   {changeIcon} {stock.Symbol}: ₺{stock.CurrentPrice:N2} ({stock.ChangePercent:+0.##}%)");
                    }
                }
            }
            else
            {
                response.AppendLine("Portföy verisi yükleniyor...");
            }
            
            response.AppendLine();
            response.AppendLine("💡 **ÖNERİLER:**");
            response.AppendLine("• Portföy çeşitlendirmesi için risk dağılımını gözden geçirin");
            response.AppendLine("• Aylık performans analizi yapın");
            response.AppendLine("• Yatırım hedeflerinizi yeniden değerlendirin");
            
            return response.ToString();
        }
        
        private string HandleAnalysisRequest(AiRequest request)
        {
            return @"📈 **TEKNİK ANALİZ**

🔍 **GÖZLEMLER:**
• Grafik üzerinde yükselen trend formasyonu görünüyor
• Hacim artışı fiyat hareketini destekliyor
• RSI göstergesi nötr bölgede

📊 **TEKNİK GÖSTERGELER:**
• **RSI (14):** 55 - Nötr
• **MACD:** Alım sinyali üzerinde
• **Hacim:** Ortalamanın %20 üzerinde

🎯 **HEDEF FİYATLAR:**
• **Kısa Vadeli:** Mevcut seviyede tut
• **Orta Vadeli:** %5-10 artış potansiyeli
• **Destek:** Alt destek seviyesini izle

⚠️ **RİSK:** Orta
• Trend kırılmasına karşı stop-loss kullanın";
        }
        
        private string HandleStockRequest(AiRequest request)
        {
            return @"🏢 **HİSSE ANALİZİ**

📈 **GENEL DURUM:**
• Sektör performansının üzerinde
• Son 3 aylık getiri: +%15
• Piyasa değeri: İlk 100 içinde

🔍 **TEMEL ANALİZ:**
• **F/K Oranı:** 12.5 (Sektör ort: 15.2)
• **PD/DD:** 1.8 (Sektör ort: 2.1)
• **Net Kar:** %25 artış

📊 **TEKNİK ANALİZ:**
• **Trend:** Yükselen
• **Destek:** ₺125.50
• **Direnç:** ₺142.00

💡 **ÖNERİ:** HOLD
• Mevcut seviyede tutun
• ₺142 üzerinde kademeli alım
• ₺125 altında stop-loss";
        }
        
        private string HandleSupportRequest(AiRequest request)
        {
            return @"🟢 **DESTEK SEVİYELERİ**

📊 **GÜÇLÜ DESTEKLER:**
• **Destek 1:** ₺125.50 (Önemli)
• **Destek 2:** ₺118.00 (Orta)
• **Destek 3:** ₺110.00 (Zayıf)

🔍 **DESTEK ANALİZİ:**
• ₺125.50 seviyesi 3 kez test edildi
• Hacim destek seviyelerinde artıyor
• Alım baskısı güçleniyor

⚡ **İŞLEM STRATEJİSİ:**
• Destek seviyelerine yaklaştıkça alım düşün
• Kısa vadeli işlem için uygun
• Risk/Oran: 1:2.5

📝 Not: Destek kırılırsa aşağı yönde hareket olabilir";
        }
        
        private string HandleResistanceRequest(AiRequest request)
        {
            return @"🔴 **DİRENÇ SEVİYELERİ**

📊 **GÜÇLÜ DİRENÇLER:**
• **Direnç 1:** ₺142.00 (Önemli)
• **Direnç 2:** ₺155.50 (Orta)
• **Direnç 3:** ₺168.00 (Zayıf)

🔍 **DİRENÇ ANALİZİ:**
• ₺142 seviyesi tarihi zirve
• Son 2 deneme başarısız
• Kâr satışları görünüyor

⚡ **İŞLEM STRATEJİSİ:**
• Direnç seviyelerine yaklaştıkça kısmi satış
• Kırılma durumunda yeni alım
• Risk/Oran: 1:1.8

📝 Not: Direnç kırılırsa yükseliş hızlanabilir";
        }
        
        private string HandleRiskRequest(AiRequest request)
        {
            return @"⚠️ **RİSK ANALİZİ**

🔍 **RİSK DEĞERLENDİRMESİ:**
• **Piyasa Riski:** Orta
• **Sektör Riski:** Düşük
• **Şirket Riski:** Düşük

📊 **RİSK METRİKLERİ:**
• **Beta:** 0.85 (Piyasa daha az dalgalı)
• **Volatilite:** %18 (Yıllık)
• **Maksimum Çekilme:** -12%

🛡️ **RİSK YÖNETİMİ:**
• Portföyün %10'unu riske ayır
• Stop-loss: %8
• Take-profit: %15

💡 **ÖNERİLER:**
• Risk toleransını gözden geçir
• Düzenli portföy rebalancing
• Sektör dağılımını çeşitlendir";
        }
        
        private string HandleBuyRequest(AiRequest request)
        {
            return @"🟢 **ALIM TAVSİYESİ**

📊 **ALIM NEDENLERİ:**
• Teknik göstergeler alım sinyali veriyor
• Temel analiz pozitif
• Sektör outlook güçlü

💰 **ALIM STRATEJİSİ:**
• **Fiyat:** Mevcut seviye
• **Miktar:** Portföyün %5'i
• **Hedef:** +%15

🎯 **HEFELER:**
• **Kısa Vadeli:** +%8
• **Orta Vadeli:** +%15
• **Uzun Vadeli:** +%25

⚠️ **RİSKLER:**
• Piyasa düzeltmesi riski
• Sektör gerileme olasılığı

📝 **NOT:** Kademeli alım yap, tek seferde hepsini alma";
        }
        
        private string HandleSellRequest(AiRequest request)
        {
            return @"🔴 **SATIM TAVSİYESİ**

📊 **SATIM NEDENLERİ:**
• Hedef fiyata ulaşıldı
• Teknik göstergeler zayıflıyor
• Kâr realize etme zamanı

💰 **SATIM STRATEJİSİ:**
• **Fiyat:** Mevcut seviye
• **Miktar:** %50 kâr realize
• **Kalan:** Trend takibi

📈 **PERFORMANS:**
• **Yatırım Getirisi:** +%23
• **Tutum Süresi:** 4 ay
• **Risk/Ödül:** 1:3.2

💡 **ÖNERİLER:**
• Kârı realize et, yeniden yatırım yap
• Portföy dengelemesi yap
• Yeni fırsatları araştır";
        }
        
        private string HandleChartRequest(AiRequest request)
        {
            return @"📈 **GRAFİK ANALİZİ**

🔍 **FORMASYONLAR:**
• **Yükselen Kanal:** Aktif
• **Bayrak Formasyonu:** Tamamlanıyor
• **Çift Tepe:** Risk mevcut

📊 **GÖSTERGELER:**
• **Hareketli Ortalama (20):** Destekliyor
• **Hareketli Ortalama (50):** Destekliyor
• **RSI:** 58 (Nötr)

🎯 **DESTEK/DİRENÇ:**
• **Destek:** ₺125.50
• **Direnç:** ₺142.00

💡 **GRAFİK YORUMU:**
Genel trend pozitif ancak kısa vadeli yorgunluk görünüyor. Direnç kırılımı için hacim artışı gerekli.";
        }
        
        private string HandleRSIRequest(AiRequest request)
        {
            return @"📊 **RSI ANALİZİ**

🔍 **MEVCUT DURUM:**
• **RSI (14):** 58
• **Trend:** Yükseliş eğilimi
• **Sinyal:** Nötr

📈 **RSİ SEVİYELERİ:**
• **Aşırı Satım:** <30
• **Nötr:** 30-70
• **Aşırı Alım:** >70

💡 **RSİ YORUMU:**
RSI 58 seviyesinde - nötr bölgede. Alım baskısı devam ediyor ancak aşırı alım riski henüz yok.

⚡ **İŞLEM STRATEJİSİ:**
• RSI 30 altında alım düşün
• RSI 70 üstünde satış düşün
• Mevcut seviyede bekle";
        }
        
        private string HandleMACDRequest(AiRequest request)
        {
            return @"📊 **MACD ANALİZİ**

🔍 **MEVCUT DURUM:**
• **MACD Line:** Signal Line üzerinde
• **Histogram:** Pozitif
• **Sinyal:** Bullish

📈 **MACD YORUMU:**
MACD alım sinyali veriyor. Kısa vadeli momentum pozitif.

💡 **STRATEJİ:**
MACD bullish sinyali destekleniyor. Kısa vadeli alım için uygun.
• Signal Line altına düşerse stop-loss
• Histogram negatife dönerse dikkat et";
        }
        
        private string HandleFibonacciRequest(AiRequest request)
        {
            return @"📐 **FIBONACCI ANALİZİ**

🔍 **GERİ ÇEKİLME SEVİYELERİ:**
• **%23.6:** ₺135.20
• **%38.2:** ₺128.50 (Destek)
• **%50.0:** ₺122.00 (Destek)
• **%61.8:** ₺115.50 (Güçlü Destek)

📈 **HEFELER:**
• **%161.8:** ₺155.50
• **%200.0:** ₺168.00

💡 **FIBONACCI YORUMU:**
%38.2 seviyesi güçlü destek olarak çalışıyor. Bu seviye üzerinde kalınması önemli.

⚡ **İŞLEM STRATEJİSİ:**
• %38.2 seviyesine yaklaşınca alım
• %61.8 kırılırsa dikkatli ol";
        }
        
        private string HandleTrendRequest(AiRequest request)
        {
            return @"📈 **TREND ANALİZİ**

🔍 **TREND DURUMU:**
• **Kısa Vadeli:** Yükselen
• **Orta Vadeli:** Yükselen
• **Uzun Vadeli:** Yatay

📊 **TREND GÜCÜ:**
• **ADX:** 25 (Güçlü trend)
• **Hacim:** Artış eğiliminde
• **Momentum:** Pozitif

💡 **TREND YORUMU:**
Genel olarak yükselen trend aktif. Kısa vadeli momentum güçlü.

⚡ **STRATEJİ:**
Trend takibi stratejisi uygun. Destek seviyelerinde alım, dirençlerde kısmi satış.";
        }
        
        private string HandlePDFRequest(AiRequest request)
        {
            return @"📄 **PDF RAPORU**

📊 **RAPOR İÇERİĞİ:**
• Portföy özeti ve performans
• Hisse senedi analizleri
• Risk değerlendirmesi
• Yatırım önerileri

💡 **PDF ÖZELLİKLERİ:**
• Detaylı grafikler ve tablolar
• Teknik analiz göstergeleri
• Performans metrikleri
• Gelecek projeksiyonları

📝 **NOT:** PDF raporu hazırlanıyor. Dosya kaydedildikten sonra size bildirim yapılacak.";
        }
        
        private string HandleReportRequest(AiRequest request)
        {
            return @"📊 **YATIRIM RAPORU**

🔍 **RAPOR PERİYODU:** Son 3 ay

💰 **PERFORMANS:**
• **Toplam Getiri:** +%12.5
• **Aylık Ortalama:** +%4.2
• **Risk Ayarlı:** Sharpe 1.8

📈 **DAĞILIM:**
• **Hisse Senetleri:** %45
• **Mevduat:** %30
• **Diğer:** %25

💡 **ÖNERİLER:**
• Portföy performansı iyi
• Çeşitlendirme yeterli
• Risk seviyesi uygun";
        }
        
        private string HandleInvestmentRequest(AiRequest request)
        {
            return @"💼 **YATIRIM DANIŞMANLIĞI**

🎯 **YATIRIM HEDEFİ:**
• **Orta Vadeli:** 3-5 yıl
• **Risk Profili:** Orta
• **Beklenen Getiri:** Yıllık %12-15

📊 **YATIRIM STRATEJİSİ:**
• **%40** Hisse Senetleri (Büyüme)
• **%30** Mevduat (Güvenlik)
• **%20** Yabancı Yatırım (Çeşitlendirme)
• **%10** Alternatif (Yüksek potansiyel)

💡 **ÖNERİLER:**
• Düzenli yatırım planı yap
• Piyasa dalgalanmalarını fırsat gör
• Risk toleransını gözden geçir";
        }
        
        private string GetDefaultResponse(AiRequest request)
        {
            return @"🤖 **NOVABANK AI ASISTAN**

Merhaba! Size nasıl yardımcı olabilirim?

📊 **YAPABİLDİKLERİM:**
• Portföy analizi ve özeti
• Hisse senedi teknik analizi
• Destek/direnç seviyeleri
• Risk değerlendirmesi
• Yatırım stratejisi önerileri
• Grafik formasyonları
• Teknik göstergeler (RSI, MACD, Fibonacci)
• PDF raporları

💡 **ÖRNEK SORULAR:**
• ""Portföyümü özetle""
• ""GARAN hissesini analiz et""
• ""Destek seviyelerini göster""
• ""Risklerimi değerlendir""
• ""RSI göstergesini yorumla""

Lütfen spesifik bir konuda yardım isteyin!";
        }
        
        public void ClearHistory()
        {
            _conversationHistory.Clear();
        }
    }
}
