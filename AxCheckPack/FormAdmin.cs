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
    public partial class FormAdmin : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public FormAdmin()
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
                    cmd.CommandText = string.Format(@"DELETE FROM [dbo].[AssemblyAdmin] WHERE Seq = '{0}'", row["Seq"].ToString());
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

        private void FormAdmin_Load(object sender, EventArgs e)
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
            DataTable dtLoad = STM.QueryDataProductEngineering(@"SELECT [Seq],[User],[Active] FROM [dbo].[AssemblyAdmin]");

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

                cmd.CommandText = @"DELETE FROM [dbo].[AssemblyAdmin] WHERE User <> ''";
                cmd.ExecuteNonQuery();

                foreach (DataRow row in dt.Rows)
                {
                    cmd.CommandText = string.Format(@"INSERT INTO [dbo].[AssemblyAdmin]
                                                           (Seq
                                                           ,[User]
                                                           ,Active)
                                                     VALUES
                                                           ((SELECT isnull(max(Seq),0)+1 FROM [dbo].[AssemblyAdmin])
                                                           ,@User
                                                           ,@Active)");
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(new SqlParameter("User", row["User"].ToString()));
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
    }
}
