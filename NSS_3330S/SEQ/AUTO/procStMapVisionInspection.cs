using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSK_8000S.SEQ.AUTO
{
    public class procStMapVisionInspection : SeqBase
    {
        public procStMapVisionInspection(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLoading;

            //cycleFunction = new cycLoading(nSeqID);
            //cycleFunction.SetErrorFunc(SetError);
            //cycleFunction.SetAddMsgFunc(SetProcMsgEvent);
            //cycleFunction.SetAutoManualMode(true);

            //m_cycleInfo = cycleFunction;
        }

        void NextSeq(SEQ_SOTER_MAP_VISION_INSPECTION seqNo)
        {
            NextSeq((int)seqNo);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            //cycleFunction.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            //cycleFunction.InitSeq();
        }

        protected override void SeqHistory(string strHistory)
        {
            //SeqHistory(strHistory, m_nSeqNo, cycleFunction.SEQ_NO);
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

            switch ((SEQ_SOTER_MAP_VISION_INSPECTION)m_nSeqNo)
            {

                case SEQ_SOTER_MAP_VISION_INSPECTION.FINISH:
                    // JIG LOAD COMPLETED
                    GbVar.Seq.sLoading.SetJig();

                    GbVar.Seq.sLoading.ProcEnd();
                    //GbVar.Seq.sLoading.ProcTimeStop((int)GbVar.TACT_SEQ_UNIT_IDX.LOADING, 0);

                    GbVar.Seq.sLoading.nCurrentSlotNo++;
                    NextSeq((int)SEQ_SOTER_MAP_VISION_INSPECTION.NONE);
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
