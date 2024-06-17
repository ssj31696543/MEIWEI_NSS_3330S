using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using NSS_3330S.FORM;
using DionesTool.DEVICE.READER;
using DionesTool.DATA;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.IO;

namespace NSS_3330S
{
    public static class GbVar
    {
        private static EasyIO easyIO = new EasyIO();
        public static EasyIO IO
        {
            get { return easyIO; }
        }

        public static string SWVersrion = "Copyright ver. 1.3.0.35";
        public static int[] GB_INPUT = new int[IODF.IN_TOTAL_CNT];
        public static int[] GB_OUTPUT = new int[IODF.OUT_TOTAL_CNT];
        public static double[] GB_AINPUT = new double[(int)IODF.A_INPUT.MAX];
        public static int[] GB_AOUTPUT = new int[(int)IODF.A_OUTPUT.MAX];
        public static bool[] GB_CHIP_PK_VAC = new bool[20];
        public static bool[] GB_CHIP_PK_BLOW = new bool[20];

        public static Queue<string[]> qMsg = new Queue<string[]>();

        public static bool gbProgramExit = false;
        public static bool gbPopMCState = false;

        public static int g_nPanelInCount = 0;
        public static int g_nPanelOutCount = 0;

        public static int[] MarkVisionContiCount = new int[100];
        public static int[] BallVisionContiCount = new int[100];

        // mc state db table update
        public static bool MCEMOSTATE = false;
        public static bool MCDOORSTATE = false;

        public static string[] strWarnMsg = new string[100];

        public static bool g_bPauseTrigSkip = false;
        public static PadSkipParam padSkipParam = new PadSkipParam();
        public static HostLotInfo PrevHostLot = new HostLotInfo();//메거진 배출 시 여기에 Lot정보 넣기()
        public static ProductLotInfo PrevProdLot = new ProductLotInfo();//메거진 배출 시 여기에 Lot정보 넣기()
        public static EqpProcInfo PrevProcLot = new EqpProcInfo();//메거진 배출 시 여기에 Lot정보 넣기()
        public static BindingList<HostLotInfo> lstBinding_HostLot = new BindingList<HostLotInfo>();
        public static BindingList<ProductLotInfo> lstBinding_ProdLot = new BindingList<ProductLotInfo>();
        public static BindingList<EqpProcInfo> lstBinding_EqpProc = new BindingList<EqpProcInfo>();
        public static List<ErrorInfo> errorInfos = new List<ErrorInfo>();
        public static List<LoginInfo> loginInfos = new List<LoginInfo>();
        public static List<UserColorInfo> UserColorInfos = new List<UserColorInfo>();
        public static int[] PrevBallResultQty = new int[(int)LotBga.ColName.MAX];
        public static int[] PrevMapResultQty = new int[(int)LotMap.ColName.MAX];
        public static int[] BallResultQty = new int[(int)LotBga.ColName.MAX];
        public static int[] MapResultQty = new int[(int)LotMap.ColName.MAX];

        public static int[] PrevBinResultQty = new int[(int)LotBin.ColName.MAX];
        public static int[] BinResultQty = new int[(int)LotBin.ColName.MAX];
        public static int LOT_BALL_NG_QTY = 0;
        public static int LOT_MAP_NG_QTY = 0;

        public static LoginInfo CurLoginInfo = new LoginInfo();
        public static UserColorInfo CurUserColorInfo = new UserColorInfo();
        public static TrayCountInfo CurTrayCountInfo = new TrayCountInfo(); // 20220620 pending item list Loader부분 2번 UI리셋 버튼 추가 _KTH

        public static ProductInfo product = new ProductInfo();
        public static McStage mcState = new McStage();
        public static SeqShared Seq = new SeqShared();

        public static bool bAutoRecipeChange = false;

        public static bool bFirstAutoStart = true;//220609

        public static bool bChangeRecipe_Main;
        public static bool bChangeRecipe_Recipe;
        public static int nLoggingUserLevel;
        public static string strLoggingUerID;

        public static int nActiveScreen = 0;

        public static bool bPlcReadWaitCmd = false;
        public static bool bPlcConnected = false;

        public static bool bIsStopAlignTeachPos = false;
        public static bool bIsStop2DTeachPos = false;
        public static bool bIsStopBrushTeachPos = false;
        public static bool bIsStopTopVisionTeachPos = false;
        public static bool bIsStopBtmVisionTeachPos = false;
        public static bool bIsStopTrayVisionTeachPos = false;

        public static bool bIsDryRunCutTableUnitExist1 = false;
        public static bool bIsDryRunCutTableUnitExist2 = false;
        public static bool bIsDryRunCutTableUnit1 = false;
        //Alarm//
        public static string sDate = System.DateTime.Now.ToString("yyyy-MM-dd");
        public static int pageNum = 0;
        public static int seqBtn = 0;

        public static double[] gdbIFArray = new double[100];
        public static int[] gdbIFArray_MCState = new int[100];

        public static int[] arrMgzOhtStep = new int[2];
        public static int[] arrTrayOhtStepTL = new int[5];
        public static int[] arrTrayOhtStepTU = new int[5];

        public static int m_gnCurrentRecipeNo = 0;
        public static string m_gstrCurrentRecipeName = "";
        public static string m_gstrCurrentPPID = "";

        public static bool g_bUpdateProdData = false;
        public static bool g_bWorkProdData = false;
        public static string gbVersion = "1.0.0.0";

        public static Stopwatch[] g_timeStampInit = new Stopwatch[MCDF.MACHINE_MAX];

        //popMessage
        public static bool bMessage = false;

        //public static DBExLogData[] dbLogData = new DBExLogData[(int)DBDF.LOG_TYPE.MAX];
        public static DBExLogData2[] dbLogData = new DBExLogData2[(int)DBDF.LOG_TYPE.MAX];
        //public static DBExEventData dbEventData = null;

        public static DBExTactTime dbTactTime = null;

        public static DBExSeqInfo dbSeqInfo = null;

        public static DBExLotLog dbLotLog = null;
        public static DBExMgzInfo dbMgzInfo = null;
        public static DBExStripInfo[] dbStripInfo = new DBExStripInfo[(int)STRIP_MDL.MAX];
        public static DBExStripLog dbStripLog = new DBExStripLog();
        public static DBExLotInfo dbLotInfo = null;
        public static DBExProcQtyInfo dbProcsQtyInfo = null; //220611 pjh
        public static DBExProcQtyLog dbProcsQtyLog = null;

        public static DBExITSInfo dbITSInfo = null;

        // SAW SVID 수신 데이터
        public static ConcurrentDictionary<string, string> g_dictSawSVID = new ConcurrentDictionary<string, string>();

        // SORTER SVID 수신 데이터
        public static ConcurrentDictionary<int, string> g_dictSorterSVID = new ConcurrentDictionary<int, string>();

        public const string STAGE_SPEED_CH1 = "30131";
        public const string SPINDLE_RPM_1CH_1 = "30136";
        public const string SPINDLE_RPM_1CH_2 = "30137";

        public const string STAGE_SPEED_CH2 = "30156";
        public const string SPINDLE_RPM_2CH_1 = "30161";
        public const string SPINDLE_RPM_2CH_2 = "30162";
        #region 대덕전자 NEW DB
        // Vision에 MapBlock, TrainName, Barcode, LOT ID, Unit Size 등을 전달하는 DB
        public static DBExToVision dbSetVision = new DBExToVision();

        // Vision에 Pre Align 검사 모드를 전달하는 DB (기존 DBExPreMain 클래스)
        public static DBEXToVisionPreAlign dbSetPreAlign = new DBEXToVisionPreAlign();

        // Dicing에 Pre Align 결과 정보를 가져오는 DB (기존 DBExPreVision 클래스)
        public static DBExToHandlerPreAlign dbGetPreAlign = new DBExToHandlerPreAlign();

        // Sorter의 Picker Calibration 결과 정보를 가져오는 DB
        public static DBExToHandlerPickerCal dbGetPickerCal = new DBExToHandlerPickerCal();

        // Sorter 상부 Align 결과 정보를 가져오는 DB
        public static DBExToHandlerTopAlign dbGetTopAlign = new DBExToHandlerTopAlign();

        // Sorter 상부 Inspection 결과 정보를 가져오는 DB (기존 DBExMapVision 클래스)
        public static DBExToHandlerTopResult[] dbGetTopInspection = new DBExToHandlerTopResult[(int)MCDF.eUNIT.MAX];

        // Sorter 하부 Inspection 결과 정보를 가져오는 DB (기존 DBExBallVision 클래스)
        public static DBExToHandlerBtmResult[] dbGetBtmInspection = new DBExToHandlerBtmResult[(int)MCDF.eUNIT.MAX];

        // ALARM LOG
        public static DBExAlarmLog2 dbAlarmLog = null;

        // STRIP LOG
        //public static DBExStripLog2 dbStripLog2 = null;

        // EVENT LOG
        public static DBExEventLog2 dbEventLog2 = null;

        // ITS LOG
        public static DBExITSLog2 dbITSLog2 = null;

        // PROC QTY LOG
        public static DBExProcQtyLog2 dbProcQtyLog2 = null;
        #endregion

        public static DBExFromSorter dbFromSorter;
        public static DBExFromSaw dbFromSaw;
        public static SeqProperties[] SeqInfo = new SeqProperties[(int)SEQ_ID.MAX];

        public static SeqManager SeqMgr = new SeqManager();
        public static StripManager StripMgr = new StripManager();
        public static LotManager LotMgr = new LotManager();

        public static bool[] g_bUpdateLogData = new bool[(int)DBDF.LOG_TYPE.MAX];
        public static bool g_bUpdateAlarmData = false;
        public static bool g_bUpdateEventData = false;
        public static bool g_bUpdateTactTime = false;
        public static bool g_bUpdateMCC = false;

        public static string BLADE_ID1 = "";
        public static string BLADE_ID2 = "";

        //public static SawWorkingData WorkingData = new SawWorkingData();
        public static int[] WorkingData = new int[(int)MESDF.eWORKING_DATA.MAX];

        //EES관련
        //public static CommManager EESManager = new CommManager();
        public static EES_INFO EES_info_New = new EES_INFO();
        public static EES_INFO EES_info_Pre = new EES_INFO();

        public static EES_FDC_PARAM EES_FDC_Para = new EES_FDC_PARAM();

        public static LabelPrint TU01Label = new LabelPrint();
        public static LabelPrint TU02Label = new LabelPrint();
        public static LabelPrint SCRAPLabel = new LabelPrint();
		
		public static bool isInfiniteHome = false;

        //220511 pjh
        //트레이 Out시 이미 센서가 감지 되어있는 경우 해당  true
        public static bool[] IsAlreadyDetectOutSen = new bool[MCDF.ELV_MAX_CNT];
        public static bool[] IsReqTraySupply = new bool[MCDF.ELV_MAX_CNT];

        //220528 pjh
        //트레이 없을 시 최조 한번만 알람 발생시키기 위함
        public static bool bOccuredTrayReqWarning = false;

        // [2022.07.03.kmlee] Door Alarm 발생시키는 변수 (Start나 Initial 시 사용)
        public static int g_nDoorAlarmResult = FNC.SUCCESS;

        #region LOT 관련 // 0503 KTH
        public static string LOT_ID;
        public static string ITS_ID;
        public static int LOT_Kind; //LOT종류     0 :초도 , 1 : 본낫 , 2: 더미 , 3: 재초도 , 4: 재작업
        public static int STRIP_CNT;//STRIP 수량
        #endregion
        #region VISION 통신
        // UDP 통신 메시지 로그
        public static ConcurrentQueue<MessageLog> g_queueMsgLogVisionUdp = new ConcurrentQueue<MessageLog>();

        // VISION SVID 수신 데이터
        public static ConcurrentDictionary<string, string> g_dictVisionSVID = new ConcurrentDictionary<string, string>();

        // JOG 이동 데이터
        public static ConcurrentQueue<VisionMoveCmd> g_queueVisionMoveCmd = new ConcurrentQueue<VisionMoveCmd>();

        // JOG 이동 실패 응답
        public static bool g_bResponseJogError = false;
        #endregion

        public static Stopwatch g_swUPH = new Stopwatch();
        public static double g_dUPH = 0.0;

        public static bool isInTray = false;

        public static bool[] bUnitPauseFlag = new bool[20];

        public static int g_nFirstUltraWaterChangeCount = 0;
        public static int g_nSecondUltraWaterChangeCount = 1;

        // 배큠 알람 시 스크랩이 필요한 영역을 따로 표시하기 위해
        public static int[] g_nScrapNeed = new int[(int)STRIP_MDL.MAX];

        public static bool bLotEndComp = true;
        public static bool bLotInsert = false;

        public static bool bDryBlockAlways = false;

        public static bool bLightCurtainDetect = false;

        //2022 10 28 HEP
        // 랏입력후 시작시 소재가 있으면 안됨
        public static bool bFirstLotCheck = false;

        public static StripTTLogThread StripTTLog; 
        public static LotReportLogThread LotReportLog;

        public static bool[] bTableDistanceWait = new bool[2]; // 20230613 choh: 상대 트레이 이동거리 확보 대기중인지

        public static int FIRST_ULTRASONIC_WATER_CHANGE_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(PathMgr.Inst.PATH_PROGRAM_INFO_NAME);
                //ini.SetIniValue("ULTRASONIC_1", "ULTRA_WATER_CHANGE_COUNT", value.ToString());
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(PathMgr.Inst.PATH_PROGRAM_INFO_NAME);
                int nRet = 0;
                int.TryParse(ini.GetIniValue("ULTRASONIC_1", "ULTRA_WATER_CHANGE_COUNT", "0"), out nRet);
                return nRet;
            }
        }
        public static int SECOND_ULTRASONIC_WATER_CHANGE_COUNT
        {
            set
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(PathMgr.Inst.PATH_PROGRAM_INFO_NAME);
                //ini.SetIniValue("ULTRASONIC_2", "ULTRA_WATER_CHANGE_COUNT", value.ToString());
            }
            get
            {
                DionesTool.FileLib.iniUtil ini = new DionesTool.FileLib.iniUtil(PathMgr.Inst.PATH_PROGRAM_INFO_NAME);
                int nRet = 0;
                int.TryParse(ini.GetIniValue("ULTRASONIC_2", "ULTRA_WATER_CHANGE_COUNT", "0"), out nRet);
                return nRet;
            }
        }
        //frmMain 좌측 사이드바 화면에서 매뉴얼로 확인창 스킵하는 기능
        public static bool MANUAL_MOVE_CHECK_SKIP { get; set; }

        #region 일시정지
        public static bool INSERT_STRIP_PAUSE { get; set; }
        public static bool SAW_PLACE_PAUSE { get; set; }
        public static bool SAW_PICK_UP_PAUSE { get; set; }
        public static bool DRY_BLOCK_PLACE_PAUSE { get; set; }
        public static bool MAP_PICKER_PICK_UP_PAUSE { get; set; }
        public static bool MAP_PICKER_PLACE_PAUSE { get; set; }
        public static bool CHIP_PICKER_PICK_UP_PAUSE { get; set; }
        public static bool CHIP_PICKER_PLACE_PAUSE { get; set; }
        public static bool TRAY_PICKER_PICK_UP_PAUSE { get; set; }
        public static bool TRAY_PICKER_PLACE_PAUSE { get; set; }
        public static bool TOP_INSP_PAUSE { get; set; }
        public static bool BTM_INSP_PASE { get; set; }

        public static bool LOADER_TEST { get; set; }
        #endregion

        static GbVar()
        {
            mcState.isHomeComplete = new bool[SVDF.SV_CNT];
            mcState.dStopPos = new double[SVDF.SV_CNT];
            mcState.nSeqStep = new int[50];
            mcState.strStepName = new string[50];
			//220611 pjh 삭제 
        }

        public static string[] strInputWaitMsg = new string[3];
        public static bool[] bInputWaitMsg = new bool[3];


        public static int[] nLampStatus = new int[6]; // 0: OFF, 1: ON, 2: BLINK
        public static int[] nSwitchStatus = new int[3]; // 0: OFF, 1: ON, 2: BLINK


        public static bool bMgzConfirm = false;
        public static bool bTrayConfirm = false;
        public static bool bVisionConfirm = false;
        public static bool bCleanConfirm = false;


        public static bool bPopHostRecipeUpload = false;
        public static bool bPopHostRecipeSelected = false;
        public static bool bPopHostRecipeDelete = false;

        public static bool[] bTrayInReq = new bool[5] { false, false, false, false, false };
        public static bool[] bTrayOutReq = new bool[5] { false, false, false, false, false };
    }
    

    public static class GbForm
    {
        public static frmMain fMain = null;
        public static frmLogging fLogin = null;
        public static frmAuto fAuto = null;
        public static frmManual fManual = null;
        public static frmLog fLog = null;
        public static frmRecipe fRcp = null;
        public static frmConfig fCfg = null;
        public static frmAlarm fAlarm = null;
        public static frmDio fDIO = null;
        public static frmTeaching fTeach = null;

        //public static popJog pJog = null;
        public static popNeonJog pNeonJog = null;
        public static popMgrRecipe pRcp = new popMgrRecipe();
        //public static popRcpPositionTable pRpt = new popRcpPositionTable();
        //public static popCycleInspection pInspCyc = new popCycleInspection();
        //public static popCycleInspection pInspCyc = null;
        public static popMCState pMcStatus = new popMCState();
        public static popHostMsg pHostMsg = new popHostMsg();

        public static popMgzManualCycleRun pCycMgz = null;
        public static popStripTransferManualCycleRun pCycStrip = null;
        public static popUnitTransferManualCycleRun pCycUnit = null;
        public static popClrNDryManualCycleRun pCycClean = null;
        public static popMapTransferManualCycleRun pCycMapTransfer = null;
        public static popPickNPlaceManualCycleRun pCycPnP = null;
        public static popTrayTransferManualCycleRun pCycTrayTransfer = null;

        public static popTest pTest = null;

        public static void Close()
        {
            #region FORM CLOSE
            if (fLogin != null && !fLogin.IsDisposed)
                fLogin.Close();

            if (fAuto != null && !fAuto.IsDisposed)
                fAuto.Close();

            if (fManual != null && !fManual.IsDisposed)
                fManual.Close();

            if (fLog != null && !fLog.IsDisposed)
                fLog.Close();

            if (fDIO != null && !fDIO.IsDisposed)
                fDIO.Close();

            //if (fFnc != null && !fFnc.IsDisposed)
            //    fFnc.Close();

            if (fRcp != null && !fRcp.IsDisposed)
                fRcp.Close();

            if (fCfg != null && !fCfg.IsDisposed)
                fCfg.Close();

            if (fAlarm != null && !fAlarm.IsDisposed)
                fAlarm.Close();

            #endregion
        }
    }

    public static class GbDev
    {
        public const int ACPM_MAX_COUNT = 4;
        const int ULTRA_MAX_COUNT = 4;
        //DEVICE
        //public static IMPIX_RFID LD_RFID = null;
        //public static CEYON_RFID TRAY_RFID = null;
        public static DataManReader BarcodeReader = null;
        public static cKeyenceSR100 MGZBarcodeReader = new cKeyenceSR100();

        #region 수세기
        //public static DSH_20_B7_HT thermostat = new DSH_20_B7_HT(); //온조기
        //public static TempController tempController = new TempController(); //열풍기 온도 측정
        //public static UltraSonicComm[] ultrasonic = new UltraSonicComm[ULTRA_MAX_COUNT]; // 초음파 설정
        //public static UltraSonicComm currentUltra = new UltraSonicComm();
        #endregion
        public static KYC_IX trayVision = new KYC_IX();
        public static int CONV_5F = 1;
        public static int CONV_4F = 6;
        public static int MGZ_ULD_ELV = 2;
        public static int MGZ_LD_ELV = 3;

        static GbDev()
        {

        }

        /// <summary>
        /// 통신 확인후 재연결
        /// </summary>

        static void ConnectedBarcode_CK()
        {
#if !_CONNECTCHECK
            popMessageBox msg;
            if (BarcodeReader.IsConnect())
            {

                msg = new popMessageBox("정상적인 통신 상태입니다.", "정상");
                msg.ShowDialog();
            }
            else
            {
                msg = new popMessageBox("연결이 불안정한 상태입니다 다시 연결하겠습니까?", "비정상");
                if (msg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    BarcodeReader.Close();
                    RemoteInfo remoteInfo = new RemoteInfo();
                    if (!File.Exists(PathMgr.Inst.PATH_BARCODEREADER))
                    {
                        remoteInfo.remotePath = "169.254.60.223";
                        remoteInfo.port = 23;

                        remoteInfo.Serialize(PathMgr.Inst.PATH_BARCODEREADER);
                    }
                    else
                    {
                        remoteInfo = RemoteInfo.Deserialize(PathMgr.Inst.PATH_BARCODEREADER);
                    }
                    BarcodeReader.Connect_Tcp(remoteInfo.remotePath, remoteInfo.port);
                }
            }
#endif
        }

    }

	public class EasyIO
    {
        public int this[IODF.OUTPUT index]
        {
            get
            {
                if (index == IODF.OUTPUT.NONE || index == IODF.OUTPUT.MAX) return 0;
                return GbVar.GB_OUTPUT[(int)index];
            }
        }

        public int this[IODF.INPUT index]
        {
            get
            {
                if (index == IODF.INPUT.NONE || index == IODF.INPUT.MAX) return 0;
                return GbVar.GB_INPUT[(int)index];
            }
        }
    }


    /// <summary>
    /// 220621
    /// </summary>
    public struct PadSkipParam
    {
        private bool bFDir;
        private int nMapTableNo;
        private int nHeadNo;

        public PadSkipParam(bool bFDir, int nMapTableNo, int nHeadNo)
        {
            this.bFDir = bFDir;
            this.nMapTableNo = nMapTableNo;
            this.nHeadNo = nHeadNo;
        }

        public bool FDir
        {
            get { return bFDir; }
        }
        public int MapTableNo
        {
            get { return nMapTableNo; } 
        }
        public int HeadNo
        {
            get { return nHeadNo; }
        }
    }

    [Serializable]
    public class CIM_RECIPE
    {
        //
    }



    [Serializable]

    public class CIM_SVID
    {
        //
    }

}
