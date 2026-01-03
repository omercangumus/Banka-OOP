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
                Series newSeries = (Series)sourceSeries.Clone();
                _chartControl.Series.Add(newSeries);
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
