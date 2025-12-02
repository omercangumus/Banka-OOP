using System;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using DevExpress.Skins;
// using DevExpress.UserSkins;
using Npgsql;
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
        // STATIC LOG DIR - created IMMEDIATELY
        private static readonly string LogDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "NovaBank", "logs");
        
        [STAThread]
        static async System.Threading.Tasks.Task Main()
        {
            // ==================================================================
            // STEP 0: CREATE LOG DIR IMMEDIATELY + TRACE
            // ==================================================================
            try { Directory.CreateDirectory(LogDir); } catch { }
            WriteTrace("APP_START");
            
            // ==================================================================
            // STEP 0.5: CONFIGURE QUESTPDF LICENSE
            // ==================================================================
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            WriteTrace("QUESTPDF_CONFIGURED");
            
            // ==================================================================
            // STEP 1: GLOBAL EXCEPTION HANDLERS - PREVENT SILENT CRASHES
            // ==================================================================
            
            // Handler 1: UI thread exceptions
            Application.ThreadException += (sender, e) =>
            {
                WriteTrace($"THREAD_EXCEPTION: {e.Exception?.Message}");
                LogAndShowException(e.Exception, "UI Thread Exception");
            };
            
            // Handler 2: Non-UI thread exceptions
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogAndShowException(ex, "AppDomain Unhandled Exception");
            };
            
            // Handler 3: Unobserved task exceptions
            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogAndShowException(e.Exception, "Unobserved Task Exception");
                e.SetObserved();
            };
            
            // Set exception mode to catch all exceptions
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // DevExpress Skins
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");

            try
            {
                // 1. System Level Initialization
                var sysInit = new BankApp.Infrastructure.Initialization.SystemInitializer();
                sysInit.StartPostgresService();
                sysInit.CreateDatabaseIfNotExists();

                // 2. EF Core Code-First Initialization
                using (var context = new BankApp.Infrastructure.Data.BankDbContext())
                {
                    context.Database.EnsureCreated();
                }

                // 3. Existing Seeding Logic
                var dataInit = new BankApp.Infrastructure.Data.DbInitializer();
                dataInit.Initialize(); 
                
                // 4. ENSURE ADMIN USER EXISTS (Fix Login Issue) - SORUN DÜZELTİLDİ: Daha iyi hata yönetimi
                try 
                {
                    var dapperContext = new Infrastructure.Data.DapperContext();
                    var userRepo = new Infrastructure.Data.UserRepository(dapperContext);
                    
                    // SORUN DÜZELTİLDİ: Veritabanı bağlantısını test et
                    try
                    {
                        var adminUser = await userRepo.GetByUsernameAsync("admin");
                        if (adminUser == null)
                        {
                            System.Diagnostics.Debug.WriteLine("Admin kullanıcısı bulunamadı, oluşturuluyor...");
                            
                            var auditRepo = new Infrastructure.Data.AuditRepository(dapperContext);
                            var emailService = new Infrastructure.Services.SmtpEmailService();
                            var authService = new Infrastructure.Services.AuthService(userRepo, emailService, auditRepo);
                            
                            var newUser = new BankApp.Core.Entities.User 
                            { 
                                Username = "admin", 
                                Email = "admin@novabank.com", 
                                FullName = "System Administrator", 
                                Role = "Admin" 
                            };
                            
                            // Register (Hashes password 'admin123')
                            var registerResult = await authService.RegisterAsync(newUser, "admin123");
                            if (registerResult != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Admin kayıt hatası: {registerResult}");
                            }
                            
                            // Auto Verify
                            var createdAdmin = await userRepo.GetByUsernameAsync("admin");
                            if (createdAdmin != null)
                            {
                                createdAdmin.IsVerified = true;
                                createdAdmin.IsActive = true;
                                await userRepo.UpdateAsync(createdAdmin);
                                System.Diagnostics.Debug.WriteLine("Admin kullanıcısı oluşturuldu ve doğrulandı.");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Admin kullanıcısı mevcut: {adminUser.Username} (Verified: {adminUser.IsVerified}, Active: {adminUser.IsActive})");
                        }
                    }
                    catch (Npgsql.NpgsqlException dbEx)
                    {
                        // Veritabanı bağlantı hatası - uygulamayı durdurma ama uyarı ver
                        System.Diagnostics.Debug.WriteLine($"VERİTABANI BAĞLANTI HATASI: {dbEx.Message}");
                        if (dbEx.Message.Contains("Failed to connect") || dbEx.Message.Contains("5432"))
                        {
                            throw new Exception("PostgreSQL bağlantı hatası! Lütfen PostgreSQL servisinin çalıştığını kontrol edin.", dbEx);
                        }
                    }
                }
                catch (Exception seedEx)
                {
                    // SORUN DÜZELTİLDİ: Hata mesajını daha görünür yap
                    System.Diagnostics.Debug.WriteLine($"Admin Seeding Failed: {seedEx.Message}");
                    if (seedEx.InnerException != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Inner Exception: {seedEx.InnerException.Message}");
                    }
                    // Uygulama başlayabilir ama admin kullanıcısı olmayabilir - kullanıcıya uyarı verilebilir
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Kritik Başlangıç Hatası:\n\n{ex.Message}";
                
                if (ex.Message.Contains("Failed to connect") || ex.Message.Contains("5432"))
                {
                    errorMessage += "\n\n🔴 PostgreSQL servisi çalışmıyor olabilir!\n";
                    errorMessage += "Çözüm: Services.msc -> postgresql başlatın.";
                }
                
                DevExpress.XtraEditors.XtraMessageBox.Show(errorMessage, "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return; 
            }

            // 5. Run UI (wrapped in try/catch)
            try
            {
                Application.Run(new LoginForm());
            }
            catch (Exception runEx)
            {
                LogAndShowException(runEx, "Application.Run Exception");
            }
        }
        
        /// <summary>
        /// Logs exception to file and shows MessageBox. Called by all global handlers.
        /// </summary>
        private static void LogAndShowException(Exception ex, string source)
        {
            if (ex == null) return;
            
            string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string logDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "NovaBank",
                "logs"
            );
            Directory.CreateDirectory(logDirectory);
            
            string logFilePath = Path.Combine(logDirectory, $"crash-{timestamp}.txt");
            
            // Build detailed log content
            var logContent = new System.Text.StringBuilder();
            logContent.AppendLine("=".PadRight(70, '='));
            logContent.AppendLine($"CRASH LOG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            logContent.AppendLine($"SOURCE: {source}");
            logContent.AppendLine($"LAST_UI_ACTION: {UiActionTrace.LastAction}");
            logContent.AppendLine("=".PadRight(70, '='));
            logContent.AppendLine();
            logContent.AppendLine($"Exception Type: {ex.GetType().FullName}");
            logContent.AppendLine($"Message: {ex.Message}");
            logContent.AppendLine();
            logContent.AppendLine("Stack Trace:");
            logContent.AppendLine(ex.StackTrace ?? "(no stack trace)");
            logContent.AppendLine();
            
            // Include inner exceptions
            var innerEx = ex.InnerException;
            int innerCount = 1;
            while (innerEx != null)
            {
                logContent.AppendLine($"--- Inner Exception #{innerCount} ---");
                logContent.AppendLine($"Type: {innerEx.GetType().FullName}");
                logContent.AppendLine($"Message: {innerEx.Message}");
                logContent.AppendLine($"Stack Trace: {innerEx.StackTrace ?? "(no stack trace)"}");
                logContent.AppendLine();
                innerEx = innerEx.InnerException;
                innerCount++;
            }
            
            logContent.AppendLine("Environment:");
            logContent.AppendLine($"  .NET Runtime: {Environment.Version}");
            logContent.AppendLine($"  OS: {Environment.OSVersion}");
            logContent.AppendLine($"  Working Directory: {Environment.CurrentDirectory}");
            logContent.AppendLine($"  Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
            logContent.AppendLine("=".PadRight(70, '='));
            
            // Write to file
            try
            {
                File.WriteAllText(logFilePath, logContent.ToString());
            }
            catch
            {
                // If we can't write log, at least show the error
            }
            
            // Show MessageBox
            string messageBoxText = $"❌ APPLICATION CRASH ❌\n\n" +
                                   $"Source: {source}\n" +
                                   $"Exception Type: {ex.GetType().Name}\n\n" +
                                   $"Message:\n{ex.Message}\n\n" +
                                   $"Log saved to:\n{logFilePath}";
            
            MessageBox.Show(messageBoxText, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        /// <summary>
        /// Writes a simple trace line to trace.txt for debugging startup/crash issues
        /// </summary>
        private static void WriteTrace(string message)
        {
            try
            {
                var tracePath = Path.Combine(LogDir, "trace.txt");
                File.AppendAllText(tracePath, $"{DateTime.Now:HH:mm:ss.fff} | {message}\n");
            }
            catch { }
        }
    }
}

