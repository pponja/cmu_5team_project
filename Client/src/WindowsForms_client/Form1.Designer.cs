namespace WindowsForms_client
{
    partial class MainFrm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainFrm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.LPN = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RE = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ON = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ODB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OSA = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VM = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VModel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.VC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveWithEncryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decryptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.BTN_LOGIN = new System.Windows.Forms.Button();
            this.TXT_PW = new System.Windows.Forms.TextBox();
            this.TXT_ID = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.TXT_DESC = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LPN,
            this.Status,
            this.RE,
            this.ON,
            this.ODB,
            this.OSA,
            this.OC,
            this.VM,
            this.VModel,
            this.VC});
            resources.ApplyResources(this.dataGridView1, "dataGridView1");
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowTemplate.Height = 27;
            // 
            // LPN
            // 
            resources.ApplyResources(this.LPN, "LPN");
            this.LPN.Name = "LPN";
            this.LPN.ReadOnly = true;
            // 
            // Status
            // 
            resources.ApplyResources(this.Status, "Status");
            this.Status.Name = "Status";
            this.Status.ReadOnly = true;
            // 
            // RE
            // 
            resources.ApplyResources(this.RE, "RE");
            this.RE.Name = "RE";
            this.RE.ReadOnly = true;
            // 
            // ON
            // 
            resources.ApplyResources(this.ON, "ON");
            this.ON.Name = "ON";
            this.ON.ReadOnly = true;
            // 
            // ODB
            // 
            resources.ApplyResources(this.ODB, "ODB");
            this.ODB.Name = "ODB";
            this.ODB.ReadOnly = true;
            // 
            // OSA
            // 
            resources.ApplyResources(this.OSA, "OSA");
            this.OSA.Name = "OSA";
            this.OSA.ReadOnly = true;
            // 
            // OC
            // 
            resources.ApplyResources(this.OC, "OC");
            this.OC.Name = "OC";
            this.OC.ReadOnly = true;
            // 
            // VM
            // 
            resources.ApplyResources(this.VM, "VM");
            this.VM.Name = "VM";
            this.VM.ReadOnly = true;
            // 
            // VModel
            // 
            resources.ApplyResources(this.VModel, "VModel");
            this.VModel.Name = "VModel";
            this.VModel.ReadOnly = true;
            // 
            // VC
            // 
            resources.ApplyResources(this.VC, "VC");
            this.VC.Name = "VC";
            this.VC.ReadOnly = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // menuToolStripMenuItem
            // 
            this.menuToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.saveWithEncryptToolStripMenuItem,
            this.decryptToolStripMenuItem,
            this.logoutToolStripMenuItem});
            this.menuToolStripMenuItem.Name = "menuToolStripMenuItem";
            resources.ApplyResources(this.menuToolStripMenuItem, "menuToolStripMenuItem");
            this.menuToolStripMenuItem.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuToolStripMenuItem_DropDownItemClicked);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            // 
            // saveWithEncryptToolStripMenuItem
            // 
            this.saveWithEncryptToolStripMenuItem.Name = "saveWithEncryptToolStripMenuItem";
            resources.ApplyResources(this.saveWithEncryptToolStripMenuItem, "saveWithEncryptToolStripMenuItem");
            // 
            // decryptToolStripMenuItem
            // 
            this.decryptToolStripMenuItem.Name = "decryptToolStripMenuItem";
            resources.ApplyResources(this.decryptToolStripMenuItem, "decryptToolStripMenuItem");
            // 
            // logoutToolStripMenuItem
            // 
            this.logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            resources.ApplyResources(this.logoutToolStripMenuItem, "logoutToolStripMenuItem");
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.BTN_LOGIN);
            this.groupBox1.Controls.Add(this.TXT_PW);
            this.groupBox1.Controls.Add(this.TXT_ID);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // BTN_LOGIN
            // 
            resources.ApplyResources(this.BTN_LOGIN, "BTN_LOGIN");
            this.BTN_LOGIN.Name = "BTN_LOGIN";
            this.BTN_LOGIN.UseVisualStyleBackColor = true;
            this.BTN_LOGIN.Click += new System.EventHandler(this.BTN_LOGIN_Click);
            // 
            // TXT_PW
            // 
            resources.ApplyResources(this.TXT_PW, "TXT_PW");
            this.TXT_PW.Name = "TXT_PW";
            // 
            // TXT_ID
            // 
            resources.ApplyResources(this.TXT_ID, "TXT_ID");
            this.TXT_ID.Name = "TXT_ID";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.progressBar1);
            this.panel1.Name = "panel1";
            // 
            // panel2
            // 
            resources.ApplyResources(this.panel2, "panel2");
            this.panel2.Name = "panel2";
            // 
            // label4
            // 
            resources.ApplyResources(this.label4, "label4");
            this.label4.Name = "label4";
            // 
            // progressBar1
            // 
            resources.ApplyResources(this.progressBar1, "progressBar1");
            this.progressBar1.Maximum = 200;
            this.progressBar1.Name = "progressBar1";
            // 
            // TXT_DESC
            // 
            resources.ApplyResources(this.TXT_DESC, "TXT_DESC");
            this.TXT_DESC.Name = "TXT_DESC";
            this.TXT_DESC.ReadOnly = true;
            // 
            // MainFrm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.Controls.Add(this.TXT_DESC);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainFrm";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainFrm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem menuToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button BTN_LOGIN;
        private System.Windows.Forms.TextBox TXT_PW;
        private System.Windows.Forms.TextBox TXT_ID;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TXT_DESC;
        private System.Windows.Forms.ToolStripMenuItem logoutToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn LPN;
        private System.Windows.Forms.DataGridViewTextBoxColumn Status;
        private System.Windows.Forms.DataGridViewTextBoxColumn RE;
        private System.Windows.Forms.DataGridViewTextBoxColumn ON;
        private System.Windows.Forms.DataGridViewTextBoxColumn ODB;
        private System.Windows.Forms.DataGridViewTextBoxColumn OSA;
        private System.Windows.Forms.DataGridViewTextBoxColumn OC;
        private System.Windows.Forms.DataGridViewTextBoxColumn VM;
        private System.Windows.Forms.DataGridViewTextBoxColumn VModel;
        private System.Windows.Forms.DataGridViewTextBoxColumn VC;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripMenuItem saveWithEncryptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decryptToolStripMenuItem;
        private System.Windows.Forms.Panel panel2;
    }
}

