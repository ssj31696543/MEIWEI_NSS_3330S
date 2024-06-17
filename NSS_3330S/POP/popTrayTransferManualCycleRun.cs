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
    public partial class popTrayTransferManualCycleRun : BaseSubMenu
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

        public popTrayTransferManualCycleRun()
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

        private void OnTrayTransferManualCycleButtonClick(object sender, EventArgs e)
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

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nLoadingElvNo = 0, nLoadingTableNo = 0;
            int nUnloadingElvNo = 0, nUnloadingTableNo = 0;

            switch (tbcTrayTransferCycle.SelectedIndex)
            {
                case 0:
                    if (rdbTbLdLoadingEmpty1Elv.Checked)
                    {
                        nLoadingElvNo = 0;
                    }
                    else
                    {
                        nLoadingElvNo = 1;
                    }

                    if (rdbTbLdUnloadingGD1.Checked)
                    {
                        nUnloadingTableNo = 0;
                    }
                    else if (rdbTbLdUnloadingGD2.Checked)
                    {
                        nUnloadingTableNo = 1;
                    }
                    else
                    {
                        nUnloadingTableNo = 2;
                    }
                    
                    break;

                case 1:
                    if (rdbElvUnldLoadingGD1.Checked)
                    {
                        nLoadingTableNo = 0;
                        nUnloadingElvNo = 0;
                    }
                    else if (rdbElvUnldLoadingGD2.Checked)
                    {
                        nLoadingTableNo = 1;
                        nUnloadingElvNo = 0;
                    }
                    else
                    {
                        nLoadingTableNo = 2;
                        nUnloadingElvNo = 1;
                    }

                    break;

                case 2:
                    if (rdbCoverLdEmpty1.Checked)
                    {
                        nLoadingElvNo = 0;
                        nUnloadingElvNo = 2;

                        if (nSeqNo ==  710)
	                    {
                            nSeqNo = 720;
	                    }
                    }
                    else if (rdbCoverLdEmpty2.Checked)
                    {
                        nLoadingElvNo = 1;
                        nUnloadingElvNo = 2;

                        if (nSeqNo == 710)
                        {
                            nSeqNo = 720;
                        }
                    }
                    else
                    {
                        nLoadingElvNo = 2;
   
                        if (rdbCoverUldGood.Checked)
                        {
                            nUnloadingElvNo = 0;
                        }
                        else
                        {
                            nUnloadingElvNo = 1;
                        }
                    }

                    break;

                default:
                    break;
            }

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingElvNo, nLoadingTableNo, nUnloadingElvNo, nUnloadingTableNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnTrigg_Click(object sender, EventArgs e)
        {
            //IFMgr.Inst.VISION.TrgTrayOneShot();
        }

        private void OnElvUnloadingLoadGoodTableClick(object sender, EventArgs e)
        {
            lbElvUldUnloadGoodElv.BackColor = Color.Orange;
            lbElvUldUnloadReworkElv.BackColor = SystemColors.Control;
        }

        private void OnElvUnloadingLoadReworkTableClick(object sender, EventArgs e)
        {
            lbElvUldUnloadGoodElv.BackColor = SystemColors.Control;
            lbElvUldUnloadReworkElv.BackColor = Color.Orange; 
        }

        private void OnCoverTrEmptyElvButtonClick(object sender, EventArgs e)
        {
            rdbCoverUldGood.Visible = false;
            rdbCoverUldRework.Visible = false;
        }

        private void OnCoverTrCoverElvButtonClick(object sender, EventArgs e)
        {
            rdbCoverUldGood.Visible = true;
            rdbCoverUldRework.Visible = true;
        }
    }
}
