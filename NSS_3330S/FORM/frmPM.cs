using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.FORM
{
    public partial class frmPM : Form
    {
        bool m_bFlagTmrShake = false;
        bool m_bStartShake = false;
        int m_nShakeCnt = 0;
        int m_nPosOrg = 0;

        private Stopwatch timeStamp = Stopwatch.StartNew();

        public frmPM()
        {
            InitializeComponent();
        }

        private void frmPM_Load(object sender, EventArgs e)
        {
            tmrPM.Enabled = true;
            tmrShake.Enabled = true;

            timeStamp.Restart();
            m_nPosOrg = txtPass.Left;
        }

        private void tmrPM_Tick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", timeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", timeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", timeStamp.Elapsed.Seconds));

            dTxtElap.DigitText = strElapsedTime;
            lbCurrentTime.Text = DateTime.Now.ToString("yyyyMMdd hh:mm:ss tt");
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
                        txtPass.Left = m_nPosOrg - 1;
                    else
                        txtPass.Left = m_nPosOrg + 1;

                    m_nShakeCnt++;

                    if (m_nShakeCnt > 10)
                    {
                        m_bStartShake = false;
                        m_nShakeCnt = 0;

                        txtPass.Left = m_nPosOrg;
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

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (GbVar.CurLoginInfo.PASSWORD == txtPass.Text)
            {
                tmrShake.Stop();
                tmrPM.Stop();

                Close();
            }
            else
            {
                txtPass.Focus();
                m_nShakeCnt = 0;
                m_bStartShake = true;
            }
        }
    }
}
