using DionesTool.Motion;
using DionesTool.Motion.AJIN;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.MOTION
{
    public class MotionMgr : IDisposable
    {
        enum eSDO_OBJ_IDX
        {
            SMART_IO = 0x3001,
            ANALOG_1 = 0x2310,
            ANALOG_2 = 0x2B10,
            DIGITAL_OUTPUT_1 = 0x60FE,
            DIGITAL_OUTPUT_2 = 0x68FE,
        }

        private const int nAnalog_Bitlen = 16;
        private uint[,] unAnalogPioAddr = new uint[2, 8] {
        {32,400,768,1136,1504,1872,2240,2608},
        {32,400,768,1136,1504,1872,2240,2608}};

        //{1520,1216,912,608,304,0}};

        private static volatile MotionMgr instance;
        private static object syncRoot = new Object();

        MotionBase[] motion = null;
        MotionAjinMgr AjmMgr = new MotionAjinMgr();

        Thread[] threadIO = new Thread[2];
        bool[] flagAliveThread = new bool[2];

        MotionDataMgr dataMgr;
        public delegate void monitorOutputState(int nOutputNo, bool bState);
        public event monitorOutputState OnMonitorOutputState;

        #region Property & Singleton
        public static MotionMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MotionMgr();
                    }
                }

                return instance;
            }
        }

        /// <summary>
        /// 각 축에 대한 배열 접근
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public MotionBase this[int index]
        {
            get
            {
                if (instance.motion == null) return null;
                if (index == -1) return null;
                
                return instance.motion[index];
            }
        }

        public MotionBase this[SVDF.AXES index]
        {
            get
            {
                if (instance.motion == null) return null;

                return instance.motion[(int)index];
            }
        }

        public MotionBase[] MOTION
        {
            get { return instance.motion; }
        }


        #endregion

        public MotionMgr()
        {
            //
        }

        ~MotionMgr()
        {
        }

        public void Init()
        {
            // 모션 데이터 불러오기
            // 여기서는 Config 값에 설정 값이 부분적으로 있음
            // 아래에서 따로 지정하여 줌 (Servo 반대나 Real Pos Mode만 사용)
            dataMgr = MotionDataMgr.Deserialize(PathMgr.Inst.PATH_SETTING_MOTION_DATA);
            if (dataMgr == null)
                dataMgr = new MotionDataMgr();
            dataMgr.Create((int)SVDF.AXES.MAX);
            dataMgr.Serialize(PathMgr.Inst.PATH_SETTING_MOTION_DATA);

            // 모션 추가
            motion = new MotionBase[(int)SVDF.AXES.MAX];

            // 디바이스 로드
            bool bInt = AjmMgr.InitMotionIo(PathMgr.Inst.FILE_PATH_MOTOR_FILE);

            for (int indexMotion = 0; indexMotion < (int)SVDF.AXES.MAX; indexMotion++)
            {
                bool simul = false;
#if _NOTEBOOK
                simul = true;
                motion[indexMotion] = new MotionAjin(indexMotion, simul);

#else
              motion[indexMotion] = new MotionAjin(indexMotion, simul);
#endif

            }

            for (int indexMotion = (int)SVDF.AXES.CHIP_PK_1_Z_1; indexMotion <= (int)SVDF.AXES.CHIP_PK_2_Z_8; indexMotion = indexMotion + 2)
            {
                SetSigOutputMask(indexMotion, 0xFF);
            }

            #region Smart Io Setting
            //220510
            SetSigOutputSmartIOMode(6, 0x200);
            SetSigOutputSmartIOMode(7, 0x200);
            SetSigOutputSmartIOMode(8, 0x200);
            SetSigOutputSmartIOMode(9, 0x200);
            SetSigOutputSmartIOMode(10, 0x200);
            SetSigOutputSmartIOMode(11, 0x200);
            SetSigOutputSmartIOMode(12, 0x200);
            SetSigOutputSmartIOMode(13, 0x200);
            SetSigOutputSmartIOMode(14, 0x200);

            SetSigOutputSmartIOMode(18, 0x200);
            SetSigOutputSmartIOMode(19, 0x200);

            SetSigOutputSmartIOMode(23, 0x200);
            SetSigOutputSmartIOMode(24, 0x200);
            SetSigOutputSmartIOMode(25, 0x200);
            #endregion
            // SVDF.Init() 에서 초기화 함.
            // SetMotionData();

            if (!bInt)
            {
                System.Windows.Forms.MessageBox.Show("AJIN Lib load fail..");
            }

            StartStopThreadReadDIO(true);
            StartStopThreadReadWriteAIO(true);
        }

        public void Dispose()
        {
            StartStopThreadReadDIO(false);
            StartStopThreadReadWriteAIO(false);
        }

        void StartStopThreadReadDIO(bool bStart)
        {
            if (threadIO[0] != null)
            {
                flagAliveThread[0] = false;
                threadIO[0].Join(500);
                threadIO[0].Abort();
                threadIO[0] = null;
            }

            if (bStart)
            {
                flagAliveThread[0] = true;
                threadIO[0] = new Thread(new ParameterizedThreadStart(ThreadMonitorDIO));
                threadIO[0].Name = "READ DIO THREAD";
                //threadReadIO.IsBackground = true;
                if (threadIO[0].IsAlive == false)
                    threadIO[0].Start(this);
            }
        }

        void StartStopThreadReadWriteAIO(bool bStart)
        {
            if (threadIO[1] != null)
            {
                flagAliveThread[1] = false;
                threadIO[1].Join(500);
                threadIO[1].Abort();
                threadIO[1] = null;
            }

            if (bStart)
            {
                flagAliveThread[1] = true;
                threadIO[1] = new Thread(new ParameterizedThreadStart(ThreadMonitorAIO));
                threadIO[1].Name = "READ AIO THREAD";
                //threadReadIO.IsBackground = true;
                if (threadIO[1].IsAlive == false)
                    threadIO[1].Start(this);
            }
        }

        public void ThreadMonitorDIO(object obj)
        {
            MotionMgr mgr = obj as MotionMgr;

            while (mgr.flagAliveThread[0])
            {
                Thread.Sleep(10);

#if _AJIN
                try
                {
                    AjmMgr.GetDIn(ref GbVar.GB_INPUT);
                    AjmMgr.GetDOut(ref GbVar.GB_OUTPUT);
                }
                catch (Exception)
                {
                }
#endif
            }
        }

        public void ThreadMonitorAIO(object obj)
        {
            MotionMgr mgr = obj as MotionMgr;
            int nAxisOffset = 0;
            while (mgr.flagAliveThread[1])
            {
                Thread.Sleep(10);

#if _AJIN
                for (int nIdx = 0; nIdx < (int)IODF.A_INPUT.MAX; nIdx++)
                {
                    string strEName = Enum.GetName(typeof(IODF.A_INPUT), (IODF.A_INPUT)nIdx);

                    if (strEName == null) continue;

                    if (strEName.IndexOf("pare") > 0 || strEName.IndexOf("PARE") > 0)
                    {
                        continue;
                    }

                    double dVac = 0.0;
                    //Cal 부분 추가
                    if (nIdx < (int)IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM)
                    {
                        dVac = AjmMgr.GetAIn(nIdx);
                    }
                    else
                    {
                        int usData = 0;
                        int nAxis = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nAxisOffset;
                        GetAnalogValue(nAxis, ref usData);
                        dVac = usData;
                        nAxisOffset += 2;
                    }

                    if (double.IsNaN(dVac) || double.IsInfinity(dVac))
                        dVac = 0;

                    GbVar.GB_AINPUT[nIdx] = dVac;
                }

                uint uOutputOn = 0;
                for (int i = 0; i < 16; i++)
                {
                    AjmMgr.GetSignalOutputBit((int)(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2)), 2, ref uOutputOn);

                    if (uOutputOn == 1)
                    {
                        GbVar.GB_CHIP_PK_VAC[i] = true;
                    }
                    else
                    {
                        GbVar.GB_CHIP_PK_VAC[i] = false;
                    }

                    AjmMgr.GetSignalOutputBit((int)(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2)), 3, ref uOutputOn);

                    if (uOutputOn == 1)
                    {
                        GbVar.GB_CHIP_PK_BLOW[i] = true;
                    }
                    else
                    {
                        GbVar.GB_CHIP_PK_BLOW[i] = false;
                    }
                }

                nAxisOffset = 0;
#endif
            }
        }

        public bool GetInput(int no)
        {
            return GbVar.GB_INPUT[no] == 1;
        }

        public bool GetInput(IODF.INPUT no)
        {
            return GbVar.GB_INPUT[(int)no] == 1;
        }

        public double GetAInput(int no)
        {
            return GbVar.GB_AINPUT[no];
        }

        public double GetAInput(IODF.A_INPUT no)
        {
            return GbVar.GB_AINPUT[(int)no];
        }

        public void SetOutput(int no, bool flagOn)
        {
#if _AJIN
            AjmMgr.SetOut(no, flagOn);
            if (OnMonitorOutputState != null)
                OnMonitorOutputState(no, flagOn);
#endif
#if _NOTEBOOK
            GbVar.GB_OUTPUT[no] = flagOn ? 1 : 0;
#endif
        }

        public void SetOutput(IODF.OUTPUT no, bool flagOn)
        {
#if _AJIN
            AjmMgr.SetOut((int)no, flagOn);
            if (OnMonitorOutputState != null)
                OnMonitorOutputState((int)no, flagOn);
#endif
#if _NOTEBOOK
            GbVar.GB_OUTPUT[(int)no] = flagOn ? 1 : 0;
#endif
        }

        public void SetAOutput(int nCh, double dValue)
        {
#if _AJIN
            AjmMgr.SetAOut(nCh, dValue);
#endif
        }

        [Obsolete("사용 안함. SetTriggerOneShot으로 대체")]
        public bool SetCntTriggerOutput(int nCh, bool bOnOff)
        {
#if !_AJIN
            return true;
#endif
            //임의로 지정
            uint[] uPos = new uint[]{99999};

            return AjmMgr.SetCountTriggerAbsMode(nCh, uPos, bOnOff);
        }

        public bool SetTriggerOneShotMap()
        {
#if !_AJIN
            return true;
#else

            motion[(int)SVDF.AXES.VISION_TRIGGER_MAP].SetTriggerEnable();
            return motion[(int)SVDF.AXES.VISION_TRIGGER_MAP].TriggerOneShot();
#endif
        }

        public bool SetTriggerOneShotBall()
        {
#if !_AJIN
            return true;
#else

            motion[(int)SVDF.AXES.VISION_TRIGGER_BALL].SetTriggerEnable();
            return motion[(int)SVDF.AXES.VISION_TRIGGER_BALL].TriggerOneShot();
#endif
        }

        /// <summary>
        /// 속도 문제로 안씀....
        /// </summary>
        /// <param name="nAxis"></param>
        /// <param name="nNo"></param>
        /// <param name="uData"></param>
        /// <returns></returns>
        public bool GetAnalogValue(int nAxis, int nNo, ref uint uData)
        {
#if !_AJIN
            return true;
#endif
            return AjmMgr.GetSDOFromAxisDWord(nAxis, (ushort)eSDO_OBJ_IDX.ANALOG_1, (byte)nNo, ref uData);
        }
        /// <summary>
        /// Chip Picker 전용 아날로그 값 읽는 함수
        /// </summary>
        /// <param name="nAxis"></param>
        /// <param name="uData"></param>
        /// <returns></returns>
        public bool GetAnalogValue(int nAxis, ref int uData)
        {
#if !_AJIN
            return true;
#endif
            int nBoradNo = 2;
            int nAddrIdx = (nAxis - (int)SVDF.AXES.CHIP_PK_1_Z_1) / 2;

            if (nAxis > (int)SVDF.AXES.CHIP_PK_1_T_8)
            {
                nBoradNo = 3;
                nAddrIdx = (nAxis - (int)SVDF.AXES.CHIP_PK_2_Z_1) / 2;
            }

            return AjmMgr.GetECatReadPdoInputEx(nBoradNo, unAnalogPioAddr[nBoradNo - 2, nAddrIdx], nAnalog_Bitlen, ref uData);
        }

        public bool SetSigOutputMask(int nAxis, ushort usMask)
        {
#if !_AJIN
            return true;
#endif
            return AjmMgr.SetSDOFromAxisWord(nAxis, (ushort)eSDO_OBJ_IDX.DIGITAL_OUTPUT_1, 0x02, usMask);
        }

        public bool SetSigOutput(int nAxis, int nBitNo, bool bOn)
        {
#if !_AJIN
            return true;
#endif
            uint uOnOff = Convert.ToUInt32(bOn);
            return AjmMgr.SetSignalOutputBit(nAxis, nBitNo, uOnOff);
        }

        public bool GetSigOutput(int nAxis, int nBitNo, ref uint uData)
        {
#if !_AJIN
            return true;
#endif
            return AjmMgr.GetSignalOutputBit(nAxis, nBitNo, ref uData);
        }


        public bool SetSigOutputSmartIOMode(int nMdl, ushort usMode)
        {
#if !_AJIN
            return true;
#endif
            return AjmMgr.SetSDOFromWord(nMdl, (ushort)eSDO_OBJ_IDX.SMART_IO, 0x02, usMode);
        }


        public void AllStop()
        {
            foreach (MotionBase m in motion)
            {
                m.MoveStop();
            }
        }

        public void AllAlarmClear()
        {
            foreach (MotionBase m in motion)
            {
                m.ResetAlarm();
            }
        }

        public bool AllBuzzerOff()
        {
            SetOutput(IODF.OUTPUT.ERROR_BUZZER, false);
            SetOutput(IODF.OUTPUT.END_BUZZER, false);
            return true;
        }

        public bool ErrorBuzzerOn()
        {
            SetOutput(IODF.OUTPUT.ERROR_BUZZER, true);
            return true;
        }

        public bool ErrorBuzzerOff()
        {
            SetOutput(IODF.OUTPUT.ERROR_BUZZER, false);
            return true;
        }

        public bool EndBuzzerOn()
        {
            SetOutput(IODF.OUTPUT.END_BUZZER, true);
            return true;
        }

        public bool EndBuzzerOff()
        {
            SetOutput(IODF.OUTPUT.END_BUZZER, false);
            return true;
        }

        public bool IsAllStop()
        {
            for (int nAxisNo = 0; nAxisNo < SVDF.SV_CNT; nAxisNo++)
            {
                if (MotionMgr.Inst[nAxisNo] != null)
                {
                    if (MotionMgr.Inst[nAxisNo].GetServoOnOff() &&
                        MotionMgr.Inst[nAxisNo].IsBusy())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 도어 잠금/해제
        /// </summary>
        /// <param name="bLock"></param>
        /// <returns></returns>
        public bool SetDoorLock(bool bLock)
        {
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, bLock);
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DOOR_LOCK_SIGNAL, bLock);

            return true;
        }
    }
}
