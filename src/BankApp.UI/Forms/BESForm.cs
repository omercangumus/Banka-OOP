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
            diag.AxisX.Visible = true;
            diag.AxisY.Visible = true;
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

                decimal balance = 0;
                chartProjection.Series.Clear();
                Series series = new Series("Birikim", ViewType.Area);
                
                for(int i=1; i<=years; i++)
                {
                    decimal yearlyPaid = monthly * 12;
                    balance += yearlyPaid;
                    balance += balance * (rate/100);
                    balance += yearlyPaid * 0.30m; 

                    series.Points.Add(new SeriesPoint(i, (double)balance));
                }
                
                chartProjection.Series.Add(series);
                lblEstimatedTotal.Text = $"{balance:N0} TL";
            } catch {}
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
             XtraMessageBox.Show("BES Hesabınız Açıldı!", "Basarili", MessageBoxButtons.OK, MessageBoxIcon.Information);
             this.Close();
        }
    }
}
