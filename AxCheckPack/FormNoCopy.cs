using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class FormNoCopy : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public FormNoCopy()
        {
            InitializeComponent();
        }

        private void FormNoCopy_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();
                loaddata();
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

        private void loaddata()
        {
            string sql = string.Format(@"select Seq, 
	                                        Name, 
	                                        cast(LOCK as bit) Lock, 
	                                        cast(LOCKPRINTPACK as bit) LockPrintPack,  
	                                        cast(CODEPART as bit) CodePart  
                                        from dbo.STMROOMCATEGORY  order by SEQ");
            DataTable dt = STM.QueryData(sql);

            gridControl1.DataSource = dt;
            gridView1.BestFitColumns();
        }

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            DataTable dt = gridControl1.DataSource as DataTable;

            int max = Convert.ToInt32(dt.AsEnumerable().Max(row => row["Seq"]));

            DataRow dr = dt.NewRow();

            dr["Seq"] = max + 1;
            dr["Name"] = "";
            dr["Lock"] = false;
            dt.Rows.Add(dr);

            gridControl1.DataSource = dt;
            gridView1.BestFitColumns();
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.FocusedRowHandle >= 0)
            {
                if (STM.MessageBoxConfirm("Confirm Delete")) return;
                try
                {
                    gridView1.DeleteRow(gridView1.FocusedRowHandle);
                }
                catch (Exception ex)
                {
                    STM.MessageBoxError(ex.Message);
                }
            }
        }

        private void barButtonItem3_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {

        }

        private void barButtonItem4_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (STM.MessageBoxConfirm("Confirm Save")) return;
            gridView1.PostEditor();

            DataTable dt = gridControl1.DataSource as DataTable;
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            SqlCommand cmd = new SqlCommand();

            try
            {
                con.Open();
                cmd.Connection = con;
                foreach (DataRow row in dt.Rows)
                {
                    cmd.CommandText = string.Format(@"UPDATE dbo.STMROOMCATEGORY
                                                        SET LOCK = @LOCK, LOCKPRINTPACK = @LOCKPRINTPACK, CODEPART = @CODEPART
                                                        WHERE NAME = @NAME");
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("NAME", row["NAME"].ToString()));
                    cmd.Parameters.Add(new SqlParameter("LOCK", row["LOCK"]));
                    cmd.Parameters.Add(new SqlParameter("CODEPART", row["CODEPART"]));
                    cmd.Parameters.Add(new SqlParameter("LOCKPRINTPACK", row["LOCKPRINTPACK"]));
                    cmd.ExecuteNonQuery();
                }

                STM.MessageBoxInformation("Save Complete");

                loaddata();
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
            finally
            {
                con.Close();
            }
        }

        private void btnUser_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FormAdmin frm = new FormAdmin();
            frm.ShowDialog();
        }

        private void btnInputBlocker_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            FormInputBlocker frm = new FormInputBlocker();
            frm.ShowDialog();
        }

    }
}
