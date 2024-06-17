using NSK_8000S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSK_8000S.SEQ.AUTO
{
    public class procLdMzULDTransfer : SeqBase
    {
        cycLdMzTransfer cycFunc = null;

        public procLdMzULDTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLdMzLoading;

            cycFunc = new cycLdMzTransfer(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqLdMzUnloading.SEQ_LD_MZ_UNLOADING seqNo)
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
            if (!IsAcceptAutoRun()) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_seqInfo != null)
            {
                if (m_seqInfo != GbVar.Seq.sLdMzLoading)
                {
                    m_seqInfo = GbVar.Seq.sLdMzLoading;
                    m_nSeqNo = GbVar.Seq.sLdMzLoading.nCurrentSeqNo;
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

            switch ((seqLdMzUnloading.SEQ_LD_MZ_UNLOADING)m_nSeqNo)
            {
                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.INIT:
                    //
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.IS_MGZ_UNLOAD_REQ:
                    if (!GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_REQ] == true) return;

                    SeqHistory("-----------", "Uld Mgz Elv", "Start !!");
                        
                    break;

                // Unloading Cycle
                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.CYC_MGZ_UNLOADING_START:
                    if (GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.WORK_MGZ_LOADING_RUN] == true) return;

                    GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_REQ] = false;
                    GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_RUN] = true;
                    GbVar.Seq.sLdMzLoading.ProcCycleStart(10);
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.CYC_MGZ_CONV_LOAD_3F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Load mgz from 3F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzUnloadingFrom3F();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Load mgz from 3F conv", "Uld Mgz Elv", "Done");
                        
                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(10);
                        GbVar.Seq.sLdMzLoading.ProcCycleStart(11);
                    }

                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.CYC_MGZ_MOVE_TO_4F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move mgz to 4F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzElvMoveToUnloading();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move mgz to 4F conv", "Uld Mgz Elv", "Done");

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(11);
                        GbVar.Seq.sLdMzLoading.ProcCycleStart(12);
                    }

                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.CYC_MGZ_CONV_UNLOAD_4F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Unload mgz to 4F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzUnloadingTo4F();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Unload mgz to 4F conv", "Uld Mgz Elv", "Done");

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(12);
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                        {
                            IFMgr.Inst.MES.UpdateMgzStatus(MESDF.eML_PORT.ML04, GbVar.Seq.sLdMzLoading.MGZ_ID, true);
                            IFMgr.Inst.MES.UpdateMgzStatus(MESDF.eML_PORT.ML03, "", false);
                        }
                    }
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.OHT_MGZ_UNLOAD_INTERFACE:
                    GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_RUN] = false;
                    //GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.OHT_MGZ_UNLOADING_REQ] = true;
                    GbVar.Seq.sLdMzUnloading.nElvMzUnloadCount++;
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.FINISH:
                    SeqHistory("Move to Init", "Uld Mgz Elv", "Finish");
                    NextSeq((int)seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.INIT);
                    return;
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
