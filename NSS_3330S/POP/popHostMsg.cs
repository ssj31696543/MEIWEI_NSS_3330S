using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S
{
    public partial class popHostMsg : Form
    {
        public popHostMsg()
        {
            InitializeComponent();
        }

        public void SetMessage(string strMsg)
        {
            if (this.InvokeRequired)
	        {
		        this.Invoke((MethodInvoker)delegate()
                {
                    lbMsg.Text = strMsg;
                });
	        }
            else
	        {
                lbMsg.Text = strMsg;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
