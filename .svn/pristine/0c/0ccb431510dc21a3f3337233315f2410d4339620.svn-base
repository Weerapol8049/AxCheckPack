using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class FormMessageBox : XtraForm
    {
        public FormMessageBox()
        {
            InitializeComponent();
        }

        private void FormMessageBox_Load(object sender, EventArgs e)
        {

        }

        public void ShowDialog(string msg)
        {
            labelControl1.Text = msg;
            this.ShowDialog();
            simpleButton1.Focus();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
