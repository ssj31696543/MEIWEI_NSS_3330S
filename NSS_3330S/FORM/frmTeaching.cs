using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using NSS_3330S.GLOBAL;
using NSS_3330S.UC;
using NSS_3330S.POP;
using NSS_3330S.MOTION;

using Glass;

namespace NSS_3330S.FORM
{
    public partial class frmTeaching : BaseTeachingPosControl
    {
        #region UI HISTORY
        // 2022.05.01 : STRIP PICKER -> CYCLE RUN -> CUTTING TABLE 1,2를 L,R로 변경
        #endregion

        private bool flagTimerPopRmsRun = false;
        private bool flagTimerRefreshStart = false;
        private bool flagTimerRefresh100msStart = false;
        public delegate void EqRcpChange(MESDF.ePPChangeCode mode, string rcpName, bool isHostBody = false);
        public event EqRcpChange eqRcpChange = null;

        public static int G_Row;
        public static int G_Col;

        ComboBox[] m_Cboxs;
        ucServoStatus[] m_Servos;
        DataGridView[] m_Grids;

        List<ucManualOutputButton> m_listOuputButton = new List<ucManualOutputButton>();
        List<InputPanel> m_listInputPanel = new List<InputPanel>();

        Button[] m_btnVacOnOff;
        Button[] m_btnConvOnOff;

        Label[] m_lbPkSvOn;
        CheckBox[] m_cbxPkZ;
        CheckBox[] m_cbxPkT;
        DateTime dtPkPadDelay = DateTime.Now;

        //List<RcpMotPosEditButtonEx> m_listMotPosBtn = new List<RcpMotPosEditButtonEx>();
        //List<RcpMotPosGetButtonEx> m_listMotPosGet = new List<RcpMotPosGetButtonEx>();
        //List<RcpMotPosMoveButtonEx> m_listMotPosMove = new List<RcpMotPosMoveButtonEx>();

        popKeypad pKeyPad;

        FileInfo[] m_fLdMgzInfo;
        FileInfo[] m_fMapTableInfo;
        FileInfo[] m_fUldTrayInfo;

        int PageNo
        {
            get
            {
                return tabPageAxes.SelectedIndex;
            }
            set
            {
                tabPageAxes.SelectedIndex = value;
            }
        }

        int nSelectedMgzNo = 0;
        public frmTeaching()
        {
            InitializeComponent();
            LinkTeachingControl();

            FindOutputButton(this.Controls);
            FindInputPanel(this.Controls);
            //FindMotPosButton(this.Controls);
            //FindMotPosGet(this.Controls);
            //FindMotPosMove(this.Controls);

            // 탭컨트롤 아이템사이즈 변경
            foreach (TabControl item in new TabControl[] 
            {tbcLoaderTeachGroup, tbcStripTransferTeachGroup, tbcUnitTransferTeachGroup, tbcDryStageTeachGroup,
              tbcMapTransferTeachGroup, tbcPnPTeachGroup, tbcUldTrayTeachGroup, tabLoader, tabStripPk, tabDryStage,
              tabUnitPk, tabMaptable, tabSorter, tabUnloadTray })
            {
                item.ItemSize = new Size(0, 1);
            }

            tabPageAxes.Location = new Point(tabPageAxes.Location.X, -tabPageAxes.ItemSize.Height);
            m_Cboxs = new ComboBox[] { cbMzLoaderAxes, cbStripTransferAxes, cbStripLoadingAxes,cbDryStageAxes, cbVisionTableAxes, cbPickerHeadAxes, cbTrayTransferAxes };
            m_Servos = new ucServoStatus[] { ucMzLoaderServoStatus, ucStripTransferServoStatus, ucStripLoadingServoStatus,ucDryStageServoStatus, ucVisionTableServoStatus,
                ucPickerHeadServoStatus, ucTrayTransferServoStatus};
            m_Grids = new DataGridView[] { gridMzLoaderPosition, gridStripTransferPosition, gridStripLoadingPosition, gridDryStagePosition, gridVisionTablePosition,
                gridPickerHeadPosition, gridTrayTransferPosition };

            m_lbPkSvOn = new Label[] {
                lbPk1Z1,lbPk1T1,lbPk1Z2,lbPk1T2,lbPk1Z3,lbPk1T3,lbPk1Z4,lbPk1T4,lbPk1Z5,lbPk1T5,lbPk1Z6,lbPk1T6,lbPk1Z7,lbPk1T7,lbPk1Z8,lbPk1T8,
                lbPk2Z1,lbPk2T1,lbPk2Z2,lbPk2T2,lbPk2Z3,lbPk2T3,lbPk2Z4,lbPk2T4,lbPk2Z5,lbPk2T5,lbPk2Z6,lbPk2T6,lbPk2Z7,lbPk2T7,lbPk2Z8,lbPk2T8
            };

            m_cbxPkZ = new CheckBox[] {
                cbxPk1Z1,cbxPk1Z2,cbxPk1Z3,cbxPk1Z4,cbxPk1Z5,cbxPk1Z6,cbxPk1Z7,cbxPk1Z8,
                cbxPk2Z1,cbxPk2Z2,cbxPk2Z3,cbxPk2Z4,cbxPk2Z5,cbxPk2Z6,cbxPk2Z7,cbxPk2Z8
            };

            m_cbxPkT = new CheckBox[] {
                cbxPk1T1,cbxPk1T2,cbxPk1T3,cbxPk1T4,cbxPk1T5,cbxPk1T6,cbxPk1T7,cbxPk1T8,
                cbxPk2T1,cbxPk2T2,cbxPk2T3,cbxPk2T4,cbxPk2T5,cbxPk2T6,cbxPk2T7,cbxPk2T8
            };

            m_btnVacOnOff = new Button[]{
                btnVacOnOff7, btnVacOnOff8, btnVacOnOff9, btnVacOnOff10,
                btnVacOnOff11, btnVacOnOff12, btnVacOnOff13, btnVacOnOff14,
                btnVacOnOff15, btnVacOnOff18, btnVacOnOff19, btnVacOnOff20,
                btnVacOnOff24, btnVacOnOff25, btnVacOnOff26, btnVacOnOff30,
                btnVacOnOff31, btnVacOnOff32, btnVacOnOff35, btnVacOnOff36, btnVacOnOff37,
                btnAirKnife1_ON, btnAirKnife2_ON,btnAirKnife1_OFF, btnAirKnife2_OFF
            };

            foreach (DataGridView item in m_Grids)
            {
                item.EditingControlShowing += gridRecipeMotion_EditingControlShowing;
                item.CellValueChanged += gridRecipeMotion_CellValueChanged;
                item.CellClick += gridMzLoaderPosition_CellClick;
                item.CellBeginEdit += gridRecipeMotion_CellBeginEdit;
            }

            foreach (ucServoStatus item in m_Servos)
            {
                item.servoHomeClicked += item_servoHomeClicked;
            }

            InitComboBoxItems();
        }


        void FindOutputButton(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(ucManualOutputButton))
                    {
                        m_listOuputButton.Add((ucManualOutputButton)ctr);
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

        void FindInputPanel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(InputPanel))
                    {
                        m_listInputPanel.Add((InputPanel)ctr);
                    }

                    if (ctr.Controls.Count > 0)
                        FindInputPanel(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void frmTeaching_Load(object sender, EventArgs e)
        {

            foreach (var item in m_Cboxs)
            {
                item.SelectedIndexChanged += cbAxes_SelectedIndexChanged;
                item.SelectedIndex = 0;
            }

            foreach (var item in m_Grids)
            {
                item.CellMouseClick += item_CellMouseClick;
            }

            foreach (Button item in m_btnVacOnOff)
            {
                item.MouseClick += btnVacOnOff_Click;
            }

            //foreach (Button item in m_btnConvOnOff)
            //{
            //    item.MouseClick += btnConv_Click;
            //}

            toolTip1.IsBalloon = true;

            UpdateTchData();

            tmrRefresh100ms.Enabled = true;
        }


        private void btnVacOnOff_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            GbFunc.ManualVacBlow(nTag);
        }

        private void rdbLdTeaching_CheckedChanged(object sender, EventArgs e)
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

        private void rdbStripTeaching_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabStripPk.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabStripPk.SelectedIndex = nTag;
            }
        }


        private void rdbUnitTeaching_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabUnitPk.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabUnitPk.SelectedIndex = nTag;
            }
        }

        private void rdbDryStageTeaching_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabDryStage.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabDryStage.SelectedIndex = nTag;
            }
        }

        private void rdbClaenerTeaching_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabMaptable.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabMaptable.SelectedIndex = nTag;
            }
        }

        private void rdbSoterTeaching_CheckedChanged(object sender, EventArgs e)
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

        private void rdbUldTrayTeaching_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabUnloadTray.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabUnloadTray.SelectedIndex = nTag;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

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
                        cbMzLoaderAxes.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.DRY_BLOCK_STG_X:
                        cbDryStageAxes.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.MAP_PK_X:
                    case SVDF.AXES.MAP_PK_Z:
                    case SVDF.AXES.MAP_VISION_Z:
                    case SVDF.AXES.MAP_STG_1_Y:
                    case SVDF.AXES.MAP_STG_2_Y:
                    case SVDF.AXES.MAP_STG_1_T:
                    case SVDF.AXES.MAP_STG_2_T:
                        cbVisionTableAxes.Items.Add(SVDF.GetAxisName(axesName));    
                        break;

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
                        cbTrayTransferAxes.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.BALL_VISION_Y:
                    case SVDF.AXES.BALL_VISION_Z:
                    case SVDF.AXES.CHIP_PK_1_X:
                    case SVDF.AXES.CHIP_PK_2_X:
                    case SVDF.AXES.CHIP_PK_1_Z_1:
                    case SVDF.AXES.CHIP_PK_1_T_1:
                    case SVDF.AXES.CHIP_PK_2_Z_1:
                    case SVDF.AXES.CHIP_PK_2_T_1:
                        cbPickerHeadAxes.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.LD_RAIL_T:
                    case SVDF.AXES.LD_RAIL_Y_FRONT:
                    case SVDF.AXES.LD_RAIL_Y_REAR:
                    case SVDF.AXES.BARCODE_Y:
                    case SVDF.AXES.LD_VISION_X:
                    case SVDF.AXES.STRIP_PK_X:
                    case SVDF.AXES.STRIP_PK_Z:
                        cbStripTransferAxes.Items.Add(SVDF.GetAxisName(axesName));
                        break;

                    case SVDF.AXES.UNIT_PK_X:
                    case SVDF.AXES.UNIT_PK_Z:
                        cbStripLoadingAxes.Items.Add(SVDF.GetAxisName(axesName));   
                        break;
                    default:
                        break;
                }
            }
        }

        private void cbAxes_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadGridViewItem();
        }

        void LoadGridViewItem()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    string m_strAxis = "";
                    int m_nAxis = 0;

                    m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
                    m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);

                    string strTeach= "";
                    double dTeachPos = 0;
                    double dTeachVel = 0;
                    double dTeachAcc = 0;
                    double dTeachDec = 0;

                    m_Grids[PageNo].Rows.Clear();

                    for (int nIdx = 0; nIdx < POSDF.MAX_POS_COUNT; nIdx++)
                    {
                        strTeach = POSDF.GetPosName(POSDF.GetTeachPosModeAxis(m_nAxis), nIdx);
                        if (string.IsNullOrEmpty(strTeach)) continue;

                        dTeachPos = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dPos[nIdx];
                        dTeachVel = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dVel[nIdx];
                        dTeachAcc = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dAcc[nIdx];
                        dTeachDec = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dDec[nIdx];

                        m_Grids[PageNo].Rows.Add(strTeach, dTeachPos, dTeachVel, dTeachAcc, dTeachDec);

                        //220510 pjh 레시피 값이라면 READONLY = true
                        bool bReadOnly = IsRecipeValue(m_nAxis, nIdx);
                        m_Grids[PageNo].Rows[nIdx].Cells[1].ReadOnly = bReadOnly;
                        m_Grids[PageNo].Rows[nIdx].Cells[5].ReadOnly = bReadOnly;
                    }

                    m_Servos[PageNo].AXIS = SVDF.GetAxisByName(m_strAxis);
                    m_Servos[PageNo].AXIS_NAME = SVDF.GetAxisName((int)m_Servos[PageNo].AXIS);
                });
            }
            else
            {
                string m_strAxis = "";
                int m_nAxis = 0;

                m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
                m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);

                string strTeach = "";
                double dTeachPos = 0;
                double dTeachVel = 0;
                double dTeachAcc = 0;
                double dTeachDec = 0;

                m_Grids[PageNo].Rows.Clear();

                for (int nIdx = 0; nIdx < POSDF.MAX_POS_COUNT; nIdx++)
                {
                    strTeach = POSDF.GetPosName(POSDF.GetTeachPosModeAxis(m_nAxis), nIdx);
                    if (string.IsNullOrEmpty(strTeach))
                    {
                        continue;
                    }
                    dTeachPos = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dPos[nIdx];
                    dTeachVel = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dVel[nIdx];
                    dTeachAcc = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dAcc[nIdx];
                    dTeachDec = TeachMgr.Inst.Tch.dMotPos[m_nAxis].dDec[nIdx];

                    m_Grids[PageNo].Rows.Add(strTeach, dTeachPos, dTeachVel, dTeachAcc, dTeachDec);

                    //220510 pjh 레시피 값이라면 READONLY = true
                    bool bReadOnly = IsRecipeValue(m_nAxis, nIdx);
                    m_Grids[PageNo].Rows[nIdx].Cells[1].ReadOnly = bReadOnly;
                    m_Grids[PageNo].Rows[nIdx].Cells[5].ReadOnly = bReadOnly;
                }

                m_Servos[PageNo].AXIS = SVDF.GetAxisByName(m_strAxis);
                m_Servos[PageNo].AXIS_NAME = SVDF.GetAxisName((int)m_Servos[PageNo].AXIS);
            }
        }

        private void OnRcpTeacingPosSubChangeButtonClick(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;

            if (rdb.Checked)
            {
                int nTag;
                if (int.TryParse(rdb.Tag.ToString(), out nTag))
                {
                    tabPageAxes.SelectedIndex = nTag;
                    LoadGridViewItem();
                }
            }
        }



        private void btnPkPadSvOn_Click(object sender, EventArgs e)
        {
            int nAxisNo = 0;

            for (int i = 0; i < m_cbxPkZ.Length; i++)
            {
                if (m_cbxPkZ[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].SetServoOnOff(true);
                }
            }
            for (int i = 0; i < m_cbxPkT.Length; i++)
            {
                if (m_cbxPkT[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_T_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].SetServoOnOff(true);
                }
            }

            dtPkPadDelay = DateTime.Now;
            tmrPkSvHomeDelay.Enabled = true;
        }

        private void btnPkPadSvOff_Click(object sender, EventArgs e)
        {
            int nAxisNo = 0;

            for (int i = 0; i < m_cbxPkZ.Length; i++)
            {
                if (m_cbxPkZ[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].SetServoOnOff(false);
                }
            }
            for (int i = 0; i < m_cbxPkT.Length; i++)
            {
                if (m_cbxPkT[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_T_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].SetServoOnOff(false);
                }
            }

            dtPkPadDelay = DateTime.Now;
            tmrPkSvHomeDelay.Enabled = true;
        }

        private void btnPkPadSvHome_Click(object sender, EventArgs e)
        {
            int nAxisNo = 0;

            for (int i = 0; i < m_cbxPkZ.Length; i++)
            {
                if (m_cbxPkZ[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].HomeStart();
                    GbVar.mcState.isHomeComplete[nAxisNo] = true;
                }
            }
            for (int i = 0; i < m_cbxPkT.Length; i++)
            {
                if (m_cbxPkT[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_T_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].HomeStart();
                    GbVar.mcState.isHomeComplete[nAxisNo] = true;
                }
            }

            dtPkPadDelay = DateTime.Now;
            tmrPkSvHomeDelay.Enabled = true;
        }

        private void btnPkPadSvResetAlarm_Click(object sender, EventArgs e)
        {
            int nAxisNo = 0;

            for (int i = 0; i < m_cbxPkZ.Length; i++)
            {
                if (m_cbxPkZ[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].ResetAlarm();
                }
            }
            for (int i = 0; i < m_cbxPkT.Length; i++)
            {
                if (m_cbxPkT[i].Checked)
                {
                    nAxisNo = (int)SVDF.AXES.CHIP_PK_1_T_1 + (i * 2);
                    MotionMgr.Inst[nAxisNo].ResetAlarm();
                }
            }

            dtPkPadDelay = DateTime.Now;
            tmrPkSvHomeDelay.Enabled = true;
        }

        void item_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            string m_strAxis = "";
            int m_nAxis = 0;
            if (e.RowIndex == -1) return;

            m_strAxis = m_Cboxs[PageNo].SelectedItem.ToString();
            m_nAxis = (int)System.Enum.Parse(typeof(SVDF.AXES), m_strAxis, true);
            if (e.ColumnIndex == 5)
            {
                //GET 버튼 클릭시
                bool bReadOnly = IsRecipeValue(m_nAxis, e.RowIndex);
                if (bReadOnly) return;

                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = MOTION.MotionMgr.Inst[m_nAxis].GetRealPos().ToString("F3");
            }
            else if (e.ColumnIndex == 6)
            {
                //MOVE 버튼 클릭시

            }
        }

        private void gridGetButton_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                //TODO - Button Clicked - Execute Code Here
            }
        }
        private void gridMoveButton_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                //TODO - Button Clicked - Execute Code Here
            }
        }

        public void UpdateData()
        {
            TeachMgr.Inst.CopyTempTch();
        }

        bool CheckValidation()
        {
            popMessageBox popMsgWin = null;
            string msg = "";

            Teach tempTch = TeachMgr.Inst.TempTch;
            //VALIDATION CHECK
            {
                //tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_1F_DOWN_1]
                //    = tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1];
                //tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_LD_1F_UP_1]
                //    = tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1];

                //tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_2F_DOWN_1]
                //    = tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1];
                //tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_UP_1]
                //    = tempTch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Y].dPos[POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1];
            }

            if (false)
            {
                msg = string.Format("");

                popMsgWin = new popMessageBox(msg, "Warning", MessageBoxButtons.OK);
                popMsgWin.ShowDialog(this);
                return false;
            }
            return true;
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
                switch (nCol)
                {
                    case 1:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dPos[nRow];
                        }
                        break;
                    case 2:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dVel[nRow];
                        }
                        break;
                    case 3:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dAcc[nRow];
                        }
                        break;
                    case 4:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dDec[nRow];
                        }
                        break;
                    default:
                        break;
                }
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
                switch (nCol)
                {
                    case 1:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dPos[nRow];
                        }
                        break;
                    case 2:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dVel[nRow];
                        }
                        break;
                    case 3:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dAcc[nRow];
                        }
                        break;
                    case 4:
                        {
                            dgv.Rows[nRow].Cells[nCol].Value = TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dDec[nRow];
                        }
                        break;
                    default:
                        break;
                }
                return;
            }
            int nCnt = 0;

            nCnt = 1;
            try
            {
                TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dPos[nRow] = double.Parse(dgv.Rows[nRow].Cells[nCnt++].Value.ToString());
                TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dVel[nRow] = double.Parse(dgv.Rows[nRow].Cells[nCnt++].Value.ToString());
                TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dAcc[nRow] = double.Parse(dgv.Rows[nRow].Cells[nCnt++].Value.ToString());
                TeachMgr.Inst.TempTch.dMotPos[m_nAxis].dDec[nRow] = double.Parse(dgv.Rows[nRow].Cells[nCnt++].Value.ToString());
            }
            catch (Exception ex)
            {

            }
    
        }

        private bool IsRecipeValue(int m_nAxis, int nPos )
        {
            try
            {

                switch ((SVDF.AXES)m_nAxis)
                {
                    case SVDF.AXES.LD_RAIL_T:
                        if (nPos == POSDF.LD_RAIL_T_STRIP_LOADING ||
                            nPos == POSDF.LD_RAIL_T_STRIP_ALIGN)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.MAP_PK_X:
                        if (nPos == POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 ||
                            nPos == POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2 ||
                            nPos == POSDF.MAP_PICKER_MAP_VISION_START_T1 ||
                            nPos == POSDF.MAP_PICKER_MAP_VISION_START_T2 ||
                            nPos == POSDF.MAP_PICKER_MAP_VISION_END_T1 ||
                            nPos == POSDF.MAP_PICKER_MAP_VISION_END_T2)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.MAP_STG_1_Y:
                    case SVDF.AXES.MAP_STG_2_Y:
                        if (nPos == POSDF.MAP_STAGE_UNIT_UNLOADING_P1 ||
                            nPos == POSDF.MAP_STAGE_UNIT_UNLOADING_P2 ||
                            nPos == POSDF.MAP_STAGE_TOP_ALIGN_MARK1 ||
                            nPos == POSDF.MAP_STAGE_TOP_ALIGN_MARK2 ||
                            nPos == POSDF.MAP_STAGE_MAP_VISION_START ||
                            nPos == POSDF.MAP_STAGE_MAP_VISION_END)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.CHIP_PK_1_X:
                    case SVDF.AXES.CHIP_PK_2_X:
                        if (nPos == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 ||
                            nPos == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2 ||
                            nPos == POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 ||
                            nPos == POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2 ||
                            nPos == POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.GD_TRAY_STG_1_Y:
                    case SVDF.AXES.GD_TRAY_STG_2_Y:
                    case SVDF.AXES.RW_TRAY_STG_Y:
                        if (nPos == POSDF.TRAY_STAGE_TRAY_WORKING_P1 ||
                            nPos == POSDF.TRAY_STAGE_TRAY_WORKING_P2)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.LD_RAIL_Y_FRONT:
                    case SVDF.AXES.LD_RAIL_Y_REAR:
                        if (nPos == POSDF.LD_RAIL_T_STRIP_LOADING ||
                            nPos == POSDF.LD_RAIL_T_STRIP_ALIGN ||
                            nPos == POSDF.LD_RAIL_T_STRIP_UNLOADING)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.BARCODE_Y:
                        if (nPos == POSDF.BOT_BARCDOE_SCAN)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.LD_VISION_X:
                        if (nPos == POSDF.LD_VISION_X_STRIP_GRIP || 
                            nPos == POSDF.LD_VISION_X_STRIP_ALIGN_1 ||
                            nPos == POSDF.LD_VISION_X_STRIP_ALIGN_2 ||
                            nPos == POSDF.LD_VISION_X_STRIP_ORIENT_CHECK ||
                            nPos == POSDF.LD_VISION_X_STRIP_BARCODE)
                        {
                            return true;
                        }
                        break;
                    case SVDF.AXES.STRIP_PK_X:
                    case SVDF.AXES.STRIP_PK_Z:
                        if (nPos == POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY || 
                            nPos == POSDF.STRIP_PICKER_STRIP_GRIP ||
                            nPos == POSDF.STRIP_PICKER_STRIP_UNGRIP||
                            nPos == POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY ||
                            nPos == POSDF.STRIP_PICKER_STRIP_PREALIGN_1 ||
                            nPos == POSDF.STRIP_PICKER_STRIP_PREALIGN_2 ||
                            nPos == POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK ||
                            nPos == POSDF.STRIP_PICKER_STRIP_2D_CODE)
                        {
                            return true;
                        }
                        break;
                    default:
                        break;
                }

            }
            catch (Exception)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 컨트롤 마다 문자입력 불가 이벤트 추가
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridRecipeMotion_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress -= new KeyPressEventHandler(gridRecipeMotion_KeyPress);
            e.Control.KeyPress += new KeyPressEventHandler(gridRecipeMotion_KeyPress);


        }
         /// <summary>
         /// 문자 입력 불가 이벤트
         /// </summary>
         /// <param name="sender"></param>
         /// <param name="e"></param>
        private void gridRecipeMotion_KeyPress(object sender, KeyPressEventArgs e)
        {
            int keyCode = (int)e.KeyChar;
            if (!(char.IsDigit(e.KeyChar)) && keyCode != 8  && keyCode != 46)
            {
                //v V ㅍ 입력되도 복사하게 변경 [2022.05.17]작성자 : 홍은표
                if (keyCode == 118 ||
                    keyCode == 86 ||
                    keyCode == 12621)  // 누른 키가 V 인경우 위의 셀 복사
                {
                    if (frmTeaching.G_Row != 0)
                    {
                        m_Grids[PageNo].Rows[frmTeaching.G_Row].Cells[G_Col].Value = m_Grids[PageNo].Rows[frmTeaching.G_Row - 1].Cells[G_Col].Value;
                        e.Handled = true;
                        return;
                    }
                }
                e.Handled = true;
            }
        }
        /// <summary>
        /// 현재 셀 row, col 정보 받아오는 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridRecipeMotion_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            frmTeaching.G_Row = e.RowIndex;
            frmTeaching.G_Col = e.ColumnIndex;
        }


        private void frmTeaching_VisibleChanged(object sender, EventArgs e)
        {
            //20211126 CHOH: 다른화면 나갔다오면 레시피 다시 표시
            if (this.Visible == true)
            {
                if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP)
                {
                    btnTchSave.Enabled = false;
                }
                else
                {
                    btnTchSave.Enabled = true;
                }
                TeachMgr.Inst.CopyTempTch();
                UpdateTchData();
                LoadGridViewItem();

                FindControl(this.Controls);
            }

            tmrRefresh.Enabled = this.Visible;

        }


        static void FindControl(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (
                    ctrl.GetType() == typeof(UC.TeachingMotPosEditButtonEx) ||
                    ctrl.GetType() == typeof(UC.TeachingMotPosGetButtonEx) ||
                    ctrl.GetType() == typeof(UC.TeachingMotPosMoveButtonEx) ||
                    ctrl.GetType() == typeof(UC.ucServoStatus) ||
                    ctrl.GetType() == typeof(UC.BaseTeachingPosControl) ||
                    ctrl.GetType() == typeof(UC.ucNumericKeyInputButtonEx) ||
                    ctrl.GetType() == typeof(UC.DataGridViewEx) ||
                    ctrl.GetType() == typeof(DataGridView) ||
                    ctrl.GetType() == typeof(TabPage) ||
                    ctrl.GetType() == typeof(NumericUpDown) ||
                    ctrl.GetType() == typeof(CheckBox) ||
                    ctrl.GetType() == typeof(Button) ||
                    ctrl.GetType() == typeof(TextBox))
                {
                    try
                    {
                        ctrl.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                        if (
                            ctrl.Name == "tabPage2" ||
                            ctrl.Name == "tabPage5" ||
                            ctrl.Name == "tabPage6" ||
                            ctrl.Name == "tabPage7" ||
                            ctrl.Name == "tabPage11" ||
                            ctrl.Name == "tabPage12" ||
                            ctrl.Name == "tabPage13" ||
                            ctrl.Name == "tabPage23" ||
                            ctrl.Name == "tabPage35" ||
                            ctrl.Name == "tabPage36" ||
                            ctrl.Name == "tabPage37" ||
                            ctrl.Name == "tabPage38" ||
                            ctrl.Name == "tabPage39" ||
                            ctrl.Name == "tabPage40" ||
                            ctrl.Name == "tabPage41" ||
                            ctrl.Name == "tabPage42" ||
                            ctrl.Name == "tabPage43" ||
                            ctrl.Name == "tabPage44" ||
                            ctrl.Name == "tabPage47" ||
                            ctrl.Name == "tabPage48" ||
                            ctrl.Name == "tabPage15"
                            )
                        {
                            ctrl.Enabled = true;
                        }
                    }
                    catch (Exception ex)
                    {

                    }

                }

                if (ctrl.Controls.Count > 0)
                    FindControl(ctrl.Controls);
            }
        }
        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            foreach (ucManualOutputButton item in m_listOuputButton)
            {
                if (item.Output >= 0 && item.Output < IODF.OUTPUT.MAX)
                {
                    item.On = GbVar.GB_OUTPUT[(int)item.Output] == 1;
                }
            }

            foreach (InputPanel item in m_listInputPanel)
            {
                if (item.Input >= 0 && item.Input < IODF.INPUT.MAX)
                {
                    item.On = GbVar.GB_INPUT[(int)item.Input] == 1;
                }
            }

#if _AJIN
            if (tabPageAxes.SelectedIndex == 5)
            {
                for (int i = 0; i < m_lbPkSvOn.Length; i++)
                {
                    if (MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i].IsAlarm())
                    {
                        if (m_lbPkSvOn[i].BackColor != Color.Red) m_lbPkSvOn[i].BackColor = Color.Red;
                    }
                    else m_lbPkSvOn[i].BackColor = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i].GetServoOnOff() ? Color.Lime : SystemColors.Control;
                }
            }

            foreach (ucServoStatus item in m_Servos)
            {

                item.RefreshUI();

            }

            m_Servos[PageNo].RefreshUI();
#endif
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
            LoadGridViewItem();
        }

        private void gridMzLoaderPosition_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            int nRow = e.RowIndex;
            int nCol = e.ColumnIndex;

            if (nRow < 0) return;
            if (nCol == 5)
            {
                // GET
                int nAxisNo = (int)m_Servos[PageNo].AXIS;
                //220516 레시피 항목
                bool bReadOnly = IsRecipeValue(nAxisNo, nRow);
                if (bReadOnly)
                {
                    popMessageBox msg = new popMessageBox("레시피항목입니다!.\r\n레시피에서 수정하십시오.","안내", MessageBoxButtons.OK);
                    msg.ShowDialog();   
                    return;
                }

                dgv.Rows[nRow].Cells[1].Value = MotionMgr.Inst[nAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 6)
            {
                // MOVE
                int nAxisNo = (int)m_Servos[PageNo].AXIS;
                double dTargetPos = 0.0;
                if (!double.TryParse(dgv.Rows[nRow].Cells[1].Value.ToString(), out dTargetPos)) return;
                double dSpeed = 0.0;
                if (!double.TryParse(dgv.Rows[nRow].Cells[2].Value.ToString(), out dSpeed)) return;
                double dAcc = 0.0;
                if (!double.TryParse(dgv.Rows[nRow].Cells[3].Value.ToString(), out dAcc)) return;
                double dDec = 0.0;
                if (!double.TryParse(dgv.Rows[nRow].Cells[4].Value.ToString(), out dDec)) return;

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

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        void item_servoHomeClicked(ucServoStatus ucSvStatus, SVDF.AXES axis)
        {
            GbSeq.manualRun.SetSingleHome(axis);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.SINGLE_HOME);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void OnLoaderTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnLoaderTeachGroup1.BackColor = SystemColors.Control;
                btnLoaderTeachGroup2.BackColor = SystemColors.Control;
                btnLoaderTeachGroup3.BackColor = SystemColors.Control;
                glassButton46.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcLoaderTeachGroup.SelectedIndex = nPgTag;
            }
        }

        private void OnStripTransferTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            label15.Text = "MGZ SLOT INDEX ( 0 - 19 )";
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnStripTransferTeachGroup1.BackColor = SystemColors.Control;
                btnStripTransferTeachGroup2.BackColor = SystemColors.Control;
                btnStripTransferTeachGroup3.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcStripTransferTeachGroup.SelectedIndex = nPgTag;
            }
        }

        private void OnUnitTransferTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnUnitTransferTeachGroup1.BackColor = SystemColors.Control;
                btnUnitTransferTeachGroup2.BackColor = SystemColors.Control;
                btnUnitTransferTeachGroup3.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcUnitTransferTeachGroup.SelectedIndex = nPgTag;
            }
        }

        private void OnDryStageTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnDryStageTeachGroup1.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcDryStageTeachGroup.SelectedIndex = nPgTag;
            }
        }


        private void OnMapTransferTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnMapTeachGroup1.BackColor = SystemColors.Control;
                btnMapTeachGroup2.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcMapTransferTeachGroup.SelectedIndex = nPgTag;
            }
        }

        private void OnPnPTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnPnPTeachGroup1.BackColor = SystemColors.Control;
                btnPnPTeachGroup2.BackColor = SystemColors.Control;
                btnPnPTeachGroup3.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcPnPTeachGroup.SelectedIndex = nPgTag;
            }
        }

        private void OnUldTrayTeachGroupChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nPgTag = 0;
            if (int.TryParse(btn.Tag as string, out nPgTag))
            {
                btnUldTrayTeachGroup1.BackColor = SystemColors.Control;
                btnUldTrayTeachGroup2.BackColor = SystemColors.Control;
                btnUldTrayTeachGroup3.BackColor = SystemColors.Control;

                btn.BackColor = Color.DarkOrange;
                tbcUldTrayTeachGroup.SelectedIndex = nPgTag;
            }
        }

        public bool FindFileName(string strName)
        {
            string pathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\WorkFile";
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(pathExeFile);

            foreach (System.IO.FileInfo f in di.GetFiles())
            {
                if (strName == f.Name)
                    return false;
            }

            return true;
        }

        private void tmrRefresh100ms_Tick(object sender, EventArgs e)
        {

        }

        private void btnPickerZRAllHome_Click(object sender, EventArgs e)
        {
            int[] axis = new int[] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_7,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_8,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                       (int)SVDF.AXES.CHIP_PK_1_T_1,
                                       (int)SVDF.AXES.CHIP_PK_1_T_2,
                                       (int)SVDF.AXES.CHIP_PK_1_T_3,
                                       (int)SVDF.AXES.CHIP_PK_1_T_4,
                                       (int)SVDF.AXES.CHIP_PK_1_T_5,
                                       (int)SVDF.AXES.CHIP_PK_1_T_6,
                                       (int)SVDF.AXES.CHIP_PK_1_T_7,
                                       (int)SVDF.AXES.CHIP_PK_1_T_8,
                                       (int)SVDF.AXES.CHIP_PK_2_T_1,
                                       (int)SVDF.AXES.CHIP_PK_2_T_2,
                                       (int)SVDF.AXES.CHIP_PK_2_T_3,
                                       (int)SVDF.AXES.CHIP_PK_2_T_4,
                                       (int)SVDF.AXES.CHIP_PK_2_T_5,
                                       (int)SVDF.AXES.CHIP_PK_2_T_6,
                                       (int)SVDF.AXES.CHIP_PK_2_T_7,
                                       (int)SVDF.AXES.CHIP_PK_2_T_8,};

            GbSeq.manualRun.SetSingleHome(axis);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.SINGLE_HOME);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        private void tmrPkSvHomeDelay_Tick(object sender, EventArgs e)
        {
            double dDelaySec = (DateTime.Now - dtPkPadDelay).TotalSeconds;
            if (dDelaySec < 8)
            {
                btnPkPadSvOn.Enabled = false;
                btnPkPadSvOff.Enabled = false;
                btnPkPadSvHome.Enabled = false;
                btnPkPadSvResetAlarm.Enabled = false;
                btnPkPadSvOn.Text = (8 - dDelaySec).ToString("0") + "초\n대기";
                btnPkPadSvOff.Text = (8 - dDelaySec).ToString("0") + "초\n대기";
                btnPkPadSvHome.Text = (8 - dDelaySec).ToString("0") + "초\n대기";
                btnPkPadSvResetAlarm.Text = (8 - dDelaySec).ToString("0") + "초\n대기";
                return;
            }

            btnPkPadSvOn.Enabled = true;
            btnPkPadSvOff.Enabled = true;
            btnPkPadSvHome.Enabled = true;
            btnPkPadSvResetAlarm.Enabled = true;
            btnPkPadSvOn.Text = "선택 축\nSV ON";
            btnPkPadSvOff.Text = "선택 축\nSV OFF";
            btnPkPadSvHome.Text = "선택 축\nHOME";
            btnPkPadSvResetAlarm.Text = "선택 축\n알람해제";

            tmrPkSvHomeDelay.Enabled = false;
        }

        private void btnTchSave_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Do you want to save Teach parameters?"), "SAVE");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            TeachMgr.Inst.SaveBackup();
            TeachMgr.Inst.SaveTempTch();
            TeachMgr.Inst.Save();

            //CopyToRecipePos();//220517 레시피 항목옴길 때 값넣는 함수

            UpdateTchData();

      
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    CloseDigPopRmsRun();
                    msg = new popMessageBox(FormTextLangMgr.FindKey("SAVE SUCCESS"), "TEACH SAVE", MessageBoxButtons.OK);
                    msg.TopMost = true;
                    msg.Show();
                });
            }
            else
            {
                CloseDigPopRmsRun();
                msg = new popMessageBox(FormTextLangMgr.FindKey("SAVE SUCCESS"), "TEACH SAVE", MessageBoxButtons.OK);
                msg.TopMost = true;
                msg.Show();
            }

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "Teaching Save");
        }

        public void UpdateTchData(bool bSaveAndValidate = false)
        {
            if (m_bIsUpdate == true) return;
            m_bIsUpdate = true;

            if (bSaveAndValidate == false)
            {
                #region TempRcp2UI

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        UpdateTeachingMotData();
                    });
                }
                else
                {
                    UpdateTeachingMotData();
                }

                #endregion
            }
            else
            {
                #region UI2TempRcp

                #endregion
            }

            m_bIsUpdate = false;
        }


        /// <summary>
        /// 220510 pjh 
        /// 최초 한번 티칭에있는 값을 레시피로 옴기기 위한 임시 함수
        /// </summary>
        public void CopyToRecipePos()
        {
            #region Inspection Pos
            //T1 Align
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_T1_LEFT_TOP_UNIT_POS] =
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1] -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_LEFT_TOP_POS] =
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);

            //T2 Align
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_T2_LEFT_TOP_UNIT_POS] =
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2] -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_LEFT_TOP_POS] =
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
            #endregion

            return;

            #region Loading Rail
            //Loading
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];


            //Centering
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];


            //Unloadnig
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

            //BarCode
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN];
            #endregion

            #region Pre align
            //Grab 1
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

            //Grab
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2];
            #endregion

            #region Strip Info
            //Orient
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK];

            //2D Code
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE];
            #endregion

            #region Pick Up
            //T1
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];

            //T2
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];
            #endregion

            #region Place
            //GD1
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];


            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


            //GD2
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];


            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


            //RW
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];


            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];
            #endregion
        }
        private void btn1fConvLf_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            GbFunc.WriteEventLog(btn.Text, btn.Text + "Moving");

            GbFunc.ManualConvOnOff(nTag);
        }

        private void cbxPk1ZAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                m_cbxPkZ[i].Checked = cbxPk1ZAll.Checked;
            }
        }
        private void cbxPk1TAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < 8; i++)
            {
                m_cbxPkT[i].Checked = cbxPk1TAll.Checked;
            }
        }

        private void cbxPk2ZAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 8; i < 16; i++)
            {
                m_cbxPkZ[i].Checked = cbxPk2ZAll.Checked;
            }
        }

        private void cbxPk2TAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 8; i < 16; i++)
            {
                m_cbxPkT[i].Checked = cbxPk2TAll.Checked;
            }
        }

        private void cbxInfiniteHome_CheckedChanged(object sender, EventArgs e)
        {
            GbVar.isInfiniteHome = cbxInfiniteHome.Checked;
        }

        #region CYCLE LOADER

        private void btnCycleStart_Loader_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartCycle_Loader(btn);
        }
        void StartCycle_Loader (GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualLdCycleRun(nSeqNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_LD_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        #endregion

        #region CYCLE STRIP TRANSFER
        void StartCycle_Saw(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualSawCycleRun(nSeqNo, (int)numLoadMzSlotNo.Value, 0, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SAW_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnCycleStart_Saw_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartCycle_Saw(btn);
        }
        #endregion

        #region DRY STAGE
        void DryStartCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnCycleStart_Dry_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            DryStartCycle(btn);
        }

        #endregion

        #region CYCLE MAP TABLE
        private void OnCycMapPickerStartBurronClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartMapPickerCycle(btn);
        }

        void StartMapPickerCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nLoadingMapTableNo = rdbCycMapVisionTableL.Checked ? 0 : 1;
            int nInspMapTableNo = radioButton10.Checked ? 0 : 1;

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingMapTableNo, nInspMapTableNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        } 
        #endregion

        private void btnSetPickUpPickerOffsetT1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, "Do you want to get picker head offset position?",
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY.ToString("F3");
        }

        private void btnSetPickUpPickerOffsetT2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY.ToString("F3");
        }

        #region Picker가 Vision을 보고 있는 위치에서 클릭 시 Pickup 작업 위치를 계산
        /// <summary>
        /// Picker가 Vision을 보고 있는 위치에서 클릭 시 Pickup 작업 위치를 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetVisionPickUpPosP1T1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.MAP_STG_1_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(0, false).x;

            //220428 pjh 
            //double dTeachPosY = dCurrPosY - ConfigMgr.Inst.GetCamToPickerOffset(0, false).y; //org
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(0, false).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Picker가 Vision을 보고 있는 위치에서 클릭 시 Pickup 작업 위치를 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetVisionPickUpPosP1T2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.MAP_STG_2_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(0, false).x;

            //220428 pjh
            //double dTeachPosY = dCurrPosY - ConfigMgr.Inst.GetCamToPickerOffset(0, false).y;//org
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(0, false).y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Picker가 Vision을 보고 있는 위치에서 클릭 시 Pickup 작업 위치를 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetVisionPickUpPosP2T1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_2_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.MAP_STG_1_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(1, false).x;

            //220428 pjh
            //double dTeachPosY = dCurrPosY - ConfigMgr.Inst.GetCamToPickerOffset(1, false).y;//org
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(1, false).y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Picker가 Vision을 보고 있는 위치에서 클릭 시 Pickup 작업 위치를 계산
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetVisionPickUpPosP2T2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_2_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.MAP_STG_2_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(1, false).x;

            //220428 pjh
            //double dTeachPosY = dCurrPosY - ConfigMgr.Inst.GetCamToPickerOffset(1, false).y; //org
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(1, false).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY.ToString("F3");
        } 
        #endregion

        #region Pickup 티칭 위치에서 Vision 위치로 이동
        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH1_T1_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[8] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_1_X,
                                         (int)SVDF.AXES.MAP_STG_1_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;

            double[] dTargetPos = new double[8] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7]};
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH2_T1_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.MAP_STG_1_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH1_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_1_X,
                                         (int)SVDF.AXES.MAP_STG_2_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).x;

            //220428 pjh 
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH2_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.MAP_STG_2_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        } 
        #endregion

        /// <summary>
        /// Config에서 설정했던 Ball Inspection 위치를 가져온다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGetBallInspPosFromCfg_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y.ToString("F3");
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1].Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].y.ToString("F3");
            }

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y.ToString("F3");
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].y.ToString("F3");
            }
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270)
            {
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].x.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].y.ToString("F3");
            }
        }

        /// <summary>
        /// BALL INSPECTION : Picker 1번 위치를 가지고 Head간 Offset을 이용해 Picker 2번 위치를 계산하고 설정한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetInspPosPkOffset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            // [2022.06.11.kmlee] Teach가 맞음
            double dP1X = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            double dP1Y = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.BALL_VISION_Y, POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_2].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Place Good 1번 Table : Picker 1번 위치에서 Picker Offset을 이용하여 Picker 2번 위치를 계산한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetPlacePkOffsetGT1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Place Good 2번 Table : Picker 1번 위치에서 Picker Offset을 이용하여 Picker 2번 위치를 계산한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSetPlacePkOffsetGT2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Place Rework Table : Picker 1번 위치에서 Picker Offset을 이용하여 Picker 2번 위치를 계산한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y - ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        /// <summary>
        /// Place Reject Box : Picker 1번 위치에서 Picker Offset을 이용하여 Picker 2번 위치를 계산한다
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN];
            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN].Text = dTeachPosX.ToString("F3");
        }

        private void btnGetVisionPlacePosP1T1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(0, true).x;

            //2200428 pjh
            //double dTeachPosY = dCurrPosY - ConfigMgr.Inst.GetCamToPickerOffset(0, true).y; //org
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(0, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text = dTeachPosY.ToString("F3");
        }

        private void btnGetVisionPlacePosP2T1_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_2_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(1, true).x;

            //220428 pjh
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(1, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        private void btnMoveToPlacePosH1_T1_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_7,
                                          (int)SVDF.AXES.CHIP_PK_1_Z_8,
                                          (int)SVDF.AXES.CHIP_PK_1_X,
                                          (int)SVDF.AXES.GD_TRAY_STG_1_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;// org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;

            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMoveToPlacePosH2_T1_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                          (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                          (int)SVDF.AXES.CHIP_PK_2_X,
                                          (int)SVDF.AXES.GD_TRAY_STG_1_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            //220428 pjh
            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).x;
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   0.0f,
                                                   dMovePosX,
                                                   dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnGetVisionPlacePosP1T2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).x;
            //220428 pjh
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text = dTeachPosY.ToString("F3");
        }

        private void btnGetVisionPlacePosP2T2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_2_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(1, true).x;

            //220428 pjh
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(1, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        private void btnGetVisionPlacePosP1T3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.RW_TRAY_STG_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(0, true).x;

            //220428 pjh
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(0, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text = dTeachPosY.ToString("F3");
        }

        private void btnGetVisionPlacePosP2T3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }
            double dCurrPosX = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_2_X].GetRealPos();
            double dCurrPosY = MotionMgr.Inst[(int)SVDF.AXES.RW_TRAY_STG_Y].GetRealPos();

            double dTeachPosX = dCurrPosX - ConfigMgr.Inst.GetCamToPickerOffset(1, true).x;

            //220428 pjh
            double dTeachPosY = dCurrPosY + ConfigMgr.Inst.GetCamToPickerOffset(1, true).y;


            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY.ToString("F3");
        }

        private void btnMoveToPlacePosH1_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_1_X,
                                         (int)SVDF.AXES.GD_TRAY_STG_2_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).x;

            //220428 pjh
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,   
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMoveToPlacePosH2_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.GD_TRAY_STG_2_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).x;

            //220428 pjh
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMoveToPlacePosH1_T3_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_1_X,
                                         (int)SVDF.AXES.RW_TRAY_STG_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).x;

            //220428 pjh
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMoveToPlacePosH2_T3_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[10] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_7,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_8,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.RW_TRAY_STG_Y};

            double dRcpPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            double dRcpPosY = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).x;
            //220428 pjh
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, true).y;


            double[] dTargetPos = new double[10] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            m_dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            m_nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            m_dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            m_dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            m_dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dDec[8], dDec[9] };

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnCycChipPnP_Click(object sender, EventArgs e)
        {
            //popPickNPlaceManualCycleRun cyc = new popPickNPlaceManualCycleRun();
            //cyc.TopMost = true;
            //if (cyc.ShowDialog(this) != DialogResult.OK)
            //{
            //    //
            //}

            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartChipPickerCycle(btn);
        }

        void StartChipPickerCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nLoadingMapTableNo = rdbCycMapVisionTableL.Checked ? 0 : 1;
            int nInspMapTableNo = radioButton10.Checked ? 0 : 1;
            int nPickerHeadNo = cmbInspHeadNo.SelectedIndex;
            int nWorkTableNo = rdbCycMapVisionTableL2.Checked ? 0 : 1;

            switch (tbcPnPTeachGroup.SelectedIndex)
            {
                case 1:
                    break;
                default:
                    // Pick Up과 Place는 Cycle을 하지 않는다..Ball Inspection만 한다
                    return;
            }

            PickerPadInfo[] unitPicker = new PickerPadInfo[CFG_DF.MAX_PICKER_PAD_CNT];

            nSeqNo = 300;
            nPickerHeadNo = cmbInspHeadNo.SelectedIndex;

            for (int nPickerCnt = 0; nPickerCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerCnt++)
            {
                unitPicker[nPickerCnt] = new PickerPadInfo();
                unitPicker[nPickerCnt].IS_PICKER_SKIP = false;
            }

            if (nPickerHeadNo < 0) return;

            GbSeq.manualRun.SetManualSorterInspectionCycleRun(nSeqNo + (nPickerHeadNo * 10), nPickerHeadNo, nWorkTableNo, unitPicker, args);
            //GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingMapTableNo, nInspMapTableNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void OnTrayTransferManualCycleButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));

            StartTrayPickerCycle(btn);
        }

        void StartTrayPickerCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nLoadingElvNo = 0, nLoadingTableNo = 0;
            int nUnloadingElvNo = 0, nUnloadingTableNo = 0;
            int nMeasureTableNo = 0;
            double dTargetPos = 0.0;
            switch (tbcUldTrayTeachGroup.SelectedIndex)
            {
                case 0:
                    if (rdbGdTrayTable1.Checked)
                    {
                        nMeasureTableNo = 0;
                        nLoadingTableNo = 0;
                        nUnloadingTableNo = 0;
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[(int)POSDF.TRAY_STAGE_TRAY_UNLOADING];
                    }
                    else if (rdbGdTrayTable2.Checked)
                    {
                        nMeasureTableNo = 1;
                        nLoadingTableNo = 1;
                        nUnloadingTableNo = 1;
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[(int)POSDF.TRAY_STAGE_TRAY_UNLOADING];
                    }

                    else if (rdbRwTrayTable.Checked)
                    {
                        nLoadingTableNo = 2;
                        nUnloadingTableNo = 2;
                    }

                    break;

                case 1:
                    if (rdbGdTrayElv1.Checked)
                    {
                        nUnloadingElvNo = 0;
                    }
                    else if (rdbGdTrayElv2.Checked)
                    {
                        nUnloadingElvNo = 1;
                    }
                    else
                    {
                        nUnloadingElvNo = 2;
                    }

                    if (rdbEmptyTrayElv1.Checked)
                    {
                        nLoadingElvNo = 0;
                    }
                    else 
                    {
                        nLoadingElvNo = 1;
                    }

                    break;

                case 2:
                    if (rdbGdInspTrayTable1.Checked)
                    {
                        nMeasureTableNo = 0;
                        nLoadingTableNo = 0;
                        nUnloadingTableNo = 0;
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[(int)POSDF.TRAY_STAGE_TRAY_UNLOADING];
                    }
                    else
                    {
                        nMeasureTableNo = 1;
                        nLoadingTableNo = 1;
                        nUnloadingTableNo = 1;
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[(int)POSDF.TRAY_STAGE_TRAY_UNLOADING];
                    }

                    break;

                default:
                    break;
            }

            if (nSeqNo != 500 && nSeqNo != 510)//스테이지로 부터 트레이 로딩, 언로딩 아닐 경우에만 220926 pjh
            { 
                nSeqNo += nMeasureTableNo * 10;
            }

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingElvNo, nLoadingTableNo, nUnloadingElvNo, nUnloadingTableNo, nMeasureTableNo, dTargetPos, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (nSeqNo == 1400 ||
               nSeqNo == 1410)
            {
                lvTrayVision.View = View.Details;
                lvTrayVision.GridLines = true;
                lvTrayVision.Items.Clear();
                lvTrayVision.BeginUpdate();

                for (int nCount = 0; nCount < GbDev.trayVision.ListMeasure.Count; nCount++)
                {
                    ListViewItem item = new ListViewItem(String.Format("#.{0}-{1}", nCount / 2 + 1, nCount % 2 + 1));
                    if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY == 0)
                    {
                        if (nCount % 2 == 1)
                        {
                            continue;
                        }
                    }
                    for (int nIdx = 0; nIdx < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX; nIdx++)
                    {
                        #region 값만 확인
                        ////item.SubItems.Add(String.Format("#.{0}",nIdx+1));
                        //item.SubItems.Add(GbDev.trayVision.ListMeasure[nCount][nIdx].ToString("F3"));
                        #endregion

                        #region 결과 표시
                        if (GbDev.trayVision.ListMeasure[nCount][nIdx] < RecipeMgr.Inst.Rcp.TrayInfo.dInspChipMissMaxValue[nMeasureTableNo])
                        {
                            item.SubItems.Add("없음 : " + GbDev.trayVision.ListMeasure[nCount][nIdx].ToString("F3"));
                        }
                        else if (GbDev.trayVision.ListMeasure[nCount][nIdx] > RecipeMgr.Inst.Rcp.TrayInfo.dInspChipFloatingMinValue[nMeasureTableNo])
                        {
                            item.SubItems.Add("들뜸 : " + GbDev.trayVision.ListMeasure[nCount][nIdx].ToString("F3"));
                        }
                        else
                        {
                            item.SubItems.Add("정상 : " + GbDev.trayVision.ListMeasure[nCount][nIdx].ToString("F3"));
                        }

                        #endregion
                        //lvTrayVision.Items.Add(GbDev.trayVision.MEASURE_CODE[nIdx].ToString("F3"));
                        if (nIdx == RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - 1)
                        {
                            lvTrayVision.Items.Add(item);
                        }
                    }

                }
                lvTrayVision.EndUpdate();
            }
        }

        private void btnCleanFirst_Click(object sender, EventArgs e)
        {

        }

        void CleaningStartCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualCleaningCycleRun(nSeqNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_CLEANER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnUnitLoadDown_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
                btn.Text));
            int nTableNo = 0;
            SawStartCycle(btn, nTableNo);
        }

        void SawStartCycle(GlassButton btn, params object[] args)
        {
            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualSawCycleRun(nSeqNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SAW_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //
            popLoaderSeqTest loaderSeq = new popLoaderSeqTest();
            //loaderSeq.TopMost = true;
            loaderSeq.ShowDialog();
        }
        private void btnTrayOutCycle_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nElvNo = 0;

            if (rdbGdTrayElv1.Checked)
            {
                nElvNo = 0;
            }
            else if (rdbGdTrayElv2.Checked)
            {
                nElvNo = 1;
            }
            else if (rdbRwTrayElv.Checked)
            {
                nElvNo = 2;
            }

            nSeqNo +=  nElvNo * 10;

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nElvNo);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnTrayEmptyOutCycle_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nElvNo = 3;

             if (rdbEmptyTrayElv1.Checked)//220608
            {
                nElvNo = 3;
            }
            else if (rdbEmptyTrayElv2.Checked)
            {
                nElvNo = 4;
            }

            nSeqNo += nElvNo * 10;

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nElvNo);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void brnTrayInCycle_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            int nSeqNo = btn.GetTag();
            if (nSeqNo < 0) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nElvNo = 0;

            if (rdbEmptyTrayElv1.Checked)
            {
                nElvNo = 3;
            }
            else if (rdbEmptyTrayElv2.Checked)
            {
                nElvNo = 4;
            }

            nSeqNo += nElvNo * 10;

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nElvNo);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Top Align을 하고 난 후 얼라인 정보를 적용한 Map Inspection 위치를 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveMapInspPosApplyTopAlign_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;

            popMessageBox msg = new popMessageBox("얼라인 정보가 적용된 맵 검사 위치로 이동하시겠습니까?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int nInspMapTableNo = radioButton10.Checked ? 0 : 1;

            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[4];
            dPosArray = new double[4];
            nOrderArray = new uint[4];
            dSpeedArray = new double[4];
            dAccArray = new double[4];
            dDecArray = new double[4];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_Z;
            nAxisArray[1] = (int)SVDF.AXES.MAP_VISION_Z;
            nAxisArray[2] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[3] = (int)SVDF.AXES.MAP_STG_1_Y + nInspMapTableNo;

            dPosArray[0] = 0;
            dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_VISION_FOCUS_T1 + nInspMapTableNo];
            //
            //dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nInspMapTableNo)];
            //dPosArray[3] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[3]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];

            int nLoadingMapTableNo = radioButton10.Checked ? 0 : 1;
       
            if (nLoadingMapTableNo == 0)
            {
                dPosArray[2] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);
                dPosArray[3] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);

            }
            else
            {
                dPosArray[2] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);
                dPosArray[3] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
            }


            // Top Align 보정 값
            if (!GbVar.Seq.sMapVisionTable[nInspMapTableNo].Info.bTopAlignCorrectResult)
            {
                msg = new popMessageBox("맵 테이블 얼라인을 성공한 후 다시 시도해주세요.", "경고", MessageBoxButtons.OK);
                msg.ShowDialog(this);
                return;
            }

            dPosArray[2] += GbVar.Seq.sMapVisionTable[nInspMapTableNo].Info.xyTopAlignCorrectOffset.x;            
            dPosArray[3] += GbVar.Seq.sMapVisionTable[nInspMapTableNo].Info.xyTopAlignCorrectOffset.y;

            nOrderArray[0] = 0;
            nOrderArray[1] = 0;
            nOrderArray[2] = 1;
            nOrderArray[3] = 1;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[0];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[0];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[0];
            }

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnMoveAlignMarkCenter_Click(object sender, EventArgs e)
        {
            int nInspMapTableNo = radioButton10.Checked ? 0 : 1;

            popMessageBox msg = new popMessageBox("얼라인 마크를 화면 센터로 이동하시겠습니까?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualMoveCenterTopAlign(nInspMapTableNo);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_MOVE_CENTER_TOP_ALIGN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnApplyPickUpTableOffset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dP2X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dP2Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];


            double dTeachPosX1 = dP1X - ConfigMgr.Inst.GetMapStageToStageOffset().x;
            double dTeachPosY1 = dP1Y - ConfigMgr.Inst.GetMapStageToStageOffset().y;

            double dTeachPosX2 = dP2X - ConfigMgr.Inst.GetMapStageToStageOffset().x;
            double dTeachPosY2 = dP2Y - ConfigMgr.Inst.GetMapStageToStageOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX1.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1].Text = dTeachPosY1.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX2.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY2.ToString("F3");
        }

        private void btnPlaceTableOffsetApply_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            double dP1X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dP1Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dP2X = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dP2Y = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            DionesTool.Objects.dxy[] dpTableOffset = ConfigMgr.Inst.GetTrayTableToTableOffset();

            double dTeachPosX1;
            double dTeachPosY1;

            double dTeachPosX2;
            double dTeachPosY2;

            if (tbcPlaceTable.SelectedIndex == 0)
            {
                dTeachPosX1 = dP1X - dpTableOffset[0].x;
                dTeachPosY1 = dP1Y - dpTableOffset[0].y;

                dTeachPosX2 = dP2X - dpTableOffset[0].x;
                dTeachPosY2 = dP2Y - dpTableOffset[0].y;

                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text = dTeachPosX1.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text = dTeachPosY1.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text = dTeachPosX2.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY2.ToString("F3");
            }
            else if (tbcPlaceTable.SelectedIndex == 1)
            {
                dTeachPosX1 = dP1X - dpTableOffset[1].x;
                dTeachPosY1 = dP1Y - dpTableOffset[1].y;

                dTeachPosX2 = dP2X - dpTableOffset[1].x;
                dTeachPosY2 = dP2Y - dpTableOffset[1].y;

                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text = dTeachPosX1.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text = dTeachPosY1.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text = dTeachPosX2.ToString("F3");
                m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P2].Text = dTeachPosY2.ToString("F3");
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void SignalSearchClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nAxisOffset = 0;
            int nAxisNo = 0;

            if (int.TryParse(btn.Tag.ToString(), out nAxisOffset))
            {
                nAxisNo = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nAxisOffset;

                string strTitle = "AXIS MOVE";
                string strMsg = string.Format("DO YOU WANT TO {0} AXIS SIGNAL SEARCH? ", SVDF.GetAxisName(nAxisNo));
                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                // 이미 시그널 감지상태이면 무브 하지 못하도록 처리함
                uint nBit = 0;
                if (CAXM.AxmSignalReadInputBit(nAxisNo, 2, ref nBit) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    // 시그널 읽기 실패시 알람
                    //return mInfo[i].nMoveErrNo;
                    return;
                }
                else
                {
                    // 이미 시그널 감지상태면 SUCCESS
                    if (nBit == 1) return;
                }

                MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + nAxisOffset].MoveSignalSearch();
            }
        }

        private void btnMotionMovingStop_Click(object sender, EventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            MotionMgr.Inst.AllStop();
        }


        private void btnMOVEChipPnPCam_Click(object sender, EventArgs e)
        {
            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int _nHeadNo; 
            int _nPickerNo; 
            int _nPickerColCount; 
            int _nTableNo;
            int _nTableCount;


             _nTableNo = rbtnUnitPick_Stage_L.Checked ? 0 : 1;
            _nHeadNo = rbtnUnitPick_Picker_1.Checked ? 0 : 1;


            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (int.TryParse(tbxUnitPick_COL.Text, out nCol) == false)
                return;

            if (int.TryParse(tbxUnitPick_ROW.Text, out nRow) == false)
                return;

            if (int.TryParse(tbxUnitPick_PAD.Text, out _nPickerNo) == false)
                return;

            if (0 < _nPickerNo && _nPickerNo <= 8)
            {//0~7
                nCol = nCol - 1;
                nRow = nRow - 1;

                _nPickerColCount = nCol;
                _nTableCount = (nTotMapCountX * nCol) + nRow + nCol;

                GbSeq.manualRun.SetMove_ChipPickerCamPOS(_nHeadNo, _nPickerNo-1, _nPickerColCount, _nTableNo, _nTableCount);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        private void btnCycChipPnP_Click_1(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",  btn.Text));

            int nSeqNo = 260;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            int _nHeadNo;
            int _nPickerNo;
            int _nPickerColCount;
            int _nTableNo;
            int _nTableCount;


            _nTableNo = rbtnUnitPick_Stage_L.Checked ? 0 : 1;
            _nHeadNo = rbtnUnitPick_Picker_1.Checked ? 0 : 1;


            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (int.TryParse(tbxUnitPick_COL.Text, out nCol) == false)
                return;

            if (int.TryParse(tbxUnitPick_ROW.Text, out nRow) == false)
                return;

            if (int.TryParse(tbxUnitPick_PAD.Text, out _nPickerNo) == false)
                return;

            if (0 < _nPickerNo && _nPickerNo <= 8)
            {
                nCol = nCol - 1;
                nRow = nRow - 1;

                _nPickerColCount = nCol;
                _nTableCount = (nTotMapCountX * nCol) + nRow + nCol;

                GbSeq.manualRun.SetManualSorterInspectionCycleRun(nSeqNo + (_nHeadNo * 10), _nHeadNo, _nPickerNo-1, _nPickerColCount, _nTableNo, _nTableCount);
                //GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingMapTableNo, nInspMapTableNo, args);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        private void btnMOVEChipPlace_Click(object sender, EventArgs e)
        {
            string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Tray Place 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int _nHeadNo;
            int _nPickerNo;
            int _nTableNo = 0;
            int _nTableCount;


            if (rbtnUnitPlace_Gd1.Checked)
                _nTableNo = 0;
            else if (rbtnUnitPlace_Gd2.Checked)
                _nTableNo = 1;
            else if (rbtnUnitPlace_Reject.Checked)
                _nTableNo = 0;

            _nHeadNo = rbtnUnitPlace_Picker_1.Checked ? 0 : 1;

            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY;

            if (int.TryParse(tbxUnitPlace_COL.Text, out nCol) == false)
                return;

            if (int.TryParse(tbxUnitPlace_ROW.Text, out nRow) == false)
                return;

            if (int.TryParse(tbxUnitPlace_PAD.Text, out _nPickerNo) == false)
                return;

            if (0 < _nPickerNo && _nPickerNo <= 8)
            {//1~8
                nCol = nCol - 1;
                nRow = nRow - 1;
                _nTableCount = (nTotMapCountX * nRow) + nCol;

                if (rbtnUnitPlace_Reject.Checked)
                    GbSeq.manualRun.SetMove_ChipRejectPlacePOS(_nHeadNo, _nPickerNo - 1, _nTableCount);
                else
                    GbSeq.manualRun.SetMove_ChipGoodPlacePOS(_nHeadNo, _nPickerNo - 1, _nTableNo, _nTableCount);

                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        private void btnMOVEChipPickMove_Click(object sender, EventArgs e)
        {
             string strTitle = "축 이동";
            string strMsg = FormTextLangMgr.FindKey("Chip Pick 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int _nHeadNo; 
            int _nPickerNo; 
            int _nPickerColCount; 
            int _nTableNo;
            int _nTableCount;


             _nTableNo = rbtnUnitPick_Stage_L.Checked ? 0 : 1;
            _nHeadNo = rbtnUnitPick_Picker_1.Checked ? 0 : 1;


            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (int.TryParse(tbxUnitPick_COL.Text, out nCol) == false)
                return;

            if (int.TryParse(tbxUnitPick_ROW.Text, out nRow) == false)
                return;

            if (int.TryParse(tbxUnitPick_PAD.Text, out _nPickerNo) == false)
                return;

            if (0 < _nPickerNo && _nPickerNo <= 8)
            {//1~8
                nCol = nCol - 1;
                nRow = nRow - 1;

                _nPickerColCount = nCol;
                _nTableCount = (nTotMapCountX * nRow)  + nCol;

                GbSeq.manualRun.SetMove_ChipPickPOS(_nHeadNo, _nPickerNo-1, _nPickerColCount, _nTableNo, _nTableCount);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }
    }
}
