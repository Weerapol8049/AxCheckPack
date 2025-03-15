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
    public partial class FormAdminExc : Form
    {
        int admin = 0;
        public FormAdminExc()
        {
            InitializeComponent();
        }

        internal void ShowDialog(int _admin)
        {
            admin = _admin;
            this.ShowDialog();
        }

        private void FormAdminExc_Load(object sender, EventArgs e)
        {
            if (admin == 1)
                layoutControlItem4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
            else
                layoutControlItem4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Text = textEdit1.EditValue.ToString();
            this.Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if(!STM.MessageBoxConfirm("ยืนยัน เปลี่ยนรหัส"))
            {

            }
        }

       
    }
}
