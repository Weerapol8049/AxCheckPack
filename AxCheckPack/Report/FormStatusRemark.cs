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
    public partial class FormStatusRemark : Form
    {
        

        public FormStatusRemark()
        {
            InitializeComponent();
        }

        public void ShowDialog(string formName)
        {
            this.Text = "Print " + formName;
            this.ShowDialog();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            List<Process> list = new List<Process>();
            foreach (var item in checkedListBoxProcess.CheckedItems)
            {
                var value = ((DevExpress.XtraEditors.Controls.ListBoxItem)(item)).Value;
                list.Add(new Process
                {
                    ProcessChk = Convert.ToInt32(value)
                });
            }

            STM.ProcessSelected = list;
            STM.PrintCopy = Convert.ToInt32(txtPrint.EditValue);

            this.Text = (textEditRemark.EditValue == null ? "" : textEditRemark.EditValue.ToString());
        }

        private void textEditRemark_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.DialogResult = DialogResult.OK;
                this.Text = textEditRemark.EditValue == null ? "" : textEditRemark.EditValue.ToString();
            }
        }

        private void FormStatusRemark_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.DialogResult != DialogResult.OK)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
