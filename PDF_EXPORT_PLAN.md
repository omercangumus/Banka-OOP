# PDF Export Fix Plan

## Durum Özeti

### ✅ Tamamlanan Testler
| Test | Sonuç |
|------|-------|
| NO-OP click (sadece mesaj) | ✅ Çalıştı |
| Basit text dosyası yazma | ✅ Çalıştı |
| Basit XtraReport (boş) | ✅ Çalıştı |
| InvestmentAnalysisReport | ❌ Crash |

### 🔍 Tespit
- **Sorun:** `InvestmentAnalysisReport` constructor'ı crash yapıyor
- **Neden:** try-catch bile yakalamıyor → StackOverflow veya AccessViolation olabilir
- **Konum:** `Reports/InvestmentAnalysisReport.cs` → `InitializeReport()` method

---

## 📋 Yapılacaklar

### 1. InvestmentAnalysisReport'u Basitleştir
- [ ] Tüm kontrolleri kaldır, sadece 1 label ekle
- [ ] Test et - çalışırsa adım adım ekle
- [ ] Hangi kontrol crash yaptığını bul

### 2. Sorunlu Kodu Düzelt
- [ ] Crash yapan kontrol/property'yi tespit et
- [ ] Alternatif yöntem kullan veya kaldır

### 3. Final PDF Export
- [ ] InstrumentDetailView - tam PDF export
- [ ] ChartAnalysisForm - tam PDF export
- [ ] Test ve doğrulama

### 4. Cleanup
- [ ] Debug kodlarını temizle
- [ ] Commit ve push

---

## 🧪 Şu Anki Test Kodu (InstrumentDetailView)

```csharp
private void BtnExportPdf_Click(object sender, EventArgs e)
{
    try
    {
        var data = new InvestmentAnalysisData { Symbol = "TEST", ... };
        
        // Bu satır crash yapıyor:
        using var report = new InvestmentAnalysisReport(data);
        
        // Buraya ulaşamıyor
    }
    catch (Exception ex)
    {
        // try-catch bile yakalamıyor - StackOverflow?
    }
}
```

---

## 📁 İlgili Dosyalar
- `Controls/InstrumentDetailView.cs` - PDF buton handler
- `Forms/ChartAnalysisForm.cs` - PDF buton handler  
- `Reports/InvestmentAnalysisReport.cs` - **SORUNLU**
- `Reports/PdfReportExporter.cs` - Export helper
- `Services/Pdf/InvestmentAnalysisData.cs` - Data model

---

## 🎯 Hedef
PDF butonuna basınca crash olmadan Desktop'a PDF dosyası oluşturulacak.
