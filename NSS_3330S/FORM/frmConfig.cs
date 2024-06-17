using System;
using System.Collections.Generic;
using System.Drawing;

using System.Windows.Forms;
using System.Reflection;
using NSS_3330S.POP;
using System.Drawing.Design;
using NSS_3330S.UC;
using NSS_3330S.UC.CONFIG;
using Glass;
using DionesTool.UTIL;
using NSS_3330S.MOTION;
using Microsoft.VisualBasic;

namespace NSS_3330S.FORM
{
    public partial class frmConfig : BaseCfgPosControl
    {
        public delegate void deleSaveCfgEvent();
        public event deleSaveCfgEvent deleSaveCfg;

        public delegate void langEvnet();
        public event langEvnet ChangeFormLang;

        List<CfgMotPosEditButtonEx> m_listMotPosBtn = new List<CfgMotPosEditButtonEx>();
        List<CfgMotPosGetButtonEx> m_listMotPosGet = new List<CfgMotPosGetButtonEx>();
        List<CfgMotPosMoveButtonEx> m_listMotPosMove = new List<CfgMotPosMoveButtonEx>();

        private List<int> m_dgvCommonItemIdx;
        private List<int> m_dgvMachineItemIdx;
        private List<int> m_dgvLoaderItemIdx;
        private List<int> m_dgvSawItemIdx;
        private List<int> m_dgvSorterItemIdx;
        private List<int> m_dgvVisionItemIdx;
        private List<int> m_dgvCleanerItemIdx;

        private int[] m_nGridLoadingAxisNo;
        private int[] m_nGridPicker1AxisNo;
        private int[] m_nGridPicker2AxisNo;
        private int[] m_nGridSorterAxisNo;

        popVisionManual popVisionManual = new popVisionManual();

        public frmConfig()
        {
            InitializeComponent();
            LinkRecipeControl();
            ShowOptionData();
            SetMotionGridAxisNo();

            // [2022.05.10.kmlee] 요청이 있을 때까지 임시로 막음
            // [2022.06.03 kmlee] 270도를 -90도로 해달라는 요청이 있어서
            btnHeadOffset1PickerN90.Visible = true;
            btnHeadOffset1PickerN180.Visible = true;
            btnHeadOffset1PickerN270.Visible = true;
            btnHeadOffset2PickerN90.Visible = true;
            btnHeadOffset2PickerN180.Visible = true;
            btnHeadOffset2PickerN270.Visible = true;

            cbPickerCalAngleHead1.Items.Clear();
            cbPickerCalAngleHead1.Items.Add("0º");
            cbPickerCalAngleHead1.Items.Add("90º");
            cbPickerCalAngleHead1.Items.Add("180º");
            cbPickerCalAngleHead1.Items.Add("270º");
            cbPickerCalAngleHead1.Items.Add("- 90º");
            cbPickerCalAngleHead1.Items.Add("- 180º");
            cbPickerCalAngleHead1.Items.Add("- 270º");


            cbPickerCalAngleHead2.Items.Clear();
            cbPickerCalAngleHead2.Items.Add("0º");
            cbPickerCalAngleHead2.Items.Add("90º");
            cbPickerCalAngleHead2.Items.Add("180º");
            cbPickerCalAngleHead2.Items.Add("270º");
            cbPickerCalAngleHead2.Items.Add("- 90º");
            cbPickerCalAngleHead2.Items.Add("- 180º");
            cbPickerCalAngleHead2.Items.Add("- 270º");

            //// 상부 맵 테이블은 캘리브레이션 할 필요가 없다
            //radioButton2.Visible = false;
            //radioButton1.Top = radioButton2.Top;
            //tbcStageOffset.SelectedIndex = 1;

            if (cbPickerCalAngleHead1.Items.Count > 0)
                cbPickerCalAngleHead1.SelectedIndex = 0;
            if (cbPickerCalAngleHead2.Items.Count > 0)
                cbPickerCalAngleHead2.SelectedIndex = 0;
        }

        private void SetMotionGridAxisNo()
        {
            m_nGridLoadingAxisNo = new int[]
            {
                (int)SVDF.AXES.MAGAZINE_ELV_Z,
                (int)SVDF.AXES.LD_RAIL_T,
                (int)SVDF.AXES.LD_RAIL_Y_FRONT,
                (int)SVDF.AXES.LD_RAIL_Y_REAR,
                (int)SVDF.AXES.BARCODE_Y,
                (int)SVDF.AXES.LD_VISION_X,
                (int)SVDF.AXES.STRIP_PK_X,
                (int)SVDF.AXES.STRIP_PK_Z,
                (int)SVDF.AXES.UNIT_PK_X,
                (int)SVDF.AXES.UNIT_PK_Z,
            };

            m_nGridPicker1AxisNo = new int[]
            {
                (int)SVDF.AXES.CHIP_PK_1_Z_1,
                (int)SVDF.AXES.CHIP_PK_1_T_1,
                (int)SVDF.AXES.CHIP_PK_1_Z_2,
                (int)SVDF.AXES.CHIP_PK_1_T_2,
                (int)SVDF.AXES.CHIP_PK_1_Z_3,
                (int)SVDF.AXES.CHIP_PK_1_T_3,
                (int)SVDF.AXES.CHIP_PK_1_Z_4,
                (int)SVDF.AXES.CHIP_PK_1_T_4,
                (int)SVDF.AXES.CHIP_PK_1_Z_5,
                (int)SVDF.AXES.CHIP_PK_1_T_5,
                (int)SVDF.AXES.CHIP_PK_1_Z_6,
                (int)SVDF.AXES.CHIP_PK_1_T_6,
                (int)SVDF.AXES.CHIP_PK_1_Z_7,
                (int)SVDF.AXES.CHIP_PK_1_T_7,
                (int)SVDF.AXES.CHIP_PK_1_Z_8,
                (int)SVDF.AXES.CHIP_PK_1_T_8,
            };

            m_nGridPicker2AxisNo = new int[]
            {
                (int)SVDF.AXES.CHIP_PK_2_Z_1,
                (int)SVDF.AXES.CHIP_PK_2_T_1,
                (int)SVDF.AXES.CHIP_PK_2_Z_2,
                (int)SVDF.AXES.CHIP_PK_2_T_2,
                (int)SVDF.AXES.CHIP_PK_2_Z_3,
                (int)SVDF.AXES.CHIP_PK_2_T_3,
                (int)SVDF.AXES.CHIP_PK_2_Z_4,
                (int)SVDF.AXES.CHIP_PK_2_T_4,
                (int)SVDF.AXES.CHIP_PK_2_Z_5,
                (int)SVDF.AXES.CHIP_PK_2_T_5,
                (int)SVDF.AXES.CHIP_PK_2_Z_6,
                (int)SVDF.AXES.CHIP_PK_2_T_6,
                (int)SVDF.AXES.CHIP_PK_2_Z_7,
                (int)SVDF.AXES.CHIP_PK_2_T_7,
                (int)SVDF.AXES.CHIP_PK_2_Z_8,
                (int)SVDF.AXES.CHIP_PK_2_T_8,
            };

            m_nGridSorterAxisNo = new int[]
            {
                (int)SVDF.AXES.DRY_BLOCK_STG_X,
                (int)SVDF.AXES.MAP_PK_X,
                (int)SVDF.AXES.MAP_PK_Z,
                (int)SVDF.AXES.MAP_VISION_Z,
                (int)SVDF.AXES.MAP_STG_1_Y,
                (int)SVDF.AXES.MAP_STG_2_Y,
                (int)SVDF.AXES.MAP_STG_1_T,
                (int)SVDF.AXES.MAP_STG_2_T,
                (int)SVDF.AXES.CHIP_PK_1_X,
                (int)SVDF.AXES.CHIP_PK_2_X,
                (int)SVDF.AXES.BALL_VISION_Y,
                (int)SVDF.AXES.BALL_VISION_Z,
                (int)SVDF.AXES.TRAY_PK_X,
                (int)SVDF.AXES.TRAY_PK_Z,
                (int)SVDF.AXES.TRAY_PK_Y,
                (int)SVDF.AXES.GD_TRAY_1_ELV_Z,                
                (int)SVDF.AXES.GD_TRAY_2_ELV_Z,
                (int)SVDF.AXES.RW_TRAY_ELV_Z,
                (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z,
                (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z,
                (int)SVDF.AXES.GD_TRAY_STG_1_Y,
                (int)SVDF.AXES.GD_TRAY_STG_2_Y,
                (int)SVDF.AXES.RW_TRAY_STG_Y,
            };
        }

        private void frmConfig_Load(object sender, EventArgs e)
        {
            // 탭컨트롤 아이템 사이즈 줄이기
            foreach (TabControl item in new TabControl[] { tabOperating, tbcPickerOffset, tbcStageOffset, tabMotionGrid, tabPnpOffsetHeads, tabPnpPkOffsetHeads, tabDegree1, tabDegree2 , tabXyztOffsetHeads })
            {
                item.ItemSize = new Size(0, 1);
                item.Location = new Point(item.Location.X, item.Location.Y - item.ItemSize.Height);
            }

            UpdateMotionGridData();
            InitControl();
            UpdateCfgData();

            UpdatePickerHeadOffsetGridData();

            GbFunc.SetDoubleBuffered(gridMotorLoading);
            gridMotorLoading.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridMotorLoading.CellValueChanged += gridMotor_CellValueChanged;

            GbFunc.SetDoubleBuffered(gridMotorPicker1);
            gridMotorPicker1.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridMotorPicker1.CellValueChanged += gridMotor_CellValueChanged;

            GbFunc.SetDoubleBuffered(gridMotorPicker2);
            gridMotorPicker2.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridMotorPicker2.CellValueChanged += gridMotor_CellValueChanged;

            GbFunc.SetDoubleBuffered(gridMotorSorter);
            gridMotorSorter.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridMotorSorter.CellValueChanged += gridMotor_CellValueChanged;

            ////////////////////////////////////////////////////
#region 피커 패드 간 거리 1번 헤드
            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1deg0);
            gridPickerHeadOffset1deg0.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1deg0.CellValueChanged += gridPickerHeadOffset1_CellValueChanged;
            gridPickerHeadOffset1deg0.CellClick += gridPickerHeadOffset_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1deg90);
            gridPickerHeadOffset1deg90.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1deg90.CellValueChanged += gridPickerHeadOffsetDeg90_CellValueChanged;
            gridPickerHeadOffset1deg90.CellClick += gridPickerHeadOffsetDeg90_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1deg180);
            gridPickerHeadOffset1deg180.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1deg180.CellValueChanged += gridPickerHeadOffsetDeg180_CellValueChanged;
            gridPickerHeadOffset1deg180.CellClick += gridPickerHeadOffsetDeg180_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1deg270);
            gridPickerHeadOffset1deg270.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1deg270.CellValueChanged += gridPickerHeadOffsetDeg270_CellValueChanged;
            gridPickerHeadOffset1deg270.CellClick += gridPickerHeadOffsetDeg270_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1degN90);
            gridPickerHeadOffset1degN90.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1degN90.CellValueChanged += gridPickerHeadOffsetDegN90_CellValueChanged;
            gridPickerHeadOffset1degN90.CellClick += gridPickerHeadOffsetDegN90_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1degN180);
            gridPickerHeadOffset1degN180.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1degN180.CellValueChanged += gridPickerHeadOffsetDegN180_CellValueChanged;
            gridPickerHeadOffset1degN180.CellClick += gridPickerHeadOffsetDegN180_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset1degN270);
            gridPickerHeadOffset1degN270.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset1degN270.CellValueChanged += gridPickerHeadOffsetDegN270_CellValueChanged;
            gridPickerHeadOffset1degN270.CellClick += gridPickerHeadOffsetDegN270_CellClick;
#endregion
            ////////////////////////////////////////////////////

            ////////////////////////////////////////////////////
#region 피커 패드 간 거리 2번 헤드
            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2deg0);
            gridPickerHeadOffset2deg0.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2deg0.CellValueChanged += gridPickerHeadOffset1_CellValueChanged;
            gridPickerHeadOffset2deg0.CellClick += gridPickerHeadOffset_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2deg90);
            gridPickerHeadOffset2deg90.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2deg90.CellValueChanged += gridPickerHeadOffsetDeg90_CellValueChanged;
            gridPickerHeadOffset2deg90.CellClick += gridPickerHeadOffsetDeg90_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2deg180);
            gridPickerHeadOffset2deg180.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2deg180.CellValueChanged += gridPickerHeadOffsetDeg180_CellValueChanged;
            gridPickerHeadOffset2deg180.CellClick += gridPickerHeadOffsetDeg180_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2deg270);
            gridPickerHeadOffset2deg270.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2deg270.CellValueChanged += gridPickerHeadOffsetDeg270_CellValueChanged;
            gridPickerHeadOffset2deg270.CellClick += gridPickerHeadOffsetDeg270_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2degN90);
            gridPickerHeadOffset2degN90.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2degN90.CellValueChanged += gridPickerHeadOffsetDegN90_CellValueChanged;
            gridPickerHeadOffset2degN90.CellClick += gridPickerHeadOffsetDegN90_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2degN180);
            gridPickerHeadOffset2degN180.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2degN180.CellValueChanged += gridPickerHeadOffsetDegN180_CellValueChanged;
            gridPickerHeadOffset2degN180.CellClick += gridPickerHeadOffsetDegN180_CellClick;

            GbFunc.SetDoubleBuffered(gridPickerHeadOffset2degN270);
            gridPickerHeadOffset2degN270.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPickerHeadOffset2degN270.CellValueChanged += gridPickerHeadOffsetDegN270_CellValueChanged;
            gridPickerHeadOffset2degN270.CellClick += gridPickerHeadOffsetDegN270_CellClick;
#endregion
            ////////////////////////////////////////////////////

            GbFunc.SetDoubleBuffered(gridPnpOffsetHead1);
            gridPnpOffsetHead1.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPnpOffsetHead1.CellValueChanged += gridPnpOffsetHead1_CellValueChanged;

            GbFunc.SetDoubleBuffered(gridPnpOffsetHead2);
            gridPnpOffsetHead2.EditingControlShowing += new DataGridViewEditingControlShowingEventHandler(dataGridView_EditingControlShowing);
            gridPnpOffsetHead2.CellValueChanged += gridPnpOffsetHead2_CellValueChanged;

            DataGridView[] tmpDgv = new DataGridView[] { dgvCommon, dgvMachine, dgvLoader, dgvSaw, dgvSorter };
            for (int i = 0; i < tmpDgv.Length; i++)
            {
                GbFunc.SetDoubleBuffered(tmpDgv[i]);
                tmpDgv[i].CellClick += dgvOption_CellClick;
                tmpDgv[i].CellPainting += dgvOption_CellPainting;
                tmpDgv[i].CellValueChanged += dgvOption_CellValueChanged;
            }
        }

        private void rdbOperating1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tabOperating.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tabOperating.SelectedIndex = nTag;
            }
        }

        #region MOTION GRID
        private void dataGridView_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyPress += new KeyPressEventHandler(TypeDouble_KeyPress);
        }

        private void TypeDouble_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!GbFunc.IsKeyInDouble(sender, e))
            {
                e.Handled = true;
                return;
            }
        }
        public void UpdateMotionGridData()
        {
            int nCol = 0;
            int nAxisNo = 0;

            //LOADER
            gridMotorLoading.RowCount = m_nGridLoadingAxisNo.Length;
            for (int i = 0; i < gridMotorLoading.RowCount; i++)
            {
                gridMotorLoading.Rows[i].Height = 34;
                nAxisNo = m_nGridLoadingAxisNo[i];
                nCol = 0;

                gridMotorLoading.Rows[i].Cells[nCol++].Value = (nAxisNo + 1).ToString();
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].strAxisName;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dVel;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dAcc;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dDec;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dMaxVel;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dHomeOffset;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                gridMotorLoading.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dInpositionBand;
            }
            //PICKER1
            gridMotorPicker1.RowCount = m_nGridPicker1AxisNo.Length;
            for (int i = 0; i < gridMotorPicker1.RowCount; i++)
            {
                gridMotorPicker1.Rows[i].Height = 34;
                nAxisNo = m_nGridPicker1AxisNo[i];
                nCol = 0;

                gridMotorPicker1.Rows[i].Cells[nCol++].Value = (nAxisNo + 1).ToString();
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].strAxisName;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dVel;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dAcc;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dDec;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dMaxVel;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dHomeOffset;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                gridMotorPicker1.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dInpositionBand;
            }
            //PICKER2
            gridMotorPicker2.RowCount = m_nGridPicker2AxisNo.Length;
            for (int i = 0; i < gridMotorPicker2.RowCount; i++)
            {
                gridMotorPicker2.Rows[i].Height = 34;
                nAxisNo = m_nGridPicker2AxisNo[i];
                nCol = 0;

                gridMotorPicker2.Rows[i].Cells[nCol++].Value = (nAxisNo + 1).ToString();
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].strAxisName;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dVel;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dAcc;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dDec;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dMaxVel;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dHomeOffset;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                gridMotorPicker2.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dInpositionBand;
            }
            //SORTER
            gridMotorSorter.RowCount = m_nGridSorterAxisNo.Length;
            for (int i = 0; i < gridMotorSorter.RowCount; i++)
            {
                gridMotorSorter.Rows[i].Height = 34;
                nAxisNo = m_nGridSorterAxisNo[i];
                nCol = 0;

                gridMotorSorter.Rows[i].Cells[nCol++].Value = (nAxisNo + 1).ToString();
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].strAxisName;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dVel;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dAcc;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dDec;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dMaxVel;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dHomeOffset;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                gridMotorSorter.Rows[i].Cells[nCol++].Value = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dInpositionBand;
            }
        }

        private void gridMotor_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //Y
            int nCol = dgv.CurrentCell.ColumnIndex;   //X
            if (nCol < 2) return;
            int nCnt = 0;
            int nAxisNo = Convert.ToInt16(dgv.Rows[nRow].Cells[0].Value) - 1;

            nCnt = 2;
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dSwLimitN = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dSwLimitP = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dVel = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dAcc = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dDec = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dMaxVel = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dHomeOffset = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dJogLowVel = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dJogMdlVel = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dJogHighVel = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
            ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dInpositionBand = Convert.ToDouble(dgv.Rows[nRow].Cells[nCnt++].Value);
        }

        private void rdbMotion1_CheckedChanged(object sender, EventArgs e)
        {
            // 모션 축 정보 선택
            RadioButton rdb = sender as RadioButton;
            int nTag = Convert.ToInt16(rdb.Tag.ToString());
            tabMotionGrid.SelectedIndex = nTag;
        }
        #endregion

        public void InitControl()
        {

        }

        public void UpdateCfgMotData(bool bSaveAndValidate = false)
        {
            m_bIsUpdate = true;

            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn != null && m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Text = ConfigMgr.Inst.Cfg.dMotPos[nCntSv].dPos[nCntPos].ToString("F3");
                    }
                }
            }

            m_bIsUpdate = false;
        }

        public void UpdateCfgData(bool bSaveAndValidate = false)
        {
            if (m_bIsUpdate == true) return;
            m_bIsUpdate = true;
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    if (bSaveAndValidate == false)
                    {
                        //ConfigMgr.Inst.Init();
                        ConfigMgr.Inst.CopyTempCfg();

                        UpdateCfgMotData();
                        UpdateMotionGridData();
                        UpdatePickerHeadOffsetGridData();
                        ShowOptionData();

                        edtCnPOffsetX1.Text = ConfigMgr.Inst.Cfg.OffsetCnP[0].dDefPickerOffsetX.ToString("F3");
                        edtCnPOffsetY1.Text = ConfigMgr.Inst.Cfg.OffsetCnP[0].dDefPickerOffsetY.ToString("F3");
                        edtCnPOffsetX2.Text = ConfigMgr.Inst.Cfg.OffsetCnP[1].dDefPickerOffsetX.ToString("F3");
                        edtCnPOffsetY2.Text = ConfigMgr.Inst.Cfg.OffsetCnP[1].dDefPickerOffsetY.ToString("F3");

                        edtPnPDefOffsetX1.Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dDefPickerPitchX.ToString("F3");
                        edtPnPDefOffsetX2.Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dDefPickerPitchX.ToString("F3");

                        numPickerCalRetryCount.Value = ConfigMgr.Inst.Cfg.General.nPickerCalRetryCount;
                        edtPickerTolerence.Text = ConfigMgr.Inst.Cfg.General.dPickerCalTolerence.ToString("F3");

                        edtZerosetStartHead1.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead1Z.ToString("F3");//221011 제로셋 Z위치 및 Down Step 저장
                        edtZerosetStartHead2.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead2Z.ToString("F3");

                        edtZerosetStepHead1.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead1Z.ToString("F3");
                        edtZerosetStepHead2.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead2Z.ToString("F3");

                        edtPlaceZeroSetStartHead1.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead1Z.ToString("F3");//221011 제로셋 Z위치 및 Down Step 저장
                        edtPlaceZeroSetStartHead2.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead2Z.ToString("F3");

                        edtPlaceZeroSetStepHead1.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead1Z.ToString("F3");
                        edtPlaceZeroSetStepHead2.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead2Z.ToString("F3");

                        switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                        {
                            case Language.ENGLISH:
                                rdbLanguageEng.Checked = true;
                                rdbLanguageChn.Checked = false;
                                rdbLanguageKor.Checked = false;
                                break;
                            case Language.CHINA:
                                rdbLanguageEng.Checked = false;
                                rdbLanguageChn.Checked = true;
                                rdbLanguageKor.Checked = false;
                                break;
                            case Language.KOREAN:
                                rdbLanguageEng.Checked = false;
                                rdbLanguageChn.Checked = false;
                                rdbLanguageKor.Checked = true;
                                break;
                        }
                    }
                    else
                    {
                        ConfigMgr.Inst.TempCfg.OffsetCnP[0].dDefPickerOffsetX = edtCnPOffsetX1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[0].dDefPickerOffsetY = edtCnPOffsetY1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[1].dDefPickerOffsetX = edtCnPOffsetX2.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[1].dDefPickerOffsetY = edtCnPOffsetY2.ToDouble();

                        ConfigMgr.Inst.TempCfg.OffsetPnP[0].dDefPickerPitchX = edtPnPDefOffsetX1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetPnP[1].dDefPickerPitchX = edtPnPDefOffsetX2.ToDouble();

                        ConfigMgr.Inst.TempCfg.General.nPickerCalRetryCount = (int)numPickerCalRetryCount.Value;
                        ConfigMgr.Inst.TempCfg.General.dPickerCalTolerence = edtPickerTolerence.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStartHead1Z = edtZerosetStartHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStartHead2Z = edtZerosetStartHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStepHead1Z = edtZerosetStepHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStepHead2Z = edtZerosetStepHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStartHead1Z = edtPlaceZeroSetStartHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStartHead2Z = edtPlaceZeroSetStartHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStepHead1Z = edtPlaceZeroSetStepHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStepHead2Z = edtPlaceZeroSetStepHead2.ToDouble();
                    }

                    foreach (CfgMotPosEditButtonEx item in m_listMotPosBtn)
                    {
                        item.Text = ConfigMgr.Inst.Cfg.dMotPos[(int)item.Axis].dPos[item.PosNo].ToString("F3");
                    }
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    if (bSaveAndValidate == false)
                    {
                        //Debug.WriteLine(String.Format("start : {0}", DateTime.Now));
                        //ConfigMgr.Inst.Init();
                        //Debug.WriteLine(String.Format("  end : {0}", DateTime.Now));
                        ConfigMgr.Inst.CopyTempCfg();
                        UpdateCfgMotData();
                        UpdateMotionGridData();
                        UpdatePickerHeadOffsetGridData();
                        ShowOptionData();

                        edtCnPOffsetX1.Text = ConfigMgr.Inst.Cfg.OffsetCnP[0].dDefPickerOffsetX.ToString("F3");
                        edtCnPOffsetY1.Text = ConfigMgr.Inst.Cfg.OffsetCnP[0].dDefPickerOffsetY.ToString("F3");
                        edtCnPOffsetX2.Text = ConfigMgr.Inst.Cfg.OffsetCnP[1].dDefPickerOffsetX.ToString("F3");
                        edtCnPOffsetY2.Text = ConfigMgr.Inst.Cfg.OffsetCnP[1].dDefPickerOffsetY.ToString("F3");

                        edtPnPDefOffsetX1.Text = ConfigMgr.Inst.Cfg.OffsetPnP[0].dDefPickerPitchX.ToString("F3");
                        edtPnPDefOffsetX2.Text = ConfigMgr.Inst.Cfg.OffsetPnP[1].dDefPickerPitchX.ToString("F3");

                        numPickerCalRetryCount.Value = ConfigMgr.Inst.Cfg.General.nPickerCalRetryCount;
                        edtPickerTolerence.Text = ConfigMgr.Inst.Cfg.General.dPickerCalTolerence.ToString("F3");

                        edtZerosetStartHead1.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead1Z.ToString("F3");//221011 제로셋 Z위치 및 Down Step 저장
                        edtZerosetStartHead2.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead2Z.ToString("F3");

                        edtZerosetStepHead1.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead1Z.ToString("F3");
                        edtZerosetStepHead2.Text = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead2Z.ToString("F3");

                        edtPlaceZeroSetStartHead1.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead1Z.ToString("F3");//221011 제로셋 Z위치 및 Down Step 저장
                        edtPlaceZeroSetStartHead2.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead2Z.ToString("F3");

                        edtPlaceZeroSetStepHead1.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead1Z.ToString("F3");
                        edtPlaceZeroSetStepHead2.Text = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead2Z.ToString("F3");

                        switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                        {
                            case Language.ENGLISH:
                                rdbLanguageEng.Checked = true;
                                rdbLanguageChn.Checked = false;
                                rdbLanguageKor.Checked = false;
                                break;
                            case Language.CHINA:
                                rdbLanguageEng.Checked = false;
                                rdbLanguageChn.Checked = true;
                                rdbLanguageKor.Checked = false;
                                break;
                            case Language.KOREAN:
                                rdbLanguageEng.Checked = false;
                                rdbLanguageChn.Checked = false;
                                rdbLanguageKor.Checked = true;
                                break;
                        }
                    }
                    else
                    {

                        ConfigMgr.Inst.TempCfg.OffsetCnP[0].dDefPickerOffsetX = edtCnPOffsetX1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[0].dDefPickerOffsetY = edtCnPOffsetY1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[1].dDefPickerOffsetX = edtCnPOffsetX2.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetCnP[1].dDefPickerOffsetY = edtCnPOffsetY2.ToDouble();

                        ConfigMgr.Inst.TempCfg.OffsetPnP[0].dDefPickerPitchX = edtPnPDefOffsetX1.ToDouble();
                        ConfigMgr.Inst.TempCfg.OffsetPnP[1].dDefPickerPitchX = edtPnPDefOffsetX2.ToDouble();

                        ConfigMgr.Inst.TempCfg.General.nPickerCalRetryCount = (int)numPickerCalRetryCount.Value;
                        ConfigMgr.Inst.TempCfg.General.dPickerCalTolerence = edtPickerTolerence.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStartHead1Z = edtZerosetStartHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStartHead2Z = edtZerosetStartHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStepHead1Z = edtZerosetStepHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPickUpZerosetStepHead2Z = edtZerosetStepHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStartHead1Z = edtPlaceZeroSetStartHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStartHead2Z = edtPlaceZeroSetStartHead2.ToDouble();

                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStepHead1Z = edtPlaceZeroSetStepHead1.ToDouble();
                        ConfigMgr.Inst.TempCfg.dPlaceZerosetStepHead2Z = edtPlaceZeroSetStepHead2.ToDouble();
                    }

                    foreach (CfgMotPosEditButtonEx item in m_listMotPosBtn)
                    {
                        item.Text = ConfigMgr.Inst.Cfg.dMotPos[(int)item.Axis].dPos[item.PosNo].ToString("F3");
                    }
                });
            }
            m_bIsUpdate = false;
        }

        static void MoveSplitterTo(PropertyGrid grid, int x)
        {
            // HEALTH WARNING: reflection can be brittle...
            FieldInfo field = typeof(PropertyGrid)
                .GetField("gridView",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            field.FieldType
                .GetMethod("MoveSplitterTo",
                    BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(field.GetValue(grid), new object[] { x });
            grid.Tag = x.ToString();
        }

        private void tabOperating_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void frmConfig_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible == true)
            {
                UpdateCfgData();
                FindControl(this.Controls);
                btnGetCfgCamToPickerOffset1.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                btnGetCfgCamToPickerOffset2.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                btnTrayVisionView_P1.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                btnTrayVisionView_P2.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
            }
        }
        static void FindControl(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(UC.BaseCfgPosControl) ||
                    ctrl.GetType() == typeof(UC.ucNumericKeyInputButtonEx) ||
                    ctrl.GetType() == typeof(UC.CONFIG.CfgMotPosEditButtonEx) ||
                    ctrl.GetType() == typeof(UC.CONFIG.CfgMotPosGetButtonEx) ||
                    ctrl.GetType() == typeof(UC.CONFIG.CfgMotPosMoveButtonEx) ||
                    ctrl.GetType() == typeof(UC.DataGridViewEx) ||
                    ctrl.GetType() == typeof(DataGridView) ||
                    ctrl.GetType() == typeof(NumericUpDown) ||
                    ctrl.GetType() == typeof(CheckBox) ||
                    ctrl.GetType() == typeof(Button) ||
                    ctrl.GetType() == typeof(TextBox))
                {
                    try
                    {
                        ctrl.Enabled = GbVar.CurLoginInfo.USER_LEVEL != MCDF.LEVEL_OP;
                    }
                    catch (Exception ex)
                    {

                    }

                }

                if (ctrl.Controls.Count > 0)
                    FindControl(ctrl.Controls);
            }
        }
        private void btnConfigSave_Click(object sender, EventArgs e)
        {
            popCheckPassword chk = new popCheckPassword();
            chk.TopMost = true;
            if (chk.ShowDialog(this) != DialogResult.OK) return;
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnSaveConfig_Click");

            if (!ConfigMgr.Inst.DataCheckBeforeSave()) return;

            UpdateCfgData(true);
            //UpdateMotionTime();
            //CompareValue(ConfigMgr.Inst.TempCfg, ConfigMgr.Inst.Cfg);

            ConfigMgr.Inst.SaveBackup();
            ConfigMgr.Inst.SaveTempCfg();
            ConfigMgr.Inst.Save();

            UpdateCfgData();
            SetHomeOffset();
            SetMaxVelocity();
            SetInPositionTolerance();

            //로그 강제 삭제
            for (DBDF.LOG_TYPE i = 0; i < DBDF.LOG_TYPE.MAX; i++)
            {
                GbVar.dbLogData[(int)i].ForcedDeleteOldFile();
            }

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "SaveConfig Complete");
            popMessageBox pMsg = new popMessageBox(FormTextLangMgr.FindKey("SAVE COMPLETE"), FormTextLangMgr.FindKey("ACCOUNT"), MessageBoxButtons.OK);
            if (pMsg.ShowDialog(this) != DialogResult.OK) return;

            if (deleSaveCfg != null) deleSaveCfg();
            if (ChangeFormLang != null)
            {
                ChangeFormLang();
            }
            ////열풍기 온도설정
            //GbDev.tempController.SetValue(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.HEATING_SV_VALUE].nValue);
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.HEATING_MODE_AUTO].bOptionUse == false)
            //{
            //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, false);
            //    GbDev.tempController.StartTempController(1); //1이면 스탑
            //}
        }

        private void tabConfigPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
        }

        private void gridPickerHeadOffset_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg0) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_0;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }

        private void gridPickerHeadOffsetDeg90_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg90) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_P90;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }
        private void gridPickerHeadOffsetDeg180_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg180) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_P180;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }
        private void gridPickerHeadOffsetDeg270_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg270) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_P270;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }

        private void gridPickerHeadOffsetDegN90_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1degN90) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_N90;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }
        private void gridPickerHeadOffsetDegN180_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg180) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_N180;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }
        private void gridPickerHeadOffsetDegN270_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;

            if (e.ColumnIndex < 0) return;
            if (e.RowIndex != 4 && e.RowIndex != 9) return;
            int nHeadNo = 0;

            if (dgv == gridPickerHeadOffset1deg270) nHeadNo = 0;
            else nHeadNo = 1;

            int nIndex = e.ColumnIndex - 1;
            int nDegree = CFG_DF.DEGREE_N270;

            gridPickerHeadOffsetCellClick(nHeadNo, nIndex, nDegree);

            UpdatePickerHeadOffsetGridData();
        }

        void gridPickerHeadOffsetCellClick(int nHeadNo, int nIndex, int nDegree)
        {
            if (nIndex < 0)
            {
                // 전체 다시 계산
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][0].x = 0.0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][0].y = 0.0;

                for (int i = 1; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                {
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][i].x =
                        ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][i].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][0].x;
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][i].y =
                        ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][i].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][0].y;
                }
            }
            else
            {
                // 선택 축만 계산
#if !_NOTEBOOK
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][nIndex].x = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].GetRealPos();
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][nIndex].y = MotionMgr.Inst[(int)SVDF.AXES.BALL_VISION_Y].GetRealPos();
#endif
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][0].x = 0.0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][0].y = 0.0;

                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][nIndex].x =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][nIndex].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][0].x;

                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nDegree][nIndex].y =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][nIndex].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nDegree][0].y;
            }

            if (nIndex < 0)
            {
                // 전체 다시 계산
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][0].x = 0.0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][0].y = 0.0;

                for (int i = 1; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                {
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][i].x =
                        ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][i].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][0].x;
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][i].y =
                        ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][i].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][0].y;
                }
            }
            else
            {
                // 선택 축만 계산
#if !_NOTEBOOK
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][nIndex].x = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].GetRealPos();
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][nIndex].y = MotionMgr.Inst[(int)SVDF.AXES.BALL_VISION_Y].GetRealPos();
#endif
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][0].x = 0.0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][0].y = 0.0;

                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][nIndex].x =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][nIndex].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][0].x;

                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nDegree][nIndex].y =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][nIndex].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nDegree][0].y;
            }
        }
        public void UpdatePickerHeadOffsetGridData()
        {
            DataGridView[] dgvArray = { gridPickerHeadOffset1deg0, gridPickerHeadOffset2deg0, 
                gridPickerHeadOffset1deg90, gridPickerHeadOffset2deg90, 
                gridPickerHeadOffset1deg180, gridPickerHeadOffset2deg180,
                gridPickerHeadOffset1deg270, gridPickerHeadOffset2deg270,
                gridPickerHeadOffset1degN90, gridPickerHeadOffset2degN90,
                gridPickerHeadOffset1degN180, gridPickerHeadOffset2degN180,
                gridPickerHeadOffset1degN270, gridPickerHeadOffset2degN270

            };

            for (int nCnt = 0; nCnt < dgvArray.Length; nCnt++)
            {
                dgvArray[nCnt].SuspendLayout();
                dgvArray[nCnt].RowCount = 14;
            }

            #region PNP용 Z축 Picker Offset Row 생성
            gridPickerHeadOffset1deg0.Rows[0].Cells[0].Value = "0º PNP X REF";
            gridPickerHeadOffset1deg0.Rows[1].Cells[0].Value = "0º PNP X OFF";
            gridPickerHeadOffset1deg0.Rows[2].Cells[0].Value = "0º PNP Y REF";
            gridPickerHeadOffset1deg0.Rows[3].Cells[0].Value = "0º PNP Y OFF";
            gridPickerHeadOffset1deg0.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg0.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg0.Rows[0].Cells[0].Value = "0º PNP X REF";
            gridPickerHeadOffset2deg0.Rows[1].Cells[0].Value = "0º PNP X OFF";
            gridPickerHeadOffset2deg0.Rows[2].Cells[0].Value = "0º PNP Y REF";
            gridPickerHeadOffset2deg0.Rows[3].Cells[0].Value = "0º PNP Y OFF";
            gridPickerHeadOffset2deg0.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg0.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg90.Rows[0].Cells[0].Value = "90º PNP X REF";
            gridPickerHeadOffset1deg90.Rows[1].Cells[0].Value = "90º PNP X OFF";
            gridPickerHeadOffset1deg90.Rows[2].Cells[0].Value = "90º PNP Y REF";
            gridPickerHeadOffset1deg90.Rows[3].Cells[0].Value = "90º PNP Y OFF";
            gridPickerHeadOffset1deg90.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg90.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg90.Rows[0].Cells[0].Value = "90º PNP X REF";
            gridPickerHeadOffset2deg90.Rows[1].Cells[0].Value = "90º PNP X OFF";
            gridPickerHeadOffset2deg90.Rows[2].Cells[0].Value = "90º PNP Y REF";
            gridPickerHeadOffset2deg90.Rows[3].Cells[0].Value = "90º PNP Y OFF";
            gridPickerHeadOffset2deg90.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg90.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg180.Rows[0].Cells[0].Value = "180º PNP X REF";
            gridPickerHeadOffset1deg180.Rows[1].Cells[0].Value = "180º PNP X OFF";
            gridPickerHeadOffset1deg180.Rows[2].Cells[0].Value = "180º PNP Y REF";
            gridPickerHeadOffset1deg180.Rows[3].Cells[0].Value = "180º PNP Y OFF";
            gridPickerHeadOffset1deg180.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg180.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg180.Rows[0].Cells[0].Value = "180º PNP X REF";
            gridPickerHeadOffset2deg180.Rows[1].Cells[0].Value = "180º PNP X OFF";
            gridPickerHeadOffset2deg180.Rows[2].Cells[0].Value = "180º PNP Y REF";
            gridPickerHeadOffset2deg180.Rows[3].Cells[0].Value = "180º PNP Y OFF";
            gridPickerHeadOffset2deg180.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg180.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg270.Rows[0].Cells[0].Value = "270º PNP X REF";
            gridPickerHeadOffset1deg270.Rows[1].Cells[0].Value = "270º PNP X OFF";
            gridPickerHeadOffset1deg270.Rows[2].Cells[0].Value = "270º PNP Y REF";
            gridPickerHeadOffset1deg270.Rows[3].Cells[0].Value = "270º PNP Y OFF";
            gridPickerHeadOffset1deg270.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg270.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg270.Rows[0].Cells[0].Value = "270º PNP X REF";
            gridPickerHeadOffset2deg270.Rows[1].Cells[0].Value = "270º PNP X OFF";
            gridPickerHeadOffset2deg270.Rows[2].Cells[0].Value = "270º PNP Y REF";
            gridPickerHeadOffset2deg270.Rows[3].Cells[0].Value = "270º PNP Y OFF";
            gridPickerHeadOffset2deg270.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg270.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN90.Rows[0].Cells[0].Value = "-90º PNP X REF";
            gridPickerHeadOffset1degN90.Rows[1].Cells[0].Value = "-90º PNP X OFF";
            gridPickerHeadOffset1degN90.Rows[2].Cells[0].Value = "-90º PNP Y REF";
            gridPickerHeadOffset1degN90.Rows[3].Cells[0].Value = "-90º PNP Y OFF";
            gridPickerHeadOffset1degN90.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN90.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN90.Rows[0].Cells[0].Value = "-90º PNP X REF";
            gridPickerHeadOffset2degN90.Rows[1].Cells[0].Value = "-90º PNP X OFF";
            gridPickerHeadOffset2degN90.Rows[2].Cells[0].Value = "-90º PNP Y REF";
            gridPickerHeadOffset2degN90.Rows[3].Cells[0].Value = "-90º PNP Y OFF";
            gridPickerHeadOffset2degN90.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN90.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN180.Rows[0].Cells[0].Value = "-180º PNP X REF";
            gridPickerHeadOffset1degN180.Rows[1].Cells[0].Value = "-180º PNP X OFF";
            gridPickerHeadOffset1degN180.Rows[2].Cells[0].Value = "-180º PNP Y REF";
            gridPickerHeadOffset1degN180.Rows[3].Cells[0].Value = "-180º PNP Y OFF";
            gridPickerHeadOffset1degN180.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN180.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN180.Rows[0].Cells[0].Value = "-180º PNP X REF";
            gridPickerHeadOffset2degN180.Rows[1].Cells[0].Value = "-180º PNP X OFF";
            gridPickerHeadOffset2degN180.Rows[2].Cells[0].Value = "-180º PNP Y REF";
            gridPickerHeadOffset2degN180.Rows[3].Cells[0].Value = "-180º PNP Y OFF";
            gridPickerHeadOffset2degN180.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN180.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN270.Rows[0].Cells[0].Value = "-270º PNP X REF";
            gridPickerHeadOffset1degN270.Rows[1].Cells[0].Value = "-270º PNP X OFF";
            gridPickerHeadOffset1degN270.Rows[2].Cells[0].Value = "-270º PNP Y REF";
            gridPickerHeadOffset1degN270.Rows[3].Cells[0].Value = "-270º PNP Y OFF";
            gridPickerHeadOffset1degN270.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN270.Rows[4].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN270.Rows[0].Cells[0].Value = "-270º PNP X REF";
            gridPickerHeadOffset2degN270.Rows[1].Cells[0].Value = "-270º PNP X OFF";
            gridPickerHeadOffset2degN270.Rows[2].Cells[0].Value = "-270º PNP Y REF";
            gridPickerHeadOffset2degN270.Rows[3].Cells[0].Value = "-270º PNP Y OFF";
            gridPickerHeadOffset2degN270.Rows[4].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN270.Rows[4].Cells[0].Value = "CALCULATE";
            #endregion

            #region INSPECTION용 Z축 Picker Offset Row 생성
            gridPickerHeadOffset1deg0.Rows[5].Cells[0].Value = "0º INSP X REF";
            gridPickerHeadOffset1deg0.Rows[6].Cells[0].Value = "0º INSP X OFF";
            gridPickerHeadOffset1deg0.Rows[7].Cells[0].Value = "0º INSP Y REF";
            gridPickerHeadOffset1deg0.Rows[8].Cells[0].Value = "0º INSP Y OFF";
            gridPickerHeadOffset1deg0.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg0.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg0.Rows[5].Cells[0].Value = "0º INSP X REF";
            gridPickerHeadOffset2deg0.Rows[6].Cells[0].Value = "0º INSP X OFF";
            gridPickerHeadOffset2deg0.Rows[7].Cells[0].Value = "0º INSP Y REF";
            gridPickerHeadOffset2deg0.Rows[8].Cells[0].Value = "0º INSP Y OFF";
            gridPickerHeadOffset2deg0.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg0.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg90.Rows[5].Cells[0].Value = "90º INSP X REF";
            gridPickerHeadOffset1deg90.Rows[6].Cells[0].Value = "90º INSP X OFF";
            gridPickerHeadOffset1deg90.Rows[7].Cells[0].Value = "90º INSP Y REF";
            gridPickerHeadOffset1deg90.Rows[8].Cells[0].Value = "90º INSP Y OFF";
            gridPickerHeadOffset1deg90.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg90.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg90.Rows[5].Cells[0].Value = "90º INSP X REF";
            gridPickerHeadOffset2deg90.Rows[6].Cells[0].Value = "90º INSP X OFF";
            gridPickerHeadOffset2deg90.Rows[7].Cells[0].Value = "90º INSP Y REF";
            gridPickerHeadOffset2deg90.Rows[8].Cells[0].Value = "90º INSP Y OFF";
            gridPickerHeadOffset2deg90.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg90.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg180.Rows[5].Cells[0].Value = "180º INSP X REF";
            gridPickerHeadOffset1deg180.Rows[6].Cells[0].Value = "180º INSP X OFF";
            gridPickerHeadOffset1deg180.Rows[7].Cells[0].Value = "180º INSP Y REF";
            gridPickerHeadOffset1deg180.Rows[8].Cells[0].Value = "180º INSP Y OFF";
            gridPickerHeadOffset1deg180.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg180.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg180.Rows[5].Cells[0].Value = "180º INSP X REF";
            gridPickerHeadOffset2deg180.Rows[6].Cells[0].Value = "180º INSP X OFF";
            gridPickerHeadOffset2deg180.Rows[7].Cells[0].Value = "180º INSP Y REF";
            gridPickerHeadOffset2deg180.Rows[8].Cells[0].Value = "180º INSP Y OFF";
            gridPickerHeadOffset2deg180.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg180.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1deg270.Rows[5].Cells[0].Value = "270º INSP X REF";
            gridPickerHeadOffset1deg270.Rows[6].Cells[0].Value = "270º INSP X OFF";
            gridPickerHeadOffset1deg270.Rows[7].Cells[0].Value = "270º INSP Y REF";
            gridPickerHeadOffset1deg270.Rows[8].Cells[0].Value = "270º INSP Y OFF";
            gridPickerHeadOffset1deg270.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1deg270.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2deg270.Rows[5].Cells[0].Value = "270º INSP X REF";
            gridPickerHeadOffset2deg270.Rows[6].Cells[0].Value = "270º INSP X OFF";
            gridPickerHeadOffset2deg270.Rows[7].Cells[0].Value = "270º INSP Y REF";
            gridPickerHeadOffset2deg270.Rows[8].Cells[0].Value = "270º INSP Y OFF";
            gridPickerHeadOffset2deg270.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2deg270.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN90.Rows[5].Cells[0].Value = "-90º INSP X REF";
            gridPickerHeadOffset1degN90.Rows[6].Cells[0].Value = "-90º INSP X OFF";
            gridPickerHeadOffset1degN90.Rows[7].Cells[0].Value = "-90º INSP Y REF";
            gridPickerHeadOffset1degN90.Rows[8].Cells[0].Value = "-90º INSP Y OFF";
            gridPickerHeadOffset1degN90.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN90.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN90.Rows[5].Cells[0].Value = "-90º INSP X REF";
            gridPickerHeadOffset2degN90.Rows[6].Cells[0].Value = "-90º INSP X OFF";
            gridPickerHeadOffset2degN90.Rows[7].Cells[0].Value = "-90º INSP Y REF";
            gridPickerHeadOffset2degN90.Rows[8].Cells[0].Value = "-90º INSP Y OFF";
            gridPickerHeadOffset2degN90.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN90.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN180.Rows[5].Cells[0].Value = "-180º INSP X REF";
            gridPickerHeadOffset1degN180.Rows[6].Cells[0].Value = "-180º INSP X OFF";
            gridPickerHeadOffset1degN180.Rows[7].Cells[0].Value = "-180º INSP Y REF";
            gridPickerHeadOffset1degN180.Rows[8].Cells[0].Value = "-180º INSP Y OFF";
            gridPickerHeadOffset1degN180.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN180.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN180.Rows[5].Cells[0].Value = "-180º INSP X REF";
            gridPickerHeadOffset2degN180.Rows[6].Cells[0].Value = "-180º INSP X OFF";
            gridPickerHeadOffset2degN180.Rows[7].Cells[0].Value = "-180º INSP Y REF";
            gridPickerHeadOffset2degN180.Rows[8].Cells[0].Value = "-180º INSP Y OFF";
            gridPickerHeadOffset2degN180.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN180.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset1degN270.Rows[5].Cells[0].Value = "-270º INSP X REF";
            gridPickerHeadOffset1degN270.Rows[6].Cells[0].Value = "-270º INSP X OFF";
            gridPickerHeadOffset1degN270.Rows[7].Cells[0].Value = "-270º INSP Y REF";
            gridPickerHeadOffset1degN270.Rows[8].Cells[0].Value = "-270º INSP Y OFF";
            gridPickerHeadOffset1degN270.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset1degN270.Rows[9].Cells[0].Value = "CALCULATE";

            gridPickerHeadOffset2degN270.Rows[5].Cells[0].Value = "-270º INSP X REF";
            gridPickerHeadOffset2degN270.Rows[6].Cells[0].Value = "-270º INSP X OFF";
            gridPickerHeadOffset2degN270.Rows[7].Cells[0].Value = "-270º INSP Y REF";
            gridPickerHeadOffset2degN270.Rows[8].Cells[0].Value = "-270º INSP Y OFF";
            gridPickerHeadOffset2degN270.Rows[9].Cells[0] = new DataGridViewButtonCell();
            gridPickerHeadOffset2degN270.Rows[9].Cells[0].Value = "CALCULATE";
            #endregion

            #region PNP용 Picker Offset 값 입력
            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                gridPickerHeadOffset1deg0.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg0.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg0.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg0.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg90.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg90.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg180.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg180.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg270.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg270.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN90.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN90.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN180.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN180.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN270.Rows[4].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[4].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN270.Rows[4].Cells[nPadNo + 1].Value = "GET";
            }
            #endregion

            #region INSPECTION용 Picker Offset 값 입력
            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                gridPickerHeadOffset1deg0.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg0.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg0.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg0.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg0.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg0.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg90.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg90.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg90.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg90.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg180.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg180.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg180.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg180.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1deg270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1deg270.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1deg270.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2deg270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2deg270.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2deg270.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN90.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN90.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN90.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN90.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN180.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN180.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN180.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN180.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset1degN270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset1degN270.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset1degN270.Rows[9].Cells[nPadNo + 1].Value = "GET";

                gridPickerHeadOffset2degN270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                gridPickerHeadOffset2degN270.Rows[9].Cells[nPadNo + 1] = new DataGridViewButtonCell();
                gridPickerHeadOffset2degN270.Rows[9].Cells[nPadNo + 1].Value = "GET";
            }
            #endregion

            for (int nCnt = 0; nCnt < dgvArray.Length; nCnt++)
            {
                dgvArray[nCnt].ResumeLayout();
            }

            #region Picker Head 1 Z축 다운 위치
            gridPnpOffsetHead1.RowCount = 11;
            int nTmp = 0;
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 Pick Up Wait Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 Pick Up Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 Pick Up Wait Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 Pick Up Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 1 Place Wait Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 1 Place Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 2 Place Wait Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 2 Place Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE REWORK Place Wait Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "TRAY STAGE REWORK Place Z";
            gridPnpOffsetHead1.Rows[nTmp++].Cells[0].Value = "REJECT BOX Place Z";

            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                nTmp = 0;
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_1];
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_1];

                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_2];
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_2];

                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD1];
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD1];

                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD2];
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD2];

                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_RWK];
                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RWK];

                gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RJT];
            }
            #endregion

            #region Picker Head 2 Z축 다운 위치
            gridPnpOffsetHead2.RowCount = 11;
            nTmp = 0;
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 Pick Up Wait Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 Pick Up Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 Pick Up Wait Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 Pick Up Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 1 Place Wait Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 1 Place Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 2 Place Wait Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE GOOD 2 Place Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE REWORK Place Wait Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "TRAY STAGE REWORK Place Z";
            gridPnpOffsetHead2.Rows[nTmp++].Cells[0].Value = "REJECT BOX Place Z";

            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                nTmp = 0;
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_1];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_1];

                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_2];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_2];

                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD1];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD1];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD2];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD2];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_RWK];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RWK];
                gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RJT];
            }
            #endregion

            #region Picker Head 1 XYZT축 OFFSET
            gridXyztOffsetHead1.RowCount = 20;
            nTmp = 0;
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP X";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP Y";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP Z";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP T";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP X";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP Y";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP Z";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP T";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE X";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE Y";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE Z";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE T";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE X";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE Y";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE Z";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE T";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE X";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE Y";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE Z";
            gridXyztOffsetHead1.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE T";

            UpdatePickerXyztGridData(0);
            #endregion

            #region Picker Head 2 XYZT축 OFFSET
            gridXyztOffsetHead2.RowCount = 20;
            nTmp = 0;
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP X";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP Y";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP Z";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 1 PICK UP T";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP X";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP Y";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP Z";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "MAP STAGE 2 PICK UP T";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE X";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE Y";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE Z";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 1 PLACE T";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE X";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE Y";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE Z";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "GOOD TRAY 2 PLACE T";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE X";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE Y";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE Z";
            gridXyztOffsetHead2.Rows[nTmp++].Cells[0].Value = "REWORK TRAY PLACE T";

            UpdatePickerXyztGridData(1);
            #endregion

            for (int i = 0; i < 20; i++)
            {
                gridXyztOffsetHead1.Rows[i].Height = 28;
                gridXyztOffsetHead2.Rows[i].Height = 28;
            }
        }
        void UpdatePickerDownGridData(int nHeadNo)
        {
            int nTmp = 0;
            if (nHeadNo == 0)
            {
                for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
                {
                    nTmp = 0;
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_1];
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_1];

                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_2];
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_2];

                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD1];
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD1];

                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD2];
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD2];

                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_RWK];
                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RWK];

                    gridPnpOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RJT];
                }
            }
            else
            {

                for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
                {
                    nTmp = 0;
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_1];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_1];

                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUpWait[CFG_DF.MAP_STG_2];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPickUp[CFG_DF.MAP_STG_2];

                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD1];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD1];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_GD2];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_GD2];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlaceWait[CFG_DF.TRAY_STG_RWK];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RWK];
                    gridPnpOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nPadNo].dPlace[CFG_DF.TRAY_STG_RJT];
                }
            }
        }
        void UpdatePickerXyztGridData(int nHeadNo)
        {
            int nTmp = 0;
            if (nHeadNo == 0)
            {
                for (int nPadNo = 0; nPadNo < MCDF.PAD_MAX; nPadNo++)
                {
                    nTmp = 0;

                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_1][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_1][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_1][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_1][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_2][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_2][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_2][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.MAP_TABLE_2][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_1][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_1][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_1][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_1][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_2][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_2][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_2][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.GOOD_TRAY_2][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.REWORK_TRAY][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.REWORK_TRAY][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.REWORK_TRAY][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead1.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD1][MCDF.REWORK_TRAY][MCDF.XYZT_T][nPadNo];
                }
            }
            else
            {
                for (int nPadNo = 0; nPadNo < MCDF.PAD_MAX; nPadNo++)
                {
                    nTmp = 0;

                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_1][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_1][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_1][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_1][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_2][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_2][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_2][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.MAP_TABLE_2][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_1][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_1][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_1][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_1][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_2][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_2][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_2][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.GOOD_TRAY_2][MCDF.XYZT_T][nPadNo];

                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.REWORK_TRAY][MCDF.XYZT_X][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.REWORK_TRAY][MCDF.XYZT_Y][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.REWORK_TRAY][MCDF.XYZT_Z][nPadNo];
                    gridXyztOffsetHead2.Rows[nTmp++].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.dOffsetXYZT[MCDF.HEAD2][MCDF.REWORK_TRAY][MCDF.XYZT_T][nPadNo];
                }
            }
        }
        private void gridPickerHeadOffset1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1deg0) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_0][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nCol - 1].y = dValue;
        }

        private void gridPickerHeadOffsetDeg90_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1deg90) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nCol - 1].y = dValue;
        }

        private void gridPickerHeadOffsetDeg180_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1deg180) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nCol - 1].y = dValue;
        }
        private void gridPickerHeadOffsetDeg270_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1deg270) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nCol - 1].y = dValue;
        }

        private void gridPickerHeadOffsetDegN90_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1degN90) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nCol - 1].y = dValue;
        }

        private void gridPickerHeadOffsetDegN180_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1degN180) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nCol - 1].y = dValue;
        }
        private void gridPickerHeadOffsetDegN270_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            int nHeadNo = 0;
            DataGridView dgv = sender as DataGridView;
            if (dgv == gridPickerHeadOffset1degN270) nHeadNo = 0;
            else nHeadNo = 1;

            if (dgv.CurrentCell == null) return;
            int nRow = dgv.CurrentCell.RowIndex;      //x
            int nCol = dgv.CurrentCell.ColumnIndex;   //y
            if (nCol < 1) return;

            double dValue = 0.0;
            if (!double.TryParse(dgv.CurrentCell.Value.ToString(), out dValue)) return;

            if (nRow == 0) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nCol - 1].x = dValue;
            else if (nRow == 1) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nCol - 1].x = dValue;
            else if (nRow == 2) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nCol - 1].y = dValue;
            else if (nRow == 3) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nCol - 1].y = dValue;
            else if (nRow == 5) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nCol - 1].x = dValue;
            else if (nRow == 6) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nCol - 1].x = dValue;
            else if (nRow == 7) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nCol - 1].y = dValue;
            else if (nRow == 8) ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nCol - 1].y = dValue;
        }

        /// <summary>
        /// 220427 pjh 수정(T관련 row삭제)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridPnpOffsetHead1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gridPnpOffsetHead1.CurrentCell == null) return;
            int nRow = gridPnpOffsetHead1.CurrentCell.RowIndex;      //x
            int nCol = gridPnpOffsetHead1.CurrentCell.ColumnIndex;   //y
            if (nCol < 1 || nCol > 8) return;
            if (gridPnpOffsetHead1.Rows[nRow].Cells[nCol].Value == null)
            {
                return;
            }
            double dVal;
            if (!double.TryParse(gridPnpOffsetHead1.Rows[nRow].Cells[nCol].Value.ToString(), out dVal)) return;

            int nTmp = 0;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPickUpWait[CFG_DF.MAP_STG_1] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPickUp[CFG_DF.MAP_STG_1] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPickUpWait[CFG_DF.MAP_STG_2] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPickUp[CFG_DF.MAP_STG_2] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_GD1] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_GD1] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_GD2] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_GD2] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_RWK] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_RWK] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_RJT] = dVal;
        }

        /// <summary>
        /// 220427 pjh 수정(T관련 row삭제)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridPnpOffsetHead2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gridPnpOffsetHead2.CurrentCell == null) return;
            int nRow = gridPnpOffsetHead2.CurrentCell.RowIndex;      //x
            int nCol = gridPnpOffsetHead2.CurrentCell.ColumnIndex;   //y
            if (nCol < 1 || nCol > 8) return;

            double dVal;
            if (!double.TryParse(gridPnpOffsetHead2.Rows[nRow].Cells[nCol].Value.ToString(), out dVal)) return;

            int nTmp = 0;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPickUpWait[CFG_DF.MAP_STG_1] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPickUp[CFG_DF.MAP_STG_1] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPickUpWait[CFG_DF.MAP_STG_2] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPickUp[CFG_DF.MAP_STG_2] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_GD1] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_GD1] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_GD2] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_GD2] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlaceWait[CFG_DF.TRAY_STG_RWK] = dVal;
            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_RWK] = dVal;

            if (nRow == nTmp++) ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[nCol - 1].dPlace[CFG_DF.TRAY_STG_RJT] = dVal;
        }

        private void OnHeadOffsetCalSubMenuButtonClick(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tbcPickerOffset.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tbcPickerOffset.SelectedIndex = nTag;
            }
        }

        private void OnStageOffsetCalSubMenuButtonClick(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;
            int nTag;
            if (!int.TryParse(rdb.Tag.ToString(), out nTag)) return;

            if (tbcStageOffset.TabPages.Count < nTag) return;

            if (rdb.Checked)
            {
                // 탭 전환
                tbcStageOffset.SelectedIndex = nTag;
            }
        }

        private void ShowOptionData()
        {
            Font font = new Font("맑은 고딕", 10, FontStyle.Regular);

            int nRoopCnt = 0;
            int nOffset = 0;
            m_dgvCommonItemIdx = new List<int>();
            m_dgvMachineItemIdx = new List<int>();
            m_dgvLoaderItemIdx = new List<int>();
            m_dgvSawItemIdx = new List<int>();
            m_dgvSorterItemIdx = new List<int>();
            m_dgvVisionItemIdx = new List<int>();
            m_dgvCleanerItemIdx = new List<int>();

            dgvCommon.Rows.Clear();
            dgvCommon.AllowUserToAddRows = false;
            dgvCommon.Font = font;
            nRoopCnt = (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT - (int)OPTNUM.GEM_USE;
            nOffset = (int)OPTNUM.GEM_USE;

            ChangeDataGridView(dgvCommon,nRoopCnt,nOffset,m_dgvCommonItemIdx, true);
            rdbUseCommon.Checked = true;

            dgvLoader.Rows.Clear();
            dgvLoader.AllowUserToAddRows = false;
            dgvLoader.Font = font;
            nRoopCnt = (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET - (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT;
            nOffset = (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT;

            ChangeDataGridView(dgvLoader, nRoopCnt, nOffset, m_dgvLoaderItemIdx, true);
            rdbUseLoader.Checked = true;


            dgvSaw.Rows.Clear();
            dgvSaw.AllowUserToAddRows = false;
            dgvSaw.Font = font;
            nRoopCnt = 1800 - (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET;
            nOffset = (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET;

            ChangeDataGridView(dgvSaw, nRoopCnt, nOffset, m_dgvSawItemIdx, false);
            rdbUseSaw.Visible = false;

            dgvSorter.Rows.Clear();
            dgvSorter.AllowUserToAddRows = false;
            dgvSorter.Font = font;
            nRoopCnt = (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET - 1800;
            nOffset = 1800;
            ChangeDataGridView(dgvSorter, nRoopCnt, nOffset, m_dgvSorterItemIdx, true);
            rdbUseSorter.Checked = true;
        }

        /// <summary>
        /// DataGridView Setting
        /// </summary>
        /// <param name="dgv">DataGridView</param>
        /// <param name="nRoopCnt">범위</param>
        /// <param name="nOffset">오프셋</param>
        /// <param name="dgvList">멤버변수</param>
        void ChangeDataGridView(DataGridView dgv, int nRoopCnt, int nOffset,  List<int> dgvList, bool bIsUseSkip)
        {
            Font font = new Font("맑은 고딕", 10, FontStyle.Regular);

            dgv.Rows.Clear();
            dgv.AllowUserToAddRows = false;
            dgv.Font = font;

            for (int i = 0; i < nRoopCnt; i++)
            {
                OptionItem item = ConfigMgr.Inst.Cfg.itemOptions[i + nOffset];
                if (item.strOptionName == "NotDefined")
                    continue;
                dgvList.Add(item.nIndex);
                dgv.Rows.Add(item.strOptionPart, item.strOptionName, item.strValue);
            }
            if (bIsUseSkip)
            {
                dgv.Columns[2].Visible = false;
                dgv.Columns[3].Visible = true;
            }
            else
            {
                dgv.Columns[2].Visible = true;
                dgv.Columns[3].Visible = false;
            }

            foreach (DataGridViewRow item in dgv.Rows)
            {
                if (item.Cells[1].Value.ToString().Contains("니다") ||
                    item.Cells[1].Value.ToString().Contains("사용"))
                {
                    item.Visible = !bIsUseSkip ? false : true;
                }
                else
                {
                    item.Visible = bIsUseSkip ? false : true;
                }
                for (int nIdx = 0; nIdx < ConfigMgr.Inst.Cfg.itemOptions.Length; nIdx++)
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[nIdx].strOptionName == item.Cells[1].Value.ToString())
                    {
                        switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                        {
                            case Language.ENGLISH:
                                    switch (nIdx)
                                    {
                                        case (int)OPTNUM.GEM_USE: { item.Cells[1].Value =  "Use GEM"; } break;
                                        case (int)OPTNUM.VAC_SENSOR_USE: { item.Cells[1].Value = "Check the VACUM Sensor"; } break;
                                        case (int)OPTNUM.DRY_RUN_MODE: { item.Cells[1].Value = "Use DRY MODE"; } break;
                                        case (int)OPTNUM.DRY_RUN_SAW_USE: { item.Cells[1].Value = "Use the interface with SAW when in DRY mode."; } break;
                                        case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { item.Cells[1].Value = "Randomly eject materials when in dry mode"; } break;
                                        case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { item.Cells[1].Value = "Use the vision and interface when in DRY mode."; } break;

                                        case (int)OPTNUM.TOP_VISION_USE: { item.Cells[1].Value = "Use upper vision"; } break;
                                        case (int)OPTNUM.TOP_ALIGN_USE: { item.Cells[1].Value = "Upper alignment is used."; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_USE: { item.Cells[1].Value = "Use lower vision"; } break;
                                        case (int)OPTNUM.NO_MSG_STOP_TIME: { item.Cells[1].Value ="Facility silent stop judgment time (unit: minutes)"; } break;
                                        case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { item.Cells[1].Value ="Magazine Emission Sensor Detection Time Set (in milliseconds)"; } break;
                                        case (int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE: { item.Cells[1].Value = "Use the CLEAR function before reading vision data."; } break;

                                        case (int)OPTNUM.TRAY_INSPECTION_USE: { item.Cells[1].Value ="Use tray unit sorry check"; } break;
                                        case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { item.Cells[1].Value ="Delay (msec) during tray unit sorry test"; } break;

                                        case (int)OPTNUM.EQP_INIT_TIME: { item.Cells[1].Value = "Equipment Initialization Timeout (msec)"; } break;
                                        case (int)OPTNUM.CYL_MOVE_TIME: { item.Cells[1].Value = "Cylinder Operation Timeout (msec)"; } break;
                                        case (int)OPTNUM.VAC_CHECK_TIME: { item.Cells[1].Value = "Vacuum Sensor Check Timeout (msec)"; } break;
                                        case (int)OPTNUM.MOT_MOVE_TIME: { item.Cells[1].Value = "Motor Movement Timeout (msec)"; } break;
                                        case (int)OPTNUM.MES_REPLY_TIME: { item.Cells[1].Value = "MES Interface Response Timeout (msec)"; } break;

                                        case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { item.Cells[1].Value ="Applies offset for lower vision"; } break;
                                        case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { item.Cells[1].Value ="Check the VACUUM Sensor on Blow"; } break;
                                        case (int)OPTNUM.ITS_XOUT_USE: { item.Cells[1].Value ="ITS Eject XOUT as a result."; } break;
                                        case (int)OPTNUM.IN_LET_TABLE_UNUSED_MODE: { item.Cells[1].Value ="IN LET TABLE USE / SKIP"; } break;

                                        case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { item.Cells[1].Value ="Check the front and rear doors and EMO SW status of the facility."; } break;
                                        case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="Time to remind again if it is not cleared after the scrapbox alarm occurs (ms)"; } break;
                                        case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="Time to re-notify if it has not been cleared after the REJECT box alarm (ms)"; } break;
                                        case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { item.Cells[1].Value ="Event Log Archive"; } break;
                                        case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { item.Cells[1].Value ="alarm log retention period"; } break;
                                        case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { item.Cells[1].Value ="sequence log retention period"; } break;
                                        case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { item.Cells[1].Value ="Rat Log Archive"; } break;
                                        case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { item.Cells[1].Value ="ITS log retention period"; } break;
                                        case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { item.Cells[1].Value ="Production Quantity Log Archive"; } break;

                                    case (int)OPTNUM.GROUP1_USE: { item.Cells[1].Value = "GROUP1 USE"; } break; //sj.shin 2023-10-31 ADD
                                    case (int)OPTNUM.MAPVISIONDATA_USE: { item.Cells[1].Value = "MAP VISION DATA USE"; } break;  //sj.shin 2023-11-02 ADD
                                    case (int)OPTNUM.AIRKNIFE_RUN_START_ON_USE: { item.Cells[1].Value = "Use Turn On Air Knife when Auto Start"; }break;

                                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { item.Cells[1].Value ="ITS ID and STRIP ID compare front N digits option."; } break;
                                        case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { item.Cells[1].Value ="Common character length of ITS ID and STRIP ID"; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { item.Cells[1].Value ="Use alarm when lower vision matching fails. If not in use, it will be seated on the reuse tray."; } break;
                                        case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { item.Cells[1].Value ="lower vision interface retry count"; } break;//220615
                                        case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { item.Cells[1].Value = "VISION Interface Response Timeout (msec)"; } break;
                                        case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { item.Cells[1].Value = "SAW Interface Response Timeout (msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { item.Cells[1].Value = "before grab during load vision pre-align operation (msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { item.Cells[1].Value = "before grab during load vision barcode operation (msec)"; } break;
                                        case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { item.Cells[1].Value = "MAP VISION VISIONAL INSPECTION OPERATION DELAY (msec)"; } break;
                                        case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { item.Cells[1].Value = "before grab during ball vision inspection (msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { item.Cells[1].Value = "conveyor maximum operating time (msec)"; } break;
                                        case (int)OPTNUM.CONV_ROLLING_TIME: { item.Cells[1].Value = "time to turn the conveyor after detecting the conveyor arrival sensor (msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { item.Cells[1].Value = "Use conveyor maximum operating time (msec)"; } break;
                                        case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { item.Cells[1].Value = "Use conveyor inlet safety sensor check."; } break;


                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { item.Cells[1].Value = "before reading the strip lower barcode (msec)"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { item.Cells[1].Value = "strip lower barcode read retry count"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { item.Cells[1].Value = "STRIP BOTTOM_BARCODE_READ_TIMEOUT"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { item.Cells[1].Value = "Use Strip Lower Barcode Read"; } break;
                                        case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { item.Cells[1].Value = "Use the overload sensor on the strip pusher"; } break;
                                        // Loader Rail Top Magazine Sense Sensor Usage Mode
                                        case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { item.Cells[1].Value = "Rail Top Material Detection Sensor (A Contact)"; } break;
                                        case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { item.Cells[1].Value = "Pre-move to load rail Y unload position before pick-up of strip picker."; } break;

                                        case (int)OPTNUM.STRIP_PREALIGN_USE: { item.Cells[1].Value = "Use Strip Pre-align"; } break;
                                        case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { item.Cells[1].Value = "Strip Upper Barcode Read (read with PREALIGN camera"; } break;
                                        case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { item.Cells[1].Value = "Use Strip Orient Check"; } break;
                                        case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { item.Cells[1].Value = "Pre-align calibration angle offset on the strip"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { item.Cells[1].Value = "Strip Press Mode."; } break;
                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "Slow Down Height (mm) of Strip Picker"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "Slow Down Rate (mm/sec) of Strip Picker"; } break;
                                        case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { item.Cells[1].Value = "interlock distance between the strip picker and the unit picker (unit+strip mm)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "Slow Down Height (mm) of Unit Picker"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "Slow Down Rate (mm/sec) of Unit Picker"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { item.Cells[1].Value = "Use only position 1 for unit picker scrap"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { item.Cells[1].Value = "Vacuum waiting time after settling the dicing cow cutting table."; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "Slow Down Height (mm) of Map Picker"; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "MAPPICKER's slow-down rate (mm/sec)"; } break;
                                        case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { item.Cells[1].Value = "MAPPICKER air dry repeat count"; } break;
                                        case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { item.Cells[1].Value = "Quantity of alarm when map inspection fails"; } break;

                                        case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { item.Cells[1].Value = " vertical size(mm)) of map stage"; } break;
                                        case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { item.Cells[1].Value = "horizontal size (mm) of map stage"; } break;
                                        case (int)OPTNUM.MAP_STAGE_1_USE: {item.Cells[1].Value = "use map stage 1."; } break;
                                        case (int)OPTNUM.MAP_STAGE_2_USE: {item.Cells[1].Value = "use map stage 2."; } break;
                                        case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { item.Cells[1].Value = "Use T-axis pre-move after pickup"; } break; //220617
                                        case (int)OPTNUM.MULTI_PICKER_TTLOG_USE: { item.Cells[1].Value = "Use TTLOG"; } break; //220617
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { item.Cells[1].Value = "Delay time for multi-picker to check vacuum (msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { item.Cells[1].Value = "time for multi-picker to vacuum (msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { item.Cells[1].Value = "Time for multi-picker to wait for vacuum dig (msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { item.Cells[1].Value = "Time for multi-picker to wait (msec)) after moving to the pickup location"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { item.Cells[1].Value = "Time for multi-picker to wait after moving to the ball inspection position (msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { item.Cells[1].Value = "Time for multi-picker to wait (msec)) after moving to the place location"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { item.Cells[1].Value = "Number of times the multi-picker retries if pick-up fails"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { item.Cells[1].Value = "Delay time (msec) for multi-picker to check vacuum if pick-up fails"; } break;
                                        case (int)OPTNUM.BOTTOM_CAM_ANGLE: { item.Cells[1].Value = "Place time angle offset"; } break;

                                        case (int)OPTNUM.PICKER_1_USE: { item.Cells[1].Value = "Use multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_1_USE: { item.Cells[1].Value = "Use pad number 1 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_2_USE: { item.Cells[1].Value = "Use pad number 2 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_3_USE: { item.Cells[1].Value = "Use pad number 3 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_4_USE: { item.Cells[1].Value = "Use pad #4 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_5_USE: { item.Cells[1].Value = "Use pad no.5 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_6_USE: { item.Cells[1].Value = "Use pad #6 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_7_USE: { item.Cells[1].Value = "Use pad #7 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_8_USE: { item.Cells[1].Value = "Use pad #8 of multi-picker head 1"; } break;
                                        case (int)OPTNUM.PICKER_2_USE: { item.Cells[1].Value = "Use multi-picker head 2"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_1_USE: { item.Cells[1].Value = "Use pad number 1 of multi-picker head 2"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_2_USE: { item.Cells[1].Value = "Use pad number 2 of multi-picker head"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_3_USE: { item.Cells[1].Value = "Use pad #3 of multi-picker head #2)"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_4_USE: { item.Cells[1].Value = "Use pad #4 of multi-picker head #2)"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_5_USE: { item.Cells[1].Value = "Use pad no.5 of multi-picker head no.2"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_6_USE: { item.Cells[1].Value = "Use pad no.6 of multi-picker head no.2"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_7_USE: { item.Cells[1].Value = "Use pad no.7 of multi-picker head no.2"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_8_USE: { item.Cells[1].Value = "Use pad no.8 of multi-picker head no.2"; } break;
                                        case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { item.Cells[1].Value = "Use the ability to skip UNIT in the map table in case of a pickup failure."; } break;

                                        case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { item.Cells[1].Value = "TRAY STAGE UP/DOWN size(mm) for action and collision avoidance)"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "GOOD tray stage 1 CLAMP repeat count"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "GOOD Tray Stage 2 CLAMP Repeat Count"; } break;
                                        case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "REWORK Tray Stage CLAMP Repeat Count"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { item.Cells[1].Value = "Use GOOD Tray 1"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { item.Cells[1].Value = "Use GOOD Tray 2"; } break;

                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value =  "Slow-down height (mm) of tray picker"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value =  "Tray Picker's Slow Down Speed (mm/sec)"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { item.Cells[1].Value =  "use the tray picker's sense sensor."; } break;
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { item.Cells[1].Value =  "Flipping the tray is used."; } break;//220516
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { item.Cells[1].Value =  "Pitch in mm to be added to the picker Z axis when the process is reversed."; } break;//220516
                                        case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { item.Cells[1].Value =  "Waiting time for tray picker to move to the ready position (sec.)"; } break;

                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "Safe position offset where the good tray elevator (1) waits to receive the tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "Safe Position Offset where the good tray elevator (2) waits to receive the tray"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "Safe position offset waiting for rework tray elevator to receive tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { item.Cells[1].Value = "Position offset where the good tray elevator (1) is held up for receiving the tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { item.Cells[1].Value = "Position offset where the good tray elevator (2) is supported for receiving the tray"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { item.Cells[1].Value = "Offset of position where the rework tray elevator is supported for receiving the tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { item.Cells[1].Value = "Offset safe position before the pedestal is reached by the good tray elevator (1)"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { item.Cells[1].Value = "Default safe position before the pedestal is reached by the good tray elevator (2)"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "Offset safe position before rework tray elevator reaches pedestal"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "empty tray (1) offset to safe position before elevator reaches pedestal when fully loaded tray"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "empty tray (2) offset before elevator reaches pedestal"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { item.Cells[1].Value = "low speed to the pedestal when the good tray elevator (1) ejects the full tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { item.Cells[1].Value = "Low speed to the pedestal when the good tray elevator (2) ejects the loaded tray"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "low speed at which the rework tray elevator moves to the pedestal when it ejects the loaded tray"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "an empty rack tray is taken when draining the tray (1) The elevator is full load to travel to low speed and I"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "Low speed that moves to the pedestal when the elevator ejects the loaded tray"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "reference position where the good tray elevator (1) can determine whether a tray is present"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "reference position where the good tray elevator (2) can determine whether a tray is present"; } break;


                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "rework tray elevator can determine if a tray is present"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "the reference position where the empty tray (1) elevator can determine if the tray is present"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "empty tray (2) reference position where elevator can determine if tray exists"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "empty tray (1) offset for elevator to drain tray"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "empty tray (2) offset for elevator to drain tray"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "empty tray (1) safe position offset for elevator to reset the ready position"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "empty tray (2) safe position offset for elevator to reset the ready position"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { item.Cells[1].Value = "Good Tray Elevator (1) Full Emission Count"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { item.Cells[1].Value = "Good Tray Elevator (2) Full Emission Count"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { item.Cells[1].Value = "Number of rework tray elevator exhaust emissions"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "low speed at which the good tray elevator (1) moves offset"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "low speed at which the good tray elevator (2) moves offset"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { item.Cells[1].Value = "Low speed at which the rework tray elevator moves offset"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "low speed at which the empty tray elevator (1) moves offset"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "low speed with empty tray elevator (2) offset"; } break;
                                        case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { item.Cells[1].Value = "Time to wait after the elevator cover is closed"; } break;
                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "Slow Down Height (mm) of Cleaner Table"; } break;
                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "slowdown rate (mm/sec)) of the cleaner table"; } break;
                                        case (int)OPTNUM.SCRAP_MODE_USE: { item.Cells[1].Value = "use scrap mode."; } break;
                                        case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { item.Cells[1].Value = "Cleaner Analysis Log option. (test)"; } break;
                                        case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { item.Cells[1].Value = "Cleaner cylinder delay time (msec)"; } break;
                                    }
                                break;

                            case Language.CHINA:
                                    switch (nIdx)
                                    {
                                        case (int)OPTNUM.GEM_USE: { item.Cells[1].Value ="请使用GEM"; } break;
                                        case (int)OPTNUM.VAC_SENSOR_USE: { item.Cells[1].Value ="请确认真空感应器"; } break;
                                        case (int)OPTNUM.DRY_RUN_MODE: { item.Cells[1].Value ="请使用干燥模式"; } break;
                                        case (int)OPTNUM.DRY_RUN_SAW_USE: { item.Cells[1].Value = "在干燥开始模式时 与切割机一起使用界面."; } break;
                                        case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { item.Cells[1].Value ="干燥开始模式时素材任何位置可排除"; } break;
                                        case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { item.Cells[1].Value ="干燥开始模式时 与视觉检测进行连接"; } break;

                                        case (int)OPTNUM.TOP_VISION_USE: { item.Cells[1].Value ="使用上部视觉检测"; } break;
                                        case (int)OPTNUM.TOP_ALIGN_USE: { item.Cells[1].Value ="使用 上部对齐功能"; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_USE: { item.Cells[1].Value ="使用下部视觉检测"; } break;
                                        case (int)OPTNUM.NO_MSG_STOP_TIME: { item.Cells[1].Value ="设备停止确认时间（单位：分）"; } break;
                                        case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { item.Cells[1].Value ="料盒 排出感应时间设定 (单位 :ms)"; } break;
                                        case (int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE: { item.Cells[1].Value = "在读取视力数据前使用清晰功能"; } break;

                                        case (int)OPTNUM.TRAY_INSPECTION_USE: { item.Cells[1].Value = "使用萃盘单元未放置检查"; } break;
                                        case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { item.Cells[1].Value = "萃盘单元未放置检查时延迟(msec)"; } break;
                                        case (int)OPTNUM.IN_LET_TABLE_UNUSED_MODE: { item.Cells[1].Value = "支撑台 使用/跳过"; } break;

                                        case (int)OPTNUM.EQP_INIT_TIME: { item.Cells[1].Value = "设备初始化 限制时间(msec)"; } break;
                                        case (int)OPTNUM.CYL_MOVE_TIME: { item.Cells[1].Value = "气缸动作制约时间(msec)"; } break;
                                        case (int)OPTNUM.VAC_CHECK_TIME: { item.Cells[1].Value = "真空感应确认限制时间(msec)"; } break;
                                        case (int)OPTNUM.MOT_MOVE_TIME: { item.Cells[1].Value = "马达移动限制时间(msec)"; } break;
                                        case (int)OPTNUM.MES_REPLY_TIME: { item.Cells[1].Value = "MES 连接时 回应限制时间(msec)"; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { item.Cells[1].Value = "下方视觉检测相机适用抵销"; } break;
                                        case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { item.Cells[1].Value ="吹气时 确认真空感应"; } break;
                                        case (int)OPTNUM.ITS_XOUT_USE: { item.Cells[1].Value ="用ITS结果 进行XOUT排出"; } break;

                                        case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { item.Cells[1].Value ="设备 前后门及EMO SW 状态确认"; } break;
                                        case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="废料箱报警后不能清洗时限制的时间 (ms)"; } break;
                                        case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="报废箱报警后不能清洗时限制的时间 (ms)"; } break;

                                        case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { item.Cells[1].Value = "项目对数保存时效"; } break;
                                        case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { item.Cells[1].Value ="报警记录保存时效"; } break;
                                        case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { item.Cells[1].Value ="日记记录保存时效"; } break;
                                        case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { item.Cells[1].Value ="批次记录保存时效"; } break;
                                        case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { item.Cells[1].Value ="ITS 记录保存时效"; } break;
                                        case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { item.Cells[1].Value ="生产数量记录保存时效"; } break;

                                    case (int)OPTNUM.GROUP1_USE: { item.Cells[1].Value = "请使用GROUP1"; } break; //sj.shin 2023-10-31 ADD
                                    case (int)OPTNUM.MAPVISIONDATA_USE: { item.Cells[1].Value = "请使用 MAP VISION DATA "; } break; //sj.shin 2023-11-02 ADD
                                    case (int)OPTNUM.AIRKNIFE_RUN_START_ON_USE: { item.Cells[1].Value = "设备运行时开启吹气"; } break;

                                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { item.Cells[1].Value ="默认ITS ID和 基板ID前 位置 进行对比"; } break;
                                        case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { item.Cells[1].Value ="ITS ID和 基板ID共同字符长度"; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { item.Cells[1].Value ="下方视觉检测匹配失败时报警,未使用时 放置在再使用萃盘中"; } break;
                                        case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { item.Cells[1].Value ="下方视觉检测连接指示次数"; } break;//220615
                                        case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { item.Cells[1].Value = "视觉检测连接回应等待时间(msec)"; } break;
                                        case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { item.Cells[1].Value = "SAW 连接回应等待时间(msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { item.Cells[1].Value = "入料检测对齐运行时，抓图延迟时间(msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { item.Cells[1].Value = "入料二维码检测启用时，推料延迟(msec)"; } break;
                                        case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { item.Cells[1].Value = "MAP 视觉检测运行时抓图延迟时间(msec)"; } break;
                                        case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { item.Cells[1].Value = "球图形检测运行时，抓图延迟(msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { item.Cells[1].Value = "传送带最长运行时间(msec)"; } break;
                                        case (int)OPTNUM.CONV_ROLLING_TIME: { item.Cells[1].Value = "传送带到达感应区位置后，传送带继续转动时间(msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { item.Cells[1].Value = "使用传送带最长运行时间(msec)."; } break;
                                        case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { item.Cells[1].Value = "使用确认传送带入料口安全位感应"; } break;


                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { item.Cells[1].Value = "基板 下方条形码识别前延迟(msec)"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { item.Cells[1].Value = "基板下方 条形码再指示识别次数"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { item.Cells[1].Value = "基板 下方 条形码成功识别等待时间(msec)"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { item.Cells[1].Value = "选择使用基板下方 条形码进行识别"; } break;
                                        case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { item.Cells[1].Value = "使用基板推进器 过载感应"; } break;
                                        case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { item.Cells[1].Value = "矫正导轨上方素材感应（A接点）"; } break;
                                        case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { item.Cells[1].Value = "基板 吸取产品前，矫正导轨 提前移动到 Y轴出料位置"; } break;

                                        case (int)OPTNUM.STRIP_PREALIGN_USE: { item.Cells[1].Value = "使用基板校直功能"; } break;
                                        case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { item.Cells[1].Value = "使用基板上方 条形码识别(预对齐 相机来识别)"; } break;
                                        case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { item.Cells[1].Value = "使用基板方向检查"; } break;
                                        case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { item.Cells[1].Value = "基板 校直 补正角度设置"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { item.Cells[1].Value = "使用基板下压模式"; } break;

                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "基板_手臂 缓慢下降高度(mm)"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "基板_手臂 缓慢下降速度(mm/sec)"; } break;
                                        case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { item.Cells[1].Value = "搬运(基板)吸盘与 手臂（单元）吸盘 接触距离(基板+单元 mm)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "手臂(单元)吸盘 缓慢下降高度(mm)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "手臂(单元)吸盘 缓慢下降速度(mm/sec)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { item.Cells[1].Value = "手臂(单元)吸盘 废料移除时，只使用1号位置"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { item.Cells[1].Value = "切割机 放置到切割台面后，真空等待时间."; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "检查手臂吸盘 缓慢下降高度(mm)"; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "检查手臂吸盘缓慢下降速度(mm/sec)"; } break;
                                        case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { item.Cells[1].Value = "MAP_PK吸盘干燥反复次数"; } break;
                                        case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { item.Cells[1].Value = "MAP_PK 选取失败时发生警报数量"; } break;

                                        case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { item.Cells[1].Value = "检查 台面 竖向尺寸(mm)"; } break;
                                        case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { item.Cells[1].Value = "检查 台面 横向尺寸(mm)"; } break;
                                        case (int)OPTNUM.MAP_STAGE_1_USE: { item.Cells[1].Value = "使用 1号检查 台面"; } break;
                                        case (int)OPTNUM.MAP_STAGE_2_USE: { item.Cells[1].Value = "使用 2号检查 台面"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { item.Cells[1].Value = "使用吸附后  提前让T轴移动"; } break; //220617
                                        case (int)OPTNUM.MULTI_PICKER_TTLOG_USE: { item.Cells[1].Value = "使用TTLOG"; } break; 
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { item.Cells[1].Value = "分拣手臂 确认真空 延迟时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { item.Cells[1].Value = "分拣手臂 开真空延迟时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { item.Cells[1].Value = "分拣手臂 关真空后等待时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { item.Cells[1].Value = "分拣手臂 移动到吸附位置后等待时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { item.Cells[1].Value = "分拣手臂 移动到球形检测位置后 等待时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { item.Cells[1].Value = "分拣手臂 移动到指定下压位后 等待时间(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { item.Cells[1].Value = "分拣手臂 吸附失败时重新开启次数"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { item.Cells[1].Value = "分拣手臂 吸附失败时 确认真空延迟时间(msec)"; } break;
                                        case (int)OPTNUM.BOTTOM_CAM_ANGLE: { item.Cells[1].Value = "下压时 角度选择"; } break;

                                        case (int)OPTNUM.PICKER_1_USE: { item.Cells[1].Value = "使用分拣手臂 1号"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_1_USE: { item.Cells[1].Value = "使用分拣手臂 1中 1号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_2_USE: { item.Cells[1].Value = "使用分拣手臂 1中 2号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_3_USE: { item.Cells[1].Value = "使用分拣手臂 1中 3号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_4_USE: { item.Cells[1].Value = "使用分拣手臂 1中 4号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_5_USE: { item.Cells[1].Value = "使用分拣手臂 1中 5号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_6_USE: { item.Cells[1].Value = "使用分拣手臂 1中 6号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_7_USE: { item.Cells[1].Value = "使用分拣手臂 1中 7号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_8_USE: { item.Cells[1].Value = "使用分拣手臂 1中 8号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_USE: { item.Cells[1].Value = "使用分拣手臂 2号"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_1_USE: { item.Cells[1].Value = "使用分拣手臂 2中 1号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_2_USE: { item.Cells[1].Value = "使用分拣手臂 2中 2号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_3_USE: { item.Cells[1].Value = "使用分拣手臂 2中 3号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_4_USE: { item.Cells[1].Value = "使用分拣手臂 2中 4号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_5_USE: { item.Cells[1].Value = "使用分拣手臂 2中 5号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_6_USE: { item.Cells[1].Value = "使用分拣手臂 2中 6号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_7_USE: { item.Cells[1].Value = "使用分拣手臂 2中 7号吸嘴"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_8_USE: { item.Cells[1].Value = "使用分拣手臂 2中 8号吸嘴"; } break;
                                        case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { item.Cells[1].Value = "吸附失败时，检查台面的 单元进行隐藏"; } break;

                                        case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { item.Cells[1].Value = "为了托盘台面 上 / 下 动作及防止冲突而定的台面尺寸(mm)"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "良品1台面 夹子反复使用次数"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "良品2台面 夹子反复使用次数"; } break;
                                        case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "返工 台面 夹子反复使用次数"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { item.Cells[1].Value = "使用良品1中的托盘"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { item.Cells[1].Value = "使用良品2中的托盘"; } break;

                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value =  "托盘手臂 吸附时 缓慢下降高度(mm)"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value =  "托盘手臂 缓慢下降速度(mm/sec)"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { item.Cells[1].Value =  "使用托盘手臂感应器"; } break;
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { item.Cells[1].Value =  "托盘翻转后使用"; } break;//220516
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { item.Cells[1].Value =  "托盘翻转后继续后工序时 Z轴增加幅度(mm)."; } break;//220516
                                        case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { item.Cells[1].Value =  "托盘搬运手臂移动到准备位置时的等待时间(sec)."; } break;

                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "良品1托盘升降板 为了接受托盘 等待安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "良品2托盘升降板 为了接受托盘 等待安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "待检查托盘升降板 为了接受托盘 等待安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { item.Cells[1].Value = "良品1升降板 为了接受托盘，支撑位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { item.Cells[1].Value = "良品1升降板 为了接受托盘，支撑位置补偿"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { item.Cells[1].Value = "待检查升降板 为了接受托盘，支撑位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { item.Cells[1].Value = "良品(1)升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { item.Cells[1].Value = "良品(2)升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "待检查升降板将满载后的托盘移出时 支撑台面到达安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "空托盘(1)升降板将满载后托盘移出时 支撑台面到达安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "空托盘(2)升降板将满载后托盘移出时 支撑台面到达安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { item.Cells[1].Value = "良品(1)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { item.Cells[1].Value = "良品(2)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "待检查升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "空托盘(1)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "空托盘(2)升降板将满载后的托盘移出时,移动到支撑台面为止的低速度"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "良品(1) 升降板 确认有无托盘的判定基准位"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "良品(2) 升降板 确认有无托盘的判定基准位"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "待检查升降板 确认有无托盘的判定基准位"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "空托盘(1) 升降板 确认有无托盘的判定基准位"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "空托盘(2) 升降板 确认有无托盘的判定基准位"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "空托盘(1) 升降板 移出托盘时 基准位补偿"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "空托盘(2) 升降板 移出托盘时 基准位补偿"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "空托盘(1) 升降板 为了重新设定准备位置时所需的安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "空托盘(2) 升降板 为了重新设定准备位置时所需的安全位置补偿"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { item.Cells[1].Value = "良品(1)升降板 满载时 可排出的个数"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { item.Cells[1].Value = "良品(2)升降板 满载时 可排出的个数"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { item.Cells[1].Value = "待检查升降板 满载时 可排出的个数"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "良品(1)升降板 补偿位移动时的低速度 "; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "良品(2)升降板 补偿位移动时的低速度"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { item.Cells[1].Value = "待检查升降板 补偿位移动时的低速度"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "空托盘(1)升降板 补偿位移动时的低速度"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "空托盘(2)升降板 补偿位移动时的低速度"; } break;
                                        case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { item.Cells[1].Value = "升降板 门关闭后 等待时间"; } break;
                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "清洗台面 缓慢下降高度(mm)"; } break;
                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "清洗台面 缓慢下降速度(mm/sec)"; } break;
                                        case (int)OPTNUM.SCRAP_MODE_USE: { item.Cells[1].Value = "启用废料模式"; } break;
                                        case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { item.Cells[1].Value = "默认使用清洗 分析记录(测试中)"; } break;
                                        case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { item.Cells[1].Value = "清洗气缸延迟时间(msec)"; } break;
                                    }
                                break;
                            case Language.KOREAN:
                                    switch (nIdx)
                                    {
                                        case (int)OPTNUM.GEM_USE: { item.Cells[1].Value ="GEM을 사용합니다"; } break;
                                        case (int)OPTNUM.VAC_SENSOR_USE: { item.Cells[1].Value ="배큠 센서를 확인합니다"; } break;
                                        case (int)OPTNUM.DRY_RUN_MODE: { item.Cells[1].Value ="드라이 런 모드를 사용합니다"; } break;
                                        case (int)OPTNUM.DRY_RUN_SAW_USE: { item.Cells[1].Value ="드라이 런 모드일때 SAW와 인터페이스를 사용합니다."; } break;
                                        case (int)OPTNUM.DRY_RUN_RANDOM_SORT: { item.Cells[1].Value ="드라이 런 모드일때 자재를 무작위로 배출합니다"; } break;
                                        case (int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE: { item.Cells[1].Value ="드라이 런 모드일때 비젼과 인터페이스를 사용합니다."; } break;

                                        case (int)OPTNUM.TOP_VISION_USE: { item.Cells[1].Value ="상부 비전을 사용합니다"; } break;
                                        case (int)OPTNUM.TOP_ALIGN_USE: { item.Cells[1].Value ="상부 얼라인을 사용합니다."; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_USE: { item.Cells[1].Value ="하부 비전을 사용합니다"; } break;
                                        case (int)OPTNUM.NO_MSG_STOP_TIME: { item.Cells[1].Value ="설비 무언 정지 판단 시간 (단위 : 분)"; } break;
                                        case (int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME: { item.Cells[1].Value ="매거진 배출 센서 감지 시간 설정 (단위 : 밀리초)"; } break;

                                        case (int)OPTNUM.TRAY_INSPECTION_USE: { item.Cells[1].Value ="트레이 유닛 미안착 검사를 사용합니다"; } break;
                                        case (int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY: { item.Cells[1].Value ="트레이 유닛 미안착 검사 시 딜레이(msec)"; } break;
                                        case (int)OPTNUM.IN_LET_TABLE_UNUSED_MODE: { item.Cells[1].Value = "인렛테이블을 사용합니다."; } break;

                                        case (int)OPTNUM.EQP_INIT_TIME: { item.Cells[1].Value = "장비 초기화 제한시간(msec)"; } break;
                                        case (int)OPTNUM.CYL_MOVE_TIME: { item.Cells[1].Value = "실린더 동작 제한시간(msec)"; } break;
                                        case (int)OPTNUM.VAC_CHECK_TIME: { item.Cells[1].Value = "배큠 센서 확인 제한시간(msec)"; } break;
                                        case (int)OPTNUM.MOT_MOVE_TIME: { item.Cells[1].Value = "모터 이동 제한시간(msec)"; } break;
                                        case (int)OPTNUM.MES_REPLY_TIME: { item.Cells[1].Value = "MES 인터페이스 응답 제한시간(msec)"; } break;

                                        case (int)OPTNUM.BOTTOM_VISION_OFFSET_USE: { item.Cells[1].Value ="하부 비전 시 오프셋을 적용합니다"; } break;
                                        case (int)OPTNUM.BLOW_VAC_SENSOR_USE: { item.Cells[1].Value ="블로우 시 배큠 센서를 확인합니다"; } break;
                                        case (int)OPTNUM.ITS_XOUT_USE: { item.Cells[1].Value ="ITS결과로 XOUT을 배출합니다."; } break;

                                        case (int)OPTNUM.DOOR_SAFETY_CHECK_USE: { item.Cells[1].Value ="설비 전후면 도어 및 EMO SW 상태를 체크 합니다."; } break;
                                        case (int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="스크랩 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"; } break;
                                        case (int)OPTNUM.REJECT_BOX_ALARM_CLEAR_TIME_OUT: { item.Cells[1].Value ="리젝 박스 알람 발생 후 클리어 안되었을 시 재알림하는 시간 (ms)"; } break;

                                        case (int)OPTNUM.EVENT_LOG_EXP_PERIOD: { item.Cells[1].Value ="이벤트 로그 보관 기간"; } break;
                                        case (int)OPTNUM.ALARM_LOG_EXP_PERIOD: { item.Cells[1].Value ="알람 로그 보관 기간"; } break;
                                        case (int)OPTNUM.SEQ_LOG_EXP_PERIOD: { item.Cells[1].Value ="시퀀스 로그 보관 기간"; } break;
                                        case (int)OPTNUM.LOT_LOG_EXP_PERIOD: { item.Cells[1].Value ="랏 로그 보관 기간"; } break;
                                        case (int)OPTNUM.ITS_LOG_EXP_PERIOD: { item.Cells[1].Value ="ITS 로그 보관 기간"; } break;
                                        case (int)OPTNUM.PROC_QTY_LOG_EXP_PERIOD: { item.Cells[1].Value ="생산 수량 로그 보관 기간"; } break;

                                    case (int)OPTNUM.GROUP1_USE: { item.Cells[1].Value = "GROUP1 USE"; } break; //sj.shin 2023-10-31 ADD
                                    case (int)OPTNUM.MAPVISIONDATA_USE: { item.Cells[1].Value = "MAP VISION DATA USE"; } break;  //sj.shin 2023-11-02 ADD
                                    case (int)OPTNUM.AIRKNIFE_RUN_START_ON_USE: { item.Cells[1].Value = "설비 시작시 에어 나이프 사용합니다."; } break;

                                    case (int)OPTNUM.ITS_ID_STRIP_ID_VALID_USE: { item.Cells[1].Value ="ITS ID와 STRIP ID의 앞 N자리 비교 옵션을 사용합니다."; } break;
                                        case (int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN: { item.Cells[1].Value ="ITS ID와 STRIP ID의 공통 문자 길이"; } break;
                                        case (int)OPTNUM.BOTTOM_VISION_ALARM_USE: { item.Cells[1].Value ="하부 비전 매칭 실패 시 알람을 사용합니다. 미사용 시 재사용 트레이에 안착합니다."; } break;
                                        case (int)OPTNUM.VISION_INTERFACE_RETRY_COUNT: { item.Cells[1].Value ="하부 비전 인터페이스 재시도 횟수"; } break;//220615
                                        case (int)OPTNUM.VISION_INTERFACE_TIMEOUT: { item.Cells[1].Value = "VISION 인터페이스 응답 제한시간(msec)"; } break;
                                        case (int)OPTNUM.SAW_INTERFACE_TIMEOUT: { item.Cells[1].Value = "SAW 인터페이스 응답 제한시간(msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY: { item.Cells[1].Value = "로드 비전 프리얼라인 동작시 그랩 전 딜레이(msec)"; } break;
                                        case (int)OPTNUM.LD_VISION_BARCODE_GRAB_DELAY: { item.Cells[1].Value = "로드 비전 바코드 동작시 그랩 전 딜레이(msec)"; } break;
                                        case (int)OPTNUM.MAP_VISION_GRAB_DELAY: { item.Cells[1].Value = "맵 비전 비전검사 동작시 그랩 전 딜레이(msec)"; } break;
                                        case (int)OPTNUM.BALL_VISION_GRAB_DELAY: { item.Cells[1].Value = "볼 비전 비전검사 동작시 그랩 전 딜레이(msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT: { item.Cells[1].Value = "컨베이어 최대 동작시간(msec)"; } break;
                                        case (int)OPTNUM.CONV_ROLLING_TIME: { item.Cells[1].Value = "컨베이어 도착 센서감지 후 컨베이어 돌리는 시간(msec)"; } break;
                                        case (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE: { item.Cells[1].Value = "컨베이어 최대 동작시간(msec)을 사용 합니다."; } break;
                                        case (int)OPTNUM.CONV_SAFETY_SENSOR_USE: { item.Cells[1].Value = "컨베이어 투입구 안전 센서 체크를 사용합니다."; } break;


                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY: { item.Cells[1].Value = "스트립 하부 바코드 읽기 전 딜레이(msec)"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT: { item.Cells[1].Value = "스트립 하부 바코드 읽기 재시도 횟수"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT: { item.Cells[1].Value = "스트립 하부 바코드 읽기 성공 제한시간(msec)"; } break;
                                        case (int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE: { item.Cells[1].Value = "스트립 하부 바코드 읽기를 사용합니다"; } break;
                                        case (int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK: { item.Cells[1].Value = "스트립 푸셔의 오버로드 센서를 사용합니다"; } break;
                                        // 로더 레일 상단 매거진 감지 센서 사용 모드
                                        case (int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE: { item.Cells[1].Value = "레일 상단 자재 감지 센서(A접점) 를 사용합니다."; } break;
                                        case (int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE: { item.Cells[1].Value = "스트립 피커 픽업 전 로드 레일 Y 언로드 위치로 미리 이동기능을 사용합니다."; } break;

                                        case (int)OPTNUM.STRIP_PREALIGN_USE: { item.Cells[1].Value = "스트립 프리얼라인을 사용합니다"; } break;
                                        case (int)OPTNUM.STRIP_VISION_BARCODE_READ_USE: { item.Cells[1].Value = "스트립 상부 바코드 읽기를 사용합니다(PRE ALIGN 카메라로 읽음)"; } break;
                                        case (int)OPTNUM.STRIP_ORIENT_CHECK_USE: { item.Cells[1].Value = "스트립 오리엔트 검사를 사용합니다"; } break;
                                        case (int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE: { item.Cells[1].Value = "스트립의 프리얼라인 보정 각도 오프셋"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE: { item.Cells[1].Value = "스트립 프레스 모드를 사용합니다."; } break;

                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "스트립 피커의 슬로우 다운 높이(mm)"; } break;
                                        case (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "스트립 피커의 슬로우 다운 속도(mm/sec)"; } break;
                                        case (int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP: { item.Cells[1].Value = "스트립 피커와 유닛 피커 사이의 인터락 거리(유닛+스트립 mm)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "유닛 피커의 슬로우 다운 높이(mm)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "유닛 피커의 슬로우 다운 속도(mm/sec)"; } break;
                                        case (int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT: { item.Cells[1].Value = "유닛 피커 스크랩 시 1번 위치만 사용합니다."; } break;
                                        case (int)OPTNUM.UNIT_PICKER_VACUUM_DELAY: { item.Cells[1].Value = "다이싱 소우 커팅 테이블 안착 후 진공 대기 시간."; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "맵 피커의 슬로우 다운 높이(mm)"; } break;
                                        case (int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "맵 피커의 슬로우 다운 속도(mm/sec)"; } break;
                                        case (int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT: { item.Cells[1].Value = "맵 피커 에어 드라이 반복 횟수 "; } break;
                                        case (int)OPTNUM.MAP_INSPECTION_CHECK_COUNT: { item.Cells[1].Value = "맵 인스펙션 실패 시 알람 발생 수량"; } break;

                                        case (int)OPTNUM.MAP_STAGE_SIZE_HEIGHT: { item.Cells[1].Value = "맵 스테이지의 세로 사이즈(mm)"; } break;
                                        case (int)OPTNUM.MAP_STAGE_SIZE_WIDTH: { item.Cells[1].Value = "맵 스테이지의 가로 사이즈(mm)"; } break;
                                        case (int)OPTNUM.MAP_STAGE_1_USE: { item.Cells[1].Value = "맵스테이지 1번을 사용합니다."; } break;
                                        case (int)OPTNUM.MAP_STAGE_2_USE: { item.Cells[1].Value = "맵스테이지 2번을 사용합니다."; } break;
                                        case (int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE: { item.Cells[1].Value = "픽업 후 T축 미리 이동을 사용합니다"; } break; //220617
                                        case (int)OPTNUM.MULTI_PICKER_TTLOG_USE: { item.Cells[1].Value = "TTLOG을 사용합니다"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY: { item.Cells[1].Value = "멀티 피커가 진공을 확인하는 지연시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY: { item.Cells[1].Value = "멀티 피커가 진공을 파기하는 시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY: { item.Cells[1].Value = "멀티 피커가 진공 파기를 끄고 기다리는 시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY: { item.Cells[1].Value = "멀티 피커가 픽업 위치에 이동 후 기다리는 시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY: { item.Cells[1].Value = "멀티 피커가 볼 검사 위치에 이동 후 기다리는 시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY: { item.Cells[1].Value = "멀티 피커가 플레이스 위치에 이동 후 기다리는 시간(msec)"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY: { item.Cells[1].Value = "멀티 피커가 픽업 실패 시 재시도하는 횟수"; } break;
                                        case (int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY: { item.Cells[1].Value = "멀티 피커가 픽업 실패 시 진공을 확인하는 지연시간(msec)"; } break;
                                        case (int)OPTNUM.BOTTOM_CAM_ANGLE: { item.Cells[1].Value = "플레이스 시 각도 오프셋"; } break;

                                        case (int)OPTNUM.PICKER_1_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_1_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 1번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_2_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 2번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_3_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 3번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_4_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 4번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_5_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 5번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_6_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 6번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_7_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 7번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_1_PAD_8_USE: { item.Cells[1].Value = "멀티 피커 헤드 1번의 패드 8번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_1_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 1번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_2_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 2번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_3_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 3번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_4_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 4번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_5_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 5번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_6_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 6번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_7_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 7번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKER_2_PAD_8_USE: { item.Cells[1].Value = "멀티 피커 헤드 2번의 패드 8번을 사용합니다"; } break;
                                        case (int)OPTNUM.PICKUP_UNIT_SKIP_USAGE: { item.Cells[1].Value = "픽업 실패 시 맵테이블의 UNIT을 스킵하는 기능을 사용합니다."; } break;

                                        case (int)OPTNUM.GOOD_TRAY_STAGE_SIZE: { item.Cells[1].Value = "트레이 스테이지 UP / DOWN 동작 및 충돌 방지를 위한 스테이지 사이즈(mm)"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "GOOD 트레이 스테이지 1 CLAMP 반복 횟수"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "GOOD 트레이 스테이지 2 CLAMP 반복 횟수"; } break;
                                        case (int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT: { item.Cells[1].Value = "REWORK 트레이 스테이지 CLAMP 반복 횟수"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_1_USE: { item.Cells[1].Value = "GOOD 트레이 1을 사용합니다"; } break;
                                        case (int)OPTNUM.GOOD_TRAY_STAGE_2_USE: { item.Cells[1].Value = "GOOD 트레이 2를 사용합니다"; } break;

                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value =  "트레이 피커의 슬로우 다운 높이(mm)"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value =  "트레이 피커의 슬로우 다운 속도(mm/sec)"; } break;
                                        case (int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE: { item.Cells[1].Value =  "트레이 피커의 감지 센서를 사용합니다."; } break;
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE: { item.Cells[1].Value =  "트레이를 뒤집어서 사용합니다."; } break;//220516
                                        case (int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH: { item.Cells[1].Value =  "트레이를 뒤집어서 공정을 진행할 시 피커 Z축에 더해지는 피치(mm)."; } break;//220516
                                        case (int)OPTNUM.TRAY_PICKER_MOVE_READY_WAIT_TIME: { item.Cells[1].Value =  "트레이 피커가 준비 위치로 이동하는 대기 시간(sec)."; } break;

                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 트레이를 받기위해 대기하는 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 트레이를 받기위해 대기하는 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_WORK_SAFETY_OFFSET: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 트레이를 받기위해 대기하는 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 트레이를 받기위해 떠받히는 위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_GETTING_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 트레이를 받기위해 떠받히는 위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_GETTING_OFFSET: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 트레이를 받기위해 떠받히는 위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_OFFSET: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_OFFSET: { item.Cells[1].Value = "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대 도달 전 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_SAFETY_MOVE_VEL: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "빈 트레이(1) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_SAFETY_MOVE_VEL: { item.Cells[1].Value = "빈 트레이(2) 엘리베이터가 만재 트레이를 배출할 때 받침대까지 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 트레이 유무 판단할 수 있는 기준위치"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 트레이 유무 판단할 수 있는 기준위치"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "빈 트레이(1) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT: { item.Cells[1].Value = "빈 트레이(2) 엘리베이터가 트레이 유무 판단할 수 있는 기준위치"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "빈 트레이(1) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_WORKING_WAIT_OFFSET: { item.Cells[1].Value = "빈 트레이(2) 엘리베이터가 트레이를 배출 할 수 있는 준비위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "빈 트레이(1) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_EMPTY2_TRAY_ELV_RESEARCH_OFFSET: { item.Cells[1].Value = "빈 트레이(2) 엘리베이터가 준비위치를 다시 설정 하기 위한 안전위치 오프셋"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT: { item.Cells[1].Value = "양품 트레이 엘리베이터(1) 만재 배출 개수"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OUT_MAX_COUNT: { item.Cells[1].Value = "양품 트레이 엘리베이터(2) 만재 배출 개수"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT: { item.Cells[1].Value = "재작업 트레이 엘리베이터 만재 배출 개수"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "양품 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_GD_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "양품 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_RW_TRAY_ELV_OFFSET_MOVE_VEL: { item.Cells[1].Value = "재작업 트레이 엘리베이터가 오프셋 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_1_OFFSET_MOVE_VEL: { item.Cells[1].Value = "빈 트레이 엘리베이터(1)가 오프셋 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_EMPTY_TRAY_ELV_2_OFFSET_MOVE_VEL: { item.Cells[1].Value = "빈 트레이 엘리베이터(2)가 오프셋 이동하는 저속 스피드"; } break;
                                        case (int)OPTNUM.ULD_ELV_COVER_WAIT_TIME: { item.Cells[1].Value = "엘리베이터 커버가 닫히고 나서 기다리는 시간"; } break;

                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET: { item.Cells[1].Value = "클리너 테이블의 슬로우 다운 높이(mm)"; } break;
                                        case (int)OPTNUM.CLEANER_Z_SLOW_DOWN_VEL: { item.Cells[1].Value = "클리너 테이블의 슬로우 다운 속도(mm/sec)"; } break;
                                        case (int)OPTNUM.SCRAP_MODE_USE: { item.Cells[1].Value = "스크랩 모드를 사용합니다."; } break;
                                        case (int)OPTNUM.CLEANER_LOG_ANALYSIS: { item.Cells[1].Value = "클리너 분석 로그 옵션을 사용합니다. (테스트)"; } break;
                                        case (int)OPTNUM.CLEAN_CYLINDER_DELAY: { item.Cells[1].Value = "클리너 실린더 딜레이 시간(msec)"; } break;
                                    }
                                break;
                            default:
                                break;
                        }
                    }
                }
                //item.Cells[1].Value = FormTextLangMgr.FindKey(item.Cells[1].Value.ToString());
            }
        }

        private void dgvOption_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            List<int> ArrIdx = new List<int>();

            if (dgv == dgvCommon) ArrIdx = m_dgvCommonItemIdx;
            else if (dgv == dgvMachine) ArrIdx = m_dgvMachineItemIdx;
            else if (dgv == dgvLoader) ArrIdx = m_dgvLoaderItemIdx;
            else if (dgv == dgvSaw) ArrIdx = m_dgvSawItemIdx;
            else if (dgv == dgvSorter) ArrIdx = m_dgvSorterItemIdx;

            if (e.ColumnIndex >= 3 && e.RowIndex >= 0 && (e.PaintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
            {
                bool bIsUse = false;
                int nItemIdx = ArrIdx[e.RowIndex];
                bIsUse = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].bOptionUse;
                if (!ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strOptionName.Contains("사용") && !ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strOptionName.Contains("카운트") && !ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strOptionPart.Contains("OPERATION"))
                {
                    return;
                }
                Font font = new Font("맑은 고딕", 11, FontStyle.Bold);

                if (bIsUse)
                {
                    using (Brush gridBrush = new SolidBrush(dgv.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.Gold))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                                e.Graphics.DrawString("USE", font, Brushes.Black, new PointF(e.CellBounds.Location.X + 30, e.CellBounds.Location.Y + 3));
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                else
                {
                    using (Brush gridBrush = new SolidBrush(dgv.GridColor))
                    {
                        using (Brush backColorBrush = new SolidBrush(Color.FromArgb(200, 200, 200)))
                        {
                            using (Pen gridLinePen = new Pen(gridBrush))
                            {
                                // Clear cell
                                e.Graphics.FillRectangle(backColorBrush, e.CellBounds);

                                e.Graphics.DrawString("SKIP", font, Brushes.Black, new PointF(e.CellBounds.Location.X + 28, e.CellBounds.Location.Y + 3));
                                //force paint of content
                                e.PaintContent(e.ClipBounds);
                                e.Handled = true;
                            }
                        }
                    }
                }
                DataGridViewPaintParts paintParts = e.PaintParts & ~DataGridViewPaintParts.Background;
                e.Paint(e.ClipBounds, paintParts);
                e.Handled = true;
            }
        }

        private void dgvOption_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            List<int> ArrIdx = new List<int>();
            if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP) return;

            if (e.RowIndex < 0) return;

            if (dgv == dgvCommon) ArrIdx = m_dgvCommonItemIdx;
            else if (dgv == dgvMachine) ArrIdx = m_dgvMachineItemIdx;
            else if (dgv == dgvLoader) ArrIdx = m_dgvLoaderItemIdx;
            else if (dgv == dgvSaw) ArrIdx = m_dgvSawItemIdx;
            else if (dgv == dgvSorter) ArrIdx = m_dgvSorterItemIdx;

            int nItemIdx = ArrIdx[e.RowIndex];

            if (e.ColumnIndex == 2)
            {
                dgv.ReadOnly = false;
            }
            else
            {
                dgv.ReadOnly = true;
            }

            if (dgv.Rows[e.RowIndex].Cells[1].Value.ToString().Contains("사용"))
            {
                dgv.ReadOnly = true;
            }

            if (e.ColumnIndex != 3) return;
            if (e.RowIndex < 0) return;

            int nIndex = e.RowIndex;

            ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].bOptionUse =
                !ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].bOptionUse;
        }

        private void dgvOption_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 2 || e.RowIndex < 0) return;
            
            DataGridView dgv = sender as DataGridView;
            List<int> ArrIdx = new List<int>();
           
            if (dgv == dgvCommon) ArrIdx = m_dgvCommonItemIdx;
            else if (dgv == dgvMachine) ArrIdx = m_dgvMachineItemIdx;
            else if (dgv == dgvLoader) ArrIdx = m_dgvLoaderItemIdx;
            else if (dgv == dgvSaw) ArrIdx = m_dgvSawItemIdx;
            else if (dgv == dgvSorter) ArrIdx = m_dgvSorterItemIdx;

            int nItemIdx = ArrIdx[e.RowIndex];
            if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP)
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strValue;
                return;
            }
            double dVal = 0.0;
            int nVal = 0;
            long lVal = 0;
            if (double.TryParse(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out dVal) ||
                int.TryParse(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out nVal) ||
                long.TryParse(dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), out lVal))
            {
                //OptionItem item = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx];
                ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
            else
            {
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ConfigMgr.Inst.TempCfg.itemOptions[nItemIdx].strValue;
            }

        }
        private void SetMaxVelocity()
        {
            for (int nAxisNo = 0; nAxisNo < (int)SVDF.AXES.MAX; nAxisNo++)
            {
                double dMaxVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dMaxVel;
                MOTION.MotionMgr.Inst[nAxisNo].SetMaxVelocity(dMaxVel);
            }
        }

        private void SetHomeOffset()
        {
            for (int nAxisNo = 0; nAxisNo < (int)SVDF.AXES.MAX; nAxisNo++)
            {
                double dOffset = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dHomeOffset;
                MOTION.MotionMgr.Inst[nAxisNo].SetHomeOffset(dOffset);
            }
        }

        private void SetInPositionTolerance()
        {
            for (int nAxisNo = 0; nAxisNo < (int)SVDF.AXES.MAX; nAxisNo++)
            {
                double dOffset = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dInpositionBand;
                MOTION.MotionMgr.Inst[nAxisNo].SetInPositionTolerance(dOffset);
            }
        }

        private void OnPnPOffsetPageChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            int nTg;

            if (int.TryParse(btn.Tag as string, out nTg))
            {
                btnPnPOffsetHead1.BackColor = SystemColors.ButtonFace;
                btnPnPOffsetHead2.BackColor = SystemColors.ButtonFace;

                btn.BackColor = Color.OrangeRed;
                tabPnpOffsetHeads.SelectedIndex = nTg;
            }
        }

        private void btnGetCfgCamToPickerOffset1_Click(object sender, EventArgs e)
        {
            double dViewPosX = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY];
            double dViewPosY = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_1_VISION_OFFSET_READY];

            double dP1PosX = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            double dP1PosY = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;

            double dOffsetX = dViewPosX - dP1PosX;
            double dOffsetY = dViewPosY - dP1PosY;

            edtCnPOffsetX1.Text = dOffsetX.ToString("F3");
            edtCnPOffsetY1.Text = dOffsetY.ToString("F3");
        }

        private void btnGetCfgCamToPickerOffset2_Click(object sender, EventArgs e)
        {
			//권준우 차장님 요청 내용 OFFSET값 잘못 들어가는 부분 수정 HEP
            double dViewPosX = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY];
            double dViewPosY = ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.BALL_VISION_Y].dPos[POSDF.CFG_PICKER_HEAD_2_VISION_OFFSET_READY];

            double dP2PosX = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            double dP2PosY = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;

            double dOffsetX = dViewPosX - dP2PosX;
            double dOffsetY = dViewPosY - dP2PosY;

            edtCnPOffsetX2.Text = dOffsetX.ToString("F3");
            edtCnPOffsetY2.Text = dOffsetY.ToString("F3");
        }

        private void btnPnPPkOffsetHead1_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            int nTg;

            if (int.TryParse(btn.Tag as string, out nTg))
            {
                btnPnPPkOffsetHead1.BackColor = SystemColors.ButtonFace;
                btnPnPPkOffsetHead2.BackColor = SystemColors.ButtonFace;

                btn.BackColor = Color.OrangeRed;
                tabPnpPkOffsetHeads.SelectedIndex = nTg;
            }
        }

        private void btnCalcTray4PickerHead1Coord_Click(object sender, EventArgs e)
        {
            double dColLen, dRowLen;
            double dColPitch, dRowPitch;
            double dHorAngle, dVerAngle;
            int nTableOffset = tbcTrayCoordPickerHead1.SelectedIndex;
            dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (3 * nTableOffset)]);

            dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (3 * nTableOffset)]);

            dColLen = MathUtil.GetDistance(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P1 + (3 * nTableOffset)]);

            dRowLen = MathUtil.GetDistance(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P1 + (3 * nTableOffset)]);

            dColPitch = dColLen / (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - 1);
            dRowPitch = dRowLen / (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1);

            string strCalcMsg = string.Format("[{11} #1 ({12} - {0})] \n{7} {9} = {1:0.000}\n{8} {9} = {2:0.000}\n{7} {10}: = {3:0.000} ({4:0.000})\n{8} {10} = {5:0.000} ({6:0.000})",
                                                         nTableOffset, dHorAngle, dVerAngle, dColLen, dColPitch, dRowLen, dRowPitch, FormTextLangMgr.FindKey("가로"), FormTextLangMgr.FindKey("세로"), FormTextLangMgr.FindKey("Angle"), FormTextLangMgr.FindKey("Len"), FormTextLangMgr.FindKey("Picker Head"), FormTextLangMgr.FindKey("Table"));
            MessageBox.Show(strCalcMsg);

        }

        private void btnCalcTray4PickerHead2Coord_Click(object sender, EventArgs e)
        {
            double dColLen, dRowLen;
            double dColPitch, dRowPitch;
            double dHorAngle, dVerAngle;
            int nTableOffset = tbcTrayCoordPickerHead2.SelectedIndex;
            dHorAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2 + (3 * nTableOffset)]);

            dVerAngle = MathUtil.GetAngle(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2 + (3 * nTableOffset)],
                                          ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2 + (3 * nTableOffset)]);

            dColLen = MathUtil.GetDistance(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_X_P2 + (3 * nTableOffset)]);

            dRowLen = MathUtil.GetDistance(ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_START_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2 + (3 * nTableOffset)],
                                           ConfigMgr.Inst.Cfg.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableOffset].dPos[POSDF.CFG_ULD_TRAY_TABLE_1_OFFSET_LAST_Y_P2 + (3 * nTableOffset)]);

            dColPitch = dColLen / (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - 1);
            dRowPitch = dRowLen / (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1);

            string strCalcMsg = string.Format("[{11} #2 ({12} - {0})] \n{7} {9} = {1:0.000}\n{8} {9} = {2:0.000}\n{7} {10}: = {3:0.000} ({4:0.000})\n{8} {10} = {5:0.000} ({6:0.000})",
                                                         nTableOffset, dHorAngle, dVerAngle, dColLen, dColPitch, dRowLen, dRowPitch, FormTextLangMgr.FindKey("가로"), FormTextLangMgr.FindKey("세로"), FormTextLangMgr.FindKey("Angle"), FormTextLangMgr.FindKey("Len"), FormTextLangMgr.FindKey("Picker Head"), FormTextLangMgr.FindKey("Table"));
            MessageBox.Show(strCalcMsg);

        }

        private void rdbUseOrSetting1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;

            string[] arrTag;

            arrTag = rdb.Tag.ToString().Split('_');
            int nRoopCnt = (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT - (int)OPTNUM.GEM_USE;
            int nOffset = (int)OPTNUM.GEM_USE;

            if (arrTag.Length < 1) return;
            if (rdb.Checked)
            {
                switch (arrTag[0])
                {
                    case "COMMON":
                        if (arrTag[1] == "USE/SKIP")
                        {
                            dgvCommon.Columns[2].Visible = false;
                            dgvCommon.Columns[3].Visible = true;
                        }
                        else
                        {
                            dgvCommon.Columns[2].Visible = true;
                            dgvCommon.Columns[3].Visible = false;
                        }
                        nRoopCnt = (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT - (int)OPTNUM.GEM_USE;
                        nOffset = (int)OPTNUM.GEM_USE;
                        if (arrTag[1] == "SETTING")
                        {
                            ChangeDataGridView(dgvCommon, nRoopCnt, nOffset, new List<int>(), false);
                        }
                        else
                        {
                            ChangeDataGridView(dgvCommon, nRoopCnt, nOffset, new List<int>(), true);
                        }
                        //foreach (DataGridViewRow item in dgvCommon.Rows)
                        //{
                        //    if (item.Cells[1].Value.ToString().Contains("니다") ||
                        //        item.Cells[1].Value.ToString().Contains("사용"))
                        //    {
                        //        item.Visible = arrTag[1] == "SETTING" ? false : true;
                        //    }
                        //    else
                        //    {
                        //        item.Visible = arrTag[1] == "USE/SKIP" ? false : true;
                        //    }
                        //}
                        break;
                    case "LOADER":
                        if (arrTag[1] == "USE/SKIP")
                        {
                            dgvLoader.Columns[2].Visible = false;
                            dgvLoader.Columns[3].Visible = true;
                        }
                        else
                        {
                            dgvLoader.Columns[2].Visible = true;
                            dgvLoader.Columns[3].Visible = false;
                        }

                        nRoopCnt = (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET - (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT;
                        nOffset = (int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT; 
                        if (arrTag[1] == "SETTING")
                        {
                            ChangeDataGridView(dgvLoader, nRoopCnt, nOffset, new List<int>(), false);
                        }
                        else
                        {
                            ChangeDataGridView(dgvLoader, nRoopCnt, nOffset, new List<int>(), true);
                        }
                        break;
                    case "SAW":
                        if (arrTag[1] == "USE/SKIP")
                        {
                            dgvSaw.Columns[2].Visible = false;
                            dgvSaw.Columns[3].Visible = true;
                        }
                        else
                        {
                            dgvSaw.Columns[2].Visible = true;
                            dgvSaw.Columns[3].Visible = false;
                        }

                        nRoopCnt = 1800 - (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET;
                        nOffset = (int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET;
                        if (arrTag[1] == "SETTING")
                        {
                            ChangeDataGridView(dgvSaw, nRoopCnt, nOffset, new List<int>(), false);
                        }
                        else
                        {
                            ChangeDataGridView(dgvSaw, nRoopCnt, nOffset, new List<int>(), true);
                        }
                        break;
                    case "SORTER":
                        if (arrTag[1] == "USE/SKIP")
                        {
                            dgvSorter.Columns[2].Visible = false;
                            dgvSorter.Columns[3].Visible = true;
                        }
                        else
                        {
                            dgvSorter.Columns[2].Visible = true;
                            dgvSorter.Columns[3].Visible = false;
                        }

                        nRoopCnt = (int)OPTNUM.CLEANER_Z_SLOW_DOWN_OFFSET - 1800;
                        nOffset = 1800;
                        if (arrTag[1] == "SETTING")
                        {
                            ChangeDataGridView(dgvSorter, nRoopCnt, nOffset, new List<int>(), false);
                        }
                        else
                        {
                            ChangeDataGridView(dgvSorter, nRoopCnt, nOffset, new List<int>(), true);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void glassButton2_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            int nTg;

            if (int.TryParse(btn.Tag as string, out nTg))
            {
                btnHeadOffset1PickerP90.BackColor = SystemColors.ButtonFace;
                btnHeadOffset1PickerP180.BackColor = SystemColors.ButtonFace;
                btnHeadOffset1PickerP270.BackColor = SystemColors.ButtonFace;
                btnHeadOffset1PickerN90.BackColor = SystemColors.ButtonFace;
                btnHeadOffset1PickerN180.BackColor = SystemColors.ButtonFace;
                btnHeadOffset1PickerN270.BackColor = SystemColors.ButtonFace;

                btn.BackColor = Color.DodgerBlue;
                tabDegree1.SelectedIndex = nTg;
            }
        }

        private void btnHeadOffset2Picker90_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            int nTg;

            if (int.TryParse(btn.Tag as string, out nTg))
            {
                btnHeadOffset2PickerP90.BackColor = SystemColors.ButtonFace;
                btnHeadOffset2PickerP180.BackColor = SystemColors.ButtonFace;
                btnHeadOffset2PickerP270.BackColor = SystemColors.ButtonFace;
                btnHeadOffset2PickerN90.BackColor = SystemColors.ButtonFace;
                btnHeadOffset2PickerN180.BackColor = SystemColors.ButtonFace;
                btnHeadOffset2PickerN270.BackColor = SystemColors.ButtonFace;
                btn.BackColor = Color.DodgerBlue;
                tabDegree2.SelectedIndex = nTg;
            }
        }

        void StartMapViewCycle(int nTableNo, int nHeadNo, params object[] args)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + FormTextLangMgr.FindKey(" Map Vision View ]?"), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetMapVisionViewCycleRun(nTableNo, nHeadNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MAP_VISION_VIEW_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        void StartTrayViewCycle(int nTableNo, int nHeadNo, params object[] args)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + FormTextLangMgr.FindKey(" Tray Vision View ]?"), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetTrayVisionViewCycleRun(nTableNo, nHeadNo, args);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.TRAY_VISION_VIEW_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        private void btnTrayVisionView_P1_Click(object sender, EventArgs e)
        {
            StartTrayViewCycle(tbcTrayCoordPickerHead1.SelectedIndex, 0);
        }

        private void btnTrayVisionView_P2_Click(object sender, EventArgs e)
        {
            StartTrayViewCycle(tbcTrayCoordPickerHead2.SelectedIndex, 1);
        }

        private void btnVisionInterfaceTest_Click(object sender, EventArgs e)
        {
            popVisionManual.ShowDialog();
        }

        private void btnPickerCalHead1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            int nHeadNo = nTag;

            ComboBox[] cbAngleArray = { cbPickerCalAngleHead1, cbPickerCalAngleHead2 };
            ucNumericKeyInputButtonEx[] numPickerPitchX = { edtPnPDefOffsetX1, edtPnPDefOffsetX2 };

            int nAngleIndex = cbAngleArray[nTag].SelectedIndex;
            int nAngleOffset = 90 * nAngleIndex;
            if(nAngleIndex >= 4)
            {
                nAngleOffset = -90 * (nAngleIndex - 3);
            }

            double dPickerPitchX = numPickerPitchX[nTag].ToDouble();
            int nRetryCount = (int)numPickerCalRetryCount.Value;
            double dTolerence = edtPickerTolerence.ToDouble();

            if (nAngleIndex < 0) return;

            popMessageBox msg = new popMessageBox(String.Format("{3} : {0}, {1}˚ {2}", nHeadNo + 1, nAngleOffset,FormTextLangMgr.FindKey("캘리브레이션을 시작하시겠습니까?"), FormTextLangMgr.FindKey("피커")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualPickerCal(nHeadNo, nAngleIndex, dPickerPitchX, nRetryCount, dTolerence, chkUsePickerCalMovePitchX.Checked);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_CALIBRATION);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // 전체 다시 계산
            ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nAngleIndex][0].x = 0.0;
            ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nAngleIndex][0].y = 0.0;

            for (int i = 1; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nAngleIndex][i].x =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nAngleIndex][i].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nAngleIndex][0].x;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchOffset[nAngleIndex][i].y =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nAngleIndex][i].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[nAngleIndex][0].y;

                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nAngleIndex][i].x =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nAngleIndex][i].x - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nAngleIndex][0].x;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspOffset[nAngleIndex][i].y =
                    ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nAngleIndex][i].y - ConfigMgr.Inst.TempCfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[nAngleIndex][0].y;
            }

            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                if (nHeadNo == 0)
                {
                    if (nAngleIndex == CFG_DF.DEGREE_0)
                    {
                        gridPickerHeadOffset1deg0.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1deg0.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg0.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                    }
                    else if(nAngleIndex == CFG_DF.DEGREE_P90)
                    {
                        gridPickerHeadOffset1deg90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1deg90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_P180)
                    {
                        gridPickerHeadOffset1deg180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1deg180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                    }
                    else if(nAngleIndex == CFG_DF.DEGREE_P270)
                    {
                        gridPickerHeadOffset1deg270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1deg270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1deg270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N90)
                    {
                        gridPickerHeadOffset1degN90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1degN90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N180)
                    {
                        gridPickerHeadOffset1degN180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1degN180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N270)
                    {
                        gridPickerHeadOffset1degN270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset1degN270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset1degN270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[0].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                    }
                }
                else
                {
                    if (nAngleIndex == CFG_DF.DEGREE_0)
                    {
                        gridPickerHeadOffset2deg0.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2deg0.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg0.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_0][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_P90)
                    {
                        gridPickerHeadOffset2deg90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2deg90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P90][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_P180)
                    {
                        gridPickerHeadOffset2deg180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2deg180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P180][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_P270)
                    {
                        gridPickerHeadOffset2deg270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2deg270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2deg270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_P270][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N90)
                    {
                        gridPickerHeadOffset2degN90.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2degN90.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN90.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N90][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N180)
                    {
                        gridPickerHeadOffset2degN180.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2degN180.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN180.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N180][nPadNo].y.ToString("0.000");
                    }
                    else if (nAngleIndex == CFG_DF.DEGREE_N270)
                    {
                        gridPickerHeadOffset2degN270.Rows[0].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[1].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[2].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[3].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");

                        gridPickerHeadOffset2degN270.Rows[5].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[6].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].x.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[7].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                        gridPickerHeadOffset2degN270.Rows[8].Cells[nPadNo + 1].Value = ConfigMgr.Inst.TempCfg.OffsetPnP[1].dpPickerPitchInspOffset[CFG_DF.DEGREE_N270][nPadNo].y.ToString("0.000");
                    }
                }
            }
        }

        private void SetLogExpPreiod()
        {
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ALARM_LOG_EXP_PERIOD].nValue != ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.ALARM_LOG_EXP_PERIOD].nValue)
            //{
            //    GbVar.dbAlarmLog.SetDeletePeriod(ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.ALARM_LOG_EXP_PERIOD].nValue);
            //}

            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EVENT_LOG_EXP_PERIOD].nValue != ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.EVENT_LOG_EXP_PERIOD].nValue)
            //{
            //    GbVar.dbEventData.SetDeletePeriod(ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.EVENT_LOG_EXP_PERIOD].nValue);
            //}

            ////if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue != ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue)
            ////{
            ////    for (int i = 0; i < (int)DBDF.LOG_TYPE.MAX; i++)
            ////    {
            ////        GbVar.dbLogData[i].SetDeletePeriod(ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue);
            ////    }
            ////}

            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.LOT_LOG_EXP_PERIOD].nValue != ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.LOT_LOG_EXP_PERIOD].nValue)
            //{
            //    GbVar.dbLotLog.SetDeletePeriod(ConfigMgr.Inst.TempCfg.itemOptions[(int)OPTNUM.LOT_LOG_EXP_PERIOD].nValue);
            //}
        }

        private void rdbLanguageChn_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbtn = sender as RadioButton;
            if (rbtn.Checked)
            {
                int nTag;
                string strTag = rbtn.Tag as string;
                if (int.TryParse(strTag, out nTag))
                {
                    ConfigMgr.Inst.TempCfg.General.nLanguage = nTag;
                }
            }
        }

        private void OnXyztOffsetPageChangeButtonClick(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            int nTg;

            if (int.TryParse(btn.Tag as string, out nTg))
            {
                btnXyztOffsetHead1.BackColor = SystemColors.ButtonFace;
                btnXyztOffsetHead2.BackColor = SystemColors.ButtonFace;

                btn.BackColor = Color.OrangeRed;
                tabXyztOffsetHeads.SelectedIndex = nTg;
            }
        }

        private void btnXyztOffsetClear_Click(object sender, EventArgs e)
        {
            double dAddValue = 0.0;
            int nSelectedRow = 0;
            int nSelectedHead = 0;

            if (tabXyztOffsetHeads.SelectedIndex == 0)
            {
                if (gridXyztOffsetHead1.SelectedCells.Count != 1) return;
                if (gridXyztOffsetHead1.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridXyztOffsetHead1.SelectedCells[0].RowIndex;
                nSelectedHead = 0;
            }
            else
            {
                if (gridXyztOffsetHead2.SelectedCells.Count != 1) return;
                if (gridXyztOffsetHead2.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridXyztOffsetHead2.SelectedCells[0].RowIndex;
                nSelectedHead = 1;
            }

            for (int i = 0; i < MCDF.PAD_MAX; i++)
            {
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_X][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_Y][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_Z][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_T][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_X][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_Y][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_Z][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_T][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_X][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_Y][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_Z][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_T][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_X][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_Y][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_Z][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_T][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_X][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_Y][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_Z][i] = 0;
                ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_T][i] = 0;
            }

            UpdatePickerXyztGridData(nSelectedHead);
            //popMessageBox pbox;
            //pbox = new popMessageBox("컨피그를 저장해주세요.", "Warning");
            //pbox.TopMost = true;
            //pbox.ShowDialog(this);
        }

        private void btnXyztOffsetLineAdd_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("AddValue", "AddValue", "0.0");

            double dAddValue = 0.0;
            int nSelectedRow = 0;
            int nSelectedHead = 0;

            if (!double.TryParse(input, out dAddValue)) return;

            if (tabXyztOffsetHeads.SelectedIndex == 0)
            {
                if (gridXyztOffsetHead1.SelectedCells.Count != 1) return;
                if (gridXyztOffsetHead1.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridXyztOffsetHead1.SelectedCells[0].RowIndex;
                nSelectedHead = 0;
            }
            else
            {
                if (gridXyztOffsetHead2.SelectedCells.Count != 1) return;
                if (gridXyztOffsetHead2.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridXyztOffsetHead2.SelectedCells[0].RowIndex;
                nSelectedHead = 1;
            }

            int nTmp = 0;
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_X][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_Y][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_Z][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_1][MCDF.XYZT_T][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_X][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_Y][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_Z][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.MAP_TABLE_2][MCDF.XYZT_T][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_X][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_Y][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_Z][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_1][MCDF.XYZT_T][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_X][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_Y][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_Z][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.GOOD_TRAY_2][MCDF.XYZT_T][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_X][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_Y][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_Z][i] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.dOffsetXYZT[nSelectedHead][MCDF.REWORK_TRAY][MCDF.XYZT_T][i] += dAddValue; } }

            UpdatePickerXyztGridData(nSelectedHead);
        }

        private void btnZeroSetLfStageHead2_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            int nHeadNo = 1;
            int nStageNo = nTag;

            bool[] isUsePad = { chkZeroSetHead2Pad1.Checked, chkZeroSetHead2Pad2.Checked, chkZeroSetHead2Pad3.Checked, chkZeroSetHead2Pad4.Checked,
                                chkZeroSetHead2Pad5.Checked, chkZeroSetHead2Pad6.Checked, chkZeroSetHead2Pad7.Checked, chkZeroSetHead2Pad8.Checked };

            double dStartZ = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead2Z;

            double dStepZ = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead2Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP].ToDouble();
            double dStartPosY1 = m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP].ToDouble();
            double dStartPosY2 = m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP].ToDouble();

            if (nStageNo == 1)
            {
                dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PICKUP_T2].ToDouble();
            }

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Pickup(nHeadNo, nStageNo, isUsePad, dStartZ, dStepZ, dStartPosX, nStageNo == 0 ? dStartPosY1 : dStartPosY2);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PICK_UP);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetLfStageHead1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            int nHeadNo = 0;
            int nStageNo = nTag;

            bool[] isUsePad = { chkZeroSetHead1Pad1.Checked, chkZeroSetHead1Pad2.Checked, chkZeroSetHead1Pad3.Checked, chkZeroSetHead1Pad4.Checked,
                                chkZeroSetHead1Pad5.Checked, chkZeroSetHead1Pad6.Checked, chkZeroSetHead1Pad7.Checked, chkZeroSetHead1Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPickUpZerosetStartHead1Z;

            double dStepZ = ConfigMgr.Inst.Cfg.dPickUpZerosetStepHead1Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP].ToDouble();
            double dStartPosY1 = m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP].ToDouble();
            double dStartPosY2 = m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP].ToDouble();

            if (nStageNo == 1)
            {
                dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PICKUP_T2].ToDouble();
            }

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Pickup(nHeadNo, nStageNo, isUsePad, dStartZ, dStepZ, dStartPosX, nStageNo == 0 ? dStartPosY1 : dStartPosY2);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PICK_UP);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetGd1StageHead1_Click(object sender, EventArgs e)
        {
            int nHeadNo = 0;

            bool[] isUsePad = { chkPlaceZeroSetHead1Pad1.Checked, chkPlaceZeroSetHead1Pad2.Checked, chkPlaceZeroSetHead1Pad3.Checked, chkPlaceZeroSetHead1Pad4.Checked,
                                chkPlaceZeroSetHead1Pad5.Checked, chkPlaceZeroSetHead1Pad6.Checked, chkPlaceZeroSetHead1Pad7.Checked, chkPlaceZeroSetHead1Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead1Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead1Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 0, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetGd2StageHead1_Click(object sender, EventArgs e)
        {
            int nHeadNo = 0;

            bool[] isUsePad = { chkPlaceZeroSetHead1Pad1.Checked, chkPlaceZeroSetHead1Pad2.Checked, chkPlaceZeroSetHead1Pad3.Checked, chkPlaceZeroSetHead1Pad4.Checked,
                                chkPlaceZeroSetHead1Pad5.Checked, chkPlaceZeroSetHead1Pad6.Checked, chkPlaceZeroSetHead1Pad7.Checked, chkPlaceZeroSetHead1Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead1Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead1Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_T2].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 1, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetRwStageHead1_Click(object sender, EventArgs e)
        {
            int nHeadNo = 0;

            bool[] isUsePad = { chkPlaceZeroSetHead1Pad1.Checked, chkPlaceZeroSetHead1Pad2.Checked, chkPlaceZeroSetHead1Pad3.Checked, chkPlaceZeroSetHead1Pad4.Checked,
                                chkPlaceZeroSetHead1Pad5.Checked, chkPlaceZeroSetHead1Pad6.Checked, chkPlaceZeroSetHead1Pad7.Checked, chkPlaceZeroSetHead1Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead1Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead1Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE_RW].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.CFG_PICKER_HEAD_1_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 2, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetGd1StageHead2_Click(object sender, EventArgs e)
        {
            int nHeadNo = 1;

            bool[] isUsePad = { chkPlaceZeroSetHead2Pad1.Checked, chkPlaceZeroSetHead2Pad2.Checked, chkPlaceZeroSetHead2Pad3.Checked, chkPlaceZeroSetHead2Pad4.Checked,
                                chkPlaceZeroSetHead2Pad5.Checked, chkPlaceZeroSetHead2Pad6.Checked, chkPlaceZeroSetHead2Pad7.Checked, chkPlaceZeroSetHead2Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead2Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead2Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 0, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetGd2StageHead2_Click(object sender, EventArgs e)
        {
            int nHeadNo = 1;

            bool[] isUsePad = { chkPlaceZeroSetHead2Pad1.Checked, chkPlaceZeroSetHead2Pad2.Checked, chkPlaceZeroSetHead2Pad3.Checked, chkPlaceZeroSetHead2Pad4.Checked,
                                chkPlaceZeroSetHead2Pad5.Checked, chkPlaceZeroSetHead2Pad6.Checked, chkPlaceZeroSetHead2Pad7.Checked, chkPlaceZeroSetHead2Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead2Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead2Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_T2].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 1, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnZeroSetRwStageHead2_Click(object sender, EventArgs e)
        {
            int nHeadNo = 1;

            bool[] isUsePad = { chkPlaceZeroSetHead2Pad1.Checked, chkPlaceZeroSetHead2Pad2.Checked, chkPlaceZeroSetHead2Pad3.Checked, chkPlaceZeroSetHead2Pad4.Checked,
                                chkPlaceZeroSetHead2Pad5.Checked, chkPlaceZeroSetHead2Pad6.Checked, chkPlaceZeroSetHead2Pad7.Checked, chkPlaceZeroSetHead2Pad8.Checked  };

            double dStartZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStartHead2Z;
            double dStepZ = ConfigMgr.Inst.Cfg.dPlaceZerosetStepHead2Z;

            double dStartPosX = m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE_RW].ToDouble();
            double dStartPosY = m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.CFG_PICKER_HEAD_2_ZEROSET_POS_4_PLACE].ToDouble();

            popMessageBox msg = new popMessageBox(String.Format("{0} PICKER {1} ", nHeadNo + 1, FormTextLangMgr.FindKey("제로셋을 시작하시겠습니까?")), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbSeq.manualRun.SetManualZeroSet_Place(nHeadNo, 2, isUsePad, dStartZ, dStepZ, dStartPosX, dStartPosY);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

            if (GbSeq.manualRun.RESULT < 0) return;

            // Thread에서 Temp에 저장하였음

            // UI를 갱신한다
            UpdatePickerDownGridData(nHeadNo);
        }

        private void btnPnPOffsetClear_Click(object sender, EventArgs e)
        {
            double dAddValue = 0.0;
            int nSelectedRow = 0;
            int nSelectedHead = 0;

            if (tabPnpOffsetHeads.SelectedIndex == 0)
            {
                if (gridPnpOffsetHead1.SelectedCells.Count != 1) return;
                if (gridPnpOffsetHead1.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridPnpOffsetHead1.SelectedCells[0].RowIndex;
                nSelectedHead = 0;
            }
            else
            {
                if (gridPnpOffsetHead2.SelectedCells.Count != 1) return;
                if (gridPnpOffsetHead2.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridPnpOffsetHead2.SelectedCells[0].RowIndex;
                nSelectedHead = 1;
            }

            for (int i = 0; i < MCDF.PAD_MAX; i++)
            {
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUp[CFG_DF.MAP_STG_1] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUp[CFG_DF.MAP_STG_2] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_GD1] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_GD1] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_GD2] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_GD2] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_RWK] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_RWK] = 0;
                ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_RJT] = 0;
            }

            UpdatePickerDownGridData(nSelectedHead);
        }

        private void btnPnPOffsetAdd_Click(object sender, EventArgs e)
        {
            string input = Interaction.InputBox("AddValue", "AddValue", "0.0");

            double dAddValue = 0.0;
            int nSelectedRow = 0;
            int nSelectedHead = 0;

            if (!double.TryParse(input, out dAddValue)) return;

            if (tabPnpOffsetHeads.SelectedIndex == 0)
            {
                if (gridPnpOffsetHead1.SelectedCells.Count != 1) return;
                if (gridPnpOffsetHead1.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridPnpOffsetHead1.SelectedCells[0].RowIndex;
                nSelectedHead = 0;
            }
            else
            {
                if (gridPnpOffsetHead2.SelectedCells.Count != 1) return;
                if (gridPnpOffsetHead2.SelectedCells[0].ColumnIndex != 0) return;
                nSelectedRow = gridPnpOffsetHead2.SelectedCells[0].RowIndex;
                nSelectedHead = 1;
            }

            int nTmp = 0;


            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUp[CFG_DF.MAP_STG_1] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPickUp[CFG_DF.MAP_STG_2] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_GD1] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_GD1] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_GD2] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_GD2] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlaceWait[CFG_DF.TRAY_STG_RWK] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_RWK] += dAddValue; } }
            if (nSelectedRow == nTmp++) { for (int i = 0; i < MCDF.PAD_MAX; i++) { ConfigMgr.Inst.TempCfg.OffsetPnP[nSelectedHead].dPnpOffsetZ[i].dPlace[CFG_DF.TRAY_STG_RJT] += dAddValue; } }

            UpdatePickerDownGridData(nSelectedHead);
        }
    }

    public class MyBoolEditor : UITypeEditor
    {
        public override bool GetPaintValueSupported
            (System.ComponentModel.ITypeDescriptorContext context)
        { return true; }
        public override void PaintValue(PaintValueEventArgs e)
        {
            var rect = e.Bounds;
            rect.Inflate(1, 1);
            ControlPaint.DrawCheckBox(e.Graphics, rect, ButtonState.Flat |
                (((bool)e.Value) ? ButtonState.Checked : ButtonState.Normal));
        }
    }
}
