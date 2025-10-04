using System;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class AuditLogsForm : XtraForm
    {
        private readonly AuditRepository _repository;

        public AuditLogsForm()
        {
            InitializeComponent();
            var context = new DapperContext();
            _repository = new AuditRepository(context);
            LoadLogs();
        }

        private async void LoadLogs()
        {
            // SORUN DÜZELTİLDİ: Null kontrolü eklendi
            if (gridLogs == null || _repository == null)
            {
                return;
            }

            try
            {
                var logs = await _repository.GetAllLogsAsync();
                gridLogs.DataSource = logs ?? new System.Collections.Generic.List<BankApp.Core.Entities.AuditLog>();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Loglar yüklenemedi: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
