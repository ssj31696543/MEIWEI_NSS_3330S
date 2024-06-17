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

namespace NSS_3330S
{
    public class KYC_IX : CommBase
    {
        const int MAX_BUFFER = 100;
        const int MAX_TOOL = 15;
        //통신 커맨드
        public enum DATA_NO_COMMAND
        {
            NONE,
            ZERO_OFFSET_REQ = 001,
            ZERO_OFFSET_CLEAR_REQ = 002,
            RESET_REQ = 003,

            SENSOR_AMP_ERROR_STATE = 033,
            OUTPUT_STATE = 036,
            CURRENT_PROGRAM_NO = 043,

            ZERO_OFFSET_RESULT = 054,
            RESET_RESULT = 055,

            ZERO_OFFSET_CLEAR_RESULT = 059,

            FTP_FILE_NAME_WRITE_REQ = 129,
            FTP_FILE_NAME_WRITE_RESULT = 130,

            FTP_FILE_NAME_READ_REQ = 131,
            FTP_FILE_NAME_READ_RESULT = 132,

            FTP_FILE_NAME_1 = 133,
            FTP_FILE_NAME_2 = 134,
            FTP_FILE_NAME_3 = 135,
            FTP_FILE_NAME_4 = 136,
            FTP_FILE_NAME_5 = 137,
            FTP_FILE_NAME_6 = 138,
            FTP_FILE_NAME_7 = 139,
            FTP_FILE_NAME_8 = 140,
            FTP_FILE_NAME_9 = 141,
            FTP_FILE_NAME_10 = 142,
            FTP_FILE_NAME_11 = 143,
            FTP_FILE_NAME_12 = 144,
            FTP_FILE_NAME_13 = 145,
            FTP_FILE_NAME_14 = 146,
            FTP_FILE_NAME_15 = 147,
            FTP_FILE_NAME_16 = 148,
            FTP_FILE_NAME_17 = 149,
            FTP_FILE_NAME_18 = 150,
            FTP_FILE_NAME_19 = 151,
            FTP_FILE_NAME_20 = 152,
            FTP_FILE_NAME_21 = 153,
            FTP_FILE_NAME_22 = 154,
            FTP_FILE_NAME_23 = 155,
            FTP_FILE_NAME_24 = 156,
            FTP_FILE_NAME_25 = 157,
            FTP_FILE_NAME_26 = 158,
            FTP_FILE_NAME_27 = 159,

            TRIGGER_OR_TIMING_SETTING = 161,
            EMISSION_STOP_INPUT = 162,
            PROGRAM_CHANGE_REQ = 163,
            PROGRAM_CHANGE_RESULT = 164,
            SETTING_VALUE_READ_REQ = 165,
            SETTING_VALUE_READ_RESULT = 166,
            SETTING_VALUE_ONLY_READ = 167,
            SETTING_VALUE_ONLY_WRITE = 168,
            SETTING_VALUE_WRITE_REQ = 169,
            SETTING_VALUE_WRITE_RESULT = 170,
            MEASURED_VALUE_READ_REQ = 171,
            MEASURED_VALUE_READ_RESULT = 172,
            MEASURED_VALUE_ONLY_READ = 173,

            PRODUCT_CODE = 193,
            MODEL_NAME_1 = 200,
            MODEL_NAME_2 = 201,
            MODEL_NAME_3 = 202,
            MODEL_NAME_4 = 203, 
            MAX
        }
        //데이터 번호
        public class DATA_NO
        {
            public const string ZERO_OFFSET_REQ = "001";
            public const string ZERO_OFFSET_CLEAR_REQ = "002";
            public const string RESET_REQ = "003";

            public const string SENSOR_AMP_ERROR_STATE = "033";
            public const string OUTPUT_STATE = "036";
            public const string CURRENT_PROGRAM_NO = "043";

            public const string ZERO_OFFSET_RESULT = "054";
            public const string RESET_RESULT = "055";

            public const string ZERO_OFFSET_CLEAR_RESULT = "059";

            public const string FTP_FILE_NAME_WRITE_REQ = "129";
            public const string FTP_FILE_NAME_WRITE_RESULT = "130";

            public const string FTP_FILE_NAME_READ_REQ = "131";
            public const string FTP_FILE_NAME_READ_RESULT = "132";

            public const string FTP_FILE_NAME_1 = "133";
            public const string FTP_FILE_NAME_2 = "134";
            public const string FTP_FILE_NAME_3 = "135";
            public const string FTP_FILE_NAME_4 = "136";
            public const string FTP_FILE_NAME_5 = "137";
            public const string FTP_FILE_NAME_6 = "138";
            public const string FTP_FILE_NAME_7 = "139";
            public const string FTP_FILE_NAME_8 = "140";
            public const string FTP_FILE_NAME_9 = "141";
            public const string FTP_FILE_NAME_10 = "142";
            public const string FTP_FILE_NAME_11 = "143";
            public const string FTP_FILE_NAME_12 = "144";
            public const string FTP_FILE_NAME_13 = "145";
            public const string FTP_FILE_NAME_14 = "146";
            public const string FTP_FILE_NAME_15 = "147";
            public const string FTP_FILE_NAME_16 = "148";
            public const string FTP_FILE_NAME_17 = "149";
            public const string FTP_FILE_NAME_18 = "150";
            public const string FTP_FILE_NAME_19 = "151";
            public const string FTP_FILE_NAME_20 = "152";
            public const string FTP_FILE_NAME_21 = "153";
            public const string FTP_FILE_NAME_22 = "154";
            public const string FTP_FILE_NAME_23 = "155";
            public const string FTP_FILE_NAME_24 = "156";
            public const string FTP_FILE_NAME_25 = "157";
            public const string FTP_FILE_NAME_26 = "158";
            public const string FTP_FILE_NAME_27 = "159";

            public const string TRIGGER_OR_TIMING_SETTING = "161";
            public const string EMISSION_STOP_INPUT = "162";
            public const string PROGRAM_CHANGE_REQ = "163";
            public const string PROGRAM_CHANGE_RESULT = "164";
            public const string SETTING_VALUE_READ_REQ = "165";
            public const string SETTING_VALUE_READ_RESULT = "166";
            public const string SETTING_VALUE_ONLY_READ = "167";
            public const string SETTING_VALUE_ONLY_WRITE = "168";
            public const string SETTING_VALUE_WRITE_REQ = "169";
            public const string SETTING_VALUE_WRITE_RESULT = "170";
            public const string MEASURED_VALUE_READ_REQ = "171";
            public const string MEASURED_VALUE_READ_RESULT = "172";
            public const string MEASURED_VALUE_ONLY_READ = "173";

            public const string PRODUCT_CODE = "193";
            public const string MODEL_NAME_1 = "200";
            public const string MODEL_NAME_2 = "201";
            public const string MODEL_NAME_3 = "202";
            public const string MODEL_NAME_4 = "203";
        }

        //에러 번호
        public enum ERROR_NO
        {
            ILLEGAL_COMMAND_ERROR = 0,
            DATA_LENGTH_ERROR = 20,
            NUMBER_OF_PARAMETERS_ERROR = 21,
            PARAMETER_ERROR = 22,
            COMM_ERROR = 29,
            AMP_NUMBER_ERROR = 65,
            EXTENSION_LINE_ERROR = 66,
            WRITE_CONTROL_ERROR = 67
        }

        public enum AMP_ERROR_STATE_BIT
        {
            NONE = -1,
            INNER_COMM_ERROR = 0,
            PROGRAM_DAMAGED_ERROR = 1,
            NON_VOLATILE_MEMORY_ERROR = 2,
            SENSOR_HEAD_DISCONN_ERROR = 3,
            SYSTEM_ERROR = 10,
            FTP_ERROR = 13,
            ZERO_SHIFT_ERROR = 14,
            TRIGGER_ERROR = 15,
            MAX,
        }

        public enum OUTPUT_STATE_BIT
        {
            NONE = -1,
            OUT_1 = 0,
            OUT_2,
            OUT_3,
            OUT_4,
            OUT_5,
            OUT_6,
            OUT_7,
            OUT_8,
            OUT_9,
            OUT_10,   
            MAX,
        }
        public enum FTP_WRITE_BIT
        {
            NONE = -1,
            SEND_CONDITION_1 = 0,
            SEND_CONDITION_2,
            SEND_CONDITION_3,
            SEND_CONDITION_4,
            SYSTEM_RESERVED_1,
            SYSTEM_RESERVED_2,
            SYSTEM_RESERVED_3,
            SYSTEM_RESERVED_4,
            NON_VOLITILE_MEMORY_WRITE, //ON 쓰기, OFF쓰지 않기
            MAX,
        }
        public enum FTP_READ_BIT
        {
            NONE = -1,
            SEND_CONDITION_1 = 0,
            SEND_CONDITION_2,
            SEND_CONDITION_3,
            SEND_CONDITION_4,
            SYSTEM_RESERVED_1,
            SYSTEM_RESERVED_2,
            SYSTEM_RESERVED_3,
            SYSTEM_RESERVED_4,
            READ_REQ_TYPE_DESIGNATION, //ON 불휘발 메모리 읽기, OFF 현재 파일명 읽기
            MAX,
        }
        public int[] m_arrAmpErrorStateBit = new int[(int)AMP_ERROR_STATE_BIT.MAX];
        public int[] m_arrOutputStateBit = new int[(int)OUTPUT_STATE_BIT.MAX];
        public int[] m_arrFTPWriteBit = new int[(int)FTP_WRITE_BIT.MAX];
        public int[] m_arrFTPReadBit = new int[(int)FTP_READ_BIT.MAX];

        public bool IsTrayInspReq = false;
        public List<double[]> ListMeasure = new List<double[]>();
        public bool bManualCheck = false;

        public enum SETTING_VALUE_REQ
        {
            TOOL_1_LOW_SETTING = 0,
            TOOL_1_HIGH_SETTING,
            TOOL_2_LOW_SETTING,
            TOOL_2_HIGH_SETTING,
            TOOL_3_LOW_SETTING,
            TOOL_3_HIGH_SETTING,
            TOOL_4_LOW_SETTING,
            TOOL_4_HIGH_SETTING,
            TOOL_5_LOW_SETTING,
            TOOL_5_HIGH_SETTING,
            TOOL_6_LOW_SETTING,
            TOOL_6_HIGH_SETTING,
            TOOL_7_LOW_SETTING,
            TOOL_7_HIGH_SETTING,
            TOOL_8_LOW_SETTING,
            TOOL_8_HIGH_SETTING,
            TOOL_9_LOW_SETTING,
            TOOL_9_HIGH_SETTING,
            TOOL_10_LOW_SETTING,
            TOOL_10_HIGH_SETTING,
            TOOL_11_LOW_SETTING,
            TOOL_11_HIGH_SETTING,
            TOOL_12_LOW_SETTING,
            TOOL_12_HIGH_SETTING,
            TOOL_13_LOW_SETTING,
            TOOL_13_HIGH_SETTING,
            TOOL_14_LOW_SETTING,
            TOOL_14_HIGH_SETTING,
            TOOL_15_LOW_SETTING,
            TOOL_15_HIGH_SETTING,
            TOOL_16_LOW_SETTING,
            TOOL_16_HIGH_SETTING,
            POSITION_CORRECTION_LOWER_SETTING,
        }
        public enum MEASURED_VALUE_REQ
        {
            TOOL_1 = 0,
            TOOL_2,
            TOOL_3,
            TOOL_4,
            TOOL_5,
            TOOL_6,
            TOOL_7,
            TOOL_8,
            TOOL_9,
            TOOL_10,
            TOOL_11,
            TOOL_12,
            TOOL_13,
            TOOL_14,
            TOOL_15,
            TOOL_16,
            POSITION_CORRECTION,
            SLOPE_CORRECTION_SCAN,
            SLOPE_CORRECTION_LINE,
        }

        public enum EXECUTION_RESULT
        {
            RUNNING = 0,
            NORMAL = 1,
            ABNORMAL = 2,
        }

        public delegate void DeleException(string strErr);
        public event DeleException deleException = null;

        public delegate void DeleCommandCompleted(DATA_NO_COMMAND cmd, bool bSuccess, string strSend, string strResult);
        public event DeleCommandCompleted deleCommandCompleted = null;

        string m_strBuffer = "";
        string m_strReceived = "";
        bool m_bReceived = false;

        string m_currentCommCommand = "";
        string m_currentAmpNo = "";
        DATA_NO_COMMAND m_currentDataCommand = DATA_NO_COMMAND.NONE;
        STATUS m_Status = STATUS.SEND;

        tagSetValue m_SetValue = new tagSetValue();
        public tagGetValue m_GetValue = new tagGetValue();

        int m_nIndexAddCommand = 0;
        int m_nCurrentCmdIdx = 0;
        bool m_bCommand = false;

        List<string> m_listCommCmdMonitor = new List<string>();
        List<string> m_listAmpNoMonitor = new List<string>();
        List<DATA_NO_COMMAND> m_listDataCmdMonitor = new List<DATA_NO_COMMAND>();

        string[] m_arrCommCommand = new string[MAX_BUFFER];
        string[] m_arrAmpNo = new string[MAX_BUFFER];
        DATA_NO_COMMAND[] m_arrDataNoCommand = new DATA_NO_COMMAND[MAX_BUFFER];

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

        Dictionary<string, string> m_dictErrorMsg = new Dictionary<string, string>();

        // 마지막 정상 통신한 시간
        Stopwatch m_swNormalComm = new Stopwatch();

        private object syncBuffer = new Object();
        //
        string[] m_arrStrCommandReplyData_Normal = new string[(int)DATA_NO_COMMAND.MAX];
        string[] m_arrStrCommandReplyData_Expansion = new string[(int)DATA_NO_COMMAND.MAX];

        double[] m_arrMeasuredValue = new double[MAX_TOOL];
        int[] m_arrMeasureCode = new int[MAX_TOOL];

        public struct tagSetValue
        {
            public string strZeroOffsetReq;
            public string strZeroOffsetClearReq;
            public string strResetReq;

            public string strFTPFileNameWriteReq;
            public string strFTPFileNameReadReq;
            public string strFTPFileName1;
            public string strFTPFileName2;
            public string strFTPFileName3;
            public string strFTPFileName4;
            public string strFTPFileName5;
            public string strFTPFileName6;
            public string strFTPFileName7;
            public string strFTPFileName8;
            public string strFTPFileName9;
            public string strFTPFileName10;
            public string strFTPFileName11;
            public string strFTPFileName12;
            public string strFTPFileName13;
            public string strFTPFileName14;
            public string strFTPFileName15;
            public string strFTPFileName16;
            public string strFTPFileName17;
            public string strFTPFileName18;
            public string strFTPFileName19;
            public string strFTPFileName20;
            public string strFTPFileName21;
            public string strFTPFileName22;
            public string strFTPFileName23;
            public string strFTPFileName24;
            public string strFTPFileName25;
            public string strFTPFileName26;
            public string strFTPFileName27;

            public string strTriggerOrTimingInput;
            public string strEmissionStopInput;
            public string strProgramChangeReq;
            public string strSettingValueReadReq;
            public string strSettingValueOnlyWrite;

            public string strSettingValueWriteReq;

            public string strMeasuredValueReadReq;
        }
        public struct tagGetValue
        {
            public string strZeroOffsetReq;
            public string strZeroOffsetClearReq;
            public string strResetReq;

            public string strSensorAmpErrorState;
            public string strSensorOutputState;
            public string strPgmNo;

            public string strZeroOffsetResult;
            public string strResetResult;

            public string strZeroOffsetClearResult;

            public string strFTPFileNameWriteReq;
            public string strFTPFileNameWriteResult;
            public string strFTPFileNameReadReq;
            public string strFTPFileNameReadResult;
            public string strFTPFileName1;
            public string strFTPFileName2;
            public string strFTPFileName3;
            public string strFTPFileName4;
            public string strFTPFileName5;
            public string strFTPFileName6;
            public string strFTPFileName7;
            public string strFTPFileName8;
            public string strFTPFileName9;
            public string strFTPFileName10;
            public string strFTPFileName11;
            public string strFTPFileName12;
            public string strFTPFileName13;
            public string strFTPFileName14;
            public string strFTPFileName15;
            public string strFTPFileName16;
            public string strFTPFileName17;
            public string strFTPFileName18;
            public string strFTPFileName19;
            public string strFTPFileName20;
            public string strFTPFileName21;
            public string strFTPFileName22;
            public string strFTPFileName23;
            public string strFTPFileName24;
            public string strFTPFileName25;
            public string strFTPFileName26;
            public string strFTPFileName27;

            public string strTriggerOrTimingInput;
            public string strEmissionStopInput;
            public string strProgramChangeReq;
            public string strProgramChangeResult;
            public string strSettingValueReadReq;
            public string strSettingValueReadResult;
            public string strSettingValueOnlyRead;
            public string strSettingValueOnlyWrite;

            public string strSettingValueWriteReq;
            public string strSettingValueWriteResult;

            public string strMeasuredValueReadReq;
            public string strMeasuredValueReadResult;
            public string strMeasuredValueOnlyRead;

            public string strProductCode;
            public string strModel1;
            public string strModel2;
            public string strModel3;
            public string strModel4;
        }
        #region PROPERTY
        public string[] COMM_COMMAND_LIST
        {
            get {return m_arrCommCommand;}
        }
        public string[] AMP_NO_LIST
        {
            get { return m_arrAmpNo; }
        }

        public DATA_NO_COMMAND[] DATA_NO_LIST
        {
            get { return m_arrDataNoCommand; }
        }
        public double[] TOOL_VALUE
        {
            get { return m_arrMeasuredValue; }
        }

        public int[] MEASURE_CODE
        {
            get { return m_arrMeasureCode;  }
            set { m_arrMeasureCode = value;}
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
                return m_swNormalComm.Elapsed.TotalSeconds < 5.0;
            }
        }
        public string[] COMMAND_REPLY_DATA_NORMAL
        {
            get { return m_arrStrCommandReplyData_Normal; }
        }
        public string[] COMMAND_REPLY_DATA_EXPANSION
        {
            get { return m_arrStrCommandReplyData_Expansion; }
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
        public int[] AMP_ERROR_STATE
        {
            get { return m_arrAmpErrorStateBit; }
        }
        public int[] OUTPUT_STATE
        {
            get { return m_arrOutputStateBit; }
        }
        public int[] FTP_READ
        {
            get { return m_arrFTPReadBit; }
        }
        public int[] FTP_WRITE
        {
            get { return m_arrFTPWriteBit; }
        }
        #endregion

        public KYC_IX() : base()
        {
            m_port.DataBits = 8;
            m_port.StopBits = System.IO.Ports.StopBits.One;
            m_port.Parity = System.IO.Ports.Parity.None;
            m_port.BaudRate = 9600;

            #region MONITOR
            //
            //실시간 모니터링 하고 싶은 아이템 골라서 Add 하면 됨
            //
            //m_listMonitor.Add(COMMAND.GET_POWER);
            //m_listMonitor.Add(COMMAND.GET_HV);
            //m_listMonitor.Add(COMMAND.GET_REPRATE);
            //m_listMonitor.Add(COMMAND.GET_EGY);
            //m_listMonitor.Add(COMMAND.GET_MODE);
            //m_listMonitor.Add(COMMAND.GET_TRIGGER);
            //m_listMonitor.Add(COMMAND.GET_OPMODE);
            #endregion

            #region ERROR MESSAGE
            //ERROR MSG 
            foreach (ERROR_NO item in Enum.GetValues(typeof(ERROR_NO)))
            {
                m_dictErrorMsg.Add(((int)item).ToString(), item.ToString());
            }
            #endregion

            //m_GetValue.strFault = "SYSTEM OK";
        }

        ~KYC_IX()
        {
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

            int nPos = m_strBuffer.IndexOf("\r\n");
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

            int nPos = m_strBuffer.IndexOf("\r\n");
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
                m_threadRun.Name = "KYC IX THREAD";
                if (m_threadRun.IsAlive == false)
                    m_threadRun.Start(this);
            }
        }

        void ThreadRun(object obj)
        {
            KYC_IX ctrl = obj as KYC_IX;

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
                            Thread.Sleep(100);
                            m_currentCommCommand = m_arrCommCommand[m_nCurrentCmdIdx];
                            m_currentAmpNo = m_arrAmpNo[m_nCurrentCmdIdx];
                            m_currentDataCommand = m_arrDataNoCommand[m_nCurrentCmdIdx];
                            m_bCommand = true;
                            if (m_currentAmpNo == "00")
                            {
                                m_arrStrCommandReplyData_Normal[(int)m_currentDataCommand] = "";
                            }
                            else
                            {
                                m_arrStrCommandReplyData_Expansion[(int)m_currentDataCommand] = "";
                            }
                        }
                        // 명령이 안들어왔다면 모니터링 명령 수행
                        else
                        {
                            if (m_listDataCmdMonitor.Count <= 0)
                                continue;

                            if (nIndexMonitor >= m_listDataCmdMonitor.Count)
                                nIndexMonitor = 0;
                            m_bCommand = false;

                            m_currentCommCommand = m_listCommCmdMonitor[nIndexMonitor++];
                            m_currentAmpNo = m_listAmpNoMonitor[nIndexMonitor++];
                            m_currentDataCommand = m_listDataCmdMonitor[nIndexMonitor++];
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

                    switch (m_currentCommCommand)
                    {
                        case "SR" :
                            {
                                FieldInfo fio = typeof(DATA_NO).GetField(m_currentDataCommand.ToString());
                                string strNo = "";
                                strNo = (string)fio.GetValue(strNo);

                                _SendReadCommandForamt(m_currentCommCommand, m_currentAmpNo, strNo);
                            }
                            break;
                        case "M0":
                            {
                                _MZeroCommandFormat();
                            }
                            break;
                        case "SW":
                            {
                                
                                FieldInfo fio = typeof(DATA_NO).GetField(m_currentDataCommand.ToString());
                                string strNo = "";
                                strNo = (string)fio.GetValue(strNo);

                                string strSetValue = "";
                                switch (m_currentDataCommand)
                                {
                                    case DATA_NO_COMMAND.NONE:
                                        break;
                                    case DATA_NO_COMMAND.ZERO_OFFSET_REQ:
                                        strSetValue = m_SetValue.strZeroOffsetReq; 
                                        break;
                                    case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_REQ:
                                        strSetValue = m_SetValue.strZeroOffsetClearReq; 
                                        break;
                                    case DATA_NO_COMMAND.RESET_REQ:
                                        strSetValue = m_SetValue.strResetReq; 
                                        break;
                                    case DATA_NO_COMMAND.SENSOR_AMP_ERROR_STATE:
                                        break;
                                    case DATA_NO_COMMAND.OUTPUT_STATE:
                                        break;
                                    case DATA_NO_COMMAND.CURRENT_PROGRAM_NO:
                                        break;
                                    case DATA_NO_COMMAND.ZERO_OFFSET_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.RESET_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ:
                                        strSetValue = m_SetValue.strFTPFileNameWriteReq; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_READ_REQ:
                                        strSetValue = m_SetValue.strFTPFileNameReadReq; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_READ_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_1:
                                        strSetValue = m_SetValue.strFTPFileName1; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_2:
                                        strSetValue = m_SetValue.strFTPFileName2; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_3:
                                        strSetValue = m_SetValue.strFTPFileName3; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_4:
                                        strSetValue = m_SetValue.strFTPFileName4; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_5:
                                        strSetValue = m_SetValue.strFTPFileName5; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_6:
                                        strSetValue = m_SetValue.strFTPFileName6; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_7:
                                        strSetValue = m_SetValue.strFTPFileName7; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_8:
                                        strSetValue = m_SetValue.strFTPFileName8; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_9:
                                        strSetValue = m_SetValue.strFTPFileName9; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_10:
                                        strSetValue = m_SetValue.strFTPFileName10; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_11:
                                        strSetValue = m_SetValue.strFTPFileName11; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_12:
                                        strSetValue = m_SetValue.strFTPFileName12; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_13:
                                        strSetValue = m_SetValue.strFTPFileName13; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_14:
                                        strSetValue = m_SetValue.strFTPFileName14; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_15:
                                        strSetValue = m_SetValue.strFTPFileName15; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_16:
                                        strSetValue = m_SetValue.strFTPFileName16; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_17:
                                        strSetValue = m_SetValue.strFTPFileName17; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_18:
                                        strSetValue = m_SetValue.strFTPFileName18; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_19:
                                        strSetValue = m_SetValue.strFTPFileName19; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_20:
                                        strSetValue = m_SetValue.strFTPFileName20; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_21:
                                        strSetValue = m_SetValue.strFTPFileName21; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_22:
                                        strSetValue = m_SetValue.strFTPFileName22; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_23:
                                        strSetValue = m_SetValue.strFTPFileName23; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_24:
                                        strSetValue = m_SetValue.strFTPFileName24; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_25:
                                        strSetValue = m_SetValue.strFTPFileName25; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_26:
                                        strSetValue = m_SetValue.strFTPFileName26; 
                                        break;
                                    case DATA_NO_COMMAND.FTP_FILE_NAME_27:
                                        strSetValue = m_SetValue.strFTPFileName27; 
                                        break;
                                    case DATA_NO_COMMAND.TRIGGER_OR_TIMING_SETTING:
                                        strSetValue = m_SetValue.strTriggerOrTimingInput; 
                                        break;
                                    case DATA_NO_COMMAND.EMISSION_STOP_INPUT:
                                        strSetValue = m_SetValue.strEmissionStopInput; 
                                        break;
                                    case DATA_NO_COMMAND.PROGRAM_CHANGE_REQ:
                                        strSetValue = m_SetValue.strProgramChangeReq; 
                                        break;
                                    case DATA_NO_COMMAND.PROGRAM_CHANGE_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_READ_REQ:
                                        strSetValue = m_SetValue.strSettingValueReadReq; 
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_READ_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_ONLY_READ:
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_ONLY_WRITE:
                                        strSetValue = m_SetValue.strSettingValueOnlyWrite; 
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_WRITE_REQ:
                                        strSetValue = m_SetValue.strSettingValueWriteReq; 
                                        break;
                                    case DATA_NO_COMMAND.SETTING_VALUE_WRITE_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.MEASURED_VALUE_READ_REQ:
                                        strSetValue = m_SetValue.strMeasuredValueReadReq; 
                                        break;
                                    case DATA_NO_COMMAND.MEASURED_VALUE_READ_RESULT:
                                        break;
                                    case DATA_NO_COMMAND.MEASURED_VALUE_ONLY_READ:
                                        break;
                                    case DATA_NO_COMMAND.PRODUCT_CODE:
                                        break;
                                    case DATA_NO_COMMAND.MODEL_NAME_1:
                                        break;
                                    case DATA_NO_COMMAND.MODEL_NAME_2:
                                        break;
                                    case DATA_NO_COMMAND.MODEL_NAME_3:
                                        break;
                                    case DATA_NO_COMMAND.MODEL_NAME_4:
                                        break;
                                    case DATA_NO_COMMAND.MAX:
                                        break;
                                    default:
                                        break;
                                }
                                _SendWriteCommandForamt(m_currentCommCommand, m_currentAmpNo, strNo, strSetValue);
                            }
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

                        if (m_bCommand)
                        {

                        }
                        string strReplyData = "";

                        //Parsing
                        ParsingData(strReceived, out strReplyData);
                        if (m_currentCommCommand != "SW")
                        {

                            switch (m_currentDataCommand)
                            {
                                case DATA_NO_COMMAND.NONE:
                                    break;
                                case DATA_NO_COMMAND.ZERO_OFFSET_REQ:
                                    {
                                        m_GetValue.strZeroOffsetReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_REQ:
                                    {
                                        m_GetValue.strZeroOffsetClearReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.RESET_REQ:
                                    {
                                        m_GetValue.strResetReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SENSOR_AMP_ERROR_STATE:
                                    {
                                        m_GetValue.strSensorAmpErrorState = strReplyData;
                                        string strBinValue = ChangeBinary(strReplyData);
                                        for (int nIdx = 0; nIdx < strBinValue.Length; nIdx++)
                                        {
                                            m_arrAmpErrorStateBit[nIdx] =  strBinValue[strBinValue.Length - (1 + nIdx)] - '0';
                                        }
                                    }
                                    break;
                                case DATA_NO_COMMAND.OUTPUT_STATE:
                                    {
                                        m_GetValue.strSensorOutputState = strReplyData;
                                        string strBinValue = ChangeBinary(strReplyData);
                                        for (int nIdx = 0; nIdx < strBinValue.Length; nIdx++)
                                        {
                                            m_arrOutputStateBit[nIdx] = strBinValue[strBinValue.Length - (1 + nIdx)] - '0';
                                        }
                                    }
                                    break;
                                case DATA_NO_COMMAND.CURRENT_PROGRAM_NO:
                                    {
                                        m_GetValue.strPgmNo = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.ZERO_OFFSET_RESULT:
                                    {
                                        m_GetValue.strZeroOffsetResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.RESET_RESULT:
                                    {
                                        m_GetValue.strResetResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_RESULT:
                                    {
                                        m_GetValue.strZeroOffsetClearResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ:
                                    {
                                        m_GetValue.strFTPFileNameWriteReq = strReplyData;
                                        string strBinValue = ChangeBinary(strReplyData);
                                        for (int nIdx = 0; nIdx < strBinValue.Length; nIdx++)
                                        {
                                            m_arrFTPWriteBit[nIdx] = strBinValue[strBinValue.Length - (1 + nIdx)] - '0';
                                        }
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_RESULT:
                                    {
                                        m_GetValue.strFTPFileNameWriteResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_READ_REQ:
                                    {
                                        m_GetValue.strFTPFileNameReadReq = strReplyData;
                                        string strBinValue = ChangeBinary(strReplyData);
                                        for (int nIdx = 0; nIdx < strBinValue.Length; nIdx++)
                                        {
                                            m_arrFTPReadBit[nIdx] = strBinValue[strBinValue.Length - (1 + nIdx)] - '0';
                                        }
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_READ_RESULT:
                                    {
                                        m_GetValue.strFTPFileNameReadResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_1:
                                    {
                                        m_GetValue.strFTPFileName1 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_2:
                                     {
                                        m_GetValue.strFTPFileName2 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_3:
                                     {
                                        m_GetValue.strFTPFileName3 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_4:
                                     {
                                        m_GetValue.strFTPFileName4 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_5:
                                     {
                                        m_GetValue.strFTPFileName5 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_6:
                                     {
                                        m_GetValue.strFTPFileName6 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_7:
                                     {
                                        m_GetValue.strFTPFileName7 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_8:
                                     {
                                        m_GetValue.strFTPFileName8 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_9:
                                     {
                                        m_GetValue.strFTPFileName9 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_10:
                                     {
                                        m_GetValue.strFTPFileName10 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_11:
                                     {
                                        m_GetValue.strFTPFileName11 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_12:
                                     {
                                        m_GetValue.strFTPFileName12 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_13:
                                     {
                                        m_GetValue.strFTPFileName13 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_14:
                                     {
                                        m_GetValue.strFTPFileName14 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_15:
                                     {
                                        m_GetValue.strFTPFileName15 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_16:
                                     {
                                        m_GetValue.strFTPFileName16 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_17:
                                     {
                                        m_GetValue.strFTPFileName17 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_18:
                                     {
                                        m_GetValue.strFTPFileName18 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_19:
                                     {
                                        m_GetValue.strFTPFileName19 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_20:
                                     {
                                        m_GetValue.strFTPFileName20 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_21:
                                     {
                                        m_GetValue.strFTPFileName21 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_22:
                                     {
                                        m_GetValue.strFTPFileName22 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_23:
                                     {
                                        m_GetValue.strFTPFileName23 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_24:
                                     {
                                        m_GetValue.strFTPFileName24 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_25:
                                     {
                                        m_GetValue.strFTPFileName25 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_26:
                                     {
                                        m_GetValue.strFTPFileName26 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.FTP_FILE_NAME_27:
                                     {
                                        m_GetValue.strFTPFileName27 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.TRIGGER_OR_TIMING_SETTING:
                                    {
                                        m_GetValue.strTriggerOrTimingInput = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.EMISSION_STOP_INPUT:
                                    {
                                        m_GetValue.strEmissionStopInput = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.PROGRAM_CHANGE_REQ:
                                    {
                                        m_GetValue.strProgramChangeReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.PROGRAM_CHANGE_RESULT:
                                    {
                                        m_GetValue.strProgramChangeResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_READ_REQ:
                                    {
                                        m_GetValue.strSettingValueReadReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_READ_RESULT:
                                    {
                                        m_GetValue.strSettingValueReadResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_ONLY_READ:
                                    {
                                        m_GetValue.strSettingValueOnlyRead = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_ONLY_WRITE:
                                    {
                                        m_GetValue.strSettingValueOnlyWrite = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_WRITE_REQ:
                                    {
                                        m_GetValue.strSettingValueWriteReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.SETTING_VALUE_WRITE_RESULT:
                                    {
                                        m_GetValue.strSettingValueWriteResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MEASURED_VALUE_READ_REQ:
                                    {
                                        m_GetValue.strMeasuredValueReadReq = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MEASURED_VALUE_READ_RESULT:
                                    {
                                        m_GetValue.strMeasuredValueReadResult = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MEASURED_VALUE_ONLY_READ:
                                    {
                                        m_GetValue.strMeasuredValueOnlyRead = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.PRODUCT_CODE:
                                    {
                                        m_GetValue.strProductCode = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MODEL_NAME_1:
                                    {
                                        m_GetValue.strModel1 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MODEL_NAME_2:
                                    {
                                        m_GetValue.strModel2 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MODEL_NAME_3:
                                    {
                                        m_GetValue.strModel3 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MODEL_NAME_4:
                                    {
                                        m_GetValue.strModel4 = strReplyData;
                                    }
                                    break;
                                case DATA_NO_COMMAND.MAX:
                                    break;
                                default:
                                    break;
                            }
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

                            // 결과 전달
                            if (deleCommandCompleted != null)
                                deleCommandCompleted(m_arrDataNoCommand[m_nCurrentCmdIdx],
                                                     !m_arrbErrorCommand[m_nCurrentCmdIdx],
                                                     m_arrStrCommandSend[m_nCurrentCmdIdx],
                                                     m_arrStrCommandResult[m_nCurrentCmdIdx]);

                            //우리가 주는 값은 엠제로만 줄것이므로 나머지는 그냥 넘김
                            if (strReceived.Contains("M0") ||
                                strReceived.Contains("SR") ||
                                strReceived.Contains("SW"))
                            {
                                // 다음 명령어
                                m_nCurrentCmdIdx++;
                            }
                            
                        }
                        //우리가 주는 값은 엠제로만 줄것이므로 나머지는 그냥 넘김
                        if (strReceived.Contains("M0") ||
                            strReceived.Contains("SR") ||
                            strReceived.Contains("SW"))
                        {
                            m_Status = STATUS.SEND;
                        }
                    }

                    // Timeout
                    double dTimeOut = 10.0;
                    if (Debugger.IsAttached)
                        dTimeOut = 10;
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
                                deleCommandCompleted(m_arrDataNoCommand[m_nCurrentCmdIdx],
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
                send += "\r\n";

                return Send(send);
            }
            catch (Exception ex)
            {
                if (deleException != null)
                    deleException(ex.ToString());
                return false;
            }
        }
      
        public void AddMonitoring(DATA_NO_COMMAND cmd)
        {
            m_listDataCmdMonitor.Add(cmd);
        }
        int AddCommand(string strCommCmd, string strAmpNo = "00", DATA_NO_COMMAND dataCmd = DATA_NO_COMMAND.NONE)
        {
            if (m_nIndexAddCommand >= m_arrDataNoCommand.Length)
                m_nIndexAddCommand = 0;

            m_arrCommCommand[m_nIndexAddCommand] = strCommCmd;
            m_arrAmpNo[m_nIndexAddCommand] = strAmpNo;
            m_arrDataNoCommand[m_nIndexAddCommand] = dataCmd;
            m_arrStrCommandResult[m_nIndexAddCommand] = "";
            m_arrbErrorCommand[m_nIndexAddCommand] = false;
            m_arrbReceivedCommand[m_nIndexAddCommand] = false;
            m_arrbFlagCommand[m_nIndexAddCommand] = true;

            return m_nIndexAddCommand++;
        }

        #region 내부 함수
        void _SendReadCommandForamt(string strCommCmd, string strAmpNo, string strDataNo)
        {
            SendFormat("{0},{1},{2}", strCommCmd, strAmpNo, strDataNo);
        }
        void _MZeroCommandFormat()
        {
            SendFormat("M0");
        }
        void _SendWriteCommandForamt(string strCommCmd, string strAmpNo, string strDataNo, string strValue)
        {
            SendFormat("{0},{1},{2},{3}", strCommCmd, strAmpNo, strDataNo, strValue);
        }

        bool ParsingData(string strData, out string strReplyData)
        {
            strData = strData.Replace("\r\n", "");
            string[] arrData = strData.Split(',');
            m_strLastRcvMsg = strData;
            strReplyData = "";

            switch (arrData[0])
            {
                case "ER":
                    {
                        //arrData[0] ER
                        //arrData[1] Send : Comm COmmand
                        //arrData[2] ErrCode
                        strReplyData = arrData[2];
                        int nErrNo = 0;
                        int.TryParse(arrData[2], out nErrNo);
                        m_strErrorrDesc = Enum.GetName(typeof(ERROR_NO), nErrNo);
                    }
                    break;
                case "SW":
                    {
                        ////arrData[0] Send : Comm Command
                        ////arrData[1] AmpNo
                        ////arrData[2] Data
                        //if (arrData[1] == "00")
                        //{
                        //    strReplyData = m_arrStrCommandReplyData_Normal[(int)m_nCurrentCmdIdx] = arrData[2];
                        //}
                        //else
                        //{
                        //    strReplyData = m_arrStrCommandReplyData_Expansion[(int)m_nCurrentCmdIdx] = arrData[2];
                        //}
                    }
                    break;
                case "SR":
                    {
                        //arrData[0] Send : Comm Command
                        //arrData[1] AmpNo
                        //arrData[2] DataNo 
                        //arrData[3] Data
                        if (arrData[1] == "00")
                        {
                            strReplyData = m_arrStrCommandReplyData_Normal[(int)m_nCurrentCmdIdx] = arrData[3];
                        }
                        else
                        {
                            strReplyData = m_arrStrCommandReplyData_Expansion[(int)m_nCurrentCmdIdx] = arrData[3];
                        }
                    }
                    break;
                case "M0":
                    {
                        bool bCheck = true;
                        for (int nIdx = 1; nIdx < arrData.Length; nIdx++)
                        {
                            bCheck &= double.TryParse(arrData[nIdx], out m_arrMeasuredValue[nIdx-1]);

                        }
                        if(bCheck)
                        {
                            IsTrayInspReq = true;

                            if (bManualCheck == false)
                            {
                                double[] arrMeasuredValue = new double[MAX_TOOL];
                                m_arrMeasuredValue.CopyTo(arrMeasuredValue, 0);

                                ListMeasure.Add(arrMeasuredValue);
                                System.Diagnostics.Debug.WriteLine(string.Format("체크 : {0}", arrMeasuredValue[0]));

                            }
                        }
                        else
                        {
                            IsTrayInspReq = false;
                        }
                    }
                    break;
                default:
                    {
                        strReplyData = "";
                        return false;
                    }
            }
            return true;
        }

        #endregion

        #region 외부 함수

        #region SR : READ
        public int GetZeroOffsetReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_REQ);
        }
        public int GetZeroOffsetClearReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_REQ);
        }
        public int GetResetReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.RESET_REQ);
        }
        public int GetSensorAmpErrorState(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SENSOR_AMP_ERROR_STATE);
        }
        public int GetSensorOutputState(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.OUTPUT_STATE);
        }

        public int GetProgramNo(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.CURRENT_PROGRAM_NO);
        }

        public int GetZeroOffsetResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_RESULT);
        }
        public int GetResetResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.RESET_RESULT);
        }

        public int GetZeroOffsetClearResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_RESULT);
        }

        public int GetFTPFileNameWriteReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ);
        }
        public int GetFTPFileNameWriteResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ);
        }
        public int GetFTPFileNameReadReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ);
        }
        public int GetFTPFileNameReadResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ);
        }
        public int GetProgramChangeReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.PROGRAM_CHANGE_REQ);
        }
        public int GetProgramChangeResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.PROGRAM_CHANGE_RESULT);
        }
        public int GetSettingValueReadReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_READ_REQ);
        }
        public int GetSettingValueReadResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_READ_RESULT);
        }
        public int GetSettingValueOnlyRead(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_ONLY_READ);
        }
        public int GetSettingValueOnlyWrite(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_ONLY_WRITE);
        }
        public int GetSettingValueWriteReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_WRITE_REQ);
        }
        public int GetSettingValueWriteResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_WRITE_RESULT);
        }
        public int GetMeasuredValueReadReq(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.MEASURED_VALUE_READ_REQ);
        }
        public int GetMeasuredValueReadResult(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.MEASURED_VALUE_READ_RESULT);
        }
        public int GetMeasuredValueOnlyRead(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.MEASURED_VALUE_ONLY_READ);
        }
        public int GetProductCode(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.PRODUCT_CODE);
        }
        public int GetModelName1(string strAmpNo = "00")
        {
            return AddCommand("SR", strAmpNo, KYC_IX.DATA_NO_COMMAND.MODEL_NAME_1);
        }
        #endregion

        #region SW : WRITE
        public int SetZeroOffsetReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strZeroOffsetReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_REQ);
        }
        public int SetZeroOffsetClearReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strZeroOffsetClearReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.ZERO_OFFSET_REQ);
        }
        public int SetResetReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strResetReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.RESET_REQ);
        }
        public int SetFTPFileNameWriteReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strFTPFileNameWriteReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ);
        }
        public int SetFTPFileNameReadReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strFTPFileNameReadReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.FTP_FILE_NAME_READ_REQ);
        }
        public int SetTriggerOrTimingInput(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strTriggerOrTimingInput = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.TRIGGER_OR_TIMING_SETTING);
        }
        public int SetEmissionStopInput(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strEmissionStopInput = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.EMISSION_STOP_INPUT);
        }
        public int SetProgramChangeReq(string strSetValue, string strAmpNo = "00")
        {
            if (strSetValue.Length == 1)
            {
                strSetValue = "0" + strSetValue;
            }
            m_SetValue.strProgramChangeReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.PROGRAM_CHANGE_REQ);
        }
        public int SetSettingValueReadReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strSettingValueReadReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_READ_REQ);
        }
        public int SetSettingValueOnlyWrite(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strSettingValueOnlyWrite = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_ONLY_WRITE);
        }
        public int SetSettingValueWriteReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strSettingValueWriteReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.SETTING_VALUE_WRITE_REQ);
        }
        public int SetMeasuredValueReadReq(string strSetValue, string strAmpNo = "00")
        {
            m_SetValue.strMeasuredValueReadReq = strSetValue;

            return AddCommand("SW", strAmpNo, KYC_IX.DATA_NO_COMMAND.MEASURED_VALUE_READ_REQ);
        }
        #endregion

        public int SendReadCommandFormat(string strAmpNo, DATA_NO_COMMAND dataCmd)
        {
            return AddCommand("SR", strAmpNo, dataCmd);
        }
        public int MZeroCommandFormat(bool bManual = false)
        {
            IsTrayInspReq = false;
            bManualCheck = bManual;

            return AddCommand("M0");
        }
        public int SendWriteCommandFormat(string strAmpNo, DATA_NO_COMMAND dataCmd, string strSetValue)
        {
            switch (dataCmd)
            {
                case DATA_NO_COMMAND.NONE:
                    break;
                case DATA_NO_COMMAND.ZERO_OFFSET_REQ:
                    m_SetValue.strZeroOffsetReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_REQ:
                    m_SetValue.strZeroOffsetClearReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.RESET_REQ:
                    m_SetValue.strResetReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.SENSOR_AMP_ERROR_STATE:
                    break;
                case DATA_NO_COMMAND.OUTPUT_STATE:
                    break;
                case DATA_NO_COMMAND.CURRENT_PROGRAM_NO:
                    break;
                case DATA_NO_COMMAND.ZERO_OFFSET_RESULT:
                    break;
                case DATA_NO_COMMAND.RESET_RESULT:
                    break;
                case DATA_NO_COMMAND.ZERO_OFFSET_CLEAR_RESULT:
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_REQ:
                    m_SetValue.strFTPFileNameWriteReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_WRITE_RESULT:
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_READ_REQ:
                    m_SetValue.strFTPFileNameReadReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_READ_RESULT:
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_1:
                    m_SetValue.strFTPFileName1 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_2:
                    m_SetValue.strFTPFileName2 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_3:
                    m_SetValue.strFTPFileName3 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_4:
                    m_SetValue.strFTPFileName4 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_5:
                    m_SetValue.strFTPFileName5 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_6:
                    m_SetValue.strFTPFileName6 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_7:
                    m_SetValue.strFTPFileName7 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_8:
                    m_SetValue.strFTPFileName8 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_9:
                    m_SetValue.strFTPFileName9 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_10:
                    m_SetValue.strFTPFileName10 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_11:
                    m_SetValue.strFTPFileName11 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_12:
                    m_SetValue.strFTPFileName12 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_13:
                    m_SetValue.strFTPFileName13 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_14:
                    m_SetValue.strFTPFileName14 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_15:
                    m_SetValue.strFTPFileName15 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_16:
                    m_SetValue.strFTPFileName16 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_17:
                    m_SetValue.strFTPFileName17 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_18:
                    m_SetValue.strFTPFileName18 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_19:
                    m_SetValue.strFTPFileName19 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_20:
                    m_SetValue.strFTPFileName20 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_21:
                    m_SetValue.strFTPFileName21 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_22:
                    m_SetValue.strFTPFileName22 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_23:
                    m_SetValue.strFTPFileName23 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_24:
                    m_SetValue.strFTPFileName24 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_25:
                    m_SetValue.strFTPFileName25 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_26:
                    m_SetValue.strFTPFileName26 = strSetValue;
                    break;
                case DATA_NO_COMMAND.FTP_FILE_NAME_27:
                    m_SetValue.strFTPFileName27 = strSetValue;
                    break;
                case DATA_NO_COMMAND.TRIGGER_OR_TIMING_SETTING:
                    m_SetValue.strTriggerOrTimingInput = strSetValue;
                    break;
                case DATA_NO_COMMAND.EMISSION_STOP_INPUT:
                    m_SetValue.strEmissionStopInput = strSetValue;
                    break;
                case DATA_NO_COMMAND.PROGRAM_CHANGE_REQ:
                    m_SetValue.strProgramChangeReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.PROGRAM_CHANGE_RESULT:
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_READ_REQ:
                    m_SetValue.strSettingValueReadReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_READ_RESULT:
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_ONLY_READ:
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_ONLY_WRITE:
                    m_SetValue.strSettingValueOnlyWrite = strSetValue;
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_WRITE_REQ:
                    m_SetValue.strSettingValueWriteReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.SETTING_VALUE_WRITE_RESULT:
                    break;
                case DATA_NO_COMMAND.MEASURED_VALUE_READ_REQ:
                    m_SetValue.strMeasuredValueReadReq = strSetValue;
                    break;
                case DATA_NO_COMMAND.MEASURED_VALUE_READ_RESULT:
                    break;
                case DATA_NO_COMMAND.MEASURED_VALUE_ONLY_READ:
                    break;
                case DATA_NO_COMMAND.PRODUCT_CODE:
                    break;
                case DATA_NO_COMMAND.MODEL_NAME_1:
                    break;
                case DATA_NO_COMMAND.MODEL_NAME_2:
                    break;
                case DATA_NO_COMMAND.MODEL_NAME_3:
                    break;
                case DATA_NO_COMMAND.MODEL_NAME_4:
                    break;
                case DATA_NO_COMMAND.MAX:
                    break;
                default:
                    break;
            }

            return AddCommand("SW", strAmpNo, dataCmd);
        }
        #endregion

        public string ChangeBinary(string strNumValue)
        {
            string strBinary = "";
            int nNum = 0;
            int.TryParse(strNumValue, out nNum);

            strBinary = Convert.ToString(nNum, 2);

            return strBinary;
        }
    }
}
