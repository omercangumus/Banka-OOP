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
            try
            {
                var logs = await _repository.GetAllLogsAsync();
                gridLogs.DataSource = logs;
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("Loglar yüklenemedi: " + ex.Message);
            }
        }
    }
}
