using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.Utils;

namespace BankApp.UI.Forms
{
    public class BESForm : XtraForm
    {
        private CalcEdit txtMonthlyPayment;
        private SpinEdit spinYears;
        private TrackBarControl trackContributionRate;
        private LabelControl lblContributionRate;
        private LabelControl lblTotalContribution;
        private LabelControl lblStateContribution;
        private LabelControl lblEstimatedTotal;
        private ChartControl chartProjection;
        private SimpleButton btnStart;

        public BESForm()
        {
            InitializeComponent();
            UpdateCalculation();
        }

        private void InitializeComponent()
        {
            // Form Settings
            this.ClientSize = new Size(1000, 650);
            this.Text = "Bireysel Emeklilik Sistemi (BES) Başvurusu";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42); // Theme Background
            this.ForeColor = Color.White;

            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "BES Simülasyonu ve Başvuru";
            lblTitle.Location = new Point(30, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.FromArgb(59, 130, 246); // Blue
            this.Controls.Add(lblTitle);

            // Controls Group (Left)
            var pnlControls = new PanelControl();
            pnlControls.Location = new Point(30, 80);
            pnlControls.Size = new Size(400, 450);
            pnlControls.Appearance.BackColor = Color.FromArgb(30, 41, 59); // Panel background
            pnlControls.Appearance.Options.UseBackColor = true;
            pnlControls.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.Controls.Add(pnlControls);

            // Input: Monthly Payment
            var lblMonthly = new LabelControl { Text = "Aylık Katkı Payı (TL):", Location = new Point(20, 20), Parent = pnlControls };
            lblMonthly.Appearance.Font = new Font("Segoe UI", 10F);
            lblMonthly.Appearance.ForeColor = Color.White;

            txtMonthlyPayment = new CalcEdit();
            txtMonthlyPayment.Location = new Point(20, 50);
            txtMonthlyPayment.Size = new Size(360, 40);
            txtMonthlyPayment.Properties.Appearance.Font = new Font("Segoe UI", 12F);
            txtMonthlyPayment.Value = 2500;
            txtMonthlyPayment.EditValueChanged += (s, e) => UpdateCalculation();
            pnlControls.Controls.Add(txtMonthlyPayment);

            // Input: Investment Duration (Years)
            var lblYears = new LabelControl { Text = "Yatırım Süresi (Yıl):", Location = new Point(20, 110), Parent = pnlControls };
            lblYears.Appearance.Font = new Font("Segoe UI", 10F);
            lblYears.Appearance.ForeColor = Color.White;

            spinYears = new SpinEdit();
            spinYears.Location = new Point(20, 140);
            spinYears.Size = new Size(360, 40);
            spinYears.Properties.Appearance.Font = new Font("Segoe UI", 12F);
            spinYears.Properties.MinValue = 5;
            spinYears.Properties.MaxValue = 40;
            spinYears.Value = 10;
            spinYears.EditValueChanged += (s, e) => UpdateCalculation();
            pnlControls.Controls.Add(spinYears);

            // Input: Expected Return Rate
            lblContributionRate = new LabelControl { Text = "Tahmini Yıllık Fon Getirisi: %15", Location = new Point(20, 200), Parent = pnlControls };
            lblContributionRate.Appearance.Font = new Font("Segoe UI", 10F);
            lblContributionRate.Appearance.ForeColor = Color.White;

            trackContributionRate = new TrackBarControl();
            trackContributionRate.Location = new Point(20, 230);
            trackContributionRate.Size = new Size(360, 45);
            trackContributionRate.Properties.Minimum = 5;
            trackContributionRate.Properties.Maximum = 60;
            trackContributionRate.Value = 15;
            trackContributionRate.LookAndFeel.UseDefaultLookAndFeel = false;
            trackContributionRate.LookAndFeel.SkinName = "Office 2019 Black";
            trackContributionRate.EditValueChanged += (s, e) => {
                lblContributionRate.Text = $"Tahmini Yıllık Fon Getirisi: %{trackContributionRate.Value}";
                UpdateCalculation();
            };
            pnlControls.Controls.Add(trackContributionRate);

            // Results Labels
            lblTotalContribution = new LabelControl { Text = "Toplam Ödemeniz: 0 TL", Location = new Point(20, 300), Parent = pnlControls };
            lblTotalContribution.Appearance.Font = new Font("Segoe UI", 10F);
            lblTotalContribution.Appearance.ForeColor = Color.LightGray;

            lblStateContribution = new LabelControl { Text = "+ Devlet Katkısı (%30): 0 TL", Location = new Point(20, 330), Parent = pnlControls };
            lblStateContribution.Appearance.Font = new Font("Segoe UI", 10F);
            lblStateContribution.Appearance.ForeColor = Color.FromArgb(74, 222, 128); // Green

            lblEstimatedTotal = new LabelControl { Text = "TAHMİNİ BİRİKİM: 0 TL", Location = new Point(20, 380), Parent = pnlControls };
            lblEstimatedTotal.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblEstimatedTotal.Appearance.ForeColor = Color.Gold;

            // Apply Button
            btnStart = new SimpleButton();
            btnStart.Text = "BAŞVURUYU TAMAMLA";
            btnStart.Location = new Point(30, 550);
            btnStart.Size = new Size(400, 60);
            btnStart.Appearance.BackColor = Color.FromArgb(37, 99, 235); // Blue
            btnStart.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnStart.Appearance.ForeColor = Color.White;
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Chart (Right)
            chartProjection = new ChartControl();
            chartProjection.Location = new Point(460, 80);
            chartProjection.Size = new Size(510, 530);
            chartProjection.AppearanceNameSerializable = "Dark Chameleon";
            chartProjection.BackColor = Color.Transparent;
            chartProjection.BorderOptions.Visibility = DefaultBoolean.False;
            
            if (chartProjection.Diagram is XYDiagram diag)
            {
                diag.AxisX.Label.TextColor = Color.LightGray;
                diag.AxisY.Label.TextColor = Color.LightGray;
                diag.AxisX.GridLines.Visible = false;
                diag.AxisY.GridLines.Color = Color.FromArgb(50, 50, 50);
                diag.DefaultPane.BackColor = Color.Transparent;
                diag.DefaultPane.BorderVisible = false;
            }
            
            this.Controls.Add(chartProjection);
        }

        private void UpdateCalculation()
        {
            try 
            {
                if (chartProjection == null) return;

                decimal monthly = txtMonthlyPayment.Value;
                int years = (int)spinYears.Value;
                double rate = trackContributionRate.Value;

                // Series Setup
                chartProjection.Series.Clear();
                Series seriesTotal = new Series("Toplam Birikim", ViewType.Area);
                Series seriesPrincipal = new Series("Ana Para", ViewType.Line);

                decimal currentBalance = 0;
                decimal totalPrincipal = 0;
                decimal totalState = 0;

                for (int i = 0; i <= years; i++)
                {
                    seriesTotal.Points.Add(new SeriesPoint("Yıl " + i, (double)currentBalance));
                    seriesPrincipal.Points.Add(new SeriesPoint("Yıl " + i, (double)totalPrincipal));

                    if (i < years)
                    {
                        decimal annualContribution = monthly * 12;
                        decimal annualState = Math.Min(annualContribution * 0.30m, 24000); // Caps at hypothetical limit
                        
                        totalPrincipal += annualContribution;
                        totalState += annualState;
                        
                        // Compound Interest
                        currentBalance += annualContribution + annualState;
                        currentBalance += currentBalance * (decimal)(rate / 100);
                    }
                }

                chartProjection.Series.Add(seriesTotal);
                chartProjection.Series.Add(seriesPrincipal);

                // Styling
                if(seriesTotal.View is AreaSeriesView areaView)
                {
                    areaView.Color = Color.FromArgb(100, 34, 197, 94); // Transparent Green
                    areaView.Border.Visibility = DefaultBoolean.False;
                }

                lblTotalContribution.Text = $"Toplam Ödemeniz: {totalPrincipal:N0} TL";
                lblStateContribution.Text = $"+ Devlet Katkısı (%30): {totalState:N0} TL";
                lblEstimatedTotal.Text = $"TAHMİNİ BİRİKİM: {currentBalance:N0} TL";
            } 
            catch { }
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            XtraMessageBox.Show("BES Başvurunuz başarıyla alınmıştır.\nSözleşmeniz e-posta adresinize gönderilecektir.", 
                "Başvuru Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
