using DionesTool.UTIL;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycMapTransfer : SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;

        int m_nInspectionTableNo = 0;
        int m_nUnloadingTableNo = 0;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;
        bool bDryBlockVacChecked = false;

        int m_nManualTriggerCount = 0;

        // Align 번호. Align 1, Align 2
        int m_nAlignNo = 0;
        // Angle 보정 전 얼라인인지, 보정 후 얼라인인지 (true면 보정 후)
        bool m_bAfterRotateCheck = false;

        double m_dTopAlignAngle = 0.0;

        int m_nAirDryCnt = 0;

        int m_nRetryAlignCount = 0;

        #region PROPERTY
        public seqMapTransfer SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sMapTransfer; }
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

        public int NEXT_ULD_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nNextUnloadingTableNo;
                }

                return m_nUnloadingTableNo;
            }
            set
            {
                m_nUnloadingTableNo = value;
            }
        }

        public int ULD_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nUnloadingTableNo;
                }

                return m_nUnloadingTableNo;
            }
            set
            {
                m_nUnloadingTableNo = value;
            }
        }

        public int INSP_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nUnloadingTableNo;
                }

                return m_nInspectionTableNo;
            }
            set
            {
                m_nInspectionTableNo = value;
            }
        }

        public int RETRY_ALIGN_COUNT
        {
            get
            {
                
                return m_nRetryAlignCount;
            }
            set
            {
                m_nRetryAlignCount = value;
            }
        }
        #endregion

        public void CmdInspectionDBRead()
        {
        }
        public cycMapTransfer(int nSeqID, int nMoveTableNo)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nUnloadingTableNo = nMoveTableNo;
            m_seqInfo = GbVar.Seq.sMapTransfer;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];

            m_nRetryAlignCount = 0;
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            m_nUnloadingTableNo = (int)args[0];
            m_nInspectionTableNo = (int)args[1];
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
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
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                nFuncResult = SafetyMgr.Inst.GetAxisSafetyBeforeHome(m_nInitAxisNoArray[nCnt]);
                                if (FNC.IsErr(nFuncResult))
                                {
                                    return nFuncResult;
                                }
                            }
                        }
                    }
                    break;
                case 22:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].HomeStart();
                            }
                        }
                    }
                    break;
                case 24:
                    {
                        if (FNC.IsBusy(RunLib.msecDelay(200)))
                            return FNC.BUSY;
                    }
                    break;
                case 26:
                    {
                        for (int nCnt = 0; nCnt < m_nInitAxisNoArray.Length; nCnt++)
                        {
                            if (m_nInitOrderArray[nCnt] == m_nCurrentOrder)
                            {
                                if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Fail)
                                {
                                    return (int)ERDF.E_SV_NOT_HOME + m_nInitAxisNoArray[nCnt];
                                }
                                else if (MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].GetHomeResult() == DionesTool.Motion.HomeResult.HR_Process ||
                                         MotionMgr.Inst[m_nInitAxisNoArray[nCnt]].IsBusy())
                                {
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
                case 28:
                    {
                        m_nCurrentOrder++;

                        if (m_nCurrentOrder < 10)
                        {
                            NextSeq(20);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 30:
                    {

                    }
                    break;
                case 40:
                    {
                        GbSeq.autoRun[SEQ_ID.MAP_TRANSFER].InitSeq();
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

        #region LoadDryBlock
        public int LoadingDryBlock()
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
                        // 초기화 작업

                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.LOAD_DRY_BLOCK, true);
                        }
                    }
                    break;

                // 드라이 블럭 위치 확인
                case 1:
                    {
                        if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                            break;

#if _NOTEBOOK
                        break;
#endif
                        if (IsInPosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING) == false)
                        {
                            return (int)ERDF.E_WRONG_POS_DRY_BLOCK_UNLOAD_POS;
                        }
                    }
                    break;

                // 자재 정보가 있을 경우 확인
                case 5:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;

                            //아직 데이터가 넘어가기 전 이므로 드라이블럭의 자재 유무를 확인한다.
                            if (GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                // 드라이 블럭은 여기서 움직이지 않기 떄문에 IsInPos를 확인 안하고 이전 스텝에서 알람만 띄운다
                                if (IsInPosMapPkX(POSDF.MAP_PICKER_STRIP_LOADING))
                                {
                                    //자재는 있으나 트렌스퍼에 넘겨주는 과정에서 베큠 알람이 뜬경우
                                    //트렌스퍼는 베큠 상태이다.
                                    if (AirStatus(STRIP_MDL.MAP_TRANSFER) == AIRSTATUS.VAC)
                                    {
                                        //이때 피커이동은 하지 않기위해 바로 베큠 On 시퀀스로 이동한다.
                                        // VAC이 켜져있어도 픽업 위치가 아닐 수 있기 때문에 Z축 READY로 보내는 것을 제외하고 정상 시퀀스를 탄다.
                                        NextSeq(10);
                                        return FNC.BUSY;
                                    }
                                }
                            }
                        }
                    }
                    break;

                // 맵 피커 Z축 레디 위치 이동
                case 6:
                    // MAP TRANSFER Z AXIS UP POS  MOVE
                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 자재 정보가 없을 경우 확인
                case 7:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;

                            if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                if (AirStatus(STRIP_MDL.MAP_TRANSFER) != AIRSTATUS.NONE)
                                {
                                    nFuncResult = (int)ERDF.E_MAP_PICKER_VAC_N_DATA_MISMATCH;
                                }
                                else if (AirStatus(STRIP_MDL.UNIT_DRY) != AIRSTATUS.NONE)
                                {
                                    nFuncResult = (int)ERDF.E_DRY_BLOCK_VAC_N_DATA_MISMATCH;
                                }
                                else
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP NOT EXIST ", STEP_ELAPSED));

                                    // 8번에서 끝냄
                                    //NextSeq(50);
                                    //return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;

                case 8:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                if (m_bFirstSeqStep)
                                {

                                }
                                nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_READY);

                                if (FNC.IsSuccess(nFuncResult))
                                {
                                    NextSeq(50);
                                    return FNC.BUSY;
                                }
                            }
                        }
                    }
                    break;
					
				case 9:
					{
						//if (m_bAutoMode) break;
                        //메뉴얼 모드일 경우 드라이블럭 언로딩 위치로 이동한다.
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK X UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
					}
					break;

                case 10:
                    break;

                case 11:
                    // MAP TRANSFER X AXIS DRY BLOCK POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkX(POSDF.MAP_PICKER_STRIP_LOADING))
                                break;
                        }

                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 14:
                    {
                        // DRY BLOCK이 UNLOADING 위치가 아니면 알람
                        {
                            //SetError((int)ERDF.E_INTL_MAP_PK_X_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS);
                        }
                    }
                    break;
                case 20:
                    // MAP TRANSFER Z AXIS DRY BLOCK PRE-DOWN POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_STRIP_LOADING))
                                break;
                        }

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_LOADING, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS LOADING SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // MAP TRANSFER Z AXIS DRY BLOCK DOWN POS  MOVE

                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_LOADING, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 23:
                    {	
                        nFuncResult = MapPickerVac(true, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // DRY TABLE BLOW ON
                    {
                        // [2022.05.04.kmlee] BLOW는 체크 안하도록 요청
                        //nFuncResult = DryBlockWorkBlow(true, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse);
                        nFuncResult = DryBlockWorkBlow(true, 0, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 28:
                    // MAP TRANSFER VACUUM ON CHECK
                    {
                        //220505 pjh 베큠 센서 체크 구문 추가
                        //위 시퀀스 확인 후 사용할지 정할 것

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse ||
                             ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        nFuncResult = VacBlowCheck((int)IODF.A_INPUT.MAP_PK_WORK_VAC,
                                                    true,
                                                    (int)ERDF.E_MAP_PK_WORK_VAC_NOT_ON,
                                                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_CHECK_TIME].lValue);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            // [2022.05.13.kmlee] VAC 알람 발생 시 BLOW OFF
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_VAC_PUMP, false);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);

                            //밑에서 배큠 체크 하려고 
                            nFuncResult = FNC.SUCCESS;
                        }
                    }
                    break;

                case 30:
                    // 현재 위치를 확인
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.MAP_PK_Z].GetRealPos();
                    double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_Z].dPos[POSDF.MAP_PICKER_STRIP_LOADING] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos = dTargetPos * 0.1;
                    }

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // MAP TRANSFER Z AXIS DRY BLOCK PRE-DOWN POS  MOVE

                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_LOADING, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS LOADING SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    
                case 34:
                    // MAP TRANSFER Z AXIS UP POS  MOVE

                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 35:
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse ||
                             ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (!GbVar.Seq.sUnitDry.Info.IsStrip()) break;

                        nFuncResult = VacBlowCheck((int)IODF.A_INPUT.MAP_PK_WORK_VAC,
                                                  true,
                                                  (int)ERDF.E_MAP_PK_WORK_VAC_NOT_ON,
                                                  50);
                    }
                    break;
                case 36:
                    // DRY TABLE BLOW OFF
                    {
                        nFuncResult = DryBlockWorkBlow(false, 1000, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 40:
                    // MAP TRANSFER X AXIS DRY BLOCK POS MOVE
                    {
                        if (!m_bAutoMode) break;//220517
                        if (m_bAutoMode)
                        {
                            if (m_bFirstSeqStep)
                            {
								//안봐도 됨 2022 08 12 hEP
                                //if (GbVar.Seq.sMapTransfer.nUnloadingTableNo == 1)
                                //{
                                //    if (GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START])
                                //    {
                                //        return FNC.CYCLE_CHECK;
                                //    }
                                //    GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] = true;
                                //}
                            }
                        }
                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 50:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));

                        TTDF.SetTact((int)TTDF.CYCLE_NAME.LOAD_DRY_BLOCK, false);
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

        #region UnloadMapTable
        public int UnloadingMapTable()
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOAD_MAP_TABLE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY RECEIVE CYCLE START", STEP_ELAPSED));
                    }
                    break;

                // 자재가 있을 경우 확인
                case 6:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;

                            if (GbVar.Seq.sMapTransfer.Info.IsStrip())
                            {
                                if (AirStatus(STRIP_MDL.MAP_VISION_TABLE_1 + ULD_TABLE_NO) == AIRSTATUS.VAC)
                                {
                                    if (IsInPosMapPkX(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO))
                                    {
                                        if ((ULD_TABLE_NO == 0 && IsInPosMapStgY1(POSDF.MAP_STAGE_STRIP_LOADING)) ||
                                            (ULD_TABLE_NO == 1 && IsInPosMapStgY2(POSDF.MAP_STAGE_STRIP_LOADING)))
                                        {
                                            NextSeq(13);
                                            return FNC.BUSY;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                // Z축 안전 위치 이동
                case 10:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
                            {
                                break;
                            }
                        }

                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 자재가 없을 경우 확인
                case 11:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;

                            if (!GbVar.Seq.sMapTransfer.Info.IsStrip())
                            {
                                if (AirStatus(STRIP_MDL.MAP_TRANSFER) != AIRSTATUS.NONE)
                                {
                                    nFuncResult = (int)ERDF.E_MAP_PICKER_VAC_N_DATA_MISMATCH;
                                }
                                else if (AirStatus(STRIP_MDL.MAP_VISION_TABLE_1 + ULD_TABLE_NO) != AIRSTATUS.NONE)
                                {
                                    nFuncResult = (int)ERDF.E_MAP_STAGE_1_VAC_N_DATA_MISMATCH + ULD_TABLE_NO;
                                }
                                else
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP NOT EXIST ", STEP_ELAPSED));

                                    NextSeq(50);
                                    return FNC.BUSY;
                                }
                            }
                        }

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapStgT(ULD_TABLE_NO,POSDF.MAP_STAGE_STRIP_LOADING))
                                break;
                        }
                        // 맵 스테이지 T축 로딩 위치 이동
                        nFuncResult = MovePosMapStgT(ULD_TABLE_NO, POSDF.MAP_STAGE_STRIP_LOADING);
                    }
                    break;


                // 맵 스테이지 Y축 로딩 위치 이동
                case 13:
                    {
                        if (ULD_TABLE_NO == 0)
                        {
                            if (m_bFirstSeqStep)
                            {
                                if (IsInPosMapStgY1(POSDF.MAP_STAGE_STRIP_LOADING))
                                    break;
                            }

                            nFuncResult = MovePosMapStgY1(POSDF.MAP_STAGE_STRIP_LOADING);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else
                        {
                            if (m_bFirstSeqStep)
                            {
                                if (IsInPosMapStgY2(POSDF.MAP_STAGE_STRIP_LOADING))
                                    break;
                            }

                            nFuncResult = MovePosMapStgY2(POSDF.MAP_STAGE_STRIP_LOADING);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }    
                        }
                    }
                    break;

                // 맵 피커 X축 언로딩 위치 이동
                case 15:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkX(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO))
                                break;
                        }

                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 맵 피커 SLOW DOWN 위치까지 이동
                case 20:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO))
                                break;
                        }

                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS UNLOADING SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 맵 피커 Z축 언로딩 위치 이동
                case 22:
                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS UNLOADING DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 맵 스테이지 VAC. 다음 시퀀스에서 센서 체크
                case 24:    
                    nFuncResult = MapStageVac(ULD_TABLE_NO, true, 1000, false);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP STAGE 1 ODD EVEN VACUUM ON COMPLETE", STEP_ELAPSED));
                    }

                    break;

                // 맵 피커 블로우
				case 27:
                    nFuncResult = MapPickerBlow(true, 1000, false);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                    }
                    
                    break;

                // 맵 스테이지 VAC 체크 (알람 시 맵 피커 BLOW OFF)
                case 28:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;

                    nFuncResult = VacBlowCheck((int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (ULD_TABLE_NO * 4), 
                                                true, 
                                                (int)ERDF.E_MAP_STAGE_1_WORK_VAC_NOT_ON + (ULD_TABLE_NO * 5),
                                                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_CHECK_TIME].lValue);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP STAGE 1 ODD EVEN VACUUM ON COMPLETE", STEP_ELAPSED));
                    }
                    else if(FNC.IsErr(nFuncResult))
                    {
                        //220513
                        //VAC알람 났을 시 Blow끔
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PK_BLOW, false);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PK_VAC_ON, false);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP, false);

                        //밑에서 알람 체크 하려고 
                        nFuncResult = FNC.SUCCESS;
                    }
                    break;
        
                case 30:
                    // 현재 위치를 확인
                    //Slow Down피치가 음수로 설정되어있을 경우를 대비해서 인터락 
                    double dCurrPos = MotionMgr.Inst[(int)SVDF.AXES.MAP_PK_Z].GetRealPos();
                    double dTargetPos = RecipeMgr.Inst.Rcp.dMotPos[(int)SVDF.AXES.MAP_PK_Z].dPos[POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        dTargetPos *= 0.1;
                    }

                    if (dCurrPos < dTargetPos)
                    {
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    // MAP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE

                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1 + ULD_TABLE_NO, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS UNLOADING SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // MAP STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 35:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == true) break;
                        if (!GbVar.Seq.sMapTransfer.Info.IsStrip()) break;

                        nFuncResult = VacBlowCheck((int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (ULD_TABLE_NO * 4),
                                                    true,
                                                    (int)ERDF.E_MAP_STAGE_1_WORK_VAC_NOT_ON + (ULD_TABLE_NO * 5),
                                                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_CHECK_TIME].lValue);

                    }
                    break;

                case 36:
                    // MAP TRANSFER BLOW OFF 20211111 CHOH : 블로우 오프 추가
                    {
                        nFuncResult = MapPickerBlow(false, 10, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 40:
                    {
                        if (!m_bAutoMode)
                        {
                            nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_READY);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        
                    }
                    break;

                case 50:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOAD_MAP_TABLE, false);
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

        /// <summary>
        /// 상부 얼라인을 한다.
        /// 맵 테이블 수평을 맞추고, 맵 인스펙션 위치를 보정한다. Pick-Up 시에도 보정 값이 들어간다. 
        /// 기존 Pick-Up 시 Map Inspection 티칭 위치에서 Map Inspection 보정 값을 더하기 떄문
        /// 2022.04.27.01 : Vision Interface를 Reset -> Trigger 1회 -> Comp로 변경
        /// 2022.04.27.02 : USE 변경
        /// </summary>
        /// <returns></returns>
        #region Top Align
        public int TopAlign()
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
                        // 값 초기화
                        SEQ_INFO_CURRENT.Info.InitTopAlignInfo();

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse)
                        {
                            return FNC.SUCCESS;
                        }

                        if (m_bFirstSeqStep)
                        {
                            m_nAlignNo = 0;
                            m_bAfterRotateCheck = false;

                            SEQ_INFO_CURRENT.nCurrGroupCount = 0;
                            SEQ_INFO_CURRENT.nCurrInspCount = 0;

                            //2022 08 24 HEP
                            //상부 탑 얼라인 초기화
                            //결과 값 초기화
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                            {
                                GbVar.dbGetTopAlign.InitData();
                            }
                        }
                    }
                    break;
                case 1:
                    {
                        if(m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            if (!GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.IsStrip())
                            {
                                break;
                            }

                            // 이미 Map Inspection 결과를 받은 상태
                            if (GbVar.Seq.sMapTransfer.bMapAlignDone)
                            {
                                // 알람 창에서 재검사가 아닌 진행을 눌렀을 때 SUCCESS
                                if (!GbVar.Seq.sMapTransfer.bRetryMapAlign)
                                {
                                    return FNC.SUCCESS;
                                }
                            }
                        }
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
                        }
                        if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;
#endif
                    }
                    break;
                case 2:
                    {
                        if (m_nRetryAlignCount > 0)
                        {
                            if (WaitDelay(200)) return FNC.BUSY;
                        }

                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                        IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + INSP_TABLE_NO);
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
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "TRAY RECEIVE CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 10:
                    // MAP TRANSFER Z READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION ALREADY COMPLETE", STEP_ELAPSED));
                                break;
                            }
                        }
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 11:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapVisionZ(POSDF.MAP_VISION_FOCUS_T1 + INSP_TABLE_NO))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER VISION Z AXIS FOCUS POSITION ALREADY COMPLETE", STEP_ELAPSED));
                                break;
                            }
                        }

                        // Vision Focus 위치 이동
                        nFuncResult = MovePosMapVisionZ(POSDF.MAP_VISION_FOCUS_T1 + INSP_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER VISION Z AXIS FOCUS POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 14:
                    {
                        nFuncResult = MoveTopAlignPosMapPkXYT(POSDF.MAP_STAGE_TOP_ALIGN_MARK1, INSP_TABLE_NO);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "TOP ALIGN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        // Inspection 시작위치 이동
                        // TableY, MapPickerX, MapPickerZ
                        nFuncResult = MoveTopAlignPosMapPkXY(POSDF.MAP_STAGE_TOP_ALIGN_MARK1 + m_nAlignNo, INSP_TABLE_NO);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER XY AXIS VISION ALIGN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 22:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;

                        SeqHistory(string.Format("GRAB DELAY COMPLETE : {0}", lDelay));
                    }
                    break;

                case 24:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        IFMgr.Inst.VISION.SetTopAlignCountReset(true);

                        SeqHistory(string.Format("Interface Reset TRUE"));
                    }
                    break;

                case 26:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        // True가 맞다..
#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetryAlignCount++;

                                IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                IFMgr.Inst.VISION.SetTopAlignCountComplete(true);

                                SeqHistory(string.Format("Interface Reset ON Fail Retry : {0}", m_nRetryAlignCount));

                                NextSeq(0);
                                return FNC.BUSY;
                            }
                            m_nRetryAlignCount = 0;

                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_RESET_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsTopAlignCountReset) return FNC.BUSY;
                        SeqHistory(string.Format("Interface Reset RECEIVE"));
#endif
                        // [2022.04.12.kmlee] 바로 끄지않고 Complete 주기 전 Reset OFF
                        //IFMgr.Inst.VISION.SetMapInspCountReset(false);

                        m_nRetryAlignCount = 0;
                    }
                    break;
                case 28:
                    {
                        // [2022.04.12.kmlee] Reset 끄는걸 기다리지 않음
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse)
                        //{
                        //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                        //               IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        //    {
                        //        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_1;
                        //    }
                        //    if (IFMgr.Inst.VISION.IsMapInspCountReset) return FNC.BUSY;
                        //}
                    }

                    break;
                case 30:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        // Trigger
                        IFMgr.Inst.VISION.TrgMapOneShot();
                        SeqHistory(string.Format("Interface Trigger One Shot"));
                    }
                    break;
                case 32:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetryAlignCount++;

                                IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                IFMgr.Inst.VISION.SetTopAlignCountComplete(true);

                                SeqHistory(string.Format("Interface Complete ON Fail Retry : {0}", m_nRetryAlignCount));

                                NextSeq(0);
                                return FNC.BUSY;
                            }
                            m_nRetryAlignCount = 0;

                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_ON;
                        }
                        if (!IFMgr.Inst.VISION.IsTopAlignCompleted) return FNC.BUSY;
#endif
                        SeqHistory(string.Format("Interface Complete ON CONFIRM"));
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);
                        SeqHistory(string.Format("Interface Complete TRUE"));

                        m_nRetryAlignCount = 0;
                    }
                    break;
                case 34:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetryAlignCount++;

                                IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                IFMgr.Inst.VISION.SetTopAlignCountComplete(false);

                                SeqHistory(string.Format("Interface Complete OFF Fail Retry : {0}", m_nRetryAlignCount));

                                NextSeq(0);
                                return FNC.BUSY;
                            }
                            m_nRetryAlignCount = 0;

                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_OFF;
                        }

                        if (IFMgr.Inst.VISION.IsTopAlignCompleted) return FNC.BUSY;
                        m_nRetryAlignCount = 0;

                        SeqHistory(string.Format("Interface Complete OFF CONFIRM"));
                        // [2022.04.13.kmlee] Complete 주고난 후 Reset OFF
                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                        SeqHistory(string.Format("Interface Reset, Complete FALSE"));
                    }
                    break;
                case 36:
                    {
                        // 비젼 화면의 좌상단이 X-, Y- 우하단이 X+,Y+
                        tagRetAlignData[] stAlignData = IFMgr.Inst.VISION.GetTopAlignResult();
              
                        // 0번 인덱스 데이터만 사용 (1번 마크 얼라인 -> 2번 마크 얼라인 -> Angle 보정 -> 1번 마크 얼라인)
                        // 기존 Reset -> Trigger -> Trigger -> Comp -> Get Result에서
                        // Reset -> Trigger -> Comp -> Get Result -> Reset -> Trigger -> Comp -> Get Result -> Reset -> Trigger -> Comp -> Get Result로 변경
                        if (m_bAfterRotateCheck)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (stAlignData.Length > 0)
                                {
                                    //2022 09 23 HEP
                                    // 0번으로만 써야함 //stAlignData[0]
                                    // Map Inspection 시 참조하는 X, Y 보정 값
                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.bTopAlignCorrectResult = stAlignData[0].bJudge;
                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.xyTopAlignCorrectOffset.x = stAlignData[0].dX;
                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.xyTopAlignCorrectOffset.y = stAlignData[0].dY; 
                           
                                }
                                else
                                {
                                    if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                    {
                                        m_nRetryAlignCount++;

                                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);

                                        SeqHistory(string.Format("After Rotate Data Casting Fail. Retry : {0}", m_nRetryAlignCount));

                                        NextSeq(0);
                                        return FNC.BUSY;
                                    }
                                    m_nRetryAlignCount = 0;

                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.bTopAlignCorrectResult = false;
                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.xyTopAlignCorrectOffset.x = 0;
                                    GbVar.Seq.sMapVisionTable[INSP_TABLE_NO].Info.xyTopAlignCorrectOffset.y = 0;

                                    //2022 08 25 HEP
                                    // DATA PARSING ERROR 추가
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                    {
                                        return (int)ERDF.E_TOP_ALIGN_DATA_MISSING_ERROR;
                                    }
                                }
                                // 재확인 시에는 바로 알람 (재확인 시에는 Mark 1번만 본다)
                                if (!SEQ_INFO_CURRENT.Info.bTopAlignResult[0])
                                {
                                    GbVar.Seq.sMapTransfer.bMapAlignDone = true;
                                    return (int)ERDF.E_MAP_STAGE_1_TOP_AIGN_NG + INSP_TABLE_NO;
                                }
                            }
                        }
                        else
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (stAlignData.Length > 0)
                                {
                                    // Angle을 보정하기 위한 X, Y 보정 값
                                    SEQ_INFO_CURRENT.Info.bTopAlignResult[m_nAlignNo] = stAlignData[0].bJudge;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].x = stAlignData[0].dX;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].y = stAlignData[0].dY;
                                }
                                else
                                {
                                    if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                    {
                                        m_nRetryAlignCount++;

                                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);

                                        SeqHistory(string.Format("Data Casting Fail. Retry : {0}", m_nRetryAlignCount));
                                        NextSeq(0);
                                        return FNC.BUSY;
                                    }
                                    m_nRetryAlignCount = 0;

                                    //2022 08 25 HEP
                                    // DATA PARSING ERROR 추가
                                    SEQ_INFO_CURRENT.Info.bTopAlignResult[m_nAlignNo] = false;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].x = 0;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].y = 0;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                    {
                                        return (int)ERDF.E_TOP_ALIGN_DATA_MISSING_ERROR;
                                    }
                                }
                            }
                            else
                            {
                                if (stAlignData.Length > 0)
                                {
                                    // Angle을 보정하기 위한 X, Y 보정 값
                                    SEQ_INFO_CURRENT.Info.bTopAlignResult[m_nAlignNo] = stAlignData[0].bJudge;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].x = stAlignData[0].dX;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].y = stAlignData[0].dY;
                                }
                                else
                                {
                                    if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                    {
                                        m_nRetryAlignCount++;

                                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);

                                        SeqHistory(string.Format("Data Casting Fail. Retry : {0}", m_nRetryAlignCount));
                                        NextSeq(0);
                                        return FNC.BUSY;
                                    }
                                    m_nRetryAlignCount = 0;

                                    //2022 08 25 HEP
                                    // DATA PARSING ERROR 추가
                                    SEQ_INFO_CURRENT.Info.bTopAlignResult[m_nAlignNo] = false;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].x = 0;
                                    SEQ_INFO_CURRENT.Info.xyTopAlignOffset[m_nAlignNo].y = 0;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                    {
                                        return (int)ERDF.E_TOP_ALIGN_DATA_MISSING_ERROR;
                                    }
                                }
                            }
                        }

                        SeqHistory(string.Format("Get Align Data"));
                    }
                    break;
                case 38:
                    {
                        // Angle 보정 후 재확인 하였다면 완료 시퀀스로 이동
                        if (m_bAfterRotateCheck)
                        {
                            NextSeq(60);
                            return FNC.BUSY;
                        }

                        // Angle 보정 전이면 얼라인 번호 확인 후 진행
                        m_nAlignNo++;

                        if (m_nAlignNo < 2)
                        {
                            NextSeq(20);
                            return FNC.BUSY;
                        }
                    }
                    break;
                #region Angle 보정 전 Angle 보정 부분
                case 40:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                break;
                        }

                        // 탑얼라인 NG이면 알람
                        if (!SEQ_INFO_CURRENT.Info.IsTopAlignOk)
                        {
                            GbVar.Seq.sMapTransfer.bMapAlignDone = true;
                            return (int)ERDF.E_MAP_STAGE_1_TOP_AIGN_NG + INSP_TABLE_NO;
                        }
                    }
                    break;
                case 42:
                    // LOADING RAIL ANGLE 보정값 계산
                    {
                        m_dTopAlignAngle = MotionMgr.Inst[SVDF.AXES.MAP_STG_1_T + INSP_TABLE_NO].GetRealPos() + CalcTopAlignAngle();

                        SEQ_INFO_CURRENT.Info.dTopAngleOffset = m_dTopAlignAngle;

                        SeqHistory(string.Format("{0} : {1}", "Top Angle Offset", m_dTopAlignAngle));
                    }
                    break;

                case 44:
                    // ANGLE 보정
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                break;
                        }

                        nFuncResult = MovePosMapStgT(INSP_TABLE_NO, m_dTopAlignAngle);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP TOP-ALIGN CORRECT POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 46:
                    {
                        if (m_bAfterRotateCheck == false)
                        {
                            // Angle 보정 후 위치 확인을 위하여 첫번째 얼라인 위치를 다시 진행
                            m_nAlignNo = 0;
                            m_bAfterRotateCheck = true;

                            NextSeq(20);
                            return FNC.BUSY;
                        }
                        else
                        {
                            // 여기는 들어오면 안된다. 여기 들어오면 시퀀스 처리를 잘못한 것임.
                            System.Diagnostics.Debugger.Break();
                            return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
                        }
                    }
                //break; 
                #endregion

                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TOP ALIGN CYCLE FINISH", STEP_ELAPSED, strStripInfo));

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

        #region MapVision Inspection
        public int MapVisionInspection()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }
#if _NOTEBOOK
            if (!IFMgr.Inst.VISION.GetMapInspAllResult(INSP_TABLE_NO))
            {
                if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                {
                    m_nRetryAlignCount++;

                    IFMgr.Inst.VISION.SetMapInspCountReset(false);
                    IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                    SeqHistory(string.Format("Map Insp Data Casting Fail. Retry : {0}", m_nRetryAlignCount));

                    NextSeq(0);
                    return FNC.BUSY;
                }
                m_nRetryAlignCount = 0;

                return (int)ERDF.E_TOP_VISION_INSPECTION_DATA_MISSING_ERROR;
            }
            GbVar.Seq.sMapTransfer.bMapInspDone = true;
            return FNC.SUCCESS;
#endif
            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        if (m_bFirstSeqStep)
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MAP_VISION_INSP, true);

                            // 현재 스트립 자재의 바코드를 비전에 전달
                            if (m_bAutoMode)
                            {
                                GbVar.dbSetVision.UpdateBarcode(GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.STRIP_ID);
                            }
                            else
                            {
                                GbVar.dbSetVision.UpdateBarcode("MANUAL");
                            }

                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
                                break;
                        }

                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);
                    }
                    break;
                case 1:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_VISION_USE].bOptionUse)
                            {
                                // 스킵해도 정상 진행되도록 결과를 가져온다
                                NextSeq(54);
                                return FNC.BUSY;
                            }

                            // 자재가 없다면
                            if (!GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }

                            // 이미 Map Inspection 결과를 받은 상태
                            if (GbVar.Seq.sMapTransfer.bMapInspDone)
                            {
                                // 알람 창에서 재검사가 아닌 진행을 눌렀을 때 SUCCESS
                                if (!GbVar.Seq.sMapTransfer.bRetryMapInsp)
                                {
                                    return FNC.SUCCESS;
                                }
                            }
                        }

                        //2022 08 24 HEP
                        //상부 인스펙션 데이터 초기화
                        //결과 값 초기화
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                        {
                            GbVar.dbGetTopInspection[GbVar.Seq.sMapTransfer.nUnloadingTableNo].InitData(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);
                        }

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP INSPECTION CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 2:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
                        }
                        if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;

                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, true);
                    }
                    break;
                case 3:
                    SEQ_INFO_CURRENT.nCurrGroupCount = 0;
                    SEQ_INFO_CURRENT.nCurrInspCount = 0;
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.bGrabOneEdge == true)
                    {
                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX / RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX);
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX % RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX > 0) 
                            RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX++;

                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY / RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY);
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY % RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY > 0)
                            RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY++;
                    }
                    else
                    {
                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX / RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX) + 1;
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX % RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX > 0) RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX++;

                        RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY / RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY) + 1;
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY % RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY > 0) RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY++;
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                        !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                        break;

                    IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + INSP_TABLE_NO);
                    IFMgr.Inst.VISION.SetMapInspCountReset(true);
                    break;

                case 4:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                        !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                        break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetryAlignCount++;

                            IFMgr.Inst.VISION.SetMapInspCountReset(false);
                            IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetryAlignCount = 0;

                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_RESET_ON;
                    }

                    if (!IFMgr.Inst.VISION.IsMapInspCountReset) return FNC.BUSY;
                    // [2022.04.12.kmlee] 바로 끄지않고 Complete 주기 전 Reset OFF
                    //IFMgr.Inst.VISION.SetMapInspCountReset(false);
                    m_nRetryAlignCount = 0;
                    break;

                case 5:
                    // MAP TRANSFER Z READY POS MOVE
                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 10:
                    {
                        // Vision Focus 위치 이동
                        nFuncResult = MovePosMapVisionZ(POSDF.MAP_VISION_FOCUS_T1 + INSP_TABLE_NO);
                    }
                    break;

                case 20:
                    // Inspection 시작위치 이동
                    // TableY, MapPickerX, MapPickerZ
                    nFuncResult = MoveMapInspPosMapPkXY(POSDF.MAP_PICKER_MAP_VISION_START_T1, INSP_TABLE_NO);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                    }
                    break;
                case 25:
                    {
                        // UPH 계산
                        if (GbVar.g_swUPH.IsRunning)
                        {
							// [2022.06.05.kmlee] 최소치만 UPH에 기록되게끔
                            double dUPH = 3600 * (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY) / GbVar.g_swUPH.Elapsed.TotalSeconds;
                            if (GbVar.g_dUPH < 1 || GbVar.g_dUPH > dUPH)
                            {
                                GbVar.g_dUPH = dUPH;
                            }
                        }

                        // 재시작
                        GbVar.g_swUPH.Restart();
                    }
                    break;

                /////////////////////////////////////////////////////////////////////////////
                /// 반복 시작
                /////////////////////////////////////////////////////////////////////////////
                ///
                case 30:
                    if ((RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY) <= SEQ_INFO_CURRENT.nCurrGroupCount)
                    {
                        NextSeq(60);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    int nInspCntX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX / RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX;
                    int nInspCntY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY / RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY;
                    int nMaxCntX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX;
                    int nMaxCntY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY * RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY;
                   
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.bGrabOneEdge == true)
                    {
                        nMaxCntX += 1;
                        nMaxCntY *= 2;
                        if (SEQ_INFO_CURRENT.nCurrInspCount == nMaxCntY * nMaxCntX)
                        {
                            // 20220727 bhoh
                            // DB 읽은 Data를 변수에 그룹별 저장해야 함...
                            // Map의 중심 좌표점을 여기서 모두 계산해서 넣으면 굳이 Pick-up 시 그룹별로 안해도 될 것 같음
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_VISION_USE].bOptionUse) CmdInspectionDBRead();

                            NextSeq(50);
                            return FNC.BUSY;
                        }
                        else if((SEQ_INFO_CURRENT.nCurrInspCount) % 4 == 0 &&
                            SEQ_INFO_CURRENT.nCurrInspCount != 0)
                        {
                            SEQ_INFO_CURRENT.nCurrGroupCount++;
                        }
                    }
                    else
                    {
                        if ((nMaxCntX * nMaxCntY) <= SEQ_INFO_CURRENT.nCurrInspCount)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_VISION_USE].bOptionUse) CmdInspectionDBRead();

                            NextSeq(50);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 34:
                    // Inspection Next 이동
                    // TableY, MapPickerX, MapPickerZ

                    if (RecipeMgr.Inst.Rcp.MapTbInfo.bGrabOneEdge == true)
                    {
                        nFuncResult = MovePosMapInspOnlyOneEdge(SEQ_INFO_CURRENT.nCurrGroupCount, SEQ_INFO_CURRENT.nCurrInspCount, INSP_TABLE_NO);
                    }
                    else
                    {
                        nFuncResult = MovePosMapInspXYNext(SEQ_INFO_CURRENT.nCurrGroupCount, SEQ_INFO_CURRENT.nCurrInspCount, INSP_TABLE_NO);
                    }

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "INSPECTION STAGE MOVE TO NEXT POSITION", STEP_ELAPSED));
                    }
                    break;

                case 36:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 38:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                        !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                        break;

                    // TriggerShot
                    IFMgr.Inst.VISION.TrgMapOneShot();
                    break;

                case 40:
                    SEQ_INFO_CURRENT.nCurrInspCount++;
                    NextSeq(32);
                    return FNC.BUSY;
                /////////////////////////////////////////////////////////////////////////////
                /// 반복 끝
                /////////////////////////////////////////////////////////////////////////////

                case 50:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                        !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                        break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetryAlignCount++;

                            IFMgr.Inst.VISION.SetMapInspCountReset(false);
                            IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                            SeqHistory(string.Format("Interface Complete ON Fail. Retry : {0}", m_nRetryAlignCount));

                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetryAlignCount = 0;

                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_ON;
                    }

                    if (!IFMgr.Inst.VISION.IsMapInspCompleted) return FNC.BUSY;
                    m_nRetryAlignCount = 0;

                    // [2022.04.13.kmlee] Comp 전 Reset OFF. false가 최종
                    IFMgr.Inst.VISION.SetMapInspCountComplete(true);

                    break;

                case 52:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                        !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                        break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetryAlignCount++;

                            IFMgr.Inst.VISION.SetMapInspCountReset(false);
                            IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                            SeqHistory(string.Format("Interface Complete OFF Fail. Retry : {0}", m_nRetryAlignCount));

                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetryAlignCount = 0;

                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, false);
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_OFF;
                    }

                    if (IFMgr.Inst.VISION.IsMapInspCompleted) return FNC.BUSY;
                    m_nRetryAlignCount = 0;

                    IFMgr.Inst.VISION.SetMapInspCountReset(false);
                    IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, false);
                    break;

                case 54:
                    //220427 pjh
                    //드라이런, 비전 인터페이스 사용 유무에 상관 없이 DB결과는 불러 옴.
                    //단, 불러온 결과를 사용하는 조건은 아래의 함수 확인할 것
                    if(!IFMgr.Inst.VISION.GetMapInspAllResult(INSP_TABLE_NO))
                    {
                        if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetryAlignCount++;

                            IFMgr.Inst.VISION.SetMapInspCountReset(false);
                            IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                            SeqHistory(string.Format("Map Insp Data Casting Fail. Retry : {0}", m_nRetryAlignCount));

                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetryAlignCount = 0;

                        return (int)ERDF.E_TOP_VISION_INSPECTION_DATA_MISSING_ERROR;
                    }
                    m_nRetryAlignCount = 0;

                    GbVar.Seq.sMapTransfer.bMapInspDone = true;

                    break;

                case 56:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
                            !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;

                        if (m_bAutoMode)
                        {
                            // 에러 카운트를 확인해서 알람 (창에 YES/NO를 선택할 수 있게)
                            // YES면 재검사, NO이면 강제 진행
                            // 0번 case의 GbVar.Seq.sMapTransfer.bRetryMapInsp 참조
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_CHECK_COUNT].nValue > 0)
                            {
                                if (IFMgr.Inst.VISION.GetMapInspErrorCount(INSP_TABLE_NO) >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_CHECK_COUNT].nValue)
                                {
                                    return (int)ERDF.E_MAP_INSPECTION_ERROR_COUNT_OVER;
                                }
                            }
                        }
                    }
                    break;

                case 58:
                    //SEQ_INFO_CURRENT.nCurrGroupCount++;
                    //NextSeq(30);
                    //return FNC.BUSY;
                    break;
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE CYCLE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MAP_VISION_INSP, false);

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

        #region MapVision X-Mark
        //public int MapVisionXMark()
        //{
        //    if (m_nSeqNo != m_nPreSeqNo)
        //    {
        //        ResetCmd();

        //        if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
        //    }

        //    m_nPreSeqNo = m_nSeqNo;
        //    nFuncResult = FNC.SUCCESS;

        //    switch (m_nSeqNo)
        //    {
        //        case 0:
        //            {
        //                if (m_bFirstSeqStep)
        //                {
        //                    //TTDF.SetTact((int)TTDF.CYCLE_NAME.MAP_VISION_INSP, true);

        //                    // 현재 스트립 자재의 바코드를 비전에 전달
        //                    if (m_bAutoMode)
        //                    {
        //                        GbVar.dbSetVision.UpdateBarcode(GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.STRIP_ID);
        //                    }
        //                    else
        //                    {
        //                        GbVar.dbSetVision.UpdateBarcode("MANUAL");
        //                    }

        //                    if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
        //                        break;
        //                }

        //                nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);
        //            }
        //            break;
        //        case 1:
        //            {
        //                if (m_bAutoMode)
        //                {
        //                    if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
        //                    {
        //                        return FNC.CYCLE_CHECK;
        //                    }

        //                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_XMARK_USE].bOptionUse)
        //                    {
        //                        IFMgr.Inst.VISION.UpdateAllTopXMarkResult(INSP_TABLE_NO, VSDF.eJUDGE_MAP.OK);
        //                        return FNC.SUCCESS;
        //                    }

        //                    // 자재가 없다면
        //                    if (!GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.IsStrip())
        //                    {
        //                        return FNC.SUCCESS;
        //                    }

        //                    //// 이미 Map Inspection 결과를 받은 상태
        //                    //if (GbVar.Seq.sMapTransfer.bMapInspDone)
        //                    //{
        //                    //    // 알람 창에서 재검사가 아닌 진행을 눌렀을 때 SUCCESS
        //                    //    if (!GbVar.Seq.sMapTransfer.bRetryMapInsp)
        //                    //    {
        //                    //        return FNC.SUCCESS;
        //                    //    }
        //                    //}
        //                }
        //                //2022 08 24 HEP
        //                //상부 X MARK 데이터 초기화
        //                //결과 값 초기화
        //                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
        //                {
        //                    GbVar.dbGetTopXMark[GbVar.Seq.sMapTransfer.nUnloadingTableNo].InitData(RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX, RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY);
        //                }

        //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP X-MARK CYCLE START", STEP_ELAPSED));
        //            }
        //            break;

        //        case 2:
        //            {
        //                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                    !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                    break;

        //                if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
        //                {
        //                    return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
        //                }
        //                if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;

        //                IFMgr.Inst.VISION.InitTopCamInterface();

        //                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, true);
        //            }
        //            break;
        //        case 3:
        //            SEQ_INFO_CURRENT.nCurrInspCount = 0;
        //            SEQ_INFO_CURRENT.nUnitXMarkCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
        //            SEQ_INFO_CURRENT.nUnitXMarkCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

        //            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                break;

        //            IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + INSP_TABLE_NO);
        //            IFMgr.Inst.VISION.SetMapXMarkCountReset(true);
        //            break;

        //        case 4:
        //            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                break;

        //            if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
        //            {
        //                if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
        //                {
        //                    m_nRetryAlignCount++;

        //                    IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //                    IFMgr.Inst.VISION.SetMapXMarkCountComplete(false);
        //                    NextSeq(0);
        //                    return FNC.BUSY;
        //                }

        //                m_nRetryAlignCount = 0;
        //                return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_RESET_ON;
        //            }

        //            if (!IFMgr.Inst.VISION.IsMapXMarkCountReset) return FNC.BUSY;
        //            m_nRetryAlignCount = 0;

        //            IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //            break;

        //        case 5:
        //            // MAP TRANSFER Z READY POS MOVE
        //            {
        //                nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);

        //                if (FNC.IsSuccess(nFuncResult))
        //                {
        //                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
        //                }
        //            }
        //            break;

        //        case 10:
        //            {
        //                // Vision Focus 위치 이동
        //                nFuncResult = MovePosMapVisionZ(POSDF.MAP_VISION_FOCUS_T1 + INSP_TABLE_NO);
        //            }
        //            break;

        //        case 20:
        //            // Inspection 시작위치 이동
        //            // TableY, MapPickerX, MapPickerZ
        //            nFuncResult = MoveMapInspPosMapPkXY(POSDF.MAP_PICKER_MAP_VISION_START_T1, INSP_TABLE_NO);

        //            if (FNC.IsSuccess(nFuncResult))
        //            {
        //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
        //            }
        //            break;
        //        case 25:
        //            {
        //            }
        //            break;

        //        /////////////////////////////////////////////////////////////////////////////
        //        /// 반복 시작
        //        /////////////////////////////////////////////////////////////////////////////
        //        case 30:
        //            if ((SEQ_INFO_CURRENT.nUnitXMarkCountX * SEQ_INFO_CURRENT.nUnitXMarkCountY) <= SEQ_INFO_CURRENT.nCurrInspCount)
        //            {
        //                NextSeq(50);
        //                return FNC.BUSY;
        //            }
        //            break;

        //        case 32:
        //            if (m_bFirstSeqStep)
        //            {

        //            }
        //            // Inspection Next 이동
        //            // TableY, MapPickerX, MapPickerZ
        //            nFuncResult = MovePosMapXMarkXYNext(SEQ_INFO_CURRENT.nCurrInspCount, INSP_TABLE_NO);

        //            if (FNC.IsSuccess(nFuncResult))
        //            {
        //                SeqHistory(string.Format("ELAPSED, {0}, {1}", "INSPECTION STAGE MOVE TO NEXT POSITION", STEP_ELAPSED));
        //            }
        //            break;

        //        case 34:
        //            // GRAB DELAY
        //            {
        //                long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_VISION_GRAB_DELAY].lValue;
        //                if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
        //                    return FNC.BUSY;
        //            }
        //            break;

        //        case 36:
        //            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                break;

        //            // TriggerShot
        //            IFMgr.Inst.VISION.TrgMapOneShot();
        //            break;

        //        case 38:
        //            SEQ_INFO_CURRENT.nCurrInspCount++;
        //            NextSeq(30);
        //            return FNC.BUSY;
        //        /////////////////////////////////////////////////////////////////////////////
        //        /// 반복 끝
        //        /////////////////////////////////////////////////////////////////////////////

        //        case 50:
        //            // Map Stage Unloading Pos move
        //            break;

        //        case 52:
        //            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                break;

        //            if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
        //            {
        //                if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
        //                {
        //                    m_nRetryAlignCount++;

        //                    IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //                    IFMgr.Inst.VISION.SetMapXMarkCountComplete(false);
        //                    NextSeq(0);
        //                    return FNC.BUSY;
        //                }

        //                m_nRetryAlignCount = 0;
        //                return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_ON;
        //            }
        //            if (!IFMgr.Inst.VISION.IsMapXMarkCompleted) return FNC.BUSY;
        //            m_nRetryAlignCount = 0;

        //            // [2022.04.13.kmlee] Comp 전 Reset OFF. false가 최종
        //            IFMgr.Inst.VISION.SetMapXMarkCountComplete(true);

        //            break;

        //        case 53:
        //            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                break;

        //            if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
        //            {
        //                if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
        //                {
        //                    m_nRetryAlignCount++;

        //                    IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //                    IFMgr.Inst.VISION.SetMapXMarkCountComplete(false);
        //                    NextSeq(0);
        //                    return FNC.BUSY;
        //                }
        //                m_nRetryAlignCount = 0;

        //                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, false);
        //                return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_OFF;
        //            }

        //            if (IFMgr.Inst.VISION.IsMapXMarkCompleted) return FNC.BUSY;
        //            m_nRetryAlignCount = 0;

        //            IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //            IFMgr.Inst.VISION.SetMapXMarkCountComplete(false);
        //            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MARK_VISION_BLOW, false);
        //            break;

        //        case 54:

        //            if (!IFMgr.Inst.VISION.GetMapXMarkAllResult(INSP_TABLE_NO))
        //            {
        //                if (m_nRetryAlignCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
        //                {
        //                    m_nRetryAlignCount++;

        //                    IFMgr.Inst.VISION.SetMapXMarkCountReset(false);
        //                    IFMgr.Inst.VISION.SetMapXMarkCountComplete(false);
        //                    NextSeq(0);
        //                    return FNC.BUSY;
        //                }
        //                m_nRetryAlignCount = 0;

        //                return (int)ERDF.E_X_MARK_INSPECTION_DATA_MISSING_ERROR;
        //            }
        //            m_nRetryAlignCount = 0;

        //            //GbVar.Seq.sMapTransfer.bMapInspDone = true;

        //            break;
        //        case 55:
        //            {
        //                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse &&
        //                    !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
        //                    break;

        //                //if (m_bAutoMode)
        //                //{
        //                //    // 에러 카운트를 확인해서 알람 (창에 YES/NO를 선택할 수 있게)
        //                //    // YES면 재검사, NO이면 강제 진행
        //                //    // 0번 case의 GbVar.Seq.sMapTransfer.bRetryMapInsp 참조
        //                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_CHECK_COUNT].nValue > 0)
        //                //    {
        //                //        if (IFMgr.Inst.VISION.GetMapInspErrorCount(INSP_TABLE_NO) >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_CHECK_COUNT].nValue)
        //                //        {
        //                //            return (int)ERDF.E_MAP_INSPECTION_ERROR_COUNT_OVER;
        //                //        }
        //                //    }
        //                //}
        //            }
        //            break;

        //        case 60:
        //            {
        //                string strStripInfo = string.Format("STRIP ID : {0}", SEQ_INFO_CURRENT.Info.STRIP_ID);
        //                SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "MAP VISION XMARK CYCLE FINISH", STEP_ELAPSED, strStripInfo));
        //                //TTDF.SetTact((int)TTDF.CYCLE_NAME.MAP_VISION_INSP, false);

        //                return FNC.SUCCESS;
        //            }
        //        default:
        //            break;
        //    }

        //    #region AFTER SWITCH
        //    if (m_bFirstSeqStep)
        //    {
        //        // Position Log
        //        if (string.IsNullOrEmpty(m_strMotPos) == false)
        //        {
        //            SeqHistory(m_strMotPos);
        //        }

        //        m_bFirstSeqStep = false;
        //    }

        //    if (FNC.IsErr(nFuncResult))
        //    {
        //        return nFuncResult;
        //    }
        //    else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

        //    m_nSeqNo++;

        //    if (m_nSeqNo > 10000)
        //    {
        //        System.Diagnostics.Debugger.Break();
        //        FINISH = true;
        //        return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
        //    }

        //    return FNC.BUSY;
        //    #endregion
        //}
        #endregion

        #region Manual Align Interface
        /// <summary>
        /// 수동으로 Align Interface 용도
        /// </summary>
        /// <param name="nTableNo">테이블 번호</param>
        /// <returns></returns>
        public int ManualTopAlignInterface(int nTableNo, int nMaxTriggerCount)
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
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);

                        m_nManualTriggerCount = 0;
                    }
                    break;

                case 2:
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;
                case 4:
                    {
                        IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + nTableNo);
                        IFMgr.Inst.VISION.SetTopAlignCountReset(true);
                    }
                    break;
                case 6:
                    {
#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_RESET_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsTopAlignCountReset) return FNC.BUSY;
#endif

//                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                    }
                    break;
                case 8:
                    {
//#if !_NOTEBOOK
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
//                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
//                        {
//                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_1;
//                        }
//                        if (IFMgr.Inst.VISION.IsTopAlignCountReset) return FNC.BUSY;
//#endif
                    }
                    break;
                case 10:
                    {
                        // Trigger
                        IFMgr.Inst.VISION.TrgMapOneShot();

                        m_nManualTriggerCount++;
                    }
                    break;
                case 12:
                    {
                        if (WaitDelay(500)) return FNC.BUSY;

                        if (m_nManualTriggerCount < nMaxTriggerCount)
                        {
                            NextSeq(10);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 14:
                    {
#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_ON;
                        }
                        if (!IFMgr.Inst.VISION.IsTopAlignCompleted) return FNC.BUSY;
#endif
                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);
                    }
                    break;
                case 16:
                    {
#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_OFF;
                        }
                        if (IFMgr.Inst.VISION.IsTopAlignCompleted) return FNC.BUSY;
#endif
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                    }
                    break;
                case 18:
                    {
                        tagRetAlignData[] stAlignData = IFMgr.Inst.VISION.GetTopAlignResult();


                    }
                    break;

                case 90:
                    {
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

        public int ManualMapInspInterface(int nTableNo, int nMaxTriggerCount)
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
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);

                        m_nManualTriggerCount = 0;
                    }
                    break;

                case 2:
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;
                case 4:
                    {
                        IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + nTableNo);
                        IFMgr.Inst.VISION.SetMapInspCountReset(true);
                    }
                    break;
                case 6:
                    {
#if !_NOTEBOOK
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_RESET_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsMapInspCountReset) return FNC.BUSY;
#endif

//                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                    }
                    break;
                case 8:
                    {
//#if !_NOTEBOOK
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
//                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
//                        {
//                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_1;
//                        }
//                        if (IFMgr.Inst.VISION.IsMapInspCountReset) return FNC.BUSY;
//#endif
                    }
                    break;
                case 10:
                    {
                        // Trigger
                        IFMgr.Inst.VISION.TrgMapOneShot();

                        m_nManualTriggerCount++;
                    }
                    break;
                case 12:
                    {
                        if (WaitDelay(500)) return FNC.BUSY;

                        if (m_nManualTriggerCount < nMaxTriggerCount)
                        {
                            NextSeq(10);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 14:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_ON;
                        }
                        if (!IFMgr.Inst.VISION.IsMapInspCompleted) return FNC.BUSY;
                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                        IFMgr.Inst.VISION.SetMapInspCountComplete(true);
                    }
                    break;
                case 16:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_MAP_INSP_COMP_OFF;
                        }
                        if (IFMgr.Inst.VISION.IsMapInspCompleted) return FNC.BUSY;
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                    }
                    break;
                case 18:
                    {
                        if(!IFMgr.Inst.VISION.GetMapInspAllResult(nTableNo))
                        {
                            return (int)ERDF.E_VISION_DATA_PARSING_ERROR;
                        }
                    }
                    break;

                case 90:
                    {
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

        public int ManualBallInspInterface(int nHeadNo, int nMaxTriggerCount)
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
                        IFMgr.Inst.VISION.SetBallInspCountComplete(false);

                        m_nManualTriggerCount = 0;
                    }
                    break;

                case 2:
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BALL_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;
                case 4:
                    {
                        IFMgr.Inst.VISION.SetBallHeadNo(MCDF.eUNIT.NO1 + nHeadNo);
                        IFMgr.Inst.VISION.SetBallInspCountReset(true);
                    }
                    break;
                case 6:
                    {
#if !_NOTEBOOK
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsBallCountReset) return FNC.BUSY;
#endif

//                        IFMgr.Inst.VISION.SetBallInspCountReset(false);
                    }
                    break;
                case 8:
                    {
//#if !_NOTEBOOK
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
//                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
//                        {
//                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_1;
//                        }
//                        if (IFMgr.Inst.VISION.IsBallCountReset) return FNC.BUSY;
//#endif
                    }
                    break;
                case 10:
                    {
                        // Trigger
                        IFMgr.Inst.VISION.TrgBallOneShot();

                        m_nManualTriggerCount++;
                    }
                    break;
                case 12:
                    {
                        if (WaitDelay(100)) return FNC.BUSY;

                        if (m_nManualTriggerCount < nMaxTriggerCount)
                        {
                            NextSeq(10);
                            return FNC.BUSY;
                        }
                    }
                    break;
                case 14:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_ON;
                        }
                        if (!IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                        IFMgr.Inst.VISION.SetBallInspCountReset(false);
                        IFMgr.Inst.VISION.SetBallInspCountComplete(true);
                    }
                    break;
                case 16:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_OFF;
                        }
                        if (IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                        IFMgr.Inst.VISION.SetBallInspCountComplete(false);
                    }
                    break;
                case 18:
                    {
                        IFMgr.Inst.VISION.GetBallInspResult(nHeadNo);
                    }
                    break;

                case 90:
                    {
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

        #region Air Dry
        /// <summary>
        /// 220507 pjh추가
        /// 맵피커가 드라이블럭에 있는 자재를 에어 드라이한다.
        /// </summary>
        /// <returns></returns>
        public int AirDry()
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
                        if (m_bFirstSeqStep)
                        {
                            m_nAirDryCnt = 0;
                            m_bFirstSeqStep = false;
                        }
                    }
                    break;
                case 2:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
                                break;
                        }

                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);
                    }
                    break;
                // 자재 정보가 없을 경우
                case 3:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.Seq.sUnitDry.Info.IsStrip() == false)
                            {
                                return FNC.SUCCESS;
                            }
                        }

                    }
                    break;
                case 4://시작 위치 
                    {              
                        if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT].nValue == 0)
                            return FNC.SUCCESS;

                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_AIR_DRY_START);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS AIR DRY START POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 5://끝 위치 
                    {
                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_AIR_DRY_END, 500);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS AIR DRY END POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 10://반복 횟수 확인
                    {
                        if (m_nAirDryCnt + 1 >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_AIR_DRY_REPEAT_COUNT].nValue)
                        {
                            return FNC.SUCCESS;
                        }

                        m_nAirDryCnt++;

                        NextSeq(4);
                        return FNC.BUSY;
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

        #region Air Knife
        /// <summary>
        /// jy.yang 추가
        /// Map Table 위 자재를 에어 드라이한다. (Semi Auto 용)
        /// </summary>
        /// <returns></returns>
        public int AirKnife()
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
                        if (m_bFirstSeqStep)
                        {
                            m_nAirDryCnt = 0;
                            m_bFirstSeqStep = false;
                        }
                    }
                    break;
                //AIR KNIFE 동작 - jy.yang
                case 2:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Start pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosMapStgY(m_nInspectionTableNo, POSDF.MAP_STAGE_AIR_KNIFE_START);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Start pos.", "Map Stage", "Done");
                    }
                    break;

                case 3:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Blow ON", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MapStageAirKnife(m_nInspectionTableNo, true);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Blow ON.", "Map Stage", "Done");
                    }
                    break;

                case 4:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife End pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosMapStgY(m_nInspectionTableNo, POSDF.MAP_STAGE_AIR_KNIFE_END);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife End pos.", "Map Stage", "Done");
                    }

                    break;

                case 10:
                    if (m_nAirDryCnt + 1 >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_AIRKNIFE_COUNT].lValue)
                    {
                        break;
                    }

                    m_nAirDryCnt++;
                    NextSeq(3);
                    return FNC.BUSY;

                case 11:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Blow OFF", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MapStageAirKnife(m_nInspectionTableNo, false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Blow OFF.", "Map Stage", "Done");
                        return FNC.SUCCESS;
                    }
                    return FNC.BUSY;

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

        double CalcTopAlignAngle()
        {
            double dAngle = 0.0;

            if(m_bAutoMode)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    return dAngle;
            }


            double dPosX1 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + INSP_TABLE_NO];
            double dPosY1 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y + INSP_TABLE_NO].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1];

            double dPosX2 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_PK_X].dPos[POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + INSP_TABLE_NO];
            double dPosY2 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y + INSP_TABLE_NO].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK2];

            if (INSP_TABLE_NO == 0)
            {
                dPosX1 = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX;
                dPosY1 = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY;
                dPosX1 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosY1 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);

                dPosX2 = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosX;
                dPosY2 = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosY;
                dPosX2 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosY2 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);

            }
            else
            {
                dPosX1 = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX;
                dPosY1 = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY;
                dPosX1 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosY1 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);

                dPosX2 = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosX;
                dPosY2 = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosY;
                dPosX2 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosY2 -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }

            dPosX1 += SEQ_INFO_CURRENT.Info.xyTopAlignOffset[0].x;
            dPosY1 += SEQ_INFO_CURRENT.Info.xyTopAlignOffset[0].y;

            dPosX2 += SEQ_INFO_CURRENT.Info.xyTopAlignOffset[1].x;
            dPosY2 += SEQ_INFO_CURRENT.Info.xyTopAlignOffset[1].y;

            // [2022.05.06.kmlee] 90에서 빼는게 최종
            //dAngle = MathUtil.GetAngle(dPosX1, dPosY1, dPosX2, dPosY2) - 90.0;
            dAngle = 90.0 - MathUtil.GetAngle(dPosX1, dPosY1, dPosX2, dPosY2);

            // Theta축의 Angle이 정확히 맞지 않는다. 0.1도를 움직이면 0.05도 정도 움직인다
            //dAngle *= 2.0;

            return dAngle;
        }
    }
}
