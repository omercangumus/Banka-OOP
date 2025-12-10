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
            _stockContext = stockContext ?? "";
            _aiProvider = AiProviderFactory.CreateProvider();
            _contextBuilder = new AiContextBuilder();
            _dashboardService = new DashboardSummaryService(new DapperContext());
            
            InitUI();
        }

        private void InitUI()
        {
            // Form
            this.Text = "NovaBank AI Asistan";
            this.Size = new Size(550, 500);
            this.MinimumSize = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            
            // Chat display
            rtbChat = new RichTextBox();
            rtbChat.Dock = DockStyle.Fill;
            rtbChat.BackColor = Color.FromArgb(25, 25, 25);
            rtbChat.ForeColor = Color.White;
            rtbChat.Font = new Font("Segoe UI", 10F);
            rtbChat.ReadOnly = true;
            rtbChat.BorderStyle = BorderStyle.None;
            rtbChat.Padding = new Padding(10);
            
            // Input panel
            var pnlInput = new Panel();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 50;
            pnlInput.BackColor = Color.FromArgb(35, 35, 35);
            pnlInput.Padding = new Padding(10, 8, 10, 8);
            
            txtInput = new TextBox();
            txtInput.Dock = DockStyle.Fill;
            txtInput.BackColor = Color.FromArgb(45, 45, 45);
            txtInput.ForeColor = Color.White;
            txtInput.Font = new Font("Segoe UI", 11F);
            txtInput.BorderStyle = BorderStyle.FixedSingle;
            txtInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter && !e.Shift)
                {
                    e.SuppressKeyPress = true;
                    _ = SendAsync();
                }
            };
            
            btnSend = new SimpleButton();
            btnSend.Text = "Gönder";
            btnSend.Dock = DockStyle.Right;
            btnSend.Width = 80;
            btnSend.Appearance.BackColor = Color.FromArgb(59, 130, 246);
            btnSend.Appearance.ForeColor = Color.White;
            btnSend.Appearance.Options.UseBackColor = true;
            btnSend.Appearance.Options.UseForeColor = true;
            btnSend.Click += async (s, e) => await SendAsync();
            
            pnlInput.Controls.Add(txtInput);
            pnlInput.Controls.Add(btnSend);
            
            this.Controls.Add(rtbChat);
            this.Controls.Add(pnlInput);
            
            // Welcome
            AppendMessage("AI", "Merhaba! Size nasıl yardımcı olabilirim?\n\n" +
                "• \"Portföyümü PDF indir\" - PDF rapor\n" +
                "• Finansal sorularınızı sorun");
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
