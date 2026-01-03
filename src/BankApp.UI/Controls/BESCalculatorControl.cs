using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;

namespace BankApp.UI.Controls
{
    public class BESCalculatorControl : XtraUserControl
    {
        private TrackBarControl trackMonthly;
        private TrackBarControl trackYears;
        private LabelControl lblMonthlyValue;
        private LabelControl lblYearsValue;
        private LabelControl lblTotalResult;
        private LabelControl lblStateMatch;
        private ChartControl chartGrowth;

        public BESCalculatorControl()
        {
            InitializeComponent();
            UpdateCalculation();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(15, 23, 42); 
            this.Size = new Size(850, 450);

            var lblTitle = new LabelControl { Text = "💰 BES Birikim Projeksiyonu", Location = new Point(20, 10) };
            lblTitle.Appearance.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.Gold;
            
            // Left Panel for Inputs
            PanelControl pnlInputs = new PanelControl();
            pnlInputs.Location = new Point(20, 60);
            pnlInputs.Size = new Size(350, 370);
            pnlInputs.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlInputs.Appearance.BackColor = Color.FromArgb(30, 41, 59);

            // Monthly Slider
            var lblM = new LabelControl { Text = "Aylık Ödeme (TL):", Location = new Point(20, 20), ForeColor = Color.White };
            lblMonthlyValue = new LabelControl { Text = "2500 TL", Location = new Point(250, 20), ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            
            trackMonthly = new TrackBarControl();
            trackMonthly.Location = new Point(20, 45);
            trackMonthly.Size = new Size(310, 45);
            trackMonthly.Properties.Minimum = 1000;
            trackMonthly.Properties.Maximum = 50000;
            trackMonthly.Value = 2500;
            trackMonthly.EditValueChanged += (s, e) => { lblMonthlyValue.Text = $"{trackMonthly.Value:N0} TL"; UpdateCalculation(); };

            // Years Slider
            var lblY = new LabelControl { Text = "Vade (Yıl):", Location = new Point(20, 110), ForeColor = Color.White };
            lblYearsValue = new LabelControl { Text = "10 Yıl", Location = new Point(250, 110), ForeColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };

            trackYears = new TrackBarControl();
            trackYears.Location = new Point(20, 135);
            trackYears.Size = new Size(310, 45);
            trackYears.Properties.Minimum = 1;
            trackYears.Properties.Maximum = 40;
            trackYears.Value = 10;
            trackYears.EditValueChanged += (s, e) => { lblYearsValue.Text = $"{trackYears.Value} Yıl"; UpdateCalculation(); };

            // Results Label
            lblTotalResult = new LabelControl { Text = "₺0", Location = new Point(20, 220), Size = new Size(310, 60), AutoSizeMode = LabelAutoSizeMode.None };
            lblTotalResult.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            lblTotalResult.Appearance.ForeColor = Color.White;
            lblTotalResult.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            lblStateMatch = new LabelControl { Text = "+ Devlet Katkısı: ₺0", Location = new Point(20, 300), Size = new Size(310, 30), AutoSizeMode = LabelAutoSizeMode.None };
            lblStateMatch.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblStateMatch.Appearance.ForeColor = Color.FromArgb(34, 197, 94);
            lblStateMatch.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            pnlInputs.Controls.AddRange(new Control[] { lblM, lblMonthlyValue, trackMonthly, lblY, lblYearsValue, trackYears, lblTotalResult, lblStateMatch });

            // Right Panel for Chart
            chartGrowth = new ChartControl();
            chartGrowth.Location = new Point(390, 60);
            chartGrowth.Size = new Size(440, 370);
            chartGrowth.BackColor = Color.Transparent;
            chartGrowth.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;

            this.Controls.AddRange(new Control[] { lblTitle, pnlInputs, chartGrowth });
        }

        private void UpdateCalculation()
        {
            decimal monthly = trackMonthly.Value;
            int years = trackYears.Value;
            decimal growthRate = 1.15m; // 15% growth
            
            chartGrowth.Series.Clear();
            Series seriesPrincipal = new Series("Ana Para", ViewType.Area);
            Series seriesTotal = new Series("Toplam Birikim", ViewType.Area);
            
            decimal totalBalance = 0;
            decimal totalPrincipal = 0;
            decimal totalState = 0;
            
            for(int i=1; i<=years; i++) {
                decimal yearlyContrib = monthly * 12;
                decimal stateContribution = Math.Min(yearlyContrib * 0.30m, 72000m);
                
                totalPrincipal += yearlyContrib;
                totalBalance += yearlyContrib + stateContribution;
                totalState += stateContribution;
                
                totalBalance *= growthRate;

                seriesPrincipal.Points.Add(new SeriesPoint(i, totalPrincipal));
                seriesTotal.Points.Add(new SeriesPoint(i, totalBalance));
            }

            lblTotalResult.Text = $"₺{totalBalance:N0}";
            lblStateMatch.Text = $"+ Devlet Katkısı: ₺{totalState:N0} (Dahil)";

            chartGrowth.Series.AddRange(new Series[] { seriesTotal, seriesPrincipal });
            
            // Look & Feel
            if (chartGrowth.Diagram is XYDiagram diag) {
                diag.AxisX.Title.Text = "Yıl";
                diag.AxisX.Title.TextColor = Color.White;
                diag.AxisX.Label.TextColor = Color.White;
                diag.AxisY.Label.TextColor = Color.White;
                diag.AxisY.GridLines.Color = Color.FromArgb(60, 60, 60);
                diag.DefaultPane.BackColor = Color.FromArgb(30, 41, 59);
            }

            ((AreaSeriesView)seriesTotal.View).Transparency = 150;
            ((AreaSeriesView)seriesTotal.View).Color = Color.FromArgb(76, 175, 80);
            ((AreaSeriesView)seriesPrincipal.View).Transparency = 100;
            ((AreaSeriesView)seriesPrincipal.View).Color = Color.FromArgb(59, 130, 246);
            
            chartGrowth.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            chartGrowth.Legend.TextColor = Color.White;
            chartGrowth.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            chartGrowth.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;
        }
    }
}
