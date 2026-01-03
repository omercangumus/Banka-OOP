using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace BankApp.UI.Forms
{
    public partial class SupportForm : XtraForm
    {
        private RichTextBox txtChatHistory;
        private TextBox txtUserInput;
        private SimpleButton btnSend;
        private SimpleButton btnEscalate;

        public SupportForm()
        {
            InitializeComponent();
            AddBotMessage("Merhaba, ben NovaBank Asistanı. Size nasıl yardımcı olabilirim?");
        }

        private void InitializeComponent()
        {
            this.Text = "NovaBank Destek";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.LookAndFeel.SetSkinStyle("Office 2019 Black");

            // Chat History (WhatsApp tarzı)
            txtChatHistory = new RichTextBox
            {
                Location = new Point(20, 20),
                Size = new Size(440, 450),
                ReadOnly = true,
                BackColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.None
            };
            this.Controls.Add(txtChatHistory);

            // User Input
            txtUserInput = new TextBox
            {
                Location = new Point(20, 490),
                Size = new Size(340, 30),
                Font = new Font("Segoe UI", 11),
                PlaceholderText = "Mesajınızı yazın..."
            };
            txtUserInput.KeyPress += (s, e) => {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    e.Handled = true;
                    btnSend.PerformClick();
                }
            };
            this.Controls.Add(txtUserInput);

            // Send Button
            btnSend = new SimpleButton
            {
                Text = "Gönder",
                Location = new Point(370, 490),
                Size = new Size(90, 30)
            };
            btnSend.Appearance.BackColor = Color.FromArgb(0, 210, 255);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Click += BtnSend_Click;
            this.Controls.Add(btnSend);

            // Escalate to Admin Button (Initially Hidden)
            btnEscalate = new SimpleButton
            {
                Text = "📧 Admine İlet",
                Location = new Point(150, 530),
                Size = new Size(180, 40),
                Visible = false
            };
            btnEscalate.Appearance.BackColor = Color.FromArgb(255, 0, 122);
            btnEscalate.Appearance.ForeColor = Color.White;
            btnEscalate.Appearance.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnEscalate.Click += BtnEscalate_Click;
            this.Controls.Add(btnEscalate);
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string userMsg = txtUserInput.Text.Trim();
            if (string.IsNullOrEmpty(userMsg)) return;

            AddUserMessage(userMsg);
            
            // Get AI Response
            string botReply = GetAIResponse(userMsg);
            AddBotMessage(botReply);

            txtUserInput.Clear();

            // Show escalate button if user asks for human support
            string lowerMsg = userMsg.ToLower();
            if (lowerMsg.Contains("yetkili") || lowerMsg.Contains("admin") || 
                lowerMsg.Contains("insan") || lowerMsg.Contains("sorunu çözemedin"))
            {
                btnEscalate.Visible = true;
            }
        }

        private string GetAIResponse(string input)
        {
            string lower = input.ToLower();

            // Kredi Sorguları
            if (lower.Contains("kredi"))
                return "💳 Kredi faiz oranlarımız %3.5'ten başlamaktadır. Başvuru için Ana Menü > Krediler bölümüne gidin.";

            // Hesap/Bakiye
            if (lower.Contains("hesap") || lower.Contains("bakiye") || lower.Contains("para"))
                return "💰 Hesap bakiyenizi Dashboard'dan anlık olarak görebilirsiniz.";

            // Transfer
            if (lower.Contains("transfer") || lower.Contains("gönder"))
                return "📤 Para transferi için Ana Menü > Para Transferi'ne tıklayın. IBAN ile hızlı transfer yapabilirsiniz.";

            // Yatırım
            if (lower.Contains("yatırım") || lower.Contains("hisse") || lower.Contains("borsa"))
                return "📈 Yatırım yapmak için Ana Menü > Yatırım Dashboard'a gidin. Hisse senedi ve kripto işlemlerinizi buradan yapabilirsiniz.";

            // Kart
            if (lower.Contains("kart") || lower.Contains("bankamatik"))
                return "💳 Kart işlemleriniz için müşteri hizmetlerimizi arayabilirsiniz: 0850 123 45 67";

            // Şifre/Güvenlik
            if (lower.Contains("şifre") || lower.Contains("güvenlik") || lower.Contains("unuttum"))
                return "🔐 Şifre sıfırlama için Login ekranında 'Şifremi Unuttum' seçeneğini kullanın.";

            // Default Response
            return "🤔 Üzgünüm, bu konuda size tam olarak yardımcı olamıyorum. Bir yetkiliye bağlanmak ister misiniz?";
        }

        private void BtnEscalate_Click(object sender, EventArgs e)
        {
            try
            {
                string subject = "NovaBank Destek Talebi";
                string body = $"Merhaba NovaBank Ekibi,%0D%0A%0D%0ADestek talebim var.%0D%0A%0D%0ASaygılarımla";
                string mailto = $"mailto:novabank.com@gmail.com?subject={Uri.EscapeDataString(subject)}&body={body}";
                
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(mailto) { UseShellExecute = true });
                
                AddBotMessage("✅ Mail uygulamanız açıldı. Talebinizi detaylı bir şekilde yazabilirsiniz.");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Mail gönderme hatası: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddUserMessage(string message)
        {
            txtChatHistory.SelectionAlignment = HorizontalAlignment.Right;
            txtChatHistory.SelectionBackColor = Color.FromArgb(0, 210, 255);
            txtChatHistory.SelectionColor = Color.White;
            txtChatHistory.AppendText($"Sen: {message}\n\n");
            txtChatHistory.ScrollToCaret();
        }

        private void AddBotMessage(string message)
        {
            txtChatHistory.SelectionAlignment = HorizontalAlignment.Left;
            txtChatHistory.SelectionBackColor = Color.White;
            txtChatHistory.SelectionColor = Color.Black;
            txtChatHistory.AppendText($"🤖 NovaBank: {message}\n\n");
            txtChatHistory.ScrollToCaret();
        }
    }
}
