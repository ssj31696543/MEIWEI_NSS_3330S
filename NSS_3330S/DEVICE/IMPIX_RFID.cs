using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

using DionesTool.DATA;

namespace NSS_3330S
{
    public class IMPIX_RFID : SerialPort//COM1, 9600, None, One, 8
    {
        #region DELE
        public delegate void DeleException(string strErr);
        public event DeleException deleException = null;

        public delegate void RecvLoaderMgzID(int nPort);
        public event RecvLoaderMgzID recvLoaderMgzID = null;
        #endregion

        #region ENUM
        enum eCMD
        {
            READ = 0x44,//'D'Read
            RECEIVE = 0x41,//'A'RECEIVE
            ERROR = 0x45,//'E'ERROR
        }

        enum ePARAM
        {
            READ = 0x30,//'0'
            OK = 0x32,//'2'
        }

        enum eDATA
        {
            SP = 0x20//' '
        }

        enum eERR_CODE//CMD.ERROR 일경우 해당 이넘 값 참조
        {
            BCC = 0x30,
            NOCHIP = 0x32,
            FAIL = 0x33,
            NOREAD = 0x37,
        }
        #endregion

        #region VAR
        public ConcurrentQueue<int> CMDQueue = new ConcurrentQueue<int>();

        // 포트 정보
        PortInfo portInfo = new PortInfo();
        Stopwatch swTimer = new Stopwatch();

        Thread SendThread;
        bool bAlive = false;
        bool bWait = false;
        int nReadPort = 0;
        string RevData = "";
        #endregion

        #region PROPERTY
        private string[] strReadedId = new string[12];
        public string[] GETID
        {
            get { return strReadedId; }
            set { value = strReadedId; }
        }

        private bool[] isRecv = new bool[12];
        public bool[] ISRECV
        {
            get { return isRecv; }
            set { value = isRecv; }
        }

        public bool ISCONNECT
        {
            get { return IsOpen; }
        }
        #endregion

        public IMPIX_RFID()
            : base() 
        {
            for (int i = 0; i < isRecv.Length; i++)
            {
                isRecv[i] = false;
            }
        }

        ~IMPIX_RFID()
        {
        }

        #region CONNECT
        /// <summary>
        /// 포트를 연다
        /// 부모 형식으로 호출하면 안됨
        /// </summary>
        /// <returns></returns>
        public new bool Open()
        {
            if (IsOpen) return true;

            try
            {
                base.Open();

                bAlive = true;
                SendThread = new Thread(new ThreadStart(Run));
                if(!SendThread.IsAlive)
                    SendThread.Start();
            }
            catch (Exception ex)
            {
                if (deleException != null)
                    deleException(ex.ToString());
                return false;
            }

            DataReceived += RFID_DataReceived;

            return true;
        }

        /// <summary>
        /// 포트를 닫는다
        /// 부모 형식으로 호출하면 안됨
        /// </summary>
        public new void Close()
        {
            if (!IsOpen) return;

            bAlive = true;
            SendThread.Join(500);
            SendThread.Abort();
            SendThread = null;

            DataReceived -= RFID_DataReceived;

            base.Close();
        }
        #endregion

        #region READ WRITE
        private const int CMD = 0;
        private const int PARAM = 1;
        private const int DATA_START = 2;
        private const int DATA_END = 17;
        private const int ID = 18;
        private const int BCC = 19;

        public void Read(int nPort)
        {
            if (!IsOpen) return;
            isRecv[nPort] = false;
            CMDQueue.Enqueue(nPort);
        }
        private void Run()
        {
            while(bAlive)
            {
                Thread.Sleep(10);
                if (CMDQueue.Count > 0 && !bWait)
                {
                    CMDQueue.TryDequeue(out nReadPort);

                    swTimer.Restart();

                    #region CREATE CMD
                    StringBuilder msg = new StringBuilder();
                    int nCnt = 0;

                    //CMD
                    byte[] Data = new byte[20];
                    Data[nCnt++] = (byte)eCMD.READ;
                    Data[nCnt++] = (byte)ePARAM.READ;
                    for (int i = 0; i < 16; i++)
                    {
                        Data[nCnt++] = (byte)eDATA.SP;
                    }
                    Data[nCnt++] = (byte)(Convert.ToChar(nReadPort.ToString()));

                    //BCC = CMD to ID XOR
                    Data[Data.Length - 1] = Data[0];
                    for (int i = 1; i < Data.Length - 1; i++)
                    {
                        Data[Data.Length - 1] = (byte)(Data[Data.Length - 1] ^ Data[i]);
                    }
                    #endregion

                    byte[] data = null;
                    using (MemoryStream m = new MemoryStream())
                    {
                        using (BinaryWriter writer = new BinaryWriter(m))
                        {
                            for (int i = 0; i < Data.Length; i++)
                            {
                                writer.Write(Data[i]);
                            }
                            // 실 byte 데이터
                            data = m.ToArray();
                        }
                    }

                    try
                    {
                        Write(data, 0, data.Length);
                        bWait = true;
                    }
                    catch (Exception ex)
                    {
                        if (deleException != null)
                            deleException(ex.ToString());
                    }
                }

                if (bWait == true)
                {
                    if (swTimer.ElapsedMilliseconds > 3000)
                    {
                        strReadedId[nReadPort] = "TIME-OUT ERR";
                        swTimer.Stop();

                        isRecv[nReadPort] = true;
                        bWait = false;
                        if (recvLoaderMgzID != null)
                            recvLoaderMgzID(nReadPort);
                    }
                }
            }
        }

        /// <summary>
        /// 응답 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RFID_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;

            int length = sp.BytesToRead;
            byte[] buf = new byte[length];
            sp.Read(buf, 0, length);

            RevData += Encoding.ASCII.GetString(buf);

            if (RevData.Length >= 20)
            {
                Parsing(RevData);
                RevData = "";
            }
        }

        void Parsing(string Data)
        {
            int nPort = 0;

            if (!int.TryParse(((char)Data[ID]).ToString(), out nPort)) return;

            switch ((byte)Data[CMD])
            {
                case (byte)eCMD.RECEIVE:
                    strReadedId[nPort] = Data.Substring(DATA_START, 16).Replace(" ","");
                    break;

                case (byte)eCMD.ERROR:
                    if (Data[PARAM] == (byte)eERR_CODE.BCC)
                    {
                        strReadedId[nPort] = "BCC ERROR";
                    }
                    else if (Data[PARAM] == (byte)eERR_CODE.FAIL)
                    {
                        strReadedId[nPort] = "READ FAIL";
                    }
                    else if (Data[PARAM] == (byte)eERR_CODE.NOCHIP)
                    {
                        strReadedId[nPort] = "NO CHIP";
                    }
                    else
                    {
                        strReadedId[nPort] = "UNKNOWN ERR";
                    }
                    break;
                default:
                    break;
            }

            swTimer.Stop();
            isRecv[nPort] = true;
            bWait = false;

            if (recvLoaderMgzID != null)
            {
                recvLoaderMgzID(nPort);
            }
        }
        #endregion

        #region FILE
        /// <summary>
        /// 포트 정보를 파일에 쓰기
        /// </summary>
        /// <param name="filePath">경로</param>
        public bool Serialize(string filePath)
        {
            return portInfo.SetInfo(this, filePath);
        }

        /// <summary>
        /// 파일에서 포트 정보 불러오기
        /// </summary>
        /// <param name="filePath">경로</param>
        public bool Deserialize(string filePath)
        {
            SerialPort port = this;
            return portInfo.GetInfo(ref port, filePath);
        }
        #endregion
    }
}
