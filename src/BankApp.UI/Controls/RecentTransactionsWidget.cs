using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dapper;
using BankApp.Infrastructure.Data;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Services;
using BankApp.Core.Entities;

namespace BankApp.UI.Controls
{
    public partial class RecentTransactionsWidget : UserControl
    {
        private readonly IDashboardService _dashboardService;
        private Panel pnlContent;
        private Label lblTitle;
        private Panel pnlEmpty;
        
        public RecentTransactionsWidget()
        {
            var context = new DapperContext();
            _dashboardService = new DashboardService(context);
            
            InitializeComponent();
            LoadTransactions();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 300);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Padding = new Padding(15);
            this.DoubleBuffered = true;
            
            // Title
            lblTitle = new Label
            {
                Text = "📋 Son İşlemler",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);
            
            // Content panel
            pnlContent = new Panel
            {
                Location = new Point(15, 45),
                Size = new Size(this.Width - 30, this.Height - 60),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            this.Controls.Add(pnlContent);
            
            // Empty state panel
            pnlEmpty = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Visible = false
            };
            
            var lblEmptyIcon = new Label
            {
                Text = "📭",
                Font = new Font("Segoe UI Emoji", 36),
                ForeColor = Color.FromArgb(100, 100, 100),
                AutoSize = false,
                Size = new Size(pnlEmpty.Width, 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            };
            
            var lblEmptyText = new Label
            {
                Text = "Henüz işlem bulunmuyor",
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = false,
                Size = new Size(pnlEmpty.Width, 30),
                TextAlign = ContentAlignment.TopCenter,
                Dock = DockStyle.Top
            };
            
            pnlEmpty.Controls.Add(lblEmptyText);
            pnlEmpty.Controls.Add(lblEmptyIcon);
            pnlContent.Controls.Add(pnlEmpty);
        }

        public async void RefreshData()
        {
            await LoadTransactionsAsync();
        }

        private async void LoadTransactions()
        {
            await LoadTransactionsAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            try
            {
                var transactions = await _dashboardService.GetRecentTransactionsAsync(AppEvents.CurrentSession.UserId, 5);

                pnlContent.Controls.Clear();
                pnlContent.Controls.Add(pnlEmpty);

                if (!transactions.Any())
                {
                    pnlEmpty.Visible = true;
                    return;
                }

                pnlEmpty.Visible = false;
                int y = 0;
                foreach (var tx in transactions)
                {
                    Panel txPanel = CreateTransactionRow(tx, y);
                    pnlContent.Controls.Add(txPanel);
                    y += 48;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTransactions Error: {ex.Message}");
                pnlEmpty.Visible = true;
            }
        }

        private Panel CreateTransactionRow(RecentTransactionDto tx, int yPos)
        {
            Panel pnl = new Panel
            {
                Size = new Size(pnlContent.Width - 5, 44),
                Location = new Point(0, yPos),
                BackColor = Color.FromArgb(38, 38, 38),
                Cursor = Cursors.Hand
            };
            
            // Hover effect
            pnl.MouseEnter += (s, e) => pnl.BackColor = Color.FromArgb(48, 48, 48);
            pnl.MouseLeave += (s, e) => pnl.BackColor = Color.FromArgb(38, 38, 38);

            // Icon background
            Panel pnlIcon = new Panel
            {
                Size = new Size(36, 36),
                Location = new Point(6, 4),
                BackColor = GetIconBackgroundColor(tx.TransactionType)
            };
            
            Label lblIcon = new Label
            {
                Text = GetIcon(tx.TransactionType),
                Font = new Font("Segoe UI Emoji", 14),
                Size = new Size(36, 36),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };
            pnlIcon.Controls.Add(lblIcon);

            // Description
            Label lblDesc = new Label
            {
                Text = tx.Description ?? "İşlem",
                Font = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = Color.White,
                Location = new Point(50, 6),
                AutoSize = true
            };
            
            // Date
            Label lblDate = new Label
            {
                Text = tx.TransactionDate.ToString("dd MMM HH:mm"),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(128, 128, 128),
                Location = new Point(50, 24),
                AutoSize = true
            };

            // Amount
            bool isIncome = tx.TransactionType == "Deposit" || tx.TransactionType == "TransferIn";
            Color amountColor = isIncome ? Color.FromArgb(34, 197, 94) : Color.FromArgb(239, 68, 68);
            
            Label lblAmount = new Label
            {
                Text = $"{(isIncome ? "+" : "-")}₺{tx.Amount:N2}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = amountColor,
                AutoSize = true
            };
            lblAmount.Location = new Point(pnl.Width - lblAmount.PreferredWidth - 10, 12);

            pnl.Controls.AddRange(new Control[] { pnlIcon, lblDesc, lblDate, lblAmount });
            return pnl;
        }
        
        private Color GetIconBackgroundColor(string type)
        {
            return type switch
            {
                "Deposit" => Color.FromArgb(34, 197, 94, 40),
                "Withdraw" => Color.FromArgb(239, 68, 68, 40),
                "TransferIn" => Color.FromArgb(59, 130, 246, 40),
                "TransferOut" => Color.FromArgb(249, 115, 22, 40),
                _ => Color.FromArgb(100, 100, 100, 40)
            };
        }

        private string GetIcon(string type)
        {
            return type switch
            {
                "Deposit" => "💰",
                "Withdraw" => "💸",
                "TransferIn" => "📥",
                "TransferOut" => "📤",
                _ => "🔄"
            };
        }
    }
}
