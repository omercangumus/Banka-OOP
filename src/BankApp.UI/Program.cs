using System;
using System.Windows.Forms;
using DevExpress.Skins;
// using DevExpress.UserSkins;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.UI.Forms;

namespace BankApp.UI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress Skins
            // DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");

            // Subscribe to email simulation events (for development mode)
            SmtpEmailService.OnEmailSimulated += (to, subject, body) =>
            {
                // Extract verification code from body if present
                string message = $"Email Simülasyonu (Geliştirme Modu)\n\nAlıcı: {to}\nKonu: {subject}\n\n";
                
                // Try to extract code from HTML body
                if (body.Contains("<b>") && body.Contains("</b>"))
                {
                    int start = body.IndexOf("<b>") + 3;
                    int end = body.IndexOf("</b>");
                    if (end > start)
                    {
                        string code = body.Substring(start, end - start);
                        message += $"Doğrulama Kodu: {code}\n\n";
                    }
                }
                
                message += "Dikkat: Gerçek email için appsettings.json'u yapılandırın.";
                
                DevExpress.XtraEditors.XtraMessageBox.Show(message, "Email Simülasyonu", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            try
            {
                // 1. System Level Initialization (Service & Raw DB)
                var sysInit = new BankApp.Infrastructure.Initialization.SystemInitializer();
                sysInit.StartPostgresService();
                sysInit.CreateDatabaseIfNotExists();

                // 2. EF Core Code-First Initialization (Tables)
                using (var context = new BankApp.Infrastructure.Data.BankDbContext())
                {
                    context.Database.EnsureCreated();
                }

                // 3. Existing Seeding Logic (Optional: Keep DbInitializer for data seeding if EnsureCreated doesn't seed)
                // If DbInitializer uses Dapper, it's fine to run it after Tables are created by EF, 
                // just so it ensures Data exists.
                var dataInit = new BankApp.Infrastructure.Data.DbInitializer();
                dataInit.Initialize(); // Check logic inside to avoid duplicate errors
            }
            catch (Exception ex)
            {
                // SORUN DÜZELTİLDİ: Hata mesajı daha detaylı ve açıklayıcı hale getirildi
                string errorMessage = $"Kritik Başlangıç Hatası:\n\n{ex.Message}";
                
                if (ex.Message.Contains("Failed to connect") || ex.Message.Contains("5432"))
                {
                    errorMessage += "\n\n🔴 PostgreSQL servisi çalışmıyor olabilir!\n\n";
                    errorMessage += "Çözüm adımları:\n";
                    errorMessage += "1. PostgreSQL servisini başlatın:\n";
                    errorMessage += "   - Windows'ta: Services.msc açın ve 'postgresql' servisini başlatın\n";
                    errorMessage += "   - Veya PowerShell'de: Start-Service postgresql*\n";
                    errorMessage += "2. PostgreSQL'in Port 5432'de çalıştığını kontrol edin\n";
                    errorMessage += "3. Connection string'i kontrol edin (appsettings.json)\n\n";
                    errorMessage += "Bağlantı: Server=127.0.0.1;Port=5432;User Id=postgres;Password=1";
                }
                else
                {
                    errorMessage += "\n\nLütfen PostgreSQL şifresini kontrol edin (Default: 1).";
                }
                
                DevExpress.XtraEditors.XtraMessageBox.Show(errorMessage, "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; // Stop app
            }

            // 4. Run UI
            Application.Run(new LoginForm());
        }
    }
}

