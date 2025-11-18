using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.UI.Services;

namespace BankApp.UI.Forms
{
    public class IPODetailForm : XtraForm
    {
        private readonly string _company;
        private readonly string _ticker;
        private readonly string _priceRange;
        private readonly string _date;
        private readonly string _status;
        private double? _price;
        
        private LabelControl lblTitle;
        private LabelControl lblStatus;
        private LabelControl lblDate;
        private LabelControl lblPriceRange;
        private LabelControl lblPriceInfo;
        private SpinEdit spinLot;
        private TextEdit txtTotal;
        private SimpleButton btnSubmit;
        private SimpleButton btnCancel;
        
        public IPODetailForm(string company, string ticker, string priceRange, string date, string status)
        {
            _company = company;
            _ticker = ticker;
            _priceRange = priceRange;
            _date = date;
            _status = status;
            
            // Parse price from range (use midpoint or single value)
            _price = ParsePrice(priceRange);
            
            InitializeForm();
            InitializeControls();
        }
        
        private void InitializeForm()
        {
            this.Text = "Halka Arz Detayı";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(24, 24, 24);
        }
        
        private void InitializeControls()
        {
            int y = 20;
            
            // Title
            lblTitle = new LabelControl();
            lblTitle.Text = _company;
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;
            lblTitle.Location = new Point(20, y);
            lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            lblTitle.Size = new Size(450, 35);
            this.Controls.Add(lblTitle);
            y += 45;
            
            // Ticker
            var lblTicker = new LabelControl();
            lblTicker.Text = $"Sembol: {_ticker}";
            lblTicker.Appearance.Font = new Font("Segoe UI", 11F);
            lblTicker.Appearance.ForeColor = Color.FromArgb(150, 150, 150);
            lblTicker.Location = new Point(20, y);
            this.Controls.Add(lblTicker);
            y += 30;
            
            // Status badge
            lblStatus = new LabelControl();
            lblStatus.Text = _status;
            lblStatus.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblStatus.Appearance.ForeColor = GetStatusColor(_status);
            lblStatus.Appearance.BackColor = GetStatusBackColor(_status);
            lblStatus.AutoSizeMode = LabelAutoSizeMode.None;
            lblStatus.Size = new Size(100, 26);
            lblStatus.Location = new Point(20, y);
            lblStatus.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblStatus.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            this.Controls.Add(lblStatus);
            y += 40;
            
            // Date
            lblDate = new LabelControl();
            lblDate.Text = $"Tarih: {_date}";
            lblDate.Appearance.Font = new Font("Segoe UI", 10F);
            lblDate.Appearance.ForeColor = Color.White;
            lblDate.Location = new Point(20, y);
            this.Controls.Add(lblDate);
            y += 30;
            
            // Price Range
            lblPriceRange = new LabelControl();
            lblPriceRange.Text = $"Fiyat Aralığı: {(_price.HasValue ? _priceRange : "—")}";
            lblPriceRange.Appearance.Font = new Font("Segoe UI", 10F);
            lblPriceRange.Appearance.ForeColor = Color.White;
            lblPriceRange.Location = new Point(20, y);
            this.Controls.Add(lblPriceRange);
            y += 30;
            
            // Price Info (for TBA)
            if (!_price.HasValue)
            {
                lblPriceInfo = new LabelControl();
                lblPriceInfo.Text = "ℹ️ Fiyat açıklanınca talep alınacaktır.";
                lblPriceInfo.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
                lblPriceInfo.Appearance.ForeColor = Color.FromArgb(255, 152, 0);
                lblPriceInfo.Location = new Point(20, y);
                lblPriceInfo.AutoSizeMode = LabelAutoSizeMode.None;
                lblPriceInfo.Size = new Size(400, 20);
                this.Controls.Add(lblPriceInfo);
                y += 25;
            }
            y += 25;
            
            // Lot selection
            var lblLot = new LabelControl();
            lblLot.Text = "Lot Sayısı";
            lblLot.Appearance.Font = new Font("Segoe UI", 10F);
            lblLot.Appearance.ForeColor = Color.FromArgb(200, 200, 200);
            lblLot.Location = new Point(20, y);
            this.Controls.Add(lblLot);
            y += 25;
            
            spinLot = new SpinEdit();
            spinLot.Size = new Size(200, 28);
            spinLot.Location = new Point(20, y);
            spinLot.Properties.MinValue = 1;
            spinLot.Properties.MaxValue = 100;
            spinLot.Properties.Increment = 1;
            spinLot.EditValue = 1;
            spinLot.Properties.Appearance.BackColor = Color.FromArgb(35, 35, 35);
            spinLot.Properties.Appearance.ForeColor = Color.White;
            spinLot.EditValueChanged += (s, e) => UpdateTotal();
            this.Controls.Add(spinLot);
            y += 40;
            
            // Total
            var lblTotalLabel = new LabelControl();
            lblTotalLabel.Text = "Toplam Tutar";
            lblTotalLabel.Appearance.Font = new Font("Segoe UI", 10F);
            lblTotalLabel.Appearance.ForeColor = Color.FromArgb(200, 200, 200);
            lblTotalLabel.Location = new Point(20, y);
            this.Controls.Add(lblTotalLabel);
            y += 25;
            
            txtTotal = new TextEdit();
            txtTotal.Size = new Size(200, 28);
            txtTotal.Location = new Point(20, y);
            txtTotal.Properties.ReadOnly = true;
            txtTotal.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            txtTotal.Properties.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            txtTotal.Properties.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.Controls.Add(txtTotal);
            y += 60;
            
            // Buttons
            btnSubmit = new SimpleButton();
            btnSubmit.Text = "Talep Gönder";
            btnSubmit.Size = new Size(140, 38);
            btnSubmit.Location = new Point(20, y);
            btnSubmit.Appearance.BackColor = Color.FromArgb(38, 166, 91);
            btnSubmit.Appearance.ForeColor = Color.White;
            btnSubmit.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSubmit.Appearance.Options.UseBackColor = true;
            btnSubmit.Appearance.Options.UseForeColor = true;
            btnSubmit.Appearance.Options.UseFont = true;
            btnSubmit.Click += BtnSubmit_Click;
            btnSubmit.Enabled = (_status == "Open" || _status == "Upcoming") && _price.HasValue;
            this.Controls.Add(btnSubmit);
            
            btnCancel = new SimpleButton();
            btnCancel.Text = "İptal";
            btnCancel.Size = new Size(100, 38);
            btnCancel.Location = new Point(170, y);
            btnCancel.Appearance.BackColor = Color.FromArgb(60, 60, 60);
            btnCancel.Appearance.ForeColor = Color.White;
            btnCancel.Appearance.Options.UseBackColor = true;
            btnCancel.Appearance.Options.UseForeColor = true;
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
            
            UpdateTotal();
        }
        
        private void UpdateTotal()
        {
            if (!_price.HasValue)
            {
                txtTotal.EditValue = "—";
                txtTotal.Properties.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
                return;
            }
            
            var lot = (int)spinLot.Value;
            var total = lot * _price.Value;
            txtTotal.EditValue = total.ToString("N2") + " ₺";
            txtTotal.Properties.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
        }
        
        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_price.HasValue)
                {
                    XtraMessageBox.Show(
                        "Fiyat henüz açıklanmadı.",
                        "Uyarı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
                
                var lot = (int)spinLot.Value;
                var total = lot * _price.Value;
                
                var request = new IPORequest
                {
                    Symbol = _ticker,
                    Company = _company,
                    Lot = lot,
                    Price = _price.Value,
                    Total = total,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };
                
                var success = IPOStore.SaveRequest(request);
                
                if (success)
                {
                    XtraMessageBox.Show(
                        $"Talebiniz başarıyla kaydedildi!\n\n" +
                        $"Şirket: {_company}\n" +
                        $"Lot: {lot}\n" +
                        $"Toplam: {total:N2} ₺\n\n" +
                        $"Durum: Pending (Onay Bekliyor)",
                        "Talep Alındı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    XtraMessageBox.Show(
                        "Talep kaydedilemedi. Lütfen tekrar deneyin.",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IPO Submit Error: {ex.Message}");
                XtraMessageBox.Show(
                    "Bir hata oluştu. Lütfen tekrar deneyin.",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
        
        private double? ParsePrice(string priceRange)
        {
            try
            {
                // Handle formats: "₺180-200", "₺45-55", "TBA", "$95"
                if (string.IsNullOrWhiteSpace(priceRange) || 
                    priceRange.Contains("TBA") || 
                    priceRange.Contains("Announced"))
                    return null;
                
                var cleaned = priceRange.Replace("₺", "").Replace("$", "").Trim();
                
                if (cleaned.Contains("-"))
                {
                    var parts = cleaned.Split('-');
                    if (parts.Length == 2)
                    {
                        if (double.TryParse(parts[0].Trim(), out var low) && 
                            double.TryParse(parts[1].Trim(), out var high))
                        {
                            return (low + high) / 2;
                        }
                    }
                }
                
                if (double.TryParse(cleaned, out var price))
                    return price;
                    
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        private Color GetStatusColor(string status)
        {
            return status switch
            {
                "Open" => Color.FromArgb(76, 175, 80),
                "Upcoming" => Color.FromArgb(33, 150, 243),
                "Closed" => Color.FromArgb(150, 150, 150),
                "Announced" => Color.FromArgb(255, 152, 0),
                _ => Color.White
            };
        }
        
        private Color GetStatusBackColor(string status)
        {
            return status switch
            {
                "Open" => Color.FromArgb(30, 60, 35),
                "Upcoming" => Color.FromArgb(20, 50, 80),
                "Closed" => Color.FromArgb(50, 50, 50),
                "Announced" => Color.FromArgb(60, 45, 20),
                _ => Color.FromArgb(40, 40, 40)
            };
        }
    }
}
