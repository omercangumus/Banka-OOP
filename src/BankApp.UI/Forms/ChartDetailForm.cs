using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;

namespace BankApp.UI.Forms
{
    public partial class ChartDetailForm : XtraForm
    {
        public ChartControl SourceChart { get; set; }

        public ChartDetailForm(ChartControl sourceChart)
        {
            SourceChart = sourceChart;
            InitializeComponent();
            CloneChart();
        }

        private void InitializeComponent()
        {
            this.Text = "Detaylı Grafik Analizi";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Normal;
            this.LookAndFeel.SetSkinStyle("Office 2019 Black");
        }

        private void CloneChart()
        {
            if (SourceChart == null) return;

            ChartControl clone = new ChartControl();
            clone.Dock = DockStyle.Fill;
            clone.AppearanceNameSerializable = SourceChart.AppearanceNameSerializable;
            clone.PaletteName = SourceChart.PaletteName;
            
            // Clone Series
            foreach (Series s in SourceChart.Series)
            {
                Series newSeries = (Series)s.Clone();
                
                // Specific View Settings for better visual
                if (newSeries.View is DoughnutSeriesView dv) {
                    dv.HoleRadiusPercent = 60;
                    newSeries.Label.TextPattern = "{A}: {VP:P1}";
                }
                
                clone.Series.Add(newSeries);
            }

            // Clone Titles
            foreach (ChartTitle t in SourceChart.Titles)
            {
                clone.Titles.Add(new ChartTitle { 
                    Text = t.Text, 
                    Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                    TextColor = Color.Gold 
                });
            }

            // Enable Animation for the Clone
            try {
                // simple animation trigger
                clone.Animate();
            } catch {}

            this.Controls.Add(clone);
            
            // Add Close Button (Floating)
            SimpleButton btnClose = new SimpleButton();
            btnClose.Text = "Kapat";
            btnClose.Size = new Size(100, 40);
            btnClose.Location = new Point(this.Width - 130, 20);
            btnClose.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnClose.Appearance.BackColor = Color.Crimson;
            btnClose.Appearance.ForeColor = Color.White;
            btnClose.Click += (s, e) => this.Close();
            clone.Controls.Add(btnClose);
        }
    }
}
