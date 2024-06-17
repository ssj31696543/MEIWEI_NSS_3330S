using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procSawUnitTransfer : SeqBase
    {
        cycUnitTransfer cycFunc = null;

        public procSawUnitTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUnitTransfer;

            cycFunc = new cycUnitTransfer(nSeqID, 0);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER seqNo)
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
                if (m_seqInfo != GbVar.Seq.sUnitTransfer)
                {
                    m_seqInfo = GbVar.Seq.sUnitTransfer;
                    m_nSeqNo = GbVar.Seq.sUnitTransfer.nCurrentSeqNo;
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
            switch ((seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER)m_nSeqNo)
            {
                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.INIT:
                    if (GbVar.LOADER_TEST)
                        return;

                    //if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                    //{
                    //    if (AirStatus(STRIP_MDL.UNIT_TRANSFER) != AIRSTATUS.NONE)
                    //    {
                    //        //배큠을 꺼야함
                    //        SetError((int)ERDF.E_INTL_THERE_IS_NO_UNIT_PK_STRIP_INFO);
                    //        return;
                    //    }
                    //}
                    break;
                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.READY_POS_MOVE_Z_AXIS:
                    // Z Axis Ready
                    // X Axis 도피위치
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        SeqHistory("Move ready pos", "Unit Picker Z Axis", "Start");
                    }
                    nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move ready pos", "Unit Picker Z Axis", "Done");
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.READY_POS_MOVE_X_AXIS:
                    // Z Axis Ready
                    // X Axis 도피위치
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosUnitPkX(POSDF.UNIT_PICKER_READY)) break;
                        SeqHistory("Move ready pos", "Unit Picker X Axis", "Start");
                    }
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //초기화를 안전위치까지 간 다음에 진행
                        //221025 HEP
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T2] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.MAP_TRANSFER_UNLOAD_INTERLOCK_1] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.MAP_TRANSFER_UNLOAD_INTERLOCK_2] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_RUN] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_COMPLETE] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_SPONGE_CLEAN] = false;
                        
                        SeqHistory("Move ready pos", "Unit Picker X Axis", "Done");
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.WAIT_CUTTING_TABLE_LOAD_REQ:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Wait cutting table unload req", "Unit Picker", "Wait");
                    }

                    if (!LeaveCycle()) return;

                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        GbVar.Seq.sUnitTransfer.nLoadingTableNo = 0;
                        break;
                    }

                    if (IFMgr.Inst.SAW.IsTableUnloadingReq())
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.IsStrip())
                        {
                            SetError((int)ERDF.E_UNIT_TRANSFER_NO_STRIP_SAW_CUTTING_TABLE_1_REQ_ON);
                            return;
                        }

                        GbVar.Seq.sUnitTransfer.nLoadingTableNo = 0;
                        SeqHistory("Load strip from left table(Left)", "Unit Picker", "Start");

                        //아래 조건 필요 없을 것으로 예상됨. 확인 후 삭제할 것
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                        IFMgr.Inst.SAW.RequestWorkingData(0);//미리 WorkingData요청
                        IFMgr.Inst.SAW.ifWorkingData.swTimeStamp.Restart();
                        SeqHistory("Request saw Working data(Left)", "Unit Picker", "Request");

                        //STRIP_CUTTING_LF 싸이클타임 측정종료
                        //TTDF.SetTact(TTDF.STRIP_CUTTING_LF + GbVar.Seq.sUnitTransfer.nLoadingTableNo, false);
                        break;
                    }
                    return;
                // Todo ; 20211028 bhoh
                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.IS_STRIP_TRANSFER_PLACE_CYCLE:

                    // Strip Picker가 Stop 상태면 Req 체크하여 정지한다
                    if (!GbVar.mcState.IsCycleRun(SEQ_ID.STRIP_TRANSFER))
                    {
                        LeaveCycle();
                        return;
                    }
                    if (!GbVar.Seq.sCuttingTable.Info.IsStrip())
                    {
                        LeaveCycle();
                        return;
                    }
                    // 스트립 트랜스퍼가 픽업하기 전까지 대기한다.
                    if (GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] == true) return;

                    // Strip Picker가 1번 테이블 Unloading 시 Unit Picker가 1번 테이블 Loading 시 기다린다
                    if (GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] == true) return;

                    // 충돌 위험이 있을 경우 대기
                    int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo((int)SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_LOADING_TABLE_1 + GbVar.Seq.sUnitTransfer.nLoadingTableNo);
                    if (FNC.IsErr(nSafety)) return;

                    GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] = true;

                    GbVar.Seq.sUnitTransfer.ProcCycleStart(GbVar.Seq.sUnitTransfer.nLoadingTableNo);

                    //STRIP_UNLOADING_TO_CLEAN_TABLE 싸이클타임 측정시작
                    //TTDF.SetTact(TTDF.STRIP_UNLOADING_TO_CLEAN_TABLE, true);
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.CYC_UNIT_PICK_UP_FROM_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.SAW_PICK_UP_PAUSE)
                        {
                            //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                            // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                            LeaveCycle();
                            return;
                        }
                        SeqHistory(string.Format("Load strip from cutting table({0})", GbVar.Seq.sUnitTransfer.nLoadingTableNo == 0 ? "Left" : "Right"), "Unit Picker", "Start");
                    }

                    nFuncResult = cycFunc.LoadingTableUnit();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("Load strip from cutting table({0})", GbVar.Seq.sUnitTransfer.nLoadingTableNo == 0 ? "Left" : "Right"), "Unit Picker", "Done");

                        GbVar.Seq.sCuttingTable.SetProcCycleOutTime((int)MCC.TURN_TABLE);

                        if (GbVar.Seq.sCuttingTable.Info.IsStrip())
                        {
                            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            //{
                            //    GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] = true;
                            //}
                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_CUTTING_COUNT++;

                            // 함수 내부에서 언로딩 시 스크랩 조건에 따라 인터락을 해제한다. 작성자 : 홍은표
                            //GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nLoadingTableNo] = false;

                            GbVar.Seq.sUnitTransfer.DataShiftCutTableToTranser();
                            GbVar.Seq.sUnitTransfer.SetProcCycleInTime((int)MCC.UNIT_PICKER);
                            // nUnloadingTableNo
                            GbVar.Seq.sUnitTransfer.nUnloadingTableNo = GbVar.Seq.sUnitTransfer.nLoadingTableNo;

                            string strSpindle1_1Ch = "";
                            string strSpindle2_1Ch = "";
                            string strSpindle1_2Ch = "";
                            string strSpindle2_2Ch = "";
                            string strStageSpeed_1Ch = "";
                            string strStageSpeed_2Ch = "";
                            if (GbVar.g_dictSawSVID.Count > 0)
                            {
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_1CH_1, out strSpindle1_1Ch);
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_2CH_1, out strSpindle2_1Ch);
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_1CH_2, out strSpindle1_2Ch);
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.SPINDLE_RPM_2CH_2, out strSpindle2_2Ch);
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.STAGE_SPEED_CH1, out strStageSpeed_1Ch);
                                GbVar.g_dictSawSVID.TryGetValue(GbVar.STAGE_SPEED_CH2, out strStageSpeed_2Ch);
                            }
                            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH1 = strSpindle1_1Ch;
                            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH1 = strSpindle2_1Ch;
                            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_1_CH2 = strSpindle1_2Ch;
                            GbVar.Seq.sUnitTransfer.Info.SPINDLE_RPM_2_CH2 = strSpindle2_2Ch;
                            GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH1 = strStageSpeed_1Ch;
                            GbVar.Seq.sUnitTransfer.Info.STAGE_SPEED_CH2 = strStageSpeed_2Ch;

                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.SCRAP_1);
                            return;
                        }
                        else
                        {
                            // 자재가 없을 경우 처리 구간
                            // 어디로 되돌릴지 확인 후 처리
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.SCRAP_1:
                    nFuncResult = cycFunc.UnitPickerScrap1();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Dummy Scrap1", "Unit Picker", "Finish");
                        //스크랩 끝나도 안전함 
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] = false;

                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }      
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.SCRAP_2:
                    nFuncResult = cycFunc.UnitPickerScrap2();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Dummy Scrap2", "Unit Picker", "Finish");

                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.SPONGE_CLEANING:
                    if (m_bFirstSeqStep)
                    {
                        //스폰지 체크
                        if (RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount <= 0)
                        {
                            break;
                        }
                        if (GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] == true) return;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_SPONGE_CLEAN] = true ;
                    }
                    nFuncResult = cycFunc.SpongeCleaning();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Sponge Cleaning", "Unit Picker", "Finish");
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_SPONGE_CLEAN] = false;

                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.CHECK_DRY_STAGE_STATUS:
                    if (GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] == true) return;
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.WATER_JET_CLEANING:
                    nFuncResult = cycFunc.WaterCleaning();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Water Cleaning", "Unit Picker", "Finish");

                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }
                    }
                    break;


                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.REQ_UNIT_BTM_DRY:
                    GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] = true;
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.CHECK_INTERLOCK:
                    if (!LeaveCycle()) return;

                    //221021 HEP
                    //리픽 사용하면 드라이 다끝나고 복귀
                    if (RecipeMgr.Inst.Rcp.cleaning.nRepickCount != 0 &&
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN])
                    {
                        if (GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] == false ||
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] == true) return;

                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] = false;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_RUN] = true;

                        SeqHistory("UNLOADING SEQ ", "Unit Picker", "NEXT");
                        break;
                    }

                    if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                    {
                        SeqHistory("UNIT TRANSFER STRIP IS NOT EXIST", "Unit Picker", "NEXT");
                        NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                        return;
                    }

                    if (GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] == false || 
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_BTM_DRY_REQ] == true) return;

                    GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_LOADING_READY] = false;
                    GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_RUN] = true;
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.UNLOADING_TO_DRY_STAGE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.DRY_BLOCK_PLACE_PAUSE)
                        {
                            //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                            // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Load strip from dryblock", "Unit Picker", "Start");
                    }
                    nFuncResult = cycFunc.UnloadingToDryStage();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_COMPLETE] = true;
                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.UNIT_UNLOAD_RUN] = false;
                        SeqHistory("Unloading to Dry Stage", "Unit Picker", "Finish");
                        //동기가 안맞을때가 있어서 여기서도 true를 해줌
                        if (GbVar.Seq.sUnitDry.nCurrentSeqNo == (int)seqUnitDry.SEQ_SOTER_UNIT_DRY.WAIT_UNIT_PICKER_UNLOAD)
                        {
                            GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] = true;
                        }

                        if (GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            GbVar.Seq.sUnitTransfer.DataShiftTransferToDryStage();
                            GbVar.Seq.sUnitTransfer.SetProcCycleInTime((int)MCC.DRY_BLOCK);
                            SeqHistory("Check Interlock", "Unit Picker", "NEXT SEQ");
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.CHECK_INTERLOCK);
                            return;
                        }
                        else
                        {
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS);
                            return;
                        }
                    }
                    break;

                #region 자재가 없을 때 대기 위치로 이동 후 인터락 초기화
                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_Z_AXIS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Move ready pos", "Unit Picker Z Axis", "Start");
                        }

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Move ready pos", "Unit Picker Z Axis", "Done");
                        }
                    }
                    break;

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NO_STRIP_READY_POS_MOVE_X_AXIS:
                    {
                        // Z Axis Ready
                        // X Axis 도피위치
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_READY)) break;
                            SeqHistory("Move ready pos", "Unit Picker X Axis", "Start");
                        }

                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_READY);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Move ready pos", "Unit Picker X Axis", "Done");

                            // 인터락 초기화 후 케이스 되돌림
                            //GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] = false;
                            NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.INIT);
                            return;
                        }
                    }
                    break;
                #endregion

                case seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.FINISH:
                    NextSeq((int)seqUnitTransfer.SEQ_SAW_UNIT_TRANSFER.NONE);
                    SeqHistory("Seq Finish", "Unit Picker", "Finish");
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
                GbVar.Seq.sUnitTransfer.Info.bIsError = true;

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
