using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Dapper;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    public partial class AddContactForm : XtraForm
    {
        public string ContactName { get; private set; }
        public string ContactIBAN { get; private set; }
        public string ContactColor { get; private set; }

        public AddContactForm()
        {
            InitializeComponent();
        }

        private TextEdit txtName;
        private TextEdit txtIBAN;
        private ColorPickEdit colorPicker;
        private SimpleButton btnSave;
        private SimpleButton btnCancel;

        private void InitializeComponent()
        {
            this.Text = "Yeni Kişi Ekle";
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.LookAndFeel.SetSkinStyle("Office 2019 Black");

            // Name Label & Input
            var lblName = new LabelControl { Text = "Kişi Adı:", Location = new Point(20, 30) };
            txtName = new TextEdit { Location = new Point(120, 27), Size = new Size(240, 28) };
            txtName.Properties.NullText = "ör: Annem";

            // IBAN Label & Input
            var lblIBAN = new LabelControl { Text = "IBAN:", Location = new Point(20, 70) };
            txtIBAN = new TextEdit { Location = new Point(120, 67), Size = new Size(240, 28) };
            txtIBAN.Properties.NullText = "TR00 0000 0000 0000 0000 0000 00";
            txtIBAN.Properties.MaxLength = 26;

            // Color Picker
            var lblColor = new LabelControl { Text = "Avatar Rengi:", Location = new Point(20, 110) };
            colorPicker = new ColorPickEdit { Location = new Point(120, 107), Size = new Size(100, 28) };
            colorPicker.Color = Color.DodgerBlue;

            // Buttons
            btnSave = new SimpleButton { Text = "Kaydet", Location = new Point(120, 180), Size = new Size(100, 35) };
            btnSave.Appearance.BackColor = Color.FromArgb(34, 197, 94); // Green
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Click += BtnSave_Click;

            btnCancel = new SimpleButton { Text = "İptal", Location = new Point(230, 180), Size = new Size(100, 35) };
            btnCancel.Appearance.BackColor = Color.FromArgb(100, 100, 100);
            btnCancel.Appearance.ForeColor = Color.White;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] { lblName, txtName, lblIBAN, txtIBAN, lblColor, colorPicker, btnSave, btnCancel });
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtIBAN.Text))
            {
                XtraMessageBox.Show("Lütfen tüm alanları doldurun.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clean IBAN
            string cleanIBAN = txtIBAN.Text.Replace(" ", "").ToUpper();
            if (!cleanIBAN.StartsWith("TR") || cleanIBAN.Length < 20)
            {
                XtraMessageBox.Show("Geçerli bir IBAN girin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ContactName = txtName.Text.Trim();
            ContactIBAN = cleanIBAN;
            ContactColor = ColorTranslator.ToHtml(colorPicker.Color);

            try
            {
                var context = new DapperContext();
                using (var conn = context.CreateConnection())
                {
                    await conn.ExecuteAsync(
                        "INSERT INTO \"QuickContacts\" (\"UserId\", \"Name\", \"IBAN\", \"ColorHex\") VALUES (1, @Name, @IBAN, @Color)",
                        new { Name = ContactName, IBAN = ContactIBAN, Color = ContactColor });
                }

                XtraMessageBox.Show($"{ContactName} başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Kayıt hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
