using DionesTool.UTIL;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycStripTransfer : SeqBase
    {
        //PreProp PreAlignResult = new PreProp();

        bool m_bAutoMode = true;
        bool m_bVacCheck = true;
        bool m_bIsStrip = true;

        int m_nLoadingMgzSlotNo = 0;
        int m_nUnloadingTableNo = 0;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        double m_dStripPreAngle;
        string m_strStripID;
        string[] strVisionResult = null;

        int m_nBottomBarcodeRetryCount = 0;
        int m_nMapPusherSearchCount = 0;
        bool m_bIsOverload = false;

        double m_dReloadCorrOffsetXRead = 0.0f;
        double m_dReloadCorrOffsetXResult = 0.0f;
        bool m_bIsReloadAgain = false;

        // Align 번호. Align 1, Align 2
        int m_nAlignNo = 0;
        // Angle 보정 전 얼라인인지, 보정 후 얼라인인지 (true면 보정 후)
        bool m_bAfterRotateCheck = false;

        Stopwatch swBarcodeTimeoutCheck = new Stopwatch();
        int m_nRetry = 0;

        bool m_bMgzZStop = false;
        #region PROPERTY
        public seqStripTransfer SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sStripTransfer; }
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

        public int MGZ_LOAD_SLOT_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nMgzLoadSlotNum;
                }

                return m_nLoadingMgzSlotNo;
            }
        }

        public double RELOAD_SAW_OFFSET
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.dReloadCorrOffsetX;
                }

                return m_dReloadCorrOffsetXRead;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.dReloadCorrOffsetX = value;
                }
                else
                {
                    m_dReloadCorrOffsetXRead = value;
                }
            }
        }

        #endregion

        public cycStripTransfer(int nSeqID, int nMoveTableNo)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_nUnloadingTableNo = nMoveTableNo;
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
            m_nLoadingMgzSlotNo = (int)args[0];
            m_nUnloadingTableNo = (int)args[1];
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
                        GbSeq.autoRun[SEQ_ID.STRIP_TRANSFER].InitSeq();
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


        #region 기존 코드

   
        #region Strip Push
        public int StripPushAndBarcode()
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
                        m_nBottomBarcodeRetryCount = 0;
                        m_nMapPusherSearchCount = 0;

                        GbVar.Seq.sStripTransfer.bIsNotExistStrip = false;
                        m_bIsOverload = false;
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
                        // 이미 LD RAIL에 STRIP 감지됨
                        //TODO 20211122 센서 감도 조절 후 주석 풀기 20211122 pjh
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 ||
                            GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1)
                        {
                            return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_ALREADY_ON;
                        }
                    }
                    break;

                case 10:
                    // STRIP GRIPPER UP
                    {
                        nFuncResult = LdVisionGripUpDown(true);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP GRIPPER UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    // LD VISION GRIPPER UNGRIP (GRIPPER OPEN CHECK)
                    {
                        nFuncResult = LdVisionGrip(false, 200);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 18:
                    // PUSHER CYLINDER BWD 이동
                    {
                        nFuncResult = MapPusherFwdBwd(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER BWD COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // INLET TABLE DOWN
                    {
                        ////2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        //if (m_bFirstSeqStep)
                        //{
                        //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        //}
                        nFuncResult = InletTableUpDown(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
                    {
                        
                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // MGZ LD ELV Z축 스트립 포지션 이동
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

                        double dOffset = 0.0;

                        dOffset = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch * MGZ_LOAD_SLOT_NO;
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY, -dOffset);
                        }
                        else
                        {
                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_QUAD_SUPPLY, -dOffset);
                        }
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Z AXIS STRIP SUPPLY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 31:
                    {
                        double dOffset = 0.0;

                        dOffset = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch * MGZ_LOAD_SLOT_NO;
                        int nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                        {
                            nPosNo = POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                        }
                        else
                        {
                            nPosNo = POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                        }
                        //221026 HEP 세이프티 센서에 따른 정지기능
                        if (!IsInPosMgzLdElvZ(nPosNo, -dOffset))
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
                case 32:
                    // PUSHER CYLINDER FWD 이동
                    {
                        // 오버로드 감지 확인
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK].bOptionUse)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_OVERLOAD_SENSOR] != 1)
                            {
                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL, false);
                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL, true);
                                m_bIsOverload = true;
                                NextSeq(60);
                                return FNC.BUSY;
                            }
                        }
                        
                        nFuncResult = MapPusherFwdBwd(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER FWD COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // PUSHER SERVO FWD 이동
                    {
                        // 오버로드 감지 확인
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK].bOptionUse)
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_OVERLOAD_SENSOR] != 1)
                            {
                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL, false);
                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL, true);
                                m_bIsOverload = true;
                                NextSeq(60);
                                return FNC.BUSY;
                            }
                        }
                        // 푸셔 모터 동작 삭제 됨  
                    }
                    break;

                case 36:
                    // 스트립이 존재하는가?
                    {
                        if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (m_bAutoMode)
                            {
                                GbVar.Seq.sStripTransfer.bIsNotExistStrip = false;
                            }
                        }
                        else
                        {
                            // 스트립 감지 성공
                            //TODO 대응하는 SENSOR찾기
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 0)
                            {
                                if (m_bAutoMode)
                                {
                                    GbVar.Seq.sStripTransfer.bIsNotExistStrip = true;
                                }

                                NextSeq(60);
                                return FNC.BUSY;
                            }
                        }
                        
                    }
                    break;

                    // GRIPPER 모터가 자재 집으러 이동 해야 함

                #region barcode
                case 40:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE].bOptionUse)
                    {
                        NextSeq(50);
                        return FNC.BUSY;
                    }


                    long lDelay = 50;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY].bOptionUse)
                        lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY].lValue;
                    if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                        return FNC.BUSY;
                    break;

                case 41:
                    // STRIP TRANSFER X GRIP 위치 이동
                    {
                        nFuncResult = MovePosStripPkXBarcode(POSDF.STRIP_PICKER_STRIP_2D_CODE, POSDF.BOT_BARCDOE_SCAN);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS BARCODE READ POS COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 42:
                    // BOTOOM BARCODE READ 트리거 ON
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                        if (!GbDev.BarcodeReader.IsConnect())
                        {
                            return (int)ERDF.E_COGNEX_BARCODE_READER_NOT_CONNECTED;
                        }
                        GbDev.BarcodeReader.TriggerOn();
                    }
                    break;

                case 43:
                    // BOTOOM BARCODE READ 결과 확인
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        string sReadResult = "";

                        // TIMEOUT CHECK
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT].bOptionUse &&
                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_STRIP_LOADING_BOTTOM_BARCODE_TIMEOUT;
                        }

                        if (GbDev.BarcodeReader.FLAG_RECEIVED == true)
                        {
                            GbDev.BarcodeReader.FLAG_RECEIVED = false;
                            if (GbDev.BarcodeReader.FLAG_ERROR == true)
                            {
                                GbDev.BarcodeReader.FLAG_ERROR = false;

                                // RETRY CHECK
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT].bOptionUse &&
                                    m_nBottomBarcodeRetryCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT].nValue)
                                {
                                    m_nBottomBarcodeRetryCount++;
                                    NextSeq(50);
                                    return FNC.BUSY;
                                }

                                return (int)ERDF.E_STRIP_LOADING_BOTTOM_BARCODE_READ_FAIL;
                            }

                            sReadResult = GbDev.BarcodeReader.RESULT;
                            GbVar.Seq.sStripRail.Info.STRIP_ID = sReadResult;
                        }
                        else
                        {
                            return FNC.BUSY;
                        }
                    }
                    break;
                #endregion


                case 62:
                    // PUSHER CYLINDER BWD 이동
                    {
                        nFuncResult = MapPusherFwdBwd(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER BWD COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 64:
                    if (m_bIsOverload)
	                {
                        return (int)ERDF.E_INTL_MAP_PUSHER_OVERLOAD_SENSOR_DETECTED;
	                }
                    break;

                case 70:
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

        
        #region Strip Loading
        public int StripRailLoading()
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
                        m_nBottomBarcodeRetryCount = 0;
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
                    if (GbVar.IO[IODF.INPUT.LD_X_GRIP] == 1 &&
                        GbVar.IO[IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
                    {
                        NextSeq(50);//언그립하는 위치로 이동
                        return FNC.BUSY;
                    }
                    break;

                case 8:
                    {
                        if (!GbVar.Seq.sStripRail.Info.IsStrip())
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 0 &&
                                    GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 0 &&
                                    GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 0)
                            {
                                return FNC.SUCCESS;
                            }
                        }
                        else
                        {
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1)
                            {
                                NextSeq(62);
                                return FNC.BUSY;
                            }
                            else
                            {
                                if (GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
                                {
                                    NextSeq(34);
                                    return FNC.BUSY;
                                }
                            }
                        }

                        // 이미 LD RAIL에 STRIP 감지됨
                        //TODO 20211122 센서 감도 조절 후 주석 풀기 20211122 pjh
                        //if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 || GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1)
                        //{
                        //    return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_ALREADY_ON;
                        //}
                        
                    }
                    break;

                case 10:
                    // STRIP GRIPPER UP
                    {
                        nFuncResult = LdVisionGripUpDown(true);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP GRIPPER UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    // LD VISION GRIPPER UNGRIP (GRIPPER OPEN CHECK)
                    {
                        nFuncResult = LdVisionGrip(false, 200);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // INLET TABLE DOWN
                    {
                        ////2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        //if (m_bFirstSeqStep)
                        //{
                        //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        //}
                        nFuncResult = InletTableUpDown(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
                    {
                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // STRIP TRANSFER X GRIP SAFETY 위치 이동
                    {
                        nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY, POSDF.LD_VISION_X_STRIP_GRIP);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS GRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // STRIP TRANSFER Z GRIP SAFETY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS GRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // LD VISION GRIPPER DOWN
                    {
                        nFuncResult = LdVisionGripUpDown(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // STRIP TRANSFER X GRIP 위치 이동
                    {
                        //nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY, POSDF.LD_VISION_Y_STRIP_GRIP);

                        //if (FNC.IsSuccess(nFuncResult))
                        //{
                        //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS GRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        //}
                    }
                    break;

                case 30:
                    // STRIP TRANSFER X GRIP 위치 이동
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_GRIP);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS GRIP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 32:
                    // STRIP TRANSFER Z GRIP 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS GRIP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // LD VISION GRIPPER GRIP
                    {
                        nFuncResult = LdVisionGrip(true, 200);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            if (GbVar.IO[IODF.INPUT.GRIP_MATERIAL_CHECK] != 1)
                            {
                                nFuncResult = (int)ERDF.E_LD_VISION_GRIPPER_MATERIAL_IS_NOT_DETECTION;
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER MATERIAL IS NOT CHECKED", STEP_ELAPSED));
                                break;
                            }

                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER CLOSE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 50:
                    // STRIP TRANSFER X UNGRIP SAFETY 위치 이동
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS UNGRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 52:
                    // CHECK STRIP CHECK SENSOR
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 레일 Y의 자재 감지 센서가 하나라도 OFF이면 에러
                        if (/*GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] != 1 ||*/
                            GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] != 1)
                        {
                            //return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON;
                        }
                    }
                    break;

                case 54:
                    // LD VISION GRIPPER UNGRIP
                    {
                        nFuncResult = LdVisionGrip(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 55:
                    // STRIP TRANSFER Z GRIP SAFETY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS GRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 56:
                    // STRIP TRANSFER X UNGRIP 위치 이동
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_UNGRIP);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS GRIP UP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;



                case 60:
                    // STRIP TRANSFER X UNGRIP SAFETY 위치 이동
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS UNGRIP SAFETY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 62:
                    // LD VISION GRIPPER UP
                    {
                        nFuncResult = LdVisionGripUpDown(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 64:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY))
                                break;
                        }

                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 66:
                    // LOAD RAIL ALIGN POS MOVE  (T,  Guide)
                    {
                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_ALIGN);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS ALIGN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 68:
                    // INLET TABLE UP
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }

                        nFuncResult = InletTableUpDown(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 70:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "STRIP LOADING CYCLE FINISH", STEP_ELAPSED, strStripInfo));

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

//        #region 신규 코드

//        /// <summary>
//        /// 맵 푸셔가 스트립을 밀고 레일 내 그리퍼가 그립하는 동작
//        /// 에러 후 재시작 시 레일 내 있는 자재를 다시 매거진에 넣고 시작하면 됨
//        /// </summary>
//        /// <returns></returns>
//        public int StripPushAndGrip()
//        {
//            if (m_nSeqNo != m_nPreSeqNo)
//            {
//                ResetCmd();

//                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
//            }

//            m_nPreSeqNo = m_nSeqNo;
//            nFuncResult = FNC.SUCCESS;

//            // 오버로드 센서 확인
//            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK].bOptionUse)
//            {
//                //if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] != 1)
//                if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] == 1)
//                {
//                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_FWD, false);
//                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_BWD, true);
//                }
//            }

//            switch (m_nSeqNo)
//            {
//                case 0:
//                    {
//                        m_nBottomBarcodeRetryCount = 0;
//                        m_nMapPusherSearchCount = 0;

//                        GbVar.Seq.sStripTransfer.bIsNotExistStrip = false;
//                        m_bIsOverload = false;
//                    }
//                    break;

//                case 4:
//                    {
//                        if (m_bAutoMode)
//                        {
//                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
//                            {
//                                return FNC.CYCLE_CHECK;
//                            }
//                        }
//                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PUSH_AND_GRIP, true);
//                    }
//                    break;

//                case 5:
//                    {
//                        // ::: 여기서 추가로 매거진 상단에 자재 감지 센서도 확인해야 함 :::


//                        //이미 LD RAIL에 STRIP 감지됨
//                        if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 || GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1)
//                        {
//                            return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_ALREADY_ON;
//                        }
//                    }
//                    break;

//                case 12:
//                    // LD VISION GRIPPER UNGRIP (GRIPPER OPEN CHECK)
//                    {
//                        nFuncResult = LdVisionGrip(false);
//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 14:
//                    // STRIP TRANSFER Z READY 위치 이동
//                    {
//                         nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 18:
//                    // PUSHER CYLINDER BWD 이동
//                    {
//                        nFuncResult = MapPusherFwdBwd(false);

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER BWD COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 20:
//                    // INLET TABLE DOWN
//                    {
//                        nFuncResult = InletTableUpDown(false);

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 22:
//                    {
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

//                        // 매거진 클램프 이 후 센서 체크
//                        if (!IsLdElvCheckSensorOn())
//                        {
//                            return (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST;
//                        }
//                    }               
//                    break;

//                case 24:
//                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
//                    {
//                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 26:
//                    {
//                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
//                        {
//                            // Y축 매거진 공급 위치로 이동
//                            nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY);
//                        }
//                        else
//                        {
//                            // Y축 매거진 공급 위치로 이동
//                            nFuncResult = MovePosMgzLdElvY(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE);
//                        }
                       
//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Y AXIS SUPPLY POSITION COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 28:
//                    {
//                    }
//                    break;

//                case 30:
//                    // MGZ LD ELV Z축 스트립 포지션 이동
//                    {
//#if !_NOTEBOOK
//                        // 매거진과 레일 내 상단에서 자재를 체크하는 센서가 있음
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE].bOptionUse)
//                        {
//                            // 상단 자재 체크 센서 사용 모드 & 드라이런이 아닌경우에만 센서 체크
//                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
//                            {
//                                //if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_VISON_X_GRIP_MATERIAL_CHECK] != 1)
//                                //    return (int)ERDF.E_LD_MGZ_TO_RAIL_X_MATERIAL_ALREADY_DETECTED;
//                                // A접으로 변경

//                                if (GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
//                                    return (int)ERDF.E_LD_MGZ_TO_RAIL_X_MATERIAL_ALREADY_DETECTED;
//                            }
//                        }

//#endif

//                        if(MGZ_LOAD_SLOT_NO >= RecipeMgr.Inst.Rcp.MgzInfo.nSlotCount)
//                        {
//                            return (int)ERDF.E_MGZ_LD_ELV_MGZ_SLOT_COUNT_OVER;
//                        }

//                        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
//                        {
//                            double dOffset = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch * MGZ_LOAD_SLOT_NO;
//                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY, dOffset, 300);
//                        }
//                        else
//                        {
//                            double dOffset = RecipeMgr.Inst.Rcp.MgzInfo.dSlotPitch * MGZ_LOAD_SLOT_NO;
//                            nFuncResult = MovePosMgzLdElvZ(POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE, dOffset, 300);
//                        }

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MGZ LD ELV Z AXIS STRIP SUPPLY POSITION COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 32:
//                    // PUSHER CYLINDER FWD 이동
//                    {
//                        // 오버로드 감지 확인
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK].bOptionUse)
//                        {
//                            //if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] != 1)
//                            if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] == 1)
//                            {
//                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_FWD, false);
//                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_BWD, true);
//                                m_bIsOverload = true;
//                                NextSeq(60);
//                                return FNC.BUSY;
//                            }
//                        }

//                        nFuncResult = MapPusherFwdBwd(true);

//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER FWD COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 34:
//                    // PUSHER SERVO FWD 이동
//                    {
//                        // 오버로드 감지 확인
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PUSHER_OVERLOAD_CHECK].bOptionUse)
//                        {
//                            //if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] != 1)
//                            if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PUSHER_AXIS_OVERLOAD] == 1)
//                            {
//                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_FWD, false);
//                                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PUSHER_PUSHER_BWD, true);
//                                m_bIsOverload = true;
//                                NextSeq(60);
//                                return FNC.BUSY;
//                            }
//                        }
//                        // 푸셔 모터 동작 삭제 됨  
//                    }
//                    break;

//                case 40:
//                    // 스트립이 존재하는가?
//                    {
//                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
//                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
//                        {
//                            break;
//                        }
//                        else
//                        {
//                            if (WaitDelay(300)) return FNC.BUSY;
//                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 0)
//                            {
//                                if (m_bAutoMode)
//                                {
//                                    GbVar.Seq.sStripTransfer.bIsNotExistStrip = true;
//                                }

//                                NextSeq(60);
//                                return FNC.BUSY;
//                            }
//                        }
//                    }
//                    break;

//                case 54:
//                    {
//                        // 실 사용시 해제 하고 사용하면 됨
//                        // 그리퍼 그립 실린더 동작
//                        nFuncResult = LdVisionGrip(true, 300);
//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER CLOSE COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 58:
//                    {
//                        // 그리퍼 자재 언그립 위치로 이동
//                        //nFuncResult = MovePosLdGripperX(POSDF.LD_RAIL_GRIPPER_UNGRIP);

//                        // 변경 요청
//                        // GRIP X , RAIL Y 축 동시 동작하도록 수정 함
//                        nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_ALIGN);
//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y ALIGN POS AND GRIPPER X AXIS UN-GRIP POSITION COMPLETE", STEP_ELAPSED));
//                        }
                        
//                    }
//                    break;
                        
//                case 62:
//                    // PUSHER CYLINDER BWD 이동
//                    {
//                        nFuncResult = MapPusherFwdBwd(false);
//                        if (FNC.IsSuccess(nFuncResult))
//                        {
//                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PUSHER BWD COMPLETE", STEP_ELAPSED));
//                        }
//                    }
//                    break;

//                case 64:
//                    if (m_bIsOverload)
//                    {
//                        return (int)ERDF.E_INTL_MAP_PUSHER_OVERLOAD_SENSOR_DETECTED;
//                    }
//                    break;

//                case 70:
//                    {
//                        string strStripInfo = string.Format("{0}",
//                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
//                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
//                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PUSH_AND_GRIP, false);

//                        return FNC.SUCCESS;
//                    }

//                default:
//                    break;
//            }

//            #region AFTER SWITCH
//            if (m_bFirstSeqStep)
//            {
//                // Position Log
//                if (string.IsNullOrEmpty(m_strMotPos) == false)
//                {
//                    SeqHistory(m_strMotPos);
//                }

//                m_bFirstSeqStep = false;
//            }

//            if (FNC.IsErr(nFuncResult))
//            {
//                return nFuncResult;
//            }
//            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

//            m_nSeqNo++;

//            if (m_nSeqNo > 10000)
//            {
//                System.Diagnostics.Debugger.Break();
//                FINISH = true;
//                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
//            }

//            return FNC.BUSY;
//            #endregion
//        }
//        #endregion


        /// <summary>
        /// 그립 완료 후 하부 바코드 시작
        /// </summary>
        /// <returns></returns>
        public int StripBottomBarcode()
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
                        m_nBottomBarcodeRetryCount = 0;
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_BOTTOM_BARCODE, true);
                    }
                    break;

                case 6:
                    {
                        // 바코드 스킵이거나 DryRun이면 진행 하지 않음
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE].bOptionUse
                            || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            return FNC.SUCCESS;
                        }
                    }
                    break;

                case 7:
                    {
                        if(m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            // 자재 데이터가 없다면 동작하지 않고 완료 처리
                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 8:
                    {
                        // 하부 바코드는 언그립 위치에서 그립 상태부터 시작 함

                        // DryRun 모드
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 바코드 위치에 따라서 자재 위치가 달라진다면 주석 처리하면 됨
                        // 레일 내 자재 유무 확인
                        //if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] != 1)
                        //{
                        //    return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON;
                        //}

                        // 그리퍼 X축 자재 유무 확인 ( A접점 )
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] != 1)
                        {
                            return (int)ERDF.E_LD_GRIPPER_X_MATERIAL_DETECT_FAIL;
                        }
                    }
                    break;

                case 12:
                    {
                        // 한번 더 자재 센서 체크 확인 후 그립 해야 할듯

                        nFuncResult = LdVisionGrip(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // INLET TABLE DOWN
                    {
                        ////2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        //if (m_bFirstSeqStep)
                        //{
                        //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        //}
                        nFuncResult = InletTableUpDown(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
                    {
                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                #region 하부 바코드 스캔
                case 30:
                    {
                        // 그립이 완료 됐으면 바코드 스캔

                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_USE].bOptionUse)
                        {
                            NextSeq(60);
                            return FNC.BUSY;
                        }

                        // 하부 바코드 스캔 그리퍼축 위치로 이동      
                        nFuncResult = MovePosBarcodeY(POSDF.BOT_BARCDOE_SCAN);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER CLOSE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 50:
                    // BOTTOM BARCODE READ DELAY
                    {
                        long lDelay = 50;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY].bOptionUse)
                            lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 52:
                    // BOTOOM BARCODE READ 트리거 ON
                    {
                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (!GbDev.BarcodeReader.IsConnect())
                        {
                            return (int)ERDF.E_COGNEX_BARCODE_READER_NOT_CONNECTED;
                        }
                        GbDev.BarcodeReader.TriggerOn();
                        swBarcodeTimeoutCheck.Restart();//220513 타임아웃스톱워치 추가
                    }
                    break;

                case 54:
                    // BOTOOM BARCODE READ 결과 확인
                    {
                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        string sReadResult = "";

                        // TIMEOUT CHECK
                        if (swBarcodeTimeoutCheck .ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_READ_TIMEOUT].lValue)
                        {
                            swBarcodeTimeoutCheck.Stop();
                            return (int)ERDF.E_STRIP_LOADING_BOTTOM_BARCODE_TIMEOUT;
                        }

                        if (GbDev.BarcodeReader.FLAG_RECEIVED == true)
                        {
                            swBarcodeTimeoutCheck.Stop();
                            GbDev.BarcodeReader.FLAG_RECEIVED = false;

                            if (GbDev.BarcodeReader.FLAG_ERROR == true)
                            {
                                GbDev.BarcodeReader.FLAG_ERROR = false;

                                // RETRY CHECK
                                if (m_nBottomBarcodeRetryCount < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_BOTTOM_BARCODE_RETRY_COUNT].nValue)
                                {
                                    m_nBottomBarcodeRetryCount++;
                                    NextSeq(50);
                                    return FNC.BUSY;
                                }
                                return (int)ERDF.E_STRIP_LOADING_BOTTOM_BARCODE_READ_FAIL;
                            }
                            sReadResult = GbDev.BarcodeReader.RESULT;
                            GbVar.Seq.sStripRail.Info.STRIP_ID = sReadResult;
                        }
                        else
                        {
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 55:
                    //220604 pjh
                    //스트립 ID와 ITS ID의 앞 N자리는 일치함
                    //만약 불일치 할경우 알람
                    if (!m_bAutoMode) break;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    if (GbVar.lstBinding_HostLot.Count == 0)
                    {
                        //예외 상황
                        nFuncResult = (int)ERDF.E_THERE_IS_NO_ITS_INFORMATION;
                        break;
                    }

                    //if (GbVar.Seq.sStripRail.Info.STRIP_ID.Length < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue ||
                    //   GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID.Length < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue)
                    //{
                    //    //예외 상황
                    //    nFuncResult = (int)ERDF.E_ITS_ID_STRIP_ID_UNMATCHED;
                    //    break;
                    //}


                    //if (GbVar.Seq.sStripRail.Info.STRIP_ID.Substring(0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue) !=
                    //    GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID.Substring(0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue))
                    //    nFuncResult = (int)ERDF.E_ITS_ID_STRIP_ID_UNMATCHED;

                    break;

                #endregion
                case 58:
                    //220513 오토모드 아닐경우
                    if (!m_bAutoMode) break;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    break;
                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "STRIP LOADING CYCLE FINISH", STEP_ELAPSED, strStripInfo));

                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_BOTTOM_BARCODE, false);

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


        /// <summary>
        /// 그립 및 하부 바코드 이후 언그립 위치로 이동 후 언그립
        /// </summary>
        /// <returns></returns>
        public int StripRailUngrip()
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
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 6:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            // 자재 데이터가 없다면 동작하지 않고 완료 처리
                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_RAIL_UNGRIP, true);

                    }
                    break;

                case 8:
                    {
                        // 언그립 동작 진행

                        // DryRun 모드
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 그리퍼 X축 자재 유무 확인 ( A접점 )
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
                        {
                            return (int)ERDF.E_LD_GRIPPER_X_MATERIAL_DETECT_FAIL;
                        }
                    }
                    break;

                case 12:
                    {
                        // 처음 시작 시 그립 상태기 때문에 그립으로 시작
                        // 에러 후 재시작시에도 현재 위치에서 그립하면 됨

                        nFuncResult = LdVisionGrip(true);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER CLOSE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // INLET TABLE DOWN
                    {
                        ////2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        //if (m_bFirstSeqStep)
                        //{
                        //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        //}
                        nFuncResult = InletTableUpDown(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
                    {
                        //nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);

                        // T만 동작, 레일 Y축은 얼라인 상태를 유지 함
                        nFuncResult = MovePosLdRailT(POSDF.LD_RAIL_T_STRIP_LOADING, 0.0);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL T AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 20:
                    {
                        // 언로딩 위치로
                        //nFuncResult = MovePosLdGripperX(POSDF.LD_RAIL_GRIPPER_UNGRIP);
                        //if (FNC.IsSuccess(nFuncResult))
                        //{
                        //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "GRIPPER X AXIS UNGRIP POSITION COMPLETE", STEP_ELAPSED));
                        //}

                        // 변경 요청
                        // RAIL Y 축 동시 동작하도록 수정 함
                        nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_ALIGN);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y ALIGN POS AND GRIPPER X AXIS UN-GRIP POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // LD VISION GRIPPER UNGRIP
                    {
                        nFuncResult = LdVisionGrip(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    {
                        string strStripInfo = string.Format("{0}",
                         "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "STRIP LOADING CYCLE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_RAIL_UNGRIP, false);

                        return FNC.SUCCESS;
                    }
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


        /// <summary>
        /// 언그립 이 후 자재 로딩 위치로 이동 후 레일 Y가 얼라인 동작 진행
        /// </summary>
        /// <returns></returns>
        public int StripRailLoadingAndAlign()
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_RAIL_LOADING, true);

                    }
                    break;

                case 6:
                    {
                        // 레일 내 자재가 없다면 동작하지 않고 완료 처리
                        if(m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 8:
                    {
                        // 레일 로딩 및 얼라인 진행 

                        // DryRun 모드
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        //jy.yang 센서 없어짐

                        //// 매거진과 레일 사이 자재가 감지 된다면 에러 ( B접점, 감지 시 OFF )
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE].bOptionUse)
                        //{
                        //    
                        //    // A접으로 변경 됨
                        //    if (GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3111] == 1)
                        //    {
                        //        return (int)ERDF.E_LD_MGZ_TO_RAIL_X_MATERIAL_ALREADY_DETECTED;
                        //    }
                        //}

                        //::: 자재 위치가 맞는지 확인 해야 함 :::
                        //레일 내 자재 유무 확인
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 0)
                        {
                            return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON;
                        }
                    }
                    break;

                case 12:
                    {
                        // ::: 위 시퀀스에서 했던 대기 동작들은 확인용으로 변경 하자 :::
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    // 시작 전 테이블 VAC OFF 해야 함
                    {
                        // 블로우 오프 ADD 22.05.19
						// 시작 전 VAC이 켜져 있을 경우 OFF 
                        {
                            //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                            if (m_bFirstSeqStep)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                            }
                            nFuncResult = InletTableBlowOn(false, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE BLOW OFF COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                    //2022 08 16 불필요한 움직임
					//HEP ADD
                case 16:
                    //// INLET TABLE DOWN
                    //{
                    //    nFuncResult = InletTableUpDown(false);
                    //    if (FNC.IsSuccess(nFuncResult))
                    //    {
                    //        SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                    //    }
                    //}
                    break;

                case 18:
                    // LOAD RAIL LOADING POS MOVE  (T,  Guide)
                    {
                        //nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_LOADING);

                        // T만 동작, 레일 Y축은 얼라인 상태를 유지 함
                        nFuncResult = MovePosLdRailT(POSDF.LD_RAIL_T_STRIP_LOADING, 0.0);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL T AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                #region 자재 레일 로딩

                case 30:
                    // LD VISION GRIPPER UNGRIP
                    {
                        nFuncResult = LdVisionGrip(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD VISION GRIPPER OPEN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    // LOAD RAIL ALIGN POS MOVE  (T,  Guide)
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        nFuncResult = InletTableUpDown(true, 300);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 38:
                    {
                        // 기술팀 요청사항
                        // RAIL 얼라인 시 FRONT /REAR 둘 다 사용해서 동작 요청
                        nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_ALIGN);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS ALIGN POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y AXIS ALIGN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 40:
                    // INLET TABLE VAC ON
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        //테스트
                        //nFuncResult = (int)ERDF.E_LD_RAIL_VAC_NOT_ON;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_PRESS_TO_STRIP_USE].bOptionUse) break;

                        nFuncResult = InletTableVacOn(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE VAC ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 42:
                    {
                        // 프리 얼라인 보정 하기 전 앵글 티칭 위치로 이동
                        nFuncResult = MovePosLdRailT(POSDF.LD_RAIL_T_STRIP_ALIGN, 0.0);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL T AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                #endregion

                case 50:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "STRIP LOADING CYCLE FINISH", STEP_ELAPSED, strStripInfo));

                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_RAIL_LOADING, false);

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

        #region Strip Press and Vacuum
        public int StripPressAndVac()
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

                case 2:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PRESS_AND_VACUUM, true);
                    }
                    break;

                case 4:
                    {
                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 레일 Y의 자재 감지 센서가 하나라도 OFF이면 에러
                        if (/*GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] != 1*/ // 다 투입된 경우 프론트가 감지가 안됨 
                            GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 0)
                        {
                            //return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON;
                        }
                    }
                    break;

                case 6:
                    // BLOW ON
                    {
                        nFuncResult = StripPickerBlow(true,0, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 8:
                    // BLOW OFF
                    {
                        nFuncResult = StripPickerBlow(false, 0, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 10:
                    // STRIP GRIPPER UP
                    {
                        nFuncResult = LdVisionGripUpDown(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP GRIPPER UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosLdVisionX(POSDF.LD_VISION_X_READY)) break;
                    }
                    nFuncResult = MovePosLdVisionX(POSDF.LD_VISION_X_READY);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP VISION READY POS MOVE COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 16:
                    // INLET TABLE UP
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        nFuncResult = InletTableUpDown(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE UP COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 18:
                    // LOAD RAIL ALIGN POS MOVE  (T,  Guide)
                    {
                        nFuncResult = MovePosLdRailYT(POSDF.LD_RAIL_T_STRIP_ALIGN);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL YT AXIS ALIGN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // STRIP TRANSFER X STRIP PUSH DOWN 위치 이동
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_PUSH_DOWN);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS STRIP PUSH DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 23:
                    // STRIP TRANSFER Z STRIP PUSH DOWN SLOW DOWN 위치 이동
                    {
                        double dslowdownoffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_PUSH_DOWN, -dslowdownoffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS STRIP PUSH DOWN SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // STRIP TRANSFER Z STRIP PUSH DOWN 위치 이동
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_PUSH_DOWN, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS STRIP PUSH DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 25:
                    nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_UNLOADING);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y PRESS - OUT POSITION COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 26:
                    // BLOW ON
                    {
                        nFuncResult = StripPickerBlow(true, 0, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 27:
                    // INLET TABLE VAC ON
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        nFuncResult = InletTableVacOn(true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 30:
                    // STRIP TRANSFER Z STRIP PUSH DOWN 위치 이동
                    {
                        double dslowdownoffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_PUSH_DOWN, -dslowdownoffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS STRIP PUSH DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 32:
                    // STRIP TRANSFER Z READY 위치 이동
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // BLOW OFF
                    {
                        nFuncResult = StripPickerBlow(false, 0, false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 40:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PRESS_AND_VACUUM, false);

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


        #region Strip PreAlign
        public int StripPreAlign()
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
                        m_nAlignNo = 0;
                        m_bAfterRotateCheck = false;

                        if (m_bAutoMode)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_USE].bOptionUse)
                            {
                                GbVar.Seq.sStripTransfer.dAlignXCorrectCurrOffset = 0.0;
                                GbVar.Seq.sStripTransfer.dAlignYCorrectCurrOffset = 0.0;
                                SEQ_INFO_CURRENT.Info.dPreAngleOffset = 0.0;
                                return FNC.SUCCESS;
                            }

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                    break;
                            }

                            // 자재 데이터가 없다면 동작하지 않고 완료 처리
                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                return FNC.SUCCESS;
                            }
                        }

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
                        }
                        
                        if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;

                        //2022 08 24 HEP
                        //프리얼라인 데이터 초기화
                        //결과 값 초기화
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                        {
                            GbVar.dbGetPreAlign.InitDataPreAlign();
                        }
                    }
                    break;
                case 1:
                    {
                        if (m_nRetry > 0)
                        {
                            if (WaitDelay(200)) return FNC.BUSY;
                        }

                        IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                        IFMgr.Inst.VISION.SetPreAlignComplete(false);
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

                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PREALIGN, true);
                    }
                    break;

                case 5:
                    {
                        //20211113 CHOH: 드라이런 모드일때 (자재없이)
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (m_bAutoMode)
                        {
                            // 레일 Y의 자재 감지 센서가 하나라도 OFF이면 에러 //센서 위치 수정으로인한 REAR만 본다
                            if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 0)
                            {
                                return (int)ERDF.E_LD_RAIL_Y_MATERIAL_SENSOR_NOT_ON;
                            }
                        }
                    }
                    break;

                case 6:
                    // STRIP TRANSFER Z READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 8:
                    {
                        //2022 09 27 HEP
                        //제거 이부분에서 문제
                        //if (m_bAutoMode)
                        //{
                        //    if (!SEQ_INFO_CURRENT.Info.IsStrip())
                        //    {
                        //        return FNC.SUCCESS;
                        //    }
                        //}
                    }
                    break;

                case 10:
                    {
                        // T축 얼라인 위치 이동
                        nFuncResult = MovePosLdRailT(POSDF.LD_RAIL_T_STRIP_ALIGN, 0.0f);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PRE-ALIGN CORRECT POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                #region ALIGN INTERFACE
                case 20:
                    if (m_bAutoMode)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }
                    }

                    IFMgr.Inst.VISION.SetPreAlignMode(VSDF.ePRE_INSP_MODE.ALIGN1 + m_nAlignNo);
                    break;

                case 22:
                    // STRIP TRANSFER X AXIS 1ST ALIGN POS MOVE
                    {
                        nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_STRIP_PREALIGN_1 + m_nAlignNo, POSDF.LD_VISION_X_STRIP_ALIGN_1 + m_nAlignNo);
                        //nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_PREALIGN_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            if (m_nAlignNo == 0)
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XY AXIS 1ST ALIGN POSITION COMPLETE", STEP_ELAPSED));
                            }
                            else
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XY AXIS 2ND ALIGN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                case 24:
                    // STRIP TRANSFER Z AXIS 1ST ALIGN POS MOVE
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_PREALIGN_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            if (m_nAlignNo == 0)
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS 1ST ALIGN POSITION COMPLETE", STEP_ELAPSED));
                            }
                            else
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS 2ND ALIGN POSITION COMPLETE", STEP_ELAPSED));
                            }
                        }
                    }
                    break;

                case 26:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 28:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                    break;
                            }
                        }

                        IFMgr.Inst.VISION.SetPreAlignGrabReq(true);
                    }
                    break;

                case 29:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                    break;
                            }
                        }

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetry++;
                                // 알람 발생시 비전 인터페이스 클리어 하기 위해
                                //20220707 홍은표
								//컴플리트로 초기화 하기전 그랩리퀘스트 신호 오프 // 2022 07 21 홍은표
                                IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                                IFMgr.Inst.VISION.SetPreAlignComplete(true);
                                NextSeq(0);
                                return FNC.BUSY;
                            }
                            m_nRetry = 0;
                            
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_REQ_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsPreAlignGrabReq) return FNC.BUSY;
                        // [2022.04.12.kmlee] 바로 끄지않고 Complete 주기 전 Req OFF
                        //IFMgr.Inst.VISION.SetPreInspGrabReq(false);

                        m_nRetry = 0;
                    }
                    break;

                case 30:
                    if (m_bAutoMode)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetry++;
                            // 알람 발생시 비전 인터페이스 클리어 하기 위해
                            //20220707 홍은표
							//컴플리트로 초기화 하기전 그랩리퀘스트 신호 오프 // 2022 07 21 홍은표
                            IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                            IFMgr.Inst.VISION.SetPreAlignComplete(true);
                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetry = 0;
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_ON;
                    }

                    if (!IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;

                    // [2022.05.04.kmlee] Complete 켜기 전 Req OFF
                    IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                    IFMgr.Inst.VISION.SetPreAlignComplete(true);

                    m_nRetry = 0;
                    break;

                case 32:
                    if (m_bAutoMode)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_OFF;
                    }

                    if (IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;
                    IFMgr.Inst.VISION.SetPreAlignComplete(false);
                    break;

                case 34:
                    // READ DB
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                    break;
                            }
                        }

                        strVisionResult = IFMgr.Inst.VISION.GetPreInspResult(VSDF.ePRE_INSP_MODE.ALIGN1 + m_nAlignNo);

                        // Angle 보정 후 재확인
                        if (m_bAfterRotateCheck)
                        {
                            if (strVisionResult[0] == "1")
                            {
                                GbVar.Seq.sStripTransfer.dAlignXCorrectCurrOffset = Convert.ToDouble(strVisionResult[1]);
                                GbVar.Seq.sStripTransfer.dAlignYCorrectCurrOffset = Convert.ToDouble(strVisionResult[2]);
                            }
                            else if (strVisionResult[0] == "")
                            {
                                //2022 09 25 HEP
                                // 데이터가 없는 경우 다시 처음부터 인터페이스 하는 기능 추가
                                if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                {
                                    m_nRetry++;
                                    // 알람 발생시 비전 인터페이스 클리어 하기 위해
                                    //20220707 홍은표
                                    IFMgr.Inst.VISION.SetPreAlignComplete(true);
                                    NextSeq(0);
                                    return FNC.BUSY;
                                }
                                m_nRetry = 0;

                                //2022 08 25 HEP
                                // 데이터가 들어오지 않았음
                                SEQ_INFO_CURRENT.Info.bPreResult1 = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                {
                                    return (int)ERDF.E_PRE_ALIGN_DATA_MISSING_ERROR;
                                }
                                m_nRetry = 0;

                            }
                            else
                            {
                                // Matching이 안되었을 때 Error 또는 이전 Data를 사용할 지 ...
                                // [2022.04.06. kmlee] 우선 알람 발생. 추 후 방안 협의 필요
                                return (int)ERDF.E_PREALIGN_NG;
                            }
                        }
                        // Angle 보정 전 얼라인 값
                        else
                        {
                            if (m_nAlignNo == 0)
                            {
                                if (strVisionResult[0] == "1")
                                {
                                    SEQ_INFO_CURRENT.Info.bPreResult1 = true;
                                    SEQ_INFO_CURRENT.Info.ptPreOffset1.X = (float)Convert.ToDouble(strVisionResult[1]);
                                    SEQ_INFO_CURRENT.Info.ptPreOffset1.Y = (float)Convert.ToDouble(strVisionResult[2]);
                                }
                                else if (strVisionResult[0]== "")
                                {
                                    //2022 09 25 HEP
                                    // 데이터가 없는 경우 다시 처음부터 인터페이스 하는 기능 추가
                                    if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                    {
                                        m_nRetry++;
                                        // 알람 발생시 비전 인터페이스 클리어 하기 위해
                                        //20220707 홍은표
                                        IFMgr.Inst.VISION.SetPreAlignComplete(true);
                                        NextSeq(0);
                                        return FNC.BUSY;
                                    }
                                    m_nRetry = 0;

                                    //2022 08 25 HEP
                                    // 데이터가 들어오지 않았음
                                    SEQ_INFO_CURRENT.Info.bPreResult1 = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                    {
                                        return (int)ERDF.E_PRE_ALIGN_DATA_MISSING_ERROR;
                                    }
                                }
                                else // NG일 때는 "2"
                                {
                                    SEQ_INFO_CURRENT.Info.bPreResult1 = false;
                                }
                            }
                            else
                            {
                                if (strVisionResult[0] == "1")
                                {
                                    SEQ_INFO_CURRENT.Info.bPreResult2 = true;
                                    SEQ_INFO_CURRENT.Info.ptPreOffset2.X = (float)Convert.ToDouble(strVisionResult[1]);
                                    SEQ_INFO_CURRENT.Info.ptPreOffset2.Y = (float)Convert.ToDouble(strVisionResult[2]);
                                }
                                else if (strVisionResult[0] == "")
                                {
                                    //2022 09 25 HEP
                                    // 데이터가 없는 경우 다시 처음부터 인터페이스 하는 기능 추가
                                    if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                                    {
                                        m_nRetry++;
                                        // 알람 발생시 비전 인터페이스 클리어 하기 위해
                                        //20220707 홍은표
                                        IFMgr.Inst.VISION.SetPreAlignComplete(true);
                                        NextSeq(0);
                                        return FNC.BUSY;
                                    }
                                    m_nRetry = 0;

                                    //2022 08 25 HEP
                                    // 데이터가 들어오지 않았음
                                    SEQ_INFO_CURRENT.Info.bPreResult1 = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_DATA_CLEAR_MODE].bOptionUse)
                                    {
                                        return (int)ERDF.E_PRE_ALIGN_DATA_MISSING_ERROR;
                                    }
                                }
                                else // NG일 때는 "2"
                                {
                                    SEQ_INFO_CURRENT.Info.bPreResult2 = false;
                                }
                            }
                        }
                    }
                    break;
                case 36:
                    {
                        // Angle 보정 후 재확인 하였다면 완료 시퀀스로 이동
                        if (m_bAfterRotateCheck)
                        {
                            NextSeq(90);
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
                #endregion

                #region Angle 보정 전 Angle 보정 부분
                case 40:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                            {
                                SEQ_INFO_CURRENT.Info.dPreAngleOffset = 0;

                                NextSeq(46);
                                return FNC.BUSY;
                            }
                        }

                        // 프리얼라인 NG이면 알람
                        if (!SEQ_INFO_CURRENT.Info.IsPreOk)
                        {
                            return (int)ERDF.E_PREALIGN_NG;
                        }
                    }
                    break;

                case 42:
                    // LOADING RAIL ANGLE 보정값 계산
                    {
                        m_dStripPreAngle = GetPreAlignCalcAngle();

                        m_dStripPreAngle += ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE].dValue;

                        SEQ_INFO_CURRENT.Info.dPreAngleOffset = m_dStripPreAngle;
                    }
                    break;

                case 44:
                    // ANGLE 보정
                    {
                        nFuncResult = MovePosLdRailT(POSDF.LD_RAIL_T_STRIP_ALIGN, m_dStripPreAngle);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PRE-ALIGN CORRECT POS MOVE COMPLETE", STEP_ELAPSED));
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

                case 90:
                    {
                        if (!m_bAutoMode)
                        {
                            IFMgr.Inst.SAW.SetPreAlignOffset(GbVar.Seq.sStripTransfer.dAlignYCorrectCurrOffset, 0.0);
                        }
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_PREALIGN, false);

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

        #region Strip Orient Check
        public int StripOrientMarkCheck()
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
                        IFMgr.Inst.VISION.SetPreAlignComplete(false);

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_ORIENT_CHECK_USE].bOptionUse)
                        {
                            return FNC.SUCCESS;
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

                case 10:
                    // STRIP TRANSFER Z READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                //case 20:
                //    IFMgr.Inst.VISION.SetPreInspMode(VSDF.ePRE_INSP_MODE.ORIENTATION);
                //    break;

                //case 21:
                //    // STRIP TRANSFER X AXIS 1ST ALIGN POS MOVE
                //    {
                //        nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK, POSDF.LD_VISION_Y_STRIP_ORIENT_CHECK);

                //        if (FNC.IsSuccess(nFuncResult))
                //        {
                //            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XY AXIS ORIENT POSITION COMPLETE", STEP_ELAPSED));
                //        }
                //    }
                //    break;

                //case 22:
                //    // STRIP TRANSFER Z AXIS 1ST ALIGN POS MOVE
                //    {
                //        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK);

                //        if (FNC.IsSuccess(nFuncResult))
                //        {
                //            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XYZ AXIS ORIENT POSITION COMPLETE", STEP_ELAPSED));
                //        }
                //    }
                //    break;

                //case 23:
                //    // TRIGGER ONE-SHOT
                //    {
                //        //IFMgr.Inst.VISION.TrgPreOneShot();
                //        IFMgr.Inst.VISION.SetPreInspGrabReq(true);
                //    }
                //    break;

                //case 24:
                //    // TRIGGER ONE-SHOT
                //    {
                //        if (!IFMgr.Inst.VISION.IsPreInspGrabReq) return FNC.BUSY;
                //        IFMgr.Inst.VISION.SetPreInspGrabReq(false);
                //    }
                //    break;

                //case 25:
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                //        IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                //    {
                //        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_8;
                //    }
                //    if (!IFMgr.Inst.VISION.IsPreInspCompleted) return FNC.BUSY;
                //    IFMgr.Inst.VISION.SetPreInspComplete(true);
                //    break;

                //case 26:
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                //        IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                //    {
                //        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_9;
                //    }
                //    if (IFMgr.Inst.VISION.IsPreInspCompleted) return FNC.BUSY;
                //    IFMgr.Inst.VISION.SetPreInspComplete(false);
                //    break;

                //case 28:
                //    // READ DB
                //    strVisionResult = IFMgr.Inst.VISION.GetPreInspResult(VSDF.ePRE_INSP_MODE.ORIENTATION);
                //    break;

                case 40:
                    // STRIP SET DATA
                    {

                    }
                    break;

                case 50:
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

        #region Strip ReadBarcode
        public int StripReadBarcode()
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
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
                        }

                        if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;

                        IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                        IFMgr.Inst.VISION.SetPreAlignComplete(false);

                        GbVar.dbGetPreAlign.InitDataBarcode();
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_READ_BARCODE, true);
                    }
                    break;

                case 10:
                    // STRIP TRANSFER Z READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 12:
                    {
                        if (m_bAutoMode)
                        {
                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "LD RAIL STRIP INFO IS NOT EXISTS. RETURN SUCCESS.", STEP_ELAPSED));
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 20:
                    IFMgr.Inst.VISION.SetPreAlignMode(VSDF.ePRE_INSP_MODE.CODE);
                    break;

                case 21:
                    // STRIP TRANSFER X AXIS BARCODE POS MOVE
                    {
                        nFuncResult = MovePosStripPkXY(POSDF.STRIP_PICKER_STRIP_2D_CODE, POSDF.LD_VISION_X_STRIP_BARCODE);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XY AXIS BARCODE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 22:
                    // STRIP TRANSFER Z AXIS BARCODE POS MOVE
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_2D_CODE);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER XYZ AXIS BARCODE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 23:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }

                        IFMgr.Inst.VISION.SetPreAlignGrabReq(true);
                    }
                    break;

                case 24:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                                break;
                        }

                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                            {
                                m_nRetry++;
                                // 알람 발생시 비전 인터페이스 클리어 하기 위해
                                //20220707 홍은표
                                IFMgr.Inst.VISION.SetPreAlignComplete(true);
                                NextSeq(0);
                                return FNC.BUSY;
                            }
                            m_nRetry = 0;
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_REQ_ON;
                        }
                        m_nRetry = 0;

                        if (!IFMgr.Inst.VISION.IsPreAlignGrabReq) return FNC.BUSY;
                        // [2022.04.12.kmlee] 바로 끄지않고 Complete 주기 전 Req OFF
                        //IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                    }
                    break;

                case 25:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetry++;
                            // 알람 발생시 비전 인터페이스 클리어 하기 위해
                            //20220707 홍은표
                            IFMgr.Inst.VISION.SetPreAlignComplete(true);
                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetry = 0;
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_ON;
                    }
                    m_nRetry = 0;

                    if (!IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;

                    // [2022.04.12.kmlee] Complete 주기 전 Req OFF
                    IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                    IFMgr.Inst.VISION.SetPreAlignComplete(true);
                    break;

                case 26:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_OFF;
                    }
                    if (IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;
                    IFMgr.Inst.VISION.SetPreAlignComplete(false);
                    break;

                case 28:
                    // READ DB
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_VISION_INTERFACE_USE].bOptionUse)
                            break;
                    }

                    string[] VisionResult = IFMgr.Inst.VISION.GetPreInspResult(VSDF.ePRE_INSP_MODE.CODE);

                    if (VisionResult == null)
                    {
                        GbVar.Seq.sStripRail.Info.STRIP_ID = "";
                        break;
                    }

                    if (VisionResult.Length > 0)
                    {
                        //GbVar.Seq.sStripRail.Info.bIsStrip = true;
                        GbVar.Seq.sStripRail.Info.STRIP_ID = VisionResult[0];

                    }
                    else
                    {
                        //GbVar.Seq.sStripRail.Info.bIsStrip = true;
                        GbVar.Seq.sStripRail.Info.STRIP_ID = "";
                    }

                    int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                    int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

                    for (int nRow = 0; nRow < nTotMapCountY; nRow++)
                    {
                        for (int nCol = 0; nCol < nTotMapCountX; nCol++)
			            {
                            GbVar.Seq.sStripRail.Info.UnitArr[nRow][nCol].SUB_ID = GbVar.Seq.sStripRail.Info.STRIP_ID;
			            }
                    }
                    break;

                case 30:
                    //220604 pjh
                    //스트립 ID와 ITS ID의 앞 N자리는 일치함
                    //만약 불일치 할경우 알람
                    if (!m_bAutoMode) break;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;


                    if (GbVar.lstBinding_HostLot.Count == 0)
                    {
                        //예외 상황
                        nFuncResult = (int)ERDF.E_THERE_IS_NO_ITS_INFORMATION;
                        break;
                    }

                    //if (GbVar.Seq.sStripRail.Info.STRIP_ID.Length < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue ||
                    //   GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID.Length < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue)
                    //{
                    //    //예외 상황
                    //    nFuncResult = (int)ERDF.E_ITS_ID_STRIP_ID_UNMATCHED;
                    //    break;
                    //}


                    //if (GbVar.Seq.sStripRail.Info.STRIP_ID.Substring(0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue) !=
                    //    GbVar.lstBinding_HostLot[MCDF.CURRLOT].ITS_ID.Substring(0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ITS_ID_STRIP_ID_COMMON_STR_LEN].nValue))
                    //    nFuncResult = (int)ERDF.E_ITS_ID_STRIP_ID_UNMATCHED;

                    break;

                case 40:
                    // STRIP SET DATA
                    {
                    }
                    break;

                case 50:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.STRIP_READ_BARCODE, false);

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

        #region LoadRailStrip
        /// <summary>
        /// 스트립 피커가 레일에서 픽업
        /// </summary>
        /// <returns></returns>
        public int LoadingRailStrip()
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

                case 2:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.LOADING_RAIL_STRIP, true);
                    }
                    break;

                // Transfer Vacucm 체크해서 자재가 있는지 확인해야 함

                case 4:
                    {

                        // 트랜스퍼 PICKUP 전 VAC CHECK
                        //if (GetStripPickerVac())
                        //{
                        //    // PICKUP 전 이미 VAC이 들어와있다면 자재를 제거해야 함
                        //    return (int)ERDF.E_STRIP_TRANSFER_PICKUP_ALREADY_VAC_ON;
                        //}

                        // 시작 전 자재를 집고 있으면 이미 픽업을 완료 했다고 처리 함
                        if(m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                break;
                                
                            if (GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                if (AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // 비전 오프셋 포함 위치 확인
                                    double dAlignCorrOffsetX = RecipeMgr.Inst.Rcp.dAlignXCorrOffset - GbVar.Seq.sStripTransfer.dAlignXCorrectCurrOffset;
                                    if (IsInPosStripPkX(POSDF.STRIP_PICKER_STRIP_LOADING, -dAlignCorrOffsetX))
                                    {
                                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOADING STRIP - ALREADY STRIP VAC, NEXT SEQ ", STEP_ELAPSED));

                                        // 시퀀스 번호  변경 시 주의
                                        NextSeq(8);
                                        return FNC.BUSY;
                                    }
                                }
                            }
                            else
                            {
                                if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 0 &&
                                    // GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3111] == 0 && ?? 돌출 감지 센서 인데 없음.. 추가 
                                    GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 0)
                                {
                                    return FNC.SUCCESS;
                                }
                            }

                        }
                    }
                    break;

                case 6:
                    // STRIP TRANSFER Z AXIS READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 8:
                    {
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                break;

                            if (!GbVar.Seq.sStripRail.Info.IsStrip())
                            {
                                // 자재가 없는데 VAC이나 BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    // Vac On 에러
                                    return (int)ERDF.E_STRIP_TRANSFER_LOAD_NO_STRIP_VAC_ON;
                                }

                                if(AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    // Blow On 에러
                                    return (int)ERDF.E_STRIP_TRANSFER_LOAD_NO_STRIP_BLOW_ON;
                                }

                                //STR VAC OFF
                                // 자재가 없어 Cycle 종료 로그
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOADING STRIP - ALREADY STRIP VAC, NEXT SEQ ", STEP_ELAPSED));
                                return FNC.SUCCESS;
                            }
                        }
                    }
                    break;

                case 10:
                    // STRIP TRANSFER X AXIS LOADING RAIL POS MOVE
                    {
                        double dAlignCorrOffsetX = RecipeMgr.Inst.Rcp.dAlignXCorrOffset - GbVar.Seq.sStripTransfer.dAlignXCorrectCurrOffset;

                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkX(POSDF.STRIP_PICKER_STRIP_LOADING, -dAlignCorrOffsetX))
                                break;
                        }

                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_STRIP_LOADING, -dAlignCorrOffsetX);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1} (OFFSET : {2 : 0.000})", "STRIP PICKER X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED, -dAlignCorrOffsetX));
                        }
                    }
                    break;
                case 11:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_Y_UNLOAD_PRE_MOVE].bOptionUse)
                        {
                            //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                            if (m_bFirstSeqStep)
                            {
                                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                            }
                            nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_UNLOADING);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "SUCESS LD RAIL PRE UNLOAD MOVE DONE", STEP_ELAPSED));
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;
                case 12:
                    // STRIP TRANSFER Z AXIS PRE-LOADING POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_STRIP_LOADING))
                                break;
                        }

                        double dslowdownoffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_LOADING, -dslowdownoffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS PRE LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 14:
                    // STRIP TRANSFER Z AXIS LOADING POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_STRIP_LOADING))
                                break;
                        }

                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_LOADING, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 16:
                    {
                        nFuncResult = StripPickerVac(true, !RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat, false);
                    }
                    break;
                case 18:
                    // IN LET TABLE 블로우 온
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        nFuncResult = InletTableBlowOn(true, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    // STRIP PICKER VACUUM ON
                    // ERROR? -> INIT_SEQ ALARM   (VACUUM OFF & TRANSFER UP)
                    {
                        //nFuncResult = (int)ERDF.E_STRIP_PK_VAC_NOT_ON;
                        nFuncResult = StripPickerVac(true, !RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                        //알람이 발생하는 경우
                        if (FNC.IsErr(nFuncResult))
                        {
                            //Z축 올리고 알람 밑에서 띄우려고
                            nFuncResult = FNC.SUCCESS;
                        }
                    }
                    break;

                case 22:
                    // LOAD RAIL READY POS MOVE  (Guide)
                    {
                        nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_UNLOADING);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // STRIP TRANSFER Z SLOW DOWN POS UP
                    {
                        double dslowdownoffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_STRIP_LOADING, -dslowdownoffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 26:
                    // STRIP TRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 28:
                    {
                        if (!GbVar.Seq.sStripRail.Info.IsStrip()) break;

                        if (GbFunc.IsAVacOn((int)IODF.A_INPUT.STRIP_PK_VAC) == false)
                        {
                            nFuncResult = StripPickerVac(true, !RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse, 50);
                            if (FNC.IsSuccess(nFuncResult))
                            {
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                            }
                        }
                        else
                        {
                            nFuncResult = FNC.SUCCESS;
                        }
                    }
                    break;

                case 30:
                    // 블로우 오프
                    {
                        //2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        if (m_bFirstSeqStep)
                        {
                            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        }
                        nFuncResult = InletTableBlowOn(false, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;


                case 32:
                    // INLET TABLE DOWN
                    {
                        ////2022 10 13 HEP ADD 인렛테이블 사용하지 않는 기능 추가
                        //if (m_bFirstSeqStep)
                        //{
                        //    if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IN_LET_TABLE_UNUSED_MODE].bOptionUse) break;
                        //}
                        nFuncResult = InletTableUpDown(false);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "INLET TABLE DOWN COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    
                case 34:
                    // LOAD RAIL READY POS MOVE  (Guide)
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosLdRailY(POSDF.LD_RAIL_T_STRIP_LOADING)) break;
                        }
                        nFuncResult = MovePosLdRailY(POSDF.LD_RAIL_T_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD RAIL Y AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.LOADING_RAIL_STRIP, false);

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

        #region UnloadToCuttingTable
        public int UnloadingToCuttingTable()
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
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    break;

                case 2:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_SAW_NOT_READY;
                    }
                    //임시로 막음
                    //if (!IFMgr.Inst.SAW.IsSawRun()) return FNC.BUSY;
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOADING_CUTTING_TABLE, true);

                    }
                    break;

                case 8:
                    {
                        // 확인 후 사용하면 됨
                        if (m_bAutoMode)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                break;

                            // 자재 정보가 있을 때는 이어서 진행되도록
                            if (GbVar.Seq.sStripTransfer.Info.IsStrip())
                            {
                                // 트랜스퍼 피커에 VAC상태가 아니라면 자재를 내려놓았다 판단하고 이어서 시작하기 위함
                                if (AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    if (IsInPosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1))
                                    {
                                        if(IsInPosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1))
                                        {
                                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER UNLOADING TABLE - ALREADY STRIP VAC, NEXT SEQ ", STEP_ELAPSED));
                                            NextSeq(20); //주의 CASE 번호 
                                            return FNC.BUSY;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;

                case 10:
                    // STRIP TRANSFER Z AXIS READY POS MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock( false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                // 자재가 없을 경우 VAC, BLOW OFF인지 확인하고 Cycle 종료
                case 12:
                    {
                        if(m_bAutoMode)
                        {
                            //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                            // 자재 정보가 없을 경우
                            if (!GbVar.Seq.sStripTransfer.Info.IsStrip())
                            {
                                // VAC, BLOW OFF 확인
                                // VAC이 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.VAC)
                                {
                                    return (int)ERDF.E_STRIP_TRANSFER_UNLOAD_NO_STRIP_VAC_ON;
                                }

                                // BLOW가 켜져있으면 알람
                                if (AirStatus(STRIP_MDL.STRIP_TRANSFER) == AIRSTATUS.BLOW)
                                {
                                    return (int)ERDF.E_STRIP_TRANSFER_UNLOAD_NO_STRIP_BLOW_ON;
                                }

                                // 자재가 없는데 VAC, BLOW가 다 꺼져있으면 Cycle 종료인데
                                // 스트립 피커의 경우 Saw와 인터페이스를 꺼야함
                                IFMgr.Inst.SAW.ResetStripTransferIF();
                                SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER UNLOADING TABLE - NO STRIP, CYCLE SUCCESS ", STEP_ELAPSED));
                                return FNC.SUCCESS;
                            }
                        }

                    }
                    break;

                case 20:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (m_bAutoMode)
                    {
                        if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.STRIP_TRANSFER] == false)
                        {
                            // Saw는 우리가 알람 떠도 계속 런이어서 따로 런 상태를 확인하지 않는다
                            return FNC.CYCLE_CHECK;
                        }
                    }

                    if (!IFMgr.Inst.SAW.IsTableLoadingReq()) return FNC.BUSY;

                    break;
                case 22:
                    // STRIP TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_USE].bOptionUse)
                        {
                            if (m_bFirstSeqStep)
                            {
                                if (IsInPosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1))
                                    break;
                            }

                            nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1);

                        }
                        else
                        {
                            if (m_bFirstSeqStep)
                            {
                                if (IsInPosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1_WITHOUT_PREALIGN))
                                    break;
                            }

                            nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1_WITHOUT_PREALIGN);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS TABLE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // STRIP TRANSFER X AXIS 이동 완료 신호 ON
                    {
                        IFMgr.Inst.SAW.SetPreAlignOffset(GbVar.Seq.sStripTransfer.dAlignYCorrectCurrOffset, 0.0);
                        IFMgr.Inst.SAW.SetStripTransferXLoadPos(true);
                    }
                    break;

                case 30:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }

                    if (m_bAutoMode)
                    {
                        if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.STRIP_TRANSFER] == false)
                        {
                            // Saw는 우리가 알람 떠도 계속 런이어서 따로 런 상태를 확인하지 않는다
                            return FNC.CYCLE_CHECK;
                        }
                    }
                    if (!IFMgr.Inst.SAW.IsTableLoadingPos()) return FNC.BUSY;

                    break;

                case 32:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    
                case 34:
                    // STRIP TRANSFER Z AXIS UNLOADING POS  MOVE
                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                    

                case 36:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    {
                        IFMgr.Inst.SAW.SetStripTransferZLoadPos(true);
                    }
                    break;

                case 38:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        // DELAY
                        {
                            if (WaitDelay(1000)) return FNC.BUSY;
                        }
                        break;
                    }
                    else
                    {
                        // UP REQ?
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                        {
                            // Cycle이라 안해도 될 듯
                            //InitSeq();

                            // Vacuum 알람이라 판단하고 Blow를 끈다
                            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.STRIP_PK_VAC_SOL_STRIP, false);
                            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD, false);
                            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.STRIP_PK_VAC_OFF_2, false);
                            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.STRIP_PK_BLOW, false);
                            return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                        }

                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }

                        if (!IFMgr.Inst.SAW.IsTableLoadingVacOn()) return FNC.BUSY;

                        // 테스트
                        //return FNC.BUSY;
                        break;
                    }

                case 40:
                    // BLOW ON
                    {
                        nFuncResult = StripPickerBlow(true, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 42:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 44:
                    // BLOW OFF
                    {
                        nFuncResult = StripPickerBlow(false, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 46:
                    // STRIP STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 48:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    IFMgr.Inst.SAW.SetStripTransferLoadComplete(true);
                    break;

                case 50:
                    //20211113 CHOH: 드라이런 모드일때 (SAW SKIP)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse)
                    {
                        IFMgr.Inst.SAW.ResetStripTransferIF();
                        break;
                    }

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    //if (!IFMgr.Inst.SAW.IsTableLoadingComplete(ULD_TABLE_NO)) return FNC.BUSY;

                    if (IFMgr.Inst.SAW.IsTableLoadingReq()) return FNC.BUSY;
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    break;

                case 52:
                    // STRIP STRANSFER X READY POS MOVE
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 60:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.UNLOADING_CUTTING_TABLE, false);

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

        #region Reloading
        public int ReloadingToCuttingTable(bool bReloadAgain = false)
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
                        //20211206 CHOH : 리로드 오프셋 초기화하는 조건 생각해볼것
                        //if (!bReloadAgain) dReloadCorrOffsetXResult = 0.0;
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
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.RELOADING_CUTTING_TABLE, true);
                    }
                    break;

                case 5:
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    //if (!IFMgr.Inst.SAW.IsTableReloadReq()) return FNC.BUSY;
                    //SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER RELOAD REQUEST SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));
                    break;

                case 10:
                    // STRIP TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;


                    //if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    //{
                    //    InitSeq();
                    //    return (int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    //}
                    if (!IFMgr.Inst.SAW.IsTableReloadReq()) return FNC.BUSY;
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER RELOAD REQUEST SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case 22:
                    // STRIP TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            //m_strLogMsg = string.Format("STRIP PICKER X RELOAD CORR OFFSET X: {0}", dReloadCorrOffsetXResult.ToString("F3"));
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS TABLE POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 24:
                    // STRIP TRANSFER X AXIS 이동 완료 신호 ON
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetStripTransferXLoadPos(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 30:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    if (!IFMgr.Inst.SAW.IsTableLoadingPos()) return FNC.BUSY;
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOADING REQUEST SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case 32:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (WaitDelay(1000)) return FNC.BUSY;
                        break;
                    }

                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 34:
                    // STRIP TRANSFER Z AXIS UNLOADING POS  MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (WaitDelay(1000)) return FNC.BUSY;
                        break;
                    }

                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 36:
                    // VACUUM ON
                    {
                        nFuncResult = StripPickerVac(true, !RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER VACUUM ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 38:
                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetStripTransferZLoadPos(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 40:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_LOAD_FROM_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    // Vac Off Check !!!
                    if (!IFMgr.Inst.SAW.IsTableUnloadingBlowOn()) return FNC.BUSY;
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    break;

                case 44:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                    {
                        if (WaitDelay(1000)) return FNC.BUSY;
                        break;
                    }

                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 46:
                    // STRIP STRANSFER Z AXIS UP
                    {
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 48:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    IFMgr.Inst.SAW.SetStripTransferLoadComplete(true);
                    IFMgr.Inst.SAW.SetStripTransferZLoadPos(false);
                    IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                    //IFMgr.Inst.SAW.ResetStripTransferIF(ULD_TABLE_NO);
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER LOAD COMPLETE SIGNAL ON COMPLETE", STEP_ELAPSED));
                    break;

                case 50:
                    //if (!IFMgr.Inst.SAW.IsTableLoadingReq(ULD_TABLE_NO)) return FNC.BUSY;
                    //string[] strCorrData = GbVar.dbFromSaw.SelectResult();
                    //double dCorrOffsetX = 0.0f;
                    //if (!double.TryParse(strCorrData[0], out dCorrOffsetX)) dCorrOffsetX = 0.0f;

                    RELOAD_SAW_OFFSET += IFMgr.Inst.SAW.GetReloadOffset();

                    m_strLogMsg = string.Format("STRIP PICKER X RELOAD REQUEST OFFSET X: {0}", RELOAD_SAW_OFFSET.ToString("F3"));
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                    if (RELOAD_SAW_OFFSET > 1.0)
                    {
                        RELOAD_SAW_OFFSET = 0.0f;

                        //여기서 알람이 뜨면 SAW 설비는 초기화가 되지 않는다.
                        //return (int)ERDF.E_STRIP_PK_CORRECT_OFFSET_OVER;
                    }
                    break;

                case 54:
                    break;

                case 56:
                    // STRIP TRANSFER X AXIS CUT TABLE POS MOVE
                    {
                        double dCorrOffsetX = IFMgr.Inst.SAW.GetReloadOffset();

                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, dCorrOffsetX);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS TABLE CORR POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 58:
                    // STRIP TRANSFER X AXIS 이동 완료 신호 ON
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetStripTransferXLoadPos(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X LOADING POSITION SIGNAL ON COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 60:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    if (!IFMgr.Inst.SAW.IsTableLoadingPos()) return FNC.BUSY;
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER TABLE LOADING POS SIGNAL ON CHECK COMPLETE", STEP_ELAPSED));

                    //IFMgr.Inst.SAW.SetStripTransferLoadComplete(ULD_TABLE_NO, false);
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    break;

                case 62:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS  MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 64:
                    // STRIP TRANSFER Z AXIS UNLOADING POS  MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetTransferZDownInterlock(true);
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, 0.0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 66:
                    // BLOW ON
                    {
                        nFuncResult = StripPickerBlow(true, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 68:
                    // STRIP TRANSFER Z AXIS UNLOADING POS MOVE DONE SIGNAL ON
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    {
                        IFMgr.Inst.SAW.SetStripTransferZLoadPos(true);
                    }
                    break;

                case 70:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    // UP REQ?
                    if (!IFMgr.Inst.SAW.IsTableLoadingVacOn()) return FNC.BUSY;

                    break;

                case 72:
                    // BLOW OFF
                    {
                        nFuncResult = StripPickerBlow(false, 0, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BLOW_VAC_SENSOR_USE].bOptionUse);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 74:
                    // STRIP TRANSFER Z AXIS PRE-UNLOADING POS MOVE
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_OFFSET].dValue;
                        nFuncResult = MovePosStripPkZ(POSDF.STRIP_PICKER_UNLOADING_TABLE_1, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER Z AXIS TABLE SLOW DOWN POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 76:
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

                case 78:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    IFMgr.Inst.SAW.SetStripTransferLoadComplete(true);
                    break;

                case 80:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_SAW_USE].bOptionUse) break;

                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SAW_INTERFACE_TIMEOUT].lValue))
                    {
                        InitSeq();
                        return (int)ERDF.E_STRIP_PK_UNLOAD_TO_LF_TABLE_SAW_MACHINE_INTERFACE_TIMEOUT;
                    }
                    //if (!IFMgr.Inst.SAW.IsTableLoadingComplete(ULD_TABLE_NO)) return FNC.BUSY;

                    if (IFMgr.Inst.SAW.IsTableReloadReq()) return FNC.BUSY;
                    IFMgr.Inst.SAW.ResetStripTransferIF();
                    break;

                case 82:
                    // STRIP STRANSFER X READY POS MOVE
                    {
                        nFuncResult = MovePosStripPkX(POSDF.STRIP_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            IFMgr.Inst.SAW.SetTransferZDownInterlock(false);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER X AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 100:
                    {
                        string strStripInfo = string.Format("{0}",
                            "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "TRAY RECEIVE FINISH", STEP_ELAPSED, strStripInfo));
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.RELOADING_CUTTING_TABLE, false);

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

        #region Manual Align Interface
        /// <summary>
        /// 수동으로 Align Interface 용도
        /// </summary>
        /// <param name="nAlignMode">Align Mode (ALIGN1, ALIGN2, BARCODE)</param>
        /// <returns></returns>
        public int ManualPreAlignInterface(int nAlignMode)
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
                        IFMgr.Inst.VISION.SetPreAlignComplete(false);
                    }
                    break;

                case 20:
                    IFMgr.Inst.VISION.SetPreAlignMode(VSDF.ePRE_INSP_MODE.ALIGN1 + nAlignMode);
                    break;

                case 26:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.LD_VISION_PREALIGN_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 28:
                    // TRIGGER ONE-SHOT
                    {
                        IFMgr.Inst.VISION.SetPreAlignGrabReq(true);
                    }
                    break;

                case 29:
                    // TRIGGER ONE-SHOT
                    {
#if !_NOTEBOOK
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_REQ_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsPreAlignGrabReq) return FNC.BUSY;
#endif
//                        IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                    }
                    break;

                case 30:
#if !_NOTEBOOK
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        if (m_nRetry < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_RETRY_COUNT].nValue)
                        {
                            m_nRetry++;
                            // 알람 발생시 비전 인터페이스 클리어 하기 위해
                            //20220707 홍은표
                            IFMgr.Inst.VISION.SetPreAlignComplete(true);
                            NextSeq(0);
                            return FNC.BUSY;
                        }
                        m_nRetry = 0;
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_ON;
                    }

                    if (!IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;
#endif
                    IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                    IFMgr.Inst.VISION.SetPreAlignComplete(true);
                    break;

                case 32:
#if !_NOTEBOOK
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                                IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_PRE_ALIGN_COMP_OFF;
                    }

                    if (IFMgr.Inst.VISION.IsPreAlignCompleted) return FNC.BUSY;
#endif
                    IFMgr.Inst.VISION.SetPreAlignComplete(false);
                    break;

                case 34:
                    // READ DB
                    {
                        strVisionResult = IFMgr.Inst.VISION.GetPreInspResult(VSDF.ePRE_INSP_MODE.ALIGN1 + nAlignMode);

                        switch ((VSDF.ePRE_INSP_MODE)(nAlignMode + VSDF.ePRE_INSP_MODE.ALIGN1))
                        {
                            case VSDF.ePRE_INSP_MODE.ALIGN1:
                                {
                                    if (strVisionResult.Length < 3)
                                    {
                                        return (int)ERDF.E_VISION_COMM_ERROR_1;
                                    }

                                    if (strVisionResult[0] == "1")
                                    {
                                        SEQ_INFO_CURRENT.Info.bPreResult1 = true;
                                        SEQ_INFO_CURRENT.Info.ptPreOffset1.X = (float)Convert.ToDouble(strVisionResult[1]);
                                        SEQ_INFO_CURRENT.Info.ptPreOffset1.Y = (float)Convert.ToDouble(strVisionResult[2]);
                                    }
                                    else
                                    {
                                        SEQ_INFO_CURRENT.Info.bPreResult1 = false;

                                        return (int)ERDF.E_PREALIGN_NG;
                                    }
                                }
                                break;
                            case VSDF.ePRE_INSP_MODE.ALIGN2:
                                {
                                    if (strVisionResult.Length < 3)
                                    {
                                        return (int)ERDF.E_VISION_COMM_ERROR_1;
                                    }

                                    if (strVisionResult[0] == "1")
                                    {
                                        SEQ_INFO_CURRENT.Info.bPreResult2 = true;
                                        SEQ_INFO_CURRENT.Info.ptPreOffset2.X = (float)Convert.ToDouble(strVisionResult[1]);
                                        SEQ_INFO_CURRENT.Info.ptPreOffset2.Y = (float)Convert.ToDouble(strVisionResult[2]);
                                    }
                                    else
                                    {
                                        SEQ_INFO_CURRENT.Info.bPreResult2 = false;

                                        return (int)ERDF.E_PREALIGN_NG;
                                    }
                                }
                                break;
                            case VSDF.ePRE_INSP_MODE.CODE:
                                {
                                    if (strVisionResult.Length < 1)
                                    {
                                        return (int)ERDF.E_VISION_COMM_ERROR_1;
                                    }

                                    GbVar.Seq.sStripRail.Info.STRIP_ID = strVisionResult[0];
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case 60:
                    // LOADING RAIL ANGLE 보정값 계산
                    if ((VSDF.ePRE_INSP_MODE)(nAlignMode + VSDF.ePRE_INSP_MODE.ALIGN1) == VSDF.ePRE_INSP_MODE.ALIGN2)
                    {
                        m_dStripPreAngle = GetPreAlignCalcAngle();

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE].bOptionUse)
                        {
                            m_dStripPreAngle += ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PREALIGN_TARGET_ANGLE].dValue;
                        }

                        SEQ_INFO_CURRENT.Info.dPreAngleOffset = m_dStripPreAngle;
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

        #region Common Function
        double GetPreAlignCalcAngle()
        {
            double dAngle = 0.0;

            double dPosX1 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];
            double dPosY1 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];
            
            double dPosX2 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.STRIP_PK_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_2];
            double dPosY2 = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.LD_VISION_X].dPos[POSDF.STRIP_PICKER_STRIP_PREALIGN_1];

            dPosX1 -= SEQ_INFO_CURRENT.Info.ptPreOffset1.X;
            dPosY1 -= SEQ_INFO_CURRENT.Info.ptPreOffset1.Y;

            dPosX2 -= SEQ_INFO_CURRENT.Info.ptPreOffset2.X;
            dPosY2 -= SEQ_INFO_CURRENT.Info.ptPreOffset2.Y;

            dAngle = MathUtil.GetAngle(dPosX1, dPosY1, dPosX2, dPosY2);

            return dAngle;
        }
        #endregion
    }
}
