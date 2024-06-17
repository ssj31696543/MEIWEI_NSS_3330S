using NSK_8000S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSK_8000S.SEQ.AUTO
{
    public class procLdMzLDTransfer : SeqBase
    {
        cycLdMzTransfer cycFunc = null;

        public procLdMzLDTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLdMzLoading;

            cycFunc = new cycLdMzTransfer(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING seqNo)
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

            switch ((seqLdMzLoading.SEQ_LD_MZ_LOADING)m_nSeqNo)
            {
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT:
                    //
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.MZ_LOADING_COUNT_CHECK:
                    if (GbVar.Seq.sLdMzLoading.nElvMzLoadCount > 3) return;
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.OHT_MGZ_LOAD_CHECK:
                    if (GbVar.Seq.sLdMzLoading.bIsLoadStop) return;

                    break;

                // Loading Cycle 
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CYC_MGZ_LOADING_START:

                    GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.OHT_MGZ_LOADING_REQ] = true;
                    // 해제시점은 OHT Interface 시작시점

                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.IS_ARRIVED_MGZ:
                    //20211130 choh : cycle stop 추가하기
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Mgz arrive check", "Uld Mgz Elv", "Wait");

                        m_bFirstSeqStep = false;
                    }

                    if (GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.OHT_MGZ_LOADING_COMPLETED] != true) return;

                    GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.OHT_MGZ_LOADING_COMPLETED] = false;
                    GbVar.Seq.sLdMzLoading.ProcCycleStart(0);

                    SeqHistory("Mgz arrive check", "Uld Mgz Elv", "Done");
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.IS_ULD_ELV_MGZ_EXIST:
                    if (GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_REQ] == true) return;
                    
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.IS_ULD_ELV_MGZ_UNLOADING:
                    if (GbVar.Seq.sLdMzUnloading.bSeqIfVar[seqLdMzUnloading.EMPTY_MGZ_UNLOADING_RUN] == true) return;
                    GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.WORK_MGZ_LOADING_RUN] = true;
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CYC_MGZ_RFID_READ_5F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Mgz align and read rfid", "Uld Mgz Elv Conv 5F", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzRFID_Read();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Mgz align and read rfid", "Uld Mgz Elv Conv 5F", "Done");

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                        {
                            SeqHistory("MES - Mgz Supply", "Uld Mgz Elv Conv 5F", "Requset");

                            IFMgr.Inst.MES.UpdateMgzStatus(MESDF.eML_PORT.ML01, GbVar.Seq.sLdMzLoading.MGZ_ID, true);
                            IFMgr.Inst.MES.MgzLdRep.ReplyMgzSupply(GbVar.Seq.sLdMzLoading.MGZ_ID);//상위에 도착보고
                        }

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(0);
                        GbVar.Seq.sLdMzLoading.ProcCycleStart(1);
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CYC_MGZ_CONV_LOAD_5F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Loading mgz from 5F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzLoadingFrom5F();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Loading mgz from 5F conv", "Uld Mgz Elv", "Done");

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(1);
                        GbVar.Seq.sLdMzLoading.ProcCycleStart(2);
                    }

                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CYC_MGZ_MOVE_TO_2F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move mgz to 2F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzElvMoveToLotStart2F();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move mgz to 2F conv", "Uld Mgz Elv", "Done");

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(2);
                        GbVar.Seq.sLdMzLoading.ProcCycleStart(3);
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CYC_MGZ_CONV_UNLOAD_2F:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Unload mgz to 2F conv", "Uld Mgz Elv", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.MgzLoadToLotStart();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Unload mgz to 2F conv", "Uld Mgz Elv", "Done");

                        GbVar.Seq.sLdMzLoading.ProcCycleEnd(3);
                        GbVar.Seq.sLdMzLoading.nElvMzLoadCount++;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                            IFMgr.Inst.MES.UpdateMgzStatus(MESDF.eML_PORT.ML01, "", false);

                        //매거진 로딩 싸이클타임 측정시작
                        GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.WORK_MGZ_LOADING_RUN] = false;
                    }

                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.FINISH:
                    SeqHistory("Move to Init", "Uld Mgz Elv", "Finish");
                    NextSeq((int)seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT);
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
