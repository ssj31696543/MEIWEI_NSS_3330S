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
    public partial class popCheckPassword : Form
    {
        //public delegate void CheckPassword(int nScrNo);
        //public event CheckPassword CheckPwSuccess = null;

        bool m_bFlagTmrShake = false;
        bool m_bStartShake = false;
        int m_nShakeCnt = 0;
        int m_nPosOrg = 0;

        public popCheckPassword()
        {
            InitializeComponent();
            m_nPosOrg = lbNotCorrect.Location.X;
        }

        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void pnlTop_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (GbVar.CurLoginInfo.PASSWORD == tbxPassword.Text)
            {
                //성공시
                tmrShake.Stop();
            }
            else
            {
                m_nShakeCnt = 0;
                m_bStartShake = true;
                lbNotCorrect.Visible = true;
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            Close();
        }

        private void tbxPassword_MouseClick(object sender, MouseEventArgs e)
        {
            tbxPassword.Text = "";
        }

        private void btnCANCEL_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void popCheckPassword_Load(object sender, EventArgs e)
        {
            lbNotCorrect.Visible = false;
            lbCurrentUserId.Text = GbVar.strLoggingUerID;
            tbxPassword.Text = "aaaaaaaa";
            this.CenterToScreen();
            tmrShake.Enabled = true;
        }

        private void popCheckPassword_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void tmrShake_Tick(object sender, EventArgs e)
        {
            if (m_bFlagTmrShake) return;
            m_bFlagTmrShake = true;

            try
            {
                if (m_bStartShake)
                {
                    if (m_nShakeCnt % 2 == 0)
                        lbNotCorrect.Left = m_nPosOrg - 1;
                    else
                        lbNotCorrect.Left = m_nPosOrg + 1;

                    m_nShakeCnt++;

                    if (m_nShakeCnt > 10)
                    {
                        m_bStartShake = false;
                        m_nShakeCnt = 0;

                        lbNotCorrect.Left = m_nPosOrg;
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
                m_bFlagTmrShake = false;
            }
        }
    }
}
