using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procUnitDry : SeqBase
    {
        cycUnitDry cycFunc = null;

        int m_nRepickCount = 0;
        public procUnitDry(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUnitDry;

            cycFunc = new cycUnitDry();
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY seqNo)
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

            if (m_nSeqNo != m_nPreSeqNo)
            {
                m_bFirstSeqStep = true;
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

            switch ((seqUnitDry.SEQ_SOTER_UNIT_DRY)m_nSeqNo)
            {
                case seqUnitDry.SEQ_SOTER_UNIT_DRY.NONE:
                    if (GbVar.LOADER_TEST)
                        return;
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.INIT:
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS:
                    if (m_bFirstSeqStep)
                    {
                        
                    }
                    nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //초기화
                        //221025 HEP
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] = false;
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_UNLOADING_READY] = false;
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = false;

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK X MOVE TO READY", STEP_ELAPSED));
                    }
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.CHECK_UNIT_BTM_DRY_REQ:
                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] == false) return;
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.START_UNIT_BTM_DRY:
                    nFuncResult = cycFunc.BtmAirDry();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] = false;
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK BTM DRY FINISH", STEP_ELAPSED));

                        if (GbVar.Seq.sUnitTransfer.Info.IsStrip() == false)
                        {
                            NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS);
                            return;
                        }
                    }
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.LOAD_POS_MOVE_X_AXIS:
                    nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] = true;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_COMPLETE] = false;

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK X MOVE TO STRIP LOADING POS", STEP_ELAPSED));
                    }
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.WAIT_UNIT_PICKER_UNLOAD:

                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_RUN] == true ||
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_COMPLETE] == false)
                    {
                        if (GbVar.Seq.sUnitTransfer.Info.IsStrip() == false)
                        {
                            NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS);
                            return;
                        }
                        return;
                    }
                    GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = true;

                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER UNLOAD FINISH!!", STEP_ELAPSED));
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.START_UNIT_TOP_DRY:
                    if (m_bFirstSeqStep)
                    {
                        //스폰지 클린을 안하면 인터락 체크 안함
                        if (RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount <= 0)
                        {
                            GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_SPONGE_CLEAN] = false;
                        }
                        if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_SPONGE_CLEAN]) return;
                    }
                    nFuncResult = cycFunc.TopAirDry();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //221021 소재 없으면
                        if (GbVar.Seq.sUnitDry.Info.IsStrip() == false)
                        {
                            GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = false;
                            m_nRepickCount = 0;

                            NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS);
                            return;
                        }

                        //221021 리픽 체크
                        m_nRepickCount++;
                        if (RecipeMgr.Inst.Rcp.cleaning.nRepickCount == 0 ||
                            RecipeMgr.Inst.Rcp.cleaning.nRepickCount < m_nRepickCount)
                        {
                            GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = false;
                            m_nRepickCount = 0;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK TOP DRY FINISH", STEP_ELAPSED));
                            NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.UNLOAD_POS_MOVE_X_AXIS);
                            return;
                        }
                      
                    }
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.REPICK_STEP:
                    {
                        nFuncResult = cycFunc.LoadingToDryStage();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER RE-PICK", STEP_ELAPSED));

                            GbVar.Seq.sUnitDry.DataShiftDryBlockToTransfer();

                            //221021 소재 없으면
                            if (GbVar.Seq.sUnitTransfer.Info.IsStrip() == false)
                            {
                                GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = false;
                                m_nRepickCount = 0;

                                NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS);
                                return;
                            }
                            //픽업했으면 다시 하부 드라이
                            NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.START_UNIT_BTM_DRY);
                            return;
                        }
                    }
                    break;
                case seqUnitDry.SEQ_SOTER_UNIT_DRY.UNLOAD_POS_MOVE_X_AXIS:
                    nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_UNLOADING_READY] = true;
                        GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_COMPLETE] = false;
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK X MOVE TO STRIP UNLOADING POS", STEP_ELAPSED));
                    }
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.WAIT_MAP_TRANSFER_LOAD:
                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sUnitDry.Info.IsStrip() == false)
                    {
                        NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.READY_POS_MOVE_X_AXIS);
                        return;
                    }

                    if (GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_RUN] == true ||
                        GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_COMPLETE] == false) return;

                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER LOAD FINISH!!", STEP_ELAPSED));
                    break;

                case seqUnitDry.SEQ_SOTER_UNIT_DRY.FINISH:
                    NextSeq(seqUnitDry.SEQ_SOTER_UNIT_DRY.INIT);
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
