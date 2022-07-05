namespace WindowsForms_client
{
    partial class Form2
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
            this.BTN_SENDOTP = new System.Windows.Forms.Button();
            this.TXT_OTPNUM = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // BTN_SENDOTP
            // 
            this.BTN_SENDOTP.Location = new System.Drawing.Point(246, 63);
            this.BTN_SENDOTP.Name = "BTN_SENDOTP";
            this.BTN_SENDOTP.Size = new System.Drawing.Size(75, 31);
            this.BTN_SENDOTP.TabIndex = 0;
            this.BTN_SENDOTP.Text = "SEND";
            this.BTN_SENDOTP.UseVisualStyleBackColor = true;
            this.BTN_SENDOTP.Click += new System.EventHandler(this.BTN_SENDOTP_Click);
            // 
            // TXT_OTPNUM
            // 
            this.TXT_OTPNUM.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TXT_OTPNUM.Location = new System.Drawing.Point(12, 63);
            this.TXT_OTPNUM.MaxLength = 6;
            this.TXT_OTPNUM.Name = "TXT_OTPNUM";
            this.TXT_OTPNUM.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.TXT_OTPNUM.Size = new System.Drawing.Size(217, 29);
            this.TXT_OTPNUM.TabIndex = 1;
            this.TXT_OTPNUM.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.TXT_OTPNUM.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TXT_OTPNUM_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(12, 35);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 15);
            this.label1.TabIndex = 2;
            this.label1.Text = "Please input your OTP number  (6 digit)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.Location = new System.Drawing.Point(12, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "I sent OTP number to your e-mail";
            // 
            // Form2
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(350, 110);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TXT_OTPNUM);
            this.Controls.Add(this.BTN_SENDOTP);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form2";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "OTP Request";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form2_FormClosing);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.VisibleChanged += new System.EventHandler(this.Form2_VisibleChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BTN_SENDOTP;
        private System.Windows.Forms.TextBox TXT_OTPNUM;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}