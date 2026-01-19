#nullable enable
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.AI;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;
using BankApp.UI.Reports;
using BankApp.UI.Services.Pdf;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Modern AI Assistant - Beautiful Chat UI with Advanced Features
    /// </summary>
    public class AIAssistantForm : XtraForm
    {
        private readonly IAIProvider _aiProvider;
        private readonly AiContextBuilder _contextBuilder;
        private readonly DashboardSummaryService _dashboardService;
        private readonly string _stockContext;
        private bool _isSending;
        private bool _isTyping;
        
        // Modern UI Components - Fintech Chat Layout
        private Panel pnlMain;
        private Panel pnlChatContainer;
        private Panel pnlSidebarContainer;
        
        // Chat Components
        private DevExpress.XtraEditors.PanelControl pnlChatHeader;
        private DevExpress.XtraEditors.PanelControl pnlChatArea;
        private DevExpress.XtraEditors.XtraScrollableControl scrollChat;
        private DevExpress.XtraEditors.PanelControl flowChatMessages;
        private DevExpress.XtraEditors.PanelControl pnlComposer;
        private DevExpress.XtraEditors.MemoEdit txtInput;
        private DevExpress.XtraEditors.SimpleButton btnSend;
        
        // Sidebar Components
        private DevExpress.XtraEditors.PanelControl pnlSidebar;
        private DevExpress.XtraEditors.LabelControl lblSidebarTitle;
        private DevExpress.XtraEditors.PanelControl pnlQuickCommands;
        private DevExpress.XtraEditors.PanelControl pnlRecommendedCommands;
        private DevExpress.XtraEditors.PanelControl pnlShortcuts;
        
        // Header Actions
        private DevExpress.XtraEditors.SimpleButton btnNewChat;
        private DevExpress.XtraEditors.SimpleButton btnHistory;
        private DevExpress.XtraEditors.SimpleButton btnSettings;
        
        // Status Chips
        private DevExpress.XtraEditors.LabelControl chipProvider;
        private DevExpress.XtraEditors.LabelControl chipStatus;
        private DevExpress.XtraEditors.LabelControl chipMarket;
        private DevExpress.XtraEditors.LabelControl chipData;
        
        // Existing Components (Preserve)
        private SimpleButton btnClear;
        private SimpleButton btnPDF;
        private SimpleButton btnVoice;
        private SimpleButton btnClose;
        
        // Quick Action Buttons (Preserve)
        private SimpleButton btnPortfolio;
        private SimpleButton btnAnalysis;
        private SimpleButton btnRisk;
        private SimpleButton btnMarket;
        
        // Typing indicator
        private Panel pnlTyping;
        private LabelControl lblTyping;
        
        public AIAssistantForm(string? stockContext = null)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] OPENED: {GetType().FullName}, StockContext={stockContext}");
            
            _stockContext = stockContext ?? "";
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _dashboardService = new DashboardSummaryService(new DapperContext());
            
            InitUI();
            SetupEventHandlers();
        }
        
        private void InitUI()
        {
            // Form Properties
            this.Text = "NovaBank AI Assistant";
            this.Size = new Size(1200, 800);
            this.MinimumSize = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 25, 28); // Graphite background
            this.ForeColor = Color.White;
            this.LookAndFeel.SkinName = "Office 2019 Black";
            this.FormBorderStyle = FormBorderStyle.None;
            
            // Main Panel
            pnlMain = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 25, 28),
                Padding = new Padding(16)
            };
            
            // Create horizontal split (70% chat, 30% sidebar)
            var tableLayout = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.FromArgb(24, 25, 28)
            };
            
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            
            // Chat Container (70%)
            pnlChatContainer = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 25, 28),
                Padding = new Padding(0, 0, 8, 0)
            };
            CreateChatArea();
            
            // Sidebar Container (30%)
            pnlSidebarContainer = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 25, 28),
                Padding = new Padding(8, 0, 0, 0)
            };
            CreateSidebar();
            
            tableLayout.Controls.Add(pnlChatContainer, 0, 0);
            tableLayout.Controls.Add(pnlSidebarContainer, 1, 0);
            
            pnlMain.Controls.Add(tableLayout);
            this.Controls.Add(pnlMain);
        }
        
        private void CreateChatArea()
        {
            // Simple Panel Layout
            pnlChatContainer.Dock = DockStyle.Fill;
            pnlChatContainer.BackColor = Color.FromArgb(24, 25, 28);
            
            // Chat Header
            pnlChatHeader = new DevExpress.XtraEditors.PanelControl()
            {
                BackColor = Color.FromArgb(32, 33, 36),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Height = 60,
                Dock = DockStyle.Top
            };
            
            var lblTitle = new DevExpress.XtraEditors.LabelControl()
            {
                Text = "NovaBank AI Assistant",
                Appearance = {
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    ForeColor = Color.White
                },
                Location = new Point(20, 15)
            };
            
            chipProvider = CreateStatusChip(_aiProvider.ProviderName, Color.FromArgb(100, 200, 255));
            chipProvider.Location = new Point(250, 20);
            
            chipStatus = CreateStatusChip("Bağlı", Color.FromArgb(100, 255, 100));
            chipStatus.Location = new Point(380, 20);
            
            btnClose = CreateIconButton("✕", "Kapat", pnlChatHeader.Width - 50, 15);
            
            pnlChatHeader.Controls.AddRange(new Control[] { lblTitle, chipProvider, chipStatus, btnClose });
            
            // Chat Messages
            pnlChatArea = new DevExpress.XtraEditors.PanelControl()
            {
                BackColor = Color.FromArgb(24, 25, 28),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Dock = DockStyle.Fill
            };
            
            scrollChat = new DevExpress.XtraEditors.XtraScrollableControl()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 25, 28)
            };
            
            flowChatMessages = new DevExpress.XtraEditors.PanelControl()
            {
                BackColor = Color.FromArgb(24, 25, 28),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                AutoScroll = true,
                Dock = DockStyle.Fill
            };
            
            scrollChat.Controls.Add(flowChatMessages);
            pnlChatArea.Controls.Add(scrollChat);
            
            // Input Area
            pnlComposer = new DevExpress.XtraEditors.PanelControl()
            {
                BackColor = Color.FromArgb(32, 33, 36),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                Height = 80,
                Dock = DockStyle.Bottom
            };
            
            txtInput = new DevExpress.XtraEditors.MemoEdit()
            {
                Location = new Point(20, 15),
                Size = new Size(400, 50),
                BackColor = Color.FromArgb(45, 46, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder
            };
            
            btnSend = new DevExpress.XtraEditors.SimpleButton()
            {
                Text = "Gönder",
                Size = new Size(100, 50),
                Location = new Point(430, 15),
                Appearance = {
                    BackColor = Color.FromArgb(0, 123, 255),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat
            };
            
            btnClear = CreateIconButton("🗑️", "Temizle", 540, 15);
            btnPDF = CreateIconButton("📄", "PDF", 580, 15);
            
            pnlComposer.Controls.AddRange(new Control[] { txtInput, btnSend, btnClear, btnPDF });
            
            // Add controls
            pnlChatContainer.Controls.Add(pnlComposer);
            pnlChatContainer.Controls.Add(pnlChatArea);
            pnlChatContainer.Controls.Add(pnlChatHeader);
            
            // Welcome message
            AddAssistantMessage($"🌟 **NovaBank AI Asistanına Hoş Geldiniz!**\n\n" +
                $"🔗 **Aktif Servis:** {_aiProvider.ProviderName}\n\n" +
                "💬 Size nasıl yardımcı olabilirim?\n\n" +
                "📊 **Hızlı Komutlar:**\n" +
                "• Portföy özeti için: \"Portföyüm\"\n" +
                "• Teknik analiz için: \"GARAN analiz\"\n" +
                "• Destek seviyeleri için: \"Destek göster\"\n" +
                "• Risk analizi için: \"Risklerim\"\n\n" +
                "🎯 **Özellikler:**\n" +
                "• 📈 Detaylı teknik analiz\n" +
                "• 📊 Grafik formasyonları\n" +
                "• ⚠️ Risk değerlendirmesi\n" +
                "• 📄 PDF raporları");
        }
        
        private void CreateSidebar()
        {
            pnlSidebarContainer.Dock = DockStyle.Fill;
            pnlSidebarContainer.BackColor = Color.FromArgb(32, 33, 36);
            pnlSidebarContainer.Padding = new Padding(20);
            
            // Title
            var lblTitle = new DevExpress.XtraEditors.LabelControl()
            {
                Text = "⚡ Hızlı Komutlar",
                Appearance = {
                    Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                    ForeColor = Color.White
                },
                Location = new Point(20, 20)
            };
            
            // Quick Action Buttons
            btnPortfolio = CreateSidebarButton("📊 Portföy", "Portföy özetini göster", Color.FromArgb(52, 152, 219), 20, 60);
            btnAnalysis = CreateSidebarButton("📈 Analiz", "Teknik analiz yap", Color.FromArgb(46, 204, 113), 20, 110);
            btnRisk = CreateSidebarButton("⚠️ Risk", "Risk analizi", Color.FromArgb(241, 196, 15), 20, 160);
            btnMarket = CreateSidebarButton("💰 Piyasa", "Piyasa durumu", Color.FromArgb(231, 76, 60), 20, 210);
            
            // Add controls
            pnlSidebarContainer.Controls.AddRange(new Control[] { lblTitle, btnPortfolio, btnAnalysis, btnRisk, btnMarket });
        }
        
        private DevExpress.XtraEditors.LabelControl CreateStatusChip(string text, Color color)
        {
            var chip = new DevExpress.XtraEditors.LabelControl()
            {
                Text = text,
                Appearance = {
                    BackColor = color,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold)
                },
                Size = new Size(80, 25),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            return chip;
        }
        
        private DevExpress.XtraEditors.SimpleButton CreateIconButton(string text, string tooltip, int x, int y)
        {
            var btn = new DevExpress.XtraEditors.SimpleButton()
            {
                Text = text,
                Size = new Size(35, 35),
                Location = new Point(x, y),
                Appearance = {
                    BackColor = Color.FromArgb(45, 46, 50),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 14F)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat,
                ToolTip = tooltip
            };
            
            btn.MouseEnter += (s, e) => btn.Appearance.BackColor = Color.FromArgb(60, 61, 65);
            btn.MouseLeave += (s, e) => btn.Appearance.BackColor = Color.FromArgb(45, 46, 50);
            
            return btn;
        }
        
                
        private DevExpress.XtraEditors.SimpleButton CreateSidebarButton(string text, string tooltip, Color color, int x, int y)
        {
            var btn = new DevExpress.XtraEditors.SimpleButton()
            {
                Text = text,
                Size = new Size(220, 45),
                Location = new Point(x, y),
                Appearance = {
                    BackColor = color,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat,
                ToolTip = tooltip
            };
            
            btn.MouseEnter += (s, e) => btn.Appearance.BackColor = ControlPaint.Light(color, 0.2f);
            btn.MouseLeave += (s, e) => btn.Appearance.BackColor = color;
            
            return btn;
        }
        
                
                
        private void SetupEventHandlers()
        {
            // Input handlers
            txtInput.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.Handled = true;
                    await SendAsync();
                }
            };
            
            btnSend.Click += async (s, e) => await SendAsync();
            
            // Quick action handlers
            btnPortfolio.Click += async (s, e) => {
                txtInput.Text = "Portföyümü özetle ve analiz et";
                await SendAsync();
            };
            
            btnAnalysis.Click += async (s, e) => {
                txtInput.Text = "Grafik teknik analiz yap";
                await SendAsync();
            };
            
            btnRisk.Click += async (s, e) => {
                txtInput.Text = "Risklerimi değerlendir";
                await SendAsync();
            };
            
            btnMarket.Click += async (s, e) => {
                txtInput.Text = "Piyasa durumu nedir?";
                await SendAsync();
            };
            
            // Action handlers
            btnClear.Click += (s, e) => {
                flowChatMessages.Controls.Clear();
                AddAssistantMessage("💬 Sohbet temizlendi. Yeni başlayalım!");
            };
            
            btnPDF.Click += async (s, e) => await ExportPdfAsync();
            
            // Close button handler
            btnClose.Click += (s, e) => {
                if (_isSending)
                {
                    AppendMessage("AI", "⏳ İşlem devam ediyor, lütfen bekleyin...");
                    return;
                }
                this.Close();
            };
            
            // Form close handler
            this.FormClosing += (s, e) => {
                if (_isSending)
                {
                    e.Cancel = true;
                    AppendMessage("AI", "⏳ İşlem devam ediyor, lütfen bekleyin...");
                }
            };
        }
        
        private async Task SendAsync()
        {
            if (_isSending) return;
            
            var msg = txtInput.Text?.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            
            txtInput.Text = "";
            AppendMessage("Siz", msg);
            
            _isSending = true;
            _isTyping = true;
            btnSend.Enabled = false;
            btnSend.Text = "⏳ Bekle...";
            
            try
            {
                if (IsPdfIntent(msg))
                {
                    await ExportPdfAsync();
                }
                else
                {
                    var ctx = await _contextBuilder.BuildContextAsync(
                        AppEvents.CurrentSession.UserId,
                        AppEvents.CurrentSession.Username);
                    
                    var req = new AiRequest { UserMessage = msg, Context = ctx };
                    var resp = await _aiProvider.AskAsync(req);
                    AppendMessage("AI", resp);
                }
            }
            catch (Exception ex)
            {
                AppendMessage("AI", $"❌ Hata: {ex.Message}\n\n💡 İnternet bağlantınızı kontrol edin veya offline modda kullanın.");
            }
            finally
            {
                _isSending = false;
                _isTyping = false;
                btnSend.Enabled = true;
                btnSend.Text = "Gönder";
            }
        }
        
        private void AppendMessage(string sender, string text)
        {
            if (sender == "Siz")
                AddUserMessage(text);
            else
                AddAssistantMessage(text);
        }
        
        private void AddUserMessage(string text)
        {
            if (flowChatMessages.InvokeRequired)
            {
                flowChatMessages.Invoke(new Action(() => AddUserMessage(text)));
                return;
            }
            
            var bubble = CreateMessageBubble(text, true);
            bubble.Dock = DockStyle.Top;
            flowChatMessages.Controls.Add(bubble);
            flowChatMessages.ScrollControlIntoView(bubble);
        }
        
        private void AddAssistantMessage(string text)
        {
            if (flowChatMessages.InvokeRequired)
            {
                flowChatMessages.Invoke(new Action(() => AddAssistantMessage(text)));
                return;
            }
            
            var bubble = CreateMessageBubble(text, false);
            bubble.Dock = DockStyle.Top;
            flowChatMessages.Controls.Add(bubble);
            flowChatMessages.ScrollControlIntoView(bubble);
        }
        
        private DevExpress.XtraEditors.PanelControl CreateMessageBubble(string text, bool isUser)
        {
            var bubble = new DevExpress.XtraEditors.PanelControl()
            {
                BackColor = isUser ? Color.FromArgb(0, 123, 255) : Color.FromArgb(45, 46, 50),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder,
                MaximumSize = new Size(600, 0),
                Margin = new Padding(0, 0, 0, 12)
            };
            
            // Simple label for content
            var lblContent = new DevExpress.XtraEditors.LabelControl()
            {
                Text = $"{(isUser ? "👤 Siz" : "🤖 AI")} [{DateTime.Now.ToString("HH:mm")}]\n\n{text}",
                Appearance = {
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = Color.White
                },
                Location = new Point(15, 15),
                MaximumSize = new Size(570, 0)
            };
            
            bubble.Controls.Add(lblContent);
            return bubble;
        }
        
        private bool IsPdfIntent(string msg)
        {
            var l = msg.ToLower();
            return l.Contains("pdf") || (l.Contains("indir") && (l.Contains("portföy") || l.Contains("rapor")));
        }
        
        private async Task ExportPdfAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] ExportPdfAsync called, this={GetType().FullName}");
            AppendMessage("AI", "📄 **PDF Raporu Hazırlanıyor...**");
            
            try
            {
                var data = new InvestmentAnalysisData
                {
                    Symbol = "PORTFOLIO",
                    Name = AppEvents.CurrentSession.Username ?? "User",
                    Timeframe = "Summary",
                    GeneratedAt = DateTime.Now
                };
                
                try
                {
                    var d = await _dashboardService.GetFullDashboardDataAsync(AppEvents.CurrentSession.UserId);
                    data.LastPrice = (double)d.TotalBalance;
                    data.AIAnalysis = $"Net Varlık: ₺{d.NetWorth:N0}";
                    data.AIRecommendation = "HOLD";
                }
                catch { }
                
                using var dlg = new SaveFileDialog();
                dlg.Filter = "PDF|*.pdf";
                dlg.FileName = $"NovaBank_AI_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PdfReportExporter.GenerateInvestmentReport(data, dlg.FileName);
                    AppendMessage("AI", $"✅ **PDF Başarıyla Kaydedildi!**\n\n📁 Dosya: `{Path.GetFileName(dlg.FileName)}`\n\n💡 Raporunuz hazır!");
                }
                else
                {
                    AppendMessage("AI", "❌ PDF kaydetme iptal edildi.");
                }
            }
            catch (Exception ex)
            {
                AppendMessage("AI", $"❌ **PDF Hatası:** {ex.Message}");
            }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            // Draw border with rounded corners
            var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            var path = CreateRoundedRect(rect, 10);
            
            using (var pen = new Pen(Color.FromArgb(60, 60, 70), 2))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawPath(pen, path);
            }
        }
        
        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            var diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
    }
}
