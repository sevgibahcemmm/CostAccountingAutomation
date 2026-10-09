namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class UpdateAvailableForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateAvailableForm));
            this.accentStrip = new System.Windows.Forms.Panel();
            this.card = new System.Windows.Forms.Panel();
            this._statusLabel = new System.Windows.Forms.Label();
            this.subLabel = new System.Windows.Forms.Label();
            this._percentLabel = new System.Windows.Forms.Label();
            this._progressBar = new System.Windows.Forms.ProgressBar();
            this.appName = new System.Windows.Forms.Label();
            this.title = new System.Windows.Forms.Label();
            this.version = new System.Windows.Forms.Label();
            this.hint = new System.Windows.Forms.Label();
            this.SuspendLayout();
            //
            // accentStrip
            //
            this.accentStrip.Dock = System.Windows.Forms.DockStyle.Top;
            this.accentStrip.Location = new System.Drawing.Point(0, 0);
            this.accentStrip.Name = "accentStrip";
            this.accentStrip.Size = new System.Drawing.Size(540, 4);
            this.accentStrip.TabIndex = 0;
            //
            // card
            //
            this.card.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.card.Controls.Add(this._statusLabel);
            this.card.Controls.Add(this.subLabel);
            this.card.Controls.Add(this._percentLabel);
            this.card.Controls.Add(this._progressBar);
            this.card.Location = new System.Drawing.Point(24, 114);
            this.card.Name = "card";
            this.card.Size = new System.Drawing.Size(492, 172);
            this.card.TabIndex = 5;
            //
            // _statusLabel
            //
            this._statusLabel.Font = new System.Drawing.Font(this.Font.FontFamily, 12F, System.Drawing.FontStyle.Bold);
            this._statusLabel.Location = new System.Drawing.Point(0, 34);
            this._statusLabel.Name = "_statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(492, 26);
            this._statusLabel.TabIndex = 0;
            this._statusLabel.Text = "Kurulum dosyası indiriliyor...";
            this._statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // subLabel
            //
            this.subLabel.Font = new System.Drawing.Font(this.Font.FontFamily, 9F);
            this.subLabel.Location = new System.Drawing.Point(0, 62);
            this.subLabel.Name = "subLabel";
            this.subLabel.Size = new System.Drawing.Size(492, 20);
            this.subLabel.TabIndex = 1;
            this.subLabel.Text = "Bu işlem birkaç dakika sürebilir.";
            this.subLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // _percentLabel
            //
            this._percentLabel.Font = new System.Drawing.Font(this.Font.FontFamily, 9F, System.Drawing.FontStyle.Bold);
            this._percentLabel.Location = new System.Drawing.Point(16, 96);
            this._percentLabel.Name = "_percentLabel";
            this._percentLabel.Size = new System.Drawing.Size(52, 18);
            this._percentLabel.TabIndex = 2;
            this._percentLabel.Text = "0%";
            this._percentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // _progressBar
            //
            this._progressBar.Location = new System.Drawing.Point(72, 100);
            this._progressBar.Name = "_progressBar";
            this._progressBar.Size = new System.Drawing.Size(404, 14);
            this._progressBar.TabIndex = 3;
            //
            // appName
            //
            this.appName.AutoSize = true;
            this.appName.Font = new System.Drawing.Font(this.Font.FontFamily, 8.5F);
            this.appName.Location = new System.Drawing.Point(24, 18);
            this.appName.Name = "appName";
            this.appName.Size = new System.Drawing.Size(0, 15);
            this.appName.TabIndex = 1;
            this.appName.Text = "Maliyet Muhasebesi Otomasyonu";
            //
            // title
            //
            this.title.AutoSize = true;
            this.title.Font = new System.Drawing.Font(this.Font.FontFamily, 13F, System.Drawing.FontStyle.Bold);
            this.title.Location = new System.Drawing.Point(24, 36);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(0, 22);
            this.title.TabIndex = 2;
            this.title.Text = "Yeni sürüm yükleniyor...";
            //
            // version
            //
            this.version.AutoSize = true;
            this.version.Font = new System.Drawing.Font(this.Font.FontFamily, 9.5F);
            this.version.Location = new System.Drawing.Point(24, 64);
            this.version.Name = "version";
            this.version.Size = new System.Drawing.Size(0, 16);
            this.version.TabIndex = 3;
            //
            // hint
            //
            this.hint.AutoSize = true;
            this.hint.Font = new System.Drawing.Font(this.Font.FontFamily, 8.5F);
            this.hint.Location = new System.Drawing.Point(24, 84);
            this.hint.Name = "hint";
            this.hint.Size = new System.Drawing.Size(0, 15);
            this.hint.TabIndex = 4;
            this.hint.Text = "Kurulum tamamlandığında yeni sürüm otomatik açılır.";
            //
            // UpdateAvailableForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(540, 336);
            this.Controls.Add(this.appName);
            this.Controls.Add(this.title);
            this.Controls.Add(this.version);
            this.Controls.Add(this.hint);
            this.Controls.Add(this.card);
            this.Controls.Add(this.accentStrip);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateAvailableForm";
            this.ShowInTaskbar = true;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Güncelleme";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel accentStrip;
        private System.Windows.Forms.Panel card;
        private System.Windows.Forms.Label _statusLabel;
        private System.Windows.Forms.Label subLabel;
        private System.Windows.Forms.Label _percentLabel;
        private System.Windows.Forms.ProgressBar _progressBar;
        private System.Windows.Forms.Label appName;
        private System.Windows.Forms.Label title;
        private System.Windows.Forms.Label version;
        private System.Windows.Forms.Label hint;
    }
}