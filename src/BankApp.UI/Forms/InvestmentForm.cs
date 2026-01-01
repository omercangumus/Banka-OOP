#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Yatırım Portföyü Formu - Yatırım ve borsa işlemleri
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class InvestmentForm : XtraForm
    {
        private MarketSimulatorService _marketSimulator;
        private InvestmentService _investmentService;
        private int _customerId = 1; // Demo için varsayılan
        private string _selectedCurrency = "TRY";
        private decimal _usdRate = 32.50m;
        
        /// <summary>
        /// Form yapıcı metodu
        /// </summary>
        public InvestmentForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadStocks();
            LoadPortfolio();
        }

        /// <summary>
        /// Servisleri başlatır
        /// </summary>
        private void InitializeServices()
        {
            var context = new DapperContext();
            var accountRepo = new AccountRepository(context);
            var auditRepo = new AuditRepository(context);

            _marketSimulator = new MarketSimulatorService();
            _investmentService = new InvestmentService(_marketSimulator, accountRepo, auditRepo);
            
            // Fiyat değişimlerini dinle
            _marketSimulator.PriceChanged += OnPriceChanged;
            _marketSimulator.Start();
        }

        /// <summary>
        /// Fiyat değiştiğinde tetiklenir
        /// </summary>
        private void OnPriceChanged(object? sender, StockPriceChangedEventArgs e)
        {
            // UI thread'e geç
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => OnPriceChanged(sender, e)));
                return;
            }

            // Grid'leri güncelle
            try
            {
                if(grdwHisseler != null) grdwHisseler.RefreshData();
                if(grdwPortfoy != null) grdwPortfoy.RefreshData();
            }
            catch { }
        }

        /// <summary>
        /// Hisseleri yükler
        /// </summary>
        private void LoadStocks()
        {
            if (grdHisseler == null) return;
            grdHisseler.DataSource = _marketSimulator.GetAllStocks();
            grdwHisseler.RefreshData();
        }

        /// <summary>
        /// Portföyü yükler
        /// </summary>
        private void LoadPortfolio()
        {
            if (grdPortfoy == null || lblPortfoyDegeri == null) return;

            var portfolio = _investmentService.GetPortfolio(_customerId);
            grdPortfoy.DataSource = portfolio;
            grdwPortfoy.RefreshData();
            
            // Toplam portföy değerini hesapla
            decimal total = 0;
            foreach (var item in portfolio)
            {
                total += item.CurrentValue;
            }
            
            decimal displayValue = ConvertCurrency(total);
            lblPortfoyDegeri.Text = $"{GetCurrencySymbol()} {displayValue:N2}";
        }

        /// <summary>
        /// Para birimini dönüştürür
        /// </summary>
        private decimal ConvertCurrency(decimal tlValue)
        {
            return _selectedCurrency switch
            {
                "USD" => tlValue / _usdRate,
                _ => tlValue
            };
        }

        /// <summary>
        /// Para birimi sembolünü getirir
        /// </summary>
        private string GetCurrencySymbol()
        {
            return _selectedCurrency switch
            {
                "USD" => "$",
                _ => "₺"
            };
        }

        /// <summary>
        /// Hisse al butonu tıklama olayı
        /// </summary>
        private void btnHisseAl_Click(object sender, EventArgs e)
        {
            // Seçili hisseyi al
            var rowHandle = grdwHisseler.FocusedRowHandle;
            if (rowHandle < 0)
            {
                XtraMessageBox.Show("Lütfen bir hisse seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var stock = _marketSimulator.GetAllStocks()[rowHandle];
            
            // Alım popup'ı aç
            using (var buyForm = new StockBuyPopup(stock, _investmentService, _customerId))
            {
                if (buyForm.ShowDialog() == DialogResult.OK)
                {
                    LoadPortfolio();
                }
            }
        }

        /// <summary>
        /// Hisse sat butonu tıklama olayı
        /// </summary>
        private void btnHisseSat_Click(object sender, EventArgs e)
        {
            var rowHandle = grdwPortfoy.FocusedRowHandle;
            if (rowHandle < 0)
            {
                XtraMessageBox.Show("Satmak için portföyden bir hisse seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var portfolio = _investmentService.GetPortfolio(_customerId);
            if (rowHandle >= portfolio.Count) return;

            var item = portfolio[rowHandle];
            
            using (var sellForm = new StockSellPopup(item, _investmentService, _customerId))
            {
                if (sellForm.ShowDialog() == DialogResult.OK)
                {
                    LoadPortfolio();
                }
            }
        }

        /// <summary>
        /// Para birimi değiştirme olayı
        /// </summary>
        private void tglParaBirimi_Toggled(object sender, EventArgs e)
        {
            if (tglParaBirimi.IsOn)
                _selectedCurrency = "USD";
            else
                _selectedCurrency = "TRY";
            
            LoadPortfolio();
        }

        /// <summary>
        /// Portföy grid satır stili ayarlama
        /// </summary>
        private void grdwPortfoy_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "ProfitLoss" || e.Column.FieldName == "ProfitLossPercent")
            {
                var value = grdwPortfoy.GetRowCellValue(e.RowHandle, "ProfitLoss");
                if (value != null && value is decimal profitLoss)
                {
                    if (profitLoss > 0)
                    {
                        e.Appearance.BackColor = Color.FromArgb(40, 76, 175, 80); // Yeşil
                        e.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
                    }
                    else if (profitLoss < 0)
                    {
                        e.Appearance.BackColor = Color.FromArgb(40, 244, 67, 54); // Kırmızı
                        e.Appearance.ForeColor = Color.FromArgb(244, 67, 54);
                    }
                }
            }
        }

        /// <summary>
        /// Hisse grid satır stili ayarlama
        /// </summary>
        private void grdwHisseler_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "ChangePercent")
            {
                var value = grdwHisseler.GetRowCellValue(e.RowHandle, "ChangePercent");
                if (value != null && value is decimal change)
                {
                    if (change > 0)
                    {
                        e.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
                    }
                    else if (change < 0)
                    {
                        e.Appearance.ForeColor = Color.FromArgb(244, 67, 54);
                    }
                }
            }
        }

        /// <summary>
        /// Form kapanırken kaynakları temizler
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _marketSimulator.Stop();
            _marketSimulator.Dispose();
            base.OnFormClosing(e);
        }
    }
}
