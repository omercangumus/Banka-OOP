using System;
using System.Windows.Forms;
using DevExpress.Skins;
using DevExpress.UserSkins;
using BankApp.Infrastructure.Data;
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
            DevExpress.UserSkins.BonusSkins.Register();
            DevExpress.LookAndFeel.UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");

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
                 DevExpress.XtraEditors.XtraMessageBox.Show($"Kritik Başlangıç Hatası:\n{ex.Message}\n\nLütfen PostgreSQL şifresini kontrol edin (Default: 1).", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return; // Stop app
            }

            // 4. Run UI
            Application.Run(new LoginForm());
        }
    }
}
