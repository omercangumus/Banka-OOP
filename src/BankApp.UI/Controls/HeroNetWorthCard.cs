using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BankApp.UI.Controls
{
    public partial class HeroNetWorthCard : UserControl
    {
        private decimal netWorth;
        private decimal totalDebt;
        private decimal trendPercentage;
        private bool isTrendUp;
        private System.Windows.Forms.Timer carouselTimer;
        private bool showingDebt = false;
        private string userIban = "";

        public HeroNetWorthCard()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(400, 150);
            
            // Carousel Timer (5 seconds)
            carouselTimer = new System.Windows.Forms.Timer { Interval = 5000 };
            carouselTimer.Tick += (s, e) => {
                showingDebt = !showingDebt;
                this.Invalidate(); // Repaint
            };
            carouselTimer.Start();
        }

        public void SetNetWorth(decimal worth, decimal debt, decimal trend, bool isUp, string iban = "")
        {
            this.netWorth = worth;
            this.totalDebt = debt;
            this.trendPercentage = trend;
            this.isTrendUp = isUp;
            this.userIban = iban;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background gradient
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(58, 123, 213),
                Color.FromArgb(88, 86, 214),
                45f))
            {
                GraphicsPath path = new GraphicsPath();
                path.AddArc(0, 0, 20, 20, 180, 90);
                path.AddArc(this.Width - 20, 0, 20, 20, 270, 90);
                path.AddArc(this.Width - 20, this.Height - 20, 20, 20, 0, 90);
                path.AddArc(0, this.Height - 20, 20, 20, 90, 90);
                path.CloseFigure();

                g.FillPath(brush, path);
            }

            // Title
            string title = showingDebt ? "💳 Toplam Kredi Borcum" : "💰 Toplam Net Varlık";
            using (Font titleFont = new Font("Segoe UI", 14, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.DrawString(title, titleFont, titleBrush, 20, 15);
            }

            // Main value
            decimal displayValue = showingDebt ? totalDebt : netWorth;
            string valueText = $"₺{displayValue:N2}";
            using (Font valueFont = new Font("Segoe UI", 28, FontStyle.Bold))
            using (SolidBrush valueBrush = new SolidBrush(Color.White))
            {
                g.DrawString(valueText, valueFont, valueBrush, 20, 45);
            }

            // IBAN Display
            if (!string.IsNullOrEmpty(userIban))
            {
                using (Font ibanFont = new Font("Consolas", 10, FontStyle.Regular))
                using (SolidBrush ibanBrush = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                {
                    g.DrawString($"IBAN: {userIban}", ibanFont, ibanBrush, 20, 90);
                }
            }

            // Context message
            string contextMsg = showingDebt ? "Ödenecek tutarlar" : (isTrendUp ? "↑ Artış: %" + trendPercentage : "↓ Azalma: %" + trendPercentage);
            using (Font msgFont = new Font("Segoe UI", 10, FontStyle.Regular))
            using (SolidBrush msgBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.DrawString(contextMsg, msgFont, msgBrush, 20, 110);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && carouselTimer != null)
            {
                carouselTimer.Stop();
                carouselTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
