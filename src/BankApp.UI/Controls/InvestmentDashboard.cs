using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.Utils;

namespace BankApp.UI.Controls
{
    public class InvestmentDashboard : XtraUserControl
    {
        private PanelControl pnlMain;
        private LabelControl lblTitle;
        private ChartControl chartStocks;
        private ChartControl chartGold;

        public InvestmentDashboard()
        {
            InitializeUI();
            LoadDummyData();
        }

        private void InitializeUI()
        {
            this.Dock = DockStyle.Fill;
            this.Appearance.BackColor = Color.FromArgb(20, 20, 20);
            this.Appearance.Options.UseBackColor = true;

            // 1. Container
            pnlMain = new PanelControl();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = Color.Transparent;
            pnlMain.Appearance.Options.UseBackColor = true;
            this.Controls.Add(pnlMain);

            // 2. Title
            lblTitle = new LabelControl();
            lblTitle.Text = "BORSA & EMTİA PİYASALARI";
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 20F); // Modern font
            lblTitle.Appearance.ForeColor = Color.Gold;
            lblTitle.Location = new Point(30, 20);
            pnlMain.Controls.Add(lblTitle);

            // 3. Stock Chart
            chartStocks = new ChartControl();
            chartStocks.Location = new Point(30, 80);
            chartStocks.Size = new Size(1000, 350);
            chartStocks.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            chartStocks.BorderOptions.Visibility = DefaultBoolean.False;
            pnlMain.Controls.Add(chartStocks);

            // 4. Gold Chart
            chartGold = new ChartControl();
            chartGold.Location = new Point(30, 450);
            chartGold.Size = new Size(1000, 350);
            chartGold.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            chartGold.BorderOptions.Visibility = DefaultBoolean.False;
            pnlMain.Controls.Add(chartGold);
        }

        public void LoadDummyData()
        {
            try
            {
                if (chartStocks == null || chartGold == null) return;

                chartStocks.Series.Clear();
                chartGold.Series.Clear();

                // --- BIST 100 ---
                Series seriesBist = new Series("BIST 100", ViewType.SplineArea);
                var rand = new Random();
                double price = 9000;
                for (int i = 0; i < 30; i++)
                {
                    price += rand.Next(-100, 150);
                    seriesBist.Points.Add(new SeriesPoint(DateTime.Now.AddDays(-30 + i).ToString("dd.MM"), price));
                }
                chartStocks.Series.Add(seriesBist);
                chartStocks.Titles.Clear();
                chartStocks.Titles.Add(new ChartTitle() { Text = "BIST 100 Endeksi" });
                
                if (chartStocks.Diagram is XYDiagram stockDiag)
                {
                    stockDiag.AxisX.Label.Angle = -45;
                }

                // --- Altın ---
                Series seriesGold = new Series("Ons Altın ($)", ViewType.Line);
                double goldPrice = 2050;
                for (int i = 0; i < 30; i++)
                {
                    goldPrice += rand.Next(-15, 20);
                    seriesGold.Points.Add(new SeriesPoint(DateTime.Now.AddDays(-30 + i).ToString("dd.MM"), goldPrice));
                }
                seriesGold.View.Color = Color.Gold;
                chartGold.Series.Add(seriesGold);
                chartGold.Titles.Clear();
                chartGold.Titles.Add(new ChartTitle() { Text = "Altın Borsası" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Invest Dashboard Load Error: " + ex.Message);
            }
        }
    }
}
