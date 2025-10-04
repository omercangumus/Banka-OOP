#nullable enable
using DevExpress.XtraEditors;
using DevExpress.LookAndFeel;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading.Tasks;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    public partial class AIAssistantForm : XtraForm
    {
        private readonly OpenRouterAIService _aiService;
        
        // Modern Chat UI Controls
        private PanelControl pnlHeader;
        private PanelControl pnlInput;
        private Panel pnlChatContainer;
        private FlowLayoutPanel flowChat;
        private MemoEdit txtUserInput;
        private SimpleButton btnSend;
        private SimpleButton btnClear;
        private LabelControl lblTyping;

        public AIAssistantForm()
        {
            // API Key - OpenRouter
            string apiKey = "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";
            _aiService = new OpenRouterAIService(apiKey);
            
            InitializeComponent();
            SetupModernChatUI();
            ApplyDarkTheme();
            AddWelcomeMessage();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form ayarları
            this.Name = "AIAssistantForm";
            this.Text = "🤖 NovaBank AI Asistan";
            this.Size = new Size(750, 850);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            this.ResumeLayout(false);
        }

        private void SetupModernChatUI()
        {
            // ═══════════════════════════════════════════════════════════
            // HEADER PANEL - Başlık ve Logo
            // ═══════════════════════════════════════════════════════════
            pnlHeader = new PanelControl();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 90;
            pnlHeader.Appearance.BackColor = Color.FromArgb(25, 28, 38);
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            
            // Gradient effect via paint
            pnlHeader.Paint += (s, e) => {
                using (var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, pnlHeader.Width, pnlHeader.Height),
                    Color.FromArgb(35, 45, 65),
                    Color.FromArgb(25, 28, 38),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, 0, 0, pnlHeader.Width, pnlHeader.Height);
                }
                
                // Bottom border
                using (var pen = new Pen(Color.FromArgb(60, 70, 90), 2))
                {
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };

            // AI Icon & Title
            var lblIcon = new LabelControl();
            lblIcon.Text = "🤖";
            lblIcon.Font = new Font("Segoe UI Emoji", 32F);
            lblIcon.Location = new Point(25, 18);
            lblIcon.AutoSizeMode = LabelAutoSizeMode.Default;
            pnlHeader.Controls.Add(lblIcon);

            var lblTitle = new LabelControl();
            lblTitle.Text = "AI Finansal Asistan";
            lblTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(85, 18);
            pnlHeader.Controls.Add(lblTitle);

            var lblSubtitle = new LabelControl();
            lblSubtitle.Text = "NovaBank yapay zeka destekli finansal danışmanınız";
            lblSubtitle.Font = new Font("Segoe UI", 11F);
            lblSubtitle.ForeColor = Color.FromArgb(140, 150, 170);
            lblSubtitle.Location = new Point(85, 52);
            pnlHeader.Controls.Add(lblSubtitle);

            // Online indicator
            var pnlOnline = new Panel();
            pnlOnline.Size = new Size(12, 12);
            pnlOnline.Location = new Point(695, 35);
            pnlOnline.BackColor = Color.FromArgb(76, 175, 80);
            pnlOnline.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(Color.FromArgb(76, 175, 80)))
                {
                    e.Graphics.FillEllipse(brush, 0, 0, 11, 11);
                }
            };
            pnlHeader.Controls.Add(pnlOnline);

            var lblOnline = new LabelControl();
            lblOnline.Text = "Çevrimiçi";
            lblOnline.Font = new Font("Segoe UI", 9F);
            lblOnline.ForeColor = Color.FromArgb(76, 175, 80);
            lblOnline.Location = new Point(640, 33);
            pnlHeader.Controls.Add(lblOnline);

            this.Controls.Add(pnlHeader);

            // ═══════════════════════════════════════════════════════════
            // CHAT CONTAINER - Scrollable Chat Area
            // ═══════════════════════════════════════════════════════════
            pnlChatContainer = new Panel();
            pnlChatContainer.Dock = DockStyle.Fill;
            pnlChatContainer.BackColor = Color.FromArgb(18, 20, 28);
            pnlChatContainer.Padding = new Padding(15);
            pnlChatContainer.AutoScroll = true;

            // FlowLayoutPanel for chat bubbles
            flowChat = new FlowLayoutPanel();
            flowChat.Dock = DockStyle.Top;
            flowChat.AutoSize = true;
            flowChat.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flowChat.FlowDirection = FlowDirection.TopDown;
            flowChat.WrapContents = false;
            flowChat.BackColor = Color.Transparent;
            flowChat.Padding = new Padding(10, 10, 25, 10);

            pnlChatContainer.Controls.Add(flowChat);
            this.Controls.Add(pnlChatContainer);

            // ═══════════════════════════════════════════════════════════
            // INPUT PANEL - Message Input Area
            // ═══════════════════════════════════════════════════════════
            pnlInput = new PanelControl();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 160;
            pnlInput.Appearance.BackColor = Color.FromArgb(25, 28, 38);
            pnlInput.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlInput.Padding = new Padding(20, 15, 20, 15);

            // Top border
            pnlInput.Paint += (s, e) => {
                using (var pen = new Pen(Color.FromArgb(50, 55, 70), 2))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnlInput.Width, 0);
                }
            };

            // Typing indicator
            lblTyping = new LabelControl();
            lblTyping.Text = "⏳ AI düşünüyor...";
            lblTyping.Font = new Font("Segoe UI", 10F, FontStyle.Italic);
            lblTyping.ForeColor = Color.FromArgb(100, 180, 255);
            lblTyping.Location = new Point(25, 8);
            lblTyping.Visible = false;
            pnlInput.Controls.Add(lblTyping);

            // User input - MemoEdit
            txtUserInput = new MemoEdit();
            txtUserInput.Location = new Point(20, 30);
            txtUserInput.Size = new Size(580, 75);
            txtUserInput.Properties.ScrollBars = ScrollBars.Vertical;
            txtUserInput.Properties.Appearance.BackColor = Color.FromArgb(35, 40, 55);
            txtUserInput.Properties.Appearance.ForeColor = Color.White;
            txtUserInput.Properties.Appearance.Font = new Font("Segoe UI", 12F);
            txtUserInput.Properties.Appearance.Options.UseBackColor = true;
            txtUserInput.Properties.Appearance.Options.UseForeColor = true;
            txtUserInput.Properties.Appearance.Options.UseFont = true;
            txtUserInput.Properties.NullValuePrompt = "💬 Mesajınızı buraya yazın... (Ctrl+Enter ile gönderin)";
            txtUserInput.Properties.NullValuePromptShowForEmptyValue = true;
            txtUserInput.Properties.AppearanceReadOnly.BackColor = Color.FromArgb(35, 40, 55);
            txtUserInput.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            txtUserInput.KeyDown += TxtUserInput_KeyDown;
            pnlInput.Controls.Add(txtUserInput);

            // Send Button - Modern gradient style
            btnSend = new SimpleButton();
            btnSend.Text = "Gönder 📤";
            btnSend.Location = new Point(610, 30);
            btnSend.Size = new Size(100, 40);
            btnSend.Appearance.BackColor = Color.FromArgb(56, 142, 60);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.Appearance.Options.UseFont = true;
            btnSend.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            // Clear Button
            btnClear = new SimpleButton();
            btnClear.Text = "🗑️ Temizle";
            btnClear.Location = new Point(610, 75);
            btnClear.Size = new Size(100, 30);
            btnClear.Appearance.BackColor = Color.FromArgb(60, 63, 75);
            btnClear.Appearance.ForeColor = Color.FromArgb(200, 200, 210);
            btnClear.Appearance.Font = new Font("Segoe UI", 9F);
            btnClear.Appearance.Options.UseBackColor = true;
            btnClear.Appearance.Options.UseForeColor = true;
            btnClear.Appearance.Options.UseFont = true;
            btnClear.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            btnClear.Click += BtnClear_Click;
            pnlInput.Controls.Add(btnClear);

            // Quick suggestion buttons
            var pnlSuggestions = new FlowLayoutPanel();
            pnlSuggestions.Location = new Point(20, 115);
            pnlSuggestions.Size = new Size(690, 38);
            pnlSuggestions.BackColor = Color.Transparent;
            pnlSuggestions.WrapContents = false;

            string[] suggestions = new[] { "💰 Tasarruf", "📈 Yatırım", "💳 Kredi", "🏦 Mevduat", "📊 Borsa" };
            foreach (var suggestion in suggestions)
            {
                var btnSuggestion = new SimpleButton();
                btnSuggestion.Text = suggestion;
                btnSuggestion.Height = 32;
                btnSuggestion.Width = 110;
                btnSuggestion.Appearance.BackColor = Color.FromArgb(45, 50, 65);
                btnSuggestion.Appearance.ForeColor = Color.FromArgb(170, 175, 190);
                btnSuggestion.Appearance.Font = new Font("Segoe UI", 9.5F);
                btnSuggestion.Appearance.Options.UseBackColor = true;
                btnSuggestion.Appearance.Options.UseForeColor = true;
                btnSuggestion.Appearance.Options.UseFont = true;
                btnSuggestion.Margin = new Padding(0, 0, 10, 0);
                btnSuggestion.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
                btnSuggestion.Click += (s, e) => {
                    string topic = suggestion.Substring(2).Trim();
                    txtUserInput.Text = $"{topic} hakkında bilgi verir misin?";
                    _ = SendMessage();
                };
                pnlSuggestions.Controls.Add(btnSuggestion);
            }
            pnlInput.Controls.Add(pnlSuggestions);

            this.Controls.Add(pnlInput);

            // Layer ordering
            pnlHeader.BringToFront();
            pnlInput.BringToFront();
        }

        private void AddWelcomeMessage()
        {
            string welcomeText = "Merhaba! 👋 Ben NovaBank AI Finansal Asistanınızım.\n\n" +
                "Size şu konularda yardımcı olabilirim:\n" +
                "• 💰 Tasarruf ve bütçe yönetimi\n" +
                "• 📈 Yatırım tavsiyeleri\n" +
                "• 💳 Kredi ve kart bilgileri\n" +
                "• 🏦 Mevduat hesapları\n" +
                "• 📊 Borsa ve hisse analizi\n\n" +
                "Nasıl yardımcı olabilirim?";
            
            AddChatBubble(welcomeText, false);
        }

        private void AddChatBubble(string message, bool isUser)
        {
            // Wrapper panel for alignment
            var wrapperPanel = new Panel();
            wrapperPanel.AutoSize = true;
            wrapperPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            wrapperPanel.BackColor = Color.Transparent;
            wrapperPanel.Width = flowChat.Width - 50;
            wrapperPanel.MinimumSize = new Size(flowChat.Width - 50, 0);
            wrapperPanel.Padding = new Padding(0, 5, 0, 5);

            // Chat bubble panel
            var bubblePanel = new Panel();
            bubblePanel.AutoSize = true;
            bubblePanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            bubblePanel.MaximumSize = new Size(480, 0);
            bubblePanel.MinimumSize = new Size(100, 40);
            bubblePanel.Padding = new Padding(18, 14, 18, 14);
            
            if (isUser)
            {
                // User bubble - Right side, Blue
                bubblePanel.BackColor = Color.FromArgb(45, 110, 185);
            }
            else
            {
                // AI bubble - Left side, Dark gray
                bubblePanel.BackColor = Color.FromArgb(45, 50, 62);
            }

            // Rounded corners
            bubblePanel.Paint += (s, e) => {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (var path = CreateRoundedRectPath(new Rectangle(0, 0, bubblePanel.Width, bubblePanel.Height), 16))
                {
                    bubblePanel.Region = new Region(path);
                }
            };

            // Message label
            var lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Font = new Font("Segoe UI", 12F);
            lblMessage.ForeColor = Color.White;
            lblMessage.AutoSize = true;
            lblMessage.MaximumSize = new Size(440, 0);
            lblMessage.Padding = new Padding(0);
            lblMessage.Margin = new Padding(0);
            bubblePanel.Controls.Add(lblMessage);

            // Sender label (emoji indicator)
            var lblSender = new Label();
            lblSender.Font = new Font("Segoe UI Emoji", 14F);
            lblSender.ForeColor = Color.FromArgb(150, 160, 180);
            lblSender.AutoSize = true;
            
            if (isUser)
            {
                lblSender.Text = "👤";
                lblSender.Location = new Point(wrapperPanel.Width - 35, 8);
                bubblePanel.Location = new Point(wrapperPanel.Width - bubblePanel.PreferredSize.Width - 45, 0);
            }
            else
            {
                lblSender.Text = "🤖";
                lblSender.Location = new Point(5, 8);
                bubblePanel.Location = new Point(35, 0);
            }

            wrapperPanel.Controls.Add(lblSender);
            wrapperPanel.Controls.Add(bubblePanel);
            
            // Add to flow
            flowChat.Controls.Add(wrapperPanel);
            
            // Scroll to bottom
            pnlChatContainer.ScrollControlIntoView(wrapperPanel);
            
            // Force layout recalculation
            flowChat.PerformLayout();
            wrapperPanel.PerformLayout();
            
            // Fix bubble position after layout
            if (isUser)
            {
                bubblePanel.Left = wrapperPanel.Width - bubblePanel.Width - 45;
                lblSender.Left = wrapperPanel.Width - 35;
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
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
                _ = SendMessage();
            }
        }

        private async void BtnSend_Click(object? sender, EventArgs e)
        {
            await SendMessage();
        }

        private async Task SendMessage()
        {
            string userMessage = txtUserInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(userMessage)) return;

            // Add user message bubble
            AddChatBubble(userMessage, true);
            txtUserInput.Text = "";
            
            // Show typing indicator
            lblTyping.Visible = true;
            btnSend.Enabled = false;

            try
            {
                // Get AI response
                string response = await _aiService.GetResponseAsync(userMessage);
                
                // Add AI response bubble
                AddChatBubble(response, false);
            }
            catch (Exception ex)
            {
                AddChatBubble($"❌ Hata oluştu: {ex.Message}", false);
            }
            finally
            {
                lblTyping.Visible = false;
                btnSend.Enabled = true;
                txtUserInput.Focus();
            }
        }

        private void BtnClear_Click(object? sender, EventArgs e)
        {
            var result = XtraMessageBox.Show(
                "Tüm sohbet geçmişini temizlemek istediğinize emin misiniz?",
                "Sohbeti Temizle",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _aiService.ClearHistory();
                flowChat.Controls.Clear();
                AddWelcomeMessage();
            }
        }
    }
}
