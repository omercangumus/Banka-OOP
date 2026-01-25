# NovaBank - Dijital Bankacılık Uygulaması
## Proje Raporu

---

**Ders:** .NET Uygulama Geliştirme (NTP)  
**Üniversite:** Fırat Üniversitesi  
**Bölüm:** Bilgisayar Mühendisliği  
**Öğrenci:** Ömer Can Gümüş  
**Tarih:** Ocak 2026

---

## İÇİNDEKİLER

1. [Proje Özeti](#1-proje-özeti)
2. [Giriş ve Problem Tanımı](#2-giriş-ve-problem-tanımı)
3. [Sistem Gereksinimleri](#3-sistem-gereksinimleri)
4. [Teknoloji Stack](#4-teknoloji-stack)
5. [Mimari Tasarım](#5-mimari-tasarım)
6. [Veritabanı Tasarımı](#6-veritabanı-tasarımı)
7. [UML Diyagramları](#7-uml-diyagramları)
   - 7.1 [Sınıf Diyagramı (Class Diagram)](#71-sınıf-diyagramı)
   - 7.2 [Use Case Diyagramı](#72-use-case-diyagramı)
   - 7.3 [Sequence Diyagramı](#73-sequence-diyagramı)
   - 7.4 [Activity Diyagramı](#74-activity-diyagramı)
   - 7.5 [Component Diyagramı](#75-component-diyagramı)
   - 7.6 [ER Diyagramı](#76-er-diyagramı)
8. [Modül Açıklamaları](#8-modül-açıklamaları)
9. [Ekran Görüntüleri](#9-ekran-görüntüleri)
10. [Test Stratejisi](#10-test-stratejisi)
11. [Güvenlik Önlemleri](#11-güvenlik-önlemleri)
12. [Sonuç ve Değerlendirme](#12-sonuç-ve-değerlendirme)
13. [Kaynaklar](#13-kaynaklar)

---

## 1. PROJE ÖZETİ

**NovaBank**, modern dijital bankacılık deneyimi sunan, Windows Forms ve DevExpress tabanlı kurumsal düzeyde bir masaüstü bankacılık uygulamasıdır. Proje, Clean Architecture prensiplerine uygun olarak 4 katmanlı mimari yapıda geliştirilmiştir.

### Temel Özellikler:
- ✅ Kullanıcı kayıt ve kimlik doğrulama
- ✅ Email doğrulama sistemi (OTP)
- ✅ Hesap yönetimi (TL/USD/EUR)
- ✅ Para transferi (IBAN ile)
- ✅ Yatırım platformu (Hisse, Kripto, Emtia)
- ✅ Teknik analiz araçları (RSI, MACD, Bollinger Bands)
- ✅ AI Finansal Asistan
- ✅ Admin paneli
- ✅ Kredi kartı yönetimi
- ✅ Kredi başvuru sistemi
- ✅ PDF raporlama

### Proje İstatistikleri:
| Metrik | Değer |
|--------|-------|
| Toplam C# Dosyası | 100+ |
| Satır Kod | ~25,000 |
| Form Sayısı | 25+ |
| User Control | 18 |
| Servis Sayısı | 30+ |
| Entity Sayısı | 15 |
| Unit Test | 15+ |

---

## 2. GİRİŞ VE PROBLEM TANIMI

### 2.1 Problem Tanımı

Günümüzde bankacılık işlemleri büyük ölçüde dijitalleşmiştir. Ancak geleneksel bankacılık sistemleri:

- Karmaşık ve zor kullanımlı arayüzler sunar
- Yatırım araçlarına sınırlı erişim sağlar
- Teknik analiz olanakları kısıtlıdır
- AI destekli danışmanlık hizmeti yoktur

### 2.2 Çözüm Önerisi

**NovaBank** bu sorunlara şu çözümleri sunar:

1. **Modern UI/UX**: DevExpress bileşenleri ile profesyonel görünüm
2. **Entegre Yatırım**: Tek platformda hisse, kripto ve emtia işlemleri
3. **Gelişmiş Analiz**: Candlestick grafikleri, teknik indikatörler
4. **AI Destekli**: OpenRouter API ile akıllı finansal danışman
5. **Güvenlik**: SHA256 şifreleme, email doğrulama, audit logging

### 2.3 Hedef Kitle

- Bireysel bankacılık müşterileri
- Yatırım yapmak isteyen kullanıcılar
- Finansal durumunu takip etmek isteyenler
- Banka yöneticileri (Admin)

---

## 3. SİSTEM GEREKSİNİMLERİ

### 3.1 Fonksiyonel Gereksinimler

| ID | Gereksinim | Öncelik |
|----|------------|---------|
| FR01 | Kullanıcı kayıt olabilmeli | Yüksek |
| FR02 | Email doğrulama yapılmalı | Yüksek |
| FR03 | Hesaplar arası transfer | Yüksek |
| FR04 | Yatırım alım/satım | Orta |
| FR05 | Teknik analiz görüntüleme | Orta |
| FR06 | Kredi başvurusu | Orta |
| FR07 | Kredi kartı oluşturma | Orta |
| FR08 | Admin kullanıcı yönetimi | Yüksek |
| FR09 | PDF rapor oluşturma | Düşük |
| FR10 | AI asistan sohbet | Düşük |

### 3.2 Fonksiyonel Olmayan Gereksinimler

| ID | Gereksinim | Açıklama |
|----|------------|----------|
| NFR01 | Performans | Uygulama 3 saniyede yüklenmeli |
| NFR02 | Güvenlik | Şifreler hash'lenmeli |
| NFR03 | Kullanılabilirlik | Sezgisel arayüz |
| NFR04 | Bakım Kolaylığı | Modüler mimari |
| NFR05 | Ölçeklenebilirlik | Clean Architecture |

---

## 4. TEKNOLOJİ STACK

### 4.1 Geliştirme Ortamı

| Kategori | Teknoloji | Versiyon |
|----------|-----------|----------|
| **Framework** | .NET | 8.0 |
| **Dil** | C# | 12 |
| **IDE** | Visual Studio | 2022 |
| **UI Framework** | DevExpress WinForms | 25.2 |
| **Database** | PostgreSQL | 16 |
| **ORM** | Dapper | 2.1.35 |
| **Email** | MailKit | 4.x |
| **PDF** | DevExpress XtraReports | 25.2 |
| **Test** | xUnit | 2.9 |
| **CI/CD** | GitLab CI | - |

### 4.2 Harici API Entegrasyonları

| API | Kullanım Amacı | Endpoint |
|-----|----------------|----------|
| **OpenRouter** | AI Finansal Danışman | openrouter.ai |
| **Finnhub** | Hisse Senedi Verileri | finnhub.io |
| **Binance** | Kripto Para Verileri | binance.com |

### 4.3 NuGet Paketleri

```xml
<PackageReference Include="Dapper" Version="2.1.35" />
<PackageReference Include="Npgsql" Version="8.0.2" />
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
<PackageReference Include="DevExpress.Win" Version="25.2.3" />
<PackageReference Include="xunit" Version="2.9.2" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

---

## 5. MİMARİ TASARIM

### 5.1 Clean Architecture

NovaBank projesi **Clean Architecture** prensiplerine uygun olarak tasarlanmıştır:

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│                      (BankApp.UI)                            │
│     Forms, Controls, User Interface Components               │
├─────────────────────────────────────────────────────────────┤
│                  APPLICATION / INFRASTRUCTURE                │
│                 (BankApp.Infrastructure)                     │
│       Services, Repositories, External APIs, Email           │
├─────────────────────────────────────────────────────────────┤
│                      DOMAIN LAYER                            │
│                     (BankApp.Core)                           │
│           Entities, Interfaces, Business Rules               │
├─────────────────────────────────────────────────────────────┤
│                     DATABASE LAYER                           │
│                      (PostgreSQL)                            │
│              Tables, Stored Procedures, Views                │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 Proje Yapısı

```
BankaBenim/
├── src/
│   ├── BankApp.Core/               # Domain Layer
│   │   ├── Entities/               # 15 Entity sınıfı
│   │   │   ├── User.cs
│   │   │   ├── Customer.cs
│   │   │   ├── Account.cs
│   │   │   ├── Transaction.cs
│   │   │   ├── Loan.cs
│   │   │   ├── CreditCard.cs
│   │   │   ├── CustomerPortfolio.cs
│   │   │   ├── Stock.cs
│   │   │   ├── AuditLog.cs
│   │   │   └── ...
│   │   └── Interfaces/             # Repository arayüzleri
│   │
│   ├── BankApp.Infrastructure/     # Data & Services Layer
│   │   ├── Data/                   # Veri erişim katmanı
│   │   │   ├── DapperContext.cs
│   │   │   └── DbInitializer.cs
│   │   ├── Repositories/           # Repository implementasyonları
│   │   ├── Services/               # İş mantığı servisleri
│   │   │   ├── AuthService.cs
│   │   │   ├── TransactionService.cs
│   │   │   ├── InvestmentService.cs
│   │   │   ├── LoanService.cs
│   │   │   ├── CardService.cs
│   │   │   ├── FinnhubService.cs
│   │   │   ├── BinanceMarketDataProvider.cs
│   │   │   ├── TechnicalIndicatorEngine.cs
│   │   │   └── ...
│   │   └── Events/                 # Uygulama olayları
│   │
│   ├── BankApp.UI/                 # Presentation Layer
│   │   ├── Forms/                  # 25+ Windows Form
│   │   │   ├── LoginForm.cs
│   │   │   ├── MainForm.cs
│   │   │   ├── AdminDashboardForm.cs
│   │   │   ├── TransferForm.cs
│   │   │   ├── InvestmentDashboardForm.cs
│   │   │   ├── ChartAnalysisForm.cs
│   │   │   ├── AIAssistantForm.cs
│   │   │   └── ...
│   │   ├── Controls/               # 18 User Control
│   │   ├── Reports/                # PDF raporları
│   │   └── Services/               # UI servisleri
│   │
│   └── BankApp.Tests/              # Test Projesi
│       ├── AuthServiceTests.cs
│       ├── TransactionServiceTests.cs
│       └── InvestmentDashboardV2Tests.cs
│
├── docs/                           # Dokümantasyon
└── *.md                            # Proje dökümanları
```

### 5.3 Bağımlılık Akışı

```
         ┌─────────────┐
         │  BankApp.UI │
         └──────┬──────┘
                │ depends on
                ▼
    ┌───────────────────────┐
    │ BankApp.Infrastructure │
    └───────────┬───────────┘
                │ depends on
                ▼
         ┌─────────────┐
         │ BankApp.Core │
         └─────────────┘
```

---

## 6. VERİTABANI TASARIMI

### 6.1 Veritabanı Bilgileri

| Özellik | Değer |
|---------|-------|
| RDBMS | PostgreSQL 16 |
| Veritabanı Adı | NovaBankDb |
| Karakter Seti | UTF-8 |
| Connection | Dapper (Micro-ORM) |

### 6.2 Tablo Yapıları

#### Users Tablosu
```sql
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
```

#### Customers Tablosu
```sql
CREATE TABLE Customers (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER REFERENCES Users(Id),
    IdentityNumber VARCHAR(11) UNIQUE,
    FirstName VARCHAR(50),
    LastName VARCHAR(50),
    PhoneNumber VARCHAR(20),
    Email VARCHAR(100),
    Address TEXT,
    DateOfBirth DATE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### Accounts Tablosu
```sql
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
```

#### Transactions Tablosu
```sql
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
```

#### Loans Tablosu
```sql
CREATE TABLE Loans (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    UserId INTEGER REFERENCES Users(Id),
    Amount DECIMAL(18,2) NOT NULL,
    TermMonths INTEGER,
    InterestRate DECIMAL(5,2) DEFAULT 3.5,
    Status VARCHAR(20) DEFAULT 'Pending',
    ApplicationDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    DecisionDate TIMESTAMP,
    ApprovedById INTEGER,
    RejectionReason TEXT,
    Notes TEXT
);
```

#### CreditCards Tablosu
```sql
CREATE TABLE CreditCards (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    CardNumber VARCHAR(16) UNIQUE,
    CVV VARCHAR(3),
    ExpiryDate DATE,
    TotalLimit DECIMAL(18,2),
    AvailableLimit DECIMAL(18,2),
    CurrentDebt DECIMAL(18,2) DEFAULT 0,
    CutoffDay INTEGER DEFAULT 15,
    CardType VARCHAR(20) DEFAULT 'Virtual',
    ColorTheme VARCHAR(20) DEFAULT 'Purple',
    CardHolderName VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### CustomerPortfolio Tablosu
```sql
CREATE TABLE CustomerPortfolio (
    Id SERIAL PRIMARY KEY,
    CustomerId INTEGER REFERENCES Customers(Id),
    StockSymbol VARCHAR(20) NOT NULL,
    Quantity DECIMAL(18,8) NOT NULL,
    AverageCost DECIMAL(18,4),
    PurchaseDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

#### AuditLogs Tablosu
```sql
CREATE TABLE AuditLogs (
    Id SERIAL PRIMARY KEY,
    UserId INTEGER,
    Action VARCHAR(100),
    Details TEXT,
    IpAddress VARCHAR(45),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

## 7. UML DİYAGRAMLARI

### 7.1 Sınıf Diyagramı (Class Diagram)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CLASS DIAGRAM                                   │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────────┐         ┌──────────────────┐
│   <<abstract>>   │         │      User        │
│    BaseEntity    │◄────────│──────────────────│
│──────────────────│         │ -Id: int         │
│ +Id: int         │         │ -Username: string│
│ +CreatedAt: DateTime       │ -PasswordHash: str│
└────────┬─────────┘         │ -Email: string   │
         │                   │ -Role: string    │
         │                   │ -FullName: string│
         │                   │ -IsActive: bool  │
         │                   │ -IsVerified: bool│
         │                   └────────┬─────────┘
         │                            │ 1
         │                            │
         │                            ▼ 1
         │                   ┌──────────────────┐
         │                   │    Customer      │
         ├──────────────────►│──────────────────│
         │                   │ -UserId: int     │
         │                   │ -IdentityNumber  │
         │                   │ -FirstName       │
         │                   │ -LastName        │
         │                   │ -PhoneNumber     │
         │                   │ -Address         │
         │                   └────────┬─────────┘
         │                            │ 1
         │                   ┌────────┴────────┬────────────────┐
         │                   │                 │                │
         │                   ▼ *               ▼ *              ▼ *
         │          ┌──────────────┐  ┌──────────────┐  ┌──────────────┐
         │          │   Account    │  │  CreditCard  │  │    Loan      │
         ├─────────►│──────────────│  │──────────────│  │──────────────│
         │          │-CustomerId   │  │-CustomerId   │  │-CustomerId   │
         │          │-AccountNumber│  │-CardNumber   │  │-Amount       │
         │          │-IBAN         │  │-CVV          │  │-TermMonths   │
         │          │-Balance      │  │-ExpiryDate   │  │-InterestRate │
         │          │-CurrencyCode │  │-TotalLimit   │  │-Status       │
         │          └──────┬───────┘  │-AvailableLimit│ │-MonthlyPayment│
         │                 │          │-CurrentDebt  │  └──────────────┘
         │                 │ 1        │-CardType     │
         │                 │          │-ColorTheme   │
         │                 ▼ *        └──────────────┘
         │          ┌──────────────┐
         │          │ Transaction  │
         ├─────────►│──────────────│
         │          │-AccountId    │     ┌──────────────────┐
         │          │-TransactionType    │ CustomerPortfolio│
         │          │-Amount       │     │──────────────────│
         │          │-Description  │     │-CustomerId       │
         │          │-TransactionDate    │-StockSymbol      │
         │          └──────────────┘     │-Quantity         │
         │                               │-AverageCost      │
         │                               │-TotalInvestment  │
         │                               └──────────────────┘
         │
         │          ┌──────────────┐     ┌──────────────┐
         │          │   AuditLog   │     │    Stock     │
         ├─────────►│──────────────│     │──────────────│
         │          │-UserId       │     │-Symbol       │
         │          │-Action       │     │-Name         │
         │          │-Details      │     │-CurrentPrice │
         │          │-IpAddress    │     │-Change       │
         │          └──────────────┘     │-Volume       │
         │                               └──────────────┘
         │
         │          ┌──────────────────┐
         │          │ TimeDepositAccount│
         └─────────►│──────────────────│
                    │-CustomerId       │
                    │-Principal        │
                    │-InterestRate     │
                    │-TermDays         │
                    │-StartDate        │
                    │-MaturityDate     │
                    │-MaturityAmount   │
                    └──────────────────┘
```

### 7.2 Use Case Diyagramı

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            USE CASE DIAGRAM                                  │
└─────────────────────────────────────────────────────────────────────────────┘

                        ┌─────────────────────────────────────┐
                        │          NovaBank Sistemi           │
                        │                                     │
     ┌─────┐            │  ┌─────────────────────┐           │
     │     │────────────┼──│    Kayıt Ol         │           │
     │     │            │  └─────────────────────┘           │
     │     │────────────┼──┌─────────────────────┐           │
     │     │            │  │    Giriş Yap        │           │
     │     │            │  └─────────────────────┘           │
     │     │────────────┼──┌─────────────────────┐           │
     │ M   │            │  │ Email Doğrula       │           │
     │ Ü   │            │  └─────────────────────┘           │
     │ Ş   │────────────┼──┌─────────────────────┐           │
     │ T   │            │  │ Hesap Görüntüle     │           │
     │ E   │            │  └─────────────────────┘           │
     │ R   │────────────┼──┌─────────────────────┐           │
     │ İ   │            │  │ Para Transfer Et    │           │
     │     │            │  └─────────────────────┘           │
     │     │────────────┼──┌─────────────────────┐           │
     │     │            │  │ Yatırım Yap         │◄──────────┼───┐
     │     │            │  └─────────────────────┘           │   │
     │     │────────────┼──┌─────────────────────┐           │   │
     │     │            │  │ Grafik Analiz       │◄──────────┼───┤
     │     │            │  └─────────────────────┘           │   │
     │     │────────────┼──┌─────────────────────┐           │   │ <<include>>
     │     │            │  │ AI Asistan          │           │   │
     │     │            │  └─────────────────────┘           │   │
     │     │────────────┼──┌─────────────────────┐           │   │
     │     │            │  │ Kredi Başvurusu     │           │   │
     │     │            │  └─────────────────────┘           │   │
     │     │────────────┼──┌─────────────────────┐           │   │
     │     │            │  │ Kart Oluştur        │           │   │
     └─────┘            │  └─────────────────────┘           │   │
                        │                                     │   │
                        │                                     │   │
     ┌─────┐            │  ┌─────────────────────┐           │   │
     │     │────────────┼──│ Kullanıcı Yönetimi  │           │   │
     │     │            │  └─────────────────────┘           │   │
     │ A   │────────────┼──┌─────────────────────┐           │   │
     │ D   │            │  │ Kredi Onay/Red      │           │   │
     │ M   │            │  └─────────────────────┘           │   │
     │ I   │────────────┼──┌─────────────────────┐           │   │
     │ N   │            │  │ Raporları Görüntüle │◄──────────┼───┘
     │     │            │  └─────────────────────┘           │
     │     │────────────┼──┌─────────────────────┐           │
     │     │            │  │ PDF Export          │           │
     │     │            │  └─────────────────────┘           │
     │     │────────────┼──┌─────────────────────┐           │
     │     │            │  │ Audit Log İzleme    │           │
     └─────┘            │  └─────────────────────┘           │
                        │                                     │
                        └─────────────────────────────────────┘
```

### 7.3 Sequence Diyagramı - Para Transferi

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     SEQUENCE DIAGRAM - Para Transferi                        │
└─────────────────────────────────────────────────────────────────────────────┘

  ┌──────┐          ┌────────────┐      ┌─────────────────┐     ┌──────────┐
  │Müşteri│          │TransferForm│      │TransactionService│     │ Database │
  └───┬───┘          └─────┬──────┘      └────────┬────────┘     └────┬─────┘
      │                    │                      │                    │
      │ 1. Transfer Formu  │                      │                    │
      │ ─────────────────► │                      │                    │
      │                    │                      │                    │
      │ 2. IBAN & Tutar    │                      │                    │
      │ ─────────────────► │                      │                    │
      │                    │                      │                    │
      │                    │ 3. TransferMoney()   │                    │
      │                    │ ────────────────────►│                    │
      │                    │                      │                    │
      │                    │                      │ 4. Bakiye Kontrol  │
      │                    │                      │ ──────────────────►│
      │                    │                      │                    │
      │                    │                      │ 5. Bakiye Yeterli  │
      │                    │                      │ ◄──────────────────│
      │                    │                      │                    │
      │                    │                      │ 6. Gönderen Bakiye-│
      │                    │                      │ ──────────────────►│
      │                    │                      │                    │
      │                    │                      │ 7. Alıcı Bakiye+   │
      │                    │                      │ ──────────────────►│
      │                    │                      │                    │
      │                    │                      │ 8. Transaction Log │
      │                    │                      │ ──────────────────►│
      │                    │                      │                    │
      │                    │                      │ 9. Audit Log       │
      │                    │                      │ ──────────────────►│
      │                    │                      │                    │
      │                    │ 10. Success          │                    │
      │                    │ ◄────────────────────│                    │
      │                    │                      │                    │
      │ 11. Transfer Başarılı                     │                    │
      │ ◄───────────────── │                      │                    │
      │                    │                      │                    │
  ┌───┴───┐          ┌─────┴──────┐      ┌────────┴────────┐     ┌────┴─────┐
  │Müşteri│          │TransferForm│      │TransactionService│     │ Database │
  └───────┘          └────────────┘      └─────────────────┘     └──────────┘
```

### 7.4 Activity Diyagramı - Kredi Başvurusu

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                   ACTIVITY DIAGRAM - Kredi Başvurusu                         │
└─────────────────────────────────────────────────────────────────────────────┘

                              ┌───────────────┐
                              │    Başla      │
                              └───────┬───────┘
                                      │
                                      ▼
                          ┌───────────────────────┐
                          │ Kredi Formunu Aç      │
                          └───────────┬───────────┘
                                      │
                                      ▼
                          ┌───────────────────────┐
                          │ Tutar ve Vade Gir     │
                          └───────────┬───────────┘
                                      │
                                      ▼
                              ◄───────────────►
                            ╱ Tutar Geçerli?  ╲
                           ╱                    ╲
                          ╱                      ╲
                      Hayır                      Evet
                         │                        │
                         ▼                        ▼
              ┌──────────────────┐    ┌───────────────────────┐
              │ Hata Mesajı      │    │ Taksit Hesapla        │
              │ Göster           │    └───────────┬───────────┘
              └────────┬─────────┘                │
                       │                          ▼
                       │              ┌───────────────────────┐
                       │              │ Başvuruyu Kaydet      │
                       │              │ Status = "Pending"    │
                       │              └───────────┬───────────┘
                       │                          │
                       │                          ▼
                       │              ┌───────────────────────┐
                       │              │ Audit Log Oluştur     │
                       │              └───────────┬───────────┘
                       │                          │
                       │                          ▼
                       │              ┌───────────────────────┐
                       │              │ Admin Bildirim        │
                       │              └───────────┬───────────┘
                       │                          │
                       │                          ▼
                       │                  ◄───────────────►
                       │                ╱ Admin Onayladı?  ╲
                       │               ╱                    ╲
                       │              ╱                      ╲
                       │          Hayır                      Evet
                       │             │                        │
                       │             ▼                        ▼
                       │  ┌────────────────────┐  ┌───────────────────────┐
                       │  │ Status = "Rejected"│  │ Status = "Approved"   │
                       │  │ Red Sebebi Kaydet  │  │ Hesaba Para Aktar     │
                       │  └─────────┬──────────┘  └───────────┬───────────┘
                       │            │                         │
                       │            └────────────┬────────────┘
                       │                         │
                       └────────────────────────►│
                                                 ▼
                                         ┌───────────────┐
                                         │    Bitir      │
                                         └───────────────┘
```

### 7.5 Component Diyagramı

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          COMPONENT DIAGRAM                                   │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                              <<subsystem>>                                   │
│                               BankApp.UI                                     │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐               │
│  │ LoginForm  │ │ MainForm   │ │ TransferForm│ │ AdminForm  │               │
│  └──────┬─────┘ └──────┬─────┘ └──────┬─────┘ └──────┬─────┘               │
│         │              │              │              │                      │
│  ┌──────┴──────────────┴──────────────┴──────────────┴─────┐               │
│  │                     Controls Package                      │               │
│  │  ┌────────────┐ ┌────────────┐ ┌────────────┐            │               │
│  │  │ HeroCard   │ │ ChartView  │ │ PortfolioView           │               │
│  │  └────────────┘ └────────────┘ └────────────┘            │               │
│  └─────────────────────────────────────────────────────────┘               │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │ <<uses>>
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              <<subsystem>>                                   │
│                          BankApp.Infrastructure                              │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        Services Package                               │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐    │   │
│  │  │ AuthService │ │ Transaction │ │ Investment  │ │ Loan        │    │   │
│  │  │             │ │ Service     │ │ Service     │ │ Service     │    │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                      Repositories Package                             │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐    │   │
│  │  │ UserRepo    │ │ AccountRepo │ │ TransactionRepo│ │ AuditRepo  │    │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                       External APIs Package                           │   │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐    │   │
│  │  │ Finnhub     │ │ Binance     │ │ OpenRouter  │ │ SMTP Email  │    │   │
│  │  │ Service     │ │ Provider    │ │ AI Service  │ │ Service     │    │   │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘    │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────────┬────────────────────────────────────────┘
                                     │ <<uses>>
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              <<subsystem>>                                   │
│                              BankApp.Core                                    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                         Entities Package                              │   │
│  │  ┌──────┐ ┌──────────┐ ┌─────────┐ ┌───────────┐ ┌──────┐          │   │
│  │  │ User │ │ Customer │ │ Account │ │Transaction│ │ Loan │          │   │
│  │  └──────┘ └──────────┘ └─────────┘ └───────────┘ └──────┘          │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        Interfaces Package                             │   │
│  │  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐        │   │
│  │  │ IUserRepository │ │ IAccountRepository│ │ IEmailService   │        │   │
│  │  └─────────────────┘ └─────────────────┘ └─────────────────┘        │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
                          ┌───────────────────┐
                          │    PostgreSQL     │
                          │    Database       │
                          └───────────────────┘
```

### 7.6 ER Diyagramı (Entity-Relationship)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          ER DIAGRAM - NovaBank                               │
└─────────────────────────────────────────────────────────────────────────────┘

    ┌──────────────┐                           ┌──────────────┐
    │    USERS     │                           │  CUSTOMERS   │
    │──────────────│                           │──────────────│
    │ PK Id        │◄─────────── 1:1 ─────────►│ PK Id        │
    │    Username  │                           │ FK UserId    │
    │    Email     │                           │    FirstName │
    │    Password  │                           │    LastName  │
    │    Role      │                           │    Identity  │
    │    IsActive  │                           │    Phone     │
    │    IsVerified│                           │    Address   │
    └──────────────┘                           └───────┬──────┘
                                                       │
                                                       │ 1:N
                      ┌────────────────────────────────┼────────────────────┐
                      │                                │                    │
                      ▼                                ▼                    ▼
              ┌──────────────┐                ┌──────────────┐     ┌──────────────┐
              │   ACCOUNTS   │                │    LOANS     │     │ CREDITCARDS  │
              │──────────────│                │──────────────│     │──────────────│
              │ PK Id        │                │ PK Id        │     │ PK Id        │
              │ FK CustomerId│                │ FK CustomerId│     │ FK CustomerId│
              │    AccNumber │                │ FK UserId    │     │    CardNumber│
              │    IBAN      │                │    Amount    │     │    CVV       │
              │    Balance   │                │    TermMonths│     │    ExpiryDate│
              │    Currency  │                │    Interest  │     │    TotalLimit│
              └───────┬──────┘                │    Status    │     │    AvailLimit│
                      │                       └──────────────┘     │    Debt      │
                      │ 1:N                                        │    CardType  │
                      ▼                                            └──────────────┘
              ┌──────────────┐
              │ TRANSACTIONS │                ┌──────────────┐
              │──────────────│                │ CUSTOMERPORT │
              │ PK Id        │                │──────────────│
              │ FK AccountId │                │ PK Id        │
              │    Type      │                │ FK CustomerId│◄──── Customer 1:N
              │    Amount    │                │    Symbol    │
              │    Desc      │                │    Quantity  │
              │    Date      │                │    AvgCost   │
              └──────────────┘                └──────────────┘


              ┌──────────────┐                ┌──────────────┐
              │  AUDITLOGS   │                │    STOCKS    │
              │──────────────│                │──────────────│
              │ PK Id        │                │ PK Id        │
              │ FK UserId    │◄──── User 1:N  │    Symbol    │
              │    Action    │                │    Name      │
              │    Details   │                │    Price     │
              │    IpAddress │                │    Change    │
              │    CreatedAt │                │    Volume    │
              └──────────────┘                └──────────────┘
```

---

## 8. MODÜL AÇIKLAMALARI

### 8.1 Kimlik Doğrulama Modülü (AuthService)

| Özellik | Açıklama |
|---------|----------|
| **Kayıt** | Yeni kullanıcı kaydı, şifre hash'leme |
| **Giriş** | Kullanıcı kimlik doğrulama |
| **Email Doğrulama** | 6 haneli OTP kodu gönderimi |
| **Şifre Sıfırlama** | Email ile şifre yenileme |
| **Audit Logging** | Tüm giriş/çıkış kayıtları |

#### Kod Örneği - AuthService
```csharp
public async Task<(User? user, string? error)> LoginAsync(string username, string password)
{
    var user = await _userRepository.GetByUsernameAsync(username);
    if (user == null)
        return (null, "Kullanıcı bulunamadı");
    
    if (!user.IsVerified)
        return (null, "Email doğrulanmamış");
    
    if (!user.IsActive)
        return (null, "Hesap devre dışı");
    
    var hash = HashPassword(password);
    if (user.PasswordHash != hash)
        return (null, "Şifre hatalı");
    
    await _auditRepository.LogAsync(user.Id, "LOGIN", "Başarılı giriş");
    return (user, null);
}
```

### 8.2 Transfer Modülü (TransactionService)

| Özellik | Açıklama |
|---------|----------|
| **IBAN Transfer** | IBAN numarası ile transfer |
| **Bakiye Kontrolü** | Yetersiz bakiye kontrolü |
| **Anlık Güncelleme** | Gerçek zamanlı bakiye güncelleme |
| **İşlem Geçmişi** | Tüm transferlerin kaydı |

#### Kod Örneği - TransactionService
```csharp
public async Task<(bool success, string message)> TransferMoneyAsync(
    int fromAccountId, string toIban, decimal amount, string description)
{
    var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
    if (fromAccount == null)
        return (false, "Gönderen hesap bulunamadı");
    
    if (fromAccount.Balance < amount)
        return (false, "Yetersiz bakiye");
    
    var toAccount = await _accountRepository.GetByIbanAsync(toIban);
    if (toAccount == null)
        return (false, "Alıcı hesap bulunamadı");
    
    // Transfer işlemi
    fromAccount.Balance -= amount;
    toAccount.Balance += amount;
    
    await _accountRepository.UpdateBalanceAsync(fromAccount.Id, fromAccount.Balance);
    await _accountRepository.UpdateBalanceAsync(toAccount.Id, toAccount.Balance);
    
    // İşlem kayıtları
    await _transactionRepository.CreateAsync(new Transaction
    {
        AccountId = fromAccountId,
        TransactionType = "TransferOut",
        Amount = amount,
        Description = description
    });
    
    return (true, "Transfer başarılı");
}
```

### 8.3 Yatırım Modülü (InvestmentService)

| Özellik | Açıklama |
|---------|----------|
| **Hisse Alım/Satım** | THYAO, GARAN gibi hisseler |
| **Kripto İşlemleri** | BTC, ETH alım/satım |
| **Emtia** | Altın, gümüş işlemleri |
| **Portföy Takibi** | Anlık portföy değeri |
| **Teknik Analiz** | RSI, MACD, Bollinger Bands |

### 8.4 Teknik Analiz Modülü (TechnicalIndicatorEngine)

| İndikatör | Açıklama |
|-----------|----------|
| **SMA** | Simple Moving Average (Basit Hareketli Ortalama) |
| **EMA** | Exponential Moving Average |
| **RSI** | Relative Strength Index (0-100) |
| **MACD** | Moving Average Convergence Divergence |
| **Bollinger Bands** | Volatilite bantları |

#### Kod Örneği - RSI Hesaplama
```csharp
public static List<decimal> CalculateRSI(List<decimal> prices, int period = 14)
{
    var result = new List<decimal>();
    var gains = new List<decimal>();
    var losses = new List<decimal>();
    
    for (int i = 1; i < prices.Count; i++)
    {
        var change = prices[i] - prices[i - 1];
        gains.Add(change > 0 ? change : 0);
        losses.Add(change < 0 ? Math.Abs(change) : 0);
    }
    
    for (int i = period; i < prices.Count; i++)
    {
        var avgGain = gains.Skip(i - period).Take(period).Average();
        var avgLoss = losses.Skip(i - period).Take(period).Average();
        
        var rs = avgLoss == 0 ? 100 : avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));
        result.Add(rsi);
    }
    
    return result;
}
```

### 8.5 AI Asistan Modülü (OpenRouterAIService)

| Özellik | Açıklama |
|---------|----------|
| **Portföy Analizi** | Yatırım portföyü değerlendirmesi |
| **Risk Analizi** | Portföy risk seviyesi belirleme |
| **Piyasa Durumu** | Güncel piyasa özeti |
| **Teknik Analiz Yorumu** | Grafik pattern yorumlama |
| **Sohbet** | Doğal dil finansal sorgular |

---

## 9. EKRAN GÖRÜNTÜLERİ

### 9.1 Giriş Ekranı
- Modern tasarım
- Kullanıcı adı/şifre girişi
- Kayıt ve şifre sıfırlama bağlantıları

### 9.2 Ana Panel (Dashboard)
- Net varlık kartı (HeroNetWorthCard)
- Varlık dağılımı pasta grafiği
- Son işlemler listesi
- Hızlı işlem butonları

### 9.3 Transfer Ekranı
- IBAN girişi
- Tutar ve açıklama
- Son kullanılan kişiler (Story Mode)

### 9.4 Yatırım Dashboard
- Canlı hisse fiyatları
- Candlestick grafikleri
- Teknik indikatörler
- Al/Sat butonları

### 9.5 Admin Paneli
- Kullanıcı listesi
- Kredi onay/red
- KPI kartları
- PDF/CSV export

---

## 10. TEST STRATEJİSİ

### 10.1 Unit Testler

```csharp
[Fact]
public async Task TransferMoneyAsync_ShouldTransferMoney_WhenBalanceIsSufficient()
{
    // Arrange
    var fromAccount = new Account { Id = 1, Balance = 1000 };
    var toAccount = new Account { Id = 2, Balance = 500, IBAN = "TR123456" };
    
    _mockAccountRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(fromAccount);
    _mockAccountRepo.Setup(x => x.GetByIbanAsync("TR123456")).ReturnsAsync(toAccount);
    
    // Act
    var (success, message) = await _service.TransferMoneyAsync(1, "TR123456", 200, "Test");
    
    // Assert
    success.Should().BeTrue();
    message.Should().Be("Transfer başarılı");
    fromAccount.Balance.Should().Be(800);
    toAccount.Balance.Should().Be(700);
}
```

### 10.2 Smoke Testler

| Test Kategorisi | Test Sayısı | Durum |
|-----------------|-------------|-------|
| Application Startup | 3 | ✅ PASS |
| Authentication | 2 | ✅ PASS |
| Admin Panel | 3 | ✅ PASS |
| Export | 2 | ✅ PASS |
| User Workflow | 2 | ✅ PASS |
| Integration | 2 | ✅ PASS |
| Performance | 2 | ✅ PASS |
| Security | 2 | ✅ PASS |
| **TOPLAM** | **20** | **✅ ALL PASS** |

---

## 11. GÜVENLİK ÖNLEMLERİ

### 11.1 Kimlik Doğrulama
- **SHA256** şifre hashleme
- **Email OTP** doğrulama (6 haneli kod)
- **15 dakika** kod geçerliliği
- **Hesap kilitleme** yanlış giriş sonrası

### 11.2 Veri Güvenliği
- **Parameterized SQL** - SQL Injection koruması
- **Input Validation** - Tüm form girişleri doğrulanır
- **Audit Logging** - Tüm kritik işlemler kaydedilir
- **API Key Güvenliği** - Anahtarlar gitignore'da tutulur

### 11.3 Yetkilendirme

| Rol | Yetkiler |
|-----|----------|
| **Customer** | Hesap görüntüleme, transfer, yatırım, kart |
| **Staff** | Müşteri bilgisi görüntüleme |
| **Admin** | Tüm yetkiler + kullanıcı yönetimi + kredi onay |

---

## 12. SONUÇ VE DEĞERLENDİRME

### 12.1 Proje Kazanımları

Bu proje sürecinde aşağıdaki teknik yetkinlikler kazanılmıştır:

1. **Clean Architecture** prensiplerine uygun katmanlı mimari tasarım
2. **.NET 8.0** ile modern C# geliştirme
3. **DevExpress WinForms** ile profesyonel UI tasarım
4. **PostgreSQL** veritabanı yönetimi
5. **Dapper** Micro-ORM kullanımı
6. **API Entegrasyonu** (Finnhub, Binance, OpenRouter)
7. **Unit Testing** (xUnit, Moq, FluentAssertions)
8. **Git/GitLab** versiyon kontrolü ve CI/CD

### 12.2 Karşılaşılan Zorluklar

| Zorluk | Çözüm |
|--------|-------|
| DevExpress lisans yönetimi | NuGet paket konfigürasyonu |
| PostgreSQL bağlantı sorunları | Connection string standardizasyonu |
| API rate limiting | Cache mekanizması implementasyonu |
| Form Designer hataları | Programatik UI oluşturma |

### 12.3 Gelecek Geliştirmeler

- [ ] Mobil uygulama (MAUI)
- [ ] REST API katmanı
- [ ] İki faktörlü kimlik doğrulama (2FA)
- [ ] Push notification sistemi
- [ ] Bütçe planlama modülü

---

## 13. KAYNAKLAR

### 13.1 Resmi Dokümantasyon
- Microsoft .NET Documentation: https://docs.microsoft.com/dotnet
- DevExpress Documentation: https://docs.devexpress.com
- PostgreSQL Documentation: https://www.postgresql.org/docs
- Dapper Documentation: https://dapperlib.github.io/Dapper

### 13.2 API Referansları
- Finnhub API: https://finnhub.io/docs/api
- Binance API: https://binance-docs.github.io/apidocs
- OpenRouter API: https://openrouter.ai/docs

### 13.3 Tasarım Desenleri
- Clean Architecture - Robert C. Martin
- Repository Pattern
- Factory Pattern
- Dependency Injection

---

## EKLER

### Ek-1: Veritabanı Bağlantı Ayarları

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "novabank.com@gmail.com",
    "SenderName": "NovaBank Security"
  }
}
```

### Ek-2: Varsayılan Test Kullanıcıları

| Kullanıcı | Şifre | Rol |
|-----------|-------|-----|
| admin | admin123 | Admin |
| test | test123 | Customer |
| demo | demo123 | Customer |
| staff | 123456 | Staff |

### Ek-3: Proje Kurulum Adımları

```bash
# 1. Repo'yu klonla
git clone https://github.com/omercangumus/Banka-NTP.git
cd Banka-NTP

# 2. PostgreSQL veritabanı oluştur
psql -U postgres -c "CREATE DATABASE NovaBankDb;"

# 3. Build
dotnet build BankaBenim.sln

# 4. Çalıştır
dotnet run --project src/BankApp.UI
```

---

**Proje Teslim Tarihi:** Ocak 2026  
**Hazırlayan:** Ömer Can Gümüş  
**Üniversite:** Fırat Üniversitesi  
**Bölüm:** Bilgisayar Mühendisliği

---

*Bu rapor, .NET Uygulama Geliştirme dersi kapsamında hazırlanmıştır.*
