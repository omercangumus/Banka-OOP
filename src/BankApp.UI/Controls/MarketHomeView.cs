using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.IO;
using DevExpress.XtraEditors;
using DevExpress.Utils;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Controls
{
    public class MarketHomeView : XtraUserControl
    {
        private readonly IMarketDataProvider _stockProvider;
        private readonly IMarketDataProvider _cryptoProvider;
        private IMarketDataProvider _activeProvider;
        
        private PanelControl pnlHeader;
        private FlowLayoutPanel pnlCards;
        private TextEdit txtSearch;
        private SimpleButton btnStocks, btnCrypto, btnFavorites, btnIPO, btnRefresh;
        private LabelControl lblLoading, lblEmpty;
        
        private List<MarketAsset> _allAssets = new List<MarketAsset>();
        private string _activeTab = "Stocks";
        private bool _dataLoaded = false;
        
        private static readonly HttpClient _logoClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
        private static readonly Dictionary<string, Image> _logoCache = new Dictionary<string, Image>();
        private static readonly string _logoCacheDir = Path.Combine(Path.GetTempPath(), "NovaBank_Logos");
        
        public event EventHandler<string> AssetSelected;
        public event EventHandler<string> TradeTerminalRequested;
        
        // IPO Data
        private static readonly List<IPOItem> _ipoList = new List<IPOItem>
        {
            new IPOItem { Company = "Türk Havayolları", Ticker = "THYAO", Date = "2025-02-15", PriceRange = "₺180-200", Status = "Upcoming" },
            new IPOItem { Company = "Getir Teknoloji", Ticker = "GETIR", Date = "2025-03-01", PriceRange = "₺45-55", Status = "Upcoming" },
            new IPOItem { Company = "Trendyol", Ticker = "TREND", Date = "2025-Q2", PriceRange = "TBA", Status = "Announced" },
            new IPOItem { Company = "BioNTech TR", Ticker = "BNTX.TR", Date = "2025-04-10", PriceRange = "-95", Status = "Upcoming" },
            new IPOItem { Company = "Peak Games", Ticker = "PEAK", Date = "2024-12-01", PriceRange = "₺120-140", Status = "Closed" },
            new IPOItem { Company = "Hepsiburada", Ticker = "HEPS", Date = "2024-11-20", PriceRange = "₺35-42", Status = "Open" }
        };
        
        public MarketHomeView(IMarketDataProvider stockProvider, IMarketDataProvider cryptoProvider)
        {
            _stockProvider = stockProvider;
            _cryptoProvider = cryptoProvider;
            _activeProvider = _stockProvider;
            
            if (!Directory.Exists(_logoCacheDir)) Directory.CreateDirectory(_logoCacheDir);
            
            InitializeComponents();
            this.HandleCreated += (s, e) => TriggerDataLoad();
            this.VisibleChanged += (s, e) => { if (this.Visible) TriggerDataLoad(); };
        }
        
        private async void TriggerDataLoad()
        {
            if (_dataLoaded || !this.IsHandleCreated || !this.Visible) return;
            _dataLoaded = true;
            await LoadMarketDataAsync();
        }

        private void InitializeComponents()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(14, 14, 14);
            this.Padding = new Padding(0);
            this.Margin = new Padding(0);
            
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 50;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            pnlHeader.Padding = new Padding(0);
            
            txtSearch = new TextEdit();
            txtSearch.Properties.NullText = " Ara... (AAPL, BTC)";
            txtSearch.Properties.Appearance.BackColor = Color.FromArgb(30, 30, 30);
            txtSearch.Properties.Appearance.ForeColor = Color.White;
            txtSearch.Properties.NullValuePromptShowForEmptyValue = true;
            txtSearch.Size = new Size(200, 32);
            txtSearch.Location = new Point(10, 9);
            txtSearch.EditValueChanged += (s, e) => ApplyFilters();
            
            btnStocks = CreateTabButton("Hisseler", true, 220);
            btnCrypto = CreateTabButton("Kripto", false, 325);
            btnFavorites = CreateTabButton("Favoriler", false, 430);
            btnIPO = CreateTabButton("Halka Arz", false, 535);
            
            btnStocks.Click += (s, e) => SwitchTab("Stocks");
            btnCrypto.Click += (s, e) => SwitchTab("Crypto");
            btnFavorites.Click += (s, e) => SwitchTab("Favorites");
            btnIPO.Click += (s, e) => SwitchTab("IPO");
            
            btnRefresh = new SimpleButton();
            btnRefresh.Text = "";
            btnRefresh.Size = new Size(40, 32);
            btnRefresh.Location = new Point(650, 9);
            btnRefresh.Appearance.BackColor = Color.FromArgb(40, 40, 40);
            btnRefresh.Appearance.ForeColor = Color.White;
            btnRefresh.Appearance.Options.UseBackColor = true;
            btnRefresh.Click += async (s, e) => await RefreshAsync();
            
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Controls.Add(btnStocks);
            pnlHeader.Controls.Add(btnCrypto);
            pnlHeader.Controls.Add(btnFavorites);
            pnlHeader.Controls.Add(btnIPO);
            pnlHeader.Controls.Add(btnRefresh);
            
            pnlCards = new FlowLayoutPanel();
            pnlCards.Dock = DockStyle.Fill;
            pnlCards.AutoScroll = true;
            pnlCards.BackColor = Color.FromArgb(14, 14, 14);
            pnlCards.Padding = new Padding(8, 5, 8, 8);
            pnlCards.WrapContents = true;
            
            lblLoading = new LabelControl();
            lblLoading.Text = " Yükleniyor...";
            lblLoading.Appearance.Font = new Font("Segoe UI", 14F);
            lblLoading.Appearance.ForeColor = Color.FromArgb(33, 150, 243);
            lblLoading.AutoSizeMode = LabelAutoSizeMode.None;
            lblLoading.Dock = DockStyle.Fill;
            lblLoading.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            lblLoading.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            lblLoading.Visible = false;
            
            lblEmpty = new LabelControl();
            lblEmpty.Text = "Sonuç yok";
            lblEmpty.Appearance.Font = new Font("Segoe UI", 12F);
            lblEmpty.Appearance.ForeColor = Color.Gray;
            lblEmpty.AutoSizeMode = LabelAutoSizeMode.None;
            lblEmpty.Dock = DockStyle.Fill;
            lblEmpty.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            lblEmpty.Appearance.TextOptions.VAlignment = VertAlignment.Center;
            lblEmpty.Visible = false;
            
            this.Controls.Add(lblEmpty);
            this.Controls.Add(lblLoading);
            this.Controls.Add(pnlCards);
            this.Controls.Add(pnlHeader);
            this.ResumeLayout(true);
        }

        private SimpleButton CreateTabButton(string text, bool selected, int x)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Size = new Size(100, 32);
            btn.Location = new Point(x, 9);
            btn.Appearance.Font = new Font("Segoe UI", 9.5F, selected ? FontStyle.Bold : FontStyle.Regular);
            btn.Appearance.ForeColor = selected ? Color.White : Color.FromArgb(150, 150, 150);
            btn.Appearance.BackColor = selected ? Color.FromArgb(33, 150, 243) : Color.FromArgb(30, 30, 30);
            btn.Appearance.Options.UseFont = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.Options.UseBackColor = true;
            return btn;
        }
        
        private Panel CreateAssetCard(MarketAsset asset)
        {
            // Calculate responsive card width
            int formWidth = this.Width > 0 ? this.Width : 1200;
            int cardMinWidth = 280;
            int cardMaxWidth = 350;
            int cols = Math.Max(1, (formWidth - 40) / cardMinWidth);
            int cardWidth = Math.Min(cardMaxWidth, (formWidth - 40 - (cols * 12)) / cols);
            
            var card = new Panel();
            card.Size = new Size(cardWidth, 85);
            card.Margin = new Padding(6);
            card.BackColor = Color.FromArgb(24, 24, 24);
            card.Cursor = Cursors.Hand;
            card.MouseEnter += (s, e) => card.BackColor = Color.FromArgb(35, 35, 35);
            card.MouseLeave += (s, e) => card.BackColor = Color.FromArgb(24, 24, 24);
            card.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            
            // Ticker badge instead of generic icon
            var pnlBadge = new Panel();
            pnlBadge.Size = new Size(48, 48);
            pnlBadge.Location = new Point(12, 18);
            pnlBadge.BackColor = GetBadgeColor(asset.Symbol);
            pnlBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundedRect(new Rectangle(0, 0, 48, 48), 8);
                using var brush = new SolidBrush(GetBadgeColor(asset.Symbol));
                e.Graphics.FillPath(brush, path);
                
                var ticker = asset.Symbol.Replace("USDT", "");
                if (ticker.Length > 4) ticker = ticker.Substring(0, 4);
                using var font = new Font("Segoe UI", ticker.Length > 3 ? 10F : 12F, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(ticker, font, textBrush, new RectangleF(0, 0, 48, 48), sf);
            };
            pnlBadge.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(pnlBadge);
            
            // Symbol
            var lblSymbol = new Label();
            lblSymbol.Text = asset.Symbol;
            lblSymbol.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblSymbol.ForeColor = Color.White;
            lblSymbol.Location = new Point(68, 12);
            lblSymbol.AutoSize = true;
            lblSymbol.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(lblSymbol);
            
            // Name
            var lblName = new Label();
            var displayName = asset.Name.Length > 20 ? asset.Name.Substring(0, 18) + ".." : asset.Name;
            lblName.Text = displayName;
            lblName.Font = new Font("Segoe UI", 9F);
            lblName.ForeColor = Color.FromArgb(130, 130, 130);
            lblName.Location = new Point(68, 34);
            lblName.AutoSize = true;
            lblName.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(lblName);
            
            // Exchange
            var lblExch = new Label();
            lblExch.Text = asset.Exchange;
            lblExch.Font = new Font("Segoe UI", 8F);
            lblExch.ForeColor = Color.FromArgb(90, 90, 90);
            lblExch.Location = new Point(68, 54);
            lblExch.AutoSize = true;
            lblExch.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(lblExch);
            
            // Price
            bool hasData = asset.LastPrice > 0;
            var lblPrice = new Label();
            lblPrice.Text = hasData ? "$" + asset.LastPrice.ToString("N2") : "--";
            lblPrice.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblPrice.ForeColor = Color.White;
            lblPrice.Location = new Point(175, 12);
            lblPrice.Size = new Size(105, 22);
            lblPrice.TextAlign = ContentAlignment.TopRight;
            lblPrice.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(lblPrice);
            
            // Change %
            var lblChange = new Label();
            if (hasData)
            {
                var pct = asset.ChangePercent;
                lblChange.Text = (pct >= 0 ? "+" : "") + pct.ToString("N2") + "%";
                lblChange.ForeColor = pct >= 0 ? Color.FromArgb(38, 166, 91) : Color.FromArgb(232, 65, 66);
                lblChange.BackColor = pct >= 0 ? Color.FromArgb(20, 60, 35) : Color.FromArgb(60, 25, 25);
            }
            else
            {
                lblChange.Text = "N/A";
                lblChange.ForeColor = Color.FromArgb(150, 150, 150);
                lblChange.BackColor = Color.FromArgb(40, 40, 40);
            }
            lblChange.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblChange.Location = new Point(200, 38);
            lblChange.Size = new Size(80, 20);
            lblChange.TextAlign = ContentAlignment.MiddleCenter;
            lblChange.Click += (s, e) => AssetSelected?.Invoke(this, asset.Symbol);
            card.Controls.Add(lblChange);
            
            return card;
        }
        
        private Panel CreateIPOCard(IPOItem ipo)
        {
            var card = new Panel();
            card.Size = new Size(290, 100);
            card.Margin = new Padding(6);
            card.BackColor = Color.FromArgb(24, 24, 24);
            
            // Badge
            var pnlBadge = new Panel();
            pnlBadge.Size = new Size(48, 48);
            pnlBadge.Location = new Point(12, 26);
            pnlBadge.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = GetRoundedRect(new Rectangle(0, 0, 48, 48), 8);
                using var brush = new SolidBrush(Color.FromArgb(156, 39, 176));
                e.Graphics.FillPath(brush, path);
                using var font = new Font("Segoe UI", 9F, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.White);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(ipo.Ticker.Length > 4 ? ipo.Ticker.Substring(0, 4) : ipo.Ticker, font, textBrush, new RectangleF(0, 0, 48, 48), sf);
            };
            card.Controls.Add(pnlBadge);
            
            var lblCompany = new Label();
            lblCompany.Text = ipo.Company;
            lblCompany.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblCompany.ForeColor = Color.White;
            lblCompany.Location = new Point(68, 10);
            lblCompany.AutoSize = true;
            card.Controls.Add(lblCompany);
            
            var lblTicker = new Label();
            lblTicker.Text = ipo.Ticker + " | " + ipo.Date;
            lblTicker.Font = new Font("Segoe UI", 9F);
            lblTicker.ForeColor = Color.FromArgb(130, 130, 130);
            lblTicker.Location = new Point(68, 32);
            lblTicker.AutoSize = true;
            card.Controls.Add(lblTicker);
            
            var lblPrice = new Label();
            lblPrice.Text = "Fiyat: " + ipo.PriceRange;
            lblPrice.Font = new Font("Segoe UI", 9F);
            lblPrice.ForeColor = Color.FromArgb(150, 150, 150);
            lblPrice.Location = new Point(68, 52);
            lblPrice.AutoSize = true;
            card.Controls.Add(lblPrice);
            
            // Status badge
            var lblStatus = new Label();
            lblStatus.Text = ipo.Status;
            lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblStatus.Size = new Size(80, 22);
            lblStatus.Location = new Point(200, 72);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.ForeColor = ipo.Status switch {
                "Open" => Color.FromArgb(38, 166, 91),
                "Upcoming" => Color.FromArgb(33, 150, 243),
                "Announced" => Color.FromArgb(255, 193, 7),
                _ => Color.FromArgb(150, 150, 150)
            };
            lblStatus.BackColor = ipo.Status switch {
                "Open" => Color.FromArgb(20, 60, 35),
                "Upcoming" => Color.FromArgb(20, 40, 70),
                "Announced" => Color.FromArgb(60, 50, 20),
                _ => Color.FromArgb(40, 40, 40)
            };
            card.Controls.Add(lblStatus);
            
            return card;
        }
        
        private Color GetBadgeColor(string symbol)
        {
            var hash = symbol.GetHashCode();
            var colors = new[] {
                Color.FromArgb(33, 150, 243),
                Color.FromArgb(156, 39, 176),
                Color.FromArgb(0, 150, 136),
                Color.FromArgb(255, 152, 0),
                Color.FromArgb(103, 58, 183),
                Color.FromArgb(233, 30, 99),
                Color.FromArgb(0, 188, 212),
                Color.FromArgb(139, 195, 74)
            };
            return colors[Math.Abs(hash) % colors.Length];
        }
        
        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            path.CloseFigure();
            return path;
        }
        
        private void RenderCards(List<MarketAsset> assets)
        {
            pnlCards.SuspendLayout();
            pnlCards.Controls.Clear();
            foreach (var asset in assets) pnlCards.Controls.Add(CreateAssetCard(asset));
            pnlCards.ResumeLayout(true);
        }
        
        private void RenderIPOCards()
        {
            pnlCards.SuspendLayout();
            pnlCards.Controls.Clear();
            foreach (var ipo in _ipoList) pnlCards.Controls.Add(CreateIPOCard(ipo));
            pnlCards.ResumeLayout(true);
        }

        private async Task LoadMarketDataAsync()
        {
            try
            {
                if (this.InvokeRequired) { this.BeginInvoke(new Action(async () => await LoadMarketDataAsync())); return; }
                
                if (_activeTab == "IPO")
                {
                    RenderIPOCards();
                    return;
                }
                
                lblLoading.Visible = true;
                lblLoading.BringToFront();
                pnlCards.Visible = false;
                lblEmpty.Visible = false;
                
                _allAssets = await _activeProvider.GetAssetsAsync();
                
                RenderCards(_allAssets);
                lblLoading.Visible = false;
                pnlCards.Visible = true;
                pnlCards.BringToFront();
            }
            catch (Exception ex)
            {
                lblLoading.Visible = false;
                lblEmpty.Text = "Hata: " + ex.Message;
                lblEmpty.Visible = true;
                lblEmpty.BringToFront();
            }
        }

        private async void SwitchTab(string tab)
        {
            _activeTab = tab;
            _dataLoaded = false;
            
            btnStocks.Appearance.Font = new Font("Segoe UI", 9.5F, tab == "Stocks" ? FontStyle.Bold : FontStyle.Regular);
            btnStocks.Appearance.ForeColor = tab == "Stocks" ? Color.White : Color.FromArgb(150, 150, 150);
            btnStocks.Appearance.BackColor = tab == "Stocks" ? Color.FromArgb(33, 150, 243) : Color.FromArgb(30, 30, 30);
            
            btnCrypto.Appearance.Font = new Font("Segoe UI", 9.5F, tab == "Crypto" ? FontStyle.Bold : FontStyle.Regular);
            btnCrypto.Appearance.ForeColor = tab == "Crypto" ? Color.White : Color.FromArgb(150, 150, 150);
            btnCrypto.Appearance.BackColor = tab == "Crypto" ? Color.FromArgb(33, 150, 243) : Color.FromArgb(30, 30, 30);
            
            btnFavorites.Appearance.Font = new Font("Segoe UI", 9.5F, tab == "Favorites" ? FontStyle.Bold : FontStyle.Regular);
            btnFavorites.Appearance.ForeColor = tab == "Favorites" ? Color.White : Color.FromArgb(150, 150, 150);
            btnFavorites.Appearance.BackColor = tab == "Favorites" ? Color.FromArgb(33, 150, 243) : Color.FromArgb(30, 30, 30);
            
            btnIPO.Appearance.Font = new Font("Segoe UI", 9.5F, tab == "IPO" ? FontStyle.Bold : FontStyle.Regular);
            btnIPO.Appearance.ForeColor = tab == "IPO" ? Color.White : Color.FromArgb(150, 150, 150);
            btnIPO.Appearance.BackColor = tab == "IPO" ? Color.FromArgb(33, 150, 243) : Color.FromArgb(30, 30, 30);
            
            if (tab == "IPO")
            {
                RenderIPOCards();
                return;
            }
            
            _activeProvider = tab == "Crypto" ? _cryptoProvider : _stockProvider;
            if (tab != "Favorites") await LoadMarketDataAsync();
            else ApplyFilters();
        }

        private void ApplyFilters()
        {
            var q = (txtSearch.Text ?? "").Trim().ToUpperInvariant();
            var filtered = _allAssets.Where(a => {
                if (_activeTab == "Stocks") return a.AssetType == "Stock";
                if (_activeTab == "Crypto") return a.AssetType == "Crypto";
                if (_activeTab == "Favorites") return a.IsFavorite;
                return true;
            }).Where(a => string.IsNullOrEmpty(q) || a.Symbol.ToUpperInvariant().Contains(q) || a.Name.ToUpperInvariant().Contains(q)).ToList();
            
            RenderCards(filtered);
            lblEmpty.Visible = filtered.Count == 0;
        }

        public async Task RefreshAsync() { _dataLoaded = false; await LoadMarketDataAsync(); }
        
        private class IPOItem
        {
            public string Company { get; set; }
            public string Ticker { get; set; }
            public string Date { get; set; }
            public string PriceRange { get; set; }
            public string Status { get; set; }
        }
    }
}
