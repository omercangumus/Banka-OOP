#nullable enable
using DevExpress.XtraEditors;
using DevExpress.XtraBars.Navigation;
using DevExpress.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using BankApp.Infrastructure.Services;
using BankApp.Infrastructure.Services.AI;
using BankApp.Infrastructure.Services.Dashboard;
using BankApp.Infrastructure.Data;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// AI Financial Assistant V3 - Premium DevExpress UI with Tool Calling
    /// </summary>
    public class AIAssistantFormV3 : XtraForm
    {
        private readonly IAIProvider _aiProvider;
        private readonly AiContextBuilder _contextBuilder;
        private readonly AIActionRouter _actionRouter;
        private readonly DashboardSummaryService _dashboardService;
        
        // State
        private bool _isSending = false;
        private string _currentTopic = "Genel";
        private string _stockContext = "";
        private List<ChatMessage> _chatHistory = new();
        
        // Main Layout
        private SplitContainerControl splitMain;
        
        // Left Panel - Navigation
        private AccordionControl accordionNav;
        private AccordionControlElement grpTopics;
        private AccordionControlElement grpQuickActions;
        
        // Right Panel - Chat
        private PanelControl pnlChatArea;
        private PanelControl pnlStatusBar;
        private PanelControl pnlChatHistory;
        private FlowLayoutPanel flowChatMessages;
        private PanelControl pnlInput;
        private MemoEdit txtInput;
        private SimpleButton btnSend;
        private SimpleButton btnAttachImage;
        private LabelControl lblTyping;
        
        // Prompt Chips
        private FlowLayoutPanel flowPromptChips;
        
        // Status indicators
        private LabelControl lblStatus;

        public AIAssistantFormV3(string? stockContext = null)
        {
            System.Diagnostics.Debug.WriteLine("=== AIASSISTANTFORMV3 LOADED v2 ===");
            
            _stockContext = stockContext ?? "";
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _actionRouter = new AIActionRouter();
            _dashboardService = new DashboardSummaryService(new DapperContext());
            
            SetupActionRouter();
            InitializeComponent();
            SetupUI();
            ApplyTheme();
            AddWelcomeMessage();
            
            if (!string.IsNullOrEmpty(_stockContext))
            {
                _currentTopic = "Borsa";
                ShowPromptChips("Borsa");
            }
        }

        private void SetupActionRouter()
        {
            _actionRouter.ActionRequested += OnActionRequested;
            _actionRouter.ConfirmationRequired += OnConfirmationRequired;
        }

        private void OnActionRequested(object? sender, ActionEventArgs e)
        {
            switch (e.Action)
            {
                case "Navigate":
                    e.Handled = true;
                    var screen = e.Parameters.GetValueOrDefault("screen")?.ToString() ?? "";
                    e.ResultMessage = $"'{screen}' ekranına yönlendiriliyor...";
                    // MainForm'a event gönder
                    AppEvents.NotifyDataChanged("AIAssistant", $"Navigate:{screen}");
                    break;
                    
                case "GetUserSnapshot":
                    e.Handled = true;
                    _ = GetUserSnapshotAsync(e);
                    break;
            }
        }

        private async Task GetUserSnapshotAsync(ActionEventArgs e)
        {
            try
            {
                var data = await _dashboardService.GetFullDashboardDataAsync(AppEvents.CurrentSession.UserId);
                e.ResultData = data;
                e.ResultMessage = $"Net Varlık: ₺{data.NetWorth:N0}, Bakiye: ₺{data.TotalBalance:N0}, Borç: ₺{data.TotalDebt:N0}";
            }
            catch (Exception ex)
            {
                e.ResultMessage = $"Veri alınamadı: {ex.Message}";
            }
        }

        private void OnConfirmationRequired(object? sender, ConfirmationEventArgs e)
        {
            var result = XtraMessageBox.Show(
                e.Message + "\n\nBu işlem onayınız olmadan gerçekleştirilmeyecektir.",
                "İşlem Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            e.Confirmed = result == DialogResult.Yes;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "AIAssistantFormV3";
            this.Text = "🤖 NovaBank AI Asistan";
            this.Size = new Size(1000, 700);
            this.MinimumSize = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.ResumeLayout(false);
        }

        private void SetupUI()
        {
            // Main Split Container
            splitMain = new SplitContainerControl();
            splitMain.Dock = DockStyle.Fill;
            splitMain.Horizontal = false;
            splitMain.SplitterPosition = 220;
            splitMain.FixedPanel = SplitFixedPanel.Panel1;
            splitMain.ShowSplitGlyph = DefaultBoolean.False;
            this.Controls.Add(splitMain);

            // ═══════════════════════════════════════════════════════════
            // LEFT PANEL - Navigation
            // ═══════════════════════════════════════════════════════════
            SetupLeftPanel();

            // ═══════════════════════════════════════════════════════════
            // RIGHT PANEL - Chat Area
            // ═══════════════════════════════════════════════════════════
            SetupRightPanel();
        }

        private void SetupLeftPanel()
        {
            var pnlLeft = new PanelControl();
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlLeft.Appearance.BackColor = Color.FromArgb(22, 27, 34);
            splitMain.Panel1.Controls.Add(pnlLeft);

            // Accordion Navigation
            accordionNav = new AccordionControl();
            accordionNav.Dock = DockStyle.Fill;
            accordionNav.AllowItemSelection = true;
            accordionNav.Appearance.AccordionControl.BackColor = Color.FromArgb(22, 27, 34);
            accordionNav.Appearance.AccordionControl.ForeColor = Color.White;

            // Topics Group
            grpTopics = new AccordionControlElement { Text = "📋 Konular", Style = ElementStyle.Group, Expanded = true };
            
            var topics = new (string icon, string name, string key)[]
            {
                ("💰", "Tasarruf", "Tasarruf"),
                ("📈", "Yatırım", "Yatırım"),
                ("💳", "Kredi", "Kredi"),
                ("🏦", "Mevduat", "Mevduat"),
                ("📊", "Borsa", "Borsa"),
                ("💵", "Transfer", "Transfer")
            };

            foreach (var (icon, name, key) in topics)
            {
                var item = new AccordionControlElement 
                { 
                    Text = $"{icon} {name}", 
                    Style = ElementStyle.Item,
                    Tag = key
                };
                item.Click += (s, e) => SelectTopic(key);
                grpTopics.Elements.Add(item);
            }

            // Quick Actions Group
            grpQuickActions = new AccordionControlElement { Text = "⚡ Hızlı İşlemler", Style = ElementStyle.Group, Expanded = true };
            
            var actions = new (string icon, string name, string action)[]
            {
                ("📊", "Portföy Özeti", "GetUserSnapshot"),
                ("🏠", "Dashboard'a Git", "Navigate:Dashboard"),
                ("💸", "Transfer Yap", "Navigate:Transfer")
            };

            foreach (var (icon, name, action) in actions)
            {
                var item = new AccordionControlElement 
                { 
                    Text = $"{icon} {name}", 
                    Style = ElementStyle.Item,
                    Tag = action
                };
                item.Click += (s, e) => ExecuteQuickAction(action);
                grpQuickActions.Elements.Add(item);
            }

            accordionNav.Elements.Add(grpTopics);
            accordionNav.Elements.Add(grpQuickActions);
            pnlLeft.Controls.Add(accordionNav);
        }

        private void SetupRightPanel()
        {
            pnlChatArea = new PanelControl();
            pnlChatArea.Dock = DockStyle.Fill;
            pnlChatArea.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlChatArea.Appearance.BackColor = Color.FromArgb(13, 17, 23);
            splitMain.Panel2.Controls.Add(pnlChatArea);

            // Status Bar (compact)
            pnlStatusBar = new PanelControl();
            pnlStatusBar.Dock = DockStyle.Top;
            pnlStatusBar.Height = 32;
            pnlStatusBar.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlStatusBar.Appearance.BackColor = Color.FromArgb(22, 27, 34);
            
            lblStatus = new LabelControl();
            lblStatus.Text = GetStatusText();
            lblStatus.Location = new Point(12, 8);
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(139, 148, 158);
            pnlStatusBar.Controls.Add(lblStatus);
            pnlChatArea.Controls.Add(pnlStatusBar);

            // Prompt Chips Area
            flowPromptChips = new FlowLayoutPanel();
            flowPromptChips.Dock = DockStyle.Top;
            flowPromptChips.Height = 45;
            flowPromptChips.BackColor = Color.FromArgb(22, 27, 34);
            flowPromptChips.Padding = new Padding(10, 8, 10, 8);
            flowPromptChips.AutoScroll = true;
            flowPromptChips.WrapContents = false;
            pnlChatArea.Controls.Add(flowPromptChips);

            // Input Panel (Bottom)
            pnlInput = new PanelControl();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 80;
            pnlInput.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlInput.Appearance.BackColor = Color.FromArgb(22, 27, 34);
            pnlInput.Padding = new Padding(12, 8, 12, 8);
            
            // Typing indicator
            lblTyping = new LabelControl();
            lblTyping.Text = "⏳ AI düşünüyor...";
            lblTyping.Location = new Point(15, 5);
            lblTyping.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
            lblTyping.ForeColor = Color.FromArgb(88, 166, 255);
            lblTyping.Visible = false;
            pnlInput.Controls.Add(lblTyping);

            // Input TextBox
            txtInput = new MemoEdit();
            txtInput.Location = new Point(12, 22);
            txtInput.Size = new Size(620, 50);
            txtInput.Properties.ScrollBars = ScrollBars.None;
            txtInput.Properties.Appearance.BackColor = Color.FromArgb(33, 38, 45);
            txtInput.Properties.Appearance.ForeColor = Color.White;
            txtInput.Properties.Appearance.Font = new Font("Segoe UI", 11F);
            txtInput.Properties.AppearanceFocused.BorderColor = Color.FromArgb(88, 166, 255);
            txtInput.Properties.NullValuePrompt = "Mesajınızı yazın... (Ctrl+Enter gönderir)";
            txtInput.Properties.NullValuePromptShowForEmptyValue = true;
            txtInput.KeyDown += TxtInput_KeyDown;
            pnlInput.Controls.Add(txtInput);

            // Send Button
            btnSend = new SimpleButton();
            btnSend.Text = "Gönder";
            btnSend.Size = new Size(80, 50);
            btnSend.Location = new Point(640, 22);
            btnSend.Appearance.BackColor = Color.FromArgb(35, 134, 54);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Appearance.Font = new Font("Segoe UI Semibold", 10F);
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            pnlChatArea.Controls.Add(pnlInput);

            // Chat History Panel (Fill)
            pnlChatHistory = new PanelControl();
            pnlChatHistory.Dock = DockStyle.Fill;
            pnlChatHistory.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlChatHistory.Appearance.BackColor = Color.FromArgb(13, 17, 23);
            pnlChatHistory.AutoScroll = true;

            flowChatMessages = new FlowLayoutPanel();
            flowChatMessages.Dock = DockStyle.Top;
            flowChatMessages.AutoSize = true;
            flowChatMessages.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowChatMessages.FlowDirection = FlowDirection.TopDown;
            flowChatMessages.WrapContents = false;
            flowChatMessages.BackColor = Color.Transparent;
            flowChatMessages.Padding = new Padding(15, 10, 15, 10);
            
            pnlChatHistory.Controls.Add(flowChatMessages);
            pnlChatArea.Controls.Add(pnlChatHistory);

            // Z-Order
            pnlStatusBar.BringToFront();
            flowPromptChips.BringToFront();
            pnlInput.BringToFront();

            // Resize handler
            this.Resize += (s, e) => AdjustInputWidth();
            AdjustInputWidth();
        }

        private void AdjustInputWidth()
        {
            if (txtInput != null && pnlInput != null)
            {
                int availWidth = pnlInput.Width - 120;
                txtInput.Width = Math.Max(200, availWidth);
                btnSend.Left = txtInput.Right + 8;
            }
        }

        private string GetStatusText()
        {
            bool online = AiProviderFactory.IsOnlineAvailable();
            string status = online ? "🟢 Groq Bağlı" : "🟡 Çevrimdışı";
            return $"{status} • 📊 Veri Hazır • Konu: {_currentTopic}";
        }

        private void SelectTopic(string topic)
        {
            _currentTopic = topic;
            lblStatus.Text = GetStatusText();
            ShowPromptChips(topic);
        }

        private void ShowPromptChips(string topic)
        {
            flowPromptChips.Controls.Clear();

            var prompts = GetPromptsForTopic(topic);
            foreach (var prompt in prompts)
            {
                var chip = CreateChipButton(prompt);
                flowPromptChips.Controls.Add(chip);
            }
        }

        private string[] GetPromptsForTopic(string topic)
        {
            return topic switch
            {
                "Tasarruf" => new[] { "Bu ay harcamalarımı analiz et", "Tasarruf önerileri ver", "Gereksiz harcamalarım neler?" },
                "Yatırım" => new[] { "Portföy riskimi değerlendir", "Çeşitlendirme öner", "Hangi sektöre yatırım yapmalıyım?" },
                "Kredi" => new[] { "Kredi borcumu özetle", "Ödeme planımı göster", "Kredi kapatma stratejisi" },
                "Mevduat" => new[] { "Faiz getirisi hesapla", "Vadeli mevduat karşılaştır", "Param nerede değerlenir?" },
                "Borsa" => new[] { "Teknik analiz yap", "Destek/direnç seviyeleri", "RSI/MACD yorumu", "Trend analizi" },
                "Transfer" => new[] { "Son transferlerim", "Transfer limitlerim", "EFT/Havale farkı nedir?" },
                _ => new[] { "Finansal durumumu özetle", "Portföyümü göster", "Tavsiye ver" }
            };
        }

        private SimpleButton CreateChipButton(string text)
        {
            var chip = new SimpleButton();
            chip.Text = text;
            chip.Height = 28;
            chip.AutoSize = true;
            chip.Padding = new Padding(12, 0, 12, 0);
            chip.Appearance.BackColor = Color.FromArgb(33, 38, 45);
            chip.Appearance.ForeColor = Color.FromArgb(201, 209, 217);
            chip.Appearance.Font = new Font("Segoe UI", 9F);
            chip.Appearance.Options.UseBackColor = true;
            chip.Appearance.Options.UseForeColor = true;
            chip.AppearanceHovered.BackColor = Color.FromArgb(48, 54, 61);
            chip.AppearanceHovered.Options.UseBackColor = true;
            chip.Click += (s, e) => {
                txtInput.Text = text;
                _ = SendMessageAsync();
            };
            return chip;
        }

        private void ExecuteQuickAction(string action)
        {
            if (action.StartsWith("Navigate:"))
            {
                var screen = action.Substring(9);
                AppEvents.NotifyDataChanged("AIAssistant", $"Navigate:{screen}");
                AddChatBubble($"'{screen}' ekranına yönlendiriliyor...", false);
            }
            else if (action == "GetUserSnapshot")
            {
                txtInput.Text = "Finansal durumumu özetle";
                _ = SendMessageAsync();
            }
        }

        private void AddWelcomeMessage()
        {
            string welcome = "Merhaba! 👋 Ben NovaBank AI Asistanınızım.\n\n" +
                "Size yardımcı olabileceğim konular:\n" +
                "• 💰 Tasarruf ve bütçe analizi\n" +
                "• 📈 Yatırım ve portföy yönetimi\n" +
                "• 💳 Kredi ve borç takibi\n" +
                "• 📊 Borsa ve teknik analiz\n\n" +
                "Soldaki menüden konu seçin veya doğrudan sorunuzu yazın!";
            
            if (!string.IsNullOrEmpty(_stockContext))
            {
                welcome += $"\n\n📊 Bağlam: {_stockContext}";
            }
            
            AddChatBubble(welcome, false);
        }

        private void AddChatBubble(string message, bool isUser)
        {
            var wrapper = new Panel();
            wrapper.AutoSize = true;
            wrapper.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            wrapper.BackColor = Color.Transparent;
            wrapper.Padding = new Padding(0, 4, 0, 4);
            wrapper.Width = flowChatMessages.Width - 40;
            wrapper.MinimumSize = new Size(flowChatMessages.Width - 40, 0);

            var bubble = new Panel();
            bubble.AutoSize = true;
            bubble.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bubble.MaximumSize = new Size(500, 0);
            bubble.MinimumSize = new Size(60, 30);
            bubble.Padding = new Padding(14, 10, 14, 10);
            bubble.BackColor = isUser 
                ? Color.FromArgb(31, 111, 235)  // Blue for user
                : Color.FromArgb(33, 38, 45);   // Dark for AI

            // Rounded corners
            bubble.Paint += (s, e) => {
                using (var path = CreateRoundedRect(new Rectangle(0, 0, bubble.Width, bubble.Height), 12))
                    bubble.Region = new Region(path);
            };

            var lblMsg = new Label();
            lblMsg.Text = message;
            lblMsg.Font = new Font("Segoe UI", 10.5F);
            lblMsg.ForeColor = Color.White;
            lblMsg.AutoSize = true;
            lblMsg.MaximumSize = new Size(470, 0);
            bubble.Controls.Add(lblMsg);

            // Position
            if (isUser)
            {
                bubble.Anchor = AnchorStyles.Right;
            }
            
            wrapper.Controls.Add(bubble);
            flowChatMessages.Controls.Add(wrapper);

            // Scroll to bottom
            pnlChatHistory.ScrollControlIntoView(wrapper);
            
            // Reposition after layout
            wrapper.PerformLayout();
            if (isUser)
            {
                bubble.Left = wrapper.Width - bubble.Width - 10;
            }
            else
            {
                bubble.Left = 10;
            }

            _chatHistory.Add(new ChatMessage { IsUser = isUser, Text = message });
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

        private void ApplyTheme()
        {
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.LookAndFeel.SkinName = "Office 2019 Black";
            this.Appearance.BackColor = Color.FromArgb(13, 17, 23);
            this.Appearance.Options.UseBackColor = true;
        }

        private void TxtInput_KeyDown(object? sender, KeyEventArgs e)
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
            
            string userMessage = txtInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(userMessage)) return;

            AddChatBubble(userMessage, true);
            txtInput.Text = "";
            
            _isSending = true;
            SetSendingState(true);

            try
            {
                // Build context
                var context = await _contextBuilder.BuildContextAsync(
                    AppEvents.CurrentSession.UserId,
                    AppEvents.CurrentSession.Username);
                
                // Add stock context if available
                if (!string.IsNullOrEmpty(_stockContext))
                {
                    context.StockSymbol = _stockContext;
                }

                var request = new AiRequest
                {
                    UserMessage = userMessage,
                    Topic = _currentTopic,
                    Context = context
                };

                // Get AI response
                string response = await _aiProvider.AskAsync(request);
                
                // Check for tool calls
                var toolCall = _actionRouter.ParseToolCall(response);
                if (toolCall != null)
                {
                    var result = await _actionRouter.ExecuteActionAsync(toolCall);
                    response = response.Replace(toolCall.RawJson, "").Trim();
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        response += $"\n\n✅ {result.Message}";
                    }
                }

                AddChatBubble(response, false);
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

        private void SetSendingState(bool sending)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => SetSendingState(sending)));
                return;
            }
            
            txtInput.Enabled = !sending;
            btnSend.Enabled = !sending;
            btnSend.Text = sending ? "..." : "Gönder";
            lblTyping.Visible = sending;
        }

        private class ChatMessage
        {
            public bool IsUser { get; set; }
            public string Text { get; set; } = "";
        }
    }
}
