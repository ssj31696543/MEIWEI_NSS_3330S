using DionesTool.Objects;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NSS_3330S
{
    /// <summary>
    /// 기본 시퀀스 베이스 (설비 전용 함수는 SeqBaseMc에 추가)
    /// </summary>
    public class SeqBase
    {
        // 자동 운전 시퀀스냐, 수동 운전 시퀀스냐
        protected bool m_bManualSeqMode = false;
        // Auto 시퀀스인지, Cycle 시퀀스인지
        protected bool m_bIsCycle = true;

        protected RunLib RunLib = new RunLib();
        protected RunLib RunLibCyl = new RunLib();

        protected Stopwatch swTimeStamp = new Stopwatch();

        public delegate void ErrorStopDelegate(int nErrNo);
        public event ErrorStopDelegate ErrStopEvent = null;

        public delegate void procAddMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView);
        public event procAddMsgEvent procMsgEvent;

        protected int m_nSeqNo = 0;
        protected int m_nPreSeqNo = 0;
        protected int nFuncResult = 0;

        protected int m_nSeqID = 0;
        protected string m_strSeqID = "";
        public string m_strLogMsg = "";

        protected string m_strMotPos = "";

        protected bool m_bIgnoreLog = false;
        protected bool m_bFirstSeqStep = true;  // Sequence Step 당 처음에 할 작업

        //시퀀스별로 택타임 분석을 위해 스탭별 걸린 시간을 측정하기위해 만듬
        protected DateTime dtStepElapsed = DateTime.Now;
        protected TimeSpan tsStepElapsed = TimeSpan.Zero;

        protected SeqDefault m_seqInfo = null;
        protected SeqBase m_cycleInfo = null;

        protected bool m_bWait = false;

        //HEP 추가 에어리어 센서 튈까봐 넣음
        int m_nLdElvY_InterlockCount = 0;
        int m_nLdElvZ_InterlockCount = 0;


        public string STEP_ELAPSED
        {
            get
            {
                tsStepElapsed = DateTime.Now - dtStepElapsed;
                dtStepElapsed = DateTime.Now;

                return string.Format("{0}:{1}:{2}.{3}",
                    tsStepElapsed.Hours.ToString("00"),
                    tsStepElapsed.Minutes.ToString("00"),
                    tsStepElapsed.Seconds.ToString("00"),
                    tsStepElapsed.Milliseconds.ToString("000"));

            }
        }

        public int SEQ_NO
        {
            get { return m_nSeqNo; }
        }

        public int CYCLE_SEQ_NO
        {
            get
            {
                if (m_cycleInfo == null) return -1;

                return m_cycleInfo.SEQ_NO;
            }
        }

        public bool FINISH { get; set; }

        public const int NOT_USED = -1;

        public virtual void Run()
        {
            if (!IsAcceptRun()) return;

            if (FINISH) return;

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
            }

            m_bFirstSeqStep = false;

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
            m_nSeqNo++;
        }

        public SeqBase()
        {
            swTimeStamp.Reset();

            FINISH = false;
        }

        /// <summary>
        /// 알람 호출하려면 필수
        /// </summary>
        /// <param name="dele"></param>
        public void SetErrorFunc(ErrorStopDelegate dele)
        {
            ErrStopEvent = dele;
        }

        public void SetAddMsgFunc(procAddMsgEvent dele)
        {
            procMsgEvent = dele;
        }

        /// <summary>
        /// ms 단위의 시간 딜레이 발생시킴. 지정된 시간이 지났으면 true 반환
        /// </summary>
        /// <param name="ms"></param>
        /// <returns>Busy면 false, Delay 시간이 지났으면 true</returns>
        public bool IsDelayOver(long ms)
        {
            if (FNC.IsBusy(RunLib.msecDelay(ms)))
                return false;

            return true;
        }

        /// <summary>
        /// ms 단위의 시간 딜레이 발생시킴. 지정된 시간이 지났으면 false 반환
        /// </summary>
        /// <param name="ms"></param>
        /// <returns>Busy면 true, Delay 시간이 지났으면 false</returns>
        public bool WaitDelay(long ms)
        {
            if (FNC.IsBusy(RunLib.msecDelay(ms)))
                return true;

            return false;
        }

        #region Basic Axis Move

        /// <summary>
        /// recipe 에서 설정된 위치 번호에 해당하는 위치만큼 해당 축을 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="nPosNo">recipe에서 설정된 위치 번호</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <returns></returns>
        protected int AxisMovePosNo(int nAxis, int nPosNo, long lDelay)
        {
            double dPos = TeachMgr.Inst.Tch.dMotPos[nAxis].dPos[nPosNo];
            int nSafety = FNC.SUCCESS;

            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            if (FNC.IsErr(nSafety))
            {
                SetError(nSafety);
                return nSafety;
            }
            //해당축에 구동위치와 속도를 설정한다.
            SVDF.SetMotionInfo(nAxis, dPos);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// 해당 축을 위치 값만큼 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="nPos">위치 값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <returns></returns>
        protected int AxisMovePos(int nAxis, double dPos, long lDelay)
        {
            int nSafety = FNC.SUCCESS;

            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            if (FNC.IsErr(nSafety))
            {
                SetError(nSafety);
                return nSafety;
            }
            //해당축에 구동위치와 속도를 설정한다.
            SVDF.SetMotionInfo(nAxis, dPos);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// config 에서 설정된 각 해당축의 안전위치로 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <returns></returns>
        protected int AxisMoveSafetyPos(int nAxis)
        {
            double dPos = ConfigMgr.Inst.Cfg.MotData[nAxis].dSafetyPos[0];

#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;

            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            if (FNC.IsErr(nSafety))
            {
                SetError(nSafety);
                return nSafety;
            }
            //해당축에 구동위치와 속도를 설정한다.
            SVDF.SetMotionInfo(nAxis, dPos);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvMove(mArray, 0);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }
        /// <summary>
        /// 해당 축을 velocity 값 변경하여 위치 값만큼 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="nPos">위치 값</param>
        /// <param name="dVel">Velocity</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <returns></returns>
        protected int AxisMovePos(int nAxis, double dPos, double dVel, long lDelay)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            if (FNC.IsErr(nSafety))
            {
                SetError(nSafety);
                return nSafety;
            }
            //해당축에 구동위치와 속도를 설정한다.
            SVDF.SetMotionInfo(nAxis, dPos, dVel);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }

        /// <summary>
        /// 각 축들의 설정된 인터록 bit를 확인하고 각 축들을 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="dPos">위치 값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock">인터록 을 확인할 것인지 설정하는 값</param>
        /// <param name="bResetFinish">명령 완료 후 자동으로 RunLib의 ResetCmd를 할건지</param>
        /// <returns></returns>
        protected int AxisMultiMovePos(int[] nAxis, double[] dPos, long lDelay, bool bCheckInterlock = true, bool bResetFinish = true)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i]);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    for (int j = 0; j < nAxis.Length; j++)
                    {
                        MotionMgr.Inst[nAxis[j]].MoveStop(); //구동정지
                    }
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], dPos[i]);
            }

            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                mArray[i] = SVDF.mInfo[nAxis[i]];
                mArray[i].dPos = dPos[i];
                //if (GbVar.mcState.IsRun()) mArray[i].dVel = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dAutoVel;
                mArray[i].dVel = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dVel;
                mArray[i].dAcc = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dAcc;
                mArray[i].dDec = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dDec;
            }

            int nCmdResult = RunLib.SvMove(mArray, lDelay, bResetFinish);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// 각 축들과 복동 실린더 의 설정된 인터록 bit를 확인하고 각 축들을 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="dPos">위치 값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock">인터록 을 확인할 것인지 설정하는 값</param>
        /// <param name="bResetFinish">명령 완료 후 자동으로 RunLib의 ResetCmd를 할건지</param>
        /// <returns></returns>
        //protected int AxisCylMultiMovePos(int[] nAxis, double[] dPos, CylinderAB[] cylinder, long lDelay, bool bCheckInterlock = true, bool bResetFinish = true)
        //{
        //    int nSafety = FNC.SUCCESS;
        //    if (RunLib.IsCurRunCmdStart())
        //    {
        //        if (bCheckInterlock)
        //        {
        //            //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
        //            for (int i = 0; i < nAxis.Length; i++)
        //            {
        //                nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i]);
        //                if (FNC.IsErr(nSafety))
        //                {
        //                    SetError(nSafety);
        //                    return nSafety;
        //                }
        //            }
        //        }
        //        else
        //        {

        //        }
        //    }
        //    //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
        //    for (int i = 0; i < nAxis.Length; i++)
        //    {
        //        nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
        //        if (FNC.IsErr(nSafety))
        //        {
        //            for (int j = 0; j < nAxis.Length; j++)
        //            {
        //                MotionMgr.Inst[nAxis[j]].MoveStop(); //구동정지
        //            }
        //            SetError(nSafety);
        //            return nSafety;
        //        }
        //    }
        //    //해당축에 구동위치와 속도를 설정한다.
        //    for (int i = 0; i < nAxis.Length; i++)
        //    {
        //        SVDF.SetMotionInfo(nAxis[i], dPos[i]);
        //    }
        //    //구동할 축들에 모션정보 배열 할당
        //    MOTINFO[] mArray = new MOTINFO[nAxis.Length];
        //    for (int i = 0; i < mArray.Length; i++)
        //    {
        //        mArray[i] = SVDF.mInfo[nAxis[i]];
        //        mArray[i].dPos = dPos[i];
        //        //if (GbVar.mcState.IsRun()) mArray[i].dVel = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dAutoVel;
        //        mArray[i].dVel = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dVel;
        //        mArray[i].dAcc = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dAcc;
        //        mArray[i].dDec = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dDec;
        //    }

        //    int nCmdResult = RunLib.SvCylMove(mArray, cylinder, lDelay, bResetFinish);
        //    if (FNC.IsErr(nCmdResult))
        //    {
        //        SetError(nCmdResult);
        //    }
        //    return nCmdResult;
        //}

        /// <summary>
        /// 각 축들의 설정된 인터록 bit를 확인하고 각 축들을 설정하는 스피드로 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="dPos">위치 값</param>
        /// <param name="dSpeed">각 축의 스피드 설정값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock">인터록 을 확인할 것인지 설정하는 값</param>
        /// <param name="bResetFinish">명령 완료 후 자동으로 RunLib의 ResetCmd를 할건지</param>
        /// <returns></returns>
        protected int AxisMultiMovePos(int[] nAxis, double[] dPos, double[] dSpeed, long lDelay, bool bCheckInterlock = true, bool bResetFinish = true)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i]);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], dPos[i]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                mArray[i] = SVDF.mInfo[nAxis[i]];
                mArray[i].dPos = dPos[i];
                mArray[i].dVel = dSpeed[i];
                mArray[i].dAcc = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dAcc;
                mArray[i].dDec = ConfigMgr.Inst.Cfg.MotData[nAxis[i]].dDec;
            }

            int nCmdResult = RunLib.SvMove(mArray, lDelay, bResetFinish);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// 각 축들의 설정된 인터록 bit를 확인하고 각 축들을 설정하는 스피드, 가속도, 감속도로 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="dPos">위치 값</param>
        /// <param name="dSpeed">각 축의 스피드 설정값</param>
        /// <param name="dAcc">각 축의 가속도 설정값</param>
        /// <param name="dDec">각 축의 감속도 설정값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock">인터록 을 확인할 것인지 설정하는 값</param>
        /// <param name="bResetFinish">명령 완료 후 자동으로 RunLib의 ResetCmd를 할건지</param>
        /// <returns></returns>
        protected int AxisMultiMovePos(int[] nAxis, double[] dPos, double[] dSpeed, double[] dAcc, double[] dDec, long lDelay, bool bCheckInterlock = true, bool bResetFinish = true, bool bSkipMapPkInterlock = false)
        {
//#if _NOTEBOOK
//            if (WaitDelay(lDelay)) return FNC.BUSY;
//            return FNC.SUCCESS;
//#endif

            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i], false, bSkipMapPkInterlock);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], dPos[i]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                if (nAxis[i] < 0)
                {
                    mArray[i] = new MOTINFO();
                    mArray[i].nAxis = -1;
                    mArray[i].dPos = dPos[i];
                    mArray[i].dVel = dSpeed[i];
                    mArray[i].dAcc = dAcc[i];
                    mArray[i].dDec = dDec[i];
                    continue;
                }
                else
                {
                    mArray[i] = SVDF.mInfo[nAxis[i]];
                    mArray[i].dPos = dPos[i];
                    mArray[i].dVel = dSpeed[i];
                    mArray[i].dAcc = dAcc[i];
                    mArray[i].dDec = dDec[i];
                }
                
            }

            int nCmdResult = RunLib.SvMove(mArray, lDelay, bResetFinish);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// 각 축들의 설정된 인터록 bit를 확인하고 각 축들을 설정하는 스피드, 가속도, 감속도로 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="dPos">위치 값</param>
        /// <param name="dSpeed">각 축의 스피드 설정값</param>
        /// <param name="dAcc">각 축의 가속도 설정값</param>
        /// <param name="dDec">각 축의 감속도 설정값</param>
        /// <param name="bSkipMoveDone">이동 완료 신호를 안본다</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock">인터록 을 확인할 것인지 설정하는 값</param>
        /// <param name="bResetFinish">명령 완료 후 자동으로 RunLib의 ResetCmd를 할건지</param>
        /// <returns></returns>
        protected int AxisMultiMovePos(int[] nAxis, double[] dPos, double[] dSpeed, double[] dAcc, double[] dDec, bool[] bSkipMoveDone, long lDelay, bool bCheckInterlock = true, bool bResetFinish = true, bool bSkipMapPkInterlock = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        if (nAxis[i] < 0) continue;

                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i], false, bSkipMapPkInterlock);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                if (nAxis[i] < 0) continue;

                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                if (nAxis[i] < 0) continue;

                SVDF.SetMotionInfo(nAxis[i], dPos[i]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                if (nAxis[i] < 0)
                {
                    mArray[i].nAxis = -1;

                    continue;
                }
                mArray[i] = SVDF.mInfo[nAxis[i]];
                mArray[i].dPos = dPos[i];
                mArray[i].dVel = dSpeed[i];
                mArray[i].dAcc = dAcc[i];
                mArray[i].dDec = dDec[i];
            }

            int nCmdResult = RunLib.SvMove(mArray, bSkipMoveDone, lDelay, bResetFinish);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        /// <summary>
        /// 각 축들의 설정된 인터록 bit를 확인하고 각 축들을 각 위치만큼 동시에 이동 
        /// </summary>
        /// <param name="nAxis">해당 축 번호</param>
        /// <param name="nPos">위치 값</param>
        /// <param name="lDelay">리턴값을 받는 시점을 늦추는 ms단위의 시간</param>
        /// <param name="bCheckInterlock"인터록 을 확인할 것인지 설정하는 값 </param>
        /// <returns></returns>
        protected int AxisMultiMovePosNo(int[] nAxis, int[] nPos, long lDelay, bool bCheckInterlock = true)
        {
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], TeachMgr.Inst.Tch.dMotPos[nAxis[i]].dPos[nPos[i]]);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], TeachMgr.Inst.Tch.dMotPos[nAxis[i]].dPos[nPos[i]]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                mArray[i] = SVDF.mInfo[nAxis[i]];
            }

            int nCmdResult = RunLib.SvMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
        }

        protected int AxisLinearMove(int[] nAxis, double[] dPos, double[] dSpeed, int nMaxAxis, int nCoordinate, long lDelay, bool bCheckInterlock = true)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], dPos[i]);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], dPos[i]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                mArray[i] = SVDF.mInfo[nAxis[i]];
                mArray[i].dPos = dPos[i];
                mArray[i].dVel = dSpeed[i];
                mArray[i].dAcc = dSpeed[i] * 4;
                mArray[i].dDec = dSpeed[i] * 4;
            }

            int nCmdResult = RunLib.SvLinearMoveEx(mArray, nMaxAxis, nCoordinate, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }


        protected int AxisLinearMove(int[] nAxis, int[] nPos, int nMaxAxis, int nCoordinate, long lDelay, bool bCheckInterlock = true)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                if (bCheckInterlock)
                {
                    //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                    for (int i = 0; i < nAxis.Length; i++)
                    {
                        nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis[i], TeachMgr.Inst.Tch.dMotPos[nAxis[i]].dPos[nPos[i]]);
                        if (FNC.IsErr(nSafety))
                        {
                            SetError(nSafety);
                            return nSafety;
                        }
                    }
                }
                else
                {

                }
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            for (int i = 0; i < nAxis.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }
            //해당축에 구동위치와 속도를 설정한다.
            for (int i = 0; i < nAxis.Length; i++)
            {
                SVDF.SetMotionInfo(nAxis[i], TeachMgr.Inst.Tch.dMotPos[nAxis[i]].dPos[nPos[i]]);
            }
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[nAxis.Length];
            for (int i = 0; i < mArray.Length; i++)
            {
                mArray[i] = SVDF.mInfo[nAxis[i]];
            }

            int nCmdResult = RunLib.SvLinearMoveEx(mArray, nMaxAxis, nCoordinate, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }

        protected int AxisSinalSearch(int nAxis, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                //nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                //if (FNC.IsErr(nSafety))
                //{
                //    SetError(nSafety);
                //    return nSafety;
                //}
            }
            //매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            if (FNC.IsErr(nSafety))
            {
                SetError(nSafety);
                return nSafety;
            }
            //해당축에 구동위치와 속도를 설정한다.
            //SVDF.SetMotionInfo(nAxis, dPos, dVel);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvSignalSearchMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }

        protected int AxisPlusSinalSearch(int nAxis, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            int nSafety = FNC.SUCCESS;
            if (RunLib.IsCurRunCmdStart())
            {
                //구동 전 소프트웨어 리미트와, 해당 축의 구동인터록 확인
                //nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxis, dPos);
                //if (FNC.IsErr(nSafety))
                //{
                //    SetError(nSafety);
                //    return nSafety;
                //}
            }
            ////매번 Amp Alarm, 원점완료상태, 서보 Enable상태, 리미트 센서 상태를 감시
            //nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxis);
            //if (FNC.IsErr(nSafety))
            //{
            //    SetError(nSafety);
            //    return nSafety;
            //}
            //해당축에 구동위치와 속도를 설정한다.
            //SVDF.SetMotionInfo(nAxis, dPos, dVel);
            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[] { SVDF.mInfo[nAxis] };
            int nCmdResult = RunLib.SvPlusSignalSearchMove(mArray, lDelay);
            if (FNC.IsErr(nCmdResult))
            {
                SetError(nCmdResult);
            }
            return nCmdResult;
#endif
        }
        #endregion

        #region IO
        protected int CylListMove(int[] nOnOutput, int[] nOffOutput, int[] nOnInput, int[] nOffInput, int nErr, long lDelay, int nMode = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            return RunLib.CylListMove(nOnOutput, nOffOutput, nOnInput, nOffInput, nErr, lDelay, nMode);
#endif
        }

        protected int VacuumListOn(int[] nOnOUT, int[] nOffOUT, int[] nOnIN, int[] nOffIN, int[] nErr, long lDelay, bool isChk, bool bOnlyChk, int nMode = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            return RunLib.VacuumListOn(nOnOUT, nOffOUT, nOnIN, nOffIN, nErr, lDelay, isChk, bOnlyChk, nMode);
#endif
        }

        protected int VacuumListOff(int[] nOnOUT, int[] nOffOUT, int[] nOffIN, int[] nErr, long lDelay, bool isChk, int nMode = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#else
            return RunLib.VacuumListOff(nOnOUT, nOffOUT, nOffIN, nErr, lDelay, isChk, nMode);
#endif
        }

        /// <summary>
        /// 220513
        /// 해당 모듈의 진공블로우 상태를 반환 합니다.
        /// </summary>
        /// <param name="eMdl"></param>
        /// <returns></returns>
        public AIRSTATUS AirStatus(STRIP_MDL eMdl)
        {
            AIRSTATUS eRet = AIRSTATUS.NONE;

            switch (eMdl)
            {
                case STRIP_MDL.NONE:
                    break;
                case STRIP_MDL.STRIP_RAIL:
                    break;
                case STRIP_MDL.STRIP_TRANSFER:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if(GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.STRIP_PK_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;
                    break;
                case STRIP_MDL.CUTTING_TABLE:
                    break;
                case STRIP_MDL.UNIT_TRANSFER:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1 && 
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 1 &&                  
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.UNIT_PK_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;

                    break;
                case STRIP_MDL.UNIT_CLEANING:
                    break;
                case STRIP_MDL.UNIT_DRY:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DRY_BLOCK_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;

                    break;

                case STRIP_MDL.MAP_TRANSFER:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_PK_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;

                    break;
                case STRIP_MDL.MAP_VISION_TABLE_1:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_1_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;

                    break;
                case STRIP_MDL.MAP_VISION_TABLE_2:
                    if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 0)
                        eRet = AIRSTATUS.VAC;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 1)
                        eRet = AIRSTATUS.BLOW;
                    else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_VAC_ON] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.MAP_ST_2_BLOW] == 0)
                        eRet = AIRSTATUS.NONE;
                    else
                        eRet = AIRSTATUS.UNKNOWN;

                    break;
                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }
            return eRet;
        }


        #endregion

        #region Sequence 관련

        /// <summary>
        /// 에러 이벤트 발생  
        /// </summary>
        /// <param name="nErrNo">에러 번호</param>
        /// <returns></returns>
        protected virtual void SetError(int nErrNo)
        {
            if (nErrNo < 0) return;
            if(nErrNo >= ErrMgr.Inst.errlist.Count)
            {
                System.Diagnostics.Debugger.Break();
                nErrNo = ErrMgr.Inst.errlist.Count - 1;
            }

            ErrorInfo errInfo = null;

            try
            {
                errInfo = ErrMgr.Inst.errlist[nErrNo];
            }
            catch (Exception)
            {
                errInfo = new ErrorInfo();
            }

            string msg = string.Format("ALARM OCCURED! {0}", errInfo.NAME);
            SeqHistory(msg);

            // 중알람일 경우에만
            if (errInfo.HEAVY)
            {
                //if (m_nSeqID != (int)SEQ_ID.MONITORING)
                {
                    if (GbVar.mcState.isCycleErr[m_nSeqID]) return;
                    GbVar.mcState.isCycleErr[m_nSeqID] = true;
                }
                //else
                //{

                //}

                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                GbVar.mcState.AllCycleRunReqStop();

                GbVar.mcState.isWaitForStop = true;
            }

            if (ErrStopEvent != null)
                ErrStopEvent(nErrNo);

            //choh add - Last ErrNo Update
            GbVar.mcState.nLastErrNo = nErrNo;
            //GbVar.nAlarmOccuredCount++;
        }

        protected void OnlyStopEvent(int nErrNo)
        {
            if (ErrStopEvent != null)
                ErrStopEvent(nErrNo);
        }

        protected virtual bool IsAcceptUsingJog()
        {
            return true;
        }


        /// <summary>
        /// 메뉴얼 시퀀스 모드 set  
        /// </summary>
        /// <returns></returns>
        public void SetManualSeqMode()
        {
            m_nSeqID = (int)SEQ_ID.MANUAL_RUN;
            m_bManualSeqMode = true;
        }

        public void SetCycleMode(bool bSetCycleMode)
        {
            m_bIsCycle = bSetCycleMode;
        }

        /// <summary>
        /// 각 process 의 run flag 체크
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsAcceptRun()
        {
            //if (GbVar.mcState.isInitialized == false) return false;
            if (GbVar.mcState.isCycleErr[m_nSeqID]) return false;
            if (!GbVar.mcState.isCycleRun[m_nSeqID]) return false;
            //if (CheckCycle() == false) return false;  // Vision Error 시 Timeout 알람 안되게끔

            return true;
        }


        /// <summary>
        /// 메시지 이벤트 호출  
        /// </summary>
        /// <param name="bLogView">list log에 view 할지를 결정 flag </param>
        /// <param name="format">메시지 형식</param>
        /// <param name="args">메시지 형식 param</param>
        /// <returns></returns>
        protected void SeqHistory(string format, params object[] args)
        {
            string strHistory = string.Format(format, args);
            SeqHistory(strHistory);
        }


        /// <summary>
        /// 20220106 pjh NEW SeqHistory Function 
        /// </summary>
        /// <param name="sCommand"></param>
        /// <param name="sPart"></param>`
        /// <param name="sStatus"></param>
        protected virtual void SeqHistory(string sCommand, string sPart, string sStatus)
        {
            try
            {
                string strLog = string.Format("{0} | {1} | {2}", sCommand, sPart, sStatus);

            if (m_strLogMsg == strLog) return;
            m_strLogMsg = strLog;

            //if (Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debug.WriteLine(strHistory);
            //}

            string strNow = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (m_seqInfo != null)
            {
                 //220615 로그 저장 펑션 변경
                if (m_bIsCycle)
                {
                    GbFunc.WriteProcLog(m_nSeqID, m_seqInfo.nCurrentSeqNo, m_nSeqNo, strNow, "", strLog);
                }
                else
                {
                    GbFunc.WriteProcLog(m_nSeqID, m_nSeqNo, -1, strNow, "", strLog);
                }

                
                //if (m_bIsCycle)
                //{
                //    GbFunc.WriteProcLog(m_nSeqID, m_seqInfo.nCurrentSeqNo, m_nSeqNo, strNow, sCommand, sPart, sStatus);
                //}
                //else
                //{
                //    GbFunc.WriteProcLog(m_nSeqID, m_seqInfo.nCurrentSeqNo, 0, strNow, sCommand, sPart, sStatus);
                //}
            }
            else
            {
                    GbFunc.WriteProcLog(m_nSeqID, m_nSeqNo, -1, strNow, "", strLog);

                    //GbFunc.WriteProcLog(m_nSeqID, 0, 0, strNow, sCommand, sPart, sStatus);
                    //if (procMsgEvent != null)
                    //    procMsgEvent(m_nSeqID, m_nSeqNo, -1, strHistory, false);
                }

                GbVar.qMsg.Enqueue(new string[] { strNow, strLog });
            }
            catch (Exception ex)
            {
                //
            }
        }

        /// <summary>
        /// 메시지 이벤트 발생  
        /// </summary>
        /// <param name="strHistory">전달할 문자열</param>
        /// <param name="bLogView">list log에 view 할지를 결정 flag</param>
        /// <returns></returns>
        protected virtual void SeqHistory(string strHistory)
        {
            try
            {
                if (m_strLogMsg == strHistory)
                    return;
                m_strLogMsg = strHistory;

            //if (Debugger.IsAttached)
            //{
            //    System.Diagnostics.Debug.WriteLine(strHistory);
            //}

            string strNow = DateTime.Now.ToString("yyyyMMddHHmmssfff");

            if (m_seqInfo != null)
            {
                if (m_bIsCycle)
                {
                    GbFunc.WriteProcLog(m_nSeqID, m_seqInfo.nCurrentSeqNo, m_nSeqNo, strNow, "", strHistory);
                }
                else
                {
                    GbFunc.WriteProcLog(m_nSeqID, m_nSeqNo, -1, strNow, "", strHistory);
                }
            }
            else
            {
                GbFunc.WriteProcLog(m_nSeqID, m_nSeqNo, -1, strNow, "", strHistory);
                //if (procMsgEvent != null)
                //    procMsgEvent(m_nSeqID, m_nSeqNo, -1, strHistory, false);
            }

                GbVar.qMsg.Enqueue(new string[] { strNow, strHistory });
            }
            catch (Exception ex)
            {

            }
            
        }

        protected void SeqHistory(string strHistory, int nSeqNo, int nSeqSubNo)
        {
            if (m_strLogMsg == strHistory) return;
            m_strLogMsg = strHistory;

            if (procMsgEvent != null)
                procMsgEvent(m_nSeqID, nSeqNo, nSeqSubNo, strHistory, false);
        }

        public void SetProcMsgEvent(int nProcNo, int nSeqNo, int nSeqSubNo, string strAddMsg, bool bLogView)
        {
            if (procMsgEvent != null)
                procMsgEvent(nProcNo, m_nSeqNo, nSeqNo, strAddMsg, bLogView);
        }



        /// <summary>
        /// Cmd 현재 상태 확인 및 타임 리셋   
        /// </summary>        
        /// <returns></returns>
        public virtual void ResetCmd()
        {
            RunLib.ResetCmd();
            RunLib.SetTimeBegin();

            RunLibCyl.ResetCmd();
            RunLibCyl.SetTimeBegin();

            m_strMotPos = "";
            m_bFirstSeqStep = true;
        }

        /// <summary>
        /// 시퀀스 넘버를 변경   
        /// </summary>       
        /// <param name="nSeq">변경할 시퀀스 넘버</param>
        /// <returns></returns>
        public virtual void NextSeq(int nSeq)
        {
            if (nSeq >= 0)
                m_nSeqNo = nSeq;
            ResetCmd();
        }


        /// <summary>
        /// Timer 정지   
        /// </summary>       
        /// <returns></returns>
        protected void SetTimeStop()
        {
            swTimeStamp.Stop();
        }

        /// <summary>
        /// 시퀀스 넘버 0으로 변경   
        /// </summary>       
        /// <returns></returns>
        public virtual void InitSeq(int nSeq = 0)
        {
            if (m_nSeqNo != 0)
            {
                m_strLogMsg = "Init Sequence";
            }

            m_nSeqNo = nSeq;
            m_nPreSeqNo = 0;
            ResetCmd();
            FINISH = false;
        }

        #endregion

        #region Cycle Run 관련

        /// <summary>
        /// 알람 발생 시 시작할 시퀀스를 초기화한다
        /// Cycle 중지 요청 확인하여 시퀀스를 중지한다
        /// </summary>
        /// <returns></returns>
        protected bool LeaveCycle()
        {  
            // 중지 요청 확인
            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
            {
                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                return false;
            }

            //시퀀스 저장 자기 자신

            return true;
        }

        protected bool CheckCycle()
        {
            // 한 번만 확인
            if (m_bFirstSeqStep)
            {
                // 중지 요청 확인
                if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                {
                    GbVar.mcState.isCycleRun[m_nSeqID] = false;
                    return false;
                }
            }

            // TODO : 
            //// Vision 창이 떠 있을 때 대기
            //if (GbVar.Seq.seqLdUvw.bManualSetPopup) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_1].bManualSetPopup) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_2].bManualSetPopup) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_3].bManualSetPopup) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_4].bManualSetPopup) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_1].bErrorSelect) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_2].bErrorSelect) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_3].bErrorSelect) return false;
            //if (GbVar.Seq.seqCuttingTable[MCDF.TABLE_4].bErrorSelect) return false;

            return true;
        }

        /// <summary>
        /// 운전을 정지한다
        /// </summary>
        protected void CycleStop()
        {
            GbVar.mcState.isCycleRunReq[m_nSeqID] = false;
            GbVar.mcState.isCycleRun[m_nSeqID] = false;
        }
        #endregion

        #region 설비 전용 Interlock
        protected int IsCheckAlwaysMonitoring()
        {
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)
            {
                // EMO는 항상 체크하지만 DOOR는 사용할때만 체크 함
                int nRet = GbFunc.IsDoorOpenOrPressEmo(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse);
                if (FNC.IsErr(nRet)) return nRet;
            }



            return FNC.SUCCESS;
        }
        #endregion

        #region 설비 전용 I/O

        #region CONVEYOR
        protected int LdConv_Run(bool CW = false, long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL;

            if(CW)
            {
                // 정방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCW, true);
                MotionMgr.Inst.SetOutput(nAddrCCW, false);

            }
            else
            {
                // 역방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCCW, true);
                MotionMgr.Inst.SetOutput(nAddrCW, false);
            }

            return FNC.SUCCESS;
        }

        protected int LdConv_Stop(long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL;

            MotionMgr.Inst.SetOutput(nAddrCCW, false);
            MotionMgr.Inst.SetOutput(nAddrCW, false);
            MotionMgr.Inst.SetOutput(nAddStop, true);

            return FNC.SUCCESS;
        }
        protected int LdInputStopper_Up(bool bUp, bool isChkIn = true, long lDelay = 0)
        {

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bUp)
            {
                OnOut[0] = (int)IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL;
                OffOut[0] = (int)IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL;
                OnIn[0] = (int)IODF.INPUT._1F_INPUT_STOPPER_UP;
                OffIn[0] = (int)IODF.INPUT._1F_INPUT_STOPPER_DOWN;
                nErrNo[0] = (int)ERDF.E_LD_1F_CV_INPUT_STOPPER_UP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL;
                OffOut[0] = (int)IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL;
                OnIn[0] = (int)IODF.INPUT._1F_INPUT_STOPPER_DOWN;
                OffIn[0] = (int)IODF.INPUT._1F_INPUT_STOPPER_UP;
                nErrNo[0] = (int)ERDF.E_LD_1F_CV_INPUT_STOPPER_DOWN_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int LdStopper_Up(bool bUp, bool isChkIn = true, long lDelay = 0)
        {

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bUp)
            {
                OnOut[0] = (int)IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL;
                OffOut[0] = (int)IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL;
                OnIn[0] = (int)IODF.INPUT._1F_MZ_STOPPER_UP_CHECK;
                OffIn[0] = (int)IODF.INPUT._1F_MZ_STOPPER_DOWN_CHECK;
                nErrNo[0] = (int)ERDF.E_LD_1F_CV_STOPPER_UP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL;
                OffOut[0] = (int)IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL;
                OnIn[0] = (int)IODF.INPUT._1F_MZ_STOPPER_DOWN_CHECK;
                OffIn[0] = (int)IODF.INPUT._1F_MZ_STOPPER_UP_CHECK;
                nErrNo[0] = (int)ERDF.E_LD_1F_CV_STOPPER_DOWN_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int UldConv_Run(bool bReverse, long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL;

            if (bReverse)
            {
                // 정방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCCW, false);
                MotionMgr.Inst.SetOutput(nAddrCW, true);
            }
            else
            {
                // 역방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCW, false);
                MotionMgr.Inst.SetOutput(nAddrCCW, true);
            }

            return FNC.SUCCESS;
        }

        protected int UldStopper_Up(bool bUp, bool isChkIn = true, long lDelay = 0)
        {

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bUp)
            {
                OnOut[0] = (int)IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL;
                OffOut[0] = (int)IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL;
                OnIn[0] = (int)IODF.INPUT._2F_MZ_STOPPER_UP_CHECK;
                OffIn[0] = (int)IODF.INPUT._2F_MZ_STOPPER_DOWN_CHECK;
                nErrNo[0] = (int)ERDF.E_LD_2F_CV_STOPPER_UP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL;
                OffOut[0] = (int)IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL;
                OnIn[0] = (int)IODF.INPUT._2F_MZ_STOPPER_DOWN_CHECK;
                OffIn[0] = (int)IODF.INPUT._2F_MZ_STOPPER_UP_CHECK;
                nErrNo[0] = (int)ERDF.E_LD_2F_CV_STOPPER_DOWN_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UldConv_Stop(long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL;

            MotionMgr.Inst.SetOutput(nAddrCCW, false);
            MotionMgr.Inst.SetOutput(nAddrCW, false);
            MotionMgr.Inst.SetOutput(nAddStop, true);

            return FNC.SUCCESS;
        }



        // 컨베이어 INPUT SENSOR

        // 매거진 도착 확인
        protected bool IsLdConv_Arrival(int num)
        {
            int nAddr = 0;
            //0 : Input
            //1 : Check 1
            //2 : Check 2
            //3 : Check 3
            //4 : Elv Mag Check 1
            //5 : Elv Mag Check 2

            switch (num)
            {
                case 0:
                    nAddr = (int)IODF.INPUT._1F_MZ_INPUT_CHECK;
                    break;

                case 1:
                    nAddr = (int)IODF.INPUT._1F_MZ_CHECK_1;
                    break;

                case 2:
                    nAddr = (int)IODF.INPUT._1F_MZ_CHECK_2;
                    break;

                case 3:
                    nAddr = (int)IODF.INPUT._1F_MZ_CHECK_3;
                    break;

                case 4:
                    nAddr = (int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR;
                    break;

                case 5:
                    nAddr = (int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR_2;
                    break;
            }
            
            //return GbVar.GB_INPUT[nAddr] == 0; //이전 소스
            return GbVar.GB_INPUT[nAddr] == 1; //B접 -> A접
        }

        protected bool IsLdConv_Full()
        {
            int nAddr_1 = (int)IODF.INPUT._1F_MZ_CHECK_3;
            int nAddr_2 = (int)IODF.INPUT._1F_MZ_CHECK_2;
            int nAddr_3 = (int)IODF.INPUT._1F_MZ_CHECK_1;
            int nAddr_4 = (int)IODF.INPUT._1F_MZ_INPUT_CHECK;
            //return GbVar.GB_INPUT[nAddr] == 0; //이전 소스
            return GbVar.GB_INPUT[nAddr_1] == 1 && GbVar.GB_INPUT[nAddr_2] == 1 && GbVar.GB_INPUT[nAddr_3] == 1 && GbVar.GB_INPUT[nAddr_3] == 1;
        }

        // 신규 설비에서는 투입 센서가 없어짐
        // 매거진 언로드 동작 시 센서 체크 불가

        protected bool IsUldConv_Arrival(int num)
        {
            int nAddr = 0;
            //0 : Check 1
            //1 : Check 2
            //2 : Check 3

            switch (num)
            {
                case 0:
                    nAddr = (int)IODF.INPUT._2F_MZ_CHECK_1;
                    break;

                case 1:
                    nAddr = (int)IODF.INPUT._2F_MZ_CHECK_2;
                    break;

                case 2:
                    nAddr = (int)IODF.INPUT._2F_MZ_CHECK_3;
                    break;

            }

            //return GbVar.GB_INPUT[nAddr] == 0; //이전 소스
            return GbVar.GB_INPUT[nAddr] == 1; //B접 -> A접
        }

        // 매거진 투입 확인
        protected bool IsLdConv_Insert()
        {
            int nAddrRet = (int)IODF.INPUT._1F_MZ_INPUT_CHECK;

            //return GbVar.GB_INPUT[nAddrRet] == 1; //이전 소스
            return GbVar.GB_INPUT[nAddrRet] == 0; 
        }

        // 매거진 유/무 확인
        protected bool IsLdConv_Detect()
        {
            int nAddr = (int)IODF.INPUT._1F_MZ_CHECK_3;

            int nAddr1 = (int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR;
            int nAddr2 = (int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR_2;

            // 접점 확인해야 함
            return GbVar.GB_INPUT[nAddr] == 1 && GbVar.GB_INPUT[nAddr1] == 0 && GbVar.GB_INPUT[nAddr2] == 0;
        }

        /// <summary>
        /// 매거진 컨베이어 투입구에 라이트커튼 센서 감지 확인
        /// B접점
        /// </summary>
        /// <returns></returns>
        protected bool IsConvSafetyHandDetect(bool bDryRun)
        {
            bool bRet = false;
            
            if(bDryRun)
            {
                return false; // 정상 상태
            }

            // B접점 항상 ON, 감지되면 OFF 
            return GbVar.GB_INPUT[(int)IODF.INPUT.LD_SAFETY_SENSOR_LT_SIDE] == 0;
        }

        #endregion

        #region MGZ ELEVATOR
        protected int LdElvConv_Run(bool bReverse = false, long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL;

            if (bReverse)
            {
                // 역방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCW, true);
                MotionMgr.Inst.SetOutput(nAddrCCW, false);

            }
            else
            {
                // 정방향
                MotionMgr.Inst.SetOutput(nAddStop, false);
                MotionMgr.Inst.SetOutput(nAddrCCW, true);
                MotionMgr.Inst.SetOutput(nAddrCW, false);
            }

            return FNC.SUCCESS;
        }

        protected int LdElvConv_Stop(long lDelay = 0)
        {
            int nAddrCCW = (int)IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL;
            int nAddrCW = (int)IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL;
            int nAddStop = (int)IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL;

            MotionMgr.Inst.SetOutput(nAddrCCW, false);
            MotionMgr.Inst.SetOutput(nAddrCW, false);
            MotionMgr.Inst.SetOutput(nAddStop, true);

            return FNC.SUCCESS;
        }

        protected bool IsLdElvCheckSensorOn()
        {
            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR] == 0) return false;
            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR_2] == 0) return false;
            //if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CLAMP_CHECK_SENSOR] == 0) return false;
            return true;
        }

        // 매거진 감지 센서 둘 다 안들어왔을 경우 확인
        protected bool IsLdElvCheckSensor_MgzEmpty()
        {
            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR] == 0
                && GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CHECK_SENSOR_2] == 0)
            {
                return true;
            }

            return false;
        }

        protected bool IsLdElvDoor_Check()
        {
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                return true;

            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_DOOR_CHECK_SENSOR] == 1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 매거진 투입 전 CLIP 해제 상태 확인
        /// </summary>
        /// <returns></returns>
        protected bool IsLdElvMzClipOpenCheck()
        {
            return true;
        }

        /// <summary>
        /// 매거진 클램프 ON 상태 확인
        /// </summary>
        /// <returns></returns>
        protected bool IsLdElvCheckClampOn()
        {
            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_CLAMP_CHECK_SENSOR] == 1) return true;

            return false;
        }

        protected bool IsLdElvCheckUnClamp()
        {
            if (GbVar.GB_INPUT[(int)IODF.INPUT.ME_AXIS_MZ_UNCLAMP_CHECK_SENSOR] == 1) return true;

            return false;
        }

        /// <summary>
        /// UN CLAMP OUTPUT 
        /// </summary>
        /// <returns></returns>
        protected bool IsLdElvCheckUnClamp_Output()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ME_MZ_UNCLAMP_SOL] == 1) 
                return true;
            
            return false;
        }

        #endregion


        // 여기부터 작업 해야 함 !!!!

        #region LOADING

        protected int LdElvMgzClamp(bool bClamp, bool isChkIn = true, long lDelay = 0)
        {

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bClamp)
            {
                OnOut[0] = (int)IODF.OUTPUT.ME_MZ_CLAMP_SOL;
                OffOut[0] = (int)IODF.OUTPUT.ME_MZ_UNCLAMP_SOL;
                OnIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_CLAMP_CHECK_SENSOR;
                OffIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_UNCLAMP_CHECK_SENSOR;
                nErrNo[0] = (int)ERDF.E_MGZ_LD_ELV_CLAMP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.ME_MZ_UNCLAMP_SOL;
                OffOut[0] = (int)IODF.OUTPUT.ME_MZ_CLAMP_SOL;
                OnIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_UNCLAMP_CHECK_SENSOR;
                OffIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_CLAMP_CHECK_SENSOR;
                nErrNo[0] = (int)ERDF.E_MGZ_LD_ELV_UNCLAMP_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int LdElvDoorOpen(bool bClamp, bool isChkIn = true, long lDelay = 0)
        {

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bClamp)
            {
                OnOut[0] = (int)IODF.OUTPUT.MZ_DOOR_OPEN_SOL;
                OffOut[0] = (int)IODF.OUTPUT.MZ_DOOR_CLOSE_SOL;
                OnIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_DOOR_OPEN_SENSOR;
                OffIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_DOOR_CLOSE_SENSOR;
                nErrNo[0] = (int)ERDF.E_MGZ_LD_ELV_DOOR_OPEN_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.MZ_DOOR_CLOSE_SOL;
                OffOut[0] = (int)IODF.OUTPUT.MZ_DOOR_OPEN_SOL;
                OnIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_DOOR_CLOSE_SENSOR;
                OffIn[0] = (int)IODF.INPUT.ME_AXIS_MZ_DOOR_OPEN_SENSOR;
                nErrNo[0] = (int)ERDF.E_MGZ_LD_ELV_DOOR_CLOSE_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }


        protected int InletTableVacOn(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.LD_RAIL_VAC].lVacOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.LD_RAIL_VAC].lBlowOnDelay;
            }

            if (isVacOn)
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_VAC;
                    OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_BLOW;
                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_NONE;
                }
                else 
                {
                    OnOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_VAC;
                    OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_BLOW;
                    OnIn[0] = (int)IODF.A_INPUT.LD_RAIL_VAC;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_LD_RAIL_VAC_NOT_ON;
                }
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_VAC;
                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_LD_RAIL_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int InletTableBlowOn(bool isBlowOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[1];
            int[] OffOut = new int[2];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.LD_RAIL_VAC].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.LD_RAIL_VAC].lBlowOffDelay;
            }

            // 210907 ANALOG VAC SENSOR 연결완료
            if (isBlowOn)
            {
                OnOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_BLOW;
                OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_VAC;
                OffOut[1] = (int)IODF.OUTPUT.NONE;
                OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;

                // 2021030 bhoh
                OffIn[0] = (int)IODF.A_INPUT.LD_RAIL_VAC;
                nErrNo[0] = (int)ERDF.E_LD_RAIL_VAC_NOT_OFF;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_VAC;
                OffOut[1] = (int)IODF.OUTPUT.IN_LET_TABLE_BLOW;
                OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;

                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_LD_RAIL_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int InletTableUpDown(bool bUp, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bUp)
            {
                OnOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_UP;
                OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_DOWN;
                OnIn[0] = (int)IODF.INPUT.IN_LET_TABLE_UP;
                OffIn[0] = (int)IODF.INPUT.IN_LET_TABLE_DOWN;
                nErrNo[0] = (int)ERDF.E_IN_LET_TABLE_UP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_DOWN;
                OffOut[0] = (int)IODF.OUTPUT.IN_LET_TABLE_UP;
                OnIn[0] = (int)IODF.INPUT.IN_LET_TABLE_DOWN;
                OffIn[0] = (int)IODF.INPUT.IN_LET_TABLE_UP;
                nErrNo[0] = (int)ERDF.E_IN_LET_TABLE_DOWN_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected int MapPusherFwdBwd(bool bFwd, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bFwd)
            {
                OnOut[0] = (int)IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL;
                OffOut[0] = (int)IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL;
                OnIn[0] = (int)IODF.INPUT.PUSHER_FORWARD_SENSOR;
                OffIn[0] = (int)IODF.INPUT.PUSHER_BACKWARD_SENSOR;
                nErrNo[0] = (int)ERDF.E_MAP_PK_PUSHER_FWD_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL;
                OffOut[0] = (int)IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL;
                OnIn[0] = (int)IODF.INPUT.PUSHER_BACKWARD_SENSOR;
                OffIn[0] = (int)IODF.INPUT.PUSHER_FORWARD_SENSOR;
                nErrNo[0] = (int)ERDF.E_MAP_PK_PUSHER_BWD_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected int LdVisionGripUpDown(bool bUp, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bUp)
            {
                OnOut[0] = (int)IODF.OUTPUT.LD_X_GRIP_UP;
                OffOut[0] = (int)IODF.OUTPUT.LD_X_GRIP_DOWN;
                OnIn[0] = (int)IODF.INPUT.LD_X_GRIP_UP;
                OffIn[0] = (int)IODF.INPUT.LD_X_GRIP_DOWN;
                nErrNo[0] = (int)ERDF.E_LD_VISION_GRIPPER_UP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.LD_X_GRIP_DOWN;
                OffOut[0] = (int)IODF.OUTPUT.LD_X_GRIP_UP;
                OnIn[0] = (int)IODF.INPUT.LD_X_GRIP_DOWN;
                OffIn[0] = (int)IODF.INPUT.LD_X_GRIP_UP;
                nErrNo[0] = (int)ERDF.E_LD_VISION_GRIPPER_DOWN_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }


        // VISION축이 아니라 별도 GRIPPER축에서 사용 함
        protected int LdVisionGrip(bool bGrip, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (bGrip)
            {
                OnOut[0] = (int)IODF.OUTPUT.LD_X_GRIP;
                OffOut[0] = (int)IODF.OUTPUT.LD_X_UNGRIP;
                OnIn[0] = (int)IODF.INPUT.LD_X_GRIP;
                OffIn[0] = (int)IODF.INPUT.LD_X_UNGRIP;
                nErrNo[0] = (int)ERDF.E_LD_VISION_GRIPPER_GRIP_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.LD_X_UNGRIP;
                OffOut[0] = (int)IODF.OUTPUT.LD_X_GRIP;
                OnIn[0] = (int)IODF.INPUT.LD_X_UNGRIP;
                OffIn[0] = (int)IODF.INPUT.LD_X_GRIP;
                nErrNo[0] = (int)ERDF.E_LD_VISION_GRIPPER_UNGRIP_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }
        #endregion

        #region STRIP TRSF
       
        protected int StripPickerVac(bool isVacOn = true, bool isQuad = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[4];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.STRIP_PK_VAC].lVacOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.STRIP_PK_VAC].lBlowOnDelay;
            }

            if (isVacOn)
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    if (isQuad)
                    {
                        OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP;
                        OnOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                        OnOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;

                        OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                        OffOut[1] = (int)IODF.OUTPUT.NONE;
                        OffOut[2] = (int)IODF.OUTPUT.NONE;
                        OffOut[3] = (int)IODF.OUTPUT.NONE;

                        OnIn[0] = (int)IODF.A_INPUT.NONE;
                        OffIn[0] = (int)IODF.A_INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_ON;
                    }
                    else
                    {
                        OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP;
                        OnOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                        OnOut[2] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                        OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                        OffOut[2] = (int)IODF.OUTPUT.NONE;
                        OffOut[3] = (int)IODF.OUTPUT.NONE;

                        OnIn[0] = (int)IODF.A_INPUT.NONE;
                        OffIn[0] = (int)IODF.A_INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_ON;
                    }

                }
                else
                {
                    if (isQuad)
                    {
                        OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP;
                        OnOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                        OnOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;

                        OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                        OffOut[1] = (int)IODF.OUTPUT.NONE;
                        OffOut[2] = (int)IODF.OUTPUT.NONE;
                        OffOut[3] = (int)IODF.OUTPUT.NONE;

                        OnIn[0] = (int)IODF.A_INPUT.STRIP_PK_VAC;
                        OffIn[0] = (int)IODF.A_INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_ON;
                    }
                    else
                    {
                        OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP;
                        OnOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                        OnOut[2] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                        OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                        OffOut[2] = (int)IODF.OUTPUT.NONE;
                        OffOut[3] = (int)IODF.OUTPUT.NONE;

                        OnIn[0] = (int)IODF.A_INPUT.STRIP_PK_VAC;
                        OffIn[0] = (int)IODF.A_INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_ON;
                    }
                }
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OnOut[1] = (int)IODF.OUTPUT.NONE;
                OnOut[2] = (int)IODF.OUTPUT.NONE;

                OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP;
                OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                OffOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                OffOut[3] = (int)IODF.OUTPUT.STRIP_PK_BLOW;


                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }


        protected int StripPickerBlow(bool isBlowOn, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[2];
            int[] OffOut = new int[4];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.STRIP_PK_VAC].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.STRIP_PK_VAC].lBlowOffDelay;
            }

            if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
            {
                if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
                {
                    OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.NONE;

                    OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                    OffOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                    OffOut[3] = (int)IODF.OUTPUT.NONE;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_OFF;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OnOut[1] = (int)IODF.OUTPUT.NONE;

                    OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                    OffOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                    OffOut[3] = (int)IODF.OUTPUT.STRIP_PK_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_OFF;
                }
            }
            else
            {
                if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
                {
                    OnOut[0] = (int)IODF.OUTPUT.STRIP_PK_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.NONE;

                    OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                    OffOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                    OffOut[3] = (int)IODF.OUTPUT.NONE;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.STRIP_PK_VAC;
                    nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_OFF;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OnOut[1] = (int)IODF.OUTPUT.NONE;

                    OffOut[0] = (int)IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD;
                    OffOut[2] = (int)IODF.OUTPUT.STRIP_PK_VAC_ON;
                    OffOut[3] = (int)IODF.OUTPUT.STRIP_PK_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_STRIP_PK_VAC_NOT_OFF;
                }
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        #endregion

        #region UNIT TRSF
        protected int UnitPickerScrap1Vac(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_1].lVacOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.SCRAP_VAC_1;
                    //OffIn[0] = (int)IODF.A_INPUT.NONE;
                    //nErrNo[0] = (int)ERDF.E_NONE;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                //OffOut[1] = (int)IODF.OUTPUT.NONE;
                //OffOut[2] = (int)IODF.OUTPUT.NONE;

                //OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;
                //nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UnitPickerScrap1Blow(bool isBlowOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_1].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_1].lBlowOffDelay;
            }

            if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                //OffOut[1] = (int)IODF.OUTPUT.NONE;
                //OffOut[2] = (int)IODF.OUTPUT.NONE;

                //OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;
                //nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UnitPickerScrap2Vac(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_2].lVacOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.SCRAP_VAC_2;

                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                //OffOut[1] = (int)IODF.OUTPUT.NONE;
                //OffOut[2] = (int)IODF.OUTPUT.NONE;

                //OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;
                //nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UnitPickerScrap2Blow(bool isBlowOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_2].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_2].lBlowOffDelay;
            }

            if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;
                 
                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
                    //OffOut[1] = (int)IODF.OUTPUT.NONE;
                    //OffOut[2] = (int)IODF.OUTPUT.NONE;

                    //OnIn[0] = (int)IODF.A_INPUT.NONE;
                    //OffIn[0] = (int)IODF.A_INPUT.NONE;
                    //nErrNo[0] = (int)ERDF.E_NONE;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
                    //OffOut[1] = (int)IODF.OUTPUT.NONE;
                    //OffOut[2] = (int)IODF.OUTPUT.NONE;

                    //OnIn[0] = (int)IODF.A_INPUT.NONE;
                    //OffIn[0] = (int)IODF.A_INPUT.NONE;
                    //nErrNo[0] = (int)ERDF.E_NONE;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                //OffOut[1] = (int)IODF.OUTPUT.NONE;
                //OffOut[2] = (int)IODF.OUTPUT.NONE;

                //OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;
                //nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UnitPickerScrapAllBlow(bool isBlowOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[6];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_2].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.SCRAP_VAC_2].lBlowOffDelay;
            }

            if (isBlowOn)
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                    OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                    OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                    OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                OffOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                OffOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        protected int UnitPickerWorkVac(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[6];
            int[] OffOut = new int[6];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];
            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.UNIT_PK_VAC].lVacOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP;

                    OnOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP;
                    OnOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                    OnOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;
                    OnOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_BLOW;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP;

                    OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                    OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;

                    OffOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                    OffOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;

                    //OnIn[0] = (int)IODF.A_INPUT.NONE;
                    //OffIn[0] = (int)IODF.A_INPUT.NONE;
                    //nErrNo[0] = (int)ERDF.E_NONE;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON;
                    OnOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP;

                    OnOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP;
                    OnOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                    OnOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP;
                    OnOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_BLOW;
                    OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP;

                    OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                    OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;

                    OffOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                    OffOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;


                    OnIn[0] = (int)IODF.A_INPUT.UNIT_PK_VAC;
                    nErrNo[0] = (int)ERDF.E_UNIT_PK_VAC_NOT_ON;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                //OnIn[0] = (int)IODF.A_INPUT.NONE;
                //OffIn[0] = (int)IODF.A_INPUT.NONE;
                //nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int UnitPickerWorkBlow(bool isBlowOn = true, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[9];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.UNIT_PK_VAC].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.UNIT_PK_VAC].lBlowOffDelay;
            }

            if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                OnOut[0] = (int)IODF.OUTPUT.UNIT_PK_BLOW;

                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP;

                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                OffOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                OffOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;

                OffIn[0] = (int)IODF.A_INPUT.UNIT_PK_VAC;
                nErrNo[0] = (int)ERDF.E_UNIT_PK_VAC_NOT_OFF;
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.UNIT_PK_BLOW;
                OffOut[1] = (int)IODF.OUTPUT.UNIT_PK_VAC_ON;
                OffOut[2] = (int)IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP;

                OffOut[3] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW;
                OffOut[4] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP;
                OffOut[5] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON;

                OffOut[6] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW;
                OffOut[7] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP;
                OffOut[8] = (int)IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }
        #endregion



        #region CLEANER
        protected int CleanerSwingFwd(bool isFwd = true, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(lDelay)) return FNC.BUSY;

            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isFwd)
            {
                OnOut[0] = (int)IODF.OUTPUT.CLEAN_SWING_FWD;
                OffOut[0] = (int)IODF.OUTPUT.CLEAN_SWING_BWD;
                OnIn[0] = (int)IODF.INPUT.CLEANER_SWING_FWD;
                OffIn[0] = (int)IODF.INPUT.CLEANER_SWING_BWD;
                nErrNo[0] = (int)ERDF.E_CLEANER_SWING_FWD_FAIL;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.CLEAN_SWING_BWD;
                OffOut[0] = (int)IODF.OUTPUT.CLEAN_SWING_FWD;
                OnIn[0] = (int)IODF.INPUT.CLEANER_SWING_BWD;
                OffIn[0] = (int)IODF.INPUT.CLEANER_SWING_FWD;
                nErrNo[0] = (int)ERDF.E_CLEANER_SWING_BWD_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        /// <summary>
        /// Water Jet Water On, Off
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="nNo">Jet의 번호. (4 = 전부 제어)</param>
        /// <returns></returns>
        protected void CleanerWaterAirJet(bool isOn = true, int nNo = 4)
        {
            switch (nNo)
            {
                case 0:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_AIR, isOn);
                    break;

                case 1:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_AIR, isOn);
                    break;

                case 2:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_AIR, isOn);
                    break;

                case 3:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_AIR, isOn);
                    break;

                case 4:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_WATER, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_WATER, isOn);

                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_AIR, isOn);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Water Jet Air On, Off
        /// </summary>
        /// <param name="isOn"></param>
        /// <param name="nNo">Jet의 번호. (4 = 전부 제어)</param>
        /// <returns></returns>
        protected void CleanerAirJet(bool isOn = true, int nNo = 4)
        {
            switch (nNo)
            {
                case 0:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_AIR, isOn);
                    break;

                case 1:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_AIR, isOn);
                    break;

                case 2:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_AIR, isOn);
                    break;

                case 3:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_AIR, isOn);
                    break;

                case 4:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_1_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_2_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_3_AIR, isOn);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_WATER_4_AIR, isOn);
                    break;

                default:
                    break;
            }
        }
        #endregion


        #region WORKING TABLE
        protected int MapPickerVac(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_PK_WORK_VAC].lVacOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_PK_WORK_VAC].lBlowOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.MAP_PK_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.MAP_PK_BLOW;
                    OffOut[1] = (int)IODF.OUTPUT.MAP_PK_VAC_OFF_PUMP;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_NONE;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.MAP_PK_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.MAP_PK_BLOW;
                    OffOut[1] = (int)IODF.OUTPUT.MAP_PK_VAC_OFF_PUMP;


                    OnIn[0] = (int)IODF.A_INPUT.MAP_PK_WORK_VAC;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_MAP_PK_WORK_VAC_NOT_ON;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.MAP_PK_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.MAP_PK_BLOW;
                OffOut[2] = (int)IODF.OUTPUT.MAP_PK_VAC_OFF_PUMP;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_MAP_PK_WORK_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int MapPickerBlow(bool isBlowOn = true, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[3];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_PK_WORK_VAC].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_PK_WORK_VAC].lBlowOffDelay;
            }

            if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                OnOut[0] = (int)IODF.OUTPUT.MAP_PK_VAC_ON_PUMP;
                OnOut[1] = (int)IODF.OUTPUT.MAP_PK_BLOW;

                OffOut[0] = (int)IODF.OUTPUT.MAP_PK_VAC_ON;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.MAP_PK_WORK_VAC;
                nErrNo[0] = (int)ERDF.E_MAP_PK_WORK_VAC_NOT_OFF;
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.MAP_PK_BLOW;
                OffOut[1] = (int)IODF.OUTPUT.MAP_PK_VAC_ON;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_MAP_PK_WORK_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int DryBlockWorkVac(bool isVacOn = true, bool isChkIn = true, long lDelay = 0)
        {
            int[] OnOut = new int[2];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC].lVacOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC].lBlowOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_NONE;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP;
                    OnOut[1] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_ON;

                    OffOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_BLOW;

                    OnIn[0] = (int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;
                    nErrNo[0] = (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_ON;
                }
            }
            else
            {
                OffOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP;
                OffOut[1] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_ON;
                OffOut[2] = (int)IODF.OUTPUT.DRY_BLOCK_BLOW;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_OFF;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int DryBlockWorkBlow(bool isBlowOn = true, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[2];
            int[] OffOut = new int[3];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC].lBlowOffDelay;
            }

            if (isBlowOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                OnOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP;
                OnOut[1] = (int)IODF.OUTPUT.DRY_BLOCK_BLOW;


                OffOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_ON;
                OffOut[1] = (int)IODF.OUTPUT.NONE;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC;
                nErrNo[0] = (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_OFF;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;

                OffOut[0] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_PUMP;
                OffOut[1] = (int)IODF.OUTPUT.DRY_BLOCK_BLOW;
                OffOut[2] = (int)IODF.OUTPUT.DRY_BLOCK_VAC_ON;


                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_DRY_BLOCK_WORK_VAC_NOT_OFF;
            }
            
            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int MapStageVac(int nTableNo, bool isVacOn, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[2];
            int[] OffOut = new int[2];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isVacOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lVacOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lBlowOnDelay;
            }

            if (isVacOn) // 210907 ANALOG VAC SENSOR 연결필요
            {
                if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    OnOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP + ( 6 * nTableNo);
                    OnOut[1] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON + (6 * nTableNo);

                    OffOut[0] = (int)IODF.OUTPUT.MAP_ST_1_BLOW + (6 * nTableNo);
                    OffOut[1] = (int)IODF.OUTPUT.NONE;

                    OnIn[0] = (int)IODF.A_INPUT.NONE;
                    OffIn[0] = (int)IODF.A_INPUT.NONE;

                    nErrNo[0] = (int)ERDF.E_NONE;
                }
                else
                {
                    OnOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP + (6 * nTableNo);
                    OnOut[1] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON + (6 * nTableNo);

                    OffOut[0] = (int)IODF.OUTPUT.MAP_ST_1_BLOW + (6 * nTableNo);
                    OffOut[1] = (int)IODF.OUTPUT.NONE;

                    OnIn[0] = (int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo);
                    OffIn[0] = (int)IODF.A_INPUT.NONE;

                    nErrNo[0] = (int)ERDF.E_MAP_STAGE_1_WORK_VAC_NOT_ON + (5 * nTableNo);
                }
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OnOut[1] = (int)IODF.OUTPUT.NONE;

                OffOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON + (6 * nTableNo);
                OffOut[1] = (int)IODF.OUTPUT.MAP_ST_1_BLOW + (6 * nTableNo);

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                //220428 phj자연파기시 센서 볼지 말지 정할 것 
                //OffIn[0] = (int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo);
                OffIn[0] = (int)IODF.A_INPUT.NONE;

                nErrNo[0] = (int)ERDF.E_MAP_STAGE_1_WORK_VAC_NOT_OFF + (5 * nTableNo);
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

 
        protected int MapStageBlow(int nTableNo, bool isBlowOn, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[2];
            int[] OffOut = new int[2];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (lDelay == 0)
            {
                if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lBlowOnDelay;
                else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lBlowOffDelay;
            }

            if (isBlowOn)
            {
                OnOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP + (6 * nTableNo);
                OnOut[1] = (int)IODF.OUTPUT.MAP_ST_1_BLOW + (6 * nTableNo);

                OffOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON + (6 * nTableNo);
                OffOut[1] = (int)IODF.OUTPUT.NONE;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo);
                nErrNo[0] = (int)ERDF.E_MAP_STAGE_1_BLOW_ON_FAIL + (8 * nTableNo);
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OnOut[1] = (int)IODF.OUTPUT.NONE;

                OffOut[0] = (int)IODF.OUTPUT.MAP_ST_1_VAC_ON + (6 * nTableNo); 
                OffOut[1] = (int)IODF.OUTPUT.MAP_ST_1_BLOW + (6 * nTableNo);

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int MapStageAirKnife(int nTableNo, bool isBlowOn, long lDelay = 0, bool isChkIn = true)
        {
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            //if (lDelay == 0)
            //{
            //    if (isBlowOn) lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lBlowOnDelay;
            //    else lDelay = ConfigMgr.Inst.Cfg.Vac[(int)IODF.A_INPUT.MAP_STG_1_WORK_VAC + (4 * nTableNo)].lBlowOffDelay;
            //}

            if (isBlowOn)
            {
                OnOut[0] = (int)IODF.OUTPUT.MAP_ST_AIR_KNIFE_1 + nTableNo;

                OffOut[0] = (int)IODF.OUTPUT.NONE;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_NONE;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;

                OffOut[0] = (int)IODF.OUTPUT.MAP_ST_AIR_KNIFE_1 + nTableNo;

                OnIn[0] = (int)IODF.A_INPUT.NONE;
                OffIn[0] = (int)IODF.A_INPUT.NONE;
                nErrNo[0] = (int)ERDF.E_NONE;
            }

            int nRetV = RunLibCyl.VacuumListOn(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay, isChkIn);
            return nRetV;
        }

        protected int BottomVisionAlignFwdBwd(bool isFwd, long lDelay = 0)
        {
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isFwd)
            {
                OnOut[0] = (int)IODF.OUTPUT.BTM_VISION_ALIGN_FWD;
                OffOut[0] = (int)IODF.OUTPUT.BTM_VISION_ALIGN_BWD;
                OnIn[0] = (int)IODF.INPUT.BALL_VISION_ALIGN_FWD;
                OffIn[0] = (int)IODF.INPUT.BALL_VISION_ALIGN_BWD;
                nErrNo[0] = (int)ERDF.E_BALL_VISION_ALIGN_FWD;
            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.BTM_VISION_ALIGN_BWD;
                OffOut[0] = (int)IODF.OUTPUT.BTM_VISION_ALIGN_FWD;
                OnIn[0] = (int)IODF.INPUT.BALL_VISION_ALIGN_BWD;
                OffIn[0] = (int)IODF.INPUT.BALL_VISION_ALIGN_FWD;
                nErrNo[0] = (int)ERDF.E_BALL_VISION_ALIGN_BWD;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }
        #endregion

        #region PICKER TOOLS
        protected void PadVacOnOff(int nPkNo, int nPadNo, bool bIsOn)
        {
            if (bIsOn)
            {
                //블로우 오프, 배큠 온
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo * 2), 3, 0);
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo * 2), 2, 1);
            }
            else
            {
                //배큠 오프
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo * 2), 2, 0);
            }
        }
        protected void PadVacOnOff(int nPkNo, int[] nPadNo, bool bIsOn)
        {
            for (int i = 0; i < nPadNo.Length; i++)
            {
                
            }
            if (bIsOn)
            {
                for (int i = 0; i < nPadNo.Length; i++)
                {
                    //블로우 오프, 배큠 온
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo[i] * 2), 3, 0);
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo[i] * 2), 2, 1);
                }
            }
            else
            {
                for (int i = 0; i < nPadNo.Length; i++)
                {
                    //배큠 오프
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * 20) + (nPadNo[i] * 2), 2, 0);   
                }
            }
        }
        protected void PadBlowOnOff(int nPkNo, int nPadNo, bool bIsOn)
        {
            if (bIsOn)
            {
                //배큠 오프, 블로우 온
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo * 2), 2, 0);
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo * 2), 3, 1);
            }
            else
            {
                //배큠 오프, 블로우 오프
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo * 2), 2, 0);
                CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo * 2), 3, 0);
            }
        }

        protected void PadBlowOnOff(int nPkNo, int[] nPadNo, bool bIsOn)
        {
            if (bIsOn)
            {
                for (int i = 0; i < nPadNo.Length; i++)
                {
                    //배큠 오프, 블로우 온
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo[i] * 2), 2, 0);
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo[i] * 2), 3, 1);
                }
            }
            else
            {
                for (int i = 0; i < nPadNo.Length; i++)
                {
                    //배큠 오프, 블로우 오프
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo[i] * 2), 2, 0);
                    CAXM.AxmSignalWriteOutputBit(24 + (nPkNo * CFG_DF.MAX_PICKER_PAD_CNT * 2) + (nPadNo[i] * 2), 3, 0);   
                }
            }
        }

        protected bool IsVacOn(int nPkNo, int nPadNo)
        {
            int nInputNo = (int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + (CFG_DF.MAX_PICKER_PAD_CNT * nPkNo) + nPadNo;
            return ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio)
                < ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelLow;
        }

        protected bool IsVacOn(int nPkNo, int[] nPadNo)
        {
            bool bRtn = true;
            int nInputNo = 0;
            for (int i = 0; i < nPadNo.Length; i++)
            {
                nInputNo = (int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM + (CFG_DF.MAX_PICKER_PAD_CNT * nPkNo) + nPadNo[i];
                bRtn &= ((GbVar.GB_AINPUT[nInputNo] - ConfigMgr.Inst.Cfg.Vac[nInputNo].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nInputNo].dRatio)
                < ConfigMgr.Inst.Cfg.Vac[nInputNo].dVacLevelLow;
            }
            return bRtn;
        }
        #endregion
        
        #region UNLOADING TABLE
        protected int TrayStageAlignClamp(int nTableNo, bool isClamp, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isClamp)
            {
                // GOOD 1
                if (nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_FWD;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_BWD;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_ALIGN_FWD_FAIL;
                }
                // GOOD 2
                else if (nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_FWD;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_BWD;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_ALIGN_FWD_FAIL;
                }
                // REWORK
                else if (nTableNo == 2)
                {
                    OnOut[0] = (int)IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.RW_TRAY_STG_ALIGN_FWD;
                    OffIn[0] = (int)IODF.INPUT.RW_TRAY_STG_ALIGN_BWD;
                    nErrNo[0] = (int)ERDF.E_RW_TRAY_STAGE_ALIGN_FWD_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }
            else
            {
                // GOOD 1
                if (nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_BWD;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_FWD;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_ALIGN_BWD_FAIL;
                }
                // GOOD 2
                else if (nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_BWD;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_FWD;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_ALIGN_BWD_FAIL;
                }
                // REWORK
                else if (nTableNo == 2)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP;
                    OnIn[0] = (int)IODF.INPUT.RW_TRAY_STG_ALIGN_BWD;
                    OffIn[0] = (int)IODF.INPUT.RW_TRAY_STG_ALIGN_FWD;
                    nErrNo[0] = (int)ERDF.E_RW_TRAY_STAGE_ALIGN_BWD_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected int TrayStageGrip(int nTableNo, bool isGrip, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isGrip)
            {
                // GOOD 1
                if (nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_GRIP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_CLOSE;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_OPEN;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_GRIP_FAIL;
                }
                // GOOD 2
                else if (nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_GRIP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_CLOSE;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_OPEN;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_GRIP_FAIL;
                }
                // REWORK
                else if (nTableNo == 2)
                {
                    OnOut[0] = (int)IODF.OUTPUT.RW_TRAY_STG_GRIP;
                    OffOut[0] = (int)IODF.OUTPUT.NONE;
                    OnIn[0] = (int)IODF.INPUT.RW_TRAY_STG_GRIP_CLOSE;
                    OffIn[0] = (int)IODF.INPUT.RW_TRAY_STG_GRIP_OPEN;
                    nErrNo[0] = (int)ERDF.E_RW_TRAY_STAGE_GRIP_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }
            else
            {
                // GOOD 1
                if (nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_GRIP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_OPEN;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_CLOSE;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_UNGRIP_FAIL;
                }
                // GOOD 2
                else if (nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_GRIP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_OPEN;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_CLOSE;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_UNGRIP_FAIL;
                }
                // REWORK
                else if (nTableNo == 2)
                {
                    OnOut[0] = (int)IODF.OUTPUT.NONE;
                    OffOut[0] = (int)IODF.OUTPUT.RW_TRAY_STG_GRIP;
                    OnIn[0] = (int)IODF.INPUT.RW_TRAY_STG_GRIP_OPEN;
                    OffIn[0] = (int)IODF.INPUT.RW_TRAY_STG_GRIP_CLOSE;
                    nErrNo[0] = (int)ERDF.E_RW_TRAY_STAGE_UNGRIP_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }
            
            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected bool SafetyCheckTrayTableUpDownCyl()
        {
            return GbFunc.IsGdTrayTableYMoveSafe();
        }

        protected bool SafetyCheckTrayTableMove(int nTableNo, int nTargetPosIdx)
        {
            return GbFunc.IsGdTrayTableYMoveSafe(nTableNo, nTargetPosIdx);
        }

        protected int GoodTrayStageUpDown(int nTableNo, bool isUp, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isUp)
            {
                if(nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_UP;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_UP;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_DOWN;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_UP_FAIL;
                }
                else if(nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_UP;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_UP;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_DOWN;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_UP_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }
            else
            {
                if(nTableNo == 0)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_1_STG_UP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_DOWN;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_1_STG_UP;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_1_DOWN_FAIL;//220602
                }
                else if(nTableNo == 1)
                {
                    OnOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN;
                    OffOut[0] = (int)IODF.OUTPUT.GD_TRAY_2_STG_UP;
                    OnIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_DOWN;
                    OffIn[0] = (int)IODF.INPUT.GD_TRAY_2_STG_UP;
                    nErrNo[0] = (int)ERDF.E_GD_TRAY_STAGE_2_DOWN_FAIL;
                }
                else
                {
                    return (int)ERDF.E_WRONG_TRAY_STAGE_NUMBER;
                }
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        /// <summary>
        /// 굿 트레이 스테이지가 업 상태인지 다운 상태인지 확인합니다.
        /// </summary>
        /// <param name="nTableNo"></param>
        /// <param name="bIsUp"></param>
        /// <returns></returns>
        protected bool CheckTrayStage(int nTableNo, bool bIsUp)
        {
            if (nTableNo == 0)
            {
                if (bIsUp)
                {
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == 1 &&
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_UP] == 0 &&
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_STG_DOWN] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_UP] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN] == 1)
                    {
                        return true;
                    }
                }
            }
            else if (nTableNo == 1)
            {
                if (bIsUp)
                {
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] == 1 &&
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] == 0)
                    {
                        return true;
                    }
                }
                else
                {
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_UP] == 0 &&
                        GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_STG_DOWN] == 1 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_UP] == 0 &&
                        GbVar.GB_OUTPUT[(int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN] == 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        #endregion
        
        #region UNLOADING TRANSFER
        
        protected int TrayPickerAtcClamp(bool isClamp, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[4];
            int[] OffIn = new int[4];
            int[] nErrNo = new int[2];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            if (isClamp)
            {
                OnOut[0] = (int)IODF.OUTPUT.TRAY_PK_ATC_CLAMP;
                OffOut[0] = (int)IODF.OUTPUT.NONE;

                OnIn[0] = (int)IODF.INPUT.TRAY_PK_FRONT_CLAMP;
                OnIn[1] = (int)IODF.INPUT.TRAY_PK_REAR_CLAMP;

                OffIn[0] = (int)IODF.INPUT.NONE;
                OffIn[1] = (int)IODF.INPUT.NONE;
                OffIn[2] = (int)IODF.INPUT.NONE;
                OffIn[3] = (int)IODF.INPUT.NONE;

                nErrNo[0] = (int)ERDF.E_TRAY_PK_CLAMP_FAIL;

            }
            else
            {
                OnOut[0] = (int)IODF.OUTPUT.NONE;
                OffOut[0] = (int)IODF.OUTPUT.TRAY_PK_ATC_CLAMP;

                OnIn[0] = (int)IODF.INPUT.NONE;
                OnIn[1] = (int)IODF.INPUT.NONE;
                OnIn[2] = (int)IODF.INPUT.NONE;
                OnIn[3] = (int)IODF.INPUT.NONE;

                OffIn[0] = (int)IODF.INPUT.TRAY_PK_FRONT_CLAMP;
                OffIn[1] = (int)IODF.INPUT.TRAY_PK_REAR_CLAMP;

                nErrNo[0] = (int)ERDF.E_TRAY_PK_UNCLAMP_FAIL;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }
        #endregion
        
        #region UNLOADING ELEVATOR
        protected int TrayElevStackerUpDown(int nElevNo, bool isUp, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[2];
            int[] OffIn = new int[2];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            switch (nElevNo)
            {
                case 0:
                    {
                        if (isUp)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP;
                            OffOut[0] = (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN;
                            OnIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_UP;
                            OnIn[1] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_UP;
                            OffIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_DOWN;
                            OffIn[1] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_DOWN;
                            nErrNo[0] = (int)ERDF.E_GD_1_TRAY_ELV_STACKER_UP_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN;
                            OffOut[0] = (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP;
                            OnIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_DOWN;
                            OnIn[1] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_DOWN;
                            OffIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_UP;
                            OffIn[1] = (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_UP;
                            nErrNo[0] = (int)ERDF.E_GD_1_TRAY_ELV_STACKER_DOWN_FAIL;
                        }
                    }
                    break;
                case 1:
                    {
                        if (isUp)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP;
                            OffOut[0] = (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN;
                            OnIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_UP;
                            OnIn[1] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_UP;
                            OffIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_DOWN;
                            OffIn[1] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_DOWN;
                            nErrNo[0] = (int)ERDF.E_GD_2_TRAY_ELV_STACKER_UP_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN;
                            OffOut[0] = (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP;
                            OnIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_DOWN;
                            OnIn[1] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_DOWN;
                            OffIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_UP;
                            OffIn[1] = (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_UP;
                            nErrNo[0] = (int)ERDF.E_GD_2_TRAY_ELV_STACKER_DOWN_FAIL;

                        }
                    }
                    break;
                case 2:
                    {
                        if (isUp)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP;
                            OffOut[0] = (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN;
                            OnIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_UP;
                            OnIn[1] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_UP;
                            OffIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_DOWN;
                            OffIn[1] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_DOWN;
                            nErrNo[0] = (int)ERDF.E_RW_TRAY_ELV_STACKER_UP_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN;
                            OffOut[0] = (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP;
                            OnIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_DOWN;
                            OnIn[1] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_DOWN;
                            OffIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_UP;
                            OffIn[1] = (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_UP;
                            nErrNo[0] = (int)ERDF.E_RW_TRAY_ELV_STACKER_DOWN_FAIL;
                        }
                    }
                    break;
                case 3:
                    {
                        if (isUp)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP;
                            OffOut[0] = (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN;
                            OnIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_UP;
                            OnIn[1] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_UP;
                            OffIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_DOWN;
                            OffIn[1] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_DOWN;
                            nErrNo[0] = (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN;
                            OffOut[0] = (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP;
                            OnIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_DOWN;
                            OnIn[1] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_DOWN;
                            OffIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_UP;
                            OffIn[1] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_UP;
                            nErrNo[0] = (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_DOWN_FAIL;
                        }
                    }
                    break;
                case 4:
                    {

                        if (isUp)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP;
                            OffOut[0] = (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN;
                            OnIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_UP;
                            OnIn[1] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_UP;
                            OffIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_DOWN;
                            OffIn[1] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_DOWN;
                            nErrNo[0] = (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN;
                            OffOut[0] = (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP;
                            OnIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_DOWN;
                            OnIn[1] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_DOWN;
                            OffIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_UP;
                            OffIn[1] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_UP;
                            nErrNo[0] = (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_DOWN_FAIL;
                        }
                    }
                    break;
                default:
                    break;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected int TrayElevGuideFwdBwd(int nElevNo, bool isFwd, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            switch (nElevNo)
            {
                case 0:
                    {
                        if (isFwd)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_1_ELV_STOPPER_FWD;
                            OffOut[0] = (int)IODF.OUTPUT.GD_1_ELV_STOPPER_BWD;
                            OnIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_GUIDE_FWD;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_GD_1_TRAY_ELV_GUIDE_FWD_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_1_ELV_STOPPER_BWD;
                            OffOut[0] = (int)IODF.OUTPUT.GD_1_ELV_STOPPER_FWD;
                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_GUIDE_FWD;
                            nErrNo[0] = (int)ERDF.E_GD_1_TRAY_ELV_GUIDE_BWD_FAIL;
                        }
                    }
                    break;
                case 1:
                    {
                        if (isFwd)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_2_ELV_STOPPER_FWD;
                            OffOut[0] = (int)IODF.OUTPUT.GD_2_ELV_STOPPER_BWD;
                            OnIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_GUIDE_FWD;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_GD_2_TRAY_ELV_GUIDE_FWD_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD_2_ELV_STOPPER_BWD;
                            OffOut[0] = (int)IODF.OUTPUT.GD_2_ELV_STOPPER_FWD;
                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_GUIDE_FWD;
                            nErrNo[0] = (int)ERDF.E_GD_2_TRAY_ELV_GUIDE_BWD_FAIL;
                        }
                    }
                    break;
                case 2:
                    {
                        if (isFwd)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_ELV_STOPPER_FWD;
                            OffOut[0] = (int)IODF.OUTPUT.RW_ELV_STOPPER_BWD;
                            OnIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_GUIDE_FWD;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_RW_TRAY_ELV_GUIDE_FWD_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_ELV_STOPPER_BWD;
                            OffOut[0] = (int)IODF.OUTPUT.RW_ELV_STOPPER_FWD;
                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_GUIDE_FWD;
                            nErrNo[0] = (int)ERDF.E_RW_TRAY_ELV_GUIDE_BWD_FAIL;
                        }
                    }
                    break;
                case 3:
                    {
                        if (isFwd)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD;
                            OffOut[0] = (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD;
                            OnIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_GUIDE_FWD;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_EMPTY_1_TRAY_ELV_GUIDE_FWD_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD;
                            OffOut[0] = (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD;
                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_GUIDE_FWD;
                            nErrNo[0] = (int)ERDF.E_EMPTY_1_TRAY_ELV_GUIDE_BWD_FAIL;
                        }
                    }
                    break;
                case 4:
                    {
                        if (isFwd)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD;
                            OffOut[0] = (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD;
                            OnIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_GUIDE_FWD;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_EMPTY_2_TRAY_ELV_GUIDE_FWD_FAIL;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD;
                            OffOut[0] = (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD;
                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_GUIDE_FWD;
                            nErrNo[0] = (int)ERDF.E_EMPTY_2_TRAY_ELV_GUIDE_BWD_FAIL;
                        }
                    }
                    break;
                default:
                    break;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected bool IsTrayGuideFwd(int nPortNo)
        {
            int[] OnIn = new int[5]
            {
                (int)IODF.INPUT.GD_1_TRAY_ELV_GUIDE_FWD,
                (int)IODF.INPUT.GD_2_TRAY_ELV_GUIDE_FWD,
                (int)IODF.INPUT.RW_TRAY_ELV_GUIDE_FWD,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_GUIDE_FWD,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_GUIDE_FWD,
            };
            return GbVar.GB_INPUT[OnIn[nPortNo]] == 1;
        }

        protected bool IsTrayResidueSensorOn(int nPortNo)
        {
            int[] OnIn = new int[5]
            {
                (int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK,
                (int)IODF.INPUT.GD_2_TRAY_ELV_RESIDUAL_QTY_CHECK,
                (int)IODF.INPUT.RW_TRAY_ELV_RESIDUAL_QTY_CHECK,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_RESIDUAL_QTY_CHECK,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_RESIDUAL_QTY_CHECK,
            };

            return GbVar.GB_INPUT[OnIn[nPortNo]] == 1;
        }

        /// <summary>
        /// resudual센서는 B접임으로 트레이 감지 시 Off됨
        /// </summary>
        /// <param name="nElevNo"></param>
        /// <param name="lTimeOut"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int IsElvResidualOn(int nElevNo, long lTimeOut, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif

            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            switch (nElevNo)
            {
                case 0:
                    {
                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.GD_1_TRAY_ELV_RESIDUAL_QTY_CHECK;

                        nErrNo[0] = (int)ERDF.E_GD_1_ELV_RESIDUAL_DETECT_FAIL;
                    }
                    break;
                case 1:
                    {
                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.GD_2_TRAY_ELV_RESIDUAL_QTY_CHECK;

                        nErrNo[0] = (int)ERDF.E_GD_2_ELV_RESIDUAL_DETECT_FAIL;
                    }
                    break;
                case 2:
                    {
                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.RW_TRAY_ELV_RESIDUAL_QTY_CHECK;

                        nErrNo[0] = (int)ERDF.E_RW_ELV_RESIDUAL_DETECT_FAIL;
                    }
                    break;
                case 3:
                    {
                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.EMTY_1_TRAY_ELV_RESIDUAL_QTY_CHECK;

                        nErrNo[0] = (int)ERDF.E_EMPTY_1_ELV_RESIDUAL_DETECT_FAIL;
;
                    }
                    break;
                case 4:
                    {
                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.EMTY_2_TRAY_ELV_RESIDUAL_QTY_CHECK;

                        nErrNo[0] = (int)ERDF.E_EMPTY_2_ELV_RESIDUAL_DETECT_FAIL;
                    }
                    break;
                default:
                    break;
            }

            int nRetV = RunLibCyl.IsInputOnOff(OnIn, OffIn, lTimeOut, nErrNo, lDelay);
            return nRetV;
        }
        protected int UnloaderConvCw(int nElevNo, bool isCw, long lDelay = 0)
        {

#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[1];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            switch (nElevNo)
            {
                case 0://GD1
                    {
                        if (isCw)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD1_CONV_CW;//컨베이어 CW연결

                            OffOut[0] = (int)IODF.OUTPUT.GD1_CONV_CCW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD1_CONV_CCW;//컨베이어 CCW연결

                            OffOut[0] = (int)IODF.OUTPUT.GD1_CONV_CW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                    }
                    break;
                case 1://GD2
                    {
                        if (isCw)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD2_CONV_CW;//컨베이어 CW연결

                            OffOut[0] = (int)IODF.OUTPUT.GD2_CONV_CCW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.GD2_CONV_CCW;//컨베이어 CCW연결

                            OffOut[0] = (int)IODF.OUTPUT.GD2_CONV_CW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                    }
                    break;
                case 2://RW
                    {
                        if (isCw)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_CONV_CW;//컨베이어 CW연결

                            OffOut[0] = (int)IODF.OUTPUT.RW_CONV_CCW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.RW_CONV_RESET;//컨베이어 CCW연결

                            OffOut[0] = (int)IODF.OUTPUT.RW_CONV_CW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                    }
                    break;
                case 3://EMT1
                    {
                        if (isCw)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPTY1_CONV_CW;//컨베이어 CW연결

                            OffOut[0] = (int)IODF.OUTPUT.EMPTY1_CONV_CCW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPTY1_CONV_CCW;//컨베이어 CCW연결

                            OffOut[0] = (int)IODF.OUTPUT.EMPTY1_CONV_CW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                    }
                    break;
                case 4://EMT2
                    {
                        if (isCw)
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPTY2_CONV_CW;//컨베이어 CW연결

                            OffOut[0] = (int)IODF.OUTPUT.EMPTY2_CONV_CCW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                        else
                        {
                            OnOut[0] = (int)IODF.OUTPUT.EMPTY2_CONV_CCW;//컨베이어 CCW연결

                            OffOut[0] = (int)IODF.OUTPUT.EMPTY2_CONV_CW;

                            OnIn[0] = (int)IODF.INPUT.NONE;
                            OffIn[0] = (int)IODF.INPUT.NONE;
                            nErrNo[0] = (int)ERDF.E_NONE;
                        }
                    }
                    break;
                default:
                    break;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }
        protected int UnloaderConvStop(int nElevNo, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] OnOut = new int[1];
            int[] OffOut = new int[2];
            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnOut.Populate(NOT_USED);
            OffOut.Populate(NOT_USED);
            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);

            switch (nElevNo)
            {
                case 0://GD1
                    {
                        OnOut[0] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.GD1_CONV_CW;
                        OffOut[1] = (int)IODF.OUTPUT.GD1_CONV_CCW;

                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_NONE;
                    }
                    break;
                case 1://GD1
                    {
                        OnOut[0] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.GD2_CONV_CW;
                        OffOut[1] = (int)IODF.OUTPUT.GD2_CONV_CCW;

                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_NONE;
                    }
                    break;
                case 2://RW
                    {
                        OnOut[0] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.RW_CONV_CW;
                        OffOut[1] = (int)IODF.OUTPUT.RW_CONV_CCW;

                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_NONE;
                    }
                    break;
                case 3://EMT1
                    {
                        OnOut[0] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.EMPTY1_CONV_CW;
                        OffOut[1] = (int)IODF.OUTPUT.EMPTY1_CONV_CCW;

                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_NONE;
                    }
                    break;
                case 4://EMT2
                    {
                        OnOut[0] = (int)IODF.OUTPUT.NONE;

                        OffOut[0] = (int)IODF.OUTPUT.EMPTY2_CONV_CW;
                        OffOut[1] = (int)IODF.OUTPUT.EMPTY2_CONV_CCW;

                        OnIn[0] = (int)IODF.INPUT.NONE;
                        OffIn[0] = (int)IODF.INPUT.NONE;
                        nErrNo[0] = (int)ERDF.E_NONE;
                    }
                    break;
                default:
                    break;
            }

            int nRetV = RunLibCyl.CylListMove(OnOut, OffOut, OnIn, OffIn, nErrNo, lDelay);
            return nRetV;
        }

        protected int VacBlowCheck(int nAIO, bool bVac, int nErr, long lTimeOut, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif

            int[] OnIn = new int[1];
            int[] OffIn = new int[1];
            int[] nErrNo = new int[1];

            OnIn.Populate(NOT_USED);
            OffIn.Populate(NOT_USED);
            nErrNo.Populate(NOT_USED);


            if (bVac)
            {
                OnIn[0] = nAIO;
            }
            else
            {
                OffIn[0] = nAIO;
            }

            nErrNo[0] = nErr;

            int nRetV = RunLib.IsVacBlowOnOff(OnIn, OffIn, lTimeOut, nErrNo, lDelay);
            return nRetV;
        }
        #endregion

        #endregion

        #region 설비 전용 이동 함수
        #region MGZ LD ELEV

        //protected int MovePosMgzLdElvY(int nPosNo, long lDelay = 0)
        //{
        //    int[] nAxisArray = new int[1];
        //    double[] dPosArray = new double[1];
        //    double[] dSpeedArray = new double[1];
        //    double[] dAccArray = new double[1];
        //    double[] dDecArray = new double[1];

        //    nAxisArray[0] = (int)SVDF.AXES.SPARE_63;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        //dPosArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        //dSpeedArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
        //        //dAccArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
        //        //dDecArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
     
        //        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
        //        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
        //        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
        //    }

        //    if (RunLib.IsCurRunCmdStart())
        //    {
        //        m_strMotPos = "MOVE POSITION";
        //        for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //        {
        //            m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
        //                                        SVDF.GetAxisName(nAxisArray[nCnt]),
        //                                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
        //                                        dPosArray[nCnt]);
        //        }
        //    }

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        if (nAxisArray[nCnt] < 0)
        //            continue;
        //        int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
        //        if (FNC.IsErr(nSafety)) return nSafety;
        //    }

        //    return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        //}

        protected int MovePosMgzLdElvZ(int nPosNo, double dAxisZOffset = 0.0, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAGAZINE_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
               
                //dPosArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                //dSpeedArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                //dAccArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                //dDecArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            dPosArray[0] += dAxisZOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }
            
            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        //protected int MovePosMgzLdElvYZ(int nPosNo, double dAxisZOffset = 0.0, long lDelay = 0)
        //{
        //    int[] nAxisArray = new int[2];
        //    double[] dPosArray = new double[2];
        //    double[] dSpeedArray = new double[2];
        //    double[] dAccArray = new double[2];
        //    double[] dDecArray = new double[2];

        //    nAxisArray[0] = (int)SVDF.AXES.SPARE_63;
        //    nAxisArray[1] = (int)SVDF.AXES.MAGAZINE_ELV_Z;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        //dPosArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        //dSpeedArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
        //        //dAccArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
        //        //dDecArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];

        //        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
        //        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
        //        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
        //    }

        //    if (RunLib.IsCurRunCmdStart())
        //    {
        //        m_strMotPos = "MOVE POSITION";
        //        for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //        {
        //            m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
        //                                        SVDF.GetAxisName(nAxisArray[nCnt]),
        //                                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
        //                                        dPosArray[nCnt]);
        //        }
        //    }

        //    dPosArray[1] += dAxisZOffset;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], nPosNo);
        //        if (FNC.IsErr(nSafety)) return nSafety;
        //    }

        //    return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        //}

        //public bool IsInPosMgzLdElvY(int nPosNo)
        //{
        //    int[] nAxisArray = new int[1];
        //    double[] dPosArray = new double[1];
        //    double[] dCurrentPosArray = new double[1];

        //    nAxisArray[0] = (int)SVDF.AXES.SPARE_63;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
        //    }

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public bool IsInPosMgzLdElvZ(int nPosNo, double dAxisZOffset = 0.0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAGAZINE_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            dPosArray[0] += dAxisZOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region MGZ ULD ELEV


        protected int MovePosMgzUldElvZ(int nPosNo, double dAxisZOffset = 0.0, double dSpeed = 0.0, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAGAZINE_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            dPosArray[0] += dAxisZOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }


        public bool IsInPosMgzUldElvZ(int nPosNo, double dAxisZOffset = 0.0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAGAZINE_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            dPosArray[0] += dAxisZOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region MAP PUSHER

        // 삭제 됨
        /*
        protected int MovePosMapPusher(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.NONE;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosMapPusher(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.NONE;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        */
        #endregion

        #region MGZ LD RAIL

        /// <summary>
        /// 동시 동작으로 변경 함
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MovePosLdRailY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
            nAxisArray[1] = (int)SVDF.AXES.LD_RAIL_Y_REAR;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosLdRailY_Front(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_FRONT;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosLdRailY_Rear(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_REAR;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosLdRailT(int nPosNo, double dAngle, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_T;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

			// [2022.05.31.kmlee] +가 최종
            dPosArray[0] += dAngle;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosLdRailYT(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
            nAxisArray[1] = (int)SVDF.AXES.LD_RAIL_Y_REAR;
            nAxisArray[2] = (int)SVDF.AXES.LD_RAIL_T;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// 하부 바코드 Y축
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MovePosBarcodeY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BARCODE_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosLdRailY(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_FRONT;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosLdRailT(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_T;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosLdRailYT(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
            nAxisArray[1] = (int)SVDF.AXES.LD_RAIL_T;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosBarcodeY(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BARCODE_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }


        #endregion

        #region LD VISION
        protected int MovePosLdVisionX(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_VISION_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosLdVisionX(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.LD_VISION_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region STRIP PICKER

        protected int MovePosStripPkX(int nPosNo, double dOffset = 0.0f,  long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffset;
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosStripPkXY(int nPosNoPkY, int nPosNoVisX, double dOffsetZ = 0.0, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.LD_VISION_X;
            //nAxisArray[2] = (int)SVDF.AXES.STRIP_PK_Z;

            {
                dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNoPkY];
                dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNoPkY];
                dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNoPkY];
                dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNoPkY];

                dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[nPosNoVisX];
                dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[nPosNoVisX];
                dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[nPosNoVisX];
                dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[nPosNoVisX];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNoPkY),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNoPkY);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosStripPkXBarcode(int nPosNoPkY, int nPosNoBarcodeX, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.BARCODE_Y;

            {
                dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNoPkY];
                dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNoPkY];
                dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNoPkY];
                dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNoPkY];

                dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[nPosNoBarcodeX];
                dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[nPosNoBarcodeX];
                dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[nPosNoBarcodeX];
                dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[nPosNoBarcodeX];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNoPkY),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNoPkY);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosStripPkZ(int nPosNo, double dOffsetZ = 0.0, bool bIsSlow = false, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo]+ dOffsetZ, nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bIsSlow)
            {
                dSpeedArray[0] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PICKER_Z_SLOW_DOWN_VEL].dValue;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosStripPkXZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.STRIP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosStripPkX(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

             
        public bool IsInPosStripPkX(int nPosNo, double dOffset)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffset; // 오프셋 적용
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosStripPkZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosStripPkXZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.STRIP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.STRIP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region UNIT PICKER

        protected int MovePosUnitPkX(int nPosNo, bool bIsSlow = false, double dSpeed = 10, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bIsSlow)
            {
                dSpeedArray[0] = dSpeed;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosUnitPkZ(int nPosNo, double dOffsetZ = 0.0, bool bIsSlow = false, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffsetZ, nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bIsSlow)
            {
                dSpeedArray[0] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_VEL].dValue;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosUnitPkXZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.UNIT_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosUnitPkX(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosUnitPkZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosUnitPkXZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.UNIT_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.UNIT_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region DRY BLOCK STAGE
        protected int MovePosDryBlockStgX(int nPosNo, bool bIsSlow = false, double dSpeed = 5, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(lDelay)) return FNC.BUSY;
            return FNC.SUCCESS;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.DRY_BLOCK_STG_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if(bIsSlow)
            {
                dSpeedArray[0] = dSpeed;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosDryBlockStgX(int nPosNo)
        {
#if _NOTEBOOK
            return true;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.DRY_BLOCK_STG_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region MAP PICKER

        protected int MovePosMapPkX(int nPosNo, long lDelay = 0, bool bSkipMapPkInterlock = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, true, true, bSkipMapPkInterlock);
        }

        /// <summary>
        /// CONFIG의 OPTNUM에 저장된 위치로 이동 (속도와 가감속은 READY 것을 사용)
        /// </summary>
        /// <param name="eOptNum"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipMapPkInterlock"></param>
        /// <returns></returns>
        protected int MoveCfgPosMapPkX(OPTNUM eOptNum, long lDelay = 0, bool bSkipMapPkInterlock = false)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                // 속도와 가감속은 READY를 사용
                dPosArray[nCnt] = ConfigMgr.Inst.Cfg.itemOptions[(int)eOptNum].dValue;
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[0];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[0];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[0];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                string.Format("CONFIG OPTNUM : {0}", eOptNum),
                                                dPosArray[nCnt]);
                }
            }

            // OPTNUM 이동은 사용하지 않는다
            //for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            //{
            //    int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nOptNum);
            //    if (FNC.IsErr(nSafety)) return nSafety;
            //}

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, true, true, bSkipMapPkInterlock);
        }

        protected int MovePosMapPkZ(int nPosNo, double dOffsetZ = 0.0, bool bIsSlow = false, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffsetZ, nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bIsSlow)
            {
                dSpeedArray[0] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL].dValue;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) 
                    return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosMapVisionZ(int nPosNo, double dOffsetZ = 0.0, bool bIsSlow = false, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffsetZ, nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bIsSlow)
            {
                dSpeedArray[0] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_PICKER_Z_SLOW_DOWN_VEL].dValue;
                dAccArray[0] = dSpeedArray[0] * 10;
                dDecArray[0] = dSpeedArray[0] * 10;
            }

            dPosArray[0] += dOffsetZ;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// Top Align 이동 함수
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="nStageNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MoveTopAlignPosMapPkXY(int nPosNo, int nStageNo, long lDelay = 0)
        {
            int[] nPosArray = new int[2];
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            int nAlignNo = nPosNo - (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
            if (nStageNo == 0)
            {
                if (nAlignNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
                else
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
               
            }
            else
            {
                if (nAlignNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
                else
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
            }


            //nPosArray[0] = POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + nStageNo;
            //nPosArray[1] = nPosNo;  // MAP_STAGE_TOP_ALIGN_MARK1, MAP_STAGE_TOP_ALIGN_MARK2

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;

            for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
            {
                //dPosArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];
                dSpeedArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dVel[nPosArray[nAxisCnt]];
                dAccArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dAcc[nPosArray[nAxisCnt]];
                dDecArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dDec[nPosArray[nAxisCnt]];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nAxisCnt]), nPosArray[nAxisCnt]),
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// Top Align 이동 함수
        /// 2022.04.28 : T를 포함한 위치 이동
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="nStageNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MoveTopAlignPosMapPkXYT(int nPosNo, int nStageNo, long lDelay = 0)
        {
            int[] nPosArray = new int[3];
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];


            int nAlignNo = nPosNo - (int)POSDF.MAP_STAGE_TOP_ALIGN_MARK1;
            if (nStageNo == 0)
            {
                if (nAlignNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
                else
                {
                    //2022 11 15 HEP
                    //나중에는 그룹 피치 계산해서 위치 값 기입 하도록 변경하여야함 
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[RecipeMgr.Inst.Rcp.listMapGrpInfoL.Count - 1].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }

            }
            else
            {
                if (nAlignNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
                else
                {
                    //2022 11 15 HEP
                    //나중에는 그룹 피치 계산해서 위치 값 기입 하도록 변경하여야함 
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosX - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[RecipeMgr.Inst.Rcp.listMapGrpInfoR.Count - 1].dTopInspPosY - (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
                }
            }


            //nPosArray[0] = POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + nStageNo;
            //nPosArray[1] = nPosNo;  // MAP_STAGE_TOP_ALIGN_MARK1, MAP_STAGE_TOP_ALIGN_MARK2
            //nPosArray[2] = POSDF.MAP_STAGE_TOP_ALIGN_MARK1;

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;
            nAxisArray[2] = (int)SVDF.AXES.MAP_STG_1_T + nStageNo;

            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dPos[POSDF.MAP_STAGE_TOP_ALIGN_MARK1];

            for (int nAxisCnt = 0;  nAxisCnt < nAxisArray.Length; nAxisCnt++)
            {
                //dPosArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];
                dSpeedArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dVel[nPosArray[nAxisCnt]];
                dAccArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dAcc[nPosArray[nAxisCnt]];
                dDecArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dDec[nPosArray[nAxisCnt]];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nAxisCnt]), nPosArray[nAxisCnt]),
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// [2022.04.12.kmlee] 이름 변경함. Position에 따른 이동 함수랑 헷갈림
        /// Map Inspection 시 이동 함수
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="nStageNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MoveMapInspPosMapPkXY(int nPosNo, int nStageNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;

            if (nStageNo == 0)
            {
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY;

                dPosArray[0] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosArray[1] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }
            else
            {
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY;

                dPosArray[0] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dPosArray[1] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }

            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];

            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_MAP_VISION_START];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_MAP_VISION_START];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_MAP_VISION_START];

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[0]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)),
                        dPosArray[0]);

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[1]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[1]), POSDF.MAP_STAGE_MAP_VISION_START),
                        dPosArray[1]);
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], nPosNo + (2 * nStageNo));
                if (FNC.IsErr(nSafety)) return nSafety;
            }

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);

        }

        /// <summary>
        /// Inspection XY next inspection pos 이동
        /// </summary>
        /// <param name="nInspectionCount"></param>
        /// <param name="nStageNo"></param>
        /// <param name="lDelay"></param>
        /// <param name="bCheckInterlock"></param>
        /// <returns></returns>
        public int MovePosMapInspXYNext(int nMapGroupNo, int nInspectionCount, int nStageNo, long lDelay = 0, bool bCheckInterlock = true)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;

            if (nStageNo == 0)
            {
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[nMapGroupNo].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[nMapGroupNo].dTopInspPosY;

                dPosArray[0] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);
                dPosArray[1] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
            }
            else
            {
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[nMapGroupNo].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[nMapGroupNo].dTopInspPosY;

                dPosArray[0] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX / 2);
                dPosArray[1] -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY / 2);
            }

            #region SPEED / ACC
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];

            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_MAP_VISION_START];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_MAP_VISION_START];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_MAP_VISION_START];
            #endregion SPEED / ACC

            int nRow, nCol;
            double dStepOffsetX = 0, dStepOffsetY = 0;

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nStageNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.y;
                }
            }

            
            // 세로 모드 (좌측 상단부터 하단으로 이동)
            if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_MODE_VERTICAL_USE].bOptionUse)
            {
                nCol = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY);

                if (nCol % 2 == 0)
                {
                    nRow = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY);
                    
                    //if (nRow == (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY - 1))
                    //{
                    //    nRow = nRow - 1;
                    //    dStepOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
                    //}
                }
                else
                {
                    nRow = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY - (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY)) - 1;
                    
                    //if (nRow == (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY - 1))
                    //{
                    //    nRow = nRow - 1;
                    //    dStepOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
                    //}
                }

                //if (nCol == (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX - 1))
                //{
                //    nCol = nCol - 1;
                //    dStepOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
                //}
            }
            // 지그재그로 진행 (ㄹ자. 실제로 우측 상단부터 시작하여 좌측으로 이동. 가로 모드)
            else
            {
                nRow = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);

                if (nRow % 2 == 0)
                {
                    nCol = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);
                }
                else
                {
                    nCol = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX - (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX)) - 1;
                }
            }
            // 한 방향 진행
            //else
            //{
            //    nRow = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);
            //    nCol = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);
            //}

            dStepOffsetX = nCol * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;
            dStepOffsetY = nRow * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;


            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_MODE_VERTICAL_USE].bOptionUse)
            {
                // X는 OP 기준으로 왼쪽에서 오른쪽, Y는 상부에서 하부로
                dPosArray[0] += dStepOffsetX;
                dPosArray[1] += dStepOffsetY;
            }
            else
            {
                // [2022.04.13.kmlee] 모터의 방향 변경. OP 기준으로 오른쪽에서 왼쪽으로. 물류 역방향 (네온 비젼 담당자 요청)
                //dPosArray[0] += dStepOffsetX;
                dPosArray[0] -= dStepOffsetX;
                dPosArray[1] += dStepOffsetY;
            }

            //double dInspStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            //double dInspStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];
            //if (nStageNo == 0)
            //{
            //    dInspStartPosX = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosX;
            //    dInspStartPosY = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].dTopInspPosY;

            //    dInspStartPosX -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
            //    dInspStartPosY -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            //}
            //else
            //{
            //    dInspStartPosX = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosX;
            //    dInspStartPosY = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].dTopInspPosY;

            //    dInspStartPosX -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
            //    dInspStartPosY -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            //}
            //dxy dpMovePos;
            //dpMovePos = ConfigMgr.Inst.GetMapStageInspCountAngleOffset(nStageNo, dInspStartPosX, dInspStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[0]),
                            POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)),
                            dPosArray[0]);

                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[1]),
                            POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[1]), POSDF.MAP_STAGE_MAP_VISION_START),
                            dPosArray[1]);
            }

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[0], POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo));
            if (FNC.IsErr(nSafety)) return nSafety;

            nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[1], POSDF.MAP_STAGE_MAP_VISION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }
        public int MovePosMapInspXYAnother(int nInspectionCount, int nStageNo, long lDelay = 0, bool bCheckInterlock = true)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;


            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];

            dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_MAP_VISION_START];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_MAP_VISION_START];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_MAP_VISION_START];

            int nRow, nCol;
            double dStepOffsetX = 0, dStepOffsetY = 0;

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nStageNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.y;
                }
            }


            // 세로 모드 (좌측 상단부터 하단으로 이동)
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_MODE_VERTICAL_USE].bOptionUse)
            {
                nCol = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY);

                if (nCol % 2 == 0)
                {
                    nRow = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY);

                    dStepOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
                }
                else
                {
                    nRow = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY - (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY) - 1;
                }

            }
            // 지그재그로 진행 (ㄹ자. 실제로 우측 상단부터 시작하여 좌측으로 이동. 가로 모드)
            else
            {
                nRow = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);

                if (nRow % 2 == 0)
                {
                    nCol = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);

                    dStepOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
                }
                else
                {
                    nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX - (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX) - 1;
                    
                }

            }
            // 한 방향 진행
            //else
            //{
            //    nRow = (nInspectionCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);
            //    nCol = (nInspectionCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountX);
            //}

            dStepOffsetX += nCol * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;
            dStepOffsetY += nRow * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;

            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_INSPECTION_MODE_VERTICAL_USE].bOptionUse)
            {
                // X는 OP 기준으로 왼쪽에서 오른쪽, Y는 상부에서 하부로
                dPosArray[0] += dStepOffsetX;
                dPosArray[1] += dStepOffsetY;
            }
            else
            {
                // [2022.04.13.kmlee] 모터의 방향 변경. OP 기준으로 오른쪽에서 왼쪽으로. 물류 역방향 (네온 비젼 담당자 요청)
                //dPosArray[0] += dStepOffsetX;
                dPosArray[0] -= dStepOffsetX;
                dPosArray[1] += dStepOffsetY;
            }

            double dInspStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            double dInspStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];

            dxy dpMovePos;
            dpMovePos = ConfigMgr.Inst.GetMapStageInspCountAngleOffset(nStageNo, dInspStartPosX, dInspStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;
            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[0]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)),
                        dPosArray[0]);

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[1]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[1]), POSDF.MAP_STAGE_MAP_VISION_START),
                        dPosArray[1]);
            }

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[0], POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo));
            if (FNC.IsErr(nSafety)) return nSafety;

            nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[1], POSDF.MAP_STAGE_MAP_VISION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }


        public int MovePosMapInspOnlyOneEdge(int nMapGroupNo, int nInspectionCount, int nStageNo, long lDelay = 0, bool bCheckInterlock = true)
        {

            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;
            nInspectionCount %= 4;

            if (nStageNo == 0)
            {
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[nMapGroupNo].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[nMapGroupNo].dTopInspPosY;
            }
            else
            {
            
                dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[nMapGroupNo].dTopInspPosX;
                dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[nMapGroupNo].dTopInspPosY;
            }
            
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];

            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_MAP_VISION_START];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_MAP_VISION_START];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_MAP_VISION_START];

            int nRow, nCol;
            double dStepOffsetX = 0, dStepOffsetY = 0;

            int nInspCount = nInspectionCount / 2;
            int nEdgeCount = nInspectionCount % 2; // 0 = top edge, 1 = btm edge

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nStageNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nStageNo].Info.xyTopAlignCorrectOffset.y;
                }
            }
            //예전소스
            //nCol = nInspectionCount / (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY * 2);

            //if (nCol % 2 == 0)//위에서 아래로
            //{
            //    nRow = (nInspectionCount % (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY * 2));
            //    if (nRow % 2 == 0)
            //    {
            //        dStepOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;

            //        dStepOffsetX += (nCol / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;
            //        dStepOffsetY += (nRow / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;
            //    }
            //    else
            //    {
            //        dStepOffsetX += (nCol / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;

            //        dStepOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
            //        dStepOffsetY += (nRow / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;
            //    }
            //}
            //else //아래에서 위로
            //{
            //    nRow = (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY * 2) - (nInspectionCount % (RecipeMgr.Inst.Rcp.MapTbInfo.nUnitInspCountY * 2)) - 1;

            //    if (nRow % 2 == 0)
            //    {
            //        dStepOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
            //        dStepOffsetX += (nCol / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;

            //        dStepOffsetY += (nRow / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;
            //    }
            //    else
            //    {
            //        dStepOffsetX = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX;
            //        dStepOffsetX += (nCol / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountX;

            //        dStepOffsetY = RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY;
            //        dStepOffsetY += (nRow / 2) * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY * RecipeMgr.Inst.Rcp.MapTbInfo.nInspChipViewCountY;
            //    }
            //}
            if (nInspectionCount == 1)
            {
                dStepOffsetX -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dStepOffsetY += (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }
            else if(nInspectionCount == 2)
            {
                dStepOffsetX += (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dStepOffsetY += (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }
            else if(nInspectionCount == 3)
            {
                dStepOffsetX += (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dStepOffsetY -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }
            else
            {
                // 0일경우
                dStepOffsetX -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeX / 2);
                dStepOffsetY -= (RecipeMgr.Inst.Rcp.MapTbInfo.dUnitSizeY / 2);
            }

            // X는 OP 기준으로 왼쪽에서 오른쪽, Y는 상부에서 하부로
            dPosArray[0] += dStepOffsetX;
            dPosArray[1] += dStepOffsetY;

            double dInspStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)];
            double dInspStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_MAP_VISION_START];

            //Angle why??
            dxy dpMovePos;
            dpMovePos = ConfigMgr.Inst.GetMapStageInspCountAngleOffset(nStageNo, dInspStartPosX, dInspStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;


            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[0]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo)),
                        dPosArray[0]);

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[1]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[1]), POSDF.MAP_STAGE_MAP_VISION_START),
                        dPosArray[1]);
            }

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[0], POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nStageNo));
            if (FNC.IsErr(nSafety)) return nSafety;

            nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[1], POSDF.MAP_STAGE_MAP_VISION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }


        protected int MovePosMapPkXZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosMapPkX(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosMapPkZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosMapVisionZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosMapPkXZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region MAP STAGE

        protected int MovePosMapStgY(int nStageNo, int nPosNo, bool bSlowMove = false, double dSlowVel = 10, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y + nStageNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }
            if (nPosNo == (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1 ||
                nPosNo == (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P2)
            {
                int nHeadNo = nPosNo - (int)POSDF.MAP_STAGE_UNIT_UNLOADING_P1;
                dPosArray[0] = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;

                if (nStageNo == 1)
                {
                    dPosArray[0] = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;
                }
            }
            if (bSlowMove == true)
            {
                dSpeedArray[0] = dSlowVel;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosMapStgT(int nStageNo, int nPosNo, bool bSlowMove = false, double dSlowVel = 10, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_T + nStageNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (bSlowMove == true)
            {
                dSpeedArray[0] = dSlowVel;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosMapStgT(int nStageNo, double dPos, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_T + nStageNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = dPos;
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.MAP_STAGE_TOP_ALIGN_MARK1];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.MAP_STAGE_TOP_ALIGN_MARK1];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.MAP_STAGE_TOP_ALIGN_MARK1];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.MAP_STAGE_TOP_ALIGN_MARK1),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.MAP_STAGE_TOP_ALIGN_MARK1);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }


        protected int MovePosMapStgY1(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosMapStgY2(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_2_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosMapStgY1Y2(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_2_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosMapStgY1(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosMapStgY2(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_2_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        public bool IsInPosMapStgT(int TableNO, int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            if (TableNO == 0)
            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_T;
            else
                nAxisArray[0] = (int)SVDF.AXES.MAP_STG_2_T;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        public bool IsInPosMapStgY1Y2(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_2_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region CHIP PICKER
        protected int MovePosChipPkZ(int nPosNo, int nHeadNo, int nPickerNo, double dOffset, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (2 * nPickerNo) + nHeadOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[nPosNo] + dOffset;
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosChipPkX(int nPosNo, int nHeadNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_X - (int)SVDF.AXES.CHIP_PK_1_X) * nHeadNo;
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosChipPkX1X2(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosChipPkX1(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosChipPkX2(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosChipPkX1X2(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_2_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsReadyPosChipPkZAxis(int nHeadNo)
        {
            int[] nAxisArray = new int[10];
            double[] dPosArray = new double[10];
            double[] dCurrentPosArray = new double[10];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = 0.0f;
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > 0.1f)
                {
                    return false;
                }
            }

            return true;
        }

        protected int MovePosChipPkAllXYR(int nPosNo, int nTableNo, int nHeadNo, int nPickerNo, int nPickUpTotCount, int nPickerColCount, long lDelay = 0)
        {
            int[] nAxisArray = new int[16];
            double[] dPosArray = new double[16];
            double[] dSpeedArray = new double[16];
            double[] dAccArray = new double[16];
            double[] dDecArray = new double[16];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;

            switch (nPosNo)
	        {
                case POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1:
                    int nRow, nCol;
                    int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                    int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

                    nRow = nPickUpTotCount / nTotMapCountX;
                    nCol = nPickerColCount % nTotMapCountX;

                    nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadOffset;
                    nAxisArray[11] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
            
                    // X Axis
                    dPosArray[10] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[10]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                    dSpeedArray[10] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[10]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                    dAccArray[10] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[10]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                    dDecArray[10] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[10]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                    
                    // Y Axis 
                    dPosArray[11] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[11]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                    dSpeedArray[11] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[11]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                    dAccArray[11] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[11]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                    dDecArray[11] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[11]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];


                    // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                    dPosArray[10] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);
                    dPosArray[10] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                    dPosArray[11] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nPickUpTotCount);
                    dPosArray[11] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;

                    break;

                case POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF:
                    nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadOffset;
                    nAxisArray[11] = (int)SVDF.AXES.BALL_VISION_Y;
            
                    break;

                case POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1:
                    nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadOffset;
                    nAxisArray[11] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;
            
                    break;

		        default:
                    break;
	        }
                

            for (int nCnt = 0; nCnt < CFG_DF.MAX_PICKER_PAD_CNT; nCnt++)
            {
                if (nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2)
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
                else
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo] + ConfigMgr.Inst.GetPlaceAngleOffset();
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosChipPkAllZR(int nPosNo, int nHeadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            if(WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[16];
            double[] dPosArray = new double[16];
            double[] dSpeedArray = new double[16];
            double[] dAccArray = new double[16];
            double[] dDecArray = new double[16];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;
            
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;

            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[9] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[11] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[12] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[13] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[14] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[15] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;


            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                // T축
                if (nCnt >= CFG_DF.MAX_PICKER_PAD_CNT)
                {
                    if (nPosNo == POSDF.CHIP_PICKER_READY || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2)
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo];// +ConfigMgr.Inst.GetPlaceAngleOffset(0, nHeadNo, nCnt - CFG_DF.MAX_PICKER_PAD_CNT);
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                    else
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo] + ConfigMgr.Inst.GetPlaceAngleOffset();
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                }
                // Z축
                else
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt].ToString("F3"));
                }
            }

            #region PAD SKIP 시 움직이지 않는다
            for (int nPadCnt = 0; nPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPadCnt++)
            {
                if (GbFunc.IsChipPickerSkip(nHeadNo, nPadCnt))
                {
                    nAxisArray[nPadCnt] = -1;
                    nAxisArray[nPadCnt + CFG_DF.MAX_PICKER_PAD_CNT] = -1;
                }
            }
            #endregion

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0)
                    continue;

                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int IsInPosChipPkR(int nHeadNo)
        {
            int nSafety = 0;
            int[] nAxisArray = new int[8];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;

            for (int i = 0; i < nAxisArray.Length; i++)
            {
                if (GbFunc.IsChipPickerSkip(nHeadNo, i))
                    continue;

                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxisArray[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (GbFunc.IsChipPickerSkip(nHeadNo, nCnt))
                    continue;

                if (MotionMgr.Inst[nAxisArray[nCnt]].IsBusy()) return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }

        protected int IsInPosChipPkR(int nHeadNo, int nPadNo)
        {
            if (GbFunc.IsChipPickerSkip(nHeadNo, nPadNo))
                return FNC.SUCCESS;

            int nSafety = 0;
            int[] nAxisArray = new int[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPadNo);

            for (int i = 0; i < nAxisArray.Length; i++)
            {
                nSafety = SafetyMgr.Inst.GetAxisSafetyMoving(nAxisArray[i]);
                if (FNC.IsErr(nSafety))
                {
                    SetError(nSafety);
                    return nSafety;
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (MotionMgr.Inst[nAxisArray[nCnt]].IsBusy()) return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }

        protected int MovePosChipPkAllZR(int nPosNo, int nHeadNo, bool bSkipMoveT, long lDelay = 0)
        {
#if _NOTEBOOK
            if(WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[16];
            double[] dPosArray = new double[16];
            double[] dSpeedArray = new double[16];
            double[] dAccArray = new double[16];
            double[] dDecArray = new double[16];
            bool[] bSkipMoveArray = new bool[16];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;

            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[9] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[11] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[12] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[13] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[14] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[15] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;


            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                // T축
                if (nCnt >= CFG_DF.MAX_PICKER_PAD_CNT)
                {
                    if (nPosNo == POSDF.CHIP_PICKER_READY || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2)
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo];// +ConfigMgr.Inst.GetPlaceAngleOffset(0, nHeadNo, nCnt - CFG_DF.MAX_PICKER_PAD_CNT);
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                    else
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo] + ConfigMgr.Inst.GetPlaceAngleOffset();
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                }
                // Z축
                else
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            for (int nCnt = 0; nCnt < bSkipMoveArray.Length; nCnt++)
            {
                bSkipMoveArray[nCnt] = false;
            }

            if (bSkipMoveT)
            {
                for (int nCnt = 8; nCnt < bSkipMoveArray.Length; nCnt++)
                {
                    bSkipMoveArray[nCnt] = true;
                }
            }

            #region PAD SKIP 시 움직이지 않는다
            for (int nPadCnt = 0; nPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPadCnt++)
            {
                if (GbFunc.IsChipPickerSkip(nHeadNo, nPadCnt))
                {
                    nAxisArray[nPadCnt] = -1;
                    nAxisArray[nPadCnt + CFG_DF.MAX_PICKER_PAD_CNT] = -1;
                }
            } 
            #endregion

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    if (nAxisArray[nCnt] < 0) continue;

                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt].ToString("F3"));
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (nAxisArray[nCnt] < 0) continue;

                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, bSkipMoveArray, lDelay);
        }

        protected int MovePosChipPkAllZ(int nPosNo, int nHeadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            // 이 때 Z Axis Cal-Offset을 위치에 가감한다.
            int[] nAxisArray = new int[8];
            double[] dPosArray = new double[8];
            double[] dSpeedArray = new double[8];
            double[] dAccArray = new double[8];
            double[] dDecArray = new double[8];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// PickUp하기 전 준비 위치 이동
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPickUpStandbyPosXYZ(int nHeadNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            // X Axis Vision View Pos
            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1 + nTableNo];

            // Vision 위치에서 실제 Pick Up 위치 계산
            dPosArray[0] -= ConfigMgr.Inst.GetCamToPickerOffset(nHeadNo, false).x;

            dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, 0).x;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        /// PickUp하기 전 준비 위치 이동
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPickUpStandbyPosX(int nHeadNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, 0).x;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        /// PickUp하기 전 준비 위치 이동
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPickUpStandbyPosXT(int nHeadNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[9];
            double[] dPosArray = new double[9];
            double[] dSpeedArray = new double[9];
            double[] dAccArray = new double[9];
            double[] dDecArray = new double[9];


            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, 0).x;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        /// 준비 위치 이동 완료 확인
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int IsInPosChipPickUpStandbyPosXYZ(int nHeadNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, 0).x;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            if(IsDelayOver(ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].lMoveTime))
            {
                return SVDF.mInfo[nAxisArray[0]].nMoveErrNo;
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return FNC.BUSY;
                }
            }

            return FNC.SUCCESS;
        }

        protected int MovePosChipGDPlaceStandbyPosXYZ(int nHeadNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            }


            int nRow, nCol;

            nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - 1;


            dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nCol);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, 0).x;
            
            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPrePickUpY(int nHeadNo, int nPickerNo, int nPickerColCount, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            nAxisArray = new int[1];
            dPosArray = new double[1];
            dSpeedArray = new double[1];
            dAccArray = new double[1];
            dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            // Y Axis 
            // [2022.05.03.kmlee] POS NO 변경
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            dPosArray[0] = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1).y;

            if (nTableNo == 1)
            {
                dPosArray[0] = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1).y;
            }

            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];

            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            nRow = nTableCount / nTotMapCountX;
            nCol = nPickerColCount % nTotMapCountX;

            // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
            dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);
            dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
            dPosArray[0] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;

            // 미리 가 있는 이동이기 때문에 보정치는 빼자
            //double dPlaceStartPosX, dPlaceStartPosY;
            //dxy dpMovePos;

            //dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_X + nHeadNo].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            //dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            //dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        /// PickUp 위치 이동
		/// 220617 phj
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="bFDir"></param>
        /// <param name="nPickerNo"></param>
        /// <param name="nPickerColCount"></param>
        /// <param name="dPreZOffset"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPickUpXYZ(int nHeadNo, bool bFDir, int nPickerNo, int nPickerColCount, double dPreZOffset, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            if (GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker.Count <= 1)
            {
                nAxisArray = new int[3];
                dPosArray = new double[3];
                dSpeedArray = new double[3];
                dAccArray = new double[3];
                dDecArray = new double[3];

                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
                nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
            }
            else
            {
                nAxisArray = new int[4];
                dPosArray = new double[4];
                dSpeedArray = new double[4];
                dAccArray = new double[4];
                dDecArray = new double[4];

                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
                nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);

                //nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * (nPickerNo - 1));
                int nPrePickUpPickerNo = GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker[GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker.Count - 2];
                nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPrePickUpPickerNo);
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            // X Axis Vision View Pos
            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_VISION_T1 + nTableNo];

            // Y Axis Vision View Pos 
            dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_VISION_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];

            // Vision 위치에서 실제 Pick Up 위치 계산
            dPosArray[0] -= ConfigMgr.Inst.GetCamToPickerOffset(nHeadNo, false).x;
            dPosArray[1] += ConfigMgr.Inst.GetCamToPickerOffset(nHeadNo, false).y;

            // Z Axis 
            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo] + dPreZOffset;
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            if (dPosArray.Length >= 4)
            {
                dPosArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nPickerColCount % nTotMapCountX;

                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);
                dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);
                dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;
            }
            else
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nTotMapCountX - (nPickerColCount % nTotMapCountX) - 1;

                //nRow = nTableCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                //nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(false, nTableCount);
                dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);
                dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;

                // Inspection 좌표보정을 일단 적용하지 말고 검증이 완료되면 적용할 것... 20211114 bhoh
                // 픽업시 300um 정도 가로방향으로 틀어지는 현상 있음 (manual로 확인했을 때 정밀도 틀어지지 않음)
            }

            //double dPlaceStartPosX, dPlaceStartPosY;
            //dxy dpMovePos;

            //dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            //dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            //dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        /// Motionn  LOG Write
        /// </summary>
        /// <param name="strMsg"></param>
        public void SetStringToTxt(string strMsg)
        {
            try
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_TTLOG_USE].bOptionUse == true)
                {
                    string strPath = $"C:\\MACHINE\\TTLog\\Pick\\{DateTime.Now.ToString("yyMMdd")}_Pick.txt";

                    FileStream objFileStream = new FileStream(strPath, FileMode.Append, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter(objFileStream, Encoding.Default);
                    objStreamWriter.WriteLine($"{DateTime.Now.ToString("HHmmss.fff")}    {strMsg}");
                    objStreamWriter.Close();
                    objFileStream.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public void SetStringPlaceToTxt(string strMsg)
        {
            try
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_TTLOG_USE].bOptionUse == true)
                {
                    string strPath = $"C:\\MACHINE\\TTLog\\Place\\{DateTime.Now.ToString("yyMMdd")}_Place.txt";

                    FileStream objFileStream = new FileStream(strPath, FileMode.Append, FileAccess.Write);
                    StreamWriter objStreamWriter = new StreamWriter(objFileStream, Encoding.Default);
                    objStreamWriter.WriteLine($"{DateTime.Now.ToString("HHmmss.fff")}    {strMsg}");
                    objStreamWriter.Close();
                    objFileStream.Close();
                }
            }
            catch (Exception ex)
            {

            }
        }
        StringBuilder strbMotionLog = new StringBuilder();
        /// <summary>
        /// PickUp 위치 이동
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="bFDir"></param>
        /// <param name="nPickerNo"></param>
        /// <param name="nPickerColCount"></param>
        /// <param name="dPreZOffset"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPickUpXYZT(int nHeadNo, bool bFDir, int nPickerNo, int nPickerColCount, double dPreZOffset, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffsetZ = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;
            int nHeadOffsetT = ((int)SVDF.AXES.CHIP_PK_2_T_1 - (int)SVDF.AXES.CHIP_PK_1_T_1) * nHeadNo;

            strbMotionLog.Clear();
            strbMotionLog.Append("=================== HeadNO : ");
            strbMotionLog.Append(nHeadNo.ToString());
            strbMotionLog.Append(" PickerNo : ");
            strbMotionLog.AppendLine(nPickerNo.ToString());
            if (GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker.Count <= 1)
            {
                nAxisArray = new int[4];
                dPosArray = new double[4];
                dSpeedArray = new double[4];
                dAccArray = new double[4];
                dDecArray = new double[4];

                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
                nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffsetZ + (2 * nPickerNo);
                nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffsetT + (2 * nPickerNo);
            }
            else
            {
                int nPrePickUpPickerNo = GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker[GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPickUpPicker.Count - 2];

                if(GbFunc.IsChipPickerSkip(nHeadNo, nPrePickUpPickerNo))
                {
                    nAxisArray = new int[4];
                    dPosArray = new double[4];
                    dSpeedArray = new double[4];
                    dAccArray = new double[4];
                    dDecArray = new double[4];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffsetZ + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffsetT + (2 * nPickerNo);
                }
                else
                {
                    nAxisArray = new int[5];
                    dPosArray = new double[5];
                    dSpeedArray = new double[5];
                    dAccArray = new double[5];
                    dDecArray = new double[5];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffsetZ + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffsetT + (2 * nPickerNo);

                    //nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * (nPickerNo - 1));

                    nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffsetZ + (2 * nPrePickUpPickerNo);
                }
            }

            // TODO :
            // 여기서 저장된 위치는 Camera로 본 위치이기 때문에 Picker Pad 위치로 변경해주는 함수를 만들어 적용해야 함.
            dxy dpPickerPos;

            if (nTableNo == 0)
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            else
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            //SeqHistory(string.Format("[TEST] GetCamToPickerupPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // X Axis 
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            // Y Axis 
            // [2022.05.03.kmlee] POS NO 변경
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];

            // Z Axis 
            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo] + dPreZOffset;
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            // T
            dPosArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dSpeedArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            if (dPosArray.Length >= 5)
            {
                dPosArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dSpeedArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dAccArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
                dDecArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            }

            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nPickerColCount % nTotMapCountX;

                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);

                double VisionDataX = 0;
                double VisionDataY = 0;
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }

                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }

               dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y; //////2024-02-01 sj.shin 부호 반대로 적용 함.
              //  dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;


                dPosArray[3] -= GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.T;

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}",VisionDataX,VisionDataY));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    -ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));

            }
            else
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nTotMapCountX - (nPickerColCount % nTotMapCountX) - 1;

                //nRow = nTableCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                //nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -

                double VisionDataX = 0;
                double VisionDataY = 0;

                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(false, nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }
                dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;

                dPosArray[3] -= GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.T;

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}",VisionDataX, VisionDataY));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    -ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));
            }

            //SeqHistory(string.Format("[TEST] dPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nTableNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y;
                }


            }
            strbMotionLog.AppendLine(string.Format("[Pick_Offset] VisionOffset SHIFT X : {0} Y : {1}",
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x,
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y));

            strbMotionLog.AppendLine(string.Format("[Pick_Offset] POS X : {0} Y : {1}", dPosArray[0], dPosArray[1]));
            //SeqHistory(string.Format("[TEST] dPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // 맵 테이블은 Angle 보정을 하므로 Calibration 값을 적용할 필요가 없다
            //double dPlaceStartPosX, dPlaceStartPosY;
            //dxy dpMovePos;

            //dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1];
            //dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            //dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }

                SetStringToTxt(strbMotionLog.ToString());
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPickUpXYNEW(int nHeadNo, bool bFDir, int nPickerNo, int nPickerColCount, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            strbMotionLog.Clear();
            strbMotionLog.Append("=================== HeadNO : ");
            strbMotionLog.Append(nHeadNo.ToString());
            strbMotionLog.Append(" PickerNo : ");
            strbMotionLog.AppendLine(nPickerNo.ToString());

            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;

            // TODO :
            // 여기서 저장된 위치는 Camera로 본 위치이기 때문에 Picker Pad 위치로 변경해주는 함수를 만들어 적용해야 함.
            dxy dpPickerPos;

            if (nTableNo == 0)
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            else
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            //SeqHistory(string.Format("[TEST] GetCamToPickerupPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // X Axis 
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            // Y Axis 
            // [2022.05.03.kmlee] POS NO 변경
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];


            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nPickerColCount % nTotMapCountX;

                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);

                double VisionDataX = 0;
                double VisionDataY = 0;
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }

                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }

               //  dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y; //////2024-02-01 sj.shin 부호 반대로 적용 함.
                dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;


                //dPosArray[3] -= GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.T;

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}", VisionDataX, VisionDataY));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));

            }
            else
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nTotMapCountX - (nPickerColCount % nTotMapCountX) - 1;

                //nRow = nTableCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                //nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -

                double VisionDataX = 0;
                double VisionDataY = 0;

                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(false, nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }
                dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;


                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}", VisionDataX, VisionDataY));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    -ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));
            }

            //SeqHistory(string.Format("[TEST] dPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nTableNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y;
                }
            }
            strbMotionLog.AppendLine(string.Format("[Pick_Offset] VisionOffset SHIFT X : {0} Y : {1}",
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x,
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y));

            strbMotionLog.AppendLine(string.Format("[Pick_Offset] POS X : {0} Y : {1}", dPosArray[0], dPosArray[1]));
            //SeqHistory(string.Format("[TEST] dPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));
                        

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }

                SetStringToTxt(strbMotionLog.ToString());
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }
        protected int MovePosChipPickUpXYCam(int nHeadNo, bool bFDir, int nPickerNo, int nPickerColCount, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[]    nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];


            strbMotionLog.Clear();
            strbMotionLog.Append("=================== HeadNO : ");
            strbMotionLog.Append(nHeadNo.ToString());
            strbMotionLog.Append(" PickerNo : ");
            strbMotionLog.AppendLine(nPickerNo.ToString());
            
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;
            // TODO :
            // 여기서 저장된 위치는 Camera로 본 위치이기 때문에 Picker Pad 위치로 변경해주는 함수를 만들어 적용해야 함.
          
            if (nTableNo == 0)
            {
                if (nHeadNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1;
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1;
                }
                else
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2;
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y2;
                }
            }
            else
            {
                if (nHeadNo == 0)
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1;
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1;
                }
                else
                {
                    dPosArray[0] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2;
                    dPosArray[1] = RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y2;
                }
            }
            //SeqHistory(string.Format("[TEST] GetCamToPickerupPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // X Axis 
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            // Y Axis 
            // [2022.05.03.kmlee] POS NO 변경
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];

       
           
            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nPickerColCount % nTotMapCountX;

                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);

                double VisionDataX = 0;
                double VisionDataY = 0;
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }

              //  dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }

                // dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y; //////2024-01-16 sj.shin 칭허 에서 부호 반대로 적용 함.
               // dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;


             

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}", VisionDataX, VisionDataY));

                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    -ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));

            }
            else
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nTotMapCountX - (nPickerColCount % nTotMapCountX) - 1;

                //nRow = nTableCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                //nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -

                double VisionDataX = 0;
                double VisionDataY = 0;

                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(false, nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataX = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                    dPosArray[0] += VisionDataX;
                }
             //   dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAPVISIONDATA_USE].bOptionUse)
                {
                    VisionDataY = GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                    dPosArray[1] += VisionDataY;
                }
         //       dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;

             
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetMapTablePickUpOffsetX SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount),
                    ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount)));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] sMapVisionTable SHIFT X : {0} Y : {1}", VisionDataX, VisionDataY));
                strbMotionLog.AppendLine(string.Format("[Pick_Offset] GetPickerPitchOffset SHIFT X : {0} Y : {1}",
                    ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x,
                    -ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y));
            }

            //SeqHistory(string.Format("[TEST] dPos X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nTableNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y;
                }


            }
            strbMotionLog.AppendLine(string.Format("[Pick_Offset] VisionOffset SHIFT X : {0} Y : {1}",
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x,
                GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y));

            strbMotionLog.AppendLine(string.Format("[Pick_Offset] POS X : {0} Y : {1}", dPosArray[0], dPosArray[1]));

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }

                SetStringToTxt(strbMotionLog.ToString());
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }


        protected int MovePosChipPickUpXY(int nHeadNo, bool bFDir, int nPickerNo, int nPickerColCount, double dPreZOffset, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffsetZ = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;
            int nHeadOffsetT = ((int)SVDF.AXES.CHIP_PK_2_T_1 - (int)SVDF.AXES.CHIP_PK_1_T_1) * nHeadNo;

            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;

            // TODO :
            // 여기서 저장된 위치는 Camera로 본 위치이기 때문에 Picker Pad 위치로 변경해주는 함수를 만들어 적용해야 함.
            dxy dpPickerPos;

            if (nTableNo == 0)
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoL[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            else
            {
                if (nHeadNo == 0)
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X1, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y1);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
                else
                {
                    dpPickerPos = ConfigMgr.Inst.GetCamToPickerupPos(nHeadNo, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_X2, RecipeMgr.Inst.Rcp.listMapGrpInfoR[0].PICK_UP_POS_Y2);
                    dPosArray[0] = dpPickerPos.x;
                    dPosArray[1] = dpPickerPos.y;
                }
            }
            

            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo];

            // Y Axis 
            // [2022.05.03.kmlee] POS NO 변경
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.MAP_STAGE_PICKUP_MOVE];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.MAP_STAGE_PICKUP_MOVE];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.MAP_STAGE_PICKUP_MOVE];

            int nRow, nCol;
            int nTotMapCountX = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
            int nTotMapCountY = RecipeMgr.Inst.Rcp.MapTbInfo.nMapGroupCntY * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY;

            if (bFDir == true)
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nPickerColCount % nTotMapCountX;

                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(true, nPickerColCount);
                dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);
                dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;
            }
            else
            {
                nRow = nTableCount / nTotMapCountX;
                nCol = nTotMapCountX - (nPickerColCount % nTotMapCountX) - 1;

                //nRow = nTableCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX;
                //nCol = RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

                // [2022.05.04.kmlee]MAP_COORDI  좌표 계산을 하기 때문에 X+, Y+가 최종이다
                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 맵 테이블 기준 -
                dPosArray[0] += ConfigMgr.Inst.GetMapTablePickUpOffsetX(false, nTableCount);
                dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.X;
                dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).x;

                dPosArray[1] += ConfigMgr.Inst.GetMapTablePickUpOffsetY(nTableCount);
                dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.UnitArr[nRow][nCol].MAP_COORDI.Y;
                dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, nPickerNo).y;
            }

            // TOP ALIGN 사용 시 Angle 보정 후 재확인된 Offset 값을 적용해야 한다
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse == false)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TOP_ALIGN_USE].bOptionUse &&
                    GbVar.Seq.sMapVisionTable[nTableNo].Info.bTopAlignCorrectResult)
                {
                    dPosArray[0] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.x;
                    dPosArray[1] += GbVar.Seq.sMapVisionTable[nTableNo].Info.xyTopAlignCorrectOffset.y;
                }
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + nTableNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPlaceToGoodXYZT(int nHeadNo, bool bFDir, int nPickerNo, double dPreZOffset, double dChipAngle, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            strbMotionLog.Clear();
            strbMotionLog.Append("===================[Plcae] HeadNO : ");
            strbMotionLog.Append(nHeadNo.ToString());
            strbMotionLog.Append(" PickerNo : ");
            strbMotionLog.AppendLine(nPickerNo.ToString());

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            if (GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker.Count <= 1)
            {
                nAxisArray = new int[4];
                dPosArray = new double[4];
                dSpeedArray = new double[4];
                dAccArray = new double[4];
                dDecArray = new double[4];

                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;
                nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);
            }
            else
            {
                int nPrePickUpPickerNo = GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker[GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker.Count - 2];

                if (GbFunc.IsChipPickerSkip(nHeadNo, nPrePickUpPickerNo))
                {
                    nAxisArray = new int[4];
                    dPosArray = new double[4];
                    dSpeedArray = new double[4];
                    dAccArray = new double[4];
                    dDecArray = new double[4];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);
                }
                else
                {
                    nAxisArray = new int[5];
                    dPosArray = new double[5];
                    dSpeedArray = new double[5];
                    dAccArray = new double[5];
                    dDecArray = new double[5];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);

                    //nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * (nPickerNo - 1));
                    nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPrePickUpPickerNo);
                }
            }

            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            strbMotionLog.AppendLine($"[Place] Cam To Place  Vision Teach X : {TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo]} " +
                $"Teach Y : {TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");
            // X Axis
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];

            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            // Z Axis 
            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo] + dPreZOffset;
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];

            // T Axis 
            dPosArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo] + dChipAngle;
            dSpeedArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dAccArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dDecArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];

            if (dPosArray.Length >= 5)
            {
                dPosArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dSpeedArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dAccArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dDecArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            }

            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }

                strbMotionLog.Append($" Row : {nRow}, Col : {nCol}");
                //dPosArray[0] -= ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX * nPickerNo;
                //원래 소스 
                //// [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
                //// 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                //dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount);
                //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                //dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                ////dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
                //dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);
                //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                //dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;

                //변경 소스
                //문제시  원래소스주석 해제 변경소스 주석
                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);

                strbMotionLog.AppendLine($"[Place] FDir Get Good Tray Table Place Offset X : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount)} Y : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount)}" +
                    $", Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");


            }
            else
            {
                //nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                //nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - ((nPickerColCount + 1) % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);

                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }

                strbMotionLog.Append($" Row : {nRow}, Col : {nCol}");
                //원래 소스 
                //// [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
                ////dPosArray[0] -= ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX * nPickerNo;
                //dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount);
                //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                //dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                ////dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
                //dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);
                //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                //dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;

                //변경 소스 
                //문제시  원래소스주석 해제 변경소스 주석

                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
                //dPosArray[0] -= ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX * nPickerNo;
                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);

                strbMotionLog.AppendLine($"[Place] RDir Get Good Tray Table Place Offset X : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount)} Y : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount)}" +
                   $", Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            }

            //변경 소스 ----------------------------
            //문제시  원래소스주석 해제 변경소스 주석

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            
            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            strbMotionLog.AppendLine($"[Place] Get Tray Table Place Count Angle Offset  X : {dpMovePos.x}, Y : {dpMovePos.y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
           
            strbMotionLog.AppendLine($"[Place] Get Picker Pitch Offset X : {ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x} Y : {-ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            strbMotionLog.AppendLine($"[Place] Recipe Tray Good Offset  X : {RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x}, Y : {RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");


            //---------------------------       -  변경소스 종료 -----------------------------------------

            // 하부 비전에서 검사한 오프셋을 적용
            // X는 -, Y는 +, T는 +
            // [2022.05.26.kmlee] Offset 최종
            //                    X는 Vision에서 -0.9가 들어오면 Jog로 +0.9 가야함
            //                    Y는 Vision에서 -1이 들어오면 Jog로 +1 가야함
            //                    T는 Vision에서 -2가 들어오면 Jog로 -2 가야함
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse &&
                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;
                dPosArray[3] += GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.T;

                strbMotionLog.AppendLine($"[Place] Bottom Vision  X : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X}, Y : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");
                strbMotionLog.AppendLine($"[Place] Bottom Vision  T : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.T}");
            }

            // PLACE ANGLE OFFSET은 무조건
            dPosArray[3] -= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_CAM_ANGLE].dValue;
			//원래 소스 
            //double dPlaceStartPosX, dPlaceStartPosY;
            //dxy dpMovePos;

            //dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            //dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            //dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }

                SetStringPlaceToTxt(strbMotionLog.ToString());
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                //int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo, bSkipIntl);
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt], bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        /// <summary>
        ///  메뉴얼 동작 용
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="bFDir"></param>
        /// <param name="nPickerNo"></param>
        /// <param name="dPreZOffset"></param>
        /// <param name="dChipAngle"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPlaceToGoodXY_NEW(int nHeadNo, bool bFDir, int nPickerNo, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            strbMotionLog.Clear();
            strbMotionLog.Append("===================[Plcae] HeadNO : ");
            strbMotionLog.Append(nHeadNo.ToString());
            strbMotionLog.Append(" PickerNo : ");
            strbMotionLog.AppendLine(nPickerNo.ToString());

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;

        
            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            strbMotionLog.AppendLine($"[Place] Cam To Place  Vision Teach X : {TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo]} " +
                $"Teach Y : {TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            // X Axis
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];

            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];


            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }

                strbMotionLog.Append($" Row : {nRow}, Col : {nCol}");
                
                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);

                strbMotionLog.AppendLine($"[Place] FDir Get Good Tray Table Place Offset X : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount)} Y : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount)}" +
                    $", Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");


            }
            else
            {
     
                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }

                strbMotionLog.Append($" Row : {nRow}, Col : {nCol}");
               
                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);

                strbMotionLog.AppendLine($"[Place] RDir Get Good Tray Table Place Offset X : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount)} Y : {ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount)}" +
                   $", Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            }

            //변경 소스 ----------------------------
            //문제시  원래소스주석 해제 변경소스 주석

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);


            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            strbMotionLog.AppendLine($"[Place] Get Tray Table Place Count Angle Offset  X : {dpMovePos.x}, Y : {dpMovePos.y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;

            strbMotionLog.AppendLine($"[Place] Get Picker Pitch Offset X : {ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x} Y : {-ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");

            // dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;////2024-01-16 sj.shin 칭허 에서 부호 반대로 적용 함.
            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
            strbMotionLog.AppendLine($"[Place] Recipe Tray Good Offset  X : {RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x}, Y : {RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");



            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_USE].bOptionUse &&
                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;

                strbMotionLog.AppendLine($"[Place] Bottom Vision  X : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X}, Y : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y}, Move X : {dPosArray[0]}, Move Y : {dPosArray[1]}");
                strbMotionLog.AppendLine($"[Place] Bottom Vision  T : {GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.T}");
            }

  
            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }

                SetStringPlaceToTxt(strbMotionLog.ToString());
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt], bSkipIntl);
                if (FNC.IsErr(nSafety))
                    return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }


        /// <summary>
        /// X,Y 축 이동 (R축을 제외하고 미리 이동하기 위해)
        /// </summary>
        /// <param name="nHeadNo"></param>
        /// <param name="bFDir"></param>
        /// <param name="nPickerNo"></param>
        /// <param name="nPickerColCount"></param>
        /// <param name="dPreZOffset"></param>
        /// <param name="dChipAngle"></param>
        /// <param name="nTableNo"></param>
        /// <param name="nTableCount"></param>
        /// <param name="lDelay"></param>
        /// <param name="bSkipIntl"></param>
        /// <returns></returns>
        protected int MovePosChipPlaceToGoodXY(int nHeadNo, bool bFDir, int nPickerNo, double dPreZOffset, double dChipAngle, int nTableNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;
 
            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;

            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            // X Axis
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            int nRow, nCol;

            if (bFDir == true)
            {
                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }

                //dPosArray[0] -= ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX * nPickerNo;

                //원래소스 
                //// [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
                //// 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                //dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount);
                //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                //dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                ////dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
                //dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);
                //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                //dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;

                //변경 소스 
                //문제시  원래소스주석 해제 변경소스 주석

                // [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
                // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(true, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);


            }
            else
            {
                nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
                nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MULTI_PICKER_CHIP_PLACE_TOP_TRAY].bOptionUse)
                {
                    nRow = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY - (nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;
                }
                //원래소스 
                //dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount);
                //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
                //dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

                //dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);
                //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
                //dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;

                //변경 소스 
                //문제시  원래소스주석 해제 변경소스 주석

                dPosArray[0] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetX(false, nTableNo, nTableCount);
                dPosArray[1] += ConfigMgr.Inst.GetGoodTrayTablePlaceOffsetY(nTableNo, nTableCount);

            }

            //변경 소스 
            //문제시  원래소스주석 해제 변경소스 주석

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].x;

            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dGdTrayOffset[nHeadNo][nTableNo].y;
			//변경 소스 종료  ----------------------------------------------------------------
            // 하부 비전에서 검사한 오프셋을 적용
            // X는 -, Y는 +, T는 +
            // [2022.05.26.kmlee] Offset 최종
            //                    X는 Vision에서 -0.9가 들어오면 Jog로 +0.9 가야함
            //                    Y는 Vision에서 -1이 들어오면 Jog로 +1 가야함
            //                    T는 Vision에서 -2가 들어오면 Jog로 -2 가야함
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;
            }

			//원래소스 
            //double dPlaceStartPosX, dPlaceStartPosY;
            //dxy dpMovePos;

            //dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            //dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            //dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            //dPosArray[0] = dpMovePos.x;
            //dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                //int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo, bSkipIntl);
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt], bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPlaceToReworkXYZT(int nHeadNo, int nPickerNo, double dPreZOffset, double dChipAngle, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            // RW은 정,역 방향 안함...
            if (GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker.Count <= 1)
            {
                nAxisArray = new int[4];
                dPosArray = new double[4];
                dSpeedArray = new double[4];
                dAccArray = new double[4];
                dDecArray = new double[4];

                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;
                nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);
            }
            else
            {
                int nPrePickUpPickerNo = GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker[GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].lstPlacePicker.Count - 2];

                if (GbFunc.IsChipPickerSkip(nHeadNo, nPrePickUpPickerNo))
                {
                    nAxisArray = new int[4];
                    dPosArray = new double[4];
                    dSpeedArray = new double[4];
                    dAccArray = new double[4];
                    dDecArray = new double[4];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);
                }
                else
                {
                    nAxisArray = new int[5];
                    dPosArray = new double[5];
                    dSpeedArray = new double[5];
                    dAccArray = new double[5];
                    dDecArray = new double[5];

                    nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
                    nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;
                    nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPickerNo);
                    nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset + (2 * nPickerNo);

                    //nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * (nPickerNo - 1));
                    nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (2 * nPrePickUpPickerNo);
                }
            }

            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            // X Axis 
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            // Z Axis 
            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] + dPreZOffset;
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            // T Axis 
            dPosArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF] + dChipAngle;
            dSpeedArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dAccArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dDecArray[3] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_T_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            if (dPosArray.Length >= 5)
            {
                dPosArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
                dSpeedArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
                dAccArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
                dDecArray[4] = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CHIP_PK_1_Z_1].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            }

            int nRow, nCol;

            nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            //nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            //nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

            // [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
            // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
            dPosArray[0] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetX(nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].x;

            //dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
            dPosArray[1] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetY(nTableCount);
            //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;  //2024-01-16 sj.shin 칭허 에서 부호 반대로 적용 함.
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].y;

            // 하부 비전에서 검사한 오프셋을 적용
            // X는 -, Y는 +, T는 +
            // [2022.05.26.kmlee] Offset 최종
            //                    X는 Vision에서 -0.9가 들어오면 Jog로 +0.9 가야함
            //                    Y는 Vision에서 -1이 들어오면 Jog로 +1 가야함
            //                    T는 Vision에서 -2가 들어오면 Jog로 -2 가야함
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;
                dPosArray[3] += GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.T;
            }

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(2, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPlaceToReworkXY(int nHeadNo, int nPickerNo, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;


            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            // X Axis 
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];



            int nRow, nCol;

            nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            //nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            //nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

            // [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
            // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
            dPosArray[0] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetX(nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].x;

            //dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
            dPosArray[1] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetY(nTableCount);
            //dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;  //2024-01-16 sj.shin 칭허 에서 부호 반대로 적용 함.
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].y;

            // 하부 비전에서 검사한 오프셋을 적용
            // X는 -, Y는 +, T는 +
            // [2022.05.26.kmlee] Offset 최종
            //                    X는 Vision에서 -0.9가 들어오면 Jog로 +0.9 가야함
            //                    Y는 Vision에서 -1이 들어오면 Jog로 +1 가야함
            //                    T는 Vision에서 -2가 들어오면 Jog로 -2 가야함
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;
            }

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(2, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPlaceToReworkXY(int nHeadNo, int nPickerNo, double dPreZOffset, double dChipAngle, int nTableCount, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray = new int[2];
            dPosArray = new double[2];
            dSpeedArray = new double[2];
            dAccArray = new double[2];
            dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            dxy dpPickerPos;
            dpPickerPos = ConfigMgr.Inst.GetCamToPlacePos(nHeadNo,
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF],
                                                          TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo]);
            dPosArray[0] = dpPickerPos.x;
            dPosArray[1] = dpPickerPos.y;

            //X Axis
            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];

            // Y Axis 
            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            int nRow, nCol;

            nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            nCol = RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX - (nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX) - 1;

            //nRow = nTableCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;
            //nCol = nTableCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX;

            // [2022.06.13.kmlee] GetPickerPitchOffset Y는 트레이 스테이지 기준 -
            // 정방향일 경우 무조건 0부터 시작을 해야 하기 때문에...nPickerColCount
            dPosArray[0] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetX(nTableCount);
            dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).x;
            dPosArray[0] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].x;

            //dPosArray[1] += ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchY;
            dPosArray[1] += ConfigMgr.Inst.GetReworkTrayTablePlaceOffsetY(nTableCount);
            dPosArray[1] -= ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nPickerNo).y;
            dPosArray[1] += RecipeMgr.Inst.Rcp.TrayInfo.dRwTrayOffset[nHeadNo].y;

            // 하부 비전에서 검사한 오프셋을 적용
            // X는 -, Y는 +, T는 +
            // [2022.05.26.kmlee] Offset 최종
            //                    X는 Vision에서 -0.9가 들어오면 Jog로 +0.9 가야함
            //                    Y는 Vision에서 -1이 들어오면 Jog로 +1 가야함
            //                    T는 Vision에서 -2가 들어오면 Jog로 -2 가야함
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.BOTTOM_VISION_OFFSET_USE].bOptionUse)
            {
                dPosArray[0] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.X;
                dPosArray[1] -= GbVar.Seq.sPkgPickNPlace.pInfo[nHeadNo].unitPickUp[nPickerNo].BALL_COORDI.Y;
            }

            double dPlaceStartPosX, dPlaceStartPosY;
            dxy dpMovePos;

            dPlaceStartPosX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_REWORK_REF];
            dPlaceStartPosY = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(2, nHeadNo, dPlaceStartPosX, dPlaceStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }

        protected int MovePosChipPlaceToBinBoxXY(int nHeadNo, int nBinBoxNo, long lDelay = 0, bool bSkipIntl = false)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray;
            double[] dPosArray;
            double[] dSpeedArray;
            double[] dAccArray;
            double[] dDecArray;

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray = new int[1];
            dPosArray = new double[1];
            dSpeedArray = new double[1];
            dAccArray = new double[1];
            dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN];
            }

            
            //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(false, nHeadNo, 0).x;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_UNLOADING_BIN, bSkipIntl);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, !bSkipIntl);
        }
        

        /// <summary>
        /// Inspection XY next inspection pos 이동
        /// </summary>
        /// <param name="nPosIdx"></param>
        /// <param name="strMotPos"></param>
        /// <param name="lDelay"></param>
        /// <param name="bCheckInterlock"></param>
        /// <returns></returns>
        public int MovePosBallInspXYNext(int nInspectionCount, int nHeadNo, long lDelay = 0, bool bCheckInterlock = true)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_X - (int)SVDF.AXES.CHIP_PK_1_X) * nHeadNo;
            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Y;

            /*
                        for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                        {
                            //dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                            dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nInspectionCount].x;
                            dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                            dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                            dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

                            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nInspectionCount].x;
                            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nInspectionCount].x;
                            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nInspectionCount].x;
                            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nInspectionCount].x;
                            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nInspectionCount].x;
                            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[nCnt] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nInspectionCount].x;
                        }

                        //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                        dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][nInspectionCount].y;
                        dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                        dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                        dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];

                        if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][nInspectionCount].y;
                        else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][nInspectionCount].y;
                        else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][nInspectionCount].y;
                        else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][nInspectionCount].y;
                        else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][nInspectionCount].y;
                        else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][nInspectionCount].y;
                        //double dStepOffsetX;
                        //dStepOffsetX = nInspectionCount * ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX;
            */
            // [2022.06.13.kmlee] 1번 패드 위치 기준으로 Offset 적용 (방향 확인 겸)

            //dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            //dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].x;
            dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][0].x;
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

            //if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].x;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].x;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].x;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].x;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].x;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].x;
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][0].x;

            //dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
            //dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P90][0].y;
            dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][0].y;
            dSpeedArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dVel[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
            dAccArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dAcc[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
            dDecArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dDec[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];

            //if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P180][0].y;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_P270][0].y;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_0][0].y;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N90][0].y;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N180][0].y;
            //else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchAbsRef[CFG_DF.DEGREE_N270][0].y;
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[1] = ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][0].y;


            //double dStepOffsetX;
            //dStepOffsetX = nInspectionCount * ConfigMgr.Inst.Cfg.OffsetPnP[nHeadNo].dDefPickerPitchX;

            // [2022.06.13.kmlee] GetPickerPitchOffset Y는 볼 비전 기준 +
            //dPosArray[0] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nInspectionCount).x;
            //dPosArray[1] += ConfigMgr.Inst.GetPickerPitchOffset(true, nHeadNo, nInspectionCount).y;
            dPosArray[0] += ConfigMgr.Inst.GetPickerInspPitchOffset(true, nHeadNo, nInspectionCount).x;
            dPosArray[1] += ConfigMgr.Inst.GetPickerInspPitchOffset(true, nHeadNo, nInspectionCount).y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);

        }

        protected int MovePosChipPkInspectionReady(int nHeadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            //int[] nAxisArray = new int[13];
            //double[] dPosArray = new double[13];
            //double[] dSpeedArray = new double[13];
            //double[] dAccArray = new double[13];
            //double[] dDecArray = new double[13];
            int[] nAxisArray = new int[11];
            double[] dPosArray = new double[11];
            double[] dSpeedArray = new double[11];
            double[] dAccArray = new double[11];
            double[] dDecArray = new double[11];


            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;


            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            nAxisArray[9] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[10] = (int)SVDF.AXES.BALL_VISION_Z;

            //nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_X + nHeadNo;
            //nAxisArray[11] = (int)SVDF.AXES.BALL_VISION_Y;
            //nAxisArray[12] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length - 2; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

                if (nCnt < 9)
                {
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
                }
            }

            for (int nCnt = 9; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo];
            }
            
            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length - 2; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF),
                                                dPosArray[nCnt]);
                }

                for (int nCnt = 8; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length - 2; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            for (int nCnt = 7; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nHeadNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public int MovePosTrayInspYNext(int nInspectionCount, int nStageNo, long lDelay = 0, bool bCheckInterlock = true)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nStageNo;

            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];

            double dStepOffsetY;
            dStepOffsetY = nInspectionCount * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            dPosArray[0] -= dStepOffsetY;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[0]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.TRAY_STAGE_TRAY_INSPECTION_START),
                        dPosArray[0]);
            }

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[0], POSDF.TRAY_STAGE_TRAY_INSPECTION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public int MovePosTrayInspYOffset(int nInspectionCount, int nStageNo, long lDelay = 0, bool bCheckInterlock = true)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nStageNo;

            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.TRAY_STAGE_TRAY_INSPECTION_START];

            double dStepOffsetY;
            dStepOffsetY = nInspectionCount * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;
            dPosArray[0] -= dStepOffsetY;
            dPosArray[0] -= RecipeMgr.Inst.Rcp.TrayInfo.dInspChipOffsetY;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                        SVDF.GetAxisName(nAxisArray[0]),
                        POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[0]), POSDF.TRAY_STAGE_TRAY_INSPECTION_START),
                        dPosArray[0]);
            }

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[0], POSDF.TRAY_STAGE_TRAY_INSPECTION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsPickerPadSkip(bool bFDir, int nHeadNo, int nPadNo)
        {
            bool[] bSkip = new bool[8];

            if (nHeadNo == CFG_DF.HEAD_1)
            {
                bSkip = new bool[8] { ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_1_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_2_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_3_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_4_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_5_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_6_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_7_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_1_PAD_8_USE].bOptionUse};
            }
            else
            {
                bSkip = new bool[8] { ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_1_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_2_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_3_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_4_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_5_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_6_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_7_USE].bOptionUse,
                                      ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKER_2_PAD_8_USE].bOptionUse};
            }

            if (bFDir == true)
            {
                return !bSkip[nPadNo];
            }
            else
            {
                int nBwdPickerNo = CFG_DF.MAX_PICKER_PAD_CNT - nPadNo - 1;
                return !bSkip[nBwdPickerNo];
            }
        }
        #endregion

        #region BALL VISION
        protected int MovePosBallVisionY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosBallVisionZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosBallVisionXYZ(int nPosNo, int nPickerHeadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Z;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_X + nPickerHeadNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// 220608
        /// Ball Vision준비 위치 이동 시 T축 Move추가
        /// </summary>
        /// <param name="nPosNo"></param>
        /// <param name="nPickerHeadNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        protected int MovePosBallVisionXYZT(int nPosNo, int nPickerHeadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            if (WaitDelay(1000))
            {
                return FNC.SUCCESS;
            }
            else
            {
                return FNC.BUSY;
            }
#endif
            //int[] nAxisArray = new int[3];
            //double[] dPosArray = new double[3];
            //double[] dSpeedArray = new double[3];
            //double[] dAccArray = new double[3];
            //double[] dDecArray = new double[3];

            //nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;
            //nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Z;
            //nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_X + nPickerHeadNo;

            int[] nAxisArray = new int[11];
            double[] dPosArray = new double[11];
            double[] dSpeedArray = new double[11];
            double[] dAccArray = new double[11];
            double[] dDecArray = new double[11];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nPickerHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Z;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_X + nPickerHeadNo;

            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[9] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;


            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                // T축
                if (nCnt > 2)
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[3]].dPos[nPosNo] + ConfigMgr.Inst.GetPlaceAngleOffset();
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[3]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[3]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[3]].dDec[nPosNo];
                }
                else
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
                }
            }

            //2023-01-10 칭허 소스 로 변경 sj.shin
           // dPosArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
           // dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
           // dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
           // dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

            dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][0].x;
            dSpeedArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dVel[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dAccArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dAcc[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dDecArray[2] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[2]].dDec[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];

            //============ 추가
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][0].x;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[2] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][0].x;


            dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P90][0].y;
            dSpeedArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nPickerHeadNo];
            dAccArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nPickerHeadNo];
            dDecArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[POSDF.BALL_VISION_CHIP_BGA_INSPECTION_HEAD_1 + nPickerHeadNo];

            
            if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P180][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_P270][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == 0) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_0][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -90) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N90][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -180) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N180][0].y;
            else if (RecipeMgr.Inst.Rcp.TrayInfo.nPlaceAngle == -270) dPosArray[0] = ConfigMgr.Inst.Cfg.OffsetPnP[nPickerHeadNo].dpPickerPitchInspAbsRef[CFG_DF.DEGREE_N270][0].y;

            //============ 추가

            dPosArray[2] += ConfigMgr.Inst.GetPickerInspPitchOffset(true, nPickerHeadNo, CFG_DF.MAX_PICKER_PAD_CNT - 1).x;
            dPosArray[0] += ConfigMgr.Inst.GetPickerInspPitchOffset(true, nPickerHeadNo, CFG_DF.MAX_PICKER_PAD_CNT - 1).y;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosBallVisionY(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosBallVisionZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosBallVisionYZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region TRAY PICKER
        protected int MovePosTrayPkX(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkZ(int nPosNo, double dOffsetZ = 0.0, bool bIsSlow = false, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo] + dOffsetZ, nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            //220516
            //트레이 뒤집어서 사용할 시 Z축 높이 옵션
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_FLIP_LD_N_ULD_USE].bOptionUse && nPosNo != POSDF.UNIT_PICKER_READY)
            {
                dPosArray[0] += ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_FLIP_LD_N_ULD_PITCH].dValue;
            }

            if (bIsSlow == true)
			{
            	dSpeedArray[0] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.TRAY_PICKER_Z_SLOW_DOWN_VEL].dValue;
            	dAccArray[0] = dSpeedArray[0] * 10;
            	dDecArray[0] = dSpeedArray[0] * 10;
			}

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkXY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkXZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkYZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Y;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosTrayPkXYZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Y;
            nAxisArray[2] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = ZAxisPos(TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo], nAxisArray[nCnt]);
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosTrayPkX(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkY(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkXY(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkXZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkYZ(int nPosNo)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dCurrentPosArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_Y;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsInPosTrayPkXYZ(int nPosNo)
        {
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dCurrentPosArray = new double[3];

            nAxisArray[0] = (int)SVDF.AXES.TRAY_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.TRAY_PK_Y;
            nAxisArray[2] = (int)SVDF.AXES.TRAY_PK_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region TRAY ELEVATOR
        protected int MovePosGdTrayElvZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosGdTrayElvZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        protected int MovePosRwTrayElvZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.RW_TRAY_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosRwTrayElvZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.RW_TRAY_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        protected int MovePosEmtyTray1ElvZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosEmtyTray1ElvZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        protected int MovePosEmtyTray2ElvZ(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosEmtyTray2ElvZ(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        
        public bool IsInPosTrayElvZ(int nElvIdx, int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nElvIdx;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region TRAY STAGE
        protected int MovePosGdTrayStgY(int nTableNo, int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosGdTrayStgY(int nTableNo, int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + nTableNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > 0.1f/*ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand*/)
                {
                    return false;
                }
            }

            return true;
        }

        //protected int MovePosGdTrayStg2Y(int nPosNo, long lDelay = 0)
        //{
        //    int[] nAxisArray = new int[1];
        //    double[] dPosArray = new double[1];
        //    double[] dSpeedArray = new double[1];
        //    double[] dAccArray = new double[1];
        //    double[] dDecArray = new double[1];

        //    nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_2_Y;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        dPosArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        dSpeedArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
        //        dAccArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
        //        dDecArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
        //    }

        //    if (RunLib.IsCurRunCmdStart())
        //    {
        //        m_strMotPos = "MOVE POSITION";
        //        for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //        {
        //            m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
        //                                        SVDF.GetAxisName(nAxisArray[nCnt]),
        //                                        POSDF.GetPosName(POSDF.GetRcpPosModeAxis(nAxisArray[nCnt]), nPosNo),
        //                                        dPosArray[nCnt]);
        //        }
        //    }

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
        //        if (FNC.IsErr(nSafety)) return nSafety;
        //    }

        //    return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        //}

        //public bool IsInPosGdTrayStg2Y(int nPosNo)
        //{
        //    int[] nAxisArray = new int[1];
        //    double[] dPosArray = new double[1];
        //    double[] dCurrentPosArray = new double[1];

        //    nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_STG_2_Y;

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        dPosArray[nCnt] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
        //        dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
        //    }

        //    for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
        //    {
        //        if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) > 0.1f/*ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand*/)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        public bool IsGDTrayTableCylinderSafety()
        {
            double dGdTrayTableCurrPos1 = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();
            double dGdTrayTableCurrPos2 = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

            double dGdTrayTableDist = Math.Abs(dGdTrayTableCurrPos1 - dGdTrayTableCurrPos2);

            if (dGdTrayTableDist >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
            {
                return true;
            }

            return false;
        }

        protected int MovePosRwTrayStgY(int nPosNo, long lDelay = 0)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        public bool IsInPosRwTrayStgY(int nPosNo)
        {
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dCurrentPosArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.RW_TRAY_STG_Y;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dCurrentPosArray[nCnt] = MotionMgr.Inst[nAxisArray[nCnt]].GetRealPos();
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                if (Math.Abs(dCurrentPosArray[nCnt] - dPosArray[nCnt]) >= 0.1f/*ConfigMgr.Inst.Cfg.MotData[nAxisArray[nCnt]].dInpositionBand*/)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        
        #region Good Elv
        protected int MovePosElvZAxis(int nElvPortNo, int nPos, double dOffset, long lDelay = 0)
        {

#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif

            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nElvPortNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPos] + dOffset;
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPos];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPos];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPos];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPos),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPos);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosElvZAxis(int nElvPortNo, int nPos, double dVel, double dOffset, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nElvPortNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPos] + dOffset;
                dSpeedArray[nCnt] = dVel;
                dAccArray[nCnt] = dVel * 5.0f;
                dDecArray[nCnt] = dVel * 5.0f;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPos),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPos);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MoveElvOffsetPos(int nElvPortNo, double dCurrPos, double dOffsetZ, long lDelay = 0)
        {
#if _NOTEBOOK
            return RunLib.msecDelay(300);
#endif
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nAxisArray[0] = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nElvPortNo;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = dCurrPos + dOffsetZ;
                //예전 소스
                //dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[0];
                //dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[0];
                //dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[0];
                //변경 소스
                dSpeedArray[nCnt] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL + nElvPortNo].dValue;
                dAccArray[nCnt] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL + nElvPortNo].dValue * 10;
                dDecArray[nCnt] = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_GD_TRAY_ELV_1_OFFSET_MOVE_VEL + nElvPortNo].dValue * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), 0),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], 0);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected int MovePosEmptyElvZAxis(int nPos, long lDelay = 0)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nAxisArray[0] = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
            nAxisArray[1] = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPos];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPos];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPos];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPos];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPos),
                                                dPosArray[nCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPos);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        protected bool IsInPosNo(SVDF.AXES nAxis, int PosNo, long lDelay = 0)
        {
            return SafetyMgr.Inst.IsInPosNo(nAxis, PosNo);
        }

        /// <summary>
        ///  Z축 구동 시 드라이런 모드 일 위치경우 값의 10%만 down합니다.
        /// </summary>
        /// <param name="nOrgPos"></param>
        /// <returns>nOrgPos * 0.1</returns>

        #endregion

        protected double ZAxisPos(double nOrgPos, int nAxis)
        {
            double dPos = nOrgPos;

            if (nAxis == (int)SVDF.AXES.STRIP_PK_Z ||
                nAxis == (int)SVDF.AXES.UNIT_PK_Z ||
                nAxis == (int)SVDF.AXES.MAP_PK_Z ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_1 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_2 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_3 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_4 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_5 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_6 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_7 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_1_Z_8 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_1 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_2 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_3 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_4 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_5 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_6 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_7 ||
                nAxis == (int)SVDF.AXES.CHIP_PK_2_Z_8)
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                {
                    if (dPos > 0)
                    {
                        dPos = dPos * 0.1;
                    }
                }
            }

            return dPos;
        }
        #endregion

        #region Manual Move InterLock
        protected int ManualInterLockCheck(int[] nAxi)
        {
            for (int i = 0; i < nAxi.Length; i++)
            {
                switch ((SVDF.AXES)nAxi[i])
                {
                    case SVDF.AXES.STRIP_PK_X:
                        if (IsInPosStripPkZ(POSDF.STRIP_PICKER_READY) == false)
                        {
                            // 다운 상태
                            return (int)ERDF.E_INTL_STRIP_PK_X_CAN_NOT_MOVE_STRIP_PK_Z_NOT_READY_POS;
                        }
                        break;
                    case SVDF.AXES.UNIT_PK_X:
                        if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY) == false)
                        {
                            // 다운 상태
                            return (int)ERDF.E_INTL_UNIT_PK_X_CAN_NOT_MOVE_UNIT_PK_Z_NOT_READY_POS;
                        }
                        break;
                    case SVDF.AXES.CHIP_PK_1_X:
                        if (IsReadyPosChipPkZAxis(0) == false)
                        {
                            // Chip Picker 다운 상태
                            return (int)ERDF.E_INTL_MULTI_PK_1_Z_Down;
                        }
                        break;
                    case SVDF.AXES.CHIP_PK_2_X:
                        if (IsReadyPosChipPkZAxis(1) == false)
                        {
                            //Chip Picker 다운 상태
                            return (int)ERDF.E_INTL_MULTI_PK_2_Z_Down;
                        }
                        break;
                    case SVDF.AXES.TRAY_PK_X:
                        if (IsInPosTrayPkZ(POSDF.TRAY_PICKER_READY) == false)
                        {
                            // 다운 상태
                            return (int)ERDF.E_INTL_TRAY_PK_X_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                        }
                        break;
                    case SVDF.AXES.TRAY_PK_Y:
                        if (IsInPosTrayPkZ(POSDF.TRAY_PICKER_READY) == false)
                        {
                            // 다운 상태
                            return (int)ERDF.E_INTL_TRAY_PK_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                        }
                        break;

                    case SVDF.AXES.GD_TRAY_STG_1_Y:
                        bool bGood1InUp = MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_STG_UP);
                        bool bGood1InDn = MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_STG_DOWN);

                        if (bGood1InUp &&
                            IsReadyPosChipPkZAxis(0) == false &&
                             IsReadyPosChipPkZAxis(1) == false)
                        {
                            //스테이지 업상태이고,  Chip Picker 다운 상태
                            return (int)ERDF.E_INTL_MULTI_PK_1_Z_Down;
                        }
                        break;
                    case SVDF.AXES.GD_TRAY_STG_2_Y:
                        bool bGood2InUp = MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_STG_UP);
                        bool bGood2InDn = MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_STG_DOWN);

                        if (bGood2InUp &&
                              IsReadyPosChipPkZAxis(0) == false &&
                             IsReadyPosChipPkZAxis(1) == false)
                        {
                            //스테이지 업상태이고,  Chip Picker 다운 상태
                            return (int)ERDF.E_INTL_MULTI_PK_2_Z_Down;
                        }
                        break;
        

                }

            }

            return FNC.SUCCESS;
        }
        #endregion Manual Move InterLock
    }
}