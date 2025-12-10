using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services;
using Dapper;

namespace BankApp.UI.Controls
{
    public partial class InvestmentOpportunitiesWidget : UserControl
    {
        private System.Windows.Forms.Timer rotationTimer;
        private int currentIndex = 0;
        private List<string> opportunities = new List<string>();

        public InvestmentOpportunitiesWidget()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            
            LoadOpportunities();
            
            rotationTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            rotationTimer.Tick += (s, e) => { currentIndex = (currentIndex + 1) % Math.Max(1, opportunities.Count); this.Invalidate(); };
            rotationTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 200);
            this.Paint += InvestmentOpportunitiesWidget_Paint;
        }

        private void InvestmentOpportunitiesWidget_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Rounded rectangle
            GraphicsPath path = GetRoundedRect(this.ClientRectangle, 12);
            
            // Card background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.FillPath(bgBrush, path);
            }

            // Subtle border
            using (Pen borderPen = new Pen(Color.FromArgb(60, 60, 60), 1))
            {
                g.DrawPath(borderPen, path);
            }

            // Title with accent
            using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(236, 72, 153)))
            {
                g.DrawString("📈 Yatırım Fırsatları", titleFont, titleBrush, new PointF(16, 14));
            }

            // Show skeleton rows if loading, otherwise show opportunities
            if (!opportunities.Any() || opportunities[0].Contains("yükleniyor"))
            {
                // Skeleton loading rows
                DrawSkeletonRow(g, 16, 50, "BTC", "Bitcoin", "₺---,---", true);
                DrawSkeletonRow(g, 16, 90, "THYAO", "Türk Hava Yolları", "₺---.--", false);
            }
            else
            {
                // Current opportunity text
                using (Font oppFont = new Font("Segoe UI", 14, FontStyle.Bold))
                using (SolidBrush oppBrush = new SolidBrush(Color.White))
                {
                    string text = opportunities[currentIndex];
                    SizeF size = g.MeasureString(text, oppFont, this.Width - 40);
                    float y = 55;
                    g.DrawString(text, oppFont, oppBrush, new RectangleF(16, y, this.Width - 32, size.Height + 20));
                }
            }

            // Dots indicator
            int dotY = this.Height - 24;
            int count = Math.Max(1, opportunities.Count);
            int dotsStartX = (this.Width - (count * 12)) / 2;
            for (int i = 0; i < Math.Min(count, 5); i++)
            {
                Color dotColor = i == currentIndex ? Color.FromArgb(236, 72, 153) : Color.FromArgb(60, 60, 60);
                using (SolidBrush dotBrush = new SolidBrush(dotColor))
                {
                    g.FillEllipse(dotBrush, dotsStartX + i * 12, dotY, 6, 6);
                }
            }
        }
        
        private void DrawSkeletonRow(Graphics g, int x, int y, string symbol, string name, string price, bool isUp)
        {
            // Symbol badge
            using (SolidBrush badgeBrush = new SolidBrush(Color.FromArgb(45, 45, 45)))
            using (Font symbolFont = new Font("Segoe UI", 9, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                g.FillRectangle(badgeBrush, x, y, 50, 24);
                g.DrawString(symbol, symbolFont, textBrush, x + 5, y + 4);
            }
            
            // Name
            using (Font nameFont = new Font("Segoe UI", 10))
            using (SolidBrush nameBrush = new SolidBrush(Color.FromArgb(120, 120, 120)))
            {
                g.DrawString(name, nameFont, nameBrush, x + 60, y + 3);
            }
            
            // Price placeholder (animated shimmer effect simulated)
            using (SolidBrush priceBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
            {
                g.FillRectangle(priceBrush, this.Width - 100, y + 2, 80, 18);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }

        private async void LoadOpportunities()
        {
            try
            {
                var context = new DapperContext();
                using var conn = context.CreateConnection();
                
                // Simple query - get stocks if table exists
                var stocks = await conn.QueryAsync<dynamic>(@"
                    SELECT ""Symbol"", ""Name"", ""CurrentPrice"", ""ChangePercent""
                    FROM ""Stocks""
                    ORDER BY ABS(""ChangePercent"") DESC
                    LIMIT 5");
                
                var stockList = stocks.ToList();
                System.Diagnostics.Debug.WriteLine($"[OPPORTUNITIES] loaded count={stockList.Count}");
                
                if (stockList.Any())
                {
                    opportunities = new List<string>();
                    foreach (var s in stockList)
                    {
                        string symbol = s.Symbol?.ToString() ?? "";
                        decimal change = (decimal)(s.ChangePercent ?? 0);
                        string emoji = change > 2 ? "🚀" : (change < -2 ? "📉" : "📊");
                        string sign = change >= 0 ? "+" : "";
                        opportunities.Add($"{emoji} {symbol} {sign}{change:N1}%");
                    }
                }
                else
                {
                    SetDefaultOpportunities();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OPPORTUNITIES] Error: {ex.Message}");
                SetDefaultOpportunities();
            }
            
            this.Invalidate();
        }
        
        private void SetDefaultOpportunities()
        {
            opportunities = new List<string>
            {
                "📈 BTC - Kripto piyasası",
                "📊 THYAO - BIST yıldızı",
                "💎 AAPL - Teknoloji lideri",
                "🔔 Piyasaları takip edin"
            };
            System.Diagnostics.Debug.WriteLine("[OPPORTUNITIES] Using default data");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && rotationTimer != null)
            {
                rotationTimer.Stop();
                rotationTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
