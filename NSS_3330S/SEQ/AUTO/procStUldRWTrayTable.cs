using NSS_3330S.MOTION;
using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldRWTrayTable : SeqBase
    {
        cycUldTrayTable cycFunc = null;

        public procStUldRWTrayTable(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldRWTrayTable;

            cycFunc = new cycUldTrayTable(nSeqID, MCDF.ULD_TRAY_REWORK);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
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
                if (m_seqInfo != GbVar.Seq.sUldRWTrayTable)
                {
                    m_seqInfo = GbVar.Seq.sUldRWTrayTable;
                    m_nSeqNo = GbVar.Seq.sUldRWTrayTable.nCurrentSeqNo;
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

            m_bFirstSeqStep = false;

            switch (m_nSeqNo)
            {
                case 0:
                    // Init
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Rework Tray Table", "Start !!");

                        m_bFirstSeqStep = false;
                    }
                    break;

                // 언로딩 위치 이동
                case 10:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Unloading Move", "Rework Table Y Axis", "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = MovePosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Tray Unloading Move", "Rework Table Y Axis", "Done");
                    }
                    break;

                // 트레이 만재 체크
                case 12:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Tray Unit Full", "Rework Tray Table", "Check");

                        m_bFirstSeqStep = false;
                    }

                    // 트레이 만재 및 LOT END 체크
                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] == true || GbVar.Seq.bIsLotEndRun)
                    {
                        SeqHistory("Tray Unit Full", "Rework Tray Table", "Full");

                        GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] = false;
                        // 트레이가 있거나 드라이런이면 트레이 배출 //20221207 CHOH : 트레이 없이 드라이런 일때 조건 추가
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            SeqHistory("Tray Unit Full", "Rework Tray Table", "Not Detected Tray");
                            NextSeq(20);
                            return;
                        }
                        else
                        {
                            if (MotionMgr.Inst.GetInput(IODF.INPUT.RW_TRAY_CHECK))
                            {
                                GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_READY] = true;

                                //TRAY_UNLOADING_REWORK 싸이클타임 측정시작
                                //TTDF.SetTact(TTDF.TRAY_UNLOADING_REWORK, true);
                            }
                            else
                            {
                                SeqHistory("Tray Unit Full", "Rework Tray Table", "Not Detected Tray");
                                NextSeq(20);
                                return;
                            }
                        }
                    }
                    else
                    {
                        SeqHistory("Tray Unit Full", "Rework Tray Table", "Not Full");
                        NextSeq(20);
                        return;
                    }
                    break;

                // 트레이 언로딩 준비 완료 대기
                case 13:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Unloading Ready", "Rework Tray Table", "Wait");

                        m_bFirstSeqStep = false;

                        if (!IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SetError((int)ERDF.E_WRONG_POS_UNLOADING_REWORK_TRAY_STAGE);
                            return;
                        }
                    }

                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_READY] == true) return;

                    SeqHistory("Unloading Ready", "Rework Tray Table", "Complete");

                    break;

                // 트레이 언로딩 완료 대기
                case 14:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Unloading Complete", "Rework Tray Table", "Wait");

                        m_bFirstSeqStep = false;

                        if (!IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SetError((int)ERDF.E_WRONG_POS_UNLOADING_REWORK_TRAY_STAGE);
                            return;
                        }
                    }

                    if (!GbVar.mcState.IsCycleRun(SEQ_ID.TRAY_TRANSFER))
                    {
                        if (!LeaveCycle()) return;
                    }

                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_COMPLETE] == false) return;

                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNLOADING_COMPLETE] = false;

                    SeqHistory("Unloading Complete", "Rework Tray Table", "Complete");

                    //TRAY_UNLOADING_REWORK 싸이클타임 측정종료
                    //TTDF.SetTact(TTDF.TRAY_UNLOADING_REWORK, false);
                    break;

                // LOT END 시 트레이 투입 안하고 대기
                // 일반 상황 시 트레이 투입
                case 20:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Lot End Run", "Rework Tray Table", "Wait");

                        m_bFirstSeqStep = false;
                    }

                    // 공급중단 Var 변수 true이면 return;
                    if (GbVar.Seq.bIsLotEndRun)
                    {
                        LeaveCycle();
                        return;
                    }

                    if (!IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING))
                    {
                        SetError((int)ERDF.E_WRONG_POS_LOADING_REWORK_TRAY_STAGE);
                        return;
                    }

                    SeqHistory("Lot End Run", "Rework Tray Table", "Start");
                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_READY] = true;

                    //TRAY_LOADING_REWORK 싸이클타임 측정시작
                    //TTDF.SetTact(TTDF.TRAY_LOADING_REWORK, true);
                    break;

                // 트레이 로딩 준비 대기
                case 21:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Loading Ready", "Rework Tray Table", "Wait");

                        m_bFirstSeqStep = false;

                        if (!IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SetError((int)ERDF.E_WRONG_POS_LOADING_REWORK_TRAY_STAGE);
                            return;
                        }
                    }

                    if (!GbVar.mcState.IsCycleRun(SEQ_ID.TRAY_TRANSFER))
                    {
                        if (!LeaveCycle()) return;
                    }

                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_READY] == true) return;

                    SeqHistory("Loading Ready", "Rework Tray Table", "Complete");
                    break;

                // 트레이 로딩 완료 대기
                case 22:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Loading Complete", "Rework Tray Table", "Wait");

                        m_bFirstSeqStep = false;

                        if (!IsInPosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SetError((int)ERDF.E_WRONG_POS_LOADING_REWORK_TRAY_STAGE);
                            return;
                        }
                    }

                    if (!GbVar.mcState.IsCycleRun(SEQ_ID.TRAY_TRANSFER))
                    {
                        if (!LeaveCycle()) return;
                    }

                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_COMPLETE] == false) return;

                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].REWORK_TABLE_TRAY_WORK_COUNT++;

                    GbVar.Seq.sUldRWTrayTable.Info.Clear();
                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_LOADING_COMPLETE] = false;

                    SeqHistory("Loading Complete", "Rework Tray Table", "Complete");

                    //TRAY_LOADING_REWORK 싸이클타임 측정종료
                    //TTDF.SetTact(TTDF.TRAY_LOADING_REWORK, false);
                    break;


                //220511 pjh
                case 27:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray Align", "Rework Tray Table", "Start");

                            m_bFirstSeqStep = false;
                        }

                        nFuncResult = cycFunc.TrayAlign();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Tray Align", "Rework Tray Table", "Complete");
                        }
                    }
                    break;

                // 트레이 작업 위치 이동
                case 30:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Working P1 Move", "Rework Table Y Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosRwTrayStgY(POSDF.TRAY_STAGE_TRAY_WORKING_P1);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        GbVar.Seq.sUldRWTrayTable.nUnitPlaceCount = 0;
                        GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_READY] = true;

                        SeqHistory("Tray Working P1 Move", "Rework Table Y Axis", "Done");
                    }
                    break;


                case 32:
                    //if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_READY] == true) return;

                    GbVar.Seq.sUldRWTrayTable.ProcCycleStart(0);
                    break;

                // 트레이 만재 대기
                case 40:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Tray Table PLACE FULL CHECK", "Rework Table Wait Complete", "Wait");

                        m_bFirstSeqStep = false;
                    }

                    if (LeaveCycle() == false) return;

                    // LOT END 체크
                    if (GbVar.Seq.bIsLotEndRun)
                    {
                        GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] = true;
                        SeqHistory("Tray Table PLACE FULL CHECK", "Rework Table Wait Complete", "Lot End Complete");
                    }
                    // Rework 트레이 배출
                    else if (GbVar.Seq.bReworkTrayOut)
                    {
                        // 소터에 자재가 없을 경우에만 배출
                        //if (GbFunc.IsLotEndCondition(true, true))
                        if (GbFunc.IsPnpNoStrip(true))
                        {
                            GbVar.Seq.bReworkTrayOut = false;

                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] = true;
                            SeqHistory("Tray Table PLACE FULL CHECK", "Rework Table Wait Complete", "REWORK TRAY OUT FLAG DETECT");
                        }
                    }

                    // Tray Unit 만재?
                    if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] == false)
                    {
                        if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_NOT_EXIST])
                        {
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_NOT_EXIST] = false;
                            NextSeq(0);
                            SeqHistory("Tray table sequence init cause no tray on the table", "RW TRAY NOT EXIST", "RW TRAY NOT EXIST");
                            return;
                        }
                        return;
                    }

                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_READY] = false;

                    SeqHistory("Tray Table PLACE FULL CHECK", "Rework Table Wait Complete", "Complete");
                    GbVar.Seq.sUldRWTrayTable.ProcCycleEnd(0);
                    break;

                case 50:
                    SeqHistory("-----------", "Rework Tray Table", "End !!");
                    NextSeq(0);
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

        bool IsExistTray()
        {
            return MotionMgr.Inst.GetInput(IODF.INPUT.RW_TRAY_CHECK);
        }
    }
}
