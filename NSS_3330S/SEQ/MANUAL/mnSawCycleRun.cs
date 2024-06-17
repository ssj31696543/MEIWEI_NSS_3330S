using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnSawCycleRun : SeqBase
    {
        int m_nStartSeqNo = 0;
        SEQ_ID m_seqID = SEQ_ID.STRIP_TRANSFER;

        bool m_bVacSkip = true;
        object[] m_args = null;

        public cycStripTransfer m_cycStripTransfer = null;
        public cycUnitTransfer m_cycUnitTransfer = null;
        
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

        public mnSawCycleRun()
        {
            m_cycStripTransfer = new cycStripTransfer((int)SEQ_ID.STRIP_TRANSFER, 0);
            m_cycUnitTransfer = new cycUnitTransfer((int)SEQ_ID.UNIT_TRANSFER, 0);

            m_cycStripTransfer.SetAutoManualMode(false);
            m_cycUnitTransfer.SetAutoManualMode(false);

            m_cycStripTransfer.SetAddMsgFunc(SetProcMsgEvent);
            m_cycUnitTransfer.SetAddMsgFunc(SetProcMsgEvent);
        }

        public void SetParam(int nStartSeqNo, params object[] args)
        {
            m_nStartSeqNo = nStartSeqNo;
            m_bVacSkip = !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse;
            m_args = args;

            #region Set Parameter
            switch (m_nStartSeqNo)
            {
                // STRIP PUSH AND BARCODE
                case 10:
                case 11:
                case 12:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP LOADING
                case 20:
                case 21:
                case 22:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP PRESS AND VACUUM
                case 30:
                case 31:
                case 32:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP PRE-ALIGN
                case 40:
                case 41:
                case 42:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP ORIGIN CHECK
                case 50:
                case 51:
                case 52:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP 2D CODE READ
                case 60:
                case 61:
                case 62:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP TRANSFER LOADING
                case 70:
                case 71:
                case 72:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP TRANSFER UNLOADING
                case 80:
                case 81:
                case 82:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // STRIP TRANSFER RE-UNLOADING
                case 90:
                case 91:
                case 92:
                    m_cycStripTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.STRIP_TRANSFER;
                    break;

                // UNIT LOADING FROM TABLE
                case 100:
                case 101:
                case 102:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;

                // UNIT WATER CLEANING
                case 110:
                case 111:
                case 112:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;

                // UNIT UNLOADING TO DRY BLOCK
                case 120:
                case 121:
                case 122:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;

                // UNIT BRUSH
                case 130:
                case 131:
                case 132:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;

                // UNIT DRY AIR 
                case 140:
                case 141:
                case 142:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;

                // UNIT KIT AIR CLEANING
                case 150:
                case 151:
                case 152:
                    m_cycUnitTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNIT_TRANSFER;
                    break;
            }
            #endregion
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            m_cycStripTransfer.InitSeq();
            m_cycUnitTransfer.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            m_cycStripTransfer.InitSeq();
            m_cycUnitTransfer.InitSeq();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;
            if (FINISH) 
                return;

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

                // STRIP PUSH AND BARCODE
                case 10:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(11);
                    return;

                case 11:
                    nFuncResult = m_cycStripTransfer.StripPushAndBarcode();
                    break;
                case 12:
                    break;

                // STRIP LOADING
                case 20:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(21);
                    return;

                case 21:
                    nFuncResult = m_cycStripTransfer.StripRailLoading();
                    if(nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq(22);
                        return;
                    }
                    break;

                case 22:
                    nFuncResult = FNC.SUCCESS;
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq(23);
                        return;
                    }
                    break;
                case 23:
                    nFuncResult = m_cycStripTransfer.StripRailLoadingAndAlign();                  
                    break;

                // STRIP PRESS AND VACUUM
                case 30:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(31);
                    return;

                case 31:
                    nFuncResult = m_cycStripTransfer.StripPressAndVac();                  
                    return;
                case 32:
                    break;

                // STRIP PRE-ALIGN
                case 40:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(41);
                    return;

                case 41:
                    nFuncResult = m_cycStripTransfer.StripPreAlign();
                    break;
                case 42:
                    break;

                // STRIP ORIGIN CHECK
                case 50:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(51);
                    return;

                case 51:
                    //nFuncResult = m_cycStripTransfer.StripOrientMarkCheck();
                    break;
                case 52:
                    break;

                // STRIP 2D CODE READ
                case 60:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(61);
                    return;

                case 61:
                    nFuncResult = m_cycStripTransfer.StripReadBarcode();
                    break;
                case 62:
                    break;

                // STRIP TRANSFER LOADING
                case 70:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(71);
                    return;

                case 71:
                    nFuncResult = m_cycStripTransfer.LoadingRailStrip();
                    break;
                case 72:
                    break;

                // STRIP TRANSFER UNLOADING
                case 80:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(81);
                    return;

                case 81:
                    nFuncResult = m_cycStripTransfer.UnloadingToCuttingTable();
                    break;
                case 82:
                    break;

                // STRIP TRANSFER RE-UNLOADING
                case 90:
                    m_cycStripTransfer.InitSeq();
                    NextSeq(91);
                    return;

                case 91:
                    nFuncResult = m_cycStripTransfer.ReloadingToCuttingTable();
                    break;
                case 92:
                    break;

                // UNIT LOADING FROM TABLE
                case 100:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(101);
                    return;

                case 101:
                    nFuncResult = m_cycUnitTransfer.LoadingTableUnit();
                    break;
                case 102:
                    break;

                // UNIT CLEANING
                case 110:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(111);
                    return;

                case 111:
                    nFuncResult = m_cycUnitTransfer.WaterCleaning();
                    break;
                case 112:
                    break;

                // UNIT UNLOADING TO CLEANER
                case 120:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(121);
                    return;

                case 121:
                    nFuncResult = m_cycUnitTransfer.UnloadingToDryStage();
                    break;
                case 122:
                    break;

                // UNIT UNLOADING TO BRUSH
                case 130:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(131);
                    return;

                case 131:
                    nFuncResult = m_cycUnitTransfer.SpongeCleaning();
                    break;
                case 132:
                    break;

                // UNIT AIR DRY
                case 140:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(141);
                    return;

                case 141:
                    nFuncResult = m_cycUnitTransfer.AirDryUnit();
                    break;
                case 142:
                    break;

                // UNIT KIT AIR CLEAN
                case 150:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(151);
                    return;

                case 151:
                    nFuncResult = m_cycUnitTransfer.PickerKitAirCleaning();
                    break;
                case 152:
                    break;

                // UNIT AIR DRY
                case 180:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(181);
                    return;

                case 181:
                    nFuncResult = m_cycUnitTransfer.UnitPickerScrap1();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        NextSeq(190);
                        return;
                    }
                    break;
                case 182:
                    break;

                // UNIT AIR DRY
                case 190:
                    m_cycUnitTransfer.InitSeq();
                    NextSeq(191);
                    return;

                case 191:
                    nFuncResult = m_cycUnitTransfer.UnitPickerScrap2();
                    break;
                case 192:
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
