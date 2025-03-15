using AxCheckPack.Report;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class FormPrintSetting : XtraForm
    {
        public FormPrintSetting()
        {
            InitializeComponent();
        }

        private void FormPrintSetting_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("printer"));

                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    dt.Rows.Add(printer);
                }

                glFull.Properties.DataSource = dt;
                glLabel.Properties.DataSource = dt;

                PrinterSetting printsetting = STM.Print;

                checkLable.Checked = printsetting.PrintLabelActive;
                checkFull.Checked = printsetting.PrintFullActive;
                radioGroup1.EditValue = printsetting.LabelSize;

                glFull.EditValue = printsetting.PrintFull;
                glLabel.EditValue = printsetting.PrintLabel;
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
            finally
            {
                STM.SplashScreenManagerManual_Hide();
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
                SqlCommand cmd = new SqlCommand();
                SqlTransaction tr = null;

                try
                {

                    con.Open();
                    cmd.Connection = con;
                    tr = con.BeginTransaction();

                    cmd.CommandText = string.Format( @" delete FROM [dbo].[AxCheckConfig]
                                                        where [ComputerName]='{0}' ",STM.GetComputerName);
                    cmd.Parameters.Clear();
                    cmd.Transaction = tr;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = @"INSERT INTO [dbo].[AxCheckConfig]
                                               ([ComputerName]
                                               ,[PrintFull]
                                               ,[PrintLabel]
                                               ,[PrintFullActive]
                                               ,[PrintLabelActive]
                                               ,LabelSize
                                               ,[ModifyDate])
                                         VALUES
                                               (@ComputerName
                                               ,@PrintFull
                                               ,@PrintLabel
                                               ,@PrintFullActive
                                               ,@PrintLabelActive
                                               ,@LabelSize
                                               ,GETDATE()) ";
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("ComputerName", STM.GetComputerName));
                    cmd.Parameters.Add(new SqlParameter("PrintFull", glFull.EditValue.ToString()));
                    cmd.Parameters.Add(new SqlParameter("PrintLabel", glLabel.EditValue.ToString()));
                    cmd.Parameters.Add(new SqlParameter("LabelSize", radioGroup1.EditValue == null ? "small" : radioGroup1.EditValue.ToString()));
                    cmd.Parameters.Add(new SqlParameter("PrintFullActive", checkFull.Checked == true ? "1" : "0"));
                    cmd.Parameters.Add(new SqlParameter("PrintLabelActive", checkLable.Checked == true ? "1" : "0"));
                    cmd.Transaction = tr;
                    cmd.ExecuteNonQuery();

                    tr.Commit();

                    STM.MessageBoxInformation("Complete");

                    this.DialogResult = DialogResult.OK;
                }
                catch (Exception ex)
                {
                    tr.Rollback();
                    STM.MessageBoxError(ex);
                }
                finally
                {
                    con.Close();
                }

                
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
            finally
            {
                STM.SplashScreenManagerManual_Hide();
            }
        }

       
    }
}
