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
        static async System.Threading.Tasks.Task Main()
        {
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
                
                // 4. ENSURE ADMIN USER EXISTS (Fix Login Issue)
                try 
                {
                    var dapperContext = new Infrastructure.Data.DapperContext();
                    var userRepo = new Infrastructure.Data.UserRepository(dapperContext);
                    
                    var adminUser = await userRepo.GetByUsernameAsync("admin");
                    if (adminUser == null)
                    {
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
                        await authService.RegisterAsync(newUser, "admin123");
                        
                        // Auto Verify
                        var createdAdmin = await userRepo.GetByUsernameAsync("admin");
                        if (createdAdmin != null)
                        {
                            createdAdmin.IsVerified = true;
                            createdAdmin.IsActive = true;
                            await userRepo.UpdateAsync(createdAdmin);
                        }
                    }
                }
                catch (Exception seedEx)
                {
                    System.Diagnostics.Debug.WriteLine("Admin Seeding Failed: " + seedEx.Message);
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

            // 5. Run UI
            Application.Run(new LoginForm());
        }
    }
}

