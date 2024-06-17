using NSS_3330S.FORM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using DionesTool.DATA;
using DionesTool.DEVICE.READER;
using SQLiteDB;
using NSS_3330S.MOTION;
using NSS_3330S.GLOBAL;

namespace NSS_3330S.POP
{
    public partial class popAppLoading : Form
    {
        Thread threadLoading = null;
        bool flagAliveThreadLoading = false;

        public delegate void DeleProgress(int nProgress, string msg);
        public event DeleProgress OnProgress = null;

        public delegate void DeleCreateForm(int nProgress, string msg, int nFormIdx);
        public event DeleCreateForm OnCreateForm = null;

        public delegate void DeleMessageBox(string msg);
        public event DeleMessageBox OnMessageBox = null;

        public delegate void DeleFinish();
        public event DeleFinish OnFinish = null;

        public popAppLoading()
        {
            InitializeComponent();
        }

        private void popAppLoading_Load(object sender, EventArgs e)
        {
            OnProgress += popStartUp_OnProgress;
            OnCreateForm += popStartUp_OnCreateForm;
            OnMessageBox += popStartUp_OnMessageBox;
            OnFinish += popStartUp_OnFinish;

            StartStopThreadLoading(true);
            this.CenterToScreen();
        }

        private void popAppLoading_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnProgress -= popStartUp_OnProgress;
            OnCreateForm -= popStartUp_OnCreateForm;
            OnMessageBox -= popStartUp_OnMessageBox;
            OnFinish -= popStartUp_OnFinish;

            StartStopThreadLoading(false);
        }

        void popStartUp_OnFinish()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    popStartUp_OnFinish();
                });
                return;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        void popStartUp_OnMessageBox(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    popStartUp_OnMessageBox(msg);
                });
                return;
            }

#if !_NOTEBOOK
            MessageBox.Show(new Form() { TopMost = true }, msg);
#endif
        }

        void popStartUp_OnCreateForm(int nProgress, string msg, int nFormIdx)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    popStartUp_OnCreateForm(nProgress, msg, nFormIdx);
                });
                return;
            }

            switch (nFormIdx)
            {
                case 0:
                    GbForm.fLogin = new frmLogging();
                    break;
                case 1:
                    GbForm.fAuto = new frmAuto();
                    break;
                case 2:
                    GbForm.fLog = new frmLog();
                    break;
                case 3:
                    GbForm.fManual = new frmManual();
                    break;
                case 4:
                    GbForm.fRcp = new frmRecipe();
                    break;
                case 5:
                    GbForm.fCfg = new frmConfig();
                    break;
                case 6:
                    GbForm.fDIO = new frmDio();
                    break;
                case 7:
                    GbForm.fAlarm = new frmAlarm();
                    break;
                case 8:
                    GbForm.fTeach = new frmTeaching();
                    break;

                default:
                    break;
            }

            panProgressBar.Width = (panProgressBg.Width / 100) * nProgress;
            lbLoadingStatus.Text = msg;
        }

        void popStartUp_OnProgress(int nProgress, string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate()
                {
                    popStartUp_OnProgress(nProgress, msg);
                });
                return;
            }

            panProgressBar.Width = (panProgressBg.Width / 100) * nProgress;
            lbLoadingStatus.Text = msg;
        }

        void StartStopThreadLoading(bool bStart)
        {
            if (threadLoading != null)
            {
                flagAliveThreadLoading = false;
                threadLoading.Join(500);
                threadLoading.Abort();
                threadLoading = null;
            }

            if (bStart)
            {
                flagAliveThreadLoading = true;
                threadLoading = new Thread(new ParameterizedThreadStart(ThreadStartProgram));
                threadLoading.Name = "START UP THREAD";
                //threadReadIO.IsBackground = true;
                if (threadLoading.IsAlive == false)
                    threadLoading.Start(this);
            }
        }

        public void ThreadStartProgram(object obj)
        {
            popAppLoading _this = obj as popAppLoading;

            if (OnProgress != null)
                OnProgress(10, "Configuration and recipe object initialize...");

            // INIT
            ConfigMgr.Inst.Init(true);
            TeachMgr.Inst.Init();
            RecipeMgr.Inst.Init(true);
            ErrMgr.Inst.Init();
            RcpDevMgr.Inst.Init();

            GbVar.mcState.nControlStatus = (int)MESDF.eControlStatus.ONLINE_REMOTE;
            GbVar.mcState.nEqpStatus = (int)MESDF.eEQPStatus.IDLE_1;

            // [2022.04.19.kmlee] RecipeMgr Init에서 처리되게끔 변경
            //RecipeMgr.Inst.InitTrayInfo();
            //RecipeMgr.Inst.InitStripInfo();

            MESDF.init();

            Thread.Sleep(100);

            if (OnProgress != null)
                OnProgress(20, "Motion data setting...");

            MotionMgr.Inst.Init();
            Thread.Sleep(200);

            #region FORM INIT
            ////////////////////
            // FORM
            ////////////////////
            if (OnProgress != null)
                OnProgress(40, "Load GUI Form design and Initialize...");
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(41, "Load GUI Form design and Initialize... (Login Form)", 0);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(42, "Load GUI Form design and Initialize... (Auto Form)", 1);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(43, "Load GUI Form design and Initialize... (Log Form)", 2);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(44, "Load GUI Form design and Initialize... (Function Form)", 3);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(45, "Load GUI Form design and Initialize... (Recipe Form)", 4);
            Thread.Sleep(100);

            if (OnCreateForm != null)
                OnCreateForm(46, "Load GUI Form design and Initialize... (Config Form)", 5);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(50, "Load GUI Form design and Initialize... (DIO Form)", 6);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(50, "Load GUI Form design and Initialize... (Alarm Form)", 7);
            Thread.Sleep(10);

            if (OnCreateForm != null)
                OnCreateForm(50, "Load GUI Form design and Initialize... (Teaching Form)", 8);
            Thread.Sleep(10);

            #endregion


            #region DB

//            GbVar.dbITS = new DBExITS();
//#if !_NOTEBOOK
//            DBExITS.Open();
//#else
//            DBExITS.Open("localhost\\SQLEXPRESS,1433", "MCDB","MCDB","diones");
//#endif

            LogKeyWordMgr.KeyWordDic = LogKeyWordMgr.Read(LogKeyWordMgr.LogPaths);

            for (DBDF.LOG_TYPE i = 0; i < DBDF.LOG_TYPE.MAX; i++)
            {
                GbVar.dbLogData[(int)i] = new DBExLogData2((int)i);
            }


            for (int i = 0; i < (int)STRIP_MDL.MAX; i++)
            {
                GbVar.dbStripInfo[(int)i] = new DBExStripInfo((int)i);
            }


            SQLiteMgr.Inst.StartDB();

            //GbVar.dbAlarmData = new DBExAlarmData();
            //GbVar.dbEventData = new DBExEventData();

            GbVar.dbTactTime = new DBExTactTime();

            GbVar.dbLotLog = new DBExLotLog();
            //GbVar.dbMgzInfo = new DBExMgzInfo();
            GbVar.dbLotInfo = new DBExLotInfo();

            GbVar.dbSeqInfo = new DBExSeqInfo();
            GbVar.dbProcsQtyInfo = new DBExProcQtyInfo();//220611 pjh
            GbVar.dbProcsQtyLog = new DBExProcQtyLog();
            GbVar.dbITSInfo = new DBExITSInfo();

            for (int NO = 0; NO < (int)MCDF.eUNIT.MAX; NO++)
            {
                GbVar.dbGetTopInspection[NO] = new DBExToHandlerTopResult(NO + 1);
                GbVar.dbGetBtmInspection[NO] = new DBExToHandlerBtmResult(NO + 1);
            }

            GbVar.dbFromSorter = new DBExFromSorter();
            GbVar.dbFromSaw = new DBExFromSaw();

            for (int i = 0; i < (int)SEQ_ID.MAX; i++)
            {
                GbVar.SeqInfo[i] = new SeqProperties();
            }


            // ALARM LOG
            GbVar.dbAlarmLog = new DBExAlarmLog2();

            // STRIP LOG
            // 트래킹 로그 사용 안함
            //GbVar.dbStripLog2 = new DBExStripLog2();

            // EVENT LOG
            GbVar.dbEventLog2 = new DBExEventLog2();

            // ITS LOG
            // XOUT 사용 안함
            //GbVar.dbITSLog2 = new DBExITSLog2();

            // PROC QTY LOG
            GbVar.dbProcQtyLog2 = new DBExProcQtyLog2();

            #endregion
            GbVar.SWVersrion = GbFunc.GetBuildDateTime().ToString("yyyy-MM-dd HH:mm:ss") + "v 1.3.0.30" ;

            IFMgr.Inst.Open();

            if (OnProgress != null)
                OnProgress(70, "DB Done...");

            #region DEVICE
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

#if !_NOTEBOOK
            if (!GbDev.BarcodeReader.Connect_Tcp(remoteInfo.remotePath, remoteInfo.port))
            {

            }
#endif

            if (!File.Exists(PathMgr.Inst.PATH_MGZ_BARCODEREADER))
            {
                remoteInfo.remotePath = "169.254.60.223";
                remoteInfo.port = 23;

                remoteInfo.Serialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }
            else
            {
                remoteInfo = RemoteInfo.Deserialize(PathMgr.Inst.PATH_MGZ_BARCODEREADER);
            }

#if !_NOTEBOOK
            GbDev.MGZBarcodeReader.Connect(remoteInfo.remotePath);

            if (!GbDev.MGZBarcodeReader.IsConnect)
            {

            }
#endif

            //{
            //    MessageBox.Show("Barcode Reader is not opened!!");
            //    //if (OnMessageBox != null)
            //    //    OnMessageBox("BarCodeReader is not opened!!");
            //}

            //            for (int nCount = 0; nCount < GbDev.ACPM_MAX_COUNT; nCount++)
            //            {
            //#if _NOTEBOOK
            //                GbDev.IonizerMoniroing[nCount].SetCommMode(DionesUtil.DATA.CommBase.ECOMM_MODE.TCP_CLIENT);
            //#else
            //                GbDev.IonizerMoniroing[nCount].SetCommMode(DionesUtil.DATA.CommBase.ECOMM_MODE.SERIAL);
            //#endif
            //                DionesUtil.DATA.CommInfo commInfo = new DionesUtil.DATA.CommInfo();

            //                // TEST
            //                commInfo.ip = "127.0.0.1";
            //                commInfo.port = 501 + nCount;

            //                if (!File.Exists(PathMgr.Inst.IonizerMonitor(nCount)))
            //                {
            //                    // 실제
            //                    commInfo.BaudRate = 115200;
            //                    commInfo.DataBits = 8;
            //                    commInfo.Parity = System.IO.Ports.Parity.None;
            //                    commInfo.StopBits = System.IO.Ports.StopBits.One;
            //                    commInfo.PortName = string.Format("COM{0}", 32 + nCount);
            //                    GbDev.IonizerMoniroing[nCount].SetInfo(commInfo);
            //                    GbDev.IonizerMoniroing[nCount].Serialize(PathMgr.Inst.IonizerMonitor(nCount));
            //                }
            //                else
            //                {
            //                    GbDev.IonizerMoniroing[nCount].Deserialize(PathMgr.Inst.IonizerMonitor(nCount));
            //                }



            //                if (!GbDev.IonizerMoniroing[nCount].Open())
            //                {
            //                    MessageBox.Show("A-CPM1300 Open Fail");
            //                }
            //            }
            DionesUtil.DATA.CommInfo commInfo = new DionesUtil.DATA.CommInfo();
            GbDev.trayVision.SetCommMode(DionesUtil.DATA.CommBase.ECOMM_MODE.SERIAL);

            commInfo = new DionesUtil.DATA.CommInfo();

            if (!File.Exists(PathMgr.Inst.PATH_TRAY_VISION))
            {
                // 실제
                commInfo.BaudRate = 9600;
                commInfo.DataBits = 8;
                commInfo.Parity = System.IO.Ports.Parity.None;
                commInfo.StopBits = System.IO.Ports.StopBits.One;
                commInfo.PortName = string.Format("COM3");
                GbDev.trayVision.SetInfo(commInfo);
                GbDev.trayVision.Serialize(PathMgr.Inst.PATH_TRAY_VISION);
            }
            else
            {
                GbDev.trayVision.Deserialize(PathMgr.Inst.PATH_TRAY_VISION);
            }
            if (!GbDev.trayVision.Open())
            {
                MessageBox.Show("트레이 비전 통신이 열리지 않았습니다");
            }

            #endregion


            if (OnProgress != null)
                OnProgress(80, "Device Done...");

            for (int i = 0; i < (int)SVDF.AXES.MAX; i++)
            {
                MotionMgr.Inst[i].SetHomeOffset(ConfigMgr.Inst.Cfg.MotData[i].dHomeOffset);
                MotionMgr.Inst[i].SetInPositionTolerance(ConfigMgr.Inst.Cfg.MotData[i].dInpositionBand);
            }


            if (OnProgress != null)
                OnProgress(100, "Initialize Done...");

            // THREAD
            GbSeq.manualRun = new THREAD.ManualRunThread();
            GbSeq.autoRun = new THREAD.AutoRunThread();

            Thread.Sleep(1000);

            GbVar.StripTTLog = new StripTTLogThread();//220817 pjh
            GbVar.StripTTLog.StartStopManualThread(true);

            GbVar.LotReportLog = new LotReportLogThread();
            GbVar.LotReportLog.StartStopManualThread(true);

            if (OnFinish != null)
                OnFinish();
        }        
    }
}
