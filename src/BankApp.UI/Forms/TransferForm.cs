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
using Dapper;
using System.Threading.Tasks;

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

        private async void SetupStories()
        {
            try {
                pnlStories.Controls.Clear();
                var context = new DapperContext();
                using (var conn = context.CreateConnection())
                {
                    // **FETCH USER'S QUICK CONTACTS**
                    var contacts = await conn.QueryAsync<QuickContact>(@"
                        SELECT * FROM ""QuickContacts"" 
                        WHERE ""UserId"" = @UserId 
                        ORDER BY ""CreatedAt"" DESC",
                        new { UserId = AppEvents.CurrentSession.UserId });
                    
                    // Add "+" Button for New Contact
                    var pnlAdd = new Panel { Size = new Size(90, 100), Margin = new Padding(5) };
                    var btnAdd = new SimpleButton { Size = new Size(70, 70), Location = new Point(10, 0), Text = "+", ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder };
                    btnAdd.Appearance.BackColor = Color.FromArgb(50, 50, 50);
                    btnAdd.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
                    btnAdd.Appearance.ForeColor = Color.White;
                    System.Drawing.Drawing2D.GraphicsPath pathAdd = new System.Drawing.Drawing2D.GraphicsPath();
                    pathAdd.AddEllipse(0, 0, 70, 70);
                    btnAdd.Region = new Region(pathAdd);
                    btnAdd.Click += (s, e) => {
                        AddContactForm addForm = new AddContactForm();
                        if (addForm.ShowDialog() == DialogResult.OK)
                        {
                            SetupStories(); // Refresh
                        }
                    };
                    
                    var lblAdd = new LabelControl { Text = "Ekle", AutoSize = false, Size = new Size(90, 20), Location = new Point(0, 75) };
                    lblAdd.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
                    pnlAdd.Controls.AddRange(new Control[] { btnAdd, lblAdd });
                    pnlStories.Controls.Add(pnlAdd);

                    foreach (var c in contacts)
                    {
                        var pnl = new Panel { Size = new Size(90, 100), Margin = new Padding(5) };
                        var btn = new SimpleButton { Size = new Size(70, 70), Location = new Point(10, 0), Text = c.Name.Substring(0, 1) };
                        btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
                        try { btn.Appearance.BackColor = ColorTranslator.FromHtml(c.ColorHex); } catch { btn.Appearance.BackColor = Color.DodgerBlue; }
                        btn.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
                        btn.Appearance.ForeColor = Color.White;
                        
                        System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
                        path.AddEllipse(0, 0, 70, 70);
                        btn.Region = new Region(path);

                        btn.Click += (s, e) => {
                            txtIBAN.Text = c.IBAN;
                            txtDescription.Text = $"{c.Name} ödeme";
                        };

                        var lbl = new LabelControl { Text = c.Name, AutoSize = false, Size = new Size(90, 20), Location = new Point(0, 75) };
                        lbl.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

                        pnl.Controls.AddRange(new Control[] { btn, lbl });
                        pnlStories.Controls.Add(pnl);
                    }
                }
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"Stories Error: {ex.Message}");
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
                
                // Auto-select first account if available
                if (accounts.Any())
                {
                    lueAccount.EditValue = accounts.First().Id;
                }
            }
            catch {}
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if(lueAccount.EditValue == null) { XtraMessageBox.Show("Lütfen gönderen hesabı seçiniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if(string.IsNullOrWhiteSpace(txtIBAN.Text)) { XtraMessageBox.Show("Lütfen IBAN giriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if(txtAmount.Value <= 0) { XtraMessageBox.Show("Geçerli bir tutar giriniz!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try {
                int accountId = (int)lueAccount.EditValue;
                
                // IBAN Sanitization: Remove spaces and trim
                string cleanIban = txtIBAN.Text.Trim().Replace(" ", "");
                
                var res = await _transactionService.TransferMoneyAsync(accountId, cleanIban, txtAmount.Value, txtDescription.Text);
                
                if(res == null) // Success returns null
                {
                    XtraMessageBox.Show("Transfer Başarıyla Gerçekleşti! ✅\n\nAlıcıya bildirim gönderildi.", "İşlem Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show($"Transfer Yapılamadı:\n{res}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            } catch (Exception ex) {
                XtraMessageBox.Show($"Beklenmedik Bir Hata Oluştu:\n{ex.Message}", "Sistem Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
