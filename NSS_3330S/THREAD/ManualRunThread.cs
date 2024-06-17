using NSS_3330S;
using NSS_3330S.MOTION;
using NSS_3330S.SEQ.MANUAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSS_3330S.THREAD
{
    public class ManualRunThread : IDisposable
    {
        public delegate void manCfgStopEvent(int nErrSafty);
        public event manCfgStopEvent errSaftyStopEvent;

        public delegate void DeleFinishSeq(int nResult);
        public event DeleFinishSeq OnFinishSeq;

        public delegate void procAddMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView);
        public event procAddMsgEvent procMsgEvent;

        public enum MANUAL_SEQ
        {
            NONE = 0,
            RUN_MODULE,
            FINISH,
        }

        Thread threadRun;

        public int nSeqNo = 0;

        MANUAL_SEQ manualSeq = MANUAL_SEQ.NONE;
        bool flagThreadAlive = false;

        int m_nResultNo = 0;

        // 모듈 별 수동 운전
        SeqBase m_ModuleSelect = null;
        MODULE_TYPE m_ModuleType = MODULE_TYPE.MOVE_TEACH_POS;

        // 전체 모듈
        SeqBase[] m_ModuleAll = new SeqBase[(int)MODULE_TYPE.MAX];

        public mnTeachPosMove mTeachPosMove = new mnTeachPosMove();
        public mnCfgPosMove mCfgPosMove = new mnCfgPosMove();

        public mnMachineAllHome mAllHome = new mnMachineAllHome();

        //public ManualCycleVacCyl mnCycleVacCyl = new ManualCycleVacCyl();
        public mnLoaderCycleRun mLdCycleRun = new mnLoaderCycleRun();
        public mnSorterCycleRun mSorterCycleRun = new mnSorterCycleRun();
        public mnSawCycleRun mSawCycleRun = new mnSawCycleRun();
        //public mnCleanerCycleRun mCleanCycleRun = new mnCleanerCycleRun();

        public mnMultiAxisHome mSingleHome = new mnMultiAxisHome();

        public mnVisionTableOffset mVisionTableOffset = new mnVisionTableOffset();
        public mnTrayTableOffset mTrayTableOffset = new mnTrayTableOffset();
        public mnPickerHeadOffset mPickerHeadOffset = new mnPickerHeadOffset();

        public mnVisionAlign mVisionAlign = new mnVisionAlign();
        public mnMapTableVisionView mMapVisionView = new mnMapTableVisionView();
        public mnTrayVisionView mTrayVisionView = new mnTrayVisionView();
        public mnVisionPreAlignInterface mVisionPreAlignInterface = new mnVisionPreAlignInterface();
        public mnVisionTopAlignInterface mVisionTopAlignInterface = new mnVisionTopAlignInterface();
        public mnVisionMapInspInterface mVisionMapInspInterface = new mnVisionMapInspInterface();
        public mnVisionBallInspInterface mVisionBallInspInterface = new mnVisionBallInspInterface();
        public MnMoveCenterTopAlignMark mMoveCenterTopAlignMark = new MnMoveCenterTopAlignMark();
        public MnPickerCalibration mPickerCal = new MnPickerCalibration();
        public MnPickerZeroSet_Pickup mPickerZeroSet_Pickup = new MnPickerZeroSet_Pickup();
        public mnPickerZeroSet_Place mPickerZeroSet_Place = new mnPickerZeroSet_Place();
        //public ManualInitCycle mnInitCycle = new ManualInitCycle();
        //public ManualPosNoMove mnPosNoMove = new ManualPosNoMove();

        ///////////////////////

        public MANUAL_SEQ CURRENT_SEQ
        {
            get { return manualSeq; }
        }

        public int RESULT
        {
            get { return m_nResultNo; }
        }

        public ManualRunThread()
        {
            m_ModuleAll[(int)MODULE_TYPE.MOVE_TEACH_POS] = mTeachPosMove;
            m_ModuleAll[(int)MODULE_TYPE.MOVE_CFG_POS] = mCfgPosMove;
            m_ModuleAll[(int)MODULE_TYPE.ALL_HOME] = mAllHome;
            m_ModuleAll[(int)MODULE_TYPE.SINGLE_HOME] = mSingleHome;

            //m_ModuleAll[(int)MODULE_TYPE.CYCLE_MANUAL_VAC_CYL] = mnCycleVacCyl;

            m_ModuleAll[(int)MODULE_TYPE.MANUAL_LD_CYCLE_RUN] = mLdCycleRun;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_SAW_CYCLE_RUN] = mSawCycleRun;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN] = mSorterCycleRun;

            m_ModuleAll[(int)MODULE_TYPE.MAP_VISION_VIEW_CYCLE_RUN] = mMapVisionView;
            m_ModuleAll[(int)MODULE_TYPE.TRAY_VISION_VIEW_CYCLE_RUN] = mTrayVisionView;
            //m_ModuleAll[(int)MODULE_TYPE.MANUAL_CLEANER_CYCLE_RUN] = mCleanCycleRun;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_PRE_ALIGN] = mVisionPreAlignInterface;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_TOP_ALIGN] = mVisionTopAlignInterface;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_MAP_INSPECTION] = mVisionMapInspInterface;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_BALL_INSPECTION] = mVisionBallInspInterface;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_MOVE_CENTER_TOP_ALIGN] = mMoveCenterTopAlignMark;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_PICKER_CALIBRATION] = mPickerCal;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_PICKER_ZEROSET_PICK_UP] = mPickerZeroSet_Pickup;
            m_ModuleAll[(int)MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE] = mPickerZeroSet_Place;

            foreach (SeqBase item in m_ModuleAll)
            {
                // 수동 운전은 반드시 해주어야 한다
                if (item != null)
                {
                    item.SetManualSeqMode();
                    item.SetErrorFunc(SetError);
                    item.SetAddMsgFunc(SetMsgEvent);
                }
            }

            StartStopManualThread(true);

            InitSeqNum();
        }

        #region COMMON FUNCTION
        private void SetError(int nErrNo)
        {
            //ErrorInfo eInfo = ErrMgr.Inst.Get(nErrNo);
            ErrorInfo eInfo = ErrMgr.Inst.errlist[nErrNo];
            m_nResultNo = nErrNo;

            // 중알람일 때만 정지
            if (eInfo.HEAVY)
            {
                LeaveCycle(nErrNo);
            }

            if (errSaftyStopEvent != null)
                errSaftyStopEvent(nErrNo);
        }

        private void SetMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView)
        {
            if (procMsgEvent != null)
                procMsgEvent(nProcNo, nSeqNo, nSeqSubNo, strAddMsg, bLogView);
        }

        public bool SetRunModule(MODULE_TYPE mType)
        {
            if (mType == MODULE_TYPE.MAX) return false;

            m_ModuleType = mType;
            m_ModuleSelect = m_ModuleAll[(int)mType];

            return true;
        }

        public MODULE_TYPE GetCurrentModuleType()
        {
            return m_ModuleType;
        }

        public void Dispose()
        {
            StartStopManualThread(false);
        }

        public bool StartManualProcRun(MANUAL_SEQ nProcNo, DeleFinishSeq deleFinSeq = null, int nSeqNo = 0)
        {
            if (manualSeq != MANUAL_SEQ.NONE)
                return false;

            if (deleFinSeq != null)
                OnFinishSeq = deleFinSeq;

            manualSeq = nProcNo;

            switch (manualSeq)
            {
                case MANUAL_SEQ.RUN_MODULE:
                    m_ModuleSelect.InitSeq(nSeqNo);
                    CheckStartStopRunModuleCondition(true);
                    break;
            }

            GbVar.mcState.isManualRun = true;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MANUAL_RUN] = true;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MANUAL_RUN] = true;

            return true;
        }

        void StartStopManualThread(bool bStart)
        {
            if (threadRun != null)
            {
                flagThreadAlive = false;
                threadRun.Join(500);
                threadRun.Abort();
                threadRun = null;
            }

            if (bStart)
            {
                flagThreadAlive = true;
                threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                threadRun.Name = "Manual run THREAD";
                if (threadRun.IsAlive == false)
                    threadRun.Start(this);
            }
        }

        public void InitSeqNum()
        {
            nSeqNo = 0;
            if (manualSeq != MANUAL_SEQ.NONE)
                manualSeq = MANUAL_SEQ.FINISH;

            foreach (SeqBase item in m_ModuleAll)
            {
                if (item != null)
                {
                    item.InitSeq();
                }
            }
        }

        void ThreadRun(object obj)
        {
            ManualRunThread manualRun = obj as ManualRunThread;

            while (manualRun.flagThreadAlive)
            {
                if (GbVar.mcState.isCycleErr[(int)SEQ_ID.MANUAL_RUN] &&
                    manualRun.manualSeq != MANUAL_SEQ.NONE &&
                    manualRun.manualSeq != MANUAL_SEQ.FINISH)
                {
                    manualRun.manualSeq = MANUAL_SEQ.FINISH;
                }

                switch (manualRun.manualSeq)
                {
                    case MANUAL_SEQ.NONE:
                        if (nSeqNo != 0)
                        {
                            nSeqNo = 0;
                        }
                        break;

                    case MANUAL_SEQ.RUN_MODULE:
                        if (RunModule() != FNC.SUCCESS) continue;
                        LeaveCycle();
                        break;
                    case MANUAL_SEQ.FINISH:
                        manualRun.manualSeq = MANUAL_SEQ.NONE;
                        InitSeqNum();
                        GbVar.mcState.isManualRun = false;
                        GbVar.mcState.isCycleRun[(int)SEQ_ID.MANUAL_RUN] = false;
                        GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MANUAL_RUN] = false;

                        if (OnFinishSeq != null)
                            OnFinishSeq(m_nResultNo);
                        OnFinishSeq = null;
                        break;

                    default:
                        break;
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        int RunModule()
        {
            if (m_ModuleSelect != null)
            {
                m_ModuleSelect.Run();

                if (!m_ModuleSelect.FINISH)
                    return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }

        public void LeaveCycle(int nResultNo = 0)
        {
            MANUAL_SEQ nPrevSeq = manualSeq;

            m_nResultNo = nResultNo;
            GbVar.mcState.isManualRun = false;
            GbVar.mcState.isCycleRun[(int)SEQ_ID.MANUAL_RUN] = false;
            GbVar.mcState.isCycleRunReq[(int)SEQ_ID.MANUAL_RUN] = false;

            switch (nPrevSeq)
            {
                case MANUAL_SEQ.NONE:
                    break;

                case MANUAL_SEQ.RUN_MODULE:
                    CheckStartStopRunModuleCondition(false, nResultNo);
                    break;

                case MANUAL_SEQ.FINISH:
                    break;
                default:
                    break;
            }

            InitSeqNum();
            manualSeq = MANUAL_SEQ.FINISH;

            //SeqHistory("LEAVE MANUAL CYCLE : {0}", nPrevNo);
        }
        #endregion

        void CheckStartStopRunModuleCondition(bool bStart, int nResultNo = 0)
        {
            if (manualSeq != MANUAL_SEQ.RUN_MODULE)
                return;

            switch (m_ModuleType)
            {
                case MODULE_TYPE.ALL_HOME:
                    if (bStart == false)
                    {
                        MotionMgr.Inst.AllStop();
                    }
                    break;

                
                case MODULE_TYPE.MOVE_TEACH_POS:
                case MODULE_TYPE.MOVE_CFG_POS:
                    if (bStart == false)
                    {
                        MotionMgr.Inst.AllStop();
                    }
                    break;

                case MODULE_TYPE.MANUAL_LD_CYCLE_RUN:
                case MODULE_TYPE.MANUAL_SAW_CYCLE_RUN:
                case MODULE_TYPE.MANUAL_SORTER_CYCLE_RUN:
                    if (bStart == false)
                    {
                        //// 1F CONV STOP
                        //MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_1_CONV_CCW, false);
                        //MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_1_CONV_CW, false);
                        //MotionMgr.Inst.SetOutput(IODF.OUTPUT.LD_1_CONV_STOP, true);

                        MotionMgr.Inst.AllStop();
                    }
                    break;

                case MODULE_TYPE.SINGLE_HOME:
                    if (bStart == false)
                    {
                        MotionMgr.Inst.AllStop();
                    }
                    break;
                case MODULE_TYPE.MANUAL_CLEANER_CYCLE_RUN:
                    if (bStart == false)
                    {
                        MotionMgr.Inst.AllStop();
                        //모든 상황에서 끄면 안될듯 [2022.05.12 작성자 홍은표] 
                        //if (GbVar.IO[IODF.OUTPUT.HEATING_POWER_ON_SIGNAL] == 1)
                        //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.HEATING_POWER_ON_SIGNAL, false);
                        //if (GbVar.IO[IODF.OUTPUT.PLASMA_HEATING_AIR_SOL] == 1)
                        //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.PLASMA_HEATING_AIR_SOL, false);
                    }
                    break;
					
				case MODULE_TYPE.MAP_VISION_VIEW_CYCLE_RUN:
                case MODULE_TYPE.TRAY_VISION_VIEW_CYCLE_RUN:
                    if (bStart == false)
                    {
                        MotionMgr.Inst.AllStop();
                    }
                    break;
                case MODULE_TYPE.MANUAL_PRE_ALIGN:
                    if (bStart == false)
                    {
                        IFMgr.Inst.VISION.SetPreAlignGrabReq(false);
                        IFMgr.Inst.VISION.SetPreAlignComplete(false);
                    }
                    break;
                case MODULE_TYPE.MANUAL_TOP_ALIGN:
                    {
                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                    }
                    break;
                case MODULE_TYPE.MANUAL_MAP_INSPECTION:
                    {
                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                    }
                    break;
                case MODULE_TYPE.MANUAL_BALL_INSPECTION:
                    {
                        IFMgr.Inst.VISION.SetBallInspCountReset(false);
                        IFMgr.Inst.VISION.SetBallInspCountComplete(false);
                    }
                    break;
                case MODULE_TYPE.MANUAL_PICKER_CALIBRATION:
                    {
                        if (bStart == false)
                        {
                            IFMgr.Inst.VISION.SetPickerCalReset(false);
                            IFMgr.Inst.VISION.SetPickerCalComplete(false);
                            IFMgr.Inst.VISION.SetPickerCalLedOff(false);
                            IFMgr.Inst.VISION.SetPickerCalLedOn(false);
                        }
                    }
                    break;
                case MODULE_TYPE.MANUAL_PICKER_ZEROSET_PICK_UP:
                    {
                        if (bStart == false)
                        {
                            MotionMgr.Inst.AllStop();
                        }
                    }
                    break;
                case MODULE_TYPE.MANUAL_PICKER_ZEROSET_PLACE:
                    {
                        if (bStart == false)
                        {
                            MotionMgr.Inst.AllStop();
                        }
                    }
                    break;
                case MODULE_TYPE.MAX:
                    break;
                default:
                    break;
            }
        }

        public void SetMcInitializePara(bool bLoaderHome, bool bSawHome, bool bSorterHome)
        {
            mAllHome.SetHommingPara(bLoaderHome, bSawHome, bSorterHome);
        }

        public void SetMoveTeachPos(int[] nAxisArr, double[] dPosArr, uint[] nOrderArr, bool bInterlock = true, bool bManualSpeed = false, double[] dSpeed = null, double[] dAcc = null, double[] dDec = null)
        {
            mTeachPosMove.SetParam(nAxisArr, dPosArr, nOrderArr, bInterlock, bManualSpeed, dSpeed, dAcc, dDec);
        }

        public void SetMove_ChipPickerCamPOS(int _nHeadNo, int _nPickerNo, int _nPickerColCount, int _nTableNo, int _nTableCount)
        {
            mTeachPosMove.SetParam_ChipPickerMove( _nHeadNo,  _nPickerNo,  _nPickerColCount,  _nTableNo,  _nTableCount);
        }
        public void SetMove_ChipPickPOS(int _nHeadNo, int _nPickerNo, int _nPickerColCount, int _nTableNo, int _nTableCount)
        {
            mTeachPosMove.SetParam_ChipPickMove(_nHeadNo, _nPickerNo, _nPickerColCount, _nTableNo, _nTableCount);
        }
        public void SetMove_ChipGoodPlacePOS(int _nHeadNo, int _nPickerNo, int _nTableNo, int _nTableCount)
        {
            mTeachPosMove.SetParam_ChipGoodPlaceMove(_nHeadNo, _nPickerNo, _nTableNo, _nTableCount);
        }
        public void SetMove_ChipRejectPlacePOS(int _nHeadNo, int _nPickerNo, int _nTableCount)
        {
            mTeachPosMove.SetParam_ChipRejectMove(_nHeadNo, _nPickerNo,  _nTableCount);
        }
        public void SetMoveCfgPos(int[] nAxisArr, double[] dPosArr, uint[] nOrderArr, bool bInterlock = true, bool bManualSpeed = false, double[] dSpeed = null, double[] dAcc = null, double[] dDec = null)
        {
            mCfgPosMove.SetParam(nAxisArr, dPosArr, nOrderArr, bInterlock, bManualSpeed, dSpeed, dAcc, dDec);
        }

        public void SetSingleHome(SVDF.AXES axis)
        { 
            mSingleHome.SetParam(new int[] { (int)axis });
        }

        public void SetSingleHome(int[] axis)
        {
            mSingleHome.SetParam(axis);
        }

        public void SetVisionTableOffset(int nTableNo, bool bMoveCamViewPos)
        {
            //mVisionTableOffset.SetParam(nTableNo, bMoveCamViewPos);
        }

        public void SetInitCycle(SEQ_ID seqID)
        {
            //mnInitCycle.SetParam(seqID);
        }

        public void SetManualLdCycleRun(int seqSelect, params object[] args)
        {
            mLdCycleRun.SetParam(seqSelect, args);
        }

        public void SetManualSawCycleRun(int seqSelect, params object[] args)
        {
            mSawCycleRun.SetParam(seqSelect, args);
        }
        public void SetManualCleaningCycleRun(int seqSelect, params object[] args)
        {
            //mCleanCycleRun.SetParam(seqSelect, args);
        }
        public void SetManualSorterCycleRun(int seqSelect, params object[] args)
        {
            mSorterCycleRun.SetParam(seqSelect, args);
        }

        public void SetManualSorterPickUpCycleRun(int seqSelect, params object[] args)
        {
            mSorterCycleRun.SetParam(seqSelect, args);
        }

        public void SetManualSorterInspectionCycleRun(int seqSelect, params object[] args)
        {
            mSorterCycleRun.SetParam(seqSelect, args);
        }

        public void SetManualSorterPlaceCycleRun(int seqSelect, params object[] args)
        {
            mSorterCycleRun.SetParam(seqSelect, args);
        }

        public void SetMapVisionViewCycleRun(int nTableNo, int nHeadNo, params object[] args)
        {
            mMapVisionView.SetParam(nTableNo, nHeadNo, args);
        }

        public void SetTrayVisionViewCycleRun(int nTableNo, int nHeadNo, params object[] args)
        {
            mTrayVisionView.SetParam(nTableNo, nHeadNo, args);
        }

        public void SetManualPreAlign(int nAlignMode)
        {
            mVisionPreAlignInterface.SetParam(nAlignMode);
        }

        public void SetManualTopAlign(int nTableNo)
        {
            mVisionTopAlignInterface.SetParam(nTableNo);
        }

        public void SetManualMapInspection(int nTableNo, int nMaxTriggerCount)
        {
            mVisionMapInspInterface.SetParam(nTableNo, nMaxTriggerCount);
        }

        public void SetManualBallInspection(int nTableNo, int nMaxTriggerCount)
        {
            mVisionBallInspInterface.SetParam(nTableNo, nMaxTriggerCount);
        }

        public void SetManualMoveCenterTopAlign(int nTableNo)
        {
            mMoveCenterTopAlignMark.SetParam(nTableNo);
        }
        public void SetManualZeroSet_Pickup(int nHeadNo, int nStageNo, bool[] isUsePad, double dStartZ, double dStepZ, double dStartX, double dStartY)
        {
            mPickerZeroSet_Pickup.SetParam(nHeadNo, nStageNo, isUsePad, dStartZ, dStepZ, dStartX, dStartY);
        }

        public void SetManualZeroSet_Place(int nHeadNo, int nStageNo, bool[] isUsePad, double dStartZ, double dStepZ, double dStartX, double dStartY)
        {
            mPickerZeroSet_Place.SetParam(nHeadNo, nStageNo, isUsePad, dStartZ, dStepZ, dStartX, dStartY);
        }
        public void SetManualPickerCal(int nPickerNo, int nAngleIndex, double dPickerPitch, int nMaxRetryCount, double dTolerence, bool bMovePitch = false)
        {
            mPickerCal.SetParam(nPickerNo, nAngleIndex, dPickerPitch, nMaxRetryCount, dTolerence, bMovePitch);
        }
    }
}
