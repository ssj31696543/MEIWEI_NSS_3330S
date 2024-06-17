using DionesTool.DATA;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using DionesUtil.DATA;

namespace NSSU_3400
{
    public class DSH_20_B7_HT : CommBase
    {
        const int MAX_BUFFER = 100;
        //
        // STX + RID + SID + CMD + SC + LEN + DATA + ETX 0번 프로토콜
        // STX + CMD + SC + LEN + DATA + ETX 2번 프로토콜
        private const char STX = (char)0x02;
        private const char ETX = (char)0x03;
        ///// <summary>
        ///// Chiller ID (ex: "01", 보드 내 딥스위치에서 설정)
        ///// </summary>
        //private string RID { get; set; }
        ///// <summary>
        ///// PC ID ("02"~"90" 선택)
        ///// </summary>
        //private string SID { get; set; }
        /// <summary>
        /// Command
        /// </summary>
        private string CMD { get; set; }
        /// <summary>
        /// Sub Command (0 : REQ, 1: ANS, 2: SET, 3: SET OK, 4: SET FAIL
        /// </summary>
        private string SC { get; set; }
        /// <summary>
        /// Length : Data 길이 
        /// </summary>
        private string LEN { get; set; }
        /// <summary>
        /// 실제 Data
        /// </summary>
        private string DATA { get; set; }

        private string SEND_MESSAGE
        {
            get
            {
                return string.Format(String.Format("{0}{1}{2}{3}{4}{5}", STX, CMD, SC, LEN, DATA, ETX));
            }
        }

        #region Structure

        public enum COMMAND
        {
            NONE,
            TARGET_TEMP,            //040 설정온도
            HIGH_TEMP_WARN,         //041 1차 Warning
            HIGH_TEMP_ERR,          //042 2차 Alarm
            LOW_TEMP_WARN,          //043 1차 Warning
            LOW_TEMP_ERR,           //044 2차 Alarm
            CH0_REAL_TEMP,          //150 Water Tank 내부온다 
            CH1_REAL_TEMP,          //151 Water Return 온도
            SYSTEM_OP_STATUS,       //154동작 및 상태
            SYSTEM_ERR_STATUS,      //156 에러코드
            FLOW,                   //157 Laser에서 칠러로 유입되는 유량
            PID_CONTROL_VALUE,     //158 PID 제어출력값
            MAX,
        }
        public string[] arrCOMMAND = { "NONE", "040", "041", "042", "043", "044", "150", "151", "154", "156", "157", "158", "MAX" };

        public Dictionary<string, COMMAND> dictCmd = new Dictionary<string, COMMAND>();


        public enum STATUS
        {
            SEND,
            WAIT,
        }

        public enum SUB_COMMAND
        {
            NONE = -1,
            REQ,            //REQUEST
            ANS,            //ANSWER
            SET,            //SET 요청
            SET_OK,         //SET 성공
            SET_FAIL,       //SET 실패
        }
        public enum ERROR_CODE
        {
            NO_ERROR,
            COMP_OVERF_HEAT_ALARM,         //알람
            OVER_CURRENT_TRIP_ALARM,       //알람
            LOW_PRESSURE_ERROR_ALARM,      //알람
            HIGH_PRESSURE_ERROR_ALARM,     //알람
            FLOW_METER_SENSOR_ERROR_ALARM, //알람
            FLOW_SWITCH_SENSOR_ERROR_ALARM,//알람
            FLOW_METER_LOW_ERROR_ALARM,    //알람
            FLOW_SWITCH_LOW_ERROR_ALARM,   //알람
            TEMP_SENSOR_ERROR_ALARM,       //알람
            LEVEL_SENSOR_ERROR_ALARM,      //알람
            LOW_LEVEL_2_ALARM,             //알람
            LOW_LEVEL_1_WARNING,           //경고
            LOW_TEMP_2_ALARM,              //알람
            LOW_TEMP_1_WARNING,            //경고
            HIGH_TEMP_2_ALARM,             //알람
            HIGH_TEMP_1_WARNING_ALARM,     //경고
            MAX,
        }
        public bool[] arrErrorCode = new bool[(int)ERROR_CODE.MAX];
        
        public void SetErrorCode(int nErrNum)
        {
            int nNoError = 0;
            //정수형 이진화 변환 작업
            string binary = "";
            binary = Convert.ToString(nErrNum, 2);
            char[] charsArr = binary.ToCharArray();
            Array.Reverse(charsArr);

            for (int i = 0; i < charsArr.Length; i++)
            {
                arrErrorCode[i] = charsArr[i] == '1';
                if(arrErrorCode[i])
                {
                    if(((ERROR_CODE)i+1).ToString().Contains("ALARM"))
                    {
                        m_GetValue.strErrorString = "ALARM";
                        nNoError++;
                    }
                    else if (((ERROR_CODE)i+1).ToString().Contains("WARNING") &&
                        m_GetValue.strErrorString != "ALARM")
                    {
                        m_GetValue.strErrorString = "WARNING";
                        nNoError++;
                    }
                }
            }
            if (nNoError > 0)
            {
                m_GetValue.strErrorString = "NO ERROR";
            }
        }

        public enum SYSTEM_OPERATION_STATUS
        {
            STANDBY,
            START_CHECK,
            PUMP_START,
            PUMP_DELAY,
            PREP_START,
            PREP_DELAY,
            READY_START,
            READY_DELAY,
            RUN,
            STOP,
            STOP_DELAY,
            MAX
        }

        public struct tagSetValue
        {
            //public string strRID_ToChiller;
            //public string strSID_FromPC;
            public string strData;
            public string strOpData;
            public string strTargetTemp;
            public string strHighTemp1;
            public string strHighTemp2;
            public string strLowTemp1;
            public string strLowTemp2;
            public string strCH0RealTemp;
            public string strCH1RealTemp;
            public string strFlow;
            public string strPIDConValue;
            public string strSysOpStatus;
        }

        public struct tagGetValue
        {
            //public string strRID_ToPC;
            //public string strSID_FromChiller;
            public string strData;
            public string strTargetTemp;
            public string strHighTemp1;
            public string strHighTemp2;
            public string strLowTemp1;
            public string strLowTemp2;
            public string strCH0RealTemp;
            public string strCH1RealTemp;
            public string strFlow;
            public string strPIDConValue;
            public string strSysOpStatus;
            public string strSysErrStatus;
            public string strSettingOkFail;
            public string strErrorString;
        }


        #endregion

        public delegate void DeleException(string strErr);
        public event DeleException deleException = null;

        public delegate void DeleCommandCompleted(COMMAND cmd, bool bSuccess, string strSend, string strResult);
        public event DeleCommandCompleted deleCommandCompleted = null;

        string m_strBuffer = "";
        string m_strReceived = "";
        bool m_bReceived = false;

        COMMAND m_currentCommand = COMMAND.NONE;
        SUB_COMMAND m_currentSubCommand = SUB_COMMAND.REQ;
        STATUS m_Status = STATUS.SEND;
        public tagSetValue m_SetValue = new tagSetValue();
        public tagGetValue m_GetValue = new tagGetValue();

        int m_nIndexAddCommand = 0;
        int m_nCurrentCmdIdx = 0;
        bool m_bCommand = false;

        List<COMMAND> m_listMonitor = new List<COMMAND>();
        List<SUB_COMMAND> m_listSubMonitor = new List<SUB_COMMAND>();

        COMMAND[] m_arrCommand = new COMMAND[MAX_BUFFER];
        SUB_COMMAND[] m_arrSubCommand = new SUB_COMMAND[MAX_BUFFER];

        string[] m_arrStrCommandSend = new string[MAX_BUFFER];
        string[] m_arrStrCommandResult = new string[MAX_BUFFER];
        bool[] m_arrbErrorCommand = new bool[MAX_BUFFER];
        bool[] m_arrbFlagCommand = new bool[MAX_BUFFER];
        bool[] m_arrbReceivedCommand = new bool[MAX_BUFFER];

        string m_strLastSendMsg = "";
        string m_strErrorrDesc = "";

        Thread m_threadRun;
        bool m_bThreadAlive = false;

        Stopwatch m_swTimeout = new Stopwatch();

        Dictionary<string, string> m_dictErrorMsg = new Dictionary<string, string>();

        // 마지막 정상 통신한 시간
        Stopwatch m_swNormalComm = new Stopwatch();

        private object syncBuffer = new Object();
        //
        string[] m_arrStrCommandReplyCode = new string[(int)COMMAND.MAX];

        #region PROPERTY
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

        public bool IS_REAL_CONNECT
        {
            get
            {
                return m_swNormalComm.Elapsed.TotalSeconds < 5.0;
            }
        }

        public string[] COMMAND_REPLY_CODE
        {
            get { return m_arrStrCommandReplyCode; }
        }
        public tagSetValue SET_VALUE
        {
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

        public DSH_20_B7_HT() : base()
        {
            m_port.DataBits = 8;
            m_port.StopBits = System.IO.Ports.StopBits.One;
            m_port.Parity = System.IO.Ports.Parity.None;
            m_port.BaudRate = 19200;

            #region MONITOR
            //
            //실시간 모니터링 하고 싶은 아이템 골라서 Add 하면 됨
            //
            m_listMonitor.Add(COMMAND.TARGET_TEMP);
            m_listSubMonitor.Add(SUB_COMMAND.REQ);

            m_listMonitor.Add(COMMAND.CH0_REAL_TEMP);
            m_listSubMonitor.Add(SUB_COMMAND.REQ);

            m_listMonitor.Add(COMMAND.CH1_REAL_TEMP);
            m_listSubMonitor.Add(SUB_COMMAND.REQ);

            m_listMonitor.Add(COMMAND.SYSTEM_ERR_STATUS);
            m_listSubMonitor.Add(SUB_COMMAND.REQ);

            m_listMonitor.Add(COMMAND.SYSTEM_OP_STATUS);
            m_listSubMonitor.Add(SUB_COMMAND.REQ);
            #endregion
            foreach (COMMAND item in Enum.GetValues(typeof(COMMAND)))
            {
                dictCmd.Add(arrCOMMAND[(int)item], item);
            }

            #region ERROR MESSAGE
            // ERROR MSG 
            //foreach (STATUS_CODE item in Enum.GetValues(typeof(STATUS_CODE)))
            //{
            //    m_dictErrorMsg.Add(((int)item).ToString(), item.ToString());
            //}
            #endregion

            //m_GetValue.strFault = "SYSTEM OK";
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
            string data = Encoding.ASCII.GetString(buf);

            m_strBuffer += data;

            if (m_strBuffer.Contains(ETX))
            {
                if (m_bCommand)
                {

                }

                m_strReceived = m_strBuffer;
                m_strBuffer = "";

                m_bReceived = true;
            }
        }

        protected override void DataReceived(System.Net.Sockets.Socket sw, string data)
        {
            m_strBuffer += data;

            if (m_strBuffer.Contains(ETX))
            {
                if (m_bCommand)
                {

                }

                m_strReceived = m_strBuffer;
                m_strBuffer = "";

                m_bReceived = true;
            }
        }
        /// <summary>
        /// 통신 확인후 재연결
        /// </summary>
        
        void Connected_CK()
        {
#if !_CONNECTCHECK
            popMessageBox msg;
            int length = m_port.BytesToRead;
            byte[] buf = new byte[length];
            DataReceived(m_port, buf);
            if (m_bReceived)
            {

                msg = new popMessageBox("정상적인 통신 상태입니다.", "정상");
                msg.ShowDialog();
            }
            else
            {
                msg = new popMessageBox("연결이 불안정한 상태입니다 다시 연결하겠습니까?", "비정상");
                if (msg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Close();
                    Open();
                    StartStopThread(true);
                }
            }
#endif
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
                m_threadRun.Name = "DSH 20 B7 HT THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            DSH_20_B7_HT ctrl = obj as DSH_20_B7_HT;

            int nIndexMonitor = 0;
            string strReceived = "";

            string strCommandName = "";
            string strStatusValue = "";

            int nValue = 0;
            double dValue = 0;
            float fValue = 0.0f;

            Thread.Sleep(2000);
            while (ctrl.m_bThreadAlive)
            {
                Thread.Sleep(50);
                // continue;
                // 보내기 상태라면
                if (m_Status == STATUS.SEND)
                {
                    Thread.Sleep(50);
                    Monitor.Enter(syncBuffer);
                    try
                    {
                        if (m_nCurrentCmdIdx >= m_arrbFlagCommand.Length)
                            m_nCurrentCmdIdx = 0;

                        // 명령이 들어왔다면 명령 수행
                        if (m_arrbFlagCommand[m_nCurrentCmdIdx])
                        {
                            Thread.Sleep(100);
                            m_currentCommand = m_arrCommand[m_nCurrentCmdIdx];
                            m_currentSubCommand = m_arrSubCommand[m_nCurrentCmdIdx];
                            m_bCommand = true;
                            //m_arrStrCommandReplyCode[(int)m_currentCommand] = ""; 
                        }
                        // 명령이 안들어왔다면 모니터링 명령 수행
                        else
                        {
                            if (m_listMonitor.Count <= 0)
                                continue;

                            if (nIndexMonitor >= m_listMonitor.Count)
                                nIndexMonitor = 0;
                            m_bCommand = false;

                            m_currentSubCommand = m_listSubMonitor[nIndexMonitor];
                            m_currentCommand = m_listMonitor[nIndexMonitor];

                            nIndexMonitor++;
                        }

                    }
                    catch (Exception ex)
                    {
                        if (deleException != null)
                            deleException(ex.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(syncBuffer);
                    }

                    // 명령 보내고
                    #region ### Send Command ###
                    string dataValue = "";

                    switch (m_currentCommand)
                    {
                        case COMMAND.NONE:
                            break;
                        case COMMAND.TARGET_TEMP:
                        case COMMAND.HIGH_TEMP_WARN:
                        case COMMAND.HIGH_TEMP_ERR:
                        case COMMAND.LOW_TEMP_WARN:
                        case COMMAND.LOW_TEMP_ERR:
                            {
                                switch (m_currentSubCommand)
                                {
                                    //단순 요청이기 때문 RECEIVE에서 DATA 확인 필요
                                    case SUB_COMMAND.REQ:
                                        dataValue = "";
                                        break;
                                    //설정이기 때문 DATA값이 들어가야한다.
                                    case SUB_COMMAND.SET:
                                        dataValue = SET_VALUE.strData;
                                        break;
                                    // 칠러에서 주는 커맨드여서 RECEIVE할때 설정하면 됨
                                    case SUB_COMMAND.ANS:
                                    case SUB_COMMAND.SET_OK:
                                    case SUB_COMMAND.SET_FAIL:
                                        break;
                                    default:
                                        break;
                                }
                                SetSendFormatValue(arrCOMMAND[(int)m_currentCommand], String.Format("{0}", (int)m_currentSubCommand), dataValue);
                                SendFormat();
                            }
                            break;

                        case COMMAND.CH0_REAL_TEMP:
                        case COMMAND.CH1_REAL_TEMP:
                        case COMMAND.FLOW:
                        case COMMAND.PID_CONTROL_VALUE:
                            {
                                //설정 값이 없음
                                SetSendFormatValue(arrCOMMAND[(int)m_currentCommand], String.Format("{0}", (int)m_currentSubCommand));
                                SendFormat();
                            }
                            break;
                        case COMMAND.SYSTEM_OP_STATUS:
                            {
                                switch (m_currentSubCommand)
                                {
                                    //단순 요청이기 때문 RECEIVE에서 DATA 확인 필요
                                    case SUB_COMMAND.REQ:
                                        dataValue = "";
                                        break;
                                    //설정이기 때문 DATA값이 들어가야한다.
                                    case SUB_COMMAND.SET:
                                        dataValue = SET_VALUE.strOpData;
                                        break;
                                    // 칠러에서 주는 커맨드여서 RECEIVE할때 설정하면 됨
                                    case SUB_COMMAND.ANS:
                                    case SUB_COMMAND.SET_OK:
                                    case SUB_COMMAND.SET_FAIL:
                                        break;
                                    default:
                                        break;
                                }
                                SetSendFormatValue(arrCOMMAND[(int)m_currentCommand], String.Format("{0}", (int)m_currentSubCommand), dataValue);
                                SendFormat();
                            }
                            break;
                        case COMMAND.SYSTEM_ERR_STATUS:
                            //설정 값이 없음
                            SetSendFormatValue(arrCOMMAND[(int)m_currentCommand], String.Format("{0}", (int)m_currentSubCommand));
                            SendFormat();
                            break;
                        case COMMAND.MAX:
                            break;
                        default:
                            break;
                    }

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

                        ParsingReceived(strReceived);

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

                            m_currentCommand = COMMAND.NONE;
                            m_currentSubCommand = SUB_COMMAND.NONE;

                            if (m_port != null)
                            {
#if !_NOTEBOOK
                                this.m_port.DiscardInBuffer();
                                this.m_port.DiscardOutBuffer();
#else
#endif

                            }

                            // 결과 전달
                            if (deleCommandCompleted != null)
                                deleCommandCompleted(m_arrCommand[m_nCurrentCmdIdx],
                                                     !m_arrbErrorCommand[m_nCurrentCmdIdx],
                                                     m_arrStrCommandSend[m_nCurrentCmdIdx],
                                                     m_arrStrCommandResult[m_nCurrentCmdIdx]);

                            // 다음 명령어
                            m_nCurrentCmdIdx++;
                        }

                        m_Status = STATUS.SEND;
                    }

                    // Timeout
                    double dTimeOut = 10.0;
                    if (Debugger.IsAttached)
                        dTimeOut = 10000;
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

                            if (deleCommandCompleted != null)
                                deleCommandCompleted(m_arrCommand[m_nCurrentCmdIdx],
                                                     !m_arrbErrorCommand[m_nCurrentCmdIdx],
                                                     m_arrStrCommandSend[m_nCurrentCmdIdx],
                                                     m_arrStrCommandResult[m_nCurrentCmdIdx]);

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

        bool SetSendFormatValue(string strCMD, string strSC, string strData = "")
        {
            CMD = strCMD;
            SC = strSC;
            DATA = strData;
            if (DATA != "")
            {
                char[] buffer = DATA.ToCharArray();
                LEN = buffer.Length.ToString();
            }
            else
            {
                LEN = "00";
            }

            return false;
        }

        bool SendFormat()
        {
            try
            {
                string send = "";

                m_strLastSendMsg = SEND_MESSAGE;

                if (m_bCommand)
                {
                    m_arrStrCommandSend[m_nCurrentCmdIdx] = SEND_MESSAGE;
                }

                return Send(SEND_MESSAGE);
            }
            catch (Exception ex)
            {
                if (deleException != null)
                    deleException(ex.ToString());
                return false;
            }
        }

        byte GetCheckSum()
        {
            byte checksum = 0x00;

            DionesTool.ALGORITHM.CRC8Calc calc = new DionesTool.ALGORITHM.CRC8Calc(DionesTool.ALGORITHM.CRC8_POLY.CRC8_CCITT);

            byte[] val = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A };
            calc.Checksum(val);

            return checksum;
        }

        int AddCommand(COMMAND cmd, SUB_COMMAND sub_cmd)
        {
            if (m_nIndexAddCommand >= m_arrCommand.Length)
                m_nIndexAddCommand = 0;

            m_arrCommand[m_nIndexAddCommand] = cmd;
            m_arrSubCommand[m_nIndexAddCommand] = sub_cmd;
            m_arrStrCommandResult[m_nIndexAddCommand] = "";
            m_arrbErrorCommand[m_nIndexAddCommand] = false;
            m_arrbReceivedCommand[m_nIndexAddCommand] = false;
            m_arrbFlagCommand[m_nIndexAddCommand] = true;

            return m_nIndexAddCommand++;
        }

        #region ### EXTERNAL FUNCTION ###
        public int GetTargetTemp()
        {
            return AddCommand(COMMAND.TARGET_TEMP, SUB_COMMAND.REQ);
        }
        public int SetTargetTemp(string strTemp)
        {
            m_SetValue.strData = strTemp;
            return AddCommand(COMMAND.TARGET_TEMP, SUB_COMMAND.SET);
        }
        public int GetHighTemp1()
        {
            return AddCommand(COMMAND.HIGH_TEMP_WARN, SUB_COMMAND.REQ);
        }
        public int SetHighTemp1(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strHighTemp1 = strTemp;
            return AddCommand(COMMAND.HIGH_TEMP_WARN, SUB_COMMAND.SET);
        }
        public int GetHighTemp2()
        {
            return AddCommand(COMMAND.HIGH_TEMP_ERR, SUB_COMMAND.REQ);
        }
        public int SetHighTemp2(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strHighTemp2 = strTemp;
            return AddCommand(COMMAND.HIGH_TEMP_ERR, SUB_COMMAND.SET);
        }
        public int GetLowTemp1()
        {
            return AddCommand(COMMAND.LOW_TEMP_WARN, SUB_COMMAND.REQ);
        }
        public int SetLOWTemp1(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strLowTemp1 = strTemp;
            return AddCommand(COMMAND.LOW_TEMP_WARN, SUB_COMMAND.SET);
        }
        public int GetLowTemp2()
        {
            return AddCommand(COMMAND.LOW_TEMP_ERR, SUB_COMMAND.REQ);
        }
        public int SetLowTemp2(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strLowTemp2 = strTemp;
            return AddCommand(COMMAND.LOW_TEMP_ERR, SUB_COMMAND.SET);
        }
        public int GetCH0RealTemp()
        {
            return AddCommand(COMMAND.CH0_REAL_TEMP, SUB_COMMAND.REQ);
        }
        public int SetCH0RealTemp(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strCH0RealTemp = strTemp;
            return AddCommand(COMMAND.CH0_REAL_TEMP, SUB_COMMAND.SET);
        }
        public int GetCH1RealTemp()
        {
            return AddCommand(COMMAND.CH1_REAL_TEMP, SUB_COMMAND.REQ);
        }
        public int SetCH1RealTemp(string strTemp)
        {
            m_SetValue.strData = strTemp;
            m_SetValue.strCH1RealTemp = strTemp;
            return AddCommand(COMMAND.CH1_REAL_TEMP, SUB_COMMAND.SET);
        }
        public int GetFlow()
        {
            return AddCommand(COMMAND.FLOW, SUB_COMMAND.REQ);
        }
        public int SetFlow(string strValue)
        {
            m_SetValue.strData = strValue;
            m_SetValue.strFlow = strValue;
            return AddCommand(COMMAND.FLOW, SUB_COMMAND.SET);
        }
        public int GetPIDConValue()
        {
            return AddCommand(COMMAND.PID_CONTROL_VALUE, SUB_COMMAND.REQ);
        }
        public int SetPIDConValue(string strValue)
        {
            m_SetValue.strData = strValue;
            m_SetValue.strPIDConValue = strValue;
            return AddCommand(COMMAND.PID_CONTROL_VALUE, SUB_COMMAND.SET);
        }
        public int GetSysOpStatus()
        {
            return AddCommand(COMMAND.SYSTEM_OP_STATUS, SUB_COMMAND.REQ);
        }
        public int SetSysOpStatus(string strValue)
        {
            m_SetValue.strData = strValue;
            m_SetValue.strSysOpStatus = strValue;
            return AddCommand(COMMAND.SYSTEM_OP_STATUS, SUB_COMMAND.SET);
        }
        public int GetSysErrStatus()
        {
            return AddCommand(COMMAND.SYSTEM_ERR_STATUS, SUB_COMMAND.REQ);
        }
        #endregion

        #region ### INTERNAL FUNCTION ###



        #endregion


        /// <summary>
        /// Parsing the received ones.
        /// </summary>
        /// <param name="strRcv"></param>
        void ParsingReceived(string strRcv)
        {
            char[] arrChar = strRcv.ToCharArray();
            string stx = "";
            string rid = "";
            string sid = "";
            string cmd = "";
            string sc = "";
            string len = "";
            string data = "";
            string etx = "";
            try
            {
                //stx = strRcv.Substring(0, 1);
                //rid = strRcv.Substring(1, 2);
                //sid = strRcv.Substring(3, 2);
                //cmd = strRcv.Substring(5, 3);
                //sc = strRcv.Substring(8, 1);
                //len = strRcv.Substring(9, 2);
                stx = strRcv.Substring(0, 1);
                cmd = strRcv.Substring(1, 3);
                sc = strRcv.Substring(4, 1);
                len = strRcv.Substring(5, 2);
                if (len != "00")
                {
                    int nLength = 0;
                    int.TryParse(len, out nLength);
                    data = strRcv.Substring(7, nLength);
                    etx = strRcv.Substring(7 + nLength, 1);
                }
                else
                {
                    data = "";
                    etx = strRcv.Substring(7, 1);
                }
                if (len != "00")
                {
                    COMMAND command = COMMAND.NONE;
                    dictCmd.TryGetValue(cmd, out command);
                    switch (command)
                    {
                        case COMMAND.NONE:
                            break;
                        case COMMAND.TARGET_TEMP:
                            m_GetValue.strTargetTemp = data;
                            break;
                        case COMMAND.HIGH_TEMP_WARN:
                            m_GetValue.strHighTemp1 = data;
                            break;
                        case COMMAND.HIGH_TEMP_ERR:
                            m_GetValue.strHighTemp2 = data;
                            break;
                        case COMMAND.LOW_TEMP_WARN:
                            m_GetValue.strLowTemp1 = data;
                            break;
                        case COMMAND.LOW_TEMP_ERR:
                            m_GetValue.strLowTemp2 = data;
                            break;
                        case COMMAND.CH0_REAL_TEMP:
                            m_GetValue.strCH0RealTemp = data;
                            break;
                        case COMMAND.CH1_REAL_TEMP:
                            m_GetValue.strCH1RealTemp = data;
                            break;
                        case COMMAND.SYSTEM_OP_STATUS:
                            m_GetValue.strSysOpStatus = data;
                            break;
                        case COMMAND.SYSTEM_ERR_STATUS:
                            m_GetValue.strSysErrStatus = data;
                            int nChkValue = 0;
                            int.TryParse(data, out nChkValue);
                            SetErrorCode(nChkValue);
                            break;
                        case COMMAND.FLOW:
                            m_GetValue.strFlow = data;
                            break;
                        case COMMAND.PID_CONTROL_VALUE:
                            m_GetValue.strPIDConValue = data;
                            break;
                        case COMMAND.MAX:
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    m_GetValue.strSettingOkFail = (sc == "3") ? "OK" : "FAIL";
                }
            }
            catch (Exception)
            {

            }

        }
    }
}
