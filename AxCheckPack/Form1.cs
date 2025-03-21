﻿using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class Form1 : XtraForm
    {
        public Form1()
        {
            InitializeComponent();
        }

        SoundPlayer sp = new SoundPlayer(Properties.Resources.beep11);

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {


                SqlConnection con = new SqlConnection(STM.ConnectionString);
                con.Open();
                this.Text += " [" + con.Database + "] ";
                con.Close();

                LoadLot();
                gridView1.BestFitColumns();

            }
            catch (Exception ex)
            {
                toggleSwitch1.IsOn = false;

                STM.MessageBoxError(ex);
            }
            finally
            {
                STM.SplashScreenManagerManual_Hide();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtScan.Focus();
        }
        private void btPrintFull_Click(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();
                if (gridControl1.DataSource == null) return;

                DataTable dt = gridControl1.DataSource as DataTable;
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    string STMLOTIDSTM = row["STMLOTIDSTM"].ToString();
                    string CODEPACK = row["CODEPACK"].ToString();

                    Report.FormFull frm = new Report.FormFull();
                    //frm.ShowDialog(STMLOTIDSTM, CODEPACK, PrintType.Full);
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
        private void btPrintLabel_Click(object sender, EventArgs e)
        {

        }
        private void btClear_Click(object sender, EventArgs e)
        {
            if (STM.MessageBoxConfirm("ต้องการล้างข้อมูล ใช่หรือไม่ ")) return;

            glLot.EditValue = null;
            gridControl1.DataSource = null;
            gridControl2.DataSource = null;
            gridControl3.DataSource = null;
        }

        private void btReload_Click(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                LoadLot();
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
        private void toggleSwitch1_Toggled(object sender, EventArgs e)
        {
            timer1.Enabled = toggleSwitch1.IsOn;
        }
        private void glLot_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                if (glLot.EditValue == null) return;

                LoadPack();
            }
            catch (Exception ex)
            {
                toggleSwitch1.IsOn = false;

                STM.MessageBoxError(ex);
            }
            finally
            {
                STM.SplashScreenManagerManual_Hide();
            }
        }

        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            txtTotal.Text = gridView1.RowCount.ToString("#,##0");
        }
        private void gridView3_RowCountChanged(object sender, EventArgs e)
        {
            txtPackTotal.Text = gridView3.RowCount.ToString("#,##0");
        }
        private void gridView2_RowCountChanged(object sender, EventArgs e)
        {
            txtPackComplete.Text = gridView2.RowCount.ToString("#,##0");
        }

        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barcode = txtScan.Text.Trim();
                txtScan.Text = string.Empty;
                if (barcode == "") return;
                //if (glLot.EditValue == null)
                //{
                //    STM.MessageBoxError("กรุณาเลือก Lot");
                //    return;
                //}

                try
                {
                    STM.SplashScreenManagerManual_Show();

                    if (glLot.EditValue == null)
                    {
                        DataTable dtLot = STM.QueryData(string.Format(@"SELECT [STMLOTIDSTM]
                                                                        FROM {0}
                                                                        where [CODEPART]='{1}' ", STM.GetTableName, barcode));
                        if (dtLot != null && dtLot.Rows.Count > 0)
                        {
                            glLot.EditValue = dtLot.Rows[0]["STMLOTIDSTM"].ToString();
                        }
                    }

                    if (gridControl1.DataSource == null)
                    {
                        string sql = string.Format(@"select STMLOTIDSTM, RECID, BARCODE, PRODUCTARTICLE, PARTNAME, CODEPART, FINISHLENGTH, FINISHWIDTH, CODEPACK, SHELFNO, CHECKPART,CHECKPACK, CHECKPACKDATE, CHECKPACKUSER
                                                    from {0}
                                                    where STMLOTIDSTM = '{1}' and CODEPACK in (select CODEPACK from {0} where CODEPART='{2}')
                                                    order by PRODUCTARTICLE,PARTNAME,FINISHLENGTH,FINISHWIDTH ", STM.GetTableName, glLot.EditValue.ToString(), barcode);
                        DataTable dtCodePack = STM.QueryData(sql);
                        gridControl1.DataSource = dtCodePack;
                        gridView1.BestFitColumns();
                    }

                    if (gridControl1.DataSource != null)
                    {
                        var pack = from r in (gridControl1.DataSource as DataTable).AsEnumerable()
                                   where r["CODEPART"].ToString() == barcode
                                   select r;
                        if (pack.Count() == 0)
                        {
                            STM.MessageBoxError(string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode));
                        }
                        else
                        {
                            foreach (DataRow row in pack.ToArray())
                            {
                                if (Convert.ToInt32(row["CHECKPACK"]) == 0)
                                {
                                    row["CHECKPACK"] = 1;
                                }
                            }

                            var complate = from r in (gridControl1.DataSource as DataTable).AsEnumerable()
                                           where Convert.ToInt32(r["CHECKPACK"]) == 0
                                           select r;

                            if (complate.Count() == 0)
                            {
                                RefreshPack();

                                DataTable dt = gridControl1.DataSource as DataTable;

                                string CODEPACK = dt.Rows[0]["CODEPACK"].ToString();
                                string STMLOTIDSTM = dt.Rows[0]["STMLOTIDSTM"].ToString();

                                SqlConnection con = new SqlConnection(STM.ConnectionString);
                                SqlCommand cmd = new SqlCommand();

                                con.Open();
                                cmd.Connection = con;

                                cmd.CommandText = string.Format(@"
                                            UPDATE {0}
                                               SET [CHECKPACKUSER] = @CHECKPACKUSER
                                                  ,[CHECKPACKDATE] = GETDATE()
                                                  ,[CHECKPACK] = 1
                                             WHERE STMLOTIDSTM=@STMLOTIDSTM and CODEPACK=@CODEPACK ",STM.GetTableName);
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", STM.GetLoginName));
                                cmd.Parameters.Add(new SqlParameter("STMLOTIDSTM", STMLOTIDSTM));
                                cmd.Parameters.Add(new SqlParameter("CODEPACK", CODEPACK));
                                cmd.ExecuteNonQuery();

                                con.Close();

                                Report.FormFull frm = new Report.FormFull();

                                //frm.ShowDialog(
                                //    dt.Rows[0]["STMLOTIDSTM"].ToString(),
                                //    dt.Rows[0]["CODEPACK"].ToString(),
                                //    "all");
                            }
                        }
                    }
                    else
                    {
                        STM.MessageBoxError(string.Format(" ไม่พบข้อมูล Lot '{0}' ", glLot.EditValue.ToString()));
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

        private void UpdateComplete()
        {

            if (gridControl1.DataSource == null)
            {
                txtComplete.Text = "0";
            }
            else
            {
                var sum = from r in (gridControl1.DataSource as DataTable).AsEnumerable()
                          where Convert.ToInt32(r["CHECKPACK"]) != 0
                          group r["STMLOTIDSTM"].ToString() by new
                          {
                              STMLOTIDSTM = r["STMLOTIDSTM"].ToString(),
                              CHECKPACK = Convert.ToInt32(r["CHECKPACK"])
                          }
                              into g
                              select new
                              {
                                  STMLOTIDSTM = g.Key.STMLOTIDSTM,
                                  CHECKPACK = g.Sum(x => g.Key.CHECKPACK)
                              };
                foreach (var r in sum)
                {
                    txtComplete.Text = r.CHECKPACK.ToString("#,##0");
                }
            }
        }
        private void RefreshPack()
        {
            if (glLot.EditValue == null) return;

            string sql = string.Format(@"declare @STMLOTIDSTM varchar(50)='{1}'

                                        ;with a as (
                                            SELECT distinct [STMLOTIDSTM]
                                                    ,[PRODUCTARTICLE]
                                                    ,[CODEPACK]
                                                    ,[SHELFNO]
		                                            ,sum(checkpart) over(partition by STMLOTIDSTM,CODEPACK) as PartComplete
	                                                ,sum(CHECKPACK) over(partition by STMLOTIDSTM,CODEPACK) as PackComplete
	                                                ,COUNT(*) over(partition by STMLOTIDSTM,CODEPACK) as PartTotal
                                            FROM {0}
                                            where STMLOTIDSTM=@STMLOTIDSTM
                                            ) 

                                        select  STMLOTIDSTM,PRODUCTARTICLE,CODEPACK,SHELFNO,PartComplete,PackComplete,PartTotal,
		                                        case when PartComplete=PartTotal then 1 else 0 end as PartSuccess,
		                                        case when PackComplete=PartTotal then 1 else 0 end as PackSuccess
                                        from a
                                        order by SHELFNO ", STM.GetTableName, glLot.EditValue.ToString());
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            con.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter(sql, con);
            ad.Fill(ds);
            con.Close();

            gridControl3.DataSource = ds.Tables[0];
            gridView3.BestFitColumns();

            glLot.EditValue = null;
        }
        private void LoadPack()
        {
            if (glLot.EditValue == null) return;

            string sql = string.Format(@"declare @STMLOTIDSTM varchar(50)='{1}'

                                       /* ;with a as (
                                        SELECT distinct [STMLOTIDSTM]
                                              ,[PRODUCTARTICLE]
                                              ,[CODEPACK]
                                              ,[SHELFNO]
	                                          ,sum(CHECKPACK) over(partition by STMLOTIDSTM,CODEPACK) as PackComplete
	                                          ,COUNT(*) over(partition by STMLOTIDSTM,CODEPACK) as PartTotal
                                        FROM {0}
                                        where STMLOTIDSTM=@STMLOTIDSTM
                                        ) 

                                        select STMLOTIDSTM,PRODUCTARTICLE,CODEPACK,SHELFNO,PackComplete,PartTotal,case when PackComplete=PartTotal then 1 else 0 end as Complete
                                        from a
                                        where PackComplete=PartTotal
                                        order by SHELFNO */

                                        ;with a as (
                                            SELECT distinct [STMLOTIDSTM]
                                                    ,[PRODUCTARTICLE]
                                                    ,[CODEPACK]
                                                    ,[SHELFNO]
		                                            ,sum(checkpart) over(partition by STMLOTIDSTM,CODEPACK) as PartComplete
	                                                ,sum(CHECKPACK) over(partition by STMLOTIDSTM,CODEPACK) as PackComplete
	                                                ,COUNT(*) over(partition by STMLOTIDSTM,CODEPACK) as PartTotal
                                            FROM {0}
                                            where STMLOTIDSTM=@STMLOTIDSTM
                                            ) 

                                        select  STMLOTIDSTM,PRODUCTARTICLE,CODEPACK,SHELFNO,PartComplete,PackComplete,PartTotal,
		                                        case when PartComplete=PartTotal then 1 else 0 end as PartSuccess,
		                                        case when PackComplete=PartTotal then 1 else 0 end as PackSuccess
                                        from a
                                        order by SHELFNO ", STM.GetTableName, glLot.EditValue.ToString());
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            con.Open();
            DataSet ds = new DataSet();
            SqlDataAdapter ad = new SqlDataAdapter(sql, con);
            ad.Fill(ds);
            con.Close();

            //gridControl2.DataSource = ds.Tables[0];
            //gridControl3.DataSource = ds.Tables[1];

            gridControl3.DataSource = ds.Tables[0];

            gridView2.BestFitColumns();
            gridView3.BestFitColumns();

            gridControl1.DataSource = null;
        }
        private void LoadLot()
        {
            DataTable dtLot = STM.QueryData(string.Format(@"  select STMLOTIDSTM,SUM_CHECKPACK,COUNT_CHECKPACK
                                                from (
			                                                SELECT [STMLOTIDSTM],sum(CHECKPACK) as SUM_CHECKPACK,count(CHECKPACK) as COUNT_CHECKPACK
			                                                FROM {0}
			                                                group by [STMLOTIDSTM]
                                                ) t
                                                where SUM_CHECKPACK <> COUNT_CHECKPACK
                                                order by  [STMLOTIDSTM] ", STM.GetTableName));
            glLot.Properties.DataSource = dtLot;
        }
        private void UpdatePack(DataRow row)
        {
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            SqlCommand cmd = new SqlCommand();

            con.Open();
            cmd.Connection = con;

            cmd.CommandText = string.Format(@"UPDATE {0}
                                   SET [CHECKPACKUSER] = @CHECKPACKUSER
                                      ,[CHECKPACKDATE] = GETDATE()
                                      ,[CHECKPACK] = 1
                                 WHERE RECID=@RECID 

                                if exists (select * from {0} where RECID=@RECID and CHECKPART=0)
                                begin
		                                UPDATE {0}
		                                SET  CHECKPARTUSER = @CHECKPACKUSER
			                                ,CHECKPARTDATE = GETDATE()
			                                ,CHECKPART = 1
		                                WHERE RECID=@RECID
                                end ", STM.GetTableName);
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", STM.GetLoginName));
            cmd.Parameters.Add(new SqlParameter("RECID", row["RECID"]));
            cmd.ExecuteNonQuery();

            con.Close();

        }

        private void gridView3_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.HitInfo.RowHandle < 0) return;

            GridView view = sender as GridView;
            if (e.MenuType == DevExpress.XtraGrid.Views.Grid.GridMenuType.Row)
            {
                int rowHandle = e.HitInfo.RowHandle;

                e.Menu.Items.Clear();
                e.Menu.Items.Add(CreateRowPrintFull(view, rowHandle));
                e.Menu.Items.Add(CreateRowPrintLabel(view, rowHandle));
                e.Menu.Items.Add(CreateRowViewPack(view, rowHandle));
                e.Menu.Items.Add(CreateRowRefresh(view, rowHandle));
            }
        }
        private DXMenuItem CreateRowPrintFull(GridView view, int rowHandle)
        {
            Image img2 = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AxCheckPack.image.Custom-Icon-Design-Pretty-Office-4-Report.ico"));
            DXMenuItem menuItemFilterRow = new DXMenuItem("Report Full", new EventHandler(OnCreatePrintFull_Click), (Image)(new Bitmap(img2, new Size(16, 16))));
            menuItemFilterRow.Tag = new RowInfo(view, rowHandle);
            return menuItemFilterRow;
        }
        private void OnCreatePrintFull_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow row = gridView3.GetFocusedDataRow();
                string STMLOTIDSTM = row["STMLOTIDSTM"].ToString();
                string CODEPACK = row["CODEPACK"].ToString();

                Report.FormFull frm = new Report.FormFull();
                //frm.ShowDialog(STMLOTIDSTM, CODEPACK, PrintType.Full);

            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
        }

        private DXMenuItem CreateRowPrintLabel(GridView view, int rowHandle)
        {
            Image img2 = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AxCheckPack.image.Custom-Icon-Design-Flatastic-5-Product-sale-report.ico"));
            DXMenuItem menuItemFilterRow = new DXMenuItem("Report Label", new EventHandler(OnCreatePrintLabel_Click), (Image)(new Bitmap(img2, new Size(16, 16))));
            menuItemFilterRow.Tag = new RowInfo(view, rowHandle);
            return menuItemFilterRow;
        }
        private void OnCreatePrintLabel_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow row = gridView3.GetFocusedDataRow();
                string STMLOTIDSTM = row["STMLOTIDSTM"].ToString();
                string CODEPACK = row["CODEPACK"].ToString();

                Report.FormFull frm = new Report.FormFull();
                //frm.ShowDialog(STMLOTIDSTM, CODEPACK, PrintType.Label);

            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
        }

        private DXMenuItem CreateRowViewPack(GridView view, int rowHandle)
        {
            Image img2 = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AxCheckPack.image.view.ico"));
            DXMenuItem menuItemFilterRow = new DXMenuItem("View Pack", new EventHandler(OnCreateViewPack_Click), (Image)(new Bitmap(img2, new Size(16, 16))));
            menuItemFilterRow.Tag = new RowInfo(view, rowHandle);
            return menuItemFilterRow;
        }
        private void OnCreateViewPack_Click(object sender, EventArgs e)
        {
            try
            {
                DataRow row = gridView3.GetFocusedDataRow();
                string STMLOTIDSTM = row["STMLOTIDSTM"].ToString();
                string CODEPACK = row["CODEPACK"].ToString();

                //Report.FormFull frm = new Report.FormFull();
                //frm.ShowDialog(STMLOTIDSTM, CODEPACK, "label");

                FormViewPack frm = new FormViewPack();
                frm.ShowDialog(STMLOTIDSTM, CODEPACK);
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
        }

        private DXMenuItem CreateRowRefresh(GridView view, int rowHandle)
        {
            Image img2 = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("AxCheckPack.image.Custom-Icon-Design-Pretty-Office-5-Refresh.ico"));
            DXMenuItem menuItemFilterRow = new DXMenuItem("Refresh", new EventHandler(OnCreateViewRefresh_Click), (Image)(new Bitmap(img2, new Size(16, 16))));
            menuItemFilterRow.Tag = new RowInfo(view, rowHandle);
            return menuItemFilterRow;
        }
        private void OnCreateViewRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                RefreshPack();
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            FormPrintSetting frm = new FormPrintSetting();
            frm.ShowDialog();
        }
    }
}
