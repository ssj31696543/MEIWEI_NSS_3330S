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
    public partial class popTerminalMsg : Form
    {
        public popTerminalMsg()
        {
            InitializeComponent();
        }

        public void SetMessage(string strMsg)
        {
            if (this.InvokeRequired)
	        {
		        this.Invoke((MethodInvoker)delegate()
                {
                    string strTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                         strTime.Substring(0, 4),
                         strTime.Substring(4, 2),
                         strTime.Substring(6, 2),
                         strTime.Substring(8, 2),
                         strTime.Substring(10, 2),
                         strTime.Substring(12, 2),
                         strTime.Substring(14));

                    ListViewItem lvitem = new ListViewItem(strTime);
                    lvitem.SubItems.Add(strMsg);

                    lsvMsg.Items.Add(lvitem);
                });
	        }
            else
	        {
                string strTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                     strTime.Substring(0, 4),
                     strTime.Substring(4, 2),
                     strTime.Substring(6, 2),
                     strTime.Substring(8, 2),
                     strTime.Substring(10, 2),
                     strTime.Substring(12, 2),
                     strTime.Substring(14));

                ListViewItem lvitem = new ListViewItem(strTime);
                lvitem.SubItems.Add(strMsg);

                lsvMsg.Items.Add(lvitem);
            }
        }

        public void SetMessage(List<string> lstMsg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    lsvMsg.BeginUpdate();

                    string strTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                    strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                         strTime.Substring(0, 4),
                         strTime.Substring(4, 2),
                         strTime.Substring(6, 2),
                         strTime.Substring(8, 2),
                         strTime.Substring(10, 2),
                         strTime.Substring(12, 2),
                         strTime.Substring(14));

                    ListViewItem lvitem;
                    
                    for (int i = 0; i < lstMsg.Count; i++)
                    {
                        lvitem = new ListViewItem(strTime); 
                        lsvMsg.View = View.Details;
                        lvitem.SubItems.Add(lstMsg[i]);
                        lsvMsg.Items.Add(lvitem);
                        lsvMsg.Items[lsvMsg.Items.Count - 1].EnsureVisible();
                    }

                    lsvMsg.EndUpdate();

                });
            }
            else
            {
                lsvMsg.BeginUpdate();
                string strTime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                strTime = string.Format("{0}-{1}-{2} {3}:{4}:{5}.{6}",
                     strTime.Substring(0, 4),
                     strTime.Substring(4, 2),
                     strTime.Substring(6, 2),
                     strTime.Substring(8, 2),
                     strTime.Substring(10, 2),
                     strTime.Substring(12, 2),
                     strTime.Substring(14));

                ListViewItem lvitem;
                for (int i = 0; i < lstMsg.Count; i++)
                {
                    lvitem = new ListViewItem(strTime);
                    lsvMsg.View = View.Details;
                    lvitem.SubItems.Add(lstMsg[i]);
                    lsvMsg.Items.Add(lvitem);
                    lsvMsg.Items[lsvMsg.Items.Count - 1].EnsureVisible();
                }

                lsvMsg.EndUpdate();
            }
        }


        private void btnClose_Click(object sender, EventArgs e)
        {
            lsvMsg.Items.Clear();
            this.Hide();
        }
    }
}
