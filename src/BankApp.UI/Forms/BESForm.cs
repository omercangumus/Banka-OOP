#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;
using BankApp.Infrastructure.Services;

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
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            var lblTitle = new LabelControl();
            lblTitle.Text = "BES BASVURU MERKEZİ";
            lblTitle.Location = new Point(30, 20);
            lblTitle.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.Gold;

            this.txtMonthlyPayment = new CalcEdit();
            this.txtMonthlyPayment.Location = new Point(30, 160);
            this.txtMonthlyPayment.Size = new Size(250, 50);
            this.txtMonthlyPayment.Value = 2500;
            this.txtMonthlyPayment.EditValueChanged += (s, e) => UpdateCalculation();

            this.spinYears = new SpinEdit();
            this.spinYears.Location = new Point(300, 160);
            this.spinYears.Size = new Size(120, 50);
            this.spinYears.Value = 10;
            this.spinYears.EditValueChanged += (s, e) => UpdateCalculation();

            this.trackContributionRate = new TrackBarControl();
            this.trackContributionRate.Location = new Point(30, 270);
            this.trackContributionRate.Size = new Size(390, 45);
            this.trackContributionRate.Properties.Minimum = 5;
            this.trackContributionRate.Properties.Maximum = 50;
            this.trackContributionRate.Value = 15;
            this.trackContributionRate.EditValueChanged += (s, e) => UpdateCalculation();

            this.lblContributionRate = new LabelControl { Text = "Getiri: %15", Location = new Point(30, 240) };
            this.lblTotalContribution = new LabelControl { Text = "Odeme: 0", Location = new Point(30, 350) };
            this.lblStateContribution = new LabelControl { Text = "Devlet: 0", Location = new Point(30, 380) };
            this.lblEstimatedTotal = new LabelControl { Text = "0", Location = new Point(30, 420) };
            
            chartProjection = new ChartControl();
            chartProjection.Location = new Point(450, 100);
            chartProjection.Size = new Size(500, 440);
            
            XYDiagram diag = new XYDiagram();
            diag.AxisX.Visibility = DevExpress.Utils.DefaultBoolean.True;
            diag.AxisY.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartProjection.Diagram = diag;
            
            btnStart = new SimpleButton();
            btnStart.Text = "HESAP AÇ";
            btnStart.Location = new Point(30, 500);
            btnStart.Size = new Size(390, 60);
            btnStart.Click += BtnStart_Click;

            this.Controls.Add(lblTitle);
            this.Controls.Add(txtMonthlyPayment);
            this.Controls.Add(spinYears);
            this.Controls.Add(trackContributionRate);
            this.Controls.Add(lblContributionRate);
            this.Controls.Add(lblTotalContribution);
            this.Controls.Add(lblStateContribution);
            this.Controls.Add(lblEstimatedTotal);
            this.Controls.Add(chartProjection);
            this.Controls.Add(btnStart);
            
            this.ClientSize = new Size(1000, 650);
            this.Text = "BES Basvuru";
        }

        private void UpdateCalculation()
        {
            try {
                if(chartProjection == null) return;
                
                decimal monthly = txtMonthlyPayment.Value;
                int years = (int)spinYears.Value;
                decimal rate = trackContributionRate.Value;
                
                lblContributionRate.Text = $"Tahmini Yıllık Getiri: %{rate}";

                decimal balance = 0; // New application starts at 0
                chartProjection.Series.Clear();
                Series series = new Series("Birikim", ViewType.Area);
                
                // Add start point
                series.Points.Add(new SeriesPoint(0, 0));

                for(int i=1; i<=years; i++)
                {
                    decimal yearlyContribution = monthly * 12;
                    decimal stateMatch = Math.Min(yearlyContribution * 0.30m, 24000m); // 2025 limits: 30% state match
                    
                    // Add contributions
                    balance += yearlyContribution + stateMatch;
                    
                    // Apply compound interest
                    balance *= (1 + rate / 100);

                    series.Points.Add(new SeriesPoint(i, (double)balance));
                }
                
                if (series.View is AreaSeriesView areaView)
                {
                    areaView.Color = Color.FromArgb(251, 191, 36);
                    areaView.Transparency = 120;
                    areaView.Border.Color = Color.FromArgb(251, 191, 36);
                }
                
                chartProjection.Series.Add(series);
                
                // Style the diagram
                if(chartProjection.Diagram is XYDiagram diag)
                {
                    diag.DefaultPane.BackColor = Color.FromArgb(40, 40, 40);
                    diag.AxisX.Label.TextColor = Color.White;
                    diag.AxisY.Label.TextColor = Color.White;
                    diag.AxisX.GridLines.Color = Color.FromArgb(60, 60, 60);
                    diag.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
                }

                lblEstimatedTotal.Text = $"Tahmini: {balance:N0} TL";
                lblTotalContribution.Text = $"Ödemeniz: {monthly * 12 * years:N0} TL";
                lblStateContribution.Text = $"Devlet Katkısı: {(monthly * 12 * years * 0.30m):N0} TL (Max)";
            } catch {}
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
             XtraMessageBox.Show("BES Hesabınız Açıldı!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
             this.Close();
        }
    }
}
