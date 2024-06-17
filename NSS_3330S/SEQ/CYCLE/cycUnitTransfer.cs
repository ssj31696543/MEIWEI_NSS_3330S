using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycUnitTransfer : SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;
        int m_nLoadingTableNo = 0;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        int m_nSpongeCount = 0;

        int m_nWaterJetSwingCount = 0;

        int m_nCleanAirBlowingCount = 0;
        int m_nToolKitCleanAirCount = 0;

        int m_nTotalCleanerSwingCount = 0;

        int m_nScrap1Count = 0;
        int m_nScrap2Count = 0;
        #region PROPERTY
        public seqUnitTransfer SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sUnitTransfer; }
        }

        public bool IS_STRIP
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.Info.IsStrip();
                }

                return m_bIsStrip;
            }
        }
        #endregion

        public cycUnitTransfer(int nSeqID, int nMoveTableNo)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nLoadingTableNo = nMoveTableNo;
            m_seqInfo = GbVar.Seq.sStripTransfer;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            m_nLoadingTableNo = (int)args[0];
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }

        enum CYCLE_SCRAP
        {
            #region 스크랩 구간
            NONE = -1,
            INIT,
            CHECK_STRIP_EXIST,
            UNIT_PICKER_SCRAP_X_MOVE,
            UNIT_PICKER_SCRAP_Z_MOVE,
            SCRAP_BLOW_ON,
            WAIT_DELAY,
            UNIT_PICKER_READY_Z_MOVE,
            SCRAP_BLOW_OFF,
            CHECK_COUNT,
            FINISH,
            #endregion
        }

        #region INIT CYCLE
        public int InitCycle()
        {
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
                        m_nCurrentOrder = 0;
                    }
                    break;
                case 2:
                    {
                    }
                    break;
                case 20:
                    {

                    }
                    break;
                case 22:
                    {

                    }
                    break;
                case 24:
                    {

                    }
                    break;
                case 26:
                    {

                    }
                    break;
                case 28:
                    {

                    }
                    break;
                case 30:
                    {

                    }
                    break;
                case 40:
                    {
                        GbSeq.autoRun[SEQ_ID.UNIT_TRANSFER].InitSeq();
                        SEQ_INFO_CURRENT.Init();

                        return FNC.SUCCESS;
                    }
                //break;
                default:
                    break;
            }

            #region AFTER SWITCH

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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Loading

        public int LoadingTableUnit()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        IFMgr.Inst.SAW.ResetUnitTransferIF();
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "리셋 유닛 트랜스퍼 인터페이스", STEP_ELAPSED));

                        // [2022.05.15.kmlee] 이미 자재가 있을 경우 다음 시퀀스 진행
                        //                    아직은 적용하지말고 데이터 쉬프트 검증되면 확인
                        //if (SEQ_INFO_CURRENT.Info.IsStrip())
                        //{
                        //    return FNC.SUCCESS;
                        //}
                    }
                    break;
                case 2:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_SAW_NOT_READY;
                    }
                    if (!IFMgr.Inst.SAW.IsSawRun()) return FNC.BUSY;
                    break;
                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 8:
                    {
                        if (m_bAutoMode)
                        {
                            // 자재 정보가 있을 때만
                            if (GbVar.Seq.sCuttingTable.Info.IsStrip())
                            {
                                //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 시작 전 VAC 확인 후 ON 상태면 이미 자재가 있다고 판단 하고 처리 하자
                                // [2022.05.15.kmlee] 누군가가 VAC을 켜놓은 상태로 진행하면 이상 동작을 하므로 변경함
                                //                    Z축 올리는 부분만 생략하고 진행해도 이전 상태 그대로 가능하다 (마지막에 전부 끄므로)
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // X가 이미 Loading Position이라면 Z축을 올리지 않아도 됨
                                    if (IsInPosUnitPkX(POSDF.UNIT_PICKER_LOADING_TABLE_1))
                                    {
										SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER LOADING TABLE - ALREADY STRIP VAC , NEXT SEQ", STEP_ELAPSED));
                                        NextSeq(20);
                                        return FNC.BUSY;
                                    }
                                }
                            }
                            else
                            {
                                // 자재 정보가 없으면 자동으로 생성? 내지 알람
								// 일반 유닛과 다르게 Saw와 인터페이스를 하므로
                                // case 12번에서 처리
                            }
                        }
                    }
                    break;

                case 10:
                    // UNIT TRANSFER Z AXIS READY POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sCuttingTable.Info.IsStrip())
                            {
                                //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                {  
                                    // Vac On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                }

                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    // Blow On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                }

                                //STR VAC OFF
                                // 자재가 없어 Cycle 종료 로그
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER LOADING TABLE - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));

                                // 소우와 나머지 비트 처리 후 스크랩 위치로 이동 
                                NextSeq(46); // SetUnitTransferUnloadComplete
                                return FNC.BUSY;
                                //return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 20:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;


                    if (m_bAutoMode)
                    {
                        if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        {
                            return FNC.CYCLE_CHECK;
                        }
                    }

                    if (!IFMgr.Inst.SAW.IsTableUnloadingReq()) return FNC.BUSY;

                    break;

                case 22:
                    // UNIT TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_LOADING_TABLE_1))
                                break;
                        }

                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_LOADING_TABLE_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS TABLE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // UNIT TRANSFER X AXIS 이동 완료 신호 ON
                    {
                        IFMgr.Inst.SAW.SetUnitTransferXUnloadPos(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER UNLOAD POS SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 30:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (!IFMgr.Inst.SAW.IsTableUnloadingPos()) return FNC.BUSY;
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, ", "UNIT PICKER SAW UNLOAD POS SIGNAL ON CHECK COMPLETE",STEP_ELAPSED));

                    break;

                case 32:
                    // UNIT TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1))
                                break;
                        }

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    
                case 34:
                    // UNIT TRANSFER Z AXIS UNLOADING POS  MOVE
                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1))
                                break;
                        }

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    

                case 36:
                    // VACUUM ON
                    {
                        nFuncResult = UnitPickerWorkVac(true, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                        else if(FNC.IsErr(nFuncResult))
                        {
                            //베큠 알람일 경우 테이블 1번인지 2번인지 분기하기 위해 알람 번호를 바꿈
                            nFuncResult = (int)ERDF.E_UNIT_PK_VAC_NOT_ON_1;
                        }
                    }
                    break;
                    
                case 38:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)

                    //return FNC.BUSY;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    {
                        IFMgr.Inst.SAW.SetUnitTransferZUnloadPos(true);
                    }
                    break;

                case 40:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        // DELAY
                        {
                            if (WaitDelay(5000)) return FNC.BUSY;
                        }
                        break;
                    }
                    else
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                        {
                            InitSeq();
                            return (int)ERDF.E_UNIT_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                        }
                        // UP REQ?
                        if (!IFMgr.Inst.SAW.IsTableUnloadingBlowOn()) return FNC.BUSY;

                        break;
                    }

                case 42:
                    // VACUUM ON
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        nFuncResult = UnitPickerWorkVac(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            //베큠 알람일 경우 테이블 1번인지 2번인지 분기하기 위해 알람 번호를 바꿈
                            nFuncResult = (int)ERDF.E_UNIT_PK_VAC_NOT_ON_1;
                        }
                    }
                    break;
                case 44:                  
                    // UNIT TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    
                case 46:
                    // UNIT STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 48:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    IFMgr.Inst.SAW.SetUnitTransferUnloadComplete(true);
                    break;

                case 50:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        IFMgr.Inst.SAW.ResetUnitTransferIF();
                        break;
                    }
                    
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_UNIT_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }

                    // Interface Input이 Off되면 Ouput 모두 reset
                    if (IFMgr.Inst.SAW.IsTableUnloadingReq()) return FNC.BUSY;

                    IFMgr.Inst.SAW.ResetUnitTransferIF();
                    break;
                case 52:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_MODE_USE].bOptionUse)
                        //{
                        //    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SCRAP_1);

                        //    if (FNC.IsSuccess(nFuncResult))
                        //    {
                        //        //스폰지 끝나면 false
                        //        //GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nLoadingTableNo] = false;
                        //    }
                        //}
                        //else
                        //{
                        //    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_SWING_START);

                        //    if (FNC.IsSuccess(nFuncResult))
                        //    {
                        //        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nUnloadingTableNo] = false;
                        //    }
                        //}
                    }
                    break;
                case 54:
                    {

                    }
                    break;
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        //PRE DOWN 까지
        public int LoadingTableUnitLoadDown()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    IFMgr.Inst.SAW.ResetUnitTransferIF();
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 10:
                    // UNIT TRANSFER Z AXIS READY POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (!IFMgr.Inst.SAW.IsTableUnloadingReq()) return FNC.BUSY;

                    break;

                case 22:
                    // UNIT TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_LOADING_TABLE_1)) break;
                        }
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_LOADING_TABLE_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1},", "UNIT PICKER X AXIS TABLE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // UNIT TRANSFER X AXIS 이동 완료 신호 ON
                    {
                        IFMgr.Inst.SAW.SetUnitTransferXUnloadPos(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER UNLOAD POS SIGNAL ON COMPLETE",STEP_ELAPSED));
                    }
                    break;

                case 30:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (!IFMgr.Inst.SAW.IsTableUnloadingPos()) return FNC.BUSY;
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER SAW UNLOAD POS SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case 32:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        // UNIT TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                        {
                            //IFMgr.Inst.SAW.SetTransferZDownInterlock(LD_TABLE_NO, true);
                            double dOffset = 20;
                            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, -dOffset);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    }
                    else
                    {
                        // UNIT TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                            double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, -dOffset);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    }
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        //DOWN 좀더 하고 배큠
        public int LoadingTableUnitLoadVacuum()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    IFMgr.Inst.SAW.ResetUnitTransferIF();
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;
                case 34:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        break;
                    }
                    else
                    {
                        // UNIT TRANSFER Z AXIS UNLOADING POS  MOVE
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, 0.0, true);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    }

                case 36:
                    // VACUUM ON
                    {
                        nFuncResult = UnitPickerWorkVac(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 38:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)

                    //return FNC.BUSY;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    {
                        IFMgr.Inst.SAW.SetUnitTransferZUnloadPos(true);
                    }
                    break;

                case 40:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        // DELAY
                        {
                            if (WaitDelay(5000)) return FNC.BUSY;
                        }
                        break;
                    }
                    else
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                        {
                            InitSeq();
                            return (int)ERDF.E_UNIT_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                        }
                        // UP REQ?
                        if (!IFMgr.Inst.SAW.IsTableUnloadingBlowOn()) return FNC.BUSY;

                        break;
                    }
               
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int LoadingTableUnitLoadUp()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    IFMgr.Inst.SAW.ResetUnitTransferIF();
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 44:
                    {
                        //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                        // UNIT TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                        {
                            double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_LOADING_TABLE_1, -dOffset, true);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    }
                case 46:
                    // UNIT STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 48:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    IFMgr.Inst.SAW.SetUnitTransferUnloadComplete(true);
                    break;

                case 50:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        IFMgr.Inst.SAW.ResetUnitTransferIF();
                        break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_UNIT_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }

                    // Interface Input이 Off되면 Ouput 모두 reset
                    if (IFMgr.Inst.SAW.IsTableUnloadingReq()) return FNC.BUSY;

                    IFMgr.Inst.SAW.ResetUnitTransferIF();
                    break;
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Scrap

        public int UnitPickerScrap1()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SCRAP)m_nSeqNo)
            {
                case CYCLE_SCRAP.NONE:
                    break;
                case CYCLE_SCRAP.INIT:
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    //{
                    //    NextSeq((int)CYCLE_SCRAP.FINISH);
                    //    return FNC.BUSY;
                    //}

                    if (RecipeMgr.Inst.Rcp.cleaning.nScrapCount1 == 0)
                    {
                        NextSeq((int)CYCLE_SCRAP.FINISH);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_1, true);
                    }
                    m_nScrap1Count = 0;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                    }
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;

                case CYCLE_SCRAP.CHECK_STRIP_EXIST:
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (m_bAutoMode)
                            {
                                if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                                {
                                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                    // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                    if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                    {
                                        // Vac On 에러
                                        return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                    }

                                    if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.BLOW)
                                    {
                                        // Blow On 에러
                                        return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                    }

                                    //STR VAC OFF
                                    // 자재가 없어 Cycle 종료 로그
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER SCRAP1 - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));
                                    NextSeq((int)CYCLE_SCRAP.FINISH);
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_X_MOVE:
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SCRAP_1);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SCRAP_1);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_ON:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT].bOptionUse ||
                        RecipeMgr.Inst.Rcp.cleaning.nScrapMode == 0)
                    {
                        nFuncResult = UnitPickerScrapAllBlow(true, false, RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay);
                    }
                    else
                    {
                        nFuncResult = UnitPickerScrap1Blow(true, false, RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay);
                    }
                    break;
                case CYCLE_SCRAP.WAIT_DELAY:
                    //블로우 딜레이로 조절
                    if (WaitDelay(RecipeMgr.Inst.Rcp.cleaning.nScrap1Delay)) return FNC.BUSY;
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_READY_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_OFF:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT].bOptionUse ||
                        RecipeMgr.Inst.Rcp.cleaning.nScrapMode == 0)
                    {
                        nFuncResult = UnitPickerScrapAllBlow(false);
                    }
                    else
                    {
                        nFuncResult = UnitPickerScrap1Blow(false);
                    }
                    break;
                case CYCLE_SCRAP.CHECK_COUNT:
                    if (m_nScrap1Count < RecipeMgr.Inst.Rcp.cleaning.nScrapCount1 - 1) //0502 1132 KTH
                    {
                        m_nScrap1Count++;
                        NextSeq((int)CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_SCRAP.FINISH:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_1, false);
                    }
                    return FNC.SUCCESS;
                    break;

                default:
                    break;
            }
            #region AFTER SWITCH

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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int UnitPickerScrap2()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch ((CYCLE_SCRAP)m_nSeqNo)
            {
                case CYCLE_SCRAP.NONE:
                    break;
                case CYCLE_SCRAP.INIT:
                    if (RecipeMgr.Inst.Rcp.cleaning.nScrapCount2 == 0 || 
                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_SCRAP_ONE_POINT].bOptionUse ||
                        RecipeMgr.Inst.Rcp.cleaning.nScrapMode == 0)
                    {
                        NextSeq((int)CYCLE_SCRAP.FINISH);
                        return FNC.BUSY;
                    }
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_2, true);
                    }
                    m_nScrap2Count = 0;
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                    }
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.CHECK_STRIP_EXIST:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // Vac On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                }

                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    // Blow On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                }

                                //STR VAC OFF
                                // 자재가 없어 Cycle 종료 로그
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER SCRAP2 - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));
                                NextSeq((int)CYCLE_SCRAP.FINISH);
                                return FNC.BUSY;
                            }
                        }
                    }
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_X_MOVE:
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SCRAP_2);
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SCRAP_2);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_ON:
                    nFuncResult = UnitPickerScrap2Blow(true, false, RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay);
                    break;
                case CYCLE_SCRAP.WAIT_DELAY:
                    //블로우 딜레이로 조절
                    if (WaitDelay(RecipeMgr.Inst.Rcp.cleaning.nScrap2Delay)) return FNC.BUSY;
                    break;
                case CYCLE_SCRAP.UNIT_PICKER_READY_Z_MOVE:
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                    break;
                case CYCLE_SCRAP.SCRAP_BLOW_OFF:
                    nFuncResult = UnitPickerScrap2Blow(false);
                    break;
                case CYCLE_SCRAP.CHECK_COUNT:
                    if (m_nScrap2Count < RecipeMgr.Inst.Rcp.cleaning.nScrapCount2 - 1) //0502 1132 KTH
                    {
                        m_nScrap2Count++;
                        NextSeq((int)CYCLE_SCRAP.UNIT_PICKER_SCRAP_Z_MOVE);
                        return FNC.BUSY;
                    }
                    break;
                case CYCLE_SCRAP.FINISH:
                    if (m_bFirstSeqStep)
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.SCRAP_2, false);
                    }
                    return FNC.SUCCESS;
                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        #endregion

        #region Cleaning
        public int SpongeCleaning()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nSpongeCount = 0;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 6:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 8:
                    {
                        if ((!IsInPosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_READY)))
                        {
                            return FNC.CYCLE_CHECK;
                        }

                    }
                    break;

                case 11:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // Vac On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                }

                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    // Blow On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                }

                                //STR VAC OFF
                                // 자재가 없어 Cycle 종료 로그
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER SPONGE - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));
                                NextSeq(60);
                                return FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if (RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount <= 0)
                        {
                            // 스펀지 클리닝 종료
                            NextSeq(50);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 14:
                    // UNIT TRANSFER X AXIS SPONGE START POS MOVE
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    // UNIT TRANSFER Z AXIS SPONGE START POS MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SPONGE_START, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // UNIT TRANSFER Z AXIS SPONGE START POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_SPONGE_START,0,true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // SPONGE COUNT CHECK
                    {
                        int nTotalCount = RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount;
                        if (m_nSpongeCount >= nTotalCount)
                        {
                            // 스펀지 클리닝 종료
                            NextSeq(50);
                            return FNC.BUSY;

                        }

                        // 스펀지 클리닝 해야함
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("UNIT PICKER SPONGE CLEAN START ({0}/{1})", m_nSpongeCount, nTotalCount), STEP_ELAPSED));
                        break;
                    }

                case 22:
                    // BRUSH WATER ON
                    {
                        // SPONGE CLEAN 하는데 BRUSH WATER ON 해야하는지 확인 필요
                        if (GbVar.IO[IODF.OUTPUT.CLEAN_BRUSH_WATER] == 0)
                        {
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.CLEAN_BRUSH_WATER, true);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER SPONGE WATER SOL ON({0}/{1})"), STEP_ELAPSED);
                        }
                    }
                    break;

                case 24:
                    // UNIT TRANSFER X AXIS SPONGE START POS MOVE
                    {
                        double dSpongeSpeed = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_START, false, dSpongeSpeed, RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // UNIT TRANSFER X AXIS SPONGE END POS MOVE
                    {
                        double dSpongeSpeed = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_END, false, dSpongeSpeed, RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeEndDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE END POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 28:
                    // GO TO SPONGE COUNT CHECK
                    {
                        m_nSpongeCount++;

                        NextSeq(20);
                        return FNC.BUSY;
                    }
                case 50:
                    // UNIT TRANSFER X AXIS SPONGE START POS MOVE
                    {
                        double dSpongeSpeed = RecipeMgr.Inst.Rcp.cleaning.dUnitPkSpongeVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_SPONGE_START, false, dSpongeSpeed, RecipeMgr.Inst.Rcp.cleaning.lUnitPkSpongeStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 52:
                    // UNIT TRANSFER Z AXIS UP POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int WaterCleaning()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {

                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 5:
                    {
                        m_nWaterJetSwingCount = 0;

                        m_nTotalCleanerSwingCount = RecipeMgr.Inst.Rcp.cleaning.nWaterSwingCnt;
                    }
                    break;

                case 6:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 8:
                    {
                        if ((!IsInPosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_READY)))
                        {
                            return FNC.CYCLE_CHECK;
                        }

                    }
                    break;

                case 11:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // Vac On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                }

                                if (AirStatus(STRIP_MDL.UNIT_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    // Blow On 에러
                                    return (int)ERDF.E_STRIP_UNIT_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                }

                                //STR VAC OFF
                                // 자재가 없어 Cycle 종료 로그
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER WATER JET - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));
                                NextSeq((int)CYCLE_SCRAP.FINISH);
                                return FNC.BUSY;
                            }
                        }
                    }
                    break;
                case 12:
                    // UNIT TRASNFER X STRIP CLEAN POS MOVE
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_WATER_AIR_SWING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS STRIP CLEAN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    {
                        if (m_nTotalCleanerSwingCount <= 0)
                        {
                            NextSeq(30);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 15:
                    // UNIT TRANSFER Z AXIS SPONGE START POS MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_WATER_AIR_SWING, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS SPONGE START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 16:
                    // UNIT TRASNFER Z CLEAN POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_WATER_AIR_SWING, 0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS STRIP CLEAN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // CLEAN WATER COUNT CHECK
                    {
                        if (m_nWaterJetSwingCount >= m_nTotalCleanerSwingCount)
                        {
                            CleanerWaterAirJet(false);

                            // 워터 클리닝 종료
                            NextSeq(30);
                            return FNC.BUSY;
                        }

                        // 워터 클리닝 해야함
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("UNIT PICKER STRIP CLEAN WATER START ({0}/{1})", m_nWaterJetSwingCount, m_nTotalCleanerSwingCount), STEP_ELAPSED));
                        break;
                    }

                case 22:
                    // CLEAN WATER ON
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            CleanerWaterAirJet(false);
                        }
                        else
                        {
                            CleanerWaterAirJet(true);
                        }

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN WATER ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 24:
                    // CLEAN SWING FWD
                    {
                        nFuncResult = CleanerSwingFwd(true, RecipeMgr.Inst.Rcp.cleaning.lWaterSwingStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN SWING FWD COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // CLEAN SWING BWD
                    {
                        nFuncResult = CleanerSwingFwd(false, RecipeMgr.Inst.Rcp.cleaning.lnWaterSwingEndDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN SWING BWD COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 28:
                    // GO TO CLEAN WATER COUNT CHECK
                    {
                        m_nWaterJetSwingCount++;

                        NextSeq(20);
                        return FNC.BUSY;
                    }

                case 30:
                    // UNIT TRANSFER Z AXIS UP POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            CleanerWaterAirJet(false);
                        }

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 31:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    //return FNC.SUCCESS;
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region AirDry
        public int AirDryUnit(bool bIsNoUnit = false)
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nCleanAirBlowingCount = 0;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;

                case 10:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // UNIT TRANSFER Z AXIS BOX BLOW START POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_AIR_KNIFE_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS BOX BLOW START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // CLEAN BOX BLOW COUNT CHECK
                    {
                        if (m_nCleanAirBlowingCount >= RecipeMgr.Inst.Rcp.cleaning.nUnitPkDryBlowCount)
                        {
                            // 박스 블로우 클리닝 종료
                            NextSeq(44);
                            return FNC.BUSY;
                        }

                        // 박스 블로우 클리닝 해야함
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("UNIT PICKER STRIP CLEAN AIR START ({0}/{1})", m_nCleanBoxBlowCount, nTotalCount), STEP_ELAPSED));
                        break;
                    }

                case 32:
                    // CLEAN BOX BLOW ON (1번 블로우만 켠다)
                    {
                        //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_BOX_BLOW_1, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN BOX BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // UNIT TRANSFER X AXIS BOX BLOW START POS MOVE
                    {
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_AIR_KNIFE_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS BOX BLOW START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    // UNIT TRANSFER X AXIS SPONGE END POS MOVE
                    {
                        double dAirBlowSpeed = RecipeMgr.Inst.Rcp.cleaning.dUnitPkDryBlowVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_AIR_KNIFE_END, true, dAirBlowSpeed, RecipeMgr.Inst.Rcp.cleaning.lBtmDryEndDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS BOX BLOW END POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 38:
                    // CLEAN BOX BLOW OFF
                    {
                        //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_BOX_BLOW_1, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN BOX BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 40:
                    // UNIT TRANSFER X AXIS BOX BLOW START POS MOVE
                    {
                        double dAirBlowSpeed = RecipeMgr.Inst.Rcp.cleaning.dUnitPkDryBlowVel;
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_AIR_KNIFE_START, true, dAirBlowSpeed, RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS BOX BLOW START POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 41:
                    break;

                case 42:
                    // GO TO CLEAN BOX BLOW COUNT CHECK
                    {
                        m_nCleanAirBlowingCount++;

                        NextSeq(30);
                        return FNC.BUSY;
                    }

                case 44:
                    // CLEAN BOX BLOW OFF
                    {
                        //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_BOX_BLOW_1, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN BOX BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 50:
                    // UNIT TRANSFER Z AXIS UP POS MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 52:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        return FNC.SUCCESS;
                    }
                default:
                    break;
            }

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region Unloading
        public int UnloadingToDryStage()
        { 
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch (m_nSeqNo)
            {
                case 0:
                    break;

                case 1:
                    if (m_bAutoMode)
                    {
                        // 자재 정보가 있을 때만
                        if (GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                            // 시작 전 VAC 확인 후 ON 상태면 이미 자재가 있다고 판단 하고 처리 하자
                            // [2022.05.15.kmlee] 누군가가 VAC을 켜놓은 상태로 진행하면 이상 동작을 하므로 변경함
                            //                    Z축 올리는 부분만 생략하고 진행해도 이전 상태 그대로 가능하다 (마지막에 전부 끄므로)
                            if (AirStatus(STRIP_MDL.UNIT_DRY) == AIRSTATUS.VAC)
                            {
                                // X가 이미 Loading Position이라면 Z축을 올리지 않아도 됨
                                if (IsInPosUnitPkX(POSDF.UNIT_PICKER_STRIP_UNLOADING) && IsInPosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE - ALREADY STRIP VAC , NEXT SEQ", STEP_ELAPSED));
                                    NextSeq(10);
                                    return FNC.BUSY;
                                }
                            }
                        }
                        else
                        {
                            // 자재 정보가 없으면 자동으로 생성? 내지 알람
                            // 일반 유닛과 다르게 Saw와 인터페이스를 하므로
                            // case 12번에서 처리
                        }
                    }
                    break;

                case 2:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.UNIT_DRY) == AIRSTATUS.VAC || AirStatus(STRIP_MDL.UNIT_DRY) == AIRSTATUS.BLOW)
                                {
                                    return (int)ERDF.E_DRY_BLOCK_VAC_N_DATA_MISMATCH;
                                }

                                ////STR VAC OFF
                                //// 자재가 없어 Cycle 종료 로그
                                //SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK - NO STRIP CYCLE SUCCESS", STEP_ELAPSED));

                                //NextSeq(19);
                                //return FNC.BUSY;
                            }
                        }
                    }
                    break;
                case 5:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_STRIP_UNLOADING)) break;
                        }
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_STRIP_UNLOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 6:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosNo(SVDF.AXES.DRY_BLOCK_STG_X, POSDF.DRY_BLOCK_STAGE_STRIP_LOADING)) break;
                            double dTchPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.DRY_BLOCK_STG_X].dPos[POSDF.DRY_BLOCK_STAGE_STRIP_LOADING];
                            double dPosDry = MotionMgr.Inst[SVDF.AXES.DRY_BLOCK_STG_X].GetTargetMoveAbsPos();
                            if (SafetyMgr.Inst.IsWithInRangePos(dTchPos, dPosDry, 0.01))
                            {
                                break;
                            }
                        }
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 8:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 10:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, 0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 11:
                    {
                        nFuncResult = DryBlockWorkVac(true, false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE VAC ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 13:
                    {
                        nFuncResult = UnitPickerWorkBlow(true, 0, false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse == false) break;

                        nFuncResult = VacBlowCheck((int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC, true, (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_ON, 50);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CHECK! DRY STAGE VAC ON", STEP_ELAPSED));
                        }
                    }
                    break;


                case 17:
                    {
                        nFuncResult = UnitPickerWorkBlow(false, 0, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 18:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 19:
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    return FNC.SUCCESS;

                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion

        #region CleaningKit
        public int PickerKitAirCleaning(bool bIsNoUnit = false)
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            //switch (m_nSeqNo)
            //{
            //    case 0:
            //        {
            //            m_nToolKitCleanAirCount = 0;
            //        }
            //        break;

            //    case 4:
            //        {
            //            if (m_bAutoMode)
            //            {
            //                if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
            //                {
            //                    return FNC.CYCLE_CHECK;
            //                }
            //            }
            //        }
            //        break;

            //    case 10:
            //        // UNIT TRANSFER Z AXIS UP POS  MOVE
            //        {
            //            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 20:
            //        // UNIT TRASNFER X STRIP CLEAN POS MOVE
            //        {
            //            nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_STRIP_CLEAN);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS STRIP CLEAN POSITION COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 22:
            //        // UNIT TRASNFER Z CLEAN POS MOVE
            //        {
            //            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_CLEAN);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS STRIP CLEAN POSITION COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 30:
            //        // CLEAN WATER COUNT CHECK
            //        {
            //            if (m_nToolKitCleanAirCount >= RecipeMgr.Inst.Rcp.cleaning.nUnitPkKitAirBlowCount)
            //            {
            //                // 워터 클리닝 종료
            //                NextSeq(40);
            //                return FNC.BUSY;
            //            }

            //            // 워터 클리닝 해야함
            //            SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("UNIT PICKER STRIP CLEAN WATER START ({0}/{1})", m_nUnitPkWaterSwingCount, RecipeMgr.Inst.Rcp.cleaning.nUnitPkWaterCleanCount), STEP_ELAPSED));
            //            break;
            //        }

            //    case 32:
            //        // CLEAN WATER ON
            //        {
            //            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEANER_AIR_CLEAN, true);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN WATER ON COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 34:
            //        // CLEAN SWING FWD
            //        {
            //            nFuncResult = CleanerSwingFwd(true);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN SWING FWD COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 36:
            //        // CLEAN SWING BWD
            //        {
            //            nFuncResult = CleanerSwingFwd(false);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEANER STRIP CLEAN SWING BWD COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 38:
            //        // GO TO CLEAN WATER COUNT CHECK
            //        {
            //            m_nToolKitCleanAirCount++;

            //            NextSeq(30);
            //            return FNC.BUSY;
            //        }

            //    case 40:
            //        // CLEAN WATER OFF
            //        {
            //            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEANER_AIR_CLEAN, false);

            //        }
            //        break;

            //    case 42:
            //        // UNIT TRANSFER Z AXIS UP POS MOVE
            //        {
            //            nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

            //            if (FNC.IsSuccess(nFuncResult))
            //            {
            //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
            //            }
            //        }
            //        break;

            //    case 50:
            //        {
            //            string strStripInfo = string.Format("{0}",
            //                "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
            //            SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

            //            return FNC.SUCCESS;
            //        }
            //    default:
            //        break;
            //}

            #region AFTER SWITCH
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
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
        #endregion
    }
}
