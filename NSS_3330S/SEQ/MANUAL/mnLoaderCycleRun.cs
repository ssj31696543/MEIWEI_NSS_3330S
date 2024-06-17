using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.SEQ.CYCLE;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnLoaderCycleRun : SeqBase
    {
        int m_nStartSeqNo = 0;
        SEQ_ID m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;

        bool m_bVacSkip = true;
        object[] m_args = null;

        //public cycLdMzTransfer m_cycLdMzTransfer = null;
        //public cycLdMzLotStart m_cycLdMzLotStart = null;

        public cycLoader_MzTransfer m_cycLoaderMzTransfer = null;
        public cycLoader_LdConv m_cycLoader_LdConv = null;
        public cycLoader_UldConv m_cycLoader_UldConv = null;


        bool IS_VACUUM_CHECK
        {
            get
            {
                if (m_bVacSkip) return false;

                return true;
            }
        }

        public SEQ_ID CURRENT_SEQ_ID
        {
            get { return m_seqID; }
        }

        public mnLoaderCycleRun()
        {
            // 매거진 컨베이어
            m_cycLoader_LdConv = new cycLoader_LdConv((int)SEQ_ID.LD_MZ_LD_CONV);
            m_cycLoader_LdConv.SetAutoManualMode(false);
            m_cycLoader_LdConv.SetAddMsgFunc(SetProcMsgEvent);

            // 매거진 트랜스퍼
            m_cycLoaderMzTransfer = new cycLoader_MzTransfer((int)SEQ_ID.LD_MZ_ELV_TRANSFER);
            m_cycLoaderMzTransfer.SetAutoManualMode(false);
            m_cycLoaderMzTransfer.SetAddMsgFunc(SetProcMsgEvent);

            // 매거진 컨베이어 (2F)
            m_cycLoader_UldConv = new cycLoader_UldConv((int)SEQ_ID.LD_MZ_ULD_CONV);
            m_cycLoader_UldConv.SetAutoManualMode(false);
            m_cycLoader_UldConv.SetAddMsgFunc(SetProcMsgEvent);
        }

        public void SetParam(int nStartSeqNo, params object[] args)
        {
            m_nStartSeqNo = nStartSeqNo;
            m_bVacSkip = !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse;
            m_args = args;

            #region Set Parameter
            switch (m_nStartSeqNo)
            {
                // MGZ CLAMPING
                case 10:
                case 11:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.LD_MZ_LD_CONV;
                    break;

                case 12:
                case 13:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.LD_MZ_LD_CONV;
                    break;

                case 14:
                case 15:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.LD_MZ_LD_CONV;
                    break;

                // MGZ LOADING PORT 1
                case 20:
                case 21:
                case 22:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;
                    break;

                // MGZ LOADING PORT 2
                case 30:
                case 31:
                case 32:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;
                    break;

                // MGZ UNLOADING PORT 1
                case 40:
                case 41:
                case 42:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;
                    break;

                // MGZ UNLOADING PORT 2
                case 50:
                case 51:
                case 52:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;
                    break;

                // MGZ SUPPLY MOVE
                case 60:
                case 61:
                case 62:
                    m_cycLoaderMzTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ELV_TRANSFER;
                    break;


                // LOAD CONV 1 LOADING / UNLOADING         
                case 100:
                case 101:
                case 102:
                case 110:
                case 111:
                case 112:
                    m_cycLoader_LdConv.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_LD_CONV;
                    break;

                // LOAD CONV 2 LOADING / UNLOADING    
                case 120:
                case 121:
                case 122:
                case 130:
                case 131:
                case 132:
                    //m_cycLoader_LdConv[(int)MCDF.MZ_LD_CONV.LOAD_2_LEFT].SetManualModeParam(m_args);

                    //m_seqID = SEQ_ID.LD_MZ_LD_CONV_2;
                    break;

                // UNLOAD CONV 1 LOADING /UNLOADING 
                case 140:
                case 141:
                case 142:
                case 150:
                case 151:
                case 152:
                    m_cycLoader_UldConv.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ULD_CONV;
                    break;

                // UNLOAD CONV 2 LOADING / UNLOADING      
                case 160:
                case 161:
                case 162:
                case 170:
                case 171:
                case 172:
                    m_cycLoader_UldConv.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.LD_MZ_ULD_CONV;
                    break;

            }
            #endregion
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            //m_cycLdMzTransfer.InitSeq();
            //m_cycLdMzLotStart.InitSeq();

            m_cycLoaderMzTransfer.InitSeq();

            m_cycLoader_LdConv.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            //m_cycLdMzTransfer.InitSeq();
            //m_cycLdMzLotStart.InitSeq();

            m_cycLoaderMzTransfer.InitSeq();

            m_cycLoader_LdConv.InitSeq();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;
            if (FINISH) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        NextSeq(m_nStartSeqNo);
                        return;
                    }

                // MGZ CLAMPING
                case 10:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(11);
                    return;
                case 11:
                    nFuncResult = m_cycLoader_LdConv.Mz_Input_Conveyor();
                    break;

                case 12:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(13);
                    return;
                case 13:
                    nFuncResult = m_cycLoader_LdConv.Mz_Loading_Conveyor_1();
                    break;

                case 14:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(15);
                    return;
                case 15:
                    nFuncResult = m_cycLoader_LdConv.Mz_Loading_Conveyor_2();
                    break;

                // MGZ LOADING PORT 1
                case 20:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(21);
                    return;
                case 21:
                    nFuncResult = m_cycLoaderMzTransfer.MzLoadingMove();
                    break;

                case 22:
                    break;

                // MGZ LOADING PORT 2
                case 30:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(31);
                    return;
                case 31:
                    //nFuncResult = m_cycLoaderMzTransfer.MgzTransferLoading(MCDF.MZ_LD_CONV.LOAD_2_LEFT);
                    break;
                case 32:
                    break;

                // MGZ UNLOADING PORT 1
                case 40:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(41);
                    return;
                case 41:
                    nFuncResult = m_cycLoaderMzTransfer.MzUnloadingMove();
                    break;

                case 42:
                    break;

                // MGZ UNLOADING PORT 2
                case 50:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(51);
                    return;
                case 51:
                    //nFuncResult = m_cycLoaderMzTransfer.MgzTransferUnloading(MCDF.MZ_ULD_CONV.UNLOAD_2_LEFT);
                    break;

                // MGZ SUPPLY MOVE
                case 60:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(61);
                    return;
                case 61:
                    nFuncResult = m_cycLoaderMzTransfer.MgzTransferSupplyPos();
                    break;

                case 70:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(71);
                    return;
                    
                // 매거진 클립 체크 동작
                case 71:
                    nFuncResult = m_cycLoaderMzTransfer.MgzTransferMzClipCheckPos();
                    break;

                case 80:
                    m_cycLoaderMzTransfer.InitSeq();
                    NextSeq(81);
                    return;

                    // 매거진 대기 위치로 이동
                case 81:
                    nFuncResult = m_cycLoaderMzTransfer.MgzTransferReadyPos();
                    break;

                // LOAD CONV 1 LOADING  
                case 100:
                    m_cycLoader_LdConv.InitSeq();
                    NextSeq(101);
                    return;
                case 101:
                    //nFuncResult = m_cycLoader_LdConv.MzLoadingMove();
                    if(FNC.IsErr(nFuncResult))
                    {
                        LdConv_Stop();
                    }
                    break;

                // LOAD CONV 1 UNLOADING
                case 110:
                    m_cycLoader_LdConv.InitSeq();
                    NextSeq(111);
                    return;
                case 111:
                    nFuncResult = m_cycLoader_LdConv.MzUnloadingMove();
                    if (FNC.IsErr(nFuncResult))
                    {
                        LdConv_Stop();
                    }
                    break;

                // LOAD CONV 2 LOADING     
                case 120:
                    NextSeq(121);
                    return;
                case 121:
                    break;

                // LOAD CONV 2 UNLOADING     
                case 130:
                    NextSeq(131);
                    return;
                case 131:
                    break;

                // UNLOAD CONV 1 LOADING      
                case 140:
                    NextSeq(141);
                    return;
                case 141:
                    break;

                // UNLOAD CONV 1 UNLOADING     
                case 150:
                    NextSeq(151);
                    return;
                case 151:
                    break;

                case 152:
                    m_cycLoader_UldConv.InitSeq();
                    NextSeq(153);
                    return;
                case 153:
                    nFuncResult = m_cycLoader_UldConv.MzUnloadingRun_1();
                    break;

                case 154:
                    m_cycLoader_UldConv.InitSeq();
                    NextSeq(155);
                    return;
                case 155:
                    nFuncResult = m_cycLoader_UldConv.MzUnloadingRun_2();
                    break;

                // UNLOAD CONV 2 LOADING      
                case 160:
                    NextSeq(161);
                    return;
                case 161:
                    break;

                // UNLOAD CONV UNLOADING 2        
                case 170:
                    NextSeq(171);
                    return;
                case 171:
                    break;

                default:
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            // Cycle이 끝나면 종료
            FINISH = true;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
            m_nSeqNo++;
        }
    }
}
