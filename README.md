# NovaBank - Modern Banking Application

A comprehensive banking management system built with modern .NET technologies, featuring AI-powered financial assistance and real-time portfolio management.

## 🚀 Features

### Core Banking
- ✅ User registration and authentication
- ✅ Email verification system
- ✅ Account management
- ✅ Money transfers
- ✅ Transaction history
- ✅ Audit logging
- ✅ Role-based authorization (Admin, Staff, Customer)

### AI Integration
- ✅ AI-powered financial assistant
- ✅ Real-time portfolio analysis
- ✅ Investment recommendations
- ✅ PDF export functionality
- ✅ Chart analysis tools

### Dashboard & Analytics
- ✅ Real-time portfolio tracking
- ✅ Asset allocation charts
- ✅ Net worth visualization
- ✅ Transaction analytics
- ✅ Performance metrics

## 🛠️ Tech Stack

- **.NET 8.0** - Latest framework
- **C# WinForms** - Desktop UI
- **DevExpress UI Framework** - Rich UI components
- **PostgreSQL** - Primary database
- **Entity Framework Core** - ORM
- **Dapper** - High-performance data access
- **AI Integration** - Multiple AI providers

## 📋 Requirements

- **PostgreSQL** 14+
- **.NET 8.0 SDK**
- **Visual Studio 2022** or **VS Code**

## 🚀 Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/omercangumus/Banka-NTP.git
   cd Banka-NTP
   ```

2. **Configure Database**
   - Ensure PostgreSQL is running
   - Update connection string in `src/BankApp.UI/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=127.0.0.1;Port=5432;User Id=postgres;Password=1;Database=NovaBankDb;"
     }
   }
   ```

3. **Build and Run**
   ```bash
   dotnet build BankaBenim.sln
   cd src/BankApp.UI
   dotnet run
   ```

## 👤 Default Users

| Username | Password | Role | Status |
|----------|----------|------|--------|
| `admin` | `admin123` | Admin | Verified |
| `test` | `test123` | Customer | Verified |
| `demo` | `demo123` | Customer | Verified |
| `staff` | `123456` | Staff | Verified |

## 🏗️ Architecture

```
NovaBank/
├── src/
│   ├── BankApp.Core/              # Domain layer (Entities, Interfaces)
│   ├── BankApp.Infrastructure/    # Data Access layer (Repositories, Services)
│   ├── BankApp.Business/          # Business Logic layer
│   ├── BankApp.UI/                # UI layer (WinForms)
│   └── BankApp.Tests/             # Test project
├── docs/                          # Documentation
└── BankaBenim.sln                 # Solution file
```

## 🔒 Security

- SHA256 password hashing
- Email verification system
- Comprehensive audit logging
- Role-based access control
- Secure API integration

## 📧 Email Configuration

Configure SMTP settings in `appsettings.json`:

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

## 🤖 AI Features

NovaBank includes advanced AI capabilities:

- **Financial Analysis**: AI-powered portfolio analysis
- **Investment Advice**: Personalized recommendations
- **Risk Assessment**: Real-time risk evaluation
- **Market Insights**: Latest market trends

## 📊 Dashboard Features

- **Real-time Updates**: Live portfolio tracking
- **Interactive Charts**: Advanced visualization
- **Export Options**: PDF and Excel exports
- **Custom Reports**: Tailored financial reports

## 🔧 Troubleshooting

### Database Connection Issues
1. Verify PostgreSQL service is running
2. Check connection string in `appsettings.json`
3. Ensure database exists: `NovaBankDb`

### Login Problems
1. Check user is verified
2. Verify credentials from default users table
3. Review audit logs for failed attempts

## 📝 Development Notes

- Auto-database initialization on first run
- Centralized configuration management
- Comprehensive error handling
- Extensive logging and monitoring

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

This project is for educational purposes.

## 👨‍💻 Developer

Modern banking application with AI integration.

---

**Last Updated**: Complete AI integration and modern UI implementation ✅

