using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Controls
{
    public partial class InvestmentOpportunitiesWidget : UserControl
    {
        private System.Windows.Forms.Timer rotationTimer;
        private int currentIndex = 0;
        private string[] opportunities = new[]
        {
            "🔥 THYAO -%4 düştü - Dip fırsatı!",
            "🚀 Bitcoin $100,000'ı aştı!",
            "📈 Altın tüm zamanların zirvesinde",
            "💎 EUR/TRY düşüş trendinde"
        };

        public InvestmentOpportunitiesWidget()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            
            rotationTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            rotationTimer.Tick += (s, e) => { currentIndex = (currentIndex + 1) % opportunities.Length; this.Invalidate(); };
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

            // Rounded rectangle
            GraphicsPath path = GetRoundedRect(this.ClientRectangle, 20);
            
            // Card background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
            {
                g.FillPath(bgBrush, path);
            }

            // Accent border
            using (Pen borderPen = new Pen(Color.FromArgb(255, 0, 122), 3))
            {
                g.DrawPath(borderPen, path);
            }

            // Title
            using (Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(255, 0, 122)))
            {
                g.DrawString("Yatırım Fırsatları", titleFont, titleBrush, new PointF(20, 20));
            }

            // Current opportunity text
            using (Font oppFont = new Font("Segoe UI", 18, FontStyle.Bold))
            using (SolidBrush oppBrush = new SolidBrush(Color.White))
            {
                string text = opportunities[currentIndex];
                SizeF size = g.MeasureString(text, oppFont, this.Width - 40);
                float y = (this.Height - size.Height) / 2 + 20;
                g.DrawString(text, oppFont, oppBrush, new RectangleF(20, y, this.Width - 40, size.Height));
            }

            // Dots indicator
            int dotY = this.Height - 30;
            for (int i = 0; i < opportunities.Length; i++)
            {
                Color dotColor = i == currentIndex ? Color.FromArgb(255, 0, 122) : Color.FromArgb(80, 80, 80);
                using (SolidBrush dotBrush = new SolidBrush(dotColor))
                {
                    g.FillEllipse(dotBrush, 20 + i * 15, dotY, 8, 8);
                }
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
