using NSS_3330S.MOTION;
using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 신아이 소스 적용시 원복 해야 할 항목
 * IsExistTray() - 드라이러 옵션 원복 하기
 * IsSideExistTray
 * 
 */

namespace NSS_3330S.SEQ.AUTO
{
    /// <summary>
    /// 언로딩 쪽 UP, DOWN 가능한 양품 트레이 테이블
    /// [2022.05.21.kmlee] 알람 추가 및 시퀀스 순서 변경
    /// </summary>
    public class procStUldGDTrayTable1 : SeqBase
    {
        cycUldTrayTable cycFunc = null;
        int m_nTableNo = 0;
        int m_nSideTableNo = 0;
		string m_strTableName = "";
        bool m_bIsTableUp = false;

        public procStUldGDTrayTable1(int nSeqID)
        {
            m_nTableNo = CFG_DF.TRAY_STG_GD1;
            m_nSideTableNo = m_nTableNo == CFG_DF.TRAY_STG_GD1 ? CFG_DF.TRAY_STG_GD2 : CFG_DF.TRAY_STG_GD1;
            m_strTableName = "Good Tray Stage 1";
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sUldGDTrayTable[m_nTableNo];

            cycFunc = new cycUldTrayTable(nSeqID, m_nTableNo);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
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
                if (m_seqInfo != GbVar.Seq.sUldGDTrayTable[m_nTableNo])
                {
                    m_seqInfo = GbVar.Seq.sUldGDTrayTable[m_nTableNo];
                    m_nSeqNo = GbVar.Seq.sUldGDTrayTable[m_nTableNo].nCurrentSeqNo;
                }
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                if (m_nSeqNo == 46)
                {
                    // 46번 케이스일때만 m_bFirstSeqStep 사용
                    m_bFirstSeqStep = true;
                }

                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        // Init
                        //if (GbVar.LOADER_TEST)
                        //    return;

                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("-----------", m_strTableName, "Start !!");

                            m_bFirstSeqStep = false;
                        }
                    }
                    break;

                case 2:
                    {
                        // 현재 테이블 상태 확인
#if _NOTEBOOK
                        if (GbVar.Seq.sUldGDTrayTable[1].smTableUP == false)
                        {
                            GbVar.Seq.sUldGDTrayTable[0].smTableUP = true;
                        }
                        else
                        {
                            if (!LeaveCycle())
                                return;

                            return;
                        }
#endif

                    }
                    break;

               

                case 4:
                    {
                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY]
                            && GbVar.Seq.sUldGDTrayTable[m_nSideTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                        {
                            // 알람 처리
                            SetError((int)ERDF.E_TRAY_BOTH_SORTING);
                            return;
                        }

                        // 초기화 잡고 난 이후면 Unloading 위치에 위~ 아래로 있을듯.
                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                        {
                            // Sorting 중
                            if (IsStageUp())
                            {
                                if (!IsSideStageUp())
                                {
                                    nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_WORKING_P1);

                                    if (nFuncResult == FNC.SUCCESS)
                                    {
                                        SeqHistory("Working Position Move", m_strTableName, "Done");
                                        NextSeq(52);
                                    }
                                    return;
                                }
                            }
                        }
                        else if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL])
                        {
                            // 만재 중
                            if (IsStageUp())
                            {
                                NextSeq(10);
                                return;
                            }
                            else
                            {
                                NextSeq(40);
                                return;
                            }
                        }
                        //else if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_NOT_EXIST])
                        //{
                        //    // Tray Empty
                        //    if (IsStageUp())
                        //    {
                        //        NextSeq(20);
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        NextSeq(40);
                        //        return;
                        //    }
                        //}
                    }
                    break;

                // 스테이지가 다운 상태일 경우 작업 위치 이동 대기 후 작업 준비
                case 8:
                    {
                        // 테이블 실린더 UP 체크
                        // DOWN 상태일 경우(UP이 아니면) 작업위치로 이동
                        if (!IsStageUp())
                        {
                            // DOWN 상태에서 해당 스테이지 사용하지 않을 때 대기
                            if (IsUseStage() == false)
                            {
                                LeaveCycle();
                                return;
                            }

                            // 작업 위치로 이동
                            NextSeq(40);
                            return;
                        }
                    }
                    break;

                
                case 10:
                    {
                        if (!LeaveCycle()) return;

#if _NOTEBOOK
                        break;
#endif
                        // 처음에는 같은 위치에 있을 수 있으므로 UP, DOWN은 확인하지 않는다
                        // 위치 이동 인터락
                        if (!SafetyCheckTrayTableMove(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING)) return;

                        SeqHistory("Tray Stage Y Axis Move Available Check OK", m_strTableName, "Complete");
                    }
                    break;
                // 테이블 준비위치 이동
                case 12:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Unloading Move", m_strTableName, "Start");

                            if (!IsStageUp())
                            {
                                SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }

                        nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Unloading Move", m_strTableName, "Done");
                        }
                    }
                    break;

                case 14:
                    {
                        
                    }
                    break;

                // 트레이 만재(배출) 체크
                case 16:
                    {
                        // 테이블 위에 트레이 감지 센서 확인
                        // 감지되어 있으면 완료 트레이인지 작업중인 트레이인지 확인 필요
                        // 완료 트레이이면 배출로 이동
                        // 작업중인 트레이이면 작업자에게 알리고 사용자 맵핑할 수 있게 함

                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] == true)
                        {
                            SeqHistory("Tray Unit Full is True.");

                            if (IsExistTray())
                            {
                                SeqHistory("Tray Exist is True. Unloading Start.");
                                NextSeq(60);
                                return;
                            }
                        }
                    }
                    break;

                #region 트레이가 업 상태일 때 트레이 공급 (case 22 ~)
                case 20:
                    {
                        // LOT END 확인
                        if (GbVar.Seq.bIsLotEndRun)
                        {
                            SeqHistory("Lot End is True.");

                            // 반대편 스테이지에 자재가 있을 경우 스테이지 다운 후 워킹으로 가야 반대편 스테이지가 트레이 배출 가능
                            if (IsSideExistTray())
                            {
                                SeqHistory("Side Table Tray Exist is True.");
                                NextSeq(30);
                                return;
                            }
                            // 없으면 대기
                            else
                            {
                                if (!LeaveCycle()) return;

                                return;
                            }
                        }

                        // 사용 유무 확인
                        // 스테이지가 업 상태이고 사용하지 않는다면 트레이를 받지 않고 다운해야 한다
                        if (!IsUseStage())
                        {
                            SeqHistory("Tray Stage Use Option is False.");
                            NextSeq(30);
                            return;
                        }
#if _NOTEBOOK
                        if (GbVar.Seq.sUldGDTrayTable[1].smTableUP == false)
                        {
                            GbVar.Seq.sUldGDTrayTable[0].smTableUP = true;
                        }
                        else
                        {
                            LeaveCycle();
                            return;
                        }
#endif
                    }
                    break;
                case 21:
                    {
    
                    }
                    break;

                // 트레이 공급 요청
                case 22:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray Loading Request", m_strTableName, "Tray Request Condition Check Start");

                            m_bFirstSeqStep = false;
                        }

                        if (!IsStageUp())
                        {
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            SeqHistory("Tray Loading Request", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            return;
                        }

                        if (IsExistTray())
                        {
                            SeqHistory("Tray Loading Request", m_strTableName, "Tray Exist is True. Go to Unloading");
                            NextSeq(60);
                            return;
                        }

                        SeqHistory("Tray Loading Request", m_strTableName, "Tray Stage is Up and Tray Exist is False. Tray Loading Start.");
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] = true;
                    }
                    break;

                // 트레이 공급 완료 대기
                case 24:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Wait Tray Loading", m_strTableName, "Wait");

                            m_bFirstSeqStep = false;
                        }

                        if (!LeaveCycle()) return;

                        if (!IsStageUp())
                        {
                            SeqHistory("Wait Tray Loading", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_READY] == true) return;

                        SeqHistory("Tray Loading Completed", m_strTableName, "Complete");
                    }
                    break;

                // 트레이 공급 완료 확인
                case 26:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Check Tray Loading Comp", m_strTableName, "Wait Tray Loading From Tray Picker.");

                            m_bFirstSeqStep = false;

                            if (!IsStageUp())
                            {
                                SeqHistory("Check Tray Loading Comp", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                                SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }

                            if (!IsInPosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                            {
                                SeqHistory("Check Tray Loading Comp", m_strTableName, "Tray Stage is Not Tray Loading Position. Alarm Occured!!");
                                SetError((int)ERDF.E_WRONG_POS_LOADING_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }
                        if (!LeaveCycle()) return;

                        if (!GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE]) return;

                        GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT++;

                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].Info.Clear();
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_LOADING_COMPLETE] = false;

                        string strCnt = string.Format("Current Lot Good Table Tray Work Count: {0}", GbVar.lstBinding_EqpProc[MCDF.CURRLOT].GOOD_TABLE_1_TRAY_WORK_COUNT);
                        SeqHistory("Check Tray Loading Comp", m_strTableName, strCnt);
                        SeqHistory("Check Tray Loading Comp", m_strTableName, "Tray Loading From Tray Picker Complete.");
                    }
                    break;

                //220511 pjh
                case 27:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray Align", m_strTableName, "Start");

                            m_bFirstSeqStep = false;

                            if (!IsStageUp())
                            {
                                SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }

                        nFuncResult = cycFunc.TrayAlign();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Tray Align", m_strTableName, "Complete");
                        }
                    }
                    break;
                #endregion

                #region 트레이를 공급 받고 난 후 스테이지 다운 -> 작업 위치 이동 -> 스테이지 업 -> 작업 시작 -> 트레이 만재 대기 (case 30 ~)
                // 테이블 DOWN 가능?
                case 30:
                    {
                        if (!LeaveCycle()) return;

                        // 다른 스테이지를 사용하지 않는다면 내려가지말고 바로 작업 위치로 이동
                        if (IsUseSideStage() == false)
                        {
                            SeqHistory("Tray Stage Down Condition Check", m_strTableName, "Side Tray Stage Use Option is False.");
                            NextSeq(40);
                            return;
                        }

                        // 이미 DOWN 상태라면 패스 (예외상황이 발생한 상태이니 알람으로 대체)
                        //if (IsStageDown())
                        //{
                        //    break;
                        //}

                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Stage Down Condition Check", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (!IsGDTrayTableCylinderSafety())
                        {
                           // GbVar.bTableDistanceWait[m_nTableNo] = true;
                            return;
                        }
                        else
                           // GbVar.bTableDistanceWait[m_nTableNo] = false;

                        SeqHistory("Tray Stage Down Condition Check", m_strTableName, "Tray Stage is Available to Down");
                    }
                    break;

                // 테이블 DOWN
                case 32:
                    {
                        // 다른 스테이지를 사용하지 않는다면 내려가면 안됨
                        if (IsUseSideStage() == false)
                            break;

                        nFuncResult = GoodTrayStageUpDown(m_nTableNo, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory("Tray Table Cylinder Down", m_strTableName, "Complete");
                        }
                    }
                    break;

                case 34:
                    {
                        // 작업 위치 가서 체크
                        //if (GbVar.Seq.bIsLotEndRun)
                        //{
                        //    if (LeaveCycle()) return;

                        //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse ||
                        //        IsExistTray() == true)
                        //        return;
                        //}
                    }
                    break;

                // 작업위치 이동 가능?
                case 40:
                    {
                        if (!LeaveCycle()) return;

                        // 다른 스테이지 사용할 때만 확인
                        if (IsUseSideStage())
                        {
                            if (!IsStageDown())
                            {
                                SeqHistory("Tray Stage Move Condition Check", m_strTableName, "Tray Stage is Not Down. Alarm Occured!!");
                                SetError((int)ERDF.E_NOT_DOWN_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }

                        // 현재 상태에서는 다른 테이블이 같은 높이에 있으면 안된다
                        if (!SafetyCheckTrayTableUpDownCyl()) 
                            return;
                        // 위치 이동 인터락
                        if (!SafetyCheckTrayTableMove(m_nTableNo, POSDF.TRAY_STAGE_TRAY_WORKING_P1)) 
                            return;

                        SeqHistory("Tray Stage Move Condition Check", m_strTableName, "Tray Stage is Available to Move");
                    }
                    break;

                // 작업위치 이동
                case 42:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Working Position Move", m_strTableName, "Start");

                            m_bFirstSeqStep = false;

                            // 다른 스테이지 사용할 때만 확인
                            if (IsUseSideStage())
                            {
                                if (!IsStageDown())
                                {
                                    SeqHistory("Working Position Move", m_strTableName, "Tray Stage is Not Down. Alarm Occured!!");
                                    SetError((int)ERDF.E_NOT_DOWN_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                    return;
                                }
                            }
                        }

                        //nFuncResult = MovePosGdTrayStg1Y(POSDF.TRAY_STAGE_TRAY_WORKING_P1);
                        nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_WORKING_P1);
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Working Position Move", m_strTableName, "Done");
                        }
                    }
                    break;

                case 44:
                    {
                        // LOT END 체크
                        if (GbVar.Seq.bIsLotEndRun)
                        {
                            // 스테이지에 트레이가 없고, 다른 스테이지가 업 상태라면 스테이지를 업하기 전 대기.
                            // Lot End 후 초기화를 할 때 둘 다 업 상태면 진행 안될 수 있으므로
                            if (IsExistTray() == false && IsSideStageUp() == true)
                            {
                                if (!LeaveCycle()) return;

                                return;
                            }
                        }

                        // 스테이지 사용을 하지 않는다면 여기서 대기
                        if (IsUseStage() == false)
                        {
                            if (!LeaveCycle()) return;

                            return;
                        }

                        SeqHistory("Tray Stage Cylinder Up Condition Check", m_strTableName, "Complete.");
                    }
                    break;

                // 테이블 실린더 UP 가능?
                case 46:
                    {
                        // 20230223 CHOH : 이미 테이블 UP 상태이면 브레이크
                        if (m_bFirstSeqStep)
                        {
                            if (CheckTrayStage(m_nTableNo, true))
                            {
                                SeqHistory("Tray Table Cylinder Already Up Case 9", m_strTableName, "Complete");
                                break;
                            }
                        }

                        if (!LeaveCycle()) return;

                        // 다른 스테이지 사용할 때만 확인
                        if (IsUseSideStage())
                        {
                            if (!IsStageDown())
                            {
                                SeqHistory("Tray Stage Cylinder Up Condition Check", m_strTableName, "Tray Stage is Not Down. Alarm Occured!!");
                                SetError((int)ERDF.E_NOT_DOWN_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }

                            if (!IsInPosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_WORKING_P1))
                            {
                                SeqHistory("Tray Stage Cylinder Up Condition Check", m_strTableName, "Tray Stage is Not Work Position. Alarm Occured!!");
                               // SetError((int)ERDF.E_WRONG_POS_WORKING_P1_GOOD_TRAY_1_STAGE + m_nTableNo);
                                NextSeq(40);
                                return;
                            }
                        }


                        // UP, DOWN을 할 수 있는 거리 차인지 확인
                        if (!IsGDTrayTableCylinderSafety())
                        {
                           // GbVar.bTableDistanceWait[m_nTableNo] = true;
                            return;
                        }
                        else
                        {
                            //GbVar.bTableDistanceWait[m_nTableNo] = false;
                        }

                        // 트레이 검사가 추가되면 다른 스테이지 검사 중 에러 발생 시 작업하는 현재 스테이지와 인터락이 발생할 수 있으므로 추가되고 난 후 여기서 인터락을 걸어야 할 수도 있다
                        if (GbVar.Seq.sUldGDTrayTable[m_nSideTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_VISION_START])
                        {
                            return;
                        }

                        SeqHistory("Tray Stage Cylinder Up Condition Check", m_strTableName, "Complete.");
                    }
                    break;

                // 테이블 실린더 UP
                case 48:
                    {
                        nFuncResult = GoodTrayStageUpDown(m_nTableNo, true, 200);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory("Tray Stage Cylinder Up", m_strTableName, "Complete");
                        }
                    }
                    break;

                // 테이블 작업 가능 true
                case 50:
                    {
                        // 트레이가 없을 경우 트레이 검사 및 트레이 배출 시퀀스로 넘어간다
                        if (GbVar.Seq.bIsLotEndRun)
                        {
                            // 다른 스테이지에 트레이가 있고, 다운 상태라면 배출 쪽으로 빠져야 함 (업 상태라면 트레이 언로딩 중일 수 있으므로 대기)
                            if (IsSideExistTray() && !IsSideStageUp())
                            {
                                SeqHistory("Tray Stage Work Ready", m_strTableName, "Side Tray Stage Tray Exist is True and Down");
                                NextSeq(60);
                                return;
                            }
                            // 트레이를 가지고 있으면 배출 쪽으로 빠져야 함
                            else if (IsExistTray())
                            {
                                SeqHistory("Tray Stage Work Ready", m_strTableName, "Tray Stage Tray Exist is True");
                                NextSeq(60);
                                return;
                            }
                            else
                            {
                                if (!LeaveCycle()) return;

                                return;
                            }
                        }
                        else
                        {
                            if (!IsExistTray())
                            {
                                SeqHistory("Tray Stage Work Ready", m_strTableName, "Tray Stage Tray Exist is False.");
                                NextSeq(90);
                                return;
                            }
                        }

                        if (!IsStageUp())
                        {
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        // 테이블 작업 가능 true
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].nUnitPlaceCount = 0; // 만약 사용자 맵핑이 되어 있다면 0으로 Reset하면 안됨
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY] = true;
						GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_VISION_START] = true;
                        SeqHistory("Tray Stage Work Ready", m_strTableName, "Tray Stage Sort Out Ready is True");
                    }
                    break;

                // 테이블 작업완료 대기
                case 52:
                    {
                        if (!LeaveCycle()) return;

                        // LOT END 체크
                        if (GbVar.Seq.bIsLotEndRun)
                        {
                            GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                            SeqHistory("Tray Stage Wait Full Check", m_strTableName, "Lot End");
                        }
                        // Good 트레이 배출
                        else if (GbVar.Seq.bGoodTrayOut) //else if (GbVar.Seq.bGoodTrayOut[m_nTableNo]) 2023-12-31 sj.shin 신아이 원복
                        {
                            // 소터에 자재가 없을 경우에만 배출
                            //if (GbFunc.IsLotEndCondition(true, true))
                            if (GbFunc.IsPnpNoStrip(true))
                            {
                                //GbVar.Seq.bGoodTrayOut = false;
                                //GbVar.Seq.bGoodTrayOutToTurnRail = false;
                                GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                                SeqHistory("Tray Stage Wait Full Check", m_strTableName, "Pnp No Strip");
                            }
                        }
                        // 사용 안할 때
                        else if(!IsUseStage())
                        {
                            GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                            SeqHistory("Tray Stage Wait Full Check", m_strTableName, "Tray Stage Use Option is False");
                        }

                        // 스테이지가 UP 상태가 아니면 알람
                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Stage Wait Full Check", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        // Tray Unit 만재?

                        if (!GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL])
                        {
                            //if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_NOT_EXIST])
                            //{
                            //    GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_NOT_EXIST] = false;
                            //    NextSeq(0);
                            //    SeqHistory("Tray table sequence init cause no tray on the table", m_strTableName, "TRAY NOT EXIST");
                            //    return;
                            //}
                            return;
                        }

                        SeqHistory("Tray Table PLACE FULL CHECK", m_strTableName, "TRAY UNIT FULL");

                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = false;
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY] = false;
                        SeqHistory("Tray Stage Wait Full Check", m_strTableName, "Complete");
                    }
                    break;
                #endregion

                #region 트레이 검사 (case 60 ~)
                case 60:
                    {
                        // 테이블 검사모드 사용?
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_INSPECTION_USE].bOptionUse)
                        {
                            SeqHistory("Tray Vision Inspection", m_strTableName, "Option Use is False.");
                            NextSeq(90);
                            return;
                        }

                        cycFunc.MEASURE_COUNT = 0;
                        GbVar.Seq.bTrayUnitMisPlace[m_nTableNo] = false;
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_VISION_START] = true;

                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }
                    }
                    break;

                case 62:
                    { 
                    }
                    break;

                case 64:
                    {
                        if (!LeaveCycle()) return;

                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        // 트레이 검사 시 같은 위치(작업 위치)에 있을 수 있으므로 UP, DOWN은 확인하지 않는다
                        // if (!SafetyCheckTrayTableUpDownCyl()) return;
                        // 위치 이동 인터락
                        if (!SafetyCheckTrayTableMove(m_nTableNo, POSDF.TRAY_STAGE_TRAY_INSPECTION_START)) return;
                    }
                    break;

                case 66:
                    // 테이블 검사위치 이동
                    break;

                case 70:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Tray Vision Inspection", m_strTableName, "Start");
                            m_bFirstSeqStep = false;

                            if (!IsStageUp())
                            {
                                SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                                SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }

                        if (GbVar.Seq.bTrayUnitMisPlace[m_nTableNo])
                        {
                            GbVar.Seq.bTrayUnitMisPlace[m_nTableNo] = false;
                            //SeqHistory("굿 트레이 #1 트레이 비전 재검사 하지 않음!!");
                            SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Vision Inspection Finish");
                            break;
                        }
                      
                        nFuncResult = cycFunc.TrayTalbeChipMeasure();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            for (int nRow = 0; nRow < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY; nRow++)
                            {
                                for (int nCol = 0; nCol < RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX; nCol++)
                                {
                                    if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].Info.UnitArr[nRow][nCol].IS_UNIT &&
                                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].Info.UnitArr[nRow][nCol].TRAY_MEASURE_CODE != 0)
                                    {
                                        GbVar.Seq.bTrayUnitMisPlace[m_nTableNo] = true;
                                        // 위 변수는 ALARM 이후 재시작 할 때 재검을 할지 아니면 재검 없이 다음으로 넘길지 MESSAGE BOX를 뛰우기 위함으로 사용함.

                                        SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Vision Inspection Finish. Result NG");
                                        SetError((int)ERDF.E_GD_TRAY_STAGE_1_PLACE_MISS);
                                        return;
                                    }
                                }
                            }

                            SeqHistory("Tray Vision Inspection", m_strTableName, "Tray Vision Inspection Finish. Result OK");
                        }
                    }
                    break;

                case 80:
                    {
                        // 검사 결과 DB Read
                        // 결과 확인 후 문제 있으면 Alarm
                    }
                    break;
                #endregion

                #region 트레이 배출 (case 90 ~)
                // 테이블 준비위치 이동 가능?
                case 90:
                    {
                        if (!LeaveCycle()) return;

                        if (!IsStageUp())
                        {
                            SeqHistory("Unloading Position Move Condition Check", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        // UP, DOWN 확인은 하지 않는다
                        // 위치 이동 인터락
                        if (!SafetyCheckTrayTableMove(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING)) return;
                        SeqHistory("Unloading Position Move Condition Check", m_strTableName, "Y Move Safety Check Complete");
                    }
                    break;

                // 테이블 준비위치 이동
                case 92:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("Unloading Position Move", m_strTableName, "Start");

                            m_bFirstSeqStep = false;

                            if (!IsStageUp())
                            {
                                SeqHistory("Unloading Position Move", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                                SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                                return;
                            }
                        }
                        //nFuncResult = MovePosGdTrayStg1Y(POSDF.TRAY_STAGE_TRAY_UNLOADING);
                        nFuncResult = MovePosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "GOOD TRAY STAGE 1 MOVE TRAY UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory("Unloading Position Move", m_strTableName, "Done");
                            //[22.07.04] 홍은표 트레이 비전 인터락 해제
                            GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_VISION_START] = false;
                        }
                    }
                    break;

                // 테이블 트레이 유무확인
                case 94:
                    {
                        // 트레이가 없을 경우 트레이 공급으로 이동
                        if (IsExistTray() == false)
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray is not exists. Go to Tray In.");

                            NextSeq(20);
                            return;
                        }
                    }
                    break;
                // 테이블 트레이 배출 요청
                case 100:
                    {


                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (!IsInPosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Not Unload Position. Alarm Occured!!");
                            SetError((int)ERDF.E_WRONG_POS_UNLOADING_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_COMPLETE] = false;
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] = true;

                        //[22.07.04] 홍은표 트레이 비전 인터락 해제
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_VISION_START] = false;

                        SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Ready Complete to Unload Tray.");
                    }
                    break;

                // 트레이 배출 완료 대기
                case 102:
                    {
                        if (!LeaveCycle()) return;

                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (!IsInPosGdTrayStgY(m_nTableNo, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Not Unload Position. Alarm Occured!!");
                            SetError((int)ERDF.E_WRONG_POS_UNLOADING_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_READY] == true) return;
                        SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Picker Tray Pick Up From Stage Start");
                    }
                    break;

                // 트레이 배출 완료 대기
                case 104:
                    {
                        if (GbVar.mcState.IsCycleRun(SEQ_ID.TRAY_TRANSFER) == false)
                        {
                            if (!LeaveCycle()) return;
                        }

                        if (!IsStageUp())
                        {
                            SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Stage is Not Up. Alarm Occured!!");
                            SetError((int)ERDF.E_NOT_UP_STATUS_GOOD_TRAY_1_STAGE + m_nTableNo);
                            return;
                        }

                        if (GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_COMPLETE] == false)
                            return;
                        GbVar.Seq.sUldGDTrayTable[m_nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNLOADING_COMPLETE] = false;

                        SeqHistory("Tray Unload by Tray Picker", m_strTableName, "Tray Picker Tray Pick Up From Stage Done");
                        NextSeq(20);
                        return;
                    }
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

        bool IsStageUp()
        {
#if _NOTEBOOK
            if (GbVar.Seq.sUldGDTrayTable[0].smTableUP)
                return true;
            else
                return false;
#endif
            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_STG_UP, IODF.INPUT.GD_TRAY_2_STG_UP };

            // UP만 본다. DOWN 센서가 안들어 올 수도 있기 때문
            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsStageDown()
        {
#if _NOTEBOOK
            if (GbVar.Seq.sUldGDTrayTable[0].smTableUP ==false)
                return true;
            else
                return false;
#endif

            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_STG_DOWN, IODF.INPUT.GD_TRAY_2_STG_DOWN };

            // DOWN만 본다. UP 센서가 안들어 올 수도 있기 때문
            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsExistTray()
        {
            //20230307 CHOH : 트레이 없이 드라이런 일때 조건 추가
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse )
                ////2023-12-31 sj.shin 신아이 원복
                //&& !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_TRAY_USE].bOptionUse)
            {
                return GbVar.Seq.sUldGDTrayTable[m_nTableNo].bTrayExist;
            }

            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_1_CHECK, IODF.INPUT.GD_TRAY_2_CHECK };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsSideExistTray()
        {
            //20230307 CHOH : 트레이 없이 드라이런 일때 조건 추가
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                // ////2023-12-31 sj.shin 신아이 원복
                //&& !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_TRAY_USE].bOptionUse)
            {
                return GbVar.Seq.sUldGDTrayTable[m_nSideTableNo].bTrayExist;
            }

            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_2_CHECK, IODF.INPUT.GD_TRAY_1_CHECK };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsSideStageUp()
        {
#if _NOTEBOOK
            if (GbVar.Seq.sUldGDTrayTable[1].smTableUP)
                return true;
            else
                return false;
#endif

            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_2_STG_UP, IODF.INPUT.GD_TRAY_1_STG_UP };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsSideStageDown()
        {
#if _NOTEBOOK
            if (GbVar.Seq.sUldGDTrayTable[1].smTableUP)
                return false;
            else
                return true;
#endif

            IODF.INPUT[] input = { IODF.INPUT.GD_TRAY_2_STG_DOWN, IODF.INPUT.GD_TRAY_1_STG_DOWN };

            return MotionMgr.Inst.GetInput(input[m_nTableNo]);
        }

        bool IsUseStage()
        {


            bool[] bUseArray = { ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_1_USE].bOptionUse,
                                 ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_2_USE].bOptionUse };

            return bUseArray[m_nTableNo];
        }

        bool IsUseSideStage()
        {


            bool[] bUseArray = { ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_1_USE].bOptionUse,
                                 ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_2_USE].bOptionUse };

            return bUseArray[m_nSideTableNo];
        }

        bool IsWorkSideStage()
        {
#if _NOTEBOOK
            if (GbVar.Seq.sUldGDTrayTable[1].smTableUP)
                return true;
            else
                return false;
#endif
            return GbVar.Seq.sUldGDTrayTable[m_nSideTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY];
        }
    }
}
