# NovaBank AI (Groq) Entegrasyon Planı

## Genel Bakış

NovaBank yatırım ekranında AI destekli piyasa analizi özelliği eklenecek. Groq API kullanılarak hızlı ve düşük maliyetli LLM inference sağlanacak.

## API Bilgileri

- **Provider:** Groq (console.groq.com)
- **API Key:** Kullanıcıdan alınacak veya `appsettings.json` / environment variable
- **Model:** `llama-3.1-70b-versatile` veya `mixtral-8x7b-32768`
- **Endpoint:** `https://api.groq.com/openai/v1/chat/completions`

## UI Konumu

### Seçenek 1: Sağ Panel (Önerilen)
```
┌─────────────────────────────────────┐
│ Watchlist                           │
├─────────────────────────────────────┤
│ Sembol Detayları                    │
├─────────────────────────────────────┤
│ Emir Girişi                         │
├─────────────────────────────────────┤
│ 🤖 AI Analiz                        │
│ [Analiz Başlat] [Ayarlar]           │
│ ─────────────────────────────────── │
│ Analiz sonuçları burada gösterilir  │
└─────────────────────────────────────┘
```

### Seçenek 2: Alt Panel Tab
```
[Emirlerim] [Pozisyonlar] [Haberler] [AI Analiz] [Trade Terminal]
```

**Mevcut uygulama:** Alt panel tab olarak eklendi (`tabAnalysis`).

## AI'ya Gönderilecek Veriler

AI'ya görüntü DEĞİL, yapılandırılmış veri gönderilecek:

```json
{
  "symbol": "AAPL",
  "quote": {
    "current": 178.50,
    "open": 176.20,
    "high": 179.80,
    "low": 175.90,
    "previousClose": 175.00,
    "change": 3.50,
    "changePercent": 2.0
  },
  "candles": {
    "timeframe": "D",
    "count": 60,
    "data": [
      { "date": "2025-01-03", "o": 176, "h": 179, "l": 175, "c": 178.5 },
      // ... son 60 mum
    ]
  },
  "technicals": {
    "sma20": 172.5,
    "sma50": 168.0,
    "rsi14": 55.2,
    "macd": { "macd": 1.2, "signal": 0.8, "histogram": 0.4 }
  },
  "news": [
    { "headline": "Apple Q4 earnings beat expectations", "sentiment": "positive" },
    // ... son 5 haber
  ],
  "userPositions": [
    { "symbol": "AAPL", "quantity": 25, "avgCost": 165.00 }
  ],
  "userOrders": [
    { "symbol": "AAPL", "type": "LIMIT", "side": "BUY", "price": 170.00, "quantity": 10 }
  ]
}
```

## AI Çıktı Formatı

```
═══════════════════════════════════════
AI MARKET ANALİZİ - {SYMBOL}
═══════════════════════════════════════

📊 PAZAR ÖZETİ:
- Genel trend değerlendirmesi
- Hacim analizi
- Sektör karşılaştırması

📈 TEKNİK GÖRÜNÜM:
- Trend yönü (Yükseliş/Düşüş/Yatay)
- RSI durumu (Aşırı alım/satım)
- MACD sinyali
- Hareketli ortalama kesişimleri

🎯 DESTEK/DİRENÇ SEVİYELERİ:
- Destek 1: $XXX.XX
- Destek 2: $XXX.XX
- Direnç 1: $XXX.XX
- Direnç 2: $XXX.XX

⚠️ RİSK NOTLARI:
- Volatilite değerlendirmesi
- Piyasa riskleri
- Sektörel riskler

🔔 ALARM ÖNERİLERİ:
- Fiyat seviyesi alarmları
- Teknik gösterge alarmları

💡 STRATEJİ ÖNERİSİ:
- Kısa vadeli görünüm
- Orta vadeli görünüm
```

## Kod Mimarisi

### 1. Groq Service (yeni)
```csharp
// BankApp.Infrastructure/Services/GroqService.cs
public class GroqService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    
    public async Task<string> AnalyzeMarketAsync(MarketAnalysisRequest request)
    {
        var prompt = BuildAnalysisPrompt(request);
        var response = await CallGroqApiAsync(prompt);
        return response;
    }
    
    private string BuildAnalysisPrompt(MarketAnalysisRequest request)
    {
        // System prompt + user data
    }
}
```

### 2. Analysis Request Model
```csharp
public class MarketAnalysisRequest
{
    public string Symbol { get; set; }
    public QuoteData Quote { get; set; }
    public List<CandleData> Candles { get; set; }
    public TechnicalIndicators Technicals { get; set; }
    public List<NewsItem> RecentNews { get; set; }
    public List<Position> UserPositions { get; set; }
    public List<Order> UserOrders { get; set; }
}
```

### 3. UI Integration
```csharp
// InvestmentView.cs - RunAIAnalysisAsync metodu
private async Task RunAIAnalysisAsync()
{
    memoAnalysis.Text = "AI analizi çalışıyor...";
    
    // 1. Veri topla
    var request = new MarketAnalysisRequest
    {
        Symbol = _currentSymbol,
        Quote = await _finnhubService.GetQuoteAsync(_currentSymbol),
        Candles = await _finnhubService.GetCandlesAsync(_currentSymbol, "D", 60),
        // ... diğer veriler
    };
    
    // 2. AI'dan analiz al
    var analysis = await _groqService.AnalyzeMarketAsync(request);
    
    // 3. Sonucu göster
    memoAnalysis.Text = analysis;
}
```

## System Prompt (Türkçe)

```
Sen NovaBank için bir finansal analiz asistanısın. Sana verilen piyasa verilerini analiz edip Türkçe özet ve öneriler üreteceksin.

KURALLAR:
1. Sadece verilen verilere dayanarak analiz yap
2. Kesin alım/satım tavsiyeleri VERME, sadece teknik görünüm sun
3. Risk uyarılarını her zaman ekle
4. Kısa ve öz ol
5. Emoji kullanarak okunabilirliği artır
6. Destek/direnç seviyelerini sayısal olarak belirt

FORMAT:
[Yukarıdaki çıktı formatını kullan]

VERİLER:
{JSON veriler buraya}
```

## Güvenlik Notları

1. **API Key Güvenliği:**
   - Hardcode YAPMA
   - Environment variable veya encrypted config kullan
   - Client-side'da key expose etme

2. **Rate Limiting:**
   - Groq free tier: 30 req/min
   - Cooldown mekanizması ekle

3. **Data Privacy:**
   - Kullanıcı pozisyon/emir verilerini anonim tut
   - PII gönderme

## Uygulama Adımları

1. [ ] `GroqService.cs` oluştur
2. [ ] `MarketAnalysisRequest` model oluştur
3. [ ] API key configuration ekle
4. [ ] `InvestmentView` içinde entegre et
5. [ ] Error handling ve retry logic ekle
6. [ ] Rate limiting ekle
7. [ ] Test et

## Örnek Groq API Call

```csharp
var request = new
{
    model = "llama-3.1-70b-versatile",
    messages = new[]
    {
        new { role = "system", content = systemPrompt },
        new { role = "user", content = JsonSerializer.Serialize(analysisRequest) }
    },
    temperature = 0.7,
    max_tokens = 1024
};

var response = await httpClient.PostAsJsonAsync(
    "https://api.groq.com/openai/v1/chat/completions",
    request
);
```

---

**Durum:** Tasarım tamamlandı, UI placeholder eklendi. Kod implementasyonu bekliyor.
