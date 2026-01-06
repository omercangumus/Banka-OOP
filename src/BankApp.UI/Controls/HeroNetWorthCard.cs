using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BankApp.UI.Controls
{
    public partial class HeroNetWorthCard : UserControl
    {
        private decimal netWorth;
        private decimal totalBalance;
        private decimal totalDebt;
        private decimal monthlyChange;
        private int activeAccounts;
        private string userIban = "";
        private Rectangle ibanRect;
        private ToolTip copyTooltip;
        private bool ibanHovered = false;

        public HeroNetWorthCard()
        {
            this.DoubleBuffered = true;
            this.Size = new Size(480, 200);
            this.MinimumSize = new Size(400, 180);
            
            // IBAN copy tooltip
            copyTooltip = new ToolTip();
            copyTooltip.InitialDelay = 0;
            copyTooltip.ShowAlways = true;
            
            this.MouseClick += HeroNetWorthCard_MouseClick;
            this.MouseMove += HeroNetWorthCard_MouseMove;
        }
        
        private void HeroNetWorthCard_MouseClick(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] HANDLER: HeroNetWorthCard clicked at ({e.X},{e.Y}), control={GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"[IBAN] MouseClick at ({e.X},{e.Y}), ibanRect={ibanRect}, userIban={userIban}");
            
            // DEBUG: Her click'te MessageBox göster
            DevExpress.XtraEditors.XtraMessageBox.Show(
                $"CLICK DEBUG:\n\nClick Pozisyon: ({e.X}, {e.Y})\nIBAN Rect: {ibanRect}\nIBAN: {userIban ?? "(boş)"}\n\nRect içinde mi? {ibanRect.Contains(e.Location)}",
                "HeroNetWorthCard Click Test",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            // Check if click is on IBAN area
            if (!string.IsNullOrEmpty(userIban) && ibanRect.Contains(e.Location))
            {
                try
                {
                    Clipboard.SetText(userIban);
                    copyTooltip.Show("✓ IBAN kopyalandı!", this, e.X, e.Y - 25, 1500);
                    System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] IBAN COPY SUCCESS: {userIban}");
                    System.Diagnostics.Debug.WriteLine($"[IBAN] IBAN COPIED TO CLIPBOARD: {userIban}");
                    
                    // MessageBox onay
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        $"✓ IBAN KOPYALANDI!\n\n{userIban}\n\nPanoya kopyalandı.",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex) 
                { 
                    System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] IBAN COPY FAILED: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[IBAN] Copy failed: {ex.Message}");
                    DevExpress.XtraEditors.XtraMessageBox.Show(
                        $"IBAN kopyalama hatası:\n{ex.Message}",
                        "Hata",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] Click outside IBAN area or no IBAN set");
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    $"IBAN alanı dışında tıkladınız veya IBAN set edilmemiş.\n\nIBAN: {userIban ?? "(yok)"}\nRect: {ibanRect}\nClick: ({e.X}, {e.Y})",
                    "IBAN Kopyalanamadı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        
        private void HeroNetWorthCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(userIban) && ibanRect.Contains(e.Location))
            {
                if (!ibanHovered)
                {
                    ibanHovered = true;
                    this.Cursor = Cursors.Hand;
                    copyTooltip.SetToolTip(this, "Tıkla: IBAN'ı kopyala");
                }
            }
            else
            {
                if (ibanHovered)
                {
                    ibanHovered = false;
                    this.Cursor = Cursors.Default;
                    copyTooltip.SetToolTip(this, "");
                }
            }
        }

        public void SetNetWorth(decimal worth, decimal debt, decimal trend, bool isUp, string iban = "")
        {
            this.netWorth = worth;
            this.totalDebt = debt;
            this.monthlyChange = trend;
            this.userIban = iban;
            this.Invalidate();
        }
        
        public void SetFullData(decimal balance, decimal debt, decimal netWorthValue, decimal change, int accounts, string iban)
        {
            this.totalBalance = balance;
            this.totalDebt = debt;
            this.netWorth = netWorthValue;
            this.monthlyChange = change;
            this.activeAccounts = accounts;
            this.userIban = iban;
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cornerRadius = 16;
            int padding = 20;
            
            // Premium gradient background
            Color gradientStart = Color.FromArgb(30, 58, 138); // Deep blue
            Color gradientEnd = Color.FromArgb(79, 70, 229);   // Purple-ish blue

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle, gradientStart, gradientEnd, 135f))
            {
                GraphicsPath path = CreateRoundedRectangle(0, 0, this.Width - 1, this.Height - 1, cornerRadius);
                g.FillPath(brush, path);
            }
            
            // Glass effect overlay (top)
            using (LinearGradientBrush glassBrush = new LinearGradientBrush(
                new Rectangle(0, 0, this.Width, this.Height / 3),
                Color.FromArgb(40, 255, 255, 255),
                Color.FromArgb(0, 255, 255, 255), 90f))
            {
                GraphicsPath glassPath = CreateRoundedRectangle(1, 1, this.Width - 3, this.Height / 3, cornerRadius);
                g.FillPath(glassBrush, glassPath);
            }

            // === TOP SECTION: Title + Badge ===
            using (Font titleFont = new Font("Segoe UI", 11, FontStyle.Regular))
            using (SolidBrush titleBrush = new SolidBrush(Color.FromArgb(200, 255, 255, 255)))
            {
                g.DrawString("💰 Toplam Net Varlık", titleFont, titleBrush, padding, padding);
            }
            
            // Badge: "Aktif Hesap: N"
            if (activeAccounts > 0)
            {
                string badgeText = $"🏦 {activeAccounts} Hesap";
                using (Font badgeFont = new Font("Segoe UI", 9, FontStyle.Regular))
                {
                    SizeF badgeSize = g.MeasureString(badgeText, badgeFont);
                    int badgeX = this.Width - padding - (int)badgeSize.Width - 16;
                    int badgeY = padding - 2;
                    
                    // Badge background
                    using (SolidBrush badgeBg = new SolidBrush(Color.FromArgb(60, 255, 255, 255)))
                    {
                        g.FillRoundedRectangle(badgeBg, badgeX - 8, badgeY - 2, (int)badgeSize.Width + 16, (int)badgeSize.Height + 4, 10);
                    }
                    
                    using (SolidBrush badgeBrush = new SolidBrush(Color.White))
                    {
                        g.DrawString(badgeText, badgeFont, badgeBrush, badgeX, badgeY);
                    }
                }
            }

            // === MAIN VALUE ===
            string valueText = $"₺{netWorth:N2}";
            using (Font valueFont = new Font("Segoe UI", 36, FontStyle.Bold))
            using (SolidBrush valueBrush = new SolidBrush(Color.White))
            {
                g.DrawString(valueText, valueFont, valueBrush, padding, padding + 28);
            }
            
            // === BOTTOM SECTION: Sub-metrics ===
            int bottomY = this.Height - 60;
            int metricWidth = (this.Width - padding * 2) / 3;
            
            // Metric 1: Toplam Bakiye
            DrawMetric(g, padding, bottomY, "Toplam Bakiye", $"₺{totalBalance:N0}", Color.FromArgb(134, 239, 172));
            
            // Metric 2: Toplam Borç
            DrawMetric(g, padding + metricWidth, bottomY, "Toplam Borç", $"₺{totalDebt:N0}", Color.FromArgb(252, 165, 165));
            
            // Metric 3: 30 Gün Değişim
            string changeText = monthlyChange >= 0 ? $"+₺{monthlyChange:N0}" : $"-₺{Math.Abs(monthlyChange):N0}";
            Color changeColor = monthlyChange >= 0 ? Color.FromArgb(134, 239, 172) : Color.FromArgb(252, 165, 165);
            DrawMetric(g, padding + metricWidth * 2, bottomY, "30 Gün Değişim", changeText, changeColor);
            
            // === IBAN (bottom right) - Clickable ===
            if (!string.IsNullOrEmpty(userIban))
            {
                using (Font ibanFont = new Font("Consolas", 9, FontStyle.Regular))
                {
                    string maskedIban = userIban.Length > 10 
                        ? userIban.Substring(0, 4) + " •••• •••• " + userIban.Substring(userIban.Length - 4)
                        : userIban;
                    string displayText = "📋 " + maskedIban;
                    SizeF ibanSize = g.MeasureString(displayText, ibanFont);
                    
                    int ibanX = this.Width - padding - (int)ibanSize.Width;
                    int ibanY = this.Height - 24;
                    
                    // Store rect for click detection
                    ibanRect = new Rectangle(ibanX - 4, ibanY - 2, (int)ibanSize.Width + 8, (int)ibanSize.Height + 4);
                    
                    // Hover highlight
                    if (ibanHovered)
                    {
                        using (SolidBrush hoverBg = new SolidBrush(Color.FromArgb(40, 255, 255, 255)))
                        {
                            g.FillRoundedRectangle(hoverBg, ibanRect.X, ibanRect.Y, ibanRect.Width, ibanRect.Height, 4);
                        }
                    }
                    
                    using (SolidBrush ibanBrush = new SolidBrush(Color.FromArgb(ibanHovered ? 255 : 180, 255, 255, 255)))
                    {
                        g.DrawString(displayText, ibanFont, ibanBrush, ibanX, ibanY);
                    }
                }
            }
        }
        
        private void DrawMetric(Graphics g, int x, int y, string label, string value, Color valueColor)
        {
            using (Font labelFont = new Font("Segoe UI", 8, FontStyle.Regular))
            using (Font valueFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
            using (SolidBrush valueBrush = new SolidBrush(valueColor))
            {
                g.DrawString(label, labelFont, labelBrush, x, y);
                g.DrawString(value, valueFont, valueBrush, x, y + 14);
            }
        }
        
        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(x, y, diameter, diameter, 180, 90);
            path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
            path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
            path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
    }
    
    // Extension method for rounded rectangle fill
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics g, Brush brush, int x, int y, int width, int height, int radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                int diameter = radius * 2;
                path.AddArc(x, y, diameter, diameter, 180, 90);
                path.AddArc(x + width - diameter, y, diameter, diameter, 270, 90);
                path.AddArc(x + width - diameter, y + height - diameter, diameter, diameter, 0, 90);
                path.AddArc(x, y + height - diameter, diameter, diameter, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }
        }
    }
}
