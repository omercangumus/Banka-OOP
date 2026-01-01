#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// BES (Bireysel Emeklilik Sistemi) Formu
    /// Gelecek tahmini grafiği ve aylık ödeme hesaplayıcı
    /// </summary>
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
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Title
            var lblTitle = new LabelControl();
            lblTitle.Text = "🏛️ Bireysel Emeklilik Sistemi";
            lblTitle.Location = new Point(30, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.White;

            var lblSubtitle = new LabelControl();
            lblSubtitle.Text = "Geleceğinizi bugünden planlayın";
            lblSubtitle.Location = new Point(30, 60);
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 10F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(150, 150, 160);

            // Monthly Payment
            var lblPaymentTitle = new LabelControl();
            lblPaymentTitle.Text = "💰 Aylık Katkı Payı";
            lblPaymentTitle.Location = new Point(30, 110);
            lblPaymentTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblPaymentTitle.Appearance.ForeColor = Color.White;

            this.txtMonthlyPayment = new CalcEdit();
            this.txtMonthlyPayment.Location = new Point(30, 140);
            this.txtMonthlyPayment.Size = new Size(200, 50);
            this.txtMonthlyPayment.Value = 1000;
            this.txtMonthlyPayment.Properties.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.txtMonthlyPayment.Properties.Appearance.ForeColor = Color.FromArgb(156, 39, 176);
            this.txtMonthlyPayment.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtMonthlyPayment.Properties.DisplayFormat.FormatString = "₺ #,##0";
            this.txtMonthlyPayment.Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.txtMonthlyPayment.EditValueChanged += (s, e) => UpdateCalculation();

            // Years
            var lblYearsTitle = new LabelControl();
            lblYearsTitle.Text = "📅 Süre (Yıl)";
            lblYearsTitle.Location = new Point(250, 110);
            lblYearsTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblYearsTitle.Appearance.ForeColor = Color.White;

            this.spinYears = new SpinEdit();
            this.spinYears.Location = new Point(250, 140);
            this.spinYears.Size = new Size(120, 50);
            this.spinYears.Value = 10;
            this.spinYears.Properties.MinValue = 3;
            this.spinYears.Properties.MaxValue = 35;
            this.spinYears.Properties.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.spinYears.Properties.Appearance.ForeColor = Color.White;
            this.spinYears.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.spinYears.EditValueChanged += (s, e) => UpdateCalculation();

            // State Contribution Rate (devlet katkısı %30)
            var lblRateTitle = new LabelControl();
            lblRateTitle.Text = "🏛️ Getiri Oranı (Tahmini)";
            lblRateTitle.Location = new Point(30, 210);
            lblRateTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblRateTitle.Appearance.ForeColor = Color.White;

            this.trackContributionRate = new TrackBarControl();
            this.trackContributionRate.Location = new Point(30, 240);
            this.trackContributionRate.Size = new Size(340, 40);
            this.trackContributionRate.Properties.Minimum = 5;
            this.trackContributionRate.Properties.Maximum = 25;
            this.trackContributionRate.Value = 12;
            this.trackContributionRate.EditValueChanged += (s, e) => UpdateCalculation();

            this.lblContributionRate = new LabelControl();
            this.lblContributionRate.Location = new Point(30, 285);
            this.lblContributionRate.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblContributionRate.Appearance.ForeColor = Color.FromArgb(33, 150, 243);

            // Results Panel
            var pnlResults = new Panel();
            pnlResults.Location = new Point(30, 330);
            pnlResults.Size = new Size(340, 170);
            pnlResults.BackColor = Color.FromArgb(30, 35, 45);
            pnlResults.Paint += (s, e) => {
                using (var brush = new LinearGradientBrush(pnlResults.ClientRectangle, 
                    Color.FromArgb(40, 156, 39, 176), Color.FromArgb(20, 33, 150, 243), 45F))
                {
                    e.Graphics.FillRectangle(brush, pnlResults.ClientRectangle);
                }
            };

            var lblResultTitle = new LabelControl();
            lblResultTitle.Text = "📊 Tahmini Sonuçlar";
            lblResultTitle.Location = new Point(15, 10);
            lblResultTitle.Appearance.Font = new Font("Segoe UI Semibold", 11F);
            lblResultTitle.Appearance.ForeColor = Color.White;
            pnlResults.Controls.Add(lblResultTitle);

            this.lblTotalContribution = new LabelControl();
            this.lblTotalContribution.Location = new Point(15, 45);
            this.lblTotalContribution.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblTotalContribution.Appearance.ForeColor = Color.FromArgb(180, 180, 190);
            pnlResults.Controls.Add(lblTotalContribution);

            this.lblStateContribution = new LabelControl();
            this.lblStateContribution.Location = new Point(15, 75);
            this.lblStateContribution.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblStateContribution.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            pnlResults.Controls.Add(lblStateContribution);

            this.lblEstimatedTotal = new LabelControl();
            this.lblEstimatedTotal.Location = new Point(15, 115);
            this.lblEstimatedTotal.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblEstimatedTotal.Appearance.ForeColor = Color.FromArgb(255, 193, 7);
            pnlResults.Controls.Add(lblEstimatedTotal);

            // Chart
            this.chartProjection = new ChartControl();
            this.chartProjection.Location = new Point(400, 100);
            this.chartProjection.Size = new Size(380, 400);
            this.chartProjection.BackColor = Color.Transparent;
            
            var diagram = new XYDiagram();
            diagram.AxisX.Title.Text = "Yıl";
            diagram.AxisX.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diagram.AxisX.Color = Color.FromArgb(80, 80, 90);
            diagram.AxisY.Title.Text = "Birikim (₺)";
            diagram.AxisY.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diagram.AxisY.Color = Color.FromArgb(80, 80, 90);
            this.chartProjection.Diagram = diagram;
            
            var series = new Series("Birikim", ViewType.Area);
            series.View.Color = Color.FromArgb(100, 156, 39, 176);
            this.chartProjection.Series.Add(series);
            
            this.chartProjection.Legend.Visibility = DevExpress.Utils.DefaultBoolean.False;

            // Start Button
            this.btnStart = new SimpleButton();
            this.btnStart.Text = "🚀 BES'e Başla";
            this.btnStart.Location = new Point(30, 520);
            this.btnStart.Size = new Size(340, 55);
            this.btnStart.Appearance.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.btnStart.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            this.btnStart.Appearance.ForeColor = Color.White;
            this.btnStart.Appearance.Options.UseBackColor = true;
            this.btnStart.Appearance.Options.UseForeColor = true;
            this.btnStart.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnStart.Click += BtnStart_Click;

            // Form
            this.Controls.AddRange(new Control[] { 
                lblTitle, lblSubtitle, 
                lblPaymentTitle, txtMonthlyPayment,
                lblYearsTitle, spinYears,
                lblRateTitle, trackContributionRate, lblContributionRate,
                pnlResults, chartProjection, btnStart
            });
            
            this.ClientSize = new Size(810, 600);
            this.Text = "BES - Bireysel Emeklilik";
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
        }

        private void UpdateCalculation()
        {
            decimal monthly = txtMonthlyPayment.Value;
            int years = (int)spinYears.Value;
            decimal rate = trackContributionRate.Value;

            int totalMonths = years * 12;
            decimal totalContribution = monthly * totalMonths;
            
            // Devlet katkısı %30 (max yıllık asgari ücret)
            decimal stateContribution = totalContribution * 0.30m;
            
            // Bileşik getiri hesabı (basitleştirilmiş)
            decimal totalWithReturn = 0;
            decimal balance = 0;
            
            // Grafik için veri
            var chartData = new List<(int Year, decimal Value)>();
            
            for (int year = 1; year <= years; year++)
            {
                // Yıllık katkı
                balance += monthly * 12;
                // Yıllık getiri
                balance += balance * (rate / 100);
                // Devlet katkısı (sadece %30 ana para üzerinden, yılsonunda)
                decimal yearlyStateContrib = (monthly * 12) * 0.30m;
                balance += yearlyStateContrib;
                
                chartData.Add((year, balance));
            }
            
            totalWithReturn = balance;

            // Labels güncelle
            lblContributionRate.Text = $"%{rate:N0} Yıllık Getiri";
            lblTotalContribution.Text = $"Toplam Katkı Payınız: {totalContribution:N0} ₺";
            lblStateContribution.Text = $"Tahmini Devlet Katkısı: + {stateContribution:N0} ₺ (Max %30)";
            lblEstimatedTotal.Text = $"Toplam: {totalWithReturn:N0} ₺";

            // Grafik güncelle
            var series = chartProjection.Series[0];
            series.Points.Clear();
            foreach (var (year, value) in chartData)
            {
                series.Points.Add(new SeriesPoint(year, (double)value));
            }
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            var confirm = XtraMessageBox.Show(
                $"Aylık {txtMonthlyPayment.Value:N0} ₺ katkı payı ile BES'e başlamak istiyor musunuz?\n\n" +
                $"Not: Bu bir simülasyondur.",
                "BES Başvurusu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm == DialogResult.Yes)
            {
                XtraMessageBox.Show(
                    "BES başvurunuz alındı! 🎉\n\nAylık ödemeleriniz otomatik olarak hesabınızdan çekilecektir.",
                    "Başarılı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                
                AppEvents.NotifyDataChanged("BES", "Started");
                this.Close();
            }
        }
    }
}
