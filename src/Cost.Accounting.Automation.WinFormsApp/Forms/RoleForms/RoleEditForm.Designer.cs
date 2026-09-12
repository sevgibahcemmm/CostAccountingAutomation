namespace Cost.Accounting.Automation.WinFormsApp.Forms.RoleForms
{
    partial class RoleEditForm
    {
        private System.ComponentModel.IContainer components = null;

        private DevExpress.XtraEditors.PanelControl pnlHeader;
        private DevExpress.XtraEditors.LabelControl lblHeaderIcon;
        private DevExpress.XtraEditors.LabelControl lblTitle;
        private DevExpress.XtraEditors.LabelControl lblSubtitle;
        private DevExpress.XtraEditors.PanelControl pnlHeaderLine;

        private DevExpress.XtraEditors.LabelControl lblName;
        private DevExpress.XtraEditors.TextEdit txtName;
        private DevExpress.XtraEditors.CheckEdit chkActive;

        private DevExpress.XtraEditors.LabelControl lblPermissions;
        private DevExpress.XtraEditors.SimpleButton btnGroupSelectAll;
        private DevExpress.XtraEditors.SimpleButton btnGroupClear;
        private DevExpress.XtraEditors.LabelControl lblPermissionSummary;
        private DevExpress.XtraEditors.LabelControl lblSysAdminNote;
        private DevExpress.XtraTreeList.TreeList trePermissions;
        private DevExpress.XtraTreeList.Columns.TreeListColumn colPermission;

        private DevExpress.XtraEditors.PanelControl pnlFooter;
        private DevExpress.XtraEditors.PanelControl pnlFooterLine;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private DevExpress.XtraEditors.SimpleButton btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            pnlHeader = new DevExpress.XtraEditors.PanelControl();
            lblSubtitle = new DevExpress.XtraEditors.LabelControl();
            lblTitle = new DevExpress.XtraEditors.LabelControl();
            lblHeaderIcon = new DevExpress.XtraEditors.LabelControl();
            pnlHeaderLine = new DevExpress.XtraEditors.PanelControl();
            lblName = new DevExpress.XtraEditors.LabelControl();
            txtName = new DevExpress.XtraEditors.TextEdit();
            chkActive = new DevExpress.XtraEditors.CheckEdit();
            lblPermissions = new DevExpress.XtraEditors.LabelControl();
            btnGroupSelectAll = new DevExpress.XtraEditors.SimpleButton();
            btnGroupClear = new DevExpress.XtraEditors.SimpleButton();
            lblPermissionSummary = new DevExpress.XtraEditors.LabelControl();
            lblSysAdminNote = new DevExpress.XtraEditors.LabelControl();
            trePermissions = new DevExpress.XtraTreeList.TreeList();
            colPermission = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            pnlFooter = new DevExpress.XtraEditors.PanelControl();
            btnSave = new DevExpress.XtraEditors.SimpleButton();
            btnCancel = new DevExpress.XtraEditors.SimpleButton();
            pnlFooterLine = new DevExpress.XtraEditors.PanelControl();
            ((System.ComponentModel.ISupportInitialize)pnlHeader).BeginInit();
            pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).BeginInit();
            ((System.ComponentModel.ISupportInitialize)trePermissions).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).BeginInit();
            pnlFooter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblHeaderIcon);
            pnlHeader.Controls.Add(pnlHeaderLine);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(475, 58);
            pnlHeader.TabIndex = 0;
            // 
            // lblSubtitle
            // 
            lblSubtitle.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblSubtitle.Appearance.ForeColor = Color.FromArgb(130, 138, 150);
            lblSubtitle.Appearance.Options.UseFont = true;
            lblSubtitle.Appearance.Options.UseForeColor = true;
            lblSubtitle.Location = new Point(62, 35);
            lblSubtitle.Margin = new Padding(3, 2, 3, 2);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(169, 13);
            lblSubtitle.TabIndex = 3;
            lblSubtitle.Text = "Rol adını ve yetkilerini tanımlayın";
            // 
            // lblTitle
            // 
            lblTitle.Appearance.Font = new Font("Segoe UI Semibold", 12F);
            lblTitle.Appearance.Options.UseFont = true;
            lblTitle.Location = new Point(62, 12);
            lblTitle.Margin = new Padding(3, 2, 3, 2);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(83, 21);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Rol Bilgileri";
            // 
            // lblHeaderIcon
            // 
            lblHeaderIcon.Location = new Point(18, 13);
            lblHeaderIcon.Margin = new Padding(3, 2, 3, 2);
            lblHeaderIcon.Name = "lblHeaderIcon";
            lblHeaderIcon.Size = new Size(0, 13);
            lblHeaderIcon.TabIndex = 2;
            // 
            // pnlHeaderLine
            // 
            pnlHeaderLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlHeaderLine.Appearance.Options.UseBackColor = true;
            pnlHeaderLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlHeaderLine.Dock = DockStyle.Bottom;
            pnlHeaderLine.Location = new Point(0, 57);
            pnlHeaderLine.Name = "pnlHeaderLine";
            pnlHeaderLine.Size = new Size(475, 1);
            pnlHeaderLine.TabIndex = 2;
            // 
            // lblName
            // 
            lblName.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblName.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblName.Appearance.Options.UseFont = true;
            lblName.Appearance.Options.UseForeColor = true;
            lblName.Location = new Point(20, 80);
            lblName.Margin = new Padding(3, 2, 3, 2);
            lblName.Name = "lblName";
            lblName.Size = new Size(37, 13);
            lblName.TabIndex = 0;
            lblName.Text = "Rol Adı";
            // 
            // txtName
            // 
            txtName.Location = new Point(20, 98);
            txtName.Margin = new Padding(3, 2, 3, 2);
            txtName.Name = "txtName";
            txtName.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            txtName.Properties.Appearance.Options.UseFont = true;
            txtName.Properties.NullText = "muhasebe_muduru";
            txtName.Size = new Size(434, 24);
            txtName.TabIndex = 1;
            // 
            // chkActive
            // 
            chkActive.Location = new Point(20, 138);
            chkActive.Margin = new Padding(3, 2, 3, 2);
            chkActive.Name = "chkActive";
            chkActive.Properties.Appearance.Font = new Font("Segoe UI", 9.5F);
            chkActive.Properties.Appearance.Options.UseFont = true;
            chkActive.Properties.Caption = "Aktif Rol";
            chkActive.Size = new Size(160, 21);
            chkActive.TabIndex = 2;
            // 
            // lblPermissions
            // 
            lblPermissions.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPermissions.Appearance.ForeColor = Color.FromArgb(100, 106, 116);
            lblPermissions.Appearance.Options.UseFont = true;
            lblPermissions.Appearance.Options.UseForeColor = true;
            lblPermissions.Location = new Point(20, 178);
            lblPermissions.Margin = new Padding(3, 2, 3, 2);
            lblPermissions.Name = "lblPermissions";
            lblPermissions.Size = new Size(37, 13);
            lblPermissions.TabIndex = 3;
            lblPermissions.Text = "Yetkiler";
            // 
            // btnGroupSelectAll
            // 
            btnGroupSelectAll.Appearance.Font = new Font("Segoe UI", 9F);
            btnGroupSelectAll.Appearance.Options.UseFont = true;
            btnGroupSelectAll.Location = new Point(20, 198);
            btnGroupSelectAll.Margin = new Padding(3, 2, 3, 2);
            btnGroupSelectAll.Name = "btnGroupSelectAll";
            btnGroupSelectAll.Size = new Size(135, 30);
            btnGroupSelectAll.TabIndex = 4;
            btnGroupSelectAll.Text = "Tümünü Seç";
            // 
            // btnGroupClear
            // 
            btnGroupClear.Appearance.Font = new Font("Segoe UI", 9F);
            btnGroupClear.Appearance.Options.UseFont = true;
            btnGroupClear.Location = new Point(163, 198);
            btnGroupClear.Margin = new Padding(3, 2, 3, 2);
            btnGroupClear.Name = "btnGroupClear";
            btnGroupClear.Size = new Size(135, 30);
            btnGroupClear.TabIndex = 5;
            btnGroupClear.Text = "Tümünü Kaldır";
            // 
            // lblPermissionSummary
            // 
            lblPermissionSummary.AllowHtmlString = true;
            lblPermissionSummary.Appearance.Font = new Font("Segoe UI", 8.5F);
            lblPermissionSummary.Appearance.ForeColor = Color.FromArgb(90, 98, 110);
            lblPermissionSummary.Appearance.Options.UseFont = true;
            lblPermissionSummary.Appearance.Options.UseForeColor = true;
            lblPermissionSummary.Appearance.Options.UseTextOptions = true;
            lblPermissionSummary.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
            lblPermissionSummary.Location = new Point(306, 204);
            lblPermissionSummary.Margin = new Padding(3, 2, 3, 2);
            lblPermissionSummary.Name = "lblPermissionSummary";
            lblPermissionSummary.Size = new Size(68, 13);
            lblPermissionSummary.TabIndex = 7;
            lblPermissionSummary.Text = "0 yetki seçildi";
            // 
            // lblSysAdminNote
            // 
            lblSysAdminNote.AllowHtmlString = true;
            lblSysAdminNote.Appearance.Font = new Font("Segoe UI", 9F);
            lblSysAdminNote.Appearance.ForeColor = Color.FromArgb(88, 96, 108);
            lblSysAdminNote.Appearance.Options.UseFont = true;
            lblSysAdminNote.Appearance.Options.UseForeColor = true;
            lblSysAdminNote.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            lblSysAdminNote.Location = new Point(20, 196);
            lblSysAdminNote.Margin = new Padding(3, 2, 3, 2);
            lblSysAdminNote.Name = "lblSysAdminNote";
            lblSysAdminNote.Size = new Size(434, 76);
            lblSysAdminNote.TabIndex = 8;
            lblSysAdminNote.Text = "<b>sys_admin</b>, sistemin en yüksek yetkili rolüdür ve <b>TÜM yetkilere</b> otomatik olarak sahiptir.\n\nBu rol için yetki tanımlaması gerekmez; yetki ayrıcalıkları kilitlidir.";
            // 
            // trePermissions
            // 
            trePermissions.Appearance.FocusedCell.Font = new Font("Segoe UI", 9F);
            trePermissions.Appearance.FocusedCell.Options.UseFont = true;
            trePermissions.Appearance.FocusedRow.Font = new Font("Segoe UI", 9F);
            trePermissions.Appearance.FocusedRow.Options.UseFont = true;
            trePermissions.Appearance.Row.Font = new Font("Segoe UI", 9F);
            trePermissions.Appearance.Row.Options.UseFont = true;
            trePermissions.Appearance.TreeLine.Font = new Font("Segoe UI", 9F);
            trePermissions.Appearance.TreeLine.Options.UseFont = true;
            trePermissions.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] { colPermission });
            trePermissions.Location = new Point(20, 264);
            trePermissions.Margin = new Padding(3, 2, 3, 2);
            trePermissions.Name = "trePermissions";
            trePermissions.OptionsBehavior.AllowRecursiveNodeChecking = true;
            trePermissions.OptionsBehavior.Editable = false;
            trePermissions.OptionsSelection.EnableAppearanceFocusedCell = false;
            trePermissions.OptionsView.CheckBoxStyle = DevExpress.XtraTreeList.DefaultNodeCheckBoxStyle.Check;
            trePermissions.OptionsView.ShowColumns = false;
            trePermissions.OptionsView.ShowHorzLines = false;
            trePermissions.RowHeight = 30;
            trePermissions.Size = new Size(434, 235);
            trePermissions.TabIndex = 6;
            // 
            // colPermission
            // 
            colPermission.Caption = "Yetkiler";
            colPermission.FieldName = "Yetkiler";
            colPermission.MinWidth = 200;
            colPermission.Name = "colPermission";
            colPermission.OptionsColumn.AllowEdit = false;
            colPermission.Visible = true;
            colPermission.VisibleIndex = 0;
            colPermission.Width = 420;
            // 
            // pnlFooter
            // 
            pnlFooter.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Controls.Add(pnlFooterLine);
            pnlFooter.Dock = DockStyle.Bottom;
            pnlFooter.Location = new Point(0, 513);
            pnlFooter.Name = "pnlFooter";
            pnlFooter.Size = new Size(475, 60);
            pnlFooter.TabIndex = 7;
            // 
            // btnSave
            // 
            btnSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSave.Appearance.BackColor = DevExpress.LookAndFeel.DXSkinColors.FillColors.Primary;
            btnSave.Appearance.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSave.Appearance.ForeColor = Color.White;
            btnSave.Appearance.Options.UseBackColor = true;
            btnSave.Appearance.Options.UseFont = true;
            btnSave.Appearance.Options.UseForeColor = true;
            btnSave.Location = new Point(307, 13);
            btnSave.Margin = new Padding(3, 2, 3, 2);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(148, 32);
            btnSave.TabIndex = 9;
            btnSave.Text = "Kaydet";
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCancel.Appearance.Font = new Font("Segoe UI", 9.5F);
            btnCancel.Appearance.Options.UseFont = true;
            btnCancel.Location = new Point(195, 13);
            btnCancel.Margin = new Padding(3, 2, 3, 2);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(104, 32);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "İptal";
            // 
            // pnlFooterLine
            // 
            pnlFooterLine.Appearance.BackColor = Color.FromArgb(224, 226, 230);
            pnlFooterLine.Appearance.Options.UseBackColor = true;
            pnlFooterLine.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.NoBorder;
            pnlFooterLine.Dock = DockStyle.Top;
            pnlFooterLine.Location = new Point(0, 0);
            pnlFooterLine.Name = "pnlFooterLine";
            pnlFooterLine.Size = new Size(475, 1);
            pnlFooterLine.TabIndex = 0;
            // 
            // RoleEditForm
            // 
            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(475, 573);
            Controls.Add(trePermissions);
            Controls.Add(lblPermissionSummary);
            Controls.Add(lblSysAdminNote);
            Controls.Add(btnGroupClear);
            Controls.Add(btnGroupSelectAll);
            Controls.Add(lblPermissions);
            Controls.Add(chkActive);
            Controls.Add(txtName);
            Controls.Add(lblName);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RoleEditForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Rol";
            ((System.ComponentModel.ISupportInitialize)pnlHeader).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pnlHeaderLine).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtName.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)chkActive.Properties).EndInit();
            ((System.ComponentModel.ISupportInitialize)trePermissions).EndInit();
            ((System.ComponentModel.ISupportInitialize)pnlFooter).EndInit();
            pnlFooter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pnlFooterLine).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }
    }
}