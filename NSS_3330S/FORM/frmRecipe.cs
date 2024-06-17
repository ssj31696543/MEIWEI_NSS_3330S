using NSS_3330S.UC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.POP;
using NSS_3330S.MOTION;
using System.IO;
using NSS_3330S.GLOBAL;
using Glass;
using DionesTool.Objects;

namespace NSS_3330S.FORM
{
    /// <summary>
    /// [2022.05.26.kmlee] 하부 비젼 기준 값 삭제
    /// </summary>
    public partial class frmRecipe : BaseRecipePosControl
    {
        private bool flagTimerPopRmsRun = false;
        private bool flagTimerRefreshStart = false;
        private bool flagTimerRefresh100msStart = false;
        public delegate void EqRcpChange(MESDF.ePPChangeCode mode, string rcpName, bool isHostBody = false);
        public event EqRcpChange eqRcpChange = null;

        ComboBox[] m_Cboxs;
        ucServoStatus[] m_Servos;
        DataGridView[] m_Grids;

        List<ucManualOutputButton> m_listOuputButton = new List<ucManualOutputButton>();
        List<InputPanel> m_listInputPanel = new List<InputPanel>();
        public BindingList<MapGroupInfo> m_blistMapGrpInfo1 = new BindingList<MapGroupInfo>();
        public BindingList<MapGroupInfo> m_blistMapGrpInfo2 = new BindingList<MapGroupInfo>();


        ListBox[] m_lbxCleaning;
        ComboBox[] m_cbxCleaning;
        NumericUpDown[] m_numCleaning;
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

        public frmRecipe()
        {
            InitializeComponent();
            LinkRecipeControl();

            FindOutputButton(this.Controls);
            FindInputPanel(this.Controls);

            tbcRcpDeviceSetting.ItemSize = new Size(0, 1);

            m_lbxCleaning = new ListBox[] { lbxUnitTrsfClean, lbxCleanTableClean, lbxDryTableClean };
            m_cbxCleaning = new ComboBox[] { comboBoxTypeUnitPicker, comboBoxTypeCleanTable, comboBoxTypeDryTable };
            m_numCleaning = new NumericUpDown[] { numericUpDownCountUnitPicker, numericUpDownCountCleanTable, numericUpDownCountDryTable };

            UpdateListbox();

            cmbPickerPlaceT.Items.Clear();
            cmbPickerPlaceT.Items.Add("0");
            cmbPickerPlaceT.Items.Add("90");
            cmbPickerPlaceT.Items.Add("180");
            cmbPickerPlaceT.Items.Add("270");
            cmbPickerPlaceT.Items.Add("-90");
            cmbPickerPlaceT.Items.Add("-180");
            cmbPickerPlaceT.Items.Add("-270");
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

        private void frmRecipe_Load(object sender, EventArgs e)
        {
            GbFunc.SetDoubleBuffered(gridModel);
            GbVar.bChangeRecipe_Main = false;
            ShowModelFileList();
            txtRcpSoruce.Text = gridModel.Rows[0].Cells[1].Value.ToString();

            ShowLdMgzInfoList();
            ShowMapTableInfoList();
            ShowUldTrayInfoList();

            toolTip1.IsBalloon = true;
            UpdateRcpData();

            tmrRefresh100ms.Enabled = true;
            InitCleanOptTableGridView(gdvMapGroupInfo_T1, m_blistMapGrpInfo1);
            InitCleanOptTableGridView(gdvMapGroupInfo_T2, m_blistMapGrpInfo2);

            GbFunc.SetDoubleBuffered(gdvMapGroupInfo_T1);
            GbFunc.SetDoubleBuffered(gdvMapGroupInfo_T2);

            cbxGrabOnlyOneEdge.Text = FormTextLangMgr.FindKey(cbxGrabOnlyOneEdge.Text);

            //GbDev.MGZBarcodeReader.On_MGZ_BCR_Start_Display += MGZ_BCR_LIVE_START;
            //GbDev.MGZBarcodeReader.On_MGZ_BCR_Stop_Display += MGZ_BCR_LIVE_END;
        }

        void InitCleanOptTableGridView(DataGridView dgv, BindingList<MapGroupInfo> listMapGrpItem)
        {
            DataGridViewTextBoxColumn dgvcText = null;
            DataGridViewComboBoxColumn dgvcCombo = null;
            DataGridViewButtonColumn dgvcButton = null;

            dgv.Columns.Clear();
            dgv.AutoGenerateColumns = false;

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "GROUP_NO";
            dgvcText.HeaderText = "NO";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Frozen = true;
            dgvcText.Width = 70;
            dgv.Columns.Add(dgvcText);

            //dgvcText = new DataGridViewTextBoxColumn();
            //dgvcText.DataPropertyName = "GROUP_COUNT_X";
            //dgvcText.HeaderText = "GROUP COUNT (X)";
            //dgvcText.Name = dgvcText.DataPropertyName;
            //dgvcText.ReadOnly = false;
            //dgvcText.Width = 100;
            //dgv.Columns.Add(dgvcText);

            //dgvcText = new DataGridViewTextBoxColumn();
            //dgvcText.DataPropertyName = "GROUP_COUNT_Y";
            //dgvcText.HeaderText = "GROUP COUNT (Y)";
            //dgvcText.Name = dgvcText.DataPropertyName;
            //dgvcText.ReadOnly = false;
            //dgvcText.Width = 100;
            //dgv.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TOP_INSP_POS_X";
            dgvcText.HeaderText = "TOP POS X";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "TOP_INSP_POS_Y";
            dgvcText.HeaderText = "TOP POS Y";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "POSITION SET";
            dgvcButton.Text = "GET";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "MOVE CMD";
            dgvcButton.Text = "MOVE";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "PICK_UP_POS_X1";
            dgvcText.HeaderText = "PICK UP POS X1";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "PICK_UP_POS_Y1";
            dgvcText.HeaderText = "PICK UP POS Y1";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "POSITION SET";
            dgvcButton.Text = "GET";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "MOVE CMD";
            dgvcButton.Text = "MOVE";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "PICK_UP_POS_X2";
            dgvcText.HeaderText = "PICK UP POS X2";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcText = new DataGridViewTextBoxColumn();
            dgvcText.DataPropertyName = "PICK_UP_POS_Y2";
            dgvcText.HeaderText = "PICK UP POS Y2";
            dgvcText.Name = dgvcText.DataPropertyName;
            dgvcText.ReadOnly = false;
            dgvcText.Width = 100;
            dgv.Columns.Add(dgvcText);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "POSITION SET";
            //dgvcButton.Name = "GET";
            dgvcButton.Text = "GET";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgvcButton = new DataGridViewButtonColumn();
            dgvcButton.HeaderText = "MOVE CMD";
            //dgvcButton.Name = "MOVE";
            dgvcButton.Text = "MOVE";
            dgvcButton.Width = 80;
            dgvcButton.UseColumnTextForButtonValue = true;
            dgvcButton.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            dgvcButton.FlatStyle = FlatStyle.Standard;
            dgvcButton.CellTemplate.Style.BackColor = Color.Honeydew;
            dgv.Columns.Add(dgvcButton);

            dgv.DataSource = listMapGrpItem;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        private void ShowModelFileList()
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_RECIPE);

                        cmbTargetRecipeIdx.Items.Clear();
                        gridModel.Rows.Clear();

                        for (int nRcpCnt = 1; nRcpCnt <= 100; nRcpCnt++)
                        {
                            cmbTargetRecipeIdx.Items.Add(nRcpCnt);
                            gridModel.Rows.Add(nRcpCnt, "", "", "", "");
                        }

                        foreach (FileInfo f in dir.GetFiles("*.mdl"))
                        {
                            string strPath = RecipeMgr.Inst.GetRecipePath(f.Name);
                            Recipe rcp = Recipe.Deserialize(strPath);

                            string[] arr = new string[5];
                            arr[0] = rcp.strNo;
                            arr[1] = f.Name.Replace(".mdl", "");
                            arr[2] = rcp.strRecipeDescription;
                            arr[3] = Convert.ToString(f.CreationTime);
                            arr[4] = Convert.ToString(f.LastWriteTime);

                            //if (!IsAcceptTargetRecipeId(arr[0])) continue;
                            try
                            {
                                gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[0].Value = arr[0];
                                gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[1].Value = arr[1];
                                gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[2].Value = arr[2];
                                gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[3].Value = arr[3];
                                gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[4].Value = arr[4];

                                cmbTargetRecipeIdx.Items.Remove(int.Parse(rcp.strNo));
                            }
                            catch { }
                        }

                        cmbTargetRecipeIdx.SelectedIndex = 0;
                    });
                }
                else
                {
                    DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_RECIPE);

                    cmbTargetRecipeIdx.Items.Clear();
                    gridModel.Rows.Clear();

                    for (int nRcpCnt = 1; nRcpCnt <= 100; nRcpCnt++)
                    {
                        cmbTargetRecipeIdx.Items.Add(nRcpCnt);
                        gridModel.Rows.Add(nRcpCnt, "", "", "", "");
                    }

                    foreach (FileInfo f in dir.GetFiles("*.mdl"))
                    {
                        string strPath = RecipeMgr.Inst.GetRecipePath(f.Name);
                        Recipe rcp = Recipe.Deserialize(strPath);

                        string[] arr = new string[5];
                        arr[0] = rcp.strNo;
                        arr[1] = f.Name.Replace(".mdl", "");
                        arr[2] = rcp.strRecipeDescription;
                        arr[3] = Convert.ToString(f.CreationTime);
                        arr[4] = Convert.ToString(f.LastWriteTime);

                        //if (!IsAcceptTargetRecipeId(arr[0])) continue;
                        try
                        {
                            gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[0].Value = arr[0];
                            gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[1].Value = arr[1];
                            gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[2].Value = arr[2];
                            gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[3].Value = arr[3];
                            gridModel.Rows[int.Parse(rcp.strNo) - 1].Cells[4].Value = arr[4];

                            cmbTargetRecipeIdx.Items.Remove(int.Parse(rcp.strNo));
                        }
                        catch { }
                    }

                    cmbTargetRecipeIdx.SelectedIndex = 0;
                }
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 1~100사이에 ID인가
        /// </summary>
        private bool IsAcceptTargetRecipeId(string strId)
        {
            //             if (strId == "" || strId == null) return false;
            //             int nId = 0;
            //             if (!int.TryParse(strId, out nId)) return false;
            //             if (nId < 1 || nId > 100) return false;
            //             return true;

            if (strId == "" || strId == null) return false;
            if (strId.Length > 32) return false;
            char[] chInvalidArray = Path.GetInvalidFileNameChars();

            foreach (char ch in chInvalidArray)
            {
                if (strId.Contains(ch))
                    return false;
            }

            if (strId.Contains(' '))
                return false;

            return true;

        }

        public bool UpdateRcpData(bool bSaveAndValidate = false)
        {
            if (m_bIsUpdate == true) return false;
            m_bIsUpdate = true;

            if (bSaveAndValidate == false)
            {
                #region TempRcp2UI

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        UpdateRcpMotData();
                        CopyToTeachPos();

                        tbxAlignCorrOffsetX.Text = RecipeMgr.Inst.Rcp.dAlignXCorrOffset.ToString("F3");

                        lbCurrentUsingMgzDevName.Text = RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName;
                        //lbCurrentUsingMapDevName.Text = RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName;
                        //lbCurrentUsingTrayDevName.Text = RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName;

                        tbxMgzSlotPitch.Text = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch.ToString("F3");
                        tbxMgzSlotCount.Text = RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount.ToString();

                        tbxMapPitchX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX.ToString("F3");
                        tbxMapPitchY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY.ToString("F3");
                        tbxMapCountX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX.ToString();
                        tbxMapCountY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY.ToString();
                        tbxMapSizeX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX.ToString("F3");
                        tbxMapSizeY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY.ToString("F3");

                        tbxMapGroupCntX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX.ToString();
                        tbxMapGroupCntY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY.ToString();

                        tbxMapGroupOffsetX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetX.ToString("F3");
                        tbxMapGroupOffsetY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetY.ToString("F3");

                        tbxMapInspChipCountX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX.ToString();
                        tbxMapInspChipCountY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY.ToString();

                        tbxChipThickness.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness.ToString("F3");

                        cbxGrabOnlyOneEdge.Checked = RecipeMgr.Inst.Rcp.MapTbInfo.bGrabOneEdge;

                        ckbIsStrip.Checked = RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat;

                        tbxUnitGapX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitColGap.ToString();
                        tbxUnitGapY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitRowGap.ToString();

                        tbxTrayPitchX.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX.ToString("F3");
                        tbxTrayPitchY.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY.ToString("F3");
                        tbxTrayThick.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness.ToString("F3");

                        tbxTrayCountX.Text = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX.ToString();
                        tbxTrayCountY.Text = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY.ToString();


                        tbxGDTray1X_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].x.ToString();
                        tbxGDTray1Y_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].y.ToString();
                        tbxGDTray2X_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].x.ToString();
                        tbxGDTray2Y_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].y.ToString();
                        tbxRWTrayX_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].x.ToString();
                        tbxRWTrayY_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].y.ToString();


                        tbxGDTray1X_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].x.ToString();
                        tbxGDTray1Y_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].y.ToString();
                        tbxGDTray2X_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].x.ToString();
                        tbxGDTray2Y_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].y.ToString();
                        tbxRWTrayX_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].x.ToString();
                        tbxRWTrayY_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].y.ToString();
                        //tbxTrayInspOffsetY.Text = RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY.ToString("F3");

                        cmbPickerPlaceT.Text = RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle.ToString();

                        Scrap1_numericUpDown.Value = RecipeMgr.Inst.Rcp.cleaning.nScrapCount1;
                        Scrap2_numericUpDown.Value = RecipeMgr.Inst.Rcp.cleaning.nScrapCount2;

                        tbScrap1Delay.Text = RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay.ToString();
                        tbScrap2Delay.Text = RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay.ToString();

                        numCleanSwingCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nWaterSwingCnt;
                        txbCleanSwingStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lWaterSwingStartDelay.ToString();
                        txbCleanSwingEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lnWaterSwingEndDelay.ToString();

                        numBtmDryCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nBtmDryCnt;
                        tbxBtmDryStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay.ToString();

                        numTopDryCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nTopDryCnt;
                        tbxTopDryStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay.ToString();

                        numSpongeRepeat.Value = RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount;
                        tbxSpongeVel.Text = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel.ToString();
                        tbxSpongeStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeStartDelay.ToString();
                        tbxSpongeEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeEndDelay.ToString();

                        numRepickCount.Value = RecipeMgr.Inst.Rcp.cleaning.nRepickCount;
						m_blistMapGrpInfo1.Clear();
                        foreach (MapGroupInfo item in RecipeMgr.Inst.TempRcp.listMapGrpInfoL)
                        {
                            m_blistMapGrpInfo1.Add((MapGroupInfo)item.Clone());
                        }

                        m_blistMapGrpInfo2.Clear();
                        foreach (MapGroupInfo item in RecipeMgr.Inst.TempRcp.listMapGrpInfoR)
                        {
                            m_blistMapGrpInfo2.Add((MapGroupInfo)item.Clone());
                        }
                        cbScrapMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nScrapMode;
                    });
                }
                else
                {
                    UpdateRcpMotData();
                    CopyToTeachPos();

                    tbxAlignCorrOffsetX.Text = RecipeMgr.Inst.Rcp.dAlignXCorrOffset.ToString("F3");

                    lbCurrentUsingMgzDevName.Text = RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName;
                    //lbCurrentUsingMapDevName.Text = RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName;
                    //lbCurrentUsingTrayDevName.Text = RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName;

                    tbxMgzSlotPitch.Text = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch.ToString("F3");
                    tbxMgzSlotCount.Text = RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount.ToString();

                    tbxMapPitchX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX.ToString("F3");
                    tbxMapPitchY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY.ToString("F3");
                    tbxMapCountX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX.ToString();
                    tbxMapCountY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY.ToString();
                    tbxMapSizeX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX.ToString("F3");
                    tbxMapSizeY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY.ToString("F3");

                    tbxMapGroupCntX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX.ToString();
                    tbxMapGroupCntY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY.ToString();

                    tbxMapGroupOffsetX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetX.ToString("F3");
                    tbxMapGroupOffsetY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetY.ToString("F3");

                    tbxMapInspChipCountX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX.ToString();
                    tbxMapInspChipCountY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY.ToString();

                    tbxChipThickness.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness.ToString("F3");

                    cbxGrabOnlyOneEdge.Checked = RecipeMgr.Inst.Rcp.MapTbInfo.bGrabOneEdge;

                    ckbIsStrip.Checked = RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat;

                    tbxUnitGapX.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitColGap.ToString();
                    tbxUnitGapY.Text = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitRowGap.ToString();

                    tbxTrayPitchX.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX.ToString("F3");
                    tbxTrayPitchY.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY.ToString("F3");
                    tbxTrayThick.Text = RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness.ToString("F3");

                    tbxTrayCountX.Text = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX.ToString();
                    tbxTrayCountY.Text = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY.ToString();
                    //tbxTrayInspOffsetY.Text = RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY.ToString("F3");

                    cmbPickerPlaceT.Text = RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle.ToString();

            
                    tbxGDTray1X_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].x.ToString();
                    tbxGDTray1Y_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].y.ToString();
                    tbxGDTray2X_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].x.ToString();
                    tbxGDTray2Y_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].y.ToString();
                    tbxRWTrayX_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].x.ToString();
                    tbxRWTrayY_H1.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].y.ToString();

                    tbxGDTray1X_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].x.ToString();
                    tbxGDTray1Y_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].y.ToString();
                    tbxGDTray2X_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].x.ToString();
                    tbxGDTray2Y_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].y.ToString();
                    tbxRWTrayX_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].x.ToString();
                    tbxRWTrayY_H2.Text = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].y.ToString();

                    Scrap1_numericUpDown.Value = RecipeMgr.Inst.Rcp.cleaning.nScrapCount1;
                    Scrap2_numericUpDown.Value = RecipeMgr.Inst.Rcp.cleaning.nScrapCount2;

                    tbScrap1Delay.Text = RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay.ToString();
                    tbScrap2Delay.Text = RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay.ToString();

                    numCleanSwingCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nWaterSwingCnt;
                    txbCleanSwingStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lWaterSwingStartDelay.ToString();
                    txbCleanSwingEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lnWaterSwingEndDelay.ToString();

                    numBtmDryCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nBtmDryCnt;
                    tbxBottomDryVel.Text = RecipeMgr.Inst.Rcp.cleaning.nBtmDryVel.ToString();
                    tbxBtmDryStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay.ToString();
                    tbxBtmDryEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lBtmDryEndDelay.ToString();

                    numTopDryCnt.Value = RecipeMgr.Inst.Rcp.cleaning.nTopDryCnt;
                    tbxTopDryVel.Text = RecipeMgr.Inst.Rcp.cleaning.nTopDryVel.ToString();
                    tbxTopDryStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay.ToString();
                    tbxTopDryEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lTopDryEndDelay.ToString();

                    numSpongeRepeat.Value = RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount;
                    tbxSpongeVel.Text = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel.ToString();
                    tbxSpongeStartDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeStartDelay.ToString();
                    tbxSpongeEndDelay.Text = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeEndDelay.ToString();
                    
                    numRepickCount.Value = RecipeMgr.Inst.Rcp.cleaning.nRepickCount;

                    m_blistMapGrpInfo1.Clear();
                    foreach (MapGroupInfo item in RecipeMgr.Inst.TempRcp.listMapGrpInfoL)
                    {
                        m_blistMapGrpInfo1.Add((MapGroupInfo)item.Clone());
                    }

                    m_blistMapGrpInfo2.Clear();
                    foreach (MapGroupInfo item in RecipeMgr.Inst.TempRcp.listMapGrpInfoR)
                    {
                        m_blistMapGrpInfo2.Add((MapGroupInfo)item.Clone());
                    }
                    cbScrapMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nScrapMode;
                }

                #endregion
            }
            else
            {
                #region UI2TempRcp

                #region 매거진 설정 탭
                if (
                    !double.TryParse(tbxAlignCorrOffsetX.Text, out RecipeMgr.Inst.TempRcp.dAlignXCorrOffset) ||
                    !double.TryParse(tbxMgzSlotPitch.Text, out RecipeMgr.Inst.TempRcp.MgzInfo.dSlotPitch) ||
                    !int.TryParse(tbxMgzSlotCount.Text, out RecipeMgr.Inst.TempRcp.MgzInfo.nSlotCount))
                {
                    ResetRecipeParameter(1);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("매거진 레시피 변경 입력값이 잘못되었습니다."), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }

                // 매거진 자재 유형(QUAD, STRIP) 설정
                RecipeMgr.Inst.TempRcp.MapTbInfo.isStripMat = ckbIsStrip.Checked;
                RecipeMgr.Inst.TempRcp.MapTbInfo.bGrabOneEdge = cbxGrabOnlyOneEdge.Checked;
                #endregion

                #region 맵 테이블 탭

                #region 상부검사

                #region 맵 인포 설정
                if (
                    !double.TryParse(tbxMapPitchX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX) ||
                    !double.TryParse(tbxMapPitchY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY) ||
                    !int.TryParse(tbxMapCountX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX) ||
                    !int.TryParse(tbxMapCountY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY) ||
                    !double.TryParse(tbxMapSizeX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeX) ||
                    !double.TryParse(tbxMapSizeY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeY) ||

                    !int.TryParse(tbxMapInspChipCountX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountX) ||
                    !int.TryParse(tbxMapInspChipCountY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountY) ||

                    !double.TryParse(tbxUnitGapX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitColGap) ||
                    !double.TryParse(tbxUnitGapY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitRowGap) ||
                    !double.TryParse(tbxMapGroupOffsetX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetX) ||
                    !double.TryParse(tbxMapGroupOffsetY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetY) ||
                    !int.TryParse(tbxMapGroupCntX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntX) ||
                    !int.TryParse(tbxMapGroupCntY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntY))
                {
                    ResetRecipeParameter(15);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("맵 테이블 상부 검사 탭의 맵 인포 레시피 변경 입력값이 잘못되었습니다."), "확인");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                if (
                    !double.TryParse(tbxChipThickness.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dChipThickness)
                   )
                {
                    RecipeMgr.Inst.TempRcp.MapTbInfo.dChipThickness = RecipeMgr.Inst.Rcp.MapTbInfo.dChipThickness;
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("칩 두께 설정이 잘못 되었습니다."), "확인");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                #endregion

                RecipeMgr.Inst.TempRcp.listMapGrpInfoL.Clear();
                foreach (MapGroupInfo item in m_blistMapGrpInfo1)
                {
                    RecipeMgr.Inst.TempRcp.listMapGrpInfoL.Add((MapGroupInfo)item.Clone());
                }

                RecipeMgr.Inst.TempRcp.listMapGrpInfoR.Clear();
                foreach (MapGroupInfo item in m_blistMapGrpInfo2)
                {
                    RecipeMgr.Inst.TempRcp.listMapGrpInfoR.Add((MapGroupInfo)item.Clone());
                }

                #endregion

                #endregion

                #region 트레이 탭

                #region 트레이 인포
                if (
                    !double.TryParse(tbxTrayPitchX.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchX) ||
                    !double.TryParse(tbxTrayPitchY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchY) ||
                    !double.TryParse(tbxTrayThick.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayThickness) ||
                    !int.TryParse(tbxTrayCountX.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX) ||
                    !int.TryParse(tbxTrayCountY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY) ||
                    !int.TryParse(cmbPickerPlaceT.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nPlaceAngle)
                    //!double.TryParse(tbxTrayInspOffsetY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipOffsetY) ||  트레이 인스펙션
                    )
                {
                    ResetRecipeParameter(16);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("트레이 탭 내부 트레이 인포 탭의 레시피 변경 입력값이 잘못되었습니다."), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                #endregion

                #region 플레이스 오프셋
                #region 헤드 1 플레이스 오프셋 설정
                if (
                    !double.TryParse(tbxGDTray1X_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].x) ||
                    !double.TryParse(tbxGDTray1Y_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].y) ||
                    !double.TryParse(tbxGDTray2X_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].x) ||
                    !double.TryParse(tbxGDTray2Y_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].y) ||
                    !double.TryParse(tbxRWTrayX_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].x) ||
                    !double.TryParse(tbxRWTrayY_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].y)
                    )
                {
                    ResetRecipeParameter(17);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("트레이 플레이스 오프셋 탭의 헤드1 플레이스 오프셋 레시피 변경 입력값이 잘못되었습니다."), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                #endregion

                #region 헤드 2 플레이스 오프셋 설정
                if (
                    !double.TryParse(tbxGDTray1X_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].x) ||
                    !double.TryParse(tbxGDTray1Y_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].y) ||
                    !double.TryParse(tbxGDTray2X_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].x) ||
                    !double.TryParse(tbxGDTray2Y_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].y) ||
                    !double.TryParse(tbxRWTrayX_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].x) ||
                    !double.TryParse(tbxRWTrayY_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].y)
                    )
                {
                    ResetRecipeParameter(18);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("트레이 플레이스 오프셋 탭의 헤드2 플레이스 오프셋 레시피 변경 입력값이 잘못되었습니다."), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                #endregion

                #endregion

                #endregion

                #region 클리닝 탭

                #region Sponge
                if (!double.TryParse(tbxSpongeVel.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.dUnitPkSpongeVel) ||
                    !long.TryParse(tbxSpongeStartDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lUnitPkSpongeStartDelay) ||
                    !long.TryParse(tbxSpongeEndDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lUnitPkSpongeEndDelay))
                {
                    RecipeMgr.Inst.TempRcp.cleaning.dUnitPkSpongeVel = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel;
                    RecipeMgr.Inst.TempRcp.cleaning.lUnitPkSpongeStartDelay = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeStartDelay;
                    RecipeMgr.Inst.TempRcp.cleaning.lUnitPkSpongeEndDelay = RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeEndDelay;
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("PLEASE ENTER A NUMBER IN THE SPONGE VALUE"), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                RecipeMgr.Inst.TempRcp.cleaning.nUnitPkSpongeCount = (int)numSpongeRepeat.Value;
                #endregion

                #region 스크랩
                if (!int.TryParse(tbScrap1Delay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.nScrap1Delay)||
                    !int.TryParse(tbScrap2Delay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.nScrap2Delay))
                {
                    ResetRecipeParameter(19);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("PLEASE ENTER A NUMBER IN THE STRAP VALUE"), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }

                RecipeMgr.Inst.TempRcp.cleaning.nScrapCount1 = (int)Scrap1_numericUpDown.Value;
                RecipeMgr.Inst.TempRcp.cleaning.nScrapCount2 = (int)Scrap2_numericUpDown.Value;

                RecipeMgr.Inst.TempRcp.cleaning.nScrapMode = cbScrapMode.SelectedIndex;
                #endregion

                #region 워터젯
                if (!long.TryParse(txbCleanSwingStartDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lWaterSwingStartDelay) ||
                    !long.TryParse(txbCleanSwingEndDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lnWaterSwingEndDelay))
                {
                    ResetRecipeParameter(20);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("PLEASE ENTER A NUMBER IN THE WATER & AIR SWING VALUE"), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                RecipeMgr.Inst.TempRcp.cleaning.nWaterSwingCnt = (int)numCleanSwingCnt.Value;
                #endregion

                #region 드라이
                if (!long.TryParse(tbxBtmDryStartDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lBtmDryStartDelay) ||
                    !long.TryParse(tbxTopDryStartDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lTopDryStartDelay) ||
                    !long.TryParse(tbxBtmDryEndDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lBtmDryEndDelay) ||
                    !long.TryParse(tbxTopDryEndDelay.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.lTopDryEndDelay) ||
                    !int.TryParse(tbxBottomDryVel.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.nBtmDryVel) ||
                    !int.TryParse(tbxTopDryVel.Text.ToString(), out RecipeMgr.Inst.TempRcp.cleaning.nTopDryVel))
                {
                    ResetRecipeParameter(21);
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("PLEASE ENTER A NUMBER IN THE UNIT DRY VALUE"), "CHECK");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    m_bIsUpdate = false;
                    return false;
                }
                RecipeMgr.Inst.TempRcp.cleaning.nBtmDryCnt = (int)numBtmDryCnt.Value;
                RecipeMgr.Inst.TempRcp.cleaning.nTopDryCnt = (int)numTopDryCnt.Value;
                RecipeMgr.Inst.TempRcp.cleaning.nRepickCount = (int)numRepickCount.Value;

                #endregion

                #endregion

                #endregion
            }

            m_bIsUpdate = false;

            return true;
        }

        
        public void ResetRecipeParameter(int cnt) // 저장 실패시 본래 값으로 되돌림 0516 KTH
        {
            switch (cnt)
            {
                case 1:
                    if (!double.TryParse(tbxAlignCorrOffsetX.Text, out RecipeMgr.Inst.TempRcp.dAlignXCorrOffset))
                        RecipeMgr.Inst.TempRcp.dAlignXCorrOffset = RecipeMgr.Inst.Rcp.dAlignXCorrOffset;
                    if (!double.TryParse(tbxMgzSlotPitch.Text, out RecipeMgr.Inst.TempRcp.MgzInfo.dSlotPitch))
                        RecipeMgr.Inst.TempRcp.MgzInfo.dSlotPitch = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch;
                    if (!int.TryParse(tbxMgzSlotCount.Text, out RecipeMgr.Inst.TempRcp.MgzInfo.nSlotCount))
                        RecipeMgr.Inst.TempRcp.MgzInfo.nSlotCount = RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount;
                        break;
                case 15:
                    if (!double.TryParse(tbxMapPitchX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX;
                    if (!double.TryParse(tbxMapPitchY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY;
                    if (!int.TryParse(tbxMapCountX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                    if (!int.TryParse(tbxMapCountY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;
                    if (!double.TryParse(tbxMapSizeX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
                    if (!double.TryParse(tbxMapSizeY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitSizeY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
                    if (!int.TryParse(tbxMapInspChipCountX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;
                    if (!int.TryParse(tbxMapInspChipCountY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;
                    if (!double.TryParse(tbxUnitGapX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitColGap))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitColGap = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitColGap;
                    if (!double.TryParse(tbxUnitGapY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitRowGap))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitRowGap = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitRowGap;

                    if (!double.TryParse(tbxMapGroupOffsetX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetX;
                    if (!double.TryParse(tbxMapGroupOffsetY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.dMapGroupOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dMapGroupOffsetY;
                    if (!int.TryParse(tbxMapGroupCntX.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntX))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX;
                    if (!int.TryParse(tbxMapGroupCntY.Text, out RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntY))
                        RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY;
                    break;
                case 16:
                    if (!double.TryParse(tbxTrayPitchX.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchX))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchX = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX;
                    if (!double.TryParse(tbxTrayPitchY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchY))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchY = RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
                    if (!double.TryParse(tbxTrayThick.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dTrayThickness))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dTrayThickness = RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness;
                    if (!int.TryParse(tbxTrayCountX.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX))
                        RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                    if (!int.TryParse(tbxTrayCountY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY))
                        RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY;
                    if (!int.TryParse(cmbPickerPlaceT.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.nPlaceAngle))
                        RecipeMgr.Inst.TempRcp.TrayInfo.nPlaceAngle = RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle;
                    //if(!double.TryParse(tbxTrayInspOffsetY.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipOffsetY))  트레이 인스펙션
                    //    RecipeMgr.Inst.TempRcp.TrayInfo.dInspChipOffsetY = RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY;
                        break;
                case 17:
                    if (!double.TryParse(tbxGDTray1X_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].x = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].x;
                    if (!double.TryParse(tbxGDTray1Y_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][0].y = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][0].y;
                    if (!double.TryParse(tbxGDTray2X_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].x = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].x;
                    if (!double.TryParse(tbxGDTray2Y_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[0][1].y = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[0][1].y;
                    if (!double.TryParse(tbxRWTrayX_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].x = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].x;
                    if (!double.TryParse(tbxRWTrayY_H1.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[0].y = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[0].y;
                        break;
                case 18:
                    if (!double.TryParse(tbxGDTray1X_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].x = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].x;
                    if (!double.TryParse(tbxGDTray1Y_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][0].y = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][0].y;
                    if (!double.TryParse(tbxGDTray2X_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].x = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].x;
                    if (!double.TryParse(tbxGDTray2Y_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dGdTrayOffset[1][1].y = RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[1][1].y;
                    if (!double.TryParse(tbxRWTrayX_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].x))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].x = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].x;
                    if (!double.TryParse(tbxRWTrayY_H2.Text, out RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].y))
                        RecipeMgr.Inst.TempRcp.TrayInfo.dRwTrayOffset[1].y = RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[1].y;
                        break;

                case 19:
                    if (!int.TryParse(tbScrap1Delay.Text, out RecipeMgr.Inst.TempRcp.cleaning.nScrap1Delay))
                        RecipeMgr.Inst.TempRcp.cleaning.nScrap1Delay = RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay;
                    if (!int.TryParse(tbScrap2Delay.Text, out RecipeMgr.Inst.TempRcp.cleaning.nScrap2Delay))
                        RecipeMgr.Inst.TempRcp.cleaning.nScrap2Delay = RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay;
                    break;


                case 20:
                    if (!long.TryParse(txbCleanSwingStartDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lWaterSwingStartDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lWaterSwingStartDelay = RecipeMgr.Inst.Rcp.cleaning.lWaterSwingStartDelay;
                    if (!long.TryParse(txbCleanSwingEndDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lnWaterSwingEndDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lnWaterSwingEndDelay = RecipeMgr.Inst.Rcp.cleaning.lnWaterSwingEndDelay;
                    break;

                case 21:
                    if (!long.TryParse(tbxBtmDryStartDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lBtmDryStartDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lBtmDryStartDelay = RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay;
                    if (!long.TryParse(tbxTopDryStartDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lTopDryStartDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lTopDryStartDelay = RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay;
                    if (!long.TryParse(tbxBtmDryEndDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lBtmDryEndDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lBtmDryEndDelay = RecipeMgr.Inst.Rcp.cleaning.lBtmDryEndDelay;
                    if (!long.TryParse(tbxTopDryEndDelay.Text, out RecipeMgr.Inst.TempRcp.cleaning.lTopDryEndDelay))
                        RecipeMgr.Inst.TempRcp.cleaning.lTopDryEndDelay = RecipeMgr.Inst.Rcp.cleaning.lTopDryEndDelay;

                    if (!int.TryParse(tbxBottomDryVel.Text, out RecipeMgr.Inst.TempRcp.cleaning.nBtmDryVel))
                        RecipeMgr.Inst.TempRcp.cleaning.nBtmDryVel = RecipeMgr.Inst.Rcp.cleaning.nBtmDryVel;
                    if (!int.TryParse(tbxTopDryVel.Text, out RecipeMgr.Inst.TempRcp.cleaning.nTopDryVel))
                        RecipeMgr.Inst.TempRcp.cleaning.nTopDryVel = RecipeMgr.Inst.Rcp.cleaning.nTopDryVel;

                    break;
            }

        }
        private void OnRcpDeviceSelectSubChangeButtonClick(object sender, EventArgs e)
        {
            RadioButton rdb = sender as RadioButton;

            if (rdb.Checked)
            {
                int nTag;
                if (int.TryParse(rdb.Tag.ToString(), out nTag))
                {
                    tbcRcpDeviceSetting.SelectedIndex = nTag;
                }
            }
        }

        //private void OnRcpLdUldSettingSubChangeButtonClick(object sender, EventArgs e)
        //{
        //    RadioButton rdb = sender as RadioButton;

        //    if (rdb.Checked)
        //    {
        //        int nTag;
        //        if (int.TryParse(rdb.Tag.ToString(), out nTag))
        //        {
        //            tbcRcpLdUldSetting.SelectedIndex = nTag;
        //        }
        //    }
        //}

        private void cbUnloadingElevatorAxes_SelectedIndexChanged(object sender, EventArgs e)
        {

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

        private void btnRcpSave_Click(object sender, EventArgs e)
        {
            if (!CheckValidation()) return;

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Do you want to save recipe parameters?"), "SAVE");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;
           
            CalcPitchOffset();

            bool bChangedStripInfo = false;
            //UI의 레시피 값을 temp에 넣는다.
            if (!UpdateRcpData(true)) return;
         
            if (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX != RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX ||
                RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY != RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY || 
                RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX != RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX ||
                RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY!= RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY)
            {
                bChangedStripInfo = true;

                if (RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX != RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntX ||
                    RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY != RecipeMgr.Inst.TempRcp.MapTbInfo.nMapGroupCntY ||
                    RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX != RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX ||
                    RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY != RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY)
                {
                    msg = new popMessageBox(FormTextLangMgr.FindKey("생산 중에 Map Unit Count를 바꾸면 정보가 초기화 됩니다. 진행하시겠습니까?"), "SAVE");
                    msg.TopMost = true;
                    if (msg.ShowDialog(this) != DialogResult.OK) return;
                }
            }


            if (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX != RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX ||
                RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY != RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY)
            {
                //bChangedTrayInfo = true;
                msg = new popMessageBox(FormTextLangMgr.FindKey("생산 중에 Tray Unit Count를 바꾸면 정보가 초기화 됩니다. 진행하시겠습니까?"), "SAVE");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;
            }
           

            SaveRecipe();
            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "RCP Save Complete");


            if (bChangedStripInfo)
            {
                IFMgr.Inst.VISION.UpdateRecipeData();
            }
          
        }

        public void SaveRecipe(string strmsg = "Recipe Saving Completed!")
        {
            RecipeMgr.Inst.BackupRcpFile(RecipeMgr.Inst.LAST_MODEL_NAME);

            RecipeMgr.Inst.SaveTempRcp();
            RecipeMgr.Inst.Save();

            RcpDevMgr.Inst.SaveDevMgz(RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName);
            RcpDevMgr.Inst.SaveDevMap(RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName);
            RcpDevMgr.Inst.SaveDevTray(RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName);

            RecipeMgr.Inst.Init();
            UpdateRcpData();
            
            if(this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    CloseDigPopRmsRun();
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey(strmsg), "RECIPE SAVE", MessageBoxButtons.OK);
                    msg.TopMost = true;
                    msg.Show();
                });
            }
            else
            {
                CloseDigPopRmsRun();
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey(strmsg), "RECIPE SAVE", MessageBoxButtons.OK);
                msg.TopMost = true;
                msg.Show();
            }
        }


        //220510 pjh
        //칩피커 픽 엔 플레이스위치만 기존 Teach에있던 파라미터를 Recipe로 옴김.
        //때문에 기존에 연결되어있는 Teach변수를 사용하기 위해 Recipe에 저장한 값을 Teach에 복사한다.
        public void CopyToTeachPos()
        {
            #region Loading Rail
            //Loading
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_LOADING];

            //Unloadnig
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_UNLOADING];

            //Grip Safety
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_GRIP] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_GRIP];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY];

            //Grip
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_GRIP];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_GRIP];

            //Ungrip
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP];

            //Ungrip Safety
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY];

            //Centering
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_REAR].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_Y_FRONT].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

            //Barcode
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.BARCODE_Y].dPos[POSDF.BOT_BARCDOE_SCAN];
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
            #endregion

            #region Pre align
            //Grab 1
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_RAIL_T].dPos[POSDF.LD_RAIL_T_STRIP_ALIGN];

            //Grab
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ALIGN_2];
            #endregion

            #region Strip Info
            //Orient
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK];
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_ORIENT_CHECK];

            //2D Code
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.STRIP_PK_Z].dPos[POSDF.STRIP_PICKER_STRIP_2D_CODE];
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.LD_VISION_X_STRIP_BARCODE];
            #endregion

            #region Inspection Pos
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
            {

                //T1 Align
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1] =
                RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                try
                {
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK2] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosY -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                }
                catch (Exception)
                {
                }
                
            }
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
            {

                //T2 Align
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T2] =
                RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                try
                {
                    TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK2] =
                  RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosY -
                  (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
                }
                catch (Exception)
                {

                }
              
            }
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
            {

                //T1 Inspection
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1] =
                RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_MAP_VISION_START] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
            }
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
            {

                //T2 Inspection
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T2] =
                RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX -
                (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_MAP_VISION_START] =
                    RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY -
                    (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }
            #endregion

            #region Pick Up            
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count > 0)
            {
                //T1
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
            ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).x;


            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1] =
            ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).x;

            // T1 VISION
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1] =
            RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1;

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1] =
            RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2;

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
            ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
                ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;
            }
            if (RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count > 0)
            {

                //T2
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
            ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).x;

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2] =
            ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).x;

            // T2 VISION
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T2] =
            RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1;

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T2] =
            RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2;


                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] =
            ConfigMgr.Inst.GetCamToPickerupPos(0, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;

                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] =
                ConfigMgr.Inst.GetCamToPickerupPos(1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;
            }

            //TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P1] =
            //RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P1];

            //TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P2] =
            //RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P2];
            #endregion

            #region Place
            //GD1
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];


            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


            //GD2
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];


            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];


            //RW
            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];


            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2] =
            RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];
#endregion
        }

        public void UpdateRcpConfirm()
        {
            try
            {
                int nValue = 0;
                double dValue = 0.0;

#region MGZ
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Count].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.MgzInfo.nSlotCount = nValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Mgz_Slot_Pitch].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.MgzInfo.dSlotPitch = dValue;
#endregion

#region MAP
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_X].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchX = dValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Pitch_Y].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.dUnitPitchY = dValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_X].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountX = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Unit_Count_Y].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.nUnitCountY = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_X].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountX = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Map_Insp_Chip_View_Count_Y].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.MapTbInfo.nInspChipViewCountY = nValue;
#endregion

#region ULD TRAY
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_X].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountX = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Count_Y].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.TrayInfo.nTrayCountY = nValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_X].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchX = dValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Pitch_Y].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.TrayInfo.dTrayPitchY = dValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Thickness].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.TrayInfo.dTrayThickness = dValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_X].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.TrayInfo.nInspChipCountX = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Tray_Insp_Chip_Count_Y].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.TrayInfo.nInspChipCountY = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Picker_Place_T].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.TrayInfo.nPlaceAngle = nValue;
                #endregion

                #region CLEANER
                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nUnitPkSpongeCount = nValue;
                //if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Sponge_Velocity].After, out dValue)) dValue = 0.0;
                //RecipeMgr.Inst.TempRcp.cleaning.dUnitPkSpongeVel = dValue;

                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Water_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nFirstCleanWaterCount = nValue;

                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Air_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nUnitPkAirCleanCount = nValue;

                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Count].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.nUnitPkDryBlowCount = nValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Dry_Blow_Velocity].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.cleaning.dUnitPkDryBlowVel = dValue;


                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Unit_Pk_Kit_Air_Blow_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nUnitPkKitAirBlowCount = nValue;


                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Water_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nCleanTableWaterCount = nValue;
                //if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Table_Air_Count].After, out nValue)) nValue = 0;
                //RecipeMgr.Inst.TempRcp.cleaning.nCleanTableAirCount = nValue;

                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Count].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.nDryBlockAirCount = nValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dryblock_Air_Velocity].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.cleaning.dDryBlockAirVel = dValue;


                //if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Dry_block_Unloading_Velocity].After, out dValue)) dValue = 0.0;
                //RecipeMgr.Inst.TempRcp.cleaning.dDryBlockUnloadingVel = dValue;



                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Count].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.nStageBlockDryBlowCount = nValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Velocity].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.cleaning.dStageBlockDryBlowVel = dValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Stage_Block_Dry_Blow_Delay].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.lStageBlockDryBlowDelay = nValue;


                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count1].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.nMapTableDryBlowCount1 = nValue;
                if (!int.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Count2].After, out nValue)) nValue = 0;
                RecipeMgr.Inst.TempRcp.cleaning.nMapTableDryBlowCount2 = nValue;

                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity1].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.cleaning.dMapTableDryBlowVel1 = dValue;
                if (!double.TryParse(MESDF.EqRcpParam[(int)MESDF.eRCP_ITEMS.Clean_Map_Table_Dry_Blow_Velocity2].After, out dValue)) dValue = 0.0;
                RecipeMgr.Inst.TempRcp.cleaning.dMapTableDryBlowVel2 = dValue;
                #endregion
            }
            catch (Exception)
            {

            }
        }

        public void UpdateData()
        {
            RecipeMgr.Inst.CopyTempRcp();
        }

        bool CheckValidation()
        {
            popMessageBox popMsgWin = null;
            string msg = "";

            Recipe tempRcp = RecipeMgr.Inst.TempRcp;
            double dValue = 0.0;
            //double.TryParse(tbFirstUltrasonicSwingOffset.Text, out dValue);
            //if (dValue >= 3)
            //{
            //    MessageBox.Show(String.Format("반복 오프셋이 충돌 가능성이 있습니다. 수정 하십시오 {0}",dValue));
            //    return false;
            //}
            //double.TryParse(tbSecondUltrasonicSwingOffset.Text, out dValue);
            //if(dValue >= 10)
            //{
            //    MessageBox.Show(String.Format("반복 오프셋이 충돌 가능성이 있습니다. 수정 하십시오 {0}", dValue));
            //    return false;
            //}

                return true;
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
                if (GbVar.bChangeRecipe_Recipe == true)
                {
                    ShowModelFileList();
                    GbVar.bChangeRecipe_Recipe = false;
                }
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
                //if(tabPageAxes.SelectedIndex == 4)
                //{
                //    for (int i = 0; i < m_lbPkSvOn.Length; i++)
                //    {
                //        if (MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i].IsAlarm())
                //        {
                //            if (m_lbPkSvOn[i].BackColor != Color.Red) m_lbPkSvOn[i].BackColor = Color.Red;
                //        }
                //        else m_lbPkSvOn[i].BackColor = MotionMgr.Inst[(int)SVDF.AXES.CHIP_PK_1_Z_1 + i].GetServoOnOff() ? Color.Lime : SystemColors.Control;
                //    }
                //}
                
                ////                foreach (ucServoStatus item in m_listServoStatus)
                ////                {
                ////#if _AJIN
                ////                    item.RefreshUI();
                ////#endif
                ////                }

                //m_Servos[PageNo].RefreshUI();
#endif

                if (GbDev.BarcodeReader.IsConnect())
                {
                    if (panelConnectStatusBarcode.BackColor != Color.Green)
                        panelConnectStatusBarcode.BackColor = Color.Green;
                }
                else
                {
                    if (panelConnectStatusBarcode.BackColor != Color.Red)
                        panelConnectStatusBarcode.BackColor = Color.Red;
                }

                if (GbDev.MGZBarcodeReader.IsConnect)
                {
                    if (panelConnectStatusMgzBarcode.BackColor != Color.Green)
                        panelConnectStatusMgzBarcode.BackColor = Color.Green;
                }
                else
                {
                    if (panelConnectStatusMgzBarcode.BackColor != Color.Red)
                        panelConnectStatusMgzBarcode.BackColor = Color.Red;
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
        static void FindControl(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(UC.RcpMotPosGetButtonEx) ||
                    ctrl.GetType() == typeof(UC.RcpMotPosEditButtonEx) ||
                    ctrl.GetType() == typeof(UC.RcpMotPosMoveButtonEx) ||
                    ctrl.GetType() == typeof(UC.DataGridViewEx) ||
                    ctrl.GetType() == typeof(GlassButton) ||
                    ctrl.GetType() == typeof(ComboBox) ||
                    ctrl.GetType() == typeof(Button) ||
                    ctrl.GetType() == typeof(NumericUpDown) ||
                    ctrl.GetType() == typeof(CheckBox) ||
                    ctrl.GetType() == typeof(TextBox))
                {
                    try
                    {
                        
                        if (ctrl.Name == "btnRcpChange")
                        {
                            continue;
                        }
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
        private void frmRecipe_VisibleChanged(object sender, EventArgs e)
        {
                   //20211126 CHOH: 다른화면 나갔다오면 레시피 다시 표시
            if (this.Visible == true)
            {
                if (GbVar.CurLoginInfo.USER_LEVEL == MCDF.LEVEL_OP)
                {
                    btnRcpSave.Enabled = false;
                    btnRcpDelete.Enabled = false;
                    btnRcpCopy.Enabled = false;
                }
                else
                {
                    btnRcpSave.Enabled = true;
                    btnRcpDelete.Enabled = true;
                    btnRcpCopy.Enabled = true;
                }
                RecipeMgr.Inst.CopyTempRcp();
                UpdateRcpData();
                FindControl(this.Controls);
            }

            switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
            {
                case Language.ENGLISH:
                    {
                        cbPikcerToMove_PickUp.Text = "Picker to move";
                        cbScrapMode.Items.Clear();
                        cbScrapMode.Items.AddRange(new object[] {
                        "SCRAP AT THE SAME TIME",
                        "SCRAP DIVIDED"});
                    }
                    break;
                case Language.CHINA:
                    {
                        cbPikcerToMove_PickUp.Text = "手臂移动";
                        cbScrapMode.Items.Clear();
                        cbScrapMode.Items.AddRange(new object[] {
                        "同时报废",
                        "废料分割"});
                    }
                    break;
                case Language.KOREAN:
                    {
                        cbPikcerToMove_PickUp.Text = "피커로 이동";
                        cbScrapMode.Items.Clear();
                        cbScrapMode.Items.AddRange(new object[] {
                        "동시에",
                        "나눠서"});
                    }
                    break;
                default:
                    break;
            }
            cbScrapMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nScrapMode;
            cbPikcerToMove.Text = cbPikcerToMove_PickUp.Text;
            cbxGrabOnlyOneEdge.Text = FormTextLangMgr.FindKey(cbxGrabOnlyOneEdge.Text);

            tmrRefresh.Enabled = this.Visible;
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
        }

        void item_servoHomeClicked(ucServoStatus ucSvStatus, SVDF.AXES axis)
        {
            GbSeq.manualRun.SetSingleHome(axis);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.SINGLE_HOME);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

#region CleaningPage

        void UpdateListbox()
        {
            for (int i = 0; i < m_lbxCleaning.Length; i++)
            {
                m_lbxCleaning[i].Items.Clear();

                List<CleaningItem_NotUse> _ListCleaningItem;
                if (i == 0) _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.UnitTrsfItem;
                else if (i == 1) _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.CleanTableItem;
                else _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.DryTableItem;

                for (int nCnt = 0; nCnt < _ListCleaningItem.Count; nCnt++)
                {
                    string strType = "";
                    if (_ListCleaningItem[nCnt].nType == (int)CLEAN_TYPE.SPONGE) strType = "스펀지를";
                    else if (_ListCleaningItem[nCnt].nType == (int)CLEAN_TYPE.WATER_CLEAN) strType = "워터클리닝을";
                    else if (_ListCleaningItem[nCnt].nType == (int)CLEAN_TYPE.AIR_CLEAN) strType = "에어클리닝을";
                    else if (_ListCleaningItem[nCnt].nType == (int)CLEAN_TYPE.BOX_BLOW) strType = "박스블로우를";
                    m_lbxCleaning[i].Items.Add(string.Format("{0} {1}회 진행", strType, _ListCleaningItem[nCnt].nRepeatCount));
                }
            }
        }

        private void CleaningRecipeButton_Click(object sender, EventArgs e)
        {
            // 추가, 제거, 위로, 아래로
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            int nSelectedCount = -1;
            
            if (m_lbxCleaning[nTag].Items.Count > 0)
            {
                nSelectedCount = m_lbxCleaning[nTag].SelectedIndex;
            }

            int _nMode = m_cbxCleaning[nTag].SelectedIndex;
            int _nCount = (int)m_numCleaning[nTag].Value;
            List<CleaningItem_NotUse> _ListCleaningItem;

            if (nTag == 0) _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.UnitTrsfItem;
            else if (nTag == 1) _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.CleanTableItem;
            else _ListCleaningItem = RecipeMgr.Inst.TempRcp.cleaning.DryTableItem;

            switch (btn.Text)
            {
                case "추가":
                    {
                        _ListCleaningItem.Add(new CleaningItem_NotUse(_nMode, _nCount));
                        UpdateListbox();
                        m_lbxCleaning[nTag].SelectedIndex = nSelectedCount;
                    }
                    break;
                case "제거":
                    {
                        if (nSelectedCount < 0) return;
                        _ListCleaningItem.RemoveAt(nSelectedCount);
                        UpdateListbox();
                        nSelectedCount--;
                        if (nSelectedCount < 0) nSelectedCount = 0;
                        m_lbxCleaning[nTag].SelectedIndex = nSelectedCount;
                    }
                    break;
                case "위로":
                    {
                        if (nSelectedCount < 1) return;
                        CleaningItem_NotUse tmpItem = _ListCleaningItem[nSelectedCount];
                        _ListCleaningItem.RemoveAt(nSelectedCount);
                        nSelectedCount--;
                        _ListCleaningItem.Insert(nSelectedCount, tmpItem);
                        UpdateListbox();
                        m_lbxCleaning[nTag].SelectedIndex = nSelectedCount;
                    }
                    break;
                case "아래로":
                    {
                        if (nSelectedCount < 0) return;
                        if (nSelectedCount >= _ListCleaningItem.Count - 1) return;
                        CleaningItem_NotUse tmpItem = _ListCleaningItem[nSelectedCount];
                        _ListCleaningItem.RemoveAt(nSelectedCount);
                        nSelectedCount++;
                        _ListCleaningItem.Insert(nSelectedCount, tmpItem);
                        UpdateListbox();
                        m_lbxCleaning[nTag].SelectedIndex = nSelectedCount;
                    }
                    break;
                default:
                    break;
            }
        }

#endregion

        private void ShowLdMgzInfoList()
        {
            DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_DEV_MGR_MGZ);

            dgvMgzInfo.Rows.Clear();
            dgvMgzInfo.RowCount = 100; //최대 100개

            FileInfo[] sortedFiles = null;
            try
            {
                sortedFiles = dir.GetFiles("*.xml").OrderBy(x => int.Parse(x.Name.Replace(".xml", ""))).ToArray();
            }
            catch (Exception)
            {
                sortedFiles = dir.GetFiles("*.xml").ToArray();
            }

            m_fLdMgzInfo = sortedFiles;

            int nCnt = 0;
            foreach (FileInfo f in sortedFiles)
            {
                dgvMgzInfo.Rows[nCnt].Cells[0].Value = nCnt + 1;
                dgvMgzInfo.Rows[nCnt].Cells[1].Value = f.Name.Replace(".xml", "");
                nCnt++;
                if (nCnt >= 100) break;
            }
        }

        private void ShowMapTableInfoList()
        {
            //DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_DEV_MGR_MAP);

            //dgvMapTableInfo.Rows.Clear();
            //dgvMapTableInfo.RowCount = 100; //최대 100개

            //FileInfo[] sortedFiles = null;
            //try
            //{
            //    sortedFiles = dir.GetFiles("*.xml").OrderBy(x => int.Parse(x.Name.Replace(".xml", ""))).ToArray();
            //}
            //catch (Exception)
            //{
            //    sortedFiles = dir.GetFiles("*.xml").ToArray();
            //}

            //m_fMapTableInfo = sortedFiles;

            //int nCnt = 0;
            //foreach (FileInfo f in sortedFiles)
            //{
            //    dgvMapTableInfo.Rows[nCnt].Cells[0].Value = nCnt + 1;
            //    dgvMapTableInfo.Rows[nCnt].Cells[1].Value = f.Name.Replace(".xml", "");
            //    nCnt++;
            //    if (nCnt >= 100) break;
            //}
        }

        private void ShowUldTrayInfoList()
        {
            DirectoryInfo dir = new DirectoryInfo(PathMgr.Inst.PATH_DEV_MGR_TRAY);

            dgvUldTrayInfo.Rows.Clear();
            dgvUldTrayInfo.RowCount = 100; //최대 100개

            FileInfo[] sortedFiles = null;
            try
            {
                sortedFiles = dir.GetFiles("*.xml").OrderBy(x => int.Parse(x.Name.Replace(".xml", ""))).ToArray();
            }
            catch (Exception)
            {
                sortedFiles = dir.GetFiles("*.xml").ToArray();
            }

            m_fUldTrayInfo = sortedFiles;

            int nCnt = 0;
            foreach (FileInfo f in sortedFiles)
            {
                dgvUldTrayInfo.Rows[nCnt].Cells[0].Value = nCnt + 1;
                dgvUldTrayInfo.Rows[nCnt].Cells[1].Value = f.Name.Replace(".xml", "");
                nCnt++;
                if (nCnt >= 100) break;
            }
        }

        private void btnNewMgzInfo_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want create File ?", "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            popCreateDeviveFile devDlg = new popCreateDeviveFile();

            if (devDlg.ShowDialog() == DialogResult.OK)
            {

                if (!RcpDevMgr.Inst.FindFileName(0, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Already File.");
                    return;
                }

                if (!RcpDevMgr.Inst.CreateDevFile(0, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Failed to create file !!!");
                }

                ShowLdMgzInfoList();
            }
        }

        private void btnDeleteMgzInfo_Click(object sender, EventArgs e)
        {
            try
            {
                string strDelFileName = dgvMgzInfo.Rows[dgvMgzInfo.CurrentCell.RowIndex].Cells[1].Value + ".xml";
                DialogResult result = MessageBox.Show(string.Format(FormTextLangMgr.FindKey("Do you want to delete")+ " [{0}]" +FormTextLangMgr.FindKey("device file?") , strDelFileName),
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                if (strDelFileName == RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName)
                {
                    MessageBox.Show("Cannot delete. It's Current use device file.");
                    return;
                }

                if (dgvMgzInfo.Rows.Count <= 1)
                {
                    MessageBox.Show("Cannot delete. It's Last Model.");
                    return;
                }

                RcpDevMgr.Inst.DeleteMgzInfoFile(strDelFileName);
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnDeleteMgzInfo_Click" + ", [Deleted Model Name: " + strDelFileName + ".xml]");

                ShowLdMgzInfoList();
            }
            catch (System.Exception ex)
            {
                //
            }
        }

        private void btnChangeMgzInfo_Click(object sender, EventArgs e)
        {
            if (dgvMgzInfo.CurrentCell == null) return;
            if (dgvMgzInfo.Rows[dgvMgzInfo.CurrentCell.RowIndex].Cells[1].Value == null) return;
            string strChangeId = dgvMgzInfo.Rows[dgvMgzInfo.CurrentCell.RowIndex].Cells[1].Value.ToString();


            DialogResult result = MessageBox.Show("Do you want to change device file ? [" + strChangeId + "]",
                                                  "Confirm!",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question,
                                                  MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnChangeMgzInfo_Click" + ", Device file Changed: [" + RecipeMgr.Inst.Rcp.MgzInfo.strMgzFileName + " --> " + strChangeId + ".xml]");

            RcpDevMgr.Inst.ChangeDevFile(0, strChangeId + ".xml");
            //ShowLdMgzInfoList();

            UpdateRcpData();
        }

        private void btnNewMapInfo_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want create File ?", "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            popCreateDeviveFile devDlg = new popCreateDeviveFile();

            if (devDlg.ShowDialog() == DialogResult.OK)
            {

                if (!RcpDevMgr.Inst.FindFileName(1, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Already File.");
                    return;
                }

                if (!RcpDevMgr.Inst.CreateDevFile(1, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Failed to create file !!!");
                }

                ShowMapTableInfoList();
            }
        }

        private void btnDelMapInfo_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    string strDelFileName = dgvMapTableInfo.Rows[dgvMapTableInfo.CurrentCell.RowIndex].Cells[1].Value + ".xml";
            //    DialogResult result = MessageBox.Show(string.Format("Do you want to delete [{0}] device file? ", strDelFileName),
            //                                          "Confirm!",
            //                                          MessageBoxButtons.YesNo,
            //                                          MessageBoxIcon.Question,
            //                                          MessageBoxDefaultButton.Button2);
            //    if (result != System.Windows.Forms.DialogResult.Yes)
            //    {
            //        return;
            //    }

            //    if (strDelFileName == RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName)
            //    {
            //        MessageBox.Show("Cannot delete. It's Current use device file.");
            //        return;
            //    }

            //    if (dgvMapTableInfo.Rows.Count <= 1)
            //    {
            //        MessageBox.Show("Cannot delete. It's Last Model.");
            //        return;
            //    }

            //    RcpDevMgr.Inst.DeleteMapInfoFile(strDelFileName);
            //    GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnDelMapInfo_Click" + ", [Deleted Model Name: " + strDelFileName + ".xml]");

            //    ShowMapTableInfoList();
            //}
            //catch (System.Exception ex)
            //{
            //    //
            //}
        }

        private void btnChangeMapnfo_Click(object sender, EventArgs e)
        {
            //if (dgvMapTableInfo.CurrentCell == null) return;
            //if (dgvMapTableInfo.Rows[dgvMapTableInfo.CurrentCell.RowIndex].Cells[1].Value == null) return;
            //string strChangeId = dgvMapTableInfo.Rows[dgvMapTableInfo.CurrentCell.RowIndex].Cells[1].Value.ToString();


            //DialogResult result = MessageBox.Show("Do you want to change device file ? [" + strChangeId + "]",
            //                                      "Confirm!",
            //                                      MessageBoxButtons.YesNo,
            //                                      MessageBoxIcon.Question,
            //                                      MessageBoxDefaultButton.Button2);
            //if (result != System.Windows.Forms.DialogResult.Yes)
            //{
            //    return;
            //}

            //GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnChangeMapnfo_Click" + ", Device file Changed: [" + RecipeMgr.Inst.Rcp.MapTbInfo.strMapTbFileName + " --> " + strChangeId + ".xml]");

            //RcpDevMgr.Inst.ChangeDevFile(1, strChangeId + ".xml");
            ////ShowMapTableInfoList();

            //UpdateRcpData();
        }

        private void btnNewTrayInfo_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want create File ?", "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            popCreateDeviveFile devDlg = new popCreateDeviveFile();

            if (devDlg.ShowDialog() == DialogResult.OK)
            {

                if (!RcpDevMgr.Inst.FindFileName(2, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Already File.");
                    return;
                }

                if (!RcpDevMgr.Inst.CreateDevFile(2, devDlg.NEW_FILE_NAME))
                {
                    MessageBox.Show("Failed to create file !!!");
                }

                ShowUldTrayInfoList();
            }
        }

        private void btnDelTrayInfo_Click(object sender, EventArgs e)
        {
            try
            {
                string strDelFileName = dgvUldTrayInfo.Rows[dgvUldTrayInfo.CurrentCell.RowIndex].Cells[1].Value + ".xml";
                DialogResult result = MessageBox.Show(string.Format(FormTextLangMgr.FindKey("Do you want to delete") + " [{0}]" + FormTextLangMgr.FindKey("device file?"), strDelFileName),
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                if (strDelFileName == RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName)
                {
                    MessageBox.Show("Cannot delete. It's Current use device file.");
                    return;
                }

                if (dgvUldTrayInfo.Rows.Count <= 1)
                {
                    MessageBox.Show("Cannot delete. It's Last Model.");
                    return;
                }

                RcpDevMgr.Inst.DeleteTrayInfoFile(strDelFileName);
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnDelTrayInfo_Click" + ", [Deleted Model Name: " + strDelFileName + ".xml]");

                ShowUldTrayInfoList();
            }
            catch (System.Exception ex)
            {
                //
            }
        }

        private void btnChangeTrayInfo_Click(object sender, EventArgs e)
        {
            if (dgvUldTrayInfo.CurrentCell == null) return;
            if (dgvUldTrayInfo.Rows[dgvUldTrayInfo.CurrentCell.RowIndex].Cells[1].Value == null) return;
            string strChangeId = dgvUldTrayInfo.Rows[dgvUldTrayInfo.CurrentCell.RowIndex].Cells[1].Value.ToString();


            DialogResult result = MessageBox.Show("Do you want to change device file ? [" + strChangeId + "]",
                                                  "Confirm!",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question,
                                                  MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnChangeTrayInfo_Click" + ", Device file Changed: [" + RecipeMgr.Inst.Rcp.TrayInfo.strTrayFileName + " --> " + strChangeId + ".xml]");

            RcpDevMgr.Inst.ChangeDevFile(2, strChangeId + ".xml");
            //ShowUldTrayInfoList();

            UpdateRcpData();
        }

        private void btnSetInspConfigPos_Click(object sender, EventArgs e)
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

        private void gridModel_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && (e.PaintParts & DataGridViewPaintParts.Background) == DataGridViewPaintParts.Background)
            //{
            //    string[] arr = RecipeMgr.Inst.LAST_MODEL_NAME.Split('.');
            //    int nCurrRcpNo = int.Parse(arr[0]);

            //    if (e.RowIndex == nCurrRcpNo - 1)
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.DarkOrange))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    else if (e.RowIndex == gridModel.CurrentCell.RowIndex)
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.FromArgb(192, 255, 255)))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        using (Brush gridBrush = new SolidBrush(this.gridModel.GridColor))
            //        {
            //            using (Brush backColorBrush = new SolidBrush(Color.FromArgb(42, 42, 42)))
            //            {
            //                using (Pen gridLinePen = new Pen(gridBrush))
            //                {
            //                    // Clear cell
            //                    e.Graphics.FillRectangle(backColorBrush, e.CellBounds);
            //                    //force paint of content
            //                    e.PaintContent(e.ClipBounds);
            //                    e.Handled = true;
            //                }
            //            }
            //        }
            //    }
            //    DataGridViewPaintParts paintParts = e.PaintParts & ~DataGridViewPaintParts.Background;
            //    e.Paint(e.ClipBounds, paintParts);
            //    e.Handled = true;
            //}
        }

        private void gridModel_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (gridModel.CurrentCell == null) return;
            int nRow = gridModel.CurrentCell.RowIndex;      //Y
            int nCol = gridModel.CurrentCell.ColumnIndex;   //X

            // Model Name Change
            if (nCol == 2)
            {
                if (!RecipeMgr.Inst.ModelDescriptionChange(gridModel.Rows[nRow].Cells[1].Value.ToString() + ".mdl", gridModel.Rows[nRow].Cells[2].Value.ToString()))
                {
                    MessageBox.Show("Failed to Recipe Description!!!");
                }
            }
        }

        private void gridModel_CurrentCellChanged(object sender, EventArgs e)
        {
            if (gridModel.CurrentCell == null) return;
            if (gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value == null) return;
            txtRcpSoruce.Text = gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value.ToString();
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

        private void btnRcpCopy_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(FormTextLangMgr.FindKey("Do you want Copy File ? "), "Confirm!", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            //string targetModelName = tbxRcpTarget.Text;
            string targetModelName = tbxTargetRecipeID.Text + ".mdl";

            if (!FindFileName(targetModelName))
            {
                MessageBox.Show("Already Model.");
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpCopy_Click" + ", Model Copy [" + txtRcpSoruce.Text + ".mdl]" + " --> [" + targetModelName + "]");
                return;
            }

            if (!RecipeMgr.Inst.CopyToModel(cmbTargetRecipeIdx.SelectedItem.ToString(), tbxTargetRecipeID.Text + ".mdl"))
            {
                MessageBox.Show("Failed to copy model!!!");
            }

            popMessageBox pop = new popMessageBox("RECIPE COPY SUCCESS!", "RECIPE COPY", MessageBoxButtons.OK);
            pop.ShowDialog();
            ShowModelFileList();
        }

        private void btnRcpDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string strDeleteRcpPPID = txtRcpSoruce.Text;
                string strDelRecipe = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\WorkFile\\" + strDeleteRcpPPID + ".mdl";

                if (!File.Exists(strDelRecipe))
                {
                    MessageBox.Show(strDeleteRcpPPID + FormTextLangMgr.FindKey(" Delete recipe file not found!"));
                    return;
                }

                DialogResult result = MessageBox.Show(string.Format(FormTextLangMgr.FindKey("Do you want to delete") + " [{0}]" + FormTextLangMgr.FindKey("recipe file?"), strDeleteRcpPPID),
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                if (strDeleteRcpPPID == RecipeMgr.Inst.LAST_MODEL_NAME.Replace(".mdl", ""))
                {
                    MessageBox.Show("Cannot delete. It's Current Model.");
                    return;
                }

                if (gridModel.Rows.Count <= 1)
                {
                    MessageBox.Show("Cannot delete. It's Last Model.");
                    return;
                }

                RecipeMgr.Inst.DeleteModelFile(strDeleteRcpPPID);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse == true)
                {
                    //MES
                    if (eqRcpChange != null)
                    {
                        eqRcpChange(MESDF.ePPChangeCode.DELETE, tbxTargetRecipeID.Text);
                        ShowDigPopRmsRunBlock("Recipe Deleting...");
                    }

                }
                else
                {
                    popMessageBox pop = new popMessageBox("RECIPE DELETE SUCCESS!", "RECIPE DELETE", MessageBoxButtons.OK);
                    pop.ShowDialog();
                }

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpDelete_Click" + ", [Deleted Model Name: " + strDeleteRcpPPID + ".mdl]");

                ShowModelFileList();

            }
            catch (System.Exception ex)
            {
                //
            }
        }

        private void btnRcpChange_Click(object sender, EventArgs e)
        {
            try
            {
                #region 레시피 바꾸기 전 소재 유무 체크
                if (GbFunc.bAllStripCheck())
                {
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("설비에 소재가 있습니다. 레시피를 변경할 수 없습니다."), "확인");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    return;
                }
                if (GbFunc.bIsTrayMapUnitCheck())
                {
                    popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("트레이 맵에 소재가 있습니다. 레시피를 변경할 수 없습니다."), "확인");
                    msg.TopMost = true;
                    msg.ShowDialog(this);
                    return;
                }
                #endregion
                if (gridModel.CurrentCell == null) return;
                if (gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value == null) return;
                string strChangeId = gridModel.Rows[gridModel.CurrentCell.RowIndex].Cells[1].Value.ToString();


                DialogResult result = MessageBox.Show("Do you want to change recipe? [" + strChangeId + "]",
                                                      "Confirm!",
                                                      MessageBoxButtons.YesNo,
                                                      MessageBoxIcon.Question,
                                                      MessageBoxDefaultButton.Button2);
                if (result != System.Windows.Forms.DialogResult.Yes)
                {
                    return;
                }

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnRcpChange_Click" + ", Model Changed: [" + RecipeMgr.Inst.LAST_MODEL_NAME + " --> " + strChangeId + ".mdl]");
                txtRcpSoruce.Text = strChangeId;
                RecipeMgr.Inst.ChangeModelFile(txtRcpSoruce.Text + ".mdl");

                ShowModelFileList();

                UpdateRcpData();

                popMessageBox pMsg = new popMessageBox("RECIPE CHANGE SUCCESS!", "RECIPE CHANGE", MessageBoxButtons.OK);
                pMsg.ShowDialog();


                IFMgr.Inst.VISION.UpdateRecipeData();
                IFMgr.Inst.VISION.UpdateRecipeName();
            }
            catch (System.Exception ex)
            {
                //
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

        private void tmrRefresh100ms_Tick(object sender, EventArgs e)
        {
            if (GbVar.gbProgramExit)
            {
                tmrRefresh.Stop();
                return;
            }

            if (flagTimerRefresh100msStart)
                return;

            flagTimerRefresh100msStart = true;

            try
            {

            }
            catch (Exception ex)
            {     
                throw;
            }
            finally
            {
                flagTimerRefresh100msStart = false;
            }
        }

        private void btnPickerZRAllHome_Click(object sender, EventArgs e)
        {
            int[] axis = new int[] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                       (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                       (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                       (int)SVDF.AXES.CHIP_PK_1_T_1,
                                       (int)SVDF.AXES.CHIP_PK_1_T_2,
                                       (int)SVDF.AXES.CHIP_PK_1_T_3,
                                       (int)SVDF.AXES.CHIP_PK_1_T_4,
                                       (int)SVDF.AXES.CHIP_PK_1_T_5,
                                       (int)SVDF.AXES.CHIP_PK_1_T_6,
                                       (int)SVDF.AXES.CHIP_PK_2_T_1,
                                       (int)SVDF.AXES.CHIP_PK_2_T_2,
                                       (int)SVDF.AXES.CHIP_PK_2_T_3,
                                       (int)SVDF.AXES.CHIP_PK_2_T_4,
                                       (int)SVDF.AXES.CHIP_PK_2_T_5,
                                       (int)SVDF.AXES.CHIP_PK_2_T_6 };

            GbSeq.manualRun.SetSingleHome(axis);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.SINGLE_HOME);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        private void btnRcpUpload_Click(object sender, EventArgs e)
        {

            //물어 보는 팝업창 띄우기
            DialogResult result = MessageBox.Show("Do you want to Upload recipe? [" + txtRcpSoruce.Text + "]",
                                                    "Confirm!",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question,
                                                    MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }


            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
            {
                MessageBox.Show("GEM SKIP MODE!");
                return;
            }
            //설비 운영 중에는 해당 시나리오 사용 할 수 없음.
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                if (GbVar.mcState.isCycleRunReq[nCnt] == true || GbVar.mcState.isCycleRun[nCnt] == true)
                {
                    MessageBox.Show("설비 구동 중에는 레시피 업로드를 할 수 없습니다.");
                    return;
                }
            }
        }

        public void deleRecipeUploadReq()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    ShowRmsRun();
                });
            }
            else
            {
                ShowRmsRun();
            }
        }

        public void ShowRmsRun()
        {
        }

        public void deleRecipeUploadComp(bool bSuccess, string strmsg = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    CloseDigPopRmsRun();
                    popMessageBox pMsg = new popMessageBox(strmsg, "RECIPE UPLOAD");
                    pMsg.TopMost = true;
                    pMsg.Show();
                });
            }
            else
            {
                CloseDigPopRmsRun();
                popMessageBox pMsg = new popMessageBox(strmsg, "RECIPE UPLOAD");
                pMsg.TopMost = true;
                pMsg.Show();
            }
        }


        public void deleRecipeChangeComp(bool bSuccess, string strmsg = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    CloseDigPopRmsRun();
                    popMessageBox pMsg = new popMessageBox(strmsg, "RECIPE CHANGE");
                    pMsg.TopMost = true;
                    pMsg.Show();

                });
            }
            else
            {
                CloseDigPopRmsRun();
                popMessageBox pMsg = new popMessageBox(strmsg, "RECIPE CHANGE");
                pMsg.TopMost = true;
                pMsg.Show();
            }
        }
        private void tmrPopRmsRun_Tick(object sender, EventArgs e)
        {
            if(flagTimerPopRmsRun) return;
            flagTimerPopRmsRun = true;

            if (GbVar.bPopHostRecipeUpload == true)
            {
                GbVar.bPopHostRecipeUpload  = false;
                ShowDigPopRmsRunBlock("Recipe Parameter Uploading...");
            }

            if (GbVar.bPopHostRecipeSelected == true)
            {
                GbVar.bPopHostRecipeSelected = false;
                ShowDigPopRmsRunBlock("Recipe Changing...");
            }

            flagTimerPopRmsRun = false;
        }
        private void DrawTable(Graphics g, int nRow, int nCol)
        {
            pictureBox1.Refresh();
            float fRow = 200 / nRow;
            float fCol = 200 / nCol;


            RectangleF rectF = new RectangleF(50f, 50f, fRow, fCol);
            int maxRowCount = nRow;
            int maxColCount = nCol;
            for (float y = 1.0f; y < maxRowCount + 1.0f; y += 1.0f)
            {
                for (float x = 1.0f; x < maxColCount + 1.0f; x += 1.0f)
                {
                    if (x == 1.0f && y == 1.0f)
                    {
                        g.DrawArc(new Pen(Color.Black, 0.5f), x * rectF.Width, y * rectF.Height / 2, rectF.Width, rectF.Height, 0.0f, -40.0f);
                        g.DrawArc(new Pen(Color.Black, 0.5f), x * rectF.Width, y * rectF.Height / 2, rectF.Width, rectF.Height, 180.0f, 40.0f);
                        g.DrawString("20", new Font("굴림체", 100.0f / nRow), Brushes.Black, x * rectF.Width * 1.1f, 0);

                        g.DrawArc(new Pen(Color.Black, 0.5f), x * rectF.Width / 2, y * rectF.Height, rectF.Width, rectF.Height, 90.0f, 40.0f);
                        g.DrawArc(new Pen(Color.Black, 0.5f), x * rectF.Width / 2, y * rectF.Height, rectF.Width, rectF.Height, 270.0f, -40.0f);
                        g.DrawString("20", new Font("굴림체", 100.0f / nRow), Brushes.Black, 0, y * rectF.Height * 1.2f);



                        //g.DrawArc(new Pen(Color.Black, 0.5f), x * rectF.Width, y * rectF.Height / 2, rectF.Width, rectF.Height, -80.0f, -180.0f);
                    }
                    // 높이는 추후 조정..
                    g.DrawRectangle(new Pen(Color.Black, 0.5f), x * rectF.Width, y * rectF.Height, rectF.Width, rectF.Height);
                }
            }
        }


        private void numFirstPlasmaPitchCount_ValueChanged(object sender, EventArgs e)
        {
            //NumericUpDown numUpDown = (NumericUpDown)sender;
            //int nTag = 0;
            //nTag = numUpDown.GetTag();
            //if (numUpDown.Name.Contains("Repeat")) return;
            //if (nTag == 0)
            //{
            //    double dPitchPos = 0;
            //    double dMovePos = 0;
            //    if(cbSelectedPlasmaMode.SelectedIndex < 4)
            //    {
            //        dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            //    }
            //    else
            //    {
            //        dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            //    }
            //    dPitchPos = dMovePos / (double)numUpDown.Value;
            //    string S_dPitchPos = dPitchPos.ToString("F4");
            //    tbFirstPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);
            //}
            //else
            //{
            //    double dPitchPos = 0;
            //    double dMovePos = 0;
            //    if (cbSelectedPlasmaMode.SelectedIndex < 4)
            //    {
            //        dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
            //    }
            //    else
            //    {
            //        dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
            //    }
            //    dPitchPos = dMovePos / (double)numUpDown.Value;
            //    string S_dPitchPos = dPitchPos.ToString("F4");
            //    tbSecondPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);
            //}
        }

        private void btnGetStartDX1_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int nTag = 0;
            //nTag = btn.GetTag();
            //if (nTag == 0)
            //{
            //    tbFirstPlasmaStartXPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_X].GetRealPos().ToString("F3");
            //}
            //else if(nTag == 1)
            //{
            //    tbSecondPlasmaStartXPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_X].GetRealPos().ToString("F3");
            //}
            //else if(nTag == 2)
            //{
            //    tbFirstPlasmaEndXPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_X].GetRealPos().ToString("F3");
            //}
            //else
            //{
            //    tbSecondPlasmaEndXPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_X].GetRealPos().ToString("F3");
            //}
        }

        private void btnGetStartDY1_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int nTag = 0;
            //nTag = btn.GetTag();
            //if (nTag == 0)
            //{
            //    tbFirstPlasmaStartYPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].GetRealPos().ToString("F3");
            //}
            //else if (nTag == 1)
            //{
            //    tbSecondPlasmaStartYPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].GetRealPos().ToString("F3");
            //}
            //else if (nTag == 2)
            //{
            //    tbFirstPlasmaEndYPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].GetRealPos().ToString("F3");
            //}
            //else
            //{
            //    tbSecondPlasmaEndYPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Y].GetRealPos().ToString("F3");
            //}
        }

        private void cbSelectedPlasmaMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            //int nIdx = cbSelectedPlasmaMode.SelectedIndex;
            
            //switch (nIdx)
            //{
            //    case 0:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을 변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos,ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을 변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos,ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2;
            //        break;
            //    case 1:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_Y_Move_Left_Start;
            //        break;
            //    case 2:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_Y_Move_Right_Start_Up;
            //        break;
            //    case 3:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_Y_Move_Left_Start_Up;
            //        break;
            //    case 4:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_X_Move_Right_Start;
            //        break;
            //    case 5:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_X_Move_Left_Start;
            //        break;
            //    case 6:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_X_Move_Right_Start_Up;
            //        break;
            //    case 7:
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("1번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos > RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 X 시작 위치와 X 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos);
            //        }
            //        if (RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos < RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos)
            //        {
            //            popMessageBox msg = new popMessageBox("2번 플라즈마 Y 시작 위치와 Y 종료 위치가 반대로 되었습니다. \r\n 서로 값을  변경하시겠습니까?", "주의");
            //            msg.TopMost = true;
            //            if (msg.ShowDialog(this) != DialogResult.OK)
            //            {
            //                cbSelectedPlasmaMode.SelectedIndex = RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode;
            //                return;
            //            }
            //            SwapValue(ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos, ref RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos);
            //        }
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2_X_Move_Left_Start_Up;
            //        break;
            //    default:
            //        pbPlasma.Image = global::NSS_3330S.Properties.Resources.Plasma_Move_Ver2;
            //        break;
            //}
            //RecipeMgr.Inst.Rcp.cleaning.nSelectedPlasmaMode = cbSelectedPlasmaMode.SelectedIndex;
            //UpdateRcpData();
            //double dPitchPos = 0;
            //double dMovePos = 0;
            //if (cbSelectedPlasmaMode.SelectedIndex < 4)
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            //}
            //else
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            //}
            //dPitchPos = dMovePos / (double)numFirstPlasmaPitchCount.Value;
            //string S_dPitchPos = dPitchPos.ToString("F4");
            //tbFirstPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);
            
            //dPitchPos = 0;
            //dMovePos = 0;
            //if (cbSelectedPlasmaMode.SelectedIndex< 4)
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
            //}
            //else
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
            //}
            //dPitchPos = dMovePos / (double)numSecondPlasmaPitchCount.Value;
            //S_dPitchPos = dPitchPos.ToString("F4");
            //tbSecondPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);
        }

        private void ckbIsStrip_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ctr = sender as CheckBox;

            if (ctr.Checked == true)
                ctr.Text = "STRIP";
            else
                ctr.Text = "QUAD";
        }


        private void btnGetZ_ClickEvent(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int nTag = 0;
            //nTag = btn.GetTag();
            //if (nTag == 0)
            //{
            //    tbFirstPlasmaZPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Z].GetRealPos().ToString("F3");
            //}
            //else if (nTag == 1)
            //{
            //    tbSecondPlasmaZPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Z].GetRealPos().ToString("F3");
            //}
            //else if (nTag == 2)
            //{
            //    tbPickerPlasmaZPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Z].GetRealPos().ToString("F3");
            //}
            //else
            //{
            //    tbFirstPlasmaZPos.Text = MotionMgr.Inst[SVDF.AXES.CL_DRY_Z].GetRealPos().ToString("F3");
            //}
        }
        void CalcPitchOffset()
        {
            //double dPitchPos = 0;
            //double dMovePos = 0;
            //if (cbSelectedPlasmaMode.SelectedIndex < 4)
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            //}
            //else
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            //}
            //dPitchPos = dMovePos / (double)numFirstPlasmaPitchCount.Value;
            //string S_dPitchPos = dPitchPos.ToString("F4");
            //tbFirstPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);

            //dPitchPos = 0;
            //dMovePos = 0;
            //if (cbSelectedPlasmaMode.SelectedIndex < 4)
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
            //}
            //else
            //{
            //    dMovePos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos - RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
            //}
            //dPitchPos = dMovePos / (double)numSecondPlasmaPitchCount.Value;
            //S_dPitchPos = dPitchPos.ToString("F4");
            //tbSecondPlasmaStepPitch.Text = S_dPitchPos.Substring(0, S_dPitchPos.Length - 1);
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

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
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD1_LIVE);
            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH2_T1_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[8] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.MAP_STG_1_Y};

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;


            double[] dTargetPos = new double[8] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD2_LIVE);
            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH1_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[8] { (int)SVDF.AXES.CHIP_PK_1_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_1_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_1_X,
                                         (int)SVDF.AXES.MAP_STG_2_Y};

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).x;

            //220428 pjh 
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, false).y;


            double[] dTargetPos = new double[8] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] };

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD1_LIVE);
            ShowDlgPopManualRunBlock();
        }

        /// <summary>
        /// Pickup 티칭 위치에서 Vision 위치로 이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMoveToPickUpPosH2_T2_Click(object sender, EventArgs e)
        {
            int[] nAxisNo = new int[8] { (int)SVDF.AXES.CHIP_PK_2_Z_1,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_2,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_3,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_4,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_5,
                                         (int)SVDF.AXES.CHIP_PK_2_Z_6,
                                         (int)SVDF.AXES.CHIP_PK_2_X,
                                         (int)SVDF.AXES.MAP_STG_2_Y};

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P2];

            double dMovePosX = dRcpPosX + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).x;

            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;//org
            double dMovePosY = dRcpPosY - ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_2, false).y;


            double[] dTargetPos = new double[8] { 0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  0.0f,
                                                  dMovePosX,
                                                  dMovePosY };

            double[] dSpeed = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dVel[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dAcc = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dAcc[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            double[] dDec = new double[8] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dDec[POSDF.MAP_STAGE_UNIT_UNLOADING_P2] };

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD2_LIVE);
            ShowDlgPopManualRunBlock();
        }
#endregion

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

            // [2022.05.19.kmlee] 레시피 값으로 하면 사용자가 헷갈릴 수 있음
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1].Text, out dP1X);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text, out dP1Y);

            // [2022.05.19.kmlee] 부호 변경
            dxy HeadDist = ConfigMgr.Inst.GetPickerHeadOffset();
            double dTeachPosX = dP1X - HeadDist.x;
            double dTeachPosY = dP1Y + HeadDist.y;

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

            // [2022.05.19.kmlee] 레시피 값으로 하면 사용자가 헷갈릴 수 있음
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2].Text, out dP1X);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text, out dP1Y);

            // [2022.05.19.kmlee] 부호 변경
            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y + ConfigMgr.Inst.GetPickerHeadOffset().y;

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

            // [2022.05.19.kmlee] 레시피 값으로 하면 사용자가 헷갈릴 수 있음
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF].Text, out dP1X);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_WORKING_P1].Text, out dP1Y);

            // [2022.05.19.kmlee] 부호 변경
            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y + ConfigMgr.Inst.GetPickerHeadOffset().y;

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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX;
            //220428 pjh
            //double dMovePosY = dRcpPosY + ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1, true).y;// org
            double dMovePosY = dRcpPosY;

            int nHeadNo = 0;
            int nTableNo = 0;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);
              

                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, 0); 
                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, 0);


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;

                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            }

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

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1] };

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD1_LIVE);
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            //220428 pjh
            double dMovePosX = dRcpPosX;
            double dMovePosY = dRcpPosY;
            int nHeadNo = 1;
            int nTableNo = 0;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);


                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, 0);
                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, 0);


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;

                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            }

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

            double[] dSpeed = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                              TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dAcc = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            double[] dDec = new double[10] { TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1],
                                            TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P2] };

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD2_LIVE);
            ShowDlgPopManualRunBlock();
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX;

            //220428 pjh
            double dMovePosY = dRcpPosY;
            int nHeadNo = 0;
            int nTableNo = 1;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);


                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, 0);
                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, 0);


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;

                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            }
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
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD1_LIVE);
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T2];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            double dMovePosX = dRcpPosX;

            //220428 pjh
            double dMovePosY = dRcpPosY;
            int nHeadNo = 1;
            int nTableNo = 1;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);


                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, 0);
                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, 0);


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;

                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, cbSelectedPicker.SelectedIndex).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            }


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
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD2_LIVE);
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];

            double dMovePosX = dRcpPosX;

            //220428 pjh
            double dMovePosY = dRcpPosY;
            int nHeadNo = 0;
            int nTableNo = 2;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);


                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetX(0);
                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].x;

                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetY(0);
                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].y;


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;
            }

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
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD1_LIVE);
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

            double dRcpPosX = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.CHIP_PK_2_X].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            double dRcpPosY = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.RW_TRAY_STG_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];

            double dMovePosX = dRcpPosX;
            //220428 pjh
            double dMovePosY = dRcpPosY;
            int nHeadNo = 1;
            int nTableNo = 2;
            int nPickerNo = cbSelectedPicker.SelectedIndex;
            if (cbPikcerToMove.Checked)
            {
                dxy dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          dRcpPosX,
                                                          dRcpPosY);


                dMovePosX = dpPickerPos.x;
                dMovePosX += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetX(0);
                dMovePosX += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                dMovePosX += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].x;

                dMovePosY = dpPickerPos.y;
                dMovePosY += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetY(0);
                dMovePosY -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                dMovePosY += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].y;


                dxy dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dRcpPosX, dRcpPosY, dMovePosX, dMovePosY);

                dMovePosX = dpMovePos.x;
                dMovePosY = dpMovePos.y;
            }

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
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            nAxisArray = new int[] { nAxisNo[0], nAxisNo[1], nAxisNo[2], nAxisNo[3], nAxisNo[4], nAxisNo[5], nAxisNo[6], nAxisNo[7], nAxisNo[8], nAxisNo[9] };
            dPosArray = new double[] { dTargetPos[0], dTargetPos[1], dTargetPos[2], dTargetPos[3], dTargetPos[4], dTargetPos[5], dTargetPos[6], dTargetPos[7], dTargetPos[8], dTargetPos[9] };
            nOrderArray = new uint[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 };
            dSpeedArray = new double[] { dSpeed[0], dSpeed[1], dSpeed[2], dSpeed[3], dSpeed[4], dSpeed[5], dSpeed[6], dSpeed[7], dSpeed[8], dSpeed[9] };
            dAccArray = new double[] { dAcc[0], dAcc[1], dAcc[2], dAcc[3], dAcc[4], dAcc[5], dAcc[6], dAcc[7], dAcc[8], dAcc[9] };
            dDecArray = new double[] { dDec[0], dDec[1], dDec[2], dDec[3], dDec[4], dDec[5], dDec[6], dDec[7], dAcc[8], dAcc[9] };

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.HD2_LIVE);
            ShowDlgPopManualRunBlock();
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

            if (nSeqNo == 1500)
            {
                nMeasureTableNo = 0;
                nLoadingTableNo = 0;
                nUnloadingTableNo = 0;
                dTargetPos = (double)args[0];
            }
            else
            {
                nMeasureTableNo = 1;
                nLoadingTableNo = 1;
                nUnloadingTableNo = 1;
                dTargetPos = (double)args[0];
            }

            GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nLoadingElvNo, nLoadingTableNo, nUnloadingElvNo, nUnloadingTableNo, nMeasureTableNo, dTargetPos);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();

        }
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

            double dP1X = RecipeMgr.Inst.TempRcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            double dP1Y = RecipeMgr.Inst.TempRcp.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            // [2022.05.19.kmlee] 레시피 값으로 하면 사용자가 헷갈릴 수 있음
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1].Text, out dP1X);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1].Text, out dP1Y);

            // [2022.05.19.kmlee] 부호 변경
            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y + ConfigMgr.Inst.GetPickerHeadOffset().y;

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

            double dP1X = RecipeMgr.Inst.TempRcp.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2];
            double dP1Y = RecipeMgr.Inst.TempRcp.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1];

            // [2022.05.19.kmlee] 레시피 값으로 하면 사용자가 헷갈릴 수 있음
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text, out dP1X);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1].Text, out dP1Y);

            // [2022.05.19.kmlee] 부호 변경
            double dTeachPosX = dP1X - ConfigMgr.Inst.GetPickerHeadOffset().x;
            double dTeachPosY = dP1Y + ConfigMgr.Inst.GetPickerHeadOffset().y;

            m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2].Text = dTeachPosX.ToString("F3");
            m_arrMotPosBtn[(int)SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2].Text = dTeachPosY.ToString("F3");
        }
		private void tbFirstPlasmaStartXPos_TextChanged(object sender, EventArgs e)
        {
            //TextBox tbx = (TextBox)sender;
            //int nTag;
            //double dValue = 0;
            //nTag = tbx.GetTag();
            //double.TryParse(tbx.Text, out dValue);

            //switch (nTag)
            //{
            //    case 0:
            //        RecipeMgr.Inst.TempRcp.cleaning.dFirstPlasmaStartXPos = dValue;
            //        break;
            //    case 1:
            //        RecipeMgr.Inst.TempRcp.cleaning.dFirstPlasmaEndXPos = dValue;
            //        break;
            //    case 2:
            //        RecipeMgr.Inst.TempRcp.cleaning.dFirstPlasmaStartYPos = dValue;
            //        break;
            //    case 3:
            //        RecipeMgr.Inst.TempRcp.cleaning.dFirstPlasmaEndYPos = dValue;
            //        break;
            //    case 4:
            //        RecipeMgr.Inst.TempRcp.cleaning.dSecondPlasmaStartXPos = dValue;
            //        break;
            //    case 5:
            //        RecipeMgr.Inst.TempRcp.cleaning.dSecondPlasmaEndXPos = dValue;
            //        break;
            //    case 6:
            //        RecipeMgr.Inst.TempRcp.cleaning.dSecondPlasmaStartYPos = dValue;
            //        break;
            //    case 7:
            //        RecipeMgr.Inst.TempRcp.cleaning.dSecondPlasmaEndYPos = dValue;
            //        break;
            //    default:
            //        break;
            //}
            //CalcPitchOffset();
        }
        void SwapValue(double dValue1, double dValue2)
        {
            double dTemp = dValue1;
            dValue1 = dValue2;
            dValue2 = dTemp;
        }
        void SwapValue(ref double dValue1,ref double dValue2)
        {
            double dTemp = dValue1;
            dValue1 = dValue2;
            dValue2 = dTemp;
        }

        private void rcpMotPosMoveButtonEx15_Click(object sender, EventArgs e)
        {

        }

        private void btnCalcTrayPitch_Click(object sender, EventArgs e)
        {
            int nTrayCountX, nTrayCountY;
            double dP1LfTopX, dP1LfTopY, dP1RtBtmX, dP1RtBtmY;

            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_TRAY_PITCH_LT].Text, out dP1LfTopX);
            double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_TRAY_PITCH_RB].Text, out dP1RtBtmX);

            if (rbTrayPitchStage1.Checked)
            {
                double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.CHIP_PICKER_TRAY_PITCH_LT].Text, out dP1LfTopY);
                double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.CHIP_PICKER_TRAY_PITCH_RB].Text, out dP1RtBtmY);
            }
            else
            {
                double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.CHIP_PICKER_TRAY_PITCH_LT].Text, out dP1LfTopY);
                double.TryParse(m_arrMotPosBtn[(int)SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.CHIP_PICKER_TRAY_PITCH_RB].Text, out dP1RtBtmY);
            }

            int.TryParse(tbxTrayCountX.Text, out nTrayCountX);
            int.TryParse(tbxTrayCountY.Text, out nTrayCountY);

            double dWidth = dP1RtBtmX - dP1LfTopX;
            double dHeight = dP1RtBtmY - dP1LfTopY;

            double dPitchX = 0;
            double dPitchY = 0;

            if (nTrayCountX - 1 > 0)
                dPitchX = dWidth / (nTrayCountX - 1);
            if (nTrayCountY - 1 > 0)
                dPitchY = dHeight / (nTrayCountY - 1);

            tbxTrayPitchX.Text = dPitchX.ToString();
            tbxTrayPitchY.Text = dPitchY.ToString();
        }

        private void btnInfoPickUp1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            if (toolTip3.Active)
            {
                toolTip3.Hide(btn);
            }
            else
            {
                string strMsg = FormTextLangMgr.FindKey(toolTip3.GetToolTip(btn));
                //toolTip3.SetToolTip(btn,strMsg);
                toolTip3.Show(strMsg, btn);
            }
        }

        private void chkCognexLive_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk.Checked)
            {
                if (GbDev.BarcodeReader != null && pbCognexViewer != null)
                {
                    btnCognexTrigger.Enabled = false;
                    GbDev.BarcodeReader.StartLiveImage(pbCognexViewer);
                }
            }
            else
            {
                if (GbDev.BarcodeReader != null && pbCognexViewer != null)
                {
                    btnCognexTrigger.Enabled = true;
                    GbDev.BarcodeReader.StopLiveImage(pbCognexViewer);
                }
            }
        }

        private void btnCognexTrigger_MouseDown(object sender, MouseEventArgs e)
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
                else
                {
                    MessageBox.Show("연결 중입니다. 연결 완료 후 다시 시도해주세요.");
                }
#endif
                return;
            }
            GbDev.BarcodeReader.TriggerOn();
        }

        private void btnCognexTrigger_MouseUp(object sender, MouseEventArgs e)
        {
            GbDev.BarcodeReader.TriggerOff();
        }

        private void btnMoveToTopCenterMapT1_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (nTag < 0) return;

            int nTableNo = nTag;

            string strTitle = "축 이동";
            string strMsg = string.Format("Cam view 위치로 이동하시겠습니까?");
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;


            int[] nAxisArray = null;
            int[] nPosArray = null;
            double[] dPosArray = null;
            uint[] nOrderArray = null;
            double[] dSpeedArray = null;
            double[] dAccArray = null;
            double[] dDecArray = null;

            int nMaxAxisCount = 5;
            nAxisArray = new int[nMaxAxisCount];
            nPosArray = new int[nMaxAxisCount];
            dPosArray = new double[nMaxAxisCount];
            nOrderArray = new uint[nMaxAxisCount];
            dSpeedArray = new double[nMaxAxisCount];
            dAccArray = new double[nMaxAxisCount];
            dDecArray = new double[nMaxAxisCount];

            nAxisArray[0] = SVDF.Get(SVDF.AXES.MAP_PK_Z);
            nAxisArray[1] = SVDF.Get(SVDF.AXES.MAP_PK_X);
            nAxisArray[2] = SVDF.Get(SVDF.AXES.MAP_STG_1_Y + nTableNo);
            nAxisArray[3] = SVDF.Get(SVDF.AXES.MAP_STG_1_T + nTableNo);
            nAxisArray[4] = SVDF.Get(SVDF.AXES.MAP_VISION_Z);

            nOrderArray[0] = 0;
            nOrderArray[1] = 1;
            nOrderArray[2] = 1;
            nOrderArray[3] = 2;
            nOrderArray[4] = 3;

            nPosArray[0] = POSDF.MAP_PICKER_READY;
            nPosArray[1] = POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + nTableNo;
            nPosArray[2] = POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
            nPosArray[3] = POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
            nPosArray[4] = POSDF.MAP_VISION_FOCUS_T1 + nTableNo;

            for (int nAxisNo = 0; nAxisNo < nMaxAxisCount; nAxisNo++)
            {
                dPosArray[nAxisNo] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisNo]].dPos[nPosArray[nAxisNo]];
                dSpeedArray[nAxisNo] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisNo]].dVel[nPosArray[nAxisNo]];
                dAccArray[nAxisNo] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisNo]].dAcc[nPosArray[nAxisNo]];
                dDecArray[nAxisNo] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisNo]].dDec[nPosArray[nAxisNo]];
            }

            dPosArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_PICKER_T1_LEFT_TOP_UNIT_POS + nTableNo];
            dPosArray[2] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[2]].dPos[POSDF.MAP_STAGE_LEFT_TOP_POS];

            GbSeq.manualRun.SetMoveTeachPos(nAxisArray, dPosArray, nOrderArray, true, true, dSpeedArray, dAccArray, dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            IFMgr.Inst.VISION.SendCMD(IFVision.E_VS_SEND_CMD.TOP_LIVE);
            ShowDlgPopManualRunBlock();
        }

        private void btnConntection_Click(object sender, EventArgs e)
        {
            if (!GbDev.BarcodeReader.IsConnect())
            {
                DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();
                if (!System.IO.File.Exists(PathMgr.Inst.PATH_BARCODEREADER))
                {
                    remoteInfo.remotePath = "169.254.60.223";
                    //remoteInfo.remotePath = "169.254.46.127";
                    //remoteInfo.port = 23;
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
                else
                {
                    MessageBox.Show("연결 중입니다. 메시지창을 닫고 잠시만 기다려주세요. 연결이 안될 시 바코드 전원을 끈 후 다시 켜고 시도해주세요.");
                }
#endif
                return;
            }
        }

        private void cbxGrabOnlyOneEdge_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ctr = sender as CheckBox;

            if (ctr.Checked == true)
                ctr.Text = FormTextLangMgr.FindKey("USE");
            else
                ctr.Text = FormTextLangMgr.FindKey("UNUSE");
        }

        private void btnMapGrpInfoNew_Click(object sender, EventArgs e)
        {
            MapGroupInfo info = new MapGroupInfo();

            if (tabMapGroupInfo.SelectedIndex == 0)
            {
                m_blistMapGrpInfo1.Add(info);
            }
            else
            {
                m_blistMapGrpInfo2.Add(info);
            }
        }

        /// <summary>
        /// 220926 pjh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMapGrpInfoCopy_Click(object sender, EventArgs e)
        {
            int nIdx = 0;

            if (tabMapGroupInfo.SelectedIndex == 0)
            {
                if (gdvMapGroupInfo_T1.SelectedCells == null)
                {
                    popMessageBox msg = new popMessageBox("복사할 번호를 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }
                if (gdvMapGroupInfo_T1.SelectedCells.Count != 1)
                {
                    popMessageBox msg = new popMessageBox("한 줄만 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }

                nIdx = gdvMapGroupInfo_T1.SelectedCells[0].RowIndex;
                object obj = m_blistMapGrpInfo1[nIdx].Clone();

                m_blistMapGrpInfo1.Add((MapGroupInfo)obj);
            }
            else
            {
                if (gdvMapGroupInfo_T2.SelectedCells == null)
                {
                    popMessageBox msg = new popMessageBox("복사할 번호를 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }
                if (gdvMapGroupInfo_T2.SelectedCells.Count != 1)
                {
                    popMessageBox msg = new popMessageBox("한 줄만 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }
                nIdx = gdvMapGroupInfo_T2.SelectedCells[0].RowIndex;
                object obj = m_blistMapGrpInfo2[nIdx].Clone();

                m_blistMapGrpInfo2.Add((MapGroupInfo)obj);
            }
        }

        /// <summary>
        /// 220926 pjh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMapGrpInfoDel_Click(object sender, EventArgs e)
        {
            int nIdx = 0;

            if (tabMapGroupInfo.SelectedIndex == 0)
            { 
                if (gdvMapGroupInfo_T1.SelectedCells == null || gdvMapGroupInfo_T1.SelectedCells.Count < 1)
                {
                    popMessageBox msg = new popMessageBox("삭제할 번호를 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }

                for (int i = 0; i < gdvMapGroupInfo_T1.SelectedCells.Count; i++)
                {
                    nIdx = gdvMapGroupInfo_T1.SelectedCells[i].RowIndex;
                    m_blistMapGrpInfo1.RemoveAt(nIdx);
                }
            }
            else
            {
                if (gdvMapGroupInfo_T2.SelectedCells == null || gdvMapGroupInfo_T2.SelectedCells.Count < 1)
                {
                    popMessageBox msg = new popMessageBox("삭제할 번호를 선택하십시오.", "경고", MessageBoxButtons.OK);
                    msg.ShowDialog();
                    return;
                }

                for (int i = 0; i < gdvMapGroupInfo_T2.SelectedCells.Count; i++)
                {
                    nIdx = gdvMapGroupInfo_T2.SelectedCells[i].RowIndex;
                    m_blistMapGrpInfo2.RemoveAt(nIdx);
                }
            }        
        }

        private void btnMapGroupInfoUp_Click(object sender, EventArgs e)
        {

        }

        private void btnMapGroupInfoDown_Click(object sender, EventArgs e)
        {

        }

        private void gdvMapGroupInfo_T1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            int nRow = e.RowIndex;
            int nCol = e.ColumnIndex;

            if (nRow < 0) return;
            if (nCol == 3)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.MAP_PK_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;
                
                dgv.Rows[nRow].Cells[1].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[2].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if(nCol == 4)
            {
                // MOVE
                int nXAxisNo = (int)SVDF.AXES.MAP_PK_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[1].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[2].Value.ToString(), out dYTargetPos)) return;

                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");
              
                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));
                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
            else if (nCol == 7)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_1_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;

                dgv.Rows[nRow].Cells[5].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[6].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 8)
            {
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_1_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[5].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[6].Value.ToString(), out dYTargetPos)) return;

                #region Cam To Picker Offset
                if (cbPikcerToMove_PickUp.Checked)
                {
                    int nHeadNo = 0;
                    int nTableNo = 0;
                    dxy dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, dXTargetPos, dYTargetPos);
                    dXTargetPos = dpPickerPos.x;
                    dYTargetPos = dpPickerPos.y;

                    dXTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, 0);
                    //dXTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dXTargetPos += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).x;

                    dYTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetY(0);
                    //dYTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dYTargetPos -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).y;
                }
                #endregion



                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");

                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));

                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
            else if (nCol == 11)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_2_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;

                dgv.Rows[nRow].Cells[9].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[10].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 12)
            {
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_2_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_1_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[9].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[10].Value.ToString(), out dYTargetPos)) return;

                if (cbPikcerToMove_PickUp.Checked)
                {
                    #region Cam To Picker Offset
                    int nHeadNo = 1;
                    int nTableNo = 0;

                    dxy dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, dXTargetPos, dYTargetPos);
                    dXTargetPos = dpPickerPos.x;
                    dYTargetPos = dpPickerPos.y;

                    dXTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, 0);
                    //dXTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dXTargetPos += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).x;

                    dYTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetY(0);
                    //dYTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dYTargetPos -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).y;
                    #endregion
                }

                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");

                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));

                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        private void gdvMapGroupInfo_T2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            int nRow = e.RowIndex;
            int nCol = e.ColumnIndex;

            if (nRow < 0) return;
            if (nCol == 3)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.MAP_PK_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                dgv.Rows[nRow].Cells[1].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[2].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 4)
            {
                // MOVE
                int nXAxisNo = (int)SVDF.AXES.MAP_PK_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[1].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[2].Value.ToString(), out dYTargetPos)) return;


                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");

                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));

                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
            else if (nCol == 7)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_1_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                dgv.Rows[nRow].Cells[5].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[6].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 8)
            {
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_1_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[5].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[6].Value.ToString(), out dYTargetPos)) return;

                if (cbPikcerToMove_PickUp.Checked)
                {
                    #region Cam To Picker Offset
                    int nHeadNo = 0;
                    int nTableNo = 1;

                    dxy dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, dXTargetPos, dYTargetPos);
                    dXTargetPos = dpPickerPos.x;
                    dYTargetPos = dpPickerPos.y;

                    dXTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, 0);
                    //dXTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dXTargetPos += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).x;

                    dYTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetY(0);
                    //dYTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dYTargetPos -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).y;
                    #endregion
                }

                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");

                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));

                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
            else if (nCol == 11)
            {
                // GET
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_2_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                dgv.Rows[nRow].Cells[9].Value = MotionMgr.Inst[nXAxisNo].GetRealPos().ToString("F3");
                dgv.Rows[nRow].Cells[10].Value = MotionMgr.Inst[nYAxisNo].GetRealPos().ToString("F3");
            }
            else if (nCol == 12)
            {
                int nXAxisNo = (int)SVDF.AXES.CHIP_PK_2_X;
                int nYAxisNo = (int)SVDF.AXES.MAP_STG_2_Y;

                double dXTargetPos = 0.0;
                double dXSpeed = 0.0;
                double dXAcc = 0.0;
                double dXDec = 0.0;

                double dYTargetPos = 0.0;
                double dYSpeed = 0.0;
                double dYAcc = 0.0;
                double dYDec = 0.0;

                if (!double.TryParse(dgv.Rows[nRow].Cells[9].Value.ToString(), out dXTargetPos)) return;
                if (!double.TryParse(dgv.Rows[nRow].Cells[10].Value.ToString(), out dYTargetPos)) return;

                if (cbPikcerToMove_PickUp.Checked)
                {
                    #region Cam To Picker Offset
                    int nHeadNo = 1;
                    int nTableNo = 1;

                    dxy dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, dXTargetPos, dYTargetPos);
                    dXTargetPos = dpPickerPos.x;
                    dYTargetPos = dpPickerPos.y;

                    dXTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, 0);
                    //dXTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dXTargetPos += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).x;

                    dYTargetPos += ConfigMgr.Inst.GetMapTablePickUpOffsetY(0);
                    //dYTargetPos += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dYTargetPos -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, cbSelectedPicker_PickUp.SelectedIndex).y;
                    #endregion
                }
                dXSpeed = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel;
                dXAcc = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;
                dXDec = ConfigMgr.Inst.Cfg.MotData[nXAxisNo].dVel * 10.0f;

                dYSpeed = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel;
                dYAcc = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;
                dYDec = ConfigMgr.Inst.Cfg.MotData[nYAxisNo].dVel * 10.0f;

                string strTitle = FormTextLangMgr.FindKey("축 이동");

                string strMsg = string.Format("{0}, {1} / {2}, {3}{4}",
                    FormTextLangMgr.FindKey(SVDF.GetAxisName(nXAxisNo)), dXTargetPos, FormTextLangMgr.FindKey(SVDF.GetAxisName(nYAxisNo)), dYTargetPos, FormTextLangMgr.FindKey("으로 이동하시겠습니까?"));

                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != DialogResult.OK) return;

                //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

                int[] m_nAxisArray = null;
                double[] m_dPosArray = null;
                uint[] m_nOrderArray = null;
                double[] m_dSpeedArray = null;
                double[] m_dAccArray = null;
                double[] m_dDecArray = null;

                m_nAxisArray = new int[] { nXAxisNo, nYAxisNo };
                m_dPosArray = new double[] { dXTargetPos, dYTargetPos };
                m_nOrderArray = new uint[] { 0, 0 };
                m_dSpeedArray = new double[] { dXSpeed, dYSpeed };
                m_dAccArray = new double[] { dXAcc, dYAcc };
                m_dDecArray = new double[] { dXDec, dYDec };

                GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        private void a1Panel25_Paint(object sender, PaintEventArgs e)
        {

        }

        private void glassButton7_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
               btn.Text));
            double dPos = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];
            StartTrayPickerCycle(btn, dPos);
        }

        private void glassButton3_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
               btn.Text));
            double dPos = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_1_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];
            StartTrayPickerCycle(btn, dPos);
        }

        private void glassButton1_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
               btn.Text));
            double dPos = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1];
            StartTrayPickerCycle(btn, dPos);
        }

        private void glassButton4_Click(object sender, EventArgs e)
        {
            GlassButton btn = sender as GlassButton;
            GbFunc.WriteEventLog("Manual Cycle ", string.Format("{0} : BUTTON CLICK",
               btn.Text));
            double dPos = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.GD_TRAY_STG_2_Y].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P2];
            StartTrayPickerCycle(btn, dPos);
        }

        private void btnMgzBcr_Connect_Click(object sender, EventArgs e)
        {
            if (!GbDev.MGZBarcodeReader.IsConnect)
            {
                DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();
                if (!System.IO.File.Exists(PathMgr.Inst.PATH_MGZ_BARCODEREADER))
                {
                    remoteInfo.remotePath = "169.254.60.223";
                    remoteInfo.port = 23;

                    remoteInfo.Serialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
                }
                else
                {
                    remoteInfo = DionesTool.DATA.RemoteInfo.Deserialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
                }
#if !_NOTEBOOK
                GbDev.MGZBarcodeReader.Connect(remoteInfo.remotePath);

                if (!GbDev.MGZBarcodeReader.IsConnect)
                {
                    MessageBox.Show("Disconnect!");
                }
                else
                {
                    MessageBox.Show("연결 중입니다. 메시지창을 닫고 잠시만 기다려주세요. 연결이 안될 시 바코드 전원을 끈 후 다시 켜고 시도해주세요.");
                }
#endif
                return;
            }
        }

        private void btnMgzBcr_Live_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;

            if (chk.Checked)
            {
                if (GbDev.MGZBarcodeReader != null)
                {
                    btnCognexTrigger.Enabled = false;
                    MGZ_BCR_LIVE_START();
                }
            }
            else
            {
                if (GbDev.MGZBarcodeReader != null)
                {
                    btnCognexTrigger.Enabled = true;
                    MGZ_BCR_LIVE_END();
                }
            }
        }

        private void btnMgzBcr_Trigger_Click(object sender, EventArgs e)
        {
            string result = "";

            result = GbDev.MGZBarcodeReader.Send_ReadStart();

            txtMgzBcr_Result.Text = result;
        }

        private void MGZ_BCR_LIVE_START()
        {
            DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();

            if (!System.IO.File.Exists(PathMgr.Inst.PATH_MGZ_BARCODEREADER))
            {
                remoteInfo.remotePath = "169.254.60.223";
                remoteInfo.port = 23;

                remoteInfo.Serialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }
            else
            {
                remoteInfo = DionesTool.DATA.RemoteInfo.Deserialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }


            ctlBCRView_MGZBarcode.IpAddress = remoteInfo.remotePath;
            ctlBCRView_MGZBarcode.BeginReceive();
        }

        private void MGZ_BCR_LIVE_END()
        {
            DionesTool.DATA.RemoteInfo remoteInfo = new DionesTool.DATA.RemoteInfo();

            if (!System.IO.File.Exists(PathMgr.Inst.PATH_MGZ_BARCODEREADER))
            {
                remoteInfo.remotePath = "169.254.60.223";
                remoteInfo.port = 23;

                remoteInfo.Serialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }
            else
            {
                remoteInfo = DionesTool.DATA.RemoteInfo.Deserialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }

            ctlBCRView_MGZBarcode.IpAddress = remoteInfo.remotePath;
            ctlBCRView_MGZBarcode.EndReceive();
        }
    }
}
