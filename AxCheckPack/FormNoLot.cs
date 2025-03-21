using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    public partial class FormNoLot : XtraForm
    {
        /// สิทธิ Admin
        /// 1. เห็นปุ่ม Lock setting
        /// 2. เห็น column codepart
        /// 3. เปิดใช้ช่อง scan ได้
        /// 4. เห็นปุ่ม Print pack
        /// 5. เลือกหมวดงานได้
        /// 6. Edit column CodePack and CodePart 
        /// 7. ปิดการเครียข้อความในช่อง scan อัตโนมัติ
        
        SoundPlayer sp = new SoundPlayer(Properties.Resources.beep11);
        SoundPlayer spError = new SoundPlayer(Properties.Resources.Alarme);
        SoundPlayer spComplete = new SoundPlayer(Properties.Resources.ragnarok_online_level_up_sound);

        private Stopwatch stopwatch = new Stopwatch();
        private StringBuilder scannedData = new StringBuilder();
       
        string RoomCategoryLock = "";
        int status_copy = 0;
        int timeClear = 3; //WK#1.n 20230519
        int admin = 0;
        private const int scannerThreshold = 90;
        private bool isScannerInput = false;
        //IntPtr _hookID = IntPtr.Zero;//WK#1.n 20250220

        public FormNoLot()
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                InitializeComponent();

                this.KeyPreview = true; // อนุญาตให้ฟอร์มดักจับเหตุการณ์คีย์บอร์ด
                //this.KeyPress += new KeyPressEventHandler(FormNoLot_KeyPress);

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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            // เริ่มจับเวลาหากยังไม่ได้เริ่ม
            if (!stopwatch.IsRunning)
            {
                stopwatch.Start();
                isScannerInput = true;
            }

            // ตรวจสอบว่าเป็นอักขระพิมพ์ได้ (ตัวเลข/ตัวอักษร)
            //if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.Z)
            //{
            //    scannedData.Append((char)e.KeyValue);
            //    e.SuppressKeyPress = true; // บล็อกการป้อนจากคีย์บอร์ด
            //}

            scannedData.Append((char)e.KeyValue);
            e.SuppressKeyPress = true; // บล็อกการป้อนจากคีย์บอร์ด

            if (e.KeyCode == Keys.Enter) // จบการสแกนเมื่อ Enter ถูกกด
            {
                stopwatch.Stop();
                long tim = stopwatch.ElapsedMilliseconds;
                if (
                    stopwatch.ElapsedMilliseconds < 200 || //Dongle
                    (stopwatch.ElapsedMilliseconds > 1100 && stopwatch.ElapsedMilliseconds < 1300) //USB
                   ) // ถ้าพิมพ์เร็วมาก แสดงว่าเป็นเครื่องสแกน
                {
                    if (scannedData.ToString() != "")
                    {
                        timerClearScan.Enabled = false;
                        timerClearScan.Enabled = true;

                        scannedData.Replace((char)Keys.OemMinus, '-');
                        scannedData.Replace((char)Keys.ShiftKey, '*');
                        scannedData.Replace((char)Keys.Enter, '#');
                        scannedData.Replace("*-", "_");
                        scannedData.Replace("*", "");
                        scannedData.Replace("#", "");

                        FormScan(scannedData.ToString());

                        //MessageBox.Show("Barcode Scanned: " + scannedData.ToString() + " :: " + barcode + " :: " + tim, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                scannedData.Clear();
                stopwatch.Reset();
            }

            base.OnKeyDown(e);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                dateEditProduction.EditValue = DateTime.Now;
                UpdateDashboard();
                RefreshFormText(this);

                gridView1.BestFitColumns();
                gridView3.BestFitColumns();

                PrinterSetting printsetting = STM.Print;
                //DataTable dtRoomCategory = STM.QueryData(@"select NAME from dbo.STMROOMCATEGORY order by SEQ ");
                DataTable dtRoomCategory = STM.QueryData(@"SELECT NAME FROM (
	                                                            SELECT 0 SEQ, '' AS NAME 
	                                                            UNION 
	                                                            SELECT SEQ, NAME FROM dbo.STMROOMCATEGORY
                                                            ) A
                                                            ORDER BY SEQ");

                rdRoomCategory.Properties.DataSource = dtRoomCategory;
                rdRoomCategory.EditValue = "";

//                if (printsetting.PrintFull == "" && printsetting.PrintLabel == "")
//                {
//                    STM.MessageBoxError(" กรุณา set เครื่องปริ้น PrinterFull และ PrinterLabel");
//                }

//                DataTable dtConfig = STM.QueryDataProductEngineering(string.Format(@"SELECT [ComputerName],[Category]
//                                                                                    FROM [dbo].[AxCheckPackConfig]
//                                                                                    where computername='{0}' ", STM.GetComputerName));
//                if (dtConfig != null && dtConfig.Rows.Count > 0)
//                {
//                    rdRoomCategory.EditValue = dtConfig.Rows[0]["Category"].ToString() == "" ? "ห้องครัว" : dtConfig.Rows[0]["Category"].ToString();
//                }
//                else
//                {
//                    rdRoomCategory.EditValue = "ห้องครัว";
//                }

                //WK#1.n 20230223
                admin = Convert.ToInt32(STM.QueryData_ExecuteScalarProductEngineering(string.Format("SELECT [Active] FROM [dbo].[AssemblyAdmin] WHERE [User] = '{0}'", STM.GetLoginName)));
                if (STM.GetLoginName == "1011405" || admin == 1)
                //if (admin == 1)
                {
                    layoutControlItem16.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;
                    gridColumn20.Visible = true;//CodePart
                }
                else
                {
                    layoutControlItem16.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;
                    gridColumn20.Visible = false;//CodePart
                }

                txtScan.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();//ปิดคลิ๊กขวา copy paste

                LockPrintPack(true);

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

        public static void RefreshFormText(Form frm)
        {
            string msg = "Check Pack";
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            con.Open();

            if (con.Database.Trim() != "")
            {
                msg += " , DataBase => " + con.Database;
            }

            PrinterSetting printsetting = STM.Print;

            if (printsetting.PrintFull.Trim() != "")
            {
                msg += " , Full => " + printsetting.PrintFull;
            }
            if (printsetting.PrintLabel.Trim() != "")
            {
                msg += " , Lable => " + printsetting.PrintLabel;
            }
            con.Close();

            frm.Text = msg;

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

                    DateTime ProductionDate = dateEditProduction.EditValue == null ? DateTime.Now : Convert.ToDateTime(dateEditProduction.EditValue);
                    Report.FormFull frm = new Report.FormFull();
                    frm.ShowDialog(STMLOTIDSTM, CODEPACK, rdRoomCategory.EditValue.ToString(), PrintType.Full, ProductionDate);
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

            txtComplete.Text = "";
            gridControl1.DataSource = null;
            gridControl3.DataSource = null;
        }
        private void toggleSwitch1_Toggled(object sender, EventArgs e)
        {
            timer1.Enabled = toggleSwitch1.IsOn;
        }
        private void glLot_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                //STM.SplashScreenManagerManual_Show();

                //if (glLot.EditValue == null) return;

                //LoadPack();
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

        private void FormNoLot_KeyPress(object sender, KeyPressEventArgs e)
        {
            //MessageBox.Show(e.KeyChar.ToString());
            //if (!stopwatch.IsRunning)
            //{
            //    stopwatch.Start();
            //    //scannedData.Append(e.KeyChar);
            //}
            //else
            //{
            //    stopwatch.Stop();
            //    long elapsedTime = stopwatch.ElapsedMilliseconds;
            //    stopwatch.Reset();
            //    stopwatch.Start();

            //    if (elapsedTime < scannerThreshold) // ถ้าระยะเวลาสั้นมาก ถือว่าเป็น Scanner
            //    {
            //        scannedData.Append(e.KeyChar);
            //    }
            //    else // ถ้าช้า แสดงว่าเป็น Keyboard
            //    {
            //        scannedData.Clear(); // รีเซ็ตค่า
            //    }
            //}

            //if (e.KeyChar == (char)Keys.Enter) // Barcode Scanner มักจะกด Enter ต่อท้าย
            //{
            //    string filteredBarcode = RemoveOddIndexCharacters(scannedData.ToString());
            //    //string inputType = scannedData.Length > 5 ? "Scanner" : "Keyboard";
            //    MessageBox.Show(filteredBarcode);
            //    scannedData.Clear();
            //    stopwatch.Reset();
            //}
        
            //if (txtScan.Properties.ReadOnly)
            //{
            //    if (!stopwatch.IsRunning || stopwatch.ElapsedMilliseconds > 80)
            //    {
            //        // หากระยะเวลาระหว่าง KeyPress เกิน 50ms แสดงว่าอาจเป็นการพิมพ์มือ
            //        string filteredBarcode = RemoveOddIndexCharacters(scannedData.ToString());

            //        //MessageBox.Show("Clear : " + filteredBarcode, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //        scannedData.Clear();
            //    }

            //    stopwatch.Restart();  // รีเซ็ตเวลา

            //    if (e.KeyChar == (char)Keys.Enter) // เมื่อเครื่องสแกนส่ง Enter มา
            //    {
            //        if (scannedData.ToString() != "")
            //        {
            //            timerClearScan.Enabled = false;
            //            timerClearScan.Enabled = true;

            //            string filteredBarcode = RemoveOddIndexCharacters(scannedData.ToString());//PD66000025774-PD02PT01
            //            //string filteredBarcode = "PD68000017467-16388";

            //            FormScan(filteredBarcode);

            //            //MessageBox.Show("Barcode Scanned: " + scannedData.ToString() + " :: " + filteredBarcode, "Scan Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //            scannedData.Clear(); // ล้างข้อมูลที่สะสมไว้
            //            e.Handled = true; // ป้องกัน Enter จากไปก่อผลที่อื่น
            //            stopwatch.Stop();
            //        }
            //    }
            //    else
            //    {
            //        scannedData.Append(e.KeyChar); // สะสมค่าบาร์โค้ดที่ถูกสแกน
            //    }
            //}
        }


        private void FormScan(string barcode)
        {
                if (STM.GetComputerName != "STMM-IT-N-06-PC")
                    sp.Play();

                if (barcode == "") return;

                SqlConnection con = new SqlConnection(STM.ConnectionString);
                SqlCommand cmd = new SqlCommand();
                SqlDataAdapter ad = new SqlDataAdapter();

                try
                {
                    STM.SplashScreenManagerManual_Show();

                    con.Open();
                    cmd.Connection = con;

                    if (gridControl1.DataSource == null || gridView1.RowCount == 0)
                    {
                        #region

                        
//                        //WK#1.n 20230526 ล็อค check pick pack part
//                        DataTable dtcheckpick = STM.QueryData(string.Format(@"SELECT LOCK , ROOMCATEGORY
//                                                                            FROM dbo.STMSMARTPDPARTSTM part
//                                                                            LEFT JOIN dbo.STMROOMCATEGORY lock
//                                    	                                        ON lock.NAME = part.ROOMCATEGORY
//                                                                            WHERE part.CODEPART='{0}' AND CHECKPART = 0 AND part.CUTRITEMATERIALCODE <> ''", barcode));

//                        if (dtcheckpick != null && dtcheckpick.Rows.Count > 0)
//                        {
//                            RoomCategoryLock = dtcheckpick.Rows[0]["ROOMCATEGORY"].ToString();
//                            rdRoomCategory.EditValue = RoomCategoryLock;
//                            status_copy = Convert.ToInt32(dtcheckpick.Rows[0]["LOCK"]);
                        //                        }
                        #endregion

                        //if (status_copy == 0)//WK#1.n 20230526
                        {
                            //                            string sql = string.Format(@"
                            //                                                                select ROOMCATEGORY, STMLOTIDSTM, OrderNumber,RECID, BARCODE, PRODUCTARTICLE, PARTNAME, CODEPART, FINISHLENGTH, FINISHWIDTH, CODEPACK, SHELFNO, CHECKPART,CHECKPACK, CHECKPACKDATE, CHECKPACKUSER, ITEMID
                            //                                                                ,CUTRITEMATERIALCODE
                            //                                                                from [dbo].[STMSMARTPDPARTBACKDATA]
                            //                                                                where  CODEPACK in (select CODEPACK from dbo.STMSMARTPDPARTBACKDATA where CODEPART='{0}')
                            //                                                                order by PRODUCTARTICLE,PARTNAME,FINISHLENGTH,FINISHWIDTH ", barcode);

                            string sql = string.Format(@"
                                                    WITH CODEPACK AS (
            	                                        SELECT CODEPACK FROM {0} WHERE CODEPART='{1}'
                                                    )
            
                                                    SELECT ROOMCATEGORY, STMLOTIDSTM, OrderNumber,RECID, BARCODE, PRODUCTARTICLE, PARTNAME, CODEPART, FINISHLENGTH, FINISHWIDTH, part.CODEPACK, SHELFNO, CHECKPART,CHECKPACK, CHECKPACKDATE, CHECKPACKUSER, ITEMID
                                                    ,CUTRITEMATERIALCODE
                                                    from CODEPACK
                                                    INNER JOIN {0} part
            	                                        ON part.CODEPACK = CODEPACK.CODEPACK
                                                    order by PRODUCTARTICLE,PARTNAME,FINISHLENGTH,FINISHWIDTH ", STM.GetTableName, barcode);

                            DataTable dtCodePack = new DataTable();
                            cmd.CommandText = sql;
                            ad.SelectCommand = cmd;
                            ad.Fill(dtCodePack);

                            gridControl1.DataSource = dtCodePack;
                            gridView1.BestFitColumns();

                            if (dtCodePack != null && dtCodePack.Rows.Count > 0)
                            {
                                RoomCategoryLock = dtCodePack.Rows[0]["ROOMCATEGORY"].ToString();
                                rdRoomCategory.EditValue = RoomCategoryLock;

                                int hideCodePack = 0;
                                int blockScan = 0;
                                DataTable dtLock = STM.LockSetting(RoomCategoryLock);

                                foreach (DataRow row in dtLock.Rows)
	                            {
                                    if (admin == 0)
                                        status_copy = Convert.ToInt32(row["Lock"]);

                                    hideCodePack = Convert.ToInt32(row["CodePart"]);
                                    blockScan = Convert.ToInt32(row["LockScan"]);
	                            }

                                //if (admin == 0 && !string.IsNullOrEmpty(RoomCategoryLock))
                                //    status_copy = STM.LockSetting(2, RoomCategoryLock);
                                //int hideCodePack = STM.LockSetting(3, RoomCategoryLock);

                                if (admin == 0 && hideCodePack == 1)
                                    gridColumn20.Visible = false;// CodePart
                                else
                                    gridColumn20.Visible = true;// CodePart
                                

                                if (blockScan > 0)
                                    txtScan.Properties.ReadOnly = true;
                                else
                                    txtScan.Properties.ReadOnly = false;
                                    
                            }
                        }

                        RefreshPack();
                    }

                    if (gridControl1.DataSource != null)
                    {
                        DataTable dtgrid = gridControl1.DataSource as DataTable;
                        var pack = from r in dtgrid.AsEnumerable()
                                   where r["CODEPART"].ToString().ToLower() == barcode.ToLower()
                                   select r;
                        if (pack.Count() == 0)
                        {
                            STM.MessageBoxError(string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode));

                            notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                            notifyIcon1.ShowBalloonTip(5000);
                            spError.Play();
                        }
                        else
                        {
                            foreach (DataRow row in pack.ToArray())
                            {
                                if (Convert.ToInt32(row["CHECKPACK"]) == 0)
                                {
                                    row["CHECKPACK"] = 1;

                                    cmd.CommandText = string.Format(@" UPDATE {0}
                                                                SET [CHECKPACKUSER] = @CHECKPACKUSER
                                                                    ,[CHECKPACKDATE] = GETDATE()
                                                                    ,[CHECKPACK] = 1
                                                                WHERE  RECID=@RECID ", STM.GetTableName);
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.Add(new SqlParameter("RECID", row["RECID"]));
                                    cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", STM.GetLoginName));
                                    cmd.ExecuteNonQuery();
                                }
                                else if (Convert.ToInt32(row["CHECKPACK"]) == 1)
                                    scannedData.Clear();

                            }

                            UpdateComplete();

                            var complate = from r in (gridControl1.DataSource as DataTable).AsEnumerable()
                                           where Convert.ToInt32(r["CHECKPACK"]) == 0
                                           select r;

                            if (complate.Count() == 0)
                            {
                                spComplete.Play();
                                DataTable dt = gridControl1.DataSource as DataTable;

                                RefreshPack();

                                Report.FormFull frm = new Report.FormFull();
                                DateTime ProductionDate = dateEditProduction.EditValue == null ? DateTime.Now : Convert.ToDateTime(dateEditProduction.EditValue);

                                if (rdRoomCategory.EditValue.ToString().Substring(0, 3) == "DIY")
                                {
                                    frm.ShowDialog(
                                       dt.Rows[0]["STMLOTIDSTM"].ToString(),
                                       dt.Rows[0]["CODEPACK"].ToString(),
                                       rdRoomCategory.EditValue.ToString().Substring(0, 3),
                                       PrintType.DIY,
                                       ProductionDate);
                                }
                                else if (rdRoomCategory.EditValue.ToString().Substring(0, 7) == "Compack")
                                {
                                    frm.ShowDialog(
                                       dt.Rows[0]["STMLOTIDSTM"].ToString(),
                                       dt.Rows[0]["CODEPACK"].ToString(),
                                       rdRoomCategory.EditValue.ToString().Substring(0, 7),
                                       PrintType.Compack,
                                       ProductionDate);
                                }
                                else
                                {
                                    frm.ShowDialog(
                                        dt.Rows[0]["STMLOTIDSTM"].ToString(),
                                        dt.Rows[0]["CODEPACK"].ToString(),
                                        rdRoomCategory.EditValue.ToString(),
                                        PrintType.Full,
                                        ProductionDate);

                                    frm.ShowDialog(
                                        dt.Rows[0]["STMLOTIDSTM"].ToString(),
                                        dt.Rows[0]["CODEPACK"].ToString(),
                                        rdRoomCategory.EditValue.ToString(),
                                        PrintType.Label,
                                        ProductionDate);
                                }

                                gridControl1.DataSource = null;
                                txtComplete.Text = "";
                            }
                        }
                    }
                    else
                    {
                        //if (status_copy == 1)//WK#1.n 20230526 ล็อค check pick pack part
                        //{
                        //    STM.MessageBoxError(string.Format(" Barcode {0} ยังไม่ผ่าน Check Part ", barcode));
                        //}
                        //else
                        //{
                            //STM.MessageBoxError(string.Format(" ไม่พบข้อมูล OrderNumber '{0}' ", barcode));
                            notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                            notifyIcon1.ShowBalloonTip(5000);

                            if (STM.GetComputerName != "STMM-IT-N-06-PC")
                                spError.Play();
                        //}
                    }
                }
                catch (Exception ex)
                {
                    STM.MessageBoxError(ex);
                }
                finally
                {
                    con.Close();
                    let = true;
                    STM.SplashScreenManagerManual_Hide();
                }
        }

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

        bool let = true;
        private void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (!txtScan.Properties.ReadOnly)
            {
                timerClearScan.Enabled = false;
                timerClearScan.Enabled = true;

                //if (e.Control & e.KeyCode == Keys.V)
                //{
                //    txtScan.Text = "";
                //}
                //else 
                if (e.KeyCode == Keys.Enter)
                {
                    timerClearScan.Enabled = false;

                    //STM.MessageBoxError(txtScan.Text.Trim());

                    string barcode = txtScan.Text.Trim();
                    txtScan.Text = string.Empty;
                    FormScan(barcode);

                    #region 20250312
                    //                sp.Play();
                    //                string barcode = txtScan.Text.Trim();
                    //                txtScan.Text = string.Empty;
                    //                if (barcode == "") return;

                    //                SqlConnection con = new SqlConnection(STM.ConnectionString);
                    //                SqlCommand cmd = new SqlCommand();
                    //                SqlDataAdapter ad = new SqlDataAdapter();

                    //                try
                    //                {
                    //                    STM.SplashScreenManagerManual_Show();

                    //                    con.Open();
                    //                    cmd.Connection = con;

                    //                    if (gridControl1.DataSource == null || gridView1.RowCount == 0)
                    //                    {
                    //                        #region

                    //                        #endregion
                    //                        //                        //WK#1.n 20230526 ล็อค check pick pack part
                    //                        //                        DataTable dtcheckpick = STM.QueryData(string.Format(@"SELECT LOCK , ROOMCATEGORY
                    //                        //                                                                FROM dbo.STMSMARTPDPARTBACKDATA part
                    //                        //                                                                LEFT JOIN dbo.STMROOMCATEGORY lock
                    //                        //                        	                                        ON lock.NAME = part.ROOMCATEGORY
                    //                        //                                                                WHERE CODEPART='{0}' AND CHECKPART = 0 AND part.CUTRITEMATERIALCODE <> ''", barcode));

                    //                        //                        if (dtcheckpick != null && dtcheckpick.Rows.Count > 0)
                    //                        //                        {
                    //                        //                            RoomCategoryLock = dtcheckpick.Rows[0]["ROOMCATEGORY"].ToString();
                    //                        //                            rdRoomCategory.EditValue = RoomCategoryLock;
                    //                        //                            status_copy = Convert.ToInt32(dtcheckpick.Rows[0]["LOCK"]);
                    //                        //                        }

                    //                        if (status_copy == 0)//WK#1.n 20230526
                    //                        {
                    //                            //                            string sql = string.Format(@"
                    //                            //                                                                select ROOMCATEGORY, STMLOTIDSTM, OrderNumber,RECID, BARCODE, PRODUCTARTICLE, PARTNAME, CODEPART, FINISHLENGTH, FINISHWIDTH, CODEPACK, SHELFNO, CHECKPART,CHECKPACK, CHECKPACKDATE, CHECKPACKUSER, ITEMID
                    //                            //                                                                ,CUTRITEMATERIALCODE
                    //                            //                                                                from [dbo].[STMSMARTPDPARTBACKDATA]
                    //                            //                                                                where  CODEPACK in (select CODEPACK from dbo.STMSMARTPDPARTBACKDATA where CODEPART='{0}')
                    //                            //                                                                order by PRODUCTARTICLE,PARTNAME,FINISHLENGTH,FINISHWIDTH ", barcode);

                    //                            string sql = string.Format(@"
                    //                                        WITH CODEPACK AS (
                    //	                                        SELECT CODEPACK FROM {0} WHERE CODEPART='{1}'
                    //                                        )
                    //
                    //                                        SELECT ROOMCATEGORY, STMLOTIDSTM, OrderNumber,RECID, BARCODE, PRODUCTARTICLE, PARTNAME, CODEPART, FINISHLENGTH, FINISHWIDTH, part.CODEPACK, SHELFNO, CHECKPART,CHECKPACK, CHECKPACKDATE, CHECKPACKUSER, ITEMID
                    //                                        ,CUTRITEMATERIALCODE
                    //                                        from CODEPACK
                    //                                        INNER JOIN {0} part
                    //	                                        ON part.CODEPACK = CODEPACK.CODEPACK
                    //                                        order by PRODUCTARTICLE,PARTNAME,FINISHLENGTH,FINISHWIDTH ", STM.GetTableName, barcode);

                    //                            DataTable dtCodePack = new DataTable();
                    //                            cmd.CommandText = sql;
                    //                            ad.SelectCommand = cmd;
                    //                            ad.Fill(dtCodePack);

                    //                            gridControl1.DataSource = dtCodePack;
                    //                            gridView1.BestFitColumns();

                    //                            if (dtCodePack != null && dtCodePack.Rows.Count > 0)
                    //                            {
                    //                                RoomCategoryLock = dtCodePack.Rows[0]["ROOMCATEGORY"].ToString();
                    //                                rdRoomCategory.EditValue = RoomCategoryLock;

                    //                                if (admin == 0 && !string.IsNullOrEmpty(RoomCategoryLock))
                    //                                    status_copy = STM.LockSetting(2, RoomCategoryLock);

                    //                                int hideCodePack = STM.LockSetting(3, RoomCategoryLock);

                    //                                if (admin == 0 && hideCodePack == 1)
                    //                                    gridColumn20.Visible = false;// CodePart
                    //                                else
                    //                                    gridColumn20.Visible = true;// CodePart
                    //                            }
                    //                        }

                    //                        RefreshPack();
                    //                    }

                    //                    if (gridControl1.DataSource != null)
                    //                    {
                    //                        DataTable dtgrid = gridControl1.DataSource as DataTable;
                    //                        var pack = from r in dtgrid.AsEnumerable()
                    //                                   where r["CODEPART"].ToString().ToLower() == barcode.ToLower()
                    //                                   select r;
                    //                        if (pack.Count() == 0)
                    //                        {
                    //                            STM.MessageBoxError(string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode));

                    //                            notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                    //                            notifyIcon1.ShowBalloonTip(5000);
                    //                            spError.Play();
                    //                        }
                    //                        else
                    //                        {
                    //                            foreach (DataRow row in pack.ToArray())
                    //                            {
                    //                                if (Convert.ToInt32(row["CHECKPACK"]) == 0)
                    //                                {
                    //                                    row["CHECKPACK"] = 1;

                    //                                    cmd.CommandText = string.Format(@" UPDATE {0}
                    //                                                    SET [CHECKPACKUSER] = @CHECKPACKUSER
                    //                                                        ,[CHECKPACKDATE] = GETDATE()
                    //                                                        ,[CHECKPACK] = 1
                    //                                                    WHERE  RECID=@RECID ", STM.GetTableName);
                    //                                    cmd.Parameters.Clear();
                    //                                    cmd.Parameters.Add(new SqlParameter("RECID", row["RECID"]));
                    //                                    cmd.Parameters.Add(new SqlParameter("CHECKPACKUSER", STM.GetLoginName));
                    //                                    cmd.ExecuteNonQuery();
                    //                                }
                    //                            }

                    //                            UpdateComplete();

                    //                            var complate = from r in (gridControl1.DataSource as DataTable).AsEnumerable()
                    //                                           where Convert.ToInt32(r["CHECKPACK"]) == 0
                    //                                           select r;

                    //                            if (complate.Count() == 0)
                    //                            {
                    //                                spComplete.Play();
                    //                                DataTable dt = gridControl1.DataSource as DataTable;

                    //                                RefreshPack();

                    //                                Report.FormFull frm = new Report.FormFull();
                    //                                DateTime ProductionDate = dateEditProduction.EditValue == null ? DateTime.Now : Convert.ToDateTime(dateEditProduction.EditValue);

                    //                                if (rdRoomCategory.EditValue.ToString().Substring(0, 3) == "DIY")
                    //                                {
                    //                                    frm.ShowDialog(
                    //                                       dt.Rows[0]["STMLOTIDSTM"].ToString(),
                    //                                       dt.Rows[0]["CODEPACK"].ToString(),
                    //                                       rdRoomCategory.EditValue.ToString().Substring(0, 3),
                    //                                       PrintType.DIY,
                    //                                       ProductionDate);
                    //                                }
                    //                                else if (rdRoomCategory.EditValue.ToString().Substring(0, 7) == "Compack")
                    //                                {
                    //                                    frm.ShowDialog(
                    //                                       dt.Rows[0]["STMLOTIDSTM"].ToString(),
                    //                                       dt.Rows[0]["CODEPACK"].ToString(),
                    //                                       rdRoomCategory.EditValue.ToString().Substring(0, 7),
                    //                                       PrintType.Compack,
                    //                                       ProductionDate);
                    //                                }
                    //                                else
                    //                                {
                    //                                    frm.ShowDialog(
                    //                                        dt.Rows[0]["STMLOTIDSTM"].ToString(),
                    //                                        dt.Rows[0]["CODEPACK"].ToString(),
                    //                                        rdRoomCategory.EditValue.ToString(),
                    //                                        PrintType.Full,
                    //                                        ProductionDate);

                    //                                    frm.ShowDialog(
                    //                                        dt.Rows[0]["STMLOTIDSTM"].ToString(),
                    //                                        dt.Rows[0]["CODEPACK"].ToString(),
                    //                                        rdRoomCategory.EditValue.ToString(),
                    //                                        PrintType.Label,
                    //                                        ProductionDate);
                    //                                }

                    //                                gridControl1.DataSource = null;
                    //                                txtComplete.Text = "";
                    //                            }
                    //                        }
                    //                    }
                    //                    else
                    //                    {
                    //                        if (status_copy == 1)//WK#1.n 20230526 ล็อค check pick pack part
                    //                        {
                    //                            STM.MessageBoxError(string.Format(" Barcode {0} ยังไม่ผ่าน Check Part ", barcode));
                    //                        }
                    //                        else
                    //                        {
                    //                            STM.MessageBoxError(string.Format(" ไม่พบข้อมูล OrderNumber '{0}' ", barcode));
                    //                            notifyIcon1.BalloonTipText = string.Format(" ไม่พบข้อมูล Code Part '{0}' ", barcode);
                    //                            notifyIcon1.ShowBalloonTip(5000);

                    //                            spError.Play();
                    //                        }


                    //                    }

                    //                }
                    //                catch (Exception ex)
                    //                {
                    //                    STM.MessageBoxError(ex);
                    //                }
                    //                finally
                    //                {
                    //                    con.Close();
                    //                    let = true;
                    //                    STM.SplashScreenManagerManual_Hide();
                    //                }
                    #endregion

                }
                //txtScan.Text = "";
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
            string OrderNumber = "";
            if (gridControl1.DataSource != null)
            {
                DataTable dt = gridControl1.DataSource as DataTable;

                if (dt != null && dt.Rows.Count > 0)
                {
                    OrderNumber = dt.Rows[0]["OrderNumber"].ToString();
                }
            }
            string sql = string.Format(@"declare @OrderNumber varchar(50)='{1}'

                                        ;with a as (
                                            SELECT distinct 
                                                     [STMLOTIDSTM]
                                                    ,OrderNumber
                                                    ,[PRODUCTARTICLE]
                                                    ,[CODEPACK]
                                                    ,[SHELFNO]
		                                            ,sum(checkpart) over(partition by STMLOTIDSTM,CODEPACK) as PartComplete
	                                                ,sum(CHECKPACK) over(partition by STMLOTIDSTM,CODEPACK) as PackComplete
	                                                ,COUNT(*) over(partition by STMLOTIDSTM,CODEPACK) as PartTotal
                                            FROM {0}
                                            where OrderNumber=@OrderNumber
                                            ) 

                                        select  STMLOTIDSTM,OrderNumber,PRODUCTARTICLE,CODEPACK,SHELFNO,PartComplete,PackComplete,PartTotal,
		                                        case when PartComplete=PartTotal then 1 else 0 end as PartSuccess,
		                                        case when PackComplete=PartTotal then 1 else 0 end as PackSuccess
                                        from a
                                        order by SHELFNO ", STM.GetTableName, OrderNumber);

            DataTable dt1 = STM.QueryData(sql);

            gridControl3.DataSource = dt1;
            gridView3.BestFitColumns();


        }
        private void UpdateDashboard()
        {
            SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
            SqlCommand cmd = new SqlCommand();

            con.Open();
            cmd.Connection = con;
            cmd.CommandText = @"if exists (SELECT * FROM [dbo].[ViewDashboardComputer] where  [ComputerName]=@ComputerName)
                                    begin
	                                    delete FROM [dbo].[ViewDashboardComputer] where  [ComputerName]=@ComputerName
                                    end

                                    INSERT INTO [dbo].[ViewDashboardComputer]
                                               ([ComputerName]
                                               ,[Program]
                                               ,[CreateDate])
                                         VALUES
                                               (@ComputerName
                                               ,@Program
                                               ,GETDATE()) ";
            cmd.Parameters.Clear();
            cmd.Parameters.Add(new SqlParameter("ComputerName", STM.GetComputerName));
            cmd.Parameters.Add(new SqlParameter("Program", "AxCheckPack"));
            cmd.ExecuteNonQuery();

            con.Close();
        }

        //WK#1.n 20230419
        private void LockPrintPack(bool lockScan = false)
        {
            int hideCodePack = 0;
            int blockScan = 0;
            int blockPrint = 0;
            DataTable dtLock = STM.LockSetting(rdRoomCategory.EditValue.ToString().Trim());

            foreach (DataRow row in dtLock.Rows)
            {
                status_copy = Convert.ToInt32(row["Lock"]);
                blockPrint = Convert.ToInt32(row["LockPrintPack"]);
                hideCodePack = Convert.ToInt32(row["CodePart"]);
                blockScan = Convert.ToInt32(row["LockScan"]);
            }

            if (blockScan > 0 || lockScan)
                txtScan.Properties.ReadOnly = true;
            else
                txtScan.Properties.ReadOnly = false;
                                    
            //int hideCodePack = STM.LockSetting(3, rdRoomCategory.EditValue.ToString().Trim());
            //status_copy = STM.LockSetting(2, rdRoomCategory.EditValue.ToString().Trim());

            if (admin == 0 && hideCodePack == 1)
                gridColumn20.Visible = false;// CodePart
            else
                gridColumn20.Visible = true;// CodePart
            
            gridView1.RefreshEditor(false);

            if (admin == 0 && blockPrint == 1)
                layoutControlItem4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never;//Print Pack
            else
                layoutControlItem4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Always;//Print Pack

            if (admin == 0 && status_copy == 1)
                rdRoomCategory.Enabled = false;//หมวดงาน
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
                DateTime ProductionDate = dateEditProduction.EditValue == null ? DateTime.Now : Convert.ToDateTime(dateEditProduction.EditValue);
                Report.FormFull frm = new Report.FormFull();
                frm.ShowDialog(STMLOTIDSTM, CODEPACK, rdRoomCategory.EditValue.ToString(), PrintType.Full, ProductionDate);

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
                DateTime ProductionDate = dateEditProduction.EditValue == null ? DateTime.Now : Convert.ToDateTime(dateEditProduction.EditValue);
                string STMLOTIDSTM = row["STMLOTIDSTM"].ToString();
                string CODEPACK = row["CODEPACK"].ToString();

                Report.FormFull frm = new Report.FormFull();
                frm.ShowDialog(STMLOTIDSTM, CODEPACK, rdRoomCategory.EditValue.ToString(), PrintType.Label, ProductionDate);

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
            try
            {
                FormPrintSetting frm = new FormPrintSetting();
                frm.ShowDialog();

                RefreshFormText(this);
            }
            catch (Exception ex)
            {
                STM.MessageBoxError(ex);
            }
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            FormSmartPack frm = new FormSmartPack();
            frm.ShowDialog(rdRoomCategory.EditValue.ToString(), admin);
        }
        private void simpleButton3_Click(object sender, EventArgs e)
        {
            //string RSTTT = "Compack".Substring(0, 7).ToLower();
            if (admin == 0 && STM.LockSetting(1, rdRoomCategory.EditValue.ToString().Trim()) == 1)
            {
                STM.MessageBoxError("Lock print pack " + rdRoomCategory.EditValue.ToString().Trim());
                return;
            }
            
            Report.FormPrintLot frm = new Report.FormPrintLot();
            frm.ShowDialog(rdRoomCategory.EditValue.ToString());
            
            
        }

        private void rdRoomCategory_EditValueChanged(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();

            try
            {

                con.ConnectionString = STM.ConnectionStringProductEngineering;
                con.Open();
                cmd.Connection = con;

                cmd.CommandText = string.Format(@"  declare @ComputerName varchar(50)='{0}',
		                                                    @Category varchar(50)='{1}'

                                                    if exists (SELECT [ComputerName],[Category] FROM [dbo].[AxCheckPackConfig] where computername=@ComputerName)
                                                    begin
	                                                    delete FROM [dbo].[AxCheckPackConfig] where computername=@ComputerName
                                                    end

                                                    insert into [dbo].[AxCheckPackConfig] ([ComputerName], [Category]) values (@ComputerName,@Category) ",

                                                STM.GetComputerName, rdRoomCategory.EditValue == null || rdRoomCategory.EditValue.ToString().Trim() == "" ? "ห้องครัว" : rdRoomCategory.EditValue.ToString());
                cmd.ExecuteNonQuery();

                LockPrintPack();
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

        //WK#1.n 20210921
        public void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            //int hideCodePart = STM.LockSetting(3, RoomCategoryLock);
            //status_copy = STM.LockSetting(2, RoomCategoryLock);

            int hideCodePart = 0;
            int blockScan = 0;

            DataTable dtLock = STM.LockSetting(RoomCategoryLock);

            foreach (DataRow row in dtLock.Rows)
            {
                status_copy = Convert.ToInt32(row["Lock"]);
                hideCodePart = Convert.ToInt32(row["CodePart"]);
                blockScan = Convert.ToInt32(row["LockScan"]);
            }

            if (blockScan > 0)
                txtScan.Properties.ReadOnly = true;
            else
                txtScan.Properties.ReadOnly = false;

            if (admin == 0 && hideCodePart == 1)
            {
                gridColumn20.Visible = false;// CodePart
            }
            else
            {
                gridColumn20.Visible = true;// CodePart
            }


            gridView1.RefreshEditor(false);

            if (admin == 0 && status_copy == 1)
            {
                gridColumn23.OptionsColumn.AllowEdit = false;//codepack
                gridColumn20.OptionsColumn.AllowEdit = false;//codepart
            }
            else
            {
                gridColumn23.OptionsColumn.AllowEdit = true;//codepack
                gridColumn20.OptionsColumn.AllowEdit = true;//codepart
            }
        }

        //WK#1.n 20230222
        private void btnCopySetting_Click_1(object sender, EventArgs e)
        {
            FormNoCopy frm = new FormNoCopy();
            frm.ShowDialog();
        }

        //WK#1.n 20230421
        private void gridView3_ShowingEditor(object sender, CancelEventArgs e)
        {
            status_copy =  STM.LockSetting(2, RoomCategoryLock);
            if (admin == 0 && status_copy == 1)
            {
                gridColumn12.OptionsColumn.AllowEdit = false;//codepack
            }
            else
            {
                gridColumn12.OptionsColumn.AllowEdit = true;//codepack
            }
        }

        //public void changeRoomCategory(string roomCategory)
        //{
        //    this.rdRoomCategory.EditValue = roomCategory;
        //    this.rdRoomCategory.Text = roomCategory;
        //    RoomCategoryLock = roomCategory;
        //}

        //WK#1.n 20230519
        private void timerClearScan_Tick(object sender, EventArgs e)
        {
            if (admin == 0 && status_copy == 1)
            {
               //txtScan.Text = "";
               rdRoomCategory.Enabled = false;
            }
            else
                rdRoomCategory.Enabled = true;

        }

        private void timerRefreshLock_Tick(object sender, EventArgs e)
        {
            status_copy = Convert.ToInt32(STM.QueryData_ExecuteScalar(string.Format(@"select Lock from dbo.STMROOMCATEGORY WHERE NAME = '{0}'  order by SEQ ", rdRoomCategory.EditValue.ToString())));

        }

    }
}
