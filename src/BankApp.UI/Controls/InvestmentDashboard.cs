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
        private CheckButton btnProMode;
        private LabelControl lblSummary;
        private ChartControl chartStocks, chartGold, chartEuro, chartOil;
        private GroupControl grpNews;
        private LabelControl lblNewsText;

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

            pnlMain = new PanelControl();
            pnlMain.Dock = DockStyle.Fill;
            pnlMain.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlMain.Appearance.BackColor = Color.Transparent;
            pnlMain.Appearance.Options.UseBackColor = true;
            this.Controls.Add(pnlMain);

            // Header
            lblTitle = new LabelControl();
            lblTitle.Text = "BORSA & EMTİA PİYASALARI";
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 16F);
            lblTitle.Appearance.ForeColor = Color.Gold;
            lblTitle.Location = new System.Drawing.Point(20, 10);
            pnlMain.Controls.Add(lblTitle);
            
            btnProMode = new CheckButton();
            btnProMode.Text = "PRO MODU";
            btnProMode.Location = new System.Drawing.Point(1050, 10);
            btnProMode.Size = new Size(150, 40);
            btnProMode.CheckedChanged += BtnProMode_CheckedChanged;
            pnlMain.Controls.Add(btnProMode);
            
            lblSummary = new LabelControl();
            lblSummary.Text = "USD: 32.50 | EUR: 35.80 | ALTIN: 2150$ | BIST: 9200";
            lblSummary.Appearance.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblSummary.Appearance.ForeColor = Color.LimeGreen;
            lblSummary.Location = new System.Drawing.Point(20, 50);
            pnlMain.Controls.Add(lblSummary);

            // Charts
            chartStocks = CreateChart("BIST 100", new System.Drawing.Point(20, 80));
            pnlMain.Controls.Add(chartStocks);

            chartGold = CreateChart("ALTIN (ONS)", new System.Drawing.Point(620, 80));
            pnlMain.Controls.Add(chartGold);

            chartEuro = CreateChart("EURO/TL", new System.Drawing.Point(20, 290));
            pnlMain.Controls.Add(chartEuro);

            chartOil = CreateChart("BRENT PETROL", new System.Drawing.Point(620, 290));
            pnlMain.Controls.Add(chartOil);

            // News
            grpNews = new GroupControl();
            grpNews.Text = "Piyasa Haberleri & Analizler";
            grpNews.Location = new System.Drawing.Point(20, 510);
            grpNews.Size = new Size(1180, 130); 
            grpNews.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            
            lblNewsText = new LabelControl();
            lblNewsText.Dock = DockStyle.Fill;
            lblNewsText.AutoSizeMode = LabelAutoSizeMode.None;
            lblNewsText.Padding = new Padding(10);
            lblNewsText.Appearance.Font = new Font("Segoe UI", 9F);
            lblNewsText.Appearance.ForeColor = Color.LightGray;
            lblNewsText.Text = "• FED faiz kararı bekleniyor.\n• BIST 100 rekor tazeledi.\n• Altın yükselişte.\n• Petrol fiyatları stabil.";
            
            grpNews.Controls.Add(lblNewsText);
            pnlMain.Controls.Add(grpNews);
        }

        private ChartControl CreateChart(string title, System.Drawing.Point loc)
        {
            ChartControl chart = new ChartControl();
            chart.Location = loc;
            chart.Size = new Size(580, 200);
            chart.BorderOptions.Visibility = DefaultBoolean.False;
            chart.Legend.Visibility = DefaultBoolean.False;
            
            // Fix: Do not assign chart.Diagram directly. 
            // Add a dummy series to force XYDiagram creation, then configure it.
            Series dummy = new Series("Dummy", ViewType.Line);
            chart.Series.Add(dummy);

            if (chart.Diagram is XYDiagram diag)
            {
                diag.AxisX.Label.Visible = false;
                diag.AxisY.Label.Font = new Font("Tahoma", 7);
                diag.DefaultPane.BackColor = Color.FromArgb(30, 30, 35);
                diag.DefaultPane.BorderVisible = false;
                
                diag.AxisX.GridLines.Visible = false;
                diag.AxisY.GridLines.Visible = false; 
            }
            
            chart.Series.Clear(); // Remove dummy
            
            chart.Titles.Add(new ChartTitle {
                Text = title,
                Font = new Font("Tahoma", 9, FontStyle.Bold),
                TextColor = Color.LightGray
            });
            
            return chart;
        }

        private void BtnProMode_CheckedChanged(object sender, EventArgs e)
        {
             bool pro = btnProMode.Checked;
             UpdateChartPro(chartStocks, pro);
             UpdateChartPro(chartGold, pro);
             UpdateChartPro(chartEuro, pro);
             UpdateChartPro(chartOil, pro);
        }
        
        private void UpdateChartPro(ChartControl chart, bool enabled)
        {
            if(chart.Diagram is XYDiagram diag)
            {
                diag.AxisY.GridLines.Visible = enabled;
                diag.AxisX.GridLines.Visible = enabled;
                diag.AxisY.GridLines.Color = Color.Gray;
                diag.AxisX.GridLines.Color = Color.Gray;
                
                chart.CrosshairEnabled = enabled ? DefaultBoolean.True : DefaultBoolean.False;
                chart.CrosshairOptions.ShowArgumentLine = enabled;
                chart.CrosshairOptions.ShowValueLine = enabled;
            }
        }

        public void LoadDummyData()
        {
            try {
                FillChart(chartStocks, "BIST", Color.FromArgb(33, 150, 243), 9000, 150);
                FillChart(chartGold, "GOLD", Color.Gold, 2150, 20);
                FillChart(chartEuro, "EUR", Color.LightBlue, 35.8, 0.5);
                FillChart(chartOil, "OIL", Color.OrangeRed, 85, 2);
            } catch {}
        }
        
        private void FillChart(ChartControl chart, string name, Color color, double start, double vol)
        {
            chart.Series.Clear();
            Series s = new Series(name, ViewType.Line);
            var r = new Random(name.GetHashCode()); 
            double p = start;
            for(int i=0; i<40; i++) {
                p += (r.NextDouble() * vol * 2) - vol;
                s.Points.Add(new SeriesPoint(i, p));
            }
            chart.Series.Add(s);
            s.View.Color = color;
            ((LineSeriesView)s.View).LineStyle.Thickness = 2;
            if(chart.Diagram is XYDiagram d) d.AxisY.WholeRange.AlwaysShowZeroLevel = false;
        }
    }
}
