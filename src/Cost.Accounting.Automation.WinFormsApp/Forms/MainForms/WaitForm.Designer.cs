namespace Cost.Accounting.Automation.WinFormsApp.Forms.MainForms
{
    partial class WaitForm
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
            progressPanel1 = new DevExpress.XtraWaitForm.ProgressPanel();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            //
            // progressPanel1
            //
            progressPanel1.AnimationSpeed = 1;
            progressPanel1.AnimationToTextDistance = 8;
            progressPanel1.Appearance.BackColor = Color.Transparent;
            progressPanel1.Appearance.Options.UseBackColor = true;
            // Tam punto değerleri kullanılır: 13F / 9.5F gibi kesirli boyutlar %125 ve
            // %150 ölçeklemede metni yuvarlayıp bulanıklaştırıyordu.
            progressPanel1.AppearanceCaption.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            progressPanel1.AppearanceCaption.Options.UseFont = true;
            progressPanel1.AppearanceDescription.Font = new Font("Segoe UI", 9F);
            progressPanel1.AppearanceDescription.Options.UseFont = true;
            progressPanel1.CaptionToDescriptionDistance = 6;
            progressPanel1.Dock = DockStyle.Fill;
            progressPanel1.ImageHorzOffset = 20;
            progressPanel1.Location = new Point(28, 26);
            progressPanel1.Margin = new Padding(0);
            progressPanel1.Name = "progressPanel1";
            progressPanel1.Size = new Size(344, 108);
            progressPanel1.TabIndex = 0;
            progressPanel1.Text = "Lütfen Bekleyin...";
            //
            // tableLayoutPanel1
            //
            tableLayoutPanel1.AutoSize = true;
            tableLayoutPanel1.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel1.BackColor = Color.Transparent;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(progressPanel1, 0, 0);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.Padding = new Padding(28, 26, 28, 26);
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new Size(400, 160);
            tableLayoutPanel1.TabIndex = 1;
            //
            // WaitForm
            //
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(400, 160);
            Controls.Add(tableLayoutPanel1);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "WaitForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Text = "Lütfen Bekleyin";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DevExpress.XtraWaitForm.ProgressPanel progressPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
