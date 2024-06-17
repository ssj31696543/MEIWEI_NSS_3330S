using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSS_3330S.SEQ.CYCLE
{
    public class cycLoader_MzTransfer : SeqBase
    {
        bool m_bAutoMode = true;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        #region PROPERTY
        public seqLdMzLoading SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sLdMzLoading; }
        }

        #endregion

        bool m_bMgzYStop = false;
        bool m_bMgzZStop = false;

        public cycLoader_MzTransfer(int nSeqID)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLdMzLoading;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            // 매뉴얼 싸이클 동작 시 컨베이어 포트 번호 받아 옴
            //m_nWorkingLdConv = (int)args[0];
            //m_nWorkingUldConv = (int)args[1];
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }


        #region READY
        public int MgzTransferReadyPos()
        {
            try
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
                                //SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                                m_bFirstSeqStep = false;
                            }
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
                        {
                            // 매거진이 클램프 상태인지 확인

                            // 실린더 및 인터락 확인

                        }
                        break;

                    case 8:
                        {
                            //if (!IsLdElvCheckSensor_MgzEmpty())
                            //{
                            //    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            //    {
                            //        // 매거진 센서가 하나라도 들어온다면 매거진이 있다고 판단하고 Y축 위치 상태를 확인하기 위함

                            //        // 매거진 X축 이동 시 매거진 Y축이 매거진 공급 위치보다 크다면 컨베이어쪽으로 나가있는 상태이기 때문에 X축을 이동 하면 안됨
                            //        if (SafetyMgr.Inst.IsMoreThanPosNo(SVDF.AXES.SPARE_63, POSDF.MGZ_LD_ELEV_STRIP_SUPPLY, 10.0))
                            //        {
                            //            return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_NOT_SAFETY_Y_POS;
                            //        }
                            //    }
                            //    else 
                            //    {
                            //        if (SafetyMgr.Inst.IsMoreThanPosNo(SVDF.AXES.SPARE_63, POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE, 10.0))
                            //        {
                            //            return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_NOT_SAFETY_Y_POS;
                            //        }
                            //    }
                            //}
                        }
                        break;
                    case 10:
                        {

                        }
                        break;

                    case 14:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    //if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    //{
                                    //    if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                                    //    {
                                    //        m_bMgzYStop = true;
                                    //        MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                                    //        ResetCmd();
                                    //    }
                                    //    return FNC.CYCLE_CHECK;
                                    //}
                                }
                            }
                            //if (m_bMgzYStop)
                            //{
                            //    if (!MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //    {
                            //        m_bMgzYStop = false;
                            //    }
                            //    return FNC.CYCLE_CHECK;
                            //}

                            nFuncResult = FNC.SUCCESS;

                            //if (m_bFirstSeqStep)
                            //{
                            //    if (IsInPosMgzLdElvY(POSDF.MGZ_LD_ELEV_READY)) break;
                            //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS READY MOVE START", STEP_ELAPSED));
                            //}
                            //// Y축 매거진 공급 위치로 이동
                            //nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_READY);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 15:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            //if (!IsInPosMgzLdElvY(POSDF.MGZ_LD_ELEV_READY))
                            //{
                            //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                            //    {
                            //        // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            //        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            //        {
                            //            if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //            {
                            //                MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                            //                ResetCmd();
                            //            }
                            //            return FNC.CYCLE_CHECK;
                            //        }
                            //    }
                            //    NextSeq(14);
                            //    return FNC.BUSY;
                            //}
                        }
                        break;
                    case 16:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            m_bMgzZStop = true;
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                            }
                            if (m_bMgzZStop)
                            {
                                if (!MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                {
                                    m_bMgzZStop = false;
                                }
                                return FNC.CYCLE_CHECK;
                            }
                            if (m_bFirstSeqStep)
                            {
                                if (IsInPosMgzLdElvZ(POSDF.MGZ_LD_ELEV_READY)) break;
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Z AXIS READY MOVE START", STEP_ELAPSED));
                            }
                            // 매거진 X, Z축 동시 동작
                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_READY);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 17:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            if (!IsInPosMgzLdElvZ(POSDF.MGZ_LD_ELEV_READY))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                                NextSeq(16);
                                return FNC.BUSY;
                            }
                        }
                        break;

                    case 20:
                        {
                            // 완료
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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }
        #endregion

        #region SUPPLY POS
        public int MgzTransferSupplyPos()
        {
            try
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
                                //SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                                m_bFirstSeqStep = false;
                            }
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
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_SUPPLY, true);

                        }
                        break;

                    case 6:
                        {
                            // 매거진이 클램프 상태인지 확인

                            // 실린더 및 인터락 확인

                        }
                        break;

                    case 8:
                        {
                            //Dry Run 모드 
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            if (!IsLdElvCheckSensorOn())
                            {
                                // 매거진 체크 센서가 안들어 온 경우 에러 처리
                                return (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST;
                            }
                        }
                        break;
                    case 10:
                        {

                        }
                        break;

                    case 14:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    //if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    //{
                                    //    if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                                    //    {
                                    //        m_bMgzYStop = true;
                                    //        MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                                    //        ResetCmd();
                                    //    }
                                    //    return FNC.CYCLE_CHECK;
                                    //}
                                }
                            }
                            //if (m_bMgzYStop)
                            //{
                            //    if (!MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //    {
                            //        m_bMgzYStop = false;
                            //    }
                            //    return FNC.CYCLE_CHECK;
                            //}

                            //if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            //{
                            //    // Y축 매거진 공급 위치로 이동
                            //    nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY);
                            //}
                            //else
                            //{
                            //    // Y축 매거진 공급 위치로 이동
                            //    nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE);
                            //}

                            nFuncResult = FNC.SUCCESS;

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS SUPPLY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 15:
                        {
                            int nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            }
                            else
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                            }

                            //221026 HEP 세이프티 센서에 따른 정지기능
                            //if (!IsInPosMgzLdElvY(nPosNo))
                            //{
                            //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                            //    {
                            //        // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            //        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            //        {
                            //            if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //            {
                            //                MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                            //                ResetCmd();
                            //            }
                            //            return FNC.CYCLE_CHECK;
                            //        }
                            //    }
                            //    NextSeq(14);
                            //    return FNC.BUSY;
                            //}
                        }
                        break;
                    case 16:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            m_bMgzZStop = true;
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                            }
                            if (m_bMgzZStop)
                            {
                                if (!MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                {
                                    m_bMgzZStop = false;
                                }
                                return FNC.CYCLE_CHECK;
                            }
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                // 매거진 Z축 동시 동작
                                nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY);
                            }
                            else
                            {
                                // 매거진 Z축 동시 동작
                                nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_QUAD_SUPPLY);
                            }
                           
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV XZ AXIS SUPPLY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 18:
                        {

                            int nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            }
                            else
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                            }

                            //221026 HEP 세이프티 센서에 따른 정지기능
                            if (!IsInPosMgzLdElvZ(nPosNo))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                                NextSeq(16);
                                return FNC.BUSY;
                            }
                        }
                        break;

                    case 20:
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_SUPPLY, false);

                            // 완료
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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }
        #endregion


        #region MAGAZINE CLIP/LOCK CHECK

        // 매거진 공급 전 클립/락이 해제 됐는지 체크하는 동작
        public int MgzTransferMzClipCheckPos()
        {
            try
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
                            // 매거진 클립 오픈 체크 사용안하면 넘어 감
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_MAGAZINE_DOOR_CLIP_OPEN_CHECK].bOptionUse)
                            {
                                // 완료
                                return FNC.SUCCESS;
                            }

                            if (m_bFirstSeqStep)
                            {
                                //SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                                m_bFirstSeqStep = false;
                            }
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
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_CLIP_LOCK_CHECK, true);

                        }
                        break;

                    case 6:
                        {
                            // 매거진이 클램프 상태인지 확인

                            // 실린더 및 인터락 확인

                            // 그리퍼 위치 및 레일 위치 확인

                            // Strip TransferZ 위치 및 동작 가능 상태 확인

                        }
                        break;

                    case 10:
                        {
                            //Dry Run 모드 
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            if (!IsLdElvCheckSensorOn())
                            {
                                // 매거진 체크 센서가 안들어 온 경우 에러 처리
                                return (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST;
                            }

                            //이미 LD RAIL에 STRIP 감지됨
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 || 
                                GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1 )
                            {
                                return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_ALREADY_ON;
                            }
                        }
                        break;

                    case 14:
                        {
                            // 여기서 스트립 트랜스퍼를 동작하면 안됨 ( 혹시 다른 동작 중이라면 문제 됨 )
                            // Picker X , Z 축 위치 인터락만 확인하도록 수정
                            if (SafetyMgr.Inst.IsLessThanPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_UNLOADING_TABLE_1))
                            {
                                // Picker X축 위치가 Table 1번 보다 작을 때 Z축이 Down 상태라면 에러
                                if(SafetyMgr.Inst.IsMoreThanPosNoZ(SVDF.AXES.STRIP_PK_Z, POSDF.STRIP_PICKER_READY))
                                {
                                    return (int)ERDF.E_INTL_LD_RAIL_Y_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS;
                                }
                            }

                            // 로드 레일 Y,T축도 클립 체크 하는 위치로 이동 필요

                            // STRIP TRANSFER Z READY 위치 이동
                            //nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);
                            //if (FNC.IsSuccess(nFuncResult))
                            //{
                            //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            //}
                        }
                        break;

                    case 18:
                        {
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                // 레일 내 부착 된 센서로 매거진 투입 시 클립이 오픈 됐는지 확인하는 위치
                                nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK);
                            }
                            else
                            {
                                // 레일 내 부착 된 센서로 매거진 투입 시 클립이 오픈 됐는지 확인하는 위치
                                nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE);
                            }
                      
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS MAGAZINE CLIP OPEN CHECK POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;

                    case 22:
                        {

                        }
                        break;
                    case 26:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                //{
                                //    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                //    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                //    {
                                //        if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                                //        {
                                //            m_bMgzYStop = true;
                                //            MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                                //            ResetCmd();
                                //        }
                                //        return FNC.CYCLE_CHECK;
                                //    }
                                //}
                            }
                            //if (m_bMgzYStop)
                            //{
                            //    if (!MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //    {
                            //        m_bMgzYStop = false;
                            //    }
                            //    return FNC.CYCLE_CHECK;
                            //}
                            // Y축이 먼저 움직여도 되는지 확인 필요
                            //nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_MZ_CLIP_CHECK);

                            // Y축 대기 위치로 이동
                            //nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_READY);

                            nFuncResult = FNC.SUCCESS;

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 28:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            //if (!IsInPosMgzLdElvY(POSDF.MGZ_LD_ELEV_READY))
                            //{
                            //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                            //    {
                            //        // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            //        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            //        {
                            //            if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //            {
                            //                MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                            //                ResetCmd();
                            //            }
                            //            return FNC.CYCLE_CHECK;
                            //        }
                            //    }
                            //    NextSeq(26);
                            //    return FNC.BUSY;
                            //}
                        }
                        break;

                    case 30:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            m_bMgzZStop = true;
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                            }
                            if (m_bMgzZStop)
                            {
                                if (!MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                {
                                    m_bMgzZStop = false;
                                }
                                return FNC.CYCLE_CHECK;
                            }
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                // Z축 매거진 클립 체크 위치로 이동
                                nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY);
                            }
                            else
                            {
                                // Z축 매거진 클립 체크 위치로 이동
                                nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_QUAD_SUPPLY);
                            }
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Z AXIS MAGAZINE CLIP OPEN CHECK COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;
                    case 32:
                        {
                            int nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            }
                            else
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                            }

                            //221026 HEP 세이프티 센서에 따른 정지기능
                            if (!IsInPosMgzLdElvZ(nPosNo))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                {
                                    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                    {
                                        if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                        {
                                            MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                            ResetCmd();
                                        }
                                        return FNC.CYCLE_CHECK;
                                    }
                                }
                                NextSeq(30);
                                return FNC.BUSY;
                            }
                        }
                        break;
                    case 34:
                        {
                            //221026 HEP 세이프티 센서에 따른 정지기능
                            {
                                //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                                //{
                                //    // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                //    if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                                //    {
                                //        if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                                //        {
                                //            m_bMgzYStop = true;
                                //            MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                                //            ResetCmd();
                                //        }
                                //        return FNC.CYCLE_CHECK;
                                //    }
                                //}
                            }
                            //if (m_bMgzYStop)
                            //{
                            //    if (!MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //    {
                            //        m_bMgzYStop = false;
                            //    }
                            //    return FNC.CYCLE_CHECK;
                            //}
                            //if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            //{
                            //    // Y축 매거진 클립 체크 위치로 이동
                            //    nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_MZ_CLIP_CHECK);
                            //}
                            //else
                            //{
                            //    // Y축 매거진 클립 체크 위치로 이동
                            //    nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_MZ_CLIP_CHECK_STRIP_MODE);
                            //}

                            nFuncResult = FNC.SUCCESS;

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS MAGAZINE CLIP OPEN CHECK POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                        break;

                    case 38:
                        {
                            int nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                            }
                            else
                            {
                                nPosNo = (int)POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                            }

                            //221026 HEP 세이프티 센서에 따른 정지기능
                            //if (!IsInPosMgzLdElvY(nPosNo))
                            //{
                            //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                            //    {
                            //        // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            //        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            //        {
                            //            if (MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].IsBusy())
                            //            {
                            //                MotionMgr.Inst[(int)SVDF.AXES.SPARE_63].MoveStop();
                            //                ResetCmd();
                            //            }
                            //            return FNC.CYCLE_CHECK;
                            //        }
                            //    }
                            //    NextSeq(34);
                            //    return FNC.BUSY;
                            //}
                        }
                        break;

                    case 42:
                        {
                            //Dry Run 모드 
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            // 위치에서 클립 센서 확인
                            if (!IsLdElvMzClipOpenCheck())
                            {
                                // 매거진 클립이 오픈 상태가 아니면 에러
                                return (int)ERDF.E_MGZ_LD_ELV_MGZ_DOOR_LOCK_OPEN_FAIL;
                            }
                        }
                        break;

                    case 46:
                        {

                        }
                        break;

                    case 50:
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_CLIP_LOCK_CHECK, false);
                            // 완료
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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }

        #endregion

        #region 컨베이어에 매거진 공급/배출 요청
        /// <summary>
        /// 컨베이어에 매거진 로딩 신호를 준다.
        /// </summary>
        /// <param name="nConvNo"></param>
        /// <param name="bWaitMode"></param>
        /// <returns></returns>
        public int MgzLdConvLoading_Req(bool bWaitMode = true)
        {
            try
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
                                //SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                                m_bFirstSeqStep = false;
                            }
                        }
                        break;

                    case 3:
                        {
                            if (m_bAutoMode)
                            {
                                if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                                {
                                    return FNC.CYCLE_CHECK;
                                }
                            }
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_REQ, true);
                        }
                        break;

                    case 4:
                        // 해당 컨베이어가 동작 중이라면 대기
                        // 추후 타임아웃 필요 할 수도..
                        if (GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN])
                        {
                            return FNC.CYCLE_CHECK;
                        }

                        break;

                    case 5:
                        //Loading Req 요청 전, Z축 미리 이동
                        nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1);

                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV MOVE LOADING POS", STEP_ELAPSED));
                        }
                        else
                            return nFuncResult;

                        break;

                    case 6:
                        nFuncResult = LdElvMgzClamp(false);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV UNCLMAP", STEP_ELAPSED));
                        }
                        else
                            return nFuncResult;
                        break;

                    case 8:
                        {
                            //Dry RUn 모드
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                // 매거진이 존재 하지만 픽업 위치가 아니라면 컨베이어 동작 요청
                                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ] = true;
                                break;
                            }
                            else
                            {
                                // 이미 매거진이 감지 된다면 완료 
                                if (IsLdConv_Arrival(4) && IsLdConv_Arrival(5))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ CONV LOADING MAGAZINE ARRIVAL SENSOR DETECT ", STEP_ELAPSED));
                                    NextSeq(10);
                                    return FNC.BUSY;
                                }
                                else
                                {
                                    GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ] = true;
                                }
                            }
                        }
                        break;

                    case 10:
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_REQ, false);

                            // 완료
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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }


        public int MzLoadingMove()
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    }
                    break;

                case 6:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        if (!IsInPosMgzLdElvZ(POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1))
                        {
                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1);

                            if(nFuncResult == FNC.SUCCESS)
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD MZ ELV Z MOVE 1F POS", STEP_ELAPSED));
                                break;
                            }
                        }
                    }
                    break;

                case 7:
                    nFuncResult = LdElvMgzClamp(false);

                    if(nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                        break;
                    }
                    return FNC.BUSY;

                case 8:
                    nFuncResult = LdStopper_Up(false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                        break;
                    }
                    return FNC.BUSY;

                case 9:
                    nFuncResult = LdElvDoorOpen(true);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                        break;
                    }
                    return FNC.BUSY;

                case 10:
                    {
                        // 컨베이어 동작
                        nFuncResult = LdConv_Run(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 11:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) return FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(4))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    LdConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                return FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 12:
                    {
                        if (WaitDelay(1000)) return FNC.BUSY;
                        // 컨베이어 동작
                        nFuncResult = LdConv_Stop();
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAGAZINE ELV CONVEYOR RUN START", STEP_ELAPSED));
                    }
                    break;

                case 13:
                    // 컨베이어 동작
                    nFuncResult = LdElvConv_Run(true);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAGAZINE ELV CONVEYOR RUN START", STEP_ELAPSED));
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) return FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(4) || !IsLdConv_Arrival(5))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    nFuncResult = LdElvConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                return FNC.BUSY;
                            }
                        }

                    }
                    break;

                case 16:
                    {
                        // 컨베이어 정지
                        nFuncResult = LdElvConv_Stop();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));

                            // 로더는 센서 감지 후 추가 동작 시간을 따로 사용 함
                            // 센서 감지 후 추가 컨베이어 동작 시간 
                            //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                            if (WaitDelay(1000)) return FNC.BUSY;
                        }
                    }
                    break;

                case 17:
                    {
                        nFuncResult = LdElvMgzClamp(true);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                           
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    //Door Add
                    nFuncResult = LdElvDoorOpen(false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                    }
                    break;

                case 19:
                    //Door Add

                    if (IsLdElvDoor_Check())
                        nFuncResult = FNC.SUCCESS;
                    else
                        nFuncResult = (int)ERDF.E_MGZ_LD_ELV_DOOR_NOT_OPEN;

                    break;

                case 20:
                    {
                        GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN] = false;
                        GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ] = false;

                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

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

        public int MgzUldConvUnLoading_Req(bool bWaitMode = true)
        {
            try
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
                                //SeqHistory("Move to Ready Pos", "Strip Picker Z Axis", "Start");
                                m_bFirstSeqStep = false;
                            }
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
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_REQ, true);
                        }
                        break;

                    case 6:
                        // 해당 컨베이어가 동작 중이라면 대기
                        // 추후 타임아웃 필요 할 수도..
                        if (GbVar.Seq.sLoaderUldConv.bSeqIfVar[seqLoader_UldConv.MGZ_CONV_RUN])
                        {
                            return FNC.CYCLE_CHECK;
                        }

                        break;

                    case 7:
                        //Unloading Req 요청 전, Z축 미리 이동
                        nFuncResult = MovePosMgzUldElvZ(POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV MOVE LOADING POS", STEP_ELAPSED));
                        }
                        break;

                    case 8:
                        nFuncResult = LdElvMgzClamp(false);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV UNCLAMP", STEP_ELAPSED));
                        }
                        break;

                    case 9:
                        {
                            //Dry RUn 모드
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                // 매거진이 존재 하지만 픽업 위치가 아니라면 컨베이어 동작 요청
                                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_UNLOADING_REQ] = true;
                                break;
                            }
                            else
                            {
                                // 이미 매거진이 감지 된다면 완료 
                                if (!IsLdConv_Arrival(4) && !IsLdConv_Arrival(5))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ CONV LOADING MAGAZINE ARRIVAL SENSOR DETECT ", STEP_ELAPSED));
                                    NextSeq(10);
                                    nFuncResult = FNC.BUSY;
                                }
                                else
                                {
                                    GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_UNLOADING_REQ] = true;
                                }
                            }
                        }
                        break;

                    case 10:
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_REQ, false);

                            // 완료
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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }

        /// <summary>
        /// 매거진 컨베이어 배출 방향으로 구동 후 매거진 체크
        /// </summary>
        /// <param name="bCheckSensor"></param>
        /// <returns></returns>
        public int MzUnloadingMove(bool bSensorSkip = false)
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_ULD_CONV_UNLOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?

                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 언로딩은 투입 역순이기 떄문에
                        // 먼저 ARRIVAL 센서를 체크 해야 함
                        //if (!IsLdConv_Arrival(m_ConvNo))
                        //{
                        //    // 매거진이 감지 되지 않으면 에러 처리

                        //    // 컨베이어 정지
                        //    nFuncResult = LdConv_Stop();
                        //    return (int)ERDF.E_MGZ_LD_CONV_1_ULD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}          
                    }
                    break;

                case 6:
                    if(!IsInPosMgzLdElvZ(POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1))
                    {
                        //Unloading Z축 이동
                        nFuncResult = MovePosMgzUldElvZ(POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV MOVE LOADING POS", STEP_ELAPSED));
                        }
                    }
                    break;

                case 7:
                    {
                        nFuncResult = LdElvDoorOpen(true);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "ELEVATOR CONVEYOR UNLOADING RUN START", STEP_ELAPSED));
                        }
                    }
                    break;

                case 8:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                        {
                            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            {
                                // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                return FNC.CYCLE_CHECK;
                            }

                        }
                    }
                    break;

                case 9:
                    nFuncResult = UldStopper_Up(true);
                    break;

                case 10:
                    nFuncResult = LdElvMgzClamp(false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ ELV UNCLAMP", STEP_ELAPSED));
                    }
                    break;

                case 11:
                    nFuncResult = LdElvConv_Run(false);

                    if (WaitDelay(1000))
                        nFuncResult = FNC.BUSY;

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "ELEVATOR CONVEYOR UNLOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 12:

                    // 컨베이어 동작
                    nFuncResult = UldConv_Run(false);
                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNLOAD CONVEYOR UNLOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) return FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsUldConv_Arrival(0))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    LdElvConv_Stop();
                                    UldConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNLOAD CONVEYOR UNLOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                nFuncResult = FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 16:
                    {

                        LdElvConv_Stop();
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(1000)) 
                            nFuncResult = FNC.BUSY;

                        // 컨베이어 정지
                        nFuncResult = UldConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {

                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNLOAD CONVEYOR UNLOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_UNLOADING_MOVE, false);

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

        #region 트랜스퍼 매거진 공급 / 배출 조건 확인

        public int CheckStatusLoading()
        {
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
            {
                // Dry Run 모드 일 경우 처리
                // 다음 동작으로 
                return FNC.SUCCESS;
            }

            // AUTO RUN 모드
            if (IsLdConv_Detect()
                && !GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN])
            {
                // 현재 포트에 매거진이 있는 경우
                return FNC.SUCCESS;
            }
            else
            {
                // 매거진이 없다면 다음 포트로 넘기기 위함
                return FNC.BUSY;
            }

        }

        public int CheckStatusUnloading()
        {
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
            {
                // Dry Run 모드 일 경우 처리
                // 다음 동작으로 
                return FNC.SUCCESS;
            }

            // 신규 코드 
            // 테스트 후 사용하면 됨
            //if (IsUldConv_Insert() && IsUldConv_Arrival())
            if (IsUldConv_Arrival(0)
                && !GbVar.Seq.sLoaderUldConv.bSeqIfVar[seqLoader_UldConv.MGZ_CONV_RUN])
            {
                // 투입 , 도착 위치에 매거진이 모두 존재한다면 진행 할 수 없음

                // 현재 포트에 매거진이 있는 경우 진행 불가
                return FNC.BUSY;
            }
            else
            {
                // 매거진이 없다면 진행 가능
                return FNC.SUCCESS;
            }
        }
       
        /// <summary>
        /// LEFT / RIGHT 전체 언로드 컨베이어에 매거진이 FULL인지 확인
        /// </summary>
        /// <returns></returns>
        public bool CheckFullMgzUnloading()
        {
            if (IsUldConv_Arrival(0)
                && IsUldConv_Arrival(1)
                && IsUldConv_Arrival(2))
            {
                // 투입 , 도착 위치에 매거진이 모두 존재한다면 진행 할 수 없음

                // LEFT / RIGHT 모든 포트에 매거진이 있는 경우 진행 불가
                return true;

            }

            return false;
        }

        /// <summary>
        /// LEFT / RIGHT 전체 로드 컨베이어 매거진이 없는지 확인 
        /// </summary>
        /// <returns></returns>
        public bool CheckEmptyMgzLoading()
        {
            if (!IsLdConv_Detect() && !IsLdConv_Detect())
            {
                // LEFT / RIGHT 둘다 매거진이 없는 경우 
                return true;
            }

            return false;
        }

        #endregion


        #region 개별 함수 ( 내부 사용 )

        /// <summary>
        /// 싸이클 동작 중 에러 발생 시 재시작 하게 되면
        /// 현 스탭에서 상태 및 조건 확인 후 다음 스탭으로 이어서 진행 하기위함
        /// </summary>
        /// <param name="nConvNo"></param>
        /// <returns></returns>
        private bool CheckCurrentAndNextSeq_Loading()
        {
            bool bNextStep = false;

            if (IsLdElvCheckSensorOn() && IsLdElvCheckClampOn())
            {
                // 매거진 감지 ON
                // 매거진 클램프 ON
                bNextStep = true;
            }

            return bNextStep;
        }

        private bool CheckCurrentAndNextSeq_Unloading()
        {
            bool bNextStep = false;

            if (IsLdElvCheckSensorOn())
            {
                // 매거진 언클램프 상태
                // X, Y축 매거진 언로딩 위치
                bNextStep = true;
            }

            return bNextStep;
        }


        #endregion

        #endregion
    }
}
