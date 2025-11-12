using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using DevExpress.Utils;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// TradingView-like Investment Tab Control
    /// Hosts candlestick chart, watchlist, market cards, drawing tools, and bottom tabs
    /// </summary>
    public class InvestmentTabControl : XtraUserControl
    {
        #region Fields
        private readonly FinnhubService _finnhubService;
        
        // Layout containers
        private TableLayoutPanel rootLayout;
        private Panel pnlTopBar;
        private FlowLayoutPanel pnlMarketCards;
        private SplitContainer splitMain;
        private TableLayoutPanel sidebarLayout;
        private XtraTabControl tabBottomPanel;
        private Panel pnlDrawingTools;
        
        // Top bar controls
        private TextEdit txtSymbolSearch;
        private SimpleButton btnTimeframe1D;
        private SimpleButton btnTimeframe1H;
        private SimpleButton btnTimeframe1W;
        private SimpleButton btnTimeframe1M;
        private SimpleButton btnRefresh;
        private SimpleButton btnTradeTerminal;
        
        // Chart
        private ChartControl chartMain;
        
        // Sidebar controls
        private GridControl gridWatchlist;
        private GridView gridViewWatchlist;
        private Panel pnlSymbolDetails;
        private LabelControl lblSymbolName;
        private LabelControl lblSymbolPrice;
        private LabelControl lblSymbolChange;
        private LabelControl lblOHLC;
        private SimpleButton btnQuickBuy;
        private SimpleButton btnQuickSell;
        
        // Market cards
        private List<Panel> marketCards = new List<Panel>();
        
        // Drawing tools buttons
        private SimpleButton btnSupportLine;
        private SimpleButton btnResistanceLine;
        private SimpleButton btnFibonacci;
        private SimpleButton btnBuyTrigger;
        private SimpleButton btnSellTrigger;
        private SimpleButton btnClearDrawings;
        
        // Bottom tabs content
        private GridControl gridOrders;
        private GridView gridViewOrders;
        private LabelControl lblNewsContent;
        private GridControl gridPositions;
        private GridView gridViewPositions;
        
        // State
        private string _currentSymbol = "AAPL";
        private string _currentTimeframe = "D";
        private List<WatchlistItem> _watchlistData = new List<WatchlistItem>();
        #endregion

        public InvestmentTabControl()
        {
            _finnhubService = new FinnhubService();
            InitializeUI();
            this.Load += InvestmentTabControl_Load;
        }

        private async void InvestmentTabControl_Load(object sender, EventArgs e)
        {
            await LoadInitialDataAsync();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 18);
            this.DoubleBuffered = true;

            // Root TableLayoutPanel
            rootLayout = new TableLayoutPanel();
            rootLayout.Dock = DockStyle.Fill;
            rootLayout.RowCount = 4;
            rootLayout.ColumnCount = 2;
            rootLayout.BackColor = Color.FromArgb(18, 18, 18);
            
            // Row definitions
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55)); // Top bar
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 90)); // Market cards
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Main area
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 180)); // Bottom panel
            
            // Column definitions
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50)); // Drawing tools
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Main content
            
            this.Controls.Add(rootLayout);

            // Create all sections
            CreateDrawingToolsPanel();
            CreateTopBar();
            CreateMarketCardsStrip();
            CreateMainArea();
            CreateBottomPanel();
        }

        #region Drawing Tools Panel (Left Vertical Toolbar)
        private void CreateDrawingToolsPanel()
        {
            pnlDrawingTools = new Panel();
            pnlDrawingTools.Dock = DockStyle.Fill;
            pnlDrawingTools.BackColor = Color.FromArgb(25, 25, 25);
            pnlDrawingTools.Padding = new Padding(5);
            
            var toolsFlow = new FlowLayoutPanel();
            toolsFlow.Dock = DockStyle.Fill;
            toolsFlow.FlowDirection = FlowDirection.TopDown;
            toolsFlow.WrapContents = false;
            toolsFlow.AutoScroll = true;
            toolsFlow.BackColor = Color.Transparent;
            
            // Drawing tool buttons
            btnSupportLine = CreateToolButton("S", "Destek Çizgisi", Color.FromArgb(0, 200, 83));
            btnSupportLine.Click += (s, e) => AddSupportResistanceLine(true);
            
            btnResistanceLine = CreateToolButton("R", "Direnç Çizgisi", Color.FromArgb(255, 82, 82));
            btnResistanceLine.Click += (s, e) => AddSupportResistanceLine(false);
            
            btnFibonacci = CreateToolButton("F", "Fibonacci", Color.FromArgb(255, 193, 7));
            btnFibonacci.Click += (s, e) => AddFibonacciLevels();
            
            btnBuyTrigger = CreateToolButton("B", "AL Trigger", Color.FromArgb(0, 200, 83));
            btnBuyTrigger.Click += (s, e) => AddTriggerPoint(true);
            
            btnSellTrigger = CreateToolButton("X", "SAT Trigger", Color.FromArgb(255, 82, 82));
            btnSellTrigger.Click += (s, e) => AddTriggerPoint(false);
            
            btnClearDrawings = CreateToolButton("C", "Temizle", Color.FromArgb(158, 158, 158));
            btnClearDrawings.Click += (s, e) => ClearAllDrawings();
            
            toolsFlow.Controls.Add(btnSupportLine);
            toolsFlow.Controls.Add(btnResistanceLine);
            toolsFlow.Controls.Add(btnFibonacci);
            toolsFlow.Controls.Add(btnBuyTrigger);
            toolsFlow.Controls.Add(btnSellTrigger);
            toolsFlow.Controls.Add(btnClearDrawings);
            
            pnlDrawingTools.Controls.Add(toolsFlow);
            
            // Span rows 0-3
            rootLayout.SetRowSpan(pnlDrawingTools, 4);
            rootLayout.Controls.Add(pnlDrawingTools, 0, 0);
        }
        
        private SimpleButton CreateToolButton(string text, string tooltip, Color color)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(38, 38);
            btn.Margin = new Padding(2);
            btn.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btn.Appearance.ForeColor = color;
            btn.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.Options.UseFont = true;
            btn.ToolTip = tooltip;
            return btn;
        }
        #endregion

        #region Top Bar
        private void CreateTopBar()
        {
            pnlTopBar = new Panel();
            pnlTopBar.Dock = DockStyle.Fill;
            pnlTopBar.BackColor = Color.FromArgb(25, 25, 25);
            pnlTopBar.Padding = new Padding(10, 8, 10, 8);
            
            var topFlow = new FlowLayoutPanel();
            topFlow.Dock = DockStyle.Fill;
            topFlow.FlowDirection = FlowDirection.LeftToRight;
            topFlow.WrapContents = false;
            topFlow.BackColor = Color.Transparent;
            
            // Symbol search
            txtSymbolSearch = new TextEdit();
            txtSymbolSearch.Size = new Size(150, 35);
            txtSymbolSearch.Properties.NullText = "Sembol ara...";
            txtSymbolSearch.Properties.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            txtSymbolSearch.Properties.Appearance.ForeColor = Color.White;
            txtSymbolSearch.Margin = new Padding(0, 0, 15, 0);
            txtSymbolSearch.KeyDown += TxtSymbolSearch_KeyDown;
            
            // Timeframe buttons
            btnTimeframe1D = CreateTimeframeButton("1G", "D");
            btnTimeframe1H = CreateTimeframeButton("1S", "60");
            btnTimeframe1W = CreateTimeframeButton("1H", "W");
            btnTimeframe1M = CreateTimeframeButton("1A", "M");
            
            // Refresh button
            btnRefresh = new SimpleButton();
            btnRefresh.Text = "Yenile";
            btnRefresh.Size = new Size(80, 35);
            btnRefresh.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.Margin = new Padding(15, 0, 0, 0);
            btnRefresh.Click += async (s, e) => await RefreshDataAsync();
            
            // Trade Terminal button
            btnTradeTerminal = new SimpleButton();
            btnTradeTerminal.Text = "Trade Terminal";
            btnTradeTerminal.Size = new Size(120, 35);
            btnTradeTerminal.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            btnTradeTerminal.Appearance.ForeColor = Color.White;
            btnTradeTerminal.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnTradeTerminal.Margin = new Padding(10, 0, 0, 0);
            btnTradeTerminal.Click += BtnTradeTerminal_Click;
            
            topFlow.Controls.Add(txtSymbolSearch);
            topFlow.Controls.Add(btnTimeframe1D);
            topFlow.Controls.Add(btnTimeframe1H);
            topFlow.Controls.Add(btnTimeframe1W);
            topFlow.Controls.Add(btnTimeframe1M);
            topFlow.Controls.Add(btnRefresh);
            topFlow.Controls.Add(btnTradeTerminal);
            
            pnlTopBar.Controls.Add(topFlow);
            rootLayout.Controls.Add(pnlTopBar, 1, 0);
        }
        
        private SimpleButton CreateTimeframeButton(string text, string timeframe)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(50, 35);
            btn.Margin = new Padding(3, 0, 3, 0);
            btn.Tag = timeframe;
            btn.Appearance.BackColor = Color.FromArgb(50, 50, 50);
            btn.Appearance.ForeColor = Color.White;
            btn.Appearance.Font = new Font("Segoe UI", 9F);
            btn.Click += TimeframeButton_Click;
            return btn;
        }
        
        private void TimeframeButton_Click(object sender, EventArgs e)
        {
            if (sender is SimpleButton btn && btn.Tag is string tf)
            {
                _currentTimeframe = tf;
                
                // Highlight selected
                foreach (var b in new[] { btnTimeframe1D, btnTimeframe1H, btnTimeframe1W, btnTimeframe1M })
                {
                    b.Appearance.BackColor = (b == btn) ? Color.FromArgb(33, 150, 243) : Color.FromArgb(50, 50, 50);
                }
                
                _ = LoadChartDataAsync(_currentSymbol, _currentTimeframe);
            }
        }
        
        private void TxtSymbolSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !string.IsNullOrWhiteSpace(txtSymbolSearch.Text))
            {
                _currentSymbol = txtSymbolSearch.Text.ToUpper().Trim();
                _ = LoadChartDataAsync(_currentSymbol, _currentTimeframe);
                e.Handled = true;
            }
        }
        
        private void BtnTradeTerminal_Click(object sender, EventArgs e)
        {
            var tradeForm = new Forms.TradeTerminalForm();
            tradeForm.ShowDialog();
        }
        #endregion

        #region Market Cards Strip
        private void CreateMarketCardsStrip()
        {
            pnlMarketCards = new FlowLayoutPanel();
            pnlMarketCards.Dock = DockStyle.Fill;
            pnlMarketCards.BackColor = Color.FromArgb(20, 20, 20);
            pnlMarketCards.FlowDirection = FlowDirection.LeftToRight;
            pnlMarketCards.WrapContents = false;
            pnlMarketCards.AutoScroll = true;
            pnlMarketCards.Padding = new Padding(10, 10, 10, 10);
            
            // Create initial cards (will be updated with real data)
            var symbols = new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "BTC-USD", "GC=F" };
            var names = new[] { "Apple", "Microsoft", "Google", "Tesla", "Bitcoin", "Gold" };
            
            for (int i = 0; i < symbols.Length; i++)
            {
                var card = CreateMarketCard(symbols[i], names[i], 0, 0);
                marketCards.Add(card);
                pnlMarketCards.Controls.Add(card);
            }
            
            rootLayout.Controls.Add(pnlMarketCards, 1, 1);
        }
        
        private Panel CreateMarketCard(string symbol, string name, decimal price, decimal changePercent)
        {
            var card = new Panel();
            card.Size = new Size(140, 70);
            card.Margin = new Padding(5);
            card.BackColor = Color.FromArgb(30, 30, 30);
            card.Tag = symbol;
            card.Cursor = Cursors.Hand;
            // Click handler for the card
            EventHandler cardClickHandler = (s, e) => {
                _currentSymbol = symbol;
                txtSymbolSearch.Text = symbol;
                _ = LoadChartDataAsync(symbol, _currentTimeframe);
            };
            card.Click += cardClickHandler;
            
            var lblName = new Label();
            lblName.Text = name;
            lblName.ForeColor = Color.Gray;
            lblName.Font = new Font("Segoe UI", 8F);
            lblName.Location = new Point(8, 5);
            lblName.AutoSize = true;
            lblName.Click += cardClickHandler;
            
            var lblPrice = new Label();
            lblPrice.Text = price > 0 ? $"${price:N2}" : "--";
            lblPrice.ForeColor = Color.White;
            lblPrice.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPrice.Location = new Point(8, 22);
            lblPrice.AutoSize = true;
            lblPrice.Name = "lblPrice";
            lblPrice.Click += cardClickHandler;
            
            var lblChange = new Label();
            lblChange.Text = changePercent != 0 ? $"{changePercent:+0.00;-0.00}%" : "--";
            lblChange.ForeColor = changePercent >= 0 ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
            lblChange.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblChange.Location = new Point(8, 45);
            lblChange.AutoSize = true;
            lblChange.Name = "lblChange";
            lblChange.Click += cardClickHandler;
            
            card.Controls.Add(lblName);
            card.Controls.Add(lblPrice);
            card.Controls.Add(lblChange);
            
            return card;
        }
        
        private void UpdateMarketCard(Panel card, decimal price, decimal changePercent)
        {
            var lblPrice = card.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblPrice");
            var lblChange = card.Controls.OfType<Label>().FirstOrDefault(l => l.Name == "lblChange");
            
            if (lblPrice != null)
                lblPrice.Text = $"${price:N2}";
            
            if (lblChange != null)
            {
                lblChange.Text = $"{changePercent:+0.00;-0.00}%";
                lblChange.ForeColor = changePercent >= 0 ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
            }
        }
        #endregion

        #region Main Area (Chart + Sidebar)
        private void CreateMainArea()
        {
            splitMain = new SplitContainer();
            splitMain.Dock = DockStyle.Fill;
            splitMain.Orientation = Orientation.Vertical;
            splitMain.BackColor = Color.FromArgb(18, 18, 18);
            splitMain.SplitterWidth = 3;
            splitMain.Panel1MinSize = 100;
            splitMain.Panel2MinSize = 100;
            // SplitterDistance will be set after control is sized
            splitMain.SizeChanged += (s, e) => {
                if (splitMain.Width > 300)
                    splitMain.SplitterDistance = (int)(splitMain.Width * 0.70); // 70% for chart
            };
            
            // Chart panel
            CreateChartArea(splitMain.Panel1);
            
            // Sidebar
            CreateSidebar(splitMain.Panel2);
            
            rootLayout.Controls.Add(splitMain, 1, 2);
        }
        
        private void CreateChartArea(SplitterPanel panel)
        {
            chartMain = new ChartControl();
            chartMain.Dock = DockStyle.Fill;
            chartMain.BackColor = Color.FromArgb(25, 25, 25);
            chartMain.Legend.Visibility = DefaultBoolean.False;
            chartMain.BorderOptions.Visibility = DefaultBoolean.False;
            
            // Setup crosshair
            chartMain.CrosshairEnabled = DefaultBoolean.True;
            chartMain.CrosshairOptions.ShowArgumentLine = true;
            chartMain.CrosshairOptions.ShowValueLine = true;
            chartMain.CrosshairOptions.ShowValueLabels = true;
            chartMain.CrosshairOptions.ArgumentLineColor = Color.Gold;
            chartMain.CrosshairOptions.ValueLineColor = Color.Gold;
            
            // Initialize with empty series to create diagram
            var series = new Series("Price", ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;
            chartMain.Series.Add(series);
            
            if (chartMain.Diagram is XYDiagram diagram)
            {
                diagram.DefaultPane.BackColor = Color.FromArgb(25, 25, 25);
                diagram.DefaultPane.BorderVisible = false;
                
                diagram.AxisX.GridLines.Visible = true;
                diagram.AxisX.GridLines.Color = Color.FromArgb(40, 40, 40);
                diagram.AxisY.GridLines.Visible = true;
                diagram.AxisY.GridLines.Color = Color.FromArgb(40, 40, 40);
                
                diagram.AxisX.Label.TextColor = Color.Gray;
                diagram.AxisY.Label.TextColor = Color.Gray;
                
                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYScrolling = true;
                diagram.EnableAxisYZooming = true;
                diagram.ZoomingOptions.UseMouseWheel = true;
            }
            
            chartMain.Series.Clear();
            
            // Context menu for drawing tools
            InitializeChartContextMenu();
            
            panel.Controls.Add(chartMain);
        }
        
        private void CreateSidebar(SplitterPanel panel)
        {
            sidebarLayout = new TableLayoutPanel();
            sidebarLayout.Dock = DockStyle.Fill;
            sidebarLayout.RowCount = 2;
            sidebarLayout.ColumnCount = 1;
            sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            sidebarLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            sidebarLayout.BackColor = Color.FromArgb(22, 22, 22);
            
            // Watchlist grid
            CreateWatchlistGrid();
            sidebarLayout.Controls.Add(gridWatchlist, 0, 0);
            
            // Symbol details panel
            CreateSymbolDetailsPanel();
            sidebarLayout.Controls.Add(pnlSymbolDetails, 0, 1);
            
            panel.Controls.Add(sidebarLayout);
        }
        
        private void CreateWatchlistGrid()
        {
            gridWatchlist = new GridControl();
            gridWatchlist.Dock = DockStyle.Fill;
            gridWatchlist.LookAndFeel.UseDefaultLookAndFeel = false;
            gridWatchlist.LookAndFeel.SkinName = "Office 2019 Black";
            
            gridViewWatchlist = new GridView(gridWatchlist);
            gridWatchlist.MainView = gridViewWatchlist;
            
            gridViewWatchlist.OptionsView.ShowGroupPanel = false;
            gridViewWatchlist.OptionsView.ShowIndicator = false;
            gridViewWatchlist.OptionsView.ColumnAutoWidth = true;
            gridViewWatchlist.Appearance.HeaderPanel.BackColor = Color.FromArgb(30, 30, 30);
            gridViewWatchlist.Appearance.HeaderPanel.ForeColor = Color.White;
            gridViewWatchlist.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridViewWatchlist.Appearance.Row.BackColor = Color.FromArgb(22, 22, 22);
            gridViewWatchlist.Appearance.Row.ForeColor = Color.White;
            gridViewWatchlist.RowCellStyle += GridViewWatchlist_RowCellStyle;
            gridViewWatchlist.FocusedRowChanged += GridViewWatchlist_FocusedRowChanged;
        }
        
        private void GridViewWatchlist_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "Change")
            {
                var value = gridViewWatchlist.GetRowCellValue(e.RowHandle, "Change");
                if (value != null && decimal.TryParse(value.ToString().Replace("%", ""), out decimal change))
                {
                    e.Appearance.ForeColor = change >= 0 ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
                }
            }
        }
        
        private void GridViewWatchlist_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                var symbol = gridViewWatchlist.GetRowCellValue(e.FocusedRowHandle, "Symbol")?.ToString();
                if (!string.IsNullOrEmpty(symbol))
                {
                    _currentSymbol = symbol;
                    _ = LoadChartDataAsync(symbol, _currentTimeframe);
                    UpdateSymbolDetails(symbol);
                }
            }
        }
        
        private void CreateSymbolDetailsPanel()
        {
            pnlSymbolDetails = new Panel();
            pnlSymbolDetails.Dock = DockStyle.Fill;
            pnlSymbolDetails.BackColor = Color.FromArgb(25, 25, 25);
            pnlSymbolDetails.Padding = new Padding(12);
            
            lblSymbolName = new LabelControl();
            lblSymbolName.Text = "AAPL";
            lblSymbolName.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblSymbolName.Appearance.ForeColor = Color.White;
            lblSymbolName.Location = new Point(12, 12);
            
            lblSymbolPrice = new LabelControl();
            lblSymbolPrice.Text = "$0.00";
            lblSymbolPrice.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblSymbolPrice.Appearance.ForeColor = Color.White;
            lblSymbolPrice.Location = new Point(12, 40);
            
            lblSymbolChange = new LabelControl();
            lblSymbolChange.Text = "+0.00%";
            lblSymbolChange.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblSymbolChange.Appearance.ForeColor = Color.FromArgb(0, 200, 83);
            lblSymbolChange.Location = new Point(12, 75);
            
            lblOHLC = new LabelControl();
            lblOHLC.Text = "O: -- H: -- L: -- C: --";
            lblOHLC.Appearance.Font = new Font("Consolas", 9F);
            lblOHLC.Appearance.ForeColor = Color.Gray;
            lblOHLC.Location = new Point(12, 105);
            
            btnQuickBuy = new SimpleButton();
            btnQuickBuy.Text = "AL";
            btnQuickBuy.Size = new Size(70, 35);
            btnQuickBuy.Location = new Point(12, 140);
            btnQuickBuy.Appearance.BackColor = Color.FromArgb(0, 200, 83);
            btnQuickBuy.Appearance.ForeColor = Color.White;
            btnQuickBuy.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnQuickBuy.Click += (s, e) => XtraMessageBox.Show($"{_currentSymbol} için AL emri verildi!", "Emir", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            btnQuickSell = new SimpleButton();
            btnQuickSell.Text = "SAT";
            btnQuickSell.Size = new Size(70, 35);
            btnQuickSell.Location = new Point(90, 140);
            btnQuickSell.Appearance.BackColor = Color.FromArgb(255, 82, 82);
            btnQuickSell.Appearance.ForeColor = Color.White;
            btnQuickSell.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnQuickSell.Click += (s, e) => XtraMessageBox.Show($"{_currentSymbol} için SAT emri verildi!", "Emir", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            pnlSymbolDetails.Controls.Add(lblSymbolName);
            pnlSymbolDetails.Controls.Add(lblSymbolPrice);
            pnlSymbolDetails.Controls.Add(lblSymbolChange);
            pnlSymbolDetails.Controls.Add(lblOHLC);
            pnlSymbolDetails.Controls.Add(btnQuickBuy);
            pnlSymbolDetails.Controls.Add(btnQuickSell);
        }
        
        private void UpdateSymbolDetails(string symbol)
        {
            lblSymbolName.Text = symbol;
            
            var watchItem = _watchlistData.FirstOrDefault(w => w.Symbol == symbol);
            if (watchItem != null)
            {
                lblSymbolPrice.Text = $"${watchItem.Price:N2}";
                lblSymbolChange.Text = $"{watchItem.ChangePercent:+0.00;-0.00}%";
                lblSymbolChange.Appearance.ForeColor = watchItem.ChangePercent >= 0 
                    ? Color.FromArgb(0, 200, 83) 
                    : Color.FromArgb(255, 82, 82);
            }
        }
        #endregion

        #region Bottom Panel (Orders/News/Positions)
        private void CreateBottomPanel()
        {
            tabBottomPanel = new XtraTabControl();
            tabBottomPanel.Dock = DockStyle.Fill;
            tabBottomPanel.LookAndFeel.UseDefaultLookAndFeel = false;
            tabBottomPanel.LookAndFeel.SkinName = "Office 2019 Black";
            
            // Orders tab
            var tabOrders = new XtraTabPage();
            tabOrders.Text = "Emirlerim";
            CreateOrdersGrid(tabOrders);
            tabBottomPanel.TabPages.Add(tabOrders);
            
            // News tab
            var tabNews = new XtraTabPage();
            tabNews.Text = "Haberler";
            CreateNewsPanel(tabNews);
            tabBottomPanel.TabPages.Add(tabNews);
            
            // Positions tab
            var tabPositions = new XtraTabPage();
            tabPositions.Text = "Pozisyonlar";
            CreatePositionsGrid(tabPositions);
            tabBottomPanel.TabPages.Add(tabPositions);
            
            rootLayout.Controls.Add(tabBottomPanel, 1, 3);
        }
        
        private void CreateOrdersGrid(XtraTabPage tab)
        {
            gridOrders = new GridControl();
            gridOrders.Dock = DockStyle.Fill;
            gridOrders.LookAndFeel.UseDefaultLookAndFeel = false;
            gridOrders.LookAndFeel.SkinName = "Office 2019 Black";
            
            gridViewOrders = new GridView(gridOrders);
            gridOrders.MainView = gridViewOrders;
            gridViewOrders.OptionsView.ShowGroupPanel = false;
            
            // Mock data
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Tarih");
            dt.Columns.Add("Sembol");
            dt.Columns.Add("Tip");
            dt.Columns.Add("Fiyat");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Durum");
            
            dt.Rows.Add(DateTime.Now.ToString("dd.MM HH:mm"), "AAPL", "AL", "$178.50", "10", "Beklemede");
            dt.Rows.Add(DateTime.Now.AddHours(-1).ToString("dd.MM HH:mm"), "TSLA", "SAT", "$248.00", "5", "Beklemede");
            dt.Rows.Add(DateTime.Now.AddHours(-2).ToString("dd.MM HH:mm"), "MSFT", "AL", "$375.00", "8", "Kısmi");
            
            gridOrders.DataSource = dt;
            tab.Controls.Add(gridOrders);
        }
        
        private void CreateNewsPanel(XtraTabPage tab)
        {
            lblNewsContent = new LabelControl();
            lblNewsContent.Dock = DockStyle.Fill;
            lblNewsContent.AutoSizeMode = LabelAutoSizeMode.None;
            lblNewsContent.Appearance.Font = new Font("Segoe UI", 10F);
            lblNewsContent.Appearance.ForeColor = Color.LightGray;
            lblNewsContent.Padding = new Padding(15);
            lblNewsContent.Text = "Haberler yükleniyor...";
            tab.Controls.Add(lblNewsContent);
        }
        
        private void CreatePositionsGrid(XtraTabPage tab)
        {
            gridPositions = new GridControl();
            gridPositions.Dock = DockStyle.Fill;
            gridPositions.LookAndFeel.UseDefaultLookAndFeel = false;
            gridPositions.LookAndFeel.SkinName = "Office 2019 Black";
            
            gridViewPositions = new GridView(gridPositions);
            gridPositions.MainView = gridViewPositions;
            gridViewPositions.OptionsView.ShowGroupPanel = false;
            
            // Mock data
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Sembol");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Ort. Maliyet");
            dt.Columns.Add("Son Fiyat");
            dt.Columns.Add("Kar/Zarar");
            
            dt.Rows.Add("AAPL", "25", "$165.00", "$178.50", "+$337.50");
            dt.Rows.Add("MSFT", "15", "$380.00", "$375.00", "-$75.00");
            dt.Rows.Add("GOOGL", "10", "$140.00", "$142.50", "+$25.00");
            
            gridPositions.DataSource = dt;
            tab.Controls.Add(gridPositions);
        }
        #endregion

        #region Chart Context Menu & Drawing Tools
        private ContextMenuStrip _chartContextMenu;
        
        private void InitializeChartContextMenu()
        {
            _chartContextMenu = new ContextMenuStrip();
            _chartContextMenu.Renderer = new ToolStripProfessionalRenderer(new DarkColorTable());
            
            var itemSupport = _chartContextMenu.Items.Add("Destek Çizgisi");
            itemSupport.ForeColor = Color.LightGreen;
            itemSupport.Click += (s, e) => AddSupportResistanceLine(true);
            
            var itemResistance = _chartContextMenu.Items.Add("Direnç Çizgisi");
            itemResistance.ForeColor = Color.LightCoral;
            itemResistance.Click += (s, e) => AddSupportResistanceLine(false);
            
            _chartContextMenu.Items.Add(new ToolStripSeparator());
            
            var itemFib = _chartContextMenu.Items.Add("Fibonacci Seviyeleri");
            itemFib.ForeColor = Color.Gold;
            itemFib.Click += (s, e) => AddFibonacciLevels();
            
            _chartContextMenu.Items.Add(new ToolStripSeparator());
            
            var itemBuy = _chartContextMenu.Items.Add("AL Trigger");
            itemBuy.ForeColor = Color.LightGreen;
            itemBuy.Click += (s, e) => AddTriggerPoint(true);
            
            var itemSell = _chartContextMenu.Items.Add("SAT Trigger");
            itemSell.ForeColor = Color.LightCoral;
            itemSell.Click += (s, e) => AddTriggerPoint(false);
            
            _chartContextMenu.Items.Add(new ToolStripSeparator());
            
            var itemClear = _chartContextMenu.Items.Add("Çizimleri Temizle");
            itemClear.ForeColor = Color.White;
            itemClear.Click += (s, e) => ClearAllDrawings();
            
            chartMain.MouseUp += (s, e) => {
                if (e.Button == MouseButtons.Right)
                    _chartContextMenu.Show(chartMain, e.Location);
            };
        }
        
        private void AddSupportResistanceLine(bool isSupport)
        {
            if (chartMain.Diagram is XYDiagram diagram)
            {
                string input = ShowInputDialog($"{(isSupport ? "Destek" : "Direnç")} Seviyesi:", "100");
                if (decimal.TryParse(input, out decimal price))
                {
                    var line = new ConstantLine($"{(isSupport ? "Destek" : "Direnç")}: {price:N2}");
                    line.AxisValue = price;
                    line.Color = isSupport ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
                    line.LineStyle.Thickness = 2;
                    line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
                    line.ShowInLegend = false;
                    line.Title.TextColor = line.Color;
                    line.Title.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                    diagram.AxisY.ConstantLines.Add(line);
                }
            }
        }
        
        private void AddFibonacciLevels()
        {
            if (chartMain.Diagram is XYDiagram diagram && chartMain.Series.Count > 0)
            {
                string highInput = ShowInputDialog("Yüksek Fiyat:", "110");
                string lowInput = ShowInputDialog("Düşük Fiyat:", "90");
                
                if (decimal.TryParse(highInput, out decimal high) && decimal.TryParse(lowInput, out decimal low))
                {
                    decimal diff = high - low;
                    decimal[] fibs = { 0m, 0.236m, 0.382m, 0.5m, 0.618m, 1.0m };
                    
                    foreach (var f in fibs)
                    {
                        decimal levelPrice = low + (diff * f);
                        var line = new ConstantLine($"Fib {f:P1}");
                        line.AxisValue = levelPrice;
                        line.Color = Color.FromArgb(100, 255, 215, 0);
                        line.LineStyle.Thickness = 1;
                        line.ShowInLegend = false;
                        line.Title.TextColor = Color.Gold;
                        diagram.AxisY.ConstantLines.Add(line);
                    }
                }
            }
        }
        
        private void AddTriggerPoint(bool isBuy)
        {
            if (chartMain.Diagram is XYDiagram diagram)
            {
                string input = ShowInputDialog($"{(isBuy ? "AL" : "SAT")} Trigger Fiyatı:", "100");
                if (decimal.TryParse(input, out decimal price))
                {
                    var line = new ConstantLine($"{(isBuy ? "AL" : "SAT")} @ {price:N2}");
                    line.AxisValue = price;
                    line.Color = isBuy ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
                    line.LineStyle.Thickness = 3;
                    line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.DashDot;
                    line.ShowInLegend = false;
                    line.Title.TextColor = line.Color;
                    line.Title.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                    diagram.AxisY.ConstantLines.Add(line);
                }
            }
        }
        
        private void ClearAllDrawings()
        {
            chartMain.AnnotationRepository.Clear();
            if (chartMain.Diagram is XYDiagram diagram)
            {
                diagram.AxisY.ConstantLines.Clear();
            }
        }
        
        private string ShowInputDialog(string prompt, string defaultValue)
        {
            using (var form = new Form())
            {
                form.Size = new Size(300, 150);
                form.Text = prompt;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.BackColor = Color.FromArgb(35, 35, 35);
                
                var lbl = new Label { Text = prompt, Left = 15, Top = 15, ForeColor = Color.White, AutoSize = true };
                var txt = new TextBox { Left = 15, Top = 40, Width = 250, Text = defaultValue, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
                var btn = new Button { Text = "Tamam", Left = 165, Top = 75, Width = 100, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
                
                form.Controls.AddRange(new Control[] { lbl, txt, btn });
                form.AcceptButton = btn;
                
                return form.ShowDialog() == DialogResult.OK ? txt.Text : "";
            }
        }
        #endregion

        #region Data Loading
        private async Task LoadInitialDataAsync()
        {
            await LoadWatchlistDataAsync();
            await LoadMarketCardsDataAsync();
            await LoadChartDataAsync(_currentSymbol, _currentTimeframe);
            await LoadNewsAsync();
        }
        
        private async Task RefreshDataAsync()
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "...";
            
            await LoadInitialDataAsync();
            
            btnRefresh.Enabled = true;
            btnRefresh.Text = "Yenile";
        }
        
        private async Task LoadWatchlistDataAsync()
        {
            try
            {
                var symbols = new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "META", "NVDA", "AMD" };
                var quotes = await _finnhubService.GetMultipleQuotesAsync(symbols);
                
                _watchlistData.Clear();
                foreach (var q in quotes)
                {
                    _watchlistData.Add(new WatchlistItem
                    {
                        Symbol = q.symbol,
                        Price = (decimal)q.quote.C,
                        Change = $"{q.quote.Dp:+0.00;-0.00}%",
                        ChangePercent = (decimal)q.quote.Dp
                    });
                }
                
                var dt = new System.Data.DataTable();
                dt.Columns.Add("Symbol");
                dt.Columns.Add("Price");
                dt.Columns.Add("Change");
                
                foreach (var item in _watchlistData)
                {
                    dt.Rows.Add(item.Symbol, $"${item.Price:N2}", item.Change);
                }
                
                gridWatchlist.DataSource = dt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadWatchlistData Error: {ex.Message}");
            }
        }
        
        private async Task LoadMarketCardsDataAsync()
        {
            try
            {
                var symbols = new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "BTC-USD", "GC=F" };
                var quotes = await _finnhubService.GetMultipleQuotesAsync(symbols);
                
                for (int i = 0; i < Math.Min(marketCards.Count, quotes.Count); i++)
                {
                    UpdateMarketCard(marketCards[i], (decimal)quotes[i].quote.C, (decimal)quotes[i].quote.Dp);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMarketCardsData Error: {ex.Message}");
            }
        }
        
        private async Task LoadChartDataAsync(string symbol, string timeframe)
        {
            try
            {
                chartMain.Series.Clear();
                
                var series = new Series(symbol, ViewType.CandleStick);
                series.ArgumentScaleType = ScaleType.DateTime;
                
                var candles = await _finnhubService.GetCandlesAsync(symbol, timeframe, 60);
                
                if (candles?.C != null && candles.C.Count > 0)
                {
                    for (int i = 0; i < candles.C.Count; i++)
                    {
                        var date = DateTimeOffset.FromUnixTimeSeconds(candles.T[i]).DateTime;
                        series.Points.Add(new SeriesPoint(date, candles.L[i], candles.H[i], candles.O[i], candles.C[i]));
                    }
                    
                    // Update symbol details
                    var latestClose = candles.C.Last();
                    var prevClose = candles.C.Count > 1 ? candles.C[candles.C.Count - 2] : latestClose;
                    var change = ((latestClose - prevClose) / prevClose) * 100;
                    
                    lblSymbolName.Text = symbol;
                    lblSymbolPrice.Text = $"${latestClose:N2}";
                    lblSymbolChange.Text = $"{change:+0.00;-0.00}%";
                    lblSymbolChange.Appearance.ForeColor = change >= 0 ? Color.FromArgb(0, 200, 83) : Color.FromArgb(255, 82, 82);
                    lblOHLC.Text = $"O: {candles.O.Last():N2}  H: {candles.H.Last():N2}  L: {candles.L.Last():N2}  C: {candles.C.Last():N2}";
                }
                else
                {
                    // Fallback dummy data
                    GenerateFallbackChartData(series);
                }
                
                if (series.View is CandleStickSeriesView view)
                {
                    view.Color = Color.FromArgb(0, 200, 83);
                    view.ReductionOptions.Color = Color.FromArgb(255, 82, 82);
                    view.ReductionOptions.Visible = true;
                    view.ReductionOptions.Level = StockLevel.Close;
                    view.LineThickness = 1;
                    
                    // Add indicators
                    var sma = new SimpleMovingAverage();
                    sma.PointsCount = 20;
                    sma.Color = Color.Orange;
                    sma.LineStyle.Thickness = 2;
                    view.Indicators.Add(sma);
                }
                
                chartMain.Series.Add(series);
                
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    diagram.AxisY.WholeRange.AlwaysShowZeroLevel = false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadChartData Error: {ex.Message}");
            }
        }
        
        private void GenerateFallbackChartData(Series series)
        {
            var rnd = new Random();
            double price = 100;
            var date = DateTime.Now.AddDays(-60);
            
            for (int i = 0; i < 60; i++)
            {
                double change = (rnd.NextDouble() * 4) - 2;
                double open = price;
                double close = price + change;
                double high = Math.Max(open, close) + rnd.NextDouble() * 2;
                double low = Math.Min(open, close) - rnd.NextDouble() * 2;
                
                series.Points.Add(new SeriesPoint(date.AddDays(i), low, high, open, close));
                price = close;
            }
        }
        
        private async Task LoadNewsAsync()
        {
            try
            {
                var news = await _finnhubService.GetMarketNewsAsync();
                if (news?.Count > 0)
                {
                    var newsText = string.Join("\n\n", news.Take(5).Select(n =>
                        $"• {n.Headline}\n  {n.Source} - {DateTimeOffset.FromUnixTimeSeconds(n.Datetime):MMM dd, HH:mm}"));
                    lblNewsContent.Text = newsText;
                }
            }
            catch (Exception ex)
            {
                lblNewsContent.Text = "Haberler yüklenemedi.";
                System.Diagnostics.Debug.WriteLine($"LoadNews Error: {ex.Message}");
            }
        }
        #endregion

        #region Helper Classes
        private class WatchlistItem
        {
            public string Symbol { get; set; }
            public decimal Price { get; set; }
            public string Change { get; set; }
            public decimal ChangePercent { get; set; }
        }
        
        private class DarkColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(60, 60, 60);
            public override Color MenuBorder => Color.Black;
            public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 30);
        }
        #endregion
    }
}
