using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using BankApp.Infrastructure.Services;
using BankApp.Core.Entities;
using System.Collections.Generic;

namespace BankApp.UI.Forms
{
    public partial class InvestmentForm : XtraForm
    {
        private readonly StockService _stockService;
        private readonly CommodityService _commodityService;

        public InvestmentForm()
        {
            InitializeComponent();
            _stockService = new StockService();
            _commodityService = new CommodityService();
            
            PopulateTiles();
            PopulatePortfolio();
        }

        private void PopulateTiles()
        {
            tileGroup1.Items.Clear();
            var markets = _commodityService.GetAllMarkets();

            foreach(var m in markets)
            {
                TileItem item = new TileItem();
                item.ItemSize = TileItemSize.Wide; // Geniş kutular
                
                // Renk Ayarı
                Color backColor = Color.FromArgb(45, 45, 48); // Koyu Gri
                if (m.Name.Contains("Altın")) backColor = Color.FromArgb(255, 193, 7); // Amber
                if (m.Name.Contains("Dolar")) backColor = Color.FromArgb(76, 175, 80); // Green
                if (m.Name.Contains("Hisse")) backColor = Color.FromArgb(33, 150, 243); // Blue
                if (m.Name.Contains("Bitcoin")) backColor = Color.FromArgb(255, 87, 34); // Orange

                item.AppearanceItem.Normal.BackColor = backColor;
                item.AppearanceItem.Normal.BorderColor = Color.Transparent;

                // İçerik (Elements)
                // 1. İsim (Sol Üst)
                TileItemElement elName = new TileItemElement();
                elName.Text = m.Name.ToUpper();
                elName.TextAlignment = TileItemContentAlignment.TopLeft;
                elName.Appearance.Normal.FontSizeDelta = 2;
                elName.Appearance.Normal.Font = new Font("Segoe UI", 12, FontStyle.Bold);

                // 2. Fiyat (Orta Büyük)
                TileItemElement elPrice = new TileItemElement();
                elPrice.Text = $"{m.Price:N2}";
                elPrice.TextAlignment = TileItemContentAlignment.MiddleCenter;
                elPrice.Appearance.Normal.FontSizeDelta = 12;
                elPrice.Appearance.Normal.Font = new Font("Segoe UI", 24, FontStyle.Bold);

                // 3. Değişim (Sağ Alt)
                TileItemElement elChange = new TileItemElement();
                string arrow = m.ChangePercent >= 0 ? "▲" : "▼";
                elChange.Text = $"{arrow} %{Math.Abs(m.ChangePercent):N2}";
                elChange.TextAlignment = TileItemContentAlignment.BottomRight;
                elChange.Appearance.Normal.FontSizeDelta = 4;
                elChange.Appearance.Normal.ForeColor = Color.White; // Arka plan renkli zaten
                
                item.Elements.Add(elName);
                item.Elements.Add(elPrice);
                item.Elements.Add(elChange);
                
                tileGroup1.Items.Add(item);
                
                // Click Effect
                item.ItemClick += (s, e) => {
                    // Tıklanınca detay veya işlem açılabilir
                    StockMarketForm frm = new StockMarketForm();
                    frm.ShowDialog();
                };
            }
        }

        private void PopulatePortfolio()
        {
            // Dummy Portfolio Data for the Grid
            var list = new List<dynamic>
            {
                new { Symbol = "BIST 100", Type = "Endeks", Amount = 1, Cost = 9100m, Current = 9450m, PL = "+350 TL", Pct = "%3.8" },
                new { Symbol = "THYAO", Type = "Hisse", Amount = 500, Cost = 250.40m, Current = 285.10m, PL = "+17,350 TL", Pct = "%13.8" },
                new { Symbol = "USD/TRY", Type = "Döviz", Amount = 1000, Cost = 32.50m, Current = 42.50m, PL = "+10,000 TL", Pct = "%30.7" },
                new { Symbol = "Gram Altın", Type = "Emtia", Amount = 50, Cost = 2100m, Current = 2800m, PL = "+35,000 TL", Pct = "%33.3" }
            };
            grdPortfoy.DataSource = list;
        }

        private void btnYenile_Click(object sender, EventArgs e) => PopulateTiles();
        private void btnHisseAl_Click(object sender, EventArgs e) { new StockMarketForm().ShowDialog(); }
        private void btnHisseSat_Click(object sender, EventArgs e) { new StockMarketForm().ShowDialog(); }
    }
}
