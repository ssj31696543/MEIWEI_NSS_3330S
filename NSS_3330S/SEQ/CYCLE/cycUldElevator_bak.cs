using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.MOTION;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycUldElevator : SeqBase
    {
        bool m_bAutoMode = true;
        int m_nElvNo = 0;

        double dSearchedCurrPos = 0.0;

        Stopwatch coverWaitStopWatch = new Stopwatch();

        public cycUldElevator(int nSeqID)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nElvNo = m_nSeqID - (int)SEQ_ID.ULD_ELV_GOOD_1;

            switch (m_nElvNo)
            {
                case 0:
                    m_seqInfo = GbVar.Seq.sUldTrayElvGood[0];

                    break;
                case 1:
                    m_seqInfo = GbVar.Seq.sUldTrayElvGood[1];

                    break;
                case 2:
                    m_seqInfo = GbVar.Seq.sUldTrayElvRework;

                    break;
                case 3:
                    m_seqInfo = GbVar.Seq.sUldTrayElvEmpty[0];

                    break;
                case 4:
                    m_seqInfo = GbVar.Seq.sUldTrayElvEmpty[1];
                    break;
            }
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            m_nElvNo = (int)args[0];
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }

        public int TrayOut()
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
                        else//220615 pjh
                        {
                            //메뉴얼 동작 시 오토런 시퀀스 초기화

                            m_seqInfo.Init();
                            GbSeq.autoRun.SelectInitSeq(m_nSeqID);
                        }
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ELV_MOVE_TO_READY_POS
                case 10:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Start");
                    }
                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Done");
                    }
                    break;

                case 13:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Guide Bwd", String.Format("Elv {0} Tray Guide", m_nElvNo + 1), "Start");
                    }

                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Guide Bwd", String.Format("Elv {0} Tray Guide", m_nElvNo + 1), "Done");
                    }
                    break;

                case 14:

                    if (m_bAutoMode)
                    {
                        //220511 pjh
                        //트레이 배출 전 이미 배출된 트레이가 있다면 제거 후 배출 할 수 있도록하기 위함 
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1 ||
                            GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1)
                        {
                            if ((GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN] == 1 ||
                                 GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT] == 1) &&
                                (GbVar.GB_INPUT[(int)IODF.INPUT.GD2_CONV_TRAY_CHECK_IN] == 1 ||
                                 GbVar.GB_INPUT[(int)IODF.INPUT.GD2_CONV_TRAY_CHECK_OUT] == 1) &&
                                 GbVar.Seq.sUldTrayElvGood[0].nTrayInCount >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT].nValue &&
                                 GbVar.Seq.sUldTrayElvGood[1].nTrayInCount >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT].nValue
                               )
                            {
                                GbVar.IsAlreadyDetectOutSen[m_nElvNo] = true;

                                ////220615 랏엔드시 혹은 수동 배출 시 배출구에 이미 트레이 있을 경우 알람 띄움
                                //if (GbVar.Seq.bIsLotEndRun == true || m_nElvNo >= MCDF.ELV_TRAY_EMPTY_1)
                                //{
                                nFuncResult = (int)ERDF.E_GOOD1_TRAY_CONV_ALREADY_EXIST_TRAY + m_nElvNo;
                                //}
                                //else
                                //{
                                //    return FNC.BUSY;
                                //}
                            }
                            else
                            {
                                if (GbVar.Seq.bIsLotEndRun)
                                {
                                    return FNC.SUCCESS;
                                }

                                return FNC.CYCLE_CHECK;
                            }
                        }
                        else
                        {
                            GbVar.IsAlreadyDetectOutSen[m_nElvNo] = false;
                        }
                    }
                    else
                    {
                        //220608 pjh 
                        //메뉴얼 동작 시 이미 나와 있는 트레이가 있다면 알람 띄우기
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1 ||
                            GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1)
                        {
                            nFuncResult = (int)ERDF.E_GOOD1_TRAY_CONV_ALREADY_EXIST_TRAY + m_nElvNo;
                        }
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ULD_CONV_CCW
                case 15:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Conv Cw", String.Format("Elv {0} Tray Conv Rolling", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = UnloaderConvCw(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 5000);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Conv Cw", String.Format("Elv {0} Tray Conv Rolling", m_nElvNo + 1), "Done");
                    }
                    break;

                case 21:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Conv Stop", String.Format("Elv {0} Tray Conv Stop", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = UnloaderConvStop(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Conv Stop", String.Format("Elv {0} Tray Conv Stop", m_nElvNo + 1), "Done");
                    }
                    break;

                case 50:
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

        public int TrayIn()
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
                    if (!m_bAutoMode)
                    {

                        nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_READY, 0.0f);

                        if (nFuncResult == FNC.SUCCESS)
                        {

                        }
                    }
                    break;

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_GUIDE_OPEN
                case 6:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Elv Conv Guide Open", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false, 1000);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Elv Conv Guide Open", string.Format("{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                case 8:
                    //220511 pjh
                    //이미 엘리베이터에 트레이가 있는 경우 스토퍼 닫고 TrayIn Cycle끝내기
                    //TRAY_ELV_RESIDUAL_QTY_CHECK -> B접점
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 0)
                    {
                        SeqHistory("Tray is already exist", string.Format("Elv {0}", m_nElvNo + 1), "check");
                        NextSeq(15);
                        return FNC.BUSY;
                    }

                    //트레이 없다면 요청 비트 ON
                    if (m_bAutoMode)
                    {
                        GbVar.IsReqTraySupply[MCDF.ELV_TRAY_GOOD_1 + m_nElvNo] = true;
                    }
                    break;

                case 11:
                    if (m_bAutoMode)
                    {
                        if (!GbVar.mcState.IsCycleRunReq(SEQ_ID.ULD_ELV_GOOD_1 + m_nElvNo))
                        {
                            return FNC.CYCLE_CHECK;
                        }

                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray In Req", string.Format("Elv {0}", m_nElvNo + 1), "Wait");
                        }

                        //220528 pjh
                        //트레이 없을 시 최초 한번만 알람 발생
                        if (GbVar.bOccuredTrayReqWarning == false)
                        {
                            //알람 발생은 EMPTY TRAY 1에서만 발생
                            if (m_nElvNo == MCDF.ELV_TRAY_EMPTY_1 &&
                                GbVar.IsReqTraySupply[MCDF.ELV_TRAY_EMPTY_1] == true && GbVar.IsReqTraySupply[MCDF.ELV_TRAY_EMPTY_2] == true)
                            {
                                GbVar.bOccuredTrayReqWarning = true;
                                SeqHistory("THERE IS NO TRAY", string.Format("Uld Conv {0}", m_nElvNo + 1), "ERROR");
                                return (int)ERDF.E_THERE_IS_NO_TRAY;
                            }
                        }
                        //if (GbVar.bTrayInReq[m_nElvNo] == false) return FNC.BUSY;
                        SeqHistory("Uld Conv In or Out Sensor Check", string.Format("Uld Conv {0}", m_nElvNo + 1), "Detect");
                    }
                    break;

                case 12:
                    if (GbVar.Seq.sUldTrayElvEmpty[m_nElvNo - MCDF.ELV_TRAY_EMPTY_1].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                    {
                        return FNC.SUCCESS;
                    }

                    // jy.yang 231030
                    // COVER SENSOR DELTED
                    //if (GbVar.IO[IODF.INPUT.ELV1_COVER_CHECK + m_nElvNo] == 0)
                    //{
                    //    return FNC.CYCLE_CHECK;
                    //}

                    coverWaitStopWatch.Reset();

                    if (!GbVar.mcState.IsCycleRunReq(SEQ_ID.ULD_ELV_GOOD_1 + m_nElvNo))
                    {
                        return FNC.CYCLE_CHECK;
                    }

                    //트레이 감지 될 때까지 기다리기
                    //if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 1)//TRAY_ELV_RESIDUAL_QTY_CHECK -> B접점
                    //    return FNC.BUSY;

                    SeqHistory("Elv Residual Sensor Check", string.Format("Elv {0}", m_nElvNo + 1), "Detect");
                    GbVar.IsReqTraySupply[MCDF.ELV_TRAY_GOOD_1 + m_nElvNo] = false;
                    break;

                case 13:
                    //트레이 안착 후 바로 틸레이 5000초
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Uld Conv CCW", string.Format("Uld Conv {0}", m_nElvNo + 1), "Start");
                    }

                    nFuncResult = UnloaderConvCw(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 12000);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Uld Conv CCW", string.Format("Uld Conv {0}", m_nElvNo + 1), "Done");
                    }
                    break;

                case 14:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Wait Tray In", string.Format("Elv {0}", m_nElvNo + 1), "Wait");
                    }
                    nFuncResult = IsElvResidualOn(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, 500, 15000);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Wait Tray In", string.Format("Elv {0}", m_nElvNo + 1), "Done");
                    }
                    else if (nFuncResult > FNC.SUCCESS)
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD1_CONV_CW + (m_nElvNo * 2), false);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD1_CONV_CCW + (m_nElvNo * 2), false);
                    }
                    break;

                case 15:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Conv Stop", string.Format("Uld Conv {0}", m_nElvNo + 1), "Wait");
                    }

                    nFuncResult = UnloaderConvStop(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, 0);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Conv Stop", string.Format("Uld Conv {0}", m_nElvNo + 1), "Done");
                    }
                    break;

                case 16:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Elv Conv Guide Close", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 1000);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.bOccuredTrayReqWarning = false;
                        SeqHistory("Elv Conv Guide Close", string.Format("{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                case 17:
                    if (!m_bAutoMode)
                    {
                        nFuncResult = AxisSinalSearch((int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            dSearchedCurrPos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo].GetRealPos();
                        }

                    }
                    break;

                case 18:
                    if (!m_bAutoMode)
                    {
                        double dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET].dValue;
                        nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dSearchedCurrPos, dElvOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {

                        }
                    }
                    break;
                case 30:
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


        public int TrayLoadReady(ref double dSearchedCurrPos, ref double dWorkingReadyPos)
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
                    {
                        if (m_bFirstSeqStep)
                        {
                            coverWaitStopWatch.Restart();
                        }

                    }
                    break;
                case 6:
                    {
                        // jy.yang 231030
                        // COVER SENSOR DELTED
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        //if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_ELV_COVER_WAIT_TIME].nValue))
                        //{
                        //    SetError((int)ERDF.E_GOOD_1_ELV_COVER_CHECK_TIME_OUT + m_nElvNo);
                        //    return FNC.BUSY;
                        //}
                        //if (GbVar.IO[IODF.INPUT.ELV1_COVER_CHECK + m_nElvNo] == 0)
                        //{
                        //    coverWaitStopWatch.Restart();
                        //    return FNC.CYCLE_CHECK;
                        //}
                        ////커버가 닫혀도 일정시간 딜레이
                        //if (coverWaitStopWatch.ElapsedMilliseconds < 3000)
                        //{
                        //    return FNC.CYCLE_CHECK;
                        //}

                        //coverWaitStopWatch.Reset();
                    }
                    break;

                case 8:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_POS_CHECK + (m_nElvNo * 8)] == 1)
                        {
                            return (int)ERDF.E_GOOD1_ELV_STACKER_ALREADY_EXIST_TRAY + m_nElvNo;
                        }
                    }
                    break;
                   

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.GUIDE_CYL_CLOSE
                case 10:
                    // jy.yang 231030
                    // COVER SENSOR DELTED
                    //if (m_bFirstSeqStep)
                    //{
                    //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    //    if (GbVar.IO[IODF.INPUT.ELV1_COVER_CHECK + m_nElvNo] == 0)
                    //    {
                    //        SetError((int)ERDF.E_GOOD_1_ELV_COVER_CHECK_TIME_OUT + m_nElvNo);
                    //        return FNC.BUSY;
                    //    }
                    //}
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Close (Forward)", "Guide Cylinder Elv_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.INIT_STOPPER_OPEN
                case 12:
                    if (GbVar.IO[IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 1)
                    {
                        break;
                    }
                    //위에 올라가있는건 
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 200);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Stacker Up", "Good Tray Elevator_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.INIT_SIGNAL_SEARCH
                case 14:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Signal Search", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo].GetRealPos();
                        SeqHistory("Signal Search", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ELV_WORK_SAFETY_POS_MOVE
                case 16:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Safety Move", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    double dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET + m_nElvNo].dValue;
                    nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dSearchedCurrPos, dElvOffset);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo].GetRealPos();
                        SeqHistory("Safety Move", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.RECV_STOPPER_CLOSE
                case 18:
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false, 200);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Stacker Down", "Tray Elevator_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                case 50:
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

        public int TrayLoad(ref double dSearchedCurrPos, ref double dWorkingReadyPos)
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

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.WORK_OFFSET_MOVE
                case 10:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Work Offset Move", String.Format("Elv{0} Tray Z Axis", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    double dElvOffset = -ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET + m_nElvNo].dValue;
                    dElvOffset += ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET + m_nElvNo].dValue;


                    nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dWorkingReadyPos, dElvOffset);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY ELEVATOR MOVE DOWN TRAY OFFSET POSITION COMPLETE", STEP_ELAPSED));
                        SeqHistory("Work Offset Move", String.Format("Elv{0} Tray Z Axis", m_nElvNo + 1), "Done");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.RECV_STOPPER_OPEN
                case 12:
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 200);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Stacker Up", String.Format("Elv{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.SAFETY_OFFSET_MOVE
                case 14:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Safety Offset Move", String.Format("Elv{0} Tray Z Axis", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }
                    dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET + m_nElvNo].dValue;
                    dElvOffset -= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET + m_nElvNo].dValue;


                    if (m_nElvNo < 2)
                    {
                        dElvOffset = dElvOffset - (GbVar.Seq.sUldTrayElvGood[m_nElvNo].nTrayInCount * RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness);
                        nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dSearchedCurrPos, dElvOffset);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo].GetRealPos();

                            SeqHistory("Safety Offset Move", String.Format("Elv{0} Tray Z Axis", m_nElvNo + 1), "Done");
                        }
                    }
                    else
                    {
                        dElvOffset = dElvOffset - (GbVar.Seq.sUldTrayElvRework.nTrayInCount * RecipeMgr.Inst.Rcp.TrayInfo.dTrayThickness);
                        nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dSearchedCurrPos, dElvOffset);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + m_nElvNo].GetRealPos();

                            SeqHistory("Safety Offset Move", String.Format("Elv{0} Tray Z Axis", m_nElvNo + 1), "Done");
                        }
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.RECV_STOPPER_CLOSE
                case 16:
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false, 200);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Stacker Down", String.Format("Elv{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                case 50:
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

        public int TrayUnload(ref double dSearchedCurrPos, ref double dWorkingReadyPos)
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

                case 10:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Done");
                    }
                    break;
                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.GUIDE_CYL_OPEN
                case 20:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Open (Backward)", String.Format("Elv{0} Guide Cyl", m_nElvNo + 1), "START");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Open (Backward)", String.Format("Elv{0} Guide Cyl", m_nElvNo + 1), "Complete");
                    }
                    break;
                case 30:
                    {
                        //랏엔드 조건
                        if (m_nElvNo>= 2)
                        {
                            if (!GbVar.Seq.bIsLotEndRun)
                            {
                                //1. 트레이 인 카운트가 설정값보다 많은 경우
                                if (GbVar.Seq.sUldTrayElvRework.nTrayInCount >=
                                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_RW_TRAY_ELV_OUT_MAX_COUNT].nValue)
                                {
                                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_ELV_COVER_WAIT_TIME].nValue))
                                    {
                                        return (int)ERDF.E_GD_1_ELV_RESIDUAL_DETECT_FAIL + m_nElvNo;
                                    }
                                    //2. 트레이를 제거 하지 않으면 계속 대기
                                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 0)
                                    {
                                        SeqHistory("Tray is already exist", string.Format("Elv {0}", m_nElvNo + 1), "check");
                                        return FNC.CYCLE_CHECK;
                                    }
                                    SeqHistory("Tray is full", string.Format("Elv {0}", m_nElvNo + 1), "check");
                                }
                            }
                                
                        }
                        else
                        {
                            if (!GbVar.Seq.bIsLotEndRun)
                            {
                                //1. 트레이 인 카운트가 설정값보다 많은 경우
                                if (GbVar.Seq.sUldTrayElvGood[m_nElvNo].nTrayInCount >=
                                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT].nValue)
                                {
                                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_ELV_COVER_WAIT_TIME].nValue))
                                    {
                                        return (int)ERDF.E_GD_1_ELV_RESIDUAL_DETECT_FAIL + m_nElvNo;
                                    }
                                    //2. 트레이를 제거 하지 않으면 계속 대기
                                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 0)
                                    {
                                        SeqHistory("Tray is already exist", string.Format("Elv {0}", m_nElvNo + 1), "check");
                                        return FNC.CYCLE_CHECK;
                                    }
                                    SeqHistory("Tray is full", string.Format("Elv {0}", m_nElvNo + 1), "check");
                                }
                            }
                            
                        }
                        

                    }
                    break;
                case 50:
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
    }
}
