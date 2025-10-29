using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Controls
{
    /// <summary>
    /// Investment Dashboard V2 - Enhanced dashboard with 8-12 asset cards in dynamic grid
    /// Supports stocks, crypto, forex, and commodities with real-time data
    /// </summary>
    public class InvestmentDashboardV2 : XtraUserControl
    {
        private PanelControl pnlMain;
        private PanelControl pnlHeader;
        private ScrollableControl scrollableGrid;
        
        private LabelControl lblTitle;
        private LabelControl lblSummary;
        private SimpleButton btnRefresh;
        private SimpleButton btnSettings;
        
        private readonly List<AssetCardControl> _assetCards;
        private readonly FinnhubServiceV2 _finnhubService;
        private readonly System.Windows.Forms.Timer _refreshTimer;
        
        private const int GRID_COLUMNS = 3;
        private const int CARD_WIDTH = 280;
        private const int CARD_HEIGHT = 160;
        private const int CARD_MARGIN = 15;

        public event EventHandler<string> AssetSelected;

        public InvestmentDashboardV2()
        {
            _assetCards = new List<AssetCardControl>();
            _finnhubService = new FinnhubServiceV2();
            _refreshTimer = new System.Windows.Forms.Timer();
            
            InitializeUI();
            InitializeAssetCards();
            SetupAutoRefresh();
            
            // Load data on initialization
            _ = LoadAllAssetDataAsync();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Appearance.BackColor = Color.FromArgb(15, 23, 42); // Dark theme background
            this.Appearance.Options.UseBackColor = true;

            // Main panel
            pnlMain = new PanelControl();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = Color.Transparent;
            pnlMain.Appearance.Options.UseBackColor = true;
            this.Controls.Add(pnlMain);

            // Header panel
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 80;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Appearance.BackColor = Color.Transparent;
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlMain.Controls.Add(pnlHeader);

            // Title
            lblTitle = new LabelControl();
            lblTitle.Text = "INVESTMENT DASHBOARD V2";
            lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(226, 232, 240);
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSizeMode = LabelAutoSizeMode.None;
            lblTitle.Size = new Size(400, 30);
            pnlHeader.Controls.Add(lblTitle);

            // Summary
            lblSummary = new LabelControl();
            lblSummary.Text = "Loading market data for 9 assets...";
            lblSummary.Appearance.Font = new Font("Segoe UI", 11F);
            lblSummary.Appearance.ForeColor = Color.FromArgb(148, 163, 184);
            lblSummary.Location = new Point(20, 50);
            lblSummary.AutoSizeMode = LabelAutoSizeMode.None;
            lblSummary.Size = new Size(600, 20);
            pnlHeader.Controls.Add(lblSummary);

            // Refresh button
            btnRefresh = new SimpleButton();
            btnRefresh.Text = "🔄 REFRESH";
            btnRefresh.Size = new Size(120, 35);
            btnRefresh.Location = new Point(this.Width - 280, 20);
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.Appearance.BackColor = Color.FromArgb(16, 185, 129);
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnRefresh.Click += BtnRefresh_Click;
            pnlHeader.Controls.Add(btnRefresh);

            // Settings button
            btnSettings = new SimpleButton();
            btnSettings.Text = "⚙️ SETTINGS";
            btnSettings.Size = new Size(120, 35);
            btnSettings.Location = new Point(this.Width - 150, 20);
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.Appearance.BackColor = Color.FromArgb(59, 130, 246);
            btnSettings.Appearance.ForeColor = Color.White;
            btnSettings.Appearance.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSettings.Click += BtnSettings_Click;
            pnlHeader.Controls.Add(btnSettings);

            // Asset grid panel with scrolling
            scrollableGrid = new ScrollableControl();
            scrollableGrid.Dock = DockStyle.Fill;
            scrollableGrid.BackColor = Color.Transparent;
            scrollableGrid.AutoScroll = true;
            scrollableGrid.Padding = new Padding(20);
            pnlMain.Controls.Add(scrollableGrid);
        }

        private void InitializeAssetCards()
        {
            var supportedAssets = _finnhubService.GetSupportedAssets();
            
            for (int i = 0; i < supportedAssets.Length; i++)
            {
                var asset = supportedAssets[i];
                var card = new AssetCardControl(asset.Symbol);
                
                // Calculate position in 3-column grid
                int row = i / GRID_COLUMNS;
                int col = i % GRID_COLUMNS;
                
                int x = col * (CARD_WIDTH + CARD_MARGIN);
                int y = row * (CARD_HEIGHT + CARD_MARGIN);
                
                card.Location = new Point(x, y);
                card.Size = new Size(CARD_WIDTH, CARD_HEIGHT);
                
                // Set initial display name
                card.UpdateData(new MarketData 
                { 
                    Symbol = asset.Symbol, 
                    DisplayName = asset.DisplayName,
                    AssetType = asset.Type,
                    CurrentPrice = 0,
                    Change = 0,
                    ChangePercent = 0
                });
                
                card.SetLoadingState(true);
                card.AssetCardClicked += OnAssetCardClicked;
                
                _assetCards.Add(card);
                scrollableGrid.Controls.Add(card);
            }

            // Update scrollable area size
            UpdateScrollableSize();
        }

        private void UpdateScrollableSize()
        {
            if (_assetCards.Count == 0) return;

            int rows = (_assetCards.Count + GRID_COLUMNS - 1) / GRID_COLUMNS;
            int totalHeight = rows * (CARD_HEIGHT + CARD_MARGIN) + CARD_MARGIN;
            int totalWidth = GRID_COLUMNS * (CARD_WIDTH + CARD_MARGIN);

            scrollableGrid.AutoScrollMinSize = new Size(totalWidth, totalHeight);
        }

        private void SetupAutoRefresh()
        {
            _refreshTimer.Interval = 30000; // 30 seconds
            _refreshTimer.Tick += async (s, e) => await LoadAllAssetDataAsync();
            _refreshTimer.Start();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshAllDataAsync();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            // TODO: Open settings dialog for asset configuration
            XtraMessageBox.Show("Settings functionality will be implemented in future updates.", 
                               "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnAssetCardClicked(object sender, string symbol)
        {
            AssetSelected?.Invoke(this, symbol);
        }

        public async Task LoadAllAssetDataAsync()
        {
            try
            {
                lblSummary.Text = "Loading market data...";
                
                var supportedAssets = _finnhubService.GetSupportedAssets();
                var symbols = supportedAssets.Select(a => a.Symbol).ToArray();
                
                var marketDataList = await _finnhubService.GetMultipleMarketDataAsync(symbols);
                
                // Update each asset card
                foreach (var marketData in marketDataList)
                {
                    var card = _assetCards.FirstOrDefault(c => c.Symbol == marketData.Symbol);
                    if (card != null)
                    {
                        card.UpdateData(marketData);
                    }
                }

                // Update summary
                UpdateSummary(marketDataList);
            }
            catch (Exception ex)
            {
                lblSummary.Text = "Error loading market data. Using cached data.";
                System.Diagnostics.Debug.WriteLine($"LoadAllAssetData error: {ex.Message}");
                
                // Set error state for all cards
                foreach (var card in _assetCards)
                {
                    card.SetLoadingState(false);
                }
            }
        }

        public async Task RefreshAllDataAsync()
        {
            btnRefresh.Enabled = false;
            btnRefresh.Text = "🔄 Loading...";
            
            // Set all cards to loading state
            foreach (var card in _assetCards)
            {
                card.SetLoadingState(true);
            }

            // Clear cache and reload
            _finnhubService.ClearAllCaches();
            await LoadAllAssetDataAsync();
            
            btnRefresh.Enabled = true;
            btnRefresh.Text = "🔄 REFRESH";
        }

        private void UpdateSummary(List<MarketData> marketDataList)
        {
            if (marketDataList == null || marketDataList.Count == 0)
            {
                lblSummary.Text = "No market data available";
                return;
            }

            var validData = marketDataList.Where(m => m.IsValid).ToList();
            var upCount = validData.Count(m => m.IsPriceUp);
            var downCount = validData.Count(m => m.IsPriceDown);
            var unchangedCount = validData.Count - upCount - downCount;

            var topGainer = validData.OrderByDescending(m => m.ChangePercent).FirstOrDefault();
            var topLoser = validData.OrderBy(m => m.ChangePercent).FirstOrDefault();

            string summaryText = $"Assets: {validData.Count} loaded | " +
                               $"↗️ {upCount} up | ↘️ {downCount} down | → {unchangedCount} unchanged";

            if (topGainer != null && topLoser != null)
            {
                summaryText += $" | Top: {topGainer.Symbol} (+{topGainer.ChangePercent:F1}%) | " +
                              $"Bottom: {topLoser.Symbol} ({topLoser.ChangePercent:F1}%)";
            }

            lblSummary.Text = summaryText;
        }

        public List<AssetCardControl> GetDisplayedAssetCards()
        {
            return _assetCards.Where(c => c.Visible).ToList();
        }

        public MarketData GetMarketData(string symbol)
        {
            var card = _assetCards.FirstOrDefault(c => c.Symbol == symbol);
            return card?.MarketData;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _refreshTimer?.Stop();
                _refreshTimer?.Dispose();
                
                foreach (var card in _assetCards)
                {
                    card.AssetCardClicked -= OnAssetCardClicked;
                }
            }
            base.Dispose(disposing);
        }
    }
}