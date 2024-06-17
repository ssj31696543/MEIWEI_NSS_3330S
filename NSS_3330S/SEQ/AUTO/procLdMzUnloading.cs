using NSK_8000S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSK_8000S.SEQ.AUTO
{
    public class procLdMzUnloading : SeqBase
    {
        cycLdMzUnloading cycFunc = null;

        public procLdMzUnloading(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLdMzUnloading;

            cycFunc = new cycLdMzUnloading(nSeqID);
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

        protected override void SeqHistory(string strHistory)
        {
            SeqHistory(strHistory, m_nSeqNo, cycFunc.SEQ_NO);
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

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            if (m_seqInfo != null)
            {
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }

            m_bFirstSeqStep = false;

            switch ((seqLdMzUnloading.SEQ_LD_MZ_UNLOADING)m_nSeqNo)
            {
                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.INIT:
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.MGZ_UNLOAD_REQ:
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.IS_MGZ_LOADING_CYC_CHECK:
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.CYC_MGZ_UNLOADING:
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.OHT_MGZ_UNLOAD_INTERFACE:
                    break;

                case seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.FINISH:

                    NextSeq((int)seqLdMzUnloading.SEQ_LD_MZ_UNLOADING.NONE);
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
