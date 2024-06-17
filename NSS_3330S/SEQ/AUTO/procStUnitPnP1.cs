using NSS_3330S.MOTION;
using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStUnitPnP1 : SeqBase
    {
        cycPkgPickNPlace cycFunc = null;

        int m_nMapTablePickUpCount;

        string m_strHead = "MULTI PICKER HEAD 1";

        int m_nHeadNo = 0;
        int m_nSideHeadNo = 0;
        int m_nTotMapCountX = 0;
        int m_nTotMapCountY = 0;

        int m_nRetry = 0;
        public procStUnitPnP1(int nSeqID)
        {
            m_nHeadNo = CFG_DF.HEAD_1;
            m_nSideHeadNo = CFG_DF.HEAD_2;

            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo];

            cycFunc = new cycPkgPickNPlace(nSeqID, m_nHeadNo);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE seqNo)
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
                if (m_seqInfo != GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo])
                {
                    m_seqInfo = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo];
                    m_nSeqNo = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].nCurrentSeqNo;
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

            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_USE].bOptionUse == false)
            {
                if (m_nSeqNo > (int)seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_BEFORE_STANDBY_POS)
                {
                    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.INIT);
                    return;
                }
            }

            m_nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            m_nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            switch ((seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE)m_nSeqNo)
            {
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.INIT:
                    if (GbVar.LOADER_TEST)
                        return;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = false;
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_PICK_UP_POS:
                    // Picker X
                    // Picker z1... z10,  r1... r10
                    // 
                    if (m_bFirstSeqStep)
                    {
                        nFuncResult = IsInPosChipPkR(m_nHeadNo);

                        if (FNC.IsErr(nFuncResult))
                        {
                            SetError(nFuncResult);
                            return;
                        }
                        else if (FNC.IsBusy(nFuncResult)) return;
                    }
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1, m_nHeadNo, true);
                    }
                    else
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1, m_nHeadNo);
                    }

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        //
                        m_strLogMsg = string.Format("{0} MOVE ALL ZR READY POSITION COMPLETE", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                // StandBy 포지션 이동 전 SYNC_PICK_UP_START 확인하여 바로 픽업 위치로 이동할 수 있게
                // 테이블이 준비되어 있지 않을 때는 확인하지 않는다
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_BEFORE_STANDBY_POS:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_USE].bOptionUse == false)
                        {
                            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] == false)
                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = true;

                            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] == true)
                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = false;

                            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] == true)
                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] = false;

                            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] == true)
                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] = false;

                            LeaveCycle();
                            return;
                        }

                        // 테이블이 하나라도 준비되어 있으면 바로 이동 위치로
                        if (GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] ||
                            GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START])
                        {
                            // 이미 다른 헤드가 사용하고 있으면 준비 위치 이동 스텝으로 이동
                            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] == true) break;

                            // Turn 돌고 있으면 준비 위치 이동 스텝으로 이동
                            if (IsInPosChipPkR(m_nHeadNo) != FNC.SUCCESS) break;

                            // 이 헤드가 Pick Up 소유권 점유하고 바로 테이블 체크하는 스텝으로 이동
                            GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = true;
                            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = true;

                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_MAP_TABLE_INSPECTION_COMPLETED);
                            return;
                        }
                    }
                    break;

                // PICK-UP하기 위한 준비 위치 이동
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_PICK_UP_POS:
                    // TODO : 20220214 PICK-UP을  하기위한 준비위치 이동 (PICK-UP COUNT 예상)
                    // PICKER 2를 사용할 경우 PICKER PAD 사용 COUNT 만큼 +해서 미리 준비함...

                    // case 1: Table이 준비되어 있지 않을 경우
                    // case 2: Table count 가 0일 경우 (Head 2의 경우 Head 1이 Skip일 때)
                    // case 3: Head 2가 작업중일 경우 Picker Head 2 Count 합산하여 이동 (Skip pad 제외)
                    // case 4: Table 번호 미리 알고 이동
                    // delay는 X 축 이동할 경우만 사용
                    
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;

                        // 다른 테이블이 진행 중이면 진행 중인 것 포함하여 계산
                        m_nMapTablePickUpCount = GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].nMapTablePickUpCount;
                        m_nMapTablePickUpCount = 0;
                        if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START])
                        {
                            // PickUp Count Over?
                            for (int nPickPadCnt = 0; nPickPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickPadCnt++)
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_1_USE].bOptionUse == true)
                                {
                                    m_nMapTablePickUpCount++;
                                }
                            }
                        }
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                    {
                        nFuncResult = MovePosChipPickUpStandbyPosX(m_nHeadNo, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, m_nMapTablePickUpCount);
                    }
                    else
                    {
                        nFuncResult = MovePosChipPickUpStandbyPosXYZ(m_nHeadNo, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, m_nMapTablePickUpCount);
                    }

                    // [2022.05.07.kmlee] Y를 움직일 수 있는지 확인하기 위해서 강제로 성공을 주고 IS_IN_STANDBY_POS에서 이동 완료 체크를 한다
                    nFuncResult = FNC.SUCCESS;
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        m_strLogMsg = string.Format("STAGE Y{0} : {1}", cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, MotionMgr.Inst[SVDF.AXES.MAP_STG_1_Y + cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].GetRealPos().ToString("F3"));
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }

                    break;

                // 대기 위치 이동 중 2번 헤드의 작업 상태를 확인한다
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_STANDBY_POS:
                    {
                        if (!LeaveCycle())
                        {
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_PICK_UP_POS);
                            return;
                        }

                        // 다른 Head가 작업 중이라면 IS_IN_STANDBY_POS로 가서 이동 완료 확인 후 정상 시퀀스 진행
                        if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] == true)
                        {
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_IN_STANDBY_POS);
                            return;
                        }

                        GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = true;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = true;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_STAGE_Y_STANDBY_POS:
                    {
                        // 현재 진행 중인 테이블이 언로딩 상태가 아니면 Y를 움직이지 않는다
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;

                            if (GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] == false)
                                break;
                        }

                        //TODO 20220726
                        // 스테이지 Y를 미리 움직인다
                        nFuncResult = MovePosChipPrePickUpY(cycFunc.CHIP_PICKER_HEAD_NO,
                                                       0 /*cycFunc.CHIP_PICK_UP_FWD_COUNT*/,
                                                       0 /*cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT*/,
                                                       cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                                       cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                       ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                       true);
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("STAGE Y{0} : {1}", cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, MotionMgr.Inst[SVDF.AXES.MAP_STG_1_Y + cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].GetRealPos().ToString("F3"));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;

                // X축의 대기 위치 이동을 기다린다
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_IN_STANDBY_POS:
                    {
                        // 축이 움직이는지 확인
                        if (MotionMgr.Inst[SVDF.AXES.CHIP_PK_1_X].IsAlarm())
                        {
                            SetError((int)ERDF.E_SV_AMP + (int)SVDF.AXES.CHIP_PK_1_X);
                            return;
                        }

                        if (MotionMgr.Inst[SVDF.AXES.CHIP_PK_1_X].GetServoOnOff() == false)
                        {
                            SetError((int)ERDF.E_SV_SERVO_ON + (int)SVDF.AXES.CHIP_PK_1_X);
                            return;
                        }

                        if (MotionMgr.Inst[SVDF.AXES.CHIP_PK_1_X].IsInPosition() == false ||
                            MotionMgr.Inst[SVDF.AXES.CHIP_PK_1_X].IsBusy())
                        {
                            return;
                        }
                        //nFuncResult = IsInPosChipPickUpStandbyPosXYZ(m_nHeadNo, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, m_nMapTablePickUpCount);
                    }
                    break;

                // 2번 헤드의 작업 상태를 확인한다
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_PICK_UP_CYCLE_RUN:
                    
                    if (!LeaveCycle()) return;

                    // 이미 IS_OTHER_HEAD_PICK_UP_CYCLE_RUN_STANDBY_POS 스텝에서 플래그가 켜져있는 상태라면 break
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] == true)
                        break;

                    // Todo ; 20211028 bhoh
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] == true) return;

                    GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = true;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = true;

                    break;

                // 테이블 Pick Up할 수 있는 상태 확인. MAP INSPECTION 완료 상태
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_MAP_TABLE_INSPECTION_COMPLETED:
                    if (LeaveCycle() == false) return;

                    // PnP Head #1에서만 소스작업
                    if (GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] == true &&
                        GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] == false)
                    {
                        GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] = true;

                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO = 0;

                        m_strLogMsg = string.Format("{0} LEFT MAP VISION TABLE UNLOADING START", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        break;
                    }
                    else if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] == true &&
                             GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] == false)
                    {
                        GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] = true;

                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO = 1;

                        m_strLogMsg = string.Format("{0} RIGHT MAP VISION TABLE UNLOADING START", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        break;
                    }

                    return;

                // Pick Up 준비
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_PICKER_PICK_UP_CONFIRM:
                    
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].ResetPickerHeadInfo();
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].PickerUpCountClear();

                    m_strLogMsg = string.Format("{0} WAIT PICKER PICK UP CONFIRM DONE", m_strHead);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    break;

                #region F-PickUp Process
                // Reverse Pick Up 확인
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_TURN:
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp == true)
                    {
                        m_strLogMsg = string.Format("{0} FORWARD PICK UP REVERS TURN", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_MAP_TABLE_GROUP_COUNT_OVER);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_CHECKING:

                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].isExistSkipUnit == true &&
                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse)
                    {
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].isExistSkipUnit = false;
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;
                    }

                    if (cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 &&
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT % m_nTotMapCountX == 0 &&
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT != 0)
                    {
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT = 0;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp = true;

                        //m_strLogMsg = string.Format("{0} BACKWARD PICK UP MODE START [COL COUNT: {1} REVERSE PICKUP: {2}]",
                        //    m_strHead, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT, GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_TURN);
                        return;
                    }

                    m_strLogMsg = string.Format("{0} FORWARD PICK UP MODE START [COL COUNT: {1} REVERSE PICKUP: {2}]",
                        m_strHead, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT, GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_MAP_TABLE_GROUP_COUNT_OVER:
                    break;

                // 테이블에 모두 내려놓았는지 확인
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_MAP_TABLE_ARRAY_COUNT_OVER:

                    if ((m_nTotMapCountX * m_nTotMapCountY) <= cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT &&
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT != 0)
                    {
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] = false;

                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] = false;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_COMPLETE] = true;
                        
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].SetProcCycleOutTime((int)MCC.VISION_TABLE1 + cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO);

						// SYNC_PICK UP READY는 HEAD 1만
                        GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = false;

                        if (cycFunc.CHIP_PICK_UP_FWD_COUNT + cycFunc.CHIP_PICK_UP_SET_BWD_COUNT > 0)
                        {
                            // MapTable Unit 모두 PickUp 함...
                            // Inspection 로 이동...
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = true;
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = true;

                            m_strLogMsg = string.Format("{0} FORWARD PICK UP FINISH AND INSPECTION START [FWD COUNT: {1} SET BWD COUNT: {2}]",
                                m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            // BOTTOM INSPECTION 시작
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);
                            return;
                        }
                        else
                        {
                            m_strLogMsg = string.Format("{0} FORWARD PICK UP FINISH [FWD COUNT: {1} SET BWD COUNT: {2}]",
                                m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            // 종료
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FINISH);
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_PICK_UP_COUNT_OVER:
                    if ((cycFunc.CHIP_PICK_UP_FWD_COUNT + cycFunc.CHIP_PICK_UP_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        // Check MapStage PickUp Count
                        // MapTable Loading  Pos로 빨리 돌라갈  수 있도록
                        if ((m_nTotMapCountX * m_nTotMapCountY) <= cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT)
                        {
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = true;
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = true;
                        }

                        m_strLogMsg = string.Format("{0} FORWARD PICK UP COUNT OVER [{1} * {2} >= {3}]",
                            m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);
                        return;
                    }
                    break;
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_PICK_UP_SKIP:
                    if (IsPickerPadSkip(true, m_nHeadNo, cycFunc.CHIP_PICK_UP_FWD_COUNT))
                    {
                        m_strLogMsg = string.Format("{0} FORWARD PICK UP PAD SKIP [PAD NO: {1}]",
                            m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        cycFunc.CHIP_PICK_UP_FWD_COUNT++;
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_PICK_UP_COUNT_OVER);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_MAP_PICK_UP_UNIT_EMPTY:
                    //TODO 20220726:
                    if (!IsMapPickUpUnit(true, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT))
                    {
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;
                        //cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;

                        //m_strLogMsg = string.Format("{0} FORWARD PICK UP COL COUNT UP [TABLE NO: {1} TABLE COUNT: {2} COL COUNT: {3}]",
                        //    m_strHead, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_CHECKING);
                        return;
                    }
                    break;

                #region F-Loading Cycle
                // 실제 PICK-UP 한다
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_F_UNIT_PICK_UP:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if(GbVar.CHIP_PICKER_PICK_UP_PAUSE)
                            {
                                LeaveCycle();
                                return;
                            }
                        }

                        nFuncResult = cycFunc.FDirPickUp();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} FORWARD PICK UP COMPLETE [TABLE NO: {1} TABLE COUNT: {2} COL COUNT: {3} FWD_COUNT: {4}]",
                                                        m_strHead,
                                                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                                        cycFunc.CHIP_PICK_UP_FWD_COUNT);

                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            SetPickerUnitInfoFromMapTable(true,
                                                      cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                                      cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                      cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                                      m_nHeadNo,
                                                      cycFunc.CHIP_PICK_UP_FWD_COUNT);

                            SetMapPickUpUnit(true,
                                             cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                             cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                             cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                             false);

                            cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                            cycFunc.CHIP_PICK_UP_FWD_COUNT++;
                            cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;

                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PICKUP_COUNT++;

                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_CHECKING);
                            return;
                        }
                        else if(nFuncResult >= (int)ERDF.E_X1_PK_VAC_1_NOT_ON && nFuncResult <= (int)ERDF.E_X2_PK_VAC_8_NOT_ON)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse == true)
                            {
                                SetError(nFuncResult);
                                SetSkipParam(true, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, m_nHeadNo);
                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FWD_PICK_UP_REVS_CHECKING);
                                return;
                            }
                        }
                        else if(nFuncResult >= (int)ERDF.CHIP_PK1_PAD1_SKIP && nFuncResult <= (int)ERDF.CHIP_PK2_PAD8_SKIP)
                        {
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_PICK_UP_SKIP);
                            return;
                        }
                    }
                    break;
  
                #endregion
                #endregion

                #region Bwd PickUp Process
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_TURN:
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp == false)
                    {
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_F_MAP_TABLE_GROUP_COUNT_OVER);

                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP REVERS TURN", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_CHECKING:

                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].isExistSkipUnit == true &&
                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse)
                    {
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].isExistSkipUnit = false;
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;
                    }
                    if (cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 && 
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT % m_nTotMapCountX == 0 &&
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT != 0)
                    {
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT = 0;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp = false;

                        m_strLogMsg = string.Format("{0} FORWARD PICK UP MODE START [CHIP COUNT: {1} REVERSE PICKUP: {2}]",
                            m_strHead, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT, GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_TURN);
                        return;
                    }
                    
                    m_strLogMsg = string.Format("{0} BACKWARD PICK UP MODE START [CHIP COUNT: {1} REVERSE PICKUP: {2}]",
                        m_strHead, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT, GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPickUp);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_MAP_TABLE_GROUP_COUNT_OVER:
                    break;
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_MAP_TABLE_ARRAY_COUNT_OVER:

                    if ((m_nTotMapCountX * m_nTotMapCountY) <= cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT &&
                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT != 0)
                    {
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] = false;

                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] = false;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_COMPLETE] = true;
                        
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].SetProcCycleOutTime((int)MCC.VISION_TABLE1 + cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO);

                        GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = false;
                       
                        if (cycFunc.CHIP_PICK_UP_FWD_COUNT + cycFunc.CHIP_PICK_UP_SET_BWD_COUNT > 0)
                        {
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = true;
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = true;

                            m_strLogMsg = string.Format("{0} BACKWARD PICK UP FINISH AND INSPECTION START [FWD COUNT: {1} SET BWD COUNT: {2}]",
                                m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);
                            return;
                        }
                        else
                        {
                            m_strLogMsg = string.Format("{0} BACKWARD PICK UP FINISH [FWD COUNT: {1} SET BWD COUNT: {2}]",
                                m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FINISH);
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_PICK_UP_COUNT_OVER:
                    if ((cycFunc.CHIP_PICK_UP_FWD_COUNT + cycFunc.CHIP_PICK_UP_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        // Check MapStage PickUp Count
                        // MapTable Loading  Pos로 빨리 돌라갈  수 있도록
                        if ((m_nTotMapCountX * m_nTotMapCountY) <= cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT)
                        {
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = true;
                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = true;
                        }

                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP COUNT OVER [{1} * {2} >= {3}]",
                            m_strHead, m_nTotMapCountX, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_PICK_UP_SKIP:
                    if (IsPickerPadSkip(false, m_nHeadNo, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT))
                    {
                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP PAD SKIP [PAD NO: {1}]",
                            m_strHead, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        cycFunc.CHIP_PICK_UP_SET_BWD_COUNT++;
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_PICK_UP_COUNT_OVER);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_MAP_PICK_UP_UNIT_EMPTY:
                    if (!IsMapPickUpUnit(false, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT))
                    {
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;
                        //cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;

                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP COL COUNT UP [TABLE NO: {1} TABLE COUNT: {2} COL COUNT: {3}]",
                            m_strHead, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT, cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_CHECKING);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_R_UNIT_PICK_UP:

                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.CHIP_PICKER_PICK_UP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }
                    }

                    nFuncResult = cycFunc.RDirPickUp();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP COMPLETE [TABLE NO: {1} TABLE COUNT: {2} COL COUNT: {3} GET_BWD_COUNT: {4}]",
                                                    m_strHead, 
                                                    cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, 
                                                    cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT, 
                                                    cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                                    cycFunc.CHIP_PICK_UP_GET_BWD_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        SetPickerUnitInfoFromMapTable(false,
                                                      cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                                      cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                      cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                                      m_nHeadNo,
                                                      cycFunc.CHIP_PICK_UP_GET_BWD_COUNT);

                        SetMapPickUpUnit(false,
                                         cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                         cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                         cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT,
                                         false);

                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                        cycFunc.CHIP_PICK_UP_SET_BWD_COUNT++;
                        cycFunc.CHIP_PICKER_PICK_UP_COL_COUNT++;

                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PICKUP_COUNT++;
                        //배출할때 체크
                        //GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_CHECKING);
                        return;
                    }
                    else if (nFuncResult >= (int)ERDF.E_X1_PK_VAC_1_NOT_ON && nFuncResult <= (int)ERDF.E_X2_PK_VAC_8_NOT_ON)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse == true)
                        {
                            SetError(nFuncResult);
                            SetSkipParam(false, cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, m_nHeadNo);
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PICK_UP_REVS_CHECKING);
                            return;
                        }
                    }
                    else if (nFuncResult >= (int)ERDF.CHIP_PK1_PAD1_SKIP && nFuncResult <= (int)ERDF.CHIP_PK2_PAD8_SKIP)
                    {
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_R_PICK_UP_SKIP);
                        return;
                    }
                    break;
                 
                #endregion


                ///////////////////////////////////////////////////////
                // BOTTOM INSPECTION START
                ///////////////////////////////////////////////////////
                #region BOTTOM INSPECTION
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN:
                    {
                        if (!LeaveCycle()) return;

                        //#region 피커 체크 하기 2022 10 07 HEP
                        //if ((m_nTotMapCountX * m_nTotMapCountY) <= cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT &&
                        //cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT != 0)
                        //{
                        //    GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] = false;

                        //    GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] = false;
                        //    GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_COMPLETE] = true;

                        //    GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].SetProcCycleOutTime((int)MCC.VISION_TABLE1 + cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO);

                        //    // SYNC_PICK UP READY는 HEAD 1만
                        //    GbVar.Seq.sPkgPickNPlace.pInfo[CFG_DF.HEAD_1].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_READY] = false;

                        //    if (cycFunc.CHIP_PICK_UP_FWD_COUNT + cycFunc.CHIP_PICK_UP_SET_BWD_COUNT > 0)
                        //    {
                        //        // MapTable Unit 모두 PickUp 함...
                        //        // Inspection 로 이동...
                        //        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = true;
                        //        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = true;

                        //        m_strLogMsg = string.Format("{0} FORWARD PICK UP FINISH AND INSPECTION START [FWD COUNT: {1} SET BWD COUNT: {2}]",
                        //            m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                        //        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        //        //// BOTTOM INSPECTION 시작
                        //        //NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);
                        //        //return;
                        //    }
                        //    else
                        //    {
                        //        m_strLogMsg = string.Format("{0} FORWARD PICK UP FINISH [FWD COUNT: {1} SET BWD COUNT: {2}]",
                        //            m_strHead, cycFunc.CHIP_PICK_UP_FWD_COUNT, cycFunc.CHIP_PICK_UP_SET_BWD_COUNT);
                        //        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        //        // 종료
                        //        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FINISH);
                        //        return;
                        //    }
                        //}
                        //#endregion

                        if (!LeaveCycle()) return;

                        if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] == true) return;

                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = false;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] = true;
                        SeqHistory("IS_OTHER_HEAD_INSP_CYCLE_RUN SYNC_INSPECTION_START true");
                    }
                    break;

#if !_NEW_BTM_INSPECTION
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_VISION_IF_IO_STATUS://220610 pjh
                    {
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].InspectionCountClear();

                        ////상태 확인
                        //if (GbVar.GB_INPUT[(int)IODF.INPUT.BALL_CNT_RESET] == 1 || 
                        //    GbVar.GB_INPUT[(int)IODF.INPUT.BALL_INSP_COMP] == 1 ||
                        //    GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_CNT_RESET] == 1 ||
                        //    GbVar.GB_OUTPUT[(int)IODF.OUTPUT.BALL_INSP_COMP] == 1)
                        //{
                            //툴중 하나라도 켜져 있으면 둘다 끄고  WaitDelay(100)
                            IFMgr.Inst.VISION.SetBallInspCountReset(false);
                            IFMgr.Inst.VISION.SetBallInspCountComplete(false);

                        //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_PICKER_INSPECTION_CONFIRM);
                        //    return;
                        //}
                        //else
                        //{
                        //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_BTM_INSPECTION_USE);
                        //    return;
                        //}
                    }

                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_PICKER_INSPECTION_CONFIRM://220610 pjh
                    {
                        if (WaitDelay(100))
                        {
                            nFuncResult = FNC.BUSY;
                        }
                        else
                        {
                            nFuncResult = FNC.SUCCESS;

                            m_strLogMsg = string.Format("{0} WAIT INSPECTION CONFIRM DONE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }

                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_BTM_INSPECTION_USE:
                    {
                        // 사용하지 않으면 스킵 코드 추가 필요 (추 후 사이즈가 큰 자재가 들어올 경우 하부 비전을 사용할 수 없다)
                        // 하부 인스펙션의 결과값은 IFMgr.Inst.VISION.GetBallInspResult 내부에서 처리
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse)
                        {
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.GET_BTM_INSPECTION_RESULT);
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_BALL_INSP_READY:
                    {
                        if (!IFMgr.Inst.VISION.IsVisionReady)
                        {
                            SetError((int)ERDF.E_VISION_PROGRAM_IS_NOT_READY);
                            return;
                        }

                        m_strLogMsg = string.Format("{0} BALL INSPECTION READY CHECK", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BALL_INSP_INTF_INIT_REQ:
                    {
                        IFMgr.Inst.VISION.SetBallHeadNo(MCDF.eUNIT.NO1);
                        IFMgr.Inst.VISION.SetBallInspCountReset(true);

                        m_strLogMsg = string.Format("{0} BALL INSPECTION INIT REQUEST COMPLETE", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BALL_INSP_INTF_INIT_REP:
                    {
                        if (!LeaveCycle()) return;

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetry++;

                                m_strLogMsg = string.Format("{0} BALL INSPECTION BallCountReset IS NOT ON. RETRY : {1}", m_strHead, m_nRetry);
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_VISION_IF_IO_STATUS);//220610 pjh
                                return;
                            }
                            m_nRetry = 0;
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_ON);

                            //220609
                            //타임 아웃 알람 발생 시 신호, 카운트 리셋 부터 시작하기
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_VISION_IF_IO_STATUS);//220610 pjh
                            return;
                        }

                        if (!IFMgr.Inst.VISION.IsBallCountReset) return;
                        m_nRetry = 0;
                        // [2022.04.13.kmlee] 바로 끄지않고 Complete 주기 전 Req OFF
                        //IFMgr.Inst.VISION.SetBallInspCountReset(false);

                        m_strLogMsg = string.Format("{0} BALL INSPECTION INIT REPLY CHECK COMPLETE", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_VISION_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            nFuncResult = IsInPosChipPkR(m_nHeadNo);

                            if (FNC.IsErr(nFuncResult))
                            {
                                SetError(nFuncResult);
                                return;
                            }
                            else if (FNC.IsBusy(nFuncResult)) return;
                        }

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                        {
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, m_nHeadNo, true);
                        }
                        else
                        {
                            // Inspection ready pos move
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, m_nHeadNo);
                        }
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} MOVE ALL Z AXIS TO VISION REFERENCE POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_ALL_XYZ_AXIS_VISION_POS:
                    {
                       
                        //nFuncResult = MovePosBallVisionXYZ(POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + m_nHeadNo, m_nHeadNo);

                        nFuncResult = MovePosBallVisionXYZT(POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + m_nHeadNo, m_nHeadNo); //2023-01-10 sj.shin 칭허 적용

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} MOVE ALL XYZ AXIS TO VISION POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_COUNT_OVER:
                    {
                        if (cycFunc.CHIP_PICKER_INSPECTION_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                        {
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.REQ_BTM_INSPECTION_COMPLETE);

                            m_strLogMsg = string.Format("{0} BOTTOM INSPECTION COMPLETE [{1} >= {2}]",
                                m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_SKIP:
                    if (GbVar.BTM_INSP_PASE)
                    {
                        LeaveCycle();
                        return;
                    }
                    // Skip이 되어 있어도 검사를 진행해야 함... Vision Inspection count 와 Match가 안됨...
                    //if (IsPickerPadSkip(true, m_nHeadNo, cycFunc.CHIP_PICKER_INSPECTION_COUNT))
                    //{
                    //    cycFunc.CHIP_PICKER_INSPECTION_COUNT++;
                    //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_COUNT_OVER);
                    //    return;
                    //}
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_UNIT_EMPTY:
                    //if (GbVar.Seq.sPkgPickNPlace[m_nHeadNo].IsPickerEmptyUnit(cycFunc.CHIP_PICKER_INSPECTION_COUNT))
                    //{
                    //    cycFunc.CHIP_PICKER_INSPECTION_COUNT++;
                    //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_COUNT_OVER);
                    //    return;
                    //}
                    break;
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_BTM_VISION_INSPECTION_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse)
                            {
                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.GET_BTM_INSPECTION_RESULT);
                                return;
                            }
                        }
                        nFuncResult = cycFunc.BtmInspection();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} BOTTOM INSPECTION TRIGGER ON [INSPECTION COUNT: {1}]",
                                m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BALL_INSP_TRIG_ON:
                    {
                        IFMgr.Inst.VISION.TrgBallOneShot();

                        cycFunc.CHIP_PICKER_INSPECTION_COUNT++;
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_INSP_COUNT_OVER);
                        return;
                    }

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.REQ_BTM_INSPECTION_COMPLETE:
                    {
						//220608
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetry++;

                                m_strLogMsg = string.Format("{0} BALL INSPECTION BallInspCompleted IS NOT ON. RETRY : {1}", m_strHead, m_nRetry);
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_VISION_IF_IO_STATUS);
                                return;
                            }
                            m_nRetry = 0;
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_ON);
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);

                            m_strLogMsg = string.Format("{0} BOTTOM INSPECTION TIMEOUT ERROR OCCURED [INSPECTION COUNT: {1}]",
                                m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                            return;
                        }

                        if (!LeaveCycle()) return;

                        //sj.shin 비전 검사 결과 응답 신호 확인
#if _NOTEBOOK
                        // sj.shin 시뮬레이션 기능 확인
#else
                        if (!IFMgr.Inst.VISION.IsBallInspCompleted)
                            return;
#endif
                        IFMgr.Inst.VISION.SetBallInspCountComplete(true);
                        m_nRetry = 0;
                        m_strLogMsg = string.Format("{0} BOTTOM INSPECTION VISION REPLY ON CHECK DONE [INSPECTION COUNT: {1}]",
                            m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_REPLY_BTM_INSPECTION_COMPLETE:
                    {
						//220608
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetry++;

                                m_strLogMsg = string.Format("{0} BALL INSPECTION BallInspCompleted IS NOT OFF. RETRY : {1}", m_strHead, m_nRetry);
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CHECK_VISION_IF_IO_STATUS);
                                return;
                            }
                            m_nRetry = 0;
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_OFF);
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_INSP_CYCLE_RUN);

                            m_strLogMsg = string.Format("{0} BOTTOM INSPECTION TIMEOUT ERROR OCCURED [INSPECTION COUNT: {1}]",
                                m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                            return;
                        }

#if _NOTEBOOK
                        // sj.shin 시뮬레이션 기능 확인
#else
                        if (IFMgr.Inst.VISION.IsBallInspCompleted)
                            return;
#endif
                        // [2022.04.28.kmlee] Complete 주고나서 Reset OFF
                        IFMgr.Inst.VISION.SetBallInspCountReset(false);
                        IFMgr.Inst.VISION.SetBallInspCountComplete(false);
                        m_nRetry = 0;

                        m_strLogMsg = string.Format("{0} BOTTOM INSPECTION VISION REPLY OFF CHECK DONE [INSPECTION COUNT: {1}]",
                            m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;
#else
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_BTM_VISION_INSPECTION:
                    {
                        nFuncResult = cycFunc.NewBtmInspection();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} BOTTOM INSPECTION TRIGGER ON [INSPECTION COUNT: {1}]",
                                m_strHead, cycFunc.CHIP_PICKER_INSPECTION_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;
#endif

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.GET_BTM_INSPECTION_RESULT:
                    {
                        nFuncResult = IFMgr.Inst.VISION.GetBallInspResult(m_nHeadNo);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.VISION.IncreaseBallInspResultCount(m_nHeadNo);
                            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] = false;
                        }
                        else if(FNC.IsErr(nFuncResult))
                        {
                            SetError(nFuncResult);
#if _NEW_BTM_INSPECTION
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_BTM_VISION_INSPECTION);
#else
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_PICKER_INSPECTION_CONFIRM);
#endif
                            return;
                        }
                    }
                    break;
#endregion
                ///////////////////////////////////////////////////////
                // BOTTOM INSPECTION END
                ///////////////////////////////////////////////////////


                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_GD_UNLOAD_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            nFuncResult = IsInPosChipPkR(m_nHeadNo);

                            if (FNC.IsErr(nFuncResult))
                            {
                                SetError(nFuncResult);
                                return;
                            }
                            else if (FNC.IsBusy(nFuncResult)) return;
                        }

                        // Place ready pos move
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                        {
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1, m_nHeadNo, true);
                        }
                        else
                        {
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1, m_nHeadNo);
                        }

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} ALL Z MOVE TO UNLOAD POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_GD_PLACE_CYCLE_RUN:
                    {
                        if (!LeaveCycle()) return;


                        if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] == true) return;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] = true;
                    }
                    break;

                // 상대 Picker Head가 가동중인지 확인을 여기 전에 해야 할 수 있음...
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_GODDD_TRAY_TABLE_READY:

                    // 2번이 런 중인지 확인하지 않아야 함
                    //if (!GbVar.mcState.IsCycleRun(SEQ_ID.PICK_N_PLACE_2))
                    {
                        if (!LeaveCycle()) return;
                    }

                    // 트레이 테이블이 준비 완료될 때까지 대기
                    // 무한 대기 중이라면 GDTrayTable1,2의 시퀀스 번호가 각각 30, 64라면 Config의 GOOD_TRAY_STAGE_SIZE를 체크해봐야한다
                    if (GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                    {
                        cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO = 0;
                        GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_START] = true;

                        //NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_GD_PLACE_CYCLE_RUN);

                        m_strLogMsg = string.Format("{0} GOOD TRAY TABLE 1 READY", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        break;
                    }

                    if (GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                    {
                        cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO = 1;
                        GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_START] = true;

                        //NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_GD_PLACE_CYCLE_RUN);

                        m_strLogMsg = string.Format("{0} GOOD TRAY TABLE 2 READY", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        break;
                    }
                    return;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_GD_UNLOAD_POS:
                    // Place ready pos move
                    // case 1: Table이 준비되어 있지 않을 경우
                    // case 2: Table count 가 0일 경우 (Head 2의 경우 Head 1이 Skip일 때)
                    // case 3: Head 2가 작업중일 경우 Picker Head 2 Count 합산하여 이동 (Skip pad 제외)
                    // case 4: Table 번호 미리 알고 이동
                    // delay는 X 축 이동할 경우만 사용

                    //int nGDTrayTablePlaceCnt = GbVar.Seq.sUldGDTrayTable[cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO].nUnitPlaceCount;
                    //if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START])
                    //{
                    //    // PickUp Count Over?
                    //    for (int nPickPadCnt = 0; nPickPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickPadCnt++)
                    //    {
                    //        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_1_SKIP].bOptionUse == false)
                    //        {
                    //            nGDTrayTablePlaceCnt++;
                    //        }
                    //    }
                    //}

                    //nFuncResult = MovePosChipGDPlaceStandbyPosXYZ(m_nHeadNo, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO, nGDTrayTablePlaceCnt);

                    //if (FNC.IsSuccess(nFuncResult))
                    //{
                    //    //
                    //}

                    //nFuncResult = MovePosChipPkX(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO, m_nHeadNo);

                    //if (FNC.IsSuccess(nFuncResult))
                    //{

                    //}
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_PICKER_PLACE_CONFIRM:
                    {
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].PlaceCountClear();
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bRevPlace = true;

                        m_strLogMsg = string.Format("{0} WAIT PICKER PLACE CONFIRM DONE", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                ///////////////////////////////////////////////////////
                // GOOD PLACE START
                ///////////////////////////////////////////////////////
                #region Good Place
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_TRAY_COUNT_OVER:

                    if ((RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX * RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY) <= cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT)
                    {
                        // Tray 만재
                        GbVar.Seq.sUldGDTrayTable[cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY] = false;
                        GbVar.Seq.sUldGDTrayTable[cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_START] = false;
                        GbVar.Seq.sUldGDTrayTable[cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;

                        if (cycFunc.CHIP_PLACE_FWD_COUNT + cycFunc.CHIP_PLACE_SET_BWD_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                        {
                            m_strLogMsg = string.Format("{0} BACKWARD PLACE GOOD TRAY COUNT OVER CHECK NEXT REWORK TRAY TABLE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

							// [2022.05.14.kmlee] 오이사님 요청
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_RW_UNLOAD_POS);
                            return;
                        }
                        else
                        {
                            m_strLogMsg = string.Format("{0} BACKWARD PLACE GOOD TRAY COUNT OVER CHECK NEXT GOOD TRAY TABLE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_GODDD_TRAY_TABLE_READY);
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_PLACE_COUNT_OVER:
                    if (cycFunc.CHIP_PLACE_FWD_COUNT + cycFunc.CHIP_PLACE_SET_BWD_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        m_strLogMsg = string.Format("{0} BACKWARD PICK UP COL COUNT UP [{1} + {2} >= {3}]",
                            m_strHead, cycFunc.CHIP_PLACE_FWD_COUNT, cycFunc.CHIP_PLACE_SET_BWD_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

						// [2022.05.14.kmlee] 오이사님 요청
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_RW_UNLOAD_POS);
                        return;
                    }
                    break;

                // 패드 스킵 여부 확인
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_PLACE_SKIP:
                    {
                        if (IsPickerPadSkip(false, CFG_DF.HEAD_1, cycFunc.CHIP_PLACE_SET_BWD_COUNT))
                        {
                            m_strLogMsg = string.Format("{0} BACKWARD GOOD PLACE PAD SKIP [PAD NO: {1}]",
                                m_strHead, cycFunc.CHIP_PLACE_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            cycFunc.CHIP_PLACE_SET_BWD_COUNT++;
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_PLACE_COUNT_OVER);
                            return;
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_NOT_GOOD_UNIT:
                    {
                        int nRvsGdPlaceCnt = CFG_DF.MAX_PICKER_PAD_CNT - cycFunc.CHIP_PLACE_SET_BWD_COUNT - 1;

                        if (!GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].IsGoodUnit(false, cycFunc.CHIP_PLACE_SET_BWD_COUNT))
                        {
                            m_strLogMsg = string.Format("PLACE IS NOT GOOD UNIT INFO [PICKER NO {0}({1})] - HOST CODE: {2}, TOP: {3}({4}), BTM: {5}({6})",
                                                    nRvsGdPlaceCnt,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].IS_UNIT == true ? "O" : "X",
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].ITS_XOUT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].TOP_INSP_RESULT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].TOP_NG_CODE,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].BTM_INSP_RESULT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].BTM_NG_CODE);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            cycFunc.CHIP_PLACE_SET_BWD_COUNT++;
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_PLACE_COUNT_OVER);
                            return;
                        }


                        m_strLogMsg = string.Format("PLACE IS GOOD UNIT INFO [PICKER NO {0}] - HOST CODE: {1}, TOP: {2}({3}), BTM: {4}({5})",
                                                    nRvsGdPlaceCnt,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].ITS_XOUT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].TOP_INSP_RESULT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].TOP_NG_CODE,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].BTM_INSP_RESULT,
                                                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsGdPlaceCnt].BTM_NG_CODE);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_TRAY_UNIT_EMPTY:
                    {
                        //if (IsGoodTrayUnit(false, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, cycFunc.CHIP_PICKER_PLACE_GOOD_COL_COUNT))
                        //{
                        //    m_strLogMsg = string.Format("{0} BACKWARD PICK UP COL COUNT UP [TABLE NO: {1} TABLE COUNT: {2} COL COUNT: {3}]",
                        //        m_strHead, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, cycFunc.CHIP_PICKER_PLACE_GOOD_COL_COUNT);
                        //    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        //    cycFunc.CHIP_PICKER_PLACE_GOOD_COL_COUNT++;
                        //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.BWD_PLACE_REVS_CHECKING);
                        //    return;
                        //}
                    }
                    break;

                // 실제 내려놓는 싸이클
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_GOOD_UNIT_PLACE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.CHIP_PICKER_PLACE_PAUSE)
                            {
                                LeaveCycle();
                                return;
                            }
                        }

                        nFuncResult = cycFunc.PlaceToGoodTrayR();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} BACKWARD GOOD PLACE COMPLETE [TABLE NO: {1} TABLE COUNT: {2} PAD COUNT: {3}]",
                                m_strHead, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO, cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, cycFunc.CHIP_PLACE_SET_BWD_COUNT);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            SetGoodTrayUnit(false,
                                        cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                        cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                        true);

                            cycFunc.CHIP_PICKER_PLACE_GOOD_TABLE_COUNT++;
                            cycFunc.CHIP_PLACE_SET_BWD_COUNT++;

                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PLACE_COUNT++;
                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_OK_COUNT++;
                            //배출할때 체크
                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                            GbVar.product.nTotalCnt++;

                            GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.GOOD_UNIT++;
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_GOOD_TRAY_COUNT_OVER);
                            return;
                        }
                    }
                    break;

                #endregion
                ///////////////////////////////////////////////////////
                // BWD GOOD PLACE END
                ///////////////////////////////////////////////////////
				
                ///////////////////////////////////////////////////////
                // REWORK PLACE START
                ///////////////////////////////////////////////////////
                #region Rework Place
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_RW_UNLOAD_POS:
                    {
                        if (m_bFirstSeqStep)
                        {
                            nFuncResult = IsInPosChipPkR(m_nHeadNo);

                            if (FNC.IsErr(nFuncResult))
                            {
                                SetError(nFuncResult);
                                return;
                            }
                            else if (FNC.IsBusy(nFuncResult)) return;
                        }

                        // Place ready pos move
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                        {
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF, m_nHeadNo, true);
                        }
                        else
                        {
                            nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF, m_nHeadNo);
                        }

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} ALL Z MOVE TO UNLOAD POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_RW_PLACE_CYCLE_RUN:
                    {
                        if (!LeaveCycle()) return;

                        if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] == true) return;

                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] = false;
                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] = true;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_REWORK_TRAY_TABLE_READY:
                    {
                        if (!LeaveCycle()) return;

                        // 해당 피커유닛 존재여부 확인
                        if (!GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].IsPickerReworkUnitExist())
                        {
                            m_strLogMsg = string.Format("{0} REWORK UNIT EXIST CHECKED", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

							// [2022.05.14.kmlee] 오이사님 요청
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_NG_POS);
                            return;
                        }

                        if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_READY])
                        {
                            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].PlaceCountClear();

                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_START] = true;
                            NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_RW_UNLOAD_POS);
                            return;
                        }
                        return;
                    }

                
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_RW_UNLOAD_POS:
                    // Place ready pos move
                    //nFuncResult = MovePosChipPkX(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF, m_nHeadNo);

                    //if (FNC.IsSuccess(nFuncResult))
                    //{
                    //    //
                    //}
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_TRAY_COUNT_OVER:
                    {
                        if ((RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX * RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY) <= cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT)
                        {
                            // Tray 만재
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_READY] = false;
                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_SORT_OUT_START] = false;

                            GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldReworkTrayTable.TRAY_UNIT_FULL] = true;

                            if (cycFunc.CHIP_PLACE_FWD_COUNT + cycFunc.CHIP_PLACE_SET_BWD_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                            {

                                m_strLogMsg = string.Format("{0} REWORK TRAY COUNT OVER [{1} * {2} <= {3}]",
                                    m_strHead, RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX, RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY, cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT);
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
								
								// [2022.05.14.kmlee] 오이사님 요청
                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_NG_POS);
                                return;
                            }
                            else
                            {

                                m_strLogMsg = string.Format("{0} REWORK TRAY COUNT OVER [{1} * {2} <= {3}] [{4} < 9]",
                                    m_strHead, RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX, RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY, cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT, cycFunc.CHIP_PLACE_SET_BWD_COUNT);
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                                NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_REWORK_TRAY_TABLE_READY);
                                return;
                            }
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_PLACE_COUNT_OVER:
                    if (cycFunc.CHIP_PLACE_SET_BWD_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        // Check MapStage PickUp Count
                        // MapTable Loading  Pos로 빨리 돌라갈  수 있도록
						// [2022.05.14.kmlee] 오이사님 요청
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_NG_POS);

                        m_strLogMsg = string.Format("{0} REWORK TRAY PLACE COUNT OVER [{1} >= {2}]",
                            m_strHead, cycFunc.CHIP_PLACE_SET_BWD_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_PLACE_SKIP:
                    if (IsPickerPadSkip(false, m_nHeadNo, cycFunc.CHIP_PLACE_SET_BWD_COUNT))
                    {
                        m_strLogMsg = string.Format("{0} REWORK PLACE PAD SKIP [PAD NO: {1}]",
                            m_strHead, cycFunc.CHIP_PLACE_SET_BWD_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        cycFunc.CHIP_PLACE_SET_BWD_COUNT++;
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_PLACE_COUNT_OVER);
                        return;
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_NOT_REWORK_UNIT:
                    int nRvsRwPlaceCnt = CFG_DF.MAX_PICKER_PAD_CNT - cycFunc.CHIP_PLACE_SET_BWD_COUNT - 1;

                    if (!GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].IsReworkUnit(cycFunc.CHIP_PLACE_SET_BWD_COUNT))
                    {
                        m_strLogMsg = string.Format("PLACE IS NOT REWORK UNIT INFO [PICKER NO {0}] - HOST CODE: {1}, TOP: {2}({3}), BTM: {4}({5})",
                                                nRvsRwPlaceCnt,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].ITS_XOUT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].TOP_INSP_RESULT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].TOP_NG_CODE,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].BTM_INSP_RESULT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].BTM_NG_CODE);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        cycFunc.CHIP_PLACE_SET_BWD_COUNT++;
                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_PLACE_COUNT_OVER);
                        return;
                    }

                    
                    m_strLogMsg = string.Format("PLACE IS REWORK UNIT INFO [PICKER NO {0}] - HOST CODE: {1}, TOP: {2}({3}), BTM: {4}({5})",
                                                nRvsRwPlaceCnt,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].ITS_XOUT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].TOP_INSP_RESULT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].TOP_NG_CODE,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].BTM_INSP_RESULT,
                                                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nRvsRwPlaceCnt].BTM_NG_CODE);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_TRAY_UNIT_EMPTY:
                    //if (IsReworkTrayUnit(cycFunc.CHIP_PICKER_PLACE_REWORK_COL_COUNT))
                    //if (IsReworkTrayUnit(cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT, cycFunc.CHIP_PICKER_PLACE_REWORK_COL_COUNT))
                    //{
                    //    SeqHistory("IS_REWORK_TRAY_UNIT_EMPTY [TABLE COUNT : {0}, COL COUNT {1} -> {2}",
                    //                                cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT,
                    //                                cycFunc.CHIP_PICKER_PLACE_REWORK_COL_COUNT,
                    //                                cycFunc.CHIP_PICKER_PLACE_REWORK_COL_COUNT + 1);
                    //    cycFunc.CHIP_PICKER_PLACE_REWORK_COL_COUNT++;
                    //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_TRAY_COUNT_OVER);
                    //    return;
                    //}
                    break;

                // REWORK 트레이 PLACE
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.CYC_REWORK_UNIT_PLACE:

                    if (m_bFirstSeqStep)
                    {
                        if (GbVar.CHIP_PICKER_PLACE_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }
                    }

                    nFuncResult = cycFunc.PlaceToReworkTray();
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        m_strLogMsg = string.Format("{0} PLACE TO REWORK TRAY COMPLETE [REWORK TABLE COUNT: {1} PAD NO: {2}]",
                            m_strHead, cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT, cycFunc.CHIP_PLACE_SET_BWD_COUNT);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        SetReworkTrayUnit(cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT,
                                          true);

                        cycFunc.CHIP_PICKER_PLACE_REWORK_TABLE_COUNT++;
                        cycFunc.CHIP_PLACE_SET_BWD_COUNT++;

                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].MULTI_PICKER_1_PLACE_COUNT++;
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_RW_COUNT++;
                        //배출할때 체크
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;
                        GbVar.product.nTotalCnt++;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.REWORK_UNIT++;

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_TRAY_COUNT_OVER);
                        return;
                    }
                    break; 
                #endregion

                ///////////////////////////////////////////////////////
                // REWORK PLACE END
                ///////////////////////////////////////////////////////

                ///////////////////////////////////////////////////////
                // NG PLACE START
                ///////////////////////////////////////////////////////

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_NG_POS:
                    if (m_bFirstSeqStep)
                    {
                        nFuncResult = IsInPosChipPkR(m_nHeadNo);

                        if (FNC.IsErr(nFuncResult))
                        {
                            SetError(nFuncResult);
                            return;
                        }
                        else if (FNC.IsBusy(nFuncResult)) return;
                    }

                    // Place ready pos move
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN, m_nHeadNo, true);
                    }
                    else
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN, m_nHeadNo);
                    }

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        m_strLogMsg = string.Format("{0} ALL Z MOVE TO UNLOAD POSITION COMPLETE", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                    }
                    break;
                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_OTHER_HEAD_NG_PLACE_CYCLE_RUN:
                    if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nSideHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START] == true) return;

                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] = false;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START] = true;
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.WAIT_NG_BOX_READY:
                    // 해당 피커유닛 존재여부 확인
                    if (!GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].IsPickerNGUnitExist())
                    {
                        m_strLogMsg = string.Format("{0} PICKER NG UNIT NOT EXIST CHECKED", m_strHead);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                        NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FINISH);
                        return;
                    }

                    //if (GbVar.Seq.sUldRjtBoxTable.bIsBoxLoad)
                    //{
                    //    NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_REWORK_TRAY_COUNT_OVER);
                    //    return;
                    //}
                    cycFunc.CHIP_PLACE_FWD_COUNT = 0;
                    break;

                

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_X_AXIS_NG_POS:
                    // Place ready pos move
                    nFuncResult = MovePosChipPlaceToBinBoxXY(m_nHeadNo, 0);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        //
                    }
                    break;


                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.THROW_OUT_TO_REJECT_BOX_1:
                    for (int nPadCnt = 0; nPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPadCnt++)
                    {
                        if (GbVar.GB_CHIP_PK_VAC[nPadCnt])
                        {
                            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nPadCnt), 2, false);
                            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nPadCnt), 3, true);

                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_NG_COUNT++;
                            //배출할때 체크
                            GbVar.lstBinding_EqpProc[MCDF.CURRLOT].TOTAL_CHIP_PROD_COUNT++;

                            GbVar.product.nTotalCnt++;

                            SetNGUnit(nPadCnt, 0);
                        }
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.THROW_OUT_TO_REJECT_BOX_2:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return;
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.THROW_OUT_TO_REJECT_BOX_3:
                    for (int nPadCnt = 0; nPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPadCnt++)
                    {
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nPadCnt), 3, false);
                    }
                    break;

                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.IS_NG_PLACE_COUNT_OVER:
                    {
                        nFuncResult = IsInPosChipPkR(m_nHeadNo);
                    }
                    break;

                ///////////////////////////////////////////////////////
                // NG PLACE END
                ///////////////////////////////////////////////////////


                case seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.FINISH:
                    if (GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] == true)
                    {
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[16] = false;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[17] = false;

                        //스트립 배출 시간
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.STRIP_OUT_TIME = DateTime.Now;
                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT++;

                        GbVar.LotReportLog.AddStripLog(GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info);

                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.GOOD_UNIT = 0;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.REWORK_UNIT = 0;
                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.X_MARK_UNIT = 0;

                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].bSeqIfVar[seqMapVisionTable.ALL_CHIP_PLACE_COMPLETE] = false;

                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].StripDataUpdate(cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO);
                    }

                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_PICK_UP_START] = false;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_INSPECTION_START] = false;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_GD_PLACE_START] = false;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_RW_PLACE_START] = false;
                    GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].bSeqIfVar[seqPkgPickNPlace.SYNC_NG_PLACE_START] = false;

                    NextSeq((int)seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE.MOVE_TO_PICKER_ALL_ZR_AXIS_PICK_UP_POS);

                    m_strLogMsg = string.Format("{0} PICK AND PLACE CYCLE FINISH", m_strHead);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
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
                GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.bIsError = true;
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

        public bool IsMapPickUpUnit(bool bFDir, int nMapTableNo, int nTablePickUpTotCount, int nTablePickUpColCount)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = nTablePickUpColCount % m_nTotMapCountX;
            }
            else
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = m_nTotMapCountX - (nTablePickUpColCount % m_nTotMapCountX) - 1;
            }

            return GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_UNIT;// && !GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT;//220621
        }

        public void SetPickerUnitInfoFromMapTable(bool bFDir, int nMapTableNo, int nTablePickUpTotCount, int nTablePickUpColCount, int nPickerHeadNo, int nPickerCount)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = nTablePickUpColCount % m_nTotMapCountX;
            }
            else
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = m_nTotMapCountX - (nTablePickUpColCount % m_nTotMapCountX) - 1;
            }

            GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].CopyTo(ref GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount]);
            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].IS_UNIT = true;
            // 20211216 bhoh check용
            m_strLogMsg = string.Format("SET PICK-UP MAP TALBE INFO [TABLE NO: {0} TABLE COUNT: {1} ROW: {2}, COL: {3}] - HOST CODE: {4}, TOP: {5}({6}), BTM: {7}({8})",
                                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO, 
                                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                        nRow,
                                        nCol,
                                        GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].ITS_XOUT,
                                        GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT, 
                                        GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].TOP_NG_CODE,
                                        GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

            m_strLogMsg = string.Format("SET PICK-UP PICKER HEAD INFO [PICKER HEAD NO: {0} PICKER NO: {1} ROW: {2}, COL: {3}] - HOST CODE: {4}, TOP: {5}({6}), BTM: {7}({8})",
                                        m_nHeadNo,
                                        nPickerCount,
                                        nRow,
                                        nCol,
                                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].ITS_XOUT,
                                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].TOP_INSP_RESULT,
                                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].TOP_NG_CODE,
                                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].BTM_INSP_RESULT,
                                        GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerCount].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
        }

        public void SetMapPickUpUnit(bool bFDir, int nMapTableNo, int nTablePickUpTotCount, int nTablePickUpColCount, bool bSet)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = nTablePickUpColCount % m_nTotMapCountX;
            }
            else
            {
                nRow = nTablePickUpTotCount / m_nTotMapCountX;
                nCol = m_nTotMapCountX - (nTablePickUpColCount % m_nTotMapCountX) - 1;
            }

            GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_UNIT = bSet;
        }

        public void SetSkipParam(bool bFDir, int nMapTableNo, int nHeadNo)
        {
            GbVar.padSkipParam = new PadSkipParam(bFDir, nMapTableNo, nHeadNo);
        }

        public bool IsGoodTrayUnit(bool bFDir, int nTableNo, int nTablePlaceCount, int nTablePlaceColCount)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                //220606 pjh
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                }
                else
                {
                    nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                }
                
                nCol = nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }
            else
            {
                //220606 pjh
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                }
                else
                {
                    nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                }
                
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
            }

            return GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].IS_UNIT;
        }

        public void SetGoodTrayUnit(bool bFDir, int nTableNo, int nTablePlaceCount, bool bSet)
        {
            int nRow, nCol;
            int nSubRow, nSubCol;
            int nPickUpTableNo;

            if (bFDir == true)
            {            //220606 pjh
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                }
                else
                {
                    nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                }

                nCol = nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

                nSubRow = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].ROW;
                nSubCol = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].COL;
                nPickUpTableNo = cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO;

                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].OUT_PORT = (int)MCDF.eOutPort.GD_1 + nTableNo;
                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].CopyTo(ref GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol]);
                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].CopyTo(ref GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol]);

                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_FWD_COUNT].IS_UNIT = !bSet;
            }
            else
            {
                //220606 pjh
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
                }
                else
                {
                    nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                }
                
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

                nSubRow = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].ROW;
                nSubCol = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].COL;
                nPickUpTableNo = cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO;


                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].OUT_PORT = (int)MCDF.eOutPort.GD_1 + nTableNo;
                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].CopyTo(ref GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol]);
                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].CopyTo(ref GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol]);

                GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].IS_UNIT = !bSet;
            }

            GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].IS_UNIT = bSet;


            // 나중에 Tray upload 할 때 필요할 수 있기 때문에 변경 예정 (20211115 bhoh)
            //GbVar.Seq.sPkgPickNPlace[m_nHeadNo].unitPickUp[nTablePlaceColCount].CopyTo(GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol]);

            m_strLogMsg = string.Format("SET GOOD TRAY UNIT [TABLE NO: {0} TABLE COUNT: {1} ROW: {2}, COL: {3}] - HOST CODE: {4}, TOP: {5}({6}), BTM: {7}({8})",
                                        nTableNo,
                                        nTablePlaceCount,
                                        nRow,
                                        nCol,
                                        GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].ITS_XOUT,
                                        GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].TOP_INSP_RESULT,
                                        GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].TOP_NG_CODE,
                                        GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

            m_strLogMsg = string.Format("SET SQ REPORT UNIT [TABLE NO: {0} TABLE COUNT: {1} ROW: {2}, COL: {3}] - HOST CODE: {4}, TOP: {5}({6}), BTM: {7}({8})",
                                        nPickUpTableNo,
                                        nTablePlaceCount,
                                        nSubRow,
                                        nSubCol,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].ITS_XOUT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_NG_CODE,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
        }

        public bool IsReworkTrayUnit(int nTablePlaceCount)
        {
            int nRow, nCol;

            //220606 pjh
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            {
                nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            }
            else
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }

            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            return GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT;
        }


        public bool IsReworkTrayUnit(int nTablePlaceCount, int nTablePlaceColCount)
        {
            int nRow, nCol;

            //220606 pjh
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            {
                nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            }
            else
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }

            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            //org
            ////220606 pjh
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            //{
            //    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            //}
            //else
            //{
            //    nRow = (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            //}

            //nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            return GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT;
        }

        public void SetReworkTrayUnit(int nTablePlaceCount, bool bSet)
        {
            int nRow, nCol;
            //220606 pjh
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
            { 
                nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - 1 - (nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            }
            else
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }

            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            int nSubRow = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].ROW;
            int nSubCol = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].COL;
            int nPickUpTableNo = cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO;


            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].OUT_PORT = (int)MCDF.eOutPort.RW;
            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].CopyTo(ref GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol]);
            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].CopyTo(ref GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol]);

            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[cycFunc.CHIP_PLACE_GET_BWD_COUNT].IS_UNIT = !bSet;
            GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT = bSet;

            // 나중에 Tray upload 할 때 필요할 수 있기 때문에 변경 예정 (20211115 bhoh)
            //GbVar.Seq.sPkgPickNPlace[m_nHeadNo].unitPickUp[nTablePlaceColCount].CopyTo(GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol]);

            m_strLogMsg = string.Format("SET REWORK TRAY UNIT [TABLE COUNT: {0} ROW: {1}, COL: {2}] - HOST CODE: {3}, TOP: {4}({5}), BTM: {6}({7})",
                                        nTablePlaceCount,
                                        nRow,
                                        nCol,
                                        GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].ITS_XOUT,
                                        GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].TOP_INSP_RESULT,
                                        GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].TOP_NG_CODE,
                                        GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

            m_strLogMsg = string.Format("SET NQ REPORT UNIT [TABLE NO: {0} TABLE COUNT: {1} ROW: {2}, COL: {3}] - HOST CODE: {4}, TOP: {5}({6}), BTM: {7}({8})",
                                        nPickUpTableNo,
                                        nTablePlaceCount,
                                        nSubRow,
                                        nSubCol,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].ITS_XOUT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_NG_CODE,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[nPickUpTableNo].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_NG_CODE);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
        }

        public void SetNGUnit(int nPickerNo, int nBinNo)
        {
            int nSubRow = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].ROW;
            int nSubCol = GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].COL;

            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].OUT_PORT = (int)MCDF.eOutPort.X_OUT + nBinNo;

            if (GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].ITS_XOUT != (int)MESDF.eCELL_STATUS.XOUT)
                GbVar.Seq.sUldRjtBoxTable.Loss_Qty[nBinNo]++;
            else //220608
            {
                if (GbVar.lstBinding_EqpProc.Count > 0)
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].ITS_XMARK_COUNT++;
            }

            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].CopyTo(ref GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol]);
            GbVar.Seq.sPkgPickNPlace.pInfo[m_nHeadNo].unitPickUp[nPickerNo].IS_UNIT = false;


            m_strLogMsg = string.Format("SET NG REPORT UNIT [TABLE NO: {0}, ROW: {1}, COL: {2}] - STRIP ID: {8}, HOST CODE: {3}, TOP: {4}({5}), BTM: {6}({7})",
                                        cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO,
                                        nSubRow,
                                        nSubCol,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol].ITS_XOUT,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol].TOP_NG_CODE,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_INSP_RESULT,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.UnitArr[nSubRow][nSubCol].BTM_NG_CODE,
                                        GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].ReportInfo.STRIP_ID);
            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
        }

        public bool IsExistSkipUnit()
        {
            bool bRet = false;

            for (int nRow = 0; nRow < GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.UnitArr.Length; nRow++)
            {
                for (int nCol = 0; nCol < GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.UnitArr[nRow].Length; nCol++)
                {
                    if (GbVar.Seq.sMapVisionTable[cycFunc.CHIP_PICKER_PICK_UP_TABLE_NO].Info.UnitArr[nRow][nCol].IS_SKIP_UNIT == true)
                        return true;
                }
            }

            return bRet;
        }
    }
}

