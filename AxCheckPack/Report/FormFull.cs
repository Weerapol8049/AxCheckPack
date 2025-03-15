using DevExpress.XtraEditors;
using OnBarcode.Barcode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace AxCheckPack.Report
{
    public partial class FormFull : XtraForm
    {
        PrintType printtype = PrintType.Full;

        public FormFull()
        {
            InitializeComponent();
        }

        private void FormFull_Load(object sender, EventArgs e)
        {

        }

        public void ShowDialog(string Lot, string barcodepack, string RoomCategory, PrintType printtype, DateTime ProductionDate)
        {
            try
            {
                STM.SplashScreenManagerManual_Show();

                txtLot.Text = Lot;
                txtBarcodePack.Text = barcodepack;
                this.printtype = printtype;
                DataTable dtData = new DataTable();
          
                dtData = LoadData();
                    dtData.Columns.AddRange(new DataColumn[] { 
                    new DataColumn("BarcodePackImg",typeof(byte[]))
                });
                
                string endcode = "";
                byte[] img = null;
                Linear barcode1 = new Linear();
                barcode1.Type = BarcodeType.CODE128;
                barcode1.ShowText = false;
                barcode1.AutoResize = true;
                barcode1.BarcodeHeight = 100;

                foreach (DataRow row in dtData.Rows)
                {
                    row["Category"] = RoomCategory;

                    img = null;
                    endcode = row["BarcodePack"].ToString();
                    if (endcode != "")
                    {
                        barcode1.Data = endcode;
                        img = barcode1.drawBarcodeAsBytes();
                        row["BarcodePackImg"] = img;
                    }
                }

                PrinterSetting printsetting = STM.Print;

                if (printtype == PrintType.Full)
                {
                    if (printsetting.PrintFullActive == true)
                    {
                        Report.CrystalReportFull rpt = new CrystalReportFull();
                        rpt.SetDataSource(dtData);

                        System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();

                        if (printsetting.PrintFull != "")
                            printerSettings.PrinterName = printsetting.PrintFull;

                        rpt.PrintOptions.PaperSize = CrystalDecisions.Shared.PaperSize.PaperA4;
                        rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
                        rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

                        rpt.Close();
                        rpt.Dispose();
                    }
                }
                else if (printtype == PrintType.Label)
                {
                    if (printsetting.PrintLabelActive)
                    {
                        if (dtData.Columns.Contains("Qrcode"))
                        {
                            dtData.Columns.Remove("Qrcode");
                        }
                        dtData.Columns.Add(new DataColumn("Qrcode",typeof(byte[])));

                        if (dtData.Columns.Contains("CreateDate"))
                        {
                            dtData.Columns.Remove("CreateDate");
                        }
                        dtData.Columns.Add(new DataColumn("CreateDate"));

                        DataTable dtDataLabel = dtData.Clone();
                        if (dtData.Rows.Count > 0)
                        {
                            string BarcodePack = dtData.Rows[0]["BarcodePack"].ToString();
                            dtData.Rows[0]["Qrcode"] = GenQR(BarcodePack);
                            dtData.Rows[0]["CreateDate"] = Convert.ToDateTime(dtData.Rows[0]["CREATEDDATETIME"]).ToString("dd/MM/yyyy HH:mm", new CultureInfo("th-TH"));
                            
                            dtDataLabel.ImportRow(dtData.Rows[0]);
                        }

                        if (printsetting.LabelSize == "small")
                        {

                            Report.CrystalReportLabel rpt = new CrystalReportLabel();
                            rpt.SetDataSource(dtDataLabel);

                            System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                            if (printsetting.PrintLabel != "")
                                printerSettings.PrinterName = printsetting.PrintLabel;

                            rpt.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                            rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
                            rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

                            rpt.Close();
                            rpt.Dispose();
                        }
                        else
                        {
                            //Report.CrystalReportLabel2 rpt = new CrystalReportLabel2();
                            Report.CrystalReportLabel2_DS rpt = new CrystalReportLabel2_DS();
                            rpt.SetDataSource(dtDataLabel);

                            System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                            if (printsetting.PrintLabel != "")
                                printerSettings.PrinterName = printsetting.PrintLabel;

                            rpt.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                            rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
                            rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

                            rpt.Close();
                            rpt.Dispose();
                        }
                    }
                }
                else if (printtype == PrintType.Compack || printtype == PrintType.DIY)
                {

                    string ProductArticle = dtData.Rows[0]["ProductArticle"].ToString();
                    string ITEMID = dtData.Rows[0]["ITEMID"].ToString();
                    string Barcode = dtData.Rows[0]["BarcodePack"].ToString();

                    DataTable dtprint = new DataTable();
                    dtprint.Columns.AddRange(new DataColumn[] {
                                            new DataColumn("Seq"),
                                            new DataColumn("Model_TH"),
                                            new DataColumn("Size_TH"),
                                            new DataColumn("Weight_TH"),
                                            new DataColumn("Manufacture_TH"),
                                            new DataColumn("Address_TH"),
                                            new DataColumn("Country_TH"),
                                            new DataColumn("Date_TH",typeof(DateTime)),
                                            new DataColumn("Model_EN"),
                                            new DataColumn("Size_EN"),
                                            new DataColumn("Weight_EN"),
                                            new DataColumn("Manufacture_EN"),
                                            new DataColumn("Address_EN"),
                                            new DataColumn("Country_EN"),
                                            new DataColumn("Date_EN",typeof(DateTime)),
                                            new DataColumn("Root"),
                                            new DataColumn("PathFile"),
                                            new DataColumn("QrCode",typeof(byte[])),
                                            new DataColumn("PDNumberImage",typeof(byte[])),
                                            new DataColumn("CodeNumber"),
                                            new DataColumn("ItemSetId"),
                                            new DataColumn("ProductSeqTotal")//WK#1.n 20221207
                        });

                    DataTable dtAssembly = GetAssembly(ITEMID, ProductArticle);

                    string productSeqTotal = "";//WK#1.n 20221207
                    object ob = STM.QueryData_ExecuteScalar(string.Format(@"
                                                select top 1  PRODUCTQUANTITYSEQUENCETOTAL 
                                                from {0}
                                                where STMLOTIDSTM = '{1}' AND ITEMID = '{2}' and productarticle = '{3}'"
                                                , STM.GetTableName, Lot, ITEMID, ProductArticle));//WK#1.n 20221207
                    if (ob != null)//WK#1.n 20221207
                        productSeqTotal = ob.ToString();//WK#1.n 20221207

                    if (dtAssembly != null && dtAssembly.Rows.Count > 0)
                    {
                        DataRow row = dtAssembly.Rows[0];
                        DataRow row_insert = dtprint.NewRow();
                        row_insert["Seq"] = row["Seq"];
                        
                        row_insert["Model_TH"] = row["Model_TH"];
                        row_insert["Size_TH"] = row["Size_TH"];
                        row_insert["Weight_TH"] = row["Weight_TH"];
                        row_insert["Manufacture_TH"] = row["Manufacture_TH"];
                        row_insert["Address_TH"] = row["Address_TH"];
                        row_insert["Country_TH"] = row["Country_TH"];
                        row_insert["Date_TH"] = ProductionDate;

                        row_insert["Model_EN"] = row["Model_EN"];
                        row_insert["Size_EN"] = row["Size_EN"];
                        row_insert["Weight_EN"] = row["Weight_EN"];
                        row_insert["Manufacture_EN"] = row["Manufacture_EN"];
                        row_insert["Address_EN"] = row["Address_EN"];
                        row_insert["Country_EN"] = row["Country_EN"];
                        row_insert["Date_EN"] = ProductionDate;

                        row_insert["Root"] = row["Root"];
                        row_insert["PathFile"] = row["PathFile"];
                        row_insert["CodeNumber"] = Barcode;
                        row_insert["ItemSetId"] = ProductArticle;

                        row_insert["QrCode"] = GenQR(row["Root"].ToString());
                        row_insert["PDNumberImage"] = GenBar(Barcode);

                        if (printtype == PrintType.DIY)
                            row_insert["ProductSeqTotal"] = "";//WK#1.n 20231129
                        else
                            row_insert["ProductSeqTotal"] = productSeqTotal;//WK#1.n 20221207

                        dtprint.Rows.Add(row_insert);

                        Report.AssemblyLabel.CrystalReportQrCode rpt = new Report.AssemblyLabel.CrystalReportQrCode();
                        rpt.SetDataSource(dtprint);

                        System.Drawing.Printing.PrinterSettings printerSettings = new System.Drawing.Printing.PrinterSettings();
                        if (printsetting.PrintLabel != "")
                            printerSettings.PrinterName = printsetting.PrintLabel;

                        rpt.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                        rpt.PrintOptions.PaperOrientation = CrystalDecisions.Shared.PaperOrientation.Portrait;
                        rpt.PrintToPrinter(printerSettings, new System.Drawing.Printing.PageSettings(), false);

                        rpt.Close();
                        rpt.Dispose();

                    }
                    else
                    {
                        STM.MessageBoxError("ไม่พบ Assemble");
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
            }
        }

        private DataTable LoadData()
        {
            string sql = "";
            if (printtype == PrintType.Full)
            {
                sql = string.Format(@"  declare @STMLOTIDSTM varchar(50)='{1}',
		                                        @CODEPACK varchar(50)='{2}'

                                        SELECT	p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'ห้องครัว' as Category,
                           
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        [PRODUCTQUANTITYSEQUENCETOTAL] as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
		                                        ProductArticle,
		                                        PartName,
		                                        [CODEPART] as BarcodePart,
		                                        FinishWidth,
		                                        FinishLength,
		                                        p.ShelfNo
                                                ,tb.CATREMARK as Remark 
                                        FROM {0} p  with(nolock) 
                                        LEFT JOIN dbo.STMSMARTPDTABLESTM tb 
										 ON tb.STMLOTIDSTM = p.STMLOTIDSTM 
                                        where p.STMLOTIDSTM=@STMLOTIDSTM and p.CODEPACK=@CODEPACK "
                                        , STM.GetTableName, txtLot.Text, txtBarcodePack.Text);
            }
            else if (printtype == PrintType.DIY)
            {
                sql = string.Format(@"  declare @STMLOTIDSTM varchar(50)='{1}',
		                                        @CODEPACK varchar(50)='{2}'

                                        SELECT distinct	p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'DIY' as Category,
                                               
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
                                                ProductArticle,
		                                        --CASE WHEN ISNULL(library.Name_TH, '') = '' THEN ISNULL(article.Name_TH, '') ELSE ISNULL(library.Name_TH, '') END AS NameTH, 
		                                        '' AS NameTH,
'' as PartName,
		                                        '' as BarcodePart,
		                                        '' as FinishWidth,
		                                        '' as FinishLength,
		                                        p.ShelfNo,
                                                ITEMID
                                                ,tb.CATREMARK as Remark 
                                                ,GETDATE() AS CREATEDDATETIME
                                        FROM {0} p  with(nolock) 
                                         LEFT JOIN dbo.STMSMARTPDTABLESTM tb
										 ON tb.STMLOTIDSTM = p.STMLOTIDSTM 
                                        LEFT JOIN (SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[mo].[ModularV2]
	                                               UNION
	                                               SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[sd].[StandAloneV2]) library
	                                        ON library.CODE = p.PRODUCTARTICLE
                                        LEFT JOIN [10.11.0.6].ProductEngineering.pd.ProductionArticle article
	                                        ON article.CODE = p.PRODUCTARTICLE
                                        where p.STMLOTIDSTM=@STMLOTIDSTM and p.CODEPACK=@CODEPACK "
                                        , STM.GetTableName, txtLot.Text, txtBarcodePack.Text);
            }
            else if (printtype == PrintType.Compack)
            {
                sql = string.Format(@"  declare @STMLOTIDSTM varchar(50)='{1}',
		                                        @CODEPACK varchar(50)='{2}'

                                        SELECT distinct	p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'Compack' as Category,
                                       
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
                                                ProductArticle,
		                                        --CASE WHEN ISNULL(library.Name_TH, '') = '' THEN ISNULL(article.Name_TH, '') ELSE ISNULL(library.Name_TH, '') END AS NameTH, 
		                   '' AS NameTH,                     
'' as PartName,
		                                        '' as BarcodePart,
		                                        '' as FinishWidth,
		                                        '' as FinishLength,
		                                        p.ShelfNo,
                                                ITEMID
                                                ,tb.CATREMARK as Remark 
                                                ,GETDATE() AS CREATEDDATETIME
                                        FROM {0} p  with(nolock) 
                                        LEFT JOIN dbo.STMSMARTPDTABLESTM tb 
										 ON tb.STMLOTIDSTM = p.STMLOTIDSTM 
                                        LEFT JOIN (SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[mo].[ModularV2]
	                                                    UNION
	                                                    SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[sd].[StandAloneV2]) library
	                                        ON library.CODE = p.PRODUCTARTICLE
                                        LEFT JOIN [10.11.0.6].ProductEngineering.pd.ProductionArticle article
	                                        ON article.CODE = p.PRODUCTARTICLE
                                        where p.STMLOTIDSTM=@STMLOTIDSTM and p.CODEPACK=@CODEPACK "
                                        , STM.GetTableName, txtLot.Text, txtBarcodePack.Text);
            }
            else
            {
                sql = string.Format(@"  declare @STMLOTIDSTM varchar(50)='{1}',
		                                        @CODEPACK varchar(50)='{2}'

                                        SELECT distinct p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'ห้องครัว' as Category,
                                    
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
                                                ProductArticle,
		                                        --CASE WHEN ISNULL(library.Name_TH, '') = '' THEN ISNULL(article.Name_TH, '') ELSE ISNULL(library.Name_TH, '') END AS NameTH, 
'' AS NameTH,
		                                        '' as PartName,
		                                        '' as BarcodePart,
		                                        '' as FinishWidth,
		                                        '' as FinishLength,
		                                        p.ShelfNo
												,tb.CATREMARK as Remark 
                                                ,GETDATE() AS CREATEDDATETIME
                                        FROM {0} p  with(nolock) 
										LEFT JOIN dbo.STMSMARTPDTABLESTM tb 
										    ON tb.STMLOTIDSTM = p.STMLOTIDSTM 
                                        LEFT JOIN (SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[mo].[ModularV2]
	                                                    UNION
	                                                    SELECT CODE, NAME_TH FROM [10.11.0.6].Library.[sd].[StandAloneV2]) library
	                                        ON library.CODE = p.PRODUCTARTICLE
                                        LEFT JOIN [10.11.0.6].ProductEngineering.pd.ProductionArticle article
	                                        ON article.CODE = p.PRODUCTARTICLE
                                        where p.STMLOTIDSTM=@STMLOTIDSTM and p.CODEPACK=@CODEPACK  "
                                        , STM.GetTableName, txtLot.Text, txtBarcodePack.Text);
                
            }
            DataTable dt = STM.QueryData(sql);

            return dt;

        }

        private DataTable LoadDataSplitPack(string codepart)
        {
            string sql = "";
            if (printtype == PrintType.Full)
            {
                sql = string.Format(@"  
                            SELECT	p.[PROJNAME] as Project,
		                            [STMBUILDNOJIS] as Build,
		                            [STMFLOORNOJIS] as Floor,
		                            [STMROOMNOJIS] as Room,
		                            'ห้องครัว' as Category,
		                            ITEMID as Type,
		                            p.CODEPACK as BarcodePack,
		                            [PRODUCTQUANTITYSEQUENCETOTAL] as ProductSeqTotal,
		                            case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                            count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
		                            ProductArticle,
		                            PartName,
		                            [CODEPART] as BarcodePart,
		                            FinishWidth,
		                            FinishLength,
		                            p.ShelfNo
                                    ,tb.CATREMARK as Remark 
                            FROM {0} p  with(nolock) 
                            LEFT JOIN dbo.STMSMARTPDTABLESTM tb 
		                        ON tb.STMLOTIDSTM = p.STMLOTIDSTM 
                            where p.STMLOTIDSTM='{1}'
                                AND p.CODEPACK IN (
									SELECT DISTINCT CODEPACK FROM {0} WHERE CODEPART IN ({2})
								) ", STM.GetTableName, txtLot.Text, codepart);
            }
            else if (printtype == PrintType.DIY)
            {
                sql = string.Format(@"  SELECT distinct	p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'DIY' as Category,
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
		                                        ProductArticle,
		                                        '' as PartName,
		                                        '' as BarcodePart,
		                                        '' as FinishWidth,
		                                        '' as FinishLength,
		                                        p.ShelfNo,
                                                ITEMID
                                                ,tb.CATREMARK as Remark
                                        FROM {0} p  with(nolock) 
                                         LEFT JOIN dbo.STMSMARTPDTABLESTM tb 
										 ON tb.STMLOTIDSTM = p.STMLOTIDSTM
                                        where p.STMLOTIDSTM='{1}'
                                                AND p.CODEPACK IN (
									                SELECT DISTINCT CODEPACK FROM {0} WHERE CODEPART IN ({2})
								                ) ", STM.GetTableName, txtLot.Text, codepart);
            }
            else if (printtype == PrintType.Compack)
            {
                sql = string.Format(@"  
                                        SELECT distinct	p.[PROJNAME] as Project,
		                                        [STMBUILDNOJIS] as Build,
		                                        [STMFLOORNOJIS] as Floor,
		                                        [STMROOMNOJIS] as Room,
		                                        'Compack' as Category,
		                                        ITEMID as Type,
		                                        p.CODEPACK as BarcodePack,
		                                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
		                                        ProductArticle,
		                                        '' as PartName,
		                                        '' as BarcodePart,
		                                        '' as FinishWidth,
		                                        '' as FinishLength,
		                                        p.ShelfNo,
                                                ITEMID
                                                ,tb.CATREMARK as Remark
                                        FROM {0} p  with(nolock) 
                                        LEFT JOIN dbo.STMSMARTPDTABLESTM tb
										 ON tb.STMLOTIDSTM = p.STMLOTIDSTM
                                        where p.STMLOTIDSTM='{1}' 
                                                AND p.CODEPACK IN (
									                SELECT DISTINCT CODEPACK FROM {0} WHERE CODEPART IN ({2})
								                )", STM.GetTableName, txtLot.Text, codepart);
            }
            else
            {
                sql = string.Format(@" 
                        SELECT distinct p.[PROJNAME] as Project,
		                        [STMBUILDNOJIS] as Build,
		                        [STMFLOORNOJIS] as Floor,
		                        [STMROOMNOJIS] as Room,
		                        'ห้องครัว' as Category,
		                        ITEMID as Type,
		                        p.CODEPACK as BarcodePack,
		                        p.PRODUCTQUANTITYSEQUENCETOTAL as ProductSeqTotal,
		                        case when isnull(PACKQUANTITYSEQUENCE,'')='' then '1/1' else PACKQUANTITYSEQUENCE end as PackSeqTotal,
		                        count(*) over(partition by p.STMLOTIDSTM,p.CODEPACK) as PartTotal,
		                        ProductArticle,
		                        '' as PartName,
		                        '' as BarcodePart,
		                        '' as FinishWidth,
		                        '' as FinishLength,
		                        p.ShelfNo
								,tb.CATREMARK as Remark
                        FROM {0} p  with(nolock) 
						LEFT JOIN dbo.STMSMARTPDTABLESTM tb
							ON tb.STMLOTIDSTM = p.STMLOTIDSTM
                        where p.STMLOTIDSTM='{1}' 
                                AND p.CODEPACK IN (
									SELECT DISTINCT CODEPACK FROM {0} WHERE CODEPART IN ({2})
								)", STM.GetTableName, txtLot.Text, codepart);
            }
            DataTable dt = STM.QueryData(sql);

            return dt;

        }

        private byte[] GenQR(string encode)
        {
            Bitmap resultQrCode = null;

            ZXing.QrCode.QrCodeEncodingOptions options = new ZXing.QrCode.QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = 700,
                Height = 700,
                Margin = 1
            };

            BarcodeWriter qr = new ZXing.BarcodeWriter();
            qr.Options = options;
            qr.Format = ZXing.BarcodeFormat.QR_CODE;
            resultQrCode = new Bitmap(qr.Write(encode));

            byte[] img = BitmapDataFromBitmap(resultQrCode, ImageFormat.Bmp);

            return img;
        }

        private byte[] GenBar(string endcode)
        {
            byte[] img = null;
            Linear barcode1 = new Linear();
            barcode1.Type = BarcodeType.CODE128;
            barcode1.ShowText = false;
            barcode1.AutoResize = true;
            img = null;
            if (endcode != "")
            {
                barcode1.Data = endcode;
                img = barcode1.drawBarcodeAsBytes();
            }

            return img;
        }

        private byte[] BitmapDataFromBitmap(Bitmap objBitmap, ImageFormat imageFormat)
        {

            MemoryStream ms = new MemoryStream();

            objBitmap.Save(ms, imageFormat);

            return (ms.GetBuffer());

        }

        private DataTable GetAssembly(string ItemSet, string PDNumber)
        {
            DataTable dt = STM.QueryDataProductEngineering(string.Format(@"
SELECT [Seq], [ParentId], [Model_TH], [Size_TH], [Weight_TH], [Manufacture_TH], [Address_TH], [Country_TH], [Date_TH], [Model_EN], [Size_EN], [Weight_EN], [Manufacture_EN], [Address_EN], [Country_EN], [Date_EN], [Root], [PathFile], [PDNumber], [ItemSet]
FROM [dbo].[AssemblyQrCode]
where ItemSet='{0}' and PDNumber='{1}' ", ItemSet, PDNumber));

            return dt;
        }
    }
}
