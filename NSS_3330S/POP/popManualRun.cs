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
    public partial class popManualRun : Form
    {
        int m_nCountDot = 0;
        bool m_bTmrRefreshFlag = false;
        Stopwatch swElapsed = new Stopwatch();

        public popManualRun()
        {
            InitializeComponent();

            Rectangle workingArea = Screen.GetWorkingArea(this);
            this.Location = new Point(workingArea.Right - Size.Width,
                                      workingArea.Bottom - Size.Height);
        }

        private void popManualRun_Load(object sender, EventArgs e)
        {
            tmrRefresh.Start();
            lblElapsedTime.Text = "00:00:00";
            swElapsed.Restart();

            if (GbSeq.manualRun.CURRENT_SEQ == NSS_3330S.THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE)
            {
                lblManualModuleName.Text = GbSeq.manualRun.GetCurrentModuleType().ToString();
                lblModuleNameTitle.Visible = true;
                lblManualModuleName.Visible = true;
            }
            else
            {
                lblModuleNameTitle.Visible = false;
                lblManualModuleName.Visible = false;
            }
        }

        private void popManualRun_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrRefresh.Stop();
            swElapsed.Stop();
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (m_bTmrRefreshFlag) return;
            m_bTmrRefreshFlag = true;

            try
            {
                string dot = new string('.', m_nCountDot);
                string blank = new string(' ', 3 - m_nCountDot);

                StringBuilder sb = new StringBuilder();
                sb.Append("Manual Running");
                sb.Append(dot);
                sb.Append(blank);

                lblTitle.Text = sb.ToString();
                sb.Clear();

                m_nCountDot++;
                if (m_nCountDot > 3)
                    m_nCountDot = 0;

                lblElapsedTime.Text = swElapsed.Elapsed.ToString(@"mm\:ss\.fff");
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
    }
}
