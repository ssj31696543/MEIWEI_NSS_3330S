using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procSawStrpTransfer : SeqBase
    {
        cycStripTransfer cycFunc = null;
        int m_nReloadTurnBackSeqNum = 0;
        bool m_bMgzOut = false;
        public procSawStrpTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sStripTransfer;

            cycFunc = new cycStripTransfer(nSeqID, 0);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER seqNo)
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
                if (m_seqInfo != GbVar.Seq.sStripTransfer)
                {
                    m_seqInfo = GbVar.Seq.sStripTransfer;
                    m_nSeqNo = GbVar.Seq.sStripTransfer.nCurrentSeqNo;
                }
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }
                            
            if (m_nSeqNo != m_nPreSeqNo)
            {
                m_bFirstSeqStep = true;
                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            //m_bFirstSeqStep = false;


            // 테스트
            //if (!LeaveCycle()) return;

            //return;

            switch ((seqStripTransfer.SEQ_SAW_STRIP_TRANSFER)m_nSeqNo)
            {
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.INIT:
                    if (GbVar.LOADER_TEST)
                        return;

                    GbVar.Seq.sStripTransfer.nMgzLoadSlotNum = 0;
                    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false; // 초기화
                    GbVar.Seq.sStripTransfer.Init();
                    m_bMgzOut = false;
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.READY_POS_MOVE_Z:
                    // Strip Rail Cyl Down
                    // Strip Z Axis Up
                    // Gripper Open
                    // Gripper Up
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                    }

                    nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Done");
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.READY_POS_MOVE_XY:
                    // Strip Rail T Rcv Pos
                    // Strip Rail Guide  Recv
                    // Strip X Axis Ready
                    // Cam Y Axis Ready
                    if (m_bFirstSeqStep)
                    {   
                        SeqHistory("Move to Ld Vision Ready Pos", "Ld Vision(with strip pk)", "Start");
                    }

                    nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_READY, POSDF.LD_VISION_X_READY);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Ld Vision Ready Pos", "Ld Vision(with Strip pk)", "Done");

                        if (GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] == true)
                        {
                            GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] = false;
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_1:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_1;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_MGZ_LOT_START:
                    //20211130 choh : cycle stop 추가하기
                    if (!LeaveCycle()) return;

                    SeqHistory("Mgz Work", "Strip Picker", "Start");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_LOAD_STOP:
                    {
                        
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS:
                    {
                        if (!LeaveCycle()) return;
                        if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] != true) return;

                        // Gripper Push Flag 시작
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = true;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 매거진 클램프 상태에서 매거진 미감지 에러
                        //if (!IsLdElvCheckSensorOn())
                        //{
                        //    nFuncResult = (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST;
                        //}
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_2:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_2;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_LOAD_COUNT_CHECK:
                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.bOneStripLoad == true) return;

                    if (GbVar.Seq.sStripTransfer.nMgzLoadSlotNum >= RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount
                        || m_bMgzOut)
                    {
                        if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] == true) return;
                        SeqHistory("Mgz Work", "Strip Picker", "End");

                        //20211111 choh : 이거 트루해도되나..?? 트루가 되어야 매거진을 배출할텐데..
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = true;
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false;
                        m_bMgzOut = false;

                        //매거진 완료 시 수량추가 
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MGZ_COUNT++;

                        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.INIT);
                        return;
                    }
                    break;



                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_STRIP_LOAD_RAIL:
                    {
                        //커팅테이블에 소재가 있으면 진행 하지 않는다.

                        // Unit Picker가 Stop 상태면 Req 체크하여 정지한다
                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.UNIT_TRANSFER))
                        {
                            CheckCycle();
                            return;
                        }

                        // Unit Picker가 1번 테이블 Loading 시 무조건 기다린다
                        if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] == true) return;

                        //// 충돌 위험이 있을 경우 대기
                        //int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo((int)SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_UNLOADING_TABLE_1 + GbVar.Seq.sStripTransfer.nUnloadingTableNo);
                        //if (FNC.IsErr(nSafety)) return;

                        //if (GbVar.Seq.sCuttingTable.Info.IsStrip())
                        //{
                        //    //소재가 있으면 반환
                        //    return;
                        //}
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PUSH_AND_BARCODE:
                    
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.INSERT_STRIP_PAUSE)
                        {
                            //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                            // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                            LeaveCycle();
                            return;
                        }

                        SeqHistory("Strip push and read code", "Strip Picker", "Start");

                        // Gripper Push Flag Start
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = true;

                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        //{
                        //    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false;

                        //    SeqHistory("DRY RUN STRIP OK", "Strip Picker", string.Format("Done({0}/{1})", GbVar.Seq.sStripTransfer.nMgzLoadSlotNum, RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount));
                        //    GbVar.Seq.sStripRail.Info.bIsStrip = true;
                        //    break;
                        //}
                    }

                    // 신규 시퀀스
                    // 1. 맵 푸셔 동작
                    // 2. 그리퍼 그립 위치로 이동 후 그립
                    // 4. ::: 푸셔 및 그립 동작 중 에러 발생 할 경우 처리 :::
                    // -> 수동으로 언그립, 해당 자제를 기존 슬롯에 다시 집어 넣고 시작

                    nFuncResult = cycFunc.StripPushAndBarcode();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        // Gripper Push Flag End
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false;

                        if (GbVar.Seq.sStripTransfer.bIsNotExistStrip == true)
                        {
                            SeqHistory("Strip push and read code", "Strip Picker", string.Format("Not exist({0}/{1})", GbVar.Seq.sStripTransfer.nMgzLoadSlotNum, RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount));

                            GbVar.Seq.sStripTransfer.bIsNotExistStrip = false;
                            GbVar.Seq.sStripTransfer.nMgzLoadSlotNum++;
 
                            // 매번 매거진 로딩 상태 확인 하기 위함
                            NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_MGZ_LOT_START);
                            GbVar.Seq.sStripRail.SetProcCycleInTime((int)MCC.LOAD_RAIL);
                            return;
                        }
                        else
                        {
                            // 정상적으로 그리퍼가 레일까지 자재를 집고 나온 상태
                            // 자재 생성 시점
                            GbVar.Seq.sStripRail.Info.bIsStrip = true;
                        }

                        SeqHistory("Strip push and read code", "Strip Picker", string.Format("Done({0}/{1})", GbVar.Seq.sStripTransfer.nMgzLoadSlotNum, RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount));
                    }
                    else if(nFuncResult == (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST)
                    {
                        // PUSH 전 매거진 알람 발생 시 처리
                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false;
                        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_MGZ_LOT_START);
                        return;
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_3:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_3;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_UNIT_TRANSFER_LOADING:
                    //20211113 CHOH : UNIT TRASNFER가 LOADING 작업중이면 STRIP TRANSFER는 STRIP 끌어오면 안됨
                    // 일단 확실하게 하고 추후 개선작업을 통해 변경 20211106 bhoh
                    //if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.STRIP_TRANSFER_UNLOAD_INTERLOCK] != false) return;
                    GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] = true;
                    if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] == true) return;

                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_LOADING:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("CYC_STRIP_LOADING_BOT_BARCODE", "Strip Picker", "Start");
                        }

                        // 신규 시퀀스
                        // ::: 그리퍼 그립 된 상태로 시작 :::
                        // 1. 하부 바코드 사용 시 바코드 위치로 이동 후 스캔 (  StripBottomBarcode )
                        // 2. 언그립 위치로 이동 후 언그립  ( StripRailUngrip )
                        // 3. 자재 로딩 후 레일 얼라인 진행 ( StripRailLoadingAndAlign )

                        nFuncResult = cycFunc.StripRailLoading();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            // 레일 자재가 있을 경우에만 데이터 처리하고 다음 동작으로 넘어 감
                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                // 자재가 없다면 초기로 되돌림
                                NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_MGZ_LOT_START);
                                return;

                            }

                        }
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_4:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_4;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_LOADING_RAIL_UNGRIP:
                    {
                        //if (m_bFirstSeqStep)
                        //{
                        //    SeqHistory("CYC_STRIP_LOADING_RAIL_UNGRIP", "Strip Picker", "Start");
                        //}

                        //nFuncResult = cycFunc.StripRailUngrip();
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_5:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_5;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_LOADING_RAIL_ALIGN:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            break;
                        }
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("CYC_STRIP_LOADING_RAIL_ALIGN", "Strip Picker", "Start");
                        }

                        nFuncResult = cycFunc.StripRailLoadingAndAlign();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            // 레일 자재가 있을 경우에만 데이터 처리하고 다음 동작으로 넘어 감
                            if (GbVar.Seq.sStripRail.Info.IsStrip())
                            {

                                //int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                                //int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;
                                //int nTableCount = nTotMapCountX * nTotMapCountY;
                                //int nOutCnt = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT / nTableCount;

                                //bool bIsStrip = false;
                                //bIsStrip |= GbVar.Seq.sStripTransfer.Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sCuttingTable.Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sUnitTransfer.Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sUnitDry.Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sMapTransfer.Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
                                //bIsStrip |= GbVar.Seq.sMapVisionTable[1].Info.IsStrip();

                                //if (!bIsStrip)
                                //{
                                //    if (GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT != nOutCnt)
                                //    {
                                //        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT = nOutCnt;
                                //    }
                                //}

                                // 자재 생성 위치 변경 / 여기서 생성하지 않고 Push And Grip 함수 동작 완료 후 생성 하도록 수정 함
                                //GbVar.Seq.sStripRail.Info.bIsStrip = true;
                                GbVar.Seq.sStripTransfer.nMgzLoadSlotNum++;
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT++;

                                //220507 pjh 자재 데이터 쉬프트 및 XOUT정보 받아오기 
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_VISION_BARCODE_READ_USE].bOptionUse)
                                    GbVar.Seq.sStripTransfer.DataShiftMgzToLoadingRail();

                                if (GbVar.lstBinding_HostLot != null && GbVar.lstBinding_HostLot.Count > 0)
                                {

                                    string sPnlCnt = GbVar.lstBinding_HostLot[MCDF.CURRLOT].SUB_QTY;
                                    int nCount = 0;
                                    int.TryParse(sPnlCnt, out nCount);

                                    if (nCount <= GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_INPUT_COUNT)
                                    {
                                        GbVar.Seq.bLotEndReserve = true;
                                        GbVar.Seq.bAutoLotEndCheck = true;
                                        m_bMgzOut = true;
                                    }
                                }

                                //220602 leejh 원래 있던 랏 배출 플래그는
                                //seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PICK_UP_FROM_RAIL:로 이동
                            }
                            else
                            {
                                // 자재가 없다면 초기로 되돌림
                                NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_MGZ_LOT_START);
                                return;

                            }

                            #region 기존 코드
                            //// 자재 생성 위치 변경
                            ////GbVar.Seq.sStripRail.Info.bIsStrip = true;
                            //GbVar.Seq.sStripTransfer.nMgzLoadSlotNum++;
                            //GbVar.lstBinding_EqpProc[0].STRIP_INPUT_COUNT++;

                            ////220507 pjh 자재 데이터 쉬프트 및 XOUT정보 받아오기 
                            //if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_VISION_BARCODE_READ_USE].bOptionUse)
                            //    GbVar.Seq.sStripTransfer.DataShiftMgzToLoadingRail();

                            //// 현재 작업이 끝난 매거진 언로드 요청
                            //if (GbVar.Seq.sStripTransfer.nMgzLoadSlotNum >= RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount)
                            //{
                            //    SeqHistory("Mgz Work", "Strip Picker", "End");

                            //    //20211111 choh : 이거 트루해도되나..?? 트루가 되어야 매거진을 배출할텐데..
                            //    GbVar.Seq.sStripTransfer.nMgzLoadSlotNum = 0;
                            //    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                            //    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = true;
                            //    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false; // 초기화
                            //}
                            #endregion
                        }
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_6:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_6;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PRESS_AND_VACUUM:
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE].bOptionUse) break;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;

                    nFuncResult = cycFunc.StripPressAndVac();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                      
                    }
                    break;

                // 20211106 bhoh  한미설비 순서 확인 후 시퀀스 순서 변경함...
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_2D_CODE_READ:
                    //20211113 CHOH: 드라이런 모드일때 (자재없이)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_VISION_BARCODE_READ_USE].bOptionUse) break;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Read 2D Code(Vision)", "Strip Picker", "Start");

                    }

                    nFuncResult = cycFunc.StripReadBarcode();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        // 자재가 있을때만 데이터 처리 하기 위함
                        if (!GbVar.Seq.sStripRail.Info.IsStrip())
                        {
                            NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS);
                            return;
                        }

                        //220507 pjh 자재 데이터 쉬프트 및 XOUT정보 받아오기
                        GbVar.Seq.sStripTransfer.DataShiftMgzToLoadingRail();
                        SeqHistory("Read 2D Code(Vision)", "Strip Picker", "Done");
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_7:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_7;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PRE_ALIGN:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    // Message 뛰우고 일시 정지 20211115 bhoh B7 modulecut 설비 align부 참고...
                    //if (GbVar.Seq.bAlignTeachStop[0] == true)
                    //{
                    //    SetError((int)ERDF.E_BALL_VISION_ALIGN_BWD);
                    //    return;
                    //}

                    // [2022.05.10.kmlee] Cycle에서 확인함
                    //if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_USE].bOptionUse) break;

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Pre Align", "Strip Picker", "Start");
                    }

                    nFuncResult = cycFunc.StripPreAlign();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        // 자재가 있을때만 데이터 처리 하기 위함
                        if (!GbVar.Seq.sStripRail.Info.IsStrip())
                        {
                            NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS);
                            return;
                        }

                        SeqHistory("Pre Align", "Strip Picker", "Done");
                    }
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_8:
                    //if (IsCuttingTableReloadReqEventOn())
                    //{
                    //    m_nReloadTurnBackSeqNum = (int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_8;
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK);
                    //    return;
                    //}
                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CONFIRM:
                    //{
                    //    if (WaitDelay(200)) return;

                    //    if (!GetStripLoadingConfirm())
                    //    {
                    //        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CUT_TABLE_RELOAD_CHECK_8);
                    //        return;
                    //    }

                    //    SeqHistory("Pre Align", "Strip Picker", "GetStripLoadingConfirm");
                    //}
                    break;

                //case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_ORIENT_CHECK:

                //    //20211113 CHOH: 드라이런 모드일때 (자재없이)
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_STRIP_USE].bOptionUse) break;

                //    // Option 처리해야 할 수 있음
                //    // 한미 설비는 2nd Align Match 할 때 같이 확인하는 듯함...
                //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_ORIENT_CHECK_USE].bOptionUse) break;

                //    if (m_bFirstSeqStep)
                //    {
                //        SeqHistory("Orient check", "Strip Picker", "Start");
                //        m_bFirstSeqStep = false;
                //    }

                //    nFuncResult = cycFunc.StripOrientMarkCheck();

                //    if (nFuncResult == FNC.SUCCESS)
                //    {
                //        SeqHistory("Orient check", "Strip Picker", "Done");
                //    }
                //    break;

                //case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.REPORT_SUB_LOAD:
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                //    {
                //        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PICK_UP_FROM_RAIL);
                //        return;
                //    }
                        
                //    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PICK_UP_FROM_RAIL:
                    if (m_bFirstSeqStep)
                    {
                        //if (GbVar.INSERT_STRIP_PAUSE)
                        //{
                        //    //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                        //    // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                        //    LeaveCycle();
                        //    return;
                        //}

                        // 임시로 막음
                        //if (!IFMgr.Inst.SAW.IsSawRun())
                        //{
                        //    LeaveCycle();
                        //    return;
                        //}

                        SeqHistory("Load Strip to Strip Picker", "Strip Picker", "Start");
                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.LoadingRailStrip();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Load Strip to Strip Picker", "Strip Picker", "Done");

                        //20211113 CHOH: STRIP TRANSFER가 STRIP LOADING 완료하면 UNIT TRANSFER 이동 허가
                        //상황설명: 스트립 트랜스퍼가 스트립을 그립해서 끌고오는 동작중에는
                        //유닛피커가 왼쪽 테이블에 로딩하러 접근해서는 안됨.
                        GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] = false;
                        GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] = false;
                        GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T2] = false;
                        //STRIP_PUSH_AND_LOADING 싸이클타임 측정종료
                        //TTDF.SetTact(TTDF.STRIP_PUSH_AND_LOADING, false);

                        // 자재가 있을때만 데이터 처리 하기 위함
                        if (GbVar.Seq.sStripRail.Info.IsStrip())
                        {
                            //220602 leejh seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_LOADING_RAIL_ALIGN:에서 여기로 이동
                            // 현재 작업이 끝난 매거진 언로드 요청
                            if (GbVar.Seq.sStripTransfer.nMgzLoadSlotNum >= RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount)
                            {
                                SeqHistory("Mgz Work", "Strip Picker", "End");

                                //20211111 choh : 이거 트루해도되나..?? 트루가 되어야 매거진을 배출할텐데..
                                GbVar.Seq.sStripTransfer.nMgzLoadSlotNum = 0;
                                GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                                GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = true;
                                GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] = false; // 초기화

                                //매거진 완료 시 수량추가 
                                GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MGZ_COUNT++;
                            }
                            GbVar.Seq.sStripRail.NewStripDataInsert();

                            GbVar.Seq.sStripRail.SetProcCycleOutTime((int)MCC.LOAD_RAIL);
                            GbVar.Seq.sStripTransfer.DataShiftLoadingRailToTranser();
                            GbVar.Seq.sStripTransfer.SetProcCycleInTime((int)MCC.STRIP_PICKER);
                        }
                        else
                        {
                            NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS);
                            return;
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.WAIT_CUTTING_TABLE_LOAD_REQ:
                    if (m_bFirstSeqStep)
                    {
                        //SeqHistory("Wait cutting table load req", "Strip Picker", "Wait");
                    }
                    // 자재가 있을때만 데이터 처리 하기 위함
                    if (!GbVar.Seq.sStripTransfer.Info.IsStrip())
                    {
                        NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS);
                        return;
                    }
                    if (LeaveCycle() == false) return;

                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        GbVar.Seq.sStripTransfer.nUnloadingTableNo = 0;
                        break;                   
                    }
                    else
                    {
                        //임시로 막음
                        //if (!IFMgr.Inst.SAW.IsSawRun())
                        //{
                        //    LeaveCycle();
                        //    return;
                        //}

                        if (m_bFirstSeqStep)
                        {
                            //SeqHistory("Wait Cutting table load req", "Strip Picker", "Start");
                        }

                        if (IFMgr.Inst.SAW.IsTableLoadingReq())
                        {
                            SeqHistory("Wait cutting table load req(Left)", "Strip Picker", "Done");
                            GbVar.Seq.sStripTransfer.nUnloadingTableNo = 0;

                            //STRIP_LOADING_TO_LEFT_TABLE 싸이클타임 측정시작
                            //TTDF.SetTact(TTDF.STRIP_LOADING_TO_LEFT_TABLE + GbVar.Seq.sStripTransfer.nUnloadingTableNo, true);
                            break;
                        }
                    }
                    return;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_UNIT_TRANSFER_RUN:
                    {
                        // Unit Picker가 Stop 상태면 Req 체크하여 정지한다
                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.UNIT_TRANSFER))
                        {
                            CheckCycle();
                            return;
                        }

                        // Unit Picker가 1번 테이블 Loading 시 무조건 기다린다
                        if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] == true) return;

                        // 충돌 위험이 있을 경우 대기
                        int nSafety_33 = SafetyMgr.Inst.GetAxisSafetyBeforePosNo((int)SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_UNLOADING_TABLE_1 + GbVar.Seq.sStripTransfer.nUnloadingTableNo);
                        if (FNC.IsErr(nSafety_33)) return;

                        if (GbVar.Seq.sCuttingTable.Info.IsStrip())
                        {
                            //소재가 있으면 반환
                            return;
                        }
                        GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] = true;
                        GbVar.Seq.sStripTransfer.ProcCycleStart(GbVar.Seq.sStripTransfer.nUnloadingTableNo);
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PLACE_TO_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.SAW_PLACE_PAUSE)
                        {
                            //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                            // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                            LeaveCycle();
                            return;
                        }

                        SeqHistory(string.Format("Unload Strip to cutting table({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Start");
                    }

                    nFuncResult = cycFunc.UnloadingToCuttingTable();
                    if (nFuncResult == FNC.SUCCESS)
                    {
						// [2022.07.22.kmlee] FINISH에서 여기로 옮김
						// 언로딩하고 스탑 상태에서 SAW 작업이 끝난 후 실행하면 자재 정보가 없는데 언로딩 비트가 들어왔다고 알람 뜸
                        if (GbVar.Seq.sStripTransfer.Info.IsStrip())
                        {
                            GbVar.Seq.sStripTransfer.DataShiftTranserToCuttingTable();
                            GbVar.Seq.sCuttingTable.SetProcCycleInTime((int)MCC.TURN_TABLE);

                            //STRIP_CUTTING_LF 싸이클타임 측정시작
                            //TTDF.SetTact(TTDF.STRIP_CUTTING_LF + GbVar.Seq.sStripTransfer.nUnloadingTableNo, true);
                        }
                        else
                        {
                            GbVar.Seq.sStripTransfer.Info.Clear();
                        }

                        SeqHistory(string.Format("Unload Strip to cutting table({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Done");
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_TABLE_ALIGN_OK:

                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)

                    GbVar.Seq.sStripTransfer.Info.CUTTING_TABLE_NO = GbVar.Seq.sStripTransfer.nUnloadingTableNo;

                    //220505  PJH 리로드 기능 사용하지 않기때문에 쏘우의 얼라인 OK기다릴 필요 없음. 때문에 아래 내용 주석
                    #region 주석
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    //{
                    //    SeqHistory("Receive load comp(DryRun, Saw I/F Skip)", "Strip Picker", "Done");
                    //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.FINISH);
                    //    return;
                    //}

                    //if (!LeaveCycle()) return;

                    //if (!IFMgr.Inst.SAW.IsTableLoadingComplete(GbVar.Seq.sStripTransfer.nUnloadingTableNo))
                    //{
                    //    if (m_bFirstSeqStep)
                    //    {
                    //        SeqHistory(string.Format("Receive load comp from saw({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Wait");
                    //        m_bFirstSeqStep = false;
                    //    }

                    //    if (IFMgr.Inst.SAW.IsTableReloadReq(GbVar.Seq.sStripTransfer.nUnloadingTableNo))
                    //    {
                    //        SeqHistory(string.Format("Receive reload req from saw({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Received");
                    //        break;
                    //    }

                    //    return;
                    //} 
                    #endregion

                    SeqHistory(string.Format("Receive load complete from saw({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Done");
                    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.FINISH);
                    return;

                #region RELOAD
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory(string.Format("Strip Reload({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Start");
                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = cycFunc.ReloadingToCuttingTable();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("Strip Reload({0})", GbVar.Seq.sStripTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Strip Picker", "Done");
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.TRANSFER_X_READY_POS_MOVE:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to ready pos", "Strip Picker", "Start");
                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_READY);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to ready pos", "Strip Picker", "Done");

                        //20211112 FINISH SEQUENCE에서 하도록 수정
                        //GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] = false;

                        //20211206 CHOH : 리로드가 끝나면 얼라인 ok신호를 확인해야하는가?
                        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_TABLE_ALIGN_OK);
                        return;
                    }
                    break; 
                #endregion

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.FINISH:
                    {
                        GbVar.Seq.sStripTransfer.SetProcCycleOutTime((int)MCC.STRIP_PICKER);
                        GbVar.Seq.sStripTransfer.ProcCycleEnd(0);
                        
                        //GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK] = false;
                        GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] = false;

                        //STRIP_LOADING_TO_LEFT_TABLE 싸이클타임 측정종료
                        //TTDF.SetTact(TTDF.STRIP_LOADING_TO_LEFT_TABLE + GbVar.Seq.sStripTransfer.nUnloadingTableNo, false);

                        SeqHistory("Seq Finish", "Strip Picker", "Finish");

                        // 기존 코드
                        //NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_LOAD_COUNT_CHECK);

                        //변경 코드, 매번 매거진 체크까지 하기 위함
                        NextSeq((int)seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CHECK_MGZ_STATUS);
                        return;
                    }

                #region Cutting Table Reload Seq
                //case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.IS_TABLE_ALIGN_OK:

                //    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                //    {
                //        SeqHistory("Receive load comp(DryRun, Saw I/F Skip)", "Strip Picker", "Done");
                //        NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PICK_UP_FROM_RAIL);
                //        return;
                //    }



                //    NextSeq(seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_PICK_UP_FROM_RAIL);
                //    return;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.STRIP_RELOAD_INTERLOCK:

                    if (GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1] == true) return;
               

                    GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] = true;
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_1:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("SUB RELOAD START");
                        m_bFirstSeqStep = false;
                    }

                    //nFuncResult = cycFunc.ReloadingToCuttingTable();

                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNIT_TRANSFER_LOAD_INTERLOCK_1] = false;
                    //    SeqHistory(string.Format("Strip Reload({0})",  == 0 ? "Left" : "Right"), "Strip Picker", "Done");

                    //    NextSeq(m_nReloadTurnBackSeqNum);
                    //    return;
                    //}

                    //break;

                    // STRIP TRANSFER Z AXIS UP POS  MOVE
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_2:

                    if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                        break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }

                    if (!IFMgr.Inst.SAW.IsTableReloadReq()) return;
                    SeqHistory("SUB RELOAD SIG CHECKED");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_3:
                    // STRIP TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        //nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 + RELOAD_TABLE_NO, dReloadCorrOffsetXResult);

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //m_strLogMsg = string.Format("STRIP PICKER X RELOAD CORR OFFSET X: {0}", dReloadCorrOffsetXResult.ToString("F3"));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS TABLE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_4:
                    // STRIP TRANSFER X AXIS 이동 완료 신호 ON
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetStripTransferXLoadPos(true);

                        SeqHistory("X LOAD POS MOVE DONE");
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_5:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("WAIT X LOAD POS REPLY");
                        m_bFirstSeqStep = false;

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }

                    if (!IFMgr.Inst.SAW.IsTableLoadingPos()) return;

                    SeqHistory("X LOAD POS REPLY CHECKED");
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOADING REQUEST SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_6:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_7:
                    // STRIP TRANSFER Z AXIS UNLOADING POS  MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_8:
                    // VACUUM ON
                    {
                        nFuncResult = StripPickerVac(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_9:
                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetStripTransferZLoadPos(true);

                        SeqHistory("Z LOAD POS MOVE DONE");
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_10:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("WAIT Z LOAD POS REPLY");
                        m_bFirstSeqStep = false;

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }
                    // Vac Off Check !!!
                    if (!IFMgr.Inst.SAW.IsTableUnloadingBlowOn()) return;

                    SeqHistory("Z LOAD POS REPLY CHECKED");
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_11:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_12:
                    // STRIP STRANSFER Z AXIS UP
                    {
                        //nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_RELOAD);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_13:
                    if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                        break;

                    IFMgr.Inst.SAW.SetStripTransferLoadComplete(true);
                    IFMgr.Inst.SAW.SetStripTransferZLoadPos(false);
                    IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                    //IFMgr.Inst.SAW.ResetStripTransferIF(RELOAD_TABLE_NO);

                    SeqHistory("RELOAD PICK_UP DONE");
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOAD COMPLETE SIGNAL ON COMPLETE", STEP_ELAPSED));
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_14:
                    if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                    {
                        GbVar.Seq.sStripTransfer.dReloadCorrOffsetX = 0;
                        break;
                    }

                    //if (!IFMgr.Inst.SAW.IsTableLoadingReq(RELOAD_TABLE_NO)) return FNC.BUSY;
                    //string[] strCorrData = GbVar.dbFromSaw.SelectResult();
                    //double dCorrOffsetX = 0.0f;
                    //if (!double.TryParse(strCorrData[0], out dCorrOffsetX)) dCorrOffsetX = 0.0f;

                    GbVar.Seq.sStripTransfer.dReloadCorrOffsetX = IFMgr.Inst.SAW.GetReloadOffset();


                    m_strLogMsg = string.Format("STRIP PICKER X RELOAD CORR OFFSET X: {0}", GbVar.Seq.sStripTransfer.dReloadCorrOffsetX.ToString("F3"));
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                    if (GbVar.Seq.sStripTransfer.dReloadCorrOffsetX > 1.0)
                    {
                        GbVar.Seq.sStripTransfer.dReloadCorrOffsetX = 0;

                        //여기서 알람이 뜨면 SAW 설비는 초기화가 되지 않는다.
                        //return (int)ERDF.E_STRIP_PK_CORRECT_OFFSET_OVER;
                    }

                    break;
                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_15:
                    // STRIP TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , GbVar.Seq.sStripTransfer.dReloadCorrOffsetX);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS TABLE CORR POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_16:
                    // STRIP TRANSFER X AXIS 이동 완료 신호 ON
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetStripTransferXLoadPos(true);

                        SeqHistory("X OFFSET MOVE DONE");
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_17:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("WAIT X LOAD POS REPLY");
                        m_bFirstSeqStep = false;

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }
                    if (!IFMgr.Inst.SAW.IsTableLoadingPos()) return;

                    SeqHistory("X LOAD POS REPLY CHECKED");
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER TABLE LOADING POS SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    //IFMgr.Inst.SAW.SetStripTransferLoadComplete(RELOAD_TABLE_NO, false);
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    SeqHistory("RESET INTERFACE SIG");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_18:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_19:
                    // STRIP TRANSFER Z AXIS UNLOADING POS  MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_20:
                    // BLOW ON
                    {
                        nFuncResult = StripPickerBlow(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_21:
                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    {
                        IFMgr.Inst.SAW.SetStripTransferZLoadPos(true);
                        SeqHistory("Z LOAD POS MOVE DONE");
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_22:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("WAIT Z LOAD POS REPLY");
                        m_bFirstSeqStep = false;

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }

                    // UP REQ?
                    if (!IFMgr.Inst.SAW.IsTableLoadingVacOn()) return;
                    SeqHistory("Z LOAD POS REPLY CHECKED");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_23:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    {
                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1 , -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_24:
                    // STRIP STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetStripTransferZLoadPos(false);
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_25:
                    // BLOW OFF
                    {
                        nFuncResult = StripPickerBlow(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_26:
                    if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                        break;

                    IFMgr.Inst.SAW.SetStripTransferLoadComplete(true);
                    SeqHistory("SUB RELOAD COMPLETE");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_27:

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("SUB RELOAD IF RESET CHECK");
                        m_bFirstSeqStep = false;

                        if (!GbVar.Seq.sCuttingTable.Info.bIsStrip)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        SetError((int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT);
                    }
                    //if (!IFMgr.Inst.SAW.IsTableLoadingComplete(RELOAD_TABLE_NO)) return FNC.BUSY;

                    if (IFMgr.Inst.SAW.IsTableReloadReq()) return;
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    SeqHistory("SUB RELOAD IF RESET");
                    break;

                case seqStripTransfer.SEQ_SAW_STRIP_TRANSFER.CYC_STRIP_RELOAD_TABLE_28:
                    // STRIP STRANSFER X READY POS MOVE
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.ResetStripTransferIF();

                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS READY POSITION COMPLETE", STEP_ELAPSED));

                            GbVar.Seq.sStripTransfer.bSeqIfVar[seqStripTransfer.UNLOAD_INTERLOCK_T1] = false;

                            NextSeq(m_nReloadTurnBackSeqNum);
                            return;
                        }
                    }
                    break;
                    #endregion
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
                GbVar.Seq.sStripTransfer.Info.bIsError = true;
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
        public bool IsCuttingTableReloadReqEventOn()
        {
            if (!IFMgr.Inst.SAW.IsTableLoadingComplete() &&
                IFMgr.Inst.SAW.IsTableReloadReq())
            {
                return true;
            }

            return false;
        }

        public bool GetStripLoadingConfirm()
        {
            if (GbVar.Seq.sCuttingTable.Info.bIsStrip && !IFMgr.Inst.SAW.IsTableLoadingComplete())
            {
                return false;
            }
            return true;
        }
    }
}
