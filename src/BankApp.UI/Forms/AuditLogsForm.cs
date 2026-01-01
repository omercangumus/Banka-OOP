using System;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Denetim logları formu - Sistem olaylarını listeler
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class AuditLogsForm : XtraForm
    {
        private readonly AuditRepository _repository;

        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public AuditLogsForm()
        {
            InitializeComponent();
            var context = new DapperContext();
            _repository = new AuditRepository(context);
            LoadLogs();
        }

        /// <summary>
        /// Logları veritabanından yükler
        /// </summary>
        private async void LoadLogs()
        {
            if (grdLoglar == null || _repository == null)
            {
                return;
            }

            try
            {
                var logs = await _repository.GetAllLogsAsync();
                grdLoglar.DataSource = logs ?? new System.Collections.Generic.List<BankApp.Core.Entities.AuditLog>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Loglar yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
