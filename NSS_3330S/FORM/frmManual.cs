using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.UC;
using NSS_3330S.POP;
using NSS_3330S.MOTION;
using Glass;

namespace NSS_3330S.FORM
{
    public partial class frmManual : BaseSubMenu
    {
        private bool flagTimerRefreshStart = false;

        ComboBox[] m_Cboxs;
        ucSvManualJog[] m_Jogs;
        DataGridView[] m_Grids;

        Button[] m_btnPadVac;
        Button[] m_btnPadBlow;
        Button[] m_btnVacOnOff;
        Button[] m_btnConvOnOff;
        Label[] m_lbPickerVac;

        popManualRun popManual;

        Panel[] m_VacPanel = new Panel[17];
        Label[] m_VacLabel = new Label[17];
        List<AGauge> m_AGg = new List<AGauge>();
        //ucTempControl m_ucTemp = new ucTempControl();
        //ucUltrasonicComm m_ucUltra = new ucUltrasonicComm();

        int nDelay = 0;
        int PageNo
        {
            get
            {
                return tabManual.SelectedIndex;
            }
            set
            {
                tabManual.SelectedIndex = value;
            }
        }

        public frmManual()
        {
            InitializeComponent();
            FindControls(this.Controls);
            m_VacPanel = new Panel[]
            {
                pnlAGg1,pnlAGg2, pnlAGg4, pnlAGg5, pnlAGg6, pnlAGg8,pnlAGg10,
                pnlAGg12, pnlAGg15
            };
            m_VacLabel = new Label[]
            {
                lbAGgVal1, lbAGgVal2, lbAGgVal4, lbAGgVal5, lbAGgVal6, lbAGgVal8,
                lbAGgVal10, lbAGgVal12, lbAGgVal15
            };
            m_btnPadVac = new Button[]
            {
                btnPk1Pad1Vac,btnPk1Pad2Vac,btnPk1Pad3Vac,btnPk1Pad4Vac,btnPk1Pad5Vac,
                btnPk1Pad6Vac,btnPk1Pad7Vac,btnPk1Pad8Vac,
                btnPk2Pad1Vac,btnPk2Pad2Vac,btnPk2Pad3Vac,btnPk2Pad4Vac,
                btnPk2Pad5Vac,btnPk2Pad6Vac,btnPk2Pad7Vac,btnPk2Pad8Vac
            };
            m_btnPadBlow = new Button[]
            {
                btnPk1Pad1Blow,btnPk1Pad2Blow,btnPk1Pad3Blow,btnPk1Pad4Blow,btnPk1Pad5Blow,
                btnPk1Pad6Blow,btnPk1Pad7Blow,btnPk1Pad8Blow,
                btnPk2Pad1Blow,btnPk2Pad2Blow,btnPk2Pad3Blow,btnPk2Pad4Blow,
                btnPk2Pad5Blow,btnPk2Pad6Blow,btnPk2Pad7Blow,btnPk2Pad8Blow,
            };
            m_btnVacOnOff = new Button[]
            {
                btnVacOnOff0,btnVacOnOff1,btnVacOnOff2,btnVacOnOff3,
                btnVacOnOff7,btnVacOnOff8,btnVacOnOff9,
                btnVacOnOff10,btnVacOnOff11,btnVacOnOff12,
                btnVacOnOff13,btnVacOnOff14,btnVacOnOff15,
                btnVacOnOff18,btnVacOnOff19,btnVacOnOff20,
                btnVacOnOff24,btnVacOnOff25,btnVacOnOff26,
                btnVacOnOff30,btnVacOnOff31,btnVacOnOff35,
                btnVacOnOff36,btnVacOnOff44,btnVacOnOff45
            };
           
            m_lbPickerVac = new Label[]
            {
                lbVacH1P1,lbVacH1P2,lbVacH1P3,lbVacH1P4,lbVacH1P5,
                lbVacH1P6,lbVacH1P7,lbVacH1P8,
                lbVacH2P1,lbVacH2P2,lbVacH2P3,lbVacH2P4,lbVacH2P5,
                lbVacH2P6,lbVacH2P7,lbVacH2P8
            };

            m_btnConvOnOff = new Button[]
            {
                btn1stConvCw, btn1stConvCcw, btn1stConvStop,
            };

            m_Cboxs = new ComboBox[] { cbLoader, cbDicing, cbSorter, cbUnloading };
            m_Jogs = new ucSvManualJog[] { jogLoading, jogDicing, jogSorter, jogUnloading };
            m_Grids = new DataGridView[] { gridLoader, gridDicing, gridSorter, gridUnloading };
            InitComboBoxItems();
        }

        private void frmManual_Load(object sender, EventArgs e)
        {
            // 탭컨트롤 아이템사이즈 변경
            foreach (TabControl item in new TabControl[] { tabLoader, tabDicing, tabSorter, tabUnloading })
            {
                item.ItemSize = new Size(0, 1);
            }

            foreach (var item in m_Cboxs)
            {
                item.SelectedIndexChanged += cbAxes_SelectedIndexChanged;
                item.SelectedIndex = 0;
            }

            foreach (var item in m_Grids)
            {
                item.EditingControlShowing += gridRecipeMotion_EditingControlShowing;
                item.CellValueChanged += gridRecipeMotion_CellValueChanged;
                item.CellMouseClick += item_CellMouseClick;
            }

            AddAGgOnThePanel();

            FindInputLabel(this.Controls);
            FindOutputButton(this.Controls);
        }

        private void InitComboBoxItems()
        {
            foreach (var item in m_Cboxs)
            {
                item.Items.Clear();
            }

            foreach (SVDF.AXES axesName in Enum.GetValues(typeof(SVDF.AXES)))
            {
                switch (axesName)
                {
                    case SVDF.AXES.MAGAZINE_ELV_Z:
                    case SVDF.AXES.LD_RAIL_T:
                    case SVDF.AXES.LD_RAIL_Y_FRONT:
                    case SVDF.AXES.LD_RAIL_Y_REAR:
                    case SVDF.AXES.BARCODE_Y:
                    case SVDF.AXES.LD_VISION_X:
                        cbLoader.Items.Add(SVDF.GetAxisName(axesName));
                        break;
                    case SVDF.AXES.STRIP_PK_X:
                    case SVDF.AXES.STRIP_PK_Z:
                    case SVDF.AXES.UNIT_PK_X:
                    case SVDF.AXES.UNIT_PK_Z:
                        cbDicing.Items.Add(SVDF.GetAxisName(axesName));
                        break;
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
                    case SVDF.AXES.CHIP_PK_1_T_1:
                    case SVDF.AXES.CHIP_PK_1_Z_1:
                    case SVDF.AXES.CHIP_PK_2_T_1:
                    case SVDF.AXES.CHIP_PK_2_Z_1:
                        cbSorter.Items.Add(SVDF.GetAxisName(axesName));
                        break;
                    case SVDF.AXES.TRAY_PK_X:
                    case SVDF.AXES.TRAY_PK_Z:
                    case SVDF.AXES.TRAY_PK_Y:
                    case SVDF.AXES.GD_TRAY_1_ELV_Z:
                    case SVDF.AXES.GD_TRAY_2_ELV_Z:
                    case SVDF.AXES.RW_TRAY_ELV_Z:
                    case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                    case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                    case SVDF.AXES.GD_TRAY_STG_1_Y:
                    case SVDF.AXES.GD_TRAY_STG_2_Y:
                    case SVDF.AXES.RW_TRAY_STG_Y:
                        cbUnloading.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.MAX:
                    case SVDF.AXES.NONE:
                    default:
                        continue;
                }
            }
        }

        private void cbAxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadGridViewItem();
        }
        void FindInputLabel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(InputPanel))
                    {
                        InputPanel tmp = ctr as InputPanel;
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

        void LoadGridViewItem()
        {
            string m_strAxis = "";
            int m_nAxis = 0;

            m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
            m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);

            string strRecipe = "";
            double dRecipePos = 0;

            m_Grids[PageNo].Rows.Clear();

            for (int nIdx = 0; nIdx < POSDF.MAX_POS_COUNT; nIdx++)
            {
                strRecipe = POSDF.GetPosName(POSDF.GetTeachPosModeAxis(m_nAxis), nIdx);
                if (string.IsNullOrEmpty(strRecipe)) continue;

                dRecipePos = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dPos[nIdx];

                m_Grids[PageNo].Rows.Add(strRecipe, dRecipePos);
            }

            m_Jogs[PageNo].AXIS = SVDF.GetAxisByName(m_strAxis);
        }

        private void item_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            int nRow = e.RowIndex;
            int nCol = e.ColumnIndex;

            string m_strAxis = "";
            //int m_nAxis = 0;

            m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
            //m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);

            if (e.ColumnIndex == 2)
            {
                //MOVE 버튼 클릭시
                int nAxisNo = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);
                double dTargetPos = 0.0;
                dTargetPos = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dPos[nRow];
                double dSpeed = 0.0;
                dSpeed = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dVel[nRow];
                double dAcc = 0.0;
                dAcc = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dAcc[nRow];
                double dDec = 0.0;
                dDec = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dDec[nRow];

                string strTitle = FormTextLangMgr.FindKey("축 이동");
                string strMsg = string.Format("{0}, {1} {2}\n {3}{4} {5}{6} {7}{8} ",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nAxisNo))
                    , dTargetPos.ToString("F3")
                    , FormTextLangMgr.FindKey("으로 이동하시겠습니까?")
                    , FormTextLangMgr.FindKey("속도:")
                    , dSpeed.ToString("0.0")
                    , FormTextLangMgr.FindKey("가속도:")
                    , dAcc.ToString("0.0")
                    , FormTextLangMgr.FindKey("감속도:")
                    , dDec.ToString("0.0")
                    );
                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nAxisNo };
                m_dPosArray = new double[] { dTargetPos };
                m_nOrderArray = new uint[] { 0 };
                m_dSpeedArray = new double[] { dSpeed };
                m_dAccArray = new double[] { dAcc };
                m_dDecArray = new double[] { dDec };

                if (m_nAxisArray.Length == 1)
                {
                    if (m_nAxisArray[0] == (int)SVDF.AXES.STRIP_PK_Z)
                    {
                        if (MotionMgr.Inst[m_nAxisArray[0]].GetRealPos() > TeachMgr.Inst.Tch.dMotPos[m_nAxisArray[0]].dPos[POSDF.STRIP_PICKER_STRIP_LOADING])
                        {

                        }
                    }
                }

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }
        private void gridRecipeMotion_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            string m_strAxis = "";
            int m_nAxis = 0;

            m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
            m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //Y
            int nCol = dgv.CurrentCell.ColumnIndex;   //X
            if (nCol == 0)
            {
                dgv.Rows[nRow].Cells[nCol].Value = POSDF.GetPosName(POSDF.GetTeachPosModeAxis(m_nAxis), nRow);
                return;
            }
            if (dgv.Rows[nRow].Cells[nCol].Value == null)
            {
                popMessageBox messageBox = new popMessageBox("공백이 입력되었습니다.", "경고");
                messageBox.ShowDialog();
                LoadGridViewItem();
                return;
            }
            int str_length = dgv.Rows[nRow].Cells[nCol].Value.ToString().Length;
            int str_comp = 0;
            if (nCol < 1) return;
            for (int i = 0; i < str_length; i++)
            {
                if (dgv.Rows[nRow].Cells[nCol].Value.ToString()[i] == '.')
                {
                    str_comp++;
                }
            }
            if (dgv.Rows[nRow].Cells[nCol].Value.ToString()[str_length - 1] == '.' || str_comp > 1)
            {
                popMessageBox messageBox = new popMessageBox("소숫점 입력이 잘못되었습니다.", "경고");
                messageBox.ShowDialog();
                LoadGridViewItem();
                return;
            }
        }
        private void gridRecipeMotion_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress += new KeyPressEventHandler(gridRecipeMotion_KeyPress);
        }
        private void gridRecipeMotion_KeyPress(object sender, KeyPressEventArgs e)
        {

            int keyCode = (int)e.KeyChar;
            if ((keyCode < 48 || keyCode > 57) && keyCode != 8 && keyCode != 46)
            {
                e.Handled = true;
            }
        }

        private void rdbLoader1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabLoader.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabLoader.SelectedIndex = nTag;
            }
        }

        private void rdbDicing1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabDicing.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabDicing.SelectedIndex = nTag;
            }
        }

        private void rdbSorter1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabSorter.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabSorter.SelectedIndex = nTag;
            }
        }

        private void rdbUnloading1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabUnloading.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabUnloading.SelectedIndex = nTag;
            }
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            //RadioButton rdb = sender as RadioButton;
            //int nTag;
            //if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            //if (tabCleaner.TabPages.Count < nTag) return;

            //if (rdb.Checked)
            //{
            //    // 탭 전환
            //    tabCleaner.SelectedIndex = nTag;
            //}
        }

        private void tabManual_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadGridViewItem();

            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (GbVar.gbProgramExit)
            {
                tmrRefresh.Stop();
                return;
            }

            if (!this.Visible) return;

            if (flagTimerRefreshStart)
                return;

            flagTimerRefreshStart = true;

            try
            {
                foreach (AGauge item in m_AGg)
                {

                    int nTag = Convert.ToInt16(item.Tag.ToString());

                    item.Range_Idx = 0;
                    item.RangeEndValue = (float)ConfigMgr.Inst.Cfg.Vac[nTag].dVacLevelLow;
                    item.RangeStartValue = -100F;

                    item.Range_Idx = 1;
                    item.RangeEndValue = (float)ConfigMgr.Inst.Cfg.Vac[nTag].dVacLevelHigh;
                    item.RangeStartValue = item.RangesEndValue[0];

                    item.Range_Idx = 2;
                    item.RangeEndValue = 100F;
                    item.RangeStartValue = item.RangesEndValue[1];

                    item.Value = (float)((GbVar.GB_AINPUT[nTag] - ConfigMgr.Inst.Cfg.Vac[nTag].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nTag].dRatio);
                }

                foreach (Label item in m_VacLabel)
                {
                    int nTag = Convert.ToInt16(item.Tag.ToString());
                    double dValue = (GbVar.GB_AINPUT[nTag] - ConfigMgr.Inst.Cfg.Vac[nTag].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nTag].dRatio;

                    item.Text = dValue.ToString("0.00");

                    if (dValue < ConfigMgr.Inst.Cfg.Vac[nTag].dVacLevelLow)
                    {
                        if (item.BackColor != Color.SeaGreen) item.BackColor = Color.SeaGreen;
                    }
                    else
                    {
                        if (item.BackColor != SystemColors.Control) item.BackColor = SystemColors.Control;
                    }
                }

                foreach (ucManualOutputButton item in m_listOuputButton)
                {
                    if (item.Output == IODF.OUTPUT.NONE)
                    {
                        // 아웃풋 설정이 되지 않은 버튼이 있다
                    }

                    if (item.Output >= 0 && item.Output < IODF.OUTPUT.MAX)
                    {
                        item.On = GbVar.GB_OUTPUT[(int)item.Output] == 1;
                    }
                }

                foreach (InputPanel item in m_listInputPanel)
                {
                    if (item.Input == IODF.INPUT.NONE)
                    {
                        // 인풋 설정이 되지 않은 판넬이 있다
                    }

                    if (item.Input >= 0 && item.Input < IODF.INPUT.MAX)
                    {
                        item.On = GbVar.GB_INPUT[(int)item.Input] == 1;
                    }
                }

                //                foreach (ucServoStatus item in m_listServoStatus)
                //                {
                //#if _AJIN
                //                    item.RefreshUI();
                //#endif
                //                }

                // 패드 배큠 버튼 리프레시

                for (int i = 0; i < m_btnPadVac.Length; i++)
                {
                    if (GbVar.GB_CHIP_PK_VAC[i])
                    {
                        if (m_btnPadVac[i].BackColor != Color.Chartreuse)
                            m_btnPadVac[i].BackColor = Color.Chartreuse;
                    }
                    else
                    {
                        if (m_btnPadVac[i].BackColor != SystemColors.Control)
                            m_btnPadVac[i].BackColor = SystemColors.Control;
                    }


                    if (GbVar.GB_CHIP_PK_BLOW[i])
                    {
                        if (m_btnPadBlow[i].BackColor != Color.Chartreuse)
                            m_btnPadBlow[i].BackColor = Color.Chartreuse;
                    }
                    else
                    {
                        if (m_btnPadBlow[i].BackColor != SystemColors.Control)
                            m_btnPadBlow[i].BackColor = SystemColors.Control;
                    }
                }
                //m_btnPadVac
                //m_btnPadBlow

                foreach (Button item in m_btnVacOnOff)
                {
                    int nTag;
                    if (!int.TryParse(item.Tag.ToString(), out nTag)) continue;

                    if (GbFunc.IsVacOnOffColorOn(nTag))
                    {
                        if (item.BackColor != Color.Gold) item.BackColor = Color.Gold;
                    }
                    else
                    {
                        if (item.BackColor != SystemColors.Control) item.BackColor = SystemColors.Control;
                    }
                }

                foreach (Button item in m_btnConvOnOff)
                {
                    int nTag;
                    if (!int.TryParse(item.Tag.ToString(), out nTag)) continue;

                    if (GbFunc.IsConvOnOffColorOn(nTag))
                    {
                        if (item.BackColor != Color.Gold) item.BackColor = Color.Gold;
                    }
                    else
                    {
                        if (item.BackColor != SystemColors.Control) item.BackColor = SystemColors.Control;
                    }
                }

                if (GbDev.BarcodeReader.FLAG_RECEIVED == true)
                {
                    GbDev.BarcodeReader.FLAG_RECEIVED = false;
                    if (GbDev.BarcodeReader.FLAG_ERROR == true)
                    {
                        GbDev.BarcodeReader.FLAG_ERROR = false;
                        tbxBarcodeResult.Text = "READ_FAIL";
                        return;
                    }
                    tbxBarcodeResult.Text = GbDev.BarcodeReader.RESULT;
                }

                for (int i = 0; i < m_lbPickerVac.Length; i++)
                {
                    double dVacValue = ((GbVar.GB_AINPUT[(int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + i] -
                        ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + i].dDefaultVoltage) *
                        ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + i].dRatio);

                    if (m_lbPickerVac[i].Text != dVacValue.ToString("0.0")) m_lbPickerVac[i].Text = dVacValue.ToString("0.0");

                    if (dVacValue < ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + i].dVacLevelLow)
                    {
                        if (m_lbPickerVac[i].BackColor != Color.LightGreen) m_lbPickerVac[i].BackColor = Color.LightGreen;
                    }
                    else
                    {
                        if (m_lbPickerVac[i].BackColor != SystemColors.Control) m_lbPickerVac[i].BackColor = SystemColors.Control;
                    }
                }
                //if (m_ucTemp.TARGET_TEMP != GbDev.thermostat.GET_VALUE.strTargetTemp)
                //    m_ucTemp.TARGET_TEMP = GbDev.thermostat.GET_VALUE.strTargetTemp;
                //if (m_ucTemp.WATER_TANK_INNER_TEMP != GbDev.thermostat.GET_VALUE.strCH0RealTemp)
                //    m_ucTemp.WATER_TANK_INNER_TEMP = GbDev.thermostat.GET_VALUE.strCH0RealTemp;
                //if (m_ucTemp.WATER_RETURN_TEMP != GbDev.thermostat.GET_VALUE.strCH1RealTemp)
                //    m_ucTemp.WATER_RETURN_TEMP = GbDev.thermostat.GET_VALUE.strCH1RealTemp;
                //if (m_ucTemp.SYS_ERROR_STATUS != GbDev.thermostat.GET_VALUE.strErrorString)
                //    m_ucTemp.SYS_ERROR_STATUS = GbDev.thermostat.GET_VALUE.strErrorString;
                //if (m_ucTemp.SYS_OP_STATUS != GbDev.thermostat.GET_VALUE.strSysOpStatus)
                //    m_ucTemp.SYS_OP_STATUS = GbDev.thermostat.GET_VALUE.strSysOpStatus;
                //if (m_ucTemp.SYS_ERROR_STRING != GbDev.thermostat.GET_VALUE.strSysOpStatus)
                //    m_ucTemp.SYS_ERROR_STRING = GbDev.thermostat.GET_VALUE.strSysOpStatus;
                //GbDev.tempController.ReadTempController(TempController.RUN);
                //if (GbDev.tempController.GetRunStopStatus() == 0)
                //{
                //    if(btnTempConStart.BackColor != Color.Lime)
                //        btnTempConStart.BackColor = Color.Lime;
                //    if(btnTempConStop.BackColor != SystemColors.ButtonFace)
                //        btnTempConStop.BackColor = SystemColors.ButtonFace;
                //}
                //else
                //{
                //    if (btnTempConStart.BackColor != SystemColors.ButtonFace)
                //        btnTempConStart.BackColor = SystemColors.ButtonFace;
                //    if (btnTempConStop.BackColor != Color.OrangeRed)
                //        btnTempConStop.BackColor = Color.OrangeRed;
                //}
                //if (lblPvValue.Text != GbDev.tempController.GetCurrentPV_Int().ToString())
                //    lblPvValue.Text = GbDev.tempController.GetCurrentPV_Int().ToString();
                //if (lblSvValue.Text != GbDev.tempController.GetCurrentSV().ToString())
                //    lblSvValue.Text = GbDev.tempController.GetCurrentSV().ToString();
                //if (lbHeat.Text != GbDev.tempController.GetCurrentHeatValue().ToString())
                //    lbHeat.Text = GbDev.tempController.GetCurrentHeatValue().ToString();
                //if (lbCool.Text != GbDev.tempController.GetCurrentCoolValue().ToString())
                //    lbCool.Text = GbDev.tempController.GetCurrentCoolValue().ToString();

                if (GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_1] == 1 &&
                       GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_2] == 1 &&
                       GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_KNIFE] == 1)
                {
                    if(btnAir.BackColor != Color.Lime)
                    btnAir.BackColor = Color.Lime;
                }
                else
                {
                    if (btnAir.BackColor != SystemColors.Control)
                        btnAir.BackColor = SystemColors.Control;
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                flagTimerRefreshStart = false;
            }

        }

        private void frmManual_VisibleChanged(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = this.Visible;
        }

        void popCycMgz_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycMgz.FormClosed -= popCycMgz_FormClosed;
            GbForm.pCycMgz = null;
        }
        void popCycStrip_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycStrip.FormClosed -= popCycStrip_FormClosed;
            GbForm.pCycStrip = null;
        }
        void popCycUnit_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycUnit.FormClosed -= popCycUnit_FormClosed;
            GbForm.pCycUnit = null;
        }
        void popCycClean_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycClean.FormClosed -= popCycClean_FormClosed;
            GbForm.pCycClean = null;
        }
        void popCycMapTransfer_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycMapTransfer.FormClosed -= popCycMapTransfer_FormClosed;
            GbForm.pCycMapTransfer = null;
        }
        void popCycPnP_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycPnP.FormClosed -= popCycPnP_FormClosed;
            GbForm.pCycPnP = null;
        }
        void popCycTrayTransfer_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pCycTrayTransfer.FormClosed -= popCycTrayTransfer_FormClosed;
            GbForm.pCycTrayTransfer = null;
        }

        private void btnCycleMgz_Click(object sender, EventArgs e)
        {

        }

        private void btnCycleStripTr_Click(object sender, EventArgs e)
        {

        }

        private void btnCycleUnitTr_Click(object sender, EventArgs e)
        {

        }

        private void btnCycleClrNDry_Click(object sender, EventArgs e)
        {

        }

        private void btnCycleMapTr_Click(object sender, EventArgs e)
        {

        }

        private void btnCyclePnP_Click(object sender, EventArgs e)
        {

        }

        private void btnCycleTrayTr_Click(object sender, EventArgs e)
        {

        }

        private void btnPk1Pad1Vac_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            uint nData = 0;
            if (!MotionMgr.Inst.GetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 2, ref nData)) return;

            if (nData == 0)
            {
                // VAC BIT ON
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 2, true);
            }
            else
            {
                // VAC BIT ON
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 2, false);
            }
        }

        private void btnPk1Pad1Blow_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            uint nData = 0;
            if (!MotionMgr.Inst.GetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 3, ref nData)) return;

            if (nData == 0)
            {
                // BLOW BIT ON
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 3, true);
            }
            else
            {
                // BLOW BIT ON
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nTag), 3, false);
            }
        }

        void AddAGgOnThePanel()
        {
            int nTag = 0;
            for (int i = 0; i < m_VacPanel.Length; i++)
            {
                if (!int.TryParse(m_VacPanel[i].Tag.ToString(), out nTag)) continue;

                m_AGg.Add(new AGauge());

                m_AGg[i].BackColor = System.Drawing.Color.White;
                m_AGg[i].BaseArcColor = System.Drawing.Color.Gray;
                m_AGg[i].BaseArcRadius = 40;
                m_AGg[i].BaseArcStart = 180;
                m_AGg[i].BaseArcSweep = 180;
                m_AGg[i].BaseArcWidth = 2;
                m_AGg[i].Cap_Idx = ((byte)(1));
                m_AGg[i].CapColors = new System.Drawing.Color[] {
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black,
                                                                System.Drawing.Color.Black};
                m_AGg[i].CapPosition = new System.Drawing.Point(10, 10);
                m_AGg[i].CapsPosition = new System.Drawing.Point[] {
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10),
                                                                    new System.Drawing.Point(10, 10)};
                m_AGg[i].CapsText = new string[] {
                                        "",
                                        "",
                                        "",
                                        "",
                                        ""};
                m_AGg[i].CapText = "";
                m_AGg[i].Center = new System.Drawing.Point(80, 80);
                m_AGg[i].Location = new System.Drawing.Point(160, 15);
                m_AGg[i].MaxValue = 100F;
                m_AGg[i].MinValue = -100F;
                m_AGg[i].Name = "aGg" + i.ToString();
                m_AGg[i].NeedleColor1 = NSS_3330S.UC.AGauge.NeedleColorEnum.Gray;
                m_AGg[i].NeedleColor2 = System.Drawing.Color.DimGray;
                m_AGg[i].NeedleRadius = 40;
                m_AGg[i].NeedleType = 0;
                m_AGg[i].NeedleWidth = 2;
                m_AGg[i].RangesColor = new System.Drawing.Color[] {
                                                                    System.Drawing.Color.Lime,
                                                                    System.Drawing.Color.Gold,
                                                                    System.Drawing.Color.DeepSkyBlue,
                                                                    System.Drawing.SystemColors.Control,
                                                                    System.Drawing.SystemColors.Control};
                m_AGg[i].RangesEnabled = new bool[] {
                                                    true,
                                                    true,
                                                    true,
                                                    false,
                                                    false};
                m_AGg[i].RangesEndValue = new float[] {
                                                     -50F,
                                                     50F,
                                                     100F,
                                                     0F,
                                                     0F};
                m_AGg[i].RangesInnerRadius = new int[] {
                                                        40,
                                                        40,
                                                        40,
                                                        70,
                                                        70};
                m_AGg[i].RangesOuterRadius = new int[] {
                                                        50,
                                                        50,
                                                        50,
                                                        80,
                                                        80};
                m_AGg[i].RangesStartValue = new float[] {
                                                        -100F,
                                                        -50F,
                                                        50F,
                                                        0F,
                                                        0F};
                m_AGg[i].ScaleLinesInterColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleLinesInterInnerRadius = 42;
                m_AGg[i].ScaleLinesInterOuterRadius = 50;
                m_AGg[i].ScaleLinesInterWidth = 1;
                m_AGg[i].ScaleLinesMajorColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleLinesMajorInnerRadius = 40;
                m_AGg[i].ScaleLinesMajorOuterRadius = 50;
                m_AGg[i].ScaleLinesMajorStepValue = 50F;
                m_AGg[i].ScaleLinesMajorWidth = 2;
                m_AGg[i].ScaleLinesMinorColor = System.Drawing.Color.Gray;
                m_AGg[i].ScaleLinesMinorInnerRadius = 43;
                m_AGg[i].ScaleLinesMinorNumOf = 9;
                m_AGg[i].ScaleLinesMinorOuterRadius = 50;
                m_AGg[i].ScaleLinesMinorWidth = 1;
                m_AGg[i].ScaleNumbersColor = System.Drawing.Color.Black;
                m_AGg[i].ScaleNumbersFormat = null;
                m_AGg[i].ScaleNumbersRadius = 65;
                m_AGg[i].ScaleNumbersRotation = 0;
                m_AGg[i].ScaleNumbersStartScaleLine = 1;
                m_AGg[i].ScaleNumbersStepScaleLines = 2;
                m_AGg[i].Size = new System.Drawing.Size(160, 88);
                m_AGg[i].TabIndex = 0;
                m_AGg[i].Text = "aGg" + i.ToString();
                m_AGg[i].Value = 10F;

                m_AGg[i].Range_Idx = 0;
                m_AGg[i].RangeEndValue = (float)ConfigMgr.Inst.Cfg.Vac[nTag].dVacLevelLow;
                m_AGg[i].RangeStartValue = -100F;

                m_AGg[i].Range_Idx = 1;
                m_AGg[i].RangeEndValue = (float)ConfigMgr.Inst.Cfg.Vac[nTag].dVacLevelHigh;
                m_AGg[i].RangeStartValue = m_AGg[i].RangesEndValue[0];

                m_AGg[i].Range_Idx = 2;
                m_AGg[i].RangeEndValue = 100F;
                m_AGg[i].RangeStartValue = m_AGg[i].RangesEndValue[1];

                m_AGg[i].Dock = DockStyle.Fill;
                m_AGg[i].Tag = nTag.ToString();

                m_VacPanel[i].Controls.Add(m_AGg[i]);
            }
        }

        private void btnPk1VacAll_Click(object sender, EventArgs e)
        {
            bool bIsOn = false;

            if (btnPk1VacAll.BackColor != Color.Chartreuse)
            {
                btnPk1VacAll.BackColor = Color.Chartreuse;
                bIsOn = true;
            }
            else
            {
                btnPk1VacAll.BackColor = SystemColors.Control;
                bIsOn = false;
            }

            for (int i = 0; i < 8; i++)
            {
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), 2, bIsOn);
            }
        }

        private void btnPk1BlowAll_Click(object sender, EventArgs e)
        {
            bool bIsOn = false;

            if (btnPk1BlowAll.BackColor != Color.Chartreuse)
            {
                btnPk1BlowAll.BackColor = Color.Chartreuse;
                bIsOn = true;
            }
            else
            {
                btnPk1BlowAll.BackColor = SystemColors.Control;
                bIsOn = false;
            }

            for (int i = 0; i < 8; i++)
            {
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), 3, bIsOn);
            }
        }

        private void btnPk2VacAll_Click(object sender, EventArgs e)
        {
            bool bIsOn = false;

            if (btnPk2VacAll.BackColor != Color.Chartreuse)
            {
                btnPk2VacAll.BackColor = Color.Chartreuse;
                bIsOn = true;
            }
            else
            {
                btnPk2VacAll.BackColor = SystemColors.Control;
                bIsOn = false;
            }

            for (int i = 0; i < 8; i++)
            {
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), 2, bIsOn);
            }
        }

        private void btnPk2BlowAll_Click(object sender, EventArgs e)
        {
            bool bIsOn = false;

            if (btnPk2BlowAll.BackColor != Color.Chartreuse)
            {
                btnPk2BlowAll.BackColor = Color.Chartreuse;
                bIsOn = true;
            }
            else
            {
                btnPk2BlowAll.BackColor = SystemColors.Control;
                bIsOn = false;
            }

            for (int i = 0; i < 8; i++)
            {
                MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), 3, bIsOn);
            }
        }

        private void btnVacOnOff_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            GbFunc.ManualVacBlow(nTag);
        }

        private void btnConv_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            GbFunc.ManualConvOnOff(nTag);
        }

        private void btnTrayRfidRead_Click(object sender, EventArgs e)
        {
            //GbDev.TRAY_RFID.Read(0);
            //lbTrayRfid.BackColor = Color.LightGray;
            //lbTrayRfid.Text = "";
        }

        public void deleRecvMgzRfidData(int nPort)
        {
            //if (this.InvokeRequired)
            //{
            //    this.Invoke((MethodInvoker)delegate()
            //    {
            //        if (nPort == 1) cbbPortNo.SelectedIndex = 0;
            //        else if (nPort == 6) cbbPortNo.SelectedIndex = 1;
            //        else if (nPort == 3) cbbPortNo.SelectedIndex = 2;
            //        else if (nPort == 2) cbbPortNo.SelectedIndex = 3;

            //        lbMgzId.BackColor = Color.White;
            //        lbMgzId.Text = GbDev.LD_RFID.GETID[nPort];
            //    });
            //}
            //else
            //{
            //    if (nPort == 1) cbbPortNo.SelectedIndex = 0;
            //    else if (nPort == 6) cbbPortNo.SelectedIndex = 1;
            //    else if (nPort == 3) cbbPortNo.SelectedIndex = 2;
            //    else if (nPort == 2) cbbPortNo.SelectedIndex = 3;

            //    lbMgzId.BackColor = Color.White;
            //    lbMgzId.Text = GbDev.LD_RFID.GETID[nPort];
            //}
        }

        private void btnBarcodeRead_Click(object sender, EventArgs e)
        {
            if (!GbDev.BarcodeReader.IsConnect())
            {
                DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();
                if (!System.IO.File.Exists(PathMgr.Inst.PATH_BARCODEREADER))
                {
                    remoteInfo.remotePath = "169.254.60.223";
                    remoteInfo.port = 23;

                    remoteInfo.Serialize(PathMgr.Inst.PATH_BARCODEREADER);
                }
                else
                {
                    remoteInfo = DionesTool.DATA.RemoteInfo.Deserialize(PathMgr.Inst.PATH_BARCODEREADER);
                }
#if !_NOTEBOOK
                if (!GbDev.BarcodeReader.Connect_Tcp(remoteInfo.remotePath, remoteInfo.port))
                {
                    MessageBox.Show("Disconnect!");
                }
#endif
                return;
            }
            GbDev.BarcodeReader.TriggerOn();
        }
        private void btnDoorLock_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            nTag = btn.GetTag();

            if (nTag == 0 )
            {
                if (SafetyMgr.Inst.GetSafetyDoorLoader() != FNC.SUCCESS)
                {
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0)
                    {
                        popMessageBox msg = new popMessageBox("닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                        msg.TopMost = true;
                        if (msg.ShowDialog(this) != DialogResult.OK) return;
                        return;
                    }
                }
            }
            else
            {
                if (SafetyMgr.Inst.GetSafetyDoorSorter() != FNC.SUCCESS)
                {
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0)
                    {
                        popMessageBox msg = new popMessageBox("닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                        msg.TopMost = true;
                        if (msg.ShowDialog(this) != DialogResult.OK) return;
                        return;
                    }
                }
            }


        }

        private void btnDoorUnLock_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            nTag = btn.GetTag();

            if (nTag == 0)
            {
                GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, false);
            }
            else
            {
                GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL, false);
            }
        }

        private void btnBarcodeRead_MouseDown(object sender, MouseEventArgs e)
        {
            if (!GbDev.BarcodeReader.IsConnect())
            {
                DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();
                if (!System.IO.File.Exists(PathMgr.Inst.PATH_BARCODEREADER))
                {
                    remoteInfo.remotePath = "169.254.60.223";
                    remoteInfo.port = 23;

                    remoteInfo.Serialize(PathMgr.Inst.PATH_BARCODEREADER);
                }
                else
                {
                    remoteInfo = DionesTool.DATA.RemoteInfo.Deserialize(PathMgr.Inst.PATH_BARCODEREADER);
                }
#if !_NOTEBOOK
                if (!GbDev.BarcodeReader.Connect_Tcp(remoteInfo.remotePath, remoteInfo.port))
                {
                    MessageBox.Show("Disconnect!");
                }
#endif
                return;
            }
            GbDev.BarcodeReader.TriggerOn();
        }

        private void btnBarcodeRead_MouseUp(object sender, MouseEventArgs e)
        {
            GbDev.BarcodeReader.TriggerOff();
        }

        private void tabManual_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP)
                {
                    gridDicing.Enabled = false;
                    gridLoader.Enabled = false;
                    gridSorter.Enabled = false;
                    gridUnloading.Enabled = false;
                }
                else
                {
                    gridDicing.Enabled = true;
                    gridLoader.Enabled = true;
                    gridSorter.Enabled = true;
                    gridUnloading.Enabled = true;
                }
            }
        }

        private void btnAir_Click(object sender, EventArgs e)
        {
            if (GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_1] == 0 ||
                       GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_2] == 0 ||
                       GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT] == 0 ||
                       GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE] == 0 ||
                       GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE] == 0 ||
                       GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_KNIFE] == 0)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, true);
            }
            else
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);
            }

        }
    }
}
