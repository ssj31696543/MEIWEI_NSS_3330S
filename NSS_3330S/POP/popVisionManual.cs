using NSS_3330S.UC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popVisionManual : BaseSubMenu
    {
        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        public popVisionManual()
        {
            InitializeComponent();

            listViewMsgLog.Items.Clear();
            listViewSVID.Items.Clear();

            for (int nCnt = 0; nCnt < 100; nCnt++)
            {
                cbTriggerCount.Items.Add((nCnt + 1).ToString());
            }

            if (cbTriggerCount.Items.Count > 0)
                cbTriggerCount.SelectedIndex = 0;
        }

        private void popVisionManual_Load(object sender, EventArgs e)
        {
            ListViewItem lvItem = null;

            listViewMsgLog.BeginUpdate();
            while (GbVar.g_queueMsgLogVisionUdp.Count > 0)
            {
                System.Threading.Thread.Sleep(1);

                MessageLog mLog;
                if (GbVar.g_queueMsgLogVisionUdp.TryDequeue(out mLog))
                {
                    lvItem = listViewMsgLog.Items.Add(mLog._bSend ? "S" : "R");
                    lvItem.SubItems.Add(mLog._dtCreate.ToString("yyyy-MM-dd HH:mm:ss"));
                    lvItem.SubItems.Add(mLog._strMessage);

                    if (listViewMsgLog.Items.Count > 100)
                        listViewMsgLog.Items.RemoveAt(0);
                }
            }
            listViewMsgLog.EndUpdate();

            tmrMonitorLog.Start();
            tmrSVID.Start();
            tmrRefresh.Start();
        }

        private void popVisionManual_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrMonitorLog.Stop();
            tmrSVID.Stop();
            tmrRefresh.Stop();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLive_PreAlign_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.TOP_LIVE + nTag);
        }

        private void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void tmrMonitorLog_Tick(object sender, EventArgs e)
        {
            ListViewItem lvItem = null;
            if (GbVar.g_queueMsgLogVisionUdp.Count > 0)
            {
                listViewMsgLog.BeginUpdate();

                MessageLog mLog;
                if (GbVar.g_queueMsgLogVisionUdp.TryDequeue(out mLog))
                {
                    lvItem = listViewMsgLog.Items.Add(mLog._bSend ? "S" : "R");
                    lvItem.SubItems.Add(mLog._dtCreate.ToString("yyyy-MM-dd HH:mm:ss"));
                    lvItem.SubItems.Add(mLog._strMessage);

                    if (listViewMsgLog.Items.Count > 100)
                        listViewMsgLog.Items.RemoveAt(0);
                }

                listViewMsgLog.EndUpdate();
            }
        }

        private void tmrSVID_Tick(object sender, EventArgs e)
        {
            ListViewItem lvItem = null;
            foreach (KeyValuePair<string, string> item in GbVar.g_dictVisionSVID)
            {
                lvItem = null;
                if (listViewSVID.Items.Count > 0)
                {
                    lvItem = listViewSVID.FindItemWithText(item.Key, false, 0);
                }

                if (lvItem == null)
                {
                    lvItem = listViewSVID.Items.Add(item.Key);
                    lvItem.SubItems.Add(item.Value);
                }
                else
                {
                    if (lvItem.SubItems.Count > 1)
                    {
                        if (lvItem.SubItems[1].Text != item.Value)
                        {
                            lvItem.SubItems[1].Text = item.Value;
                        }
                    }
                }
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            string strPreData = "";
            strPreData = string.Format("( {0}, {1} )", GbVar.Seq.sStripTransfer.Info.ptPreOffset1.X.ToString("F3"), GbVar.Seq.sStripTransfer.Info.ptPreOffset1.Y.ToString("F3"));
            if (lbPreOffset1.Text != strPreData) lbPreOffset1.Text = strPreData;
            strPreData = string.Format("( {0}, {1} )", GbVar.Seq.sStripTransfer.Info.ptPreOffset2.X.ToString("F3"), GbVar.Seq.sStripTransfer.Info.ptPreOffset2.Y.ToString("F3"));
            if (lbPreOffset2.Text != strPreData) lbPreOffset2.Text = strPreData;
            strPreData = GbVar.Seq.sStripTransfer.Info.dPreAngleOffset.ToString("F2");
            if (lbPreOffsetAngle.Text != strPreData) lbPreOffsetAngle.Text = strPreData;

            strPreData = GbVar.Seq.sStripRail.Info.STRIP_ID;
            if (lbPre2DCode.Text != strPreData) lbPre2DCode.Text = strPreData;
        }

        private void btnInterfacePreAlign1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualPreAlign(nTag);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PRE_ALIGN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnInterfaceTopAlign_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nTableNo = 0;
            if (rdoTableNo1.Checked) nTableNo = 0;
            else if (rdoTableNo2.Checked) nTableNo = 1;

            int nMaxTriggerCount = cbTriggerCount.SelectedIndex + 1;

            GbSeq.manualRun.SetManualTopAlign(nTableNo);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_TOP_ALIGN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMapInspection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nTableNo = 0;
            if (rdoTableNo1.Checked) nTableNo = 0;
            else if (rdoTableNo2.Checked) nTableNo = 1;

            int nMaxTriggerCount = cbTriggerCount.SelectedIndex + 1;

            GbSeq.manualRun.SetManualMapInspection(nTableNo, nMaxTriggerCount);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_MAP_INSPECTION);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnBallInspection_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nTableNo = 0;
            if (rdoTableNo1.Checked) nTableNo = 0;
            else if (rdoTableNo2.Checked) nTableNo = 1;

            int nMaxTriggerCount = cbTriggerCount.SelectedIndex + 1;

            GbSeq.manualRun.SetManualBallInspection(nTableNo, nMaxTriggerCount);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_BALL_INSPECTION);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        

    }
}
