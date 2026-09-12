namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class DashboardMdiForm
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlBody;
        private System.Windows.Forms.TableLayoutPanel tblLayout;
        private System.Windows.Forms.TableLayoutPanel tblKpi;
        private System.Windows.Forms.TableLayoutPanel tblCharts;
        private DevExpress.XtraEditors.PanelControl kpi1;
        private DevExpress.XtraEditors.PanelControl kpi2;
        private DevExpress.XtraEditors.PanelControl kpi3;
        private DevExpress.XtraEditors.PanelControl kpi4;
        private System.Windows.Forms.Panel acc1;
        private System.Windows.Forms.Panel acc2;
        private System.Windows.Forms.Panel acc3;
        private System.Windows.Forms.Panel acc4;
        private System.Windows.Forms.Label lblKpi1Icon;
        private System.Windows.Forms.Label lblKpi2Icon;
        private System.Windows.Forms.Label lblKpi3Icon;
        private System.Windows.Forms.Label lblKpi4Icon;
        private System.Windows.Forms.Label lblKpi1Value;
        private System.Windows.Forms.Label lblKpi2Value;
        private System.Windows.Forms.Label lblKpi3Value;
        private System.Windows.Forms.Label lblKpi4Value;
        private System.Windows.Forms.Label lblKpi1Title;
        private System.Windows.Forms.Label lblKpi2Title;
        private System.Windows.Forms.Label lblKpi3Title;
        private System.Windows.Forms.Label lblKpi4Title;
        private System.Windows.Forms.Panel pnlChartRoles;
        private System.Windows.Forms.Label lblChartRolesTitle;
        private System.Windows.Forms.Panel pnlChartCompanies;
        private System.Windows.Forms.Label lblChartCompaniesTitle;
        private DevExpress.XtraCharts.ChartControl chartRoles;
        private DevExpress.XtraCharts.ChartControl chartCompanies;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblSub;
        private System.Windows.Forms.Label lblDate;

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
            components = new System.ComponentModel.Container();
            pnlHeader = new System.Windows.Forms.Panel();
            lblWelcome = new System.Windows.Forms.Label();
            lblSub = new System.Windows.Forms.Label();
            lblDate = new System.Windows.Forms.Label();
            pnlBody = new System.Windows.Forms.Panel();
            tblLayout = new System.Windows.Forms.TableLayoutPanel();
            tblKpi = new System.Windows.Forms.TableLayoutPanel();
            kpi1 = new DevExpress.XtraEditors.PanelControl();
            acc1 = new System.Windows.Forms.Panel();
            lblKpi1Value = new System.Windows.Forms.Label();
            lblKpi1Title = new System.Windows.Forms.Label();
            lblKpi1Icon = new System.Windows.Forms.Label();
            kpi2 = new DevExpress.XtraEditors.PanelControl();
            acc2 = new System.Windows.Forms.Panel();
            lblKpi2Value = new System.Windows.Forms.Label();
            lblKpi2Title = new System.Windows.Forms.Label();
            lblKpi2Icon = new System.Windows.Forms.Label();
            kpi3 = new DevExpress.XtraEditors.PanelControl();
            acc3 = new System.Windows.Forms.Panel();
            lblKpi3Value = new System.Windows.Forms.Label();
            lblKpi3Title = new System.Windows.Forms.Label();
            lblKpi3Icon = new System.Windows.Forms.Label();
            kpi4 = new DevExpress.XtraEditors.PanelControl();
            acc4 = new System.Windows.Forms.Panel();
            lblKpi4Value = new System.Windows.Forms.Label();
            lblKpi4Title = new System.Windows.Forms.Label();
            lblKpi4Icon = new System.Windows.Forms.Label();
            tblCharts = new System.Windows.Forms.TableLayoutPanel();
            pnlChartRoles = new System.Windows.Forms.Panel();
            lblChartRolesTitle = new System.Windows.Forms.Label();
            chartRoles = new DevExpress.XtraCharts.ChartControl();
            pnlChartCompanies = new System.Windows.Forms.Panel();
            lblChartCompaniesTitle = new System.Windows.Forms.Label();
            chartCompanies = new DevExpress.XtraCharts.ChartControl();
            pnlHeader.SuspendLayout();
            pnlBody.SuspendLayout();
            tblLayout.SuspendLayout();
            tblKpi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpi1).BeginInit();
            kpi1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpi2).BeginInit();
            kpi2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpi3).BeginInit();
            kpi3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)kpi4).BeginInit();
            kpi4.SuspendLayout();
            tblCharts.SuspendLayout();
            pnlChartRoles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartRoles).BeginInit();
            pnlChartCompanies.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartCompanies).BeginInit();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = System.Drawing.Color.Transparent;
            pnlHeader.Controls.Add(lblDate);
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblWelcome);
            pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            pnlHeader.Location = new System.Drawing.Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new System.Drawing.Size(1280, 120);
            pnlHeader.TabIndex = 0;
            // 
            // lblWelcome
            // 
            lblWelcome.AutoSize = true;
            lblWelcome.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
            lblWelcome.ForeColor = System.Drawing.Color.White;
            lblWelcome.Location = new System.Drawing.Point(34, 26);
            lblWelcome.Name = "lblWelcome";
            lblWelcome.Size = new System.Drawing.Size(300, 54);
            lblWelcome.TabIndex = 0;
            lblWelcome.Text = "Hoş Geldiniz!";
            // 
            // lblSub
            // 
            lblSub.AutoSize = true;
            lblSub.Font = new System.Drawing.Font("Segoe UI", 11F);
            lblSub.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            lblSub.Location = new System.Drawing.Point(36, 84);
            lblSub.Name = "lblSub";
            lblSub.Size = new System.Drawing.Size(250, 25);
            lblSub.TabIndex = 1;
            lblSub.Text = "-";
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Font = new System.Drawing.Font("Segoe UI", 11F);
            lblDate.ForeColor = System.Drawing.Color.FromArgb(226, 232, 240);
            lblDate.Location = new System.Drawing.Point(1110, 50);
            lblDate.Name = "lblDate";
            lblDate.Size = new System.Drawing.Size(120, 25);
            lblDate.TabIndex = 2;
            lblDate.Text = "-";
            // 
            // pnlBody
            // 
            pnlBody.Controls.Add(tblLayout);
            pnlBody.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlBody.Location = new System.Drawing.Point(0, 120);
            pnlBody.Name = "pnlBody";
            pnlBody.Padding = new System.Windows.Forms.Padding(24);
            pnlBody.Size = new System.Drawing.Size(1280, 560);
            pnlBody.TabIndex = 1;
            // 
            // tblLayout
            // 
            tblLayout.ColumnCount = 1;
            tblLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tblLayout.Controls.Add(tblKpi, 0, 0);
            tblLayout.Controls.Add(tblCharts, 0, 1);
            tblLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            tblLayout.Location = new System.Drawing.Point(24, 24);
            tblLayout.Name = "tblLayout";
            tblLayout.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            tblLayout.RowCount = 2;
            tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 36F));
            tblLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 64F));
            tblLayout.Size = new System.Drawing.Size(1232, 512);
            tblLayout.TabIndex = 0;
            // 
            // tblKpi
            // 
            tblKpi.ColumnCount = 4;
            tblKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tblKpi.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tblKpi.Controls.Add(kpi1, 0, 0);
            tblKpi.Controls.Add(kpi2, 1, 0);
            tblKpi.Controls.Add(kpi3, 2, 0);
            tblKpi.Controls.Add(kpi4, 3, 0);
            tblKpi.Dock = System.Windows.Forms.DockStyle.Fill;
            tblKpi.Location = new System.Drawing.Point(0, 0);
            tblKpi.Margin = new System.Windows.Forms.Padding(0);
            tblKpi.Name = "tblKpi";
            tblKpi.RowCount = 1;
            tblKpi.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tblKpi.Size = new System.Drawing.Size(1232, 179);
            tblKpi.TabIndex = 0;
            // 
            // kpi1
            // 
            kpi1.Controls.Add(acc1);
            kpi1.Controls.Add(lblKpi1Value);
            kpi1.Controls.Add(lblKpi1Title);
            kpi1.Controls.Add(lblKpi1Icon);
            kpi1.Dock = System.Windows.Forms.DockStyle.Fill;
            kpi1.Location = new System.Drawing.Point(3, 3);
            kpi1.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            kpi1.Name = "kpi1";
            kpi1.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            kpi1.Size = new System.Drawing.Size(299, 173);
            kpi1.TabIndex = 0;
            // 
            // acc1
            // 
            acc1.Dock = System.Windows.Forms.DockStyle.Left;
            acc1.Location = new System.Drawing.Point(0, 0);
            acc1.Name = "acc1";
            acc1.Size = new System.Drawing.Size(6, 169);
            acc1.TabIndex = 0;
            // 
            // lblKpi1Value
            // 
            lblKpi1Value.AutoSize = true;
            lblKpi1Value.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            lblKpi1Value.Location = new System.Drawing.Point(22, 26);
            lblKpi1Value.Name = "lblKpi1Value";
            lblKpi1Value.Size = new System.Drawing.Size(80, 60);
            lblKpi1Value.TabIndex = 1;
            lblKpi1Value.Text = "-";
            // 
            // lblKpi1Title
            // 
            lblKpi1Title.AutoSize = true;
            lblKpi1Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblKpi1Title.Location = new System.Drawing.Point(24, 108);
            lblKpi1Title.Name = "lblKpi1Title";
            lblKpi1Title.Size = new System.Drawing.Size(120, 20);
            lblKpi1Title.TabIndex = 2;
            lblKpi1Title.Text = "ŞİRKET";
            // 
            // lblKpi1Icon
            // 
            lblKpi1Icon.AutoSize = true;
            lblKpi1Icon.Font = new System.Drawing.Font("Segoe UI", 18F);
            lblKpi1Icon.Location = new System.Drawing.Point(242, 26);
            lblKpi1Icon.Name = "lblKpi1Icon";
            lblKpi1Icon.Size = new System.Drawing.Size(42, 41);
            lblKpi1Icon.TabIndex = 3;
            lblKpi1Icon.Text = "🏢";
            // 
            // kpi2
            // 
            kpi2.Controls.Add(acc2);
            kpi2.Controls.Add(lblKpi2Value);
            kpi2.Controls.Add(lblKpi2Title);
            kpi2.Controls.Add(lblKpi2Icon);
            kpi2.Dock = System.Windows.Forms.DockStyle.Fill;
            kpi2.Location = new System.Drawing.Point(311, 3);
            kpi2.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            kpi2.Name = "kpi2";
            kpi2.Size = new System.Drawing.Size(299, 173);
            kpi2.TabIndex = 1;
            // 
            // acc2
            // 
            acc2.Dock = System.Windows.Forms.DockStyle.Left;
            acc2.Location = new System.Drawing.Point(0, 0);
            acc2.Name = "acc2";
            acc2.Size = new System.Drawing.Size(6, 169);
            acc2.TabIndex = 0;
            // 
            // lblKpi2Value
            // 
            lblKpi2Value.AutoSize = true;
            lblKpi2Value.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            lblKpi2Value.Location = new System.Drawing.Point(22, 26);
            lblKpi2Value.Name = "lblKpi2Value";
            lblKpi2Value.Size = new System.Drawing.Size(80, 60);
            lblKpi2Value.TabIndex = 1;
            lblKpi2Value.Text = "-";
            // 
            // lblKpi2Title
            // 
            lblKpi2Title.AutoSize = true;
            lblKpi2Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblKpi2Title.Location = new System.Drawing.Point(24, 108);
            lblKpi2Title.Name = "lblKpi2Title";
            lblKpi2Title.Size = new System.Drawing.Size(120, 20);
            lblKpi2Title.TabIndex = 2;
            lblKpi2Title.Text = "KULLANICI";
            // 
            // lblKpi2Icon
            // 
            lblKpi2Icon.AutoSize = true;
            lblKpi2Icon.Font = new System.Drawing.Font("Segoe UI", 18F);
            lblKpi2Icon.Location = new System.Drawing.Point(242, 26);
            lblKpi2Icon.Name = "lblKpi2Icon";
            lblKpi2Icon.Size = new System.Drawing.Size(42, 41);
            lblKpi2Icon.TabIndex = 3;
            lblKpi2Icon.Text = "👤";
            // 
            // kpi3
            // 
            kpi3.Controls.Add(acc3);
            kpi3.Controls.Add(lblKpi3Value);
            kpi3.Controls.Add(lblKpi3Title);
            kpi3.Controls.Add(lblKpi3Icon);
            kpi3.Dock = System.Windows.Forms.DockStyle.Fill;
            kpi3.Location = new System.Drawing.Point(619, 3);
            kpi3.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
            kpi3.Name = "kpi3";
            kpi3.Size = new System.Drawing.Size(299, 173);
            kpi3.TabIndex = 2;
            // 
            // acc3
            // 
            acc3.Dock = System.Windows.Forms.DockStyle.Left;
            acc3.Location = new System.Drawing.Point(0, 0);
            acc3.Name = "acc3";
            acc3.Size = new System.Drawing.Size(6, 169);
            acc3.TabIndex = 0;
            // 
            // lblKpi3Value
            // 
            lblKpi3Value.AutoSize = true;
            lblKpi3Value.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            lblKpi3Value.Location = new System.Drawing.Point(22, 26);
            lblKpi3Value.Name = "lblKpi3Value";
            lblKpi3Value.Size = new System.Drawing.Size(80, 60);
            lblKpi3Value.TabIndex = 1;
            lblKpi3Value.Text = "-";
            // 
            // lblKpi3Title
            // 
            lblKpi3Title.AutoSize = true;
            lblKpi3Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblKpi3Title.Location = new System.Drawing.Point(24, 108);
            lblKpi3Title.Name = "lblKpi3Title";
            lblKpi3Title.Size = new System.Drawing.Size(120, 20);
            lblKpi3Title.TabIndex = 2;
            lblKpi3Title.Text = "ROL";
            // 
            // lblKpi3Icon
            // 
            lblKpi3Icon.AutoSize = true;
            lblKpi3Icon.Font = new System.Drawing.Font("Segoe UI", 18F);
            lblKpi3Icon.Location = new System.Drawing.Point(242, 26);
            lblKpi3Icon.Name = "lblKpi3Icon";
            lblKpi3Icon.Size = new System.Drawing.Size(42, 41);
            lblKpi3Icon.TabIndex = 3;
            lblKpi3Icon.Text = "🔑";
            // 
            // kpi4
            // 
            kpi4.Controls.Add(acc4);
            kpi4.Controls.Add(lblKpi4Value);
            kpi4.Controls.Add(lblKpi4Title);
            kpi4.Controls.Add(lblKpi4Icon);
            kpi4.Dock = System.Windows.Forms.DockStyle.Fill;
            kpi4.Location = new System.Drawing.Point(927, 3);
            kpi4.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            kpi4.Name = "kpi4";
            kpi4.Size = new System.Drawing.Size(302, 173);
            kpi4.TabIndex = 3;
            // 
            // acc4
            // 
            acc4.Dock = System.Windows.Forms.DockStyle.Left;
            acc4.Location = new System.Drawing.Point(0, 0);
            acc4.Name = "acc4";
            acc4.Size = new System.Drawing.Size(6, 169);
            acc4.TabIndex = 0;
            // 
            // lblKpi4Value
            // 
            lblKpi4Value.AutoSize = true;
            lblKpi4Value.Font = new System.Drawing.Font("Segoe UI", 26F, System.Drawing.FontStyle.Bold);
            lblKpi4Value.Location = new System.Drawing.Point(22, 26);
            lblKpi4Value.Name = "lblKpi4Value";
            lblKpi4Value.Size = new System.Drawing.Size(80, 60);
            lblKpi4Value.TabIndex = 1;
            lblKpi4Value.Text = "-";
            // 
            // lblKpi4Title
            // 
            lblKpi4Title.AutoSize = true;
            lblKpi4Title.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            lblKpi4Title.Location = new System.Drawing.Point(24, 108);
            lblKpi4Title.Name = "lblKpi4Title";
            lblKpi4Title.Size = new System.Drawing.Size(120, 20);
            lblKpi4Title.TabIndex = 2;
            lblKpi4Title.Text = "AKTİF OTURUM";
            // 
            // lblKpi4Icon
            // 
            lblKpi4Icon.AutoSize = true;
            lblKpi4Icon.Font = new System.Drawing.Font("Segoe UI", 18F);
            lblKpi4Icon.Location = new System.Drawing.Point(242, 26);
            lblKpi4Icon.Name = "lblKpi4Icon";
            lblKpi4Icon.Size = new System.Drawing.Size(42, 41);
            lblKpi4Icon.TabIndex = 3;
            lblKpi4Icon.Text = "🕐";
            // 
            // tblCharts
            // 
            tblCharts.ColumnCount = 2;
            tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCharts.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tblCharts.Controls.Add(pnlChartRoles, 0, 0);
            tblCharts.Controls.Add(pnlChartCompanies, 1, 0);
            tblCharts.Dock = System.Windows.Forms.DockStyle.Fill;
            tblCharts.Location = new System.Drawing.Point(0, 182);
            tblCharts.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            tblCharts.Name = "tblCharts";
            tblCharts.RowCount = 1;
            tblCharts.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tblCharts.Size = new System.Drawing.Size(1232, 326);
            tblCharts.TabIndex = 1;
            // 
            // pnlChartRoles
            // 
            pnlChartRoles.Controls.Add(lblChartRolesTitle);
            pnlChartRoles.Controls.Add(chartRoles);
            pnlChartRoles.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlChartRoles.Location = new System.Drawing.Point(0, 0);
            pnlChartRoles.Margin = new System.Windows.Forms.Padding(0, 3, 12, 0);
            pnlChartRoles.Name = "pnlChartRoles";
            pnlChartRoles.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            pnlChartRoles.Size = new System.Drawing.Size(604, 326);
            pnlChartRoles.TabIndex = 0;
            // 
            // lblChartRolesTitle
            // 
            lblChartRolesTitle.AutoSize = true;
            lblChartRolesTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblChartRolesTitle.Location = new System.Drawing.Point(18, 12);
            lblChartRolesTitle.Name = "lblChartRolesTitle";
            lblChartRolesTitle.Size = new System.Drawing.Size(200, 25);
            lblChartRolesTitle.TabIndex = 0;
            lblChartRolesTitle.Text = "Rol Dağılımı";
            // 
            // chartRoles
            // 
            chartRoles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right) | System.Windows.Forms.AnchorStyles.Left));
            chartRoles.Location = new System.Drawing.Point(16, 44);
            chartRoles.Name = "chartRoles";
            chartRoles.Size = new System.Drawing.Size(572, 270);
            chartRoles.TabIndex = 1;
            // 
            // pnlChartCompanies
            // 
            pnlChartCompanies.Controls.Add(lblChartCompaniesTitle);
            pnlChartCompanies.Controls.Add(chartCompanies);
            pnlChartCompanies.Dock = System.Windows.Forms.DockStyle.Fill;
            pnlChartCompanies.Location = new System.Drawing.Point(616, 0);
            pnlChartCompanies.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            pnlChartCompanies.Name = "pnlChartCompanies";
            pnlChartCompanies.Padding = new System.Windows.Forms.Padding(16, 12, 16, 12);
            pnlChartCompanies.Size = new System.Drawing.Size(616, 326);
            pnlChartCompanies.TabIndex = 1;
            // 
            // lblChartCompaniesTitle
            // 
            lblChartCompaniesTitle.AutoSize = true;
            lblChartCompaniesTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            lblChartCompaniesTitle.Location = new System.Drawing.Point(18, 12);
            lblChartCompaniesTitle.Name = "lblChartCompaniesTitle";
            lblChartCompaniesTitle.Size = new System.Drawing.Size(260, 25);
            lblChartCompaniesTitle.TabIndex = 0;
            lblChartCompaniesTitle.Text = "Şirket Bazlı Kullanıcılar";
            // 
            // chartCompanies
            // 
            chartCompanies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right) | System.Windows.Forms.AnchorStyles.Left));
            chartCompanies.Location = new System.Drawing.Point(16, 44);
            chartCompanies.Name = "chartCompanies";
            chartCompanies.Size = new System.Drawing.Size(584, 270);
            chartCompanies.TabIndex = 1;
            // 
            // DashboardMdiForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(1280, 680);
            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            MinimumSize = new System.Drawing.Size(1024, 640);
            Name = "DashboardMdiForm";
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlBody.ResumeLayout(false);
            tblLayout.ResumeLayout(false);
            tblKpi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)kpi1).EndInit();
            kpi1.ResumeLayout(false);
            kpi1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpi2).EndInit();
            kpi2.ResumeLayout(false);
            kpi2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpi3).EndInit();
            kpi3.ResumeLayout(false);
            kpi3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)kpi4).EndInit();
            kpi4.ResumeLayout(false);
            kpi4.PerformLayout();
            tblCharts.ResumeLayout(false);
            pnlChartRoles.ResumeLayout(false);
            pnlChartRoles.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartRoles).EndInit();
            pnlChartCompanies.ResumeLayout(false);
            pnlChartCompanies.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartCompanies).EndInit();
            ResumeLayout(false);
        }
    }
}