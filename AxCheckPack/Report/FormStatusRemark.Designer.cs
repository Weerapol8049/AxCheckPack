namespace AxCheckPack.Report
{
    partial class FormStatusRemark
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.layoutControl1 = new DevExpress.XtraLayout.LayoutControl();
            this.txtPrint = new DevExpress.XtraEditors.TextEdit();
            this.checkedListBoxProcess = new DevExpress.XtraEditors.CheckedListBoxControl();
            this.simpleButton1 = new DevExpress.XtraEditors.SimpleButton();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.btnOk = new DevExpress.XtraEditors.SimpleButton();
            this.textEditRemark = new DevExpress.XtraEditors.TextEdit();
            this.layoutControlGroup1 = new DevExpress.XtraLayout.LayoutControlGroup();
            this.layoutControlItem1 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem2 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem4 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem5 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem6 = new DevExpress.XtraLayout.LayoutControlItem();
            this.layoutControlItem7 = new DevExpress.XtraLayout.LayoutControlItem();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).BeginInit();
            this.layoutControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtPrint.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBoxProcess)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditRemark.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).BeginInit();
            this.SuspendLayout();
            // 
            // layoutControl1
            // 
            this.layoutControl1.Controls.Add(this.txtPrint);
            this.layoutControl1.Controls.Add(this.checkedListBoxProcess);
            this.layoutControl1.Controls.Add(this.simpleButton1);
            this.layoutControl1.Controls.Add(this.labelControl1);
            this.layoutControl1.Controls.Add(this.btnOk);
            this.layoutControl1.Controls.Add(this.textEditRemark);
            this.layoutControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutControl1.Location = new System.Drawing.Point(0, 0);
            this.layoutControl1.Name = "layoutControl1";
            this.layoutControl1.Root = this.layoutControlGroup1;
            this.layoutControl1.Size = new System.Drawing.Size(225, 330);
            this.layoutControl1.TabIndex = 0;
            this.layoutControl1.Text = "layoutControl1";
            // 
            // txtPrint
            // 
            this.txtPrint.EditValue = "1";
            this.txtPrint.Location = new System.Drawing.Point(66, 272);
            this.txtPrint.Name = "txtPrint";
            this.txtPrint.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric;
            this.txtPrint.Size = new System.Drawing.Size(147, 20);
            this.txtPrint.StyleController = this.layoutControl1;
            this.txtPrint.TabIndex = 10;
            // 
            // checkedListBoxProcess
            // 
            this.checkedListBoxProcess.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkedListBoxProcess.Items.AddRange(new DevExpress.XtraEditors.Controls.CheckedListBoxItem[] {
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(1, "ตัด"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(2, "ปิดขอบ"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(3, "เจาะ"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(4, "CNC"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(5, "ประกอบ"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(6, "กระจก+อลู+ไม้"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem("12", "ไฟเบอร์"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(7, "พิเศษ"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(8, "เคลือบ"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(9, "ทำสี"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(10, "จัดชุด"),
            new DevExpress.XtraEditors.Controls.CheckedListBoxItem(11, "รอส่งเข้าคลัง")});
            this.checkedListBoxProcess.Location = new System.Drawing.Point(12, 29);
            this.checkedListBoxProcess.Name = "checkedListBoxProcess";
            this.checkedListBoxProcess.Size = new System.Drawing.Size(201, 215);
            this.checkedListBoxProcess.StyleController = this.layoutControl1;
            this.checkedListBoxProcess.TabIndex = 9;
            // 
            // simpleButton1
            // 
            this.simpleButton1.Location = new System.Drawing.Point(114, 296);
            this.simpleButton1.Name = "simpleButton1";
            this.simpleButton1.Size = new System.Drawing.Size(99, 22);
            this.simpleButton1.StyleController = this.layoutControl1;
            this.simpleButton1.TabIndex = 8;
            this.simpleButton1.Text = "ยกเลิก";
            this.simpleButton1.Click += new System.EventHandler(this.simpleButton1_Click);
            // 
            // labelControl1
            // 
            this.labelControl1.Location = new System.Drawing.Point(12, 12);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(37, 13);
            this.labelControl1.StyleController = this.layoutControl1;
            this.labelControl1.TabIndex = 7;
            this.labelControl1.Text = "Process";
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(12, 296);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(98, 22);
            this.btnOk.StyleController = this.layoutControl1;
            this.btnOk.TabIndex = 5;
            this.btnOk.Text = "ตกลง";
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // textEditRemark
            // 
            this.textEditRemark.Location = new System.Drawing.Point(66, 248);
            this.textEditRemark.Name = "textEditRemark";
            this.textEditRemark.Properties.MaxLength = 14;
            this.textEditRemark.Size = new System.Drawing.Size(147, 20);
            this.textEditRemark.StyleController = this.layoutControl1;
            this.textEditRemark.TabIndex = 4;
            this.textEditRemark.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textEditRemark_KeyDown);
            // 
            // layoutControlGroup1
            // 
            this.layoutControlGroup1.EnableIndentsWithoutBorders = DevExpress.Utils.DefaultBoolean.True;
            this.layoutControlGroup1.GroupBordersVisible = false;
            this.layoutControlGroup1.Items.AddRange(new DevExpress.XtraLayout.BaseLayoutItem[] {
            this.layoutControlItem1,
            this.layoutControlItem2,
            this.layoutControlItem4,
            this.layoutControlItem5,
            this.layoutControlItem6,
            this.layoutControlItem7});
            this.layoutControlGroup1.Location = new System.Drawing.Point(0, 0);
            this.layoutControlGroup1.Name = "layoutControlGroup1";
            this.layoutControlGroup1.Size = new System.Drawing.Size(225, 330);
            this.layoutControlGroup1.TextVisible = false;
            // 
            // layoutControlItem1
            // 
            this.layoutControlItem1.Control = this.textEditRemark;
            this.layoutControlItem1.Location = new System.Drawing.Point(0, 236);
            this.layoutControlItem1.Name = "layoutControlItem1";
            this.layoutControlItem1.Size = new System.Drawing.Size(205, 24);
            this.layoutControlItem1.Text = "หมายเหตุ";
            this.layoutControlItem1.TextSize = new System.Drawing.Size(51, 13);
            // 
            // layoutControlItem2
            // 
            this.layoutControlItem2.Control = this.btnOk;
            this.layoutControlItem2.Location = new System.Drawing.Point(0, 284);
            this.layoutControlItem2.Name = "layoutControlItem2";
            this.layoutControlItem2.Size = new System.Drawing.Size(102, 26);
            this.layoutControlItem2.Text = "ตกลง";
            this.layoutControlItem2.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem2.TextVisible = false;
            // 
            // layoutControlItem4
            // 
            this.layoutControlItem4.Control = this.labelControl1;
            this.layoutControlItem4.Location = new System.Drawing.Point(0, 0);
            this.layoutControlItem4.Name = "layoutControlItem4";
            this.layoutControlItem4.Size = new System.Drawing.Size(205, 17);
            this.layoutControlItem4.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem4.TextVisible = false;
            // 
            // layoutControlItem5
            // 
            this.layoutControlItem5.Control = this.simpleButton1;
            this.layoutControlItem5.Location = new System.Drawing.Point(102, 284);
            this.layoutControlItem5.Name = "layoutControlItem5";
            this.layoutControlItem5.Size = new System.Drawing.Size(103, 26);
            this.layoutControlItem5.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem5.TextVisible = false;
            // 
            // layoutControlItem6
            // 
            this.layoutControlItem6.Control = this.checkedListBoxProcess;
            this.layoutControlItem6.Location = new System.Drawing.Point(0, 17);
            this.layoutControlItem6.Name = "layoutControlItem6";
            this.layoutControlItem6.Size = new System.Drawing.Size(205, 219);
            this.layoutControlItem6.TextSize = new System.Drawing.Size(0, 0);
            this.layoutControlItem6.TextVisible = false;
            // 
            // layoutControlItem7
            // 
            this.layoutControlItem7.Control = this.txtPrint;
            this.layoutControlItem7.Location = new System.Drawing.Point(0, 260);
            this.layoutControlItem7.Name = "layoutControlItem7";
            this.layoutControlItem7.Size = new System.Drawing.Size(205, 24);
            this.layoutControlItem7.Text = "จำนวนพิมพ์";
            this.layoutControlItem7.TextSize = new System.Drawing.Size(51, 13);
            // 
            // FormStatusRemark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(225, 330);
            this.Controls.Add(this.layoutControl1);
            this.Name = "FormStatusRemark";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Print";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormStatusRemark_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.layoutControl1)).EndInit();
            this.layoutControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.txtPrint.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkedListBoxProcess)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditRemark.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlGroup1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.layoutControlItem7)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraLayout.LayoutControl layoutControl1;
        private DevExpress.XtraLayout.LayoutControlGroup layoutControlGroup1;
        private DevExpress.XtraEditors.SimpleButton btnOk;
        private DevExpress.XtraEditors.TextEdit textEditRemark;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem2;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem4;
        private DevExpress.XtraEditors.SimpleButton simpleButton1;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem5;
        private DevExpress.XtraEditors.CheckedListBoxControl checkedListBoxProcess;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem6;
        private DevExpress.XtraEditors.TextEdit txtPrint;
        private DevExpress.XtraLayout.LayoutControlItem layoutControlItem7;
    }
}