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
    public partial class FormInputBlocker : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public FormInputBlocker()
        {
            InitializeComponent();
        }

        private void btnDelete_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gridView1.FocusedRowHandle >= 0)
            {
                if (STM.MessageBoxConfirm("Confirm Delete")) return;
                try
                {
                    var row = gridView1.GetFocusedDataRow();

                    SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
                    SqlCommand cmd = new SqlCommand();

                    con.Open();
                    cmd.Connection = con;
                    cmd.CommandText = string.Format(@"DELETE FROM pd.InputBlocker WHERE Seq = '{0}'", row["Seq"].ToString());
                    cmd.ExecuteNonQuery();

                    loaddata();
                    STM.MessageBoxConfirm("Delete completed.");
                }
                catch (Exception ex)
                {
                    STM.MessageBoxError(ex.Message);
                }
            }
        }

        private void loaddata()
        {
            DataTable dtLoad = STM.QueryDataProductEngineering(@"SELECT [Seq],[ComputerName],[Active] FROM pd.InputBlocker");

            gridControl1.DataSource = dtLoad;
            gridView1.BestFitColumns();
        }

        private void btnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (STM.MessageBoxConfirm("Confirm Save")) return;
            gridView1.PostEditor();

            DataTable dt = gridControl1.DataSource as DataTable;
            SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
            SqlCommand cmd = new SqlCommand();

            try
            {
                con.Open();
                cmd.Connection = con;

                cmd.CommandText = @"DELETE FROM pd.InputBlocker WHERE ComputerName <> ''";
                cmd.ExecuteNonQuery();

                foreach (DataRow row in dt.Rows)
                {
                    cmd.CommandText = string.Format(@"INSERT INTO pd.InputBlocker
                                                           (Seq
                                                           ,[ComputerName]
                                                           ,Active)
                                                     VALUES
                                                           ((SELECT isnull(max(Seq),0)+1 FROM pd.InputBlocker)
                                                           ,@ComputerName
                                                           ,@Active)");
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("ComputerName", row["ComputerName"].ToString()));
                    cmd.Parameters.Add(new SqlParameter("Active", row["Active"]));
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

        private void gridView1_InitNewRow(object sender, DevExpress.XtraGrid.Views.Grid.InitNewRowEventArgs e)
        {
            DevExpress.XtraGrid.Views.Grid.GridView view = sender as DevExpress.XtraGrid.Views.Grid.GridView;

            view.SetRowCellValue(e.RowHandle, view.Columns["Active"], false);
        }

        private void FormInputBlocker_Load(object sender, EventArgs e)
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
    }
}
