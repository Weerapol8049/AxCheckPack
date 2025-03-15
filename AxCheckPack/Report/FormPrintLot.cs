using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack.Report
{
    public partial class FormPrintLot : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        
        string RoomCategory = "ห้องครัว";
        
        public FormPrintLot()
        {
            InitializeComponent();
        }

        public void ShowDialog(string RoomCategory)
        {
            this.RoomCategory = RoomCategory;
            this.Text += " " + RoomCategory;
            this.ShowDialog();
        }

        private void FormPrintLot_Load(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                PrinterSetting printsetting = STM.Print;
                barFull.EditValue = printsetting.PrintFullActive;
                barLabel.EditValue = printsetting.PrintLabelActive;

                barEditItemPack.EditValue = true;
                barEditItemLot.EditValue = false;
                barEditItemPD.EditValue = false;

                int length = Convert.ToInt32(STM.QueryData_ExecuteScalarProductEngineering("SELECT DayLength FROM [Configuration].LengthLotId WHERE ProgramName = 'AxCheckPack'"));//WK#1.n 20230509
                DataTable dt = STM.QueryData(string.Format(@"select distinct STMLOTIDSTM,PROJID,PROJNAME,CREATEDDATETIME
                                                from {0}
                                                where DateDiff(DAY,CREATEDDATETIME,getdate()) <= {1} --515 --200
                                                order by CREATEDDATETIME desc,STMLOTIDSTM,PROJID,PROJNAME "
                                                , STM.GetTableName, length));
                repositoryItemGridLookUpEdit1.DataSource = dt;
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

        private void barEditItem1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                if (barEditItem1.EditValue != null)
                {
                    string Lot = barEditItem1.EditValue.ToString();
//                    string sql = string.Format(@"
//                                select	distinct CONVERT(bit,0) as chk,
//		                                STMLOTIDSTM,
//		                                SHELFNO,
//		                                PRODUCTARTICLE,
//		                                CODEPACK,
//		                                case when len(CODEPACK)>=13 then SUBSTRING(CODEPACK,1,13) else CODEPACK end as OrderNumber,
//		                                STMBUILDNOJIS,
//		                                STMFLOORNOJIS,
//                                        STMROOMNOJIS,
//		                                PRODUCTQUANTITYSEQUENCETOTAL,
//                                        ROOMCATEGORY,
//		                                case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PACKQUANTITYSEQUENCETOTAL
//                                from dbo.STMSMARTPDPARTBACKDATA
//                                where STMLOTIDSTM='{0}'
//                                order by SHELFNO,PACKQUANTITYSEQUENCETOTAL ", Lot);

                    string sql = string.Format(@"
                                            DECLARE @LOTID nvarchar(200) = '{1}'
                                            DECLARE @LOCK int

                                            SELECT DISTINCT @LOCK = ISNULL(LOCK ,0)
	                                            FROM {0} room
	                                            LEFT JOIN dbo.STMROOMCATEGORY lock
		                                            ON lock.NAME = room.ROOMCATEGORY
	                                            WHERE STMLOTIDSTM = @LOTID

                                            IF (@LOCK = 1)
                                            BEGIN 
	                                            select distinct CONVERT(bit,0) as chk,
		                                                STMLOTIDSTM,
		                                                SHELFNO,
		                                                PRODUCTARTICLE,
		                                                CODEPACK,
		                                                case when len(CODEPACK)>=13 then SUBSTRING(CODEPACK,1,13) else CODEPACK end as OrderNumber,
		                                                STMBUILDNOJIS,
		                                                STMFLOORNOJIS,
                                                        STMROOMNOJIS,
		                                                PRODUCTQUANTITYSEQUENCETOTAL,
										                ROOMCATEGORY,
		                                                case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PACKQUANTITYSEQUENCETOTAL
                                                from {0}
                                                where STMLOTIDSTM = @LOTID AND CUTRITEMATERIALCODE = ''
                                                order by SHELFNO,PACKQUANTITYSEQUENCETOTAL
                                            END
                                            ELSE
                                            BEGIN
	                                            select	distinct CONVERT(bit,0) as chk,
		                                                STMLOTIDSTM,
		                                                SHELFNO,
		                                                PRODUCTARTICLE,
		                                                CODEPACK,
		                                                case when len(CODEPACK)>=13 then SUBSTRING(CODEPACK,1,13) else CODEPACK end as OrderNumber,
		                                                STMBUILDNOJIS,
		                                                STMFLOORNOJIS,
                                                        STMROOMNOJIS,
		                                                PRODUCTQUANTITYSEQUENCETOTAL,
                                                        ROOMCATEGORY,
		                                                case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PACKQUANTITYSEQUENCETOTAL
                                                from {0} 
                                                where STMLOTIDSTM=@LOTID
                                                order by SHELFNO,PACKQUANTITYSEQUENCETOTAL
                                            END
							", STM.GetTableName, Lot);
                    DataTable dt = STM.QueryData(sql);

                    gridControl1.DataSource = dt;
                    gridView1.BestFitColumns();
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

        private void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            try
            {
                PrinterSetting printsetting = STM.Print;
                STM.SplashScreenManagerManual_Show();
                gridView1.PostEditor();
                if (gridControl1.DataSource == null) return;
                bool statusprint = false;
                //Start WK#1.n 20230419
                string Lot = barEditItem1.EditValue.ToString();
                DataTable dtLock = STM.QueryData(string.Format(@"SELECT DISTINCT ROOMCATEGORY, room.LOCK, room.LOCKPRINTPACK
                                                                    FROM {0} part 
                                                                    LEFT JOIN dbo.STMROOMCATEGORY room
	                                                                    ON room.NAME = part.ROOMCATEGORY
                                                                    WHERE STMLOTIDSTM = '{1}' AND ROOMCATEGORY <> '' "
                                                                , STM.GetTableName, Lot));
                if (dtLock.Rows.Count > 0)
                {
                    if (Convert.ToInt32(dtLock.Rows[0]["LOCKPRINTPACK"]) == 1)
                    {
                        STM.MessageBoxError("Lock print pack " + dtLock.Rows[0]["ROOMCATEGORY"].ToString());
                        return;
                    }
                }

                //End WK#1.n 20230419

                
                DataTable dt = gridControl1.DataSource as DataTable;
                var chk = from r in dt.AsEnumerable()
                          where Convert.ToBoolean(r["chk"]) == true
                          select r;

                if (chk.Count() > 0)
                {
                    ////WK#1.n 20230419
                    //if (dtLock.Rows.Count > 0)
                    //{
                    //    foreach (DataRow row in chk.ToArray())
                    //    {
                    //        if (Convert.ToInt32(dtLock.Rows[0]["LOCK"]) == 1)
                    //        {
                    //            STM.MessageBoxError("Lock print copy " + dtLock.Rows[0]["ROOMCATEGORY"].ToString());
                    //            return;
                    //        }
                    //    }
                    //}

                    if (Convert.ToBoolean(barEditItemPack.EditValue))
                    {
                        PrintPack(chk); statusprint = true;
                    }

                    if (Convert.ToBoolean(barEditItemLot.EditValue))
                    {
                        PrintLotPD(chk, "Lot"); statusprint = true;
                    }

                    if (Convert.ToBoolean(barEditItemPD.EditValue))
                    {
                        PrintLotPD(chk, "PD"); statusprint = true;
                    }

                    if (!statusprint)
                    {
                        STM.MessageBoxError("กรุณาเลือกเอกสารที่ต้องการปริ้น?");
                    }
                }
                else
                {
                    STM.MessageBoxError("ไม่พบข้อมูล");
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

        //WK#1.n 20240831
        private void PrintPack(EnumerableRowCollection<DataRow> chk)
        {
            if (STM.MessageBoxConfirm("Confirm Print")) return;

            Report.FormFull frm = new Report.FormFull();

            foreach (DataRow row in chk.ToArray())
            {
                string _roomCategory = "";
                _roomCategory = STM.QueryData_ExecuteScalar(string.Format("SELECT DISTINCT ROOMCATEGORY FROM {0} WHERE STMLOTIDSTM = '{1}'", STM.GetTableName, row["STMLOTIDSTM"].ToString())).ToString();

                if (Convert.ToBoolean(barFull.EditValue) == true)
                {
                    frm.ShowDialog(row["STMLOTIDSTM"].ToString(), row["CODEPACK"].ToString(), _roomCategory, PrintType.Full, DateTime.Now);
                }
                if (Convert.ToBoolean(barLabel.EditValue) == true)
                {
                    frm.ShowDialog(row["STMLOTIDSTM"].ToString(), row["CODEPACK"].ToString(), _roomCategory, PrintType.Label, DateTime.Now);
                }
            }

            STM.MessageBoxInformation("Complete");
        }

        //WK#1.n 20240831
        private void PrintLotPD(EnumerableRowCollection<DataRow> chk, string print = "")
        {
            FormStatusRemark frm = new FormStatusRemark();
            frm.ShowDialog(print);

            string status = "";

            if (frm.DialogResult == DialogResult.OK)
            {
                status = frm.Text.ToString();

                foreach (DataRow row in chk.ToArray())
                {
                    string sql = string.Format(@"
                        SELECT DISTINCT smart.STMLOTIDSTM AS Lot,
		                        CODEPACK,
		                        case when len(CODEPACK)>=13 then SUBSTRING(CODEPACK,1,13) else CODEPACK end as OrderNumber,
		                        STMBUILDNOJIS AS Build,
		                        STMFLOORNOJIS AS Floor,
                                STMROOMNOJIS AS Room,
                                ROOMCATEGORY AS Category,
				                ItemId,
				                PROJNAME AS Project,
				                pdtb.PRODPOOLID AS Warehouse,
				                FORMAT(CONVERT(DATE,CONVERT(varchar, YEAR(pdtb.ENDDATE) + 543) + '-' + CONVERT(varchar, MONTH(pdtb.ENDDATE)) + '-' + CONVERT(varchar, DAY(pdtb.ENDDATE))), 'dd/MM/yyyy') AS WH_Date,
				                pdtb.CATREMARK AS Remark,
                                FORMAT(CONVERT(DATE,CONVERT(varchar, YEAR(GETDATE()) + 543) + '-' + CONVERT(varchar, MONTH(GETDATE())) + '-' + CONVERT(varchar, DAY(GETDATE()))), 'dd/MM/yyyy') AS CurrentDate,
CONVERT(bit, 0) AS chkCut,
CONVERT(bit, 0) AS chkEdge,
CONVERT(bit, 0) AS chkDrill,
CONVERT(bit, 0) AS chkCNC,
CONVERT(bit, 0) AS chkAssembly,
CONVERT(bit, 0) AS chkAluminium,
CONVERT(bit, 0) AS chkSpecial,
CONVERT(bit, 0) AS chkCoating,
CONVERT(bit, 0) AS chkColor,
CONVERT(bit, 0) AS chkPack,
CONVERT(bit, 0) AS chkWH,
CONVERT(bit, 0) AS chkOther,
CONVERT(bit, 0) AS chkFiber
                        FROM [dbo].[STMSMARTPDPARTSTM] smart
		                LEFT JOIN STMSMARTPDTABLESTM pdtb
			                ON pdtb.STMLOTIDSTM = smart.STMLOTIDSTM
                        WHERE smart.STMLOTIDSTM='{0}' AND CODEPACK = '{1}'"
                        , row["STMLOTIDSTM"].ToString(), row["CODEPACK"].ToString());

                    DataTable dtReport = STM.QueryData(sql);
                    int total = STM.PrintCopy;
                    dtReport.Columns.Add(new DataColumn("DateWH"));
                    dtReport.Columns.Add(new DataColumn("Status"));
                    dtReport.Columns.Add(new DataColumn("Palette"));
                  
                    foreach (DataRow rowLot in dtReport.Rows)
                    {
                        if (print == "Lot")
                        {
                            rowLot["OrderNumber"] = "";
                            rowLot["ItemId"] = "";
                            rowLot["Build"] = "";
                            rowLot["Floor"] = "";
                            rowLot["Room"] = "";
                        }

                        foreach (var item in STM.ProcessSelected)
                        {
                            if (item.ProcessChk == 1)
                                rowLot["chkCut"] = true;
                            if (item.ProcessChk == 2)
                                rowLot["chkEdge"] = true;
                            if (item.ProcessChk == 3)
                                rowLot["chkDrill"] = true;
                            if (item.ProcessChk == 4)
                                rowLot["chkCNC"] = true;
                            if (item.ProcessChk == 5)
                                rowLot["chkAssembly"] = true;
                            if (item.ProcessChk == 6)
                                rowLot["chkAluminium"] = true;
                            if (item.ProcessChk == 7)
                                rowLot["chkSpecial"] = true;
                            if (item.ProcessChk == 8)
                                rowLot["chkCoating"] = true;
                            if (item.ProcessChk == 9)
                                rowLot["chkColor"] = true;
                            if (item.ProcessChk == 10)
                                rowLot["chkPack"] = true;
                            if (item.ProcessChk == 11)
                                rowLot["chkWH"] = true;
                            if (item.ProcessChk == 12)
                                rowLot["chkFiber"] = true;
                        }

                        rowLot["Palette"] = string.Format("{0}/{1}", 1, total);
                        rowLot["CurrentDate"] = rowLot["CurrentDate"].ToString(); // Convert.ToDateTime(rowLot["CurrentDate"]).ToString();//.ToString("dd/MM/yyyy", new CultureInfo("th-TH"));
                        rowLot["DateWH"] = rowLot["WH_Date"].ToString();//Convert.ToDateTime(rowLot["WH_Date"]).ToString("dd/MM/yyyy", new CultureInfo("th-TH"));
                        rowLot["Status"] = status;
                    }

                    #region Print copy
                    int seq = 2;
                    DataTable dtPrint = dtReport.Copy();
                    for (int i = 1; i < total; i++)
                    {
                        
                        foreach (DataRow rowCopy in dtReport.Rows)
                        {
                            
                            DataRow newRow = dtPrint.NewRow();
                            newRow["Palette"] = string.Format("{0}/{1}", seq, total);
                            newRow["Lot"] = rowCopy["Lot"];
                            newRow["CODEPACK"] = rowCopy["CODEPACK"];
                            newRow["OrderNumber"] = rowCopy["OrderNumber"];
                            newRow["Build"] = rowCopy["Build"];
                            newRow["Floor"] = rowCopy["Floor"];
                            newRow["Room"] = rowCopy["Room"];
                            newRow["Category"] = rowCopy["Category"];
                            newRow["ItemId"] = rowCopy["ItemId"];
                            newRow["Project"] = rowCopy["Project"];
                            newRow["Warehouse"] = rowCopy["Warehouse"];
                            newRow["WH_Date"] = rowCopy["WH_Date"];
                            newRow["Remark"] = rowCopy["Remark"];
                            newRow["CurrentDate"] = rowCopy["CurrentDate"];
                            newRow["DateWH"] = rowCopy["DateWH"];
                            newRow["Status"] = rowCopy["Status"];
                            newRow["chkCut"] = rowCopy["chkCut"];
                            newRow["chkEdge"] = rowCopy["chkEdge"];
                            newRow["chkDrill"] = rowCopy["chkDrill"];
                            newRow["chkCNC"] = rowCopy["chkCNC"];
                            newRow["chkAssembly"] = rowCopy["chkAssembly"];
                            newRow["chkAluminium"] = rowCopy["chkAluminium"];
                            newRow["chkSpecial"] = rowCopy["chkSpecial"];
                            newRow["chkCoating"] = rowCopy["chkCoating"];
                            newRow["chkColor"] = rowCopy["chkColor"];
                            newRow["chkPack"] = rowCopy["chkPack"];
                            newRow["chkWH"] = rowCopy["chkWH"];
                            newRow["chkOther"] = rowCopy["chkOther"];
                            newRow["chkFiber"] = rowCopy["chkFiber"];
                            dtPrint.Rows.Add(newRow);
                            seq++;
                        }
                    }
                    #endregion

                    Report.FormPreviewPDLot frmLabel = new FormPreviewPDLot();

                    PrinterSetting printsetting = STM.Print;
                    Report.CrystalReportLabelPDLot rpt = new CrystalReportLabelPDLot();
                    rpt.SetDataSource(dtPrint);

                    System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                    if (printsetting.PrintLabel != "")
                        printerSettings.PrinterName = printsetting.PrintLabel;

                    rpt.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                    rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
                    //rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

                    frmLabel.ShowDialog(rpt);

                    rpt.Close();
                    rpt.Dispose();
                }
            }
        }

        //WK#1.n 20240831
        private void PrintPD(EnumerableRowCollection<DataRow> chk)
        {

        }

        private void gridView1_RowCountChanged(object sender, EventArgs e)
        {
            textEdit1.Text = gridView1.RowCount.ToString("#,##0");
        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {
            if (gridControl1.DataSource != null)
            {
                foreach (DataRow row in (gridControl1.DataSource as DataTable).Rows)
                {
                    row["chk"] = checkEdit1.Checked;
                }
            }
        }



    }
}
