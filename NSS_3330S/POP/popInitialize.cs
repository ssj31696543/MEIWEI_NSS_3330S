using NSS_3330S.MOTION;
using NSS_3330S.UC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    /// <summary>
    /// 220428 pjh 
    /// - InitProcCircle 컨트롤러 관련 코드 디자이너에서 안하고 여기서 추가하도록 수정
    /// - input 업데이트 및 칩피커 z, t축 홈 관련 신호 화면 갱신
    /// </summary>
    public partial class popInitialize : Form
    {
        bool bLdInitSkip = false;
        bool bSawInitSkip = false;
        bool bCleanerInitSkip = false;// have to remove
        bool bSorterInitSkip = false;

        List<int> listAxisLoader = new List<int>();
        List<int> listAxisSaw = new List<int>();
        List<int> listAxisSorter = new List<int>();
        Panel[] panIsHomePicker;
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        List<ucManualOutputButton> m_listOuputButton = new List<ucManualOutputButton>();
        List<InputLabel> m_listInputLabel = new List<InputLabel>();

        MRG.Controls.UI.LoadingCircle InitProcCircle;

        int nFlicker = 1;

        public popInitialize()
        {
            InitializeComponent();

            SetListAxis();

            panIsHomePicker = new Panel[] {
            panIsHomePickerH1Z1, panIsHomePickerH1R1,
            panIsHomePickerH1Z2, panIsHomePickerH1R2,
            panIsHomePickerH1Z3, panIsHomePickerH1R3,
            panIsHomePickerH1Z4, panIsHomePickerH1R4,
            panIsHomePickerH1Z5, panIsHomePickerH1R5,
            panIsHomePickerH1Z6, panIsHomePickerH1R6,
            panIsHomePickerH1Z7, panIsHomePickerH1R7,
            panIsHomePickerH1Z8, panIsHomePickerH1R8,
            panIsHomePickerH2Z1, panIsHomePickerH2R1,
            panIsHomePickerH2Z2, panIsHomePickerH2R2,
            panIsHomePickerH2Z3, panIsHomePickerH2R3,
            panIsHomePickerH2Z4, panIsHomePickerH2R4,
            panIsHomePickerH2Z5, panIsHomePickerH2R5,
            panIsHomePickerH2Z6, panIsHomePickerH2R6,
            panIsHomePickerH2Z7, panIsHomePickerH2R7,
            panIsHomePickerH2Z8, panIsHomePickerH2R8,
            };

            GbFunc.SetDoubleBuffered(gridInitLdMotor);
            GbFunc.SetDoubleBuffered(gridInitSawMotor);
            GbFunc.SetDoubleBuffered(gridInitSorterMotor);

            FindInputLabel(this.Controls);
            FindOutputButton(this.Controls);
        }

        public popInitialize(bool bLoader, bool bSaw, bool bSorter)
        {
            InitializeComponent();


            InitProcCircle = new MRG.Controls.UI.LoadingCircle();

            Controls.Add(InitProcCircle);
            InitProcCircle.Active = false;
            InitProcCircle.BackColor = System.Drawing.Color.Transparent;
            InitProcCircle.Color = System.Drawing.Color.Orange;
            InitProcCircle.InnerCircleRadius = 5;
            InitProcCircle.Location = new System.Drawing.Point(12, 12);
            InitProcCircle.Name = "InitProcCircle";
            InitProcCircle.NumberSpoke = 12;
            InitProcCircle.OuterCircleRadius = 11;
            InitProcCircle.RotationSpeed = 100;
            InitProcCircle.Size = new System.Drawing.Size(79, 59);
            InitProcCircle.SpokeThickness = 2;
            InitProcCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.MacOSX;
            InitProcCircle.TabIndex = 1054;


            bLdInitSkip = !bLoader;
            bSawInitSkip = !bSaw;
            bSorterInitSkip = !bSorter;

            btnLoader.BackColor = bLoader ? Color.Orange : System.Drawing.SystemColors.ButtonFace;
            btnSaw.BackColor = bSaw ? Color.Orange : System.Drawing.SystemColors.ButtonFace;
            btnSorter.BackColor = bSorter ? Color.Orange : System.Drawing.SystemColors.ButtonFace;

            SetListAxis();

            panIsHomePicker = new Panel[] {
            panIsHomePickerH1Z1, panIsHomePickerH1R1,
            panIsHomePickerH1Z2, panIsHomePickerH1R2,
            panIsHomePickerH1Z3, panIsHomePickerH1R3,
            panIsHomePickerH1Z4, panIsHomePickerH1R4,
            panIsHomePickerH1Z5, panIsHomePickerH1R5,
            panIsHomePickerH1Z6, panIsHomePickerH1R6,
            panIsHomePickerH1Z7, panIsHomePickerH1R7,
            panIsHomePickerH1Z8, panIsHomePickerH1R8,
            panIsHomePickerH2Z1, panIsHomePickerH2R1,
            panIsHomePickerH2Z2, panIsHomePickerH2R2,
            panIsHomePickerH2Z3, panIsHomePickerH2R3,
            panIsHomePickerH2Z4, panIsHomePickerH2R4,
            panIsHomePickerH2Z5, panIsHomePickerH2R5,
            panIsHomePickerH2Z6, panIsHomePickerH2R6,
            panIsHomePickerH2Z7, panIsHomePickerH2R7,
            panIsHomePickerH2Z8, panIsHomePickerH2R8,
            };

            GbFunc.SetDoubleBuffered(gridInitLdMotor);
            GbFunc.SetDoubleBuffered(gridInitSawMotor);
            GbFunc.SetDoubleBuffered(gridInitSorterMotor);

            FindInputLabel(this.Controls);
        }

        void SetListAxis()
        {
            listAxisLoader = new List<int>();
            listAxisSaw = new List<int>();
            listAxisSorter = new List<int>();

            for (int nMotCnt = 0; nMotCnt < SVDF.SV_CNT; nMotCnt++)
            {
                switch ((SVDF.AXES)nMotCnt)
                {
                    case SVDF.AXES.DRY_BLOCK_STG_X:
                    case SVDF.AXES.MAP_PK_X:
                    case SVDF.AXES.MAP_PK_Z:
                    case SVDF.AXES.MAP_VISION_Z:
                    case SVDF.AXES.MAP_STG_1_Y:
                    case SVDF.AXES.MAP_STG_2_Y:
                    case SVDF.AXES.MAP_STG_1_T:
                    case SVDF.AXES.MAP_STG_2_T:
                    case SVDF.AXES.CHIP_PK_1_X:
                    case SVDF.AXES.CHIP_PK_2_X:
                    case SVDF.AXES.BALL_VISION_Y:
                    case SVDF.AXES.BALL_VISION_Z:
                    case SVDF.AXES.TRAY_PK_X:
                    case SVDF.AXES.TRAY_PK_Z:
                    case SVDF.AXES.TRAY_PK_Y:
                    case SVDF.AXES.GD_TRAY_STG_1_Y:
                    case SVDF.AXES.GD_TRAY_STG_2_Y:
                    case SVDF.AXES.RW_TRAY_STG_Y:
                    case SVDF.AXES.GD_TRAY_1_ELV_Z:
                    case SVDF.AXES.GD_TRAY_2_ELV_Z:
                    case SVDF.AXES.RW_TRAY_ELV_Z:
                    case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                    case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                        listAxisSorter.Add(nMotCnt);
                        break;
                    case SVDF.AXES.MAGAZINE_ELV_Z:
                        listAxisLoader.Add(nMotCnt);
                        break;
                    case SVDF.AXES.LD_RAIL_T:
                    case SVDF.AXES.LD_RAIL_Y_FRONT:
                    case SVDF.AXES.LD_RAIL_Y_REAR:
                    case SVDF.AXES.BARCODE_Y:
                    case SVDF.AXES.LD_VISION_X:
                    case SVDF.AXES.STRIP_PK_X:
                    case SVDF.AXES.STRIP_PK_Z:
                    case SVDF.AXES.UNIT_PK_X:
                    case SVDF.AXES.UNIT_PK_Z:
                        listAxisSaw.Add(nMotCnt);
                        break;
                    default:
                        break;
                }
            }
        }

        void FindInputLabel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(InputLabel))
                    {
                        m_listInputLabel.Add((InputLabel)ctr);
                    }

                    if (ctr.Controls.Count > 0)
                        FindInputLabel(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void popInitialize_Load(object sender, EventArgs e)
        {
            try
            {
                this.CenterToScreen();
                GbVar.g_timeStampInit[MCDF.LOADER] = new Stopwatch();
                GbVar.g_timeStampInit[MCDF.SAW] = new Stopwatch();
                GbVar.g_timeStampInit[MCDF.SORTER] = new Stopwatch();
                tbcInit.ItemSize = new Size(0, 1);

                InitProcCircle.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.IE7;
                InitProcCircle.InnerCircleRadius = 10;
                InitProcCircle.OuterCircleRadius = 20;
                InitProcCircle.NumberSpoke = 10;
                InitProcCircle.RotationSpeed = 50;
                InitProcCircle.Active = true;


                gridInitLdMotor.RowCount = 0;
                gridInitSawMotor.RowCount = 0;
                gridInitSorterMotor.RowCount = 0;

                for (int nAxis = 0; nAxis < listAxisLoader.Count; nAxis++)
                {
                    gridInitLdMotor.RowCount++;
                    gridInitLdMotor.Rows[gridInitLdMotor.RowCount - 1].Cells[0].Value = (gridInitLdMotor.RowCount).ToString();
                    gridInitLdMotor.Rows[gridInitLdMotor.RowCount - 1].Cells[1].Value = ConfigMgr.Inst.Cfg.MotData[listAxisLoader[nAxis]].strAxisName;
                }
                for (int nAxis = 0; nAxis < listAxisSaw.Count; nAxis++)
                {
                    gridInitSawMotor.RowCount++;
                    gridInitSawMotor.Rows[gridInitSawMotor.RowCount - 1].Cells[0].Value = (gridInitSawMotor.RowCount).ToString();
                    gridInitSawMotor.Rows[gridInitSawMotor.RowCount - 1].Cells[1].Value = ConfigMgr.Inst.Cfg.MotData[listAxisSaw[nAxis]].strAxisName;
                }
                for (int nAxis = 0; nAxis < listAxisSorter.Count; nAxis++)
                {
                    gridInitSorterMotor.RowCount++;
                    gridInitSorterMotor.Rows[gridInitSorterMotor.RowCount - 1].Cells[0].Value = (gridInitSorterMotor.RowCount).ToString();
                    gridInitSorterMotor.Rows[gridInitSorterMotor.RowCount - 1].Cells[1].Value = ConfigMgr.Inst.Cfg.MotData[listAxisSorter[nAxis]].strAxisName;
                }

                bool bOnlyUiCheck = false;

                if (label2.Text != FormTextLangMgr.FindKey("Initializing..."))
                {
                    label2.Text = FormTextLangMgr.FindKey("Initializing...");
                }
                if (btnLoader.Text != FormTextLangMgr.FindKey("Loader"))
                {
                    btnLoader.Text = FormTextLangMgr.FindKey("Loader");
                }
                if (btnSaw.Text != FormTextLangMgr.FindKey("Dicing"))
                {
                    btnSaw.Text = FormTextLangMgr.FindKey("Dicing");
                }
                if (btnSorter.Text != FormTextLangMgr.FindKey("Sorter"))
                {
                    btnSorter.Text = FormTextLangMgr.FindKey("Sorter");
                }
                if (btnInitCancel.Text != FormTextLangMgr.FindKey("CANCEL"))
                {
                    btnInitCancel.Text = FormTextLangMgr.FindKey("CANCEL");
                }
                if (this.Text != FormTextLangMgr.FindKey("원점 복귀"))
                {
                    this.Text = FormTextLangMgr.FindKey("원점 복귀");
                }
#if _NOTEBOOK
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("노트북 모드 입니다. 이니셜 UI만 표시합니까?\nOK: UI만 봅니다.\nCANCEL:진짜 이니셜 시퀀스를 진행합니다."), "이니셜 UI 확인용");
                msg.TopMost = true;
                if (msg.ShowDialog(this) == DialogResult.OK) bOnlyUiCheck = true;
#endif

                nFlicker = 1;

                tmrRefresh.Start();
                swTimeStamp.Restart();

                if (bOnlyUiCheck == false)
                {
                    GbSeq.manualRun.SetMcInitializePara(!bLdInitSkip, !bSawInitSkip, !bSorterInitSkip);
                    GbSeq.manualRun.SetRunModule(MODULE_TYPE.ALL_HOME);
                    GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);
                }

                if (cbxLoader.Visible == bLdInitSkip) cbxLoader.Visible = !bLdInitSkip;
                if (cbxSaw.Visible == bSawInitSkip) cbxSaw.Visible = !bSawInitSkip;
                if (cbxSorter.Visible == bSorterInitSkip) cbxSorter.Visible = !bSorterInitSkip;
            }
            catch (Exception)
            {
            }
        }

        void FindOutputButton(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(ucManualOutputButton))
                    {
                        ucManualOutputButton tmp = ctr as ucManualOutputButton;
                    }

                    if (ctr.Controls.Count > 0)
                        FindOutputButton(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void OnFinishSeq(int nResult)
        {
            System.Diagnostics.Debug.WriteLine("Finished");

            if (this.InvokeRequired)
            {
                BeginInvoke(new CommonEvent.FinishSeqEvent(OnFinishSeq), nResult);
                return;
            }

            if (nResult == 0)
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
        }

        private void popInitialize_FormClosed(object sender, FormClosedEventArgs e)
        {
            tmrRefresh.Stop();
            MotionMgr.Inst.AllStop();
        }

        private void btnInitCancel_Click(object sender, EventArgs e)
        {
            //20201126 CHOH : BUTTON CLICK EVENT LOG 추가
            GbFunc.WriteEventLog("INITIALIZE POP UP WINDOW ", string.Format("CANCEL BUTTON CLICK"));

            GbSeq.manualRun.LeaveCycle(-1);
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void OnEqpInitStatusViewChangeButtonClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;

            if (int.TryParse(btn.Tag as string, out nTag))
            {
                tbcInit.SelectedIndex = nTag;
            }
        }

        private void OnInitPageHomeSelectButtonClick(object sender, EventArgs e)
        {
            tbcInit.SelectedIndex = 0;
        }

        private void OnInitPageNextSelectButtonClick(object sender, EventArgs e)
        {
            tbcInit.SelectedIndex = tbcInit.SelectedIndex + 1;
        }

        private void OnInitPagePrevSelectButtonClick(object sender, EventArgs e)
        {
            tbcInit.SelectedIndex = tbcInit.SelectedIndex - 1;
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", swTimeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Seconds));
            lblTime.DigitText = strElapsedTime;

            lbElapsedLoader.DigitText = string.Format("{0:00}:{1:00}:{2:00}", GbVar.g_timeStampInit[MCDF.LOADER].Elapsed.Hours, GbVar.g_timeStampInit[MCDF.LOADER].Elapsed.Minutes, GbVar.g_timeStampInit[MCDF.LOADER].Elapsed.Seconds);
            lbElapsedSaw.DigitText = string.Format("{0:00}:{1:00}:{2:00}", GbVar.g_timeStampInit[MCDF.SAW].Elapsed.Hours, GbVar.g_timeStampInit[MCDF.SAW].Elapsed.Minutes, GbVar.g_timeStampInit[MCDF.SAW].Elapsed.Seconds);
            lbElapsedSorter.DigitText = string.Format("{0:00}:{1:00}:{2:00}", GbVar.g_timeStampInit[MCDF.SORTER].Elapsed.Hours, GbVar.g_timeStampInit[MCDF.SORTER].Elapsed.Minutes, GbVar.g_timeStampInit[MCDF.SORTER].Elapsed.Seconds);

            foreach (DataGridView dgv in new DataGridView[] { gridInitLdMotor, gridInitSawMotor, gridInitSorterMotor })
            {
                for (int nRow = 0; nRow < dgv.RowCount; nRow++)
                {
                    int nAxis = 0;

                    if (dgv == gridInitLdMotor) nAxis = listAxisLoader[nRow];
                    else if (dgv == gridInitSawMotor) nAxis = listAxisSaw[nRow];
                    else if (dgv == gridInitSorterMotor) nAxis = listAxisSorter[nRow];

                    dgv.Rows[nRow].Cells[2].Style.BackColor = MotionMgr.Inst[nAxis].IsMinusLimit() ? Color.OrangeRed : Color.Silver;
                    dgv.Rows[nRow].Cells[3].Style.BackColor = MotionMgr.Inst[nAxis].IsHome() ? Color.Yellow : Color.Silver;
                    dgv.Rows[nRow].Cells[4].Style.BackColor = MotionMgr.Inst[nAxis].IsPlusLimit() ? Color.OrangeRed : Color.Silver;
                    dgv.Rows[nRow].Cells[5].Value = string.Format("{0:0.000}", MotionMgr.Inst[nAxis].GetRealPos());
                    dgv.Rows[nRow].Cells[6].Style.BackColor = GbVar.mcState.isHomeComplete[nAxis] ? Color.Lime : Color.Silver;
                }
            }

            for (int i = 0; i < panIsHomePicker.Length; i++)
            {
                if (GbVar.mcState.isHomeComplete[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i])
                {
                    if (panIsHomePicker[i].BackColor != Color.Lime) panIsHomePicker[i].BackColor = Color.Lime;
                }
                else
                {
                    if (MotionMgr.Inst.MOTION[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i].IsAlarm())
                    {
                        if (panIsHomePicker[i].BackColor != Color.Red) panIsHomePicker[i].BackColor = Color.Red;
                    }
                    else
                    {
                        if ((nFlicker / 5) % 2 == 0)
                        {
                            if (panIsHomePicker[i].BackColor != Color.Silver) panIsHomePicker[i].BackColor = Color.Silver;
                        }
                        else
                        {
                            if (panIsHomePicker[i].BackColor != Color.Lime) panIsHomePicker[i].BackColor = Color.Lime;
                        }
                    }
                }
            }
            nFlicker++;

            gridInitLdMotor.Invalidate();
            gridInitSawMotor.Invalidate();
            gridInitSorterMotor.Invalidate();

            #region IO CHECK
            #region LOADER
            Color OnColor = Color.Yellow;
            Color OffColor = System.Drawing.SystemColors.Control;

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3101] == 0 &&
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3100] == 0 &&
                GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SPARE_3102] == 1)
            {
                if (lbConvStop1F.BackColor != Color.Orange)
                    lbConvStop1F.BackColor = Color.Orange;
            }
            else
            {
                if (lbConvStop1F.BackColor != SystemColors.Control)
                    lbConvStop1F.BackColor = SystemColors.Control;
            }
            #endregion

            #region SORTER
            lbGoodTrayStageSafe.BackColor =
                (GbFunc.IsGdTrayTableYMoveSafe()) ? OnColor : OffColor;
            lbGoodTrayStageDanger.BackColor =
                (!GbFunc.IsGdTrayTableYMoveSafe()) ? OnColor : OffColor;
            #endregion

            foreach (ucManualOutputButton item in m_listOuputButton)
            {
                if (item.Output == IODF.OUTPUT.NONE) continue;

                if (item.Output >= 0 && item.Output < IODF.OUTPUT.MAX)
                {
                    item.On = GbVar.GB_OUTPUT[(int)item.Output] == 1;
                }
            }

            foreach (InputLabel item in m_listInputLabel)
            {
                if (item.Input == IODF.INPUT.NONE) continue;

                if (item.Input >= 0 && item.Input < IODF.INPUT.MAX)
                {
                    item.On = GbVar.GB_INPUT[(int)item.Input] == 1;
                }
            }
            #endregion
        }
    }
}
