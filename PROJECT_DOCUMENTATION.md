# 🏦 NovaBank - Kapsamlı Proje Dokümantasyonu

> **Fırat Üniversitesi - .NET Uygulama Geliştirme Projesi**  
> **Proje Adı:** NovaBank - Dijital Bankacılık Uygulaması  
> **Son Güncelleme:** 08.01.2026

---

## 📋 İçindekiler

1. [Proje Genel Bakış](#-proje-genel-bakış)
2. [Mimari Yapı](#-mimari-yapı)
3. [Katmanlar ve Modüller](#-katmanlar-ve-modüller)
4. [Veritabanı Şeması](#-veritabanı-şeması)
5. [Servisler](#-servisler)
6. [Formlar ve Ekranlar](#-formlar-ve-ekranlar)
7. [Kontroller (User Controls)](#-kontroller-user-controls)
8. [AI Entegrasyonu](#-ai-entegrasyonu)
9. [Test Sistemi](#-test-sistemi)
10. [Smoke Testleri](#-smoke-testleri)
11. [Raporlama Sistemi](#-raporlama-sistemi)
12. [Güvenlik](#-güvenlik)
13. [Konfigürasyon](#-konfigürasyon)
14. [Kurulum ve Çalıştırma](#-kurulum-ve-çalıştırma)

---

## 🎯 Proje Genel Bakış

NovaBank, modern bir dijital bankacılık deneyimi sunan, DevExpress WinForms tabanlı bir masaüstü uygulamasıdır.

### Temel Özellikler

| Özellik | Açıklama |
|---------|----------|
| 🔐 **Kimlik Doğrulama** | Kullanıcı kaydı, giriş, email doğrulama, şifre sıfırlama |
| 💰 **Hesap Yönetimi** | TL/USD/EUR hesapları, bakiye görüntüleme, hesap açma |
| 💸 **Para Transferi** | IBAN ile transfer, anlık bakiye güncelleme |
| 📊 **Yatırım Platformu** | Hisse senedi, kripto, emtia alım-satım |
| 📈 **Teknik Analiz** | Candlestick grafik, RSI, MACD, Bollinger Bands |
| 🤖 **AI Asistan** | OpenRouter API ile akıllı finansal danışman |
| 🏢 **Admin Paneli** | Kullanıcı yönetimi, kredi onayları, raporlar |
| 💳 **Kredi Kartları** | Sanal kart oluşturma, harcama takibi |
| 🏦 **Kredi Sistemi** | Kredi başvurusu, onay süreci, ödeme planı |
| 📄 **PDF Raporları** | Yatırım analizi, admin raporları |

### Teknoloji Stack

```
├── Framework:      .NET 8.0 (Windows)
├── UI:             DevExpress WinForms 25.2
├── Database:       PostgreSQL 16
├── ORM:            Dapper (Micro-ORM)
├── AI:             OpenRouter API (DeepSeek, GPT-4)
├── Market Data:    Finnhub API, Binance API
├── Email:          MailKit (SMTP)
├── PDF:            DevExpress XtraReports
├── Testing:        xUnit, Moq, FluentAssertions
└── CI/CD:          GitLab CI
```

---

## 🏗 Mimari Yapı

```
BankaBenim/
├── src/
│   ├── BankApp.Core/           # Domain Layer (Entities, Interfaces)
│   ├── BankApp.Infrastructure/ # Data Access & Services
│   ├── BankApp.UI/             # Presentation Layer (WinForms)
│   └── BankApp.Tests/          # Unit & Integration Tests
├── docs/                       # Dokümantasyon
├── .github/                    # GitHub Actions
└── *.md                        # Proje dökümanları
```

### Clean Architecture Prensibi

```
┌─────────────────────────────────────────┐
│           BankApp.UI (Forms)            │  ← Presentation
├─────────────────────────────────────────┤
│     BankApp.Infrastructure (Services)    │  ← Application/Infrastructure
├─────────────────────────────────────────┤
│         BankApp.Core (Entities)          │  ← Domain
└─────────────────────────────────────────┘
```

---

## 📦 Katmanlar ve Modüller

### 1. BankApp.Core (Domain Layer)

**Entities (Varlıklar):**

| Entity | Açıklama | Dosya |
|--------|----------|-------|
| `User` | Kullanıcı bilgileri, kimlik doğrulama | `Entities/User.cs` |
| `Customer` | Müşteri profili, KYC bilgileri | `Entities/Customer.cs` |
| `Account` | Banka hesabı (TL/USD/EUR) | `Entities/Account.cs` |
| `Transaction` | Para transferi kayıtları | `Entities/Transaction.cs` |
| `CustomerPortfolio` | Yatırım portföyü | `Entities/CustomerPortfolio.cs` |
| `Stock` | Hisse senedi bilgileri | `Entities/Stock.cs` |
| `Loan` | Kredi başvuruları | `Entities/Loan.cs` |
| `CreditCard` | Kredi kartı bilgileri | `Entities/CreditCard.cs` |
| `AuditLog` | Denetim kayıtları | `Entities/AuditLog.cs` |
| `TimeDepositAccount` | Vadeli mevduat | `Entities/TimeDepositAccount.cs` |
| `PendingOrder` | Bekleyen emirler | `Entities/PendingOrder.cs` |

**Interfaces (Arayüzler):**

| Interface | Açıklama |
|-----------|----------|
| `IUserRepository` | Kullanıcı CRUD işlemleri |
| `IAccountRepository` | Hesap işlemleri |
| `ITransactionRepository` | Transfer işlemleri |
| `IEmailService` | Email gönderimi |
| `IAuditRepository` | Audit log kayıtları |

---

### 2. BankApp.Infrastructure (Altyapı Katmanı)

#### Data (Veri Erişim)

| Repository | Açıklama |
|------------|----------|
| `UserRepository` | Kullanıcı veritabanı işlemleri |
| `AccountRepository` | Hesap CRUD, bakiye güncelleme |
| `TransactionRepository` | Transfer kayıtları |
| `CustomerRepository` | Müşteri bilgileri |
| `CustomerPortfolioRepository` | Portföy alım-satım |
| `AuditRepository` | Denetim logları |
| `PendingOrderRepository` | Bekleyen emirler |
| `DapperContext` | PostgreSQL bağlantı yönetimi |
| `DbInitializer` | Veritabanı şema oluşturma, seed data |

#### Services (İş Mantığı)

| Servis | Açıklama |
|--------|----------|
| `AuthService` | Kimlik doğrulama, kayıt, şifre sıfırlama |
| `TransactionService` | Para transferi, bakiye kontrolü |
| `InvestmentService` | Yatırım işlemleri |
| `PortfolioService` | Portföy yönetimi |
| `LoanService` | Kredi başvuru ve onay |
| `CardService` | Kredi kartı işlemleri |
| `SmtpEmailService` | Email gönderimi (MailKit) |
| `FinnhubService` | Hisse senedi verileri (API) |
| `BinanceMarketDataProvider` | Kripto verileri (API) |
| `TechnicalIndicatorEngine` | RSI, MACD, SMA hesaplama |
| `PatternDetectionService` | Grafik pattern tespiti |
| `DashboardSummaryService` | Dashboard verileri |

---

### 3. BankApp.UI (Sunum Katmanı)

#### Forms (Formlar) - 25+ Form

| Form | Açıklama |
|------|----------|
| `LoginForm` | Kullanıcı girişi |
| `RegisterForm` | Yeni kayıt |
| `VerificationForm` | Email doğrulama |
| `ForgotPasswordForm` | Şifre sıfırlama |
| `MainForm` | Ana uygulama penceresi |
| `AdminDashboardForm` | Admin paneli |
| `TransferForm` | Para transferi |
| `CardsForm` | Kredi kartları |
| `LoanApplicationForm` | Kredi başvurusu |
| `LoanApprovalForm` | Kredi onay (Admin) |
| `InvestmentDashboardForm` | Yatırım dashboard |
| `ChartAnalysisForm` | Teknik analiz |
| `TradeTerminalForm` | Alım-satım terminali |
| `AIAssistantForm` | AI sohbet asistanı |
| `TimeDepositForm` | Vadeli mevduat |
| `SupportForm` | Destek talepleri |

#### Controls (Kullanıcı Kontrolleri) - 18 Kontrol

| Kontrol | Açıklama |
|---------|----------|
| `HeroNetWorthCard` | Net varlık kartı, IBAN kopyalama |
| `AssetAllocationChart` | Pasta grafik (varlık dağılımı) |
| `InvestmentView` | Tam yatırım ekranı |
| `PortfolioView` | Portföy görünümü |
| `MarketHomeView` | Piyasa ana sayfası |
| `InstrumentDetailView` | Enstrüman detayı |
| `RecentTransactionsWidget` | Son işlemler |
| `QuickActionsBar` | Hızlı işlem butonları |
| `AdminDashboardPanel` | Admin KPI kartları |
| `BESCalculatorControl` | BES hesaplayıcı |

---

## 🗄 Veritabanı Şeması

### PostgreSQL Tabloları

```sql
-- Kullanıcılar
CREATE TABLE Users (
    Id SERIAL PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    FullName VARCHAR(100),
    Role VARCHAR(20) DEFAULT 'Customer',
    IsActive BOOLEAN DEFAULT TRUE,
    IsVerified BOOLEAN DEFAULT FALSE,
    IsBanned BOOLEAN DEFAULT FALSE,
    VerificationCode VARCHAR(10),
    VerificationCodeExpiry TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Müşteriler
CREATE TABLE Customers (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    FirstName VARCHAR(50),
    LastName VARCHAR(50),
    TCKN VARCHAR(11) UNIQUE,
    Phone VARCHAR(20),
    Address TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Hesaplar
CREATE TABLE Accounts (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    AccountNumber VARCHAR(20) UNIQUE,
    IBAN VARCHAR(34) UNIQUE,
    Balance DECIMAL(18,2) DEFAULT 0,
    CurrencyCode VARCHAR(3) DEFAULT 'TRY',
    AccountType VARCHAR(20) DEFAULT 'Checking',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- İşlemler
CREATE TABLE Transactions (
    Id SERIAL PRIMARY KEY,
    AccountId INTEGER REFERENCES Accounts(Id),
    FromAccountId INTEGER,
    ToAccountId INTEGER,
    Amount DECIMAL(18,2) NOT NULL,
    TransactionType VARCHAR(50),
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Portföy
CREATE TABLE CustomerPortfolio (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    Symbol VARCHAR(20) NOT NULL,
    Quantity DECIMAL(18,8) NOT NULL,
    AveragePrice DECIMAL(18,4),
    AssetType VARCHAR(20),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Krediler
CREATE TABLE Loans (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    Amount DECIMAL(18,2) NOT NULL,
    InterestRate DECIMAL(5,2),
    TermMonths INTEGER,
    Status VARCHAR(20) DEFAULT 'Pending',
    ApprovedBy INTEGER,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Denetim Logları
CREATE TABLE AuditLogs (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER,
    Action VARCHAR(100),
    Details TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 🔧 Servisler

### AI Servisleri

| Servis | Açıklama |
|--------|----------|
| `AiProviderFactory` | AI sağlayıcı seçimi (Online/Offline) |
| `OpenRouterAiProvider` | OpenRouter API entegrasyonu |
| `OfflineAiProvider` | Çevrimdışı AI yanıtları |
| `AiContextBuilder` | Kullanıcı bağlamı oluşturma |
| `AIActionRouter` | AI komut yönlendirme |

### Market Data Servisleri

| Servis | Açıklama |
|--------|----------|
| `FinnhubService` | Hisse senedi fiyatları |
| `FinnhubServiceV2` | Gelişmiş veri çekme, cache |
| `BinanceMarketDataProvider` | Kripto fiyatları (WebSocket) |
| `MarketSimulatorService` | Mock veri (geliştirme) |
| `CurrencyConversionService` | Döviz kuru dönüşüm |

### Teknik Analiz Servisleri

| Servis | Açıklama |
|--------|----------|
| `TechnicalIndicatorEngine` | RSI, MACD, SMA, EMA, Bollinger |
| `PatternDetectionService` | Doji, Hammer, Engulfing pattern |

### Dashboard Servisleri

| Servis | Açıklama |
|--------|----------|
| `DashboardService` | Temel dashboard verileri |
| `DashboardSummaryService` | Kapsamlı özet (bakiye, portföy, grafikler) |

---

## 🤖 AI Entegrasyonu

### OpenRouter API

```
Provider: OpenRouter (openrouter.ai)
Model: deepseek/deepseek-chat (varsayılan)
Alternatif: openai/gpt-4-turbo, anthropic/claude-3

Özellikler:
- Portföy analizi
- Teknik analiz yorumlama
- Risk değerlendirmesi
- Piyasa durumu özeti
- Doğal dil sorgu işleme
```

### AI Asistan Özellikleri

| Özellik | Açıklama |
|---------|----------|
| 💬 **Chat UI** | Modern bubble-style sohbet arayüzü |
| 📊 **Portföy Özeti** | Hızlı buton ile portföy analizi |
| 📈 **Teknik Analiz** | Grafik yorumlama |
| ⚠️ **Risk Analizi** | Portföy risk değerlendirmesi |
| 💰 **Piyasa Durumu** | Anlık piyasa özeti |

### Konfigürasyon

```json
// appsettings.local.json (gitignore'da)
{
  "AI": {
    "OpenRouterApiKey": "sk-or-v1-xxxxx"
  }
}
```

---

## 🧪 Test Sistemi

### Unit Testler (xUnit)

#### AuthServiceTests
```csharp
[Fact] LoginAsync_ShouldReturnNull_WhenCredentialsAreCorrect()
[Fact] LoginAsync_ShouldReturnError_WhenUserNotFound()
[Fact] LoginAsync_ShouldReturnError_WhenPasswordIsIncorrect()
```

#### TransactionServiceTests
```csharp
[Fact] TransferMoneyAsync_ShouldTransferMoney_WhenBalanceIsSufficient()
[Fact] TransferMoneyAsync_ShouldReturnError_WhenBalanceIsInsufficient()
```

#### InvestmentDashboardV2Tests
```csharp
// Property Tests
[Fact] CandlestickDataCaching_ShouldCacheDataFor5Minutes()
[Fact] IndicatorDataValidation_ShouldVerifySufficientData()

// Unit Tests
[Fact] CalculateSMA_WithKnownData_ReturnsCorrectValues()
[Fact] CalculateRSI_WithKnownData_ReturnsValidRange()

// Pattern Detection
[Fact] DetectDoji_WithDojiCandle_ReturnsDetected()
[Fact] CalculateSupportResistance_WithValidData_ReturnsCorrectLevels()

// Error Handling
[Fact] CalculateRSI_WithInsufficientData_ReturnsError()
```

### Test Çalıştırma

```bash
cd src/BankApp.Tests
dotnet test
```

---

## 🔥 Smoke Testleri

### Test Kategorileri

| Kategori | Test Sayısı | Durum |
|----------|-------------|-------|
| Application Startup | 3 | ✅ PASS |
| Authentication | 2 | ✅ PASS |
| Admin Panel | 3 | ✅ PASS |
| Export | 2 | ✅ PASS |
| User Workflow | 2 | ✅ PASS |
| Integration | 2 | ✅ PASS |
| Performance | 2 | ✅ PASS |
| Configuration | 2 | ✅ PASS |
| Security | 2 | ✅ PASS |
| **TOPLAM** | **20** | **✅ ALL PASS** |

### Kritik Testler

1. **Application Boots** - Uygulama hatasız başlıyor
2. **Database Connection** - PostgreSQL bağlantısı çalışıyor
3. **Admin Login** - Admin girişi başarılı
4. **Transfer Works** - Para transferi çalışıyor
5. **CSV/PDF Export** - Raporlar oluşturuluyor
6. **SQL Injection** - Güvenlik açığı yok

---

## 📄 Raporlama Sistemi

### PDF Raporları (DevExpress XtraReports)

| Rapor | Açıklama |
|-------|----------|
| `InvestmentAnalysisReport` | Yatırım analiz raporu |
| `AdminDashboardReport` | Admin özet raporu |

### Export Formatları

| Format | Özellikler |
|--------|------------|
| **CSV** | UTF-8 BOM, virgül ayraç, Excel uyumlu |
| **PDF** | Landscape, Türkçe karakter desteği |

---

## 🔐 Güvenlik

### Kimlik Doğrulama

- SHA256 şifre hash
- Email doğrulama (6 haneli OTP)
- 15 dakika kod geçerliliği
- Hesap ban sistemi

### Veri Güvenliği

- Parameterized SQL queries (SQL Injection koruması)
- Input validation
- Audit logging
- API key'ler gitignore'da

### Yetkilendirme

| Rol | Yetkiler |
|-----|----------|
| `Customer` | Hesap, transfer, yatırım |
| `Admin` | Kullanıcı yönetimi, kredi onay, raporlar |

---

## ⚙ Konfigürasyon

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=5432;UserId=postgres;Password=xxx;Database=NovaBankDb;"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "novabank.com@gmail.com",
    "SenderPassword": "app-password",
    "SenderName": "NovaBank Security"
  },
  "Features": {
    "EnableAITradingMode": false
  }
}
```

### appsettings.local.json (gitignore'da)

```json
{
  "AI": {
    "OpenRouterApiKey": "sk-or-v1-xxxxx"
  },
  "Finnhub": {
    "ApiKey": "xxxxx"
  },
  "Binance": {
    "ApiKey": "xxxxx",
    "SecretKey": "xxxxx"
  }
}
```

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler

- .NET 8.0 SDK
- PostgreSQL 16+
- DevExpress WinForms 25.2 License
- Visual Studio 2022 / Rider

### Adımlar

```bash
# 1. Repo'yu klonla
git clone https://github.com/omercangumus/Banka-NTP.git
cd Banka-NTP

# 2. PostgreSQL veritabanı oluştur
psql -U postgres -c "CREATE DATABASE NovaBankDb;"

# 3. appsettings.local.json oluştur (API key'ler için)
cp src/BankApp.UI/appsettings.json src/BankApp.UI/appsettings.local.json
# Düzenle ve API key'leri ekle

# 4. Build
dotnet build

# 5. Çalıştır
dotnet run --project src/BankApp.UI
```

### Varsayılan Kullanıcılar

| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| `admin` | `admin123` | Admin |
| `test` | `test123` | Customer |

---

## 📊 Proje İstatistikleri

| Metrik | Değer |
|--------|-------|
| **Toplam Dosya** | ~150+ |
| **C# Dosyaları** | ~100+ |
| **Satır Kod** | ~25,000+ |
| **Form Sayısı** | 25+ |
| **Kontrol Sayısı** | 18 |
| **Servis Sayısı** | 30+ |
| **Test Sayısı** | 15+ |
| **Smoke Test** | 20 |

---

## 👨‍💻 Geliştirici

**Ömer Can Gümüş**  
Fırat Üniversitesi - Bilgisayar Mühendisliği  
.NET Uygulama Geliştirme Dersi Projesi

---

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

---

**🎉 NovaBank - Modern Dijital Bankacılık Deneyimi**
