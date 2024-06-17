using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popInitializeSet : Form
    {
        public bool bLoader = false;
        public bool bSaw = false;
        public bool bSorter = false; 

        public popInitializeSet()
        {
            InitializeComponent();
        }

        public popInitializeSet(string strMsg, string strTitle, MessageBoxButtons msgBoxBtns = MessageBoxButtons.OKCancel)
        {
            InitializeComponent();

        }

        private void popInitializeSet_Load(object sender, EventArgs e)
        {

            if (label2.Text != FormTextLangMgr.FindKey("Initializing..."))
            {
                label2.Text = FormTextLangMgr.FindKey("Initializing...");
            }
            if (btnLoader.Text != FormTextLangMgr.FindKey("Loader"))
            {
                btnLoader.Text = FormTextLangMgr.FindKey("Loader");
            }
            if (btnSaw.Text != FormTextLangMgr.FindKey("Dicing"))
            {
                btnSaw.Text = FormTextLangMgr.FindKey("Dicing");
            }
            if (btnSorter.Text != FormTextLangMgr.FindKey("Sorter"))
            {
                btnSorter.Text = FormTextLangMgr.FindKey("Sorter");
            }
            if (btnOk.Text != FormTextLangMgr.FindKey("OK"))
            {
                btnOk.Text = FormTextLangMgr.FindKey("OK");
            }
            if (btnCancel.Text != FormTextLangMgr.FindKey("CANCEL"))
            {
                btnCancel.Text = FormTextLangMgr.FindKey("CANCEL");
            }
            this.CenterToScreen();
        }

        private void btnLoader_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            switch (nTag)
            {
                case 0:
                    {
                        bLoader = !bLoader;
                        btn.BackColor = bLoader ? Color.Orange : System.Drawing.SystemColors.ButtonFace;
                    }
                    break;
                case 1:
                    {
                        bSaw = !bSaw;
                        btn.BackColor = bSaw ? Color.Orange : System.Drawing.SystemColors.ButtonFace;
                    }
                    break;
                case 2:
                    {
                        bSorter = !bSorter;
                        btn.BackColor = bSorter ? Color.Orange : System.Drawing.SystemColors.ButtonFace;
                    }
                    break;
                default:
                    break;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            // 아무 파트도 선택하지 않았으면 메시지창 띄운다.
            if (!bLoader && !bSaw && !bSorter)
            {
                popMessageBox msg = new popMessageBox("아무것도 선택하지 않았습니다.", "확인");
                msg.TopMost = true;
                msg.ShowDialog();

                return;
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            Close();
        }
    }
}
