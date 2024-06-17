using DionesTool.DATA;
using DionesTool.SOCKET;
using DionesUtil.DATA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace NSSU_3400
{
    public class A_CPM1300 : CommBase
    {
        const int MAX_BUFFER = 100;
        const char STX = (char)0x02;
        const char ETX = (char)0x03;
        //통신 커맨드
        public enum COMMAND
        {
            NONE,
            RUN_ION, //Ion Balance
            STOP_ION,
            RUN_DEC, //Decay Time
            STOP_DEC,
            MAX
        }

        Dictionary<string, string> m_dictChannelMsValue = new Dictionary<string, string>();
        Dictionary<string, string> m_dictPosDecayVol = new Dictionary<string, string>();
        Dictionary<string, string> m_dictNegDecayVol = new Dictionary<string, string>();

        public Dictionary<string,string> ChannelMeasureValue
        {
            get { return m_dictChannelMsValue; }
            set { m_dictChannelMsValue = value; }
        }
        public Dictionary<string, string> PosDecayTimeVoltage
        {
            get { return m_dictPosDecayVol; }
            set { m_dictPosDecayVol = value; }
        }
        public Dictionary<string, string> NegDecayTimeVoltage
        {
            get { return m_dictNegDecayVol; }
            set { m_dictNegDecayVol = value; }
        }
        public string PosDecayTime
        {
            get 
            {
                if (m_DecayTime.lstPosDecayTime.Count > 0)
                {
                    return m_DecayTime.lstPosDecayTime[m_DecayTime.lstPosDecayTime.Count - 1];
                }
                else
                    return "";
            }
        }
        public string NegDecayTime
        {
            get
            {
                if (m_DecayTime.lstNegDecayTime.Count > 0)
                {
                    return m_DecayTime.lstNegDecayTime[m_DecayTime.lstNegDecayTime.Count - 1];
                }
                else
                    return "";
            }
        }
        public string IonBalanceValue
        {
            get
            {
                double dValue = 0.0;
                double.TryParse(m_dictChannelMsValue["1"], out dValue);
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IONIZER_MONITOR_OFFSET].dValue != 0)
                {
                    return string.Format("{0}", dValue * ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IONIZER_MONITOR_OFFSET].dValue);
                }
                return m_IonBalance.strIonBalanceMeasureValue;
            }
        }

        #region PARSING ITEM
        public struct ION_BALANCE
        {
            public string strDeviceID; //컨트롤러 후면에 설정한 아이디
            public string strChannelID; //디폴트 값 1
            public string strIonBalanceSign;
            public string strIonBalanceMeasureValue;
            public string strCommResult;
        }

        public struct DECAY_TIME
        {
            public string strDeviceID; //컨트롤러 후면에 설정한 아이디
            public string strChannelID ; //디폴트 값 1
            public string strPosDecaySignal;
            public string strPosAppliedVoltage;
            public List<string> lstPosDecayTime;
            public string strNegDecaySignal;
            public string strNegAppliedVoltage;
            public List<string> lstNegDecayTime;
            public string strCommResult;
        }
        #endregion

        //public delegate void DeleCommandCompleted(COMMAND cmd, bool bSuccess, string strSend, string strResult);
        //public event DeleCommandCompleted deleCommandCompleted = null;

        string m_strBuffer = "";
        string m_strReceived = "";
        bool m_bReceived = false;

        string m_currentCommCommand = "";
        string m_currentKind = "";
        string m_currentDeviceID = "";
        COMMAND m_currentDataCommand = COMMAND.NONE;
        STATUS m_Status = STATUS.SEND;

        tagSetValue m_SetValue = new tagSetValue();
        public tagGetValue m_GetValue = new tagGetValue();

        int m_nIndexAddCommand = 0;
        int m_nCurrentCmdIdx = 0;
        bool m_bCommand = false;

        List<string> m_listCommCmdMonitor = new List<string>();
        List<string> m_listKindMonitor = new List<string>();
        List<string> m_listDeviceID = new List<string>();
        List<COMMAND> m_listDataCmdMonitor = new List<COMMAND>();

        string[] m_arrCommCommand = new string[MAX_BUFFER];
        string[] m_arrKind = new string[MAX_BUFFER];
        string[] m_arrDeviceID = new string[MAX_BUFFER];
        COMMAND[] m_arrCommand = new COMMAND[MAX_BUFFER];

        string[] m_arrStrCommandSend = new string[MAX_BUFFER];
        string[] m_arrStrCommandResult = new string[MAX_BUFFER];

        bool[] m_arrbErrorCommand = new bool[MAX_BUFFER];
        bool[] m_arrbFlagCommand = new bool[MAX_BUFFER];
        bool[] m_arrbReceivedCommand = new bool[MAX_BUFFER];

        string m_strLastSendMsg = "";
        string m_strLastRcvMsg = "";
        string m_strErrorrDesc = "";

        Thread m_threadRun;
        bool m_bThreadAlive = false;

        Stopwatch m_swTimeout = new Stopwatch();

        // 마지막 정상 통신한 시간
        Stopwatch m_swNormalComm = new Stopwatch();

        private object syncBuffer = new Object();
        //

        ION_BALANCE m_IonBalance = new ION_BALANCE();
        DECAY_TIME m_DecayTime = new DECAY_TIME();

        public ION_BALANCE ION_BALANCE_DATA
        {
            get { return m_IonBalance; }
        }
        public DECAY_TIME DECAY_TIME_DATA
        {
            get { return m_DecayTime; }
        }
        public struct tagSetValue
        {
            public string strZeroOffsetReq;
        }
        public struct tagGetValue
        {
            public string strZeroOffsetReq;
        }

        #region PROPERTY
        public string[] COMM_COMMAND_LIST
        {
            get {return m_arrCommCommand;}
        }
        public string[] KIND_LIST
        {
            get { return m_arrKind; }
        }
        public string[] DEVICE_ID
        {
            get { return m_arrDeviceID; }
        }
        
        public COMMAND[] COMMAND_LIST
        {
            get { return m_arrCommand; }
        }
        
        /// <summary>
        /// 보낸 명령의 응답 문자열
        /// </summary>
        public string[] COMMAND_RESULT
        {
            get { return m_arrStrCommandResult; }
        }

        /// <summary>
        /// 보낸 명령 조합
        /// </summary>
        public string[] COMMAND_SEND
        {
            get { return m_arrStrCommandSend; }
        }

        /// <summary>
        /// 응답을 받았을 때 정상인지, 에러인지
        /// </summary>
        public bool[] COMMAND_ERROR
        {
            get { return m_arrbErrorCommand; }
        }

        /// <summary>
        /// 명령이 들어왔는지 확인
        /// </summary>
        public bool[] COMMAND_FLAG
        {
            get { return m_arrbFlagCommand; }
        }

        /// <summary>
        /// 명령의 응답을 받았는지
        /// </summary>
        public bool[] COMMAND_RECEIVED
        {
            get { return m_arrbReceivedCommand; }
        }

        public string ERROR_DESC
        {
            get { return m_strErrorrDesc; }
        }
        public string LAST_RCV_MSG
        {
            get { return m_strLastRcvMsg; }
        }

        public bool IS_REAL_CONNECT
        {
            get
            {
                if (!m_swNormalComm.IsRunning) return false;

                return m_swNormalComm.Elapsed.TotalSeconds < 5.0;
            }
        }
      
        public tagSetValue SET_VALUE
        {
            set
            {
                m_SetValue = value;
            }
            get
            {
                return m_SetValue;
            }
        }
        public tagGetValue GET_VALUE
        {
            get
            {
                return m_GetValue;
            }
        }
       
        #endregion

        public A_CPM1300() : base()
        {
            m_port.DataBits = 8;
            m_port.StopBits = System.IO.Ports.StopBits.One;
            m_port.Parity = System.IO.Ports.Parity.None;
            m_port.BaudRate = 115200;

            InitDecayTime();

            #region MONITOR
            //
            //실시간 모니터링 하고 싶은 아이템 골라서 Add 하면 됨
            //
            AddMonitoring(A_CPM1300.COMMAND.RUN_ION, "1");
            AddMonitoring(A_CPM1300.COMMAND.RUN_DEC, "1");
            #endregion

            #region ERROR MESSAGE
            //ERROR MSG 
            //foreach (ERROR_NO item in Enum.GetValues(typeof(ERROR_NO)))
            //{
            //    m_dictErrorMsg.Add(((int)item).ToString(), item.ToString());
            //}
            #endregion

            //m_GetValue.strFault = "SYSTEM OK";
        }

       
        public enum STATUS
        {
            SEND,
            WAIT,
        }

        public override bool Serialize(string filePath)
        {
            return base.Serialize(filePath);
        }

        public override bool Deserialize(string filePath)
        {
            return base.Deserialize(filePath);
        }

        /// <summary>
        /// 포트 열기
        /// </summary>
        /// <returns></returns>
        public override bool Open(bool bWait = false)
        {
            bool bOpen = false;

            bOpen = base.Open(bWait);

            if (bOpen)
            {
                if (m_commMode == ECOMM_MODE.SERIAL)
                {
                    if (m_port != null && m_port.IsOpen)
                    {
                        m_port.DiscardInBuffer();
                        m_port.DiscardOutBuffer();
                    }
                }

                StartStopThread(true);

                // 정상 통신 시간 체크용
                m_swNormalComm.Restart();
            }

            return bOpen;
        }

        /// <summary>
        /// 포트 닫기
        /// </summary>
        public override void Close()
        {
            StartStopThread(false);

            //if (!IsConnected()) return;

            base.Close();
        }

        protected override void DataReceived(SerialPort sp, byte[] buf)
        {
            //base.DataReceived(sp, buf);

            string data = Encoding.ASCII.GetString(buf);

            m_strBuffer += data;

            int nPos = m_strBuffer.IndexOf(ETX);
            if (nPos >= 0)
            {
                if (m_bCommand)
                {

                }

                m_strReceived = m_strBuffer.Substring(0, nPos);
                //m_strBuffer = m_strBuffer.Substring(nPos + 2);
                m_strBuffer = "";

                m_bReceived = true;
            }
        }

        protected override void DataReceived(System.Net.Sockets.Socket sw, string data)
        {
            m_strBuffer += data;

            int nPos = m_strBuffer.IndexOf(ETX);
            if (nPos >= 0)
            {
                if (m_bCommand)
                {

                }

                m_strReceived = m_strBuffer.Substring(0, nPos);
                //m_strBuffer = m_strBuffer.Substring(nPos + 2);
                m_strBuffer = "";

                m_bReceived = true;
            }
        }

        void StartStopThread(bool bStart)
        {
            if (m_threadRun != null)
            {
                m_bThreadAlive = false;
                m_threadRun.Join(2000);
                m_threadRun.Abort();
                m_threadRun = null;
            }

            if (bStart)
            {
                m_bThreadAlive = true;
                m_threadRun = new Thread(new ParameterizedThreadStart(ThreadRun));
                m_threadRun.Name = "A-CPM1300 THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            A_CPM1300 ctrl = obj as A_CPM1300;

            int nIndexMonitor = 0;
            string strReceived = "";

            string strCommandName = "";
            string strStatusValue = "";

            int nValue = 0;
            double dValue = 0;
            float fValue = 0.0f;

            Thread.Sleep(3000);
            while (ctrl.m_bThreadAlive)
            {
                Thread.Sleep(5);
                // continue;
                // 보내기 상태라면
                if (m_Status == STATUS.SEND)
                {
                    Thread.Sleep(5);
                    Monitor.Enter(syncBuffer);
                    try
                    {
                        if (m_nCurrentCmdIdx >= m_arrbFlagCommand.Length)
                            m_nCurrentCmdIdx = 0;

                        // 명령이 들어왔다면 명령 수행
                        if (m_arrbFlagCommand[m_nCurrentCmdIdx])
                        {
                            Thread.Sleep(10);
                            m_currentCommCommand = m_arrCommCommand[m_nCurrentCmdIdx];
                            m_currentKind = m_arrKind[m_nCurrentCmdIdx];
                            m_currentDeviceID = m_arrDeviceID[m_nCurrentCmdIdx];
                            m_currentDataCommand = m_arrCommand[m_nCurrentCmdIdx];
                            m_bCommand = true;
                        }
                        // 명령이 안들어왔다면 모니터링 명령 수행
                        else
                        {
                            if (m_listDataCmdMonitor.Count <= 0)
                                continue;

                            if (nIndexMonitor >= m_listDataCmdMonitor.Count)
                                nIndexMonitor = 0;
                            m_bCommand = false;

                            m_currentDataCommand = m_listDataCmdMonitor[nIndexMonitor];
                            m_currentDeviceID = m_listDeviceID[nIndexMonitor++];
                        }

                    }
                    catch (Exception ex)
                    {
                    }
                    finally
                    {
                        Monitor.Exit(syncBuffer);
                    }

                    // 명령 보내고
                    #region ### Send Command ###
                    switch (m_currentDataCommand)
                    {
                        case COMMAND.NONE:
                            break;
                        case COMMAND.RUN_ION:
                            _RunCommandForamt("ION", m_currentDeviceID);
                            break;
                        case COMMAND.STOP_ION:
                            _StopCommandForamt("ION", m_currentDeviceID);
                            break;
                        case COMMAND.RUN_DEC:
                            _RunCommandForamt("DEC", m_currentDeviceID);
                            break;
                        case COMMAND.STOP_DEC:
                            _StopCommandForamt("DEC", m_currentDeviceID);
                            break;
                        case COMMAND.MAX:
                            break;
                        default:
                            break;
                    }
                   
                    //switch ()
                    //{
                    //    case "SR" :
                    //        {
                    //            FieldInfo fio = typeof(DATA_NO).GetField(m_currentDataCommand.ToString());
                    //            string strNo = "";
                    //            strNo = (string)fio.GetValue(strNo);

                    //            _SendReadCommandForamt(m_currentCommCommand, m_currentAmpNo, strNo);
                    //        }
                    //        break;
                    //    case "M0":
                    //        {
                    //            _MZeroCommandFormat();
                    //        }
                    //        break;
                    //    case "SW":
                    //        {
                                
                    //            FieldInfo fio = typeof(DATA_NO).GetField(m_currentDataCommand.ToString());
                    //            string strNo = "";
                    //            strNo = (string)fio.GetValue(strNo);

                    //            string strSetValue = "";
                    //            switch (m_currentDataCommand)
                    //            {
                    //                case DATA_NO_COMMAND.NONE:
                    //                    break;
                    //                case DATA_NO_COMMAND.MAX:
                    //                    break;
                    //                default:
                    //                    break;
                    //            }
                    //            _SendWriteCommandForamt(m_currentCommCommand, m_currentAmpNo, strNo, strSetValue);
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}

                    #endregion

                    // 응답 대기 상태로 변경
                    m_Status = STATUS.WAIT;

                    m_swTimeout.Restart();
                }
                else if (m_Status == STATUS.WAIT)
                {
                    if (m_bReceived)
                    {
                        strReceived = m_strReceived;
                        m_bReceived = false;

                        // 정상 통신 시간 체크용
                        m_swNormalComm.Restart();

                        // m_strReceived  "Error, bad command" string
                        if (strReceived.Contains("ER"))
                        {
                            // Error
                        }

                        if (m_bCommand)
                        {

                        }
                        string strReplyData = "";

                        //Parsing
                        ParsingData(strReceived, out strReplyData);

                        switch (m_currentDataCommand)
                        {
                            case COMMAND.NONE:
                                break;
                            case COMMAND.RUN_ION:
                                break;
                            case COMMAND.STOP_ION:
                                break;
                            case COMMAND.RUN_DEC:
                                break;
                            case COMMAND.STOP_DEC:
                                break;
                            case COMMAND.MAX:
                                break;
                            default:
                                break;
                        }
                        if (m_bCommand)
                        {
                            //if (m_arrStrCommandReplyCode[(int)m_currentCommand] != ((int)REPLY_CODE.OK).ToString())
                            //{

                            //}
                            // 에러가 있다면
                            m_arrbErrorCommand[m_nCurrentCmdIdx] = strReceived.Contains("Error");

                            // 결과 값 저장
                            m_arrStrCommandResult[m_nCurrentCmdIdx] = strReceived;

                            // 명령 종료 플래그
                            m_arrbReceivedCommand[m_nCurrentCmdIdx] = true;

                            // 다음 버퍼로
                            m_arrbFlagCommand[m_nCurrentCmdIdx] = false;

                            if (m_port != null)
                            {
#if !_NOTEBOOK
                                this.m_port.DiscardInBuffer();
                                this.m_port.DiscardOutBuffer();
#else
#endif

                            }

                            //// 결과 전달
                            //if (deleCommandCompleted != null)
                            //    deleCommandCompleted(m_arrDataNoCommand[m_nCurrentCmdIdx],
                            //                         !m_arrbErrorCommand[m_nCurrentCmdIdx],
                            //                         m_arrStrCommandSend[m_nCurrentCmdIdx],
                            //                         m_arrStrCommandResult[m_nCurrentCmdIdx]);

                            // 다음 명령어
                            m_nCurrentCmdIdx++;
                        }

                        m_Status = STATUS.SEND;
                    }

                    // Timeout
                    double dTimeOut = 1;
                    if (Debugger.IsAttached)
                        dTimeOut = 1;
                    if (m_swTimeout.Elapsed.TotalSeconds > dTimeOut)
                    {
                        if (m_bCommand)
                        {
                            // 에러가 있다면
                            m_arrbErrorCommand[m_nCurrentCmdIdx] = true;

                            // 결과 값 저장
                            m_arrStrCommandResult[m_nCurrentCmdIdx] = "Timeout";

                            // 명령 종료 플래그
                            m_arrbReceivedCommand[m_nCurrentCmdIdx] = true;

                            // 다음 버퍼로
                            m_arrbFlagCommand[m_nCurrentCmdIdx] = false;

                            //if (deleCommandCompleted != null)
                            //    deleCommandCompleted(m_arrDataNoCommand[m_nCurrentCmdIdx],
                            //                         !m_arrbErrorCommand[m_nCurrentCmdIdx],
                            //                         m_arrStrCommandSend[m_nCurrentCmdIdx],
                            //                         m_arrStrCommandResult[m_nCurrentCmdIdx]);

                            if (m_port != null && m_port.IsOpen)
                            {
                                m_port.DiscardInBuffer();
                                m_port.DiscardOutBuffer();
                            }

                            // 다음 명령어
                            m_nCurrentCmdIdx++;
                        }

                        m_Status = STATUS.SEND;
                        continue;
                    }
                }
            }
        }

        bool SendFormat(string cmd, params object[] args)
        {
            try
            {
                string send = "";

                if (args != null)
                    send = string.Format(cmd, args);
                else
                    send = cmd;

                m_strLastSendMsg = send;

                if (m_bCommand)
                {
                    m_arrStrCommandSend[m_nCurrentCmdIdx] = send;
                }

                return Send(send);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
      
        public void AddMonitoring(COMMAND cmd, string deviceID)
        {
            m_listDataCmdMonitor.Add(cmd);
            m_listDeviceID.Add(deviceID);
        }
        int AddCommand(string strCommCmd, string strKind, string strDeviceID = "1")
        {
            if (m_nIndexAddCommand >= m_arrCommand.Length)
                m_nIndexAddCommand = 0;
            if (strCommCmd == "RUN")
            {
                if (strKind == "ION")
                {
                    m_arrCommand[m_nIndexAddCommand] = COMMAND.RUN_ION;
                }
                else
                {
                    m_arrCommand[m_nIndexAddCommand] = COMMAND.RUN_DEC;
                }
            }
            else
            {
                if (strKind == "ION")
                {
                    m_arrCommand[m_nIndexAddCommand] = COMMAND.STOP_ION;
                }
                else
                {
                    m_arrCommand[m_nIndexAddCommand] = COMMAND.STOP_DEC;
                }
            }


            m_arrCommCommand[m_nIndexAddCommand] = strCommCmd;
            m_arrKind[m_nIndexAddCommand] = strKind;
            m_arrDeviceID[m_nIndexAddCommand] = strDeviceID;
            m_arrStrCommandResult[m_nIndexAddCommand] = "";
            m_arrbErrorCommand[m_nIndexAddCommand] = false;
            m_arrbReceivedCommand[m_nIndexAddCommand] = false;
            m_arrbFlagCommand[m_nIndexAddCommand] = true;

            return m_nIndexAddCommand++;
        }

        #region 내부 함수
        void _RunCommandForamt(string strKind, string strDeviceID = "1")
        {
            SendFormat("{0}RUN,{1},{2}{3}", STX, strKind, strDeviceID, ETX);
        }
        void _StopCommandForamt(string strKind, string strDeviceID = "1")
        {
            SendFormat("{0}STOP,{1},{2}{3}", STX, strKind, strDeviceID, ETX);
        }
        void _SendWriteCommandForamt(string strCommand, string strDeviceID = "1")
        {
            SendFormat("{0}{1},{2}{3}", STX, strCommand, strDeviceID, ETX);
        }

        bool ParsingData(string strData, out string strReplyData)
        {
            strData = strData.Replace("\r\n", "");
            strData = strData.Replace(string.Format("{0}", STX), "");
            strData = strData.Replace(string.Format("{0}", ETX), "");

            string[] arrData = strData.Split(',');
            m_strLastRcvMsg = strData;
            switch (arrData[1])
            {
                case "ION":
                    m_IonBalance.strDeviceID = arrData[2];
                    m_IonBalance.strChannelID = arrData[3];
                    m_IonBalance.strIonBalanceSign = arrData[4];
                   
                    m_IonBalance.strIonBalanceMeasureValue = arrData[5];
                    
                    m_IonBalance.strCommResult = arrData[6];
                    DictAddOrUpdate(m_dictChannelMsValue, m_IonBalance.strDeviceID, string.Format("{0}{1}", m_IonBalance.strIonBalanceSign, m_IonBalance.strIonBalanceMeasureValue));
                    break;
                case "DEC":

                    m_DecayTime.strDeviceID = arrData[2];
                    m_DecayTime.strChannelID = arrData[3];
                    int nNextIdx = 0;
                    int nPosIdx = 0;
                    int nNegIdx = 0;

                    if (arrData[4] == "OK")
                    {
                        m_DecayTime.strCommResult = arrData[4];
                    }
                    else
                    {
                        m_DecayTime.strPosDecaySignal = arrData[4];
                        m_DecayTime.strPosAppliedVoltage = arrData[5];
                        DictAddOrUpdate(m_dictPosDecayVol,m_DecayTime.strDeviceID, string.Format("{0}V",m_DecayTime.strPosAppliedVoltage));
                        #region POS 구간별 통과 시간
                        for (int nIdx = 6; nIdx < arrData.Length; nIdx++)
                        {
                            if (arrData[nIdx].Contains("NEG"))
                            {
                                nNextIdx = nIdx;
                                break;
                            }

                            ListAddOrUpdate(m_DecayTime.lstPosDecayTime, nPosIdx, arrData[nIdx]);
                            nPosIdx++;
                        }
                        #endregion

                        if (nNextIdx == 0)
                        {
                            //Error
                        }
                        m_DecayTime.strNegDecaySignal = arrData[nNextIdx];
                        m_DecayTime.strNegAppliedVoltage = arrData[nNextIdx + 1];
                        DictAddOrUpdate(m_dictNegDecayVol, m_DecayTime.strDeviceID, string.Format("{0}V", m_DecayTime.strNegAppliedVoltage));
                        #region NEG 구간별 통과 시간 and 통신 처리 결과
                        for (int nIdx = nNextIdx + 2; nIdx < arrData.Length; nIdx++)
                        {
                            if (arrData[nIdx].Contains("OK") ||
                                arrData[nIdx].Contains("NG") ||
                                arrData[nIdx].Contains("FAIL"))
                            {
                                m_DecayTime.strCommResult = arrData[nIdx];
                                break;
                            }

                            ListAddOrUpdate(m_DecayTime.lstNegDecayTime, nNegIdx, arrData[nIdx]);
                            nNegIdx++;
                        }
                        #endregion
                    }

                    
                   
                    break;

                default:
                    break;
            }
           
            strReplyData = "";
            return true;
        }

        #endregion

        //public int GetModelName1(string strAmpNo = "00")
        //{
        //    return AddCommand("SR", strAmpNo, A_CPM1300.DATA_NO_COMMAND.MODEL_NAME_1);
        //}

        public int RunCommandForamt(string strKind, string strDeviceID)
        {
            return AddCommand("RUN", strKind, strDeviceID);
        }
        public int StopCommandForamt(string strKind, string strDeviceID)
        {
            return AddCommand("STOP", strKind, strDeviceID);
        }

        public string ChangeBinary(string strNumValue)
        {
            string strBinary = "";
            int nNum = 0;
            int.TryParse(strNumValue, out nNum);

            strBinary = Convert.ToString(nNum, 2);

            return strBinary;
        }

        public void InitDecayTime()
        {
            m_DecayTime.lstPosDecayTime = new List<string>();
            m_DecayTime.lstNegDecayTime = new List<string>();
        }
        void ListAddOrUpdate(List<string> list, int nIdx, string newValue)
        {
            if (list.Count > nIdx)
            {
                list[nIdx] = newValue;    
            }
            else
            {
                list.Add(newValue);
            }
        }
        void DictAddOrUpdate(Dictionary<string, string> dic, string key, string newValue)
        {
            string val;
            if (dic.TryGetValue(key, out val))
            {
                // value exists! overwrite
                dic[key] = newValue;
            }
            else
            {
                // darn, lets add the value
                dic.Add(key, newValue);
            }
        }
    }
}
