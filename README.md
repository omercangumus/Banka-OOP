# BankaBenim - Banka Uygulaması

Bu proje, Windows Forms ve DevExpress kullanılarak geliştirilmiş bir banka yönetim sistemidir.

## 🛠️ Teknolojiler

- **.NET 8.0**
- **C# WinForms**
- **DevExpress UI Framework**
- **PostgreSQL** (Veritabanı)
- **Entity Framework Core**
- **Dapper** (ORM)
- **Npgsql** (PostgreSQL Driver)

## 📋 Özellikler

- ✅ Kullanıcı kayıt ve giriş sistemi
- ✅ Email doğrulama sistemi
- ✅ Şifre sıfırlama
- ✅ Müşteri yönetimi
- ✅ Hesap yönetimi
- ✅ Para transferi
- ✅ İşlem geçmişi
- ✅ Denetim kayıtları (Audit Logs)
- ✅ Rol tabanlı yetkilendirme (Admin, Staff, Customer)

## 🚀 Kurulum

### Gereksinimler

1. **PostgreSQL** (14 veya üzeri) yüklü ve çalışır durumda olmalı
2. **.NET 8.0 SDK** yüklü olmalı
3. **Visual Studio 2022** veya **VS Code** (önerilir)

### Adımlar

1. **Veritabanı Ayarları:**
   - PostgreSQL'in `postgres` kullanıcısının şifresi `1` olmalı (veya `appsettings.json` dosyasını düzenleyin)
   - Varsayılan bağlantı ayarları:
     - Host: `127.0.0.1`
     - Port: `5432`
     - Database: `NovaBankDb`
     - User: `postgres`
     - Password: `1`

2. **Connection String Ayarları:**
   `src/BankApp.UI/appsettings.json` dosyasını düzenleyerek veritabanı bağlantı bilgilerinizi güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;"
  }
}
```

3. **Projeyi Derleme:**
```bash
dotnet build BankaBenim.sln
```

4. **Uygulamayı Çalıştırma:**
```bash
cd src/BankApp.UI
dotnet run
```

Veya Visual Studio'dan `BankApp.UI` projesini başlatın.

## 👤 Varsayılan Kullanıcılar

Uygulama ilk açıldığında aşağıdaki test kullanıcıları otomatik olarak oluşturulur:

| Kullanıcı Adı | Şifre | Rol | Durum |
|--------------|-------|-----|-------|
| `admin` | `admin123` | Admin | Doğrulanmış |
| `test` | `test123` | Customer | Doğrulanmış |
| `demo` | `demo123` | Customer | Doğrulanmış |
| `staff` | `123456` | Staff | Doğrulanmış |

**ÖNEMLİ:** İlk girişte bu şifreleri kullanabilirsiniz. Üretim ortamında bu kullanıcıları mutlaka değiştirin!

## 🔧 Yapılan Düzeltmeler

### Veritabanı ve Login Sorunları Düzeltildi ✅

Aşağıdaki sorunlar tespit edilip düzeltilmiştir:

#### 1. **Connection String Format Tutarsızlığı** ✅
   - **Sorun:** Bazı dosyalarda `Server=` bazılarında `Host=` formatı kullanılıyordu
   - **Düzeltme:** Tüm connection string'ler `Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;` formatına standartlaştırıldı
   - **Etkilenen Dosyalar:**
     - `DbInitializer.cs` (master ve app connection string'leri düzeltildi)

#### 2. **DbInitializer'da Tekrarlanan İşlemler** ✅
   - **Sorun:** `Initialize()` metodunda veritabanı oluşturma, tablo oluşturma ve veri ekleme işlemleri iki kez çağrılıyordu
   - **Düzeltme:** Gereksiz tekrarlar kaldırıldı, sadece bir kez çalışacak şekilde düzenlendi
   - **Etkilenen Dosya:** `DbInitializer.cs`

#### 3. **Login'de IsVerified Kontrolü Eksikti** ✅
   - **Sorun:** Kullanıcı giriş yaparken hesabın doğrulanıp doğrulanmadığı kontrol edilmiyordu
   - **Düzeltme:** `AuthService.LoginAsync()` metoduna `IsVerified` ve `IsActive` kontrolleri eklendi
   - **Etkilenen Dosya:** `AuthService.cs`
   - **Eklenen Özellikler:**
     - Hesap doğrulanmamışsa uyarı mesajı gösteriliyor
     - Hesap aktif değilse uyarı mesajı gösteriliyor
     - Her durum için audit log kaydı yapılıyor

#### 4. **Connection String Yönetimi İyileştirildi** ✅
   - **Sorun:** Connection string'ler hardcoded olarak yazılmıştı, `appsettings.json` kullanılmıyordu
   - **Düzeltme:** `DapperContext` sınıfına `appsettings.json`'dan connection string okuma özelliği eklendi
   - **Etkilenen Dosya:** `DapperContext.cs`
   - **Avantajlar:**
     - Merkezi yapılandırma yönetimi
     - Kolay bağlantı string değişikliği
     - Fallback mekanizması (appsettings.json okunamazsa hardcoded değer kullanılır)

#### 5. **Demo Kullanıcı Şifre Hash'i Düzeltildi** ✅
   - **Sorun:** Demo kullanıcının şifre hash'i yanlıştı
   - **Düzeltme:** `demo123` şifresinin doğru SHA256 hash'i ile güncellendi
   - **Etkilenen Dosya:** `DbInitializer.cs`

## 📁 Proje Yapısı

```
BankaBenim/
├── src/
│   ├── BankApp.Core/              # Domain katmanı (Entities, Interfaces)
│   ├── BankApp.Infrastructure/    # Data Access katmanı (Repositories, Services)
│   ├── BankApp.Business/          # Business Logic katmanı (Services)
│   ├── BankApp.UI/                # UI katmanı (WinForms)
│   └── BankApp.Tests/             # Test projesi
├── appsettings.json               # Uygulama yapılandırma dosyası
└── BankaBenim.sln                # Solution dosyası
```

## 🗄️ Veritabanı Şeması

### Tablolar

- **Users** - Kullanıcı bilgileri
- **Customers** - Müşteri bilgileri
- **Accounts** - Hesap bilgileri
- **Transactions** - İşlem kayıtları
- **AuditLogs** - Denetim kayıtları

## 🔒 Güvenlik

- Şifreler SHA256 ile hash'lenir
- Email doğrulama sistemi mevcuttur
- Tüm işlemler audit log'a kaydedilir
- Rol tabanlı yetkilendirme

## 📧 Email Ayarları

Email gönderimi için `appsettings.json` dosyasında SMTP ayarlarını yapılandırın:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your_email@gmail.com",
    "SenderPassword": "your_app_password",
    "SenderName": "NovaBank Security"
  }
}
```

**Not:** Gmail kullanıyorsanız, "Uygulama Şifresi" kullanmanız gerekebilir.

## 🐛 Sorun Giderme

### Login Yapamıyorum

1. **PostgreSQL servisinin çalıştığını kontrol edin:**
   ```bash
   # Windows
   Get-Service postgresql*
   ```

2. **Connection string'in doğru olduğunu kontrol edin:**
   - `appsettings.json` dosyasındaki bağlantı bilgilerini kontrol edin
   - PostgreSQL şifresinin doğru olduğundan emin olun

3. **Veritabanının oluşturulduğunu kontrol edin:**
   - Uygulama ilk açıldığında otomatik olarak `NovaBankDb` veritabanını oluşturur
   - Manuel kontrol için: `SELECT 1 FROM pg_database WHERE datname = 'NovaBankDb';`

4. **Kullanıcının doğrulanmış olduğundan emin olun:**
   - Varsayılan test kullanıcıları zaten doğrulanmıştır
   - Yeni kayıt olan kullanıcılar email doğrulama kodu ile hesabını doğrulamalıdır

### Veritabanı Bağlantı Hatası

- PostgreSQL'in çalıştığından emin olun
- Port 5432'nin açık olduğunu kontrol edin
- Firewall ayarlarını kontrol edin
- Connection string'deki bilgilerin doğru olduğundan emin olun

## 📝 Notlar

- Uygulama ilk çalıştırıldığında otomatik olarak veritabanı oluşturulur ve test verileri eklenir
- Tüm connection string'ler artık `appsettings.json` dosyasından okunur
- Login işlemi sırasında hesap doğrulama kontrolü yapılır

## 👨‍💻 Geliştirici

Bu proje bir banka yönetim sistemi örneğidir.

## 📄 Lisans

Bu proje eğitim amaçlıdır.

---

**Son Güncelleme:** Tüm veritabanı ve login sorunları düzeltildi ✅

