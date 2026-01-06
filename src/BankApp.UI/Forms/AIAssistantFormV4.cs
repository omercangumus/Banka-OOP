#nullable enable
using DevExpress.XtraEditors;
using System;
using System.Drawing;
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
    /// AI Assistant - Simple Clean Chat UI
    /// </summary>
    public class AIAssistantFormV4 : XtraForm
    {
        private readonly IAIProvider _aiProvider;
        private readonly AiContextBuilder _contextBuilder;
        private readonly DashboardSummaryService _dashboardService;
        private readonly string _stockContext;
        private bool _isSending;
        
        // UI
        private RichTextBox rtbChat;
        private TextBox txtInput;
        private SimpleButton btnSend;

        public AIAssistantFormV4(string? stockContext = null)
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] OPENED: {GetType().FullName}, StockContext={stockContext}");
            
            _stockContext = stockContext ?? "";
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _dashboardService = new DashboardSummaryService(new DapperContext());
            
            InitUI();
        }

        private void InitUI()
        {
            // Form - Larger size for better UX
            this.Text = "NovaBank AI Asistan";
            this.Size = new Size(900, 650);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 20);
            this.ForeColor = Color.White;
            this.LookAndFeel.SkinName = "Office 2019 Black";
            
            // Header panel
            var pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 50;
            pnlHeader.BackColor = Color.FromArgb(30, 30, 30);
            pnlHeader.Padding = new Padding(15, 0, 15, 0);
            
            var lblTitle = new LabelControl();
            lblTitle.Text = "🤖 NovaBank AI Asistan";
            lblTitle.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;
            lblTitle.Location = new Point(15, 13);
            pnlHeader.Controls.Add(lblTitle);
            
            // Chat display - DevExpress MemoEdit for better rendering
            rtbChat = new RichTextBox();
            rtbChat.Dock = DockStyle.Fill;
            rtbChat.BackColor = Color.FromArgb(25, 25, 25);
            rtbChat.ForeColor = Color.White;
            rtbChat.Font = new Font("Segoe UI", 11F);
            rtbChat.ReadOnly = true;
            rtbChat.BorderStyle = BorderStyle.None;
            rtbChat.Margin = new Padding(15);
            
            // Chat container with padding
            var pnlChat = new Panel();
            pnlChat.Dock = DockStyle.Fill;
            pnlChat.BackColor = Color.FromArgb(25, 25, 25);
            pnlChat.Padding = new Padding(15, 10, 15, 10);
            pnlChat.Controls.Add(rtbChat);
            
            // Input panel - Improved styling
            var pnlInput = new Panel();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 70;
            pnlInput.BackColor = Color.FromArgb(30, 30, 30);
            pnlInput.Padding = new Padding(15, 12, 15, 12);
            
            txtInput = new TextBox();
            txtInput.Dock = DockStyle.Fill;
            txtInput.BackColor = Color.FromArgb(40, 40, 40);
            txtInput.ForeColor = Color.White;
            txtInput.Font = new Font("Segoe UI", 12F);
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            txtInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    _ = SendAsync();
                }
            };
            
            btnSend = new SimpleButton();
            btnSend.Text = "Gönder ➤";
            btnSend.Dock = DockStyle.Right;
            btnSend.Width = 100;
            btnSend.Appearance.BackColor = Color.FromArgb(59, 130, 246);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.Appearance.Options.UseFont = true;
            btnSend.Click += async (s, e) => await SendAsync();
            
            pnlInput.Controls.Add(txtInput);
            pnlInput.Controls.Add(btnSend);
            
            // Quick actions panel
            var pnlQuickActions = new Panel();
            pnlQuickActions.Dock = DockStyle.Bottom;
            pnlQuickActions.Height = 45;
            pnlQuickActions.BackColor = Color.FromArgb(28, 28, 28);
            pnlQuickActions.Padding = new Padding(15, 8, 15, 8);
            
            var btnPdf = new SimpleButton();
            btnPdf.Text = "📄 PDF İndir";
            btnPdf.Size = new Size(110, 28);
            btnPdf.Location = new Point(15, 8);
            btnPdf.Appearance.BackColor = Color.FromArgb(45, 45, 45);
            btnPdf.Appearance.ForeColor = Color.White;
            btnPdf.Appearance.Options.UseBackColor = true;
            btnPdf.Click += async (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] HANDLER: btnPdf clicked, this={GetType().FullName}");
                await ExportPdfAsync();
            };
            pnlQuickActions.Controls.Add(btnPdf);
            
            var btnPortfolio = new SimpleButton();
            btnPortfolio.Text = "📊 Portföy Özeti";
            btnPortfolio.Size = new Size(120, 28);
            btnPortfolio.Location = new Point(135, 8);
            btnPortfolio.Appearance.BackColor = Color.FromArgb(45, 45, 45);
            btnPortfolio.Appearance.ForeColor = Color.White;
            btnPortfolio.Appearance.Options.UseBackColor = true;
            btnPortfolio.Click += async (s, e) => {
                txtInput.Text = "Portföyümü özetle";
                await SendAsync();
            };
            pnlQuickActions.Controls.Add(btnPortfolio);
            
            // Add controls in order
            this.Controls.Add(pnlChat);
            this.Controls.Add(pnlQuickActions);
            this.Controls.Add(pnlInput);
            this.Controls.Add(pnlHeader);
            
            // Welcome
            AppendMessage("AI", "Merhaba! Size nasıl yardımcı olabilirim?\n\n" +
                "📄 PDF İndir - Portföy raporunuzu PDF olarak indirin\n" +
                "📊 Portföy Özeti - Yatırımlarınızın özetini görün\n" +
                "💬 Sorularınızı yazın - Finansal konularda yardım alın");
        }

        private void AppendMessage(string sender, string text)
        {
            if (rtbChat.InvokeRequired)
            {
                rtbChat.Invoke(new Action(() => AppendMessage(sender, text)));
                return;
            }
            
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.SelectionColor = sender == "Siz" ? Color.FromArgb(100, 180, 255) : Color.FromArgb(180, 180, 180);
            rtbChat.AppendText($"\n{sender}:\n");
            rtbChat.SelectionColor = Color.White;
            rtbChat.AppendText($"{text}\n");
            rtbChat.ScrollToCaret();
        }

        private async Task SendAsync()
        {
            if (_isSending) return;
            var msg = txtInput.Text?.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            
            txtInput.Text = "";
            AppendMessage("Siz", msg);
            
            _isSending = true;
            btnSend.Enabled = false;
            
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
                AppendMessage("AI", $"Hata: {ex.Message}");
            }
            finally
            {
                _isSending = false;
                btnSend.Enabled = true;
            }
        }

        private bool IsPdfIntent(string msg)
        {
            var l = msg.ToLower();
            return l.Contains("pdf") || (l.Contains("indir") && (l.Contains("portföy") || l.Contains("rapor")));
        }

        private async Task ExportPdfAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[RUNTIME-TRACE] ExportPdfAsync called, this={GetType().FullName}");
            AppendMessage("AI", "PDF hazırlanıyor...");
            
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
                dlg.FileName = $"NovaBank_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
                
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PdfReportExporter.GenerateInvestmentReport(data, dlg.FileName);
                    AppendMessage("AI", $"✅ PDF kaydedildi: {Path.GetFileName(dlg.FileName)}");
                }
                else
                {
                    AppendMessage("AI", "İptal edildi.");
                }
            }
            catch (Exception ex)
            {
                AppendMessage("AI", $"PDF hatası: {ex.Message}");
            }
        }
    }
}
