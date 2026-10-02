using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Cost.Accounting.Automation.WinFormsApp.Utils;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.SigningRoleForms
{
    /// <summary>
    /// Yetkili görev tanımı ekleme / düzenleme formu. Yerleşim ve renkler
    /// elle hesaplanmış sabitlerden gelir; tüm renkler aktif skinden çözülür
    /// (bkz. <c>EmployeeSigningRoleEditForm.ApplySkin</c>) ki koyu temalarda da
    /// okunur kalsın.
    /// </summary>
    public partial class EmployeeSigningRoleEditForm
    {
        private const int ContentLeft = 24;
        private const int ContentWidth = 572;
        private const int LabelHeight = 17;
        private const int InputHeight = 32;

        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;
        private System.Windows.Forms.Panel pnlBody;
        private DevExpress.XtraEditors.LabelControl lblNameLabel;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.LabelControl lblDescriptionLabel;
        private DevExpress.XtraEditors.TextEdit txtDescription;
        private DevExpress.XtraEditors.LabelControl lblSortLabel;
        private DevExpress.XtraEditors.SpinEdit spinSortOrder;
        private DevExpress.XtraEditors.ToggleSwitch chkRequiresWorkshop;
        private DevExpress.XtraEditors.LabelControl lblWorkshopHint;
        private DevExpress.XtraEditors.ToggleSwitch chkActive;
        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _skinBinding?.Dispose();
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            pnlBody = new System.Windows.Forms.Panel();
            chkActive = new DevExpress.XtraEditors.ToggleSwitch();
            lblWorkshopHint = new DevExpress.XtraEditors.LabelControl();
            chkRequiresWorkshop = new DevExpress.XtraEditors.ToggleSwitch();
            spinSortOrder = new DevExpress.XtraEditors.SpinEdit();
            lblSortLabel = new DevExpress.XtraEditors.LabelControl();
            txtDescription = new DevExpress.XtraEditors.TextEdit();
            lblDescriptionLabel = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            lblNameLabel = new DevExpress.XtraEditors.LabelControl();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();

            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            pnlBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)spinSortOrder.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkRequiresWorkshop.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();

            // 
            // pnlHeader
            // 
            pnlHeader.Appearance.Options.UseBackColor = true;
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(620, 60);
            pnlHeader.TabIndex = 0;

            ConfigureHeaderIcon(lblHeaderIcon);
            ConfigureTitle(lblTitle, 12, 22);
            ConfigureSubtitle(lblSubtitle, 36, 15);

            ConfigureDivider(pnlHeaderLine, System.Windows.Forms.DockStyle.Bottom, 59);

            // 
            // pnlBody
            // 
            // Zemin skin'e bırakılır; sabit beyaz koyu temada parlak bir kutu
            // olarak görünürdü.
            pnlBody.BackColor = System.Drawing.Color.Transparent;
            pnlBody.Controls.Add(chkActive);
            pnlBody.Controls.Add(lblWorkshopHint);
            pnlBody.Controls.Add(chkRequiresWorkshop);
            pnlBody.Controls.Add(spinSortOrder);
            pnlBody.Controls.Add(lblSortLabel);
            pnlBody.Controls.Add(txtDescription);
            pnlBody.Controls.Add(lblDescriptionLabel);
            pnlBody.Controls.Add(txtName);
            pnlBody.Controls.Add(lblNameLabel);
            pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlBody.Location = new System.Drawing.Point(0, 60);
            pnlBody.Name = "pnlBody";
            pnlBody.Size = new System.Drawing.Size(620, 352);
            pnlBody.TabIndex = 1;

            ConfigureFieldLabel(lblNameLabel, "lblNameLabel", "Görev Adı *", ContentLeft, 18);
            ConfigureFieldLabel(lblDescriptionLabel, "lblDescriptionLabel", "Açıklama", ContentLeft, 88);
            ConfigureFieldLabel(lblSortLabel, "lblSortLabel", "Sıra", ContentLeft, 184);

            ConfigureTextEdit(txtName, "txtName", ContentLeft, 40, ContentWidth, InputHeight,
                "Örn. Sayım Kontrol Sorumlusu", 10.5F);
            ConfigureTextEdit(txtDescription, "txtDescription", ContentLeft, 110, ContentWidth, 56,
                "Bu görevin rapor imza bloklarında ne anlama geldiğini yazın", 10F);
            txtDescription.TabIndex = 3;
            txtName.TabIndex = 1;

            spinSortOrder.Location = new System.Drawing.Point(ContentLeft, 206);
            spinSortOrder.Name = "spinSortOrder";
            spinSortOrder.Properties.Appearance.Font = new Font("Segoe UI", 10.5F);
            spinSortOrder.Properties.Appearance.Options.UseFont = true;
            spinSortOrder.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
                new DevExpress.XtraEditors.Controls.EditorButton(
                    DevExpress.XtraEditors.Controls.ButtonPredefines.Combo) });
            spinSortOrder.Properties.MaxValue = 9999;
            spinSortOrder.Properties.MinValue = 0;
            spinSortOrder.Properties.Padding = new Padding(8, 4, 8, 4);
            spinSortOrder.Size = new System.Drawing.Size(120, InputHeight);
            spinSortOrder.TabIndex = 5;

            chkRequiresWorkshop.Location = new System.Drawing.Point(164, 209);
            chkRequiresWorkshop.Name = "chkRequiresWorkshop";
            chkRequiresWorkshop.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkRequiresWorkshop.Properties.Appearance.Options.UseFont = true;
            chkRequiresWorkshop.Properties.ShowText = true;
            chkRequiresWorkshop.Properties.OnText = "Zorunlu";
            chkRequiresWorkshop.Properties.OffText = "İsteğe Bağlı";
            chkRequiresWorkshop.ToolTip = "Atölye seçimi zorunlu";
            chkRequiresWorkshop.Size = new System.Drawing.Size(240, 24);
            chkRequiresWorkshop.TabIndex = 6;

            lblWorkshopHint.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblWorkshopHint.Appearance.Options.UseFont = true;
            lblWorkshopHint.Location = new System.Drawing.Point(164, 234);
            lblWorkshopHint.Name = "lblWorkshopHint";
            lblWorkshopHint.Size = new System.Drawing.Size(432, 44);
            lblWorkshopHint.TabIndex = 7;
            lblWorkshopHint.Text =
                "İşaretliyse bu görev yalnızca bir atölye için tanımlanabilir "
                + "(örn. Atölye Şefi). Kurum geneli görevlerde işaretli olmamalıdır.";

            chkActive.Location = new System.Drawing.Point(ContentLeft, 292);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 10F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.ShowText = true;
            chkActive.Properties.OnText = "Aktif";
            chkActive.Properties.OffText = "Pasif";
            chkActive.ToolTip = "Kullanımda (Aktif)";
            chkActive.Size = new System.Drawing.Size(220, 22);
            chkActive.TabIndex = 8;

            // 
            // pnlFooter
            // 
            pnlFooter.Appearance.Options.UseBackColor = true;
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlFooter.Location = new System.Drawing.Point(0, 412);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new System.Drawing.Size(620, 58);
            pnlFooter.TabIndex = 2;

            ConfigureDivider(pnlFooterLine, System.Windows.Forms.DockStyle.Top, 0);

            ConfigureFooterButton(btnSave, "btnSave", 496, 100, "Kaydet",
                DxIcon.Check, new Font("Segoe UI Semibold", 9.5F));
            btnSave.TabIndex = 0;

            ConfigureFooterButton(btnCancel, "btnCancel", 386, 100, "Vazgeç",
                DxIcon.Close, new Font("Segoe UI", 9.5F));
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.TabIndex = 1;

            // 
            // EmployeeSigningRoleEditForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(620, 470);
            Controls.Add(pnlBody);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EmployeeSigningRoleEditForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Yetkili Görev";

            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            pnlBody.ResumeLayout(false);
            pnlBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtDescription.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)spinSortOrder.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkRequiresWorkshop.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
        }

        private static void ConfigureHeaderIcon(DevExpress.XtraEditors.LabelControl icon)
        {
            icon.ImageOptions.SvgImage = DxIcon.Roles;
            icon.ImageOptions.SvgImageSize = new System.Drawing.Size(24, 24);
            icon.Location = new System.Drawing.Point(ContentLeft, 18);
            icon.Name = "lblHeaderIcon";
            icon.Size = new System.Drawing.Size(24, 24);
            icon.TabIndex = 2;
        }

        private static void ConfigureTitle(DevExpress.XtraEditors.LabelControl label, int y, int height)
        {
            label.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            label.Appearance.Options.UseFont = true;
            label.Location = new System.Drawing.Point(58, y);
            label.Name = "lblTitle";
            label.Size = new System.Drawing.Size(300, height);
            label.TabIndex = 0;
            label.Text = "Yetkili Görev";
        }

        private static void ConfigureSubtitle(DevExpress.XtraEditors.LabelControl label, int y, int height)
        {
            label.Appearance.Font = new Font("Segoe UI", 9F);
            label.Appearance.Options.UseFont = true;
            label.Location = new System.Drawing.Point(58, y);
            label.Name = "lblSubtitle";
            label.Size = new System.Drawing.Size(400, height);
            label.TabIndex = 3;
            label.Text = "Açıklama metni";
        }

        private static void ConfigureDivider(
            DevExpress.XtraEditors.PanelControl divider,
            System.Windows.Forms.DockStyle dock,
            int y)
        {
            divider.Appearance.Options.UseBackColor = true;
            divider.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            divider.Dock = dock;
            divider.Location = new System.Drawing.Point(0, y);
            divider.Name = dock == System.Windows.Forms.DockStyle.Top ? "pnlFooterLine" : "pnlHeaderLine";
            divider.Size = new System.Drawing.Size(620, 1);
            divider.TabIndex = 2;
        }

        private static void ConfigureFieldLabel(
            DevExpress.XtraEditors.LabelControl label,
            string name,
            string caption,
            int x,
            int y)
        {
            label.Appearance.Font = new Font("Segoe UI Semibold", 9F);
            label.Appearance.Options.UseFont = true;
            label.Location = new System.Drawing.Point(x, y);
            label.Name = name;
            label.Size = new System.Drawing.Size(240, LabelHeight);
            label.TabIndex = 0;
            label.Text = caption;
        }

        private static void ConfigureTextEdit(
            DevExpress.XtraEditors.TextEdit editor,
            string name,
            int x,
            int y,
            int width,
            int height,
            string nullText,
            float fontSize)
        {
            editor.Properties.Appearance.Font = new Font("Segoe UI", fontSize);
            editor.Properties.Appearance.Options.UseFont = true;
            editor.Location = new System.Drawing.Point(x, y);
            editor.Name = name;
            editor.Properties.NullText = nullText;
            editor.Properties.Padding = new Padding(10, 5, 10, 5);
            editor.Size = new System.Drawing.Size(width, height);
        }

        private static void ConfigureFooterButton(
            DevExpress.XtraEditors.SimpleButton button,
            string name,
            int x,
            int width,
            string caption,
            DevExpress.Utils.Svg.SvgImage icon,
            Font font)
        {
            button.Anchor = System.Windows.Forms.AnchorStyles.Bottom
                | System.Windows.Forms.AnchorStyles.Right;
            button.Appearance.Font = font;
            button.Appearance.Options.UseFont = true;
            button.Cursor = System.Windows.Forms.Cursors.Hand;
            button.ImageOptions.ImageToTextAlignment =
                DevExpress.XtraEditors.ImageAlignToText.LeftCenter;
            button.ImageOptions.SvgImage = icon;
            button.ImageOptions.SvgImageSize = new System.Drawing.Size(16, 16);
            button.Location = new System.Drawing.Point(x, 11);
            button.Name = name;
            button.Size = new System.Drawing.Size(width, 34);
            button.Text = caption;
        }
    }
}
