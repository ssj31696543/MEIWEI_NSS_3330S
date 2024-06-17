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
    public partial class popPickNPlaceManualCycleRun : BaseSubMenu
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

        CheckBox[,] cbxPickerSelect;
        int[] nPlacePickerResulte;

        public popPickNPlaceManualCycleRun()
        {
            InitializeComponent();
            this.CenterToScreen();

            cbxPickerSelect = new CheckBox[3, CFG_DF.MAX_PICKER_PAD_CNT] {{ cbxPickUpPicker1,
                                                                            cbxPickUpPicker2,
                                                                            cbxPickUpPicker3,
                                                                            cbxPickUpPicker4,
                                                                            cbxPickUpPicker5,
                                                                            cbxPickUpPicker6,
                                                                            cbxPickUpPicker7,
                                                                            cbxPickUpPicker8},
                                                                          { cbxInspectionPicker1,
                                                                            cbxInspectionPicker2,
                                                                            cbxInspectionPicker3,
                                                                            cbxInspectionPicker4,
                                                                            cbxInspectionPicker5,
                                                                            cbxInspectionPicker6,
                                                                            cbxInspectionPicker7,
                                                                            cbxInspectionPicker8},
                                                                          { cbxPlacePicker1,
                                                                            cbxPlacePicker2,
                                                                            cbxPlacePicker3,
                                                                            cbxPlacePicker4,
                                                                            cbxPlacePicker5,
                                                                            cbxPlacePicker6,
                                                                            cbxPlacePicker7,
                                                                            cbxPlacePicker8}};


            nPlacePickerResulte = new int[CFG_DF.MAX_PICKER_PAD_CNT] { cmbPlacePickerRtn1.SelectedIndex,
                                                                       cmbPlacePickerRtn2.SelectedIndex,
                                                                       cmbPlacePickerRtn3.SelectedIndex,
                                                                       cmbPlacePickerRtn4.SelectedIndex,
                                                                       cmbPlacePickerRtn5.SelectedIndex,
                                                                       cmbPlacePickerRtn6.SelectedIndex,
                                                                       cmbPlacePickerRtn7.SelectedIndex,
                                                                       cmbPlacePickerRtn8.SelectedIndex};
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

        private void btnCycleStart_Click(object sender, EventArgs e)
        {
            int nCycleNo = tbcCycleGroup.SelectedIndex;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("PICK & PLACE {0} : BUTTON CLICK",
                nCycleNo));

            StartCycle(nCycleNo);
        }

        void StartCycle(int nCycleNo, params object[] args)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + FormTextLangMgr.FindKey(" Pick & Place (") + nCycleNo.ToString() + ") ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nSeqNo = 0;
            int nPickerHeadNo = 0;
            int nTotPickUpCount = 0;
            int nTotPlaceCount = 0;
            int nWorkTableNo = 0;

            PickerPadInfo[] unitPicker = new PickerPadInfo[CFG_DF.MAX_PICKER_PAD_CNT];

            switch (nCycleNo)
            {
                case 0:
                    nSeqNo = 200;
                    nPickerHeadNo = cmbPickUpHeadNo.SelectedIndex;
                    nWorkTableNo = cmbPickUpTableNo.SelectedIndex;
                    nTotPickUpCount = (int)numTablePickUpCount.Value;

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[0, nPickerCnt].Checked;
                    }

                    GbSeq.manualRun.SetManualSorterPickUpCycleRun(nSeqNo + (nPickerHeadNo * 10), nPickerHeadNo, nWorkTableNo, nTotPickUpCount, unitPicker, args);
                    break;
                case 1:
                    nSeqNo = 300;
                    nPickerHeadNo = cmbInspHeadNo.SelectedIndex;

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[1, nPickerCnt].Checked;
                    }

                    GbSeq.manualRun.SetManualSorterInspectionCycleRun(nSeqNo + (nPickerHeadNo * 10), nPickerHeadNo, nWorkTableNo, unitPicker, args);
                    break;
                case 2:
                    nSeqNo = 400;
                    nPickerHeadNo = cmbPlaceHeadNo.SelectedIndex;
                    nWorkTableNo = cmbPlaceTableNo.SelectedIndex;
                    nTotPlaceCount = (int)numTablePlaceCount.Value;

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[2, nPickerCnt].Checked;
                        unitPicker[nPickerCnt].TOP_INSP_RESULT = nPlacePickerResulte[nPickerCnt];
                    }

                    GbSeq.manualRun.SetManualSorterPlaceCycleRun(nSeqNo + (nPickerHeadNo * 10), nPickerHeadNo, nWorkTableNo, nTotPlaceCount, unitPicker, args);
                    break;
                default:
                    break;
            }
          
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnH1Trig_Click(object sender, EventArgs e)
        {
            //IFMgr.Inst.VISION.TrgHead1OneShot();
        }

        private void btnH2Trig_Click(object sender, EventArgs e)
        {
            //IFMgr.Inst.VISION.TrgHead2OneShot();
        }

        private void btnBtmTrig_Click(object sender, EventArgs e)
        {
            IFMgr.Inst.VISION.TrgBallOneShot();
        }

        private void OnChipPickerManualCycleButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartCycle(btn);
        }

        void StartCycle(GlassButton btn, params object[] args)
        {
            int nTag = btn.GetTag();
            int nSeqNo = 0;
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nPickerHeadNo = 0;
            int nPickUpTableNo = 0;
            int nPlaceTableNo = 0;

            int nPickUpTableCount = 0;
            int nBtmInspCount = 0;
            int nPlaceTableCount = 0;

            PickerPadInfo[] unitPicker = new PickerPadInfo[CFG_DF.MAX_PICKER_PAD_CNT];

            switch (nTag)
            {
                case 0:
                    if (cmbPickUpHeadNo.SelectedIndex == 0)
                    {
                        nSeqNo = 220;
                        if (rdbFwdPickUp.Checked != true)
                        {
                            nSeqNo += 10;
                        }
                    }
                    else
                    {
                        nSeqNo = 240;
                        if (rdbFwdPickUp.Checked != true)
                        {
                            nSeqNo += 10;
                        }
                    }

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[0, nPickerCnt].Checked;
                    }

                    nPickerHeadNo = cmbPickUpHeadNo.SelectedIndex;
                    nPickUpTableNo = cmbPickUpTableNo.SelectedIndex;
                    nPickUpTableCount = (int)numTablePickUpCount.Value;
                    break;

                case 1:
                    if (cmbInspHeadNo.SelectedIndex == 0)
                    {
                        nSeqNo = 320;
                    }
                    else
                    {
                        nSeqNo = 330;
                    }

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[1, nPickerCnt].Checked;
                    }

                    nPickerHeadNo = cmbInspHeadNo.SelectedIndex;
                    break;

                case 2:
                    if (cmbPlaceHeadNo.SelectedIndex == 0)
                    {
                        // GD, RW, RJT
                        nSeqNo = 420 + (10 * cmbPlaceTableNo.SelectedIndex);

                    }
                    else
                    {
                        nSeqNo = 450 + (10 * cmbPlaceTableNo.SelectedIndex);
                    }

                    for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
                    {
                        unitPicker[nPickerCnt] = new PickerPadInfo();
                        unitPicker[nPickerCnt].IS_PICKER_SKIP = !cbxPickerSelect[2, nPickerCnt].Checked;
                        unitPicker[nPickerCnt].TOP_INSP_RESULT = nPlacePickerResulte[nPickerCnt];
                    }

                    nPickerHeadNo = cmbPlaceHeadNo.SelectedIndex;
                    nPlaceTableNo = cmbPlaceTableNo.SelectedIndex;
                    nPlaceTableCount = (int)numTablePlaceCount.Value;
                    break;
                default:
                    break;
            }


            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nTag, nPickerHeadNo, 
                                                    nPickUpTableNo, nPlaceTableNo,
                                                    nPickUpTableCount, nBtmInspCount, nPlaceTableCount,
                                                    rdbBwdPickUp.Checked,
                                                    rdbBwdPlace.Checked,
                                                    cmbPickUpPickerCount.SelectedIndex,
                                                    cmbPlacePickerCount.SelectedIndex,
                                                    unitPicker,
                                                    args);

            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void cbxPickUpPickerAllUse_CheckedChanged(object sender, EventArgs e)
        {
            for (int nPickerCount = 0; nPickerCount < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCount++)
            {
                cbxPickerSelect[0, nPickerCount].Checked = cbxPickUpPickerAllUse.Checked;
            }
        }

        private void cbxInspectionPickerAllUse_CheckedChanged(object sender, EventArgs e)
        {
            for (int nPickerCount = 0; nPickerCount < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCount++)
            {
                cbxPickerSelect[1, nPickerCount].Checked = cbxInspectionPickerAllUse.Checked;
            }
        }

        private void cbxPlacePickerAllUse_CheckedChanged(object sender, EventArgs e)
        {
            for (int nPickerCount = 0; nPickerCount < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCount++)
            {
                cbxPickerSelect[2, nPickerCount].Checked = cbxPlacePickerAllUse.Checked;
            }
        }

        private void btnMapTableClear_Click(object sender, EventArgs e)
        {
            if (cmbPickUpTableNo.SelectedIndex >= 0)
            {
                GbVar.Seq.sMapVisionTable[cmbPickUpTableNo.SelectedIndex].Info.Clear();
            }
            
        }

        private void btnTrayMapClear_Click(object sender, EventArgs e)
        {
            if (cmbPlaceTableNo.SelectedIndex >= 0)
            {
                GbVar.Seq.sUldGDTrayTable[cmbPlaceTableNo.SelectedIndex].Info.Clear();
            }
        }
    }
}
