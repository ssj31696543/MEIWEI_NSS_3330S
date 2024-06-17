using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycUldTrayTable: SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;
        int m_nTableNo = 0;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        double m_nMeasureOffset = 0;
        int m_nMeasureCnt = 0;
        int m_nTrayTableAlignRetryCnt = 0;
        int m_nSettingCnt = 0;

        string m_strTableName = "";
        int m_nMeasureTableNo = 0;
        #region PROPERTY
        public seqUldGoodTrayTable SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sUldGDTrayTable[m_nTableNo]; }
        }

        public int MEASURE_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nMeasureCount;
                }

                return m_nMeasureCnt;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.nMeasureCount = value;
                }

                m_nMeasureCnt = value;
            }
        }

        #endregion

        public cycUldTrayTable(int nSeqID, int nTableNo)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nTableNo = nTableNo;
            if (m_nTableNo < MCDF.ULD_TRAY_REWORK)
            {
                m_seqInfo = SEQ_INFO_CURRENT;
            }

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];

            if (m_nTableNo == 0)
            {
                m_strTableName = "Good Tray Stage 1";
            }
            else if(m_nTableNo == 1)
            {
                m_strTableName = "Good Tray Stage 2";
            }
            else
            {
                m_strTableName = "Rework Tray Stage";
            }
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            //4가 테이블번호
            m_nTableNo = (int)args[4];
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }

        #region INIT CYCLE
        public int InitCycle()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nCurrentOrder = 0;
                    }
                    break;
                case 2:
                    {
                    }
                    break;
                case 20:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                nFuncResult = SafetyMgr.Inst.GetAxisSafetyBeforeHome(m_nInitAxisNoArray[nCnt]);
                                if (FNC.IsErr(nFuncResult))
                                {
                                    return nFuncResult;
                                }
                            }
                        }
                    }
                    break;
                case 22:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].HomeStart();
                            }
                        }
                    }
                    break;
                case 24:
                    {
                        if (FNC.IsBusy(RunLib.msecDelay(200)))
                            return FNC.BUSY;
                    }
                    break;
                case 26:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Fail)
                                {
                                    return (int)ERDF.E_SV_NOT_HOME + m_nInitAxisNoArray[nCnt];
                                }
                                else if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Process ||
                                         MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].IsBusy())
                                {
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case 28:
                    {
                        m_nCurrentOrder++;

                        if (m_nCurrentOrder < 10)
                        {
                            NextSeq(20);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 30:
                    {

                    }
                    break;
                case 40:
                    {
                        GbSeq.autoRun[SEQ_ID.UNLOAD_GD1_TRAY_TABLE + m_nTableNo].InitSeq();
                        SEQ_INFO_CURRENT.Init();

                        return FNC.SUCCESS;
                    }
                //break;
                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Tray Loading
        public int GoodTrayLoading()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        
                    }
                    break;

                case 2:
                    {
                        if (!CheckCycle()) return FNC.CYCLE_CHECK;

                        if (!SafetyCheckTrayTableMove(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING)) return FNC.BUSY;
                    }
                    break;

                // 스테이지가 다운 상태일 경우 작업 위치 이동 대기 후 작업 준비
                case 4:
                    {
                        // 테이블 실린더 UP 체크
                        // DOWN 상태일 경우(UP이 아니면) 작업위치로 이동
                        if (!IsStageUp())
                        {
                            // DOWN 상태에서 해당 스테이지 사용하지 않을 때 대기
                            if (IsUseStage() == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            return FNC.SUCCESS;
                        }
                    }
                    break;

                case 6:
                    {
                        // 테이블 준비위치 이동
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Unloading Move", m_strTableName, "Start");

                            if (IsInPosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                                break;
                        }

                        nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Unloading Move", m_strTableName, "Done");
                        }
                    }
                    break;

                case 8:
                    {
                        // 트레이가 있을 경우 WorkReady 이동
                        // 만재일 경우 배출
                        if (IsExistTray())
                        {
                            return FNC.SUCCESS;
                        }
                    }
                    break;

                case 10:
                    {
                        // LOT END 확인
                        if (GbVar.Seq.bIsLotEndRun)
                        {
                            // 반대편 스테이지에 자재가 있을 경우 스테이지 다운 후 워킹으로 가야 반대편 스테이지가 트레이 배출 가능
                            if (IsSideExistTray() == true)
                            {
                                return FNC.SUCCESS;
                            }
                            // 없으면 대기
                            else
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }

                        // 사용 유무 확인
                        // 스테이지가 업 상태이고 사용하지 않는다면 트레이를 받지 않고 다운해야 한다
                        if (IsUseStage() == false)
                        {
                            return FNC.SUCCESS;
                        }
                    }
                    break;

                // 트레이 공급 요청
                case 12:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray Loading Request", m_strTableName, "Request");

                            m_bFirstSeqStep = false;
                        }

                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                    }
                    break;

                case 14:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Wait Tray Loading", m_strTableName, "Wait");

                            m_bFirstSeqStep = false;
                        }

                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] == true) return FNC.CYCLE_CHECK;

                        SeqHistory("Tray Loading Completed", m_strTableName, "Complete");
                    }
                    break;

                case 60:
                    {
                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Tray Work Ready
        public int GoodTrayWorkReady()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {

                    }
                    break;

                case 1:
                    {
                    }
                    break;

                case 2:
                    {
                    }
                    break;

                case 3:
                    {
                    }
                    break;

                case 60:
                    {
                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Tray Evasion
        public int GoodTrayEvasionMove()
        {

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        //Y이동전에 한번더 체크 
                        //if (!CheckCycle()) return FNC.CYCLE_CHECK;

                        //if (!GbFunc.IsGdTrayTableYMoveSafe()) return FNC.BUSY;
                    }
                    break;

                case 2:
                    {
                        if (m_bAutoMode)
                        {
                            if (!CheckCycle()) return FNC.CYCLE_CHECK;
                        }

                        //Y가 이동할 수 있는지 체크, 서로 UP DOWN이 상반되어 있는 상태
                        if (GbFunc.GetGdTrayTableYMoveSafe() == FNC.SUCCESS)
                        {
                            //Y이동 가능하면 바로 그냥 이동 하는 스텝으로 가자
                            //실린더 동작하고
                            NextSeq(8);
                            return FNC.BUSY;
                        }
                    }
                    break;
                //상반되어 있지 않으면 서로 상반되게 만든다.
                case 4:
                    {
                        nFuncResult = GoodTrayStageUpDown(m_nTableNo, true);
                    }
                    break;
                case 6:
                    {
                        int nOtherTrayTableNo = m_nTableNo == 0 ? 1 : 0;

                        nFuncResult = GoodTrayStageUpDown(nOtherTrayTableNo, false);
                    }
                    break;
                case 7:
                    {
                        //상반되어있지 않으면 알람 띄우자
                        if (GbFunc.GetGdTrayTableYMoveSafe() != FNC.SUCCESS)
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STAGE_1_CYL_CAN_NOT_UP + 2 * m_nTableNo;
                        }
                    }
                    break;
                case 8:
                    {
                        nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Working Move", m_nTableNo, "Done");
                        }
                    }
                    break;
                case 10:
                    {
                        int nOtherTrayTableNo = m_nTableNo == 0 ? 1 : 0;

                        nFuncResult = MovePosGdTrayStgY(nOtherTrayTableNo, POSDF.TRAY_STAGE_TRAY_WORKING_P1);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Unloading Move", nOtherTrayTableNo, "Done");
                        }
                    }
                    break;

                case 12:
                    {
                        nFuncResult = GoodTrayStageUpDown(m_nTableNo, true);
                    }
                    break;
                case 14:
                    {
                        int nOtherTrayTableNo = m_nTableNo == 0 ? 1 : 0;

                        nFuncResult = GoodTrayStageUpDown(nOtherTrayTableNo, false);
                    }
                    break;

                case 16:
                    {
                        if (GbFunc.GetGdTrayTableYMoveSafe() != FNC.SUCCESS)
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STAGE_1_CYL_CAN_NOT_UP + 2 * m_nTableNo;
                        }
                    }
                    break;

                case 20:
                    {

                    }
                    break;

                case 60:
                    {
                        //if (!m_bAutoMode &&
                        //    GbVar.MANUAL_TRAY_TABLE_OUT_START)
                        //{
                        //    //2022 08 25 HEP
                        //    //성공했으면 다음 스텝으로 올림
                        //    GbVar.MANUAL_GOOD_TRAY_TABLE_OUT[m_nTableNo] = 2;
                        //}
                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Tray Inspection
        public int TrayTalbeChipMeasure()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    m_nMeasureTableNo = m_nTableNo;

                    MEASURE_COUNT = 0;
                   
                    // Tray Vision Skip으로 변경 필요
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {
                        //카운트 리셋
                        //IFMgr.Inst.VISION.SetTrayInspCountReset(true);
                    }
                    else
                    {
                        NextSeq(60);
                        return FNC.BUSY;
                    }

                    TTDF.SetTact((int)TTDF.CYCLE_NAME.TRAY_VISION_INSP, true);

                    break;

                case 1:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {
                        //측정 전 리스트에 쌓여 있는 데이터를 제거한다.
                        GbFunc.SetTrayInspCountReset(m_nMeasureTableNo);
                    }
                    break;

                case 2:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                        //    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        //{
                        //    return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_1;
                        //}
                        //if (IFMgr.Inst.VISION.IsTrayCountReset) return FNC.BUSY;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY RECEIVE CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 10:
                    // TRAY VISION START POS MOVE
                    {
                        nFuncResult = MovePosGdTrayStgY(CFG_DF.TRAY_STG_GD1 + m_nMeasureTableNo, POSDF.TRAY_STAGE_TRAY_INSPECTION_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE Y TRAY VISION START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    if (RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY <= MEASURE_COUNT)
                    {
                        //if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MARK_VISION_SKIP].bOptionUse) CmdInspectionDBRead();

                        NextSeq(50);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // Inspection Next 이동
                    nFuncResult = MovePosTrayInspYNext(MEASURE_COUNT, m_nMeasureTableNo);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "INSPECTION STAGE MOVE TO NEXT POSITION", STEP_ELAPSED));
                    }
                    break;

                case 34:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 36:
                    // TriggerShot
                    GbDev.trayVision.IsTrayInspReq = false;
                    GbFunc.SetTrayInspReq();
                    break;

                case 38:
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_TRAY_VISION_TIME_OUT;
                    }
                    if (!GbDev.trayVision.IsTrayInspReq) return FNC.BUSY;
                    GbFunc.GetCurrentInspResult(m_nMeasureTableNo);
                    break;

                case 40:
                    if (RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY == 0)
                    {
                        // 빈 값을 넣어줌
                        double[] arrMeasuredValue = new double[15];
                        GbDev.trayVision.ListMeasure.Add(arrMeasuredValue);
                        NextSeq(48);
                        return FNC.BUSY;
                    }

                    // Inspection Next 이동
                    nFuncResult = MovePosTrayInspYOffset(MEASURE_COUNT, m_nMeasureTableNo);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "INSPECTION STAGE MOVE TO NEXT POSITION", STEP_ELAPSED));
                    }
                    break;

                case 42:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_DETECT_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 44:
                    // TriggerShot
                    GbDev.trayVision.IsTrayInspReq = false;
                    GbFunc.SetTrayInspReq();
                    break;

                case 46:
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_TRAY_VISION_TIME_OUT;
                    }
                    if (!GbDev.trayVision.IsTrayInspReq) return FNC.BUSY;
                    GbFunc.GetCurrentInspResult(m_nMeasureTableNo);
                    break;

                case 48:
                    MEASURE_COUNT++;
                    NextSeq(30);
                    return FNC.BUSY;

                case 50:
                    // Map Stage Unloading Pos move
                    break;

                case 52:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {

                    }
                    break;

                case 53:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {

                    }
                    break;

                case 54:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                    {
                        GbFunc.GetTrayInspResult(m_nMeasureTableNo);
                    }
                    break;

                case 56:

                    break;

                case 60:
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY INSPECTION CYCLE FINISH", STEP_ELAPSED));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.TRAY_VISION_INSP, false);

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Tray Align

        /// <summary>
        /// 220511 pjh
        /// 트레이 트랜스퍼에서 하던 얼라인 사이클을 테이블에서하도록 수정
        /// </summary>
        /// <returns></returns>
        public int TrayAlign()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    m_nTrayTableAlignRetryCnt = 0;

                    if (m_nTableNo == 0)
                    {
                        m_nSettingCnt = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_1_ALIGN_RETRY_COUNT].nValue;
                    }
                    else if (m_nTableNo == 1)
                    {
                        m_nSettingCnt = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_2_ALIGN_RETRY_COUNT].nValue;
                    }
                    else
                    {
                        m_nSettingCnt = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.REWORK_TRAY_ALIGN_RETRY_COUNT].nValue;
                    }
                    break;

                case 1:
                    m_nTrayTableAlignRetryCnt++;
                    break;

                case 10:
                    // TRAY STAGE ALIGN CLAMP ON
                    {
                        nFuncResult = TrayStageAlignClamp(m_nTableNo, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE ALIGN CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 11:
                    // TRAY STAGE ALIGN CLAMP ON
                    {
                        nFuncResult = TrayStageAlignClamp(m_nTableNo, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE ALIGN CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 13:
                    if (m_nTrayTableAlignRetryCnt < m_nSettingCnt)
                    {
                        NextSeq(1);
                        return FNC.BUSY;
                    }
                    break;

                case 14:
                    // TRAY STAGE GRIP ON
                    {
                        nFuncResult = TrayStageGrip(m_nTableNo, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE GRIP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    return FNC.SUCCESS;

                default:
                    break;
            }

            #region AFTER SWITCH
            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region 전용 함수
        bool IsStageUp()
        {
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_STG_UP, IODF.INPUT.GD_TRAY_2_STG_UP };

            if (m_nTableNo > 1)
                return true;

            // UP만 본다. DOWN 센서가 안들어 올 수도 있기 때문
            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsStageDown()
        {
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_STG_DOWN, IODF.INPUT.GD_TRAY_2_STG_DOWN };

            if (m_nTableNo > 1)
                return true;

            // DOWN만 본다. UP 센서가 안들어 올 수도 있기 때문
            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsSideStageUp()
        {
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_2_STG_UP, IODF.INPUT.GD_TRAY_1_STG_UP };

            if (m_nTableNo > 1)
                return true;

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsExistTray()
        {
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_CHECK, IODF.INPUT.GD_TRAY_2_CHECK, IODF.INPUT.RW_TRAY_CHECK };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsSideExistTray()
        {
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_2_CHECK, IODF.INPUT.GD_TRAY_1_CHECK, IODF.INPUT.RW_TRAY_CHECK };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsUseStage()
        {
            int[] nOption = { (int)OPTNUM.GOOD_TRAY_STAGE_1_USE, (int)OPTNUM.GOOD_TRAY_STAGE_2_USE };

            if (m_nTableNo > 1)
                return true;

            return ConfigMgr.Inst.Cfg.itemOptions[nOption[m_nTableNo]].bOptionUse;
        }

        bool IsUseSideStage()
        {
            int[] nOption = { (int)OPTNUM.GOOD_TRAY_STAGE_2_USE, (int)OPTNUM.GOOD_TRAY_STAGE_1_USE };

            if (m_nTableNo > 1)
                return true;

            return ConfigMgr.Inst.Cfg.itemOptions[nOption[m_nTableNo]].bOptionUse;
        } 
        #endregion
    }
}
