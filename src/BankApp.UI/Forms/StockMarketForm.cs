using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;

namespace BankApp.UI.Forms
{
    public partial class StockMarketForm : XtraForm
    {
        public StockMarketForm()
        {
            InitializeComponent();
            SetupChartVisuals(); 
            InitializeContextMenu();
            LoadMockStocks();   
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

        private void LoadMockStocks()
        {
            var stocks = new List<StockInfo>
            {
                new StockInfo { Symbol = "THYAO", Name = "Türk Hava Yolları", Price = 285.50m },
                new StockInfo { Symbol = "GARAN", Name = "Garanti BBVA", Price = 105.20m },
                new StockInfo { Symbol = "ASELS", Name = "Aselsan", Price = 62.10m },
                new StockInfo { Symbol = "SASA", Name = "Sasa Polyester", Price = 42.80m },
                new StockInfo { Symbol = "EREGL", Name = "Ereğli Demir Çelik", Price = 48.90m },
                new StockInfo { Symbol = "BTCUSD", Name = "Bitcoin", Price = 95000m }
            };
            
            lstStocks.Items.Clear();
            foreach(var stock in stocks)
            {
                lstStocks.Items.Add(stock);
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

        private void GenerateCandleChart(string symbol, decimal startPrice)
        {
            chartStock.Series.Clear();

            Series series = new Series(symbol, ViewType.CandleStick);
            series.ArgumentScaleType = ScaleType.DateTime;

            Random rnd = new Random(symbol.GetHashCode());
            double price = (double)startPrice * 0.8; 
            DateTime date = DateTime.Now.AddDays(-100);

            for (int i = 0; i < 100; i++)
            {
                double changePercent = (rnd.NextDouble() * 0.04) - 0.02; 
                double open = price;
                double close = price * (1 + changePercent);
                
                double high = Math.Max(open, close) * (1 + (rnd.NextDouble() * 0.01));
                double low = Math.Min(open, close) * (1 - (rnd.NextDouble() * 0.01));

                series.Points.Add(new SeriesPoint(date.AddDays(i), low, high, open, close));
                
                price = close; 
            }
            
            if(lblPrice != null) lblPrice.Text = $"{price:N2} TL";

            if(series.View is CandleStickSeriesView view)
            {
                view.Color = Color.FromArgb(0, 255, 0); 
                view.ReductionOptions.Color = Color.FromArgb(255, 0, 0); 
                view.ReductionOptions.Visible = true;
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

        private void btnBuy_Click(object sender, EventArgs e) => XtraMessageBox.Show("Alım Emri İletildi!", "Broker", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void btnSell_Click(object sender, EventArgs e) => XtraMessageBox.Show("Satış Emri İletildi!", "Broker", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public class StockInfo 
    { 
        public string Symbol { get; set; } 
        public string Name { get; set; } 
        public decimal Price { get; set; } 
        
        public override string ToString() => $"{Symbol} - {Name}";
    }
}
