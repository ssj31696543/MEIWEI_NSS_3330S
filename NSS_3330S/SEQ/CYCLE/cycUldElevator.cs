using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSS_3330S.MOTION;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycUldElevator : SeqBase
    {
        bool m_bAutoMode = true;
        int m_nElvNo = 0;
        Stopwatch m_swTrayInDetectCheck = new Stopwatch();

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

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", "Tray Z Axis Elevator_" + (m_nElvNo + 1), "Done");
                    }
                    break;

                case 12:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Elv Guide Open", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Elv Guide Open", string.Format("{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ULD_CONV_GUIDE_CLOSE
                case 13:

                    //220511 pjh
                    //추후에 트레이 가림막 생기면 해당 실린더 백워드하도록 수정해야함
                    //if (m_bFirstSeqStep)
                    //{
                    //    SeqHistory("Conv Guide Fwd", String.Format("Elv {0} Tray Conv Guide", m_nElvNo + 1), "Start");

                    //    m_bFirstSeqStep = false;
                    //}
                    //nFuncResult = UnloaderGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true);

                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    SeqHistory("Conv Guide Fwd", String.Format("Elv {0} Tray Conv Guide", m_nElvNo + 1), "Done");
                    //}
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
                    nFuncResult = UnloaderConvCw(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 8000);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Conv Cw", String.Format("Elv {0} Tray Conv Rolling", m_nElvNo + 1), "Done");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.WAIT_TRAY_ARRIVA
                case 17:

                    //220510 phj
                    //다 나오기 전에 손으로 트레이 제거하는 경우가있어서 센서안봄
                    //if (m_bFirstSeqStep)
                    //{
                    //    SeqHistory("Tray Detect Mon", String.Format("Elv {0} Tray Conv Detect", m_nElvNo + 1), "Start");

                    //    m_bFirstSeqStep = false;
                    //}
                    //nFuncResult = IsUnloaderConvOutOn(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo,
                    //    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_TRAY_ELV_CONV_INOUT_TIMEOUT].lValue);

                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    SeqHistory("Tray Detect Mon", String.Format("Elv {0} Tray Conv Detect", m_nElvNo + 1), "Done");
                    //}
                    //else if (nFuncResult > FNC.SUCCESS)
                    //{
                    //    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ULD_CONV_1_CW + (m_nElvNo * 2), false);
                    //    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ULD_CONV_1_CCW + (m_nElvNo * 2), false);
                    //}
                    break;
           
                case 19:
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ULD_CONV_STOP
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

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_GUIDE_OPEN
                case 6:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Elv Conv Guide Open", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Elv Conv Guide Open", string.Format("{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_GUIDE_OPEN

                case 7:
                    //if (m_bFirstSeqStep)
                    //{
                    //    SeqHistory("Uld Conv Guide Open", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    //}
                    //nFuncResult = UnloaderGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    SeqHistory("Uld Conv Guide Open", string.Format("Elv {0}", m_nElvNo + 1), "Complete");
                    //}
                    break;

                case 8:
                    //220511 pjh
                    //이미 엘리베이터에 트레이가 있는 경우 스토퍼 닫고 TrayIn Cycle끝내기
                    //TRAY_ELV_RESIDUAL_QTY_CHECK -> B접점
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 0)
                    {
                        SeqHistory("Tray is already exist", string.Format("Elv {0}", m_nElvNo + 1), "check");
                        // [2022.05.15.kmlee] 가끔 컨베이어가 안멈출 때가 있어서 변경함
                        NextSeq(16);
                        //NextSeq(18);
                        return FNC.BUSY;
                    }
                    break;

                case 10:
                    //2205027 phj
                    //예비되어있는 트레이가 있다면 바로 공급하기
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1 || //바깥쪽 센서
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1) //안쪽 센서
                    {
                        SeqHistory("Tray Supply", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                        NextSeq(13);
                        return FNC.BUSY;
                    }

                    //트레이 없다면 요청 비트 ON(UI표시용)
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
                            if (m_swTrayInDetectCheck.IsRunning == true)
                                m_swTrayInDetectCheck.Stop();

                            m_bFirstSeqStep = false;
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

                                return (int)ERDF.E_THERE_IS_NO_TRAY;
                            }
                        }

                        #region 트레이 자동 투입 사용 시
                        //220604 pjh
                        //트레이를 기다리던 중 트레이가 감지되었다면
                        //일정 시간 후 다시 확인해서 트레이가 있다면 공급 진행
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_TRAY_CONV_IN_TIME_USE].bOptionUse)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1 || //바깥쪽 센서
                                GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1) //안쪽 센서
                            {
                                //타이머 재시작
                                if (m_swTrayInDetectCheck.IsRunning == false)
                                    m_swTrayInDetectCheck.Restart();                        
                            }
                            else
                            {
                                if (m_swTrayInDetectCheck.IsRunning == true)
                                    m_swTrayInDetectCheck.Stop();
                            }

                            if (m_swTrayInDetectCheck.IsRunning && 
                                m_swTrayInDetectCheck.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_TRAY_CONV_IN_SEN_DETECT_TIME].lValue)
                            {
                                if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1 || //바깥쪽 센서
                                    GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1) //안쪽 센서
                                {
                                    m_swTrayInDetectCheck.Stop();
                                    m_swTrayInDetectCheck.Reset();  
                                    break;
                                }
                            }
                        }
                        #endregion

                        if (m_nElvNo == MCDF.ELV_TRAY_EMPTY_1)
                        {
                            //랏엔드면 성공으로보내
                            if (GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                            {
                                NextSeq(50);
                                return FNC.BUSY;
                            }
                        }
                        else if(m_nElvNo == MCDF.ELV_TRAY_EMPTY_1)
                        {
                            //랏엔드면 성공으로보내
                            if (GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                            {
                                NextSeq(50);
                                return FNC.BUSY;
                            }
                        }

                        if (GbVar.bTrayInReq[m_nElvNo] == false) return FNC.BUSY;
                        if (m_swTrayInDetectCheck.IsRunning) m_swTrayInDetectCheck.Stop();

                        SeqHistory("Uld Conv In or Out Sensor Check", string.Format("Uld Conv {0}", m_nElvNo + 1), "Detect");
                    }
                    break;

                case 12:

                    if (!GbVar.mcState.IsCycleRunReq(SEQ_ID.ULD_ELV_GOOD_1 + m_nElvNo))
                    {
                        return FNC.CYCLE_CHECK;
                    }

                    //220527
                    //트레이 감지될 때 까지 기다리기
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_OUT + (m_nElvNo * 3)] == 1 || //바깥쪽 센서
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD1_CONV_TRAY_CHECK_IN + (m_nElvNo * 3)] == 1) //안쪽 센서
                    {
                        SeqHistory("Elv In or Out Sensor Check", string.Format("Elv {0}", m_nElvNo + 1), "Detect");
                        GbVar.IsReqTraySupply[MCDF.ELV_TRAY_GOOD_1 + m_nElvNo] = false;
                        break;
                    }

                    //220509 pjh 
                    //만약 공급 버튼이 눌렸을 시 이미 엘리베이터에 트레이가 있다면 컨베이어 멈추고 공급 시작
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvNo * 8)] == 0)//TRAY_ELV_RESIDUAL_QTY_CHECK -> B접점
                    {
                        SeqHistory("Elv Residual Sensor Check", string.Format("Elv {0}", m_nElvNo + 1), "Detect");
                        GbVar.IsReqTraySupply[MCDF.ELV_TRAY_GOOD_1 + m_nElvNo] = false;
                        NextSeq(16);
                        return FNC.BUSY;
                    }

                    return FNC.BUSY;

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_CCW
                case 13:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Uld Conv CCW", string.Format("Uld Conv {0}", m_nElvNo + 1), "Start");
                    }

                    nFuncResult = UnloaderConvCw(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Uld Conv CCW", string.Format("Uld Conv {0}", m_nElvNo + 1), "Done");
                    }
                    break;

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.WAIT_EMPTY_TRAY_ARRIVED
                case 14:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Wait Tray In", string.Format("Elv {0}", m_nElvNo + 1), "Wait");
                    }
                    nFuncResult = IsElvResidualOn(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo,
                        15000, 15000);
                    
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

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_STOP
                case 16:
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

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ULD_CONV_GUIDE_CLOSE
                case 18:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Guide Cylinder Close (Forward)", string.Format("Elv {0}", m_nElvNo + 1), "Start");
                    }
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.bOccuredTrayReqWarning = false;
                        GbVar.bTrayInReq[m_nElvNo] = false;
                        SeqHistory("Guide Cylinder Close (Forward)", string.Format("Elv {0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_CLOSE
                case 20:
                    //220511 pjh 안닫음
                    //if (m_bFirstSeqStep)
                    //{
                    //    SeqHistory("Uld Conv Guide Close", string.Format("Uld Conv {0}", m_nElvNo + 1), "Start");
                    //}
                    //nFuncResult = UnloaderGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    GbVar.bTrayInReq[m_nElvNo] = false;
                    //    SeqHistory("Uld Conv Guide Open", string.Format("Uld Conv {0}", m_nElvNo + 1), "Complete");
                    //}
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
                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.GUIDE_CYL_CLOSE
                case 10:
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Close (Forward)", "Guide Cylinder Elv_" + (m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.INIT_STOPPER_OPEN
                //case 12:
                //    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 200);
                //    if (FNC.IsSuccess(nFuncResult))
                //    {
                //        SeqHistory("Stacker Up", "Good Tray Elevator_" + (m_nElvNo + 1), "Complete");
                //    }
                //    break;

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

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.LOT_END_OFFSET_MOVE
                case 10:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Working Ready Offset Move", "Good Tray Z Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    double dElvOffset = -ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_WORK_SAFETY_OFFSET + m_nElvNo].dValue;
                    dElvOffset += ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_GETTING_OFFSET + m_nElvNo].dValue;

                    nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, dWorkingReadyPos, dElvOffset);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Working Ready Offset Move", "Good Tray Z Axis", "Done");
                    }
                    break;

                    //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.LOT_END_STOPPER_OPEN
                case 12:
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, true, 200);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Stacker Up", String.Format("Elv{0}", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.GUIDE_CYL_OPEN
                case 14:
                    // Work offset up
                    // Stacker Open

                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Open (Backward)", String.Format("Elv{0} Guide Cyl", m_nElvNo + 1), "Complete");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ELV_MOVE_TO_PRE_SAFETY_POS
                case 16:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Pre Safety Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_OFFSET + m_nElvNo].dValue;

                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_TRAY_OHT_LIFT_IN, dElvOffset, 0);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Pre Safety Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Done");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ELV_MOVE_TO_SAFETY_POS
                case 18:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Safety Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }
                    double dElvOffsetMoveVel = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_SAFETY_MOVE_VEL].dValue;

                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_TRAY_OHT_LIFT_IN, dElvOffsetMoveVel, 0, 0);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Safety Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Done");
                    }
                    break;

                //seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.ELV_MOVE_TO_READY_POS
                case 20:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_GOOD_1 + m_nElvNo, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Ready Move", String.Format("Elv{0} Z Axis", m_nElvNo + 1), "Done");
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
