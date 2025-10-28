using System;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;

namespace BankApp.UI.Forms
{
    public class ChartPopupForm : XtraForm
    {
        private ChartControl _chartControl;

        public ChartPopupForm(ChartControl sourceChart)
        {
            InitializeComponent();
            CloneChart(sourceChart);
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Detaylı Grafik Görünümü";
            
            // Dark Theme application
            this.LookAndFeel.UseDefaultLookAndFeel = false;
            this.LookAndFeel.SkinName = "Office 2019 Black";
        }

        private void CloneChart(ChartControl source)
        {
            _chartControl = new ChartControl();
            _chartControl.Dock = DockStyle.Fill;
            _chartControl.PaletteName = source.PaletteName;
            
            // Basic visual cloning
            _chartControl.AppearanceNameSerializable = source.AppearanceNameSerializable;
            _chartControl.BorderOptions.Visibility = DevExpress.Utils.DefaultBoolean.False;
            
            // Clone Series
            foreach (Series sourceSeries in source.Series)
            {
                Series newSeries = new Series(sourceSeries.Name, sourceSeries.View.GetType());
                
                // Copy points
                foreach (SeriesPoint p in sourceSeries.Points)
                {
                    newSeries.Points.Add(p); // SeriesPoint objects can be tricky to clone if they have complex data, but simple data works
                }
                
                // Copy critical view properties if possible
                // Note: Full deep cloning of View properties is complex, setting baselines
                newSeries.ArgumentScaleType = sourceSeries.ArgumentScaleType;
                newSeries.ValueScaleType = sourceSeries.ValueScaleType;

                _chartControl.Series.Add(newSeries);
                
                // Apply specific view settings manually if needed
                if(sourceSeries.View is DoughnutSeriesView dView && newSeries.View is DoughnutSeriesView newDView)
                {
                    newDView.HoleRadiusPercent = dView.HoleRadiusPercent;
                    newDView.ExplodedMode = dView.ExplodedMode;
                }
                if(sourceSeries.View is PieSeriesView pView && newSeries.View is PieSeriesView newPView)
                {
                    newPView.ExplodedMode = pView.ExplodedMode;
                }
            }

            // Diagram settings
            if (source.Diagram is XYDiagram sourceDiag && _chartControl.Diagram is XYDiagram newDiag)
            {
                newDiag.EnableAxisXScrolling = true;
                newDiag.EnableAxisXZooming = true;
                newDiag.EnableAxisYScrolling = true;
                newDiag.EnableAxisYZooming = true;
                newDiag.Rotated = sourceDiag.Rotated;
                
                newDiag.AxisX.DateTimeScaleOptions.MeasureUnit = sourceDiag.AxisX.DateTimeScaleOptions.MeasureUnit;
                newDiag.AxisX.Label.TextPattern = sourceDiag.AxisX.Label.TextPattern;
            }
            
            // Titles
            foreach(ChartTitle t in source.Titles)
            {
                _chartControl.Titles.Add(new ChartTitle { Text = t.Text, Font = t.Font, TextColor = t.TextColor });
            }

            // Legend
            _chartControl.Legend.Visibility = DevExpress.Utils.DefaultBoolean.True;
            _chartControl.Legend.AlignmentHorizontal = LegendAlignmentHorizontal.Center;
            _chartControl.Legend.AlignmentVertical = LegendAlignmentVertical.BottomOutside;

            this.Controls.Add(_chartControl);
        }
    }
}
