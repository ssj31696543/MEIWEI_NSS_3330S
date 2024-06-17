using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycTrayTransfer :SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;

        int m_nLoadingElvNo = 0;
        int m_nLoadingTableNo = 0;

        int m_nUnloadingElvNo = 0;
        int m_nUnloadingTableNo = 0;

        int m_nMeasureTableNo = 0;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        #region PROPERTY
        public seqUldTrayTransfer SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sUldTrayTransfer; }
        }


        public int LOAD_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nLoadTableNo;
                }

                return m_nLoadingTableNo;
            }
        }

        public int LOAD_ELV_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nLoadElvNo;
                }

                return m_nLoadingElvNo;
            }
        }

        public int UNLOAD_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nUnloadTableNo;
                }

                return m_nUnloadingTableNo;
            }
        }

        public int UNLOAD_ELV_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nUnloadElvNo;
                }

                return m_nUnloadingElvNo;
            }
        }
        #endregion

        public cycTrayTransfer(int nSeqID)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldTrayTransfer;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            m_nLoadingElvNo = (int)args[0];
            m_nLoadingTableNo = (int)args[1];

            m_nUnloadingElvNo = (int)args[2];
            m_nUnloadingTableNo = (int)args[3];

            m_nMeasureTableNo = (int)args[4];
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
                        GbSeq.autoRun[SEQ_ID.TRAY_TRANSFER].InitSeq();
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

        #region Tray Loading From Elv
        public int TrayLoadingFromElv()
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
                    }
                    break;

                case 5:
                    if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) && MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                    {
                        if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            NextSeq(26);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 10:
                    // GRIP OFF
                    {
                        nFuncResult = TrayPickerAtcClamp(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER UNCLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 13:
                    {
                        // TRAY TRANSFER Y EMPTY + n POS MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)

                        //2022 08 24 HEP
                        //트레이 피커가 빈 엘리베이터 쪽으로 이동하는것은 X충돌 위험이 있기 때문에 Y부터 미리 움직이는게 좋다.
                        nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Y AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 14:
                    // TRAY TRANSFER X EMPTY + n POS MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER X AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // TRAY TRANSFER Z AXIS LOADING DOWN POS  MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO, 0.0f, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 26:
                    // GRIP ON
                    {
                        nFuncResult = TrayPickerAtcClamp(true, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 27:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.TRAY_PK_Z].dPos[POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos = dTargetPos * 0.1;
                    }
                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(29);
                        return FNC.BUSY;
                    }
                    break;

                case 28:
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + LOAD_ELV_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW UP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 29:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 40:
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

        #region Tray Loading From Table
        public int TrayLoadingFromTable()
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
                    }
                    break;

                case 5:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) && MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                    {
                        if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            NextSeq(26);
                            return FNC.BUSY;
                        }
                        if (LOAD_TABLE_NO == 0)
                        {
                            //트레이피커는 감지되어 있고 트레이테이블에 감지가 되어 있지 않는 경우
                            if (MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_CHECK) == false)
                            {
                                NextSeq(36);
                                return FNC.BUSY;
                            }
                        }
                        if (LOAD_TABLE_NO == 1)
                        {
                            //트레이피커는 감지되어 있고 트레이테이블에 감지가 되어 있지 않는 경우
                            if (MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_CHECK) == false)
                            {
                                NextSeq(36);
                                return FNC.BUSY;
                            }
                        }

                    }
                    break;

                case 10:
                    // GRIP OFF
                    {
                        nFuncResult = TrayPickerAtcClamp(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER UNCLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 14:
                    // GOOD TRAY STAGE Y UNLOADING POS MOVE
                    {
                        if (LOAD_TABLE_NO == 0)
                        {
                            //nFuncResult = MovePosGdTrayStg1Y(POSDF.TRAY_STAGE_TRAY_UNLOADING);
                            nFuncResult = MovePosGdTrayStgY(CFG_DF.TRAY_STG_GD1, POSDF.TRAY_STAGE_TRAY_UNLOADING);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 Y AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else if (LOAD_TABLE_NO == 1)
                        {
                            //nFuncResult = MovePosGdTrayStg2Y(POSDF.TRAY_STAGE_TRAY_UNLOADING);
                            nFuncResult = MovePosGdTrayStgY(CFG_DF.TRAY_STG_GD2, POSDF.TRAY_STAGE_TRAY_UNLOADING);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 2 Y AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            }

                        }
                        else
                        {
                            nFuncResult = MovePosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "REWORK TRAY STAGE 1 Y AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                case 15:
                    // GOOD TRAY STAGE UP
                    {
                        if (LOAD_TABLE_NO == 0)
                        {
                            //20211112 choh : Good Tray Stage Up Down 함수 수정 (테이블 번호 추가)
                            //nFuncResult = GoodTray1StageUpDown(true);
                            nFuncResult = GoodTrayStageUpDown(CFG_DF.TRAY_STG_GD1, true);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 Y UP COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else if (LOAD_TABLE_NO == 1)
                        {
                            //20211112 choh : Good Tray Stage Up Down 함수 수정 (테이블 번호 추가)
                            //nFuncResult = GoodTray2StageUpDown(true);
                            nFuncResult = GoodTrayStageUpDown(CFG_DF.TRAY_STG_GD2, true);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 2 Y UP COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                case 16:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    //2022 08 25 HEP 
                    // 트레이피커 인터락관련 변경 
                    // GOOD으로 이동하는것은 X부터 움직이는게 안전
                    {
                        nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER X AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 17:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Y AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    {
                        // Z축 내려가기 전 트레이 스테이지 위치 확인
                        if (LOAD_TABLE_NO < 2)
                        {
                            if (IsInPosGdTrayStgY(LOAD_TABLE_NO, POSDF.TRAY_STAGE_TRAY_UNLOADING) == false ||
                                IsStageDown(LOAD_TABLE_NO))
                            {
                                return (int)ERDF.E_WRONG_POS_TRAY_PICKER_LOAD_TO_GOOD_TRAY_STAGE_1 + LOAD_TABLE_NO;
                            }
                        }
                        else if (LOAD_TABLE_NO == 2)
                        {
                            if (IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING) == false)
                            {
                                return (int)ERDF.E_WRONG_POS_TRAY_PICKER_LOAD_TO_GOOD_TRAY_STAGE_1 + LOAD_TABLE_NO;
                            }
                        }
                        
                    }
                    break;

                case 20:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // TRAY TRANSFER Z AXIS LOADING DOWN POS  MOVE (0: EMPTY1, 1: EMPTY2, 2: COVER)
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO, 0.0f, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 26:
                    // GRIP ON
                    {
                        nFuncResult = TrayPickerAtcClamp(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 28:
                    // ALIGN CLAMP OFF
                    {
                        nFuncResult = TrayStageAlignClamp(LOAD_TABLE_NO, false, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // GRIP OFF
                    {
                        nFuncResult = TrayStageGrip(LOAD_TABLE_NO, false, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 32:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.TRAY_PK_Z].dPos[POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                    
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos = dTargetPos * 0.1;
                    }

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(36);
                        return FNC.BUSY;
                    }
                    break;

                case 34:
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + LOAD_TABLE_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW UP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 38:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 50:
                    {
                        //string strStripInfo = string.Format("{0}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));

                        //if (!m_bAutoMode &&
                        //    GbVar.MANUAL_TRAY_TABLE_OUT_START)
                        //{
                        //    //2022 08 25 HEP
                        //    //성공했으면 다음 스텝으로 올림

                        //    if (LOAD_TABLE_NO < 2)
                        //    {
                        //        GbVar.MANUAL_GOOD_TRAY_TABLE_OUT[LOAD_TABLE_NO] = 3;
                        //    }
                        //    else
                        //    {
                        //        GbVar.MANUAL_REWORK_TRAY_TABLE_OUT = 2;
                        //    }
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

        #region Tray Unloading To Elv
        public int TrayUnloadingToElv()
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
                    }
                    break;

                case 5:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) && !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                    {
                        NextSeq(28);
                        return FNC.BUSY;
                    }
                    break;

                case 10:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    // TRAY ELV STACKER UP 0 : GOOD1, 1 : GOOD1, 2 : REWORK
                    {
                        //메뉴얼이나 오토런 상관 없이 스테커 올라가있는 경우에는 내리기 위함
                        //if (m_bAutoMode) break;

                        //220516 메뉴얼 동작시에만 스테커 다운
                        if (UNLOAD_ELV_NO == 0)
                        {
                            nFuncResult = TrayElevStackerUpDown(UNLOAD_ELV_NO, false, 200);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY ELV 1 STACKER DOWN COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else if (UNLOAD_ELV_NO == 1)
                        {
                            nFuncResult = TrayElevStackerUpDown(UNLOAD_ELV_NO, false, 200);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY ELV 2 STACKER DOWN COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else if (UNLOAD_ELV_NO == 2)
                        {
                            nFuncResult = TrayElevStackerUpDown(UNLOAD_ELV_NO, false, 200);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "REWORK TRAY ELV STACKER DOWN COMPLETE", STEP_ELAPSED));
                            }
                        }
                        
                    }
                    break;

                case 16:
                    // TRAY GUIDE FWD 0 : GOOD1, 1 : GOOD1, 2 : REWORK
                    {
                        //??
                    }
                    break;
                case 19:
                    //2022 08 25 HEP 
                    // 트레이피커 인터락관련 변경 
                    // GOOD으로 이동하는것은 X부터 움직이는게 안전
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: GOOD1, 1: GOOD2, 2: REWORK)
                    {
                        nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER X AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 20:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: GOOD1, 1: GOOD2, 2: REWORK)
                    {
                        nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Y AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0 : GOOD1, 1 : GOOD1, 2 : REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // TRAY TRANSFER Z AXIS LOADING DOWN POS  MOVE (0 : GOOD1, 1 : GOOD1, 2 : REWORK)
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO, 0.0f, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // Elv Tray check sensor
                    break;

                case 28:
                    // GRIP OFF
                    {
                        nFuncResult = TrayPickerAtcClamp(false, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.TRAY_PK_Z].dPos[POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos = dTargetPos * 0.1;
                    }

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0 : GOOD1, 1 : GOOD1, 2 : REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_GOOD_1 + UNLOAD_ELV_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW UP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 36:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 50:
                    {
                        //string strStripInfo = string.Format("{0}",

                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));

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

        #region Tray Unloading To EmptyElv
        // 이 함수는 Tray 반송을 위한 전용함수
        public int TrayUnloadingToEmptyElv()
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
                    }
                    break;

                case 5:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) && !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                    {
                        NextSeq(28);
                        return FNC.BUSY;
                    }
                    break;

                case 10:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 18:
                    //2022 08 25 HEP 
                    // 트레이피커 인터락관련 변경 
                    // EMPTY 이동하는것은 Y부터 움직이는게 안전
                    {
                        nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Y AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 20:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: GOOD, 1: REWORK)
                    {
                        nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER X AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: GOOD, 1: REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // TRAY TRANSFER Z AXIS LOADING DOWN POS  MOVE (0: GOOD, 1: REWORK)
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO, 0.0f, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // Elv Tray check sensor
                    break;

                case 28:
                    // GRIP OFF
                    {
                        nFuncResult = TrayPickerAtcClamp(false, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.TRAY_PK_Z].dPos[POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: GOOD, 1: REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_TRAY_ELV_EMPTY_1 + UNLOAD_ELV_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW UP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 36:
                    //if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                    //{
                    //    nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                    //}
                    break;

                case 50:
                    {
                        //string strStripInfo = string.Format("{0}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));

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

        #region Tray Unloading To Table
        public int TrayUnloadingToTable()
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
                    }
                    break;

                case 5:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) && !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                    {
                            NextSeq(28);
                        return FNC.BUSY;
                    }
                    break;

                case 10:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 13:
                    // GOOD TRAY STAGE Y LOADING POS MOVE
                    {
                        if (!m_bAutoMode)
                        {
                            if (UNLOAD_TABLE_NO == 0)
                            {
                                //nFuncResult = MovePosGdTrayStg1Y(POSDF.TRAY_STAGE_TRAY_UNLOADING);
                                nFuncResult = MovePosGdTrayStgY(CFG_DF.TRAY_STG_GD1, POSDF.TRAY_STAGE_TRAY_UNLOADING);
                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 Y AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                                }
                            }
                            else if (UNLOAD_TABLE_NO == 1)
                            {
                                //nFuncResult = MovePosGdTrayStg2Y(POSDF.TRAY_STAGE_TRAY_UNLOADING);
                                nFuncResult = MovePosGdTrayStgY(CFG_DF.TRAY_STG_GD2, POSDF.TRAY_STAGE_TRAY_UNLOADING);
                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 2 Y AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                                }

                            }
                            else
                            {
                                nFuncResult = MovePosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING);

                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "REWORK TRAY STAGE 1 Y AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                                }
                            }
                        }
                    }
                    break;

                case 15:
                    // TRAY STAGE ALIGN CLAMP OFF
                    {
                        nFuncResult = TrayStageAlignClamp(UNLOAD_TABLE_NO, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE ALIGN UNCLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // TRAY STAGE GRIP OFF
                    {
                        nFuncResult = TrayStageGrip(UNLOAD_TABLE_NO, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE UNGRIP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    // GOOD TRAY STAGE UP
                    {
                        if (!m_bAutoMode)
                        {
                            if (UNLOAD_TABLE_NO == 0)
                            {
                                //20211112 choh : Good Tray Stage Up Down 함수 수정 (테이블 번호 추가)
                                //nFuncResult = GoodTray1StageUpDown(true);
                                nFuncResult = GoodTrayStageUpDown(CFG_DF.TRAY_STG_GD1, true);

                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 Y UP COMPLETE", STEP_ELAPSED));
                                }
                            }
                            else if (UNLOAD_TABLE_NO == 1)
                            {
                                //20211112 choh : Good Tray Stage Up Down 함수 수정 (테이블 번호 추가)
                                //nFuncResult = GoodTray2StageUpDown(true);
                                nFuncResult = GoodTrayStageUpDown(CFG_DF.TRAY_STG_GD2, true);

                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 2 Y UP COMPLETE", STEP_ELAPSED));
                                }
                            }
                        }
                    }
                    break;
                case 19:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: GOOD 1, 1: GOOD 2, 2: REWORK)
                    //2022 08 25 HEP 
                    // 트레이피커 인터락관련 변경 
                    // GOOD 이동하는것은 X부터 움직이는게 안전
                    {
                        nFuncResult = MovePosTrayPkX(POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER X AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 20:
                    // TRAY TRANSFER XY EMPTY + n POS MOVE (0: GOOD 1, 1: GOOD 2, 2: REWORK)
                    {
                        nFuncResult = MovePosTrayPkY(POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Y AXIS LOAD POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 21:
                    {
                        // Z축 내려가기 전 트레이 스테이지 위치 확인
                        if (UNLOAD_TABLE_NO < 2)
                        {
                            if (IsInPosGdTrayStgY(UNLOAD_TABLE_NO, POSDF.TRAY_STAGE_TRAY_UNLOADING) == false ||
                                IsStageDown(UNLOAD_TABLE_NO))
                            {
                                return (int)ERDF.E_WRONG_POS_TRAY_PICKER_UNLOAD_TO_GOOD_TRAY_STAGE_1 + UNLOAD_TABLE_NO;
                            }
                        }
                        else if (UNLOAD_TABLE_NO == 2)
                        {
                            if (IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING) == false)
                            {
                                return (int)ERDF.E_WRONG_POS_TRAY_PICKER_UNLOAD_TO_GOOD_TRAY_STAGE_1 + UNLOAD_TABLE_NO;
                            }
                        }
                    }
                    break;

                case 22:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: GOOD 1, 1: GOOD 2, 2: REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // TRAY TRANSFER Z AXIS LOADING DOWN POS  MOVE (0: GOOD 1, 1: GOOD 2, 2: REWORK)
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO, 0.0f, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // Table tray check sensor!!!
                    break;

                case 28:
                    // GRIP OFF
                    {
                        nFuncResult = TrayPickerAtcClamp(false, 100);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.TRAY_PK_Z].dPos[POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos = dTargetPos * 0.1;
                    }

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // TRAY TRANSFER Z AXIS LOADING PRE-DOWN POS  MOVE (0: GOOD 1, 1: GOOD 2, 2: REWORK)
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_STAGE_GOOD_1 + UNLOAD_TABLE_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // TRAY TRANSFER Z AXIS READY POS MOVE
                    {
                        nFuncResult = MovePosTrayPkZ(POSDF.TRAY_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_DETECT_SENSOR_USE].bOptionUse)
                    {
                        if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) || MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                        {
                            nFuncResult = (int)ERDF.E_TRAY_PK_GRIP_CHECK_FAIL;
                        }
                    }
                    break;

                case 50:
                    // TRAY STAGE ALIGN CLAMP ON
                    {
                        nFuncResult = TrayStageAlignClamp(UNLOAD_TABLE_NO, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE ALIGN CLAMP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 51:
                    // TRAY STAGE ALIGN CLAMP ON
                    {
                        if (m_bAutoMode) break;

                        //메뉴얼 동작 시 그립하기
                        nFuncResult = TrayStageGrip(UNLOAD_TABLE_NO, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE GRIP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 52:
                    //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    //220513 pjh 트레이 내려 놓았을때 트레이 잘못 놓였으면 알람
                    // [2022.05.15.kmlee] INPUT 잘못 보고 있고 트레이 별로 구분되지 않아 변경
                    // TRAY STAGE TRAY CHECK
                    {
                        if (UNLOAD_TABLE_NO == 0)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 0)
                            {
                                nFuncResult = (int)ERDF.E_GD_TRAY_STAGE_1_CHECK_FAIL;
                            }
                        }
                        else if (UNLOAD_TABLE_NO == 1)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 0)
                            {
                                nFuncResult = (int)ERDF.E_GD_TRAY_STAGE_2_CHECK_FAIL;
                            }
                        }
                        else
                        {
                            // REWORK 트레이
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 0)
                            {
                                nFuncResult = (int)ERDF.E_RW_TRAY_STAGE_CHECK_FAIL;
                            }
                        }
                        


                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY STAGE ALIGN CLAMP COMPLETE", STEP_ELAPSED));
                        }
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

        bool IsStageDown(int nStageNo)
        {
            if (nStageNo == CFG_DF.TRAY_STG_GD1)
            {
                return !MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_STG_UP)
                        && MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_STG_DOWN);
            }
            else if (nStageNo == CFG_DF.TRAY_STG_GD2)
            {
                return !MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_STG_UP)
                        && MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_STG_DOWN);
            }

            return true;
        }
    }
}
