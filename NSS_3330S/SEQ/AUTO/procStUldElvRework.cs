using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.SEQ.CYCLE;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldElvRework : SeqBase
    {
        double dSearchedCurrPos = 0.0;
        double dWorkingReadyPos = 0.0;

        double dElvOffset;
        double dElvOffsetMoveVel;

        cycUldElevator cycFunc = null;

        public procStUldElvRework(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldTrayElvRework;

            cycFunc = new cycUldElevator(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }
        void NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK seqNo)
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
                if (m_seqInfo != GbVar.Seq.sUldTrayElvRework)
                {
                    m_seqInfo = GbVar.Seq.sUldTrayElvRework;
                    m_nSeqNo = GbVar.Seq.sUldTrayElvRework.nCurrentSeqNo;
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

            switch ((seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK)m_nSeqNo)
            {
                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.INIT:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Rework Tray", "Start !!");

                        m_bFirstSeqStep = false;
                    }
                    GbVar.Seq.sUldTrayElvRework.nTrayInCount = 0;

                    if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] == true)
                    {
                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] = false;
                        SeqHistory("-----------", "Rework Tray", "Init LotEnd !!");
                    }

                    if (GbVar.Seq.bIsLotEndRun)
                    {
                        LeaveCycle();
                        return;
                    }
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_LOAD_READY:
                    nFuncResult = cycFunc.TrayLoadReady(ref dSearchedCurrPos, ref dWorkingReadyPos);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        GbVar.Seq.sUldTrayElvRework.SetProcReady();
                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = true;

                        SeqHistory("Stacker Down", "Rework Tray Elevator", "Complete");
                    }
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.WAIT_TRAY_IN_TRANSFER:
                    if (LeaveCycle() == false) return;

                    if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] ||
                        GbVar.Seq.sUldTrayElvRework.nTrayInCount >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT].nValue ||
                        GbVar.bTrayOutReq[MCDF.ELV_TRAY_REWORK] == true)
                    {
                        // [2022.06.06.kmlee] 트레이 트랜스퍼가 내려놓으려고 하면 대기
                        if (!GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_TRANSFER_PLACE_PROCESS])
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.USE_ELV_COVER_UNLOAD].bOptionUse)
                            {
                                if (!GbVar.Seq.sUldRWTrayTable.bTrayExist)
                                {
                                    if(GbVar.Seq.sUldTrayElvRework.nTrayInCount == 0)
                                    {
                                        NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.FINISH);
                                        return;
                                    }

                                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_READY] = true;
                                    SeqHistory("Lot End Checked", "Rework Tray Elevator", "Checked");
                                    NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.WAIT_TRAY_COVER);
                                    return;
                                }
                                else
                                {
                                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = false;
                                    SeqHistory("Lot End Checked", String.Format("Rework Tray Elevator"), "Checked");
                                    NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_UNLOAD);
                                    return;
                                }
                            }
                            else
                            {
                                GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = false;
                                SeqHistory("Lot End Checked", "Rework Tray Elevator", "Checked");
                                NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_UNLOAD);
                                return;
                            }
                        }
                    }

                    SeqHistory("Check Tray In Comp", "Rework Tray Elevator", "Wait");

                    if (!GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_COMP])
                    {
                        if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_EXIST])
                        {
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_EXIST] = false;
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_COMP] = true;
                        }
                        return;
                    }
                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_COMP] = false;
                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = false;

                    GbVar.Seq.sUldTrayElvRework.ProcCycleStart(0);
                    // 여기서 LotEnd인지 확인을 해야 함...

                    //SeqHistory(string.Format("ELAPSED, {0}, {1}", "REWORK TRAY WAIT TRAY PICKER WORK DONE COMPLETE", STEP_ELAPSED));
                    SeqHistory("Tray In Comp", "Rework Tray Elevator", "Complete");
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_LOAD:
                    nFuncResult = cycFunc.TrayLoad(ref dSearchedCurrPos, ref dWorkingReadyPos);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dWorkingReadyPos = MotionMgr.Inst[(int)SVDF.AXES.RW_TRAY_ELV_Z].GetRealPos();

                        GbVar.Seq.sUldTrayElvRework.ProcCycleEnd(0);
                        GbVar.Seq.sUldTrayElvRework.nTrayInCount++;
                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_IN_READY] = true;
                        NextSeq(seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_LOAD_READY);

                        SeqHistory("Safety Offset Move", "Rework Tray Z Axis", "Done");
                        return;
                    }
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.WAIT_TRAY_COVER:
                    if (LeaveCycle() == false)
                        return;

                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        SeqHistory("WAIT FOR COVER TRAY LOAD", "Good Tray Elevator", "Start");
                    }

                    if (!GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_COMP])
                        return;

                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.TRAY_COVER_COMP] = false;
                    SeqHistory("WAIT FOR COVER TRAY LOAD", "Good Tray Elevator", "Done");
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_UNLOAD:
                    nFuncResult = cycFunc.TrayUnload(ref dSearchedCurrPos, ref dWorkingReadyPos);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", "REWORK TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", STEP_ELAPSED));
                        SeqHistory("Ready Move", "Rework Tray Z Axis", "Done");
                    }
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.TRAY_OUT:
                    nFuncResult = cycFunc.TrayOut();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        if (GbVar.bTrayOutReq[MCDF.ELV_TRAY_REWORK] == true)
                            GbVar.bTrayOutReq[MCDF.ELV_TRAY_REWORK] = false;

                        if (GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] == true)
                        {
                            GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] = false;
                            SeqHistory("-----------", "Rework Tray", "LotEnd !!");
                        }
                    }
                    break;

                case seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.FINISH:
                    SeqHistory("-----------", "Rework Tray", "End !!");
                    NextSeq((int)seqUldTrayElvRework.SEQ_SOTER_ULD_ELV_REWORK.INIT);
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
