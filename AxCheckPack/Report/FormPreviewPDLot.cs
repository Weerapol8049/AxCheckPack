using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack.Report
{
    public partial class FormPreviewPDLot : Form
    {
        public FormPreviewPDLot()
        {
            InitializeComponent();
        }

        public void ShowDialog(CrystalReportLabelPDLot source)
        {
            crystalReportViewer1.ReportSource = source;
            this.ShowDialog();
        }
    }
}
