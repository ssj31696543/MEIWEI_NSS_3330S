using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Diagnostics;

using DionesTool.SOCKET;
using DionesTool.DATA;

using AxXCYREM2Lib;

namespace NSS_3330S
{
    public class CEYON_RFID : TcpClientAsync
    {
        public delegate void RecvTrayId();
        public event RecvTrayId recvTrayId;

        const int DataLen = 8;

        #region Var
        Thread m_threadRun = null;
        bool m_bThreadAlive = false;


        string m_strIp = "";
        int m_nPort = 0;

        Queue<byte[]> RecvQueue = new Queue<byte[]>();
        List<byte> RecvData = new List<byte>();

        IPInfo ipInfo = new IPInfo();

        Stopwatch swTimer = new Stopwatch();
        #endregion

        #region Enum
        public enum eCOMMCHAR
        {
            STX = 0x02,
            ETX = 0x03,
            ENQ = 0x05,
            ACK = 0x06,
            NAK = 0x15,
        }

        public enum eCMD
        {
            READ_REG = 0x08,
            READ_TAG = 0x80,
            WRITE_REG = 0x18,
            WRITE_TAG = 0x90,
        }

        public enum eDATA
        {
            ERFCH = 0x03,
            CFG0 = 0x0A,
            CFG1 = 0x0B,
            CMD0 = 0x18,
            RTB = 0x1B,
            RTA = 0x1C,
            VTO = 0x1D,
        }

        public enum eERROR
        {
            NOERR,
            UNKNOWN_CMD_ID = 0x01,
            WRITE_CMD_FAIL = 0x0D,
            RF_CHANNEL_DISABLED = 0x10,
            ICONDE_TIMEOUT = 0x16,
        }
        #endregion

        #region Property
        public string IPADD
        {
            get { return m_strIp; }
            set { m_strIp = value; }
        }

        public int PORTNO
        {
            get { return m_nPort; }
            set { m_nPort = value; }
        }

        bool bRecvWaiting = false;
        public bool RECVWAITING
        {
            get { return bRecvWaiting; }
            set { bRecvWaiting = value; }
        }

        public bool IsConnect
        {
            get { return IsConnected; }
        }


        string m_strRecv = "";
        public string GETID
        {
            get { return m_strRecv; }
            set { m_strRecv = value; }
        }
        #endregion
 
        public CEYON_RFID()
        {
            deleConnected += OnConnect;
            deleDisconnected += OnDisconnect;
        }

        #region Conncect
        public override bool Connect(bool wait = false, SocketType socketType = SocketType.Stream, ProtocolType prototype = ProtocolType.Tcp, int nBufferSize = 1)
        {
            if (IsConnected) return true;

            bool bConnect = false;
            try
            {
                SetClientInfo(m_strIp, m_nPort);
                bConnect = base.Connect(wait, socketType, prototype, nBufferSize);
            }
            catch (Exception ex)
            {
                return false;
            }

            if (bConnect)
            {
                if (m_bThreadAlive == false)
                {
                    StartStopThread(true);
                }

                this.deleReceivedBytes += CEYON_RFID_deleReceived;
            }

            return bConnect;
        }

        /// <summary>
        /// 포트 닫기
        /// </summary>
        public override void Close()
        {
            StartStopThread(false);
            this.deleReceivedBytes -= CEYON_RFID_deleReceived;

            if (!IsConnected) return;

            base.Close();
        }
        private void OnConnect(bool bConnected, string msg = "")
        {
            //이전 명렁어의 응답이 오기 전까지는 다음명령어 실행 막음.
            if (bRecvWaiting) return;
            bRecvWaiting = true;

            swTimer.Restart();

            #region Packet Description
            //시작 문자
            //R ID
            //읽기 명령 + 포트번호(0번 base)
            //테그를 읽을 시작 주소
            //데이터 길이
            //체크섬 (이번까지의 바이트 다 더한 값)
            //마무리 문자 -> (프로토콜 문서에는 있는 예문도 있고 없는 예문도 있음. Test해보고 안되면 빼야할 수도있음)
            #endregion
            byte[] data = new byte[8];
            data[0] = (byte)eCOMMCHAR.ENQ;
            data[1] = 0x01;
            data[2] = (byte)eCMD.WRITE_REG;
            data[3] = (byte)eDATA.ERFCH;
            data[4] = 0x01;
            data[5] = 0xFF;

            for (int i = 0; i < 6; i++)
            {
                data[6] += data[i];//체크썸
            }

            Send(data);
        }

        private void OnDisconnect(string msg = "")
        {

        }
        #endregion

        #region Thread
        void StartStopThread(bool bStart)
        {
            if (m_threadRun != null)
            {
                m_bThreadAlive = false;
                m_threadRun.Join(1000);
                m_threadRun.Abort();
                m_threadRun = null;
            }

            if (bStart)
            {
                m_bThreadAlive = true;
                m_threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                m_threadRun.Name = "HYPER NX THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            CEYON_RFID ctrl = obj as CEYON_RFID;

            while (ctrl.m_bThreadAlive)
            {
                Thread.Sleep(10);

#if !_NOTEBOOK
                if (IsConnected == false)
                {
                    Connect();
                    Thread.Sleep(1000);
                }
#endif

                #region Low Data Parsing
                if (RecvData.Count > 0)
                {
                    int nStartIndex = 0;
                    int nEndIndex = 0;

                    //보관해둔 데이터의 시작 위치를 검출한다.
                    for (int i = 0; i < RecvData.Count; i++)
                    {
                        if (RecvData[i] == (int)eCOMMCHAR.ENQ || RecvData[i] == (int)eCOMMCHAR.STX)
                        {
                            nStartIndex = i;
                            break;
                        }
                    }

                    //보관해둔 데이터의 끝 위치를 검출한다.
                    nEndIndex = RecvData.IndexOf((int)eCOMMCHAR.ETX);

                    //시작과 끝위치가 정상적인지 확인한다.(짤릴경우 끝위치가 없기때문에 아래의 내용을 건너뛴다.)
                    if (nStartIndex >= 0 && nEndIndex >= 0 && (nEndIndex > nStartIndex))
                    {
                        byte[] data = new byte[nEndIndex - nStartIndex + 1];

                        for (int i = nStartIndex; i < nEndIndex + 1; i++)
                        {
                            data[i] = RecvData[i];
                        }

                        //잘라낸 데이터 
                        RecvQueue.Enqueue(data);

                        //큐에 옴긴 데이터만 삭제
                        RecvData.RemoveRange(nStartIndex, nEndIndex - nStartIndex + 1);
                    }
                }          
                #endregion

                #region Result Parsing
                if (RecvQueue.Count > 0)
                {
                    int nPort = 0;
                    eCMD cmd = new eCMD();
                    byte[] data;

                    data = RecvQueue.Dequeue();

                    if (data[0] == (byte)eCOMMCHAR.NAK)
                    {
                            nPort = data[1];
                            cmd = (eCMD)data[2];

                            string strErr = "UnknownError";

                            if (Enum.IsDefined(typeof(eERROR), Convert.ToInt32(data[3])))
                                strErr = ((eERROR)data[3]).ToString();

                            m_strRecv = "";
                    }
                    else if (data[0] == (byte)eCOMMCHAR.ACK)
	                {
                        //m_strRecv = "Success";
	                }
                    else if (data[0] == (byte)eCOMMCHAR.STX)
	                {
                        if (data[2] == (byte)eCMD.READ_REG)
                        {
                            //레지스터 읽은 응답
                        }
                        else
                        {
                            //태그 읽은 응답
                            nPort = data[2] - (int)eCMD.READ_TAG;

                            //sb.Append(Encoding.ASCII.GetString(data, 3, DataLen));
                            //m_strRecv = sb.ToString();          
                        }

                        m_strRecv = fctByteArray2String(3, DataLen, data);
	                }
                    else
                    {

                    }

                    swTimer.Stop();
                    bRecvWaiting = false;
                    if (recvTrayId != null)
                        recvTrayId();
                }
                #endregion

                #region TimeOut Check
                if (bRecvWaiting)
                {
                    if (swTimer.ElapsedMilliseconds > 5000)
                    {
                        bRecvWaiting = false;
                        swTimer.Stop();
                        m_strRecv = "";

                        if (recvTrayId != null)
                        {
                            recvTrayId();
                        }
                    }
                }
                #endregion            
            }
        }
        #endregion

        #region Read Write
        void CEYON_RFID_deleReceived(Socket sw, byte[] msg)
        {

            //패킷이 끊겨서 넘겨져 올것을 대비하여 받은 데이터를 바이트 리스트에 보관해둔다.
            for (int i = 0; i < msg.Length; i++)
            {
                RecvData.Add(msg[i]);
            }     
        }

        public bool Read(int nPort)
        {
            try
            {
                if (!IsConnected) return false;

                //이전 명렁어의 응답이 오기 전까지는 다음명령어 실행 막음.
                if (bRecvWaiting) return false;
                bRecvWaiting = true;
                swTimer.Restart();

                #region Packet Description
                //시작 문자
                //R ID
                //읽기 명령 + 포트번호(0번 base)
                //테그를 읽을 시작 주소
                //데이터 길이
                //체크섬 (이번까지의 바이트 다 더한 값)
                //마무리 문자 -> (프로토콜 문서에는 있는 예문도 있고 없는 예문도 있음. Test해보고 안되면 빼야할 수도있음)
                #endregion
                byte[] data = new byte[6];
                data[0] = (byte)eCOMMCHAR.ENQ;
                data[1] = 0x01;
                data[2] = (byte)(eCMD.READ_TAG + nPort);
                data[3] = 0x00;//테그를 읽을 시작 주소
                data[4] = (byte)DataLen;//테그의 길이

                for (int i = 0; i < 5; i++)
                {
                    data[5] += data[i];//체크썸
                }
                //data[6] = (byte)eCOMMCHAR.ETX;

                Send(data);
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Data Trans Fn
        // Hex 스트링을 Byte 배열로 변환
        public Byte[] fctHexString2ByteArray(string sData)
        {
            int nLen;
            sData = sData.Replace(" ", "");

            Byte[] buffer = new byte[sData.Length / 2];

            if (sData.Length % 2 == 0)
                nLen = sData.Length;
            else
                nLen = sData.Length - 1;

            for (int i = 0; i < nLen; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(sData.Substring(i, 2), 16);
            }

            return buffer;
        }

        /// <summary>
        /// Byte 배열의 값을 문자열로 변환
        /// </summary>
        /// <param name="nStart">변환할 배열의 시작</param>
        /// <param name="nLen">변환할 길이</param>
        /// <param name="bytePacket">데이터</param>
        /// <returns></returns>
        public string fctByteArray2String(int nStart, int nLen, Byte[] bytePacket)
        {
            string sReturn = "";
            try
            {
                int nCount = bytePacket.Length;

                for (int i = nStart; i < nStart + nLen + 1; i++)
                {
                    if (bytePacket[i] < 33 || bytePacket[i] > 128) // ASCII값 33보다 작거나 128보다 크면 " "으로 변환 처리
                        sReturn += " ";
                    else
                        sReturn += String.Format("{0}", Convert.ToChar(bytePacket[i]));
                }
            }
            catch (Exception)
            {
                sReturn = "";
            }
            return sReturn;
        }
        #endregion

        #region File
        ///<summary>
        ///포트 정보를 파일에 쓰기
        ///</summary>
        ///<param name="filePath">경로</param>
        public bool Serialize(string filePath)
        {
            return ipInfo.SetInfo(m_strIp, m_nPort, filePath);
        }

        /// <summary>
        /// 파일에서 포트 정보 불러오기
        /// </summary>
        /// <param name="filePath">경로</param>
        public bool Deserialize(string filePath)
        {
            return ipInfo.GetInfo(ref m_strIp, ref m_nPort, filePath);
        }
        #endregion
    }
}
