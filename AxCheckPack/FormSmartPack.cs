using AxCheckPack.Report;
using AxCheckPack.Report.NewPack;
using DevExpress.XtraEditors;
using OnBarcode.Barcode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class FormSmartPack : XtraForm
    {
        string STMLOTIDSTM = "";
        string RoomCategory = "ห้องครัว";
        string RoomCategoryLock = "";
        int status_copy = 0;
        int admin = 0;

        SoundPlayer sp = new SoundPlayer(Properties.Resources.beep11);
        SoundPlayer spError = new SoundPlayer(Properties.Resources.Alarme);
        SoundPlayer spNewPack = new SoundPlayer(Properties.Resources.notification);
        SoundPlayer spComplete = new SoundPlayer(Properties.Resources.ragnarok_online_level_up_sound);

        private Stopwatch stopwatch = new Stopwatch();
        private StringBuilder scannedData = new StringBuilder();

        public FormSmartPack()
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                InitializeComponent();

                this.KeyPress += new KeyPressEventHandler(FormSmartPack_KeyPress);
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

        public void ShowDialog(string RoomCategory, int admin)
        {
            this.admin = admin;
            this.RoomCategory = RoomCategory;
            this.Text += " " + RoomCategory;

            status_copy = STM.LockSetting(2, RoomCategory);//wk#1.n 20230519

            this.ShowDialog();
        }

        private void FormSmartPack_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                gridViewAll.BestFitColumns();
                rdPack.Properties.Items.Clear();
                FormNoLot.RefreshFormText(this);

                int block = Convert.ToInt32(STM.QueryData_ExecuteScalarProductEngineering(string.Format(@"SELECT [Seq]
                                      ,[ComputerName]
                                      ,[Active]
                                  FROM [pd].[InputBlocker]
                                  WHERE ComputerName = '{0}' AND Active = 1", STM.GetComputerName)));

                if (block > 0)
                    txtScan.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();//ปิดคลิ๊กขวา copy paste
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            txtScan.Focus();
        }

        private void toggleSwitch1_Toggled(object sender, EventArgs e)
        {
            timer1.Enabled = toggleSwitch1.IsOn;
        }

        private void gridViewAll_RowCountChanged(object sender, EventArgs e)
        {
            txtTotal.Text = gridViewAll.RowCount.ToString("#,##0");
        }

        private void FormSmartPack_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (txtScan.Properties.ReadOnly)
            {
                if (!stopwatch.IsRunning || stopwatch.ElapsedMilliseconds > 65)
                {
                    // หากระยะเวลาระหว่าง KeyPress เกิน 50ms แสดงว่าอาจเป็นการพิมพ์มือ
                    string filteredBarcode = RemoveOddIndexCharacters(scannedData.ToString());

                    //MessageBox.Show("Clear : " + filteredBarcode, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    scannedData.Clear();
                }

                stopwatch.Restart();  // รีเซ็ตเวลา

                if (e.KeyChar == (char)Keys.Enter) // เมื่อเครื่องสแกนส่ง Enter มา
                {
                    if (scannedData.ToString() != "")
                    {
                        //timerClearScan.Enabled = false;
                        //timerClearScan.Enabled = true;

                        string filteredBarcode = RemoveOddIndexCharacters(scannedData.ToString());//PD66000025774-PD02PT01
                        FormScan(filteredBarcode);

                        scannedData.Clear(); // ล้างข้อมูลที่สะสมไว้
                        e.Handled = true; // ป้องกัน Enter จากไปก่อผลที่อื่น
                        stopwatch.Stop();
                    }
                }
                else
                {
                    scannedData.Append(e.KeyChar); // สะสมค่าบาร์โค้ดที่ถูกสแกน
                }
            }
        }

        //WK#1.n 20250313
        static string RemoveOddIndexCharacters(string input)
        {
            //"PPPDD6666000000002255777744--PPDD0022PPTT0011"

            StringBuilder result = new StringBuilder();
            string result_ = "";

            for (int i = 0; i < input.Length; i++)
            {
                // เก็บอักขระที่ตำแหน่งคี่
                if (i % 2 != 0)
                {
                    result.Append(input[i]); // เพิ่มอักขระปกติ
                }
            }

            //replace PD เพื่อป้องกันการตัดข้อความพลาด
            result_ = result.ToString().Replace("P", "#");
            result_ = result_.Replace("D", "@");

            result_ = result_.Replace("#", "P");
            result_ = result_.Replace("@", "D");

            return result_;
        }

        //WK#1.n 20250313
        private void FormScan(string barcode)
        {
            bool newPack = STM.CheckBarcodeChangePalette(barcode);
            if (newPack)
            {
                CreatePack();
                spNewPack.Play();
                return;
            }

            if (STM.GetComputerName != "STMM-IT-N-06-PC")
                sp.Play();

            try
            {
                STM.SplashScreenManagerManual_Show();
                gridViewAll.PostEditor();
                if (gridControlAll.DataSource == null || gridViewAll.RowCount == 0)
                {
                    //                        string sql = string.Format(@"   
                    //                                                    SELECT
                    //                                                        CAST(0 AS bit) AS SPLITPACK,
                    //                                                        ROOMCATEGORY,
                    //                                                        PRODUCTARTICLE,PARTNAME,[CODEPART],CODEPACK,[CUTRITEMATERIALCODE],FINISHLENGTH,FINISHWIDTH,RECID,0 as Receive,'' as PACKQUANTITYSEQUENCETOTAL,CONVERT(varchar,RECID) as ID,'0' as ParentID,STMLOTIDSTM,CODEPACKORG
                    //                                                    FROM [dbo].[STMSMARTPDPARTBACKDATA]
                    //                                                    where CODEPACK in (select distinct CODEPACK from dbo.STMSMARTPDPARTBACKDATA where CODEPART='{0}') ", barcode);

                    string sql = string.Format(@"SELECT
	                                                    CAST(0 AS bit) AS SPLITPACK,
	                                                    ROOMCATEGORY,ORDERNUMBER,
	                                                    PRODUCTARTICLE,PARTNAME,part.[CODEPART],part.CODEPACK,[CUTRITEMATERIALCODE],FINISHLENGTH,FINISHWIDTH,RECID,
	                                                    0 as Receive,'' as PACKQUANTITYSEQUENCETOTAL,CONVERT(varchar,RECID) as ID,'0' as ParentID,STMLOTIDSTM,CODEPACKORG
                                                    FROM {0} part
                                                    INNER JOIN (select CODEPACK, CODEPART from {0}) A
	                                                    ON A.CODEPACK = part.CODEPACK
                                                    WHERE A.CODEPART = '{1}'
                                                    ORDER BY PARTNAME", STM.GetTableName, barcode);

                    DataTable dt = STM.QueryData(sql);
                    gridControlAll.DataSource = dt;
                    gridViewAll.BestFitColumns();

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        STMLOTIDSTM = dt.Rows[0]["STMLOTIDSTM"].ToString();
                        RoomCategoryLock = dt.Rows[0]["ROOMCATEGORY"].ToString();

                        //status_copy = STM.LockSetting(2, RoomCategoryLock);
                        //int hideCodePart = STM.LockSetting(3, RoomCategoryLock);

                        int hideCodePart = 0;
                        int blockScan = 0;
                        DataTable dtLock = STM.LockSetting(RoomCategoryLock);

                        foreach (DataRow row in dtLock.Rows)
                        {
                            if (admin == 0)
                                status_copy = Convert.ToInt32(row["Lock"]);

                            hideCodePart = Convert.ToInt32(row["CodePart"]);
                            blockScan = Convert.ToInt32(row["LockScan"]);
                        }

                        //if (admin == 0 && !string.IsNullOrEmpty(RoomCategoryLock))
                        //    status_copy = STM.LockSetting(2, RoomCategoryLock);
                        //int hideCodePack = STM.LockSetting(3, RoomCategoryLock);

                        if (blockScan > 0)
                            txtScan.Properties.ReadOnly = true;
                        else
                            txtScan.Properties.ReadOnly = false;

                        if (admin == 0 && hideCodePart == 1)
                        {
                            gridColumn3.Visible = false;//CodePart
                        }
                        else
                        {
                            gridColumn3.Visible = true;//CodePart
                        }

                        //FormNoLot nolot = new FormNoLot();
                        //nolot.changeRoomCategory(RoomCategoryLock);
                    }

                    if (treeList1.DataSource == null)
                    {
                        DataTable dtTree = dt.Clone();
                        treeList1.DataSource = dtTree;
                    }
                }

                var p = from r in (gridControlAll.DataSource as DataTable).AsEnumerable()
                        where r["CODEPART"].ToString().ToLower() == barcode.ToLower()
                        select r;
                if (p.Count() == 0)
                {
                    notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                    notifyIcon1.Visible = true;
                    notifyIcon1.ShowBalloonTip(5000);
                    spError.Play();

                    //STM.MessageBoxError("ไม่พบบาร์โคด " + barcode + "");
                }
                else
                {
                    DataTable dtTree = treeList1.DataSource as DataTable;

                    string pack = "0";
                    if (rdPack.Properties.Items.Count == 0 || rdPack.EditValue == null)
                    {
                        rdPack.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem("1", "Pack 1"));
                        pack = "1";
                        rdPack.EditValue = "1";

                        DataRow row = dtTree.NewRow();
                        row["PRODUCTARTICLE"] = "Pack 1";
                        row["PARTNAME"] = "";
                        row["CODEPART"] = "";
                        row["CODEPACK"] = "";
                        row["CUTRITEMATERIALCODE"] = "";
                        row["FINISHLENGTH"] = 0;
                        row["FINISHWIDTH"] = 0;
                        row["RECID"] = 0;
                        row["Receive"] = 0;
                        row["PACKQUANTITYSEQUENCETOTAL"] = "";
                        row["ID"] = pack;
                        row["ParentID"] = "0";
                        dtTree.Rows.Add(row);
                    }
                    else
                    {
                        pack = rdPack.EditValue.ToString();
                    }

                    foreach (DataRow row in p.ToArray())
                    {
                        if (row["CODEPACK"].ToString().Contains("_"))
                            STM.MessageBoxError("มีข้อมูลการแบ่งแพ็คแล้ว");
                        else if (Convert.ToInt32(row["Receive"]) == 0)
                        {
                            var pack_split = (from a in dtTree.AsEnumerable()
                                              where a["SPLITPACK"].ToString() == "True" && a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == pack
                                              select a);
                            //มี error Column 'SPLITPACK' does not belog to table. กรณีที่มีการกดปุ่ม new pack ไว้ก่อนแล้วยิงทีหลัง
                            if (pack_split.ToArray().Count() > 0)
                            {
                                STM.MessageBoxError("เกิดข้อผิดพลาดข้อมูลแพ็ค");
                            }
                            else
                            {
                                row["PACKQUANTITYSEQUENCETOTAL"] = pack;
                                row["ParentID"] = pack;
                                row["Receive"] = 1;

                                dtTree.ImportRow(row);
                            }
                        }
                    }

                    treeList1.ExpandAll();
                    gridViewAll.PostEditor();

                    if (gridControlAll.DataSource != null)
                    {
                        DataTable dtPackComplete = gridControlAll.DataSource as DataTable;
                        var pack_complete = from r in dtPackComplete.AsEnumerable()
                                            where Convert.ToInt32(r["Receive"]) == 0
                                            select r;
                        if (pack_complete.Count() == 0)
                        {
                            var PackSeqDistincts = (from r in dtPackComplete.AsEnumerable()
                                                    group r by Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"] == DBNull.Value || r["PACKQUANTITYSEQUENCETOTAL"].ToString() == "" ? "0" : r["PACKQUANTITYSEQUENCETOTAL"]) into g
                                                    select new
                                                    {
                                                        PACKQUANTITYSEQUENCETOTAL = g.Key,
                                                        Count = g.Count()
                                                    }
                                                    ).OrderBy(r => r.PACKQUANTITYSEQUENCETOTAL);
                            int index = 0;
                            foreach (var PackSeqDistinct in PackSeqDistincts)
                            {
                                if (PackSeqDistinct.Count > 0)
                                {
                                    Console.WriteLine(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL);
                                    index++;
                                    var RunSeq = from r in dtPackComplete.AsEnumerable()
                                                 where Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"]) == Convert.ToInt32(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL)
                                                 select r;
                                    foreach (DataRow row in RunSeq.ToArray())
                                    {
                                        row["PACKQUANTITYSEQUENCETOTAL"] = index.ToString();
                                    }
                                }
                            }

                            SqlConnection con = new SqlConnection(STM.ConnectionString);
                            SqlCommand cmd = new SqlCommand();
                            List<string> lsCodePack = new List<string>();

                            try
                            {
                                con.Open();
                                cmd.Connection = con;

                                DateTime CHECKPACKDATE = new DateTime();
                                string CHECKPACKUSER = STM.GetLoginName;

                                cmd.CommandText = "select getdate() ";
                                cmd.Parameters.Clear();
                                CHECKPACKDATE = Convert.ToDateTime(cmd.ExecuteScalar());

                                List<DataTable> tables = dtPackComplete.AsEnumerable()
                                               .GroupBy(row => new
                                               {
                                                   Lot = row.Field<string>("STMLOTIDSTM"),
                                                   CodePack = row.Field<string>("CODEPACK"),
                                                   PackSeq = row.Field<string>("PACKQUANTITYSEQUENCETOTAL")
                                               }).Select(g => g.CopyToDataTable()).ToList();

                                foreach (var row in tables)
                                {
                                    string recid = "";
                                    string CodePack = "";
                                    int seq = 0;

                                    foreach (DataRow item in row.Rows)
                                    {
                                        seq = Convert.ToInt32(item["PACKQUANTITYSEQUENCETOTAL"].ToString());
                                        CodePack = item["CODEPACK"].ToString();

                                        recid += string.Format("'{0}',", item["RECID"].ToString());

                                        if (item["CODEPACKORG"].ToString().Trim() == "")
                                        {
                                            CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
                                            CodePack = CodePack + "_" + seq.ToString();
                                            lsCodePack.Add(CodePack);
                                        }
                                        else
                                        {
                                            CodePack = item["CODEPACKORG"].ToString();
                                            CodePack = CodePack + "_" + seq.ToString();
                                            lsCodePack.Add(CodePack);
                                        }

                                    }

                                    recid = recid.Remove(recid.Length - 1);

                                    cmd.CommandText = string.Format(@"update {0}
                                                                                        set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
                                                                                            CODEPACK=@CODEPACK,
                                                                                            CHECKPACK=1,
                                                                                            CHECKPACKDATE=@CHECKPACKDATE,
                                                                                            CHECKPACKUSER=@CHECKPACKUSER
                                                                                        where RECID in ({1}) ", STM.GetTableName, recid);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", seq + "/" + index.ToString()));
                                    cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
                                    cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
                                    cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
                                    cmd.ExecuteNonQuery();
                                }

                                //                                        foreach (DataRow row in dtPackComplete.Rows)
                                //                                        {
                                //                                            string CodePack = row["CODEPACK"].ToString();

                                //                                            if (row["CODEPACKORG"].ToString().Trim() == "")
                                //                                            {
                                //                                                CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
                                //                                                CodePack = CodePack + "_" + row["PACKQUANTITYSEQUENCETOTAL"].ToString();
                                //                                                lsCodePack.Add(CodePack);
                                //                                            }
                                //                                            else
                                //                                            {
                                //                                                CodePack = row["CODEPACKORG"].ToString();
                                //                                                CodePack = CodePack + "_" + row["PACKQUANTITYSEQUENCETOTAL"].ToString();
                                //                                                lsCodePack.Add(CodePack);
                                //                                            }

                                //                                            cmd.CommandText = @"update [dbo].[STMSMARTPDPARTBACKDATA]
                                //                                                            set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
                                //                                                                CODEPACK=@CODEPACK,
                                //                                                                CHECKPACK=1,
                                //                                                                CHECKPACKDATE=@CHECKPACKDATE,
                                //                                                                CHECKPACKUSER=@CHECKPACKUSER
                                //                                                            where RECID=@RECID ";
                                //                                            cmd.Parameters.Clear();
                                //                                            cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", row["PACKQUANTITYSEQUENCETOTAL"].ToString() + "/" + index.ToString()));
                                //                                            cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
                                //                                            cmd.Parameters.Add(new SqlParameter("RECID", row["RECID"]));
                                //                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
                                //                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
                                //                                            cmd.ExecuteNonQuery();
                                //                                        }
                            }
                            catch (Exception ex)
                            {
                                STM.MessageBoxError(ex);
                            }
                            finally
                            {
                                con.Close();
                            }
                            spComplete.Play();
                            PrintReport(lsCodePack);

                            Clear();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
            finally
            {
                DataTable dtGrid = gridControlAll.DataSource as DataTable;
                DataTable dtTree = treeList1.DataSource as DataTable;

                if (dtGrid != null)
                {
                    var count_complete = (from a in dtGrid.AsEnumerable()
                                          where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == ""
                                          select a);

                    if (count_complete.Count() == 0)
                        btnSplitPack.Enabled = true;
                    else
                        btnSplitPack.Enabled = false;
                }


                STM.SplashScreenManagerManual_Hide();
            }
        }


        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (!txtScan.Properties.ReadOnly)
            {
                //timerClearScan.Enabled = false;
                //timerClearScan.Enabled = true;

                //if (e.Control && e.KeyCode == Keys.V)
                //{
                //    txtScan.EditValue = "";
                //}
                //else 
                if (e.KeyCode == Keys.Enter)
                {
                    //timerClearScan.Enabled = false;

                    string barcode = txtScan.Text.Trim();
                    txtScan.Text = string.Empty;
                    if (barcode == "") return;

                    FormScan(barcode);//WK#1.n 20250313

                    #region 20250313
                    //                bool newPack = STM.CheckBarcodeChangePalette(barcode);
                    //                if (newPack)
                    //                {
                    //                    CreatePack();
                    //                    spNewPack.Play();
                    //                    return;
                    //                }

                    //                sp.Play();

                    //                try
                    //                {
                    //                    STM.SplashScreenManagerManual_Show();
                    //                    gridViewAll.PostEditor();
                    //                    if (gridControlAll.DataSource == null || gridViewAll.RowCount == 0)
                    //                    {
                    ////                        string sql = string.Format(@"   
                    ////                                                    SELECT
                    ////                                                        CAST(0 AS bit) AS SPLITPACK,
                    ////                                                        ROOMCATEGORY,
                    ////                                                        PRODUCTARTICLE,PARTNAME,[CODEPART],CODEPACK,[CUTRITEMATERIALCODE],FINISHLENGTH,FINISHWIDTH,RECID,0 as Receive,'' as PACKQUANTITYSEQUENCETOTAL,CONVERT(varchar,RECID) as ID,'0' as ParentID,STMLOTIDSTM,CODEPACKORG
                    ////                                                    FROM [dbo].[STMSMARTPDPARTBACKDATA]
                    ////                                                    where CODEPACK in (select distinct CODEPACK from dbo.STMSMARTPDPARTBACKDATA where CODEPART='{0}') ", barcode);

                    //                        string sql = string.Format(@"SELECT
                    //	                                                    CAST(0 AS bit) AS SPLITPACK,
                    //	                                                    ROOMCATEGORY,ORDERNUMBER,
                    //	                                                    PRODUCTARTICLE,PARTNAME,part.[CODEPART],part.CODEPACK,[CUTRITEMATERIALCODE],FINISHLENGTH,FINISHWIDTH,RECID,
                    //	                                                    0 as Receive,'' as PACKQUANTITYSEQUENCETOTAL,CONVERT(varchar,RECID) as ID,'0' as ParentID,STMLOTIDSTM,CODEPACKORG
                    //                                                    FROM {0} part
                    //                                                    INNER JOIN (select CODEPACK, CODEPART from {0}) A
                    //	                                                    ON A.CODEPACK = part.CODEPACK
                    //                                                    WHERE A.CODEPART = '{1}'
                    //                                                    ORDER BY PARTNAME", STM.GetTableName, barcode);

                    //                        DataTable dt = STM.QueryData(sql);
                    //                        gridControlAll.DataSource = dt;
                    //                        gridViewAll.BestFitColumns();

                    //                        if (dt != null && dt.Rows.Count > 0)
                    //                        {
                    //                            STMLOTIDSTM = dt.Rows[0]["STMLOTIDSTM"].ToString();
                    //                            RoomCategoryLock = dt.Rows[0]["ROOMCATEGORY"].ToString();

                    //                            status_copy = STM.LockSetting(2, RoomCategoryLock);

                    //                            int hideCodePart = STM.LockSetting(3, RoomCategoryLock);

                    //                            if (admin == 0 && hideCodePart == 1)
                    //                            {
                    //                                gridColumn3.Visible = false;//CodePart
                    //                            }
                    //                            else
                    //                            {
                    //                                gridColumn3.Visible = true;//CodePart
                    //                            }

                    //                            //FormNoLot nolot = new FormNoLot();
                    //                            //nolot.changeRoomCategory(RoomCategoryLock);
                    //                        }

                    //                        if (treeList1.DataSource == null)
                    //                        {
                    //                            DataTable dtTree = dt.Clone();
                    //                            treeList1.DataSource = dtTree;
                    //                        }
                    //                    }

                    //                        var p = from r in (gridControlAll.DataSource as DataTable).AsEnumerable()
                    //                                where r["CODEPART"].ToString().ToLower() == barcode.ToLower()
                    //                                select r;
                    //                        if (p.Count() == 0)
                    //                        {
                    //                            notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                    //                            notifyIcon1.Visible = true;
                    //                            notifyIcon1.ShowBalloonTip(5000);
                    //                            spError.Play();

                    //                            //STM.MessageBoxError("ไม่พบบาร์โคด " + barcode + "");
                    //                        }
                    //                        else
                    //                        {
                    //                            DataTable dtTree = treeList1.DataSource as DataTable;

                    //                            string pack = "0";
                    //                            if (rdPack.Properties.Items.Count == 0 || rdPack.EditValue == null)
                    //                            {
                    //                                rdPack.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem("1", "Pack 1"));
                    //                                pack = "1";
                    //                                rdPack.EditValue = "1";

                    //                                DataRow row = dtTree.NewRow();
                    //                                row["PRODUCTARTICLE"] = "Pack 1";
                    //                                row["PARTNAME"] = "";
                    //                                row["CODEPART"] = "";
                    //                                row["CODEPACK"] = "";
                    //                                row["CUTRITEMATERIALCODE"] = "";
                    //                                row["FINISHLENGTH"] = 0;
                    //                                row["FINISHWIDTH"] = 0;
                    //                                row["RECID"] = 0;
                    //                                row["Receive"] = 0;
                    //                                row["PACKQUANTITYSEQUENCETOTAL"] = "";
                    //                                row["ID"] = pack;
                    //                                row["ParentID"] = "0";
                    //                                dtTree.Rows.Add(row);
                    //                            }
                    //                            else
                    //                            {
                    //                                pack = rdPack.EditValue.ToString();
                    //                            }

                    //                            foreach (DataRow row in p.ToArray())
                    //                            {
                    //                                if (row["CODEPACK"].ToString().Contains("_"))
                    //                                    STM.MessageBoxError("มีข้อมูลการแบ่งแพ็คแล้ว");
                    //                                else if (Convert.ToInt32(row["Receive"]) == 0)
                    //                                {
                    //                                    var pack_split = (from a in dtTree.AsEnumerable()
                    //                                                        where a["SPLITPACK"].ToString() == "True" && a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == pack
                    //                                                        select a);
                    //                                    //มี error Column 'SPLITPACK' does not belog to table. กรณีที่มีการกดปุ่ม new pack ไว้ก่อนแล้วยิงทีหลัง
                    //                                    if (pack_split.ToArray().Count() > 0)
                    //                                    {
                    //                                        STM.MessageBoxError("เกิดข้อผิดพลาดข้อมูลแพ็ค");
                    //                                    }
                    //                                    else
                    //                                    {
                    //                                        row["PACKQUANTITYSEQUENCETOTAL"] = pack;
                    //                                        row["ParentID"] = pack;
                    //                                        row["Receive"] = 1;

                    //                                        dtTree.ImportRow(row);
                    //                                    }
                    //                                }
                    //                            }

                    //                            treeList1.ExpandAll();
                    //                            gridViewAll.PostEditor();

                    //                            if (gridControlAll.DataSource != null)
                    //                            {
                    //                                DataTable dtPackComplete = gridControlAll.DataSource as DataTable;
                    //                                var pack_complete = from r in dtPackComplete.AsEnumerable()
                    //                                                    where Convert.ToInt32(r["Receive"]) == 0
                    //                                                    select r;
                    //                                if (pack_complete.Count() == 0)
                    //                                {
                    //                                    var PackSeqDistincts = (from r in dtPackComplete.AsEnumerable()
                    //                                                            group r by Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"] == DBNull.Value || r["PACKQUANTITYSEQUENCETOTAL"].ToString() == "" ? "0" : r["PACKQUANTITYSEQUENCETOTAL"]) into g
                    //                                                            select new
                    //                                                            {
                    //                                                                PACKQUANTITYSEQUENCETOTAL = g.Key,
                    //                                                                Count = g.Count()
                    //                                                            }
                    //                                                            ).OrderBy(r => r.PACKQUANTITYSEQUENCETOTAL);
                    //                                    int index = 0;
                    //                                    foreach (var PackSeqDistinct in PackSeqDistincts)
                    //                                    {
                    //                                        if (PackSeqDistinct.Count > 0)
                    //                                        {
                    //                                            Console.WriteLine(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL);
                    //                                            index++;
                    //                                            var RunSeq = from r in dtPackComplete.AsEnumerable()
                    //                                                         where Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"]) == Convert.ToInt32(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL)
                    //                                                         select r;
                    //                                            foreach (DataRow row in RunSeq.ToArray())
                    //                                            {
                    //                                                row["PACKQUANTITYSEQUENCETOTAL"] = index.ToString();
                    //                                            }
                    //                                        }
                    //                                    }

                    //                                    SqlConnection con = new SqlConnection(STM.ConnectionString);
                    //                                    SqlCommand cmd = new SqlCommand();
                    //                                    List<string> lsCodePack = new List<string>();

                    //                                    try
                    //                                    {
                    //                                        con.Open();
                    //                                        cmd.Connection = con;

                    //                                        DateTime CHECKPACKDATE = new DateTime();
                    //                                        string CHECKPACKUSER = STM.GetLoginName;

                    //                                        cmd.CommandText = "select getdate() ";
                    //                                        cmd.Parameters.Clear();
                    //                                        CHECKPACKDATE = Convert.ToDateTime(cmd.ExecuteScalar());

                    //                                        List<DataTable> tables = dtPackComplete.AsEnumerable()
                    //                                                       .GroupBy(row => new
                    //                                                       {
                    //                                                           Lot = row.Field<string>("STMLOTIDSTM"),
                    //                                                           CodePack = row.Field<string>("CODEPACK"),
                    //                                                           PackSeq = row.Field<string>("PACKQUANTITYSEQUENCETOTAL")
                    //                                                       }).Select(g => g.CopyToDataTable()).ToList();

                    //                                        foreach (var row in tables)
                    //                                        {
                    //                                            string recid = "";
                    //                                            string CodePack = "";
                    //                                            int seq = 0;

                    //                                            foreach (DataRow item in row.Rows)
                    //                                            {
                    //                                                seq = Convert.ToInt32(item["PACKQUANTITYSEQUENCETOTAL"].ToString());
                    //                                                CodePack = item["CODEPACK"].ToString();

                    //                                                recid += string.Format("'{0}',", item["RECID"].ToString());

                    //                                                if (item["CODEPACKORG"].ToString().Trim() == "")
                    //                                                {
                    //                                                    CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
                    //                                                    CodePack = CodePack + "_" + seq.ToString();
                    //                                                    lsCodePack.Add(CodePack);
                    //                                                }
                    //                                                else
                    //                                                {
                    //                                                    CodePack = item["CODEPACKORG"].ToString();
                    //                                                    CodePack = CodePack + "_" + seq.ToString();
                    //                                                    lsCodePack.Add(CodePack);
                    //                                                }

                    //                                            }

                    //                                            recid = recid.Remove(recid.Length - 1);

                    //                                            cmd.CommandText = string.Format(@"update {0}
                    //                                                                                        set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
                    //                                                                                            CODEPACK=@CODEPACK,
                    //                                                                                            CHECKPACK=1,
                    //                                                                                            CHECKPACKDATE=@CHECKPACKDATE,
                    //                                                                                            CHECKPACKUSER=@CHECKPACKUSER
                    //                                                                                        where RECID in ({1}) ", STM.GetTableName, recid);
                    //                                            cmd.Parameters.Clear();
                    //                                            cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", seq + "/" + index.ToString()));
                    //                                            cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
                    //                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
                    //                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
                    //                                            cmd.ExecuteNonQuery();
                    //                                        }

                    ////                                        foreach (DataRow row in dtPackComplete.Rows)
                    ////                                        {
                    ////                                            string CodePack = row["CODEPACK"].ToString();

                    ////                                            if (row["CODEPACKORG"].ToString().Trim() == "")
                    ////                                            {
                    ////                                                CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
                    ////                                                CodePack = CodePack + "_" + row["PACKQUANTITYSEQUENCETOTAL"].ToString();
                    ////                                                lsCodePack.Add(CodePack);
                    ////                                            }
                    ////                                            else
                    ////                                            {
                    ////                                                CodePack = row["CODEPACKORG"].ToString();
                    ////                                                CodePack = CodePack + "_" + row["PACKQUANTITYSEQUENCETOTAL"].ToString();
                    ////                                                lsCodePack.Add(CodePack);
                    ////                                            }

                    ////                                            cmd.CommandText = @"update [dbo].[STMSMARTPDPARTBACKDATA]
                    ////                                                            set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
                    ////                                                                CODEPACK=@CODEPACK,
                    ////                                                                CHECKPACK=1,
                    ////                                                                CHECKPACKDATE=@CHECKPACKDATE,
                    ////                                                                CHECKPACKUSER=@CHECKPACKUSER
                    ////                                                            where RECID=@RECID ";
                    ////                                            cmd.Parameters.Clear();
                    ////                                            cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", row["PACKQUANTITYSEQUENCETOTAL"].ToString() + "/" + index.ToString()));
                    ////                                            cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
                    ////                                            cmd.Parameters.Add(new SqlParameter("RECID", row["RECID"]));
                    ////                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
                    ////                                            cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
                    ////                                            cmd.ExecuteNonQuery();
                    ////                                        }
                    //                                    }
                    //                                    catch (Exception ex)
                    //                                    {
                    //                                        STM.MessageBoxError(ex);
                    //                                    }
                    //                                    finally
                    //                                    {
                    //                                        con.Close();
                    //                                    }
                    //                                    spComplete.Play();
                    //                                    PrintReport(lsCodePack);

                    //                                    Clear();
                    //                                }
                    //                            }
                    //                    }
                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    STM.MessageBoxError(ex);
                    //                }
                    //                finally
                    //                {
                    //                    DataTable dtGrid = gridControlAll.DataSource as DataTable;
                    //                    DataTable dtTree = treeList1.DataSource as DataTable;

                    //                    if (dtGrid != null)
                    //                    {
                    //                        var count_complete = (from a in dtGrid.AsEnumerable()
                    //                                              where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == ""
                    //                                              select a);

                    //                        if (count_complete.Count() == 0)
                    //                            btnSplitPack.Enabled = true;
                    //                        else
                    //                            btnSplitPack.Enabled = false;
                    //                    }


                    //                    STM.SplashScreenManagerManual_Hide();
                    //                }
                    #endregion
                }
                //txtScan.Text = "";
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();
                if (STM.MessageBoxConfirm("Confirm Clear")) return;

                Clear();
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

        public void Clear()
        {
            rdPack.Properties.Items.Clear();
            gridControlAll.DataSource = null;
            treeList1.DataSource = null;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            CreatePack();
        }

        private void CreatePack()
        {
            try
            {

                STM.SplashScreenManagerManual_Show();
                int max = 0;

                if (rdPack.Properties.Items.Count == 0)
                {
                    max = 1;
                }
                else
                {
                    var m = from r in rdPack.Properties.Items
                            select new { Value = Convert.ToInt32(r.Value) };
                    if (m.Count() > 0)
                    {
                        max = m.Max(r => r.Value) + 1;
                    }
                }

                rdPack.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(max.ToString(), "Pack " + max.ToString()));
                rdPack.EditValue = max.ToString();

                if (treeList1.DataSource == null)
                {
                    string sql = string.Format(@"   SELECT PRODUCTARTICLE,PARTNAME,[CODEPART],CODEPACK,[CUTRITEMATERIALCODE],FINISHLENGTH,FINISHWIDTH,RECID,0 as Receive,ISNULL(PACKQUANTITYSEQUENCE,'1/1') as PACKQUANTITYSEQUENCETOTAL,CONVERT(varchar,RECID) as ID,'0' as ParentID
                                                    FROM {0}
                                                    where 1=2  ", STM.GetTableName);
                    DataTable dt = STM.QueryData(sql);
                    treeList1.DataSource = dt.Clone();
                }


                DataTable dtTree = treeList1.DataSource as DataTable;

                DataRow row = dtTree.NewRow();
                row["PRODUCTARTICLE"] = "Pack " + max.ToString();
                row["PARTNAME"] = "";
                row["CODEPART"] = "";
                row["CODEPACK"] = "";
                row["CUTRITEMATERIALCODE"] = "";
                row["FINISHLENGTH"] = 0;
                row["FINISHWIDTH"] = 0;
                row["RECID"] = 0;
                row["Receive"] = 0;
                row["PACKQUANTITYSEQUENCETOTAL"] = "";
                row["ID"] = max.ToString();
                row["ParentID"] = "0";
                dtTree.Rows.Add(row);

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

        private void PrintReport(List<string> lsCodePack)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                var p = (from r in lsCodePack
                         select r).Distinct().OrderBy(r => r);

                FormFull frm = new FormFull();

                foreach (string CodePack in p.ToArray())
                {
                    //ดึง หมวดงาน ใหม่ กรณีมีที่การแก้ไข หมวดงานที่ axprogress
                    //string roomcategoryPint = STM.QueryData_ExecuteScalar(
                    //    string.Format("SELECT DISTINCT ROOMCATEGORY FROM STMSMARTPDPARTBACKDATA WHERE STMLOTIDSTM = '{0}' AND CODEPACK = '{1}' AND ROOMCATEGORY <> ''", STMLOTIDSTM, CodePack)).ToString();

                    frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategoryLock, PrintType.Full, DateTime.Now);
                    if (RoomCategoryLock.Substring(0, 7).ToLower() == "Compack".ToLower() || RoomCategoryLock.Substring(0, 3).ToLower() == "DIY".ToLower())//wk#.n 20220526
                    {
                        RoomCategoryLock = RoomCategoryLock.Substring(0, 7).ToLower() == "compack" ? "Compack" : "DIY";
                        if (RoomCategoryLock == "Compack")
                            frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategoryLock, PrintType.Compack, DateTime.Now);//wk#.n 20220526
                        else if (RoomCategoryLock == "DIY")
                            frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategoryLock, PrintType.DIY, DateTime.Now);//wk#.n 20220526
                    }
                    else
                        frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategoryLock, PrintType.Label, DateTime.Now);

                    //frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategory, PrintType.Full, DateTime.Now);
                    //if (RoomCategory == "Compack" || RoomCategory == "DIY")//wk#.n 20220526
                    //{
                    //    if (RoomCategory == "Compack")
                    //        frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategory, PrintType.Compack, DateTime.Now);//wk#.n 20220526
                    //    else if (RoomCategory == "DIY")
                    //        frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategory, PrintType.DIY, DateTime.Now);//wk#.n 20220526
                    //}
                    //else
                    //    frm.ShowDialog(STMLOTIDSTM, CodePack, RoomCategory, PrintType.Label, DateTime.Now);
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

        //WK#1.n 20230223
        private void gridViewAll_ShowingEditor(object sender, CancelEventArgs e)
        {
            //int hideCodePart = STM.LockSetting(3, RoomCategoryLock);
            //status_copy = STM.LockSetting(2, RoomCategoryLock);
            int hideCodePart = 0;
            int blockScan = 0;
            int blockPrint = 0;

            DataTable dtLock = STM.LockSetting(RoomCategoryLock);

            foreach (DataRow row in dtLock.Rows)
            {
                status_copy = Convert.ToInt32(row["Lock"]);
                blockPrint = Convert.ToInt32(row["LockPrintPack"]);
                hideCodePart = Convert.ToInt32(row["CodePart"]);
                blockScan = Convert.ToInt32(row["LockScan"]);
            }

            if (admin == 0 && hideCodePart == 1)
            {
                gridColumn3.Visible = false;//CodePart
            }
            else
            {
                gridColumn3.Visible = true;//CodePart
            }

            if (blockScan == 1)
                txtScan.Properties.ReadOnly = true;
            else
                txtScan.Properties.ReadOnly = false;

            if (admin == 0 && status_copy == 1)
            {
                //e.Cancel = true;
                gridColumn3.OptionsColumn.AllowEdit = false;
                treeListColumn3.OptionsColumn.AllowEdit = false;
                //Clipboard.Clear();
            }
            else
            {
                gridColumn3.OptionsColumn.AllowEdit = true;
                treeListColumn3.OptionsColumn.AllowEdit = true;
            }
                
        }

        //WK#1.n 20230418
        private void btnSplitPack_Click(object sender, EventArgs e)
        {
            //var stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();
            try
            {
                STM.SplashScreenManagerManual_Show();

                if (gridControlAll.DataSource != null)
                {
                    DataTable dtPackComplete = gridControlAll.DataSource as DataTable;
                    var pack_complete = from r in dtPackComplete.AsEnumerable()
                                        where Convert.ToBoolean(r["SPLITPACK"]) == true
                                        select r;
                    if (pack_complete.Count() > 0)
                    {
                        var PackSeqDistincts = (from r in dtPackComplete.AsEnumerable()
                                                group r by Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"] == DBNull.Value || r["PACKQUANTITYSEQUENCETOTAL"].ToString() == "" ? "0" : r["PACKQUANTITYSEQUENCETOTAL"]) into g
                                                select new
                                                {
                                                    PACKQUANTITYSEQUENCETOTAL = g.Key,
                                                    Count = g.Count()
                                                }
                                                ).OrderBy(r => r.PACKQUANTITYSEQUENCETOTAL);
                        int index = 0;
                        foreach (var PackSeqDistinct in PackSeqDistincts)
                        {
                            if (PackSeqDistinct.Count > 0)
                            {
                                Console.WriteLine(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL);
                                index++;
                                var RunSeq = from r in dtPackComplete.AsEnumerable()
                                             where Convert.ToInt32(r["PACKQUANTITYSEQUENCETOTAL"]) == Convert.ToInt32(PackSeqDistinct.PACKQUANTITYSEQUENCETOTAL)
                                             select r;
                                foreach (DataRow rowseq in RunSeq.ToArray())
                                {
                                    rowseq["PACKQUANTITYSEQUENCETOTAL"] = index.ToString();
                                }
                            }
                        }

                        SqlConnection con = new SqlConnection(STM.ConnectionString);
                        SqlCommand cmd = new SqlCommand();
                        List<string> lsCodePart = new List<string>();

                        try
                        {
                            con.Open();
                            cmd.Connection = con;

                            DateTime CHECKPACKDATE = new DateTime();
                            string CHECKPACKUSER = STM.GetLoginName;

                            cmd.CommandText = "select getdate() ";
                            cmd.Parameters.Clear();
                            CHECKPACKDATE = Convert.ToDateTime(cmd.ExecuteScalar());

                            List<DataTable> tables = dtPackComplete.AsEnumerable()
                                                       .GroupBy(row => new
                                                       {
                                                           Lot = row.Field<string>("STMLOTIDSTM"),
                                                           CodePack = row.Field<string>("CODEPACK"),
                                                           PackSeq = row.Field<string>("PACKQUANTITYSEQUENCETOTAL")
                                                       }).Select(g => g.CopyToDataTable()).ToList();

                            foreach (var row in tables)
                            {
                                string recid = "";
                                string CodePack = "";
                                int seq = 0;
                                int checkpack = 0;

                                foreach (DataRow item in row.Rows)
                                {
                                    seq = Convert.ToInt32(item["PACKQUANTITYSEQUENCETOTAL"].ToString());
                                    CodePack = item["CODEPACK"].ToString();
                                    string CodePart = item["CODEPART"].ToString();

                                    recid += string.Format("'{0}',", item["RECID"].ToString());
                                    if (item["CODEPACKORG"].ToString().Trim() == "")
                                    {
                                        CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
                                        CodePack = CodePack + "_" + item["PACKQUANTITYSEQUENCETOTAL"].ToString();
                                    }
                                    else
                                    {
                                        CodePack = item["CODEPACKORG"].ToString();
                                        CodePack = CodePack + "_" + item["PACKQUANTITYSEQUENCETOTAL"].ToString();
                                    }

                                    if (Convert.ToInt32(item["SPLITPACK"]) == 0 && Convert.ToInt32(item["Receive"]) == 1)
                                    {
                                        lsCodePart.Add(CodePack);
                                        checkpack = 1;
                                    }
                                }

                                recid = recid.Remove(recid.Length - 1);

                                cmd.CommandText = string.Format(@"update {0}
                                                                    set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
                                                                        CODEPACK=@CODEPACK,
                                                                        CHECKPACK=@CHECKPACK,
                                                                        CHECKPACKDATE=@CHECKPACKDATE,
                                                                        CHECKPACKUSER=@CHECKPACKUSER
                                                                    where RECID in ({1}) ", STM.GetTableName, recid);
                                cmd.Parameters.Clear();
                                cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", seq + "/" + index.ToString()));
                                cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
                                //cmd.Parameters.Add(new SqlParameter("RECID", rowcomplete["RECID"]));
                                cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
                                cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
                                cmd.Parameters.Add(new SqlParameter("CHECKPACK", checkpack));
                                cmd.ExecuteNonQuery();
                            }


//                            foreach (DataRow rowcomplete in dtPackComplete.Rows)
//                            {
//                                int seq = Convert.ToInt32(rowcomplete["PACKQUANTITYSEQUENCETOTAL"].ToString());
//                                string CodePack = rowcomplete["CODEPACK"].ToString();
//                                string CodePart = rowcomplete["CODEPART"].ToString();
//                                int checkpack = 0;

//                                if (rowcomplete["CODEPACKORG"].ToString().Trim() == "")
//                                {
//                                    CodePack = CodePack.Length >= 19 ? CodePack.Substring(0, 19) : CodePack;
//                                    CodePack = CodePack + "_" + rowcomplete["PACKQUANTITYSEQUENCETOTAL"].ToString();
//                                }
//                                else
//                                {
//                                    CodePack = rowcomplete["CODEPACKORG"].ToString();
//                                    CodePack = CodePack + "_" + rowcomplete["PACKQUANTITYSEQUENCETOTAL"].ToString();
//                                }

//                                if (Convert.ToInt32(rowcomplete["SPLITPACK"]) == 0 && Convert.ToInt32(rowcomplete["Receive"]) == 1)
//                                {
//                                    lsCodePart.Add(CodePack);
//                                    checkpack = 1;
//                                }

//                                cmd.CommandText = @"update [dbo].[STMSMARTPDPARTBACKDATA]
//                                                            set [PACKQUANTITYSEQUENCE]=@PACKQUANTITYSEQUENCETOTAL,
//                                                                CODEPACK=@CODEPACK,
//                                                                CHECKPACK=@CHECKPACK,
//                                                                CHECKPACKDATE=@CHECKPACKDATE,
//                                                                CHECKPACKUSER=@CHECKPACKUSER
//                                                            where RECID=@RECID ";
//                                cmd.Parameters.Clear();
//                                cmd.Parameters.Add(new SqlParameter("PACKQUANTITYSEQUENCETOTAL", rowcomplete["PACKQUANTITYSEQUENCETOTAL"].ToString() + "/" + index.ToString()));
//                                cmd.Parameters.Add(new SqlParameter("CODEPACK", CodePack));
//                                cmd.Parameters.Add(new SqlParameter("RECID", rowcomplete["RECID"]));
//                                cmd.Parameters.Add(new SqlParameter("CHECKPACKDATE", CHECKPACKDATE));
//                                cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", CHECKPACKUSER));
//                                cmd.Parameters.Add(new SqlParameter("CHECKPACK", checkpack));
//                                cmd.ExecuteNonQuery();
//                            }
                        }
                        catch (Exception ex)
                        {
                            STM.MessageBoxError(ex);
                        }
                        finally
                        {
                            con.Close();
                        }

                        //PrintReportSplitPack(lsCodePart);
                        PrintReport(lsCodePart);

                        Clear();

                        btnSplitPack.Enabled = false;

                    }
                }
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
            finally
            {
                STM.SplashScreenManagerManual_Hide();
                //stopwatch.Stop();

                //TimeSpan timeTaken = stopwatch.Elapsed;
                //string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");
                //STM.MessageBoxInformation(foo);
            }
        }
        //WK#1.n 20230418
        //private void PrintReportSplitPack(List<string> lsCodePart)
        //{
        //    try
        //    {
        //        STM.SplashScreenManagerManual_Show();

        //        //var p = (from r in lsCodePart select r).Distinct().OrderBy(r => r);
        //        string partsplit = "";
        //        foreach (string codepart in lsCodePart.ToArray())
        //        {
        //            partsplit += string.Format("'{0}',", codepart);
        //        }
        //        partsplit = partsplit.Remove(partsplit.Length -1);
        //        FormFull frm = new FormFull();

        //        if (!string.IsNullOrEmpty(partsplit))
        //        {
        //            frm.ShowDialog(STMLOTIDSTM, "", RoomCategory, PrintType.Full, DateTime.Now, partsplit);
        //            if (RoomCategory == "Compack" || RoomCategory == "DIY")
        //            {
        //                if (RoomCategory == "Compack")
        //                    frm.ShowDialog(STMLOTIDSTM, "", RoomCategory, PrintType.Compack, DateTime.Now, partsplit);
        //                else if (RoomCategory == "DIY")
        //                    frm.ShowDialog(STMLOTIDSTM, "", RoomCategory, PrintType.DIY, DateTime.Now, partsplit);
        //            }
        //            else
        //                frm.ShowDialog(STMLOTIDSTM, "", RoomCategory, PrintType.Label, DateTime.Now, partsplit);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        STM.MessageBoxError(ex);
        //    }
        //    finally
        //    {
        //        STM.SplashScreenManagerManual_Hide();
        //    }
        //}

        private void gridViewAll_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
           
        }

        private void gridViewAll_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.Caption == "Pack")
            {
                bool pack = Convert.ToBoolean(e.Value);

                string packgrid = gridViewAll.GetRowCellValue(gridViewAll.FocusedRowHandle, gridViewAll.Columns["SPLITPACK"]).ToString();
                string recid = gridViewAll.GetRowCellValue(gridViewAll.FocusedRowHandle, gridViewAll.Columns["RECID"]).ToString();

                STM.SplashScreenManagerManual_Show();
                int max = 0;

                if (gridControlAll.DataSource != null)
                {
                    DataTable dtGrid = gridControlAll.DataSource as DataTable;

                    var packupdate = (from a in dtGrid.AsEnumerable()
                                      where a.Field<Int64>("RECID") == Convert.ToInt64(recid)
                                      select a);
                    foreach (DataRow item in packupdate.ToArray())
                    {
                        item["SPLITPACK"] = true;
                    }
                    dtGrid.AcceptChanges();

                    gridControlAll.DataSource = dtGrid;
                    gridViewAll.BestFitColumns();

                    var packscan = (from a in dtGrid.AsEnumerable()
                                    where a.Field<bool>("SPLITPACK") == true && a.Field<int>("Receive") == 1
                                    select a);
                    int cPackSacn = packscan.Count();
                    foreach (DataRow item in packscan.ToArray())
                    {
                        item["SPLITPACK"] = false;
                    }

                    if (cPackSacn > 0) return;

                    DataTable dtTree = treeList1.DataSource as DataTable;

                    if (rdPack.Properties.Items.Count == 0)
                    {
                        max = 1;
                    }
                    else
                    {
                        var m = from r in rdPack.Properties.Items
                                select new { Value = Convert.ToInt32(r.Value) };
                        if (m.Count() > 0)
                        {
                            max = m.Max(r => r.Value);
                        }
                    }

                    if (!pack)
                    {
                        var packrow = (from a in dtGrid.AsEnumerable()
                                       where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == max.ToString()
                                           && a.Field<bool>("SPLITPACK") == false
                                       select a);

                        foreach (DataRow item in packrow.ToArray())
                        {
                            foreach (DataRow r in dtTree.Rows)
                            {
                                if (r["CodePart"].ToString() == item["CodePart"].ToString())
                                {
                                    //r.Delete();
                                    //item["PACKQUANTITYSEQUENCETOTAL"] = "";
                                    item["SPLITPACK"] = true;
                                }
                            }
                        }
                        #region remove radio
                        //dtTree.AcceptChanges();
                        //var treerow = (from a in dtTree.AsEnumerable()
                        //               where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == max.ToString()
                        //               select a).ToArray();

                        //if (treerow.Count() == 0)
                        //{
                        //    var treerowpack = (from a in dtTree.AsEnumerable()
                        //                       where a.Field<string>("ID") == max.ToString()
                        //                       select a).ToArray();

                        //    foreach (DataRow item in treerowpack.ToArray())
                        //    {
                        //        max = max - 1;

                        //        item.Delete();

                        //        rdPack.Properties.Items.RemoveAt(Convert.ToInt32(max));
                        //    }
                        //}

                        //dtTree.AcceptChanges();

                        //max = max - 1;
                        #endregion
                        
                    }
                    else
                    {
                        //เช็คกด new pack ไว้หรือไม่
                        var packrow = (from a in dtTree.AsEnumerable()
                                       where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == max.ToString()
                                       select a);

                        if (packrow.Count() > 0)//ไม่ได้กดปุ่ม new pack ให้สร้าง pack auto
                        {
                            var addpack = (from a in dtGrid.AsEnumerable()
                                           where a.Field<bool>("SPLITPACK") == true
                                           select a).ToArray();
                            if (pack && addpack.Count() == 1)
                            {
                                max = max + 1;
                                rdPack.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(max.ToString(), "Pack " + max.ToString()));
                                rdPack.EditValue = max.ToString();

                                DataRow row = dtTree.NewRow();
                                row["PRODUCTARTICLE"] = "Pack " + max.ToString();
                                row["PARTNAME"] = "";
                                row["CODEPART"] = "";
                                row["CODEPACK"] = "";
                                row["CUTRITEMATERIALCODE"] = "";
                                row["FINISHLENGTH"] = 0;
                                row["FINISHWIDTH"] = 0;
                                row["RECID"] = 0;
                                row["Receive"] = 0;
                                row["PACKQUANTITYSEQUENCETOTAL"] = "";
                                row["ID"] = max.ToString();
                                row["ParentID"] = "0";
                                dtTree.Rows.Add(row);
                            }
                            else
                            {
                                max = Convert.ToInt32(rdPack.EditValue);
                            }
                        }

                        var splitpack = (from a in dtGrid.AsEnumerable()
                                         where a.Field<bool>("SPLITPACK") == true && a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == ""
                                         select a).ToArray();

                        foreach (DataRow r in splitpack)
                        {
                            if (r["CodePack"].ToString().Contains("_"))
                            {
                                STM.MessageBoxError("มีข้อมูลการแบ่งแพ็คแล้ว");
                                r["SPLITPACK"] = false;
                            }
                            else if (Convert.ToInt32(r["Receive"]) == 0 && string.IsNullOrEmpty(r["PACKQUANTITYSEQUENCETOTAL"].ToString()))
                            {
                                var pack_split = (from a in dtTree.AsEnumerable()
                                                  where a["SPLITPACK"].ToString() == "False" && Convert.ToInt32(a["Receive"]) == 1 && Convert.ToInt32(a["PACKQUANTITYSEQUENCETOTAL"]) == max
                                                  select a);

                                if (pack_split.ToArray().Count() > 0)//รายการที่ split กับรายการที่ยิง จะต้องไม่อยู่ในแพ็คเดียวกัน
                                {
                                    STM.MessageBoxError("เกิดข้อผิดพลาดข้อมูลแพ็ค");

                                    foreach (DataRow item in packupdate.ToArray())
                                    {
                                        item["SPLITPACK"] = false;
                                    }
                                    dtGrid.AcceptChanges();
                                }
                                else
                                {
                                    r["PACKQUANTITYSEQUENCETOTAL"] = max.ToString();
                                    r["ParentID"] = max.ToString();
                                    r["Receive"] = 0;

                                    dtTree.ImportRow(r);
                                }
                            }
                        }

                        foreach (DataRow r in dtGrid.Rows)
                        {
                            var treeOrder = (from a in dtTree.AsEnumerable()
                                              where a["CodePart"].ToString() == r["CodePart"].ToString()
                                              select a).ToArray();
                            foreach (DataRow item in treeOrder)
                            {
                                r["PACKQUANTITYSEQUENCETOTAL"] = item["PACKQUANTITYSEQUENCETOTAL"].ToString();
                            }

                            //if (Convert.ToBoolean(r["SPLITPACK"]) == true)
                            //    r["PACKQUANTITYSEQUENCETOTAL"] = max.ToString();
                        }
                    }

                    treeList1.ExpandAll();
                    gridViewAll.PostEditor();

                    var count_complete = (from a in dtGrid.AsEnumerable()
                                          where a.Field<string>("PACKQUANTITYSEQUENCETOTAL") == ""
                                          select a);

                    if (count_complete.Count() == 0)
                        btnSplitPack.Enabled = true;
                    else
                        btnSplitPack.Enabled = false;

                }
            }
        }

        //WK#1.n 20230519
        private void timerClearScan_Tick(object sender, EventArgs e)
        {
            //if (admin == 0 && status_copy == 1)
            //{
            //    txtScan.Text = "";
            //}
        }

        private void timerRefreshLock_Tick(object sender, EventArgs e)
        {
            if (admin == 0 && !string.IsNullOrEmpty(RoomCategoryLock))
            {
                status_copy = Convert.ToInt32(STM.QueryData_ExecuteScalar(string.Format(@"select Lock from dbo.STMROOMCATEGORY WHERE NAME = '{0}'  order by SEQ ", RoomCategoryLock)));
            }
        }

        private void btnPrintNewPack_Click(object sender, EventArgs e)
        {
            if (STM.MessageBoxConfirm("คุณต้องการปริ้นบาร์โค๊ด New pack หรือไม่?")) return;

            string barcodeIncrease = "SMARTPACKINCREASE001";

            DataTable dt = new DataTable();

            dt.Columns.AddRange(new DataColumn[] { 
                    new DataColumn("BarcodeIncrease",typeof(byte[]))
                });

            byte[] img = null;
            Linear barcode1 = new Linear();
            barcode1.Type = BarcodeType.CODE128;
            barcode1.ShowText = false;
            barcode1.AutoResize = true;

            img = null;

            barcode1.Data = barcodeIncrease;
            img = barcode1.drawBarcodeAsBytes();

            DataRow new_row = dt.NewRow();
            new_row["BarcodeIncrease"] = img;
            dt.Rows.Add(new_row);

            PrinterSetting printsetting = STM.Print;
            CrystalReportNewPack rpt = new CrystalReportNewPack();
            rpt.SetDataSource(dt);

            System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();

            if (printsetting.PrintFull != "")
                printerSettings.PrinterName = printsetting.PrintFull;

            rpt.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA4;
            rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
            rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

            rpt.Close();
            rpt.Dispose();

        }

        //WK#1.n 20241004
        private void btnExpandCollapse_Click(object sender, EventArgs e)
        {
            FormAdminExc frm = new FormAdminExc();
            frm.ShowDialog(admin);

            if (treeListColumn3.Visible == false)
            {
                treeListColumn3.Visible = true;
            }
            else if (treeListColumn3.Visible == true)
            {
                treeListColumn3.Visible = false;
            }

            ////Expand and Collapse column code part
            //int width = 0;
            //if (treeListColumn3.Width == 20)
            //{
            //    //btnExpandCollapse.Text = "<";
            //    width = 150;
            //}
            //else if (treeListColumn3.Width == 150)
            //{
            //    //btnExpandCollapse.Text = ">";
            //    width = 20;
            //}
            //treeListColumn3.Visible = false;

        }



    }
}
