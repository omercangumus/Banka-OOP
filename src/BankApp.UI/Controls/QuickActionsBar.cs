using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Controls
{
    public partial class QuickActionsBar : UserControl
    {
        public event EventHandler SendMoneyClicked;
        public event EventHandler SupportClicked;

        public QuickActionsBar()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(400, 80);
            this.BackColor = Color.Transparent;
            this.Padding = new Padding(0);
            
            // Bankacılık tile butonları
            int tileWidth = 180;
            int tileHeight = 60;
            int spacing = 15;
            
            CreateTileButton("💸", "Para Gönder", "Hesaplar arası transfer", 0, tileWidth, tileHeight, 
                Color.FromArgb(59, 130, 246), (s, e) => SendMoneyClicked?.Invoke(this, e));
            
            CreateTileButton("🎧", "Destek", "7/24 canlı destek", tileWidth + spacing, tileWidth, tileHeight,
                Color.FromArgb(139, 92, 246), (s, e) => SupportClicked?.Invoke(this, e));
        }

        private void CreateTileButton(string icon, string title, string subtitle, int x, int width, int height, Color accentColor, EventHandler onClick)
        {
            Panel pnl = new Panel
            {
                Size = new Size(width, height),
                Location = new Point(x, 10),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(38, 38, 38)
            };

            pnl.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                // Rounded rectangle background
                using (GraphicsPath path = CreateRoundedRect(0, 0, width - 1, height - 1, 10))
                {
                    using (SolidBrush bgBrush = new SolidBrush(pnl.BackColor))
                    {
                        g.FillPath(bgBrush, path);
                    }
                    
                    // Left accent bar
                    using (SolidBrush accentBrush = new SolidBrush(accentColor))
                    {
                        g.FillRectangle(accentBrush, 0, 8, 4, height - 16);
                    }
                }

                // Icon
                using (Font iconFont = new Font("Segoe UI Emoji", 18))
                using (SolidBrush iconBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(icon, iconFont, iconBrush, 14, 12);
                }

                // Title
                using (Font titleFont = new Font("Segoe UI", 11, FontStyle.Bold))
                using (SolidBrush titleBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(title, titleFont, titleBrush, 50, 12);
                }

                // Subtitle
                using (Font subFont = new Font("Segoe UI", 8))
                using (SolidBrush subBrush = new SolidBrush(Color.FromArgb(156, 163, 175)))
                {
                    g.DrawString(subtitle, subFont, subBrush, 50, 32);
                }
            };

            pnl.Click += onClick;
            pnl.MouseEnter += (s, e) => { pnl.BackColor = Color.FromArgb(48, 48, 48); pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { pnl.BackColor = Color.FromArgb(38, 38, 38); pnl.Invalidate(); };

            this.Controls.Add(pnl);
        }
        
        private GraphicsPath CreateRoundedRect(int x, int y, int width, int height, int radius)
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
}
