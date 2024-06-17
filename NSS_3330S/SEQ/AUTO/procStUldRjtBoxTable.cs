using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUldRjtBoxTable : SeqBase
    {
        public procStUldRjtBoxTable(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldRjtBoxTable;
        }


        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        public override void ResetCmd()
        {
            base.ResetCmd();
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
                if (m_seqInfo != GbVar.Seq.sUldRjtBoxTable)
                {
                    m_seqInfo = GbVar.Seq.sUldRjtBoxTable;
                    m_nSeqNo = GbVar.Seq.sUldRjtBoxTable.nCurrentSeqNo;
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
                    if (GbVar.LOADER_TEST)
                        return;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("-----------", "Reject Box", "Start !!");

                        m_bFirstSeqStep = false;
                    }
                    break;

                case 10:
                    //
                    break;

                case 20:
                    //GbVar.Seq.sUldRjtBoxTable.bTrayLoadReq = true;
                    //Sensor Check
                    
                    break;

                case 22:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Signal Detect", "Reject Box", "Wait");

                        m_bFirstSeqStep = false;
                    }
                    if (GbVar.Seq.sUldRjtBoxTable.bIsBoxFull == true) return;

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Check Signal Detect", "Reject Box", "Complete");
                    }
                    break;

                case 30:
                    //
                    break;

                case 32:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Box Load", "Reject Box", "Wait");

                        m_bFirstSeqStep = false;
                    }
                    //20211130 choh : cycle stop 추가하기

                    if (GbVar.Seq.sUldRjtBoxTable.bIsBoxLoad == true) return;

                    m_seqInfo.ProcCycleStart(0);
                    SeqHistory("Box Load", "Reject Box Y", "Complete");
                    break;

                case 40:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Check Box Full", "Reject Box", "Wait");

                        m_bFirstSeqStep = false;
                    }
                    // Tray Unit 만재?
                    if (GbVar.Seq.sUldRjtBoxTable.bIsBoxFull == false) return;

                    m_seqInfo.ProcCycleEnd(0);
                    SeqHistory("Box Full", "Reject Box", "Complete");
                    break;


                case 50:
                    SeqHistory("-----------", "Reject Box", "End !!");
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
