using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Docking;
using DevExpress.XtraCharts;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using DevExpress.Utils;
using DevExpress.XtraBars.Navigation;
using DevExpress.XtraBars.Alerter;
using DevExpress.XtraLayout;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.AI;
using BankApp.Infrastructure.Data;
using BankApp.Core.Interfaces;
using BankApp.UI.Forms;
using BankApp.Infrastructure.Events;
using Dapper;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// TradingView-style Investment View with DevExpress DockManager layout
    /// </summary>
    public class InvestmentView : XtraUserControl
    {
        #region Fields
        private readonly FinnhubService _finnhubService;
        private readonly TransactionService _transactionService;
        private readonly IAccountRepository _accountRepository;
        private readonly CustomerPortfolioRepository _portfolioRepository;
        
        // DevExpress Layout Managers
        private BarManager barManager;
        private DockManager dockManager;
        
        // Top Bar Items
        private Bar barTop;
        private BarEditItem barSymbolSearch;
        private BarButtonItem barTimeframe1m;
        private BarButtonItem barTimeframe5m;
        private BarButtonItem barTimeframe15m;
        private BarButtonItem barTimeframe1H;
        private BarButtonItem barTimeframe4H;
        private BarButtonItem barTimeframe1D;
        private BarButtonItem barTimeframe1W;
        private BarButtonItem barIndicators;
        private BarButtonItem barRefresh;
        private BarButtonItem barFullscreen;
        
        // Left Toolbar Items (Drawing Tools) - TradingView Style
        private Bar barDrawingTools;
        private BarButtonItem barToolSelect;
        private BarButtonItem barToolCrosshair;
        private BarButtonItem barToolTrendline;
        private BarButtonItem barToolHorizontal;
        private BarButtonItem barToolRay;
        private BarButtonItem barToolFibonacci;
        private BarButtonItem barToolRectangle;
        private BarButtonItem barToolText;
        private BarButtonItem barToolTriggerPoint;
        private BarButtonItem barToolAutoSR;
        private BarButtonItem barToolAutoFib;
        private BarButtonItem barToolClear;
        private BarButtonItem barToolUndo;
        
        // Main Chart
        private ChartControl chartMain;
        private PanelControl pnlChartContainer;
        private PanelControl pnlOHLCStrip;
        private LabelControl lblOHLC;
        private LabelControl lblChartEmpty;
        private LabelControl lblChartLoading;
        private LabelControl lblChartError;
        private bool _isChartLoading = false;
        
        // OHLC Data
        private double _lastO, _lastH, _lastL, _lastC;
        private double _lastChange, _lastChangePercent;
        
        // Market Cards (TileBar)
        private TileBar tileBarMarket;
        
        // Right Panel Dock
        private DockPanel dockPanelRight;
        private ControlContainer rightContainer;
        private GridControl gridWatchlist;
        private GridView gridViewWatchlist;
        private PanelControl pnlSymbolDetails;
        private PanelControl pnlOrderTicket;
        private LabelControl lblSymbolName;
        private LabelControl lblSymbolPrice;
        private LabelControl lblSymbolChange;
        private TextEdit txtOrderQuantity;
        private ComboBoxEdit cmbOrderType;
        private TextEdit txtOrderPrice;
        private SimpleButton btnBuy;
        private SimpleButton btnSell;
        
        // Bottom Panel Dock
        private DockPanel dockPanelBottom;
        private ControlContainer bottomContainer;
        private XtraTabControl tabBottom;
        private XtraTabPage tabOrders;
        private XtraTabPage tabPositions;
        private XtraTabPage tabNews;
        private XtraTabPage tabAnalysis;
        private XtraTabPage tabTradeTerminal;
        private GridControl gridOrders;
        private GridControl gridPositions;
        private MemoEdit memoNews;
        private MemoEdit memoAnalysis;
        
        // State
        private string _currentSymbol = ""; // Empty = no symbol selected (user must click)
        private string _currentTimeframe = "D";
        private BarButtonItem _selectedTool;
        
        // Trading state
        private bool _isTrading = false;
        private AlertControl alertControl;
        private LabelControl lblTradeLoading;
        
        // AI Summary Card
        private PanelControl pnlAISummary;
        private LabelControl lblAISummaryText;
        
        // Analysis data
        private List<(string Name, double Value)> _supportResistanceLevels = new List<(string, double)>();
        private List<(string Name, double Value)> _fibonacciLevels = new List<(string, double)>();
        
        // Focus Mode state
        private bool _isFocusMode = false;
        private int _prevBottomHeight = 250;
        private int _prevRightWidth = 280;
        
        // Drawing Mode
        private DrawMode _activeDrawMode = DrawMode.None;
        #endregion
        
        #region Enums
        /// <summary>
        /// Drawing tool modes for chart interaction
        /// </summary>
        private enum DrawMode
        {
            None,
            Select,
            Crosshair,
            Trendline,
            HorizontalLine,
            Ray,
            Fib,
            Rectangle,
            Text,
            TriggerPoint,
            AutoSR,
            AutoFib
        }
        #endregion

        public InvestmentView()
        {
            // [OPENED] ZORUNLU FORMAT
            System.Diagnostics.Debug.WriteLine($"[OPENED] {GetType().FullName} | Handle=PENDING | Hash={GetHashCode()} | Parent={Parent?.Name ?? "null"} | Visible={Visible}");
            System.Diagnostics.Debug.WriteLine("=== INVESTMENTVIEW LOADED v2 ===");
            
            _finnhubService = new FinnhubService();
            
            // Initialize repositories and services for transaction processing
            var context = new DapperContext();
            _accountRepository = new AccountRepository(context);
            _portfolioRepository = new CustomerPortfolioRepository(context);
            var transactionRepo = new TransactionRepository(context);
            var auditRepo = new AuditRepository(context);
            _transactionService = new TransactionService(_accountRepository, transactionRepo, auditRepo);
            
            InitializeComponents();
            InitializeAlertControl();
            this.Load += InvestmentView_Load;
        }
        
        /// <summary>
        /// DevExpress AlertControl for toast notifications
        /// </summary>
        private void InitializeAlertControl()
        {
            alertControl = new AlertControl();
            alertControl.AutoFormDelay = 3000; // 3 seconds
            alertControl.FormLocation = AlertFormLocation.TopRight;
            alertControl.FormMaxCount = 3;
            alertControl.ShowPinButton = false;
            alertControl.ShowCloseButton = true;
            
            // Success style
            alertControl.AppearanceCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            alertControl.AppearanceCaption.ForeColor = Color.White;
            alertControl.AppearanceText.Font = new Font("Segoe UI", 9F);
            alertControl.AppearanceText.ForeColor = Color.FromArgb(220, 220, 220);
        }

        private async void InvestmentView_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] InvestmentView_Load fired, this={GetType().FullName}");
            
            // Load watchlist and market tiles, but NOT chart (user must select symbol)
            await LoadWatchlistDataAsync();
            await LoadMarketTilesDataAsync();
            await LoadPositionsDataAsync();
            await LoadOrderHistoryAsync();
            
            // Show empty state for chart
            ShowChartEmpty(true);
        }
        
        /// <summary>
        /// Override ProcessCmdKey to capture ESC for exiting draw mode
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape && _activeDrawMode != DrawMode.None)
            {
                SetDrawMode(DrawMode.None);
                return true; // Key handled
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Initialize UI
        private void InitializeComponents()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 18, 18);

            // Initialize managers first
            InitializeBarManager();
            InitializeDockManager();
            
            // Create main content
            CreateChartArea();
            CreateMarketTileBar();
            
            // Create dock panels
            CreateRightDockPanel();
            // CreateBottomDockPanel(); // Kaldırıldı - grafik tam ekran kaplasın
        }

        private void InitializeBarManager()
        {
            barManager = new BarManager();
            barManager.Form = this;
            barManager.AllowCustomization = false;
            barManager.AllowMoveBarOnToolbar = false;
            barManager.AllowQuickCustomization = false;

            // === TOP BAR (TradingView Style) ===
            barTop = new Bar();
            barTop.BarName = "Chart Bar";
            barTop.DockCol = 0;
            barTop.DockRow = 0;
            barTop.DockStyle = BarDockStyle.Top;
            barTop.OptionsBar.AllowQuickCustomization = false;
            barTop.OptionsBar.DrawDragBorder = false;
            barManager.Bars.Add(barTop);

            // Symbol Search with Exchange label
            var repoSearch = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            repoSearch.NullText = "Sembol ara (örn: AAPL)";
            repoSearch.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            repoSearch.Appearance.ForeColor = Color.White;
            barManager.RepositoryItems.Add(repoSearch);
            
            barSymbolSearch = new BarEditItem();
            barSymbolSearch.Caption = "NASDAQ";
            barSymbolSearch.Edit = repoSearch;
            barSymbolSearch.Width = 140;
            barSymbolSearch.EditValueChanged += BarSymbolSearch_EditValueChanged;
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barSymbolSearch));

            // TradingView-style timeframes: 1m, 5m, 15m, 1H, 4H, 1D, 1W
            barTimeframe1m = CreateTimeframeButton("1m", "1 Dakika", "1");
            barTimeframe5m = CreateTimeframeButton("5m", "5 Dakika", "5");
            barTimeframe15m = CreateTimeframeButton("15m", "15 Dakika", "15");
            barTimeframe1H = CreateTimeframeButton("1H", "1 Saat", "60");
            barTimeframe4H = CreateTimeframeButton("4H", "4 Saat", "240");
            barTimeframe1D = CreateTimeframeButton("1D", "1 Gün", "D");
            barTimeframe1D.Down = true; // Default
            barTimeframe1W = CreateTimeframeButton("1W", "1 Hafta", "W");

            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe1m, true));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe5m));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe15m));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe1H));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe4H));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe1D));
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barTimeframe1W));

            // Indicators button
            barIndicators = CreateBarButton("📊 Göstergeler", "Teknik Göstergeler", false);
            barIndicators.ItemClick += (s, e) => ShowIndicatorsMenu();
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barIndicators, true));

            // Refresh button
            barRefresh = CreateBarButton("🔄", "Verileri Yenile", false);
            barRefresh.ItemClick += async (s, e) => await RefreshCurrentSymbolAsync();
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barRefresh, true));

            // Focus Mode button (collapse/expand bottom panel)
            barFullscreen = CreateBarButton("⛶", "Focus Mode (Chart Büyüt)", false);
            barFullscreen.ItemClick += (s, e) => ToggleFocusMode();
            barTop.LinksPersistInfo.Add(new LinkPersistInfo(barFullscreen));

            // === LEFT DRAWING TOOLS BAR (TradingView Style) ===
            barDrawingTools = new Bar();
            barDrawingTools.BarName = "Araçlar";
            barDrawingTools.DockCol = 0;
            barDrawingTools.DockRow = 0;
            barDrawingTools.DockStyle = BarDockStyle.Left;
            barDrawingTools.OptionsBar.AllowQuickCustomization = false;
            barDrawingTools.OptionsBar.DrawDragBorder = false;
            barDrawingTools.OptionsBar.MultiLine = true;
            barDrawingTools.OptionsBar.UseWholeRow = true;
            barManager.Bars.Add(barDrawingTools);

            // Group 1: Cursor Tools
            barToolSelect = CreateToolButtonWithText("⊹", "Seç", "İmleç/Seçim Aracı (ESC)");
            barToolSelect.ItemClick += (s, e) => SetDrawMode(DrawMode.Select);
            barToolSelect.Down = true;
            
            barToolCrosshair = CreateToolButtonWithText("✛", "Crosshair", "Çapraz İmleç");
            barToolCrosshair.ItemClick += (s, e) => SetDrawMode(DrawMode.Crosshair);
            
            // Group 2: Line Tools (drawing modes)
            barToolTrendline = CreateToolButtonWithText("╲", "Trend", "Trend Çizgisi Çiz");
            barToolTrendline.ItemClick += (s, e) => SetDrawMode(DrawMode.Trendline);
            
            barToolHorizontal = CreateToolButtonWithText("─", "Yatay", "Yatay Çizgi (S/R)");
            barToolHorizontal.ItemClick += (s, e) => { SetDrawMode(DrawMode.HorizontalLine); AddHorizontalLine(); };
            
            barToolRay = CreateToolButtonWithText("↗", "Ray", "Işın Çizgisi");
            barToolRay.ItemClick += (s, e) => SetDrawMode(DrawMode.Ray);
            
            // Group 3: Fibonacci & Shapes
            barToolFibonacci = CreateToolButtonWithText("Fib", "Fibonacci", "Fibonacci Seviyeleri");
            barToolFibonacci.ItemClick += (s, e) => { SetDrawMode(DrawMode.Fib); AddFibonacciLevels(); };
            
            barToolRectangle = CreateToolButtonWithText("▭", "Dikdörtgen", "Dikdörtgen Çiz");
            barToolRectangle.ItemClick += (s, e) => SetDrawMode(DrawMode.Rectangle);
            
            barToolText = CreateToolButtonWithText("T", "Metin", "Metin Notu");
            barToolText.ItemClick += (s, e) => { SetDrawMode(DrawMode.Text); AddTextNote(); SetDrawMode(DrawMode.None); };
            
            // Group 4: Trigger Tools (one-shot analysis, returns to None)
            barToolTriggerPoint = CreateToolButtonWithText("◉", "Trigger", "TriggerPoint Analizi");
            barToolTriggerPoint.ItemClick += (s, e) => { RunTriggerPointAnalysis(); SetDrawMode(DrawMode.None); };
            
            barToolAutoSR = CreateToolButtonWithText("≡", "Auto S/R", "Otomatik Destek/Direnç");
            barToolAutoSR.ItemClick += (s, e) => { RunAutoSupportResistance(); SetDrawMode(DrawMode.None); };
            
            barToolAutoFib = CreateToolButtonWithText("⟨⟩", "Auto Fib", "Otomatik Fibonacci");
            barToolAutoFib.ItemClick += (s, e) => { RunAutoFibonacci(); SetDrawMode(DrawMode.None); };
            
            // Group 5: Clear & Undo
            barToolClear = CreateToolButtonWithText("🗑", "Temizle", "Tüm Çizimleri Temizle");
            barToolClear.ItemClick += (s, e) => ClearAllDrawings();
            
            barToolUndo = CreateToolButtonWithText("↶", "Geri Al", "Son Çizimi Geri Al");
            barToolUndo.ItemClick += (s, e) => UndoLastDrawing();

            // Add all tools to bar with group separators
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolSelect));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolCrosshair));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolTrendline, true)); // Group 2
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolHorizontal));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolRay));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolFibonacci, true)); // Group 3
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolRectangle));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolText));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolTriggerPoint, true)); // Group 4
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolAutoSR));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolAutoFib));
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolClear, true)); // Group 5
            barDrawingTools.LinksPersistInfo.Add(new LinkPersistInfo(barToolUndo));
        }
        
        private BarButtonItem CreateTimeframeButton(string caption, string hint, string tag)
        {
            var btn = new BarButtonItem();
            btn.Caption = caption;
            btn.Hint = hint;
            btn.Tag = tag;
            btn.ButtonStyle = BarButtonStyle.Check;
            btn.PaintStyle = BarItemPaintStyle.Caption;
            btn.ItemClick += TimeframeButton_Click;
            return btn;
        }
        
        private BarButtonItem CreateToolButtonWithText(string symbol, string name, string hint)
        {
            var btn = new BarButtonItem();
            btn.Caption = symbol;
            btn.Hint = hint;
            btn.SuperTip = CreateSuperTip(name, hint);
            btn.ButtonStyle = BarButtonStyle.Check;
            btn.PaintStyle = BarItemPaintStyle.Caption;
            btn.Appearance.Font = new Font("Segoe UI", 11F);
            return btn;
        }

        private BarButtonItem CreateBarButton(string caption, string hint, bool isToggle)
        {
            var btn = new BarButtonItem();
            btn.Caption = caption;
            btn.Hint = hint;
            btn.PaintStyle = BarItemPaintStyle.CaptionGlyph;
            if (isToggle)
                btn.ButtonStyle = BarButtonStyle.Check;
            return btn;
        }

        private BarButtonItem CreateToolButton(string caption, string hint, DevExpress.Utils.Svg.SvgImage icon)
        {
            var btn = new BarButtonItem();
            btn.Caption = "";
            btn.Hint = hint;
            btn.SuperTip = CreateSuperTip(caption, hint);
            btn.ImageOptions.SvgImage = icon;
            btn.ImageOptions.SvgImageSize = new Size(20, 20);
            btn.ButtonStyle = BarButtonStyle.Check;
            btn.PaintStyle = BarItemPaintStyle.Standard;
            return btn;
        }

        private SuperToolTip CreateSuperTip(string title, string text)
        {
            var tip = new SuperToolTip();
            tip.Items.AddTitle(title);
            tip.Items.Add(text);
            return tip;
        }

        private void InitializeDockManager()
        {
            dockManager = new DockManager();
            dockManager.Form = this;
            dockManager.TopZIndexControls.AddRange(new string[] { "DevExpress.XtraBars.BarDockControl" });
        }
        #endregion

        #region Chart Area
        private void CreateChartArea()
        {
            pnlChartContainer = new PanelControl();
            pnlChartContainer.Dock = DockStyle.Fill;
            pnlChartContainer.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlChartContainer.Appearance.BackColor = Color.FromArgb(25, 25, 25);
            pnlChartContainer.Padding = new Padding(5);
            
            // Empty State Label - shown when no symbol selected
            lblChartEmpty = new LabelControl();
            lblChartEmpty.Text = "📈 Grafik görüntülemek için\nüstteki sembol kartlarından birini seçin\n\n(AAPL, MSFT, TSLA...)";
            lblChartEmpty.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Regular);
            lblChartEmpty.Appearance.ForeColor = Color.FromArgb(120, 120, 120);
            lblChartEmpty.AutoSizeMode = LabelAutoSizeMode.None;
            lblChartEmpty.Dock = DockStyle.Fill;
            lblChartEmpty.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            lblChartEmpty.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            lblChartEmpty.Appearance.TextOptions.WordWrap = WordWrap.Wrap;
            lblChartEmpty.Visible = true; // Show by default until symbol selected
            
            // Loading State Label
            lblChartLoading = new LabelControl();
            lblChartLoading.Text = "⏳ Grafik yükleniyor...";
            lblChartLoading.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Regular);
            lblChartLoading.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            lblChartLoading.AutoSizeMode = LabelAutoSizeMode.None;
            lblChartLoading.Dock = DockStyle.Fill;
            lblChartLoading.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            lblChartLoading.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            lblChartLoading.Visible = false;
            
            chartMain = new ChartControl();
            chartMain.Dock = DockStyle.Fill;
            chartMain.BackColor = Color.FromArgb(25, 25, 25);
            chartMain.Legend.Visibility = DefaultBoolean.False;
            chartMain.BorderOptions.Visibility = DefaultBoolean.False;
            
            // Crosshair
            chartMain.CrosshairEnabled = DefaultBoolean.True;
            chartMain.CrosshairOptions.ShowArgumentLine = true;
            chartMain.CrosshairOptions.ShowValueLine = true;
            chartMain.CrosshairOptions.ShowValueLabels = true;
            chartMain.CrosshairOptions.ArgumentLineColor = Color.Gold;
            chartMain.CrosshairOptions.ValueLineColor = Color.Gold;
            chartMain.CrosshairOptions.CrosshairLabelMode = CrosshairLabelMode.ShowForEachSeries;
            
            // Zoom/Pan
            chartMain.SelectionMode = ElementSelectionMode.Single;
            
            pnlChartContainer.Controls.Add(chartMain);
            pnlChartContainer.Controls.Add(lblChartEmpty);
            pnlChartContainer.Controls.Add(lblChartLoading);
            this.Controls.Add(pnlChartContainer);
        }

        private void CreateMarketTileBar()
        {
            tileBarMarket = new TileBar();
            tileBarMarket.Dock = DockStyle.Top;
            tileBarMarket.Height = 85;
            tileBarMarket.BackColor = Color.FromArgb(22, 22, 22);
            tileBarMarket.AllowDrag = false;
            tileBarMarket.AllowSelectedItem = true;
            tileBarMarket.AllowGlyphSkinning = true;
            tileBarMarket.ScrollMode = TileControlScrollMode.ScrollButtons;
            tileBarMarket.ItemSize = 130;
            tileBarMarket.IndentBetweenItems = 8;
            tileBarMarket.Padding = new Padding(15, 8, 15, 8);
            tileBarMarket.SelectedItemChanged += TileBarMarket_SelectedItemChanged;
            tileBarMarket.ItemClick += TileBarMarket_ItemClick;
            
            // Add market tiles
            var symbols = new[] { 
                ("AAPL", "Apple"), 
                ("MSFT", "Microsoft"), 
                ("GOOGL", "Google"), 
                ("TSLA", "Tesla"),
                ("AMZN", "Amazon"),
                ("NVDA", "NVIDIA"),
                ("META", "Meta"),
                ("AMD", "AMD")
            };
            
            var group = new TileBarGroup();
            tileBarMarket.Groups.Add(group);
            
            foreach (var (symbol, name) in symbols)
            {
                var tile = new TileBarItem();
                tile.Tag = symbol;
                tile.ItemSize = TileBarItemSize.Default;
                
                // Normal state
                tile.AppearanceItem.Normal.BackColor = Color.FromArgb(32, 32, 32);
                tile.AppearanceItem.Normal.BorderColor = Color.FromArgb(50, 50, 50);
                tile.AppearanceItem.Normal.ForeColor = Color.White;
                tile.AppearanceItem.Normal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                
                // Hover state
                tile.AppearanceItem.Hovered.BackColor = Color.FromArgb(45, 45, 45);
                tile.AppearanceItem.Hovered.BorderColor = Color.FromArgb(33, 150, 243);
                tile.AppearanceItem.Hovered.ForeColor = Color.White;
                
                // Selected state
                tile.AppearanceItem.Selected.BackColor = Color.FromArgb(33, 150, 243);
                tile.AppearanceItem.Selected.BorderColor = Color.FromArgb(33, 150, 243);
                tile.AppearanceItem.Selected.ForeColor = Color.White;
                
                var elem1 = new TileItemElement();
                elem1.Text = symbol;
                elem1.TextAlignment = TileItemContentAlignment.TopLeft;
                elem1.Appearance.Normal.ForeColor = Color.White;
                elem1.Appearance.Normal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                
                var elem2 = new TileItemElement();
                elem2.Text = "$--";
                elem2.TextAlignment = TileItemContentAlignment.MiddleLeft;
                elem2.Appearance.Normal.ForeColor = Color.White;
                elem2.Appearance.Normal.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
                
                var elem3 = new TileItemElement();
                elem3.Text = "--%";
                elem3.TextAlignment = TileItemContentAlignment.BottomLeft;
                elem3.Appearance.Normal.ForeColor = Color.FromArgb(0, 200, 83);
                elem3.Appearance.Normal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
                
                var elem4 = new TileItemElement();
                elem4.Text = name;
                elem4.TextAlignment = TileItemContentAlignment.BottomRight;
                elem4.Appearance.Normal.ForeColor = Color.FromArgb(120, 120, 120);
                elem4.Appearance.Normal.Font = new Font("Segoe UI", 7F);
                
                tile.Elements.Add(elem1);
                tile.Elements.Add(elem2);
                tile.Elements.Add(elem3);
                tile.Elements.Add(elem4);
                group.Items.Add(tile);
            }
            
            this.Controls.Add(tileBarMarket);
            tileBarMarket.BringToFront();
        }
        
        private void TileBarMarket_ItemClick(object sender, TileItemEventArgs e)
        {
            var symbol = e.Item?.Tag?.ToString();
            if (!string.IsNullOrEmpty(symbol))
            {
                SelectSymbol(symbol);
            }
        }
        #endregion

        #region Right Dock Panel
        private void CreateRightDockPanel()
        {
            dockPanelRight = dockManager.AddPanel(DockingStyle.Right);
            dockPanelRight.Text = "Piyasa & Emir";
            dockPanelRight.Width = 280;
            dockPanelRight.Options.AllowFloating = false;
            dockPanelRight.Options.ShowCloseButton = false;
            dockPanelRight.Appearance.BackColor = Color.FromArgb(22, 22, 22);
            
            rightContainer = dockPanelRight.ControlContainer;
            rightContainer.BackColor = Color.FromArgb(22, 22, 22);
            
            // Create sections
            var layoutPanel = new TableLayoutPanel();
            layoutPanel.Dock = DockStyle.Fill;
            layoutPanel.RowCount = 3;
            layoutPanel.ColumnCount = 1;
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45)); // Watchlist
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Symbol Details
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55)); // Order Ticket
            layoutPanel.BackColor = Color.Transparent;
            
            // Watchlist
            CreateWatchlistGrid();
            layoutPanel.Controls.Add(gridWatchlist, 0, 0);
            
            // Symbol Details
            CreateSymbolDetailsPanel();
            layoutPanel.Controls.Add(pnlSymbolDetails, 0, 1);
            
            // Order Ticket
            CreateOrderTicketPanel();
            layoutPanel.Controls.Add(pnlOrderTicket, 0, 2);
            
            rightContainer.Controls.Add(layoutPanel);
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
            gridViewWatchlist.OptionsView.RowAutoHeight = true;
            gridViewWatchlist.Appearance.HeaderPanel.BackColor = Color.FromArgb(30, 30, 30);
            gridViewWatchlist.Appearance.HeaderPanel.ForeColor = Color.White;
            gridViewWatchlist.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridViewWatchlist.Appearance.Row.BackColor = Color.FromArgb(22, 22, 22);
            gridViewWatchlist.Appearance.Row.ForeColor = Color.White;
            gridViewWatchlist.RowCellStyle += GridViewWatchlist_RowCellStyle;
            gridViewWatchlist.FocusedRowChanged += GridViewWatchlist_FocusedRowChanged;
        }

        private void CreateSymbolDetailsPanel()
        {
            pnlSymbolDetails = new PanelControl();
            pnlSymbolDetails.Dock = DockStyle.Fill;
            pnlSymbolDetails.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            pnlSymbolDetails.Appearance.BackColor = Color.FromArgb(28, 28, 28);
            pnlSymbolDetails.Appearance.BorderColor = Color.FromArgb(45, 45, 45);
            pnlSymbolDetails.Padding = new Padding(15, 10, 15, 10);
            
            // Section header
            var lblHeader = new LabelControl();
            lblHeader.Text = "SEÇİLİ SEMBOL";
            lblHeader.Appearance.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblHeader.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            lblHeader.Location = new Point(15, 8);
            
            lblSymbolName = new LabelControl();
            lblSymbolName.Text = "AAPL";
            lblSymbolName.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblSymbolName.Appearance.ForeColor = Color.White;
            lblSymbolName.Location = new Point(15, 28);
            
            lblSymbolPrice = new LabelControl();
            lblSymbolPrice.Text = "$0.00";
            lblSymbolPrice.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblSymbolPrice.Appearance.ForeColor = Color.White;
            lblSymbolPrice.Location = new Point(15, 55);
            
            lblSymbolChange = new LabelControl();
            lblSymbolChange.Text = "Yükleniyor...";
            lblSymbolChange.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSymbolChange.Appearance.ForeColor = Color.FromArgb(100, 100, 100);
            lblSymbolChange.Location = new Point(15, 88);
            
            pnlSymbolDetails.Controls.Add(lblHeader);
            pnlSymbolDetails.Controls.Add(lblSymbolName);
            pnlSymbolDetails.Controls.Add(lblSymbolPrice);
            pnlSymbolDetails.Controls.Add(lblSymbolChange);
        }

        private void CreateOrderTicketPanel()
        {
            pnlOrderTicket = new PanelControl();
            pnlOrderTicket.Dock = DockStyle.Fill;
            pnlOrderTicket.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlOrderTicket.Appearance.BackColor = Color.FromArgb(25, 25, 25);
            pnlOrderTicket.Padding = new Padding(12);
            
            var lblTitle = new LabelControl();
            lblTitle.Text = "Emir Girişi";
            lblTitle.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;
            lblTitle.Location = new Point(12, 8);
            
            var lblType = new LabelControl();
            lblType.Text = "Emir Tipi";
            lblType.Appearance.ForeColor = Color.Gray;
            lblType.Location = new Point(12, 38);
            
            cmbOrderType = new ComboBoxEdit();
            cmbOrderType.Location = new Point(12, 58);
            cmbOrderType.Size = new Size(230, 28);
            cmbOrderType.Properties.Items.AddRange(new[] { "Piyasa", "Limit", "Stop", "Stop-Limit" });
            cmbOrderType.SelectedIndex = 0;
            cmbOrderType.Properties.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            cmbOrderType.Properties.Appearance.ForeColor = Color.White;
            cmbOrderType.SelectedIndexChanged += CmbOrderType_SelectedIndexChanged;
            
            var lblQuantity = new LabelControl();
            lblQuantity.Text = "Miktar";
            lblQuantity.Appearance.ForeColor = Color.Gray;
            lblQuantity.Location = new Point(12, 95);
            
            txtOrderQuantity = new TextEdit();
            txtOrderQuantity.Location = new Point(12, 115);
            txtOrderQuantity.Size = new Size(110, 28);
            txtOrderQuantity.Properties.NullText = "0";
            txtOrderQuantity.Properties.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            txtOrderQuantity.Properties.Appearance.ForeColor = Color.White;
            
            var lblPrice = new LabelControl();
            lblPrice.Text = "Fiyat";
            lblPrice.Appearance.ForeColor = Color.Gray;
            lblPrice.Location = new Point(132, 95);
            
            txtOrderPrice = new TextEdit();
            txtOrderPrice.Location = new Point(132, 115);
            txtOrderPrice.Size = new Size(110, 28);
            txtOrderPrice.Properties.NullText = "Piyasa";
            txtOrderPrice.Properties.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            txtOrderPrice.Properties.Appearance.ForeColor = Color.White;
            
            btnBuy = new SimpleButton();
            btnBuy.Text = "AL";
            btnBuy.Location = new Point(12, 160);
            btnBuy.Size = new Size(110, 40);
            btnBuy.Appearance.BackColor = Color.FromArgb(0, 200, 83);
            btnBuy.Appearance.ForeColor = Color.White;
            btnBuy.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnBuy.Appearance.Options.UseBackColor = true;
            btnBuy.Appearance.Options.UseForeColor = true;
            btnBuy.Appearance.Options.UseFont = true;
            btnBuy.Click += BtnBuy_Click;
            
            btnSell = new SimpleButton();
            btnSell.Text = "SAT";
            btnSell.Location = new Point(132, 160);
            btnSell.Size = new Size(110, 40);
            btnSell.Appearance.BackColor = Color.FromArgb(255, 82, 82);
            btnSell.Appearance.ForeColor = Color.White;
            btnSell.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSell.Appearance.Options.UseBackColor = true;
            btnSell.Appearance.Options.UseForeColor = true;
            btnSell.Appearance.Options.UseFont = true;
            btnSell.Click += BtnSell_Click;
            
            // AI Assistant Button
            var btnAIAssist = new SimpleButton();
            btnAIAssist.Text = "🤖 AI Asistan";
            btnAIAssist.Location = new Point(12, 210);
            btnAIAssist.Size = new Size(230, 35);
            btnAIAssist.Appearance.BackColor = Color.FromArgb(88, 101, 242);
            btnAIAssist.Appearance.ForeColor = Color.White;
            btnAIAssist.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnAIAssist.Appearance.Options.UseBackColor = true;
            btnAIAssist.Appearance.Options.UseForeColor = true;
            btnAIAssist.Appearance.Options.UseFont = true;
            btnAIAssist.Click += (s, e) => OpenAIAssistant();
            
            pnlOrderTicket.Controls.Add(lblTitle);
            pnlOrderTicket.Controls.Add(lblType);
            pnlOrderTicket.Controls.Add(cmbOrderType);
            pnlOrderTicket.Controls.Add(lblQuantity);
            pnlOrderTicket.Controls.Add(txtOrderQuantity);
            pnlOrderTicket.Controls.Add(lblPrice);
            pnlOrderTicket.Controls.Add(txtOrderPrice);
            pnlOrderTicket.Controls.Add(btnBuy);
            pnlOrderTicket.Controls.Add(btnSell);
            pnlOrderTicket.Controls.Add(btnAIAssist);
        }
        
        private void OpenAIAssistant()
        {
            string context = $"{_currentSymbol} @ {lblSymbolPrice?.Text ?? "N/A"}";
            var aiForm = new Forms.AIAssistantForm(context);
            aiForm.ShowDialog();
        }
        #endregion

        #region Bottom Dock Panel
        private void CreateBottomDockPanel()
        {
            dockPanelBottom = dockManager.AddPanel(DockingStyle.Bottom);
            dockPanelBottom.Text = "İşlemler & Analiz";
            dockPanelBottom.Height = 250; // Compact height
            dockPanelBottom.Options.AllowFloating = false;
            dockPanelBottom.Options.ShowCloseButton = false;
            dockPanelBottom.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            _prevBottomHeight = 250; // Store for focus mode
            
            bottomContainer = dockPanelBottom.ControlContainer;
            bottomContainer.BackColor = Color.FromArgb(20, 20, 20);
            
            // XtraTabControl with improved styling
            tabBottom = new XtraTabControl();
            tabBottom.Dock = DockStyle.Fill;
            tabBottom.LookAndFeel.UseDefaultLookAndFeel = false;
            tabBottom.LookAndFeel.SkinName = "Office 2019 Black";
            tabBottom.AppearancePage.Header.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tabBottom.AppearancePage.Header.ForeColor = Color.White;
            tabBottom.AppearancePage.HeaderActive.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            tabBottom.AppearancePage.HeaderActive.ForeColor = Color.FromArgb(33, 150, 243);
            tabBottom.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            tabBottom.BorderStylePage = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            
            // Tab: Emirler
            tabOrders = new XtraTabPage();
            tabOrders.Text = "Emirlerim";
            CreateOrdersGrid(tabOrders);
            tabBottom.TabPages.Add(tabOrders);
            
            // Tab: Pozisyonlar
            tabPositions = new XtraTabPage();
            tabPositions.Text = "Pozisyonlar";
            CreatePositionsGrid(tabPositions);
            tabBottom.TabPages.Add(tabPositions);
            
            // Tab: Haberler
            tabNews = new XtraTabPage();
            tabNews.Text = "Haberler";
            CreateNewsPanel(tabNews);
            tabBottom.TabPages.Add(tabNews);
            
            // Tab: AI Analiz
            tabAnalysis = new XtraTabPage();
            tabAnalysis.Text = "AI Analiz";
            CreateAnalysisPanel(tabAnalysis);
            tabBottom.TabPages.Add(tabAnalysis);
            
            // Tab: Trade Terminal
            tabTradeTerminal = new XtraTabPage();
            tabTradeTerminal.Text = "Trade Terminal";
            CreateTradeTerminalTab(tabTradeTerminal);
            tabBottom.TabPages.Add(tabTradeTerminal);
            
            bottomContainer.Controls.Add(tabBottom);
        }

        private void CreateOrdersGrid(XtraTabPage tab)
        {
            tab.Padding = new Padding(5);
            
            gridOrders = new GridControl();
            gridOrders.Dock = DockStyle.Fill;
            gridOrders.LookAndFeel.UseDefaultLookAndFeel = false;
            gridOrders.LookAndFeel.SkinName = "Office 2019 Black";
            
            var view = new GridView(gridOrders);
            gridOrders.MainView = view;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.OptionsView.ColumnAutoWidth = true;
            view.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            view.Appearance.Row.Font = new Font("Segoe UI", 9F);
            view.Appearance.Empty.BackColor = Color.FromArgb(22, 22, 22);
            view.Appearance.Empty.ForeColor = Color.Gray;
            view.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            view.RowHeight = 28;
            
            // Mock data
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Tarih");
            dt.Columns.Add("Sembol");
            dt.Columns.Add("Tip");
            dt.Columns.Add("Fiyat");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Durum");
            gridOrders.DataSource = dt;
            
            tab.Controls.Add(gridOrders);
        }

        private void CreatePositionsGrid(XtraTabPage tab)
        {
            tab.Padding = new Padding(5);
            
            gridPositions = new GridControl();
            gridPositions.Dock = DockStyle.Fill;
            gridPositions.LookAndFeel.UseDefaultLookAndFeel = false;
            gridPositions.LookAndFeel.SkinName = "Office 2019 Black";
            
            var view = new GridView(gridPositions);
            gridPositions.MainView = view;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsView.ShowIndicator = false;
            view.OptionsView.ColumnAutoWidth = true;
            view.Appearance.HeaderPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            view.Appearance.Row.Font = new Font("Segoe UI", 9F);
            view.Appearance.Empty.BackColor = Color.FromArgb(22, 22, 22);
            view.Appearance.Empty.ForeColor = Color.Gray;
            view.OptionsView.ShowFilterPanelMode = DevExpress.XtraGrid.Views.Base.ShowFilterPanelMode.Never;
            view.RowHeight = 28;
            
            // Mock data - will be populated from portfolio
            var dt = new System.Data.DataTable();
            dt.Columns.Add("Sembol");
            dt.Columns.Add("Miktar");
            dt.Columns.Add("Ort. Maliyet");
            dt.Columns.Add("Son Fiyat");
            dt.Columns.Add("Kar/Zarar");
            gridPositions.DataSource = dt;
            
            tab.Controls.Add(gridPositions);
        }

        private void CreateNewsPanel(XtraTabPage tab)
        {
            memoNews = new MemoEdit();
            memoNews.Dock = DockStyle.Fill;
            memoNews.Properties.ReadOnly = true;
            memoNews.Properties.ScrollBars = ScrollBars.Vertical;
            memoNews.Properties.Appearance.BackColor = Color.FromArgb(22, 22, 22);
            memoNews.Properties.Appearance.ForeColor = Color.LightGray;
            memoNews.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            memoNews.Text = "Haberler yükleniyor...";
            tab.Controls.Add(memoNews);
        }

        private void CreateAnalysisPanel(XtraTabPage tab)
        {
            var pnlAI = new PanelControl();
            pnlAI.Dock = DockStyle.Fill;
            pnlAI.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlAI.Appearance.BackColor = Color.FromArgb(22, 22, 22);
            
            var btnAnalyze = new SimpleButton();
            btnAnalyze.Text = "AI Analiz Başlat";
            btnAnalyze.Location = new Point(10, 10);
            btnAnalyze.Size = new Size(150, 35);
            btnAnalyze.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            btnAnalyze.Appearance.ForeColor = Color.White;
            btnAnalyze.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAnalyze.Click += async (s, e) => await RunAIAnalysisAsync();
            
            memoAnalysis = new MemoEdit();
            memoAnalysis.Location = new Point(10, 55);
            memoAnalysis.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            memoAnalysis.Size = new Size(800, 100);
            memoAnalysis.Properties.ReadOnly = true;
            memoAnalysis.Properties.ScrollBars = ScrollBars.Vertical;
            memoAnalysis.Properties.Appearance.BackColor = Color.FromArgb(28, 28, 28);
            memoAnalysis.Properties.Appearance.ForeColor = Color.LightGray;
            memoAnalysis.Properties.Appearance.Font = new Font("Consolas", 10F);
            memoAnalysis.Text = "AI analizi başlatmak için butona tıklayın.\n\nAnaliz içeriği:\n- Piyasa özeti\n- Teknik görünüm\n- Destek/Direnç seviyeleri\n- Risk notları\n- Alarm önerileri";
            
            pnlAI.Controls.Add(btnAnalyze);
            pnlAI.Controls.Add(memoAnalysis);
            tab.Controls.Add(pnlAI);
        }

        private void CreateTradeTerminalTab(XtraTabPage tab)
        {
            var lblInfo = new LabelControl();
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.AutoSizeMode = LabelAutoSizeMode.None;
            lblInfo.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            lblInfo.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblInfo.Appearance.Font = new Font("Segoe UI", 12F);
            lblInfo.Appearance.ForeColor = Color.Gray;
            lblInfo.Text = "Trade Terminal bu sekmeye entegre edilecek.\nMevcut TradeTerminalForm içeriği buraya taşınacak.";
            tab.Controls.Add(lblInfo);
        }
        #endregion

        #region Event Handlers
        private void BarSymbolSearch_EditValueChanged(object sender, EventArgs e)
        {
            var value = barSymbolSearch.EditValue?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                _currentSymbol = value.ToUpper();
                _ = LoadChartDataAsync(_currentSymbol, _currentTimeframe);
            }
        }

        private void TimeframeButton_Click(object sender, ItemClickEventArgs e)
        {
            // Uncheck all timeframes
            barTimeframe1m.Down = false;
            barTimeframe5m.Down = false;
            barTimeframe15m.Down = false;
            barTimeframe1H.Down = false;
            barTimeframe4H.Down = false;
            barTimeframe1D.Down = false;
            barTimeframe1W.Down = false;
            
            // Check clicked
            ((BarButtonItem)sender).Down = true;
            _currentTimeframe = e.Item.Tag?.ToString() ?? "D";
            _ = LoadChartDataAsync(_currentSymbol, _currentTimeframe);
        }

        private void TileBarMarket_SelectedItemChanged(object sender, TileItemEventArgs e)
        {
            var symbol = e.Item?.Tag?.ToString();
            if (!string.IsNullOrEmpty(symbol))
            {
                SelectSymbol(symbol);
            }
        }
        
        /// <summary>
        /// Sembol seçimi - tüm UI ve veri yüklemelerini tetikler
        /// </summary>
        private async void SelectSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol) || symbol == _currentSymbol && chartMain.Series.Count > 0)
                return;
                
            _currentSymbol = symbol;
            
            // Show loading state
            ShowChartLoading(true);
            
            // Load quote data first for immediate UI update
            await LoadSymbolQuoteAsync(symbol);
            
            // Then load chart data
            await LoadChartDataAsync(symbol, _currentTimeframe);
            
            // Hide loading
            ShowChartLoading(false);
        }
        
        private void ShowChartLoading(bool isLoading)
        {
            _isChartLoading = isLoading;
            lblChartLoading.Visible = isLoading;
            chartMain.Visible = !isLoading;
        }
        
        private void ShowChartEmpty(bool isEmpty)
        {
            lblChartEmpty.Visible = isEmpty;
            chartMain.Visible = !isEmpty;
        }

        private void GridViewWatchlist_RowCellStyle(object sender, RowCellStyleEventArgs e)
        {
            if (e.Column.FieldName == "Change")
            {
                var value = gridViewWatchlist.GetRowCellValue(e.RowHandle, "Change")?.ToString();
                if (value != null && value.StartsWith("+"))
                    e.Appearance.ForeColor = Color.FromArgb(0, 200, 83);
                else if (value != null && value.StartsWith("-"))
                    e.Appearance.ForeColor = Color.FromArgb(255, 82, 82);
            }
        }

        private void GridViewWatchlist_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle >= 0)
            {
                var symbol = gridViewWatchlist.GetRowCellValue(e.FocusedRowHandle, "Symbol")?.ToString();
                if (!string.IsNullOrEmpty(symbol))
                {
                    SelectSymbol(symbol);
                }
            }
        }

        private async void BtnBuy_Click(object sender, EventArgs e)
        {
            // [CALL] ZORUNLU FORMAT
            System.Diagnostics.Debug.WriteLine($"[CALL] btnBuy.Click -> BtnBuy_Click | senderType={sender?.GetType().Name} | senderHash={sender?.GetHashCode()} | formHash={this.GetHashCode()} | viewType={GetType().FullName}");
            await ExecuteTradeAsync(isBuy: true);
        }

        private async void BtnSell_Click(object sender, EventArgs e)
        {
            // [CALL] ZORUNLU FORMAT
            System.Diagnostics.Debug.WriteLine($"[CALL] btnSell.Click -> BtnSell_Click | senderType={sender?.GetType().Name} | senderHash={sender?.GetHashCode()} | formHash={this.GetHashCode()} | viewType={GetType().FullName}");
            await ExecuteTradeAsync(isBuy: false);
        }
        
        /// <summary>
        /// AL/SAT işlemini gerçekleştirir ve dashboard'u günceller
        /// </summary>
        private async Task ExecuteTradeAsync(bool isBuy)
        {
            // [CRITICAL] TradeStart - ZORUNLU
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] TradeStart viewType={GetType().FullName} viewHash={GetHashCode()} isBuy={isBuy} symbol={_currentSymbol} userId={AppEvents.CurrentSession.UserId}");
            
            // C) Yanlış view tespit - Bu view gerçek trade yapıyor mu?
            bool isRealTradeView = this.GetType().FullName.Contains("InvestmentView");
            if (!isRealTradeView)
            {
                System.Diagnostics.Debug.WriteLine($"[WARN] Yanlış view aktif: {GetType().FullName}. Trade DB'ye yazmayabilir.");
                ShowToast("Uyarı", "Yanlış view aktif! Lütfen Yatırım sekmesinden işlem yapın.", isError: true);
            }
            System.Diagnostics.Debug.WriteLine($"[TRADE] ExecuteTradeAsync START - isBuy={isBuy}, symbol={_currentSymbol}, qtyText={txtOrderQuantity.Text}");
            
            // Double-click prevention
            if (_isTrading)
            {
                System.Diagnostics.Debug.WriteLine("[TRADE] Already trading, skipping");
                return;
            }
            
            try
            {
                // Validate quantity first (before locking)
                if (!decimal.TryParse(txtOrderQuantity.Text, out decimal quantity) || quantity <= 0)
                {
                    ShowToast("Uyarı", "Geçerli bir miktar giriniz.", isError: true);
                    System.Diagnostics.Debug.WriteLine("[TRADE] Invalid quantity");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[TRADE] Quantity parsed: {quantity}");
                
                // Lock trading
                _isTrading = true;
                SetTradingUIState(isLoading: true);
                
                // Get current price (from symbol details or use a default)
                decimal price = 0;
                if (decimal.TryParse(lblSymbolPrice.Text.Replace("$", "").Replace(",", ""), out decimal parsedPrice))
                {
                    price = parsedPrice;
                }
                else
                {
                    price = 100m; // Fallback price
                }
                
                // S3 KUR FIX: USD fiyatı TRY'ye çevir
                string symbolCurrency = CurrencyConversionService.GetCurrencyForSymbol(_currentSymbol);
                decimal totalAmountTry = CurrencyConversionService.GetTryValue(_currentSymbol, price, quantity);
                
                System.Diagnostics.Debug.WriteLine($"[DATA] MoneyConvert symbol={_currentSymbol} qty={quantity} priceUsd={price:N2} currency={symbolCurrency} totalTry={totalAmountTry:N2}");
                
                // Get user's primary account (UserId is used as CustomerId)
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Trade START - UserId={AppEvents.CurrentSession.UserId}");
                var accounts = await _accountRepository.GetByCustomerIdAsync(AppEvents.CurrentSession.UserId);
                var primaryAccount = accounts?.FirstOrDefault();
                
                if (primaryAccount == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] ERROR: No account found for UserId={AppEvents.CurrentSession.UserId}");
                    ShowToast("Hata", "Hesap bulunamadı.", isError: true);
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[CRITICAL] Trade - AccountId={primaryAccount.Id}, CustomerId={primaryAccount.CustomerId}, UserId={AppEvents.CurrentSession.UserId}");
                
                string result;
                string actionType;
                
                if (isBuy)
                {
                    // AL: Bakiyeden düş (Withdraw) - TRY cinsinden
                    result = await _transactionService.WithdrawAsync(
                        primaryAccount.Id, 
                        totalAmountTry, 
                        $"Yatırım AL: {quantity} adet {_currentSymbol} @ ${price:N2} (Kur: {CurrencyConversionService.UsdTryRate:N2})");
                    actionType = "StockBuy";
                    
                    // Update portfolio position - TRY maliyet olarak kaydet
                    if (result == null)
                    {
                        decimal priceTry = CurrencyConversionService.ConvertToTry(price, symbolCurrency);
                        System.Diagnostics.Debug.WriteLine($"[CRITICAL] BuyAsync - CustomerId={primaryAccount.CustomerId}, Symbol={_currentSymbol}, Qty={quantity}, PriceTRY={priceTry:N2}");
                        await _portfolioRepository.BuyAsync(primaryAccount.CustomerId, _currentSymbol, quantity, priceTry);
                    }
                }
                else
                {
                    // SAT: Önce pozisyon kontrolü
                    System.Diagnostics.Debug.WriteLine($"[CRITICAL] SellAsync - CustomerId={primaryAccount.CustomerId}, Symbol={_currentSymbol}, Qty={quantity}");
                    var canSell = await _portfolioRepository.SellAsync(primaryAccount.CustomerId, _currentSymbol, quantity);
                    if (!canSell)
                    {
                        System.Diagnostics.Debug.WriteLine($"[CRITICAL] SELL FAILED: Insufficient position for {_currentSymbol}");
                        ShowToast("Hata", $"Yetersiz {_currentSymbol} pozisyonu.", isError: true);
                        return;
                    }
                    
                    // SAT: Bakiyeye ekle (Deposit) - TRY cinsinden
                    result = await _transactionService.DepositAsync(
                        primaryAccount.Id, 
                        totalAmountTry, 
                        $"Yatırım SAT: {quantity} adet {_currentSymbol} @ ${price:N2} (Kur: {CurrencyConversionService.UsdTryRate:N2})");
                    actionType = "StockSell";
                }
                
                if (result == null) // Success
                {
                    System.Diagnostics.Debug.WriteLine($"[TRADE] SUCCESS - {actionType} completed");
                    
                    decimal currentBalance = 0;
                    
                    // DB Verification - READ-ONLY CHECK
                    try
                    {
                        using var conn = new DapperContext().CreateConnection();
                        
                        // Balance verification
                        currentBalance = await conn.ExecuteScalarAsync<decimal>(
                            "SELECT \"Balance\" FROM \"Accounts\" WHERE \"Id\" = @AccId", 
                            new { AccId = primaryAccount.Id });
                        
                        // Last transaction details
                        var lastTx = await conn.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT \"Description\", \"Amount\", \"TransactionDate\" FROM \"Transactions\" WHERE \"AccountId\" = @AccId ORDER BY \"TransactionDate\" DESC LIMIT 1",
                            new { AccId = primaryAccount.Id });
                        
                        // Portfolio position for this symbol
                        var portfolioPosition = await conn.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT \"Quantity\", \"AverageCost\" FROM \"CustomerPortfolios\" WHERE \"CustomerId\" = @CustId AND \"StockSymbol\" = @Symbol",
                            new { CustId = primaryAccount.CustomerId, Symbol = _currentSymbol });
                        
                        // Total portfolio positions
                        var portfolioCount = await conn.ExecuteScalarAsync<int>(
                            "SELECT COUNT(*) FROM \"CustomerPortfolios\" WHERE \"CustomerId\" = @CustId", 
                            new { CustId = primaryAccount.CustomerId });
                        
                        System.Diagnostics.Debug.WriteLine($"[DB] ===== POST-TRADE VERIFICATION =====");
                        System.Diagnostics.Debug.WriteLine($"[DB] Action: {actionType}");
                        System.Diagnostics.Debug.WriteLine($"[DB] BalanceAfter=₺{currentBalance:N2}");
                        if (lastTx != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[DB] LastTx: {lastTx.Description} | Amount=₺{lastTx.Amount:N2} | Date={lastTx.TransactionDate:yyyy-MM-dd HH:mm:ss}");
                        }
                        if (portfolioPosition != null)
                        {
                            decimal qty = portfolioPosition.Quantity;
                            decimal avgCost = portfolioPosition.AverageCost;
                            System.Diagnostics.Debug.WriteLine($"[DB] PortfolioAfter: {_currentSymbol} qty={qty:N2} avgCost=₺{avgCost:N2} totalValue=₺{(qty * avgCost):N2}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[DB] PortfolioAfter: {_currentSymbol} - NO POSITION (sold all or never bought)");
                        }
                        System.Diagnostics.Debug.WriteLine($"[DB] Total Portfolio Positions: {portfolioCount}");
                        System.Diagnostics.Debug.WriteLine($"[DB] ===============================");
                    }
                    catch (Exception dbEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DB-VERIFY] ERROR: {dbEx.Message}");
                    }
                    
                    // Update orders grid with new entry
                    AddOrderToGrid(isBuy ? "AL" : "SAT", _currentSymbol, price, quantity);
                    
                    // Refresh positions grid
                    await LoadPositionsDataAsync();
                    
                    // B3: TEK RefreshPipeline - NotifyTradeCompleted
                    AppEvents.NotifyTradeCompleted(
                        AppEvents.CurrentSession.ActiveAccountId,
                        AppEvents.CurrentSession.CustomerId,
                        _currentSymbol,
                        totalAmountTry,
                        isBuy);
                    
                    // Legacy events (opsiyonel - yedek olarak)
                    PortfolioEvents.OnPortfolioChanged(AppEvents.CurrentSession.UserId, "Trade");
                    
                    // Show success toast - TRY cinsinden göster
                    string tradeType = isBuy ? "AL" : "SAT";
                    ShowToast(
                        $"✓ {tradeType} Emri Gerçekleşti",
                        $"{_currentSymbol} • {quantity} adet • ₺{totalAmountTry:N2}",
                        isError: false);
                    
                    // Clear quantity field
                    txtOrderQuantity.Text = "";
                    
                    System.Diagnostics.Debug.WriteLine($"[TRADE] ExecuteTradeAsync END - SUCCESS");
                }
                else
                {
                    // Show error toast with user-friendly message
                    ShowToast("İşlem Başarısız", result, isError: true);
                }
            }
            catch (Exception ex)
            {
                // Show error toast without stack trace
                string friendlyMessage = ex.Message.Contains("Yetersiz") 
                    ? "Yetersiz bakiye. Lütfen hesabınızı kontrol edin."
                    : "İşlem sırasında bir hata oluştu. Lütfen tekrar deneyin.";
                ShowToast("Hata", friendlyMessage, isError: true);
                System.Diagnostics.Debug.WriteLine($"Trade Error: {ex}");
            }
            finally
            {
                // Always unlock trading and reset UI
                _isTrading = false;
                SetTradingUIState(isLoading: false);
            }
        }
        
        /// <summary>
        /// Sets the UI state during trading (loading/idle)
        /// </summary>
        private void SetTradingUIState(bool isLoading)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetTradingUIState(isLoading)));
                return;
            }
            
            // Disable/Enable buttons
            btnBuy.Enabled = !isLoading;
            btnSell.Enabled = !isLoading;
            
            // Cursor
            Cursor.Current = isLoading ? Cursors.WaitCursor : Cursors.Default;
            
            // Update button text to show loading state
            if (isLoading)
            {
                btnBuy.Text = "...";
                btnSell.Text = "...";
            }
            else
            {
                btnBuy.Text = "AL";
                btnSell.Text = "SAT";
            }
        }
        
        /// <summary>
        /// Shows a DevExpress toast notification
        /// </summary>
        private void ShowToast(string caption, string message, bool isError)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => ShowToast(caption, message, isError)));
                return;
            }
            
            try
            {
                var alertInfo = new AlertInfo(caption, message);
                
                // Find parent form for alert
                var parentForm = this.FindForm();
                if (parentForm != null)
                {
                    alertControl.Show(parentForm, alertInfo);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Toast Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Emirler grid'ine yeni satır ekler
        /// </summary>
        private void AddOrderToGrid(string type, string symbol, decimal price, decimal quantity)
        {
            try
            {
                if (gridOrders?.DataSource is System.Data.DataTable dt)
                {
                    dt.Rows.Add(
                        DateTime.Now.ToString("dd.MM HH:mm"),
                        symbol,
                        type,
                        $"${price:N2}",
                        quantity.ToString(),
                        "Gerçekleşti"
                    );
                }
            }
            catch { /* Grid update failed, not critical */ }
        }
        #endregion

        #region S5 - Emir Tipi Mantığı
        /// <summary>
        /// S5: Emir tipi değişince fiyat alanlarını aktif/pasif yap
        /// </summary>
        private void CmbOrderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var orderType = cmbOrderType.SelectedItem?.ToString() ?? "Piyasa";
            System.Diagnostics.Debug.WriteLine($"[CALL] OrderType changed to: {orderType}");
            
            switch (orderType)
            {
                case "Piyasa":
                    // Market: Fiyat alanı pasif, anlık fiyatla işlem
                    txtOrderPrice.Enabled = false;
                    txtOrderPrice.Text = "";
                    txtOrderPrice.Properties.NullText = "Piyasa Fiyatı";
                    break;
                    
                case "Limit":
                    // Limit: Limit fiyat alanı aktif
                    txtOrderPrice.Enabled = true;
                    txtOrderPrice.Text = "";
                    txtOrderPrice.Properties.NullText = "Limit Fiyat (TRY)";
                    break;
                    
                case "Stop":
                    // Stop: Stop fiyat alanı aktif
                    txtOrderPrice.Enabled = true;
                    txtOrderPrice.Text = "";
                    txtOrderPrice.Properties.NullText = "Stop Fiyat (TRY)";
                    break;
                    
                case "Stop-Limit":
                    // Stop-Limit: Her iki fiyat alanı da aktif (basitleştirilmiş - tek alan)
                    txtOrderPrice.Enabled = true;
                    txtOrderPrice.Text = "";
                    txtOrderPrice.Properties.NullText = "Stop-Limit Fiyat (TRY)";
                    break;
            }
        }
        
        /// <summary>
        /// S5: Emir tipine göre validasyon
        /// </summary>
        private bool ValidateOrderByType(out string errorMessage)
        {
            errorMessage = null;
            var orderType = cmbOrderType.SelectedItem?.ToString() ?? "Piyasa";
            
            // Miktar kontrolü
            if (!decimal.TryParse(txtOrderQuantity.Text, out decimal qty) || qty <= 0)
            {
                errorMessage = "Geçerli bir miktar giriniz.";
                return false;
            }
            
            // Emir tipine göre fiyat kontrolü
            if (orderType != "Piyasa")
            {
                if (string.IsNullOrWhiteSpace(txtOrderPrice.Text) || 
                    !decimal.TryParse(txtOrderPrice.Text, out decimal limitPrice) || 
                    limitPrice <= 0)
                {
                    errorMessage = $"{orderType} emri için geçerli bir fiyat giriniz.";
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// S5: Emir tipine göre işlem - Limit/Stop emirleri Açık Emirler'e eklenir
        /// </summary>
        private bool IsMarketOrder()
        {
            var orderType = cmbOrderType.SelectedItem?.ToString() ?? "Piyasa";
            return orderType == "Piyasa";
        }
        #endregion

        #region Deprecated - Old Click Handlers
        private void BtnBuy_Click_Old(object sender, EventArgs e)
        {
            var qty = txtOrderQuantity.Text;
            XtraMessageBox.Show($"{_currentSymbol} için {qty} adet AL emri verildi!", 
                "Emir", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnSell_Click_Old(object sender, EventArgs e)
        {
            var qty = txtOrderQuantity.Text;
            XtraMessageBox.Show($"{_currentSymbol} için {qty} adet SAT emri verildi!", 
                "Emir", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Drawing Tools
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
                    diagram.AxisY.ConstantLines.Add(line);
                }
            }
        }

        private void AddFibonacciLevels()
        {
            if (chartMain.Diagram is XYDiagram diagram)
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

        private void ClearAllDrawings()
        {
            chartMain.AnnotationRepository.Clear();
            if (chartMain.Diagram is XYDiagram diagram)
                diagram.AxisY.ConstantLines.Clear();
            _supportResistanceLevels.Clear();
            _fibonacciLevels.Clear();
        }
        
        private void UndoLastDrawing()
        {
            if (chartMain.Diagram is XYDiagram diagram && diagram.AxisY.ConstantLines.Count > 0)
            {
                diagram.AxisY.ConstantLines.RemoveAt(diagram.AxisY.ConstantLines.Count - 1);
            }
        }
        
        private void AddHorizontalLine()
        {
            string input = ShowInputDialog("Yatay Çizgi Seviyesi:", $"{_lastC:F2}");
            if (decimal.TryParse(input, out decimal price))
            {
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    var line = new ConstantLine($"${price:N2}");
                    line.AxisValue = price;
                    line.Color = Color.FromArgb(33, 150, 243);
                    line.LineStyle.Thickness = 2;
                    line.ShowInLegend = false;
                    line.Title.TextColor = Color.FromArgb(33, 150, 243);
                    diagram.AxisY.ConstantLines.Add(line);
                }
            }
        }
        
        private void AddTextNote()
        {
            string text = ShowInputDialog("Not Metni:", "");
            if (!string.IsNullOrEmpty(text))
            {
                ShowToast("Not Eklendi", text, false);
            }
        }
        
        private void RunTriggerPointAnalysis()
        {
            // Calculate trigger points based on recent price action
            if (_lastH > 0 && _lastL > 0)
            {
                double pivot = (_lastH + _lastL + _lastC) / 3;
                double r1 = 2 * pivot - _lastL;
                double s1 = 2 * pivot - _lastH;
                double r2 = pivot + (_lastH - _lastL);
                double s2 = pivot - (_lastH - _lastL);
                
                // Add to chart
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    diagram.AxisY.ConstantLines.Clear();
                    AddConstantLine(diagram, "Pivot", pivot, Color.FromArgb(255, 193, 7));
                    AddConstantLine(diagram, "R1", r1, Color.FromArgb(255, 82, 82));
                    AddConstantLine(diagram, "R2", r2, Color.FromArgb(255, 82, 82));
                    AddConstantLine(diagram, "S1", s1, Color.FromArgb(0, 200, 83));
                    AddConstantLine(diagram, "S2", s2, Color.FromArgb(0, 200, 83));
                }
                
                // Store for analysis tab
                _supportResistanceLevels.Clear();
                _supportResistanceLevels.Add(("Pivot", pivot));
                _supportResistanceLevels.Add(("R1", r1));
                _supportResistanceLevels.Add(("R2", r2));
                _supportResistanceLevels.Add(("S1", s1));
                _supportResistanceLevels.Add(("S2", s2));
                
                UpdateAnalysisTab();
                ShowToast("TriggerPoint", "Pivot noktaları hesaplandı ve grafiğe eklendi.", false);
            }
        }
        
        private void RunAutoSupportResistance()
        {
            // Simple S/R based on recent high/low
            if (_lastH > 0 && _lastL > 0)
            {
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    AddConstantLine(diagram, "Direnç", _lastH, Color.FromArgb(255, 82, 82));
                    AddConstantLine(diagram, "Destek", _lastL, Color.FromArgb(0, 200, 83));
                }
                
                _supportResistanceLevels.Add(("Direnç (H)", _lastH));
                _supportResistanceLevels.Add(("Destek (L)", _lastL));
                UpdateAnalysisTab();
                ShowToast("Auto S/R", "Destek ve direnç seviyeleri eklendi.", false);
            }
        }
        
        private void RunAutoFibonacci()
        {
            if (_lastH > 0 && _lastL > 0)
            {
                double diff = _lastH - _lastL;
                double[] fibs = { 0, 0.236, 0.382, 0.5, 0.618, 0.786, 1.0 };
                
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    foreach (var f in fibs)
                    {
                        double level = _lastL + (diff * f);
                        var line = new ConstantLine($"Fib {f:P1}");
                        line.AxisValue = level;
                        line.Color = Color.FromArgb(100, 255, 215, 0);
                        line.LineStyle.Thickness = 1;
                        line.ShowInLegend = false;
                        line.Title.TextColor = Color.Gold;
                        diagram.AxisY.ConstantLines.Add(line);
                        
                        _fibonacciLevels.Add(($"Fib {f:P0}", level));
                    }
                }
                
                UpdateAnalysisTab();
                ShowToast("Auto Fibonacci", "Fibonacci seviyeleri hesaplandı.", false);
            }
        }
        
        private void AddConstantLine(XYDiagram diagram, string name, double value, Color color)
        {
            var line = new ConstantLine($"{name}: ${value:N2}");
            line.AxisValue = value;
            line.Color = color;
            line.LineStyle.Thickness = 2;
            line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
            line.ShowInLegend = false;
            line.Title.TextColor = color;
            diagram.AxisY.ConstantLines.Add(line);
        }
        
        private void UpdateAnalysisTab()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"═══════════════════════════════════════");
            sb.AppendLine($"TEKNİK ANALİZ - {_currentSymbol}");
            sb.AppendLine($"═══════════════════════════════════════");
            sb.AppendLine();
            sb.AppendLine($"📊 OHLC: O:{_lastO:F2} H:{_lastH:F2} L:{_lastL:F2} C:{_lastC:F2}");
            sb.AppendLine($"📈 Değişim: {_lastChangePercent:+0.00;-0.00}%");
            sb.AppendLine();
            
            if (_supportResistanceLevels.Count > 0)
            {
                sb.AppendLine("🎯 DESTEK/DİRENÇ SEVİYELERİ:");
                foreach (var (name, value) in _supportResistanceLevels)
                {
                    sb.AppendLine($"  • {name}: ${value:N2}");
                }
                sb.AppendLine();
            }
            
            if (_fibonacciLevels.Count > 0)
            {
                sb.AppendLine("📐 FİBONACCİ SEVİYELERİ:");
                foreach (var (name, value) in _fibonacciLevels)
                {
                    sb.AppendLine($"  • {name}: ${value:N2}");
                }
                sb.AppendLine();
            }
            
            sb.AppendLine("💡 \"Grafiğe Uygula\" için sol araç çubuğunu kullanın.");
            
            if (memoAnalysis != null)
            {
                memoAnalysis.Text = sb.ToString();
            }
        }
        
        private void ShowIndicatorsMenu()
        {
            // Simple indicator toggle menu
            var result = XtraMessageBox.Show(
                "MA (Hareketli Ortalama) göstergesini eklemek ister misiniz?",
                "Göstergeler",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                // MA indicator would be added here
                ShowToast("Gösterge", "MA göstergesi eklendi.", false);
            }
        }
        
        private async Task RefreshCurrentSymbolAsync()
        {
            ShowChartLoading(true);
            await LoadSymbolQuoteAsync(_currentSymbol);
            await LoadChartDataAsync(_currentSymbol, _currentTimeframe);
            ShowChartLoading(false);
            ShowToast("Yenilendi", $"{_currentSymbol} verileri güncellendi.", false);
        }
        
        /// <summary>
        /// Toggle Focus Mode - collapse/expand bottom panel to maximize chart area
        /// </summary>
        private void ToggleFocusMode()
        {
            _isFocusMode = !_isFocusMode;
            
            if (_isFocusMode)
            {
                // Store current sizes
                _prevBottomHeight = dockPanelBottom.Height;
                
                // Collapse bottom panel
                dockPanelBottom.Visibility = DockVisibility.Hidden;
                
                // Update button to show "exit focus" state
                barFullscreen.Caption = "⊟";
                barFullscreen.Hint = "Focus Mode Kapat";
            }
            else
            {
                // Restore bottom panel
                dockPanelBottom.Visibility = DockVisibility.Visible;
                dockPanelBottom.Height = _prevBottomHeight;
                
                // Update button
                barFullscreen.Caption = "⛶";
                barFullscreen.Hint = "Focus Mode (Chart Büyüt)";
            }
        }
        
        /// <summary>
        /// Set the active drawing mode and update tool button states
        /// </summary>
        private void SetDrawMode(DrawMode mode)
        {
            _activeDrawMode = mode;
            
            // Update all tool button states
            barToolSelect.Down = (mode == DrawMode.Select || mode == DrawMode.None);
            barToolCrosshair.Down = (mode == DrawMode.Crosshair);
            barToolTrendline.Down = (mode == DrawMode.Trendline);
            barToolHorizontal.Down = (mode == DrawMode.HorizontalLine);
            barToolRay.Down = (mode == DrawMode.Ray);
            barToolFibonacci.Down = (mode == DrawMode.Fib);
            barToolRectangle.Down = (mode == DrawMode.Rectangle);
            barToolText.Down = (mode == DrawMode.Text);
            
            // Update crosshair on chart
            if (mode == DrawMode.Crosshair)
            {
                chartMain.CrosshairEnabled = DefaultBoolean.True;
            }
            else if (mode == DrawMode.None || mode == DrawMode.Select)
            {
                chartMain.CrosshairEnabled = DefaultBoolean.False;
            }
            
            // Update cursor based on mode
            if (mode == DrawMode.Trendline || mode == DrawMode.HorizontalLine || mode == DrawMode.Ray || mode == DrawMode.Fib)
            {
                pnlChartContainer.Cursor = Cursors.Cross;
            }
            else
            {
                pnlChartContainer.Cursor = Cursors.Default;
            }
        }

        private string ShowInputDialog(string prompt, string defaultValue)
        {
            using (var form = new XtraForm())
            {
                form.Size = new Size(300, 150);
                form.Text = prompt;
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.LookAndFeel.SkinName = "Office 2019 Black";
                
                var lbl = new LabelControl { Text = prompt, Location = new Point(15, 15) };
                lbl.Appearance.ForeColor = Color.White;
                
                var txt = new TextEdit { Location = new Point(15, 40), Width = 250, EditValue = defaultValue };
                txt.Properties.Appearance.BackColor = Color.FromArgb(50, 50, 50);
                txt.Properties.Appearance.ForeColor = Color.White;
                
                var btn = new SimpleButton { Text = "Tamam", Location = new Point(165, 75), Width = 100, DialogResult = DialogResult.OK };
                btn.Appearance.BackColor = Color.FromArgb(33, 150, 243);
                btn.Appearance.ForeColor = Color.White;
                
                form.Controls.AddRange(new Control[] { lbl, txt, btn });
                form.AcceptButton = btn;
                
                return form.ShowDialog() == DialogResult.OK ? txt.Text : "";
            }
        }
        #endregion

        #region Data Loading
        private async Task LoadInitialDataAsync()
        {
            await Task.WhenAll(
                LoadWatchlistDataAsync(),
                LoadMarketTilesDataAsync(),
                LoadChartDataAsync(_currentSymbol, _currentTimeframe),
                LoadNewsAsync()
            );
        }

        private async Task LoadWatchlistDataAsync()
        {
            try
            {
                var symbols = new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "META", "NVDA", "AMD" };
                var quotes = await _finnhubService.GetMultipleQuotesAsync(symbols);
                
                var dt = new System.Data.DataTable();
                dt.Columns.Add("Symbol");
                dt.Columns.Add("Price");
                dt.Columns.Add("Change");
                
                foreach (var q in quotes)
                {
                    var changeStr = q.quote.Dp >= 0 ? $"+{q.quote.Dp:F2}%" : $"{q.quote.Dp:F2}%";
                    dt.Rows.Add(q.symbol, $"${q.quote.C:F2}", changeStr);
                }
                
                gridWatchlist.DataSource = dt;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadWatchlistData Error: {ex.Message}");
            }
        }

        private async Task LoadMarketTilesDataAsync()
        {
            try
            {
                var symbols = new[] { "AAPL", "MSFT", "GOOGL", "TSLA", "AMZN", "NVDA", "META" };
                var quotes = await _finnhubService.GetMultipleQuotesAsync(symbols);
                
                for (int i = 0; i < Math.Min(tileBarMarket.Groups[0].Items.Count, quotes.Count); i++)
                {
                    var tile = tileBarMarket.Groups[0].Items[i];
                    var q = quotes[i];
                    
                    if (tile.Elements.Count >= 3)
                    {
                        tile.Elements[1].Text = $"${q.quote.C:F2}";
                        tile.Elements[2].Text = q.quote.Dp >= 0 ? $"+{q.quote.Dp:F2}%" : $"{q.quote.Dp:F2}%";
                        tile.Elements[2].Appearance.Normal.ForeColor = q.quote.Dp >= 0 
                            ? Color.FromArgb(0, 200, 83) 
                            : Color.FromArgb(255, 82, 82);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadMarketTiles Error: {ex.Message}");
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
                    
                    // Store OHLC data for analysis tools
                    int lastIdx = candles.C.Count - 1;
                    _lastO = candles.O[lastIdx];
                    _lastH = candles.H.Max();
                    _lastL = candles.L.Min();
                    _lastC = candles.C[lastIdx];
                    
                    // Calculate change
                    var prevClose = candles.C.Count > 1 ? candles.C[lastIdx - 1] : _lastC;
                    _lastChange = _lastC - prevClose;
                    _lastChangePercent = prevClose > 0 ? ((_lastC - prevClose) / prevClose) * 100 : 0;
                    
                    // Update symbol details
                    lblSymbolName.Text = symbol;
                    lblSymbolPrice.Text = $"${_lastC:N2}";
                    lblSymbolChange.Text = $"{_lastChangePercent:+0.00;-0.00}%";
                    lblSymbolChange.Appearance.ForeColor = _lastChangePercent >= 0 
                        ? Color.FromArgb(0, 200, 83) 
                        : Color.FromArgb(255, 82, 82);
                }
                else
                {
                    GenerateFallbackChartData(series);
                }
                
                if (series.View is CandleStickSeriesView view)
                {
                    view.Color = Color.FromArgb(0, 200, 83);
                    view.ReductionOptions.Color = Color.FromArgb(255, 82, 82);
                    view.ReductionOptions.Visible = true;
                    view.ReductionOptions.Level = StockLevel.Close;
                }
                
                chartMain.Series.Add(series);
                
                if (chartMain.Diagram is XYDiagram diagram)
                {
                    diagram.DefaultPane.BackColor = Color.FromArgb(25, 25, 25);
                    diagram.AxisY.WholeRange.AlwaysShowZeroLevel = false;
                    diagram.AxisX.GridLines.Visible = true;
                    diagram.AxisX.GridLines.Color = Color.FromArgb(40, 40, 40);
                    diagram.AxisY.GridLines.Visible = true;
                    diagram.AxisY.GridLines.Color = Color.FromArgb(40, 40, 40);
                    diagram.AxisX.Label.TextColor = Color.Gray;
                    diagram.AxisY.Label.TextColor = Color.Gray;
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
                    var text = string.Join("\n\n", news.Take(10).Select(n =>
                        $"• {n.Headline}\n  {n.Source} - {DateTimeOffset.FromUnixTimeSeconds(n.Datetime):dd MMM, HH:mm}"));
                    memoNews.Text = text;
                }
            }
            catch (Exception ex)
            {
                memoNews.Text = "Haberler yüklenemedi.";
                System.Diagnostics.Debug.WriteLine($"LoadNews Error: {ex.Message}");
            }
        }

        private void UpdateSymbolDetails(string symbol, double price = 0, double changePercent = 0)
        {
            lblSymbolName.Text = symbol;
            
            if (price > 0)
            {
                lblSymbolPrice.Text = $"${price:N2}";
                lblSymbolChange.Text = $"{changePercent:+0.00;-0.00}%";
                lblSymbolChange.Appearance.ForeColor = changePercent >= 0 
                    ? Color.FromArgb(0, 200, 83) 
                    : Color.FromArgb(255, 82, 82);
            }
        }
        
        /// <summary>
        /// Sembol için fiyat verisini yükle ve UI'yi güncelle
        /// </summary>
        private async Task LoadSymbolQuoteAsync(string symbol)
        {
            try
            {
                var quote = await _finnhubService.GetQuoteAsync(symbol);
                if (quote != null)
                {
                    UpdateSymbolDetails(symbol, quote.C, quote.Dp);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadSymbolQuote Error: {ex.Message}");
                UpdateSymbolDetails(symbol, 0, 0);
            }
        }

        private async Task RunAIAnalysisAsync()
        {
            memoAnalysis.Text = "AI analizi çalışıyor...\n\n";
            
            // Placeholder - will integrate Groq API
            await Task.Delay(1000);
            
            memoAnalysis.Text = $@"═══════════════════════════════════════
AI MARKET ANALİZİ - {_currentSymbol}
═══════════════════════════════════════

📊 PAZAR ÖZETİ:
Piyasa şu anda karışık sinyaller veriyor. 
Volatilite orta seviyede.

📈 TEKNİK GÖRÜNÜM:
- Trend: Yükseliş eğilimi
- RSI: 55 (nötr)
- MACD: Pozitif kesişim

🎯 DESTEK/DİRENÇ:
- Destek 1: $170.00
- Destek 2: $165.00
- Direnç 1: $185.00
- Direnç 2: $190.00

⚠️ RİSK NOTLARI:
- Piyasa belirsizliği yüksek
- Hacim ortalamanın altında

🔔 ALARM ÖNERİLERİ:
- $185 üzeri kırılımda AL sinyali
- $170 altı kırılımda dikkat

[Groq API entegrasyonu yapılacak]";
        }
        #endregion

        #region Portfolio & Order Data
        /// <summary>
        /// Müşterinin portföy pozisyonlarını yükler
        /// </summary>
        private async Task LoadPositionsDataAsync()
        {
            System.Diagnostics.Debug.WriteLine("[POSITIONS] LoadPositionsDataAsync START");
            try
            {
                var accounts = await _accountRepository.GetByCustomerIdAsync(AppEvents.CurrentSession.UserId);
                var primaryAccount = accounts?.FirstOrDefault();
                if (primaryAccount == null)
                {
                    System.Diagnostics.Debug.WriteLine("[POSITIONS] No primary account found");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[POSITIONS] Loading for CustomerId={primaryAccount.CustomerId}");
                var positions = await _portfolioRepository.GetByCustomerIdAsync(primaryAccount.CustomerId);
                System.Diagnostics.Debug.WriteLine($"[POSITIONS] Found {positions?.Count() ?? 0} positions");
                
                if (gridPositions?.DataSource is System.Data.DataTable dt)
                {
                    dt.Rows.Clear();
                    
                    foreach (var pos in positions)
                    {
                        // Get current price (simplified - use average cost for now)
                        decimal currentPrice = pos.AverageCost * 1.05m; // Mock 5% gain
                        decimal totalValue = pos.Quantity * currentPrice;
                        decimal pnl = totalValue - pos.TotalInvestment;
                        
                        dt.Rows.Add(
                            pos.StockSymbol,
                            pos.Quantity.ToString("N2"),
                            $"${pos.AverageCost:N2}",
                            $"${currentPrice:N2}",
                            $"{(pnl >= 0 ? "+" : "")}${pnl:N2}"
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadPositionsData Error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// İşlem geçmişini yükler (Yatırım işlemleri)
        /// </summary>
        private async Task LoadOrderHistoryAsync()
        {
            System.Diagnostics.Debug.WriteLine("[ORDERS] LoadOrderHistoryAsync START");
            try
            {
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                // Get investment transactions
                var transactions = await conn.QueryAsync<dynamic>(@"
                    SELECT t.""TransactionDate"", t.""Description"", t.""Amount"", t.""TransactionType""
                    FROM ""Transactions"" t
                    INNER JOIN ""Accounts"" a ON t.""AccountId"" = a.""Id""
                    INNER JOIN ""Customers"" c ON a.""CustomerId"" = c.""Id""
                    WHERE c.""UserId"" = @UserId
                    AND (t.""Description"" LIKE '%Yatırım%' OR t.""Description"" LIKE '%AL:%' OR t.""Description"" LIKE '%SAT:%')
                    ORDER BY t.""TransactionDate"" DESC
                    LIMIT 50");
                
                var txList = transactions.ToList();
                System.Diagnostics.Debug.WriteLine($"[ORDERS] Found {txList.Count} investment transactions");
                
                if (gridOrders?.DataSource is System.Data.DataTable dt)
                {
                    dt.Rows.Clear();
                    
                    foreach (var tx in txList)
                    {
                        string desc = tx.Description?.ToString() ?? "";
                        string type = desc.Contains("AL") ? "AL" : "SAT";
                        string symbol = ExtractSymbolFromDescription(desc);
                        
                        dt.Rows.Add(
                            ((DateTime)tx.TransactionDate).ToString("dd.MM HH:mm"),
                            symbol,
                            type,
                            $"${(decimal)tx.Amount:N2}",
                            "-",
                            "Gerçekleşti"
                        );
                    }
                    System.Diagnostics.Debug.WriteLine($"[ORDERS] Grid populated with {dt.Rows.Count} rows");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ORDERS] LoadOrderHistory Error: {ex.Message}");
            }
        }
        
        private string ExtractSymbolFromDescription(string desc)
        {
            // Extract symbol from "Yatırım AL: 10 adet AAPL @ $150.00"
            var parts = desc.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "adet" && i + 1 < parts.Length)
                    return parts[i + 1];
            }
            return "N/A";
        }
        #endregion

        #region SVG Icons (Placeholders)
        private DevExpress.Utils.Svg.SvgImage GetSelectIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetCrosshairIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetTrendlineIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetSupportIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetResistanceIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetFibonacciIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetClearIcon() => null;
        private DevExpress.Utils.Svg.SvgImage GetRefreshIcon() => null;
        #endregion
    }
}
