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

namespace NSS_3330S.FORM
{
    public partial class frmWarningMsg : Form
    {
        public frmWarningMsg()
        {
            InitializeComponent();
        }

        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        private void frmWarningMsg_Load(object sender, EventArgs e)
        {
            Left = 121;
            Top = 871;

            GbVar.strWarnMsg[WNDF.LIGHT_CURTAIN] = "라이트 커튼 감지";
            GbVar.strWarnMsg[WNDF.BIN_BOX_FULL] = "BIN 박스를 비워주세요";
        }

        public void SetMsg(string strMsg)
        {
            Left = 121;
            Top = 871;

            lblMsgText.Text = strMsg;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResetMsg();
        }

        public void ResetMsg()
        {
            GbFunc.SetOut((int)IODF.OUTPUT.ERROR_BUZZER, false);
            GbFunc.SetOut((int)IODF.OUTPUT.END_BUZZER, false);
            this.Hide();
        }

        private void pnlTop_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
