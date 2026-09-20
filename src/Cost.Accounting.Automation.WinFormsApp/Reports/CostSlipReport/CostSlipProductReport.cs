using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace Cost.Accounting.Automation.WinFormsApp.Forms.CostSlips
{
    public partial class CostSlipProductReport : DevExpress.XtraReports.UI.XtraReport
    {
        public CostSlipProductReport()
        {
            InitializeComponent();
        }

        [System.ComponentModel.DesignerSerializationVisibility(
            System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string SlipTypeTitle
        {
            get => xrLabel1.Text;
            set => xrLabel1.Text = value;
        }
    }
}
