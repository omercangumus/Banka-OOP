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
using DevExpress.XtraLayout;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Events;
using BankApp.Core.Entities;
using Dapper;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// PROFESYONEL PORTFÖY SAYFASI V2
    /// Layout: 3 bölge
    /// A) Üst: 6 Özet Kart (Toplam Portföy, Nakit, Yatırım Değeri, Günlük P/L, Toplam P/L, Bekleyen Emir)
    /// B) Sol: Sekmeli Grid (Pozisyonlar + Bekleyen Emirler)
    /// C) Sağ: Satış Paneli (Market/Limit/Stop-Limit)
    /// </summary>
    public class PortfolioViewPro : XtraUserControl
    {
        private DapperContext _context;
        private PendingOrderRepository _pendingOrderRepository;
        private CustomerPortfolioRepository _portfolioRepository;
        private AccountRepository _accountRepository;
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
        
        // Sağ Panel: Satış Ticket
        private PanelControl pnlSellTicket;
        private LabelControl lblSelectedSymbol;
        private LabelControl lblAvailableQty;
        private LabelControl lblCurrentPrice;
        private SpinEdit spinQuantity;
        private RadioGroup rgOrderType;
        private SpinEdit spinLimitPrice;
        private SpinEdit spinStopPrice;
        private SimpleButton btnSubmitSell;
        private LabelControl lblValidation;
        
        // Summary Labels (6 kart)
        private LabelControl lblTotalPortfolio;
        private LabelControl lblCash;
        private LabelControl lblInvestedValue;
        private LabelControl lblDailyPnL;
        private LabelControl lblTotalPnL;
        private LabelControl lblPendingCount;
        
        // Seçili pozisyon
        private string _selectedSymbol = "";
        private decimal _selectedAvailableQty = 0;
        private decimal _selectedCurrentPrice = 0;
        
        public PortfolioViewPro()
        {
            System.Diagnostics.Debug.WriteLine($"[OPENED] PortfolioViewPro | Instance={GetHashCode()} | Time={DateTime.Now:HH:mm:ss}");
            
            _context = new DapperContext();
            _pendingOrderRepository = new PendingOrderRepository(_context);
            _portfolioRepository = new CustomerPortfolioRepository(_context);
            _accountRepository = new AccountRepository(_context);
            
            var transactionRepo = new TransactionRepository(_context);
            var auditRepo = new AuditRepository(_context);
            _transactionService = new TransactionService(_accountRepository, transactionRepo, auditRepo);
            
            InitializeUI();
            _ = LoadDataAsync();
            
            // Event subscriptions
            PortfolioEvents.PortfolioChanged += async (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"[EVENT] PortfolioViewPro received PortfolioChanged");
                await LoadDataAsync();
            };
            AppEvents.TradeCompleted += async (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"[EVENT] PortfolioViewPro received TradeCompleted");
                await LoadDataAsync();
            };
        }
        
        private void InitializeUI()
        {
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.Dock = DockStyle.Fill;
            
            // Main Layout: 2 satır (üst özet + alt içerik)
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.FromArgb(18, 18, 18)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            // A) Üst: 6 Özet Kart
            CreateSummaryPanel();
            mainLayout.Controls.Add(pnlSummary, 0, 0);
            
            // B+C) Alt: Sol Grid + Sağ Satış Paneli
            splitMain = new SplitContainerControl
            {
                Dock = DockStyle.Fill,
                Horizontal = true,
                SplitterPosition = (int)(this.Width * 0.65),
                FixedPanel = DevExpress.XtraEditors.SplitFixedPanel.Panel2,
                BackColor = Color.FromArgb(18, 18, 18)
            };
            splitMain.Panel1.BackColor = Color.FromArgb(25, 25, 25);
            splitMain.Panel2.BackColor = Color.FromArgb(30, 30, 30);
            splitMain.SplitterPosition = 320; // Sağ panel 320px
            
            // B) Sol: Tab Control (Pozisyonlar + Bekleyen Emirler)
            CreateTabControl();
            splitMain.Panel1.Controls.Add(tabPortfolio);
            
            // C) Sağ: Satış Ticket Paneli
            CreateSellTicketPanel();
            splitMain.Panel2.Controls.Add(pnlSellTicket);
            
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
                ColumnCount = 6,
                RowCount = 1,
                Padding = new Padding(10, 8, 10, 8),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            for (int i = 0; i < 6; i++)
                layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.67f));
            
            // Card 1: Toplam Portföy (Cash + Invested)
            var card1 = CreateSummaryCard("💼 TOPLAM PORTFÖY", "₺0,00", Color.FromArgb(33, 150, 243));
            lblTotalPortfolio = card1.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card1, 0, 0);
            
            // Card 2: Nakit (TRY)
            var card2 = CreateSummaryCard("💵 NAKİT", "₺0,00", Color.FromArgb(0, 200, 83));
            lblCash = card2.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card2, 1, 0);
            
            // Card 3: Yatırım Değeri
            var card3 = CreateSummaryCard("📊 YATIRIM DEĞERİ", "₺0,00", Color.FromArgb(255, 193, 7));
            lblInvestedValue = card3.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card3, 2, 0);
            
            // Card 4: Günlük P/L
            var card4 = CreateSummaryCard("📈 GÜNLÜK K/Z", "₺0,00", Color.FromArgb(255, 152, 0));
            lblDailyPnL = card4.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card4, 3, 0);
            
            // Card 5: Toplam P/L
            var card5 = CreateSummaryCard("💰 TOPLAM K/Z", "₺0,00", Color.FromArgb(76, 175, 80));
            lblTotalPnL = card5.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card5, 4, 0);
            
            // Card 6: Bekleyen Emir
            var card6 = CreateSummaryCard("⏳ BEKLEYEN EMİR", "0", Color.FromArgb(156, 39, 176));
            lblPendingCount = card6.Controls.OfType<LabelControl>().Last();
            layout.Controls.Add(card6, 5, 0);
            
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
            
            // Columns - Profesyonel grid
            viewPositions.Columns.Add(new GridColumn { FieldName = "Symbol", Caption = "Sembol", VisibleIndex = 0, Width = 80 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "Quantity", Caption = "Adet", VisibleIndex = 1, Width = 70 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "AvailableQty", Caption = "Kullanılabilir", VisibleIndex = 2, Width = 85 });
            viewPositions.Columns.Add(new GridColumn { FieldName = "AvgCostRaw", Caption = "Ort. Maliyet", VisibleIndex = 3, Width = 100, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺#,##0.00" } });
            viewPositions.Columns.Add(new GridColumn { FieldName = "CurrentPriceRaw", Caption = "Güncel Fiyat", VisibleIndex = 4, Width = 100, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺#,##0.00" } });
            viewPositions.Columns.Add(new GridColumn { FieldName = "MarketValueRaw", Caption = "Piyasa Değeri", VisibleIndex = 5, Width = 110, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "₺#,##0.00" } });
            viewPositions.Columns.Add(new GridColumn { FieldName = "PnL", Caption = "K/Z (₺)", VisibleIndex = 6, Width = 90, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "+₺#,##0.00;-₺#,##0.00" } });
            viewPositions.Columns.Add(new GridColumn { FieldName = "PnLPercentRaw", Caption = "K/Z (%)", VisibleIndex = 7, Width = 70, DisplayFormat = { FormatType = DevExpress.Utils.FormatType.Numeric, FormatString = "+0.00%;-0.00%" } });
            
            // Appearance - Pro style
            viewPositions.OptionsView.ShowGroupPanel = true;
            viewPositions.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.ShowAlways;
            viewPositions.OptionsView.RowAutoHeight = false;
            viewPositions.RowHeight = 32;
            viewPositions.Appearance.Row.BackColor = Color.FromArgb(30, 30, 30);
            viewPositions.Appearance.Row.ForeColor = Color.White;
            viewPositions.Appearance.Row.Font = new Font("Segoe UI", 10F);
            viewPositions.Appearance.HeaderPanel.BackColor = Color.FromArgb(45, 45, 45);
            viewPositions.Appearance.HeaderPanel.ForeColor = Color.White;
            viewPositions.Appearance.HeaderPanel.Font = new Font("Segoe UI Semibold", 9F);
            viewPositions.Appearance.Empty.BackColor = Color.FromArgb(25, 25, 25);
            viewPositions.Appearance.GroupPanel.BackColor = Color.FromArgb(35, 35, 35);
            viewPositions.Appearance.GroupPanel.ForeColor = Color.White;
            viewPositions.Appearance.FocusedRow.BackColor = Color.FromArgb(50, 70, 100);
            viewPositions.Appearance.FocusedRow.ForeColor = Color.White;
            viewPositions.Appearance.SelectedRow.BackColor = Color.FromArgb(40, 60, 90);
            viewPositions.Appearance.SelectedRow.ForeColor = Color.White;
            
            // Row style - PnL conditional formatting (yeşil/kırmızı)
            viewPositions.RowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "PnL" || e.Column.FieldName == "PnLPercentRaw")
                {
                    var value = viewPositions.GetRowCellValue(e.RowHandle, "PnL");
                    if (value != null && decimal.TryParse(value.ToString(), out decimal pnl))
                    {
                        e.Appearance.ForeColor = pnl >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                        e.Appearance.Font = new Font("Segoe UI Semibold", 10F);
                    }
                }
            };
            
            // Satır seçildiğinde sağ paneli güncelle
            viewPositions.FocusedRowChanged += (s, e) =>
            {
                if (e.FocusedRowHandle >= 0)
                {
                    var symbol = viewPositions.GetRowCellValue(e.FocusedRowHandle, "Symbol")?.ToString() ?? "";
                    var qty = viewPositions.GetRowCellValue(e.FocusedRowHandle, "AvailableQty");
                    var price = viewPositions.GetRowCellValue(e.FocusedRowHandle, "CurrentPriceRaw");
                    
                    decimal availableQty = 0;
                    decimal currentPrice = 0;
                    if (qty != null) decimal.TryParse(qty.ToString(), out availableQty);
                    if (price != null) decimal.TryParse(price.ToString(), out currentPrice);
                    
                    UpdateSellTicketPanel(symbol, availableQty, currentPrice);
                }
                else
                {
                    ClearSellTicketPanel();
                }
            };
        }
        
        private void CreateSellTicketPanel()
        {
            pnlSellTicket = new PanelControl
            {
                Dock = DockStyle.Fill,
                Appearance = { BackColor = Color.FromArgb(35, 35, 35) },
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Padding = new Padding(15)
            };
            
            int y = 15;
            
            // Başlık
            var lblTitle = new LabelControl
            {
                Text = "📉 SATIŞ EMRİ",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI Semibold", 14F), ForeColor = Color.FromArgb(239, 68, 68) }
            };
            pnlSellTicket.Controls.Add(lblTitle);
            y += 40;
            
            // Seçili Sembol
            var lblSymbolTitle = new LabelControl
            {
                Text = "Sembol:",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray }
            };
            pnlSellTicket.Controls.Add(lblSymbolTitle);
            y += 20;
            
            lblSelectedSymbol = new LabelControl
            {
                Text = "- Pozisyon seçin -",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI Semibold", 12F), ForeColor = Color.White }
            };
            pnlSellTicket.Controls.Add(lblSelectedSymbol);
            y += 35;
            
            // Kullanılabilir Adet
            var lblAvailTitle = new LabelControl
            {
                Text = "Kullanılabilir Adet:",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray }
            };
            pnlSellTicket.Controls.Add(lblAvailTitle);
            y += 20;
            
            lblAvailableQty = new LabelControl
            {
                Text = "0",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.FromArgb(0, 200, 83) }
            };
            pnlSellTicket.Controls.Add(lblAvailableQty);
            y += 35;
            
            // Güncel Fiyat
            var lblPriceTitle = new LabelControl
            {
                Text = "Güncel Fiyat:",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.Gray }
            };
            pnlSellTicket.Controls.Add(lblPriceTitle);
            y += 20;
            
            lblCurrentPrice = new LabelControl
            {
                Text = "₺0,00",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI Semibold", 11F), ForeColor = Color.FromArgb(33, 150, 243) }
            };
            pnlSellTicket.Controls.Add(lblCurrentPrice);
            y += 40;
            
            // Separator
            var separator = new PanelControl
            {
                Location = new Point(15, y),
                Size = new Size(280, 1),
                Appearance = { BackColor = Color.FromArgb(60, 60, 60) }
            };
            pnlSellTicket.Controls.Add(separator);
            y += 15;
            
            // Miktar
            var lblQtyTitle = new LabelControl
            {
                Text = "Satılacak Miktar:",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.White }
            };
            pnlSellTicket.Controls.Add(lblQtyTitle);
            y += 22;
            
            spinQuantity = new SpinEdit
            {
                Location = new Point(15, y),
                Size = new Size(280, 28),
                Properties = { MinValue = 0, MaxValue = 999999, IsFloatValue = true, Increment = 1 }
            };
            spinQuantity.Properties.Appearance.BackColor = Color.FromArgb(50, 50, 50);
            spinQuantity.Properties.Appearance.ForeColor = Color.White;
            spinQuantity.Properties.AppearanceFocused.BackColor = Color.FromArgb(60, 60, 60);
            pnlSellTicket.Controls.Add(spinQuantity);
            y += 40;
            
            // Emir Tipi
            var lblOrderType = new LabelControl
            {
                Text = "Emir Tipi:",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.White }
            };
            pnlSellTicket.Controls.Add(lblOrderType);
            y += 22;
            
            rgOrderType = new RadioGroup
            {
                Location = new Point(15, y),
                Size = new Size(280, 80),
                Properties = { Columns = 1 }
            };
            rgOrderType.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(0, "Market (Anında)"));
            rgOrderType.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(1, "Limit (Fiyat Gelince)"));
            rgOrderType.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(2, "Stop-Limit"));
            rgOrderType.SelectedIndex = 0;
            rgOrderType.Properties.Appearance.BackColor = Color.FromArgb(35, 35, 35);
            rgOrderType.Properties.Appearance.ForeColor = Color.White;
            rgOrderType.SelectedIndexChanged += (s, e) => UpdateOrderTypeFields();
            pnlSellTicket.Controls.Add(rgOrderType);
            y += 90;
            
            // Limit Fiyat
            var lblLimitPrice = new LabelControl
            {
                Text = "Limit Fiyat (₺):",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.White },
                Tag = "limit_label"
            };
            pnlSellTicket.Controls.Add(lblLimitPrice);
            y += 22;
            
            spinLimitPrice = new SpinEdit
            {
                Location = new Point(15, y),
                Size = new Size(280, 28),
                Properties = { MinValue = 0, MaxValue = 9999999, IsFloatValue = true, Increment = 0.01m },
                Visible = false,
                Tag = "limit_input"
            };
            spinLimitPrice.Properties.Appearance.BackColor = Color.FromArgb(50, 50, 50);
            spinLimitPrice.Properties.Appearance.ForeColor = Color.White;
            pnlSellTicket.Controls.Add(spinLimitPrice);
            lblLimitPrice.Visible = false;
            y += 35;
            
            // Stop Fiyat
            var lblStopPrice = new LabelControl
            {
                Text = "Stop Fiyat (₺):",
                Location = new Point(15, y),
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.White },
                Tag = "stop_label",
                Visible = false
            };
            pnlSellTicket.Controls.Add(lblStopPrice);
            y += 22;
            
            spinStopPrice = new SpinEdit
            {
                Location = new Point(15, y),
                Size = new Size(280, 28),
                Properties = { MinValue = 0, MaxValue = 9999999, IsFloatValue = true, Increment = 0.01m },
                Visible = false,
                Tag = "stop_input"
            };
            spinStopPrice.Properties.Appearance.BackColor = Color.FromArgb(50, 50, 50);
            spinStopPrice.Properties.Appearance.ForeColor = Color.White;
            pnlSellTicket.Controls.Add(spinStopPrice);
            y += 45;
            
            // Validation Label
            lblValidation = new LabelControl
            {
                Text = "",
                Location = new Point(15, y),
                AutoSizeMode = LabelAutoSizeMode.Vertical,
                Appearance = { Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(255, 82, 82) }
            };
            pnlSellTicket.Controls.Add(lblValidation);
            y += 25;
            
            // Submit Button
            btnSubmitSell = new SimpleButton
            {
                Text = "📉 SATIŞ EMRİ GÖNDER",
                Location = new Point(15, y),
                Size = new Size(280, 45),
                Appearance = { BackColor = Color.FromArgb(239, 68, 68), ForeColor = Color.White, Font = new Font("Segoe UI Semibold", 11F) },
                Enabled = false
            };
            btnSubmitSell.Click += async (s, e) => await SubmitSellOrderAsync();
            pnlSellTicket.Controls.Add(btnSubmitSell);
            
            // Initial state
            UpdateOrderTypeFields();
        }
        
        private void UpdateSellTicketPanel(string symbol, decimal availableQty, decimal currentPrice)
        {
            _selectedSymbol = symbol;
            _selectedAvailableQty = availableQty;
            _selectedCurrentPrice = currentPrice;
            
            lblSelectedSymbol.Text = symbol;
            lblAvailableQty.Text = availableQty.ToString("N0");
            lblCurrentPrice.Text = $"₺{currentPrice:N2}";
            spinQuantity.Properties.MaxValue = availableQty;
            spinQuantity.Value = Math.Min(1, availableQty);
            spinLimitPrice.Value = currentPrice;
            spinStopPrice.Value = currentPrice * 0.95m; // %5 altında default stop
            
            btnSubmitSell.Enabled = availableQty > 0;
            lblValidation.Text = "";
            
            System.Diagnostics.Debug.WriteLine($"[UI] SellTicket updated: symbol={symbol} availQty={availableQty} price={currentPrice}");
        }
        
        private void ClearSellTicketPanel()
        {
            _selectedSymbol = "";
            _selectedAvailableQty = 0;
            _selectedCurrentPrice = 0;
            
            lblSelectedSymbol.Text = "- Pozisyon seçin -";
            lblAvailableQty.Text = "0";
            lblCurrentPrice.Text = "₺0,00";
            spinQuantity.Value = 0;
            spinLimitPrice.Value = 0;
            spinStopPrice.Value = 0;
            btnSubmitSell.Enabled = false;
            lblValidation.Text = "";
        }
        
        private void UpdateOrderTypeFields()
        {
            int orderType = rgOrderType.SelectedIndex;
            
            // Limit price visibility
            bool showLimit = orderType == 1 || orderType == 2;
            foreach (Control c in pnlSellTicket.Controls)
            {
                if (c.Tag?.ToString() == "limit_label" || c.Tag?.ToString() == "limit_input")
                    c.Visible = showLimit;
                if (c.Tag?.ToString() == "stop_label" || c.Tag?.ToString() == "stop_input")
                    c.Visible = orderType == 2;
            }
            spinLimitPrice.Visible = showLimit;
            spinStopPrice.Visible = orderType == 2;
        }
        
        private async Task SubmitSellOrderAsync()
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(_selectedSymbol))
                {
                    lblValidation.Text = "❌ Pozisyon seçin";
                    return;
                }
                
                decimal quantity = (decimal)spinQuantity.Value;
                if (quantity <= 0)
                {
                    lblValidation.Text = "❌ Geçerli miktar girin";
                    return;
                }
                
                if (quantity > _selectedAvailableQty)
                {
                    lblValidation.Text = $"❌ Yetersiz adet (max: {_selectedAvailableQty:N0})";
                    return;
                }
                
                int orderType = rgOrderType.SelectedIndex;
                decimal limitPrice = (decimal)spinLimitPrice.Value;
                decimal stopPrice = (decimal)spinStopPrice.Value;
                
                if (orderType == 1 && limitPrice <= 0)
                {
                    lblValidation.Text = "❌ Limit fiyat girin";
                    return;
                }
                
                if (orderType == 2 && (limitPrice <= 0 || stopPrice <= 0))
                {
                    lblValidation.Text = "❌ Stop ve limit fiyat girin";
                    return;
                }
                
                string orderTypeName = orderType == 0 ? "Market" : (orderType == 1 ? "Limit" : "Stop-Limit");
                
                System.Diagnostics.Debug.WriteLine($"[CALL] SellTicket Submit: type={orderTypeName} symbol={_selectedSymbol} qty={quantity} limit={limitPrice} stop={stopPrice}");
                
                if (orderType == 0)
                {
                    // Market Sell - Anında işlem
                    await ExecuteMarketSellAsync(_selectedSymbol, quantity, _selectedCurrentPrice);
                }
                else
                {
                    // Limit veya Stop-Limit - Bekleyen emir oluştur
                    await CreatePendingSellOrderAsync(_selectedSymbol, quantity, limitPrice, stopPrice, orderTypeName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERR] SubmitSellOrder error: {ex.Message}");
                lblValidation.Text = $"❌ Hata: {ex.Message}";
            }
        }
        
        private async Task ExecuteMarketSellAsync(string symbol, decimal quantity, decimal price)
        {
            int customerId = await GetCustomerIdAsync();
            
            // Cash before
            var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
            var primaryAccount = accounts?.FirstOrDefault();
            decimal cashBefore = primaryAccount?.Balance ?? 0;
            
            // Position qty before
            var positionBefore = await _portfolioRepository.GetBySymbolAsync(customerId, symbol);
            decimal qtyBefore = positionBefore?.Quantity ?? 0;
            
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] MarketSell BEFORE: cash={cashBefore:N2} posQty={qtyBefore}");
            
            // Pozisyonu sat
            var sold = await _portfolioRepository.SellAsync(customerId, symbol, quantity);
            if (!sold)
            {
                lblValidation.Text = "❌ Satış başarısız - yetersiz pozisyon";
                System.Diagnostics.Debug.WriteLine($"[WARN] MarketSell FAILED: insufficient position");
                return;
            }
            
            // Hesaba para ekle
            decimal totalAmount = quantity * price;
            if (primaryAccount != null)
            {
                await _transactionService.DepositAsync(
                    primaryAccount.Id,
                    totalAmount,
                    $"Market SAT: {quantity:N0} adet {symbol} @ ₺{price:N2}");
            }
            
            // After values
            var positionAfter = await _portfolioRepository.GetBySymbolAsync(customerId, symbol);
            decimal qtyAfter = positionAfter?.Quantity ?? 0;
            var accountAfter = await _accountRepository.GetByIdAsync(primaryAccount?.Id ?? 0);
            decimal cashAfter = accountAfter?.Balance ?? 0;
            
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] MarketSell AFTER: cash={cashAfter:N2} posQty={qtyAfter}");
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] MarketSell DELTA: cash+={totalAmount:N2} qty-={quantity}");
            
            XtraMessageBox.Show(
                $"✅ Market Satış Gerçekleşti!\n\n" +
                $"Sembol: {symbol}\n" +
                $"Miktar: {quantity:N0} adet\n" +
                $"Fiyat: ₺{price:N2}\n" +
                $"Toplam: ₺{totalAmount:N2}\n\n" +
                $"💵 Nakit: ₺{cashBefore:N2} → ₺{cashAfter:N2}",
                "Satış Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            // Refresh + Events
            await LoadDataAsync();
            System.Diagnostics.Debug.WriteLine($"[TREE] RefreshPipeline: LoadDataAsync → PortfolioEvents → NotifyTradeCompleted");
            PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "MarketSell");
            AppEvents.NotifyTradeCompleted(primaryAccount?.Id ?? 0, customerId, symbol, totalAmount, false);
        }
        
        private async Task CreatePendingSellOrderAsync(string symbol, decimal quantity, decimal limitPrice, decimal stopPrice, string orderType)
        {
            int customerId = await GetCustomerIdAsync();
            var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
            var primaryAccount = accounts?.FirstOrDefault();
            
            // Bekleyen emir oluştur (adet rezervasyonu mantığı: pending sell emirleri kullanılabilir adetten düşülecek)
            var order = new PendingOrder
            {
                CustomerId = customerId,
                AccountId = primaryAccount?.Id ?? 0,
                Symbol = symbol,
                OrderType = orderType,
                Side = "Sell",
                Quantity = quantity,
                LimitPrice = limitPrice,
                StopPrice = orderType == "Stop-Limit" ? stopPrice : (decimal?)null,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            
            var orderId = await _pendingOrderRepository.CreateAsync(order);
            
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] PendingSellOrder CREATED: orderId={orderId} symbol={symbol} qty={quantity} type={orderType}");
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] Adet Rezervasyonu: {symbol} için {quantity} adet bekleyen emirde");
            
            XtraMessageBox.Show(
                $"✅ {orderType} Satış Emri Oluşturuldu!\n\n" +
                $"Emir No: #{orderId}\n" +
                $"Sembol: {symbol}\n" +
                $"Miktar: {quantity:N0} adet\n" +
                $"Limit Fiyat: ₺{limitPrice:N2}\n" +
                (orderType == "Stop-Limit" ? $"Stop Fiyat: ₺{stopPrice:N2}\n" : "") +
                $"\n📌 Not: Fiyat hedefe ulaşınca otomatik satılacak.\n" +
                $"Adet kullanılabilir miktardan rezerve edildi.",
                "Emir Oluşturuldu",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            
            // Refresh
            await LoadDataAsync();
            PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "PendingSellOrder");
        }
        
        private async Task<int> GetCustomerIdAsync()
        {
            int customerId = AppEvents.CurrentSession.CustomerId;
            if (customerId == 0)
            {
                using var conn = _context.CreateConnection();
                customerId = await conn.QueryFirstOrDefaultAsync<int>(
                    "SELECT \"Id\" FROM \"Customers\" WHERE \"UserId\" = @UserId LIMIT 1",
                    new { UserId = AppEvents.CurrentSession.UserId });
            }
            return customerId;
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
        
        public async Task LoadDataAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DATA] PortfolioViewPro.LoadDataAsync START");
                
                int customerId = await GetCustomerIdAsync();
                
                // Load Positions
                var positions = await _portfolioRepository.GetByCustomerIdAsync(customerId);
                var pendingOrders = await _pendingOrderRepository.GetPendingByCustomerIdAsync(customerId);
                var pendingList = pendingOrders.ToList();
                
                // Kullanılabilir adet hesapla (toplam - bekleyen sell emirleri)
                var positionList = positions.Select(p => 
                {
                    decimal currentPrice = p.AverageCost * 1.05m; // Simulated %5 artış
                    decimal marketValue = p.Quantity * currentPrice;
                    decimal pnl = p.Quantity * p.AverageCost * 0.05m;
                    decimal pnlPercent = 0.05m;
                    
                    // Bekleyen sell emirlerindeki miktar
                    decimal reservedQty = pendingList
                        .Where(o => o.Symbol == p.StockSymbol && o.Side == "Sell" && o.Status == "Pending")
                        .Sum(o => o.Quantity);
                    decimal availableQty = p.Quantity - reservedQty;
                    
                    return new
                    {
                        Symbol = p.StockSymbol,
                        p.Quantity,
                        AvailableQty = availableQty,
                        AvgCostRaw = p.AverageCost,
                        CurrentPriceRaw = currentPrice,
                        MarketValueRaw = marketValue,
                        PnL = pnl,
                        PnLPercentRaw = pnlPercent
                    };
                }).ToList();
                
                gridPositions.DataSource = positionList;
                gridPendingOrders.DataSource = pendingList;
                
                // Load Cash balance
                var accounts = await _accountRepository.GetByCustomerIdAsync(customerId);
                var primaryAccount = accounts?.FirstOrDefault();
                decimal cash = primaryAccount?.Balance ?? 0;
                
                // Calculate totals
                decimal investedValue = positionList.Sum(p => p.MarketValueRaw);
                decimal totalPortfolio = cash + investedValue;
                decimal totalPnL = positionList.Sum(p => p.PnL);
                decimal dailyPnL = totalPnL * 0.1m; // Simulated daily (günlük %10'u)
                int pendingCount = pendingList.Count;
                
                // Update 6 Summary Cards
                lblTotalPortfolio.Text = $"₺{totalPortfolio:N2}";
                lblCash.Text = $"₺{cash:N2}";
                lblInvestedValue.Text = $"₺{investedValue:N2}";
                
                lblDailyPnL.Text = $"{(dailyPnL >= 0 ? "+" : "")}₺{dailyPnL:N2}";
                lblDailyPnL.Appearance.ForeColor = dailyPnL >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                
                lblTotalPnL.Text = $"{(totalPnL >= 0 ? "+" : "")}₺{totalPnL:N2}";
                lblTotalPnL.Appearance.ForeColor = totalPnL >= 0 ? Color.FromArgb(76, 175, 80) : Color.FromArgb(239, 68, 68);
                
                lblPendingCount.Text = pendingCount.ToString();
                
                System.Diagnostics.Debug.WriteLine($"[DATA] PortfolioViewPro.LoadDataAsync END | positions={positionList.Count} pending={pendingCount} cash={cash:N2} invested={investedValue:N2} total={totalPortfolio:N2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERR] PortfolioViewPro.LoadDataAsync error: {ex.Message}");
            }
        }
    }
}
