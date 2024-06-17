using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NSS_3330S.POP
{
    public partial class popMessageBox : Form// : MaterialSkin.Controls.MaterialForm
    {
        Point m_ptCancel = new Point(394, 125);
        Point m_ptOK = new Point(394, 125);

        public popMessageBox(string strMsg, string strTitle, MessageBoxButtons msgBoxBtns = MessageBoxButtons.OKCancel)
        {
            InitializeComponent();

            lbTitle.Text = strTitle;
            lbMessage.Text = strMsg;
            btnCancel.Text = FormTextLangMgr.FindKey("CANCEL");
            btnOK.Text = FormTextLangMgr.FindKey("OK");
            lbTitle.Text = FormTextLangMgr.FindKey("Question");

            if (msgBoxBtns == MessageBoxButtons.OKCancel)
            {
                btnCancel.Visible = true;
                btnOK.Visible = true;

                //btnOK.Location = m_ptOKCancel;
            }
            else if (msgBoxBtns == MessageBoxButtons.OK)
            {
                btnCancel.Visible = false;
                btnOK.Visible = true;

                btnOK.Location = m_ptOK;
            }
            else
            {
                // 나중에 필요할 때
                btnCancel.Visible = true;
                btnOK.Visible = false;

                btnCancel.Location = m_ptCancel;
            }
        }
        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void lbTitle_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion
        private void popMessageBox_Load(object sender, EventArgs e)
        {
            this.CenterToScreen();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
