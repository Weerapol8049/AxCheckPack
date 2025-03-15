using DevExpress.XtraEditors;
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
    public partial class FormViewPack : XtraForm
    {
        public FormViewPack()
        {
            InitializeComponent();
        }

        private void FormViewPack_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                SqlConnection con = new SqlConnection(STM.ConnectionString);
                con.Open();
                this.Text += " [" + con.Database + "] ";
                con.Close();
                gridView1.BestFitColumns();
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
        public void ShowDialog(string STMLOTIDSTM, string CODEPACK)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();
                LoadData(STMLOTIDSTM, CODEPACK);

                this.ShowDialog();
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
        private void LoadData(string STMLOTIDSTM, string CODEPACK)
        {
            

            DataTable dtLot = STM.QueryData(string.Format(@"
                                        SELECT STMLOTIDSTM,
                                                    ROOMCATEGORY,
                                                    PRODUCTARTICLE,
                                                    PARTNAME,
                                                    CODEPART,
                                                    FINISHLENGTH,
                                                    FINISHWIDTH,
                                                    SHELFNO,
                                                    CHECKPART,
                                                    CHECKPARTDATE,
                                                    CHECKPARTUSER,
                                                    CHECKPACK,
                                                    CHECKPACKDATE,
                                                    CHECKPACKUSER
                                        FROM {0}
                                        where STMLOTIDSTM='{1}' and CODEPACK='{2}' order by CODEPART"
                                    , STM.GetTableName, STMLOTIDSTM, CODEPACK));

            //WK#1.n 20250106
            if (dtLot.Rows.Count > 0)
            {
                string sql = string.Format(@"select cast(CODEPART as bit) CodePart  
                                        from dbo.STMROOMCATEGORY 
                                        where Name = '{0}'", dtLot.Rows[0]["ROOMCATEGORY"].ToString());
                int hideCodePack = Convert.ToInt32(STM.QueryData_ExecuteScalar(sql));

                if (hideCodePack > 0)
                    gridColumn4.Visible = false;// Code part
                else
                    gridColumn4.Visible = true;// Code part
            }
            

            gridControl1.DataSource = dtLot;
            gridView1.BestFitColumns();

            // glLot.Properties.DataSource = dtLot;
        }

        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            txtTotal.Text = gridView1.RowCount.ToString("#,##0");
        }
    }
}
