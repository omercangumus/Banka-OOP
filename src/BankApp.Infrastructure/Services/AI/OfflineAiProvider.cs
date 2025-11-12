using System;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Infrastructure.Services.AI
{
    /// <summary>
    /// Offline AI Provider - Rule-based responses without LLM
    /// Used when no API key is available
    /// </summary>
    public class OfflineAiProvider : IAIProvider
    {
        public bool IsAvailable => true; // Always available as fallback
        public string ProviderName => "Offline (Kural Tabanlı)";
        
        public Task<string> AskAsync(AiRequest request)
        {
            var response = GenerateOfflineResponse(request);
            return Task.FromResult(response);
        }
        
        public void ClearHistory()
        {
            // No history in offline mode
        }
        
        private string GenerateOfflineResponse(AiRequest request)
        {
            var sb = new StringBuilder();
            var ctx = request.Context;
            var topic = request.Topic?.ToLower() ?? "";
            
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("📊 FİNANSAL ÖZET RAPORU");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            
            // Topic-based analysis
            if (topic.Contains("tasarruf") || topic.Contains("harcama"))
            {
                GenerateSavingsReport(sb, ctx);
            }
            else if (topic.Contains("yatırım") || topic.Contains("portföy"))
            {
                GenerateInvestmentReport(sb, ctx);
            }
            else if (topic.Contains("kredi") || topic.Contains("borç"))
            {
                GenerateCreditReport(sb, ctx);
            }
            else if (topic.Contains("mevduat") || topic.Contains("faiz"))
            {
                GenerateDepositReport(sb, ctx);
            }
            else if (topic.Contains("borsa") || topic.Contains("hisse") || !string.IsNullOrEmpty(ctx.StockSymbol))
            {
                GenerateStockReport(sb, ctx);
            }
            else
            {
                GenerateGeneralReport(sb, ctx);
            }
            
            sb.AppendLine();
            sb.AppendLine("─────────────────────────────────────────");
            sb.AppendLine("ℹ️ Bu rapor offline modda üretilmiştir.");
            sb.AppendLine("API anahtarı eklenince detaylı AI analizi aktif olur.");
            
            return sb.ToString();
        }
        
        private void GenerateSavingsReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine("💰 TASARRUF ANALİZİ");
            sb.AppendLine();
            
            sb.AppendLine("📋 ÖZET:");
            sb.AppendLine($"Son dönem toplam harcamanız: ₺{ctx.TotalSpending:N2}");
            sb.AppendLine($"İşlem sayısı: {ctx.RecentTransactionCount}");
            sb.AppendLine();
            
            sb.AppendLine("📊 HARCAMA DAĞILIMI:");
            if (!string.IsNullOrEmpty(ctx.SpendingByCategory))
            {
                sb.AppendLine(ctx.SpendingByCategory);
            }
            else
            {
                sb.AppendLine("• Veri yükleniyor...");
            }
            sb.AppendLine();
            
            sb.AppendLine("💡 ÖNERİLER:");
            if (ctx.TotalSpending > ctx.TotalBalance * 0.8m)
            {
                sb.AppendLine("• ⚠️ Harcamalarınız bakiyenizin %80'ini aşmış. Dikkatli olun.");
            }
            sb.AppendLine("• Aylık bütçe planı oluşturmayı düşünün");
            sb.AppendLine("• Düzenli tasarruf için otomatik transfer kurun");
        }
        
        private void GenerateInvestmentReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine("📈 YATIRIM ANALİZİ");
            sb.AppendLine();
            
            sb.AppendLine("📋 ÖZET:");
            sb.AppendLine($"Toplam Net Varlık: ₺{ctx.NetWorth:N2}");
            sb.AppendLine($"Toplam Kar/Zarar: ₺{ctx.TotalProfit:N2} ({ctx.ProfitPercent:+0.00;-0.00}%)");
            sb.AppendLine();
            
            sb.AppendLine("📊 BULGULAR:");
            if (ctx.ProfitPercent > 0)
            {
                sb.AppendLine($"• ✅ Portföyünüz %{ctx.ProfitPercent:F2} kârda");
            }
            else if (ctx.ProfitPercent < 0)
            {
                sb.AppendLine($"• ⚠️ Portföyünüz %{Math.Abs(ctx.ProfitPercent):F2} zararda");
            }
            else
            {
                sb.AppendLine("• Portföyünüz dengede");
            }
            sb.AppendLine();
            
            sb.AppendLine("💡 ÖNERİLER:");
            sb.AppendLine("• Portföy çeşitlendirmesini gözden geçirin");
            sb.AppendLine("• Risk toleransınıza uygun yatırım yapın");
            sb.AppendLine("• Uzun vadeli hedeflerinizi belirleyin");
        }
        
        private void GenerateCreditReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine("💳 KREDİ ANALİZİ");
            sb.AppendLine();
            
            sb.AppendLine("📋 ÖZET:");
            sb.AppendLine($"Mevcut Bakiye: ₺{ctx.TotalBalance:N2}");
            sb.AppendLine($"Hesap Sayısı: {ctx.AccountCount}");
            sb.AppendLine();
            
            sb.AppendLine("📊 BULGULAR:");
            sb.AppendLine("• Kredi borcu bilgisi için ilgili modülü kontrol edin");
            sb.AppendLine();
            
            sb.AppendLine("💡 ÖNERİLER:");
            sb.AppendLine("• Kredi kartı borcunuzu zamanında ödeyin");
            sb.AppendLine("• Faiz oranlarını karşılaştırın");
            sb.AppendLine("• Gereksiz kredilerden kaçının");
        }
        
        private void GenerateDepositReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine("🏦 MEVDUAT ANALİZİ");
            sb.AppendLine();
            
            sb.AppendLine("📋 ÖZET:");
            sb.AppendLine($"Toplam Bakiye: ₺{ctx.TotalBalance:N2}");
            sb.AppendLine($"Hesap Sayısı: {ctx.AccountCount}");
            sb.AppendLine();
            
            sb.AppendLine("📊 FAİZ SENARYOLARI:");
            decimal rate1 = 0.45m; // %45 yıllık
            decimal rate2 = 0.50m; // %50 yıllık
            sb.AppendLine($"• %45 faiz ile 1 yıl: ₺{ctx.TotalBalance * (1 + rate1):N2}");
            sb.AppendLine($"• %50 faiz ile 1 yıl: ₺{ctx.TotalBalance * (1 + rate2):N2}");
            sb.AppendLine();
            
            sb.AppendLine("💡 ÖNERİLER:");
            sb.AppendLine("• Vadeli mevduat faiz oranlarını karşılaştırın");
            sb.AppendLine("• Enflasyona karşı koruma için çeşitlendirin");
        }
        
        private void GenerateStockReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine("📊 BORSA ANALİZİ");
            sb.AppendLine();
            
            if (!string.IsNullOrEmpty(ctx.StockSymbol))
            {
                sb.AppendLine($"📋 {ctx.StockSymbol} ÖZETİ:");
                sb.AppendLine($"Fiyat: ${ctx.StockPrice:N2}");
                sb.AppendLine($"Değişim: {ctx.StockChangePercent:+0.00;-0.00}%");
                sb.AppendLine();
                
                sb.AppendLine("📈 TEKNİK GÖRÜNÜM:");
                if (ctx.StockChangePercent > 2)
                {
                    sb.AppendLine("• Trend: Güçlü yükseliş");
                }
                else if (ctx.StockChangePercent > 0)
                {
                    sb.AppendLine("• Trend: Hafif yükseliş");
                }
                else if (ctx.StockChangePercent > -2)
                {
                    sb.AppendLine("• Trend: Hafif düşüş");
                }
                else
                {
                    sb.AppendLine("• Trend: Güçlü düşüş");
                }
                sb.AppendLine("• RSI/MACD: Detaylı analiz için API gerekli");
                sb.AppendLine();
                
                if (!string.IsNullOrEmpty(ctx.StockNews))
                {
                    sb.AppendLine("📰 SON HABERLER:");
                    sb.AppendLine(ctx.StockNews);
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("Analiz için sembol seçin (örn: AAPL, TSLA)");
            }
            
            sb.AppendLine("⚠️ RİSK UYARISI:");
            sb.AppendLine("• Yatırım tavsiyesi değildir");
            sb.AppendLine("• Kendi araştırmanızı yapın");
        }
        
        private void GenerateGeneralReport(StringBuilder sb, AiContext ctx)
        {
            sb.AppendLine($"👋 Merhaba {ctx.Username}!");
            sb.AppendLine();
            
            sb.AppendLine("📋 GENEL FİNANSAL DURUMUNUZ:");
            sb.AppendLine($"• Net Varlık: ₺{ctx.NetWorth:N2}");
            sb.AppendLine($"• Toplam Bakiye: ₺{ctx.TotalBalance:N2}");
            sb.AppendLine($"• Hesap Sayısı: {ctx.AccountCount}");
            sb.AppendLine($"• Son İşlem Sayısı: {ctx.RecentTransactionCount}");
            sb.AppendLine();
            
            sb.AppendLine("💡 KONULAR:");
            sb.AppendLine("Detaylı analiz için soldaki konulardan birini seçin:");
            sb.AppendLine("• 💰 Tasarruf - Harcama analizi");
            sb.AppendLine("• 📈 Yatırım - Portföy değerlendirmesi");
            sb.AppendLine("• 💳 Kredi - Borç yönetimi");
            sb.AppendLine("• 🏦 Mevduat - Faiz hesaplama");
            sb.AppendLine("• 📊 Borsa - Hisse analizi");
        }
    }
}
