using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsForms_client
{

    public partial class Form2 : Form
    {
        string ReceivedData;
        public Form2(string otpNum)
        {
            InitializeComponent();
            ReceivedData = otpNum;

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
          
        }

        private void BTN_SENDOTP_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            MainFrm parentForm = (MainFrm)Owner;
            parentForm.receivedOTPNumber = TXT_OTPNUM.Text;
            this.Close();

        }

        private void Form2_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == Visible)
                TXT_OTPNUM.Clear();
        }

        private void TXT_OTPNUM_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
                e.Handled = true;

        }
    }
}
