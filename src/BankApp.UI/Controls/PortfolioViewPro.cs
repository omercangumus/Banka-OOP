using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraCharts;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTab;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Events;
using BankApp.Core.Entities;
using Dapper;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// PROFESYONEL PORTFÖY SAYFASI
    /// - Pozisyonlar (hisseler, artış/azalış, PnL)
    /// - Bekleyen Emirler + İptal butonu
    /// - Varlık Dağılımı Chart
    /// </summary>
    public class PortfolioViewPro : XtraUserControl
    {
        private DapperContext _context;
        private PendingOrderRepository _pendingOrderRepository;
        private CustomerPortfolioRepository _portfolioRepository;
        private TransactionService _transactionService;
        
        // UI Components
        private SplitContainerControl splitMain;
        private PanelControl pnlSummary;
        private XtraTabControl tabPortfolio;
        private XtraTabPage tabPositions;
        private XtraTabPage tabPendingOrders;
        
        // Grids
        private GridControl gridPositions;
        private GridView viewPositions;
        private GridControl gridPendingOrders;
        private GridView viewPendingOrders;
        
        // Chart
        private ChartControl chartAllocation;
        
        // Summary Labels
        private LabelControl lblTotalValue;
        private LabelControl lblTotalPnL;
        private LabelControl lblDailyChange;
        private LabelControl lblPendingCount;
        
        public PortfolioViewPro()
        {
            System.Diagnostics.Debug.WriteLine($"[OPENED] {GetType().FullName} | Hash={GetHashCode()}");
            
            _context = new DapperContext();
            _pendingOrderRepository = new PendingOrderRepository(_context);
            _portfolioRepository = new CustomerPortfolioRepository(_context);
            
            var accountRepo = new AccountRepository(_context);
            var transactionRepo = new TransactionRepository(_context);
            var auditRepo = new AuditRepository(_context);
            _transactionService = new TransactionService(accountRepo, transactionRepo, auditRepo);
            
            InitializeUI();
            _ = LoadDataAsync();
            
            // Event subscriptions
            PortfolioEvents.PortfolioChanged += async (s, e) => await LoadDataAsync();
            AppEvents.TradeCompleted += async (s, e) => await LoadDataAsync();
        }
        
        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.Dock = DockStyle.Fill;
            
            // Main Layout
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(18, 18, 18)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            // Summary Panel
            CreateSummaryPanel();
            mainLayout.Controls.Add(pnlSummary, 0, 0);
            
            // Split: Left = Positions/Orders, Right = Chart
            splitMain = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = 600,
                BackColor = Color.FromArgb(18, 18, 18)
            };
            splitMain.Panel1.BackColor = Color.FromArgb(25, 25, 25);
            splitMain.Panel2.BackColor = Color.FromArgb(25, 25, 25);
            
            // Left: Tab Control (Positions + Pending Orders)
            CreateTabControl();
            splitMain.Panel1.Controls.Add(tabPortfolio);
            
            // Right: Allocation Chart
            CreateAllocationChart();
            splitMain.Panel2.Controls.Add(chartAllocation);
            
            mainLayout.Controls.Add(splitMain, 0, 1);
            this.Controls.Add(mainLayout);
        }
        
        private void CreateSummaryPanel()
        {
            pnlSummary = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(30, 30, 30) },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(20, 15, 20, 15),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            
            // Card 1: Total Portfolio Value
            var card1 = CreateSummaryCard("💰 TOPLAM DEĞER", "₺0.00", Color.FromArgb(33, 150, 243));
            lblTotalValue = card1.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card1, 0, 0);
            
            // Card 2: Total PnL
            var card2 = CreateSummaryCard("📈 KAR/ZARAR", "₺0.00", Color.FromArgb(76, 175, 80));
            lblTotalPnL = card2.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card2, 1, 0);
            
            // Card 3: Daily Change
            var card3 = CreateSummaryCard("⚡ GÜNLÜK DEĞİŞİM", "0%", Color.FromArgb(255, 152, 0));
            lblDailyChange = card3.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card3, 2, 0);
            
            // Card 4: Pending Orders
            var card4 = CreateSummaryCard("⏳ BEKLEYEN EMİR", "0", Color.FromArgb(156, 39, 176));
            lblPendingCount = card4.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card4, 3, 0);
            
            pnlSummary.Controls.Add(layout);
        }
        
        private PanelControl CreateSummaryCard(string title, string value, Color accentColor)
        {
            var card = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(40, 40, 40) },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Margin = new Padding(5)
            };
            
            var lblTitle = new LabelControl
            {
                Text = title,
                Location = new Point(15, 15),
                Appearance = { Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(180, 180, 180) }
            };
            
            var lblValue = new LabelControl
            {
                Text = value,
                Location = new Point(15, 45),
                Appearance = { Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = accentColor }
            };
            
            card.Controls.Add(lblTitle);
            card.Controls.Add(lblValue);
            return card;
        }
        
        private void CreateTabControl()
        {
            tabPortfolio = new XtraTabControl
            {
                Dock = DockStyle.Fill,
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                BorderStylePage = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            tabPortfolio.LookAndFeel.UseDefaultLookAndFeel = false;
            tabPortfolio.Appearance.BackColor = Color.FromArgb(25, 25, 25);
            tabPortfolio.AppearancePage.Header.BackColor = Color.FromArgb(35, 35, 35);
            tabPortfolio.AppearancePage.Header.ForeColor = Color.White;
            tabPortfolio.AppearancePage.HeaderActive.BackColor = Color.FromArgb(50, 50, 50);
            tabPortfolio.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(0, 180, 240);
            
            // Tab 1: Positions
            tabPositions = new XtraTabPage { Text = "📊 POZİSYONLAR" };
            CreatePositionsGrid();
            tabPositions.Controls.Add(gridPositions);
            tabPortfolio.TabPages.Add(tabPositions);
            
            // Tab 2: Pending Orders
            tabPendingOrders = new XtraTabPage { Text = "⏳ BEKLEYEN EMİRLER" };
            CreatePendingOrdersGrid();
            tabPendingOrders.Controls.Add(gridPendingOrders);
            tabPortfolio.TabPages.Add(tabPendingOrders);
        }
        
        private void CreatePositionsGrid()
        {
            gridPositions = new GridControl { Dock = DockStyle.Fill };
            viewPositions = new GridView(gridPositions);
            gridPositions.MainView = viewPositions;
            
            // Columns
            viewPositions.Columns.Add(new GridColumn { FieldName = "Symbol", Caption = "Sembol", VisibleIndex = 0, Width = 80 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "Quantity", Caption = "Miktar", VisibleIndex = 1, Width = 80 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "AvgCost", Caption = "Ort. Maliyet", VisibleIndex = 2, Width = 100 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "CurrentPrice", Caption = "Güncel Fiyat", VisibleIndex = 3, Width = 100 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "MarketValue", Caption = "Piyasa Değeri", VisibleIndex = 4, Width = 120 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "PnL", Caption = "Kar/Zarar", VisibleIndex = 5, Width = 100 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "PnLPercent", Caption = "Değişim %", VisibleIndex = 6, Width = 80 });
            
            // Appearance
            viewPositions.OptionsView.ShowGroupPanel = false;
            viewPositions.OptionsView.RowAutoHeight = true;
            viewPositions.Appearance.Row.BackColor = Color.FromArgb(30, 30, 30);
            viewPositions.Appearance.Row.ForeColor = Color.White;
            viewPositions.Appearance.HeaderPanel.BackColor = Color.FromArgb(40, 40, 40);
            viewPositions.Appearance.HeaderPanel.ForeColor = Color.White;
            viewPositions.Appearance.Empty.BackColor = Color.FromArgb(25, 25, 25);
            
            // Row style - PnL coloring
            viewPositions.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "PnL" || e.Column.FieldName == "PnLPercent")
                {
                    var value = viewPositions.GetRowCellValue(e.RowHandle, "PnL");
                    if (value != null && decimal.TryParse(value.ToString(), out decimal pnl))
                    {
                        e.Appearance.ForeColor = pnl >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                    }
                }
            };
            
            // SAT BUTONU
            var btnSell = new RepositoryItemButtonEdit();
            btnSell.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            btnSell.Buttons[0].Caption = "SAT";
            btnSell.Buttons[0].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph;
            btnSell.ButtonClick += async (s, e) =>
            {
                var rowHandle = viewPositions.FocusedRowHandle;
                if (rowHandle >= 0)
                {
                    var symbol = viewPositions.GetRowCellValue(rowHandle, "Symbol")?.ToString();
                    var qty = viewPositions.GetRowCellValue(rowHandle, "Quantity");
                    if (!string.IsNullOrEmpty(symbol) && qty != null)
                    {
                        await ShowSellDialogAsync(symbol, Convert.ToDecimal(qty));
                    }
                }
            };
            
            var colSell = new GridColumn
            {
                FieldName = "SellAction",
                Caption = "💰 SAT",
                VisibleIndex = 7,
                Width = 70,
                UnboundType = DevExpress.Data.UnboundColumnType.String
            };
            colSell.ColumnEdit = btnSell;
            colSell.AppearanceHeader.ForeColor = Color.FromArgb(239, 68, 68);
            colSell.AppearanceHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            viewPositions.Columns.Add(colSell);
            gridPositions.RepositoryItems.Add(btnSell);
        }
        
        private async Task ShowSellDialogAsync(string symbol, decimal maxQuantity)
        {
            using (var form = new Form())
            {
                form.Text = $"{symbol} SAT";
                form.Size = new Size(350, 200);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.BackColor = Color.FromArgb(30, 30, 30);
                
                var lblSymbol = new Label { Text = $"Sembol: {symbol}", Location = new Point(20, 20), ForeColor = Color.White, AutoSize = true };
                var lblMax = new Label { Text = $"Mevcut: {maxQuantity}", Location = new Point(20, 45), ForeColor = Color.Gray, AutoSize = true };
                var lblQty = new Label { Text = "Satılacak Miktar:", Location = new Point(20, 75), ForeColor = Color.White, AutoSize = true };
                
                var txtQty = new TextBox { Location = new Point(20, 95), Size = new Size(150, 25), Text = maxQuantity.ToString() };
                
                var btnConfirm = new Button { Text = "SAT", Location = new Point(20, 130), Size = new Size(100, 35), BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                var btnCancel = new Button { Text = "İptal", Location = new Point(130, 130), Size = new Size(80, 35), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                
                btnCancel.Click += (s, e) => form.DialogResult = DialogResult.Cancel;
                btnConfirm.Click += async (s, e) =>
                {
                    if (decimal.TryParse(txtQty.Text, out decimal sellQty) && sellQty > 0 && sellQty <= maxQuantity)
                    {
                        await ExecuteSellAsync(symbol, sellQty);
                        form.DialogResult = DialogResult.OK;
                    }
                    else
                    {
                        MessageBox.Show($"Geçerli miktar girin (max: {maxQuantity})", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };
                
                form.Controls.AddRange(new Control[] { lblSymbol, lblMax, lblQty, txtQty, btnConfirm, btnCancel });
                form.ShowDialog();
            }
        }
        
        private async Task ExecuteSellAsync(string symbol, decimal quantity)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[TRADE] PortfolioViewPro.ExecuteSell symbol={symbol} qty={quantity}");
                
                int customerId = AppEvents.CurrentSession.CustomerId;
                if (customerId == 0)
                {
                    using var conn = _context.CreateConnection();
                    customerId = await conn.QueryFirstOrDefaultAsync<int>(
                        "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                        new { UserId = AppEvents.CurrentSession.UserId });
                }
                
                // Pozisyonu sat
                var sold = await _portfolioRepository.SellAsync(customerId, symbol, quantity);
                if (!sold)
                {
                    XtraMessageBox.Show("Satış başarısız - yetersiz pozisyon.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Hesaba para ekle (simulated price)
                var accounts = await new AccountRepository(_context).GetByCustomerIdAsync(customerId);
                var primaryAccount = accounts?.FirstOrDefault();
                if (primaryAccount != null)
                {
                    // Get position avg cost for calculation
                    var position = await _portfolioRepository.GetBySymbolAsync(customerId, symbol);
                    decimal price = position?.AverageCost ?? 100m;
                    decimal totalAmount = quantity * price;
                    
                    await _transactionService.DepositAsync(
                        primaryAccount.Id,
                        totalAmount,
                        $"Portföyden SAT: {quantity} adet {symbol}");
                    
                    System.Diagnostics.Debug.WriteLine($"[TRADE] Sell SUCCESS symbol={symbol} qty={quantity} amount={totalAmount}");
                }
                
                XtraMessageBox.Show($"✅ {symbol} satıldı!\n\nMiktar: {quantity}", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Refresh
                await LoadDataAsync();
                PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "Sell");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERR] ExecuteSell error: {ex.Message}");
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void CreatePendingOrdersGrid()
        {
            gridPendingOrders = new GridControl { Dock = DockStyle.Fill };
            viewPendingOrders = new GridView(gridPendingOrders);
            gridPendingOrders.MainView = viewPendingOrders;
            
            // Columns
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "Id", Caption = "Emir No", VisibleIndex = 0, Width = 70 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "Symbol", Caption = "Sembol", VisibleIndex = 1, Width = 80 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "Side", Caption = "Yön", VisibleIndex = 2, Width = 60 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "OrderType", Caption = "Tip", VisibleIndex = 3, Width = 80 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "Quantity", Caption = "Miktar", VisibleIndex = 4, Width = 80 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "LimitPrice", Caption = "Limit Fiyat", VisibleIndex = 5, Width = 100 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "CreatedAt", Caption = "Tarih", VisibleIndex = 6, Width = 120 });
            viewPendingOrders.Columns.Add(new GridColumn { FieldName = "Status", Caption = "Durum", VisibleIndex = 7, Width = 80 });
            
            // Appearance
            viewPendingOrders.OptionsView.ShowGroupPanel = false;
            viewPendingOrders.Appearance.Row.BackColor = Color.FromArgb(30, 30, 30);
            viewPendingOrders.Appearance.Row.ForeColor = Color.White;
            viewPendingOrders.Appearance.HeaderPanel.BackColor = Color.FromArgb(40, 40, 40);
            viewPendingOrders.Appearance.HeaderPanel.ForeColor = Color.White;
            viewPendingOrders.Appearance.Empty.BackColor = Color.FromArgb(25, 25, 25);
            
            // İPTAL BUTONU - Repository Pattern
            var btnCancel = new RepositoryItemButtonEdit();
            btnCancel.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.HideTextEditor;
            btnCancel.Buttons[0].Caption = "❌ İPTAL";
            btnCancel.Buttons[0].Kind = DevExpress.XtraEditors.Controls.ButtonPredefines.Glyph;
            btnCancel.ButtonClick += async (s, e) =>
            {
                var row = viewPendingOrders.GetFocusedRow() as PendingOrder;
                if (row != null)
                {
                    await CancelOrderAsync(row);
                }
            };
            
            // Add cancel button column
            var colCancel = new GridColumn
            {
                FieldName = "Cancel",
                Caption = "İşlem",
                VisibleIndex = 8,
                Width = 80,
                UnboundType = DevExpress.Data.UnboundColumnType.String
            };
            colCancel.ColumnEdit = btnCancel;
            viewPendingOrders.Columns.Add(colCancel);
            gridPendingOrders.RepositoryItems.Add(btnCancel);
            
            // Double-click to cancel
            viewPendingOrders.DoubleClick += async (s, e) =>
            {
                var row = viewPendingOrders.GetFocusedRow() as PendingOrder;
                if (row != null && row.Status == "Pending")
                {
                    var result = XtraMessageBox.Show(
                        $"#{row.Id} numaralı emri iptal etmek istiyor musunuz?\n\nSembol: {row.Symbol}\nMiktar: {row.Quantity}\nFiyat: ${row.LimitPrice:N2}",
                        "Emir İptali",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        await CancelOrderAsync(row);
                    }
                }
            };
        }
        
        private async Task CancelOrderAsync(PendingOrder order)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] CancelOrder START orderId={order.Id}");
                
                // İptal et
                var cancelled = await _pendingOrderRepository.CancelAsync(order.Id, "Kullanıcı tarafından iptal edildi");
                
                if (cancelled)
                {
                    // AL emri iptal edildi = Para geri verilmez (zaten çekilmemişti)
                    // SAT emri iptal edildi = Pozisyon geri verilmez (zaten satılmamıştı)
                    
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] CancelOrder SUCCESS orderId={order.Id}");
                    
                    XtraMessageBox.Show(
                        $"✅ Emir iptal edildi!\n\nEmir No: #{order.Id}\nSembol: {order.Symbol}\n\n📌 Limit emirlerde para önceden çekilmediği için iade yapılmaz.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    // Refresh
                    await LoadDataAsync();
                    PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "OrderCancelled");
                }
                else
                {
                    XtraMessageBox.Show("Emir iptal edilemedi.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERR] CancelOrder error: {ex.Message}");
                XtraMessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void CreateAllocationChart()
        {
            chartAllocation = new ChartControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(25, 25, 25)
            };
            chartAllocation.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartAllocation.Legend.BackColor = Color.FromArgb(25, 25, 25);
            chartAllocation.Legend.TextColor = Color.White;
            
            var title = new ChartTitle { Text = "Varlık Dağılımı", TextColor = Color.White };
            chartAllocation.Titles.Add(title);
        }
        
        public async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DATA] PortfolioViewPro.LoadDataAsync START");
                
                int customerId = AppEvents.CurrentSession.CustomerId;
                if (customerId == 0)
                {
                    // Get from DB
                    using var conn = _context.CreateConnection();
                    customerId = await conn.QueryFirstOrDefaultAsync<int>(
                        "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                        new { UserId = AppEvents.CurrentSession.UserId });
                }
                
                // Load Positions
                var positions = await _portfolioRepository.GetByCustomerIdAsync(customerId);
                var positionList = positions.Select(p => new
                {
                    p.StockSymbol,
                    Symbol = p.StockSymbol,
                    p.Quantity,
                    AvgCost = $"₺{p.AverageCost:N2}",
                    CurrentPrice = $"₺{p.AverageCost * 1.05m:N2}", // Simulated current price
                    MarketValue = $"₺{p.Quantity * p.AverageCost * 1.05m:N2}",
                    PnL = p.Quantity * p.AverageCost * 0.05m,
                    PnLPercent = "5.0%"
                }).ToList();
                
                gridPositions.DataSource = positionList;
                
                // Load Pending Orders
                var pendingOrders = await _pendingOrderRepository.GetPendingByCustomerIdAsync(customerId);
                gridPendingOrders.DataSource = pendingOrders.ToList();
                
                // Update Summary
                decimal totalValue = positionList.Sum(p => p.Quantity * decimal.Parse(p.AvgCost.Replace("₺", "").Replace(",", "")));
                decimal totalPnL = positionList.Sum(p => p.PnL);
                int pendingCount = pendingOrders.Count();
                
                lblTotalValue.Text = $"₺{totalValue:N2}";
                lblTotalPnL.Text = $"{(totalPnL >= 0 ? "+" : "")}₺{totalPnL:N2}";
                lblTotalPnL.Appearance.ForeColor = totalPnL >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                lblPendingCount.Text = pendingCount.ToString();
                
                // Update Chart
                UpdateAllocationChart(positionList);
                
                System.Diagnostics.Debug.WriteLine($"[DATA] PortfolioViewPro.LoadDataAsync END positions={positionList.Count} pending={pendingCount}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERR] PortfolioViewPro.LoadDataAsync error: {ex.Message}");
            }
        }
        
        private void UpdateAllocationChart(dynamic positions)
        {
            chartAllocation.Series.Clear();
            
            var series = new Series("Varlıklar", ViewType.Doughnut);
            
            foreach (var pos in positions)
            {
                decimal qty = pos.Quantity;
                decimal price = decimal.Parse(pos.AvgCost.Replace("₺", "").Replace(",", ""));
                series.Points.Add(new SeriesPoint(pos.Symbol, (double)(qty * price)));
            }
            
            if (series.Points.Count == 0)
            {
                series.Points.Add(new SeriesPoint("Varlık Yok", 1));
            }
            
            series.Label.TextPattern = "{A}: {VP:P0}";
            chartAllocation.Series.Add(series);
            chartAllocation.PaletteName = "Pastel";
        }
    }
}
