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
        public event EventHandler QRPayClicked;
        public event EventHandler ExchangeClicked;
        public event EventHandler SupportClicked;

        public QuickActionsBar()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 120);
            this.BackColor = Color.FromArgb(18, 18, 18);
            
            CreateActionButton("💸\nPara Gönder", 50, (s, e) => SendMoneyClicked?.Invoke(this, e));
            // QR Pay and Exchange removed as requested
            CreateActionButton("🎧\nDestek", 230, (s, e) => SupportClicked?.Invoke(this, e));
        }

        private void CreateActionButton(string text, int x, EventHandler onClick)
        {
            Panel pnl = new Panel
            {
                Size = new Size(100, 100),
                Location = new Point(x, 10),
                Cursor = Cursors.Hand
            };

            pnl.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Circular background
                using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(30, 30, 30)))
                {
                    g.FillEllipse(bgBrush, 10, 10, 80, 80);
                }

                // Accent border
                using (Pen borderPen = new Pen(Color.FromArgb(0, 210, 255), 2))
                {
                    g.DrawEllipse(borderPen, 10, 10, 80, 80);
                }

                // Text
                using (Font font = new Font("Segoe UI", 11, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(text, font, textBrush, new RectangleF(0, 0, 100, 100), sf);
                }
            };

            pnl.Click += onClick;
            pnl.MouseEnter += (s, e) => { pnl.BackColor = Color.FromArgb(40, 40, 40); pnl.Invalidate(); };
            pnl.MouseLeave += (s, e) => { pnl.BackColor = Color.Transparent; pnl.Invalidate(); };

            this.Controls.Add(pnl);
        }
    }
}
