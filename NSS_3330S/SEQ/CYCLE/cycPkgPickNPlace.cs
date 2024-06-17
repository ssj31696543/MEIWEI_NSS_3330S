using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
	// [2022.05.14.kmlee] 오이사님 요청으로 수정
    public class cycPkgPickNPlace : SeqBase
    {
        //bool m_bIsOneCycle = false;
        bool m_bAutoMode = true;

        // PickUp, Insp, Place
        int m_nPickerHeadNo = 0;
        int m_nPickerRunMode = 0;

        int m_nPlaceToGoodTableNo = 0;
        int m_nMapVisionTableNo = 0;

        int m_nFwdPickupCount;
        int m_nBwdPickupCount;

        bool m_bGDFirstPlace;
        bool m_bRWFirstPlace;

        int m_nFwdPlaceCount;
        int m_nBwdPlaceCount;

        int m_nPickUpColCount;
        int m_nPlaceColCount;

        int m_nMapStagePickUpCount;
        int m_nGoodTablePlaceCount;
        int m_nReworkTablePlaceCount;
        int m_nBtmInspectionCount;

        bool m_bRevPickUp = false;
        bool m_bRevPlace = false;

        PickerPadInfo[] pInfo = new PickerPadInfo[CFG_DF.MAX_PICKER_PAD_CNT];

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;
        int m_nHeadOffset;

        int m_nPickUpRetryCnt = 0;
        int m_nGdTrayTablePlaceCnt;
        int m_nRwTrayTablePlaceCnt;

        int m_nCheckHeadNo = 0;

        string m_strHead = "MULTI PICKER HEAD 1";

        #region PROPERTY
        public seqPkgPickNPlace SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sPkgPickNPlace; }
        }

        public seqMapVisionTable SEQ_INFO_MAP_TABLE
        {
            get { return GbVar.Seq.sMapVisionTable[CHIP_PICKER_PICK_UP_TABLE_NO]; }
        }

        public seqUldGoodTrayTable SEQ_INFO_GOOD_TRAY_TABLE
        {
            get { return GbVar.Seq.sUldGDTrayTable[CHIP_PICKER_PLACE_GOOD_TABLE_NO]; }
        }

        public seqUldReworkTrayTable SEQ_INFO_REWORK_TRAY_TABLE
        {
            get { return GbVar.Seq.sUldRWTrayTable; }
        }

        public int CHIP_PICKER_HEAD_NO
        {
            get
            {
                return m_nPickerHeadNo;
            }
        }

        public bool IS_CHIP_PICK_UP_RVS
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRevPickUp;
                }
                return m_bRevPickUp;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRevPickUp = value;
                }
                else
                {
                    m_bRevPickUp = value;
                }
            }
        }

        public bool IS_CHIP_PLACE_RVS
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRevPlace;
                }
                return m_bRevPlace;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRevPlace = value;
                }
                else
                {
                    m_bRevPlace = value;
                }
            }
        }

        public int CHIP_PICK_UP_FWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nFwdPickupCount;
                }
                return m_nFwdPickupCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nFwdPickupCount = value;
                }
                else
                {
                    m_nFwdPickupCount = value;
                }
            }
        }

        public int CHIP_PICK_UP_GET_BWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return CFG_DF.MAX_PICKER_PAD_CNT - SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPickupCount - 1;
                }

                return CFG_DF.MAX_PICKER_PAD_CNT - m_nBwdPickupCount - 1;
            }
        }

        public int CHIP_PICK_UP_SET_BWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPickupCount;
                }
                return m_nBwdPickupCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPickupCount = value;
                }
                else
                {
                    m_nBwdPickupCount = value;
                }
            }
        }

        public bool IS_LAST_PICKUP
        {
            get
            {
                return (CHIP_PICK_UP_FWD_COUNT + CHIP_PICK_UP_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT - 1;
            }
        }

        public bool IS_GD_FIRST_PLACE
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bGDFirstPlace;
                }
                return m_bGDFirstPlace;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bGDFirstPlace = value;
                }
                else
                {
                    m_bGDFirstPlace = value;
                }
            }
        }

        public bool IS_RW_FIRST_PLACE
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRWFirstPlace;
                }
                return m_bRWFirstPlace;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].bRWFirstPlace = value;
                }
                else
                {
                    m_bRWFirstPlace = value;
                }
            }
        }

        public int CHIP_PLACE_FWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nFwdPlaceCount;
                }
                return m_nFwdPlaceCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nFwdPlaceCount = value;
                }
                else
                {
                    m_nFwdPlaceCount = value;
                }
            }
        }

        public int CHIP_PLACE_GET_BWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return CFG_DF.MAX_PICKER_PAD_CNT - 1 - SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPlaceCount;
                }

                return CFG_DF.MAX_PICKER_PAD_CNT - 1 - m_nBwdPlaceCount;
            }
        }

        public int CHIP_PLACE_SET_BWD_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPlaceCount;
                }

                return m_nBwdPlaceCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nBwdPlaceCount = value;
                }
                else
                {
                    m_nBwdPlaceCount = value;
                }
            }
        }

        public int CHIP_PICKER_PICK_UP_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nPickUpMapTableNo;
                }

                return m_nMapVisionTableNo;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.nPickUpMapTableNo = value;
                }
                else
                {
                    m_nMapVisionTableNo = value;
                }
            }
        }

        public int CHIP_PICKER_PICK_UP_TABLE_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_MAP_TABLE.nMapTablePickUpCount;
                }

                return m_nMapStagePickUpCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_MAP_TABLE.nMapTablePickUpCount = value;
                }
                else
                {
                    m_nMapStagePickUpCount = value;
                }
            }
        }

        public int CHIP_PICKER_PICK_UP_COL_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nPickUpColCount;
                }

                return m_nPickUpColCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nPickUpColCount = value;
                }
                else
                {
                    m_nPickUpColCount = value;
                }
            }
        }

        public int CHIP_PICKER_PLACE_GOOD_TABLE_NO
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.nPlaceToGoodTableNo;
                }

                return m_nPlaceToGoodTableNo;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.nPlaceToGoodTableNo = value;
                }
                else
                {
                    m_nPlaceToGoodTableNo = value;
                }
            }
        }

        public int CHIP_PICKER_PLACE_GOOD_TABLE_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_GOOD_TRAY_TABLE.nUnitPlaceCount;
                }

                return m_nGoodTablePlaceCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_GOOD_TRAY_TABLE.nUnitPlaceCount = value;
                }
                else
                {
                    m_nGoodTablePlaceCount = value;
                }
            }
        }

        public int CHIP_PICKER_PLACE_REWORK_TABLE_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_REWORK_TRAY_TABLE.nUnitPlaceCount;
                }

                return m_nReworkTablePlaceCount;
            }

            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_REWORK_TRAY_TABLE.nUnitPlaceCount = value;
                }
                else
                {
                    m_nReworkTablePlaceCount = value;
                }
            }
        }

        public int CHIP_PICKER_INSPECTION_COUNT
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nCurrInspCount;
                }

                return m_nBtmInspectionCount;
            }
            set
            {
                if (m_bAutoMode)
                {
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].nCurrInspCount = value;
                }
                else
                {
                    m_nBtmInspectionCount = value;
                }
            }
        }
        #endregion

        public cycPkgPickNPlace(int nSeqID, int nPickerHeadNo)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sPkgPickNPlace.pInfo[nPickerHeadNo];

            m_nPickerHeadNo = nPickerHeadNo;

            if(m_nPickerHeadNo == 1)
                m_strHead = "MULTI PICKER HEAD 2";

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
            m_nPickerRunMode = (int)args[0];
            m_nPickerHeadNo = (int)args[1];

            m_nMapVisionTableNo = (int)args[2];
            m_nPlaceToGoodTableNo = (int)args[3];

            m_nMapStagePickUpCount = (int)args[4];
            m_nBtmInspectionCount = (int)args[5];
            m_nGoodTablePlaceCount = (int)args[6];

            // Revs도 
            m_bRevPickUp = (bool)args[7];
            m_bRevPlace = (bool)args[8];

            if (m_bRevPickUp)
            {
                m_nBwdPickupCount = (int)args[9];
            }
            else
            {
                m_nFwdPickupCount = (int)args[9];
            }

            // 일단 추가는 했지만 Place 개별 cycle에서는 굳이 Reverse 모드를 사용하지 않는다.
            if (m_bRevPlace)
            {
                m_nBwdPlaceCount = (int)args[10];
            }
            else
            {
                m_nFwdPlaceCount = (int)args[10];
            }

            pInfo = (PickerPadInfo[])args[11];
        }

        public void SetManualModePickUpParam(params object[] args)
        {
            m_nPickerHeadNo = (int)args[0];
            m_nMapVisionTableNo = (int)args[1];

            int nTableCount = (int)args[2];
            if (nTableCount >= 0)
            {
                m_nMapStagePickUpCount = nTableCount;
            }

            pInfo = (PickerPadInfo[])args[3];
        }

        /// <summary>
        /// sj.shin 
        /// </summary>
        /// <param name="args"></param>
        public void SetManualModePickUpParamNew(params object[] args)
        {
            // _nHeadNo, _nPickerNo, _nPickerColCount, _nTableNo, _nTableCount

            m_nPickerHeadNo = (int)args[0];
            m_nFwdPickupCount = (int)args[1];  
            m_nPickUpColCount = (int)args[2];

            m_nMapVisionTableNo = (int)args[3];
            m_nMapStagePickUpCount = (int)args[4];
        }

        public void SetManualModeInspectionParam(params object[] args)
        {
            m_nPickerHeadNo = (int)args[0];
            m_nBtmInspectionCount = (int)args[1];

            pInfo = (PickerPadInfo[])args[2];

        }


        public void SetManualModePlaceParam(params object[] args)
        {
            m_nPickerHeadNo = (int)args[0];
            m_nPlaceToGoodTableNo = (int)args[1];

            int nTableCount = (int)args[2];
            if (nTableCount >= 0)
            {
                m_nGoodTablePlaceCount = nTableCount;
            }

            pInfo = (PickerPadInfo[])args[3];
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
                        GbSeq.autoRun[SEQ_ID.PICK_N_PLACE_1 + CHIP_PICKER_HEAD_NO].InitSeq();
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].Init();

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

        #region Pk_PickUp
        public int FDirPickUp()
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
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }                        
                        }
      
                        if (IsPickerPadSkip(true, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT))
                        {
                            return (int)ERDF.CHIP_PK1_PAD1_SKIP + (CHIP_PICKER_HEAD_NO * 6);
                        }

                    }
                    break;

                case 1:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount

                    m_nPickUpRetryCnt = 0;

                    if (m_bAutoMode)
                    {
                        int nEquCnt = 0;
                        for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count; i++)
                        {
                            if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker[i] == CHIP_PICK_UP_FWD_COUNT)
                            {
                                nEquCnt++;
                            }

                        }

                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                        }
                        else
                        {
                            if (nEquCnt == 0)
                            {
                                SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                            }
                        }
                    }

                    switch (CHIP_PICKER_HEAD_NO)
                    {
                        case 0:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_1, true);
                            break;
                        case 1:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_2, true);
                            break;
                        default:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_1, true);
                            break;
                    }

                    break;

                case 2:
                    if (!m_bAutoMode)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        //nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT);
                    }
                    break;

                // 미리 이동 대기
                case 3:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT) == FNC.SUCCESS)
                            {
                                break;
                            }
                        }

                        nFuncResult = MovePosChipPickUpXY(CHIP_PICKER_HEAD_NO,
                                                       true,
                                                       CHIP_PICK_UP_FWD_COUNT,
                                                       CHIP_PICKER_PICK_UP_COL_COUNT,
                                                       ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT),
                                                       CHIP_PICKER_PICK_UP_TABLE_NO,
                                                       CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                       ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                       true);
                    }
                    break;

                case 4:
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ
                    nFuncResult = MovePosChipPickUpXYZT(CHIP_PICKER_HEAD_NO,
                                                       true,
                                                       CHIP_PICK_UP_FWD_COUNT,
                                                       CHIP_PICKER_PICK_UP_COL_COUNT,
                                                       ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT),
                                                       CHIP_PICKER_PICK_UP_TABLE_NO,
                                                       CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                       ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                       true);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        MapStageVac(CHIP_PICKER_PICK_UP_TABLE_NO, false, 1, false);
                    }
                    break;

                case 5:
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_FWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPickUpDownPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT));
                    break;

                case 6:
                    // Vac
                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // BLOW BIT OFF
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_FWD_COUNT), 3, false);
                    // VAC BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_FWD_COUNT), 2, true);

                    break;

                case 7:
                    {
                        if (m_nPickUpRetryCnt == 0)
                        {
                            // 정상 딜레이
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY].nValue)) return FNC.BUSY;
                        }
                        else
                        {
                            // 재시도 시에는 딜레이를 따로 준다
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY].nValue)) return FNC.BUSY;
                        }
                    }

                    break;

                case 8:
                    double dZAxisPreDownPosOffset = ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT);
                    if (IS_LAST_PICKUP)
                    {
                        dZAxisPreDownPosOffset = 0.0f;
                    }

                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_FWD_COUNT,
                                                 dZAxisPreDownPosOffset);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        if (!IS_LAST_PICKUP)
                        {
                            MapStageVac(CHIP_PICKER_PICK_UP_TABLE_NO, true, 1, false);
                        }
                    }
                    break;

                case 9:
                    // Picker Vacuum Check
                    // Vacuum Error == true;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse || !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse)
                    {
                        NextSeq(15);
                        return FNC.BUSY;
                    }

                    if (IsVacOn(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT))
                    {
                        NextSeq(15);
                        return FNC.BUSY;
                    }

                    // Pick Up 재시도
                    m_nPickUpRetryCnt++;
                    if (m_nPickUpRetryCnt <= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY].nValue)
                    {
                        NextSeq(4);
                        return FNC.BUSY;
                    }

                    break;


                //주석 해제 시 테스트 필요
                //case 9:
                //    //220608
                //    //베큠 알람 발생 시 Z축 0으로 이동
                //    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_READY, 
                //                                 CHIP_PICKER_HEAD_NO,
                //                                 CHIP_PICK_UP_FWD_COUNT,
                //                                 0);
                //    break;

                case 13:
                    // Vacuum Error?
                    // Picker Up
                    m_nHeadOffset = (int)ERDF.E_X2_PK_VAC_1_NOT_ON - (int)ERDF.E_X1_PK_VAC_1_NOT_ON;

                    nFuncResult = (int)ERDF.E_X1_PK_VAC_1_NOT_ON + (m_nHeadOffset * CHIP_PICKER_HEAD_NO) + (2 * CHIP_PICK_UP_FWD_COUNT);
                    break;

                case 15:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));

                        switch (CHIP_PICKER_HEAD_NO)
                        {
                            case 0:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_1, false);
                                break;
                            case 1:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_2, false);
                                break;
                            default:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_DIR_PICK_UP_PICKER_1, false);
                                break;
                        }
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

        public int RDirPickUp()
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
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }

                        if (IsPickerPadSkip(false, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_SET_BWD_COUNT))
                        {
                            return (int)ERDF.CHIP_PK1_PAD1_SKIP + (CHIP_PICKER_HEAD_NO * 6);
                        }
                    }
                    break;
                case 1:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount

                    m_nPickUpRetryCnt = 0;

                    if (m_bAutoMode)
                    {
                        int nEquCnt = 0;
                        for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count; i++)
                        {
                            if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker[i] == CHIP_PICK_UP_GET_BWD_COUNT)
                            {
                                nEquCnt++;
                            }

                        }

                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_GET_BWD_COUNT);
                        }
                        else
                        {
                            if (nEquCnt == 0)
                            {
                                SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_GET_BWD_COUNT);
                            }
                        }
                    }
                    switch (CHIP_PICKER_HEAD_NO)
                    {
                        case 0:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_1, true);
                            break;
                        case 1:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_2, true);
                            break;
                        default:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_1, true);
                            break;
                    }

                    break;

                case 2:
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        //nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT);
                    }
                    break;

                // 미리 이동 대기
                case 3:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT) == FNC.SUCCESS)
                            {
                                break;
                            }
                        }

                        nFuncResult = MovePosChipPickUpXY(CHIP_PICKER_HEAD_NO,
                                                       false,
                                                       CHIP_PICK_UP_GET_BWD_COUNT,
                                                       CHIP_PICKER_PICK_UP_COL_COUNT,
                                                       ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT),
                                                       CHIP_PICKER_PICK_UP_TABLE_NO,
                                                       CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                       ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                       true);
                    }
                    break;

                // XYZ축 이동
                case 4:
                    nFuncResult = MovePosChipPickUpXYZT(CHIP_PICKER_HEAD_NO,
                                                       false,
                                                       CHIP_PICK_UP_GET_BWD_COUNT,
                                                       CHIP_PICKER_PICK_UP_COL_COUNT,
                                                       ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT),
                                                       CHIP_PICKER_PICK_UP_TABLE_NO,
                                                       CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                       ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                       true);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        MapStageVac(CHIP_PICKER_PICK_UP_TABLE_NO, false, 1, false);
                    }
                    break;

                // Z축 SLOW DOWN
                case 5:
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPickUpDownPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT));
                    break;

                // VACUUM ON
                case 6:
                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // BLOW BIT OFF
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_GET_BWD_COUNT), 3, false);
                    // VAC BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_GET_BWD_COUNT), 2, true);

                    break;

                // DELAY
                case 7:
                    {
                        if (m_nPickUpRetryCnt == 0)
                        {
                            // 정상 딜레이
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY].nValue)) return FNC.BUSY;
                        }
                        else
                        {
                            // 재시도 시에는 딜레이를 따로 준다
                            if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY_DELAY].nValue)) return FNC.BUSY;
                        }
                    }
                    break;

                // PICK UP WAIT POS
                case 8:
                    double dZAxisPreDownPosOffset = ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT);
                    if (IS_LAST_PICKUP)
                    {
                        dZAxisPreDownPosOffset = 0.0f;
                    }

                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_GET_BWD_COUNT,
                                                 dZAxisPreDownPosOffset);
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        if (!IS_LAST_PICKUP)
                        {
                            MapStageVac(CHIP_PICKER_PICK_UP_TABLE_NO, true, 1, false);
                        }
                    }
                    break;

                // VACUUM SENSOR 확인 유무
                case 9:
                    // Picker Vacuum Check
                    // Vacuum Error == true;
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse || !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse)
                    {
                        NextSeq(15);
                        return FNC.BUSY;
                    }

                    if (IsVacOn(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT))
                    {
                        NextSeq(15);
                        return FNC.BUSY;
                    }

                    // Pick Up 재시도
                    m_nPickUpRetryCnt++;
                    if (m_nPickUpRetryCnt <= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_RETRY].nValue)
                    {
                        NextSeq(4);
                        return FNC.BUSY;
                    }

                    break;

                //주석 해제 시 테스트 필요
                //case 9:
                //    //220608
                //    //베큠 알람 발생 시 Z축 0으로 이동
                //    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_READY, 
                //                                 CHIP_PICKER_HEAD_NO,
                //                                 CHIP_PICK_UP_FWD_COUNT,
                //                                 0);
                //    break;

                // VACUUM ERROR
                case 13:
                    // Vacuum Error?
                    // Picker Up
                    int nHeadOffset = (int)ERDF.E_X2_PK_VAC_1_NOT_ON - (int)ERDF.E_X1_PK_VAC_1_NOT_ON;

                    nFuncResult = (int)ERDF.E_X1_PK_VAC_1_NOT_ON + (nHeadOffset * CHIP_PICKER_HEAD_NO) + (2 * CHIP_PICK_UP_GET_BWD_COUNT);
                    break;

                case 15:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        switch (CHIP_PICKER_HEAD_NO)
                        {
                            case 0:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_1, false);
                                break;
                            case 1:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_2, false);
                                break;
                            default:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_DIR_PICK_UP_PICKER_1, false);
                                break;
                        }

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

        #region Pk_Place
        public int PlaceToGoodTrayF()
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
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            if (!GbVar.Seq.sUldGDTrayTable[CHIP_PICKER_PLACE_GOOD_TABLE_NO].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;
                case 1:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    int nEquCnt = 0;
                    for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count; i++)
                    {
                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker[i] == CHIP_PLACE_FWD_COUNT)
                        {
                            nEquCnt++;
                        }
                    }

                    if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count == 0)
                    {
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_FWD_COUNT);
                    }
                    else
                    {
                        if (nEquCnt == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_FWD_COUNT);
                        }
                    }
                    switch (CHIP_PICKER_HEAD_NO)
                    {
                        case 0:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_1, true);
                            break;
                        case 1:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_2, true);
                            break;
                        default:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_1, true);
                            break;
                    }
                    break;

                case 2:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT) == FNC.SUCCESS) break;
                    }

                    nFuncResult = MovePosChipPlaceToGoodXY(CHIP_PICKER_HEAD_NO,
                                                            true,
                                                            CHIP_PLACE_FWD_COUNT,
                                                            0,
                                                            //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT),
                                                            ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                            true);
                    break;
                case 3:
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT);
                    }
                    break;

                case 5:
                    double dZAxisPreDownPos = ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT);
                    if (!IS_GD_FIRST_PLACE)
	                {
                        dZAxisPreDownPos = 0.0f;
	                }

                    nFuncResult = MovePosChipPlaceToGoodXYZT(CHIP_PICKER_HEAD_NO,
                                                            true,
                                                            CHIP_PLACE_FWD_COUNT,
                                                            dZAxisPreDownPos,
                                                            //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT),
                                                            ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                            true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        if (!IS_GD_FIRST_PLACE) IS_GD_FIRST_PLACE = true;

                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                    }

                    break;

                case 6:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_FWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceDownPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT));

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                        // BLOW BIT ON
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, true);
                    }

                    break;

                //case 5:
                //    break;

                //case 6:
                //    // delay
                //    //if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY].nValue)) return FNC.BUSY;
                //    break;

                case 7:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    break;

                case 8:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_FWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT));
                    break;

                case 10:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;

                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, false);

                        switch (CHIP_PICKER_HEAD_NO)
                        {
                            case 0:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_1, false);
                                break;
                            case 1:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_2, false);
                                break;
                            default:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.FORWARD_PLACE_GOOD_TRAY_PICKER_1, false);
                                break;
                        }
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

        public int PlaceToGoodTrayR()
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
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            if (!GbVar.Seq.sUldGDTrayTable[CHIP_PICKER_PLACE_GOOD_TABLE_NO].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;
                case 1:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    int nEquCnt = 0;
                    for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count; i++)
                    {
                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker[i] == CHIP_PLACE_GET_BWD_COUNT)
                        {
                            nEquCnt++;
                        }
                    }

                    if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count == 0)
                    {
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_GET_BWD_COUNT);
                    }
                    else
                    {
                        if (nEquCnt == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_GET_BWD_COUNT);
                        }
                    }
                    switch (CHIP_PICKER_HEAD_NO)
                    {
                        case 0:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1, true);
                            break;
                        case 1:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_2, true);
                            break;
                        default:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1, true);
                            break;
                    }
                    //TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1 + CHIP_PICKER_HEAD_NO, true);

                    break;

                case 2:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT) == FNC.SUCCESS) break;
                    }

                    nFuncResult = MovePosChipPlaceToGoodXY(CHIP_PICKER_HEAD_NO,
                                                                            false,
                                                                            CHIP_PLACE_GET_BWD_COUNT,
                                                                            0,
                                                                            //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                                            ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                                            CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                                            CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                                            true);
                    break;
                case 3:
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT);
                    }
                    break;

                //case 3:
                //    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_PAD_PRE_TURN_USE].bOptionUse &&
                //        CHIP_PLACE_SET_BWD_COUNT + CHIP_PLACE_FWD_COUNT == 0)
                //    {

                //    }
                //    break;

                case 5:
                    double dZAxisPreDownPos = ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT);
                    if (!IS_GD_FIRST_PLACE)
	                {
                        dZAxisPreDownPos = 0.0f;
	                }

                    nFuncResult = MovePosChipPlaceToGoodXYZT(CHIP_PICKER_HEAD_NO,
                                                                        false,
                                                                        CHIP_PLACE_GET_BWD_COUNT,
                                                                        dZAxisPreDownPos,
                                                                        //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                                        ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                                        CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                                        CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                                        true);

                    

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        if (!IS_GD_FIRST_PLACE) IS_GD_FIRST_PLACE = true;

                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                    }

                    break;

                case 6:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceDownPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                        // BLOW BIT ON
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);
                    }

                    break;

                //case 5:
                //    // Vac

                //    //m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                //    //// VAC BIT OFF
                //    ////MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                //    //// BLOW BIT ON
                //    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);

                //    break;
                //case 6:
                //    // delay
                //    //if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY].nValue)) return FNC.BUSY;
                //    break;
                case 7:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    break;
                case 8:
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));
                    break;
                case 10:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;

                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, false);

                        switch (CHIP_PICKER_HEAD_NO)
                        {
                            case 0:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1, false);
                                break;
                            case 1:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_2, false);
                                break;
                            default:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1, false);
                                break;
                        }
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.REVERSE_PLACE_GOOD_TRAY_PICKER_1 + CHIP_PICKER_HEAD_NO, false);

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

        public int PlaceToReworkTray()
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
                // 순서 변경
                case 0:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            if (!GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                    }
                    break;
                case 1:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    int nEquCnt = 0;
                    for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count; i++)
                    {
                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker[i] == CHIP_PLACE_GET_BWD_COUNT)
                        {
                            nEquCnt++;
                        }

                    }

                    if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Count == 0)
                    {
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_GET_BWD_COUNT);
                    }
                    else
                    {
                        if (nEquCnt == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Add(CHIP_PLACE_GET_BWD_COUNT);
                        }
                    }
                    switch (CHIP_PICKER_HEAD_NO)
                    {
                        case 0:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1, true);
                            break;
                        case 1:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_2, true);
                            break;
                        default:
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1, true);
                            break;
                    }
                    //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1 + CHIP_PICKER_HEAD_NO, true);
                    break;

                case 2:
                    if (m_bFirstSeqStep)
                    {
                        if (IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT) == FNC.SUCCESS) break;
                    }

                    nFuncResult = MovePosChipPlaceToReworkXY(CHIP_PICKER_HEAD_NO,
                                                                  CHIP_PLACE_GET_BWD_COUNT,
                                                                  0,
                                                                  //ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                                  //ConfigMgr.Inst.GetPlaceAngleOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                                  ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                                  CHIP_PICKER_PLACE_REWORK_TABLE_COUNT,
                                                                  ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                                  true);
                    break;
                case 3:
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT);
                    }
                    break;

                case 5:
                    double dZAxisPreDownPos = ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT);
                    if (!IS_RW_FIRST_PLACE)
	                {
                        dZAxisPreDownPos = 0.0f;
	                }

                    nFuncResult = MovePosChipPlaceToReworkXYZT(CHIP_PICKER_HEAD_NO,
                                                              CHIP_PLACE_GET_BWD_COUNT,
                                                              dZAxisPreDownPos,
                                                              //ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                              //ConfigMgr.Inst.GetPlaceAngleOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                              ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                              CHIP_PICKER_PLACE_REWORK_TABLE_COUNT,
                                                              ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                              true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        if (!IS_RW_FIRST_PLACE) IS_RW_FIRST_PLACE = true;

                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                    }

                    break;
                case 6:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceDownPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        //블로우를 안한다고해서 혹시나해서 추가
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                        // BLOW BIT ON
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);
                    }

                    break;
                
                //case 5:
                //    // Picker  PreDown Move
                //    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF,
                //                                 CHIP_PICKER_HEAD_NO,
                //                                 CHIP_PLACE_GET_BWD_COUNT,
                //                                 ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));
                //    break;


                //case 6:
                //    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                //    // VAC BIT OFF
                //    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                //    // BLOW BIT ON
                //    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);
                //    break;

                case 7:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, false);
                    break;
                // [2022.05.09.kmlee] VACUUM을 풀고 올라가야함
                case 8:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CFG_DF.TRAY_STG_RWK, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));
                    break;

                case 10:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;

                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, false);
                        switch (CHIP_PICKER_HEAD_NO)
                        {
                            case 0:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1, false);
                                break;
                            case 1:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_2, false);
                                break;
                            default:
                                TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1, false);
                                break;
                        }
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REWORK_TRAY_PICKER_1 + CHIP_PICKER_HEAD_NO, false);

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

        public int PlaceToRejectBox()
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
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    break;

                case 1:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
						
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REJECT_BOX_PICKER_1 + CHIP_PICKER_HEAD_NO, true);
                        
                    }
                    break;

                case 2:
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);
                    }
                    break;

                case 3:
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ

                    //int nBinNum = GetUnitRejectCode(CHIP_PLACE_FWD_COUNT);
                    int nBinNum = 0;
                    //nFuncResult = MovePosChipPlaceToBinBoxXYZ(CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT, nBinNum, 0, true);
                    nFuncResult = MovePosChipPlaceToBinBoxXY(CHIP_PICKER_HEAD_NO, nBinNum, 0, true);
                    break;

                case 4:
                    // Picker Down
                    // nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT, 0.0f);
                    break;
                case 5:
                    // Blow

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // VAC BIT OFF
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                    // BLOW BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, true);
                    break;
                case 6:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_ON_DELAY].nValue)) return FNC.BUSY;

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // BLOW BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, false);
                    break;
                case 7:
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    break;

                case 10:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_REJECT_BOX_PICKER_1 + CHIP_PICKER_HEAD_NO, false);

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

        #region Pk_Inspection

        public int NewBtmInspection()
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
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }

                            CHIP_PICKER_INSPECTION_COUNT = 0;
                        }
                    }
                    break;
                case 1:
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse)
                        {
                            SeqHistory("{0} BALL INSPECTION SKIP", m_strHead);
                            return FNC.SUCCESS;
                        }

                        if (!IFMgr.Inst.VISION.IsVisionReady)
                        {
                            return (int)ERDF.E_VISION_PROGRAM_IS_NOT_READY;
                        }

                        SeqHistory("{0} BALL INSPECTION READY CHECK", m_strHead);
                    }
                    break;
                case 2:
                    {
                        IFMgr.Inst.VISION.SetBallHeadNo((MCDF.eUNIT)CHIP_PICKER_HEAD_NO);
                        IFMgr.Inst.VISION.SetBallInspCountReset(true);

                        SeqHistory("{0} BALL INSPECTION INIT REQUEST COMPLETE", m_strHead);
                    }
                    break;
                case 3:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsBallCountReset) return FNC.BUSY;
                        // [2022.04.13.kmlee] 바로 끄지않고 Complete 주기 전 Req OFF
                        //IFMgr.Inst.VISION.SetBallInspCountReset(false);

                        SeqHistory("{0} BALL INSPECTION INIT REPLY CHECK COMPLETE", m_strHead);
                    }
                    break;
                case 4:
                    {
                        // Inspection ready pos move
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, CHIP_PICKER_HEAD_NO);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} MOVE ALL Z AXIS TO VISION REFERENCE POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));
                        }
                    }
                    break;
                case 5:
                    {
                        nFuncResult = MovePosBallVisionXYZ(POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + CHIP_PICKER_HEAD_NO, CHIP_PICKER_HEAD_NO);
					 
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_strLogMsg = string.Format("{0} MOVE ALL XYZ AXIS TO VISION POSITION COMPLETE", m_strHead);
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", m_strLogMsg, STEP_ELAPSED));

                            NextSeq(10);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 10:
                    {
                        if (CHIP_PICKER_INSPECTION_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                        {
                            SeqHistory("{0} BOTTOM INSPECTION COMPLETE [{1} >= {2}]",
                                m_strHead, CHIP_PICKER_INSPECTION_COUNT, CFG_DF.MAX_PICKER_PAD_CNT);

                            NextSeq(20);
                            return FNC.BUSY;
                        }
                    }
                    break;

                case 11:
                    nFuncResult = MovePosBallInspXYNext(CHIP_PICKER_INSPECTION_COUNT,
                                                        CHIP_PICKER_HEAD_NO,
                                                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY].lValue,
                                                        true);
                    break;
                case 12:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BALL_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;
                case 13:
                    {
                        IFMgr.Inst.VISION.TrgBallOneShot();

                        CHIP_PICKER_INSPECTION_COUNT++;
                        NextSeq(10);
                        return FNC.BUSY;
                    }
                //break;

                case 20:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SeqHistory("{0} BOTTOM INSPECTION TIMEOUT ERROR OCCURED [INSPECTION COUNT: {1}]",
                                m_strHead, CHIP_PICKER_INSPECTION_COUNT);
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_ON;
                        }

                        if (!IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                        IFMgr.Inst.VISION.SetBallInspCountComplete(true);

                        SeqHistory("{0} BOTTOM INSPECTION VISION REPLY ON CHECK DONE [INSPECTION COUNT: {1}]",
                            m_strHead, CHIP_PICKER_INSPECTION_COUNT);
                    }
                    break;
                case 21:
                    {
                        if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SeqHistory("{0} BOTTOM INSPECTION TIMEOUT ERROR OCCURED [INSPECTION COUNT: {1}]",
                                m_strHead, CHIP_PICKER_INSPECTION_COUNT);
                            return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_OFF;
                        }

                        if (IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                        // [2022.04.28.kmlee] Complete 주고나서 Reset OFF
                        IFMgr.Inst.VISION.SetBallInspCountReset(false);
                        IFMgr.Inst.VISION.SetBallInspCountComplete(false);

                        SeqHistory("{0} BOTTOM INSPECTION VISION REPLY OFF CHECK DONE [INSPECTION COUNT: {1}]",
                            m_strHead, CHIP_PICKER_INSPECTION_COUNT);
                    }
                    break;

                case 22:
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

        public int BtmInspection()
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
                    //
                    break;

                case 1:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.BTM_INSP_PICKER_1 + CHIP_PICKER_HEAD_NO, true);
                        
                    }
                    break;

                case 2:
                    // All Z Axis, Head X Axis, Vision Z Axis, Vision Y Axis Ball Vision Ready Pos Move
                    if (!m_bAutoMode) // && !m_bIsOneCycle)
                    {
                        nFuncResult = MovePosChipPkInspectionReady(CHIP_PICKER_HEAD_NO);
                    }
                    else
                    {
                        //nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CHIP_PICKER_INSPECTION_COUNT);

                        //2024-01-16 sj.shin 칭허 소스에 적용 되어져있음
                        nFuncResult = IsInPosChipPkR(CHIP_PICKER_HEAD_NO, CFG_DF.MAX_PICKER_PAD_CNT - 1 - CHIP_PICKER_INSPECTION_COUNT); 
                    }
                    break;

                case 3:
                    //nFuncResult = MovePosBallInspXYNext(CHIP_PICKER_INSPECTION_COUNT, 
                    //                                    CHIP_PICKER_HEAD_NO,
                    //                                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY].lValue,
                    //                                    true);
                    //2024-01-16 sj.shin 칭허 소스에 적용 되어져있음
                    nFuncResult = MovePosBallInspXYNext(CFG_DF.MAX_PICKER_PAD_CNT - 1 - CHIP_PICKER_INSPECTION_COUNT,
                                              CHIP_PICKER_HEAD_NO,
                                              ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_INSPECTION_MOTION_DELAY].lValue,
                                              true);

                    break;

                case 4:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BALL_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 5:
                    break;

                case 6:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        
						//TTDF.SetTact((int)TTDF.CYCLE_NAME.BTM_INSP_PICKER_1 + CHIP_PICKER_HEAD_NO, false);
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

        #region Pk_PickUp OneCycle
        public int UnitPickUpOneCycle()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            switch (m_nSeqNo)
            {
                case 0:
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    CHIP_PICK_UP_FWD_COUNT = 0;
                    CHIP_PICK_UP_SET_BWD_COUNT = 0;
                    CHIP_PICKER_PICK_UP_COL_COUNT = 0;

                    //m_bIsOneCycle = true;
                    IS_CHIP_PICK_UP_RVS = false;

                    m_nPickUpRetryCnt = 0;
                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Clear();

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
                        
						//TTDF.SetTact((int)TTDF.CYCLE_NAME.PICK_UP_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, true);

                    }
                    break;

                case 10:
                    // R Axis = 0
                    // Z Axis = 0
                    nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1, CHIP_PICKER_HEAD_NO);
                    break;

                case 20:
                    if (IS_CHIP_PICK_UP_RVS)
                    {
                        NextSeq(71);
                        return FNC.BUSY;
                    }
                    break;

                case 30:
                    //if (CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 && CHIP_PICK_UP_FWD_COUNT != 0 &&
                    //    CHIP_PICKER_PICK_UP_TABLE_COUNT % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX == 0)
                    
                    if (CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 && CHIP_PICKER_PICK_UP_COL_COUNT != 0 &&
                        CHIP_PICKER_PICK_UP_TABLE_COUNT % nTotMapCountX == 0)
                    {
                        CHIP_PICKER_PICK_UP_COL_COUNT = 0;
                        IS_CHIP_PICK_UP_RVS = true;

                        NextSeq(20);
                        return FNC.BUSY;
                    }
                    break;

                case 31:
                    if ((nTotMapCountX * nTotMapCountY) <= CHIP_PICKER_PICK_UP_TABLE_COUNT)
                    {
                        // OneCycle 종료...
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    if ((CHIP_PICK_UP_FWD_COUNT + CHIP_PICK_UP_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 34:
                    // Skip
                    if (IsPickerPadSkip(CHIP_PICK_UP_FWD_COUNT))
                    {
                        CHIP_PICK_UP_FWD_COUNT++;
                        NextSeq(32);
                        return FNC.BUSY;
                    }
                    break;

                case 36:
                    if (IsMapUnit(true, CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_PICK_UP_TABLE_COUNT, CHIP_PICKER_PICK_UP_COL_COUNT))
                    {
                        CHIP_PICKER_PICK_UP_COL_COUNT++;

                        //if (CHIP_PICKER_PICK_UP_COL_COUNT >= CFG_DF.MAX_PICKER_PAD_CNT)
                        //{
                        //    NextSeq(100);
                        //    return FNC.BUSY;
                        //}

                        NextSeq(30);
                        return FNC.BUSY;
                    }

                    int nEquCnt = 0;
                    for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count; i++)
                    {
                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker[i] == CHIP_PICK_UP_FWD_COUNT)
                        {
                            nEquCnt++;
                        }

                    }

                    if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count == 0)
                    {
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                    }
                    else
                    {
                        if (nEquCnt == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                        }
                    }
                    break;

                case 40:
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ

                    nFuncResult = MovePosChipPickUpXYZT(CHIP_PICKER_HEAD_NO,
                        	                            true,
                            	                        CHIP_PICK_UP_FWD_COUNT,
                                	                    CHIP_PICKER_PICK_UP_COL_COUNT,
                                    	                ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT),
                                        	            CHIP_PICKER_PICK_UP_TABLE_NO,
                                            	        CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                	    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                    	true);
                    break;

                

                case 42:
                    // Vac

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // BLOW BIT OFF
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_FWD_COUNT), 3, false);
                    // VAC BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_FWD_COUNT), 2, true);

                    break;

                case 44:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_FWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPickUpDownPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT));
                    break;

                case 46:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY].nValue)) return FNC.BUSY;

                    break;

                case 48:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_FWD_COUNT,
                                                 0.0f);
                    break;



                case 50:
                    // Picker Vacuum Check
                    // Vacuum Error == true;
                    //IsVacOn(CHIP_PICK_UP_HEAD_NO, CHIP_PICK_UP_FWD_COUNT);
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse || !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse)
                    {
                        NextSeq(58);
                        return FNC.BUSY;
                    }

                    if (IsVacOn(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_FWD_COUNT))
                    {
                        NextSeq(58);
                        return FNC.BUSY;
                    }

                    //if (m_nPickUpTryCnt >= ConfigMgr.Inst.Cfg.itemOptions[].nValue)
                    {
                        //NextSeq(56);
                        //return FNC.BUSY;
                    }

                    //m_nPickUpTryCnt++;
                    //NextSeq(40);
                    //return FNC.BUSY;
                    break;

                case 56:
                    // Vacuum  Error  -> Alarm
                    int nHeadOffset = (int)ERDF.E_X2_PK_VAC_1_NOT_ON - (int)ERDF.E_X1_PK_VAC_1_NOT_ON;

                    nFuncResult = (int)ERDF.E_X1_PK_VAC_1_NOT_ON + (nHeadOffset * CHIP_PICKER_HEAD_NO) + (2 * CHIP_PICK_UP_FWD_COUNT);
                    break;

                case 58:

                    SetMapUnit(true,
                               CHIP_PICKER_PICK_UP_TABLE_NO,
                               CHIP_PICKER_PICK_UP_TABLE_COUNT,
                               false);

                    CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                    CHIP_PICK_UP_FWD_COUNT++;
                    CHIP_PICKER_PICK_UP_COL_COUNT++;

                    NextSeq(30);
                    return FNC.BUSY;

                case 60:
                    if (!IS_CHIP_PICK_UP_RVS)
                    {
                        NextSeq(31);
                        return FNC.BUSY;
                    }
                    break;

                case 70:
                    //if (CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 &&
                    //    CHIP_PICKER_PICK_UP_TABLE_COUNT % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX == 0)
                    //{
                    if (CHIP_PICKER_PICK_UP_TABLE_COUNT != 0 &&
                        CHIP_PICKER_PICK_UP_TABLE_COUNT % nTotMapCountX == 0)
                    {
                        CHIP_PICKER_PICK_UP_COL_COUNT = 0;
                        IS_CHIP_PICK_UP_RVS = false;

                        NextSeq(60);
                        return FNC.BUSY;
                    }
                    break;

                case 71:
                    if ((nTotMapCountX * nTotMapCountY) <= CHIP_PICKER_PICK_UP_TABLE_COUNT)
                    {
                        // OneCycle 종료...
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 72:
                    if ((CHIP_PICK_UP_FWD_COUNT + CHIP_PICK_UP_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 74:
                    // Skip
                    if (IsPickerPadSkip(CHIP_PICK_UP_GET_BWD_COUNT))
                    {
                        CHIP_PICK_UP_SET_BWD_COUNT++;
                        NextSeq(72);
                        return FNC.BUSY;
                    }
                    break;

                case 76:
                    if (IsMapUnit(false, CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_PICK_UP_TABLE_COUNT, CHIP_PICKER_PICK_UP_COL_COUNT))
                    {
                        CHIP_PICKER_PICK_UP_COL_COUNT++;

                        NextSeq(70);
                        return FNC.BUSY;
                    }
                    break;

                case 80:

                    nFuncResult = MovePosChipPickUpXYZT(CHIP_PICKER_HEAD_NO,
                        	                            false,
                            	                        CHIP_PICK_UP_GET_BWD_COUNT,
                                	                    CHIP_PICKER_PICK_UP_COL_COUNT,
                                    	                ConfigMgr.Inst.GetPickerPickUpWaitPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT),
                                        	            CHIP_PICKER_PICK_UP_TABLE_NO,
                                            	        CHIP_PICKER_PICK_UP_TABLE_COUNT,
                                                	    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_MOTION_DELAY].lValue,
                                                    	true);
                    break;

                case 82:
                    // Vac

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    // BLOW BIT OFF
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_GET_BWD_COUNT), 3, false);
                    // VAC BIT ON
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PICK_UP_GET_BWD_COUNT), 2, true);

                    break;

                case 84:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPickUpDownPosOffset(CHIP_PICKER_PICK_UP_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT));
                    break;

                case 86:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PICK_UP_DELAY].nValue)) return FNC.BUSY;

                    break;

                case 88:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + CHIP_PICKER_PICK_UP_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PICK_UP_GET_BWD_COUNT,
                                                 0.0f);
                    break;



                case 90:
                    // Picker Vacuum Check
                    // Vacuum Error == true;
                    //IsVacOn(CHIP_PICK_UP_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT);
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse || !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse)
                    {
                        NextSeq(98);
                        return FNC.BUSY;
                    }

                    if (IsVacOn(CHIP_PICKER_HEAD_NO, CHIP_PICK_UP_GET_BWD_COUNT))
                    {
                        NextSeq(98);
                        return FNC.BUSY;

                    }

                    //if (m_nPickUpTryCnt >= ConfigMgr.Inst.Cfg.itemOptions[].nValue)
                    {
                        //NextSeq(96);
                        //return FNC.BUSY;
                    }

                    //m_nPickUpTryCnt++;
                    //NextSeq(80);
                    //return FNC.BUSY;
                    break;

                case 92:
                    // Vacuum Error?
                    // Picker Up
                    break;

                case 96:
                    // Vacuum  Error  -> Alarm
                    nHeadOffset = (int)ERDF.E_X2_PK_VAC_1_NOT_ON - (int)ERDF.E_X1_PK_VAC_1_NOT_ON;
                    nFuncResult = (int)ERDF.E_X1_PK_VAC_1_NOT_ON + (nHeadOffset * CHIP_PICKER_HEAD_NO) + (2 * CHIP_PICK_UP_GET_BWD_COUNT);
                    break;

                case 98:

                    SetMapUnit(false,
                               CHIP_PICKER_PICK_UP_TABLE_NO,
                               CHIP_PICKER_PICK_UP_TABLE_COUNT,
                               false);

                    CHIP_PICKER_PICK_UP_TABLE_COUNT++;
                    CHIP_PICK_UP_SET_BWD_COUNT++;
                    CHIP_PICKER_PICK_UP_COL_COUNT++;

                    NextSeq(70);
                    return FNC.BUSY;

                case 100:
                    break;

                case 110:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        //m_bIsOneCycle = false;
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PICK_UP_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, false);
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

        #region Pk_Place OneCycle
        public int UnitPlaceOneCycle()
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
                    // 만약 PickUp을 하던 도중 설비가 멈췄을 경우 PickUp Count를 저장했다가 사용해야 함.
                    // 정방향인지 역방향인지 포함, MapTableCount
                    CHIP_PLACE_FWD_COUNT = 0;
                    CHIP_PLACE_SET_BWD_COUNT = 0;

                    //m_bIsOneCycle = true;
                    IS_CHIP_PLACE_RVS = true;

                    SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPlacePicker.Clear();

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

                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, true);
                    }
                    break;

                case 10:
                    // R Axis = 0
                    // Z Axis = 0
                    nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1, CHIP_PICKER_HEAD_NO);
                    break;

                case 20:
                    if (!IS_CHIP_PLACE_RVS)
                    {
                        NextSeq(81);
                        return FNC.BUSY;
                    }
                    break;

                case 30:
                    //if (CHIP_PICKER_PLACE_GOOD_TABLE_COUNT != 0 && CHIP_PICKER_PLACE_GOOD_TABLE_COUNT % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX == 0)
                    //{
                    //    IS_CHIP_PLACE_RVS = false;

                    //    NextSeq(20);
                    //    return FNC.BUSY;
                    //}
                    break;

                case 31:
                    if ((RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX * RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY) <= CHIP_PICKER_PLACE_GOOD_TABLE_COUNT)
                    {
                        // OneCycle 종료...
                        NextSeq(110);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    if ((CHIP_PLACE_FWD_COUNT + CHIP_PLACE_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 34:
                    // Skip
                    if (IsPickerPadSkip(false, CFG_DF.HEAD_1 + CHIP_PICKER_HEAD_NO, CHIP_PLACE_SET_BWD_COUNT))
                    {
                        CHIP_PLACE_SET_BWD_COUNT++;
                        NextSeq(34);
                        return FNC.BUSY;
                    }
                    break;

                case 36:
                    //if (IsGoodTrayUnit(false, CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, CHIP_PICKER_PLACE_GOOD_COL_COUNT))
                    //{
                    //    CHIP_PICKER_PLACE_GOOD_COL_COUNT++;

                    //    NextSeq(30);
                    //    return FNC.BUSY;
                    //}

                    int nEquCnt = 0;
                    for (int i = 0; i < SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count; i++)
                    {
                        if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker[i] == CHIP_PICK_UP_FWD_COUNT)
                        {
                            nEquCnt++;
                        }

                    }

                    if (SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Count == 0)
                    {
                        SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                    }
                    else
                    {
                        if (nEquCnt == 0)
                        {
                            SEQ_INFO_CURRENT.pInfo[CHIP_PICKER_HEAD_NO].lstPickUpPicker.Add(CHIP_PICK_UP_FWD_COUNT);
                        }
                    }
                    break;


                case 40:
                    nFuncResult = MovePosChipPlaceToGoodXYZT(CHIP_PICKER_HEAD_NO,
                                                            false,
                                                            CHIP_PLACE_GET_BWD_COUNT,
                                                            ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                            //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT),
                                                            ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                            true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                    }
                    break;


                case 42:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceDownPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_GET_BWD_COUNT));

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                        // BLOW BIT ON
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);
                    }

                    break;

                case 44:
                    // Vac

                    //m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    //// VAC BIT OFF
                    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 2, false);
                    //// BLOW BIT ON
                    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, true);

                    break;

                case 45:
                    // delay
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_GET_BWD_COUNT), 3, false);

                    break;

                case 46:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_GET_BWD_COUNT,
                                                 0.0f);
                    break;

                
                case 48:

                    SetMapUnit(false, CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, true);

                    CHIP_PICKER_PLACE_GOOD_TABLE_COUNT++;
                    CHIP_PLACE_SET_BWD_COUNT++;

                    NextSeq(30);
                    return FNC.BUSY;

                case 70:
                    if (IS_CHIP_PLACE_RVS)
                    {
                        NextSeq(31);
                        return FNC.BUSY;
                    }
                    break;

                case 80:
                    //if (CHIP_PICKER_PLACE_GOOD_TABLE_COUNT != 0 &&
                    //    CHIP_PICKER_PLACE_GOOD_TABLE_COUNT % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX == 0 &&
                    //    CHIP_PICKER_PLACE_GOOD_COL_COUNT != 0)
                    //{
                    //    CHIP_PICKER_PLACE_GOOD_COL_COUNT = 0;
                    //    IS_CHIP_PLACE_RVS = true;

                    //    NextSeq(70);
                    //    return FNC.BUSY;
                    //}
                    break;

                case 81:
                    if ((RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX * RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY) <= CHIP_PICKER_PLACE_GOOD_TABLE_COUNT)
                    {
                        // OneCycle 종료...
                        NextSeq(110);
                        return FNC.BUSY;
                    }
                    break;

                case 82:
                    if ((CHIP_PLACE_FWD_COUNT + CHIP_PLACE_SET_BWD_COUNT) >= CFG_DF.MAX_PICKER_PAD_CNT)
                    {
                        NextSeq(100);
                        return FNC.BUSY;
                    }
                    break;

                case 84:
                    // Skip
                    if (IsPickerPadSkip(true, CFG_DF.HEAD_1 + CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT))
                    {
                        CHIP_PLACE_FWD_COUNT++;
                        NextSeq(84);
                        return FNC.BUSY;
                    }
                    break;

                case 86:
                    //if (IsGoodTrayUnit(true, CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_PLACE_GOOD_TABLE_COUNT, CHIP_PICKER_PLACE_GOOD_COL_COUNT))
                    //{
                    //    CHIP_PICKER_PLACE_GOOD_COL_COUNT++;

                    //    NextSeq(81);
                    //    return FNC.BUSY;
                    //}
                    break;

                case 90:
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ
                    nFuncResult = MovePosChipPlaceToGoodXYZT(CHIP_PICKER_HEAD_NO,
                                                            true,
                                                            CHIP_PLACE_FWD_COUNT,
                                                            ConfigMgr.Inst.GetPickerPlaceWaitPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT),
                                                            //ConfigMgr.Inst.GetPlaceAngleOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT),
                                                            ConfigMgr.Inst.GetPlaceAngleOffset(),
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                            CHIP_PICKER_PLACE_GOOD_TABLE_COUNT,
                                                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_MOTION_DELAY].lValue,
                                                            true);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                    }
                    break;

                case 92:
                    // Picker Down
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_FWD_COUNT,
                                                 ConfigMgr.Inst.GetPickerPlaceDownPosOffset(CHIP_PICKER_PLACE_GOOD_TABLE_NO, CHIP_PICKER_HEAD_NO, CHIP_PLACE_FWD_COUNT));

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                        // VAC BIT OFF
                        //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                        // BLOW BIT ON
                        MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, true);
                    }
                    break;

                case 94:
                    // Vac

                    //m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    //// VAC BIT OFF
                    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 2, false);
                    //// BLOW BIT ON
                    //MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, true);

                    break;

                case 95:
                    //dealy
                    if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_OFF_DELAY].nValue)) return FNC.BUSY;

                    m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * CHIP_PICKER_HEAD_NO;
                    MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * CHIP_PLACE_FWD_COUNT), 3, false);

                    break;

                case 96:
                    // Picker  PreDown Move
                    nFuncResult = MovePosChipPkZ(POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + CHIP_PICKER_PLACE_GOOD_TABLE_NO,
                                                 CHIP_PICKER_HEAD_NO,
                                                 CHIP_PLACE_FWD_COUNT,
                                                 0.0f);
                    break;

                
                case 98:

                    CHIP_PICKER_PLACE_GOOD_TABLE_COUNT++;
                    CHIP_PLACE_FWD_COUNT++;

                    NextSeq(80);
                    return FNC.BUSY;

                

                case 100:
                    // Z Axis  All Up
                    break;

                case 110:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        //m_bIsOneCycle = false;
                        //TTDF.SetTact((int)TTDF.CYCLE_NAME.PLACE_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, false);
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

        #region Ball_Vision
        public int InspBallVisionOneCycle()
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
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY;
                    }
                    if (!IFMgr.Inst.VISION.IsVisionReady) return FNC.BUSY;

                    CHIP_PICKER_INSPECTION_COUNT = 0;
                    //m_bIsOneCycle = true;

                    IFMgr.Inst.VISION.SetBallHeadNo(MCDF.eUNIT.NO1 + CHIP_PICKER_HEAD_NO);
                    IFMgr.Inst.VISION.SetBallInspCountReset(true);
                    
					//TTDF.SetTact((int)TTDF.CYCLE_NAME.BALL_VISION_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, true);
                    break;

                case 1:
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_ON;
                    }

                    if (!IFMgr.Inst.VISION.IsBallCountReset) return FNC.BUSY;
                    //IFMgr.Inst.VISION.SetBallInspCountReset(false);
                    break;

                case 2:
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                    //    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    //{
                    //    return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_RESET_OFF;
                    //}
                    //if (IFMgr.Inst.VISION.IsBallCountReset) return FNC.BUSY;

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
                    // IsBtmVisionReady
                    

                    break;

                case 11:
                    // R Axis = 0
                    // Z Axis = 0
                    nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, CHIP_PICKER_HEAD_NO);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER ZT AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 12:
                    //nFuncResult = MovePosBallVisionYZ(POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + CHIP_PICKER_HEAD_NO);

                    //if (FNC.IsSuccess(nFuncResult))
                    //{
                    //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "BALL VISION YZ AXIS INSPECTION POSITION COMPLETE", STEP_ELAPSED));
                    //}
                    break;

                case 20:
                    // Inspection 시작위치 이동
                    // TableY, MapPickerX, MapPickerZ
                    nFuncResult = MovePosChipPkInspectionReady(CHIP_PICKER_HEAD_NO);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER Z AXIS INSPECTION POSITION COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 22:
                    //nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, CHIP_PICKER_HEAD_NO);

                    //if (FNC.IsSuccess(nFuncResult))
                    //{
                    //    SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER ZT AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                    //}
                    break;

                case 30:
                    // Place  Count 10 Over? nInspCount >= MAX_COUNT
                    // NextSeq(50); return;
                    if (CFG_DF.MAX_PICKER_PAD_CNT <= CHIP_PICKER_INSPECTION_COUNT)
                    {
                        NextSeq(50);
                        return FNC.BUSY;
                    }
                    break;

                case 32:
                    if (IsPickerPadSkip(CHIP_PLACE_FWD_COUNT))
                    {
                        CHIP_PICKER_INSPECTION_COUNT++;
                        NextSeq(30);
                        return FNC.BUSY;
                    }
                    break;

                case 40:
                    // X,Y,Z Move....  SEQ_INFO_CURRENT.nFwdPickupCount
                    // X = Pitch + Picker OffsetX (역방향 Check이면 MapTable Col Count 역순)
                    // Y = Picker OffsetY
                    // Z = PreDownPos + PickerOffsetZ
                    // Picker Pitch 이동
                    nFuncResult = MovePosBallInspXYNext(CHIP_PICKER_INSPECTION_COUNT, CHIP_PICKER_HEAD_NO);
                    break;

                case 42:
                    // GRAB DELAY
                    {
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BALL_VISION_GRAB_DELAY].lValue;
                        if (FNC.IsBusy(RunLib.msecDelay(lDelay)))
                            return FNC.BUSY;
                    }
                    break;

                case 44:
                    // Trigger Shot
                    IFMgr.Inst.VISION.TrgBallOneShot();
                    break;

                case 46:
                    // TriggerShot
                    //IFMgr.Inst.VISION.TrgBallOneShot();
                    break;

                case 48:
                    CHIP_PICKER_INSPECTION_COUNT++;
                    NextSeq(30);
                    return FNC.BUSY;


                case 50:
                    nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_READY, CHIP_PICKER_HEAD_NO);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER ZT AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                    }
                    break;

                case 52:
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_ON;
                    }
                    if (!IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                    IFMgr.Inst.VISION.SetBallInspCountComplete(true);
                    break;

                case 53:
                    if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                    {
                        return (int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_BALL_INSP_COMP_OFF;
                    }
                    if (IFMgr.Inst.VISION.IsBallInspCompleted) return FNC.BUSY;
                    IFMgr.Inst.VISION.SetBallInspCountReset(false);
                    IFMgr.Inst.VISION.SetBallInspCountComplete(false);
                    break;

                case 54:
                    IFMgr.Inst.VISION.GetBallInspResult(CHIP_PICKER_HEAD_NO);
                    break;

                case 60:
                    {
                        //string strStripInfo = string.Format("{0} {1}",
                        //    "CASSETE NO : " + SEQ_INFO_CURRENT.Info.MAGAZINE_SLOT_NO,
                        //    "STRIP UINT ID : " + SEQ_INFO_CURRENT.Info.STRIP_ID);
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}, {2}", "CLEANING FINISH", STEP_ELAPSED, strStripInfo));
                        //m_bIsOneCycle = false;
                        
						//TTDF.SetTact((int)TTDF.CYCLE_NAME.BALL_VISION_ONE_CYCLE_PICKER_1 + CHIP_PICKER_HEAD_NO, false);
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
                //m_bIsOneCycle = false;
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

        #region Unit Function

        public bool IsMapUnit(bool bFDir, int nMapTableNo, int nTablePickUpCount, int nTablePickUpColCount)
        {
            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTablePickUpCount / nTotMapCountX;
                nCol = nTablePickUpColCount % nTotMapCountX;
            }
            else
            {
                nRow = nTablePickUpCount / nTotMapCountX;
                nCol = nTotMapCountX - (CHIP_PICK_UP_SET_BWD_COUNT % nTotMapCountX) - 1;
            }

            return GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_UNIT;
        }

        public void SetMapUnit(bool bFDir, int nMapTableNo, int nTablePickUpCount, bool bSet)
        {
            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTablePickUpCount / nTotMapCountX;
                nCol = nTablePickUpCount % nTotMapCountX;
            }
            else
            {
                nRow = nTablePickUpCount / nTotMapCountX;
                nCol = nTotMapCountX - (nTablePickUpCount % nTotMapCountX) - 1;
            }

            GbVar.Seq.sMapVisionTable[nMapTableNo].Info.UnitArr[nRow][nCol].IS_UNIT = bSet;
        }

        public bool IsGoodTrayUnit(bool bFDir, int nTableNo, int nTablePlaceCount, int nTablePlaceColCount)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }
            else
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
            }

            return GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].IS_UNIT;
        }

        public void SetGoodTrayUnit(bool bFDir, int nTableNo, int nTablePlaceCount, int nTablePlaceColCount, bool bSet)
        {
            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            }
            else
            {
                nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            }

            GbVar.Seq.sUldGDTrayTable[nTableNo].Info.UnitArr[nRow][nCol].IS_UNIT = bSet;
        }

        public bool IsReworkTrayUnit(int nTablePlaceCount)
        {
            int nRow, nCol;

            nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            return GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT;
        }

        public void SetReworkTrayUnit(int nTablePlaceCount, int nTablePlaceColCount, bool bSet)
        {
            int nRow, nCol;

            nRow = nTablePlaceCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTablePlaceColCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT = bSet;
        }

        public bool IsPickerPadSkip(int nPickerNo)
        {
            if (pInfo[nPickerNo].IS_PICKER_SKIP) return true;

            return false;
        }

        public bool IsNotGoodUnit(int nPickerNo)
        {
            if (pInfo[nPickerNo].TOP_INSP_RESULT != 0) return true;

            return false;
        }

        public bool IsNotReworkUnit(int nPickerNo)
        {
            if (pInfo[nPickerNo].TOP_INSP_RESULT != 1) return true;

            return false;
        }

        public bool IsNotNGUnit(int nPickerNo)
        {
            if (pInfo[nPickerNo].TOP_INSP_RESULT > 1) return true;

            return false;
        }

        #endregion Unit Function
    }
}
