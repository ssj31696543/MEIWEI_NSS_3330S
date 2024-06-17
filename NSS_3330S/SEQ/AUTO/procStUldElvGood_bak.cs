using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.SEQ.CYCLE;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldElvGood : SeqBase
    {
        int m_nElvPorNum = 0;

        double dSearchedCurrPos = 0.0;
        double dWorkingReadyPos = 0.0;

        double dElvOffset;
        double dElvOffsetMoveVel;

         cycUldElevator cycFunc = null;

        public procStUldElvGood(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_nElvPorNum = nSeqID - (int)SEQ_ID.ULD_ELV_GOOD_1;
            m_seqInfo = GbVar.Seq.sUldTrayElvGood[m_nElvPorNum];

            cycFunc = new cycUldElevator(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD seqNo)
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
                if (m_seqInfo != GbVar.Seq.sUldTrayElvGood[m_nElvPorNum])
                {
                    m_seqInfo = GbVar.Seq.sUldTrayElvGood[m_nElvPorNum];
                    m_nSeqNo = GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].nCurrentSeqNo;
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

            switch ((seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD)m_nSeqNo)
            {
                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.INIT:
                    //if (GbVar.LOADER_TEST)
                    //    return;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Good Tray", "Start !!");

                    }
                    GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].nTrayInCount = 0;

                    // 랏엔드 신호가 켜져있으면 초기화
                    if (GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] == true)
                    {
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] = false;
                        SeqHistory("-----------", "Good Tray", "Init LotEnd !!");
                    }
                    if (GbVar.Seq.bIsLotEndRun)
                    {
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK + (m_nElvPorNum * 8)] == 0)
                        {
                            LeaveCycle();
                            return;
                        }
                        else
                        {
                            if (GbVar.IO[IODF.INPUT.GD_TRAY_1_CHECK + m_nElvPorNum * 7] == 1)
                            {
                                break;
                            }
                        }
                       
                        LeaveCycle();
                        return;
                    }
                    break;

                // 만약 StackerCylinder 위에 Tray가 존재하는지 확인 후 작업 필요...

                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.TRAY_LOAD_READY:
                    nFuncResult = cycFunc.TrayLoadReady(ref dSearchedCurrPos, ref dWorkingReadyPos);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].SetProcReady();
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = true;

                        SeqHistory("Stacker Down", "Good Tray Elevator", "Complete");
                    }
                    break;

                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.WAIT_TRAY_IN_TRANSFER:
                    if (LeaveCycle() == false) return;

                    if (GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] || 
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].nTrayInCount >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OUT_MAX_COUNT].nValue ||
                        GbVar.bTrayOutReq[m_nElvPorNum] == true)
                    {
                        // [2022.06.06.kmlee] 트레이 트랜스퍼가 내려놓으려고 하면 대기
                        if (!GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_TRANSFER_PLACE_PROCESS])
                        {
                            GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;
                            SeqHistory("Lot End Checked", String.Format("Good {0} Tray Elevator", m_nElvPorNum + 1), "Checked");
                            NextSeq(seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.TRAY_UNLOAD);
                            return;
                        }
                    }

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check IF Tray In Comp", "Good Tray", "Wait");
                    }
                    if (!GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_COMP]) return;

                    //220510 pjh
                    //트레이 트렌스퍼가 내려놓고 바로 다음 동작할 수있도록 TrayLoad후 false해주는 것이 아니라
                    //여기서 바로 false하도록 수정
                    GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_COMP] = false;
                    GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = false;

                    GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].ProcCycleStart(0);
                    SeqHistory("IF Tray In Comp", "Good Tray Elevator", "Complete");
                    break;


                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.TRAY_LOAD:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Working Ready Offset Move", "Good Tray Z Axis", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.TrayLoad(ref dSearchedCurrPos, ref dWorkingReadyPos);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].ProcCycleEnd(0);
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].nTrayInCount++;
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].SetProcReady();
                        GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.TRAY_IN_READY] = true;

                        SeqHistory("Working Ready Offset Move", "Good Tray Z Axis", "Done");

                        NextSeq((int)seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.WAIT_TRAY_IN_TRANSFER);
                        return;
                    }
                    break;

                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.TRAY_UNLOAD:
                    if (m_bFirstSeqStep)
                    {
                    }

                    nFuncResult = cycFunc.TrayUnload(ref dSearchedCurrPos, ref dWorkingReadyPos);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        if (GbVar.bTrayOutReq[m_nElvPorNum] == true)
                            GbVar.bTrayOutReq[m_nElvPorNum] = false;

                        if (GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] == true)
                        {
                            GbVar.Seq.sUldTrayElvGood[m_nElvPorNum].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] = false;
                            SeqHistory("-----------", "Good Tray", "LotEnd !!");
                        }
                    }
                    break;


                case seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.FINISH:
                    SeqHistory("-----------", "Good Tray", "End !!");
                    NextSeq((int)seqUldTrayElvGood.SEQ_SOTER_ULD_ELV_GOOD.INIT);
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
