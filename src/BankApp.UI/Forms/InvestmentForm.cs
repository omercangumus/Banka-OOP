#nullable enable
using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraLayout;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Yatırım Portföyü Formu - Robinhood Tarzı Modern Tasarım
    /// </summary>
    public partial class InvestmentForm : XtraForm
    {
        private MarketSimulatorService _marketSimulator;
        private InvestmentService _investmentService;
        private int _customerId = 1; // Demo için
        private string _selectedCurrency = "TRY";
        private decimal _usdRate = 32.50m;
        private decimal _eurRate = 35.20m;

        public InvestmentForm()
        {
            InitializeComponent();
            InitializeServices();
            LoadStocks();
            LoadPortfolio();
        }

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
                gridViewStocks?.RefreshData();
                gridViewPortfolio?.RefreshData();
            }
            catch { }
        }

        private void LoadStocks()
        {
            gridStocks.DataSource = _marketSimulator.GetAllStocks();
            gridViewStocks.RefreshData();
        }

        private void LoadPortfolio()
        {
            var portfolio = _investmentService.GetPortfolio(_customerId);
            gridPortfolio.DataSource = portfolio;
            gridViewPortfolio.RefreshData();
            
            // Toplam portföy değerini hesapla
            decimal total = 0;
            foreach (var item in portfolio)
            {
                total += item.CurrentValue;
            }
            
            decimal displayValue = ConvertCurrency(total);
            lblPortfolioValue.Text = $"{GetCurrencySymbol()} {displayValue:N2}";
        }

        private decimal ConvertCurrency(decimal tlValue)
        {
            return _selectedCurrency switch
            {
                "USD" => tlValue / _usdRate,
                "EUR" => tlValue / _eurRate,
                _ => tlValue
            };
        }

        private string GetCurrencySymbol()
        {
            return _selectedCurrency switch
            {
                "USD" => "$",
                "EUR" => "€",
                _ => "₺"
            };
        }

        private void btnBuyStock_Click(object sender, EventArgs e)
        {
            // Seçili hisseyi al
            var rowHandle = gridViewStocks.FocusedRowHandle;
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

        private void btnSellStock_Click(object sender, EventArgs e)
        {
            var rowHandle = gridViewPortfolio.FocusedRowHandle;
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

        private void toggleCurrency_Toggled(object sender, EventArgs e)
        {
            // Para birimi değiştir
            int index = Array.IndexOf(new[] { "TRY", "USD", "EUR" }, _selectedCurrency);
            index = (index + 1) % 3;
            _selectedCurrency = new[] { "TRY", "USD", "EUR" }[index];
            
            toggleCurrency.Text = _selectedCurrency;
            LoadPortfolio();
        }

        private void gridViewPortfolio_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "ProfitLoss" || e.Column.FieldName == "ProfitLossPercent")
            {
                var value = gridViewPortfolio.GetRowCellValue(e.RowHandle, "ProfitLoss");
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

        private void gridViewStocks_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "ChangePercent")
            {
                var value = gridViewStocks.GetRowCellValue(e.RowHandle, "ChangePercent");
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _marketSimulator.Stop();
            _marketSimulator.Dispose();
            base.OnFormClosing(e);
        }
    }
}
