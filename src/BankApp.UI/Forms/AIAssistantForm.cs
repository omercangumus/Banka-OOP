#nullable enable
using DevExpress.XtraEditors;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.AI;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Modern AI Assistant - Fintech Chat UI
    /// </summary>
    public class AIAssistantForm : XtraForm
    {
        private readonly IAIProvider _aiProvider;
        private readonly AiContextBuilder _contextBuilder;
        private readonly DashboardSummaryService _dashboardService;
        private readonly string _stockContext;
        private bool _isSending;
        
        // UI Components
        private Panel pnlHeader;
        private Panel pnlChatArea;
        private Panel pnlComposer;
        private Panel pnlSidebar;
        
        private FlowLayoutPanel flowMessages;
        private MemoEdit txtInput;
        private SimpleButton btnSend;
        private SimpleButton btnClear;
        private SimpleButton btnClose;
        
        // Quick Action Buttons
        private SimpleButton btnPortfolio;
        private SimpleButton btnAnalysis;
        private SimpleButton btnRisk;
        private SimpleButton btnMarket;
        
        
        public AIAssistantForm(string? stockContext = null)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] OPENED: {GetType().FullName}, StockContext={stockContext}");
            
            _stockContext = stockContext ?? "";
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _dashboardService = new DashboardSummaryService(new DapperContext());
            
            InitUI();
            SetupEventHandlers();
            ShowWelcomeMessage();
        }
        
        private void InitUI()
        {
            // Form Properties
            this.Text = "NovaBank AI Assistant";
            this.Size = new Size(1100, 750);
            this.MinimumSize = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.ForeColor = Color.White;
            this.LookAndFeel.SkinName = "Office 2019 Black";
            this.FormBorderStyle = FormBorderStyle.Sizable;
            
            // ===== HEADER =====
            pnlHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(40, 40, 45),
                Padding = new Padding(15, 10, 15, 10)
            };
            
            var lblTitle = new Label()
            {
                Text = "🤖 NovaBank AI Assistant",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            
            var lblBadge = new Label()
            {
                Text = $"● {_aiProvider.ProviderName} • Connected",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(100, 255, 150),
                AutoSize = true,
                Location = new Point(280, 16)
            };
            
            btnClose = new SimpleButton()
            {
                Text = "✕",
                Size = new Size(30, 30),
                Location = new Point(pnlHeader.Width - 50, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White, Font = new Font("Segoe UI", 12F) },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat
            };
            
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblBadge, btnClose });
            
            // ===== SIDEBAR (RIGHT 280px) =====
            pnlSidebar = new Panel()
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.FromArgb(35, 35, 40),
                Padding = new Padding(15)
            };
            CreateSidebar();
            
            // ===== COMPOSER (BOTTOM 70px) =====
            pnlComposer = new Panel()
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(40, 40, 45),
                Padding = new Padding(15, 10, 15, 10)
            };
            CreateComposer();
            
            // ===== CHAT AREA (FILL) =====
            pnlChatArea = new Panel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 35),
                Padding = new Padding(20)
            };
            
            flowMessages = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.FromArgb(30, 30, 35)
            };
            
            pnlChatArea.Controls.Add(flowMessages);
            
            // Add controls in correct order
            this.Controls.Add(pnlChatArea);
            this.Controls.Add(pnlComposer);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlHeader);
        }
        
        private void CreateComposer()
        {
            txtInput = new MemoEdit()
            {
                Location = new Point(15, 10),
                Size = new Size(pnlComposer.Width - 150, 50),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(50, 50, 55),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F),
                BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple
            };
            txtInput.Properties.NullValuePrompt = "Mesajınızı yazın...";
            txtInput.Properties.NullValuePromptShowForEmptyValue = true;
            
            btnSend = new SimpleButton()
            {
                Text = "Gönder →",
                Size = new Size(100, 50),
                Location = new Point(pnlComposer.Width - 120, 10),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Appearance = { 
                    BackColor = Color.FromArgb(0, 150, 136), 
                    ForeColor = Color.White, 
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold) 
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat
            };
            
            pnlComposer.Controls.AddRange(new Control[] { txtInput, btnSend });
        }
        
        private void CreateSidebar()
        {
            // Card Container
            var cardPanel = new Panel()
            {
                Location = new Point(10, 10),
                Size = new Size(250, 320),
                BackColor = Color.FromArgb(45, 45, 50),
                Padding = new Padding(15)
            };
            
            var lblTitle = new Label()
            {
                Text = "⚡ Hızlı Komutlar",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                AutoSize = true
            };
            
            // Buttons with consistent styling
            btnPortfolio = CreateQuickButton("📊  Portföy Özeti", Color.FromArgb(52, 152, 219), 15, 55);
            btnAnalysis = CreateQuickButton("📈  Teknik Analiz", Color.FromArgb(46, 204, 113), 15, 115);
            btnRisk = CreateQuickButton("⚠️  Risk Analizi", Color.FromArgb(241, 196, 15), 15, 175);
            btnMarket = CreateQuickButton("💰  Piyasa Durumu", Color.FromArgb(231, 76, 60), 15, 235);
            
            cardPanel.Controls.AddRange(new Control[] { lblTitle, btnPortfolio, btnAnalysis, btnRisk, btnMarket });
            
            // Info label
            var lblInfo = new Label()
            {
                Text = "💡 Enter = Gönder\n    Shift+Enter = Yeni satır",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(150, 150, 150),
                Location = new Point(15, 350),
                AutoSize = true
            };
            
            // Action buttons
            btnClear = new SimpleButton()
            {
                Text = "🗑️ Temizle",
                Size = new Size(230, 35),
                Location = new Point(15, 420),
                Appearance = { BackColor = Color.FromArgb(60, 60, 65), ForeColor = Color.White, Font = new Font("Segoe UI", 9F) },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat
            };
            
            pnlSidebar.Controls.AddRange(new Control[] { cardPanel, lblInfo, btnClear });
        }
        
        private SimpleButton CreateQuickButton(string text, Color color, int x, int y)
        {
            var btn = new SimpleButton()
            {
                Text = text,
                Size = new Size(220, 50),
                Location = new Point(x, y),
                Appearance = { 
                    BackColor = color, 
                    ForeColor = Color.White, 
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold)
                },
                ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.Flat
            };
            
            btn.MouseEnter += (s, e) => btn.Appearance.BackColor = ControlPaint.Light(color, 0.15f);
            btn.MouseLeave += (s, e) => btn.Appearance.BackColor = color;
            
            return btn;
        }
        
        private void ShowWelcomeMessage()
        {
            AddAssistantMessage($"🌟 NovaBank AI Asistanına Hoş Geldiniz!\n\n" +
                $"🔗 Aktif Servis: {_aiProvider.ProviderName}\n\n" +
                "💬 Size nasıl yardımcı olabilirim?\n\n" +
                "Sağdaki hızlı komutları kullanabilir veya\ndoğrudan soru sorabilirsiniz.");
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
                flowMessages.Controls.Clear();
                AddAssistantMessage("💬 Sohbet temizlendi. Yeni başlayalım!");
            };
            
            
            // Close button handler
            btnClose.Click += (s, e) => {
                if (_isSending)
                {
                    AddAssistantMessage("⏳ İşlem devam ediyor, lütfen bekleyin...");
                    return;
                }
                this.Close();
            };
            
            // Form close handler
            this.FormClosing += (s, e) => {
                if (_isSending)
                {
                    e.Cancel = true;
                    AddAssistantMessage("⏳ İşlem devam ediyor, lütfen bekleyin...");
                }
            };
        }
        
        private async Task SendAsync()
        {
            if (_isSending) return;
            
            var msg = txtInput.Text?.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            
            txtInput.Text = "";
            AddUserMessage(msg);
            
            _isSending = true;
            btnSend.Enabled = false;
            btnSend.Text = "⏳...";
            
            try
            {
                var ctx = await _contextBuilder.BuildContextAsync(
                    AppEvents.CurrentSession.UserId,
                    AppEvents.CurrentSession.Username);
                
                var req = new AiRequest { UserMessage = msg, Context = ctx };
                var resp = await _aiProvider.AskAsync(req);
                AddAssistantMessage(resp);
            }
            catch (Exception ex)
            {
                AddAssistantMessage($"❌ Hata: {ex.Message}\n\n💡 İnternet bağlantınızı kontrol edin.");
            }
            finally
            {
                _isSending = false;
                btnSend.Enabled = true;
                btnSend.Text = "Gönder →";
            }
        }
        
        private void AddUserMessage(string text)
        {
            if (flowMessages.InvokeRequired)
            {
                flowMessages.Invoke(new Action(() => AddUserMessage(text)));
                return;
            }
            
            var bubble = CreateBubble(text, true);
            flowMessages.Controls.Add(bubble);
            flowMessages.ScrollControlIntoView(bubble);
        }
        
        private void AddAssistantMessage(string text)
        {
            if (flowMessages.InvokeRequired)
            {
                flowMessages.Invoke(new Action(() => AddAssistantMessage(text)));
                return;
            }
            
            var bubble = CreateBubble(text, false);
            flowMessages.Controls.Add(bubble);
            flowMessages.ScrollControlIntoView(bubble);
        }
        
        private Panel CreateBubble(string text, bool isUser)
        {
            // Outer container for alignment
            var container = new Panel()
            {
                Width = flowMessages.Width - 40,
                AutoSize = true,
                MinimumSize = new Size(flowMessages.Width - 40, 60),
                BackColor = Color.Transparent,
                Margin = new Padding(0, 5, 0, 5)
            };
            
            // Bubble panel
            var bubble = new Panel()
            {
                BackColor = isUser ? Color.FromArgb(0, 150, 136) : Color.FromArgb(50, 50, 55),
                MaximumSize = new Size((int)(flowMessages.Width * 0.7), 0),
                AutoSize = true,
                Padding = new Padding(12)
            };
            
            // Header label
            var lblHeader = new Label()
            {
                Text = isUser ? $"👤 Siz • {DateTime.Now:HH:mm}" : $"🤖 AI • {DateTime.Now:HH:mm}",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = isUser ? Color.FromArgb(200, 255, 200) : Color.FromArgb(150, 200, 255),
                AutoSize = true,
                Location = new Point(12, 8)
            };
            
            // Content label
            var lblContent = new Label()
            {
                Text = text,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.White,
                AutoSize = true,
                MaximumSize = new Size((int)(flowMessages.Width * 0.7) - 30, 0),
                Location = new Point(12, 30)
            };
            
            bubble.Controls.Add(lblHeader);
            bubble.Controls.Add(lblContent);
            
            // Position bubble (user right, AI left)
            bubble.Location = isUser ? new Point(container.Width - bubble.Width - 10, 0) : new Point(10, 0);
            
            container.Controls.Add(bubble);
            
            // Adjust container height
            container.Height = bubble.Height + 10;
            
            return container;
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
