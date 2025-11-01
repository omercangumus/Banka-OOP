using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public partial class TradeTerminalForm : XtraForm
    {
        private readonly FinnhubService _finnhubService;
        
        public TradeTerminalForm()
        {
            InitializeComponent();
            _finnhubService = new FinnhubService();
            SetupChartVisuals(); 
            InitializeMaximizeButton(); // Add button
            InitializeContextMenu();
            LoadRealStocks();   
        }

        private void SetupChartVisuals()
        {
            chartStock.BackColor = Color.FromArgb(30, 30, 30);
            chartStock.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            chartStock.CrosshairEnabled = DevExpress.Utils.DefaultBoolean.True;
            chartStock.CrosshairOptions.ShowArgumentLine = true;
            chartStock.CrosshairOptions.ShowValueLine = true;
            chartStock.CrosshairOptions.ShowValueLabels = true;
            chartStock.CrosshairOptions.ArgumentLineColor = Color.Gold;
            chartStock.CrosshairOptions.ValueLineColor = Color.Gold;

            if (chartStock.Diagram is XYDiagram diagram)
            {
                diagram.DefaultPane.BackColor = Color.FromArgb(35, 35, 35);
                diagram.DefaultPane.BorderVisible = false;
                
                diagram.AxisX.GridLines.Visible = true;
                diagram.AxisX.GridLines.Color = Color.FromArgb(50, 255, 255, 255);
                diagram.AxisY.GridLines.Visible = true;
                diagram.AxisY.GridLines.Color = Color.FromArgb(50, 255, 255, 255);

                diagram.AxisX.Label.TextColor = Color.White;
                diagram.AxisY.Label.TextColor = Color.White;

                diagram.EnableAxisXScrolling = true;
                diagram.EnableAxisXZooming = true;
                diagram.EnableAxisYScrolling = true;
                diagram.EnableAxisYZooming = true;
                
                diagram.ZoomingOptions.UseMouseWheel = true;
            }
        }

        private List<StockInfo> _allStocks = new List<StockInfo>();

        private async void LoadRealStocks()
        {
            // Expanded list with 50+ mixed assets (BIST, US Tech, Crypto)
            var expandedSymbols = new[] {
                // US Tech Giants
                "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "AMD", "NFLX", "DIS",
                "INTC", "CSCO", "ORCL", "CRM", "ADBE", "PYPL", "SQ", "SHOP", "UBER", "LYFT",
                // Other US
                "JPM", "BAC", "WMT", "PG", "KO", "PEP", "NKE", "MCD", "SBUX", "V",
                // BIST (Istanbul Stock Exchange - simulate)
                "THYAO", "ASELS", "GARAN", "SASA", "EREGL", "AKBNK", "ISCTR", "TUPRS", "PETKM", "SISE",
                // Crypto (via Binance prefix - Finnhub format)
                "BINANCE:BTCUSDT", "BINANCE:ETHUSDT", "BINANCE:BNBUSDT", "BINANCE:SOLUSDT", "BINANCE:ADAUSDT",
                "BINANCE:XRPUSDT", "BINANCE:DOTUSDT", "BINANCE:AVAXUSDT", "BINANCE:MATICUSDT", "BINANCE:LINKUSDT"
            };
            
            _allStocks.Clear();
            lstStocks.Items.Clear();
            lstStocks.Items.Add(new StockInfo { Symbol = "...", Name = "Loading 50+ assets...", Price = 0 });
            
            try
            {
                // Load first 20 for performance (API rate limits)
                var symbolsToLoad = expandedSymbols.Take(20).ToArray();
                var quotes = await _finnhubService.GetMultipleQuotesAsync(symbolsToLoad);
                
                foreach (var item in quotes)
                {
                    _allStocks.Add(new StockInfo 
                    { 
                        Symbol = item.symbol, 
                        Name = GetStockName(item.symbol), 
                        Price = (decimal)item.quote.C,
                        Change = (decimal)item.quote.D,
                        ChangePercent = (decimal)item.quote.Dp
                    });
                }
                
                // Add remaining as mock data to avoid API limits
                for (int i = 20; i < expandedSymbols.Length; i++)
                {
                    var symbol = expandedSymbols[i];
                    var mockPrice = new Random(symbol.GetHashCode()).Next(10, 500);
                    _allStocks.Add(new StockInfo
                    {
                        Symbol = symbol,
                        Name = GetStockName(symbol),
                        Price = mockPrice,
                        Change = (decimal)((new Random().NextDouble() * 10) - 5),
                        ChangePercent = (decimal)((new Random().NextDouble() * 6) - 3)
                    });
                }
                
                RefreshStockList("");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadRealStocks Error: {ex.Message}");
                lstStocks.Items.Clear();
                lstStocks.Items.Add(new StockInfo { Symbol = "ERROR", Name = "Failed to load data.", Price = 0 });
            }
        }
        
        private void RefreshStockList(string searchText)
        {
            lstStocks.Items.Clear();
            
            var filtered = string.IsNullOrWhiteSpace(searchText) 
                ? _allStocks 
                : _allStocks.Where(s => 
                    s.Symbol.ToUpper().Contains(searchText.ToUpper()) || 
                    s.Name.ToUpper().Contains(searchText.ToUpper())
                  ).ToList();
            
            foreach(var stock in filtered)
            {
                lstStocks.Items.Add(stock);
            }
        }
        
        private string GetStockName(string symbol)
        {
            var names = new Dictionary<string, string>
            {
                // US Tech
                { "AAPL", "Apple Inc." }, { "MSFT", "Microsoft" }, { "GOOGL", "Alphabet" }, { "AMZN", "Amazon" },
                { "TSLA", "Tesla" }, { "META", "Meta" }, { "NVDA", "NVIDIA" }, { "AMD", "AMD" },
                { "NFLX", "Netflix" }, { "DIS", "Disney" }, { "INTC", "Intel" }, { "CSCO", "Cisco" },
                { "ORCL", "Oracle" }, { "CRM", "Salesforce" }, { "ADBE", "Adobe" }, { "PYPL", "PayPal" },
                { "SQ", "Block (Square)" }, { "SHOP", "Shopify" }, { "UBER", "Uber" }, { "LYFT", "Lyft" },
                // Other US
                { "JPM", "JPMorgan" }, { "BAC", "Bank of America" }, { "WMT", "Walmart" }, { "PG", "Procter & Gamble" },
                { "KO", "Coca-Cola" }, { "PEP", "PepsiCo" }, { "NKE", "Nike" }, { "MCD", "McDonald's" },
                { "SBUX", "Starbucks" }, { "V", "Visa" },
                // BIST
                { "THYAO", "Türk Hava Yolları" }, { "ASELS", "Aselsan" }, { "GARAN", "Garanti Bankası" },
                { "SASA", "Sasa Polyester" }, { "EREGL", "Ereğli Demir Çelik" }, { "AKBNK", "Akbank" },
                { "ISCTR", "İş Bankası" }, { "TUPRS", "Tüpraş" }, { "PETKM", "Petkim" }, { "SISE", "Şişe Cam" },
                // Crypto
                { "BINANCE:BTCUSDT", "Bitcoin" }, { "BINANCE:ETHUSDT", "Ethereum" }, { "BINANCE:BNBUSDT", "Binance Coin" },
                { "BINANCE:SOLUSDT", "Solana" }, { "BINANCE:ADAUSDT", "Cardano" }, { "BINANCE:XRPUSDT", "Ripple" },
                { "BINANCE:DOTUSDT", "Polkadot" }, { "BINANCE:AVAXUSDT", "Avalanche" }, { "BINANCE:MATICUSDT", "Polygon" },
                { "BINANCE:LINKUSDT", "Chainlink" }
            };
            return names.ContainsKey(symbol) ? names[symbol] : symbol;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (sender is DevExpress.XtraEditors.TextEdit txt)
            {
                RefreshStockList(txt.Text);
            }
        }

        private void lstStocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstStocks.SelectedItem is StockInfo stock)
            {
                if(lblSymbol != null) lblSymbol.Text = $"{stock.Symbol}";
                GenerateCandleChart(stock.Symbol, stock.Price);
            }
        }

        private async void GenerateCandleChart(string symbol, decimal startPrice)
        {
            chartStock.Series.Clear();

            Series series = new Series(symbol, ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;

            try
            {
                // Get real candle data from Finnhub
                var candles = await _finnhubService.GetCandlesAsync(symbol, "D", 60);
                
                if (candles != null && candles.C != null && candles.C.Count > 0)
                {
                    for (int i = 0; i < candles.C.Count; i++)
                    {
                        var date = DateTimeOffset.FromUnixTimeSeconds(candles.T[i]).DateTime;
                        var open = candles.O[i];
                        var high = candles.H[i];
                        var low = candles.L[i];
                        var close = candles.C[i];
                        
                        series.Points.Add(new SeriesPoint(date, low, high, open, close));
                    }
                    
                    // Update price label with latest price
                    var latestPrice = candles.C[candles.C.Count - 1];
                    if(lblPrice != null) lblPrice.Text = $"${latestPrice:N2}";
                }
                else
                {
                    // Fallback to simulated data if API fails
                    GenerateFallbackChart(series, symbol, startPrice);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GenerateCandleChart Error: {ex.Message}");
                GenerateFallbackChart(series, symbol, startPrice);
            }

            if(series.View is CandleStickSeriesView view)
            {
                // Vibrant Colors: Green (Up), Red (Down)
                view.Color = Color.FromArgb(0, 255, 0); // Lime Green
                view.ReductionOptions.Color = Color.FromArgb(255, 0, 0); // Bright Red
                view.ReductionOptions.Visible = true;
                view.ReductionOptions.Level = StockLevel.Close; // Logic to determine color based on Close vs Open
                
                view.LineThickness = 1;
                view.LevelLineLength = 0.3;

                BollingerBands bb = new BollingerBands();
                bb.ValueLevel = ValueLevel.Close;
                bb.Color = Color.FromArgb(50, 255, 255, 255); 
                bb.LineStyle.Thickness = 1;
                view.Indicators.Add(bb);

                SimpleMovingAverage sma = new SimpleMovingAverage();
                sma.PointsCount = 20; 
                sma.Color = Color.Orange; 
                sma.LineStyle.Thickness = 2;
                sma.LegendText = "SMA (20)";
                view.Indicators.Add(sma);
            }

            chartStock.Series.Add(series);
            
            if (chartStock.Diagram is XYDiagram diag)
            {
                diag.AxisX.WholeRange.SideMarginsValue = 1; 
                diag.AxisX.DateTimeScaleOptions.MeasureUnit = DateTimeMeasureUnit.Day;
            }
        }

        private SimpleButton btnMaximize;
        private bool _isMaximized = false;

        private void InitializeMaximizeButton()
        {
            btnMaximize = new SimpleButton();
            btnMaximize.Text = "⛶"; // Symbol for maximize
            btnMaximize.ToolTip = "Tam Ekran Yap (Maximize)";
            btnMaximize.Size = new Size(40, 40);
            btnMaximize.Location = new Point(chartStock.Width - 50, 10);
            btnMaximize.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMaximize.Appearance.BackColor = Color.FromArgb(60, 60, 60);
            btnMaximize.Appearance.ForeColor = Color.White;
            btnMaximize.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnMaximize.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat;
            btnMaximize.Click += (s, e) => ToggleMaximize();
            
            chartStock.Controls.Add(btnMaximize);
        }

        private void ToggleMaximize()
        {
            _isMaximized = !_isMaximized;
            
            // Toggle visibility of surrounding panels
            lstStocks.Visible = !_isMaximized;
            pnlSearch.Visible = !_isMaximized;
            grpTrade.Visible = !_isMaximized;
            pnlBottom.Visible = !_isMaximized; // Hide bottom portfolio panel too
            
            btnMaximize.Text = _isMaximized ? "🔙" : "⛶";
            btnMaximize.ToolTip = _isMaximized ? "Normal Görünüm (Restore)" : "Tam Ekran Yap (Maximize)";
        }

        private ContextMenuStrip _contextMenu;

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Renderer = new ToolStripProfessionalRenderer(new CustomColorTable());

            var itemNote = _contextMenu.Items.Add("📌 Not Ekle");
            itemNote.ForeColor = Color.White;
            itemNote.Click += (s, e) => AddNote();

            var itemSupport = _contextMenu.Items.Add("🟢 Destek Çizgisi (Support)");
            itemSupport.ForeColor = Color.LightGreen;
            itemSupport.Click += (s, e) => AddSupportResistance(true);

            var itemResistance = _contextMenu.Items.Add("🔴 Direnç Çizgisi (Resistance)");
            itemResistance.ForeColor = Color.LightCoral;
            itemResistance.Click += (s, e) => AddSupportResistance(false);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var itemFib = _contextMenu.Items.Add("📐 Otomatik Fibonacci (Auto)");
            itemFib.ForeColor = Color.Gold;
            itemFib.Click += (s, e) => AddFibonacciLevels();

            _contextMenu.Items.Add(new ToolStripSeparator());
            
            var itemBuyTrigger = _contextMenu.Items.Add("🟢 AL Trigger Point (Buy)");
            itemBuyTrigger.ForeColor = Color.LightGreen;
            itemBuyTrigger.Click += (s, e) => AddTriggerPoint(true);
            
            var itemSellTrigger = _contextMenu.Items.Add("🔴 SAT Trigger Point (Sell)");
            itemSellTrigger.ForeColor = Color.LightCoral;
            itemSellTrigger.Click += (s, e) => AddTriggerPoint(false);

            _contextMenu.Items.Add(new ToolStripSeparator());

            var itemClear = _contextMenu.Items.Add("🗑️ Çizimleri Temizle");
            itemClear.ForeColor = Color.White;
            itemClear.Click += (s, e) => ClearAnnotations();

            chartStock.MouseUp += (s, e) => {
                if (e.Button == MouseButtons.Right)
                    _contextMenu.Show(chartStock, e.Location);
            };
        }

        public class CustomColorTable : ProfessionalColorTable
        {
            public override Color MenuItemSelected => Color.FromArgb(60, 60, 60);
            public override Color MenuItemBorder => Color.Gray;
            public override Color MenuBorder => Color.Black;
            public override Color MenuItemSelectedGradientBegin => Color.FromArgb(60, 60, 60);
            public override Color MenuItemSelectedGradientEnd => Color.FromArgb(60, 60, 60);
            public override Color MenuItemPressedGradientBegin => Color.FromArgb(40, 40, 40);
            public override Color MenuItemPressedGradientEnd => Color.FromArgb(40, 40, 40);
            public override Color ToolStripDropDownBackground => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientBegin => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientMiddle => Color.FromArgb(30, 30, 30);
            public override Color ImageMarginGradientEnd => Color.FromArgb(30, 30, 30);
        }

        private void AddNote()
        {
            string note = ShowInputBox("Notunuzu girin:", "Not Ekle");
            if (string.IsNullOrWhiteSpace(note)) return;

            TextAnnotation annotation = new TextAnnotation("Note_" + Guid.NewGuid().ToString(), note);
            annotation.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            annotation.TextColor = Color.White;
            annotation.BackColor = Color.FromArgb(200, 40, 40, 40);
            annotation.Border.Color = Color.Gold;
            
            annotation.ShapePosition = new RelativePosition(0.5, 0.5);
            annotation.RuntimeMoving = true;
            annotation.RuntimeResizing = true;

            chartStock.AnnotationRepository.Add(annotation);
        }

        private void AddSupportResistance(bool isSupport)
        {
            if (chartStock.Diagram is XYDiagram diagram)
            {
                decimal defaultPrice = 100;
                if (lstStocks.SelectedItem is StockInfo stock) defaultPrice = stock.Price;

                string valStr = ShowInputBox($"{(isSupport ? "Destek" : "Direnç")} Seviyesi:", "Seviye Belirle", defaultPrice.ToString());
                if(decimal.TryParse(valStr, out decimal val))
                {
                    string label = ShowInputBox("Etiket (Opsiyonel):", "Başlık", isSupport ? "Güçlü Alım Bölgesi" : "Satış Baskısı");

                    ConstantLine line = new ConstantLine(string.IsNullOrWhiteSpace(label) ? $"{(isSupport ? "Destek" : "Direnç")} : {val:N2}" : $"{label} : {val:N2}");
                    line.AxisValue = val;
                    line.Color = isSupport ? Color.FromArgb(0, 255, 127) : Color.FromArgb(255, 69, 0); 
                    line.LineStyle.Thickness = 2;
                    line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Dash;
                    line.ShowInLegend = false;
                    line.Title.Alignment = ConstantLineTitleAlignment.Far;
                    line.Title.TextColor = line.Color;
                    line.Title.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                    diagram.AxisY.ConstantLines.Add(line);
                }
            }
        }

        private void AddFibonacciLevels()
        {
            if (chartStock.Diagram is XYDiagram diagram && chartStock.Series.Count > 0)
            {
                decimal currentPrice = 0;
                if (lstStocks.SelectedItem is StockInfo stock) currentPrice = stock.Price;
                else return;

                decimal high = currentPrice * 1.1m;
                decimal low = currentPrice * 0.9m;
                decimal diff = high - low;

                decimal[] fibs = { 0m, 0.236m, 0.382m, 0.5m, 0.618m, 1.0m };

                foreach (var f in fibs)
                {
                    decimal levelPrice = low + (diff * f);
                    ConstantLine line = new ConstantLine($"Fib {f:P1}");
                    line.AxisValue = levelPrice;
                    line.Color = Color.FromArgb(100, 255, 215, 0); 
                    line.LineStyle.Thickness = 1;
                    line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.Solid;
                    line.ShowInLegend = false;
                    line.Title.Alignment = ConstantLineTitleAlignment.Near;
                    line.Title.TextColor = Color.Gold;
                    
                    diagram.AxisY.ConstantLines.Add(line);
                }
                
                XtraMessageBox.Show("Fibonacci Seviyeleri (Simüle) Eklendi!", "Analiz Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearAnnotations()
        {
            chartStock.AnnotationRepository.Clear();
            if (chartStock.Diagram is XYDiagram diagram)
            {
                diagram.AxisY.ConstantLines.Clear();
            }
        }

        private string ShowInputBox(string prompt, string title, string defaultValue = "")
        {
            Form inputForm = new Form();
            inputForm.Size = new Size(350, 180);
            inputForm.Text = title;
            inputForm.StartPosition = FormStartPosition.CenterParent;
            inputForm.FormBorderStyle = FormBorderStyle.FixedDialog;
            inputForm.MaximizeBox = false;
            inputForm.MinimizeBox = false;
            inputForm.BackColor = Color.FromArgb(35, 35, 35);
            inputForm.ForeColor = Color.White;

            Label lbl = new Label() { Left = 20, Top = 20, Text = prompt, AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
            TextBox txt = new TextBox() { Left = 20, Top = 50, Width = 290, Text = defaultValue, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 10) };
            
            Button btnOk = new Button() { Text = "Tamam", Left = 210, Top = 90, Width = 100, Height = 35, DialogResult = DialogResult.OK, BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOk.FlatAppearance.BorderSize = 0;
            
            inputForm.Controls.Add(lbl);
            inputForm.Controls.Add(txt);
            inputForm.Controls.Add(btnOk);
            inputForm.AcceptButton = btnOk;

            return inputForm.ShowDialog() == DialogResult.OK ? txt.Text : "";
        }
        
        private void AddTriggerPoint(bool isBuy)
        {
            if (chartStock.Diagram is XYDiagram diagram)
            {
                decimal defaultPrice = 100;
                if (lstStocks.SelectedItem is StockInfo stock) defaultPrice = stock.Price;

                string valStr = ShowInputBox($"{(isBuy ? "AL" : "SAT")} Trigger Fiyatı:", "Trigger Point", defaultPrice.ToString());
                if(decimal.TryParse(valStr, out decimal val))
                {
                    string label = ShowInputBox("Açıklama (Opsiyonel):", "Trigger Notu", 
                        isBuy ? "AL Sinyali - Güçlü Alım Fırsatı" : "SAT Sinyali - Kâr Al");

                    // Add constant line for the trigger
                    ConstantLine line = new ConstantLine(string.IsNullOrWhiteSpace(label) ? 
                        $"{(isBuy ? "🟢 AL" : "🔴 SAT")} : {val:N2}" : 
                        $"{(isBuy ? "🟢" : "🔴")} {label} : {val:N2}");
                    line.AxisValue = val;
                    line.Color = isBuy ? Color.FromArgb(0, 255, 127) : Color.FromArgb(255, 69, 0);
                    line.LineStyle.Thickness = 3;
                    line.LineStyle.DashStyle = DevExpress.XtraCharts.DashStyle.DashDot;
                    line.ShowInLegend = false;
                    line.Title.Alignment = ConstantLineTitleAlignment.Far;
                    line.Title.TextColor = line.Color;
                    line.Title.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                    line.Title.ShowBelowLine = !isBuy; // Sell triggers show above, buy below

                    diagram.AxisY.ConstantLines.Add(line);
                    
                    // Add annotation marker
                    TextAnnotation annotation = new TextAnnotation("Trigger_" + Guid.NewGuid().ToString(), 
                        isBuy ? "🎯 AL" : "🎯 SAT");
                    annotation.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    annotation.TextColor = Color.White;
                    annotation.BackColor = isBuy ? Color.FromArgb(200, 0, 200, 100) : Color.FromArgb(200, 200, 0, 0);
                    annotation.Border.Color = line.Color;
                    annotation.Border.Thickness = 2;
                    annotation.ShapePosition = new RelativePosition(0.85, isBuy ? 0.7 : 0.3);
                    annotation.RuntimeMoving = true;
                    
                    chartStock.AnnotationRepository.Add(annotation);
                    
                    XtraMessageBox.Show($"{(isBuy ? "AL" : "SAT")} Trigger Point {val:N2} TL seviyesinde eklendi!", 
                        "Trigger Eklendi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void GenerateFallbackChart(Series series, string symbol, decimal startPrice)
        {
            Random rnd = new Random(symbol.GetHashCode());
            double price = (double)startPrice * 0.8; 
            DateTime date = DateTime.Now.AddDays(-60);

            for (int i = 0; i < 60; i++)
            {
                double changePercent = (rnd.NextDouble() * 0.04) - 0.02; 
                double open = price;
                double close = price * (1 + changePercent);
                
                double high = Math.Max(open, close) * (1 + (rnd.NextDouble() * 0.01));
                double low = Math.Min(open, close) * (1 - (rnd.NextDouble() * 0.01));

                series.Points.Add(new SeriesPoint(date.AddDays(i), low, high, open, close));
                
                price = close; 
            }
            
            if(lblPrice != null) lblPrice.Text = $"${price:N2}";
        }

        private void btnBuy_Click(object sender, EventArgs e) => XtraMessageBox.Show("Buy Order Placed!", "Broker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnSell_Click(object sender, EventArgs e) => XtraMessageBox.Show("Sell Order Placed!", "Broker", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
