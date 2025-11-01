using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;

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

        public BESCalculatorControl()
        {
            InitializeComponent();
            UpdateCalculation();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(30, 41, 59); // Matches Dashboard
            this.Size = new Size(400, 300);

            var lblTitle = new LabelControl { Text = "💰 Emeklilik Planı Hesapla", Location = new Point(20, 10) };
            lblTitle.Appearance.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblTitle.Appearance.ForeColor = Color.Gold;
            
            // Monthly Slider
            var lblM = new LabelControl { Text = "Aylık Ödeme:", Location = new Point(20, 50), ForeColor = Color.White };
            lblMonthlyValue = new LabelControl { Text = "2500 TL", Location = new Point(300, 50), ForeColor = Color.LightGreen, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            
            trackMonthly = new TrackBarControl();
            trackMonthly.Location = new Point(20, 70);
            trackMonthly.Size = new Size(360, 45);
            trackMonthly.Properties.Minimum = 1000;
            trackMonthly.Properties.Maximum = 50000;
            trackMonthly.Properties.SmallChange = 500;
            trackMonthly.Properties.LargeChange = 2500;
            trackMonthly.Value = 2500;
            trackMonthly.EditValueChanged += (s, e) => { lblMonthlyValue.Text = $"{trackMonthly.Value:N0} TL"; UpdateCalculation(); };

            // Years Slider
            var lblY = new LabelControl { Text = "Süre (Yıl):", Location = new Point(20, 130), ForeColor = Color.White };
            lblYearsValue = new LabelControl { Text = "10 Yıl", Location = new Point(300, 130), ForeColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };

            trackYears = new TrackBarControl();
            trackYears.Location = new Point(20, 150);
            trackYears.Size = new Size(360, 45);
            trackYears.Properties.Minimum = 1;
            trackYears.Properties.Maximum = 40;
            trackYears.Value = 10;
            trackYears.EditValueChanged += (s, e) => { lblYearsValue.Text = $"{trackYears.Value} Yıl"; UpdateCalculation(); };

            // Results
            var lblEst = new LabelControl { Text = "Tahmini Birikim:", Location = new Point(20, 210), ForeColor = Color.Gray };
            lblTotalResult = new LabelControl { Text = "₺0", Location = new Point(20, 230) };
            lblTotalResult.Appearance.Font = new Font("Segoe UI", 24F, FontStyle.Bold);
            lblTotalResult.Appearance.ForeColor = Color.White;

            lblStateMatch = new LabelControl { Text = "+ Devlet Katkısı: ₺0", Location = new Point(20, 275), ForeColor = Color.FromArgb(34, 197, 94) };

            this.Controls.AddRange(new Control[] { lblTitle, lblM, lblMonthlyValue, trackMonthly, lblY, lblYearsValue, trackYears, lblEst, lblTotalResult, lblStateMatch });
        }

        private void UpdateCalculation()
        {
            decimal monthly = trackMonthly.Value;
            int years = trackYears.Value;
            
            // Logic: 
            // 1. Total Principal = Monthly * 12 * Years
            // 2. State Match = 30% (Capped at 24000/year for 2024? Lets assume 1/3 of monthly for simplicity logic or 30%)
            // 3. Growth = Assume 15% annual avg return
            
            decimal totalBalance = 0;
            decimal totalState = 0;
            
            for(int i=0; i<years; i++) {
                decimal yearlyContrib = monthly * 12;
                decimal stateContribution = Math.Min(yearlyContrib * 0.30m, 72000m); // 2025 Cap logic (approx)
                
                totalBalance += yearlyContrib + stateContribution;
                totalState += stateContribution;
                
                totalBalance *= 1.15m; // 15% growth
            }

            lblTotalResult.Text = $"₺{totalBalance:N0}";
            lblStateMatch.Text = $"+ Devlet Katkısı: ₺{totalState:N0} (Dahil)";
        }
    }
}
