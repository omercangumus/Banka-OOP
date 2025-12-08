using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BankApp.Core.Entities;
using BankApp.UI.Services.Admin;
using BankApp.UI.Services;
using BankApp.Infrastructure.Repositories;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class AdminDashboardForm : Form
    {
        private readonly AdminService _adminService;

        // Root Layout
        private TableLayoutPanel rootLayout;

        // KPI Cards
        private Panel cardTotalUsers, cardTotalDeposits, cardActiveLoans, cardBannedUsers;

        // Main Content
        private SplitContainer mainSplit;

        // Left Panel - User Management
        private TableLayoutPanel userLayout;
        private Panel userHeader;
        private DevExpress.XtraEditors.TextEdit txtSearch;
        private DevExpress.XtraEditors.ComboBoxEdit cmbFilter;
        private DataGridView gridUsers;
        private Panel userActions;
        private DevExpress.XtraEditors.SimpleButton btnRefresh, btnBanUnban, btnExportPdf, btnTestConnection, btnTestEmail;

        // Right Panel - Loan Approval
        private TableLayoutPanel loanLayout;
        private Panel loanHeader;
        private DataGridView gridLoans;
        private Panel loanReview;
        private DevExpress.XtraEditors.MemoEdit txtLoanNote;
        private DevExpress.XtraEditors.SimpleButton btnApproveLoan, btnRejectLoan;

        // Status Bar
        private Panel statusBar;
        private Label lblStatus, lblSelected;

        // Data cache
        private List<User> _cachedUsers;
        private List<Loan> _cachedLoans;

        // Colors - PHASE 3: Modern dark theme with NovaBank colors
        private static readonly Color BgDark = Color.FromArgb(15, 23, 42); // Dark blue
        private static readonly Color BgCard = Color.FromArgb(30, 41, 59); // Lighter dark blue
        private static readonly Color BgHeader = Color.FromArgb(51, 65, 85); // Header color
        private static readonly Color BgRow = Color.FromArgb(71, 85, 105); // Row color
        private static readonly Color AccentGold = Color.FromArgb(212, 175, 55); // NovaBank gold
        private static readonly Color AccentBlue = Color.FromArgb(59, 130, 246); // Blue accent
        private static readonly Color AccentGreen = Color.FromArgb(16, 185, 129); // Green for success
        private static readonly Color AccentRed = Color.FromArgb(239, 68, 68); // Red for errors
        private static readonly Color TextPrimary = Color.FromArgb(248, 250, 252); // Light text
        private static readonly Color TextSecondary = Color.FromArgb(148, 163, 184); // Secondary text

        public AdminDashboardForm()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[AdminDashboardForm] Constructor started");
                
                _adminService = new AdminService();
                System.Diagnostics.Debug.WriteLine("[AdminDashboardForm] AdminService created");
                
                InitializeComponent();
                System.Diagnostics.Debug.WriteLine("[AdminDashboardForm] InitializeComponent completed");
                
                // Load data asynchronously
                this.Load += async (s, e) => {
                    try
                    {
                        await LoadAllDataAsync();
                        System.Diagnostics.Debug.WriteLine("[AdminDashboardForm] Data loaded successfully");
                    }
                    catch (Exception loadEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboardForm] LoadAllDataAsync error: {loadEx.Message}");
                        lblStatus.Text = $"Veri yükleme hatası: {loadEx.Message}";
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardForm] Constructor error: {ex}");
                DevExpress.XtraEditors.XtraMessageBox.Show(
                    $"Admin Dashboard başlatılamadı:\n\n{ex.Message}\n\nDetay: {ex.InnerException?.Message ?? "Yok"}",
                    "Admin Başlatma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void InitializeComponent()
        {
            this.Text = "NovaBank - Admin Panel";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BgDark;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // Root Layout - PHASE 3: Improved layout with proper spacing
            rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 140)); // KPI Row (increased height)
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Main Content
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // Status Bar (increased height)

            // Row 0: KPI Dashboard
            CreateKpiDashboard();

            // Row 1: Main Content
            CreateMainContent();

            // Row 2: Status Bar
            CreateStatusBar();

            this.Controls.Add(rootLayout);
        }

        private void CreateKpiDashboard()
        {
            var kpiPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgDark,
                Padding = new Padding(20)
            };

            var kpiLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = BgDark
            };
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            kpiLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            // Total Users Card
            cardTotalUsers = CreateKpiCard("Toplam Kullanıcı", "0", "👥", AccentBlue);
            kpiLayout.Controls.Add(cardTotalUsers, 0, 0);

            // Total Deposits Card
            cardTotalDeposits = CreateKpiCard("Toplam Bakiye", "₺0", "💰", AccentGold);
            kpiLayout.Controls.Add(cardTotalDeposits, 1, 0);

            // Active Loans Card
            cardActiveLoans = CreateKpiCard("Aktif Krediler", "0", "🏦", AccentGreen);
            kpiLayout.Controls.Add(cardActiveLoans, 2, 0);

            // Banned Users Card
            cardBannedUsers = CreateKpiCard("Yasaklı Kullanıcı", "0", "🚫", AccentRed);
            kpiLayout.Controls.Add(cardBannedUsers, 3, 0);

            kpiPanel.Controls.Add(kpiLayout);
            rootLayout.Controls.Add(kpiPanel, 0, 0);
        }

        private Panel CreateKpiCard(string title, string value, string icon, Color accentColor)
        {
            var card = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgCard,
                Margin = new Padding(8),
                Padding = new Padding(15)
            };

            // Add border effect
            card.Paint += (s, e) => {
                var rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                using var pen = new Pen(accentColor, 2);
                e.Graphics.DrawRectangle(pen, rect);
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.Transparent
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // Icon
            var iconLabel = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 24F),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                ForeColor = accentColor
            };
            layout.Controls.Add(iconLabel, 0, 0);
            layout.SetRowSpan(iconLabel, 2);

            // Title
            var titleLabel = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                ForeColor = TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft
            };
            layout.Controls.Add(titleLabel, 1, 0);

            // Value
            var valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = TextPrimary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };
            layout.Controls.Add(valueLabel, 1, 1);

            card.Controls.Add(layout);
            card.Tag = new { ValueLabel = valueLabel, AccentColor = accentColor }; // Store reference for updates

            return card;
        }

        private void CreateMainContent()
        {
            mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = this.Width / 2,
                SplitterWidth = 8,
                BackColor = BgHeader
            };

            // Left Panel - User Management
            CreateUserManagementPanel();

            // Right Panel - Loan Approval
            CreateLoanApprovalPanel();

            rootLayout.Controls.Add(mainSplit, 0, 1);
        }

        private void CreateUserManagementPanel()
        {
            userLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                BackColor = BgDark
            };
            userLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Header
            userLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Filters
            userLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
            userLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50)); // Actions

            // Header
            userHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgHeader,
                Padding = new Padding(15)
            };
            var headerLabel = new Label
            {
                Text = "Kullanıcı Yönetimi",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AccentGold,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            userHeader.Controls.Add(headerLabel);
            userLayout.Controls.Add(userHeader, 0, 0);

            // Filters Row
            var filtersPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgCard,
                Padding = new Padding(10)
            };

            var filtersLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            filtersLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            // Search box - Using DevExpress TextEdit
            txtSearch = new DevExpress.XtraEditors.TextEdit
            {
                Properties = { NullText = "Kullanıcı ara...", Appearance = { BackColor = BgRow, ForeColor = TextPrimary } },
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill
            };
            txtSearch.EditValueChanged += (s, e) => FilterUsers();
            filtersLayout.Controls.Add(txtSearch, 0, 0);

            // Filter dropdown - Using DevExpress ComboBoxEdit
            cmbFilter = new DevExpress.XtraEditors.ComboBoxEdit
            {
                Properties = {
                    Items = { "Tümü", "Aktif", "Yasaklı", "Admin", "Customer" },
                    TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor,
                    Appearance = { BackColor = BgRow, ForeColor = TextPrimary }
                },
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill
            };
            cmbFilter.EditValueChanged += (s, e) => FilterUsers();
            filtersLayout.Controls.Add(cmbFilter, 1, 0);

            // Test buttons
            var testLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            testLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            testLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            btnTestConnection = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "DB Test",
                Appearance = { BackColor = AccentBlue, ForeColor = Color.White },
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2)
            };
            btnTestConnection.Click += async (s, e) => await TestConnectionAsync();
            testLayout.Controls.Add(btnTestConnection, 0, 0);

            btnTestEmail = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "Email Test",
                Appearance = { BackColor = AccentGreen, ForeColor = Color.White },
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2)
            };
            btnTestEmail.Click += async (s, e) => await TestEmailAsync();
            testLayout.Controls.Add(btnTestEmail, 1, 0);

            filtersLayout.Controls.Add(testLayout, 2, 0);
            filtersPanel.Controls.Add(filtersLayout);
            userLayout.Controls.Add(filtersPanel, 0, 1);

            // Grid
            gridUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BgDark,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Configure grid appearance
            gridUsers.DefaultCellStyle.BackColor = BgRow;
            gridUsers.DefaultCellStyle.ForeColor = TextPrimary;
            gridUsers.DefaultCellStyle.SelectionBackColor = AccentGold;
            gridUsers.DefaultCellStyle.SelectionForeColor = Color.Black;
            gridUsers.ColumnHeadersDefaultCellStyle.BackColor = BgHeader;
            gridUsers.ColumnHeadersDefaultCellStyle.ForeColor = AccentGold;
            gridUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridUsers.EnableHeadersVisualStyles = false;

            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Kullanıcı Adı", Width = 120 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "E-posta", Width = 180 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Rol", Width = 80 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "FullName", HeaderText = "Ad Soyad", Width = 150 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsActive", HeaderText = "Aktif", Width = 70 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "IsBanned", HeaderText = "Yasaklı", Width = 70 });
            gridUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Oluşturulma", Width = 120 });

            gridUsers.SelectionChanged += GridUsers_SelectionChanged;
            userLayout.Controls.Add(gridUsers, 0, 2);

            // Actions
            userActions = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgCard,
                Padding = new Padding(10)
            };

            var actionsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            actionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            btnRefresh = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "Yenile",
                Appearance = { BackColor = AccentBlue, ForeColor = Color.White },
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2)
            };
            btnRefresh.Click += async (s, e) => await LoadAllDataAsync();
            actionsLayout.Controls.Add(btnRefresh, 0, 0);

            btnBanUnban = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "Yasakla/Kaldır",
                Appearance = { BackColor = AccentRed, ForeColor = Color.White },
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Enabled = false
            };
            btnBanUnban.Click += async (s, e) => await BanUnbanUserAsync();
            actionsLayout.Controls.Add(btnBanUnban, 1, 0);

            btnExportPdf = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "PDF Dışa Aktar",
                Appearance = { BackColor = AccentGold, ForeColor = Color.Black },
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2)
            };
            btnExportPdf.Click += (s, e) => ExportToPdf();
            actionsLayout.Controls.Add(btnExportPdf, 2, 0);

            userActions.Controls.Add(actionsLayout);
            userLayout.Controls.Add(userActions, 0, 3);

            mainSplit.Panel1.Controls.Add(userLayout);
        }

        private void CreateLoanApprovalPanel()
        {
            loanLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = BgDark
            };
            loanLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60)); // Header
            loanLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Grid
            loanLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 120)); // Review Panel

            // Header
            loanHeader = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgHeader,
                Padding = new Padding(15)
            };
            var headerLabel = new Label
            {
                Text = "Kredi Onayları",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AccentGold,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            loanHeader.Controls.Add(headerLabel);
            loanLayout.Controls.Add(loanHeader, 0, 0);

            // Grid
            gridLoans = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = BgDark,
                ForeColor = TextPrimary,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Configure grid appearance
            gridLoans.DefaultCellStyle.BackColor = BgRow;
            gridLoans.DefaultCellStyle.ForeColor = TextPrimary;
            gridLoans.DefaultCellStyle.SelectionBackColor = AccentGold;
            gridLoans.DefaultCellStyle.SelectionForeColor = Color.Black;
            gridLoans.ColumnHeadersDefaultCellStyle.BackColor = BgHeader;
            gridLoans.ColumnHeadersDefaultCellStyle.ForeColor = AccentGold;
            gridLoans.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            gridLoans.EnableHeadersVisualStyles = false;

            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 60 });
            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Kullanıcı", Width = 100 });
            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "Amount", HeaderText = "Tutar", Width = 100 });
            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "TermMonths", HeaderText = "Vade (Ay)", Width = 80 });
            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "InterestRate", HeaderText = "Faiz (%)", Width = 80 });
            gridLoans.Columns.Add(new DataGridViewTextBoxColumn { Name = "ApplicationDate", HeaderText = "Başvuru", Width = 120 });

            gridLoans.SelectionChanged += GridLoans_SelectionChanged;
            loanLayout.Controls.Add(gridLoans, 0, 1);

            // Review Panel
            loanReview = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgCard,
                Padding = new Padding(10)
            };

            var reviewLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1,
                BackColor = Color.Transparent
            };
            reviewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70));
            reviewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            // Note area - Using DevExpress MemoEdit
            txtLoanNote = new DevExpress.XtraEditors.MemoEdit
            {
                Properties = {
                    NullText = "Onay/red gerekçesi (isteğe bağlı)...",
                    Appearance = { BackColor = BgRow, ForeColor = TextPrimary }
                },
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill
            };
            reviewLayout.Controls.Add(txtLoanNote, 0, 0);

            // Buttons
            var buttonsLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            buttonsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            btnApproveLoan = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "Onayla",
                Appearance = { BackColor = AccentGreen, ForeColor = Color.White },
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Enabled = false
            };
            btnApproveLoan.Click += async (s, e) => await ApproveLoanAsync();
            buttonsLayout.Controls.Add(btnApproveLoan, 0, 0);

            btnRejectLoan = new DevExpress.XtraEditors.SimpleButton
            {
                Text = "Reddet",
                Appearance = { BackColor = AccentRed, ForeColor = Color.White },
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Margin = new Padding(2),
                Enabled = false
            };
            btnRejectLoan.Click += async (s, e) => await RejectLoanAsync();
            buttonsLayout.Controls.Add(btnRejectLoan, 1, 0);

            reviewLayout.Controls.Add(buttonsLayout, 0, 1);
            loanReview.Controls.Add(reviewLayout);
            loanLayout.Controls.Add(loanReview, 0, 2);

            mainSplit.Panel2.Controls.Add(loanLayout);
        }

        private void CreateStatusBar()
        {
            statusBar = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = BgHeader,
                Padding = new Padding(10, 5, 10, 5)
            };

            var statusLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            lblStatus = new Label
            {
                Text = "Hazır",
                ForeColor = TextSecondary,
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            statusLayout.Controls.Add(lblStatus, 0, 0);

            lblSelected = new Label
            {
                Text = "Seçili: Yok",
                ForeColor = AccentGold,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight
            };
            statusLayout.Controls.Add(lblSelected, 1, 0);

            statusBar.Controls.Add(statusLayout);
            rootLayout.Controls.Add(statusBar, 0, 2);
        }

        private async Task LoadAllDataAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Veriler yükleniyor...";

                // Load KPI data
                var stats = await _adminService.GetStatsAsync();
                UpdateKpiCards(stats);

                // Load users
                _cachedUsers = await _adminService.GetUsersAsync();
                PopulateUsersGrid(_cachedUsers);

                // Load loans
                _cachedLoans = await _adminService.GetPendingLoansAsync();
                PopulateLoansGrid(_cachedLoans);

                lblStatus.Text = $"Hazır - {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Hata oluştu!";
                XtraMessageBox.Show($"Veri yükleme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void UpdateKpiCards(AdminStats stats)
        {
            // Update card values using stored references
            if (cardTotalUsers.Tag is { } totalUsersTag)
            {
                var valueLabel = (Label)totalUsersTag.GetType().GetProperty("ValueLabel")?.GetValue(totalUsersTag);
                valueLabel.Text = stats.TotalUsers.ToString();
            }

            if (cardTotalDeposits.Tag is { } depositsTag)
            {
                var valueLabel = (Label)depositsTag.GetType().GetProperty("ValueLabel")?.GetValue(depositsTag);
                valueLabel.Text = $"₺{stats.TotalDeposits:N0}";
            }

            if (cardActiveLoans.Tag is { } loansTag)
            {
                var valueLabel = (Label)loansTag.GetType().GetProperty("ValueLabel")?.GetValue(loansTag);
                valueLabel.Text = stats.ActiveLoans.ToString();
            }

            if (cardBannedUsers.Tag is { } bannedTag)
            {
                var valueLabel = (Label)bannedTag.GetType().GetProperty("ValueLabel")?.GetValue(bannedTag);
                valueLabel.Text = stats.BannedUsers.ToString();
            }
        }

        private void PopulateUsersGrid(List<User> users)
        {
            gridUsers.Rows.Clear();
            foreach (var user in users)
            {
                gridUsers.Rows.Add(
                    user.Id,
                    user.Username,
                    user.Email,
                    user.Role,
                    user.FullName ?? "",
                    user.IsActive ? "Evet" : "Hayır",
                    "Hayır", // TODO: IsBanned column not yet available in entity
                    user.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                );
            }
        }

        private void PopulateLoansGrid(List<Loan> loans)
        {
            gridLoans.Rows.Clear();
            foreach (var loan in loans)
            {
                gridLoans.Rows.Add(
                    loan.Id,
                    "Kullanıcı", // Placeholder - should be from User entity
                    $"₺{loan.Amount:N0}",
                    loan.TermMonths,
                    $"%{loan.InterestRate:F2}",
                    loan.ApplicationDate.ToString("dd.MM.yyyy HH:mm")
                );
            }
        }

        private void GridUsers_SelectionChanged(object sender, EventArgs e)
        {
            var selectedRows = gridUsers.SelectedRows;
            if (selectedRows.Count > 0)
            {
                var row = selectedRows[0];
                var username = row.Cells["Username"].Value?.ToString() ?? "";
                var isBanned = row.Cells["IsBanned"].Value?.ToString() == "Evet";
                lblSelected.Text = $"Seçili: {username}";
                btnBanUnban.Text = isBanned ? "Yasak Kaldır" : "Yasakla";
                btnBanUnban.Enabled = true;
            }
            else
            {
                lblSelected.Text = "Seçili: Yok";
                btnBanUnban.Enabled = false;
            }
        }

        private void GridLoans_SelectionChanged(object sender, EventArgs e)
        {
            var selectedRows = gridLoans.SelectedRows;
            if (selectedRows.Count > 0)
            {
                lblSelected.Text = $"Seçili Kredi: {selectedRows[0].Cells["Id"].Value}";
                btnApproveLoan.Enabled = true;
                btnRejectLoan.Enabled = true;
            }
            else
            {
                btnApproveLoan.Enabled = false;
                btnRejectLoan.Enabled = false;
            }
        }

        private async Task BanUnbanUserAsync()
        {
            if (gridUsers.SelectedRows.Count == 0) return;

            var selectedRow = gridUsers.SelectedRows[0];
            var userId = (int)selectedRow.Cells["Id"].Value;
            var username = selectedRow.Cells["Username"].Value.ToString();
            var currentlyBanned = selectedRow.Cells["IsBanned"].Value.ToString() == "Evet";

            var action = currentlyBanned ? "yasak kaldırma" : "yasaklama";
            var result = XtraMessageBox.Show(
                $"{username} kullanıcısını {action} istiyor musunuz?",
                "Onay",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "İşlem yapılıyor...";

                if (currentlyBanned)
                    await _adminService.UnbanUserAsync(userId);
                else
                    await _adminService.BanUserAsync(userId);

                // Refresh data
                await LoadAllDataAsync();
                XtraMessageBox.Show("İşlem başarılı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"İşlem hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task ApproveLoanAsync()
        {
            if (gridLoans.SelectedRows.Count == 0) return;

            var selectedRow = gridLoans.SelectedRows[0];
            var loanId = (int)selectedRow.Cells["Id"].Value;
            var note = txtLoanNote.Text?.Trim() ?? "";

            var result = XtraMessageBox.Show(
                $"Bu krediyi onaylamak istiyor musunuz?\n\nNot: {note}",
                "Kredi Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Kredi onaylanıyor...";
                await _adminService.ApproveLoanAsync(loanId, note);

                // Refresh data
                await LoadAllDataAsync();
                txtLoanNote.Text = "";
                XtraMessageBox.Show("Kredi onaylandı!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Onay hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task RejectLoanAsync()
        {
            if (gridLoans.SelectedRows.Count == 0) return;

            var selectedRow = gridLoans.SelectedRows[0];
            var loanId = (int)selectedRow.Cells["Id"].Value;
            var reason = txtLoanNote.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(reason))
            {
                XtraMessageBox.Show("Red gerekçesi zorunludur!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = XtraMessageBox.Show(
                $"Bu krediyi reddetmek istiyor musunuz?\n\nGerekçe: {reason}",
                "Kredi Reddi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                lblStatus.Text = "Kredi reddediliyor...";
                await _adminService.RejectLoanAsync(loanId, reason);

                // Refresh data
                await LoadAllDataAsync();
                txtLoanNote.Text = "";
                XtraMessageBox.Show("Kredi reddedildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Red hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private async Task TestConnectionAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnTestConnection.Enabled = false;
                lblStatus.Text = "Veritabanı bağlantısı test ediliyor...";

                var (success, message) = await BankApp.UI.Services.ConfigurationService.TestConnectionAsync();

                lblStatus.Text = success ? "Veritabanı bağlantısı başarılı" : "Veritabanı bağlantısı başarısız";
                XtraMessageBox.Show(message, success ? "Başarılı" : "Hata",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Test hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnTestConnection.Enabled = true;
            }
        }

        private async Task TestEmailAsync()
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                btnTestEmail.Enabled = false;
                lblStatus.Text = "E-posta gönderimi test ediliyor...";

                var (success, message) = await BankApp.UI.Services.ConfigurationService.TestEmailAsync();

                lblStatus.Text = success ? "E-posta testi başarılı" : "E-posta testi başarısız";
                XtraMessageBox.Show(message, success ? "Başarılı" : "Hata",
                    MessageBoxButtons.OK, success ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Test hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                btnTestEmail.Enabled = true;
            }
        }

        private void FilterUsers()
        {
            if (_cachedUsers == null) return;

            var searchText = txtSearch.Text?.Trim().ToLower() ?? "";
            var filter = cmbFilter.Text ?? "Tümü";

            var filtered = _cachedUsers.Where(u =>
            {
                // Search filter
                if (!string.IsNullOrEmpty(searchText) &&
                    !(u.Username?.ToLower().Contains(searchText) ?? false) &&
                    !(u.Email?.ToLower().Contains(searchText) ?? false) &&
                    !(u.FullName?.ToLower().Contains(searchText) ?? false))
                    return false;

                // Status filter
                switch (filter)
                {
                    case "Aktif": return u.IsActive; // TODO: Add IsBanned check when column is available
                    case "Yasaklı": return !u.IsActive; // TODO: Use IsBanned when available
                    case "Admin": return u.Role == "Admin";
                    case "Customer": return u.Role == "Customer";
                    default: return true;
                }
            }).ToList();

            PopulateUsersGrid(filtered);
        }

        // ExportToCsv method removed by user request
        
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }
            return field;
        }
        
        private System.Data.DataTable CreateUsersDataTable()
        {
            var dt = new System.Data.DataTable();
            dt.Columns.Add("ID", typeof(string));
            dt.Columns.Add("Kullanıcı Adı", typeof(string));
            dt.Columns.Add("E-posta", typeof(string));
            dt.Columns.Add("Rol", typeof(string));
            dt.Columns.Add("Ad Soyad", typeof(string));
            dt.Columns.Add("Aktif", typeof(string));
            dt.Columns.Add("Oluşturulma", typeof(string));
            
            if (_cachedUsers != null)
            {
                foreach (var user in _cachedUsers)
                {
                    dt.Rows.Add(
                        user.Id.ToString(),
                        user.Username ?? "",
                        user.Email ?? "",
                        user.Role ?? "",
                        user.FullName ?? "",
                        user.IsActive ? "Evet" : "Hayır",
                        user.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                    );
                }
            }
            return dt;
        }

        private void ExportToPdf()
        {
            // DISABLED - Admin PDF export not implemented yet
            XtraMessageBox.Show("Admin PDF export henüz hazır değil.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}