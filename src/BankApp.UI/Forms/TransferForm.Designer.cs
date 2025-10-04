using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraLayout;
using DevExpress.Utils;
using DevExpress.LookAndFeel;

namespace BankApp.UI.Forms
{
    /// <summary>
    /// Para transferi formu - Designer dosyası
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public partial class TransferForm
    {
        private IContainer components = null;
        
        // Ana kapsayıcılar
        private PanelControl pnlArkaPlan;
        private PanelControl pnlKart;
        private LayoutControl layoutAna;
        private LayoutControlGroup layoutGroupRoot;
        
        // Başlık
        private LabelControl lblBaslik;
        private SimpleButton btnKapat;
        
        // Giriş kontrolleri
        private LookUpEdit lueKaynakHesap;
        private TextEdit txtHedefIban;
        private CalcEdit calcTutar;
        private MemoEdit memoAciklama;
        private SimpleButton btnGonder;
        private SimpleButton btnIptal;
        
        // Layout öğeleri
        private LayoutControlItem layoutItemSourceAccount;
        private LayoutControlItem layoutItemTargetIban;
        private LayoutControlItem layoutItemAmount;
        private LayoutControlItem layoutItemDescription;
        private LayoutControlItem layoutItemTransferBtn;
        private LayoutControlItem layoutItemCancelBtn;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            
            UserLookAndFeel.Default.SetSkinStyle("Office 2019 Black");
            
            // Kontrolleri başlat
            this.pnlArkaPlan = new PanelControl();
            this.pnlKart = new PanelControl();
            this.layoutAna = new LayoutControl();
            this.layoutGroupRoot = new LayoutControlGroup();
            
            this.lblBaslik = new LabelControl();
            this.btnKapat = new SimpleButton();
            
            this.lueKaynakHesap = new LookUpEdit();
            this.txtHedefIban = new TextEdit();
            this.calcTutar = new CalcEdit();
            this.memoAciklama = new MemoEdit();
            this.btnGonder = new SimpleButton();
            this.btnIptal = new SimpleButton();

            // Begin Init
            ((ISupportInitialize)(this.pnlArkaPlan)).BeginInit();
            ((ISupportInitialize)(this.pnlKart)).BeginInit();
            ((ISupportInitialize)(this.layoutAna)).BeginInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).BeginInit();
            ((ISupportInitialize)(this.lueKaynakHesap.Properties)).BeginInit();
            ((ISupportInitialize)(this.txtHedefIban.Properties)).BeginInit();
            ((ISupportInitialize)(this.calcTutar.Properties)).BeginInit();
            ((ISupportInitialize)(this.memoAciklama.Properties)).BeginInit();
            this.pnlArkaPlan.SuspendLayout();
            this.pnlKart.SuspendLayout();
            this.layoutAna.SuspendLayout();
            this.SuspendLayout();

            // pnlArkaPlan
            this.pnlArkaPlan.Dock = DockStyle.Fill;
            this.pnlArkaPlan.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlArkaPlan.Appearance.BackColor = Color.FromArgb(15, 15, 20);
            this.pnlArkaPlan.Appearance.Options.UseBackColor = true;
            this.pnlArkaPlan.Name = "pnlArkaPlan";
            this.pnlArkaPlan.Controls.Add(this.pnlKart);

            // pnlKart
            this.pnlKart.Size = new Size(480, 580);
            this.pnlKart.Location = new Point(35, 20);
            this.pnlKart.Name = "pnlKart";
            this.pnlKart.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            this.pnlKart.Appearance.BackColor = Color.FromArgb(30, 32, 40);
            this.pnlKart.Appearance.Options.UseBackColor = true;
            this.pnlKart.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.pnlKart.Padding = new Padding(20);
            this.pnlKart.Controls.Add(this.lblBaslik);
            this.pnlKart.Controls.Add(this.btnKapat);
            this.pnlKart.Controls.Add(this.layoutAna);
            
            this.pnlKart.Paint += (s, e) => {
                var rect = new Rectangle(0, 0, pnlKart.Width - 1, pnlKart.Height - 1);
                using (var path = CreateRoundedRectPath(rect, 20))
                using (var pen = new Pen(Color.FromArgb(60, 70, 90), 2))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(pen, path);
                }
            };

            // lblBaslik
            this.lblBaslik.Text = "💳 Para Transferi";
            this.lblBaslik.Location = new Point(25, 20);
            this.lblBaslik.Size = new Size(350, 35);
            this.lblBaslik.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.lblBaslik.Appearance.ForeColor = Color.White;
            this.lblBaslik.Appearance.Options.UseFont = true;
            this.lblBaslik.Appearance.Options.UseForeColor = true;
            this.lblBaslik.Name = "lblBaslik";

            // btnKapat
            this.btnKapat.Text = "✕";
            this.btnKapat.Location = new Point(430, 15);
            this.btnKapat.Size = new Size(35, 35);
            this.btnKapat.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnKapat.Appearance.BackColor = Color.FromArgb(60, 60, 70);
            this.btnKapat.Appearance.ForeColor = Color.White;
            this.btnKapat.Appearance.Options.UseBackColor = true;
            this.btnKapat.Appearance.Options.UseForeColor = true;
            this.btnKapat.Appearance.Options.UseFont = true;
            this.btnKapat.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnKapat.Name = "btnKapat";
            this.btnKapat.Click += (s, e) => this.Close();

            // layoutAna
            this.layoutAna.Location = new Point(15, 65);
            this.layoutAna.Size = new Size(450, 500);
            this.layoutAna.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.layoutAna.Name = "layoutAna";
            this.layoutAna.Root = this.layoutGroupRoot;

            // lueKaynakHesap
            this.lueKaynakHesap.Name = "lueKaynakHesap";
            this.lueKaynakHesap.Properties.NullText = "💰 Hesap Seçiniz...";
            this.lueKaynakHesap.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.lueKaynakHesap.Properties.Appearance.ForeColor = Color.White;
            this.lueKaynakHesap.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.lueKaynakHesap.Properties.Appearance.Options.UseFont = true;
            this.lueKaynakHesap.Properties.Appearance.Options.UseForeColor = true;
            this.lueKaynakHesap.Properties.Appearance.Options.UseBackColor = true;
            this.lueKaynakHesap.Properties.AppearanceDropDown.Font = new Font("Tahoma", 8.25F);
            this.layoutAna.Controls.Add(this.lueKaynakHesap);

            // txtHedefIban
            this.txtHedefIban.Name = "txtHedefIban";
            this.txtHedefIban.Properties.Appearance.Font = new Font("Consolas", 12F);
            this.txtHedefIban.Properties.Appearance.ForeColor = Color.FromArgb(129, 199, 245);
            this.txtHedefIban.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.txtHedefIban.Properties.Appearance.Options.UseFont = true;
            this.txtHedefIban.Properties.Appearance.Options.UseForeColor = true;
            this.txtHedefIban.Properties.Appearance.Options.UseBackColor = true;
            this.txtHedefIban.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Simple;
            this.txtHedefIban.Properties.Mask.EditMask = "TR00-0000-0000-0000-0000-0000-00";
            this.txtHedefIban.Properties.Mask.UseMaskAsDisplayFormat = true;
            this.layoutAna.Controls.Add(this.txtHedefIban);

            // calcTutar
            this.calcTutar.Name = "calcTutar";
            this.calcTutar.Properties.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.calcTutar.Properties.Appearance.ForeColor = Color.FromArgb(76, 175, 80);
            this.calcTutar.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.calcTutar.Properties.Appearance.Options.UseFont = true;
            this.calcTutar.Properties.Appearance.Options.UseForeColor = true;
            this.calcTutar.Properties.Appearance.Options.UseBackColor = true;
            this.calcTutar.Properties.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            this.calcTutar.Properties.DisplayFormat.FormatType = FormatType.Numeric;
            this.calcTutar.Properties.DisplayFormat.FormatString = "₺ #,##0.00";
            this.calcTutar.Properties.EditFormat.FormatType = FormatType.Numeric;
            this.calcTutar.Properties.EditFormat.FormatString = "#,##0.00";
            this.layoutAna.Controls.Add(this.calcTutar);

            // memoAciklama
            this.memoAciklama.Name = "memoAciklama";
            this.memoAciklama.Properties.Appearance.Font = new Font("Tahoma", 8.25F);
            this.memoAciklama.Properties.Appearance.ForeColor = Color.White;
            this.memoAciklama.Properties.Appearance.BackColor = Color.FromArgb(45, 48, 58);
            this.memoAciklama.Properties.Appearance.Options.UseFont = true;
            this.memoAciklama.Properties.Appearance.Options.UseForeColor = true;
            this.memoAciklama.Properties.Appearance.Options.UseBackColor = true;
            this.memoAciklama.Properties.NullValuePrompt = "Transfer açıklaması yazınız...";
            this.memoAciklama.Properties.NullValuePromptShowForEmptyValue = true;
            this.layoutAna.Controls.Add(this.memoAciklama);

            // btnGonder
            this.btnGonder.Name = "btnGonder";
            this.btnGonder.Text = "💸 GÖNDER";
            this.btnGonder.Appearance.Font = new Font("Tahoma", 8.25F, FontStyle.Bold);
            this.btnGonder.Appearance.BackColor = Color.FromArgb(34, 139, 34);
            this.btnGonder.Appearance.ForeColor = Color.White;
            this.btnGonder.Appearance.Options.UseBackColor = true;
            this.btnGonder.Appearance.Options.UseForeColor = true;
            this.btnGonder.Appearance.Options.UseFont = true;
            this.btnGonder.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnGonder.Size = new Size(200, 50);
            this.btnGonder.Click += new System.EventHandler(this.btnGonder_Click);
            this.layoutAna.Controls.Add(this.btnGonder);

            // btnIptal
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Text = "İPTAL";
            this.btnIptal.Appearance.Font = new Font("Tahoma", 8.25F);
            this.btnIptal.Appearance.BackColor = Color.FromArgb(70, 70, 80);
            this.btnIptal.Appearance.ForeColor = Color.White;
            this.btnIptal.Appearance.Options.UseBackColor = true;
            this.btnIptal.Appearance.Options.UseForeColor = true;
            this.btnIptal.Appearance.Options.UseFont = true;
            this.btnIptal.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.HotFlat;
            this.btnIptal.Size = new Size(200, 50);
            this.btnIptal.Click += (s, e) => this.Close();
            this.layoutAna.Controls.Add(this.btnIptal);

            // Layout Items
            this.layoutItemSourceAccount = new LayoutControlItem(this.layoutAna, this.lueKaynakHesap);
            this.layoutItemSourceAccount.Text = "🏦 Kaynak Hesap";
            this.layoutItemSourceAccount.TextLocation = Locations.Top;
            this.layoutItemSourceAccount.AppearanceItemCaption.Font = new Font("Tahoma", 8.25F);
            this.layoutItemSourceAccount.AppearanceItemCaption.ForeColor = Color.FromArgb(220, 225, 235);
            this.layoutItemSourceAccount.AppearanceItemCaption.Options.UseFont = true;
            this.layoutItemSourceAccount.AppearanceItemCaption.Options.UseForeColor = true;
            this.layoutItemSourceAccount.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemSourceAccount.MinSize = new Size(0, 70);
            this.layoutItemSourceAccount.MaxSize = new Size(0, 70);
            this.layoutItemSourceAccount.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 10);

            this.layoutItemTargetIban = new LayoutControlItem(this.layoutAna, this.txtHedefIban);
            this.layoutItemTargetIban.Text = "🏷️ Alıcı IBAN";
            this.layoutItemTargetIban.TextLocation = Locations.Top;
            this.layoutItemTargetIban.AppearanceItemCaption.Font = new Font("Tahoma", 8.25F);
            this.layoutItemTargetIban.AppearanceItemCaption.ForeColor = Color.FromArgb(220, 225, 235);
            this.layoutItemTargetIban.AppearanceItemCaption.Options.UseFont = true;
            this.layoutItemTargetIban.AppearanceItemCaption.Options.UseForeColor = true;
            this.layoutItemTargetIban.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemTargetIban.MinSize = new Size(0, 70);
            this.layoutItemTargetIban.MaxSize = new Size(0, 70);
            this.layoutItemTargetIban.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 10);

            this.layoutItemAmount = new LayoutControlItem(this.layoutAna, this.calcTutar);
            this.layoutItemAmount.Text = "💵 Tutar";
            this.layoutItemAmount.TextLocation = Locations.Top;
            this.layoutItemAmount.AppearanceItemCaption.Font = new Font("Tahoma", 8.25F);
            this.layoutItemAmount.AppearanceItemCaption.ForeColor = Color.FromArgb(220, 225, 235);
            this.layoutItemAmount.AppearanceItemCaption.Options.UseFont = true;
            this.layoutItemAmount.AppearanceItemCaption.Options.UseForeColor = true;
            this.layoutItemAmount.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemAmount.MinSize = new Size(0, 90);
            this.layoutItemAmount.MaxSize = new Size(0, 90);
            this.layoutItemAmount.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 10);

            this.layoutItemDescription = new LayoutControlItem(this.layoutAna, this.memoAciklama);
            this.layoutItemDescription.Text = "📝 Açıklama";
            this.layoutItemDescription.TextLocation = Locations.Top;
            this.layoutItemDescription.AppearanceItemCaption.Font = new Font("Tahoma", 8.25F);
            this.layoutItemDescription.AppearanceItemCaption.ForeColor = Color.FromArgb(220, 225, 235);
            this.layoutItemDescription.AppearanceItemCaption.Options.UseFont = true;
            this.layoutItemDescription.AppearanceItemCaption.Options.UseForeColor = true;
            this.layoutItemDescription.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemDescription.MinSize = new Size(0, 100);
            this.layoutItemDescription.MaxSize = new Size(0, 100);
            this.layoutItemDescription.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 10, 10);

            this.layoutItemTransferBtn = new LayoutControlItem(this.layoutAna, this.btnGonder);
            this.layoutItemTransferBtn.TextVisible = false;
            this.layoutItemTransferBtn.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemTransferBtn.MinSize = new Size(210, 65);
            this.layoutItemTransferBtn.MaxSize = new Size(210, 65);
            this.layoutItemTransferBtn.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 15, 10);

            this.layoutItemCancelBtn = new LayoutControlItem(this.layoutAna, this.btnIptal);
            this.layoutItemCancelBtn.TextVisible = false;
            this.layoutItemCancelBtn.SizeConstraintsType = SizeConstraintsType.Custom;
            this.layoutItemCancelBtn.MinSize = new Size(210, 65);
            this.layoutItemCancelBtn.MaxSize = new Size(210, 65);
            this.layoutItemCancelBtn.Padding = new DevExpress.XtraLayout.Utils.Padding(5, 5, 15, 10);

            // Layout Group Root
            this.layoutGroupRoot.Name = "layoutGroupRoot";
            this.layoutGroupRoot.EnableIndentsWithoutBorders = DefaultBoolean.True;
            this.layoutGroupRoot.GroupBordersVisible = false;
            this.layoutGroupRoot.Padding = new DevExpress.XtraLayout.Utils.Padding(10, 10, 10, 10);
            this.layoutGroupRoot.Spacing = new DevExpress.XtraLayout.Utils.Padding(0, 0, 5, 5);

            // TransferForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(550, 620);
            this.Controls.Add(this.pnlArkaPlan);
            this.Name = "TransferForm";
            this.Text = "";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.BackColor = Color.FromArgb(15, 15, 20);
            
            this.Load += (s, e) => {
                var path = CreateRoundedRectPath(new Rectangle(0, 0, this.Width, this.Height), 25);
                this.Region = new Region(path);
            };

            // End Init
            ((ISupportInitialize)(this.pnlArkaPlan)).EndInit();
            ((ISupportInitialize)(this.pnlKart)).EndInit();
            ((ISupportInitialize)(this.layoutAna)).EndInit();
            ((ISupportInitialize)(this.layoutGroupRoot)).EndInit();
            ((ISupportInitialize)(this.lueKaynakHesap.Properties)).EndInit();
            ((ISupportInitialize)(this.txtHedefIban.Properties)).EndInit();
            ((ISupportInitialize)(this.calcTutar.Properties)).EndInit();
            ((ISupportInitialize)(this.memoAciklama.Properties)).EndInit();
            this.pnlArkaPlan.ResumeLayout(false);
            this.pnlKart.ResumeLayout(false);
            this.layoutAna.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Yuvarlak köşeli dikdörtgen path oluşturur
        /// </summary>
        /// <param name="rect">Dikdörtgen</param>
        /// <param name="radius">Köşe yarıçapı</param>
        /// <returns>GraphicsPath</returns>
        private GraphicsPath CreateRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            
            return path;
        }
    }
}
