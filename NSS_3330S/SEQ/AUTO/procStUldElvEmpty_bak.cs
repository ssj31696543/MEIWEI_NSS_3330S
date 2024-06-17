using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.SEQ.CYCLE;
using System.Diagnostics;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldElvEmpty : SeqBase
    {
        int m_nElvPorNum = 0;

        double dSearchedCurrPos = 0.0;
        double dWorkingReadyPos = 0.0;

        double dElvOffset;
        double dElvOffsetMoveVel;

        cycUldElevator cycFunc = null;

        Stopwatch coverWaitStopWatch_Empty = new Stopwatch();

        public procStUldElvEmpty(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_nElvPorNum = nSeqID -  (int)SEQ_ID.ULD_ELV_EMPTY_1;
            m_seqInfo = GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum];

            cycFunc = new cycUldElevator(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY seqNo)
        {
            NextSeq((int)seqNo);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
            cycFunc.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();
            cycFunc.InitSeq();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_seqInfo != null)
            {
                if (m_seqInfo != GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum])
                {
                    m_seqInfo = GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum];
                    m_nSeqNo = GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].nCurrentSeqNo;
                }
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }


            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            
            switch ((seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY)m_nSeqNo)
            {
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT:
                    //if (GbVar.LOADER_TEST)
                    //    return;

                    //220615 pjh
                    if (GbVar.Seq.bIsLotEndRun == true)
                    {
                        if (LeaveCycle() == false) return;

                        if (GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                        {
                            GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] = false;
                            SeqHistory("-----------", "Empty Tray", "Init Lot End !!");
                        }
                        if (GbVar.IO[IODF.INPUT.RW_TRAY_CHECK] == 1)
                        {
                            break;
                        }
                        //랏엔드 끝날대 까지 기다림
                        return;
                    }

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Empty Tray", "Start !!");
                    }
                    nFuncResult = TrayElevStackerUpDown(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory("Stacker Up", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Complete");
                        coverWaitStopWatch_Empty.Restart();
                    }
                    break;
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.COVER_CLOSE_WAIT:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // jy.yang 231030
                        // COVER SENSOR DELTED

                        //if (GbVar.IO[IODF.INPUT.ELV1_COVER_CHECK + MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum] == 0)
                        //{
                        //    if (IsDelayOver(10000))
                        //    {
                        //        SetError((int)ERDF.E_GOOD_1_ELV_COVER_CHECK_TIME_OUT + MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum);
                        //        return;
                        //    }
                        //    coverWaitStopWatch_Empty.Restart();
                        //    return;
                        //}
                        ////커버가 닫혀도 일정시간 딜레이
                        //if (coverWaitStopWatch_Empty.ElapsedMilliseconds < 3000)
                        //{
                        //    return;
                        //}

                        //coverWaitStopWatch_Empty.Reset();
                    }
                    break;
                //220505 pjh 시작할 때 가이드 실린더 전진 후 시작 추가
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_CLOSE:

                    // jy.yang 231030
                    // COVER SENSOR DELTED
                    //if (m_bFirstSeqStep)
                    //{
                    //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    //    if (GbVar.IO[IODF.INPUT.ELV1_COVER_CHECK + MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum] == 0)
                    //    {
                    //        SetError((int)ERDF.E_GOOD_1_ELV_COVER_CHECK_TIME_OUT + MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum);
                    //        return;
                    //    }
                    //}

                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, true, 500);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Close (Forward)", string.Format("Guide Cylinder Empty {0}", m_nElvPorNum + 1), "Complete");
                    }
                    break;


                //220608 트레이 개수를 over해서 투입했거나 이미 시그널 SEARCH센서가 감지되어있을 경우 알람
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.CHECK_TRAY_QTY_OVER:
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.EMTY_1_TRAY_ELV_CHECK + m_nElvPorNum * 8] == 1)
                        nFuncResult = (int)ERDF.E_EMPTY1_ELEVATOR_TRAY_QTY_OVER + m_nElvPorNum;
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT_SIGNAL_SEARCH:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis", m_nElvPorNum + 1), "Start");
                    }
                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum].GetRealPos();
                        //220615 로그 표시
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis Signal Search Pos{1}", m_nElvPorNum + 1, dSearchedCurrPos), "Complete");
                    }
                    break;
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.IS_EXIST_TRAY:
                    // Tray 공급 요청
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT + m_nElvPorNum].dValue < dSearchedCurrPos)
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Not Exist");

                        GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;

                        GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].IS_TRAY = false;
                        NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_OPEN);
                        return;
                    }
                    SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Exist");
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.WORK_OFFSET_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Work Offset Move", string.Format("Empty Tray Elevator {0} Z Axis", m_nElvPorNum + 1), "Start");
                    }

                    // READY
                    dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_WORKING_WAIT_OFFSET + m_nElvPorNum].dValue;
                    nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, dSearchedCurrPos, dElvOffset);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].SetProcReady();

                        GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = true;

                        dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum].GetRealPos();
                        SeqHistory("Work Offset Move", string.Format("Empty Tray Elevator {0} Z Axis", m_nElvPorNum + 1), "Complete");
                    }
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.WAIT_TRAY_OUT_TO_TRANSFER:

                    if (LeaveCycle() == false) return;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check IF Tray Out Comp", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Wait");
                    }

                    // 트레이 트랜스퍼가 픽업이 끝났을 때에만 동작되게
                    if (!GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PICKUP_PROCESS])
                    {
                        //220615 pjh 오토 배출 버튼 클릭 시 트레이 배출
                        if (GbVar.Seq.bEmptyElvOut[m_nElvPorNum])
                        {
                            SeqHistory("Check IF Tray Out Comp", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "bEmptyElvOut On");

                            GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;
                            NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.TRAY_OUT);
                            return;
                        }

                        if (GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                        {
                            SeqHistory("Check IF Tray Out Comp", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "LOT_END_CHECKED On");

                            GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.EMPTY_TRAY_READY] = false;
                            NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_OPEN);
                            return;
                        }
                    }

                    if (!GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP]) return;

                    GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].ProcCycleStart(0);
                    GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.TRAY_OUT_COMP] = false;

                    SeqHistory("IF Tray Out Comp", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Complete");
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ELV_RESEARCH_OFFSET_MOVE:
                    //220615 pjh
                    dElvOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET + m_nElvPorNum].dValue;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Research Offset Move", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Start");

                        if (dSearchedCurrPos - dElvOffset < TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum].dPos[POSDF.TRAY_ELEV_READY])
                        {
                            //1호기, 2호기의 엘리베이터 - 리밋 위치가 다름
                            //1호기는 2호기에 비해 10mm정도 아래에 위치함.
                            //때문에 1호기는 트레이가 full이어도 ULD_EMPTY1_TRAY_ELV_RESEARCH_OFFSET이동 시 리밋알람 발생 안함.

                            //이동해야할 거리가 0보다 작을 경우 0으로 이동
                            dSearchedCurrPos = 0;
                            dElvOffset = 0;
                        }
                    }

                    nFuncResult = MoveElvOffsetPos(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, dSearchedCurrPos, -dElvOffset);
                   
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum].GetRealPos();
                        SeqHistory("Research Offset Move", string.Format("Empty Tray Elevator {0} pos : {1}", m_nElvPorNum + 1, dWorkingReadyPos), "Done");

                        GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].ProcCycleEnd(0);
                        NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT_SIGNAL_SEARCH);
                        return;
                    }

                    return;

                    /////////////////////////////////공급 준비
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_OPEN:
                    nFuncResult = TrayElevGuideFwdBwd(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, false);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Open (Backward)", string.Format("Guide Cylinder Empty {0}", m_nElvPorNum + 1), "Complete");
                    }
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.ELV_MOVE_TO_READY_POS:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", m_nElvPorNum + 1), "Start");
                    }

                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_EMPTY_1 + m_nElvPorNum, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z + m_nElvPorNum].GetRealPos();

                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("EMPTY{0} TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", m_nElvPorNum + 1), STEP_ELAPSED));
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0} pos : {1}", m_nElvPorNum + 1, dWorkingReadyPos), "Complete");
                        if (GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                        {
                            GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] = false;
                            SeqHistory("-----------", "Empty Tray", "Lot End !!");
                            NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT);
                            return;
                        }
                    }
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.EMPTY_TRAY_LOAD_REQ:
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.TRAY_IN:
                    nFuncResult = cycFunc.TrayIn();
                    if(nFuncResult == FNC.SUCCESS)
                    {
                        //2022 06 29 HEP
                        //트레이 공급일때 랏엔드 바로 진행
                        if (GbVar.Seq.sUldTrayElvEmpty[m_nElvPorNum].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] == true)
                        {
                            NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.GUIDE_CYL_OPEN);
                            return;
                        }
                        //220608
                        NextSeq(seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.CHECK_TRAY_QTY_OVER);
                        return;
                    }
                    break;

                //220615
                //랏 엔드시 트레이 배출 할 경우이곳으로 점프
                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.TRAY_OUT:
                    nFuncResult = cycFunc.TrayOut();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.bEmptyElvOut[m_nElvPorNum] = false;
                        NextSeq((int)seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT);
                        return;
                    }
                    break;

                case seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.FINISH:
                    SeqHistory("-----------", "Empty Tray", "End !!");
                    NextSeq((int)seqUldTrayElvEmpty.SEQ_SOTER_ULD_ELV_EMPTY.INIT);
                    return;
                default:
                    break;
            }

            GbVar.mcState.nSeqStep[m_nSeqID] = m_nSeqNo;

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
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;
            else if (FNC.IsCycleCheck(nFuncResult))
            {
                LeaveCycle();

                Type type = m_seqInfo.GetType();
                GbVar.SeqMgr.UpdateSeqObj(type, m_seqInfo);
                return;
            }

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
        }
    }
}
