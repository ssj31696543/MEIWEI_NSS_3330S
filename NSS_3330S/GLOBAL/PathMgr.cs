using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static NSS_3330S.PathMgr;

namespace NSS_3330S
{
    class PathMgr
    {
        private static PathMgr instance = null;
        private static readonly object padlock = new object();

        // 폴더 이름
        private string[] dirRefName = new string[] { "Setting", "WorkFile", "Config", "Teaching","Error", "Report", "Version", "DeviceMgr", "TTLog", "ITS", "LotReport" };

        public string[] fDevCatagory = new string[] { "MGZ_INFO", "MAP_INFO", "TRAY_INFO" };


        // 파일 이름
        private const string filenameSettingAjin = "motor.mot";
        private const string filenameSettingMotionData = "MotionData.xml";

        private const string filenameSettingDio = "Dio.ini";
        private const string filenameSettingAio = "Aio.ini";

        private const string filenameCfgFile = "Config.xml";
        private const string filenameFuncFile = "Function.xml";
        private const string filenameTchFile = "Teach.xml";

        private const string filenameProgramInfo = "PgmInfo.ini";
        private const string filenameFuncSetting = "McFunc.xml";

        private const string filenameErrDefined = "ErrorDef.ini";

        private const string filenameReport = "Report.accdb";
        private const string filenameBarCodeReader = "BarCodeReader.xml";
        private const string filenameMGZBarCodeReader = "MGZBarCodeReader.xml";
        private const string filenameLdRFIDReader = "LD_RFID_Reader.xml";
        private const string filenameTrayRFIDReader = "TRAY_RFID_Reader.xml";
        private const string filenameIonizerMonitoring1 = "IonizerMonitor1.xml";
        private const string filenameIonizerMonitoring2 = "IonizerMonitor2.xml";
        private const string filenameIonizerMonitoring3 = "IonizerMonitor3.xml";
        private const string filenameIonizerMonitoring4 = "IonizerMonitor4.xml";
        private const string filenameUltrasonci1 = "Ultrasonic1.xml";
        private const string filenameUltrasonci2 = "Ultrasonic2.xml";
        private const string filenameUltrasonci3 = "Ultrasonic3.xml";
        private const string filenameUltrasonci4 = "Ultrasonic4.xml";
        private const string filenameThermost = "Thermost.xml";
        private const string filenameTempCtrl = "TempCtrl.xml";
        private const string filenameTrayVision = "TrayVision.xml";




        private const string filenameLoginInfo = "LoginInfo.xml";
        private const string filenameUserColorInfo = "UserColorInfo.xml";
        private const string filenameTrayCountInfo = "TrayCountInfo.xml";

        private const string filenameAppScriptEng = "AppScriptEng.xml";
        private const string filenameAppScriptCha = "AppScriptCha.xml";

        private const string filenameErrorInfo = "ErrorDefine.xml";
        private const string filenameErrorInfoEng = "ErrorDefineEng.xml";
        private const string filenameErrorInfoChn = "ErrorDefineChn.xml";

        public static string filenameAlarmHistory = "ErrorHistory.csv";
        private const string filenameExpiration = "EXPIRATION.txt";

        public string DBPath = "C:\\MCDB.db";

        public string folder_LogPath = "D:\\LOG\\";
        public string[] folder_LogCatagory = new string[] { "EVENT", "LOGGING", "SEQ", "ALIGNMENT", "INSPECTION" ,"ERROR", "PRODUCT", "EXCEPTION", "MES" };

        public string[] folder_SEQ = new string[] { "MGZ_TRANSFER", "MGZ_LOT_START", 
                                                    "STRIP_TRANSFER", "UNIT_TRANSFER", 
                                                    "UNIT_CLEAN_N_DRY","MAP_TRANSFER", "UNIT_PICK_N_PLACE", 
                                                    "TRAY_TRANSFER", 
                                                    "ULD_GOOD_1_ELV", "ULD_GOOD_2_ELV", "ULD_REWORK_ELV", "ULD_EMPTY_1_ELV", "ULD_EMPTY_2_ELV",
                                                    "TACT_TIME"};

        private string pathExeFile = string.Empty;
        private string[] pathRefName = new string[Enum.GetNames(typeof(ePathInfo)).Length];


        public string LogStripDefectCount = "";

        public enum ePathInfo
        {
            eSetting = 0,
            eRecipe,
            eConfig,
            eTeach,
            eError,
            eReport,
            eVersion,
            eDeviceMgr,
            eTTLog,
            eITSLog,
            eLotLog,
        }


        PathMgr()
        {
        }

        public static PathMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new PathMgr();

                            instance.Init();
                        }
                    }
                }
                return instance;
            }
        }

        public string this[ePathInfo index]
        {
            get
            {
                if (instance.pathRefName == null) return null;

                return instance.pathRefName[(int)index];
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        void Init()
        {
            //pathExeFile = "C:\\MACHINE";//System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            pathExeFile = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            for (int nPathCount = 0; nPathCount < Enum.GetNames(typeof(ePathInfo)).Length; nPathCount++)
            {
                pathRefName[nPathCount] = GetPath(dirRefName[nPathCount]);
            }
        }

        /// <summary>
        /// 현재 실행 위치에서 pathName을 더한 경로를 반환한다
        /// 경로에 폴더가 없다면 생성한다
        /// </summary>
        /// <param name="pathName"></param>
        /// <returns></returns>
        private string GetPath(string pathName)
        {
            string path = string.Format("{0}\\{1}\\", pathExeFile, pathName);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            return path;
        }

        public string PATH_RECIPE { get { return pathRefName[(int)ePathInfo.eRecipe]; } }

        public string PATH_PROGRAM_INFO_NAME
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameProgramInfo); }
        }

        public string PATH_FUNC_SETTING_NAME
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eConfig], filenameFuncSetting); }
        }

        public string PATH_ERR_DEFINED_NAME
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eError], filenameErrDefined); }
        }

        
        public string PATH_SETTING_MOTION_DATA
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameSettingMotionData); }
        }

        public string PATH_BARCODEREADER
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameBarCodeReader); }
        }

        public string PATH_MGZ_BARCODEREADER
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameMGZBarCodeReader); }
        }

        public string PATH_LD_RFID
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameLdRFIDReader); }
        }

        public string PATH_TRAY_RFID
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameTrayRFIDReader); }
        }
        public string PATH_THERMOSTAT
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameThermost); }
        }
        public string PATH_TEMPCONTROL
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameTempCtrl); }
        }
        public string PATH_TRAY_VISION
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameTrayVision); }
        }
        public string IonizerMonitor(int nDevice)
        {
            string strRet = "";

            switch (nDevice)
            {
                case 0:
                    strRet = PATH_IONIZER_MONITORING_1;
                    break;
                case 1:
                    strRet = PATH_IONIZER_MONITORING_2;
                    break;
                case 2:
                    strRet = PATH_IONIZER_MONITORING_3;
                    break;
                case 3:
                    strRet = PATH_IONIZER_MONITORING_4;
                    break;
                default:
                    strRet = PATH_IONIZER_MONITORING_1;
                    break;
            }

            return strRet;
        }
        public string UltrasonicPath(int nDevice)
        {
            string strRet = "";

            switch (nDevice)
            {
                case 0:
                    strRet = PATH_ULTRASONIC_1;
                    break;
                case 1:
                    strRet = PATH_ULTRASONIC_2;
                    break;
                case 2:
                    strRet = PATH_ULTRASONIC_3;
                    break;
                case 3:
                    strRet = PATH_ULTRASONIC_4;
                    break;
                default:
                    strRet = PATH_IONIZER_MONITORING_1;
                    break;
            }

            return strRet;
        }
        public string PATH_IONIZER_MONITORING_1
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameIonizerMonitoring1); }
        }
        public string PATH_IONIZER_MONITORING_2
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameIonizerMonitoring2); }
        }
        public string PATH_IONIZER_MONITORING_3
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameIonizerMonitoring3); }
        }
        public string PATH_IONIZER_MONITORING_4
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameIonizerMonitoring4); }
        }
        public string PATH_ULTRASONIC_1
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameUltrasonci1); }
        }
        public string PATH_ULTRASONIC_2
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameUltrasonci2); }
        }
        public string PATH_ULTRASONIC_3
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameUltrasonci3); }
        }
        public string PATH_ULTRASONIC_4
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameUltrasonci4); }
        }
        public string PATH_PRODUCT_USERINFO
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eRecipe], filenameLoginInfo); }
        }

        public string PATH_PRODUCT_USER_COLORINFO
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eRecipe], filenameUserColorInfo); }
        }
        public string PATH_PRODUCT_TRAY_COUNTINFO
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eRecipe], filenameTrayCountInfo); }
        }

        public string PATH_VERSION
        {
            get { return string.Format("{0}", pathRefName[(int)ePathInfo.eVersion]); }
        }

        public string PATH_LANGUAGE_MESSAGE
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], "Language_Message.csv"); }
        }
        public string FILE_PATH_LANGUAGE_UI
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], "Language_UI.csv"); }
        }

        public string PATH_DEV_MGR_MGZ
        {
            get { return string.Format("{0}{1}\\", pathRefName[(int)ePathInfo.eDeviceMgr], fDevCatagory[0]); }
        }

        public string PATH_DEV_MGR_MAP
        {
            get { return string.Format("{0}{1}\\", pathRefName[(int)ePathInfo.eDeviceMgr], fDevCatagory[1]); }
        }

        public string PATH_DEV_MGR_TRAY
        {
            get { return string.Format("{0}{1}\\", pathRefName[(int)ePathInfo.eDeviceMgr], fDevCatagory[2]); }
        }

        public string PATH_LOG
        {
            get { return folder_LogPath; }
        }

        //  "EVENT", "LOGGING", "SEQ", "ALIGNENT", "INSPECTION"

        public string PATH_LOG_EVENT
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[0]); }
        }

        public string PATH_LOG_LOGGING
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[1]); }
        }
        public string PATH_LOG_SEQ
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[2]); }
        }

        public string PATH_LOG_ALIGNMENT
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[3]); }
        }

        public string PATH_LOG_INSPECTION
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[4]); }
        }
        public string PATH_LOG_ERROR
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[5]); }
        }

        public string PATH_LOG_PRODUCT
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[6]); }
        }

        public string PATH_LOG_EXCEPTION
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[7]); }
        }

        public string PATH_LOG_MES
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[8]); }
        }


        public string FILE_ERROR_HISTORY_EXPIRATION
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[5], filenameExpiration); }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////
        public string PATH_LOG_SEQ_SUB_MGZ_TRANSFER
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[0]); }
        }

        public string PATH_LOG_SEQ_SUB_MGZ_LOT
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[1]); }
        }

        public string PATH_LOG_SEQ_SUB_STRIP_TRANSFER
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[2]); }
        }

        public string PATH_LOG_SEQ_SUB_UNIT_TRANSFER
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[3]); }
        }

        public string PATH_LOG_SEQ_SUB_UNIT_CLEANING
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[4]); }
        }

        public string PATH_LOG_SEQ_SUB_MAP_TRANSFER
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[5]); }
        }

        public string PATH_LOG_SEQ_SUB_UNIT_PP
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[6]); }
        }

        public string PATH_LOG_SEQ_SUB_TRAY_TRANSFER
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[7]); }
        }

        public string PATH_LOG_SEQ_SUB_ULD_GOOD_ELV
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[8]); }
        }

        public string PATH_LOG_SEQ_SUB_ULD_REWORK_ELV
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[9]); }
        }

        public string PATH_LOG_SEQ_SUB_ULD_EMPTY_1_ELV
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[10]); }
        }

        public string PATH_LOG_SEQ_SUB_ULD_EMPTY_2_ELV
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[11]); }
        }

        public string PATH_LOG_SEQ_SUB_ULD_COVER_ELV
        {
            get { return string.Format("{0}{1}\\{2}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[12]); }
        }

        public string PATH_LOG_TACT_TIME
        {
            get { return string.Format("{0}{1}", folder_LogPath, folder_LogCatagory[2], folder_SEQ[13]); }
        }

        public string FILE_PATH_DB_INFO
        {
            get
            {
                return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], "DbInfo.xml");

            }
        }
        
        public string FILE_PATH_CFG
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eConfig], filenameCfgFile); }
        }

        public string FILE_PATH_BACKUP_CFG
        {
            get { return string.Format("{0}{1}\\{2}", pathRefName[(int)ePathInfo.eConfig], "BACKUP", filenameCfgFile); }
        }

        public string FILE_PATH_TCH
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eTeach], filenameTchFile); }
        }

        public string FILE_PATH_BACKUP_TCH
        {
            get { return string.Format("{0}{1}\\{2}", pathRefName[(int)ePathInfo.eTeach], "BACKUP", filenameTchFile); }
        }

        public string FILE_PATH_FUNC
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameFuncFile); }
        }

        public string FILE_PATH_BACKUP_FUNC
        {
            get { return string.Format("{0}{1}\\{2}", pathRefName[(int)ePathInfo.eSetting], "BACKUP", filenameFuncFile); }
        }

        public string FILE_PATH_APP_SCRIPT_ENG
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameAppScriptEng); }

        }

        public string FILE_PATH_APP_SCRIPT_CHA
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameAppScriptCha); }

        }

        public string FILE_PATH_MOTOR_FILE
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameSettingAjin); }
        }

        public string FILE_PATH_ERROR
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eError], filenameErrorInfo); }
        }
        public string FILE_PATH_ERROR_ENG
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eError], filenameErrorInfoEng); }
        }
        public string FILE_PATH_ERROR_CHN
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eError], filenameErrorInfoChn); }
        }

        public string PATH_REPORT
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eReport], filenameReport); }
        }

        public string FILE_PATH_SETTING_DIO
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameSettingDio); }
        }

        public string FILE_PATH_SETTING_AIO
        {
            get { return string.Format("{0}{1}", pathRefName[(int)ePathInfo.eSetting], filenameSettingAio); }
        }


        public string FILE_ERROR_HISTORY_PATH
        {
            get { return string.Format("{0}", pathRefName[(int)ePathInfo.eError]); }
        }

        public string PATH_LOG_LOT_LOG
        {
            get { return string.Format("{0}{1}", folder_LogPath, dirRefName[(int)ePathInfo.eLotLog]); }
        }
        public string PATH_LOG_TT_LOG
        {
            get { return string.Format("{0}{1}", folder_LogPath, dirRefName[(int)ePathInfo.eTTLog]); }
        }
    }

}
