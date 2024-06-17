using NSS_3330S.MOTION;
using NSS_3330S.POP;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NSS_3330S.FORM
{
    public partial class frmAuto : Form
    {
        private bool loop_state = true;

        public delegate void startStopDelegate(int nState);
        public event startStopDelegate RunStartStopEvent;

        //Reset Event
        public delegate void resetDelegate();
        public event resetDelegate ResetEvent = null;

        private bool bOneSecWatchDog = false;
        private Stopwatch timeStamp = new Stopwatch();

        private Label[] m_SeqCurNo = null;
        private Label[] m_CycCurNo = null;
        private ListView[] m_SeqBit = null;
        private int[] m_nCheckSeqNo = null;

        private Label[] m_lbMgzOhtML;
        private Label[] m_lbMgzOhtMU;
        private Label[] m_lbTrayOhtTL;
        private Label[] m_lbTrayOhtTU;
        private Button[] m_btnTU;
        private Button[] m_btnTL;
        private int m_nTL = 2;
        private int m_nTU = 0;
        private bool bColorOn = false;

        private Label[] m_lbMgzLfRtSensor;

        private int m_nStopTicks = 0;
        private bool m_bStopMouseDown = false;
        private bool m_bIsStop = false;

        private bool m_bIsTactLabelUpdate = false;

        private UC.ucPickerPadInfo[] m_PickerPadInfo = new UC.ucPickerPadInfo[12];
        private UC.ucVacList m_VacList = new UC.ucVacList();

        public popSubMap pSubMap = new popSubMap();
        public popLOTInfo pLotInfo = new popLOTInfo();

        public popManualRun m_popManualRun = new popManualRun();//220615

        UC.ucInspectionMap m_ucInspectionMapL = null;
        UC.ucInspectionMap m_ucInspectionMapR = null;
        UC.ucUldTrayMap m_ucUldReworkTrayMap = null;
        UC.ucUldTrayMap m_ucUldGoodTrayMap = null;
        UC.ucTrayInOutWarning[] m_ucTrayInOutWarning = new UC.ucTrayInOutWarning[MCDF.ELV_MAX_CNT];//트레이 엘리베이터 개수 //220511 pjh
        UC.UcScrapInfo m_ucScrapInfo = null;


        string strBoxWarning = "";

        int CurLanguage = -1;


        public frmAuto()
        {
            InitializeComponent();
            SetPckerPadUc();
            SetVacListUc();
            //tbcAuto.TabPages.RemoveAt(1);

            m_ucScrapInfo = new UC.UcScrapInfo();
            m_ucScrapInfo.Dock = DockStyle.Fill;
            tabPageScrap.Controls.Add(m_ucScrapInfo);

            //220528
            m_ucTrayInOutWarning = new UC.ucTrayInOutWarning[]
                                {
                                    ucTrayInOutWarning1,
                                    ucTrayInOutWarning2,
                                    ucTrayInOutWarning3,
                                    ucTrayInOutWarning4,
                                    ucTrayInOutWarning5,
                                };

            for (int i = 0; i < m_ucTrayInOutWarning.Length; i++)
            {
                m_ucTrayInOutWarning[i].Location = new Point(10 + (119 * i), 211);
                m_ucTrayInOutWarning[i].Size = new Size(115, 77);
            }


            m_nCheckSeqNo = new int[]
            {
                (int)SEQ_ID.LD_MZ_LD_CONV,
                (int)SEQ_ID.LD_MZ_ULD_CONV,
                (int)SEQ_ID.LD_MZ_ELV_TRANSFER,
                (int)SEQ_ID.STRIP_TRANSFER,
                (int)SEQ_ID.UNIT_TRANSFER,
                (int)SEQ_ID.DRY_UNIT,
                (int)SEQ_ID.MAP_TRANSFER,
                (int)SEQ_ID.MAP_VISION_TABLE_1,
                (int)SEQ_ID.MAP_VISION_TABLE_2,
                (int)SEQ_ID.PICK_N_PLACE_1,
                (int)SEQ_ID.PICK_N_PLACE_2,
                (int)SEQ_ID.TRAY_TRANSFER,
                (int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE,
                (int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE,
                (int)SEQ_ID.UNLOAD_RW_TRAY_TABLE,
                (int)SEQ_ID.ULD_ELV_GOOD_1,
                (int)SEQ_ID.ULD_ELV_GOOD_2,
                (int)SEQ_ID.ULD_ELV_REWORK,
                (int)SEQ_ID.ULD_ELV_EMPTY_1,
                (int)SEQ_ID.ULD_ELV_EMPTY_2,
            };

            m_SeqCurNo = new Label[]
            {
                lbSeqNoLdConv,lbSeqNoUldConv,lbSeqNoLdTrans,lbSeqNoStripPk,lbSeqNoUnitPk, lbSeqNoCleanDry,lbSeqNoMapPk, lbSeqNoMapT1,lbSeqNoMapT2,lbSeqNoPnp1,lbSeqNoPnp2,
                lbSeqNoTrayPk,lbSeqNoGdTray1,lbSeqNoGdTray2,lbSeqNoRwTray,lbSeqNoGdElv1,lbSeqNoGdElv2,lbSeqNoRwTray,lbSeqNoEmtElv1,lbSeqNoEmtElv2

            };

            m_CycCurNo = new Label[]
            {
                lbCycNoLdConv,lbCycNoUldConv,lbCycNoLdTrans,lbCycNoStripPk,lbCycNoUnitPk, lbCycNoCleanDry,lbCycNoMapPk, lbCycNoMapT1,lbCycNoMapT2,lbCycNoPnp1,lbCycNoPnp2,
                lbCycNoTrayPk,lbCycNoGdTray1,lbCycNoGdTray2,lbCycNoRwTray,lbCycNoGdElv1,lbCycNoGdElv2,lbCycNoRwTray,lbCycNoEmtElv1,lbCycNoEmtElv2

            };

            m_SeqBit = new ListView[]
            {
                lsvSeqBitLdConv,lsvSeqBitUldConv,lsvSeqBitLdTrasf,lsvSeqBitStripPk,lsvSeqBitUnitPk, lsvSeqBitCleanDry,lsvSeqBitMapPk, lsvSeqBitMapT1,lsvSeqBitMapT2,lsvSeqBitPnp1,lsvSeqBitPnp2,
                lsvSeqBitTrayPk,lsvSeqBitGdTray1,lsvSeqBitGdTray2,lsvSeqBitRwTray,lsvSeqBitGdElv1,lsvSeqBitGdElv2,lsvSeqBitRwElv,lsvSeqBitEmtElv1,lsvSeqBitEmtElv2
            };


            for (int i = 0; i < m_SeqBit.Length; i++)
            {
                if (m_SeqBit[i] == null) continue;
                m_SeqBit[i].BeginUpdate();
                m_SeqBit[i].View = View.Details;

                ListViewItem lvi;
                for (int j = 1; j < 51; j++)
                {
                    lvi = new ListViewItem(j.ToString());
                    lvi.SubItems.Add("0");
                    m_SeqBit[i].Items.Add(lvi);
                }

                m_SeqBit[i].Items[m_SeqBit[i].Items.Count - 1].EnsureVisible();
                m_SeqBit[i].EndUpdate();
            }
        }

        private void SetPckerPadUc()
        {
            m_PickerPadInfo = new UC.ucPickerPadInfo[]
            {
                ucPickerPadInfo1,  ucPickerPadInfo2,  ucPickerPadInfo3,  ucPickerPadInfo4,  ucPickerPadInfo5,  ucPickerPadInfo6, ucPickerPadInfo7, ucPickerPadInfo8,
                ucPickerPadInfo11, ucPickerPadInfo12, ucPickerPadInfo13, ucPickerPadInfo14, ucPickerPadInfo15, ucPickerPadInfo16, ucPickerPadInfo17, ucPickerPadInfo18
            };
        }
        private void SetVacListUc()
        {
            m_VacList = ucVacList1;
        }
        private void frmAuto_Load(object sender, EventArgs e)
        {
            GbForm.pTest = new popTest();
            GbForm.pTest.OnLotStart += LotStart;
            GbForm.pTest.OnLotEnd += LotEnd;

            IFMgr.Inst.SAW.OnLogAddSaw += AddLogSaw;
            IFMgr.Inst.VISION.OnLogAddVision += AddLogVision;

            UpdateLotProcInfoGridData();

            GbFunc.SetDoubleBuffered(gridLotProcInfo);
            GbFunc.SetDoubleBuffered(dgvTactTime);

            tmrFlicker.Enabled = true;
            tmrRefresh.Enabled = true;
            tmrProcess.Enabled = true;
            tmrSeqStatus.Enabled = true;

            m_ucInspectionMapL = new UC.ucInspectionMap();
            m_ucInspectionMapR = new UC.ucInspectionMap();
            m_ucUldReworkTrayMap = new UC.ucUldTrayMap();
            m_ucUldGoodTrayMap = new UC.ucUldTrayMap();

            m_ucInspectionMapL.TABLE_NO = 0;
            m_ucInspectionMapR.TABLE_NO = 1;

            m_ucUldGoodTrayMap.TABLE_NO = 0;
            m_ucUldReworkTrayMap.TABLE_NO = 1;

            m_ucInspectionMapL.COL = 12;
            m_ucInspectionMapR.COL = 12;
            m_ucInspectionMapL.ROW = 12;
            m_ucInspectionMapR.ROW = 12;

            m_ucUldGoodTrayMap.COL = 8;
            m_ucUldReworkTrayMap.COL = 8;

            m_ucUldGoodTrayMap.ROW = 17;
            m_ucUldReworkTrayMap.ROW = 17;

            m_ucInspectionMapL.Dock = DockStyle.Fill;
            m_ucInspectionMapR.Dock = DockStyle.Fill;
            m_ucUldReworkTrayMap.Dock = DockStyle.Fill;
            m_ucUldGoodTrayMap.Dock = DockStyle.Fill;

            m_ucInspectionMapL.EdgeColor = Color.FromArgb(47, 82, 143);
            m_ucInspectionMapL.GoodColor = Color.Lime;
            m_ucInspectionMapL.NgColor = Color.FromArgb(255, 128, 0);

            m_ucInspectionMapR.EdgeColor = Color.FromArgb(47, 82, 143);
            m_ucInspectionMapR.GoodColor = Color.Lime;
            m_ucInspectionMapR.NgColor = Color.FromArgb(255, 128, 0);

            m_ucUldGoodTrayMap.EdgeColor = Color.Lime;
            m_ucUldGoodTrayMap.GoodColor = Color.Aqua;
            m_ucUldGoodTrayMap.NgColor = Color.Aqua;

            m_ucUldReworkTrayMap.EdgeColor = Color.FromArgb(255, 128, 0);
            m_ucUldReworkTrayMap.GoodColor = Color.Yellow;
            m_ucUldReworkTrayMap.NgColor = Color.Yellow;

            panInspectionStatusL.Controls.Add(m_ucInspectionMapL);
            panInspectionStatusR.Controls.Add(m_ucInspectionMapR);
            panGoodTrayStatus.Controls.Add(m_ucUldGoodTrayMap);
            panReworkTrayStatus.Controls.Add(m_ucUldReworkTrayMap);

            // 0422 KTH
            label_EmptyColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_UnitEmptyColor);  // 0426 KTH 현재 계정 색상 적용 유닛없음의 색
            label_InvaildColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_InvaildColor); // Invaild 상태의 색
            label_NgColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_VisionNgColor);//비전불량의 색

            //랏 투입 시간
            lbLotInputTime.Text = "";
        }

        public void UpdateLotProcInfoGridData()
        {
            gridLotProcInfo.RowCount = 19;

            gridLotProcInfo.Rows[0].Cells[0].Value = FormTextLangMgr.FindKey("스트립 투입 수량");
            gridLotProcInfo.Rows[1].Cells[0].Value = FormTextLangMgr.FindKey("스트립 배출 수량");
            gridLotProcInfo.Rows[2].Cells[0].Value = FormTextLangMgr.FindKey("스트립 컷팅 수량");
            gridLotProcInfo.Rows[3].Cells[0].Value = FormTextLangMgr.FindKey("멀티 피커[1] 픽업 수량");
            gridLotProcInfo.Rows[4].Cells[0].Value = FormTextLangMgr.FindKey("멀티 피커[2] 픽업 수량");
            gridLotProcInfo.Rows[5].Cells[0].Value = FormTextLangMgr.FindKey("멀티 피커[1] 안착 수량");
            gridLotProcInfo.Rows[6].Cells[0].Value = FormTextLangMgr.FindKey("멀티 피커[2] 안착 수량");
            gridLotProcInfo.Rows[7].Cells[0].Value = FormTextLangMgr.FindKey("GOOD 트레이 테이블[1] 트레이 작업수량");
            gridLotProcInfo.Rows[8].Cells[0].Value = FormTextLangMgr.FindKey("GOOD 트레이 테이블[2] 트레이 작업수량");
            gridLotProcInfo.Rows[9].Cells[0].Value = FormTextLangMgr.FindKey("REWORK 트레이 테이블 트레이 작업수량");
            gridLotProcInfo.Rows[10].Cells[0].Value = FormTextLangMgr.FindKey("상부 비전 양품 검출 수량");
            gridLotProcInfo.Rows[11].Cells[0].Value = FormTextLangMgr.FindKey("상부 비전 불량 검출 수량");
            gridLotProcInfo.Rows[12].Cells[0].Value = FormTextLangMgr.FindKey("하부 비전 양품 검출 수량");
            gridLotProcInfo.Rows[13].Cells[0].Value = FormTextLangMgr.FindKey("하부 비전 불량 검출 수량");
            gridLotProcInfo.Rows[14].Cells[0].Value = FormTextLangMgr.FindKey("총 생산량");
            gridLotProcInfo.Rows[15].Cells[0].Value = FormTextLangMgr.FindKey("양품 수량");
            gridLotProcInfo.Rows[16].Cells[0].Value = FormTextLangMgr.FindKey("불량 수량");
            gridLotProcInfo.Rows[17].Cells[0].Value = FormTextLangMgr.FindKey("설비 가동 시간");
            gridLotProcInfo.Rows[18].Cells[0].Value = FormTextLangMgr.FindKey("설비 정지 시간");
        }

        private void btnInit_Click(object sender, EventArgs e)
        {
            // Door 열림
            //if (!GbVar.bTeachingMode)
            //{
            //    if (GbVar.IO[IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0)
            //    {
            //        MessageBox.Show("Cannot Initialize when door open !!!");
            //        return;
            //    }
            //}

            // 도어 체크
            int nDoorResult = SafetyMgr.Inst.GetSafetyDoor(true);
            if (FNC.IsErr(nDoorResult))
            {
                GbVar.g_nDoorAlarmResult = nDoorResult;
                return;
            }

            MotionMgr.Inst.SetDoorLock(true);

            popInitializeSet pInitSet = new popInitializeSet();
            if (pInitSet.ShowDialog() != System.Windows.Forms.DialogResult.OK)
            {
                MotionMgr.Inst.SetDoorLock(false);
                return;
            }
            string strPart = "";
            string strMsg = "";
            string strTitle = "INIT CHECK";

            if (pInitSet.bLoader) strPart += "Loader";
            if (pInitSet.bSaw)
            {
                if (strPart == "") strPart += "Dicing";
                else strPart += ", Dicing";
            }
            if (pInitSet.bSorter)
            {
                if (strPart == "") strPart += "Sorter";
                else strPart += ", Sorter";
            }

            strMsg = string.Format(FormTextLangMgr.FindKey("Do you want initialize") + " {0}?", strPart);

            popMessageBox msg = new popMessageBox(strMsg, strTitle);
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            if (GbVar.mcState.isInitializing[MCDF.LOADER] || GbVar.mcState.isInitializing[MCDF.SAW] || GbVar.mcState.isInitializing[MCDF.SORTER]) return;


            //상태 초기화
            GbVar.mcState.AllCycleInit();



            popInitialize pInit = new popInitialize(pInitSet.bLoader, pInitSet.bSaw, pInitSet.bSorter);
            if (pInit.ShowDialog() == DialogResult.OK)
            {
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "Initial Click");

                //추후에는 시퀀스에서 자동으로 인식해서 작업 이어가기
                if (GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                {
                    GbVar.isInTray = true;
                }
                else
                {
                    GbVar.isInTray = false;
                }

                GbVar.mcState.AllCycleReady();

                GbVar.mcState.isInitializing[MCDF.LOADER] = false;
                GbVar.mcState.isInitializing[MCDF.SAW] = false;
                GbVar.mcState.isInitializing[MCDF.SORTER] = false;

                GbVar.mcState.isInitialized[MCDF.LOADER] = true;
                GbVar.mcState.isInitialized[MCDF.SAW] = true;
                GbVar.mcState.isInitialized[MCDF.SORTER] = true;

                int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;
                int nUnitCount = nTotMapCountX * nTotMapCountY;

                //if (GbVar.Seq.sStripRail.Info.IsStrip())
                //    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;

                //if(GbVar.Seq.sStripTransfer.Info.IsStrip())
                //    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;

                if (GbVar.Seq.sCuttingTable.Info.IsStrip())
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sCuttingTable.Info.LOSS_UNIT += nUnitCount;
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sCuttingTable.Info);
                }
                GbVar.Seq.sCuttingTable.Info.ClearItem();

                if (GbVar.Seq.sUnitTransfer.Info.IsStrip())
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sUnitTransfer.Info.LOSS_UNIT += nUnitCount;
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sUnitTransfer.Info);
                }
                GbVar.Seq.sUnitTransfer.Info.ClearItem();

                if (GbVar.Seq.sUnitDry.Info.IsStrip())
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sUnitDry.Info.LOSS_UNIT += nUnitCount;
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sUnitDry.Info);
                }
                GbVar.Seq.sUnitDry.Info.ClearItem();

                if (GbVar.Seq.sMapTransfer.Info.IsStrip())
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT += nUnitCount;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT += nUnitCount;
                    GbVar.Seq.sMapTransfer.Info.LOSS_UNIT += nUnitCount;
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapTransfer.Info);

                }
                GbVar.Seq.sMapTransfer.Info.ClearItem();

                bool bHead1 = false;
                bool bHead2 = false;

                for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
                {
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[nCnt].IS_UNIT)
                    {
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                        GbVar.Seq.sMapVisionTable[GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo].Info.LOSS_UNIT++;
                        bHead1 = GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo == 0;
                        bHead2 = GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo == 1;
                    }
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[1].unitPickUp[nCnt].IS_UNIT)
                    {
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                        GbVar.Seq.sMapVisionTable[GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo].Info.LOSS_UNIT++;
                        bHead1 = GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo == 0;
                        bHead2 = GbVar.Seq.sPkgPickNPlace.nPickUpMapTableNo == 1;
                    }
                }

                if (GbVar.Seq.sMapVisionTable[0].Info.IsStrip())
                {
                    for (int nRow = GbVar.Seq.sMapVisionTable[0].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                    {
                        for (int nCol = GbVar.Seq.sMapVisionTable[0].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                        {
                            if (GbVar.Seq.sMapVisionTable[0].Info.UnitArr[nRow][nCol].IS_UNIT)
                            {
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                                GbVar.Seq.sMapVisionTable[0].Info.LOSS_UNIT++;
                            }
                        }
                    }
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapVisionTable[0].Info);
                }
                else
                {
                    if (bHead1)
                    {
                        GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapVisionTable[0].Info);
                    }
                }
                GbVar.Seq.sMapVisionTable[0].Info.ClearItem();

                if (GbVar.Seq.sMapVisionTable[1].Info.IsStrip())
                {
                    for (int nRow = GbVar.Seq.sMapVisionTable[1].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                    {
                        for (int nCol = GbVar.Seq.sMapVisionTable[1].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                        {
                            if (GbVar.Seq.sMapVisionTable[1].Info.UnitArr[nRow][nCol].IS_UNIT)
                            {
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                                GbVar.Seq.sMapVisionTable[1].Info.LOSS_UNIT++;
                            }
                        }
                    }
                    GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapVisionTable[1].Info);
                }
                else
                {
                    if (bHead2)
                    {
                        GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapVisionTable[1].Info);
                    }
                }
                GbVar.Seq.sMapVisionTable[1].Info.ClearItem();

                //for (int nRow = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                //{
                //    for (int nCol = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                //    {
                //        if (GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow][nCol].IS_UNIT) GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                //    }
                //}
                //for (int nRow = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                //{
                //    for (int nCol = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                //    {
                //        if (GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow][nCol].IS_UNIT) GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                //    }
                //}
                //for (int nRow = GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length - 1; nRow > -1; nRow--)
                //{
                //    for (int nCol = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                //    {
                //        if (GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT) GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT++;
                //    }
                //}

                GbVar.Seq.Init();
                GbSeq.autoRun.AllInitSeq();

                // Lot End 초기화
                GbVar.Seq.bLotEndReserve = false;
                GbVar.Seq.bIsLotEndRun = false;

                GbVar.Seq.sPkgPickNPlace.pInfo[0].ResetPickerHeadInfo();
                GbVar.Seq.sPkgPickNPlace.pInfo[1].ResetPickerHeadInfo();

                GbVar.Seq.sUldRWTrayTable.Info.Clear();
                GbVar.Seq.sUldRWTrayTable.nUnitPlaceCount = 0;

                GbVar.Seq.sUldGDTrayTable[0].Info.Clear();
                GbVar.Seq.sUldGDTrayTable[1].Info.Clear();

                GbVar.Seq.sUldGDTrayTable[0].nUnitPlaceCount = 0;
                GbVar.Seq.sUldGDTrayTable[1].nUnitPlaceCount = 0;

                // [2022.05.15.kmlee] 초기화 시 Saw 인터페이스 초기화
                IFMgr.Inst.SAW.ResetStripTransferIF();
                IFMgr.Inst.SAW.ResetUnitTransferIF();

                //InitMzMap();
                //InitInspMap();

                MotionMgr.Inst.SetDoorLock(false);

                msg = new popMessageBox(FormTextLangMgr.FindKey("Initialize completed!"), "HOME", MessageBoxButtons.OK);
                msg.TopMost = true;
                msg.ShowDialog(this);
                GbVar.bLotInsert = false;
                GbVar.Seq.bAutoLotEndCheck = false;
            }
            else
            {
                GbVar.mcState.isInitializing[MCDF.LOADER] = false;
                GbVar.mcState.isInitializing[MCDF.SAW] = false;
                GbVar.mcState.isInitializing[MCDF.SORTER] = false;

                GbVar.mcState.isInitialized[MCDF.LOADER] = false;
                GbVar.mcState.isInitialized[MCDF.SAW] = false;
                GbVar.mcState.isInitialized[MCDF.SORTER] = false;

                MotionMgr.Inst.AllStop();
            }
        }
        private void btnStripLoadPause_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();

            switch (nTag)
            {
                case 0:
                    GbVar.INSERT_STRIP_PAUSE = !GbVar.INSERT_STRIP_PAUSE;
                    break;
                case 1:
                    GbVar.SAW_PLACE_PAUSE = !GbVar.SAW_PLACE_PAUSE;
                    break;
                case 2:
                    GbVar.SAW_PICK_UP_PAUSE = !GbVar.SAW_PICK_UP_PAUSE;
                    break;
                case 7:
                    GbVar.DRY_BLOCK_PLACE_PAUSE = !GbVar.DRY_BLOCK_PLACE_PAUSE;
                    break;
                case 8:
                    GbVar.MAP_PICKER_PICK_UP_PAUSE = !GbVar.MAP_PICKER_PICK_UP_PAUSE;
                    break;
                case 9:
                    GbVar.MAP_PICKER_PLACE_PAUSE = !GbVar.MAP_PICKER_PLACE_PAUSE;
                    break;
                case 10:
                    GbVar.TOP_INSP_PAUSE = !GbVar.TOP_INSP_PAUSE;
                    break;
                case 11:
                    GbVar.BTM_INSP_PASE = !GbVar.BTM_INSP_PASE;
                    break;
                case 12:
                    GbVar.CHIP_PICKER_PICK_UP_PAUSE = !GbVar.CHIP_PICKER_PICK_UP_PAUSE;
                    break;
                case 13:
                    GbVar.CHIP_PICKER_PLACE_PAUSE = !GbVar.CHIP_PICKER_PLACE_PAUSE;
                    break;
                case 14:
                    GbVar.TRAY_PICKER_PICK_UP_PAUSE = !GbVar.TRAY_PICKER_PICK_UP_PAUSE;
                    break;
                case 15:
                    GbVar.TRAY_PICKER_PLACE_PAUSE = !GbVar.TRAY_PICKER_PLACE_PAUSE;
                    break;
                case 16:
                    GbVar.Seq.sLdMzLoading.bIsLoadStop = !GbVar.Seq.sLdMzLoading.bIsLoadStop;
                    break;

                case 20:
                    GbVar.LOADER_TEST = !GbVar.LOADER_TEST;
                    break;
                default:
                    break;
            }

        }

        private void btnLotEnd_Click(object sender, EventArgs e)
        {
            //GbVar.mcState.isLoadStop = !GbVar.mcState.isLoadStop;

            //btnLotEnd.BackColor = GbVar.mcState.isLoadStop ? Color.DarkRed : Color.Transparent;
            string strMsg = FormTextLangMgr.FindKey(string.Format("랏엔드를 진행 하시겠습니까?"));

            popMessageBox msg = new popMessageBox(strMsg, "랏엔드");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;


            GbVar.Seq.bLotEndReserve = !GbVar.Seq.bLotEndReserve;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // AUTO RUN 버튼 누르면 도어 인터락 확인
            {
                popMessageBox pMsgStartIntl = null;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)
                {
                    if (GbFunc.IsDoorOpenOrPressEmo(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse) != FNC.SUCCESS)
                    {
                        pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("닫히지 않은 도어가 있습니다. 확인해주세요."), "Door Interlock Warning");
                        pMsgStartIntl.TopMost = true;
                        if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
                    }
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] != 1)
                    {
                        pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("SORTER 도어가 잠금상태가 아닙니다. 잠금 상태를 확인해주세요."), "Door Interlock Warning");
                        pMsgStartIntl.TopMost = true;
                        if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
                    }
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] != 1)
                    {
                        pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("LOADER 도어가 잠금상태가 아닙니다. 잠금 상태를 확인해주세요."), "Door Interlock Warning");
                        pMsgStartIntl.TopMost = true;
                        if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
                    }

                }
            }

            string _strMsg = "AUTO RUN START?";
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) _strMsg += "\nDRY RUN MODE";
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_STRIP_USE].bOptionUse) _strMsg += "\nDRY RUN NO STRIP USE";
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) _strMsg += "\nDRY RUN SAW INTERFACE USE";

            popMessageBox pMsgStart = new popMessageBox(FormTextLangMgr.FindKey("AUTO RUN START?"), "CONFIRM MESSAGE");
            pMsgStart.TopMost = true;
            if (pMsgStart.ShowDialog(this) != DialogResult.OK) return;


            if (RunStartStopEvent != null)
                RunStartStopEvent(MCDF.CMD_RUN);

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "AUTO RUN START BUTTON CLICK");
        }

        private void tmrFlicker_Tick(object sender, EventArgs e)
        {
            //tmrProcess.Enabled = true;
            loop_state = !loop_state;
            if (loop_state == false)
            {
                //lbCpu.Text = cpu.NextValue().ToString() + " %";
                //lbRam.Text = ram.NextValue().ToString() + " MB";
                //lbCpu2.Text = prcess_cpu.NextValue().ToString() + " %";
            }

            #region MAIN_SW
            if (GbVar.mcState.IsAllReady())
            {
                if (GbVar.mcState.IsAllRun())
                {
                    SetColorOperBtn(MCDF.MC_RUN);


                }
                else
                {
                    if (GbVar.mcState.IsError())
                    {
                        SetColorOperBtn(MCDF.MC_ERR_STOP);
                    }
                    else
                    {
                        SetColorOperBtn(MCDF.MC_STOP);
                    }


                }
            }
            else
            {
                if (GbVar.mcState.isInitializing[MCDF.LOADER] || GbVar.mcState.isInitializing[MCDF.SAW] || GbVar.mcState.isInitializing[MCDF.SORTER])
                {
                    SetColorOperBtn(MCDF.MC_INITIALIZING);
                }
                else
                {
                    SetColorOperBtn(MCDF.MC_IDLE); //임시 주석 210915
                }
            }

            //if (GbVar.mcState.isLotEndStart != true)
            //{
            //    if (GbVar.mcState.isLotEndResv)
            //    {
            //        btnLotEnd.BackColor = btnLotEnd.BackColor == Color.Aqua ? SystemColors.ButtonFace : Color.Aqua;
            //    }
            //    else
            //    {
            //        if (btnLotEnd.BackColor != SystemColors.ButtonFace) btnLotEnd.BackColor = SystemColors.ButtonFace;
            //    }
            //}
            //else
            //{
            //    btnLotEnd.BackColor = Color.Aqua;
            //}
            #endregion

            #region Tray In & Out Req
            if (GbVar.bTrayOutReq[MCDF.ELV_TRAY_GOOD_1] == true)
            {
                if (btnUldElv1Out.BackColor == Color.Yellow)
                    btnUldElv1Out.BackColor = SystemColors.Control;
                else
                    btnUldElv1Out.BackColor = Color.Yellow;
            }
            else
            {
                if (btnUldElv1Out.BackColor != SystemColors.Control)
                    btnUldElv1Out.BackColor = SystemColors.Control;

            }

            if (GbVar.bTrayOutReq[MCDF.ELV_TRAY_GOOD_2] == true)
            {
                if (btnUldElv2Out.BackColor == Color.Yellow)
                    btnUldElv2Out.BackColor = SystemColors.Control;
                else
                    btnUldElv2Out.BackColor = Color.Yellow;

            }
            else
            {
                if (btnUldElv2Out.BackColor != SystemColors.Control)
                    btnUldElv2Out.BackColor = SystemColors.Control;

            }

            if (GbVar.bTrayOutReq[MCDF.ELV_TRAY_REWORK] == true)
            {
                if (btnUldElv3Out.BackColor == Color.Yellow)
                    btnUldElv3Out.BackColor = SystemColors.Control;
                else
                    btnUldElv3Out.BackColor = Color.Yellow;

            }
            else
            {
                if (btnUldElv3Out.BackColor != SystemColors.Control)
                    btnUldElv3Out.BackColor = SystemColors.Control;

            }

            //if (GbVar.bTrayInReq[MCDF.ELV_TRAY_EMPTY_1] == true)
            //{
            //    if (btnUldElv4In.BackColor == Color.Yellow)
            //        btnUldElv4In.BackColor = SystemColors.Control;
            //    else
            //        btnUldElv4In.BackColor = Color.Yellow;

            //}
            //else
            //{
            //    if (btnUldElv4In.BackColor != SystemColors.Control)
            //        btnUldElv4In.BackColor = SystemColors.Control;

            //}

            //if (GbVar.bTrayInReq[MCDF.ELV_TRAY_EMPTY_2] == true)
            //{
            //    if (btnUldElv5In.BackColor == Color.Yellow)
            //        btnUldElv5In.BackColor = SystemColors.Control;
            //    else
            //        btnUldElv5In.BackColor = Color.Yellow;

            //}
            //else
            //{
            //    if (btnUldElv5In.BackColor != SystemColors.Control)
            //        btnUldElv5In.BackColor = SystemColors.Control;

            //}
            #endregion

            #region Tray Out Warning
            for (int i = 0; i < MCDF.ELV_TRAY_REWORK; i++)
            {
                if (GbVar.IsAlreadyDetectOutSen[i] == false)
                {
                    m_ucTrayInOutWarning[i].Visible = false;
                    continue;
                }

                if (m_ucTrayInOutWarning[i].Visible == true)
                    m_ucTrayInOutWarning[i].Visible = false;
                else
                    m_ucTrayInOutWarning[i].Visible = true;
            }
            #endregion

            //220516
            #region Tray In Req
            IODF.INPUT[] inputTrayElvCheck = { IODF.INPUT.GD_1_TRAY_ELV_CHECK, IODF.INPUT.GD_2_TRAY_ELV_CHECK, IODF.INPUT.RW_TRAY_ELV_CHECK, IODF.INPUT.EMTY_1_TRAY_ELV_CHECK, IODF.INPUT.EMTY_2_TRAY_ELV_CHECK };
            for (int i = MCDF.ELV_TRAY_EMPTY_1; i < MCDF.ELV_MAX_CNT; i++)
            {
                if (GbVar.IsReqTraySupply[i] == false)
                {
                    m_ucTrayInOutWarning[i].Visible = false;
                    continue;
                }

                // [2022.05.29.kmlee] 센서 감지 시 false
                if (GbVar.IO[inputTrayElvCheck[i]] == 1)
                {
                    GbVar.IsReqTraySupply[i] = false;
                    continue;
                }

                if (m_ucTrayInOutWarning[i].Visible == true)
                    m_ucTrayInOutWarning[i].Visible = false;
                else
                    m_ucTrayInOutWarning[i].Visible = true;
            }
            #endregion

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_PROGRAM_ON] != 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.SORTER_PROGRAM_ON, true);
            }

            //220615
            //오토런이 아닐 때만 보여지게함
            //btnEmptyElv1Out.Visible = !GbVar.mcState.IsRun();
            //btnEmptyElv2Out.Visible = !GbVar.mcState.IsRun();
            btnCntReset.Visible = !GbVar.mcState.IsRun();

            // LOT END 상태 갱신
            if (GbVar.Seq.bIsLotEndRun)
            {
                if (btnLotEnd.BackColor != Color.OrangeRed)
                    btnLotEnd.BackColor = Color.OrangeRed;
            }
            else if (GbVar.Seq.bLotEndReserve)
            {
                if (btnLotEnd.BackColor != SystemColors.Control)
                    btnLotEnd.BackColor = SystemColors.Control;
                else
                    btnLotEnd.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnLotEnd.BackColor != SystemColors.Control)
                    btnLotEnd.BackColor = SystemColors.Control;
            }

            // Good Tray 배출 상태 갱신
            if (GbVar.Seq.bGoodTrayOut && GbVar.mcState.IsRun())
            {
                if (btnGoodTrayOut.BackColor != Color.OrangeRed)
                    btnGoodTrayOut.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnGoodTrayOut.BackColor != SystemColors.Control)
                    btnGoodTrayOut.BackColor = SystemColors.Control;
            }

            if (GbVar.Seq.bReworkTrayOut && GbVar.mcState.IsRun())
            {
                if (btnReworkTrayOut.BackColor != Color.OrangeRed)
                    btnReworkTrayOut.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnReworkTrayOut.BackColor != SystemColors.Control)
                    btnReworkTrayOut.BackColor = SystemColors.Control;
            }

            if (GbVar.Seq.bEmptyElvOut[0] && GbVar.mcState.IsRun())
            {
                if (btnEmptyElv1Out.BackColor != Color.OrangeRed)
                    btnEmptyElv1Out.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnEmptyElv1Out.BackColor != SystemColors.Control)
                    btnEmptyElv1Out.BackColor = SystemColors.Control;
            }

            if (GbVar.Seq.bEmptyElvOut[1] && GbVar.mcState.IsRun())
            {
                if (btnEmptyElv2Out.BackColor != Color.OrangeRed)
                    btnEmptyElv2Out.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnEmptyElv2Out.BackColor != SystemColors.Control)
                    btnEmptyElv2Out.BackColor = SystemColors.Control;
            }
            //생산시간 데이터 집계
            //500msec 타이머라 1초 지날때만 확인
            if (!bOneSecWatchDog)
            {
                if (GbVar.product.nRunTime > 999999999999) GbVar.product.nRunTime = 0;
                if (GbVar.product.nStopTime > 999999999999) GbVar.product.nStopTime = 0;
                if (GbVar.product.nErrTime > 999999999999) GbVar.product.nErrTime = 0;
                if (GbVar.product.nGDCount > 999999999999) GbVar.product.nGDCount = 0;
                if (GbVar.product.nRWCount > 999999999999) GbVar.product.nRWCount = 0;

                if (GbVar.mcState.IsAllRun())
                {
                    GbVar.product.nRunTime++;
                    GbVar.product.nLotRunTime++;
                }
                else
                {
                    if (GbVar.mcState.IsError())
                    {
                        GbVar.product.nErrTime++;
                        GbVar.product.nLotErrTime++;
                    }
                    else
                    {
                        GbVar.product.nStopTime++;
                    }
                }
                IFMgr.Inst.SAW.GetSvValue();

                bOneSecWatchDog = true;
            }
            else
            {
                bOneSecWatchDog = false;
            }

            if (strBoxWarning.Length > 0)
            {
                if (lbBoxWarning.ForeColor != Color.Yellow)
                    lbBoxWarning.ForeColor = Color.Yellow;
                else
                    lbBoxWarning.ForeColor = Color.Orange;
            }
        }

        /// <summary>
        /// OP 버튼 활성화/비활성화. 색상 변경
        /// [2022.04.27.kmlee] Request 중지 상태에서 런 중일 때 STOP 버튼 깜빡이게 변경
        /// </summary>
        /// <param name="nState"></param>
        private void SetColorOperBtn(int nState)
        {
            GbVar.EES_info_New.MCSTATE = nState;

            if (nState == MCDF.MC_IDLE)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                //btnReset.Enabled = true;
                btnInit.Enabled = true;

                if (GbVar.mcState.IsError())
                {
                    //if (GbVar.errorInfos)
                    if (ErrMgr.Inst.ErrStatus.Count > 0)
                    {
                        if (ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_THERE_IS_NO_TRAY ||
                            ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_MGZ_LD_ELV_ALL_EXHAUSTED ||
                            ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_GD_1_ELV_RESIDUAL_DETECT_FAIL ||
                            ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_GD_2_ELV_RESIDUAL_DETECT_FAIL)
                        {

                            //RED BLINK
                            //YELLOW OFF
                            //GREEN OFF
                            GbVar.nLampStatus[0] = MCDF.BLINK;
                            GbVar.nLampStatus[1] = MCDF.OFF;
                            GbVar.nLampStatus[2] = MCDF.ON;

                            //START SWITCH OFF
                            //STOP SWITCH OFF
                            //RESET SWITCH ON
                            GbVar.nSwitchStatus[0] = MCDF.OFF;
                            GbVar.nSwitchStatus[1] = MCDF.OFF;
                            GbVar.nSwitchStatus[2] = MCDF.ON;
                        }
                        else
                        {

                            //RED BLINK
                            //YELLOW OFF
                            //GREEN OFF
                            GbVar.nLampStatus[0] = MCDF.BLINK;
                            GbVar.nLampStatus[1] = MCDF.OFF;
                            GbVar.nLampStatus[2] = MCDF.OFF;

                            //START SWITCH OFF
                            //STOP SWITCH OFF
                            //RESET SWITCH ON
                            GbVar.nSwitchStatus[0] = MCDF.OFF;
                            GbVar.nSwitchStatus[1] = MCDF.OFF;
                            GbVar.nSwitchStatus[2] = MCDF.ON;
                        }
                    }

                }
                else
                {
                    //RED OFF
                    //YELLOW OFF
                    //GREEN OFF
                    GbVar.nLampStatus[0] = MCDF.OFF;
                    GbVar.nLampStatus[1] = MCDF.OFF;
                    GbVar.nLampStatus[2] = MCDF.OFF;

                    //START SWITCH OFF
                    //STOP SWITCH ON
                    //RESET SWITCH OFF
                    GbVar.nSwitchStatus[0] = MCDF.OFF;
                    GbVar.nSwitchStatus[1] = MCDF.ON;
                    GbVar.nSwitchStatus[2] = MCDF.OFF;
                }

                if (btnInit.BackColor == SystemColors.Control) { btnInit.BackColor = Color.Yellow; }
                else { btnInit.BackColor = SystemColors.Control; }

                btnStart.BackColor = SystemColors.Control;
                btnStop.BackColor = SystemColors.Control;
            }
            else if (nState == MCDF.MC_INITIALIZING)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = false;
                //btnReset.Enabled = false;
                btnInit.Enabled = true;

                btnStart.BackColor = SystemColors.Control;
                btnStop.BackColor = SystemColors.Control;
                //btnReset.BackColor = SystemColors.Control;
                btnInit.BackColor = Color.Yellow;

                //RED OFF
                //YELLOW BLINK
                //GREEN OFF
                GbVar.nLampStatus[0] = MCDF.OFF;
                GbVar.nLampStatus[1] = MCDF.BLINK;
                GbVar.nLampStatus[2] = MCDF.OFF;

                //START SWITCH OFF
                //STOP SWITCH ON
                //RESET SWITCH OFF
                GbVar.nSwitchStatus[0] = MCDF.OFF;
                GbVar.nSwitchStatus[1] = MCDF.ON;
                GbVar.nSwitchStatus[2] = MCDF.OFF;
            }
            else if (nState == MCDF.MC_STOP)
            {
                btnStop.Enabled = true;
                //btnReset.Enabled = true;
                btnInit.Enabled = true;

                btnStart.BackColor = SystemColors.Control;
                //btnPause.BackColor = Color.Red;
                //btnReset.BackColor = SystemColors.Control;
                btnInit.BackColor = SystemColors.Control;

                if (GbVar.mcState.IsRunAndAllStopRunReq())
                {
                    if (btnStart.Enabled)
                        btnStart.Enabled = false;

                    if (btnStop.BackColor == SystemColors.Control)
                    {
                        btnStop.BackColor = Color.Red;
                    }
                    else
                    {
                        btnStop.BackColor = SystemColors.Control;
                    }
                }
                else
                {
                    if (!btnStart.Enabled)
                        btnStart.Enabled = true;
                    btnStop.BackColor = SystemColors.Control;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.AIRKNIFE_RUN_START_ON_USE].bOptionUse )
                    {
                        if (GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_1] == 1 ||
                        GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_2] == 1 ||
                        GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT] == 1 ||
                        GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE] == 1 ||
                        GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE] == 1 ||
                        GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_KNIFE] == 1)
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);
                        }
                    }

                    GbVar.LotMgr.UpdateAllLotInfo(GbVar.lstBinding_HostLot);
                    GbVar.LotMgr.UpdateAllProcQtyInfo(GbVar.lstBinding_EqpProc);//220622 멈출 때도 저장 
                }

                //RED OFF
                //YELLOW ON
                //GREEN OFF
                GbVar.nLampStatus[0] = MCDF.OFF;
                GbVar.nLampStatus[1] = MCDF.ON;
                GbVar.nLampStatus[2] = MCDF.OFF;

                //START SWITCH OFF
                //STOP SWITCH ON
                //RESET SWITCH OFF
                GbVar.nSwitchStatus[0] = MCDF.OFF;
                GbVar.nSwitchStatus[1] = MCDF.ON;
                GbVar.nSwitchStatus[2] = MCDF.OFF;
            }
            else if (nState == MCDF.MC_ERR_STOP)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                //btnReset.Enabled = true;
                btnInit.Enabled = true;

                btnStart.BackColor = SystemColors.Control;

                if (GbVar.mcState.IsRunAndAllStopRunReq())
                {
                    if (btnStop.BackColor == SystemColors.Control)
                    {
                        btnStop.BackColor = Color.Red;
                    }
                    else
                    {
                        btnStop.BackColor = SystemColors.Control;
                    }
                }
                else
                {
                    btnStop.BackColor = SystemColors.Control;

                    GbVar.LotMgr.UpdateAllLotInfo(GbVar.lstBinding_HostLot);
                    GbVar.LotMgr.UpdateAllProcQtyInfo(GbVar.lstBinding_EqpProc);//220622 멈출 때도 저장 
                }

                //RED BLINK
                //YELLOW OFF
                //GREEN OFF
                GbVar.nLampStatus[0] = MCDF.BLINK;
                GbVar.nLampStatus[1] = MCDF.OFF;
                GbVar.nLampStatus[2] = MCDF.OFF;

                //START SWITCH OFF
                //STOP SWITCH ON
                //RESET SWITCH ON
                GbVar.nSwitchStatus[0] = MCDF.OFF;
                GbVar.nSwitchStatus[1] = MCDF.ON;
                GbVar.nSwitchStatus[2] = MCDF.ON;
            }
            else if (nState == MCDF.MC_RUN)
            {
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                //btnReset.Enabled = true;
                btnInit.Enabled = false;

                if (GbVar.mcState.IsRunAndAllStopRunReq())
                {
                    if (btnStop.BackColor == SystemColors.Control)
                    {
                        btnStop.BackColor = Color.Red;
                    }
                    else
                    {
                        btnStop.BackColor = SystemColors.Control;
                    }
                }
                else
                {
                    //2023-01-24 ic.chol 1 호기 적용 해서 2호기 적용 sj.shin
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.AIRKNIFE_RUN_START_ON_USE].bOptionUse)
                    {
                        if (GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_1] == 0 ||
                           GbVar.IO[IODF.OUTPUT.CLEAN_AIR_KNIFE_2] == 0 ||
                           GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_KNIFE] == 0 ||
                           GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE] == 0 ||
                           GbVar.IO[IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE] == 0 ||
                           GbVar.IO[IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT] == 0)
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, true);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, true);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, true);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, true);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, true);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, true);
                        }
                    }
                    btnStop.BackColor = SystemColors.Control;
                }

                btnStart.BackColor = Color.Lime;
                //btnPause.BackColor = SystemColors.Control;
                //btnReset.BackColor = SystemColors.Control;
                btnInit.BackColor = SystemColors.Control;

                //RED OFF
                //YELLOW OFF
                //GREEN ON
                GbVar.nLampStatus[0] = MCDF.OFF;
                GbVar.nLampStatus[1] = MCDF.OFF;
                GbVar.nLampStatus[2] = MCDF.ON;

                //START SWITCH ON
                //STOP SWITCH OFF
                //RESET SWITCH OFF
                GbVar.nSwitchStatus[0] = MCDF.ON;
                GbVar.nSwitchStatus[1] = MCDF.OFF;
                GbVar.nSwitchStatus[2] = MCDF.OFF;
            }
        }

        private void frmAuto_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmrFlicker.Enabled = false;
        }

        private void btnStripInfo_Click(object sender, EventArgs e)
        {
            //if (GbForm.pTest == null)
            //{
            //    GbForm.pTest = new popTest();
            //}
            //else
            //{
            //    if (GbForm.pTest.Visible == true) return;
            //    GbForm.pTest.Dispose();
            //    GbForm.pTest = new popTest();
            //}
            //GbForm.pTest.Show();

            if (pSubMap == null)
            {
                pSubMap = new popSubMap();
            }
            else
            {
                if (pSubMap.Visible == true) return;
                pSubMap.Dispose();
                pSubMap = new popSubMap();
            }

            Button btn = sender as Button;
            string sTag = btn.Tag.ToString();
            int nTag = 0;
            if (!int.TryParse(sTag, out nTag)) nTag = 0;

            pSubMap.LoadStripInfos((STRIP_MDL)nTag);
            pSubMap.Show();
        }

        private void btnTL_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            m_nTL = nTag;

            for (int i = 0; i < m_btnTL.Length; i++)
            {
                m_btnTL[i].BackColor = Color.FromArgb(240, 240, 240);
            }
            m_btnTL[nTag - 2].BackColor = Color.SkyBlue;
        }

        private void btnTU_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            m_nTU = nTag;

            for (int i = 0; i < m_btnTU.Length; i++)
            {
                m_btnTU[i].BackColor = Color.FromArgb(240, 240, 240);
            }
            m_btnTU[nTag].BackColor = Color.SkyBlue;
        }

        private void tmrOht_Tick(object sender, EventArgs e)
        {

        }

        private void tbcAuto_SelectedIndexChanged(object sender, EventArgs e)
        {
            tmrProcess.Enabled = tbcAuto.SelectedIndex == 0 || tbcAuto.SelectedIndex == 1 || tbcAuto.SelectedIndex == 3;
            tmrIfRefresh.Enabled = tbcAuto.SelectedIndex == 2;
            tmrStatus.Enabled = tbcAuto.SelectedIndex == 3;
            //tmrIfRefresh.Enabled = tbcAuto.SelectedIndex == 1;
            //tmrStatus.Enabled = tbcAuto.SelectedIndex == 2;
            tmrSeqStatus.Enabled = tbcAuto.SelectedIndex == 0 || tbcAuto.SelectedIndex == 3;
            TabControl tbc = sender as TabControl;
            for (int i = 0; i < tbc.TabPages.Count; i++)
            {
                if (tbc.SelectedIndex == i) tbc.TabPages[i].ImageIndex = 1;
                else tbc.TabPages[i].ImageIndex = 0;
            }
        }

        private void btnStop_MouseDown(object sender, MouseEventArgs e)
        {
            m_nStopTicks = 0;
            m_bStopMouseDown = true;
        }

        private void btnStop_MouseUp(object sender, MouseEventArgs e)
        {
            m_bStopMouseDown = false;
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (lbPre2DCode.Text != GbVar.Seq.sStripRail.Info.STRIP_ID)
                lbPre2DCode.Text = GbVar.Seq.sStripRail.Info.STRIP_ID;

            TimeSpan timeRun = TimeSpan.FromSeconds((double)GbVar.product.nRunTime);
            TimeSpan timeStop = TimeSpan.FromSeconds((double)GbVar.product.nStopTime);
            TimeSpan timeErr = TimeSpan.FromSeconds((double)GbVar.product.nErrTime);

            TimeSpan timeLotRun = TimeSpan.FromSeconds((double)GbVar.product.nLotRunTime);
            TimeSpan timeLotErr = TimeSpan.FromSeconds((double)GbVar.product.nLotErrTime);

            lbRunningTime.Text = timeRun.ToString(@"hh\:mm\:ss");//time.ToString(@"hh\:mm\:ss\:fff");
            lbStopTime.Text = timeStop.ToString(@"hh\:mm\:ss");
            lbAlarmTime.Text = timeErr.ToString(@"hh\:mm\:ss");

            if (lbUPH.Text != GbVar.g_dUPH.ToString("F3"))
            {
                lbUPH.Text = GbVar.g_dUPH.ToString("F3");
            }

            string strLotID = "None";
            if (GbVar.lstBinding_HostLot.Count > 0)
            {
                strLotID = GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID;
            }

            if (m_bStopMouseDown || MESDF.IS_STOP_FROM_HOST)
            {
                if (MESDF.IS_STOP_FROM_HOST == true)
                    MESDF.IS_STOP_FROM_HOST = false;

                btnStop.BackColor = Color.FromArgb(255, 220 - m_nStopTicks * 2, 220 - m_nStopTicks * 2);
                pgStop.Value = m_nStopTicks;
                if (m_nStopTicks >= 100)
                {
                    //Stop Event
                    m_bStopMouseDown = false;

                    m_bIsStop = true;
                }
                m_nStopTicks += 20;
            }
            else if (m_bIsStop)
            {
                btnStop.BackColor = SystemColors.ControlLight;
                m_bIsStop = false;
                m_nStopTicks = 0;
                pgStop.Value = 0;

                if (RunStartStopEvent != null)
                    RunStartStopEvent(MCDF.CMD_STOP);

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "PAUSE BUTTON CLICK");

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "STOP BUTTON CLICK");
            }
            else
            {
                if (m_nStopTicks != 0)
                {
                    btnStop.BackColor = SystemColors.ControlLight;
                    //Stop Event
                    m_bStopMouseDown = false;

                    if (RunStartStopEvent != null)
                        RunStartStopEvent(MCDF.CMD_CYCLE_STOP);

                    GbFunc.WriteEventLog(this.GetType().Name.ToString(), "STOP BUTTON CLICK");

                    m_bIsStop = false;
                    m_nStopTicks = 0;
                    pgStop.Value = 0;
                }
            }

            if (listLog.Items.Count > 1000)
            {
                listLog.BeginUpdate();
                listLog.Items.Clear();
                listLog.EndUpdate();
            }

            if (GbVar.Seq.bOneStripLoad == true)
            {
                if (btnOneStripLoad.BackColor != Color.Yellow)
                    btnOneStripLoad.BackColor = Color.Yellow;
            }
            else
            {
                if (btnOneStripLoad.BackColor != SystemColors.Control)
                    btnOneStripLoad.BackColor = SystemColors.Control;
            }
            if (GbVar.qMsg.Count > 0)
            {
                GbVar.qMsg.Clear();
                //listLog.BeginUpdate();

                //int nCnt = GbVar.qMsg.Count;
                //for (int i = 0; i < nCnt; i++)
                //{
                //    string[] strMsg = GbVar.qMsg.Dequeue();
                //    ListViewItem lvi = new ListViewItem(strMsg[0]);
                //    lvi.SubItems.Add("");
                //    lvi.SubItems.Add(strMsg[1]);
                //    listLog.Items.Add(lvi);
                //}

                //listLog.Items[listLog.Items.Count - 1].EnsureVisible();
                //listLog.EndUpdate();
            }

            lbPnP1Ready.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] != true ? SystemColors.Control : Color.Lime;
            lbPnP1PickUp.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP1Inspection.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP1SQPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP1NQPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP1NGPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START] != true ? SystemColors.Control : Color.Lime;

            lbPnP2PickUp.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_2].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP2Inspection.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_2].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP2SQPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_2].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP2NQPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_2].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] != true ? SystemColors.Control : Color.Lime;
            lbPnP2NGPlace.BackColor = GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_2].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START] != true ? SystemColors.Control : Color.Lime;

            lbDryRun.BackColor = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true ? Color.Gold : SystemColors.Control;
            lbVacSkip.BackColor = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse == false ? Color.Gold : SystemColors.Control;
            lbTopVisionSkip.BackColor = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_VISION_USE].bOptionUse == false ? Color.Gold : SystemColors.Control;
            lbBtmVisionSkip.BackColor = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse == false ? Color.Gold : SystemColors.Control;

            #region LOADER
            if (GbVar.Seq.sLdMzLoading.bMgzUnloadReq)
            {
                if (btnWorkMgzUnload.BackColor != Color.LimeGreen)
                    btnWorkMgzUnload.BackColor = Color.LimeGreen;
            }
            else
            {
                if (btnWorkMgzUnload.BackColor != SystemColors.Control)
                    btnWorkMgzUnload.BackColor = SystemColors.Control;
            }

            if (GbVar.Seq.sStripTransfer != null)
            {
                if (GbVar.Seq.sStripTransfer.nMgzLoadSlotNum.ToString() != lbCurrentMgzSlotCount.ToString())
                {
                    lbCurrentMgzSlotCount.Text = GbVar.Seq.sStripTransfer.nMgzLoadSlotNum.ToString();
                }
            }
            #endregion

            #region LOAD & UNLOAD PAUSE BUTTON
            if (GbVar.INSERT_STRIP_PAUSE)
            {
                if (btnStripLoadPause.BackColor != Color.OrangeRed)
                    btnStripLoadPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnStripLoadPause.BackColor != Color.Gainsboro)
                    btnStripLoadPause.BackColor = Color.Gainsboro;
            }
            if (GbVar.SAW_PLACE_PAUSE)
            {
                if (btnSawPlacePause.BackColor != Color.OrangeRed)
                    btnSawPlacePause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnSawPlacePause.BackColor != Color.Gainsboro)
                    btnSawPlacePause.BackColor = Color.Gainsboro;
            }
            if (GbVar.SAW_PICK_UP_PAUSE)
            {
                if (btnSawPickUpPause.BackColor != Color.OrangeRed)
                    btnSawPickUpPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnSawPickUpPause.BackColor != Color.Gainsboro)
                    btnSawPickUpPause.BackColor = Color.Gainsboro;
            }
            if (GbVar.DRY_BLOCK_PLACE_PAUSE)
            {
                if (btnDryBlockPlacePause.BackColor != Color.OrangeRed)
                    btnDryBlockPlacePause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnDryBlockPlacePause.BackColor != Color.Gainsboro)
                    btnDryBlockPlacePause.BackColor = Color.Gainsboro;
            }
            if (GbVar.MAP_PICKER_PICK_UP_PAUSE)
            {
                if (btnMapPickerPickUpPause.BackColor != Color.OrangeRed)
                    btnMapPickerPickUpPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnMapPickerPickUpPause.BackColor != Color.Gainsboro)
                    btnMapPickerPickUpPause.BackColor = Color.Gainsboro;
            }
            if (GbVar.MAP_PICKER_PLACE_PAUSE)
            {
                if (btnMapTablePlacePause.BackColor != Color.OrangeRed)
                    btnMapTablePlacePause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnMapTablePlacePause.BackColor != Color.Gainsboro)
                    btnMapTablePlacePause.BackColor = Color.Gainsboro;
            }

            if (GbVar.TOP_INSP_PAUSE)
            {
                if (btnTopInspPause.BackColor != Color.OrangeRed)
                    btnTopInspPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnTopInspPause.BackColor != Color.Gainsboro)
                    btnTopInspPause.BackColor = Color.Gainsboro;
            }
            if (GbVar.BTM_INSP_PASE)
            {
                if (btnBtmInspPause.BackColor != Color.OrangeRed)
                    btnBtmInspPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnBtmInspPause.BackColor != Color.Gainsboro)
                    btnBtmInspPause.BackColor = Color.Gainsboro;
            }

            if (GbVar.CHIP_PICKER_PICK_UP_PAUSE)
            {
                if (btnChipPickerPickUpPause.BackColor != Color.OrangeRed)
                    btnChipPickerPickUpPause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnChipPickerPickUpPause.BackColor != Color.Gainsboro)
                    btnChipPickerPickUpPause.BackColor = Color.Gainsboro;
            }
            if (GbVar.CHIP_PICKER_PLACE_PAUSE)
            {
                if (btnChipPickerPlacePause.BackColor != Color.OrangeRed)
                    btnChipPickerPlacePause.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnChipPickerPlacePause.BackColor != Color.Gainsboro)
                    btnChipPickerPlacePause.BackColor = Color.Gainsboro;
            }
            if(GbVar.Seq.sLdMzLoading.bIsLoadStop)
            {
                if (btnMgz_LoadStop.BackColor != Color.OrangeRed)
                    btnMgz_LoadStop.BackColor = Color.OrangeRed;
            }
            else
            {
                if (btnMgz_LoadStop.BackColor != Color.Gainsboro)
                    btnMgz_LoadStop.BackColor = Color.Gainsboro;
            }

            if (GbVar.LOADER_TEST)
            {
                if (button1.BackColor != Color.OrangeRed)
                    button1.BackColor = Color.OrangeRed;
            }
            else
            {
                if (button1.BackColor != Color.Gainsboro)
                    button1.BackColor = Color.Gainsboro;
            }
            #endregion

            #region BOX INPUT
            //A접점
            if (GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_CHECK] == 1)
            {
                if (lbRejectBoxExist.BackColor != Color.Lime)
                {
                    lbRejectBoxExist.BackColor = Color.Lime;
                    strBoxWarning = strBoxWarning.Replace(FormTextLangMgr.FindKey("REJECT BOX IS NOT EXIST!!    "), "");
                }
            }
            else
            {
                if (lbRejectBoxExist.BackColor != SystemColors.Control)
                {
                    lbRejectBoxExist.BackColor = SystemColors.Control;
                    strBoxWarning += FormTextLangMgr.FindKey("REJECT BOX IS NOT EXIST!!    ");
                }
            }

            //B접점
            if (GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_FULL1] == 0)
            {
                if (lbRejectBoxFull1.BackColor != Color.Lime)
                {
                    lbRejectBoxFull1.BackColor = Color.Lime;
                    strBoxWarning += FormTextLangMgr.FindKey("REJECT BOX 1 IS FULL!!    ");
                }
            }
            else
            {
                if (lbRejectBoxFull1.BackColor != SystemColors.Control)
                {
                    lbRejectBoxFull1.BackColor = SystemColors.Control;
                    strBoxWarning = strBoxWarning.Replace(FormTextLangMgr.FindKey("REJECT BOX 1 IS FULL!!    "), "");
                }
            }

            //B접점
            if (GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_FULL2] == 0)
            {
                if (lbRejectBoxFull2.BackColor != Color.Lime)
                {
                    lbRejectBoxFull2.BackColor = Color.Lime;
                    strBoxWarning += FormTextLangMgr.FindKey("REJECT BOX 2 IS FULL!!    ");
                }
            }
            else
            {
                if (lbRejectBoxFull2.BackColor != SystemColors.Control)
                {
                    lbRejectBoxFull2.BackColor = SystemColors.Control;
                    strBoxWarning = strBoxWarning.Replace(FormTextLangMgr.FindKey("REJECT BOX 2 IS FULL!!    "), "");
                }
            }

            //B접점
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SCRAP_BOX_CHECK] == 0)
            {
                if (lbScrapBoxExist.BackColor != SystemColors.Control)
                {
                    lbScrapBoxExist.BackColor = SystemColors.Control;
                    strBoxWarning += FormTextLangMgr.FindKey("SCRAP BOX IS NOT EXIST!!    ");
                }
            }
            else
            {
                if (lbScrapBoxExist.BackColor != Color.Lime)
                {
                    lbScrapBoxExist.BackColor = Color.Lime;
                    strBoxWarning = strBoxWarning.Replace(FormTextLangMgr.FindKey("SCRAP BOX IS NOT EXIST!!    "), "");
                }
            }

            //B접점
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SCRAP_BOX_FULL_CHECK] == 1)
            {
                if (lbScrapBoxFull.BackColor != Color.Lime)
                {
                    lbScrapBoxFull.BackColor = Color.Lime;
                    strBoxWarning += FormTextLangMgr.FindKey("SCRAP BOX IS FULL!!      ");
                }
            }
            else
            {
                if (lbScrapBoxFull.BackColor != SystemColors.Control)
                {
                    lbScrapBoxFull.BackColor = SystemColors.Control;
                    strBoxWarning = strBoxWarning.Replace(FormTextLangMgr.FindKey("SCRAP BOX IS FULL!!      "), "");
                }
            }


            lbBoxWarning.Text = strBoxWarning;
            #endregion

            m_VacList.refresh_VacInfo();

            if (GbVar.IO[IODF.OUTPUT.FLUORESCENT_LIGHT_ONOFF] == 0)
            {
                if (btnLightOn.BackColor != SystemColors.Control)
                    btnLightOn.BackColor = SystemColors.Control;
            }
            else
            {
                if (btnLightOn.BackColor != Color.Yellow)
                    btnLightOn.BackColor = Color.Yellow;
            }

            if (lbLotId.Text != strLotID)
            {
                lbLotId.Text = strLotID;
            }
            if (GbVar.mcState.IsRun())
            {
                if (btnLotInfoReg.Enabled)
                {
                    btnLotInfoReg.Enabled = false;
                }
            }
            else
            {
                if (!btnLotInfoReg.Enabled)
                {
                    btnLotInfoReg.Enabled = true;
                }
            }
        }

        private void tmrProcess_Tick(object sender, EventArgs e)
        {
            tmrProcess.Enabled = false;

            m_ucInspectionMapL.Invalidate();
            m_ucInspectionMapR.Invalidate();


            m_ucUldGoodTrayMap.Invalidate();
            m_ucUldReworkTrayMap.Invalidate();

            for (int i = 0; i < m_PickerPadInfo.Length; i++)
            {
                if (m_PickerPadInfo[i] == null) continue;

                int nIoIdx = (int)m_PickerPadInfo[i].A_INPUT;
                double dVacValue = (GbVar.GB_AINPUT[nIoIdx] - ConfigMgr.Inst.Cfg.Vac[nIoIdx].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nIoIdx].dRatio;

                m_PickerPadInfo[i].VAC_VAL = dVacValue.ToString("F1");

                if (dVacValue < ConfigMgr.Inst.Cfg.Vac[nIoIdx].dVacLevelLow)
                {
                    m_PickerPadInfo[i].ON = true;
                }
                else
                {
                    m_PickerPadInfo[i].ON = false;
                }

                if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.NOCHIP)
                {
                    m_PickerPadInfo[i].TOP_JUDGE_TEXT = "WAIT";
                }
                else if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.OK)
                {
                    m_PickerPadInfo[i].TOP_JUDGE_TEXT = "OK";
                }
                else if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].TOP_INSP_RESULT == (int)VSDF.eJUDGE_MAP.RW)
                {
                    m_PickerPadInfo[i].TOP_JUDGE_TEXT = "RW";
                }
                else
                {
                    m_PickerPadInfo[i].TOP_JUDGE_TEXT = "NG";
                }

                if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.NOCHIP)
                {
                    m_PickerPadInfo[i].BTM_JUDGE_TEXT = "WAIT";
                }
                else if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.OK)
                {
                    m_PickerPadInfo[i].BTM_JUDGE_TEXT = "OK";
                }
                else if (GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1 + (i / CFG_DF.MAX_PICKER_PAD_CNT)].unitPickUp[i % CFG_DF.MAX_PICKER_PAD_CNT].BTM_INSP_RESULT == (int)VSDF.eJUDGE_BALL.RW)
                {
                    m_PickerPadInfo[i].BTM_JUDGE_TEXT = "RW";
                }
                else
                {
                    m_PickerPadInfo[i].BTM_JUDGE_TEXT = "NG";
                }
            }

            ShowIsStripA1Panel();
            ShowLabel();

            #region LOT INFO
            if (GbVar.lstBinding_EqpProc != null)
            {
                if (GbVar.lstBinding_EqpProc.Count > 0)
                {
                    gridLotProcInfo.Rows[0].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT;//"스트립 투입 수량";
                    gridLotProcInfo.Rows[1].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT;//"스트립 배출 수량";
                    gridLotProcInfo.Rows[2].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_CUTTING_COUNT;//"스트립 컷팅 수량";
                    gridLotProcInfo.Rows[3].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PICKUP_COUNT;//"멀티 피커[1] 픽업 수량";
                    gridLotProcInfo.Rows[4].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_2_PICKUP_COUNT;//"멀티 피커[2] 픽업 수량";
                    gridLotProcInfo.Rows[5].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PLACE_COUNT;//"멀티 피커[1] 안착 수량";
                    gridLotProcInfo.Rows[6].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_2_PLACE_COUNT;//"멀티 피커[2] 안착 수량";
                    gridLotProcInfo.Rows[7].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT;//"GOOD 트레이 테이블[1] 트레이 작업수량";
                    gridLotProcInfo.Rows[8].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_2_TRAY_WORK_COUNT;//"GOOD 트레이 테이블[2] 트레이 작업수량";
                    gridLotProcInfo.Rows[9].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].REWORK_TABLE_TRAY_WORK_COUNT;//"REWORK 트레이 테이블 트레이 작업수량";
                    gridLotProcInfo.Rows[10].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_OK_COUNT;//"상부 비전 양품 검출 수량";
                    gridLotProcInfo.Rows[11].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_RW_COUNT;//"상부 비전 불량 검출 수량";
                    gridLotProcInfo.Rows[12].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_OK_COUNT;//"하부 비전 양품 검출 수량";
                    gridLotProcInfo.Rows[13].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_NG_COUNT;//"하부 비전 불량 검출 수량";
                    gridLotProcInfo.Rows[14].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT;//"총 생산량";
                    gridLotProcInfo.Rows[15].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_OK_COUNT;//"양품 수량";
                    gridLotProcInfo.Rows[16].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_NG_COUNT;//"불량 수량";

                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_RUNNING_TIME = lbRunningTime.Text;
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_STOP_TIME = lbStopTime.Text;

                    gridLotProcInfo.Rows[17].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_RUNNING_TIME;//"설비 가동 시간";
                    gridLotProcInfo.Rows[18].Cells[1].Value = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_STOP_TIME;//"설비 정지 시간";
                    if (GbVar.lstBinding_HostLot != null &&
                        GbVar.lstBinding_HostLot.Count > 0)
                    {
                        if (lbCurMgzQty.Text != GbVar.lstBinding_HostLot[MCDF.CURRLOT].SUB_QTY.ToString())
                            lbCurMgzQty.Text = GbVar.lstBinding_HostLot[MCDF.CURRLOT].SUB_QTY.ToString();
                    }

                    if (lbCurGdTrayQty.Text != (GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT + GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_2_TRAY_WORK_COUNT).ToString())
                        lbCurGdTrayQty.Text = (GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT + GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_2_TRAY_WORK_COUNT).ToString();
                    if (lbCurRwTrayQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].REWORK_TABLE_TRAY_WORK_COUNT.ToString())
                        lbCurRwTrayQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].REWORK_TABLE_TRAY_WORK_COUNT.ToString();

                    if (lbCurStripQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT.ToString())
                        lbCurStripQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT.ToString();

                    if (lbCurOutStrip.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT.ToString())
                        lbCurOutStrip.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT.ToString();

                    if (lbCurXOutQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_NG_COUNT.ToString())
                        lbCurXOutQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_NG_COUNT.ToString();

                    if (lbCurTotalUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT.ToString())
                        lbCurTotalUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT.ToString();

                    if (lbCurRwUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_RW_COUNT.ToString())
                        lbCurRwUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_RW_COUNT.ToString();

                    if (lbCurGdUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_OK_COUNT.ToString())
                        lbCurGdUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_OK_COUNT.ToString();

                    if (lbCurLossUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT.ToString())
                        lbCurLossUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOSS_UNIT_COUNT.ToString();


                    if (GbVar.lstBinding_EqpProc.Count > 1)//220611
                    {
                        if (GbVar.lstBinding_HostLot != null &&
                            GbVar.lstBinding_HostLot.Count > 1)
                        {
                            if (lbPreMgzQty.Text != GbVar.lstBinding_HostLot[MCDF.PREVLOT].SUB_QTY.ToString())
                                lbPreMgzQty.Text = GbVar.lstBinding_HostLot[MCDF.PREVLOT].SUB_QTY.ToString();
                        }

                        if (lbPreGdTrayQty.Text != (GbVar.lstBinding_EqpProc[MCDF.PREVLOT].GOOD_TABLE_1_TRAY_WORK_COUNT + GbVar.lstBinding_EqpProc[MCDF.PREVLOT].GOOD_TABLE_2_TRAY_WORK_COUNT).ToString())
                            lbPreGdTrayQty.Text = (GbVar.lstBinding_EqpProc[MCDF.PREVLOT].GOOD_TABLE_1_TRAY_WORK_COUNT + GbVar.lstBinding_EqpProc[MCDF.PREVLOT].GOOD_TABLE_2_TRAY_WORK_COUNT).ToString();
                        if (lbPreRwTrayQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].REWORK_TABLE_TRAY_WORK_COUNT.ToString())
                            lbPreRwTrayQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].REWORK_TABLE_TRAY_WORK_COUNT.ToString();

                        if (lbPreStripQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].STRIP_INPUT_COUNT.ToString())
                            lbPreStripQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].STRIP_INPUT_COUNT.ToString();

                        if (lbPreOutStrip.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].STRIP_OUTPUT_COUNT.ToString())
                            lbPreOutStrip.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].STRIP_OUTPUT_COUNT.ToString();

                        if (lbPreXOutQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_NG_COUNT.ToString())
                            lbPreXOutQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_NG_COUNT.ToString();

                        if (lbPreTotalUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_CHIP_PROD_COUNT.ToString())
                            lbPreTotalUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_CHIP_PROD_COUNT.ToString();

                        if (lbPreRwUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_RW_COUNT.ToString())
                            lbPreRwUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_RW_COUNT.ToString();

                        if (lbPreGdUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_OK_COUNT.ToString())
                            lbPreGdUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].TOTAL_OK_COUNT.ToString();

                        if (lbPreLossUnitQty.Text != GbVar.lstBinding_EqpProc[MCDF.PREVLOT].LOSS_UNIT_COUNT.ToString())
                            lbPreLossUnitQty.Text = GbVar.lstBinding_EqpProc[MCDF.PREVLOT].LOSS_UNIT_COUNT.ToString();
                    }
                }
            }
            #endregion

            tmrProcess.Enabled = true;

            if (GbVar.Seq.bAutoLotEndCheck)
            {
                if (!GbVar.Seq.bLotEndReserve &&
                    !GbVar.Seq.bIsLotEndRun)
                {
                    GbVar.Seq.bAutoLotEndCheck = false;
                }
                if (btnLotEnd.Enabled)
                {
                    btnLotEnd.Enabled = false;
                }
            }
            else
            {
                if (!btnLotEnd.Enabled)
                {
                    btnLotEnd.Enabled = true;
                }
            }
        }

        void ShowIsStripA1Panel()
        {
            bool bIsOn = false;
            bIsOn = GbVar.Seq.sStripRail.Info.IsStrip();
            if (bIsOn && a1Panel9.GradientEndColor != Color.LawnGreen)
            {
                a1Panel9.GradientEndColor = Color.LawnGreen;
                a1Panel9.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1Panel9.GradientEndColor != SystemColors.Control)
            {
                a1Panel9.GradientEndColor = SystemColors.Control;
                a1Panel9.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sStripTransfer.Info.IsStrip();
            if (bIsOn && a1pStripPk.GradientEndColor != Color.LawnGreen)
            {
                a1pStripPk.GradientEndColor = Color.LawnGreen;
                a1pStripPk.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pStripPk.GradientEndColor != SystemColors.Control)
            {
                a1pStripPk.GradientEndColor = SystemColors.Control;
                a1pStripPk.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sUnitTransfer.Info.IsStrip();
            if (bIsOn && a1pUnitPk.GradientEndColor != Color.LawnGreen)
            {
                a1pUnitPk.GradientEndColor = Color.LawnGreen;
                a1pUnitPk.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pUnitPk.GradientEndColor != SystemColors.Control)
            {
                a1pUnitPk.GradientEndColor = SystemColors.Control;
                a1pUnitPk.GradientStartColor = SystemColors.Control;
            }

            //bIsOn = GbVar.Seq.sCleaning[0].Info.IsStrip();
            //if (bIsOn && a1PCleanT1.GradientEndColor != Color.LawnGreen)
            //{
            //    a1PCleanT1.GradientEndColor = Color.LawnGreen;
            //    a1PCleanT1.GradientStartColor = Color.LawnGreen;
            //}
            //else if (!bIsOn && a1PCleanT1.GradientEndColor != SystemColors.Control)
            //{
            //    a1PCleanT1.GradientEndColor = SystemColors.Control;
            //    a1PCleanT1.GradientStartColor = SystemColors.Control;
            //}

            //bIsOn = GbVar.Seq.sCleaning[1].Info.IsStrip();
            //if (bIsOn && a1PCleanT2.GradientEndColor != Color.LawnGreen)
            //{
            //    a1PCleanT2.GradientEndColor = Color.LawnGreen;
            //    a1PCleanT2.GradientStartColor = Color.LawnGreen;
            //}
            //else if (!bIsOn && a1PCleanT2.GradientEndColor != SystemColors.Control)
            //{
            //    a1PCleanT2.GradientEndColor = SystemColors.Control;
            //    a1PCleanT2.GradientStartColor = SystemColors.Control;
            //}


            //bIsOn = GbVar.Seq.sCleaning[2].Info.IsStrip();
            //if (bIsOn && a1PCleanPk.GradientEndColor != Color.LawnGreen)
            //{
            //    a1PCleanPk.GradientEndColor = Color.LawnGreen;
            //    a1PCleanPk.GradientStartColor = Color.LawnGreen;
            //}
            //else if (!bIsOn && a1PCleanPk.GradientEndColor != SystemColors.Control)
            //{
            //    a1PCleanPk.GradientEndColor = SystemColors.Control;
            //    a1PCleanPk.GradientStartColor = SystemColors.Control;
            //}

            bIsOn = GbVar.Seq.sUnitDry.Info.IsStrip();
            if (bIsOn && a1pDryBlock.GradientEndColor != Color.LawnGreen)
            {
                a1pDryBlock.GradientEndColor = Color.LawnGreen;
                a1pDryBlock.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pDryBlock.GradientEndColor != SystemColors.Control)
            {
                a1pDryBlock.GradientEndColor = SystemColors.Control;
                a1pDryBlock.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapTransfer.Info.IsStrip();
            if (bIsOn && a1pMapPk.GradientEndColor != Color.LawnGreen)
            {
                a1pMapPk.GradientEndColor = Color.LawnGreen;
                a1pMapPk.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pMapPk.GradientEndColor != SystemColors.Control)
            {
                a1pMapPk.GradientEndColor = SystemColors.Control;
                a1pMapPk.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
            if (bIsOn && a1pMapTable1.GradientEndColor != Color.LawnGreen)
            {
                a1pMapTable1.GradientEndColor = Color.LawnGreen;
                a1pMapTable1.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pMapTable1.GradientEndColor != SystemColors.Control)
            {
                a1pMapTable1.GradientEndColor = SystemColors.Control;
                a1pMapTable1.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
            if (bIsOn && a1pMapTable2.GradientEndColor != Color.LawnGreen)
            {
                a1pMapTable2.GradientEndColor = Color.LawnGreen;
                a1pMapTable2.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pMapTable2.GradientEndColor != SystemColors.Control)
            {
                a1pMapTable2.GradientEndColor = SystemColors.Control;
                a1pMapTable2.GradientStartColor = SystemColors.Control;
            }

            bIsOn = GbVar.Seq.sCuttingTable.Info.IsStrip();
            if (bIsOn && a1pChuckT1.GradientEndColor != Color.LawnGreen)
            {
                a1pChuckT1.GradientEndColor = Color.LawnGreen;
                a1pChuckT1.GradientStartColor = Color.LawnGreen;
            }
            else if (!bIsOn && a1pChuckT1.GradientEndColor != SystemColors.Control)
            {
                a1pChuckT1.GradientEndColor = SystemColors.Control;
                a1pChuckT1.GradientStartColor = SystemColors.Control;
            }
        }

        void ShowLabel()
        {
            bool bIsOn = false;
            double dPosition = 0.0;
            string sPosName = "";

#if _AJIN
            dPosition = 0.0;
            sPosName = FormTextLangMgr.FindKey("현위치: ");
            //dPosition = MotionMgr.Inst[(int)SVDF.AXES.MGZ_CLAMP_ELV_Z].GetRealPos();
            //for (int nPosNo = 0; nPosNo < 11; nPosNo++)
            //{
            //    if (Math.Abs(dPosition - TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MGZ_CLAMP_ELV_Z].dPos[nPosNo]) < 30)
            //    {
            //        sPosName += POSDF.GetPosName(POSDF.TEACH_POS_MODE.MGZ_LD_ELEV, nPosNo);
            //        break;
            //    }
            //}
#endif
            if (sPosName != lbLdElvPosName.Text)
            {
                lbLdElvPosName.Text = sPosName;
            }

            #region Tray Elv
            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK] == 1;
            if (bIsOn && lbTrayResidual1.BackColor != Color.OrangeRed) lbTrayResidual1.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayResidual1.BackColor != SystemColors.Control) lbTrayResidual1.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_2_TRAY_ELV_RESIDUAL_QTY_CHECK] == 1;
            if (bIsOn && lbTrayResidual2.BackColor != Color.OrangeRed) lbTrayResidual2.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayResidual2.BackColor != SystemColors.Control) lbTrayResidual2.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_ELV_RESIDUAL_QTY_CHECK] == 1;
            if (bIsOn && lbTrayResidual3.BackColor != Color.OrangeRed) lbTrayResidual3.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayResidual3.BackColor != SystemColors.Control) lbTrayResidual3.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.EMTY_1_TRAY_ELV_RESIDUAL_QTY_CHECK] == 1;
            if (bIsOn && lbTrayResidual4.BackColor != Color.OrangeRed) lbTrayResidual4.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayResidual4.BackColor != SystemColors.Control) lbTrayResidual4.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.EMTY_2_TRAY_ELV_RESIDUAL_QTY_CHECK] == 1;
            if (bIsOn && lbTrayResidual5.BackColor != Color.OrangeRed) lbTrayResidual5.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayResidual5.BackColor != SystemColors.Control) lbTrayResidual5.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_POS_CHECK] == 1;
            if (bIsOn && lbTrayElvPosChk1.BackColor != Color.OrangeRed) lbTrayElvPosChk1.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayElvPosChk1.BackColor != SystemColors.Control) lbTrayElvPosChk1.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_2_TRAY_ELV_POS_CHECK] == 1;
            if (bIsOn && lbTrayElvPosChk2.BackColor != Color.OrangeRed) lbTrayElvPosChk2.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayElvPosChk2.BackColor != SystemColors.Control) lbTrayElvPosChk2.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_ELV_POS_CHECK] == 1;
            if (bIsOn && lbTrayElvPosChk3.BackColor != Color.OrangeRed) lbTrayElvPosChk3.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayElvPosChk3.BackColor != SystemColors.Control) lbTrayElvPosChk3.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.EMTY_1_TRAY_ELV_POS_CHECK] == 1;
            if (bIsOn && lbTrayElvPosChk4.BackColor != Color.OrangeRed) lbTrayElvPosChk4.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayElvPosChk4.BackColor != SystemColors.Control) lbTrayElvPosChk4.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.EMTY_2_TRAY_ELV_POS_CHECK] == 1;
            if (bIsOn && lbTrayElvPosChk5.BackColor != Color.OrangeRed) lbTrayElvPosChk5.BackColor = Color.OrangeRed;
            if (!bIsOn && lbTrayElvPosChk5.BackColor != SystemColors.Control) lbTrayElvPosChk5.BackColor = SystemColors.Control;
            #endregion

            #region Tray Stage
            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1;
            if (bIsOn && lbGoodTray1.BackColor != Color.Lime) lbGoodTray1.BackColor = Color.Lime;
            if (!bIsOn && lbGoodTray1.BackColor != SystemColors.Control) lbGoodTray1.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1;
            if (bIsOn && lbGoodTray2.BackColor != Color.Lime) lbGoodTray2.BackColor = Color.Lime;
            if (!bIsOn && lbGoodTray2.BackColor != SystemColors.Control) lbGoodTray2.BackColor = SystemColors.Control;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1;
            if (bIsOn && lbReworkTray.BackColor != Color.Lime) lbReworkTray.BackColor = Color.Lime;
            if (!bIsOn && lbReworkTray.BackColor != SystemColors.Control) lbReworkTray.BackColor = SystemColors.Control;
            #endregion

            #region Analog Vac
            double dValue = 0.0;
            int nVacNo = 0;
            AIRSTATUS eAirStatus = AIRSTATUS.NONE;
            SeqBase seqBase = new SeqBase();
            Label lblCurrent = null;
            Color colorVacErr = Color.Red;

            nVacNo = (int)IODF.A_INPUT.LD_RAIL_VAC;
            lblCurrent = label17;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.STRIP_RAIL);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.STRIP_PK_VAC;
            lblCurrent = lbStripPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.STRIP_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.UNIT_PK_VAC;
            lblCurrent = lbUnitPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.UNIT_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            //nVacNo = (int)IODF.A_INPUT.SECOND_CLEANING_ZONE_KIT_MATERIAL_VACUUM_SENSOR;
            //dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            //bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            //if (bIsOn && lbCleanT1Vac.BackColor != Color.Blue) lbCleanT1Vac.BackColor = Color.Blue;
            //if (!bIsOn && lbCleanT1Vac.BackColor != Color.FromArgb(45, 45, 45)) lbCleanT1Vac.BackColor = Color.FromArgb(45, 45, 45);
            //lbCleanT1Vac.Text = dValue.ToString("F1");

            //nVacNo = (int)IODF.A_INPUT.SECOND_ULTRASONIC_ZONE_KIT_MATERIAL_VACUUM_SENSOR;
            //dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            //bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            //if (bIsOn && lbCleanT2Vac.BackColor != Color.Blue) lbCleanT2Vac.BackColor = Color.Blue;
            //if (!bIsOn && lbCleanT2Vac.BackColor != Color.FromArgb(45, 45, 45)) lbCleanT2Vac.BackColor = Color.FromArgb(45, 45, 45);
            //lbCleanT2Vac.Text = dValue.ToString("F1");

            //nVacNo = (int)IODF.A_INPUT.CLEAN_PICKER_VACUUM_SENSOR;
            //dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            //bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            //if (bIsOn && lbCleanPkVac.BackColor != Color.Blue) lbCleanPkVac.BackColor = Color.Blue;
            //if (!bIsOn && lbCleanPkVac.BackColor != Color.FromArgb(45, 45, 45)) lbCleanPkVac.BackColor = Color.FromArgb(45, 45, 45);
            //lbCleanPkVac.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC;
            lblCurrent = lbDryBlockVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.UNIT_DRY);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_PK_WORK_VAC;
            lblCurrent = lbMapPickerVac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_TRANSFER);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_STG_1_WORK_VAC;
            lblCurrent = lbMapStage1Vac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_VISION_TABLE_1);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");

            nVacNo = (int)IODF.A_INPUT.MAP_STG_2_WORK_VAC;
            lblCurrent = lbMapStage2Vac;
            eAirStatus = seqBase.AirStatus(STRIP_MDL.MAP_VISION_TABLE_2);
            dValue = (GbVar.GB_AINPUT[nVacNo] - ConfigMgr.Inst.Cfg.Vac[nVacNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nVacNo].dRatio;
            bIsOn = dValue <= ConfigMgr.Inst.Cfg.Vac[nVacNo].dVacLevelLow;
            if (bIsOn && lblCurrent.BackColor != Color.Blue) lblCurrent.BackColor = Color.Blue;
            if (!bIsOn && eAirStatus == AIRSTATUS.VAC && lblCurrent.BackColor != colorVacErr) lblCurrent.BackColor = colorVacErr;
            if (!bIsOn && eAirStatus != AIRSTATUS.VAC && lblCurrent.BackColor != Color.FromArgb(45, 45, 45)) lblCurrent.BackColor = Color.FromArgb(45, 45, 45);
            lblCurrent.Text = dValue.ToString("F1");
            #endregion

            #region Pre-Align
            string strPreData = "";
            strPreData = string.Format("( {0}, {1} )", GbVar.Seq.sStripTransfer.Info.ptPreOffset1.X.ToString("F3"), GbVar.Seq.sStripTransfer.Info.ptPreOffset1.Y.ToString("F3"));
            if (lbPreOffset1.Text != strPreData) lbPreOffset1.Text = strPreData;
            strPreData = string.Format("( {0}, {1} )", GbVar.Seq.sStripTransfer.Info.ptPreOffset2.X.ToString("F3"), GbVar.Seq.sStripTransfer.Info.ptPreOffset2.Y.ToString("F3"));
            if (lbPreOffset2.Text != strPreData) lbPreOffset2.Text = strPreData;
            strPreData = GbVar.Seq.sStripTransfer.Info.dPreAngleOffset.ToString("F2");
            if (lbPreOffsetAngle.Text != strPreData) lbPreOffsetAngle.Text = strPreData;
            #endregion

            #region Ld Elv Clamp
            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3114] == 1;
            if (bIsOn && lbLdElvClamp.BackColor != Color.Lime) lbLdElvClamp.BackColor = Color.Yellow;
            if (!bIsOn && lbLdElvClamp.BackColor != Color.Transparent) lbLdElvClamp.BackColor = Color.Transparent;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3115] == 1;
            if (bIsOn && lbLdElvUnclamp.BackColor != Color.Lime) lbLdElvUnclamp.BackColor = Color.Yellow;
            if (!bIsOn && lbLdElvUnclamp.BackColor != Color.Transparent) lbLdElvUnclamp.BackColor = Color.Transparent;


            // 기존 코드
            //bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.MLZ_AXIS_MZ_CHECK_1_TOP] != 1;
            //if (bIsOn && lbLdElvTop.BackColor != Color.Lime) lbLdElvTop.BackColor = Color.Yellow;
            //if (!bIsOn && lbLdElvTop.BackColor != Color.Transparent) lbLdElvTop.BackColor = Color.Transparent;

            //bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.MLZ_AXIS_MZ_CHECK_1_BTM] != 1;
            //if (bIsOn && lbLdElvBtm.BackColor != Color.Lime) lbLdElvBtm.BackColor = Color.Yellow;
            //if (!bIsOn && lbLdElvBtm.BackColor != Color.Transparent) lbLdElvBtm.BackColor = Color.Transparent;


            // 신규 코드
            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3112] == 1;
            if (bIsOn && lbLdElvTop.BackColor != Color.Lime) lbLdElvTop.BackColor = Color.Yellow;
            if (!bIsOn && lbLdElvTop.BackColor != Color.Transparent) lbLdElvTop.BackColor = Color.Transparent;

            bIsOn = GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3113] == 1;
            if (bIsOn && lbLdElvBtm.BackColor != Color.Lime) lbLdElvBtm.BackColor = Color.Yellow;
            if (!bIsOn && lbLdElvBtm.BackColor != Color.Transparent) lbLdElvBtm.BackColor = Color.Transparent;
            #endregion

            #region Uld Elv
            if (GbVar.Seq.sUldTrayElvGood.Length == 2)
            {
                if (GbVar.Seq.sUldTrayElvGood[0] != null)
                {
                    if (lbTrayElvTrayCnt1.Text != GbVar.Seq.sUldTrayElvGood[0].nTrayInCount.ToString())
                        lbTrayElvTrayCnt1.Text = GbVar.Seq.sUldTrayElvGood[0].nTrayInCount.ToString();
                }

                if (GbVar.Seq.sUldTrayElvGood[1] != null)
                {
                    if (lbTrayElvTrayCnt2.Text != GbVar.Seq.sUldTrayElvGood[1].nTrayInCount.ToString())
                        lbTrayElvTrayCnt2.Text = GbVar.Seq.sUldTrayElvGood[1].nTrayInCount.ToString();
                }
            }

            if (GbVar.Seq.sUldTrayElvRework != null)
            {
                if (lbTrayElvTrayCnt3.Text != GbVar.Seq.sUldTrayElvRework.nTrayInCount.ToString())
                    lbTrayElvTrayCnt3.Text = GbVar.Seq.sUldTrayElvRework.nTrayInCount.ToString();
            }

            if (GbVar.Seq.sUldTrayElvEmpty.Length == 2)
            {
                if (GbVar.Seq.sUldTrayElvEmpty[0] != null)
                {
                    if (lbTrayElvTrayCnt4.Text != GbVar.Seq.sUldTrayElvEmpty[0].nTrayCount.ToString())
                        lbTrayElvTrayCnt4.Text = GbVar.Seq.sUldTrayElvEmpty[0].nTrayCount.ToString();
                }

                if (GbVar.Seq.sUldTrayElvEmpty[1] != null)
                {
                    if (lbTrayElvTrayCnt5.Text != GbVar.Seq.sUldTrayElvEmpty[1].nTrayCount.ToString())
                        lbTrayElvTrayCnt5.Text = GbVar.Seq.sUldTrayElvEmpty[1].nTrayCount.ToString();
                }
            }
            #endregion
        }

        private void listLog_DoubleClick(object sender, EventArgs e)
        {
            listLog.BeginUpdate();
            listLog.Items.Clear();
            listLog.EndUpdate();
        }

        private void btnIonF_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;
            bool bIsOn = false;

            switch (nTag)
            {
                case 0:
                    {
                        // 이온 바 런 아웃풋은 B접 동작
                        bIsOn = btn.BackColor == Color.Lime;
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BAR_RUN_1, bIsOn);
                    }
                    break;
                default:
                    break;
            }
        }

        private void chkAlignTeachStop_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = sender as CheckBox;
            int nTag;
            if (!int.TryParse(chk.Tag.ToString(), out nTag)) return;

            switch (nTag)
            {
                case 0:
                    GbVar.bIsStopAlignTeachPos = chk.Checked;
                    break;
                case 1:
                    GbVar.bIsStop2DTeachPos = chk.Checked;
                    break;
                case 2:
                    GbVar.bIsStopBrushTeachPos = chk.Checked;
                    break;
                case 3:
                    GbVar.bIsStopTopVisionTeachPos = chk.Checked;
                    break;
                case 4:
                    GbVar.bIsStopBtmVisionTeachPos = chk.Checked;
                    break;
                case 5:
                    GbVar.bIsStopTrayVisionTeachPos = chk.Checked;
                    break;
                default:
                    break;
            }

            if (chk.Checked && chk.BackColor != Color.Yellow) chk.BackColor = Color.Yellow;
            else if (!chk.Checked && chk.BackColor != Color.Transparent) chk.BackColor = Color.Transparent;

        }
        private void tmrIfRefresh_Tick(object sender, EventArgs e)
        {
            #region Vision 

            #region Input
            if (GbVar.GB_INPUT[(int)IODF.INPUT.PRE_INSP_COMP] == 1)
            {
                if (lbPreInspComp_Vision.BackColor != Color.Chartreuse)
                    lbPreInspComp_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbPreInspComp_Vision.BackColor != SystemColors.Control)
                    lbPreInspComp_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_INSP_COMP] == 1)
            {
                if (lbMapInspComp_Vision.BackColor != Color.Chartreuse)
                    lbMapInspComp_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbMapInspComp_Vision.BackColor != SystemColors.Control)
                    lbMapInspComp_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.BALL_INSP_COMP] == 1)
            {
                if (lbBallInspComp_Vision.BackColor != Color.Chartreuse)
                    lbBallInspComp_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbBallInspComp_Vision.BackColor != SystemColors.Control)
                    lbBallInspComp_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_CNT_RESET] == 1)
            {
                if (lbMapCntRstRep_Vision.BackColor != Color.Chartreuse)
                    lbMapCntRstRep_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbMapCntRstRep_Vision.BackColor != SystemColors.Control)
                    lbMapCntRstRep_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.BALL_CNT_RESET] == 1)
            {
                if (lbBallCntRstRep_Vision.BackColor != Color.Chartreuse)
                    lbBallCntRstRep_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbBallCntRstRep_Vision.BackColor != SystemColors.Control)
                    lbBallCntRstRep_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.VISION_READY] == 1)
            {
                if (lbReady_Vision.BackColor != Color.Chartreuse)
                    lbReady_Vision.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbReady_Vision.BackColor != SystemColors.Control)
                    lbReady_Vision.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_OUTPUT[(int)IODF.INPUT.PRE_INSP_GRAB_REQ] == 1)
            {
                if (lbPreGrabRep_Sorter.BackColor != Color.Chartreuse)
                    lbPreGrabRep_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbPreGrabRep_Sorter.BackColor != SystemColors.Control)
                    lbPreGrabRep_Sorter.BackColor = SystemColors.Control;
            }

            #endregion

            #region Output
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_STAGE_NO] == 0)
            {
                if (lbMapNo_Sorter.Text.Contains("2"))
                    lbMapNo_Sorter.Text.Replace("2", "1");
            }
            else
            {
                if (lbMapNo_Sorter.Text.Contains("1"))
                    lbMapNo_Sorter.Text.Replace("1", "2");
            }
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_HEAD_NO] == 0)
            {
                if (lbBallNo_Sorter.Text.Contains("2"))
                    lbBallNo_Sorter.Text.Replace("2", "1");
            }
            else
            {
                if (lbBallNo_Sorter.Text.Contains("1"))
                    lbBallNo_Sorter.Text.Replace("1", "2");
            }

            #region Comp
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.PRE_INSP_GRAB_REQ] == 1)
            {
                if (lbPreGrabReq_Sorter.BackColor != Color.Chartreuse)
                    lbPreGrabReq_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbPreGrabReq_Sorter.BackColor != SystemColors.Control)
                    lbPreGrabReq_Sorter.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.PRE_INSP_COMP] == 1)
            {
                if (lbPreInspCompRep_Sorter.BackColor != Color.Chartreuse)
                    lbPreInspCompRep_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbPreInspCompRep_Sorter.BackColor != SystemColors.Control)
                    lbPreInspCompRep_Sorter.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_INSP_COMP] == 1)
            {
                if (lbBallInspCompRep_Sorter.BackColor != Color.Chartreuse)
                    lbBallInspCompRep_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbBallInspCompRep_Sorter.BackColor != SystemColors.Control)
                    lbBallInspCompRep_Sorter.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_INSP_COMP] == 1)
            {
                if (lbMapInspCompRep_Sorter.BackColor != Color.Chartreuse)
                    lbMapInspCompRep_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbMapInspCompRep_Sorter.BackColor != SystemColors.Control)
                    lbMapInspCompRep_Sorter.BackColor = SystemColors.Control;
            }
            #endregion

            #region Reset
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_CNT_RESET] == 1)
            {
                if (lbBallCntRstReq_Sorter.BackColor != Color.Chartreuse)
                    lbBallCntRstReq_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbBallCntRstReq_Sorter.BackColor != SystemColors.Control)
                    lbBallCntRstReq_Sorter.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_CNT_RESET] == 1)
            {
                if (lbMapCntRstReq_Sorter.BackColor != Color.Chartreuse)
                    lbMapCntRstReq_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbMapCntRstReq_Sorter.BackColor != SystemColors.Control)
                    lbMapCntRstReq_Sorter.BackColor = SystemColors.Control;
            }
            #endregion

            #endregion

            #endregion

            #region Saw Strip
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_CUTTING] == 1)
            {
                if (lbCutting_Saw.BackColor != Color.Chartreuse)
                    lbCutting_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbCutting_Saw.BackColor != SystemColors.Control)
                    lbCutting_Saw.BackColor = SystemColors.Control;
            }

            #region Input
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_LD_REQUISITION_R] == 1)
            {
                if (lbLoadReq_Saw.BackColor != Color.Chartreuse)
                    lbLoadReq_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbLoadReq_Saw.BackColor != SystemColors.Control)
                    lbLoadReq_Saw.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_LD_POS_R] == 1)
            {
                if (lbLoadPos_Saw.BackColor != Color.Chartreuse)
                    lbLoadPos_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbLoadPos_Saw.BackColor != SystemColors.Control)
                    lbLoadPos_Saw.BackColor = SystemColors.Control;
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_STAGE_VAC_ON_R] == 1)
            {
                if (lbVacOn_Saw.BackColor != Color.Chartreuse)
                    lbVacOn_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbVacOn_Saw.BackColor != SystemColors.Control)
                    lbVacOn_Saw.BackColor = SystemColors.Control;
            }
            #endregion

            #region Output
            //TODO SAW간의 INTERFACE신호 연결하기
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_X] == 1)
            {
                if (lbXPlacePos_Sorter.BackColor != Color.Chartreuse)
                    lbXPlacePos_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbXPlacePos_Sorter.BackColor != SystemColors.Control)
                    lbXPlacePos_Sorter.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_Z] == 1)
            {
                if (lbZPlacePos_Sorter.BackColor != Color.Chartreuse)
                    lbZPlacePos_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbZPlacePos_Sorter.BackColor != SystemColors.Control)
                    lbZPlacePos_Sorter.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_LD_OK_R] == 1)
            {
                if (lbUploadComp_Sorter.BackColor != Color.Chartreuse)
                    lbUploadComp_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbUploadComp_Sorter.BackColor != SystemColors.Control)
                    lbUploadComp_Sorter.BackColor = SystemColors.Control;
            }
            #endregion

            #endregion

            #region Saw Unit

            #region Input
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_ULD_REQUISITION_R] == 1)
            {
                if (lbUnloadReq_Saw.BackColor != Color.Chartreuse)
                    lbUnloadReq_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbUnloadReq_Saw.BackColor != SystemColors.Control)
                    lbUnloadReq_Saw.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_ULD_POS_R] == 1)
            {
                if (lbUnloadPos_Saw.BackColor != Color.Chartreuse)
                    lbUnloadPos_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbUnloadPos_Saw.BackColor != SystemColors.Control)
                    lbUnloadPos_Saw.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SAW_STG_BLOW_ON_R] == 1)
            {
                if (lbBlowOn_Saw.BackColor != Color.Chartreuse)
                    lbBlowOn_Saw.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbBlowOn_Saw.BackColor != SystemColors.Control)
                    lbBlowOn_Saw.BackColor = SystemColors.Control;
            }
            #endregion

            #region Output
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_X] == 1)
            {
                if (lbXPickPos_Sorter.BackColor != Color.Chartreuse)
                    lbXPickPos_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbXPickPos_Sorter.BackColor != SystemColors.Control)
                    lbXPickPos_Sorter.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_Z] == 1)
            {
                if (lbZPickPos_Sorter.BackColor != Color.Chartreuse)
                    lbZPickPos_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbZPickPos_Sorter.BackColor != SystemColors.Control)
                    lbZPickPos_Sorter.BackColor = SystemColors.Control;
            }
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_ULD_OK_R] == 1)
            {
                if (lbLoadComp_Sorter.BackColor != Color.Chartreuse)
                    lbLoadComp_Sorter.BackColor = Color.Chartreuse;
            }
            else
            {
                if (lbLoadComp_Sorter.BackColor != SystemColors.Control)
                    lbLoadComp_Sorter.BackColor = SystemColors.Control;
            }
            #endregion

            #endregion
        }

        private void btnDryReady_Click(object sender, EventArgs e)
        {
            //GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_UNLOADING_READY] = true;
        }

        private void btnMapTableReady1_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox("설비 시퀀스를 초기화 하시겠습니까?", "SEQ INIT");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;


            GbVar.Seq.Init();
            GbSeq.autoRun.AllInitSeq();
        }

        private void btnMapTableReady2_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox("설비 데이터를를 초기화 하시겠습니까?", "DATA INIT");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[0].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[1].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[2].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[3].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[4].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[5].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[6].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[7].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[8].IS_UNIT = false;
            GbVar.Seq.sPkgPickNPlace.pInfo[0].unitPickUp[9].IS_UNIT = false;

            GbVar.Seq.sMapVisionTable[0].nMapTablePickUpCount = 0;
            GbVar.Seq.sMapVisionTable[0].Info.Clear();

            GbVar.Seq.sMapVisionTable[1].nMapTablePickUpCount = 0;
            GbVar.Seq.sMapVisionTable[1].Info.Clear();

            GbVar.Seq.sUldRWTrayTable.Info.Clear();
            GbVar.Seq.sUldRWTrayTable.nUnitPlaceCount = 0;

            GbVar.Seq.sUldGDTrayTable[0].Info.Clear();
            GbVar.Seq.sUldGDTrayTable[1].Info.Clear();

            GbVar.Seq.sUldGDTrayTable[0].nUnitPlaceCount = 0;
            GbVar.Seq.sUldGDTrayTable[1].nUnitPlaceCount = 0;

            GbVar.Seq.sUldRjtBoxTable.Clear();


            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT = 0;//"스트립 투입 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT = 0;//"스트립 배출 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_CUTTING_COUNT = 0;//"스트립 컷팅 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PICKUP_COUNT = 0;//"멀티 피커[1] 픽업 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_2_PICKUP_COUNT = 0;//"멀티 피커[2] 픽업 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PLACE_COUNT = 0;//"멀티 피커[1] 안착 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_2_PLACE_COUNT = 0;//"멀티 피커[2] 안착 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT = 0;//"GOOD 트레이 테이블[1] 트레이 작업수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_2_TRAY_WORK_COUNT = 0;//"GOOD 트레이 테이블[2] 트레이 작업수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].REWORK_TABLE_TRAY_WORK_COUNT = 0;//"REWORK 트레이 테이블 트레이 작업수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_OK_COUNT = 0;//"마크 비전 양품 검출 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MARK_VISION_RW_COUNT = 0;//"마크 비전 재생산 검출 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_OK_COUNT = 0;//"볼 비전 양품 검출 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].BALL_VISION_NG_COUNT = 0;//"볼 비전 불량 검출 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT = 0;//"총 생산량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_OK_COUNT = 0;//"양품 수량";
            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_NG_COUNT = 0;//"불량 수량";

        }

        private void btnGoodTableReady1_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;

            //GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
            //GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE] = true;
        }

        private void btnGoodTableReady2_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;

            //GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = false;
            //GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE] = true;
        }

        private void btnReworkTableReady_Click(object sender, EventArgs e)
        {
            ////IFMgr.Inst.VISION.UpdateLotStart("AAA", "OO", "BBB");
            //IFMgr.Inst.VISION.UpdateLotEnd();
            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.VISION_COMM_SIG_15, true);

            //while (true)
            //{
            //    if (MotionMgr.Inst.GetInput(IODF.INPUT.VISION_COMM_SIG_15))
            //    {
            //        break;
            //    }
            //}

            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.VISION_COMM_SIG_15, false);

            //while (true)
            //{
            //    if (!MotionMgr.Inst.GetInput(IODF.INPUT.VISION_COMM_SIG_15))
            //    {
            //        break;
            //    }
            //}
            //GbVar.Seq.sUldTrayElvGood.bSeqIfVar[seqUldTrayElvGood.OHT_TRAY_OUT_REQ] = true;
        }

        private void btnSeqTest1_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox("5층에 매거진을 투입하셨습니까?", "SEQ INIT");
            msg.TopMost = true;
            if (msg.ShowDialog(this) != DialogResult.OK) return;

            // 매거진을 5층에 투입 후 눌러주세요
            //GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.OHT_MGZ_LOADING_COMPLETED] = true;
        }

        private void btnPreAlignTeachStop_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bAlignTeachStop[0] = !GbVar.Seq.bAlignTeachStop[0];
        }

        private void btnUnitCleanTeachStop_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bCleanTeachStop[0] = !GbVar.Seq.bCleanTeachStop[0];
        }

        private void btnTopVisionTeachStop_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bTopVisionTeachStop[0] = !GbVar.Seq.bTopVisionTeachStop[0];
        }

        private void btnBtmVisionTeachStop_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bBtmVisionTeachStop[0] = !GbVar.Seq.bBtmVisionTeachStop[0];
        }

        private void btnTrayVisionTeachStop_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bTrayVisionTeachStop[0] = !GbVar.Seq.bTrayVisionTeachStop[0];
        }

        public void AddLogSaw(string strLog)
        {

            try
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    lsvSawIfLog.BeginUpdate();

                    lsvSawIfLog.View = View.Details;
                    ListViewItem lvi = new ListViewItem(DateTime.Now.ToString("HH:mm:ss:fff"));
                    lvi.SubItems.Add(strLog);
                    lsvSawIfLog.Items.Add(lvi);
                    lsvSawIfLog.Items[lsvSawIfLog.Items.Count - 1].EnsureVisible();
                    if (lsvSawIfLog.Items.Count > 1000) lsvSawIfLog.Items.Clear();
                    lsvSawIfLog.EndUpdate();
                });
            }
            catch (Exception ex)
            {
            }
        }

        public void AddLogVision(string strLog)
        {

            try
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    lsvVisionLog.BeginUpdate();

                    lsvVisionLog.View = View.Details;
                    ListViewItem lvi = new ListViewItem(DateTime.Now.ToString("HH:mm:ss:fff"));
                    lvi.SubItems.Add(strLog);
                    lsvVisionLog.Items.Add(lvi);
                    lsvVisionLog.Items[lsvVisionLog.Items.Count - 1].EnsureVisible();
                    if (lsvVisionLog.Items.Count > 1000) lsvVisionLog.Items.Clear();
                    lsvVisionLog.EndUpdate();
                });
            }
            catch (Exception ex)
            {
            }
        }

        public void AddLogMes(string strLog)
        {

            try
            {
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public void LotStart(string strLotId)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        lsvLotRecord.BeginUpdate();
                        lsvLotRecord.View = View.Details;

                        ListViewItem lsv = lsvLotRecord.FindItemWithText(strLotId);
                        if (lsv == null)
                        {
                            ListViewItem lvi = new ListViewItem(strLotId);
                            lvi.SubItems.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            lvi.SubItems.Add("");
                            lsvLotRecord.Items.Add(lvi);
                            lsvLotRecord.Items[lsvLotRecord.Items.Count - 1].EnsureVisible();
                            if (lsvLotRecord.Items.Count > 1000) lsvLotRecord.Items.Clear();
                        }
                        else
                        {
                            int nIndex = lsv.Index;
                            if (nIndex < 0) return;

                            lsvLotRecord.BeginUpdate();
                            lsvLotRecord.Items[nIndex].SubItems[1].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            lsvLotRecord.EndUpdate();
                        }

                        lsvLotRecord.EndUpdate();
                    });
                }
                else
                {
                    lsvLotRecord.BeginUpdate();
                    lsvLotRecord.View = View.Details;

                    ListViewItem lsv = lsvLotRecord.FindItemWithText(strLotId);
                    if (lsv == null)
                    {
                        ListViewItem lvi = new ListViewItem(strLotId);
                        lvi.SubItems.Add(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        lvi.SubItems.Add("");
                        lsvLotRecord.Items.Add(lvi);
                        lsvLotRecord.Items[lsvLotRecord.Items.Count - 1].EnsureVisible();
                        if (lsvLotRecord.Items.Count > 1000) lsvLotRecord.Items.Clear();
                    }
                    else
                    {
                        int nIndex = lsv.Index;
                        if (nIndex < 0) return;

                        lsvLotRecord.BeginUpdate();
                        lsvLotRecord.Items[nIndex].SubItems[1].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        lsvLotRecord.EndUpdate();
                    }

                    lsvLotRecord.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        public void LotEnd(string strLotId)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        ListViewItem lsv = lsvLotRecord.FindItemWithText(strLotId);
                        if (lsv == null) return;

                        int nIndex = lsv.Index;
                        if (nIndex < 0) return;

                        lsvLotRecord.BeginUpdate();
                        lsvLotRecord.Items[nIndex].SubItems[2].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        lsvLotRecord.EndUpdate();
                    });
                }
                else
                {
                    ListViewItem lsv = lsvLotRecord.FindItemWithText(strLotId);
                    if (lsv == null) return;

                    int nIndex = lsv.Index;
                    if (nIndex < 0) return;

                    lsvLotRecord.BeginUpdate();
                    lsvLotRecord.Items[nIndex].SubItems[2].Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    lsvLotRecord.EndUpdate();
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }
        }

        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            UpdateTactTimeDgvLabel();


        }

        private void UpdateTactTimeDgvLabel()
        {
            dgvTactTime.Rows.Clear();

            for (int i = 0; i < (int)TTDF.CYCLE_NAME.MAX; i++)
            {
                dgvTactTime.Rows.Add((TTDF.CYCLE_NAME)i, TTDF.strTactTime[i]);
            }
        }


        // 0422 KTH 색상 더블클릭시 색 변경 저장 팝업 등장(goodColor 추가가능)
        private void label_InvaildColor_DoubleClick(object sender, EventArgs e)
        {
            popColorChange colorChange = new popColorChange(5);
            if (colorChange.ShowDialog() == DialogResult.OK)
            {
                label_InvaildColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_InvaildColor);
            }
        }

        private void label_EmptyColor_DoubleClick(object sender, EventArgs e)
        {
            popColorChange colorChange = new popColorChange(3);
            if (colorChange.ShowDialog() == DialogResult.OK)
            {
                label_EmptyColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_UnitEmptyColor);
            }
        }

        private void label_NgColor_DoubleClick(object sender, EventArgs e)
        {
            popColorChange colorChange = new popColorChange(2);
            if (colorChange.ShowDialog() == DialogResult.OK)
            {
                label_NgColor.BackColor = Color.FromName(GbVar.CurUserColorInfo.USER_VisionNgColor);
            }

        }
        private void btnMgzUnload_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = 0;
            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            switch (nTag)
            {
                case 0:
                    {
                        // 로드 우측 컨베이어
                        // 이미 해당 시퀀스가 동작 중이라면 동작 하지 않기 위함
                        if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] == false
                            && GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] == false)
                        {
                            // ::: 여기에 스퀀스 동작 연결 하면 됨 :::
                            GbVar.Seq.sLdMzLoading.bLdConvMgzUnloadPushSW = true;

                            GbSeq.autoRun.ResetCmd();
                            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
                            GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
                        }
                    }
                    break;
                case 1:
                    {
                        // 로드 좌측 컨베이어
                    }
                    break;
                case 2:
                    {
                        // 언로드 우측 컨베이어
                    }
                    break;
                case 3:
                    {
                        // 언로드 좌측 컨베이어

                    }
                    break;
            }

        }

        private void btnWorkMgzUnload_Click(object sender, EventArgs e)
        {
            GbVar.Seq.sLdMzLoading.bMgzUnloadReq = !GbVar.Seq.sLdMzLoading.bMgzUnloadReq;

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "작업 매거진 배출");
        }
        private void TrayInClick(object sender, EventArgs e)
        {
            //Button btn = sender as Button;

            //int nTag = 0;

            //if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            //GbVar.bTrayInReq[MCDF.ELV_TRAY_GOOD_1 +  nTag] = true;

            Button btn = sender as Button;
            int nTag = 0;

            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            int nAxisNo = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + nTag;

            double dTargetPos = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dPos[POSDF.TRAY_ELEV_READY];
            double dSpeed = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dVel[POSDF.TRAY_ELEV_READY];
            double dAcc = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dAcc[POSDF.TRAY_ELEV_READY];
            double dDec = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dDec[POSDF.TRAY_ELEV_READY];

            string strTitle = "트레이 투입";
            string[] strElvName = new string[2] { "EMPTY-1", "EMPTY-2" };
            string strMsg = string.Format("{0} TRAY를 투입하시겠습니까?\n 이동위치:{1} 속도:{2} 가속도:{3} 감속도:{4} ",
                strElvName[nTag], dTargetPos.ToString("F3"), dSpeed.ToString("0.0"), dAcc.ToString("0.0"), dDec.ToString("0.0"));
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            GbVar.Seq.sUldTrayElvEmpty[nTag].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

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

            // guide cylinder open
            // Elv Door Unlock
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_1_ELV_STOPPER_FWD + (2 * nTag), false);
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_1_ELV_STOPPER_BWD + (2 * nTag), true);

            strTitle = "엘리베이터 초기화";
            strMsg = string.Format("트레이 투입을 하셨습니까?");

            pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (GbVar.mcState.IsRun())
                {
                    GbVar.Seq.sUldTrayElvEmpty[nTag].Init();
                    GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.ULD_ELV_EMPTY_1 + nTag);
                }
                else
                {
                    GbSeq.manualRun.SetManualSorterCycleRun(830 + (10 * nTag), 3 + nTag);
                    GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
                    GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                    ShowDlgPopManualRunBlock();
                }
            }

        }


        private void TrayOutClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            int nTag = 0;

            if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;
            if (GbVar.mcState.IsAllRun())
            {
                GbVar.bTrayOutReq[MCDF.ELV_TRAY_GOOD_1 + nTag] = true;

                GbFunc.WriteEventLog(this.GetType().Name.ToString(), nTag + 1 + "번 트레이 배출");
            }
            else
            {
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + btn.Text + " ]?", "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
                if (msg.ShowDialog(this) != DialogResult.OK) return;
                int nSeqNo = 900;

                nSeqNo += nTag * 10;

                GbSeq.manualRun.SetManualSorterCycleRun(nSeqNo, nTag);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }

            // MOVE

            //Button btn = sender as Button;
            //int nTag = 0;

            //if (!int.TryParse(btn.Tag.ToString(), out nTag)) return;

            //int nAxisNo = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nTag;

            //double dTargetPos = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dPos[POSDF.TRAY_ELEV_READY];
            //double dSpeed = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dVel[POSDF.TRAY_ELEV_READY];
            //double dAcc = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dAcc[POSDF.TRAY_ELEV_READY];
            //double dDec = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dDec[POSDF.TRAY_ELEV_READY];

            //string strTitle = "트레이 배출";
            //string[] strElvName = new string[3] { "GOOD-1", "GOOD-2", "REWORK" };
            //string strMsg = string.Format("{0} TRAY를 배출하시겠습니까?\n 이동위치:{1} 속도:{2} 가속도:{3} 감속도:{4} ",
            //    strElvName[nTag], dTargetPos.ToString("F3"), dSpeed.ToString("0.0"), dAcc.ToString("0.0"), dDec.ToString("0.0"));
            //popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            //if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            //if (nTag > 1)
            //{
            //    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = false;
            //}
            //else
            //{
            //    GbVar.Seq.sUldTrayElvGood[nTag].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;
            //}

            //int[] m_nAxisArray = null;
            //double[] m_dPosArray = null;
            //uint[] m_nOrderArray = null;
            //double[] m_dSpeedArray = null;
            //double[] m_dAccArray = null;
            //double[] m_dDecArray = null;

            //m_nAxisArray = new int[] { nAxisNo };
            //m_dPosArray = new double[] { dTargetPos };
            //m_nOrderArray = new uint[] { 0 };
            //m_dSpeedArray = new double[] { dSpeed };
            //m_dAccArray = new double[] { dAcc };
            //m_dDecArray = new double[] { dDec };

            //GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            //GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            //GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            //ShowDlgPopManualRunBlock();

            //// guide cylinder open
            //// Elv Door Unlock
            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.RW_ELV_STOPPER_FWD + (2 * nTag), false);
            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.RW_ELV_STOPPER_BWD + (2 * nTag), true);

            //strTitle = "엘리베이터 초기화";
            //strMsg = string.Format("트레이 배출을 완료하였습니까?");

            //pMsg = new popMessageBox(strMsg, strTitle);
            //if (pMsg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //}

            //if (nTag > 1)
            //{
            //    GbVar.Seq.sUldTrayElvRework.Init();
            //    GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.ULD_ELV_REWORK);
            //}
            //else
            //{
            //    GbVar.Seq.sUldTrayElvGood[nTag].Init();
            //    GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.ULD_ELV_GOOD_1 + nTag);
            //}
        }

        private void LotRegClick(object sender, EventArgs e)
        {

        }

        private void btnOneStripLoad_Click(object sender, EventArgs e)
        {
            GbVar.Seq.bOneStripLoad = !GbVar.Seq.bOneStripLoad;

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "원 스트립 적재");
        }

        private void btnGoodTrayOut_Click(object sender, EventArgs e)
        {
            if (GbVar.Seq.bGoodTrayOut == false && GbVar.Seq.bIsLotEndRun)
                return;

            GbVar.Seq.bGoodTrayOut = !GbVar.Seq.bGoodTrayOut;

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "양품 트레이 배출");
        }

        private void btnReworkTrayOut_Click(object sender, EventArgs e)
        {
            if (GbVar.Seq.bReworkTrayOut == false && GbVar.Seq.bIsLotEndRun)
                return;

            GbVar.Seq.bReworkTrayOut = !GbVar.Seq.bReworkTrayOut;

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "재생산 트레이 배출");
        }

        private void a1PUnitPk_Copy_Click(object sender, EventArgs e)
        {
            // 다른 유닛들 완성되면 같이적용할 예정
            // 수세기 유닛들은 드라이런 돌며 확인 완료
            //Owf.Controls.A1Panel a1Pnl = sender as Owf.Controls.A1Panel;
            //string strTag = a1Pnl.Tag.ToString();

            //popMessageBox msg = new popMessageBox(string.Format("{0} 스크랩을 진행 하시겠습니까?.", strTag), "QUESTION", MessageBoxButtons.OKCancel);
            //msg.TopMost = true;
            //if (msg.ShowDialog(this) != DialogResult.OK) return;

            //switch (strTag)
            //{
            //    case "UnitPicker":
            //        {
            //            GbFunc.StripScrap(STRIP_MDL.UNIT_TRANSFER);
            //        }
            //        break;
            //    case "SecondClean":
            //        {
            //            GbFunc.StripScrap(STRIP_MDL.SECOND_CLEAN_ZONE);
            //        }
            //        break;
            //    case "SecondUltra":
            //        {
            //            GbFunc.StripScrap(STRIP_MDL.SECOND_ULTRA_ZONE);
            //        }
            //        break;
            //    case "CleanerPicker":
            //        {
            //            GbFunc.StripScrap(STRIP_MDL.CLEANER_PICKER);
            //        }
            //        break;
            //    default:
            //        break;
            //}
        }

        private void btnChangeClean1_Click(object sender, EventArgs e)
        {
            Glass.GlassButton btn = (Glass.GlassButton)sender;

            int nTag = btn.GetTag();

            //tabCleaningInfo.SelectedIndex = nTag == 0 ? 1: 0;
        }

        private void frmAuto_VisibleChanged(object sender, EventArgs e)
        {
            m_ucScrapInfo.VisibleChange(this.Visible);
            UpdateLotProcInfoGridData();

            if (!this.Visible)
                return;

            if (CurLanguage != ConfigMgr.Inst.Cfg.General.nLanguage)
            {
                CurLanguage = ConfigMgr.Inst.Cfg.General.nLanguage;

                gridLotProcInfo.Columns[0].HeaderText = FormTextLangMgr.FindKey("Item");
                gridLotProcInfo.Columns[1].HeaderText = FormTextLangMgr.FindKey("Value");

                lsvLotRecord.Columns[0].Text = FormTextLangMgr.FindKey("Lot Id");
                lsvLotRecord.Columns[1].Text = FormTextLangMgr.FindKey("Move In");
                lsvLotRecord.Columns[2].Text = FormTextLangMgr.FindKey("Move Out");

                lsvSawIfLog.Columns[0].Text = FormTextLangMgr.FindKey("Time");
                lsvSawIfLog.Columns[1].Text = FormTextLangMgr.FindKey("Message");

                lsvVisionLog.Columns[0].Text = FormTextLangMgr.FindKey("Time");
                lsvVisionLog.Columns[1].Text = FormTextLangMgr.FindKey("Message");
            }
        }

        /// <summary>
        /// 트레이 메뉴얼, 자동 배출 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnEmptyElvOut_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;

            int nTag = 0;
            string strTag = btn.Tag.ToString();

            if (!int.TryParse(strTag, out nTag)) nTag = 0;

            if (GbVar.mcState.IsAllRun())
            {
                if (nTag == MCDF.ELV_TRAY_EMPTY_1)
                {
                    GbVar.Seq.bEmptyElvOut[0] = !GbVar.Seq.bEmptyElvOut[0];

                    GbFunc.WriteEventLog(this.GetType().Name.ToString(), "빈 트레이 1 배출");
                }
                else //MCDF.ELV_TRAY_EMPTY_2
                {
                    GbVar.Seq.bEmptyElvOut[1] = !GbVar.Seq.bEmptyElvOut[1];

                    GbFunc.WriteEventLog(this.GetType().Name.ToString(), "빈 트레이 2 배출");
                }
            }
            else
            {
                popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure to run [") + FormTextLangMgr.FindKey("All Empty Elevetor tray Out]?"), "Cycle Confirm Ask Message", MessageBoxButtons.OKCancel);
                if (msg.ShowDialog(this) != DialogResult.OK) return;

                GbSeq.manualRun.SetManualSorterCycleRun(900 + (10 * nTag), nTag);
                GbSeq.manualRun.SetRunModule(MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN);
                GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

                ShowDlgPopManualRunBlock();
            }
        }

        protected void ShowDlgPopManualRunBlock()
        {
            if (m_popManualRun == null)
            {
                m_popManualRun = new popManualRun();
            }
            else
            {
                if (m_popManualRun.Visible == true) return;
                m_popManualRun.Dispose();
                m_popManualRun = new popManualRun();
            }
            m_popManualRun.FormClosed += popManual_FormClosed;

            if (m_popManualRun.ShowDialog(Application.OpenForms[0]) == System.Windows.Forms.DialogResult.Cancel)
                GbSeq.manualRun.LeaveCycle(-1);
        }

        protected void OnFinishSeq(int nResult)
        {
            //System.Diagnostics.Debug.WriteLine("Finished");

            if (this.InvokeRequired)
            {
                BeginInvoke(new CommonEvent.FinishSeqEvent(OnFinishSeq), nResult);
                return;
            }

            if (m_popManualRun != null && !m_popManualRun.IsDisposed)
            {
                if (nResult == 0) m_popManualRun.DialogResult = System.Windows.Forms.DialogResult.OK;
                m_popManualRun.Close();
            }
        }

        protected void popManual_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_popManualRun.FormClosed -= popManual_FormClosed;
            if (m_popManualRun != null)
            {
                m_popManualRun.Dispose();
            }
            m_popManualRun = null;
        }

        private void btnCntReset_Click(object sender, EventArgs e)
        {
            //StringBuilder str_date = new StringBuilder();
            //str_date.Append(DateTime.Now.ToString("yyyy-MM-dd")+"  ");
            //str_date.Append(DateTime.Now.ToString("HH:mm:ss"));
            //GbVar.CurTrayCountInfo.Save_DATE = str_date.ToString();
            //GbVar.CurTrayCountInfo.GOOD1_TrayCount = lbTrayElvTrayCnt1.Text.ToString();
            //GbVar.CurTrayCountInfo.GOOD2_TrayCount = lbTrayElvTrayCnt2.Text.ToString();
            //GbVar.CurTrayCountInfo.REWORK_TrayCount = lbTrayElvTrayCnt3.Text.ToString();
            //GbVar.CurTrayCountInfo.EMPTY1_TrayCount = lbTrayElvTrayCnt4.Text.ToString();
            //GbVar.CurTrayCountInfo.EMPTY2_TrayCount = lbTrayElvTrayCnt5.Text.ToString();
            ////SaveTraycountInfo();
            GbVar.Seq.TrayInit();

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), "트레이 카운트 리셋");
        }

        public void SaveTraycountInfo()
        {

            popMessageBox msg;

            if (!Serialize(PathMgr.Inst.PATH_PRODUCT_TRAY_COUNTINFO, GbVar.CurTrayCountInfo))
            {
                msg = new popMessageBox(FormTextLangMgr.FindKey("FAIL TO SAVE!!"), "FAIL SAVE");
                msg.ShowDialog();
                // 실패
            }
            else
            {
                //LoadAccountData();
                msg = new popMessageBox(FormTextLangMgr.FindKey("SAVE COMPLETE"), "COMPLETE SAVE");
                msg.ShowDialog();
            }
        }
        protected TrayCountInfo Deserialize(string path)
        {
            TrayCountInfo obj = null;

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrayCountInfo));
                using (StreamReader rd = new StreamReader(path))
                {
                    obj = (TrayCountInfo)xmlSerializer.Deserialize(rd);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return null;
            }

            return obj;
        }
        protected bool Serialize(string path, TrayCountInfo obj)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TrayCountInfo));
                using (StreamWriter wr = new StreamWriter(path))
                {
                    xmlSerializer.Serialize(wr, obj);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }

        private void btnLoadTray_Click(object sender, EventArgs e)
        {
            popMessageBox msg;
            TrayCountInfo obj = Deserialize(PathMgr.Inst.PATH_PRODUCT_TRAY_COUNTINFO);
            if (obj == null)
            {
                msg = new popMessageBox("FAIL TO Load!!", "FAIL LOAD");
                msg.ShowDialog();
                return;
            }
            msg = new popMessageBox(obj.Save_DATE + "날짜의 트레이 값을 불러왔습니다.", "COMPLETE LOAD");
            msg.ShowDialog();
            GbVar.Seq.sUldTrayElvGood[0].nTrayInCount = int.Parse(obj.GOOD1_TrayCount);
            GbVar.Seq.sUldTrayElvGood[1].nTrayInCount = int.Parse(obj.GOOD2_TrayCount);
            GbVar.Seq.sUldTrayElvRework.nTrayInCount = int.Parse(obj.REWORK_TrayCount);
            GbVar.Seq.sUldTrayElvEmpty[0].nTrayCount = int.Parse(obj.EMPTY1_TrayCount);
            GbVar.Seq.sUldTrayElvEmpty[1].nTrayCount = int.Parse(obj.EMPTY2_TrayCount);
        }

        private void tabCleaningInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Button btn = sender as Button;

            btn.Enabled = !btn.Enabled;
        }

        private void btnLightOn_Click(object sender, EventArgs e)
        {
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.FLUORESCENT_LIGHT_ONOFF, GbVar.IO[IODF.OUTPUT.FLUORESCENT_LIGHT_ONOFF] == 0);
        }

        private void glassButton1_Click(object sender, EventArgs e)
        {
            GbVar.bDryBlockAlways = !GbVar.bDryBlockAlways;
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, GbVar.bDryBlockAlways);
            if (GbVar.bDryBlockAlways)
            {
                MessageBox.Show("DRY BLOCK ALWAYS ON");
            }
            else
            {
                MessageBox.Show("DRY BLOCK BLOW OFF");
            }
        }

        private void btnLotInfoReg_Click(object sender, EventArgs e)
        {
            if (pLotInfo == null)
            {
                pLotInfo = new popLOTInfo();
            }
            else
            {
                if (pLotInfo.Visible == true) return;
                pLotInfo.Dispose();
                pLotInfo = new popLOTInfo();
            }

            DialogResult dr = pLotInfo.ShowDialog();
            if (dr == DialogResult.OK)
            {
                lbLotInputTime.Text = GbVar.product.dateTimeLotInputTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        private void label197_Click(object sender, EventArgs e)
        {
            string strSpindle1_1Ch = "";
            string strSpindle2_1Ch = "";
            string strSpindle1_2Ch = "";
            string strSpindle2_2Ch = "";
            string strStageSpeed_1Ch = "";
            string strStageSpeed_2Ch = "";
            if (GbVar.g_dictSawSVID.Count > 0)
            {
                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_1CH_1, out strSpindle1_1Ch);
                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_2CH_1, out strSpindle2_1Ch);
                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_1CH_2, out strSpindle1_2Ch);
                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_2CH_2, out strSpindle2_2Ch);
                GbVar.g_dictSawSVID.TryGetValue(GbVar.STAGE_SPEED_CH1, out strStageSpeed_1Ch);
                GbVar.g_dictSawSVID.TryGetValue(GbVar.STAGE_SPEED_CH2, out strStageSpeed_2Ch);
            }
            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH1 = strSpindle1_1Ch;
            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH1 = strSpindle2_1Ch;
            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH2 = strSpindle1_2Ch;
            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH2 = strSpindle2_2Ch;
            GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH1 = strStageSpeed_1Ch;
            GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH2 = strStageSpeed_2Ch;

            MessageBox.Show(string.Format("RPM1_CH1 : {0},RPM2_CH1 : {1},STAGE_SPEED_CH1 : {2}\n RPM1_CH2 : {3},RPM2_CH2 : {4},STAGE_SPEED_CH2 : {5}",
                GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH1, GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH1, GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH1,
                GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH2, GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH2, GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH2));
        }

        private void label50_Click(object sender, EventArgs e)
        {

        }

        bool m_LockFlag = false;
        private void tmrSeqStatus_Tick(object sender, EventArgs e)
        {
            if (m_LockFlag == true) return;
            m_LockFlag = true;

            #region SeqRun
            for (int i = 0; i < m_SeqCurNo.Length; i++)
            {
                //런 중인 경우 시퀀스 번호 레이블 색 변함
                if (GbVar.mcState.isCycleRun[m_nCheckSeqNo[i]] == true)
                {
                    if (m_SeqCurNo[i].BackColor != Color.Chartreuse)
                        m_SeqCurNo[i].BackColor = Color.Chartreuse;
                }
                else
                {
                    if (m_SeqCurNo[i].BackColor != Color.White)
                        m_SeqCurNo[i].BackColor = Color.White;
                }
            }
            #endregion

            #region SeqNo
            if (m_SeqCurNo[0].Text != GbVar.Seq.sLoaderLdConv.nCurrentSeqNo.ToString())
                m_SeqCurNo[0].Text = GbVar.Seq.sLoaderLdConv.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[1].Text != GbVar.Seq.sLoaderUldConv.nCurrentSeqNo.ToString())
                m_SeqCurNo[1].Text = GbVar.Seq.sLoaderUldConv.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[2].Text != GbVar.Seq.sLdMzLoading.nCurrentSeqNo.ToString())
                m_SeqCurNo[2].Text = GbVar.Seq.sLdMzLoading.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[3].Text != GbVar.Seq.sStripTransfer.nCurrentSeqNo.ToString())
                m_SeqCurNo[3].Text = GbVar.Seq.sStripTransfer.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[4].Text != GbVar.Seq.sUnitTransfer.nCurrentSeqNo.ToString())
                m_SeqCurNo[4].Text = GbVar.Seq.sUnitTransfer.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[5].Text != GbVar.Seq.sUnitDry.nCurrentSeqNo.ToString())
                m_SeqCurNo[5].Text = GbVar.Seq.sUnitDry.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[6].Text != GbVar.Seq.sMapTransfer.nCurrentSeqNo.ToString())
                m_SeqCurNo[6].Text = GbVar.Seq.sMapTransfer.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[7].Text != GbVar.Seq.sMapVisionTable[0].nCurrentSeqNo.ToString())
                m_SeqCurNo[7].Text = GbVar.Seq.sMapVisionTable[0].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[8].Text != GbVar.Seq.sMapVisionTable[1].nCurrentSeqNo.ToString())
                m_SeqCurNo[8].Text = GbVar.Seq.sMapVisionTable[1].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[9].Text != GbVar.Seq.sPkgPickNPlace.pInfo[0].nCurrentSeqNo.ToString())
                m_SeqCurNo[9].Text = GbVar.Seq.sPkgPickNPlace.pInfo[0].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[10].Text != GbVar.Seq.sPkgPickNPlace.pInfo[1].nCurrentSeqNo.ToString())
                m_SeqCurNo[10].Text = GbVar.Seq.sPkgPickNPlace.pInfo[1].nCurrentSeqNo.ToString();

            if (m_SeqCurNo[11].Text != GbVar.Seq.sUldTrayTransfer.nCurrentSeqNo.ToString())
                m_SeqCurNo[11].Text = GbVar.Seq.sUldTrayTransfer.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[12].Text != GbVar.Seq.sUldGDTrayTable[0].nCurrentSeqNo.ToString())
                m_SeqCurNo[12].Text = GbVar.Seq.sUldGDTrayTable[0].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[13].Text != GbVar.Seq.sUldGDTrayTable[1].nCurrentSeqNo.ToString())
                m_SeqCurNo[13].Text = GbVar.Seq.sUldGDTrayTable[1].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[14].Text != GbVar.Seq.sUldRWTrayTable.nCurrentSeqNo.ToString())
                m_SeqCurNo[14].Text = GbVar.Seq.sUldRWTrayTable.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[15].Text != GbVar.Seq.sUldTrayElvGood[0].nCurrentSeqNo.ToString())
                m_SeqCurNo[15].Text = GbVar.Seq.sUldTrayElvGood[0].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[16].Text != GbVar.Seq.sUldTrayElvGood[1].nCurrentSeqNo.ToString())
                m_SeqCurNo[16].Text = GbVar.Seq.sUldTrayElvGood[1].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[17].Text != GbVar.Seq.sUldTrayElvRework.nCurrentSeqNo.ToString())
                m_SeqCurNo[17].Text = GbVar.Seq.sUldTrayElvRework.nCurrentSeqNo.ToString();
            if (m_SeqCurNo[18].Text != GbVar.Seq.sUldTrayElvEmpty[0].nCurrentSeqNo.ToString())
                m_SeqCurNo[18].Text = GbVar.Seq.sUldTrayElvEmpty[0].nCurrentSeqNo.ToString();
            if (m_SeqCurNo[19].Text != GbVar.Seq.sUldTrayElvEmpty[1].nCurrentSeqNo.ToString())
                m_SeqCurNo[19].Text = GbVar.Seq.sUldTrayElvEmpty[1].nCurrentSeqNo.ToString();
            #endregion

            #region CycNo
                m_SeqCurNo[0].Text = GbSeq.autoRun[SEQ_ID.LD_MZ_LD_CONV].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[1].Text = GbSeq.autoRun[SEQ_ID.LD_MZ_ULD_CONV].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[2].Text = GbSeq.autoRun[SEQ_ID.LD_MZ_ELV_TRANSFER].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[3].Text = GbSeq.autoRun[SEQ_ID.STRIP_TRANSFER].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[4].Text = GbSeq.autoRun[SEQ_ID.UNIT_TRANSFER].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[5].Text = GbSeq.autoRun[SEQ_ID.DRY_UNIT].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[6].Text = GbSeq.autoRun[SEQ_ID.MAP_TRANSFER].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[7].Text = GbSeq.autoRun[SEQ_ID.MAP_VISION_TABLE_1].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[8].Text = GbSeq.autoRun[SEQ_ID.MAP_VISION_TABLE_2].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[9].Text = GbSeq.autoRun[SEQ_ID.PICK_N_PLACE_1].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[10].Text = GbSeq.autoRun[SEQ_ID.PICK_N_PLACE_2].CYCLE_SEQ_NO.ToString();

                m_SeqCurNo[11].Text = GbSeq.autoRun[SEQ_ID.TRAY_TRANSFER].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[12].Text = GbSeq.autoRun[SEQ_ID.UNLOAD_GD1_TRAY_TABLE].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[13].Text = GbSeq.autoRun[SEQ_ID.UNLOAD_GD2_TRAY_TABLE].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[14].Text = GbSeq.autoRun[SEQ_ID.UNLOAD_RW_TRAY_TABLE].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[15].Text = GbSeq.autoRun[SEQ_ID.ULD_ELV_GOOD_1].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[16].Text = GbSeq.autoRun[SEQ_ID.ULD_ELV_GOOD_2].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[17].Text = GbSeq.autoRun[SEQ_ID.ULD_ELV_REWORK].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[18].Text = GbSeq.autoRun[SEQ_ID.ULD_ELV_EMPTY_1].CYCLE_SEQ_NO.ToString();
                m_SeqCurNo[19].Text = GbSeq.autoRun[SEQ_ID.ULD_ELV_EMPTY_2].CYCLE_SEQ_NO.ToString();
            #endregion

            #region SeqBit
            for (int i = 0; i < GbVar.Seq.sLoaderLdConv.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[0].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sLoaderLdConv.bSeqIfVar[i])).ToString())
                    m_SeqBit[0].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sLoaderLdConv.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sLoaderUldConv.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[1].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sLoaderUldConv.bSeqIfVar[i])).ToString())
                    m_SeqBit[1].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sLoaderUldConv.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sLdMzLoading.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[2].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sLdMzLoading.bSeqIfVar[i])).ToString())
                    m_SeqBit[2].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sLdMzLoading.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sStripTransfer.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[3].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sStripTransfer.bSeqIfVar[i])).ToString())
                    m_SeqBit[3].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sStripTransfer.bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUnitTransfer.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[4].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUnitTransfer.bSeqIfVar[i])).ToString())
                    m_SeqBit[4].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUnitTransfer.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sUnitDry.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[5].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUnitDry.bSeqIfVar[i])).ToString())
                    m_SeqBit[5].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUnitDry.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sMapTransfer.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[6].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sMapTransfer.bSeqIfVar[i])).ToString())
                    m_SeqBit[6].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sMapTransfer.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sMapVisionTable[0].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[7].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sMapVisionTable[0].bSeqIfVar[i])).ToString())
                    m_SeqBit[7].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sMapVisionTable[0].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sMapVisionTable[1].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[8].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sMapVisionTable[1].bSeqIfVar[i])).ToString())
                    m_SeqBit[8].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sMapVisionTable[1].bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sPkgPickNPlace.pInfo[0].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[9].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sPkgPickNPlace.pInfo[0].bSeqIfVar[i])).ToString())
                    m_SeqBit[9].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sPkgPickNPlace.pInfo[0].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sPkgPickNPlace.pInfo[1].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[10].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sPkgPickNPlace.pInfo[1].bSeqIfVar[i])).ToString())
                    m_SeqBit[10].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sPkgPickNPlace.pInfo[1].bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sUldTrayTransfer.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[11].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayTransfer.bSeqIfVar[i])).ToString())
                    m_SeqBit[11].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayTransfer.bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[12].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[i])).ToString())
                    m_SeqBit[12].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[13].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[i])).ToString())
                    m_SeqBit[13].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldRWTrayTable.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[14].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldRWTrayTable.bSeqIfVar[i])).ToString())
                    m_SeqBit[14].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldRWTrayTable.bSeqIfVar[i])).ToString();
            }

            for (int i = 0; i < GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[15].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[i])).ToString())
                    m_SeqBit[15].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[16].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[i])).ToString())
                    m_SeqBit[16].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldTrayElvRework.bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[17].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayElvRework.bSeqIfVar[i])).ToString())
                    m_SeqBit[17].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayElvRework.bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[18].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[i])).ToString())
                    m_SeqBit[18].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[i])).ToString();
            }
            for (int i = 0; i < GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar.Length; i++)
            {
                if (m_SeqBit[19].Items[i].SubItems[1].Text != (Convert.ToInt32(GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[i])).ToString())
                    m_SeqBit[19].Items[i].SubItems[1].Text = (Convert.ToInt32(GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[i])).ToString();
            }
            #endregion

            m_LockFlag = false;
        }

        private void btnInputStrip_InletTable_Click(object sender, EventArgs e)
        {
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse
                || GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1)
            {
                popMessageBox msg = new popMessageBox("Do you want to Start with Rail Load && Align?", "RAIL START");
                msg.TopMost = true;

                if (msg.ShowDialog(this) != DialogResult.OK)
                    return;

                GbVar.Seq.sStripRail.Info.bIsStrip = true;
                GbSeq.autoRun[SEQ_ID.STRIP_TRANSFER].NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_LOADING_RAIL_ALIGN);
            }
            else
            {
                popMessageBox msg = new popMessageBox("No Strip Detected in LD Rail", "RAIL START", MessageBoxButtons.OK);
                msg.TopMost = true;

                if (msg.ShowDialog(this) != DialogResult.OK)
                    return;
            }
        }

        private void a1pChuckT1_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("컷팅 테이블에 데이터 정보를 생성하시겠습니까 ?"), "YES");
            msg.TopMost = true;

            if (msg.ShowDialog(this) == DialogResult.OK)
            {
                GbVar.Seq.sCuttingTable.Info.bIsStrip = true;
            }
        }

        private void a1pChuckT1_Paint(object sender, PaintEventArgs e)
        {
            
  
        }
    }
}
