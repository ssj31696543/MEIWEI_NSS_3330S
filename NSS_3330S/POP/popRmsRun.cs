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

namespace NSS_3330S.POP
{
    public partial class popRmsRun : Form
    {
        int m_nCountDot = 0;
        bool m_bTmrRefreshFlag = false;
        string m_strTitleMsg = "";
        Stopwatch swElapsed = new Stopwatch();

        public popRmsRun()
        {
            InitializeComponent();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (m_bTmrRefreshFlag) return;
            m_bTmrRefreshFlag = true;

            try
            {
                //string dot = new string('〓', m_nCountDot);
                string blank = new string(' ', m_nCountDot);

                StringBuilder sb = new StringBuilder();
                //sb.Append(dot);
                sb.Append(blank);
                sb.Append("〓〓》");

                lbArrow.Text = sb.ToString();
                sb.Clear();

                m_nCountDot++;
                if (m_nCountDot > 10)
                    m_nCountDot = 0;

                lblElapsedTime.Text = swElapsed.Elapsed.ToString(@"mm\:ss\.fff");

                if (swElapsed.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MES_REPLY_TIME].lValue)
                {
                    //this.Hide();

                    //popMessageBox popMsgBox = new popMessageBox(m_strTitleMsg + " Fail", "TIME-OUT");
                    //popMsgBox.TopMost = true;
                    //popMsgBox.ShowDialog();

                    this.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                m_bTmrRefreshFlag = false;
            }
        }

        private void popRmsRun_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrRefresh.Stop();
            swElapsed.Stop();
        }

        private void popRmsRun_Load(object sender, EventArgs e)
        {
            lblTitle.Text = m_strTitleMsg;

            tmrRefresh.Start();
            lblElapsedTime.Text = "00:00:00";
            swElapsed.Restart();
        }

        public void SetLabelTextChange(string strText)
        {
            m_strTitleMsg = strText;
        }
    }
}
