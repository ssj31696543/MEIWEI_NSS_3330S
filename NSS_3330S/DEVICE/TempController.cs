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
    public class TempController
    {
   
        public TempController()
        {
            
        }

        ModbusSerialMaster m_master = null;

        Thread m_threadRun = null;
        bool m_bThreadAlive = false;
        bool m_bWriteFlag = false;
        bool m_bReadFlag = false;

        readonly object m_objLock = new object();

        // 현재 측정 값
        int m_nCurPV = 0;
        // RUN STOP STATUS
        int m_nRun = 0; //0이면 RUN 1이면 STOP
        // SV 설정 값
        int m_nSV = 0; 
        // HEAT 
        int m_nHeat = 0;
        //COOL
        int m_nCool = 0;

        public const ushort GET_PV_VALUE = (ushort)0x03E8;
        public const ushort SET_SV_VALUE = (ushort)0x0000;
        public const ushort GET_SV_VALUE = (ushort)0x03EB;
        public const ushort          RUN = (ushort)0x0032; //0 RUN 1 STOP
        

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
                m_threadRun.Name = "Temp Controller THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            TempController Ctrl = obj as TempController;

            Thread.Sleep(2000);

            while (Ctrl.m_bThreadAlive)
            {
                Thread.Sleep(500);

                if (!m_serialPort.IsOpen)
                    continue;
                if (m_bWriteFlag || m_bReadFlag)
                {
                    continue;
                }
                try
                {
                    Task<ushort[]> task = m_master.ReadInputRegistersAsync((byte)1, (ushort)GET_PV_VALUE, (ushort)6);
                    //Task<ushort[]> task = m_master.ReadWriteMultipleRegistersAsync((byte)1, (ushort)GET_PV_VALUE, (ushort)1);
                    if (task.Wait(1000) == false)
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

                    int nValue = byteRecvArr[0];

                    Monitor.Enter(m_objLock);
                    try
                    {
                        if (nRecvArr.Length == 6)
                        {
                            m_nCurPV = nRecvArr[0];
                            //Setting_Output = nRecvArr[1];
                            //Current_Output = nRecvArr[2];
                            m_nSV = nRecvArr[3];
                            m_nHeat = nRecvArr[4];
                            m_nCool = nRecvArr[5];
                        }
                        m_nCurPV = nValue;
                    }
                    finally
                    {
                        Monitor.Exit(m_objLock);
                    }
					//Run 시작하면 읽어오지 않는다. [2022. 05.12 작성자 홍은표]
                    if (GbVar.mcState.IsRun() == false)
                    {
                        task = m_master.ReadHoldingRegistersAsync((byte)1, (ushort)RUN, (ushort)1);

                        if (task.Wait(100) == false)
                        {
                            Thread.Sleep(2000);
                            continue;
                        }

                        nRecvArr = task.Result;

                        byteRecvArr = new byte[nRecvArr.Length];
                        for (int nCnt = 0; nCnt < nRecvArr.Length; nCnt++)
                        {
                            byteRecvArr[nCnt] = (byte)nRecvArr[nCnt];
                        }

                        nValue = byteRecvArr[0];

                        Monitor.Enter(m_objLock);
                        try
                        {
                            m_nRun = nValue;
                        }
                        finally
                        {
                            Monitor.Exit(m_objLock);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Thread.Sleep(2000);
                    continue;
                }

            }
        }
        public int GetCurrentSV()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nSV;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            return nValue;
        }
        public int GetCurrentPV_Int()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nCurPV;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            return nValue;
        }

        public double GetCurrentPV_Double()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nCurPV;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            double dValue = nValue * 0.001;
            return dValue;
        }
        public int GetCurrentHeatValue()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nHeat;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            return nValue;
        }
        public int GetCurrentCoolValue()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nCool;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            return nValue;
        }
        public int GetRunStopStatus()
        {
            int nValue = 0;
            Monitor.Enter(m_objLock);
            try
            {
                nValue = m_nRun;
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }

            return nValue;
        }
        public bool WriteTempController(int nAddress, ushort nValue)
        {
            if (m_master == null) return false;
            m_bWriteFlag = true;
            Task task = m_master.WriteSingleRegisterAsync((byte)1, (ushort)nAddress, nValue);

            if (task.Wait(500) == false)
            {
                Thread.Sleep(1000);
                m_bWriteFlag = false;
                return false;
            }
            m_bWriteFlag = false;
            return true;
        }
        public bool ReadTempController(int nAddr)
        {
            if (m_master == null) return false;
            m_bReadFlag = true;
            Task<ushort[]> task = m_master.ReadHoldingRegistersAsync((byte)1, (ushort)nAddr, (ushort)1);

            if (task.Wait(10) == false)
            {
                Thread.Sleep(2000);
                m_bReadFlag = false;
                return false;
            }

            ushort[] nRecvArr = task.Result;

            byte[] byteRecvArr = new byte[nRecvArr.Length];
            for (int nCnt = 0; nCnt < nRecvArr.Length; nCnt++)
            {
                byteRecvArr[nCnt] = (byte)nRecvArr[nCnt];
            }

            int nValue = byteRecvArr[0];

            Monitor.Enter(m_objLock);
            try
            {
                if (nAddr == RUN)
                {
                    m_nRun = nValue;
                }
                else if (nAddr == GET_SV_VALUE)
                {
                    m_nSV = nValue;
                }
            }
            finally
            {
                Monitor.Exit(m_objLock);
            }
            m_bReadFlag = false;
            return true;
            return false;
        }
        public void StartTempController(int nStart = 0)
        {
            WriteTempController(RUN, (ushort)nStart);
        }
        public void SetValue(int nValue)
        {
            WriteTempController(SET_SV_VALUE, (ushort)nValue);
        }
        public void SetHeatValue(int nValue)
        {
            WriteTempController((ushort)0x0001, (ushort)nValue);
        }
        public void SetCoolValue(int nValue)
        {
            WriteTempController((ushort)0x0002, (ushort)nValue);
        }
    }
}
