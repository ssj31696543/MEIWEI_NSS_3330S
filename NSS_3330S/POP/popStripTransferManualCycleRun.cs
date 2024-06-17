using Glass;
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
    public partial class popStripTransferManualCycleRun : BaseSubMenu
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

        public popStripTransferManualCycleRun()
        {
            InitializeComponent();
            this.CenterToScreen();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void OnStripTransferManualCycleButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartCycle(btn);
        }

        void StartCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox("Are you sure to run [ " + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualSawCycleRun(nSeqNo, (int)numLoadMzSlotNo.Value, (rdbTableNo.Checked? 0 : 1), args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SAW_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnTrigg_Click(object sender, EventArgs e)
        {
            //IFMgr.Inst.VISION.TrgPreOneShot();
        }
    }
}
