using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Core.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BankApp.UI.Forms
{
    public partial class TransferForm : XtraForm
    {
        private FlowLayoutPanel pnlStories;
        private PanelControl pnlInput;
        private LookUpEdit lueAccount;
        private TextEdit txtIBAN;
        private CalcEdit txtAmount;
        private TextEdit txtDescription;
        private SimpleButton btnSend;
        private ListBoxControl lstRecent;

        private readonly AccountRepository _accountRepo;
        private readonly TransactionService _transactionService;

        public TransferForm()
        {
            InitializeComponent();
            
            // Services
            var context = new DapperContext();
            _accountRepo = new AccountRepository(context);
            var transRepo = new TransactionRepository(context);
            var auditRepo = new AuditRepository(context);
            _transactionService = new TransactionService(_accountRepo, transRepo, auditRepo);

            LoadAccounts();
            SetupStories();
            SetupRecents();
        }

        private void InitializeComponent()
        {
            this.Text = "💸 Para Transferi - Hızlı ve Güvenli";
            this.Size = new Size(500, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.LookAndFeel.SetSkinStyle("Office 2019 Black");

            // 1. Top Section: Stories (Quick Contacts)
            var lblStories = new LabelControl { Text = "Hızlı Gönder (Story Mode)", Location = new Point(20, 20) };
            lblStories.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStories.Appearance.ForeColor = Color.Gold;

            pnlStories = new FlowLayoutPanel();
            pnlStories.Location = new Point(0, 50);
            pnlStories.Size = new Size(500, 110);
            pnlStories.FlowDirection = FlowDirection.LeftToRight;
            pnlStories.WrapContents = false;
            pnlStories.AutoScroll = true;
            pnlStories.Padding = new Padding(10, 0, 10, 0);

            // 2. Middle Section: Inputs
            pnlInput = new PanelControl();
            pnlInput.Location = new Point(20, 170);
            pnlInput.Size = new Size(440, 300);
            pnlInput.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            pnlInput.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            lueAccount = new LookUpEdit();
            lueAccount.Location = new Point(20, 40);
            lueAccount.Size = new Size(400, 35);
            lueAccount.Properties.NullText = "Gönderen Hesap Seçin...";
            lueAccount.Properties.Appearance.Font = new Font("Segoe UI", 11F);

            txtIBAN = new TextEdit();
            txtIBAN.Location = new Point(20, 90);
            txtIBAN.Size = new Size(400, 35);
            txtIBAN.Properties.NullText = "TR00 0000 ... (IBAN)";
            txtIBAN.Properties.Appearance.Font = new Font("Segoe UI", 11F);

            txtAmount = new CalcEdit();
            txtAmount.Location = new Point(20, 140);
            txtAmount.Size = new Size(400, 35);
            txtAmount.Properties.NullText = "Tutar (TL)";
            txtAmount.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtAmount.Properties.DisplayFormat.FormatString = "N2";
            txtAmount.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;

            txtDescription = new TextEdit();
            txtDescription.Location = new Point(20, 190);
            txtDescription.Size = new Size(400, 35);
            txtDescription.Properties.NullText = "Açıklama (Opsiyonel)";
            txtDescription.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            
            pnlInput.Controls.AddRange(new Control[] { 
                new LabelControl { Text = "Hesap Bilgileri", Location = new Point(20, 10), Appearance = { Font = new Font("Segoe UI", 10F, FontStyle.Bold) } },
                lueAccount, txtIBAN, txtAmount, txtDescription 
            });

            // 3. Bottom Section: Recents & Button
            var lblRecent = new LabelControl { Text = "Son İşlemler", Location = new Point(20, 480) };
            
            lstRecent = new ListBoxControl();
            lstRecent.Location = new Point(20, 500);
            lstRecent.Size = new Size(440, 100);
            lstRecent.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            lstRecent.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;

            btnSend = new SimpleButton();
            btnSend.Text = "PARA GÖNDER 🚀";
            btnSend.Location = new Point(20, 620);
            btnSend.Size = new Size(440, 60);
            btnSend.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            btnSend.Click += BtnSend_Click;
            btnSend.Paint += BtnSend_Paint; // Gradient Effect

            this.Controls.AddRange(new Control[] { lblStories, pnlStories, pnlInput, lblRecent, lstRecent, btnSend });
        }

        private void SetupStories()
        {
            var contacts = new[] {
                new { Name = "Annem", IBAN = "TR12 0006 1000 0000 0012 3456 78", Color = Color.HotPink },
                new { Name = "Ev Sahibi", IBAN = "TR99 0006 1000 0000 0099 8877 66", Color = Color.Orange },
                new { Name = "Kanka", IBAN = "TR55 0006 1000 0000 0055 4433 22", Color = Color.MediumPurple },
                new { Name = "Eşim", IBAN = "TR10 0006 1000 0000 0011 2233 44", Color = Color.Crimson },
                new { Name = "Yönetici", IBAN = "TR34 0006 1000 0000 0066 7788 99", Color = Color.SlateGray }
            };

            foreach (var c in contacts)
            {
                var pnl = new Panel { Size = new Size(90, 100), Margin = new Padding(5) };
                
                // Avatar Circle
                var btn = new SimpleButton();
                btn.Size = new Size(70, 70);
                btn.Location = new Point(10, 0);
                btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                btn.Appearance.BackColor = c.Color;
                btn.Text = c.Name.Substring(0, 1);
                btn.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
                btn.Appearance.ForeColor = Color.White;
                // Make Circular
                System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddEllipse(0, 0, 70, 70);
                btn.Region = new Region(path);

                btn.Click += (s, e) => {
                    txtIBAN.Text = c.IBAN;
                    txtDescription.Text = $"{c.Name} ödeme";
                    XtraMessageBox.Show($"{c.Name} seçildi!", "Hızlı Seçim", MessageBoxButtons.OK, MessageBoxIcon.Information);
                };

                var lbl = new LabelControl { Text = c.Name, AutoSize = false, Size = new Size(90, 20), Location = new Point(0, 75) };
                lbl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                pnl.Controls.Add(btn);
                pnl.Controls.Add(lbl);
                pnlStories.Controls.Add(pnl);
            }
        }

        private void SetupRecents()
        {
            lstRecent.Items.Add("🏠 Kira Ödemesi - 15,000 TL - TR99...");
            lstRecent.Items.Add("☕ Starbucks - 250 TL - TR44...");
            lstRecent.Items.Add("🛒 Migros - 1,200 TL - TR33...");
        }

        private void BtnSend_Paint(object sender, PaintEventArgs e)
        {
            // Gradient Background for Button
            using (LinearGradientBrush brush = new LinearGradientBrush(btnSend.ClientRectangle, 
                Color.FromArgb(37, 99, 235), Color.FromArgb(147, 51, 234), 45F))
            {
                e.Graphics.FillRectangle(brush, btnSend.ClientRectangle);
            }
            // Draw Text
            TextRenderer.DrawText(e.Graphics, btnSend.Text, btnSend.Appearance.Font, btnSend.ClientRectangle, 
                btnSend.Appearance.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private async void LoadAccounts()
        {
            try
            {
                var accounts = await _accountRepo.GetAllAsync();
                lueAccount.Properties.DataSource = accounts;
                lueAccount.Properties.DisplayMember = "AccountNumber";
                lueAccount.Properties.ValueMember = "Id";
                lueAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("AccountNumber", "Hesap No"));
                lueAccount.Properties.Columns.Add(new DevExpress.XtraEditors.Controls.LookUpColumnInfo("Balance", "Bakiye"));
            }
            catch {}
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if(lueAccount.EditValue == null) { XtraMessageBox.Show("Hesap Seçiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if(string.IsNullOrWhiteSpace(txtIBAN.Text)) { XtraMessageBox.Show("IBAN Giriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if(txtAmount.Value <= 0) { XtraMessageBox.Show("Tutar Giriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try {
                int accountId = (int)lueAccount.EditValue;
                var res = await _transactionService.TransferMoneyAsync(accountId, txtIBAN.Text, txtAmount.Value, txtDescription.Text);
                
                if(res == null)
                {
                    XtraMessageBox.Show("Transfer Başarılı! ✅", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show($"Hata: {res}", "Transfer Başarısız", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } catch (Exception ex) {
                XtraMessageBox.Show($"Sistem Hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
