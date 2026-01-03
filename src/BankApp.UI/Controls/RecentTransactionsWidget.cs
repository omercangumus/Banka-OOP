using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dapper;
using BankApp.Infrastructure.Data;
using BankApp.Core.Entities;

namespace BankApp.UI.Controls
{
    public partial class RecentTransactionsWidget : UserControl
    {
        public RecentTransactionsWidget()
        {
            InitializeComponent();
            LoadTransactions();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(380, 300);
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.Padding = new Padding(15);
        }

        private async void LoadTransactions()
        {
            try
            {
                var context = new DapperContext();
                using (var conn = context.CreateConnection())
                {
                    var transactions = await conn.QueryAsync<Transaction>(
                        "SELECT * FROM \"Transactions\" ORDER BY \"TransactionDate\" DESC LIMIT 5");

                    this.Controls.Clear();

                    // Title
                    Label lblTitle = new Label
                    {
                        Text = "Son İşlemler",
                        Font = new Font("Segoe UI", 14, FontStyle.Bold),
                        ForeColor = Color.White,
                        Location = new Point(15, 15),
                        AutoSize = true
                    };
                    this.Controls.Add(lblTitle);

                    int y = 50;
                    foreach (var tx in transactions)
                    {
                        Panel txPanel = CreateTransactionRow(tx, y);
                        this.Controls.Add(txPanel);
                        y += 50;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadTransactions Error: {ex.Message}");
            }
        }

        private Panel CreateTransactionRow(Transaction tx, int yPos)
        {
            Panel pnl = new Panel
            {
                Size = new Size(350, 40),
                Location = new Point(15, yPos),
                BackColor = Color.FromArgb(40, 40, 40)
            };

            // Icon/Logo
            Label lblIcon = new Label
            {
                Text = GetIcon(tx.TransactionType),
                Font = new Font("Segoe UI Emoji", 18),
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 0)
            };

            // Description
            Label lblDesc = new Label
            {
                Text = tx.Description ?? "İşlem",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(50, 5),
                AutoSize = true
            };

            // Amount
            Color amountColor = tx.TransactionType == "Deposit" || tx.TransactionType == "TransferIn"
                ? Color.FromArgb(0, 210, 255)
                : Color.FromArgb(255, 0, 122);
            
            Label lblAmount = new Label
            {
                Text = $"{(tx.TransactionType == "Deposit" || tx.TransactionType == "TransferIn" ? "+" : "-")}₺{tx.Amount:N2}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = amountColor,
                Location = new Point(240, 5),
                AutoSize = true
            };

            pnl.Controls.AddRange(new Control[] { lblIcon, lblDesc, lblAmount });
            return pnl;
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
