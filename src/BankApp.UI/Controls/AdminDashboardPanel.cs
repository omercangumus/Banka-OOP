using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Linq;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using Dapper;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Controls
{
    public partial class AdminDashboardPanel : UserControl
    {
        private GridControl gridUsers;
        private GridView gridViewUsers;
        private GridControl gridLoans;
        private GridView gridViewLoans;
        private TextBox txtLogs;
        private SimpleButton btnExportExcel;
        private SimpleButton btnDeleteUser;

        public AdminDashboardPanel()
        {
            InitializeComponent();
            LoadAdminData();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.BackColor = Color.FromArgb(18, 18, 18);

            // Title with gradient effect
            Panel titlePanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(1200, 60),
                BackColor = Color.FromArgb(25, 25, 35)
            };
            
            Label lblTitle = new Label
            {
                Text = "⚙️ Admin Yönetim Paneli",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 255),
                Location = new Point(20, 12),
                AutoSize = true
            };
            titlePanel.Controls.Add(lblTitle);
            this.Controls.Add(titlePanel);

            // **INFO CARDS (4 CARDS IN A ROW)**
            int cardY = 80;
            int cardWidth = 280;
            int cardHeight = 130;
            int cardSpacing = 15;

            // Card 1: Toplam Kullanıcı (Blue gradient)
            Panel pnlCard1 = CreateInfoCard("👥 Toplam Kullanıcı", "0", 
                Color.FromArgb(41, 128, 185), Color.FromArgb(52, 152, 219), 
                new Point(20, cardY), cardWidth, cardHeight);
            pnlCard1.Tag = "totalUsers";
            this.Controls.Add(pnlCard1);

            // Card 2: Toplam Mevduat (Green gradient)
            Panel pnlCard2 = CreateInfoCard("💰 Toplam Mevduat", "₺0", 
                Color.FromArgb(39, 174, 96), Color.FromArgb(46, 204, 113), 
                new Point(20 + cardWidth + cardSpacing, cardY), cardWidth, cardHeight);
            pnlCard2.Tag = "totalDeposits";
            this.Controls.Add(pnlCard2);

            // Card 3: Bekleyen Krediler (Orange gradient)
            Panel pnlCard3 = CreateInfoCard("📋 Bekleyen Krediler", "0", 
                Color.FromArgb(230, 126, 34), Color.FromArgb(243, 156, 18), 
                new Point(20 + (cardWidth + cardSpacing) * 2, cardY), cardWidth, cardHeight);
            pnlCard3.Tag = "pendingLoans";
            this.Controls.Add(pnlCard3);

            // Card 4: Aktif Sorunlar (Red gradient)
            Panel pnlCard4 = CreateInfoCard("⚠️ Aktif Sorunlar", "0", 
                Color.FromArgb(192, 57, 43), Color.FromArgb(231, 76, 60), 
                new Point(20 + (cardWidth + cardSpacing) * 3, cardY), cardWidth, cardHeight);
            pnlCard4.Tag = "activeIssues";
            this.Controls.Add(pnlCard4);

            int contentY = cardY + cardHeight + 25;

            // User Management Section
            Label lblUsers = new Label
            {
                Text = "Kullanıcı Yönetimi",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 255),
                Location = new Point(20, contentY),
                AutoSize = true
            };
            this.Controls.Add(lblUsers);

            // Toolbar for User Management
            Panel toolbar = new Panel
            {
                Location = new Point(20, contentY + 30),
                Size = new Size(1160, 40),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            
            btnExportExcel = new SimpleButton
            {
                Text = "📊 Excel İndir",
                Location = new Point(10, 5),
                Size = new Size(120, 30)
            };
            btnExportExcel.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            btnExportExcel.Appearance.ForeColor = Color.White;
            btnExportExcel.Click += BtnExportExcel_Click;
            toolbar.Controls.Add(btnExportExcel);
            
            btnDeleteUser = new SimpleButton
            {
                Text = "🗑️ Kullanıcı Sil",
                Location = new Point(140, 5),
                Size = new Size(120, 30)
            };
            btnDeleteUser.Appearance.BackColor = Color.FromArgb(244, 67, 54);
            btnDeleteUser.Appearance.ForeColor = Color.White;
            btnDeleteUser.Click += BtnDeleteUser_Click;
            toolbar.Controls.Add(btnDeleteUser);
            
            this.Controls.Add(toolbar);
            
            gridUsers = new GridControl
            {
                Location = new Point(20, contentY + 75),
                Size = new Size(1160, 155)
            };
            gridViewUsers = new GridView();
            gridUsers.MainView = gridViewUsers;
            gridViewUsers.GridControl = gridUsers;
            gridViewUsers.OptionsView.ShowGroupPanel = false;
            this.Controls.Add(gridUsers);
            
            // Apply Dark Mode Styling
            StyleAdminGrid(gridViewUsers);

            // Loan Requests Section
            Label lblLoans = new Label
            {
                Text = "Kredi Başvuruları",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 0, 122),
                Location = new Point(20, contentY + 250),
                AutoSize = true
            };
            this.Controls.Add(lblLoans);

            gridLoans = new GridControl
            {
                Location = new Point(20, contentY + 280),
                Size = new Size(1160, 180)
            };
            gridViewLoans = new GridView();
            gridLoans.MainView = gridViewLoans;
            gridViewLoans.GridControl = gridLoans;
            gridViewLoans.OptionsView.ShowGroupPanel = false;
            this.Controls.Add(gridLoans);

            // System Logs Section
            Label lblLogs = new Label
            {
                Text = "Sistem Logları",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 210, 255),
                Location = new Point(20, contentY + 480),
                AutoSize = true
            };
            this.Controls.Add(lblLogs);

            txtLogs = new TextBox
            {
                Location = new Point(20, contentY + 510),
                Size = new Size(1160, 100),
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtLogs);
        }

        // Helper method to create info cards with gradients
        private Panel CreateInfoCard(string title, string value, Color color1, Color color2, Point location, int width, int height)
        {
            Panel card = new Panel
            {
                Size = new Size(width, height),
                Location = location
            };
            
            // Gradient background
            card.Paint += (s, e) =>
            {
                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    card.ClientRectangle, color1, color2, 45f))
                {
                    e.Graphics.FillRectangle(brush, card.ClientRectangle);
                }
            };

            Label lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblTitle);

            Label lblValue = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 50),
                AutoSize = true,
                Name = "lblValue",
                BackColor = Color.Transparent
            };
            card.Controls.Add(lblValue);

            return card;
        }

        private async void LoadAdminData()
        {
            try
            {
                var context = new DapperContext();
                using (var conn = context.CreateConnection())
                {
                    // **FETCH STATISTICS FOR INFO CARDS**
                    
                    // 1. Total Users
                    var totalUsers = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Users\"");
                    UpdateCardValue("totalUsers", totalUsers.ToString());

                    // 2. Total Deposits (Sum of all account balances)
                    var totalDeposits = await conn.ExecuteScalarAsync<decimal?>("SELECT COALESCE(SUM(\"Balance\"), 0) FROM \"Accounts\"") ?? 0;
                    UpdateCardValue("totalDeposits", $"₺{totalDeposits:N2}");

                    // 3. Pending Loan Applications
                    var pendingLoans = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM \"Loans\" WHERE \"Status\" = 'Pending'");
                    UpdateCardValue("pendingLoans", pendingLoans.ToString());

                    // 4. Active Issues (for now, just show 0, you can link to a real issues table later)
                    UpdateCardValue("activeIssues", "0");

                    // **LOAD GRIDS**
                    
                    // Load Users
                    var users = await conn.QueryAsync("SELECT \"Id\", \"Username\", \"Email\", \"Role\", \"CreatedAt\" FROM \"Users\" ORDER BY \"CreatedAt\" DESC");
                    gridUsers.DataSource = users;

                    // Load Loan Requests
                    var loans = await conn.QueryAsync(@"
                        SELECT l.""Id"", l.""UserId"", u.""Username"", l.""Amount"", l.""TermMonths"", 
                               l.""InterestRate"", l.""Status"", l.""ApplicationDate""
                        FROM ""Loans"" l
                        LEFT JOIN ""Users"" u ON l.""UserId"" = u.""Id""
                        WHERE l.""Status"" = 'Pending' 
                        ORDER BY l.""ApplicationDate"" DESC");
                    gridLoans.DataSource = loans;

                    // Load Recent Logs
                    var logs = await conn.QueryAsync<string>(@"
                        SELECT ""Action"" || ' - ' || ""Details"" || ' (' || TO_CHAR(""CreatedAt"", 'DD-MM-YYYY HH24:MI') || ')' 
                        FROM ""AuditLogs"" 
                        ORDER BY ""CreatedAt"" DESC 
                        LIMIT 10");
                    txtLogs.Lines = System.Linq.Enumerable.ToArray(logs);
                }
            }
            catch (Exception ex)
            {
                txtLogs.Text = $"Error loading admin data: {ex.Message}";
            }
        }

        // Helper method to update card values
        private void UpdateCardValue(string cardTag, string newValue)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is Panel panel && panel.Tag?.ToString() == cardTag)
                {
                    foreach (Control innerCtrl in panel.Controls)
                    {
                        if (innerCtrl is Label lbl && lbl.Name == "lblValue")
                        {
                            lbl.Text = newValue;
                            return;
                        }
                    }
                }
            }
        }

        // PUBLIC METHOD TO REFRESH DATA (Called from MainForm)
        public void RefreshData()
        {
            LoadAdminData();
        }
        
        // **DARK MODE GRID STYLING**
        private void StyleAdminGrid(GridView gridView)
        {
            // Background colors
            gridView.Appearance.Row.BackColor = Color.FromArgb(45, 45, 45);
            gridView.Appearance.Row.ForeColor = Color.White;
            
            // Header styling
            gridView.Appearance.HeaderPanel.BackColor = Color.FromArgb(15, 15, 15);
            gridView.Appearance.HeaderPanel.ForeColor = Color.White;
            gridView.Appearance.HeaderPanel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            gridView.Appearance.HeaderPanel.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            
            // Selection styling
            gridView.Appearance.FocusedRow.BackColor = Color.FromArgb(138, 43, 226); // Purple
            gridView.Appearance.FocusedRow.ForeColor = Color.White;
            gridView.Appearance.SelectedRow.BackColor = Color.FromArgb(0, 122, 204); // Blue
            gridView.Appearance.SelectedRow.ForeColor = Color.White;
            
            // Remove grid lines
            gridView.OptionsView.ShowVerticalLines = DevExpress.Utils.DefaultBoolean.False;
            gridView.OptionsView.ShowHorizontalLines = DevExpress.Utils.DefaultBoolean.False;
            
            // Other options
            gridView.OptionsBehavior.Editable = false;
            gridView.OptionsSelection.EnableAppearanceFocusedCell = false;
        }
        
        // **EXPORT TO CSV (Excel)**
        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = $"Kullanicilar_{DateTime.Now:yyyyMMdd}.csv"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var sb = new StringBuilder();
                    
                    // Write header
                    var columns = gridViewUsers.Columns.Cast<DevExpress.XtraGrid.Columns.GridColumn>()
                        .Where(c => c.Visible)
                        .Select(c => c.Caption);
                    sb.AppendLine(string.Join("\t", columns));
                    
                    // Write data rows
                    for (int i = 0; i < gridViewUsers.DataRowCount; i++)
                    {
                        var rowData = gridViewUsers.Columns.Cast<DevExpress.XtraGrid.Columns.GridColumn>()
                            .Where(c => c.Visible)
                            .Select(c => gridViewUsers.GetRowCellValue(i, c)?.ToString() ?? "");
                        sb.AppendLine(string.Join("\t", rowData));
                    }
                    
                    // Write with UTF-8 BOM for Excel compatibility
                    var utf8WithBom = new UTF8Encoding(true);
                    File.WriteAllText(saveDialog.FileName, sb.ToString(), utf8WithBom);
                    
                    XtraMessageBox.Show("Excel dosyası başarıyla oluşturuldu!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Export hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        // **DELETE USER**
        private async void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            try
            {
                // Get selected row
                int selectedRow = gridViewUsers.FocusedRowHandle;
                if (selectedRow < 0)
                {
                    XtraMessageBox.Show("Lütfen silmek için bir kullanıcı seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                int userId = Convert.ToInt32(gridViewUsers.GetRowCellValue(selectedRow, "Id"));
                string username = gridViewUsers.GetRowCellValue(selectedRow, "Username")?.ToString();
                
                // Confirmation
                var result = XtraMessageBox.Show(
                    $"'{username}' kullanıcısını silmek istediğinize emin misiniz?\n\nBu işlem geri alınamaz!",
                    "Kullanıcı Silme Onayı",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    var context = new DapperContext();
                    using (var conn = context.CreateConnection())
                    {
                        await conn.ExecuteAsync("DELETE FROM \"Users\" WHERE \"Id\" = @Id", new { Id = userId });
                    }
                    
                    XtraMessageBox.Show("Kullanıcı başarıyla silindi.", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Refresh data
                    LoadAdminData();
                }
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Silme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
