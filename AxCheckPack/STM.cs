using AxCheckPack.Report;
using DevExpress.Utils.Menu;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraSplashScreen;
using OnBarcode.Barcode;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    class STM
    {
        private static GridView grid = null;
        private static string app_name = "Production";
        private static string path_log = @"\\10.11.0.2\Temporary Documents\Production";
        private static string export_filename1 = "ExportExcel";

        private static GridView _GridView
        {
            set
            {
                grid = value;
            }
            get
            {
                return grid;
            }
        }
        public static void gridView_ExportXLSX_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            if (e.MenuType == GridMenuType.Row)
            {
                GridView view = sender as GridView;
                _GridView = null;
                _GridView = view;
                int rowHandle = e.HitInfo.RowHandle;
                e.Menu.Items.Clear();
                e.Menu.Items.Add(CreateRowExportExcel(view, rowHandle));
            }
        }
        public static void gridView_ExportXLSX_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e, string export_filename)
        {
            export_filename1 = export_filename;
            if (e.MenuType == GridMenuType.Row)
            {
                GridView view = sender as GridView;
                _GridView = null;
                _GridView = view;
                int rowHandle = e.HitInfo.RowHandle;
                e.Menu.Items.Clear();
                e.Menu.Items.Add(CreateRowExportExcel(view, rowHandle));
            }
        }
        private static DXMenuItem CreateRowExportExcel(GridView view, int rowHandle)
        {
            Image img2 = Image.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ProductEngineeringProduction.image.Iconstoc-Ms-Office-2013-Excel.ico"));
            DXMenuItem menuItemFilterRow = new DXMenuItem("Export Excel", new EventHandler(OnCreateRowReportExport_Click), (Image)(new Bitmap(img2, new Size(16, 16))));
            menuItemFilterRow.Tag = new RowInfo(view, rowHandle);
            return menuItemFilterRow;
        }
        private static void OnCreateRowReportExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = export_filename1 + ".xlsx";
            saveFileDialog1.Filter = "Excel |*.xlsx";
            saveFileDialog1.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (DialogResult.Cancel == saveFileDialog1.ShowDialog()) { return; }

            string file_name = saveFileDialog1.FileName;
            _GridView.ExportToXlsx(file_name);
            if (XtraMessageBox.Show(string.Format("ต้องการเปิดไฟล์ '{0}' หรือไม่", file_name), "MessageBox", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                System.Diagnostics.Process.Start(file_name);
        }
        private static List<Process> process_chk;
        public static List<Process> ProcessSelected
        {
            get { return process_chk; }
            set { process_chk = value; }
        }

        private static int print_copy;
        public static int PrintCopy
        {
            get { return print_copy; }
            set { print_copy = value; }
        }

        public static string ConnectionString
        {
            get
            {
                string strCon = "";

                if (System.Configuration.ConfigurationManager.ConnectionStrings["AxConnection"] != null)
                {
                    strCon = System.Configuration.ConfigurationManager.ConnectionStrings["AxConnection"].ConnectionString.ToString();

                    strCon += string.Format(" ; Application Name=CheckPack {0} ", STM.GetLoginName);
                }

                return strCon;
            }
        }
        public static string ConnectionStringProductEngineering
        {
            get
            {
                string strCon = "";

                if (System.Configuration.ConfigurationManager.ConnectionStrings["ProductEngineering"] != null)
                {
                    strCon = System.Configuration.ConfigurationManager.ConnectionStrings["ProductEngineering"].ConnectionString.ToString();

                    strCon += string.Format(" ; Application Name=CheckPack {0} ", STM.GetLoginName);
                }

                return strCon;
            }
        }

        public static string ConnectionStringDataAccess(string filename)
        {
            string strCon = string.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0}; Mode=ReadWrite; Persist Security Info=False;", filename);
            return strCon;
        }


        public static void MessageBoxError(string msg)
        {
            STM.SplashScreenManagerManual_Hide();
            //XtraMessageBox.Show(msg, "MessageBox", MessageBoxButtons.OK, MessageBoxIcon.Error);

            new FormMessageBox().ShowDialog(msg);
        }
        public static void MessageBoxError(Exception ex)
        {
            STM.MessageBoxError(ex.Message);
        }
        public static void MessageBoxInformation(string msg)
        {
            STM.SplashScreenManagerManual_Hide();
            XtraMessageBox.Show(msg, "MessageBox", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static bool MessageBoxConfirm(string msg)
        {
            STM.SplashScreenManagerManual_Hide();
            return XtraMessageBox.Show(msg, "MessageBox", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel ? true : false;
        }
        public static bool MessageBoxConfirm(string msg, MessageBoxIcon icon)
        {
            STM.SplashScreenManagerManual_Hide();
            return XtraMessageBox.Show(msg, "MessageBox", MessageBoxButtons.OKCancel, icon) == DialogResult.Cancel ? true : false;
        }

        /// <summary>
        /// MessageBox Question Return DialogResult YesNo
        /// </summary>
        public static System.Windows.Forms.DialogResult MessageBoxQuestion(string msg)
        {
            return XtraMessageBox.Show(msg, "MessageBox", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question);
        }

        public static string AppName
        {
            get
            {
                return app_name;
            }
            set
            {
                app_name = value;
            }
        }
        public static string GetComputerName
        {
            get
            {
                try
                {
                    return System.Environment.MachineName.Trim();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }

        public static string GetLoginName
        {
            get
            {
                try
                {
                    return Environment.UserName.Trim();
                }
                catch (Exception)
                {
                    return "";
                }
            }
        }
        public static DateTime DateTimeDB
        {
            get
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = ConnectionString;
                con.Open();
                SqlDataAdapter ad = new SqlDataAdapter(" select GETDATE() [Datetime] ", con);
                DataTable dt = new DataTable();
                ad.Fill(dt);

                con.Close();
                con.Dispose();

                DateTime datetime = Convert.ToDateTime(dt.Rows[0]["Datetime"]);

                return datetime;
            }
        }
        public static string GetDataBaseName
        {
            get
            {
                SqlConnection con = new SqlConnection(STM.ConnectionString);
                return con.Database;
            }
        }


        /// <summary>
        /// Read file excel to datatable
        /// </summary>
        /// <param name="filePath">ไฟล์ excel</param>
        /// <param name="isFirstRowHasHeader">แถวแรกเป็นคอลั่มหรือไม่</param>
        public static DataTable ReadExcel(string filePath, bool isFirstRowHasHeader)
        {
            DataTable dtexcel = new DataTable();
            bool hasHeaders = false;
            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            DataRow schemaRow = schemaTable.Rows[0];
            string sheet = schemaRow["TABLE_NAME"].ToString();
            if (!sheet.EndsWith("_"))
            {
                string query = "SELECT  * FROM [" + sheet + "]";
                OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
                dtexcel.Locale = System.Globalization.CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
            }

            conn.Close();
            return dtexcel;
        }
        public static DataTable ReadExcel(string filePath)
        {
            DataTable dtexcel = new DataTable();
            bool hasHeaders = false;
            string HDR = hasHeaders ? "Yes" : "No";
            string strConn;
            if (filePath.Substring(filePath.LastIndexOf('.')).ToLower() == ".xlsx")
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + filePath + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
            else
                strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties=\"Excel 8.0;HDR=" + HDR + ";IMEX=0\"";
            OleDbConnection conn = new OleDbConnection(strConn);
            conn.Open();
            DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            DataRow schemaRow = schemaTable.Rows[0];
            string sheet = schemaRow["TABLE_NAME"].ToString();
            if (!sheet.EndsWith("_"))
            {
                string query = "SELECT  * FROM [" + sheet + "]";
                OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
                dtexcel.Locale = System.Globalization.CultureInfo.CurrentCulture;
                daexcel.Fill(dtexcel);
            }

            conn.Close();
            return dtexcel;
        }
        public static string msg = "";
        

        public static string GetTableName
        {
            get
            {
                string table_name = "[dbo].[STMSMARTPDPARTBACKDATA]";
                //string table_name = "[dbo].[STMSMARTPDPARTSTM]";
                return table_name;
            }
        }

        public static DataTable QueryDataConfig(string sql)
        {
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
            con.Open();

            SqlDataAdapter ad = new SqlDataAdapter(sql, con);

            ad.Fill(dt);

            con.Close();

            return dt;
        }
        public static DataTable QueryData(string sql)
        {

            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            con.Open();

            SqlDataAdapter ad = new SqlDataAdapter(sql, con);

            ad.Fill(dt);

            con.Close();

            return dt;
        }
        public static DataTable QueryData(string sql, List<SqlParameter> lsParam)
        {
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter ad = new SqlDataAdapter();

            con.Open();
            cmd.Connection = con;
            cmd.CommandText = sql;
            cmd.Parameters.Clear();
            cmd.Parameters.AddRange(lsParam.ToArray());
            ad.SelectCommand = cmd;
            ad.Fill(dt);

            con.Close();

            return dt;
        }
        public static DataTable QueryDataProductEngineering(string sql)
        {
            DataTable dt = new DataTable();
            SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
            con.Open();

            SqlDataAdapter ad = new SqlDataAdapter(sql, con);

            ad.Fill(dt);

            con.Close();

            return dt;
        }

        public static object QueryData_ExecuteScalar(string sql)
        {
            SqlConnection con = new SqlConnection(STM.ConnectionString);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = sql;

            object o = cmd.ExecuteScalar();

            con.Close();

            return o;
        }

        public static object QueryData_ExecuteScalarProductEngineering(string sql)
        {
            SqlConnection con = new SqlConnection(STM.ConnectionStringProductEngineering);
            con.Open();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandText = sql;

            object o = cmd.ExecuteScalar();

            con.Close();

            return o;
        }


        public static byte[] CreateBarcode(string Endcode, bool ShowText)
        {

            byte[] img = null;

            Linear barcode1 = new Linear();
            barcode1.Type = BarcodeType.CODE128;
            barcode1.ShowText = ShowText;
            barcode1.AutoResize = true;


            if (Endcode != "")
            {
                barcode1.Data = Endcode;
                img = barcode1.drawBarcodeAsBytes();
            }

            return img;
        }

        public static void SplashScreenManagerManual_Show()
        {
            try
            {

                Cursor.Current = Cursors.WaitCursor;
                SplashScreenManagerManual_Hide();
            }
            catch (Exception) { }
        }
        public static void SplashScreenManagerManual_Show(bool ShowWaitForm)
        {
            try
            {

                Cursor.Current = Cursors.WaitCursor;
                SplashScreenManagerManual_Hide();

                if (ShowWaitForm)
                    SplashScreenManager.ShowForm(Form.ActiveForm, typeof(SplashScreen1), true, true, false);
            }
            catch (Exception) { }
        }
        public static void SplashScreenManagerManual_Hide()
        {
            try
            {

                SplashScreenManager.CloseForm();
                Cursor.Current = Cursors.Default;
            }
            catch (Exception) { Cursor.Current = Cursors.Default; }
        }

        public static PrinterSetting Print
        {
            get
            {
                PrinterSetting printsetting = new PrinterSetting()
                {
                    PrintFull = "",
                    PrintLabel = "",
                    PrintFullActive = false,
                    PrintLabelActive = false,
                    LabelSize = "small"
                };

                DataTable dt = STM.QueryDataConfig(string.Format(@" SELECT [PrintFull],[PrintLabel],[PrintFullActive],[PrintLabelActive],isnull(LabelSize,'small') as LabelSize
                                                                    FROM [dbo].[AxCheckConfig]
                                                                    where [ComputerName]='{0}'  ", STM.GetComputerName));
                if (dt != null && dt.Rows.Count > 0)
                {
                    printsetting.PrintFull = dt.Rows[0]["PrintFull"].ToString();
                    printsetting.PrintLabel = dt.Rows[0]["PrintLabel"].ToString();
                    printsetting.LabelSize = dt.Rows[0]["LabelSize"].ToString();

                    printsetting.PrintFullActive = dt.Rows[0]["PrintFullActive"].ToString() == "1" ? true : false; 
                    printsetting.PrintLabelActive = dt.Rows[0]["PrintLabelActive"].ToString() == "1" ? true : false;

                }

                return printsetting;
            }
        }

        public static bool CheckBarcodeChangePalette(string barcode)
        {
            bool isNewPack = false;
            if (barcode.ToUpper() == "SMARTPACKINCREASE001")
            {
                isNewPack = true;
            }

            return isNewPack;
        }

        //WK#1.n 20230505
        public static int LockSetting(int function, string category)
        {
            int _lock = 0;

            DataTable dtLock = STM.QueryData(string.Format(@"select Lock, LockPrintPack, CodePart, Name from dbo.STMROOMCATEGORY WHERE NAME = '{0}'  order by SEQ ", category));
            foreach (DataRow row in dtLock.Rows)
            {
                if (function == 1)
                {
                    _lock = Convert.ToInt32(row["LockPrintPack"]);
                }
                else if (function == 2)
                {
                    _lock = Convert.ToInt32(row["Lock"]);
                }
                else if (function == 3)
                {
                    _lock = Convert.ToInt32(row["CodePart"]);
                }
            }

            return _lock;
        }
    }
}

class PrinterSetting
{
    public string PrintFull { set; get; }
    public string PrintLabel { set; get; }
    public bool PrintLabelActive { set; get; }
    public bool PrintFullActive { set; get; }
    public string LabelSize { set; get; }
}

class RowInfo
{
    public RowInfo(GridView view, int rowHandle)
    {
        this.RowHandle = rowHandle;
        this.View = view;
    }
    public GridView View;
    public int RowHandle;

}

public enum PrintType
{
    Label,
    Full,
    DIY,
    Compack
}

