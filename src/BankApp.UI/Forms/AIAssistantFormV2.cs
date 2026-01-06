#nullable enable
using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.AI;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// AI Financial Assistant Form V2 - Production-ready with offline support
    /// </summary>
    public partial class AIAssistantFormV2 : XtraForm
    {
        private readonly IAIProvider _aiProvider;
        private readonly AiContextBuilder _contextBuilder;
        private readonly FinnhubService _finnhubService;
        
        // State
        private bool _isSending = false;
        private string _currentTopic = "";
        private string _selectedStockSymbol = "";
        
        // UI Controls
        private PanelControl pnlHeader;
        private PanelControl pnlStatusBar;
        private PanelControl pnlTopics;
        private PanelControl pnlInput;
        private Panel pnlChatContainer;
        private FlowLayoutPanel flowChat;
        private MemoEdit txtUserInput;
        private SimpleButton btnSend;
        private LabelControl lblTyping;
        private LabelControl lblOfflineBanner;
        
        // Status indicators
        private LabelControl lblGroqStatus;
        private LabelControl lblDataStatus;
        private LabelControl lblMarketStatus;
        private Panel pnlGroqIndicator;
        private Panel pnlDataIndicator;
        private Panel pnlMarketIndicator;
        
        // Topic buttons
        private SimpleButton btnTopicSavings;
        private SimpleButton btnTopicInvestment;
        private SimpleButton btnTopicCredit;
        private SimpleButton btnTopicDeposit;
        private SimpleButton btnTopicStock;

        public AIAssistantFormV2()
        {
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _finnhubService = new FinnhubService();
            
            InitializeComponent();
            SetupUI();
            ApplyDarkTheme();
            UpdateStatusIndicators();
            AddWelcomeMessage();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "AIAssistantFormV2";
            this.Text = "🤖 NovaBank AI Finansal Asistan";
            this.Size = new Size(920, 780);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(800, 600);
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // ═══════════════════════════════════════════════════════════
            // HEADER
            // ═══════════════════════════════════════════════════════════
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 70;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Appearance.BackColor = Color.FromArgb(25, 28, 38);
            
            var lblIcon = new LabelControl { Text = "🤖", Font = new Font("Segoe UI Emoji", 32F), Location = new Point(20, 15) };
            var lblTitle = new LabelControl { Text = "AI Finansal Asistan", Font = new Font("Segoe UI", 20F, FontStyle.Bold), ForeColor = Color.FromArgb(212, 175, 55), Location = new Point(70, 12) };
            var lblSubtitle = new LabelControl { Text = _aiProvider.ProviderName + " • Kurumsal Finansal Danışman", Font = new Font("Segoe UI", 10F), ForeColor = Color.FromArgb(148, 163, 184), Location = new Point(70, 42) };
            
            pnlHeader.Controls.AddRange(new Control[] { lblIcon, lblTitle, lblSubtitle });
            this.Controls.Add(pnlHeader);

            // ═══════════════════════════════════════════════════════════
            // STATUS BAR
            // ═══════════════════════════════════════════════════════════
            pnlStatusBar = new PanelControl();
            pnlStatusBar.Dock = DockStyle.Top;
            pnlStatusBar.Height = 40;
            pnlStatusBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlStatusBar.Appearance.BackColor = Color.FromArgb(30, 33, 45);
            
            // Groq status
            pnlGroqIndicator = CreateStatusDot(15, 12);
            lblGroqStatus = new LabelControl { Text = "Groq: --", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(150, 150, 160), Location = new Point(32, 11) };
            
            // Data status
            pnlDataIndicator = CreateStatusDot(150, 12);
            lblDataStatus = new LabelControl { Text = "Veri: --", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(150, 150, 160), Location = new Point(167, 11) };
            
            // Market status
            pnlMarketIndicator = CreateStatusDot(280, 12);
            lblMarketStatus = new LabelControl { Text = "Piyasa: --", Font = new Font("Segoe UI", 9F), ForeColor = Color.FromArgb(150, 150, 160), Location = new Point(297, 11) };
            
            pnlStatusBar.Controls.AddRange(new Control[] { pnlGroqIndicator, lblGroqStatus, pnlDataIndicator, lblDataStatus, pnlMarketIndicator, lblMarketStatus });
            this.Controls.Add(pnlStatusBar);

            // ═══════════════════════════════════════════════════════════
            // OFFLINE BANNER (shown when no API key)
            // ═══════════════════════════════════════════════════════════
            lblOfflineBanner = new LabelControl();
            lblOfflineBanner.Dock = DockStyle.Top;
            lblOfflineBanner.AutoSizeMode = LabelAutoSizeMode.None;
            lblOfflineBanner.Height = 35;
            lblOfflineBanner.Text = "   ℹ️ Çevrimdışı mod aktif. API anahtarı eklenince canlı LLM analizi kullanılabilir.";
            lblOfflineBanner.Appearance.BackColor = Color.FromArgb(50, 60, 80);
            lblOfflineBanner.Appearance.ForeColor = Color.FromArgb(180, 200, 255);
            lblOfflineBanner.Appearance.Font = new Font("Segoe UI", 9.5F);
            lblOfflineBanner.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Center;
            lblOfflineBanner.Visible = !AiProviderFactory.IsOnlineAvailable();
            this.Controls.Add(lblOfflineBanner);

            // ═══════════════════════════════════════════════════════════
            // LEFT TOPICS PANEL
            // ═══════════════════════════════════════════════════════════
            pnlTopics = new PanelControl();
            pnlTopics.Dock = DockStyle.Left;
            pnlTopics.Width = 180;
            pnlTopics.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlTopics.Appearance.BackColor = Color.FromArgb(30, 41, 59);
            pnlTopics.Padding = new Padding(15, 20, 15, 15);
            
            var lblTopicsHeader = new LabelControl { Text = "📋 KONULAR", Font = new Font("Segoe UI", 10F, FontStyle.Bold), ForeColor = Color.FromArgb(100, 110, 130), Location = new Point(15, 10) };
            pnlTopics.Controls.Add(lblTopicsHeader);
            
            int topicY = 45;
            btnTopicSavings = CreateTopicButton("💰 Tasarruf", topicY, "Tasarruf", "Bu ay harcamalarımı analiz et ve tasarruf önerileri sun.");
            btnTopicInvestment = CreateTopicButton("📈 Yatırım", topicY += 45, "Yatırım", "Portföy riskimi değerlendir ve çeşitlendirme önerileri ver.");
            btnTopicCredit = CreateTopicButton("💳 Kredi", topicY += 45, "Kredi", "Kredi borcumu ve ödeme planımı özetle.");
            btnTopicDeposit = CreateTopicButton("🏦 Mevduat", topicY += 45, "Mevduat", "Mevcut bakiyem için faiz getirisi senaryosu yap.");
            btnTopicStock = CreateTopicButton("📊 Borsa", topicY += 45, "Borsa", "");
            btnTopicStock.Click += BtnTopicStock_Click;
            
            pnlTopics.Controls.AddRange(new Control[] { btnTopicSavings, btnTopicInvestment, btnTopicCredit, btnTopicDeposit, btnTopicStock });
            this.Controls.Add(pnlTopics);

            // ═══════════════════════════════════════════════════════════
            // INPUT PANEL (Bottom)
            // ═══════════════════════════════════════════════════════════
            pnlInput = new PanelControl();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 130;
            pnlInput.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlInput.Appearance.BackColor = Color.FromArgb(25, 28, 38);
            pnlInput.Padding = new Padding(15, 10, 15, 10);
            
            lblTyping = new LabelControl { Text = "⏳ Yanıt hazırlanıyor...", Font = new Font("Segoe UI", 10F, FontStyle.Italic), ForeColor = Color.FromArgb(100, 180, 255), Location = new Point(20, 8), Visible = false };
            
            txtUserInput = new MemoEdit();
            txtUserInput.Location = new Point(15, 30);
            txtUserInput.Size = new Size(580, 80);
            txtUserInput.Properties.ScrollBars = ScrollBars.Vertical;
            txtUserInput.Properties.Appearance.BackColor = Color.FromArgb(30, 41, 59);
            txtUserInput.Properties.Appearance.ForeColor = Color.FromArgb(248, 250, 252);
            txtUserInput.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtUserInput.Properties.AppearanceFocused.BorderColor = Color.FromArgb(212, 175, 55);
            txtUserInput.Properties.AppearanceFocused.Options.UseBorderColor = true;
            txtUserInput.Properties.NullValuePrompt = "💬 Finansal sorularınızı yazın veya soldaki konulardan seçin...";
            txtUserInput.Properties.NullValuePromptShowForEmptyValue = true;
            txtUserInput.KeyDown += TxtUserInput_KeyDown;
            
            btnSend = new SimpleButton();
            btnSend.Text = "Gönder 📤";
            btnSend.Location = new Point(605, 30);
            btnSend.Size = new Size(100, 80);
            btnSend.Appearance.BackColor = Color.FromArgb(212, 175, 55);
            btnSend.Appearance.ForeColor = Color.FromArgb(15, 23, 42);
            btnSend.Appearance.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.AppearanceHovered.BackColor = Color.FromArgb(235, 195, 75);
            btnSend.AppearanceHovered.Options.UseBackColor = true;
            btnSend.AppearancePressed.BackColor = Color.FromArgb(180, 145, 40);
            btnSend.AppearancePressed.Options.UseBackColor = true;
            btnSend.Click += BtnSend_Click;
            
            pnlInput.Controls.AddRange(new Control[] { lblTyping, txtUserInput, btnSend });
            this.Controls.Add(pnlInput);

            // ═══════════════════════════════════════════════════════════
            // CHAT CONTAINER (Center/Fill)
            // ═══════════════════════════════════════════════════════════
            pnlChatContainer = new Panel();
            pnlChatContainer.Dock = DockStyle.Fill;
            pnlChatContainer.BackColor = Color.FromArgb(18, 20, 28);
            pnlChatContainer.Padding = new Padding(10);
            pnlChatContainer.AutoScroll = true;
            
            flowChat = new FlowLayoutPanel();
            flowChat.Dock = DockStyle.Top;
            flowChat.AutoSize = true;
            flowChat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowChat.FlowDirection = FlowDirection.TopDown;
            flowChat.WrapContents = false;
            flowChat.BackColor = Color.Transparent;
            flowChat.Padding = new Padding(5);
            
            pnlChatContainer.Controls.Add(flowChat);
            this.Controls.Add(pnlChatContainer);

            // Bring panels to correct z-order
            pnlHeader.BringToFront();
            pnlStatusBar.BringToFront();
            lblOfflineBanner.BringToFront();
            pnlInput.BringToFront();
            pnlTopics.BringToFront();
        }

        private Panel CreateStatusDot(int x, int y)
        {
            var dot = new Panel();
            dot.Size = new Size(12, 12);
            dot.Location = new Point(x, y);
            dot.BackColor = Color.FromArgb(100, 100, 100);
            dot.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(dot.BackColor))
                    e.Graphics.FillEllipse(brush, 0, 0, 11, 11);
            };
            return dot;
        }

        private SimpleButton CreateTopicButton(string text, int y, string topic, string prompt)
        {
            var btn = new SimpleButton();
            btn.Text = text;
            btn.Location = new Point(10, y);
            btn.Size = new Size(135, 38);
            btn.Appearance.BackColor = Color.FromArgb(35, 40, 55);
            btn.Appearance.ForeColor = Color.FromArgb(200, 205, 220);
            btn.Appearance.Font = new Font("Segoe UI", 10F);
            btn.Appearance.Options.UseBackColor = true;
            btn.Appearance.Options.UseForeColor = true;
            btn.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Near;
            btn.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            
            if (!string.IsNullOrEmpty(prompt))
            {
                btn.Click += (s, e) => {
                    _currentTopic = topic;
                    txtUserInput.Text = prompt;
                    HighlightTopicButton(btn);
                };
            }
            
            return btn;
        }

        private void HighlightTopicButton(SimpleButton selected)
        {
            foreach (Control ctrl in pnlTopics.Controls)
            {
                if (ctrl is SimpleButton btn)
                {
                    btn.Appearance.BackColor = btn == selected 
                        ? Color.FromArgb(45, 110, 185) 
                        : Color.FromArgb(35, 40, 55);
                }
            }
        }

        private void BtnTopicStock_Click(object? sender, EventArgs e)
        {
            _currentTopic = "Borsa";
            HighlightTopicButton(btnTopicStock);
            
            // Show symbol input dialog
            var result = XtraInputBox.Show("Analiz edilecek sembolü girin (örn: AAPL, TSLA, MSFT):", "Hisse Senedi Analizi", "AAPL");
            if (result != null && !string.IsNullOrEmpty(result.ToString()))
            {
                _selectedStockSymbol = result.ToString()!.ToUpperInvariant().Trim();
                txtUserInput.Text = $"{_selectedStockSymbol} için teknik analiz ve piyasa değerlendirmesi yap.";
            }
        }

        private async void UpdateStatusIndicators()
        {
            // Groq status
            bool groqOnline = AiProviderFactory.IsOnlineAvailable();
            pnlGroqIndicator.BackColor = groqOnline ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
            lblGroqStatus.Text = groqOnline ? "Groq: Bağlı" : "Groq: Çevrimdışı";
            lblGroqStatus.ForeColor = groqOnline ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
            pnlGroqIndicator.Invalidate();
            
            // Data status (try to load some data)
            try
            {
                var ctx = await _contextBuilder.BuildContextAsync(
                    AppEvents.CurrentSession.UserId, 
                    AppEvents.CurrentSession.Username);
                
                bool hasData = ctx.AccountCount > 0 || ctx.NetWorth > 0;
                pnlDataIndicator.BackColor = hasData ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 82, 82);
                lblDataStatus.Text = hasData ? "Veri: Yüklendi" : "Veri: Yok";
                lblDataStatus.ForeColor = hasData ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 82, 82);
            }
            catch
            {
                pnlDataIndicator.BackColor = Color.FromArgb(255, 82, 82);
                lblDataStatus.Text = "Veri: Hata";
                lblDataStatus.ForeColor = Color.FromArgb(255, 82, 82);
            }
            pnlDataIndicator.Invalidate();
            
            // Market status (Finnhub)
            try
            {
                var quote = await _finnhubService.GetQuoteAsync("AAPL");
                bool marketOnline = quote != null && quote.C > 0;
                pnlMarketIndicator.BackColor = marketOnline ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
                lblMarketStatus.Text = marketOnline ? "Piyasa: Bağlı" : "Piyasa: Sınırlı";
                lblMarketStatus.ForeColor = marketOnline ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
            }
            catch
            {
                pnlMarketIndicator.BackColor = Color.FromArgb(255, 152, 0);
                lblMarketStatus.Text = "Piyasa: --";
                lblMarketStatus.ForeColor = Color.FromArgb(255, 152, 0);
            }
            pnlMarketIndicator.Invalidate();
        }

        private void AddWelcomeMessage()
        {
            string welcome = AiProviderFactory.IsOnlineAvailable()
                ? "Merhaba! 👋 Ben NovaBank AI Finansal Asistanınızım.\n\n" +
                  "Size şu konularda yardımcı olabilirim:\n" +
                  "• 💰 Tasarruf ve harcama analizi\n" +
                  "• 📈 Yatırım ve portföy değerlendirmesi\n" +
                  "• 💳 Kredi ve borç yönetimi\n" +
                  "• 🏦 Mevduat ve faiz hesaplama\n" +
                  "• 📊 Borsa ve hisse analizi\n\n" +
                  "Soldaki konulardan birini seçin veya sorunuzu yazın!"
                : "Merhaba! 👋 Şu anda çevrimdışı modda çalışıyorum.\n\n" +
                  "API anahtarı olmadan da size yardımcı olabilirim:\n" +
                  "• Finansal verilerinizin özeti\n" +
                  "• Harcama dağılımı raporu\n" +
                  "• Temel portföy bilgileri\n\n" +
                  "Soldaki konulardan birini seçerek başlayın!\n\n" +
                  "💡 İpucu: GROQ_API_KEY ortam değişkenini ayarlayarak\n" +
                  "   canlı AI analizine geçebilirsiniz.";
            
            AddChatBubble(welcome, false);
        }

        private void AddChatBubble(string message, bool isUser)
        {
            var wrapper = new Panel();
            wrapper.AutoSize = true;
            wrapper.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            wrapper.BackColor = Color.Transparent;
            wrapper.Width = flowChat.Width - 30;
            wrapper.MinimumSize = new Size(flowChat.Width - 30, 0);
            wrapper.Padding = new Padding(0, 5, 0, 5);

            var bubble = new Panel();
            bubble.AutoSize = true;
            bubble.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bubble.MaximumSize = new Size(450, 0);
            bubble.MinimumSize = new Size(80, 35);
            bubble.Padding = new Padding(15, 12, 15, 12);
            bubble.BackColor = isUser ? Color.FromArgb(45, 110, 185) : Color.FromArgb(40, 45, 58);
            
            bubble.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRect(new Rectangle(0, 0, bubble.Width, bubble.Height), 14))
                    bubble.Region = new Region(path);
            };

            var lblMsg = new Label();
            lblMsg.Text = message;
            lblMsg.Font = new Font("Segoe UI", 11F);
            lblMsg.ForeColor = Color.White;
            lblMsg.AutoSize = true;
            lblMsg.MaximumSize = new Size(420, 0);
            bubble.Controls.Add(lblMsg);

            var lblEmoji = new Label();
            lblEmoji.Font = new Font("Segoe UI Emoji", 14F);
            lblEmoji.ForeColor = Color.FromArgb(140, 150, 170);
            lblEmoji.AutoSize = true;
            lblEmoji.Text = isUser ? "👤" : "🤖";
            
            if (isUser)
            {
                lblEmoji.Location = new Point(wrapper.Width - 30, 8);
                bubble.Location = new Point(wrapper.Width - bubble.PreferredSize.Width - 40, 0);
            }
            else
            {
                lblEmoji.Location = new Point(5, 8);
                bubble.Location = new Point(30, 0);
            }

            wrapper.Controls.Add(lblEmoji);
            wrapper.Controls.Add(bubble);
            flowChat.Controls.Add(wrapper);
            
            pnlChatContainer.ScrollControlIntoView(wrapper);
            flowChat.PerformLayout();
            wrapper.PerformLayout();
            
            if (isUser)
            {
                bubble.Left = wrapper.Width - bubble.Width - 40;
                lblEmoji.Left = wrapper.Width - 30;
            }
        }

        private GraphicsPath CreateRoundedRect(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void ApplyDarkTheme()
        {
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.LookAndFeel.SkinName = "Office 2019 Black";
            this.Appearance.BackColor = Color.FromArgb(18, 20, 28);
            this.Appearance.Options.UseBackColor = true;
        }

        private void TxtUserInput_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _ = SendMessageAsync();
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            await SendMessageAsync();
        }

        private async Task SendMessageAsync()
        {
            if (_isSending) return;
            
            string userMessage = txtUserInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(userMessage)) return;

            // Add user message
            AddChatBubble(userMessage, true);
            txtUserInput.Text = "";
            
            // Set loading state
            _isSending = true;
            SetSendingState(true);

            try
            {
                // Build context
                var context = !string.IsNullOrEmpty(_selectedStockSymbol)
                    ? await _contextBuilder.BuildStockContextAsync(
                        AppEvents.CurrentSession.UserId,
                        AppEvents.CurrentSession.Username,
                        _selectedStockSymbol)
                    : await _contextBuilder.BuildContextAsync(
                        AppEvents.CurrentSession.UserId,
                        AppEvents.CurrentSession.Username);
                
                // Create request
                var request = new AiRequest
                {
                    UserMessage = userMessage,
                    Topic = _currentTopic,
                    Context = context
                };
                
                // Get response
                string response = await _aiProvider.AskAsync(request);
                AddChatBubble(response, false);
                
                // Reset stock symbol after use
                _selectedStockSymbol = "";
            }
            catch (Exception ex)
            {
                AddChatBubble($"❌ Hata: {ex.Message}", false);
            }
            finally
            {
                _isSending = false;
                SetSendingState(false);
            }
        }

        private void SetSendingState(bool isSending)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetSendingState(isSending)));
                return;
            }
            
            txtUserInput.Enabled = !isSending;
            btnSend.Enabled = !isSending;
            btnSend.Text = isSending ? "⏳..." : "Gönder 📤";
            lblTyping.Visible = isSending;
            Cursor.Current = isSending ? Cursors.WaitCursor : Cursors.Default;
        }
    }
}
