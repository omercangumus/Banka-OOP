using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using DevExpress.XtraEditors;
using DevExpress.XtraCharts;
using DevExpress.LookAndFeel;

namespace BankApp.UI.Forms
{
    public partial class BESForm : XtraForm
    {
        private PanelControl cardCurrentSavings;
        private PanelControl cardStateContribution;
        private ChartControl chartProjection;
        private LookUpEdit cmbFund;
        private SimpleButton btnChangeFund;
        private LabelControl lblTitle;
        private LabelControl lblCurrentSavingsTitle;
        private LabelControl lblCurrentSavingsValue;
        private LabelControl lblStateContributionTitle;
        private LabelControl lblStateContributionValue;
        private LabelControl lblFundLabel;
        private LabelControl lblCurrentFund;

        public BESForm()
        {
            InitializeComponent();
            LoadBESData();
        }

        private void InitializeComponent()
        {
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            this.cardCurrentSavings = new PanelControl();
            this.cardStateContribution = new PanelControl();
            this.chartProjection = new ChartControl();
            this.cmbFund = new LookUpEdit();
            this.btnChangeFund = new SimpleButton();
            this.lblTitle = new LabelControl();
            this.lblCurrentSavingsTitle = new LabelControl();
            this.lblCurrentSavingsValue = new LabelControl();
            this.lblStateContributionTitle = new LabelControl();
            this.lblStateContributionValue = new LabelControl();
            this.lblFundLabel = new LabelControl();
            this.lblCurrentFund = new LabelControl();

            ((System.ComponentModel.ISupportInitialize)(this.cardCurrentSavings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cardStateContribution)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProjection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbFund.Properties)).BeginInit();
            this.cardCurrentSavings.SuspendLayout();
            this.cardStateContribution.SuspendLayout();
            this.SuspendLayout();

            // Title
            this.lblTitle.Appearance.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            this.lblTitle.Appearance.ForeColor = Color.White;
            this.lblTitle.Location = new Point(20, 15);
            this.lblTitle.Text = "🏦 Bireysel Emeklilik Sistemi (BES)";
            this.lblTitle.Name = "lblTitle";

            // ============================================
            // CARD 1: Mevcut Birikim (Yeşil)
            // ============================================
            this.cardCurrentSavings.Location = new Point(20, 60);
            this.cardCurrentSavings.Size = new Size(380, 120);
            this.cardCurrentSavings.Appearance.BackColor = Color.FromArgb(76, 175, 80);
            this.cardCurrentSavings.Appearance.Options.UseBackColor = true;
            this.cardCurrentSavings.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardCurrentSavings.Name = "cardCurrentSavings";

            this.lblCurrentSavingsTitle.Appearance.ForeColor = Color.White;
            this.lblCurrentSavingsTitle.Appearance.Font = new Font("Segoe UI", 12F);
            this.lblCurrentSavingsTitle.Location = new Point(20, 20);
            this.lblCurrentSavingsTitle.Text = "💰 Mevcut Birikim";
            this.lblCurrentSavingsTitle.Name = "lblCurrentSavingsTitle";

            this.lblCurrentSavingsValue.Appearance.ForeColor = Color.White;
            this.lblCurrentSavingsValue.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            this.lblCurrentSavingsValue.Location = new Point(20, 55);
            this.lblCurrentSavingsValue.Text = "45.680,00 TL";
            this.lblCurrentSavingsValue.Name = "lblCurrentSavingsValue";

            this.cardCurrentSavings.Controls.Add(this.lblCurrentSavingsTitle);
            this.cardCurrentSavings.Controls.Add(this.lblCurrentSavingsValue);

            // ============================================
            // CARD 2: Devlet Katkısı (Mavi)
            // ============================================
            this.cardStateContribution.Location = new Point(420, 60);
            this.cardStateContribution.Size = new Size(380, 120);
            this.cardStateContribution.Appearance.BackColor = Color.FromArgb(33, 150, 243);
            this.cardStateContribution.Appearance.Options.UseBackColor = true;
            this.cardStateContribution.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.cardStateContribution.Name = "cardStateContribution";

            this.lblStateContributionTitle.Appearance.ForeColor = Color.White;
            this.lblStateContributionTitle.Appearance.Font = new Font("Segoe UI", 12F);
            this.lblStateContributionTitle.Location = new Point(20, 20);
            this.lblStateContributionTitle.Text = "🏛️ Devlet Katkısı";
            this.lblStateContributionTitle.Name = "lblStateContributionTitle";

            this.lblStateContributionValue.Appearance.ForeColor = Color.White;
            this.lblStateContributionValue.Appearance.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            this.lblStateContributionValue.Location = new Point(20, 55);
            this.lblStateContributionValue.Text = "11.420,00 TL";
            this.lblStateContributionValue.Name = "lblStateContributionValue";

            this.cardStateContribution.Controls.Add(this.lblStateContributionTitle);
            this.cardStateContribution.Controls.Add(this.lblStateContributionValue);

            // ============================================
            // CHART: Tahmini Emeklilik Birikimi (Area)
            // ============================================
            this.chartProjection.Location = new Point(20, 200);
            this.chartProjection.Size = new Size(780, 350);
            this.chartProjection.Name = "chartProjection";
            this.chartProjection.BackColor = Color.FromArgb(30, 30, 30);
            this.chartProjection.AppearanceNameSerializable = "Dark Chameleon";

            // ============================================
            // FON SEÇİMİ
            // ============================================
            this.lblCurrentFund.Appearance.ForeColor = Color.FromArgb(156, 39, 176);
            this.lblCurrentFund.Appearance.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.lblCurrentFund.Location = new Point(20, 570);
            this.lblCurrentFund.Text = "Aktif Fon: Dengeli Fon (%60 Hisse, %40 Tahvil)";
            this.lblCurrentFund.Name = "lblCurrentFund";

            this.lblFundLabel.Appearance.ForeColor = Color.White;
            this.lblFundLabel.Appearance.Font = new Font("Segoe UI", 10F);
            this.lblFundLabel.Location = new Point(20, 610);
            this.lblFundLabel.Text = "Fon Değiştir:";
            this.lblFundLabel.Name = "lblFundLabel";

            this.cmbFund.Location = new Point(120, 605);
            this.cmbFund.Size = new Size(450, 28);
            this.cmbFund.Name = "cmbFund";
            this.cmbFund.Properties.NullText = "Fon Seçiniz...";

            this.btnChangeFund.Location = new Point(590, 600);
            this.btnChangeFund.Size = new Size(130, 35);
            this.btnChangeFund.Text = "Fon Değiştir";
            this.btnChangeFund.Appearance.BackColor = Color.FromArgb(156, 39, 176);
            this.btnChangeFund.Appearance.ForeColor = Color.White;
            this.btnChangeFund.Appearance.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnChangeFund.Appearance.Options.UseBackColor = true;
            this.btnChangeFund.Appearance.Options.UseForeColor = true;
            this.btnChangeFund.Appearance.Options.UseFont = true;
            this.btnChangeFund.Name = "btnChangeFund";
            this.btnChangeFund.Click += BtnChangeFund_Click;

            // Form
            this.ClientSize = new Size(820, 680);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.cardCurrentSavings);
            this.Controls.Add(this.cardStateContribution);
            this.Controls.Add(this.chartProjection);
            this.Controls.Add(this.lblCurrentFund);
            this.Controls.Add(this.lblFundLabel);
            this.Controls.Add(this.cmbFund);
            this.Controls.Add(this.btnChangeFund);
            this.Name = "BESForm";
            this.Text = "Bireysel Emeklilik Sistemi (BES)";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            ((System.ComponentModel.ISupportInitialize)(this.cardCurrentSavings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cardStateContribution)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chartProjection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cmbFund.Properties)).EndInit();
            this.cardCurrentSavings.ResumeLayout(false);
            this.cardStateContribution.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadBESData()
        {
            // Fon listesi
            var funds = new List<BESFund>
            {
                new BESFund { Id = 1, Name = "Muhafazakar Fon (%20 Hisse, %80 Tahvil)", ExpectedReturn = 8.5m },
                new BESFund { Id = 2, Name = "Dengeli Fon (%60 Hisse, %40 Tahvil)", ExpectedReturn = 12.0m },
                new BESFund { Id = 3, Name = "Agresif Fon (%80 Hisse, %20 Tahvil)", ExpectedReturn = 16.5m },
                new BESFund { Id = 4, Name = "Kamu Borçlanma Araçları Fonu", ExpectedReturn = 6.0m },
                new BESFund { Id = 5, Name = "Altın ve Kıymetli Madenler Fonu", ExpectedReturn = 10.0m },
                new BESFund { Id = 6, Name = "Teknoloji ve İnovasyon Fonu", ExpectedReturn = 18.0m }
            };

            cmbFund.Properties.DataSource = funds;
            cmbFund.Properties.DisplayMember = "Name";
            cmbFund.Properties.ValueMember = "Id";

            // Projeksiyon grafiği
            LoadProjectionChart(12.0m); // Dengeli fon getirisi
        }

        private void LoadProjectionChart(decimal expectedReturn)
        {
            chartProjection.Series.Clear();

            var series = new Series("Tahmini Birikim", ViewType.Area);
            
            decimal currentSavings = 45680m;
            decimal monthlyContribution = 2000m;
            decimal stateContribution = 0.25m; // %25 devlet katkısı
            
            // 25 yıllık projeksiyon
            for (int year = 0; year <= 25; year++)
            {
                decimal totalContribution = currentSavings + (monthlyContribution * 12 * year * (1 + stateContribution));
                decimal compoundGrowth = totalContribution * (decimal)Math.Pow((double)(1 + expectedReturn / 100), year);
                
                series.Points.Add(new SeriesPoint($"Yıl {year}", (double)compoundGrowth));
            }

            var areaView = (AreaSeriesView)series.View;
            areaView.Color = Color.FromArgb(100, 76, 175, 80);
            areaView.Border.Color = Color.FromArgb(76, 175, 80);
            areaView.MarkerVisibility = DevExpress.Utils.DefaultBoolean.True;
            // areaView.Marker.Color = Color.FromArgb(76, 175, 80); // Marker is protected, uses series color by default

            chartProjection.Series.Add(series);
            chartProjection.Titles.Clear();
            chartProjection.Titles.Add(new ChartTitle() 
            { 
                Text = $"Tahmini Emeklilik Birikimi (Yıllık %{expectedReturn:N1} Getiri)", 
                TextColor = Color.White 
            });

            // Y ekseni para formatı
            var diagram = chartProjection.Diagram as XYDiagram;
            if (diagram != null)
            {
                diagram.AxisY.Label.TextPattern = "{V:N0} TL";
            }
        }

        private void BtnChangeFund_Click(object sender, EventArgs e)
        {
            if (cmbFund.EditValue == null)
            {
                XtraMessageBox.Show("Lütfen bir fon seçiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedFund = cmbFund.GetSelectedDataRow() as BESFund;
            if (selectedFund != null)
            {
                lblCurrentFund.Text = $"Aktif Fon: {selectedFund.Name}";
                LoadProjectionChart(selectedFund.ExpectedReturn);

                XtraMessageBox.Show(
                    $"✅ FON DEĞİŞİKLİĞİ BAŞARILI\n\n" +
                    $"Yeni Fon: {selectedFund.Name}\n" +
                    $"Beklenen Yıllık Getiri: %{selectedFund.ExpectedReturn:N1}",
                    "Fon Değişikliği", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    // BES Fund Model
    public class BESFund
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal ExpectedReturn { get; set; }
    }
}
