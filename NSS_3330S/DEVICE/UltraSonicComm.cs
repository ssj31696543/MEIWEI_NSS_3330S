using DionesTool.DATA;
using Modbus.Device;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSSU_3400.DEVICE
{
    public class UltraSonicComm
    {
        bool m_bWriteFlag = false;
        public const int MAX_ID_CNT = 100;
        public const int MAX_POWER = 600; //단위는 W (Watt)

        #region Address
        //03H: Read Holding Register
        //06H: Write Single Register
        //10H: Write Multiple Register

        #region FC (03H) READ
        public const int START_READ_ADDR = 1001;
        public const int CHECK_SETTING_FREQ_CHANNEL = 1001; // 1: 40khZ, 2: 80kHz, 3: 120kHz, Range: 1~3
        public const int CHECK_SETTING_OUTPUT = 1002; // MaxPower 초과 못함 Range: 0~600
        public const int CHECK_CURRENT_OUTPUT = 1003; // MaxPower 초과 못함 Range: xxx
        public const int CHECK_CURRENT_FREQ = 1004; // 36.1kHz인 경우 361로 송신 Range: xxx.x
        public const int CHECK_OSCILLATOR_STATE = 1005; // 0 : 정지, 1 : 동작 Range: 0, 1
        public const int CHECK_ALARM_STATE = 1006; // 0: 정상, 1: Temp알람, 2: T/D 알람 Range: 0~2
        #endregion

        #region FC (06H) or (10H) WRITE
        public const int SET_FREQ_CHANNEL_SETTING = 1201; // 1: 40khZ, 2: 80kHz, 3: 120kHz Range: 1~3
        public const int SET_OPERATION_OUTPUT_SETTING = 1202; // 10Watt 단위로 설정 가능, MaxPower 초과 못함 Range: 0~600
        public const int SET_OSCILLATOR_ON_OFF_CONTROL = 1300; // 0: 정지, 1: 동작 Range: 0,1
        public const int SET_ALARM_CLEAR = 1301; // 3: 알람해제 Range: 3
        #endregion

        #endregion

        public ushort Setting_Freq_Channel { get; set; }
        public ushort Setting_Output { get; set; }
        public ushort Current_Output { get; set; }
        public ushort Current_Freq { get; set; }
        public ushort Oscillaotr_State { get; set; }
        public ushort Alarm_State { get; set; }
        public int ID { get; set; }

        #region 파라미터 값
        public enum FREQ_CHANNEL
        {
            NONE,
            FREQ_40KHZ,
            FREQ_80KHZ,
            FREQ_120KHZ,
        }
        public enum OSCILLATOR_STATE
        {
            STOP,
            RUN,
        }
        public enum AlarmState
        {
            NORMAL,
            TEMP_ALARM,
            TD_ALARM,
            ALARM_CLEAR,
        }
        /// <summary>
        /// * 통신으로는 에러코드를 확인하는 방법은 없으나 매뉴얼에 존재하므로 기재하였다.
        /// </summary>
        public enum eErrorCode
        {
            NONE,
            CODE_ERROR,         //UNSUPPORTED ERROR
            ADDRESS_ERROR,      //OUT OF RANGE
            VALUE_ERROR,        //OUT OF RANGE
            TIME_ERROR_1,       //<3.5 CHARACTER TIME
            TIME_ERROR_2,       //>1.5 CHARACTER TIME
            CRC16_ERROR,
        }
        #endregion
        public UltraSonicComm()
        {
            ID = 1;
        }
        //TcpClient m_client = null;
        //IModbusMaster m_master = null;
        ModbusSerialMaster m_master = null;

        Thread m_threadRun = null;
        bool m_bThreadAlive = false;

        readonly object m_objLock = new object();

        // 현재 측정 값
        int m_nCurPV = 0;

        // 포트 정보
        PortInfo portInfo = new PortInfo();
        public SerialPort m_serialPort = new SerialPort();

        public bool Serialize(string filePath)
        {
            if (!portInfo.SetInfo(m_serialPort, filePath))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 파일에서 포트 정보 불러오기
        /// </summary>
        /// <param name="filePath">경로</param>
        public bool Deserialize(string filePath)
        {
            if (!portInfo.GetInfo(ref m_serialPort, filePath))
            {
                return false;
            }

            return true;
        }
        public bool Connect()
        {
            if (m_master != null)
            {
                m_master.Dispose();
            }

            if (m_serialPort != null)
            {
                if (m_serialPort.IsOpen)
                {
                    m_serialPort.Close();
                }
            }

            m_serialPort.ReadTimeout = 500;
            m_serialPort.Open();
            m_master = ModbusSerialMaster.CreateRtu(m_serialPort);

            if (!m_serialPort.IsOpen)
            {
                return false;
            }

            StartStopThread(true);

            return true;
        }

        public void Disconnect()
        {
            StartStopThread(false);

            if (m_master != null)
            {
                m_master.Dispose();
                m_master = null;
            }

            if (m_serialPort != null)
            {
                m_serialPort.Close();
            }
        }

        void StartStopThread(bool bStart)
        {
            if (m_threadRun != null)
            {
                m_bThreadAlive = false;
                m_threadRun.Join(5000);
                m_threadRun.Abort();
                m_threadRun = null;
            }

            if (bStart)
            {
                m_bThreadAlive = true;
                m_threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                m_threadRun.Name = "Ultrasonic THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            UltraSonicComm Ctrl = obj as UltraSonicComm;

            Thread.Sleep(2000);

            while (Ctrl.m_bThreadAlive)
            {
                Thread.Sleep(1000);

                if (!m_serialPort.IsOpen)
                    continue;
                if (m_bWriteFlag)
                {
                    continue;
                }

                Task<ushort[]> task;

                //Run 시작하면 읽어오지 않는다. [2022. 05.12 작성자 홍은표]
                if (GbVar.mcState.IsRun() == false)
                {
                    // Exception 처리 필요 (포트가 닫혀있을 수 있음)
                    try
                    {
                        task = m_master.ReadHoldingRegistersAsync((byte)ID, (ushort)START_READ_ADDR, (ushort)6);

                        if (task.Wait(1000) == false)
                        {
                            Thread.Sleep(2000);
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }

                    ushort[] nRecvArr = task.Result;

                    byte[] byteRecvArr = new byte[nRecvArr.Length];
                    for (int nCnt = 0; nCnt < nRecvArr.Length; nCnt++)
                    {
                        byteRecvArr[nCnt] = (byte)nRecvArr[nCnt];
                    }

                    Monitor.Enter(m_objLock);
                    try
                    {
                        if (nRecvArr.Length == 6)
                        {
                            Setting_Freq_Channel = nRecvArr[0];
                            Setting_Output = nRecvArr[1];
                            Current_Output = nRecvArr[2];
                            Current_Freq = nRecvArr[3];
                            Oscillaotr_State = nRecvArr[4];
                            Alarm_State = nRecvArr[5];
                        }
                    }
                    finally
                    {
                        Monitor.Exit(m_objLock);
                    }
                }
            }
        }

        public bool ReadUltraSonic(int nID, int nAddress, out ushort nValue)
        {
            nValue = 0;
            Task<ushort[]> task = m_master.ReadHoldingRegistersAsync((byte)nID, (ushort)nAddress, (ushort)2);

            if (task.Wait(50) == false)
            {
                Thread.Sleep(1000);
                return false;
            }

            ushort[] nRecvArr = task.Result;

            byte[] byteRecvArr = new byte[nRecvArr.Length];
            for (int nCnt = 0; nCnt < nRecvArr.Length; nCnt++)
            {
                byteRecvArr[nCnt] = (byte)nRecvArr[nCnt];
            }

            nValue = BitConverter.ToUInt16(byteRecvArr, 0);


            return true;
        }
        public bool AllReadUltraSonic(int nID)
        {
            Task<ushort[]> task = m_master.ReadHoldingRegistersAsync((byte)nID, (ushort)START_READ_ADDR, (ushort)6);

            if (task.Wait(50) == false)
            {
                Thread.Sleep(1000);
                return false;
            }

            ushort[] nRecvArr = task.Result;

            byte[] byteRecvArr = new byte[nRecvArr.Length];
            for (int nCnt = 0; nCnt < nRecvArr.Length; nCnt++)
            {
                byteRecvArr[nCnt] = (byte)nRecvArr[nCnt];
            }

            BitConverter.ToUInt16(byteRecvArr, 0);


            return true;
        }
        public bool WriteUltraSonic(int nID, int nAddress, ushort nValue)
        {
#if _NOTEBOOK
            return true;
#endif
            m_bWriteFlag = true;
            Task task = m_master.WriteSingleRegisterAsync((byte)nID, (ushort)nAddress, nValue);

            if (task.Wait(100) == false)
            {
                Thread.Sleep(1000);
                m_bWriteFlag = false;
                return false;
            }
            m_bWriteFlag = false;
            return true;
        }
        public void SetFreqChannel(int nValue)
        {
            WriteUltraSonic(ID, SET_FREQ_CHANNEL_SETTING, (ushort)nValue);
        }
        public void SetOsillatorOnOff(bool bOn = true)
        {
            WriteUltraSonic(ID, SET_OSCILLATOR_ON_OFF_CONTROL, bOn ? (ushort)OSCILLATOR_STATE.RUN : (ushort)OSCILLATOR_STATE.STOP);
        }
        public void SetOperationOutput(int nValue)
        {
            WriteUltraSonic(ID, SET_OPERATION_OUTPUT_SETTING, (ushort)nValue);
        }
        public void SetAlarmClear()
        {
            WriteUltraSonic(ID, SET_ALARM_CLEAR, (ushort)AlarmState.ALARM_CLEAR);
        }
        ///// <summary>
        ///// Delay 함수 MS
        ///// </summary>
        ///// <param name="MS">(단위 : MS)
        /////
        //private static DateTime Delay(int MS)
        //{
        //    DateTime ThisMoment = DateTime.Now;
        //    TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
        //    DateTime AfterWards = ThisMoment.Add(duration);

        //    while (AfterWards >= ThisMoment)
        //    {
        //        System.Windows.Forms.Application.DoEvents();
        //        ThisMoment = DateTime.Now;
        //    }

        //    return DateTime.Now;
        //}

    }
}
