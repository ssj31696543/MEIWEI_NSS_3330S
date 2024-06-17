using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NSS_3330S.POP;
using SQLiteDB;
using NSS_3330S.MOTION;
using NSS_3330S.GLOBAL;

using LinkMessage;

namespace NSS_3330S.FORM
{
    public partial class frmMain : Form
    {
        frmLogging fLogin;
        frmAuto fAuto;

        frmRecipe fRcp;
        frmManual fManual;
        frmConfig fCfg;
        frmAlarm fAlarm;
        frmDio fIO;
        frmLog fLog;
        frmTeaching fTeach;

        delegate void DeleSetEnableToolBtn(bool bEnable);

        public delegate void EqInitCimReport();
        public event EqInitCimReport eqInitCimReport;

        public delegate void EqSVReport();
        public event EqSVReport eqSvReport;

        public delegate void EqECVReport();
        public event EqECVReport eqEcvReport;

        public static popErrorMsg fErrPopup = new popErrorMsg();
        public static popStripExist fErrStripExist = new popStripExist();   
        public static popMagazineMsg fErrPopupMagazine = new popMagazineMsg(); // 매거진 전용 알람 팝업 창 
        public static popErrorMapInsp fErrPopMapInsp = new popErrorMapInsp();   // Map Inspection 전용 알람 팝업 창
        public static popErrorMapAlign fErrPopMapAlign = new popErrorMapAlign();   // Map Align 전용 알람 팝업 창
        public static popLotEnd fLotEnd = new popLotEnd();//랏엔드 전용 알람 팝업 창 220615
        public static popChipExist fErrChipExist = new popChipExist(); //칩피커 진공 알람 시 팝업
        public static popTrayCheck fTrayCheck= new popTrayCheck();

        public static frmWarningMsg fWarning = null;
        public static popMCState pMcState;
        public static popHostMsg pHostMsg = new popHostMsg();
        public static popTerminalMsg pTerminalMsg = new popTerminalMsg();
        public static popChangeCtrlStatus pChangeCtrl = new popChangeCtrlStatus();

        bool m_bRun = false;
        bool m_bLastEnableButton = false;
        bool m_Blink = false;

        public bool bMainStartPushFlag = false;
        public bool bMainStopPushFlag = false;
        public bool bMainResetPushFlag = false;

        // 매거진 강제 배출 S/W FLAG
        public bool bLdConvMgzLoadPushFlag = false;
        public bool bLdConvMgzUnloadPushFlag = false;
        public bool bUldConvMgzUnloadPushFlag = false;
        public bool bUldConvMgzLoadPushFlag = false;

        int m_nLightCurtainCnt = 0;
        DateTime m_dtFullSensorCheckTime = DateTime.Now;

        bool m_bRunningUPH = false;

        bool bExactTime = false;

        public frmMain()
        {
            InitializeComponent();

            bLdConvMgzLoadPushFlag = false;
            bLdConvMgzUnloadPushFlag = false;
            bUldConvMgzUnloadPushFlag = false;
            bLdConvMgzUnloadPushFlag = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure you want to quit ?"), "EXIT");
            msg.TopMost = true;

            if (msg.ShowDialog(this) == DialogResult.OK)
            {
                //tmrRefresh.Stop();
                //tmrFlicker.Stop();
                //tmrStatus.Stop();
                GbFunc.WriteEventLog(this.GetType().Name.ToString(), "btnExit_Click");/*Application.Exit();*/
                this.Close(); /*Application.Exit();*/
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            GbDev.BarcodeReader = new DionesTool.DEVICE.READER.DataManReader(WindowsFormsSynchronizationContext.Current);

            popAppLoading popStart = new popAppLoading();
            DialogResult result = popStart.ShowDialog();

            #region Form 연결
            this.WindowState = FormWindowState.Maximized;

            fLogin = GbForm.fLogin;
            fAuto = GbForm.fAuto;
            fLog = GbForm.fLog;
            fRcp = GbForm.fRcp;
            fCfg = GbForm.fCfg;
            fAlarm = GbForm.fAlarm;
            fIO = GbForm.fDIO;
            fManual = GbForm.fManual;
            fTeach = GbForm.fTeach;

            fAuto.MdiParent = this;
            fAuto.Parent = this.panClient;

            fLog.MdiParent = this;
            fLog.Parent = this.panClient;

            fRcp.MdiParent = this;
            fRcp.Parent = this.panClient;

            fCfg.MdiParent = this;
            fCfg.Parent = this.panClient;

            fAlarm.MdiParent = this;
            fAlarm.Parent = this.panClient;

            fIO.MdiParent = this;
            fIO.Parent = this.panClient;

            fManual.MdiParent = this;
            fManual.Parent = this.panClient;

            fTeach.MdiParent = this;
            fTeach.Parent = this.panClient;

            fLogin.MdiParent = this;
            fLogin.Parent = this.panClient;
            fLogin.Show();
            #endregion

            lbVersion.Text = GbVar.SWVersrion;

            tmrRefresh.Start();
            tmrStatus.Enabled = true;

            fErrPopup.addAlarmEvent += fAlarm.addGridCurAlarm;
            fErrPopup.OnResetEvent += ResetEvent;
			fErrStripExist.addAlarmEvent +=fAlarm.addGridCurAlarm;
			fErrStripExist.OnResetEvent += ResetEvent;
            fErrPopupMagazine.OnResetEvent += ResetEvent;
            fErrPopMapInsp.OnResetEvent += ResetEvent;
            fErrPopMapAlign.OnResetEvent += ResetEvent;
            fLotEnd.OnResetEvent += ResetEvent;
            fLotEnd.addAlarmEvent += fAlarm.addGridCurAlarm;
            fErrChipExist.OnResetEvent += ResetEvent;
            fErrChipExist.addAlarmEvent += fAlarm.addGridCurAlarm;
            fTrayCheck.OnResetEvent += ResetEvent;

            fWarning = new frmWarningMsg();

            fLogin.userLogin += fLogin_userLogin;

            GbSeq.manualRun.errSaftyStopEvent += ErrorMainProcess;
            GbSeq.autoRun.errSaftyStopEvent += ErrorMainProcess;
            fAuto.RunStartStopEvent += Autorun;
            fAuto.ResetEvent += ResetEvent;

            fCfg.ChangeFormLang += ChangeFormLang;

            ChangeFormLang();

            // 프로그램 시작시 ON 할것 OUTPUT
            {
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEANER_BRUSH_WATER, true); //상시온
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_BLOCK_WORK_VAC_PUMP, true);
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_PK_WORK_VAC_PUMP, true);
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STG_1_EVEN_WORK_VAC_PUMP, true);
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STG_1_ODD_WORK_VAC_PUMP, true);
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STG_2_EVEN_WORK_VAC_PUMP, true);
                //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MAP_STG_2_ODD_WORK_VAC_PUMP, true);
            }

            //GbVar.SeqMgr.DownLoadSeq();
            GbVar.lstBinding_HostLot = GbVar.LotMgr.DownloadLotInfo();
            GbVar.lstBinding_EqpProc = GbVar.LotMgr.DownloadProcQtyInfo();//220611 pjh

            if (GbVar.lstBinding_EqpProc.Count == 0)
            {
                EqpProcInfo Eqp = new EqpProcInfo();

                if (GbVar.lstBinding_HostLot != null)
                {
                    if(GbVar.lstBinding_HostLot.Count > 0)
                    {
                        Eqp.LOT_ID = GbVar.lstBinding_HostLot[0].LOT_ID;
                        GbVar.Seq.sLdMzLotStart.LOT_ID = GbVar.lstBinding_HostLot[0].LOT_ID;

                        GbVar.Seq.sLdMzLoading.LOT_ID = GbVar.lstBinding_HostLot[0].LOT_ID;
                    }
                }
                GbVar.lstBinding_EqpProc.Add(Eqp);
            }


            //220817 pjh
            DateTime dt = new DateTime();
            if (!DateTime.TryParse(GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_IN_TIME, out dt))
                dt = DateTime.Now;
            GbVar.StripTTLog.SetFileName(dt, GbVar.lstBinding_EqpProc[MCDF.CURRLOT].LOT_ID);

            tmrFlicker.Enabled = true;
            TTDF.InitTact();

            IFMgr.Inst.VISION.UpdateRecipeData();
            IFMgr.Inst.VISION.UpdateRecipeName();
            //221021 HEP 추가
            //이오나이저 솔밸브 켤떄 온
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.IONIZER_AIR, true);
        }

        void fLogin_userLogin(int nScrNo)
        {
            ////셋업기간중엔 메이커 레벨로 간주함
            //popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("셋업 기간중에는 메이커 레벨로 로그인 됩니다."), FormTextLangMgr.FindKey("확인"), MessageBoxButtons.OK);
            //msg.TopMost = true;
            //msg.ShowDialog();

            //GbVar.nLoggingUserLevel = MCDF.LEVEL_MAK;

            panToolMenu.Visible = true;
            LogOut_panel.Visible = true;
            ScreenChange(1);
        }

        private void SetEnableToolBtn(bool bEnable)
        {
            // 로그인된 유저에 따라 결정
            if (this.InvokeRequired)
            {
                this.Invoke(new DeleSetEnableToolBtn(_SetEnableToolBtn), new object[] { bEnable });
                return;
            }

            _SetEnableToolBtn(bEnable);
        }

        private void _SetEnableToolBtn(bool bEnable)
        {
            m_bLastEnableButton = bEnable;

            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_NOT_LOGIN)
            {
                bEnable = false;

                if (lbProjectName.Enabled != bEnable) lbProjectName.Enabled = bEnable;

                if (btnToolMain.Visible != bEnable) btnToolMain.Visible = bEnable;
                if (btnToolManual.Visible != bEnable) btnToolManual.Visible = bEnable;
                if (btnToolRecipe.Visible != bEnable) btnToolRecipe.Visible = bEnable;
                if (btnToolDio.Visible != bEnable) btnToolDio.Visible = bEnable;
                if (btnToolConfig.Visible != bEnable) btnToolConfig.Visible = bEnable;
                if (btnToolAlarm.Visible != bEnable) btnToolAlarm.Visible = bEnable;
                if (btnToolLog.Visible != bEnable) btnToolLog.Visible = bEnable;
                if (btnToolFunc.Visible != bEnable) btnToolFunc.Visible = bEnable;

                if (btnJog.Visible != bEnable) btnJog.Visible = bEnable;
                if (btnDoorLock.Enabled != bEnable) btnDoorLock.Enabled = bEnable;

                if (btnExit.Visible != bEnable) btnExit.Visible = bEnable;
                if (button_Logout.Visible != bEnable) button_Logout.Visible = bEnable;
            }
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_OP)
            {
                //if (lbProjectName.Enabled != bEnable) lbProjectName.Enabled = bEnable;

                //if (btnToolMain.Visible != true) btnToolMain.Visible = true;
                //if (btnToolManual.Visible != bEnable) btnToolManual.Visible = bEnable;
                //if (btnToolRecipe.Visible != false) btnToolRecipe.Visible = false;
                //if (btnToolDio.Visible != false) btnToolDio.Visible = false;
                //if (btnToolConfig.Visible != false) btnToolConfig.Visible = false;
                //if (btnToolAlarm.Visible != bEnable) btnToolAlarm.Visible = bEnable;
                //if (btnToolLog.Visible != bEnable) btnToolLog.Visible = bEnable;
                //if (btnToolFunc.Visible != bEnable) btnToolFunc.Visible = bEnable;

                //if (btnJog.Visible != bEnable) btnJog.Visible = bEnable;
                //if (btnDoorLock.Enabled != bEnable) btnDoorLock.Enabled = bEnable;

                //if (btnExit.Visible != bEnable) btnExit.Visible = bEnable;
                //if(button_Logout.Visible != bEnable) button_Logout.Visible = bEnable;
                if (lbProjectName.Enabled != bEnable) lbProjectName.Enabled = bEnable;

                if (btnToolMain.Visible != true) btnToolMain.Visible = true;
                if (btnToolManual.Visible != bEnable) btnToolManual.Visible = bEnable;
                if (btnToolRecipe.Visible != bEnable) btnToolRecipe.Visible = bEnable;
                if (btnToolDio.Visible != bEnable) btnToolDio.Visible = true;
                if (btnToolConfig.Visible != bEnable) btnToolConfig.Visible = bEnable;
                if (btnToolAlarm.Visible != bEnable) btnToolAlarm.Visible = true;
                if (btnToolLog.Visible != bEnable) btnToolLog.Visible = true;
                if (btnToolFunc.Visible != bEnable) btnToolFunc.Visible = bEnable;

                if (btnJog.Visible != bEnable) btnJog.Visible = bEnable;
                if (btnDoorLock.Enabled != bEnable) btnDoorLock.Enabled = bEnable;

                if (btnExit.Visible != bEnable) btnExit.Visible = bEnable;
                if (button_Logout.Visible != bEnable) button_Logout.Visible = bEnable;
            }
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_ENG)
            {
                if (lbProjectName.Enabled != bEnable) lbProjectName.Enabled = bEnable;

                if (btnToolMain.Visible != true) btnToolMain.Visible = true;
                if (btnToolManual.Visible != bEnable) btnToolManual.Visible = bEnable;
                if (btnToolRecipe.Visible != bEnable) btnToolRecipe.Visible = bEnable;
                if (btnToolDio.Visible != bEnable) btnToolDio.Visible = true;
                if (btnToolConfig.Visible != bEnable) btnToolConfig.Visible = bEnable;
                if (btnToolAlarm.Visible != bEnable) btnToolAlarm.Visible = true;
                if (btnToolLog.Visible != bEnable) btnToolLog.Visible = true;
                if (btnToolFunc.Visible != bEnable) btnToolFunc.Visible = bEnable;

                if (btnJog.Visible != bEnable) btnJog.Visible = bEnable;
                if (btnDoorLock.Enabled != bEnable) btnDoorLock.Enabled = bEnable;

                if (btnExit.Visible != bEnable) btnExit.Visible = bEnable;
                if (button_Logout.Visible != bEnable) button_Logout.Visible = bEnable;
            }
            else if (GbVar.nLoggingUserLevel == MCDF.LEVEL_MAK)
            {
                if (lbProjectName.Enabled != bEnable) lbProjectName.Enabled = bEnable;

                if (btnToolMain.Visible != true) btnToolMain.Visible = true;
                if (btnToolManual.Visible != bEnable) btnToolManual.Visible = bEnable;
                if (btnToolRecipe.Visible != bEnable) btnToolRecipe.Visible = bEnable;
                if (btnToolDio.Visible != bEnable) btnToolDio.Visible = true;
                if (btnToolConfig.Visible != bEnable) btnToolConfig.Visible = bEnable;
                if (btnToolAlarm.Visible != bEnable) btnToolAlarm.Visible = true;
                if (btnToolLog.Visible != bEnable) btnToolLog.Visible = true;
                if (btnToolFunc.Visible != bEnable) btnToolFunc.Visible = bEnable;

                if (btnJog.Visible != bEnable) btnJog.Visible = bEnable;
                if (btnDoorLock.Enabled != bEnable) btnDoorLock.Enabled = bEnable;

                if (btnExit.Visible != bEnable) btnExit.Visible = bEnable;
                if (button_Logout.Visible != bEnable) button_Logout.Visible = bEnable;
            }

            //220516 오토런 시작 시 조그창 닫음
            //if (GbForm.pJog != null)
            //{
            //    if (GbForm.pJog.Visible && !bEnable)
            //        GbForm.pJog.Close();        
            //}
            if (GbForm.pNeonJog != null)
            {
                if (GbForm.pNeonJog.Visible && !bEnable)
                {
                    GbForm.pNeonJog.Close();
                }
            }
        }

        private void ResetToolButton()
        {
            btnToolMain.BackColor = Color.FromArgb(20, 33, 43);
            btnToolManual.BackColor = Color.FromArgb(20, 33, 43);
            btnToolRecipe.BackColor = Color.FromArgb(20, 33, 43);
            btnToolDio.BackColor = Color.FromArgb(20, 33, 43);
            btnToolConfig.BackColor = Color.FromArgb(20, 33, 43);
            btnToolAlarm.BackColor = Color.FromArgb(20, 33, 43);
            btnToolLog.BackColor = Color.FromArgb(20, 33, 43);
            btnToolFunc.BackColor = Color.FromArgb(20, 33, 43);
        }

        public void ScreenChange(int nScreenNo)
        {
            ResetToolButton();

            // VisibleChanged Event를 받기 위해 추가
            fLogin.Hide();

            fAuto.Hide();
            fManual.Hide();
            fRcp.Hide();
            fCfg.Hide();
            fIO.Hide();
            fAlarm.Hide();
            fLog.Hide();
            fTeach.Hide();

            //Show Screen
            switch (nScreenNo)
            {
                case 0://DF.SCREEN_LOGIN:

                    //Logout 상태로 만듬.
                    //GlobalVariable.Login.nLoginLevel = MCDF.LEVEL_NOT_LOGIN;

                    //SetEnableToolBtn(false);
                    fLogin.Show();
                    fLogin.BringToFront();

                    //setAllScreenDisabel();
                    //GbFunc.WriteLogging("User Log Out");
                    //lbActiveScreen.Text = "[ LOGGING ]";
                    panToolMenu.Visible = false;
                    LogOut_panel.Visible = false;

                    GbVar.nActiveScreen = MCDF.SCREEN_LOGIN;
                    break;

                case 1://DF.SCREEN_AUTO:
                    fAuto.Show();
                    fAuto.BringToFront();

                    //lbActiveScreen.Text = "[ AUTO ]";
                    btnToolMain.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_AUTO;

                    break;

                case 2://MANUAL
                    fManual.Show();
                    fManual.BringToFront();

                    //lbActiveScreen.Text = "[ MANUAL ]";
                    btnToolManual.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_MANUAL;

                    break;

                case 3://DF.SCREEN_TEACH:
                    fRcp.Show();
                    fRcp.BringToFront();

                    //lbActiveScreen.Text = "[ RECIPE ]";
                    btnToolRecipe.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_RECIPE;

                    break;

                case 4://DF.SCREEN_CONFIG:
                    fCfg.Show();
                    fCfg.BringToFront();

                    //lbActiveScreen.Text = "[ CONFIG ]";
                    btnToolConfig.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_CONFIG;

                    break;

                case 5://IO
                    fIO.Show();
                    fIO.BringToFront();

                    //lbActiveScreen.Text = "[ IO ]";
                    btnToolDio.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_IO;

                    break;

                case 6://DF.SCREEN_REPORT:
                    fAlarm.Show();
                    fAlarm.BringToFront();

                    //lbActiveScreen.Text = "[ ALARM ]";
                    btnToolAlarm.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_ALARM;

                    break;

                case 7://DF.SCREEN_LOG:
                    fLog.Show();
                    fLog.BringToFront();

                    //lbActiveScreen.Text = "[ LOG ]";
                    btnToolLog.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_LOG;

                    break;

                case 8://DF.SCREEN_LOG:
                    fTeach.Show();
                    fTeach.BringToFront();

                    //lbActiveScreen.Text = "[ FUNCTION ]";
                    btnToolFunc.BackColor = Color.FromArgb(58, 97, 128);
                    GbVar.nActiveScreen = MCDF.SCREEN_TEACH;

                    break;

                default: break;
            }
        }

        private void OnMainToolButtonClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nTag = btn.GetTag();
            if (GbVar.nActiveScreen == nTag) return;

            ScreenChange(Convert.ToInt16(btn.Tag));
        }



        private void ErrorMainProcess(int nErrNo)
        {
            try
            {
                GbVar.mcState.nLastErrNo = nErrNo;

                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        ShowErrorMessage(nErrNo);
                    });
                }
                else
                {
                    ShowErrorMessage(nErrNo);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
            }
        }

        public void ShowErrorMessage(int nErrNo)
        {
            //if (GbVar.mcState.isErrMain[nErrNo]) return;
            GbVar.mcState.isErrMain[nErrNo] = true;

            STRIP_MDL eMdl = STRIP_MDL.NONE;
            if (GbFunc.IsPickerAndStageVacError(nErrNo, ref eMdl))
            {
                if (fErrStripExist == null || fErrStripExist.IsDisposed)
                {
                    fErrStripExist = new popStripExist();
                }

                fErrStripExist.SetErrorText(nErrNo, eMdl);
                fErrStripExist.TopMost = true;
                fErrStripExist.StartPosition = FormStartPosition.CenterScreen;
                fErrStripExist.Show();
            }
            else
            {
            	if(nErrNo == (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST_POPUP)
            	{
                	// 매거진 에러 전용
                	if (fErrPopupMagazine == null || fErrPopupMagazine.IsDisposed)
                	{
                    	fErrPopupMagazine = new popMagazineMsg();
                	}

                	fErrPopupMagazine.SetErrorText(nErrNo);
                	fErrPopupMagazine.TopMost = true;
                	fErrPopupMagazine.StartPosition = FormStartPosition.CenterScreen;
                	fErrPopupMagazine.Show();
	            }
	            else if (nErrNo == (int)ERDF.E_MAP_INSPECTION_ERROR_COUNT_OVER)
	            {
	                // Map Inspection 에러 전용
	                if (fErrPopMapInsp == null || fErrPopMapInsp.IsDisposed)
	                {
	                    fErrPopMapInsp = new popErrorMapInsp();
	                }

	                fErrPopMapInsp.SetErrorText(nErrNo);
	                fErrPopMapInsp.TopMost = true;
	                fErrPopMapInsp.StartPosition = FormStartPosition.CenterScreen;
	                fErrPopMapInsp.Show();
            	}
                else if (nErrNo == (int)ERDF.E_MAP_STAGE_1_TOP_AIGN_NG || nErrNo == (int)ERDF.E_MAP_STAGE_2_TOP_AIGN_NG)
                {
                    // Map Inspection 에러 전용
                    if (fErrPopMapAlign == null || fErrPopMapAlign.IsDisposed)
                    {
                        fErrPopMapAlign = new popErrorMapAlign();
                    }

                    fErrPopMapAlign.SetErrorText(nErrNo);
                    fErrPopMapAlign.TopMost = true;
                    fErrPopMapAlign.StartPosition = FormStartPosition.CenterScreen;
                    fErrPopMapAlign.Show();
                }
                else if (nErrNo == (int)ERDF.E_LOT_END_STOP)
                {
                    // 랏엔드 전용 에러 팝업
                    if (fLotEnd == null || fLotEnd.IsDisposed)
                    {
                        fLotEnd = new popLotEnd();
                    }

                    fLotEnd.SetErrorText(nErrNo);
                    fLotEnd.TopMost = true;
                    fLotEnd.StartPosition = FormStartPosition.CenterScreen;
                    fLotEnd.Show();
                }

                else if ((nErrNo >= (int)ERDF.E_X1_PK_VAC_1_NOT_ON && nErrNo <= (int)ERDF.E_X2_PK_VAC_8_NOT_ON) &&
                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse == true)
                {
                    // 랏엔드 전용 에러 팝업
                    if (fErrChipExist == null || fErrChipExist.IsDisposed)
                    {
                        fErrChipExist = new popChipExist();
                    }

                    fErrChipExist.SetErrorText(nErrNo);
                    fErrChipExist.TopMost = true;
                    fErrChipExist.StartPosition = FormStartPosition.CenterScreen;
                    fErrChipExist.Show();                 
                }
                else if(nErrNo == (int) ERDF.E_GD_TRAY_STAGE_1_CHECK_FAIL ||
                        nErrNo == (int) ERDF.E_GD_TRAY_STAGE_2_CHECK_FAIL ||
                        nErrNo == (int) ERDF.E_RW_TRAY_STAGE_CHECK_FAIL)
                {
                    if (fTrayCheck == null || fTrayCheck.IsDisposed)
                    {
                        fTrayCheck = new popTrayCheck();
                    }

                    fTrayCheck.TopMost = true;
                    fTrayCheck.StartPosition = FormStartPosition.CenterScreen;
                    fTrayCheck.Show();
                }
                else
				{
                	if (fErrPopup == null || fErrPopup.IsDisposed)
                	{
                    	fErrPopup = new popErrorMsg();
                	}

	            	fErrPopup.SetErrorText(nErrNo);
	            	fErrPopup.TopMost = true;
	            	fErrPopup.StartPosition = FormStartPosition.CenterScreen;
	            	fErrPopup.Show();
				}
            }
        }

        private void HostMsgMainProcess(string strMsg, int nErrNo)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        ShowHostMessage(strMsg, nErrNo);
                    });
                }
                else
                {
                    ShowHostMessage(strMsg, nErrNo);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
            }
        }

        public void ShowHostMessage(string strMsg, int nErrNo)
        {
            if (pHostMsg == null || pHostMsg.IsDisposed)
            {
                pHostMsg = new popHostMsg();
            }

            pHostMsg.SetMessage(strMsg);
            pHostMsg.TopMost = true;
            pHostMsg.StartPosition = FormStartPosition.CenterScreen;
            pHostMsg.Show();
        }

        private void TermialMsgMainProcess(string strMsg)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        ShowTerminalMessage(strMsg);
                    });
                }
                else
                {
                    ShowTerminalMessage(strMsg);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
            }
        }

        public void ShowTerminalMessage(string strMsg)
        {
            if (pTerminalMsg == null || pTerminalMsg.IsDisposed)
            {
                pTerminalMsg = new popTerminalMsg();
            }

            pTerminalMsg.SetMessage(strMsg);
            pTerminalMsg.TopMost = true;
            pTerminalMsg.StartPosition = FormStartPosition.CenterScreen;
            pTerminalMsg.Show();
        }


        private void TermialMsgMainProcess(List<string> strMsg)
        {
            try
            {
                if (this.InvokeRequired)
                {
                    this.Invoke((MethodInvoker)delegate()
                    {
                        ShowTerminalMessage(strMsg);
                    });
                }
                else
                {
                    ShowTerminalMessage(strMsg);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
            }
        }

        public void ShowTerminalMessage(List<string> strMsg)
        {
            if (pTerminalMsg == null || pTerminalMsg.IsDisposed)
            {
                pTerminalMsg = new popTerminalMsg();
            }

            pTerminalMsg.SetMessage(strMsg);
            pTerminalMsg.TopMost = true;
            pTerminalMsg.StartPosition = FormStartPosition.CenterScreen;
            pTerminalMsg.Show();
        }


        void ResetEvent()
        {
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleErr.Length; nCnt++)
            {
                GbVar.mcState.isCycleErr[nCnt] = false;
            }

            for (int nCnt = 0; nCnt < (int)ERDF.ERR_CNT; nCnt++)
            {
                GbVar.mcState.isErrMain[nCnt] = false;
            }
        }

        void Autorun(int nRunMode)
        {
            if (nRunMode == MCDF.CMD_RUN)
            {
                StartProcess();
            }
            else if (nRunMode == MCDF.CMD_CYCLE_STOP)
            {
                StopProcess();
            }
            else if (nRunMode == MCDF.CMD_STOP)
            {
                EStopProcess();
            }
        }

        bool CheckValidationStart()
        {
            popMessageBox msg;

            IODF.INPUT[] inputGdTrayDetect = { IODF.INPUT.GD_TRAY_1_CHECK, IODF.INPUT.GD_TRAY_2_CHECK };
            for (int nTableNo = 0; nTableNo < CFG_DF.MAX_ULD_GOOD_TRAY_CNT; nTableNo++)
            {
                // 양품 트레이 스테이지가 작업 중
                if(GbVar.Seq.sUldGDTrayTable[nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
                {
                    // 트레이가 감지되지 않으면
                    if (GbVar.IO[inputGdTrayDetect[nTableNo]] == 0)
                    {
                        msg = new popMessageBox(String.Format("작업 중인 양품 트레이 스테이지 {0}에 트레이가 감지되지 않습니다.\r\n제거된 상태로 진행하시겠습니까?", nTableNo + 1), "경고");
                        msg.TopMost = true;
                        if (msg.ShowDialog(this) != DialogResult.OK) return false;

                        GbVar.Seq.sUldGDTrayTable[nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY] = false;
                        GbVar.Seq.sUldGDTrayTable[nTableNo].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                    }
                }
            }

            // REWORK 트레이 스테이지가 작업 중
            if (GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY])
            {
                // 트레이가 감지되지 않으면
                if (GbVar.IO[IODF.INPUT.RW_TRAY_CHECK] == 0)
                {
                    msg = new popMessageBox(String.Format("작업 중인 재작업 트레이 스테이지에 트레이가 감지되지 않습니다.\r\n제거된 상태로 진행하시겠습니까?"), "경고");
                    msg.TopMost = true;
                    if (msg.ShowDialog(this) != DialogResult.OK) return false;

                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_SORT_OUT_READY] = false;
                    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                }
            }

            // 도어 체크
            int nDoorResult = SafetyMgr.Inst.GetSafetyDoor(true);
            if (FNC.IsErr(nDoorResult))
            {
                GbVar.g_nDoorAlarmResult = nDoorResult;
                return false;
            }

            return true;
        }

        void StartProcess()
        {
            if (!GbVar.mcState.IsAllReady()) return;
            if (GbVar.mcState.IsError()) return;
            if (GbVar.mcState.IsAllRun()) return;
            if (GbVar.mcState.IsInitializing()) return;
            if (!GbVar.mcState.IsInitialized()) return;

            if (GbVar.nLoggingUserLevel != MCDF.LEVEL_MAK)
            {
                //if (GbForm.pJog != null && !GbForm.pJog.IsDisposed && GbForm.pJog.Visible) GbForm.pJog.Hide();
                if (GbForm.pNeonJog != null && !GbForm.pNeonJog.IsDisposed && GbForm.pNeonJog.Visible) GbForm.pNeonJog.Hide();
            }

            if (!GbVar.bLotInsert)
            {
                popMessageBox pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("PLEASE OPEN LOT!!."), "Door Interlock Warning");
                pMsgStartIntl.TopMost = true;
                if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
            }

#if _NOTEBOOK
#else
            if (GbVar.bFirstLotCheck)
            {
                bool bIsStrip = false;
                bIsStrip |= GbVar.Seq.sStripRail.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.LD_RAIL_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.IN_LET_TABLE_VAC] == 1;

                bIsStrip |= GbVar.Seq.sStripTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.STRIP_PK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.STRIP_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sCuttingTable.Info.IsStrip();

                bIsStrip |= GbVar.Seq.sUnitTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.UNIT_PK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.UNIT_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sUnitDry.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.DRY_BLOCK_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.DRY_BLOCK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapTransfer.Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_PK_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_PK_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapVisionTable[0].Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_STG_1_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_1_VAC_ON] == 1;

                bIsStrip |= GbVar.Seq.sMapVisionTable[1].Info.IsStrip();
                bIsStrip |= GbFunc.IsAVacOn((int)IODF.A_INPUT.MAP_STG_2_WORK_VAC);
                bIsStrip |= GbVar.IO[IODF.OUTPUT.MAP_ST_2_VAC_ON] == 1;

                bIsStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[0].IsPickerAllEmptyUnit();
                bIsStrip |= !GbVar.Seq.sPkgPickNPlace.pInfo[1].IsPickerAllEmptyUnit();

                IODF.A_INPUT[] _arrInput = new IODF.A_INPUT[16] {
                                                                IODF.A_INPUT.X1_AXIS_1_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_2_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_3_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_4_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_5_PICKER_VACUUM,
                                                                IODF.A_INPUT.X1_AXIS_6_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_7_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_8_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_1_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_2_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_3_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_4_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_5_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_6_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_7_PICKER_VACUUM,
                                                                IODF.A_INPUT.X2_AXIS_8_PICKER_VACUUM,
                };

                for (int i = 0; i < 16; i++)
                {
                    int nIoIdx = (int)_arrInput[i];
                    double dVacValue = (GbVar.GB_AINPUT[nIoIdx] - ConfigMgr.Inst.Cfg.Vac[nIoIdx].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nIoIdx].dRatio;

                    if (dVacValue < ConfigMgr.Inst.Cfg.Vac[nIoIdx].dVacLevelLow)
                    {
                        bIsStrip |= true;
                    }
                    else
                    {
                        bIsStrip |= false;
                    }
                    bIsStrip |= GbVar.GB_CHIP_PK_VAC[i];
                }
                for (int nRow = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldGDTrayTable[0].Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                for (int nRow = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldGDTrayTable[1].Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                for (int nRow = GbVar.Seq.sUldRWTrayTable.Info.UnitArr.Length - 1; nRow > -1; nRow--)
                {
                    for (int nCol = GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow].Length - 1; nCol > -1; nCol--)
                    {
                        bIsStrip |= GbVar.Seq.sUldRWTrayTable.Info.UnitArr[nRow][nCol].IS_UNIT;
                    }
                }
                if (MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_CLAMP) ||
                    MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_CLAMP))
                {
                    if (!MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) &&
                        !MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK))
                    {
                        MotionMgr.Inst.SetOutput(IODF.OUTPUT.TRAY_PK_ATC_CLAMP, false);
                    }
                    else
                    {
                        bIsStrip |= true;
                    }
                }
                

                if (bIsStrip)
                {
                    popMessageBox pMsgStartIntl = new popMessageBox(FormTextLangMgr.FindKey("PLEASE REMOVE ALL STRIP, UNIT, TRAY !!."), "Lot Input Warning");
                    pMsgStartIntl.TopMost = true;
                    if (pMsgStartIntl.ShowDialog(this) != DialogResult.OK) return; else return;
                }

                GbVar.bFirstLotCheck = false;
            }
#endif

            // TODO : Device 연결 상태 확인 필요

            if (!CheckValidationStart()) return;

            popMessageBox pMsgStart = null;

            if (GbVar.isInTray == true)
            {
                //추후에는 시퀀스에서 자동으로 인식해서 작업 이어가기
                if (GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                {
                    pMsgStart = new popMessageBox(FormTextLangMgr.FindKey("트레이 트랜스퍼에 트레이를 모두 제거한 뒤 시작해 주십시오. "), "경고!!", MessageBoxButtons.OK);
                    pMsgStart.TopMost = true;
                    if (pMsgStart.ShowDialog(this) == DialogResult.OK) return;
                }
                else
                {
                    GbVar.isInTray = false;
                }
            }
          
            GbSeq.autoRun.ResetCmd();

            SetEnableToolBtn(false);

            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                if (nCnt >= (int)SEQ_ID.MANUAL_RUN) continue;

                GbVar.mcState.isCycleRunReq[nCnt] = true;
                GbVar.mcState.isCycleRun[nCnt] = true;
            }

            GbFunc.ClearScrapNeed();

            // 도어 잠금
            MotionMgr.Inst.SetDoorLock(true);
            if (RecipeMgr.Inst.Rcp.cleaning.nUnitPkSpongeCount > 0)
            {
                MotionMgr.Inst.SetOutput(IODF.OUTPUT.CLEAN_BRUSH_WATER, true);
            }
            
        }

        void StopProcess()
        {
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                GbVar.mcState.isCyclePause[nCnt] = false;
                GbVar.mcState.isCycleRunReq[nCnt] = false;
            }
            
            GbVar.mcState.isWaitForStop = true;
        }

        void EStopProcess()
        {
            for (int nCnt = 0; nCnt < GbVar.mcState.isCycleRunReq.Length; nCnt++)
            {
                GbVar.mcState.isCycleRunReq[nCnt] = false;
                GbVar.mcState.isCycleRun[nCnt] = false;
            }

            MotionMgr.Inst.AllStop();
            SetEnableToolBtn(true);
            AfterCycleStop();
        }

        private void AfterCycleStop()
        {
            //MotionMgr.Inst.SetOutput(IODF.OUTPUT.CLEAN_BRUSH_WATER, true);
            MotionMgr.Inst.SetDoorLock(false);
        }

        private void btnCycleStartLoading_Click(object sender, EventArgs e)
        {

        }

        private void btnManagRcp_Click(object sender, EventArgs e)
        {
            if (GbVar.mcState.IsMcInitailizing()) return;
            //if (GbThread.mnThread.IsManualRun()) return;
            if (GbForm.pRcp == null || GbForm.pRcp.IsDisposed) GbForm.pRcp = new popMgrRecipe();

            GbForm.pRcp.ShowDialog();

            if (GbVar.bChangeRecipe_Main)
            {
                RecipeMgr.Inst.Init();
                RecipeMgr.Inst.CopyTempRcp();

                RcpDevMgr.Inst.Init();

                fRcp.UpdateRcpData();
                //lblRecipeName.Text = RecipeMgr.Inst.Rcp.strRecipeName;
            }
            RecipeMgr.Inst.Init();

            GbVar.bChangeRecipe_Main = false;
        }

        private void btnJog_Click(object sender, EventArgs e)
        {

            //if (GbForm.pJog != null)
            //{
            //    GbForm.pJog.SetDesktopLocation(200, 200);
            //    GbForm.pJog.Show(this);
            //    return;
            //}
            //GbForm.pJog = new popJog();

            //GbForm.pJog.FormClosed += popJog_FormClosed;
            //GbForm.pJog.Owner = this;
            //GbForm.pJog.Show(this);
            if (GbForm.pNeonJog != null)
            {
                GbForm.pNeonJog.SetDesktopLocation(200, 200);
                GbForm.pNeonJog.Show(this);
                return;
            }

            GbForm.pNeonJog = new popNeonJog();

            GbForm.pNeonJog.FormClosed += popNeonJog_FormClosed;
            GbForm.pNeonJog.Owner = this;
            GbForm.pNeonJog.Show(this);

        }

        //void popJog_FormClosed(object sender, FormClosedEventArgs e)
        //{
        //    GbForm.pJog.FormClosed -= popJog_FormClosed;
        //    GbForm.pJog = null;
        //}
        void popNeonJog_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbForm.pNeonJog.FormClosed -= popNeonJog_FormClosed;
            GbForm.pNeonJog = null;
        }
        private void lbProjectName_Click(object sender, EventArgs e)
        {
            frmPM frmPm = new frmPM();
            frmPm.ShowDialog();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            
            GbVar.gbProgramExit = true;

            tmrRefresh.Enabled = false;
            tmrStatus.Enabled = false;

            GbForm.Close();

            //GbVar.SeqMgr.UploadSeq();
            GbVar.LotMgr.UpdateAllLotInfo(GbVar.lstBinding_HostLot);
            GbVar.LotMgr.UpdateAllProcQtyInfo(GbVar.lstBinding_EqpProc);//220611 pjh

            //if (popWarnning != null)
            //{
            //    popWarnning.Close();
            //    popWarnning = null;
            //}

            //m_toolTipStatus.RemoveAll();

            fLogin.userLogin -= fLogin_userLogin;
            fErrPopup.addAlarmEvent -= fAlarm.addGridCurAlarm;
            fErrPopup.OnResetEvent -= ResetEvent;
            fErrStripExist.addAlarmEvent -= fAlarm.addGridCurAlarm;
            fErrStripExist.OnResetEvent -= ResetEvent;
			fErrPopupMagazine.OnResetEvent -= ResetEvent;
			fErrPopMapInsp.OnResetEvent -= ResetEvent;
            fErrPopMapAlign.OnResetEvent -= ResetEvent;
            fLotEnd.OnResetEvent -= ResetEvent;
            fLotEnd.addAlarmEvent -= fAlarm.addGridCurAlarm;
            fTrayCheck.OnResetEvent -= ResetEvent;

            fLogin.userLogin -= fLogin_userLogin;
            fAuto.RunStartStopEvent -= Autorun;
            fAuto.ResetEvent -= ResetEvent;
            fCfg.ChangeFormLang -= ChangeFormLang;

            GbSeq.manualRun.errSaftyStopEvent -= ErrorMainProcess;
            GbSeq.autoRun.errSaftyStopEvent -= ErrorMainProcess;

            MotionMgr.Inst.Dispose();
            SQLiteMgr.Inst.Dispose();
            IFMgr.Inst.Close();

            GbVar.dbGetPreAlign.Close();
            GbVar.dbGetTopAlign.Close();
            GbVar.dbGetPickerCal.Close();
            GbVar.dbGetBtmInspection[0].Close();
            GbVar.dbGetTopInspection[0].Close();
            GbVar.dbGetBtmInspection[1].Close();
            GbVar.dbGetTopInspection[1].Close();

            //GbDev.LD_RFID.Close();
            //GbDev.TRAY_RFID.Close();
            for (DBDF.LOG_TYPE i = 0; i < DBDF.LOG_TYPE.MAX; i++)
            {
                GbVar.dbLogData[(int)i].Close();
            }

            GbVar.dbAlarmLog.Close();
            GbVar.dbEventLog2.Close();
            //GbVar.dbStripLog2.Close();
            //GbVar.dbITSLog2.Close();
            GbVar.dbProcQtyLog2.Close();
            GbVar.StripTTLog.StartStopManualThread(false);//220817 phj
            GbVar.LotReportLog.StartStopManualThread(false);
#if !_NOTEBOOK
            GbDev.BarcodeReader.Close();
#endif
            GbDev.trayVision.Close();

            if (GbSeq.manualRun != null)
                GbSeq.manualRun.Dispose();

            if (GbSeq.autoRun != null)
                GbSeq.autoRun.Dispose();
        }

        int nTimerCnt = 0;
        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            // [2022.04.22.kmlee] 알람 발생 시에도 TOOL 버튼을 활성화할 수 있도록 추가
            #region TOOL BUTTON
            if (GbVar.nLoggingUserLevel < 0)
            {
                if (m_bLastEnableButton)
                {
                    SetEnableToolBtn(false);
                }
            }
            else
            {
                if (m_bRun == false)
                {
                    // 설비 Run 시작
                    if (GbVar.mcState.IsAllStopRun() == false)
                    {
                        SetEnableToolBtn(false);
                        m_bRun = true;
                    }
                    else
                    {
                        if (!m_bLastEnableButton) m_bRun = true;
                    }
                }
                else
                {
                    // 설비 정지 시작
                    if (GbVar.mcState.IsAllStopRun() == true)
                    {
                        if (MotionMgr.Inst.IsAllStop())
                        {
                            //여기에서 초기화
                            SetEnableToolBtn(true);
                            m_bRun = false;

                            AfterCycleStop();
                        }
                        else
                        {
                            if (m_bLastEnableButton) m_bRun = false;
                        }
                    }
                    else
                    {
                        if (m_bLastEnableButton) m_bRun = false;
                    }
                }
            }
            #endregion

            // UPH Start, Stop
            if (GbVar.mcState.IsAllStopRun())
            {
                // Stop
                if (GbVar.g_swUPH.IsRunning)
                {
                    GbVar.g_swUPH.Stop();
                    m_bRunningUPH = true;
                }
            }
            else
            {
                if (m_bRunningUPH)
                {
                    if (!GbVar.g_swUPH.IsRunning)
                    {
                        GbVar.g_swUPH.Start();
                    }
                }
            }

            string strCurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
            if (dtxDay.DigitText != strCurrentDate) dtxDay.DigitText = strCurrentDate;

            string strCurrentTime = DateTime.Now.ToString("HH:mm:ss");
            if (dtxTime.DigitText != strCurrentTime) dtxTime.DigitText = strCurrentTime;

            if (lblRecipeName.Text != string.Format("{0} {1}", RecipeMgr.Inst.Rcp.strRecipeId, RecipeMgr.Inst.Rcp.strRecipeDescription))
                lblRecipeName.Text = string.Format("{0} {1}", RecipeMgr.Inst.Rcp.strRecipeId, RecipeMgr.Inst.Rcp.strRecipeDescription);

            if (IFMgr.Inst.VISION.m_Receiver.IsConnected())
            {
                if (label10.BackColor != Color.Lime)
                    label10.BackColor = Color.Lime;
            }
            else
            {
                if (label10.BackColor != Color.Gray)
                    label10.BackColor = Color.Gray;
            }


            if (IFMgr.Inst.SAW.m_Receiver.IsConnected())
            {
                if (label11.BackColor != Color.Lime)
                    label11.BackColor = Color.Lime;
            }
            else
            {
                if (label11.BackColor != Color.Gray)
                    label11.BackColor = Color.Gray;
            }

            bool bIsOnLd = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 1;
            bool bIsOnSorter = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 1;
            if (bIsOnLd && bIsOnSorter)
            {
                if (btnDoorLock.BackColor != Color.Red)
                    btnDoorLock.BackColor = Color.Red;
            }
            else
            {
                if (btnDoorLock.BackColor != SystemColors.ButtonFace)
                    btnDoorLock.BackColor = SystemColors.ButtonFace;
            }

            #region SAW CUTTING TABLE INTERLOCK
            //220505 pjh
            //Strip Picker Down시 Saw Cutting Table 인터락 비트 On
            SafetyMgr.Inst.CuttingTableInterlockToSaw();
            #endregion

            #region DOOR INTERLOCK WIN
            //20220328 임시 주석
            int nRet = GbFunc.IsDoorOpenOrPressEmo(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse);
            if (!FNC.IsSuccess(nRet))
            {
                if (GbForm.pMcStatus == null || GbForm.pMcStatus.IsDisposed)
                {
                    GbForm.pMcStatus = new popMCState();
                }

                if (!GbForm.pMcStatus.Visible)
                {
                    GbForm.pMcStatus.WindowState = FormWindowState.Maximized;
                    GbForm.pMcStatus.Show(this);
                }
            }
            else
            {
                // [2022.06.11.kmlee] 문이 다시 다 닫히고나서 다시 열 때 팝업창을 띄우기 위해
                if (GbForm.pMcStatus.WindowState != FormWindowState.Maximized)
                {
                    GbForm.pMcStatus.WindowState = FormWindowState.Maximized;
                }
            }
            #endregion

            #region Main Switch Button
            // Main switch
            /////// Start
            //2022 08 10 . 홍은표
            //매뉴얼런 돌때는 시작 안눌리게
            if (!GbVar.mcState.isManualRun)
            {
                if (bMainStartPushFlag == false && GbVar.GB_INPUT[(int)IODF.INPUT.START_SWITCH] == 1)
                {
                    bMainStartPushFlag = true;
                    if (GbVar.mcState.IsRun())
                    {
                        popMessageBox msg = new popMessageBox("사이클이 완전히 끝난 뒤 시작해주십시오.!", "경고", MessageBoxButtons.OK);
                        msg.ShowDialog();
                        return;
                    }

                    Autorun(MCDF.CMD_RUN);
                }
                else if (bMainStartPushFlag == true && GbVar.GB_INPUT[(int)IODF.INPUT.START_SWITCH] == 0)
                {
                    bMainStartPushFlag = false;
                }

                /////// Stop
                if (bMainStopPushFlag == false && GbVar.GB_INPUT[(int)IODF.INPUT.STOP_SWITCH] == 1)
                {
                    bMainStopPushFlag = true;
                    if (GbVar.mcState.IsRun() == false) return;

                    Autorun(MCDF.CMD_CYCLE_STOP);
                }
                else if (bMainStopPushFlag == true && GbVar.GB_INPUT[(int)IODF.INPUT.STOP_SWITCH] == 0)
                {
                    bMainStopPushFlag = false;
                }

                /////// Reset
                if (bMainResetPushFlag == false && GbVar.GB_INPUT[(int)IODF.INPUT.RESET_SWITCH] == 1)
                {
                    bMainResetPushFlag = true;

                    ResetEvent();
                    //MotionMgr.Inst.SetOutput(IODF_PMC.OUTPUT.BUZZER_1 + ConfigMgr.Inst.Cfg.Operator.nAlarmBuzzer, false);
                }
                else if (bMainResetPushFlag == true && GbVar.GB_INPUT[(int)IODF.INPUT.RESET_SWITCH] == 0)
                {
                    bMainResetPushFlag = false;
                }

                ////// Buzzer Stop
                if (GbVar.GB_INPUT[(int)IODF.INPUT.RESET_SWITCH] == 1)
                {
                    MotionMgr.Inst.AllBuzzerOff();
                }
            }
            if (GbVar.nLoggingUserLevel == MCDF.LEVEL_OP)
            {
                if (btnJog.Enabled)
                    btnJog.Enabled = false;
            }
            else
            {
                if (!btnJog.Enabled)
                    btnJog.Enabled = true;
            }
            #endregion

            #region MAGAZINE UNLOAD S/W

            // 매거진 컨베이어에 부착된 강제 배출 버튼이 눌린 경우 처리
            // LD/ULD CONV 시퀀스가 동작중이 아니라면 시퀀스 시작 , 동작 완료 후 CYCKE STOP 

            // LOAD CONV PUSH S/W
            if (bLdConvMgzLoadPushFlag == false && IsLdConvMgzInSW() == true)
            {
                // 이미 해당 시퀀스가 동작 중이라면 동작 하지 않기 위함
                if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] == false
                    && GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] == false)
                {
                    bLdConvMgzLoadPushFlag = true;
                    SetLdConvMgzInSW(true); // SW LED ON

                    // ::: 여기에 스퀀스 동작 연결 하면 됨 :::
                    GbVar.Seq.sLdMzLoading.bLdConvMgzLoadPushSW = true;

                    GbSeq.autoRun.ResetCmd();
                    GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
                    GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] = true;

                }
            }
            else if (bLdConvMgzLoadPushFlag == true && IsLdConvMgzInSW() == false)
            {
                bLdConvMgzLoadPushFlag = false;
            }

            // LOAD CONV PUSH S/W
            if (bLdConvMgzUnloadPushFlag == false && IsLdConvMgzOutSW() == true)
            {
                // 이미 해당 시퀀스가 동작 중이라면 동작 하지 않기 위함
                if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] == false
                    && GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] == false)
                {
                    bLdConvMgzUnloadPushFlag = true;
                    SetLdConvMgzOutSW(true); // SW LED ON

                    // ::: 여기에 스퀀스 동작 연결 하면 됨 :::
                    GbVar.Seq.sLdMzLoading.bLdConvMgzUnloadPushSW = true;

                    GbSeq.autoRun.ResetCmd();
                    GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_LD_CONV] = true;
                    GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_LD_CONV] = true;

                }
            }
            else if (bLdConvMgzUnloadPushFlag == true && IsLdConvMgzOutSW() == false)
            {
                bLdConvMgzUnloadPushFlag = false;
            }


            // UNLOAD CONV PUSH S/W
            if (bUldConvMgzLoadPushFlag == false && IsUldConvMgzOutSW() == true)
            {
                // 이미 해당 시퀀스가 동작 중이라면 동작 하지 않기 위함
                if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ULD_CONV] == false
                    && GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ULD_CONV] == false)
                {
                    bUldConvMgzLoadPushFlag = true;
                    SetUldConvMgzInSW(true); // SW LED ON

                    // ::: 여기에 스퀀스 동작 연결 하면 됨 ::: 
                    GbVar.Seq.sLdMzLoading.bUldConvMgzLoadPushSW = true;

                    GbSeq.autoRun.ResetCmd();
                    GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ULD_CONV] = true;
                    GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ULD_CONV] = true;

                }
            }
            else if (bUldConvMgzLoadPushFlag == true && IsUldConvMgzInSW() == false)
            {
                bUldConvMgzLoadPushFlag = false;
            }


            // UNLOAD CONV PUSH S/W
            if (bUldConvMgzUnloadPushFlag == false && IsUldConvMgzOutSW() == true)
            {
                // 이미 해당 시퀀스가 동작 중이라면 동작 하지 않기 위함
                if (GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ULD_CONV] == false
                    && GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ULD_CONV] == false)
                {
                    bUldConvMgzUnloadPushFlag = true;
                    SetUldConvMgzOutSW(true); // SW LED ON

                    // ::: 여기에 스퀀스 동작 연결 하면 됨 ::: 
                    GbVar.Seq.sLdMzLoading.bUldConvMgzUnloadPushSW = true;

                    GbSeq.autoRun.ResetCmd();
                    GbVar.mcState.isCycleRunReq[(int)SEQ_ID.LD_MZ_ULD_CONV] = true;
                    GbVar.mcState.isCycleRun[(int)SEQ_ID.LD_MZ_ULD_CONV] = true;

                }
            }
            else if (bUldConvMgzUnloadPushFlag == true && IsUldConvMgzOutSW() == false)
            {
                bUldConvMgzUnloadPushFlag = false;
            }


            #endregion
        }

        private void btnDoorLock_Click(object sender, EventArgs e)
        {
            //if (pMcState != null)
            //    pMcState.Dispose();

            //pMcState = new popMCState();
            //pMcState.Show();

            // MC RUN 확인
            if (GbVar.mcState.IsRun()/* || GbThread.mnThread.m_bManualRun*/)
            {
                popMessageBox msg = new popMessageBox("장비가 가동중입니다.", "경고");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;
                return;
            }

            bool bIsOnLd = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0;
            bool bIsOnSorter = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0;


            // 잠금센서 3개다 on 상태인경우 잠금해제 및 3개다 off 인경우 잠금
            if (btnDoorLock.BackColor == SystemColors.ButtonFace)
            {
                if (bIsOnLd && bIsOnSorter)
                {
                    // DOOR CLOSE 확인 (LOADER)
                    if (SafetyMgr.Inst.GetSafetyDoorLoader() != FNC.SUCCESS)
                    {
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0)
                        {
                            popMessageBox msg = new popMessageBox("로더에 닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                            msg.TopMost = true;
                            if (msg.ShowDialog(this) != DialogResult.OK) return;
                            return;
                        }
                    }

                    // DOOR CLOSE 확인 (SORTER)
                    if (SafetyMgr.Inst.GetSafetyDoorSorter() != FNC.SUCCESS)
                    {
                        if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0)
                        {
                            popMessageBox msg = new popMessageBox("소터에 닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                            msg.TopMost = true;
                            if (msg.ShowDialog(this) != DialogResult.OK) return;
                            return;
                        }
                    }

                    // DOOR LOCK 
                    {
                        GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, true);
                    }

                    // DOOR LOCK 
                    {
                        GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL, true);
                    }
                    btnDoorLock.BackColor = Color.Red;
                }
                else
                {
                    StringBuilder Errdoor = new StringBuilder();
                    if (!bIsOnLd)
                        Errdoor.Append(" 로더");
                    if (!bIsOnSorter)
                        Errdoor.Append(" 소터");

                    popMessageBox msg = new popMessageBox("장비 :"+Errdoor.ToString() + "에잠금 신호가 켜져 있습니다 확인해주세요.", "경고");
                    msg.TopMost = true;
                    Errdoor.Clear();
                    if (msg.ShowDialog(this) != DialogResult.OK) return;
                    return;
                }
            }
            else
            {
                {
                    GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, false);
                }

                {
                    GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL, false);
                }
            }

            // KEY 확인
        }
		
		private void MesControlStatusChange(long lID, string strCtrID)
        {

        }

        private void MesRemoteCmd(long lID, string strRcmd, List<string> psNames, List<string> psVals)
        {
            #region RCMD
            if (strRcmd == "CONTROL_STATUS_CHANGE_REQ")
            {
                
            }
            else if(strRcmd == "STOP")
            {

            }
            else if(strRcmd == "PP_SELECT")
            {

            }
            else if(strRcmd == "PPID_LIST")
            {

            }
            else if(strRcmd == "CHANGE_PARA_CONFIRM")
            {

            }            
            else if(strRcmd == "NEW_CARRIER_TRANSFER_CONFIRM")
            {

            }            
            else if(strRcmd == "LD_PROC_PORT_CARRIER_IN_CONFIRM")
            {

            }            
            else if(strRcmd == "SUB_LOAD_CONFIRM")
            {

            }
            else if (strRcmd == "SUB_UNLOAD_CONFIRM")
            {

            }
            else if (strRcmd == "UD_LOAD_PORT_CARRIER_FULL_CONFIRM")
            {

            }
            else if (strRcmd == "LOT_END_CONFIRM_TRUE")
            {

            }
            else if (strRcmd == "LOT_END_CONFIRM_FALSE")
            {

            }
            else if (strRcmd == "MAT_RESTORE_CONFIRM")
            {

            }
            else if (strRcmd == "MAT_INFO_REP")
            {

            }
            else if (strRcmd == "MAT_ASSIGN_CONFIRM")
            {

            }
            else if (strRcmd == "MAT_STOCK_CONFIRM")
            {

            }
            else if (strRcmd == "AMC_START_CONFIRM")
            {

            }
            else if (strRcmd == "KIT_RESTORE_CONFIRM")
            {

            }
            else if (strRcmd == "KIT_ASSIGN_CONFIRM")
            {

            }
            else if (strRcmd == "KIT_STOCK_CHANGE_CONFIRM")
            {

            }
            else if (strRcmd == "ATC_START_CONFIRM")
            {

            }
            else if (strRcmd == "ATC_END_CONFIRM")
            {

            }
            #endregion
        }

        private void MesDateTimeSet(string strTime)
        {
            //호스트로 부터 받은 시간을 PC시간으로 설정
            short syear = Convert.ToInt16(strTime.Substring(0, 4));
            short smonth = Convert.ToInt16(strTime.Substring(4, 2));
            short sday = Convert.ToInt16(strTime.Substring(6, 2));
            short shour = Convert.ToInt16(strTime.Substring(8, 2));
            short smin = Convert.ToInt16(strTime.Substring(10, 2));
            short ssec = Convert.ToInt16(strTime.Substring(12, 2));

            short[] sTime = new short[6];

            sTime[0] = syear;
            sTime[1] = smonth;
            sTime[2] = sday;
            sTime[3] = shour;
            sTime[4] = smin;
            sTime[5] = ssec;

            SystemTime_Change.SetTime(syear, smonth, sday, shour, smin, ssec);
        }

        private void tmrFlicker_Tick(object sender, EventArgs e)
        {
            m_Blink = !m_Blink;

            //220512 pjh
            IODF.OUTPUT[] Lamp = {IODF.OUTPUT.TOWER_LAMP_RED_LED, IODF.OUTPUT.TOWER_LAMP_YELLOW_LED, IODF.OUTPUT.TOWER_LAMP_GREEN_LED };
            int[] nStatus = {GbVar.nLampStatus[0], GbVar.nLampStatus[1], GbVar.nLampStatus[2]};

            for (int i = 0; i < nStatus.Length; i++)
            {
                if (nStatus[i] == MCDF.OFF)
                {
                    MotionMgr.Inst.SetOutput(Lamp[i], false);
                }
                else if (nStatus[i] == MCDF.ON)
                {
                    MotionMgr.Inst.SetOutput(Lamp[i], true);
                }
                else if (nStatus[i] == MCDF.BLINK)
                {
                    MotionMgr.Inst.SetOutput(Lamp[i], m_Blink);
                }
            }

            IODF.OUTPUT[] Switch = {
                                     IODF.OUTPUT.START_SWITCH_LED, IODF.OUTPUT.STOP_SWITCH_LED, IODF.OUTPUT.RESET_SWITCH_LED
                                 };
            int[] nSwitchStatus = { 
                                GbVar.nSwitchStatus[0], GbVar.nSwitchStatus[1], GbVar.nSwitchStatus[2]
                            };

            for (int i = 0; i < nSwitchStatus.Length; i++)
            {
                if (nSwitchStatus[i] == MCDF.OFF)
                {
                    MotionMgr.Inst.SetOutput(Switch[i], false);
                }
                else if (nSwitchStatus[i] == MCDF.ON)
                {
                    MotionMgr.Inst.SetOutput(Switch[i], true);
                }
                else if (nSwitchStatus[i] == MCDF.BLINK)
                {
                    MotionMgr.Inst.SetOutput(Switch[i], m_Blink);
                }
            }
            if (bExactTime == false)
            {
                if (DateTime.Now.ToString("HH:mm:ss")== "00:00:00")
                {
                    //GbFunc.DeleteAllDB(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SEQ_LOG_EXP_PERIOD].nValue);
                    bExactTime = true;
                }
            }
            else
            {
                if (DateTime.Now.ToString("HH:mm:ss") == "11:59:59")
                {
                    bExactTime = false;
                }
            }
        }

        private void SetWarningMsg(int nWarnNo)
        {
            if (fWarning == null)
            {
                fWarning = new frmWarningMsg();
            }
            fWarning.SetMsg(GbVar.strWarnMsg[nWarnNo]);

            GbFunc.SetOut((int)IODF.OUTPUT.ERROR_BUZZER, true);

            fWarning.Show();
        }


        private void tmrStatus_Tick(object sender, EventArgs e)
        {
            return; //20220328 임시 주석
            OnIonizer();

            //TODO SORTER 
            //if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_PROGRAM_ON] != 1)
            //{
            //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SORTER_PROGRAM_ON, true);
            //}

            //if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_PROGRAM_ON] != 1)
            //{
            //    MotionMgr.Inst.SetOutput(IODF.OUTPUT.SORTER_PROGRAM_ON, true);
            //}
        }

        private void OnIonizer()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_1] == 1 || GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_2] == 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_1, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_2, false);
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ION_BAR_RUN_1] == 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BAR_RUN_1, false);
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ION_BLOWER_FAN_RUN_1] == 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BLOWER_FAN_RUN_1, false);
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.ION_BLOWER_FAN_RUN_2] == 1)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ION_BLOWER_FAN_RUN_2, false);
            }
        }

        private void ManualLotProcess(object obj)
        {
            try
            {

            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        #region 매거진 강제 배출 S/W관련 I/O

        /// <summary>
        /// // 매거진 컨베이어 배출 S/W 상태 확인
        /// </summary>
        /// <returns></returns>
        protected bool IsLdConvMgzOutSW()
        {
            int nAddr = (int)IODF.INPUT._1F_CONVEYOR_OUTPUT_SWITCH;

            return GbVar.GB_INPUT[nAddr] == 1;
        }

        protected bool IsUldConvMgzOutSW()
        {
            int nAddr = (int)IODF.INPUT._2F_CONVEYOR_OUTPUT_SWITCH;

            return GbVar.GB_INPUT[nAddr] == 1;
        }

        protected bool IsUldConvMgzInSW()
        {
            int nAddr = (int)IODF.INPUT._1F_CONVEYOR_INPUT_SWITCH;

            return GbVar.GB_INPUT[nAddr] == 1;
        }

        protected bool IsLdConvMgzInSW()
        {
            int nAddr = (int)IODF.INPUT._2F_CONVEYOR_INPUT_SWITCH;

            return GbVar.GB_INPUT[nAddr] == 1;
        }


        protected void SetLdConvMgzOutSW(bool bOn)
        {
            int nAddr = (int)IODF.OUTPUT._1F_CONVEYOR_OUTPUT_SWITCH_LED;

            MotionMgr.Inst.SetOutput(nAddr, bOn);
        }

        protected void SetUldConvMgzOutSW(bool bOn)
        {
            int nAddr = (int)IODF.OUTPUT._2F_CONVEYOR_OUTPUT_SWITCH_LED;

            MotionMgr.Inst.SetOutput(nAddr, bOn);
        }

        protected void SetLdConvMgzInSW(bool bOn)
        {
            int nAddr = (int)IODF.OUTPUT._1F_CONVEYOR_INPUT_SWITCH_LED;

            MotionMgr.Inst.SetOutput(nAddr, bOn);
        }

        protected void SetUldConvMgzInSW(bool bOn)
        {
            int nAddr = (int)IODF.OUTPUT._2F_CONVEYOR_INPUT_SWITCH_LED;

            MotionMgr.Inst.SetOutput(nAddr, bOn);
        }

        #endregion

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cbx  = (CheckBox)sender;
            GbVar.MANUAL_MOVE_CHECK_SKIP = cbx.Checked;
        }

        private void button_Logout_Click(object sender, EventArgs e)
        {

            popMessageBox msg = new popMessageBox(FormTextLangMgr.FindKey("Are you sure you want to LOGOUT ?"), "EXIT");
            msg.TopMost = true;

            if (msg.ShowDialog(this) == DialogResult.OK)
            {
                //tmrRefresh.Stop();
                //tmrFlicker.Stop();
                //tmrStatus.Stop();
                // 로그아웃 이벤트 기록
                GbFunc.WriteEventLog("LOGOUT SUCCESS", GbVar.CurLoginInfo.USER_ID + "_LOGOUT");

                ScreenChange(0);
            }

            //if (GbForm.pTest == null || GbForm.pTest.IsDisposed)
            //{
            //    GbForm.pTest = new popTest();
            //    GbForm.pTest.OnLotEnd += GbForm.fAuto.LotEnd;
            //    GbForm.pTest.OnLotStart += GbForm.fAuto.LotStart;
            //}

            //GbForm.pTest.Show();
        }

        void ChangeFormLang()
        {
            FormTextLangMgr.Load(this, PathMgr.Inst.FILE_PATH_LANGUAGE_UI);
            ErrMgr.Inst.Init();
        }

        private void label12_Click(object sender, EventArgs e)
        {
            //ConfigMgr.Inst.GetCamToPickerupPos(0, 100, 100);
            popMessageBox msg = new popMessageBox("커팅테이블에 소재를 넣었습니까 ?", "EXIT");
            msg.TopMost = true;

            if (msg.ShowDialog(this) == DialogResult.OK)
            {
                GbVar.Seq.sCuttingTable.Info.SetStrip(true);
            }
            else
            {
                GbVar.Seq.sCuttingTable.Info.SetStrip(false);
            }
        }
        /// <summary>
		///220609
        /// Lot, Its 정보 Validation함수
        /// </summary>
        /// <returns></returns>
        private bool LOTInfoValid()
        {
            bool bRet = true;

            popMessageBox pMsgStart = null;

            if (GbVar.lstBinding_HostLot == null || GbVar.lstBinding_HostLot.Count == 0)
            {
                pMsgStart = new popMessageBox(FormTextLangMgr.FindKey("ITS정보를 등록 후에 시작하십시오"), "경고!!", MessageBoxButtons.OK);
                pMsgStart.TopMost = true;
                if (pMsgStart.ShowDialog(this) == DialogResult.OK) return false;
            }
            else
            {
                if (GbVar.bFirstAutoStart)
                {
                    GbVar.bFirstAutoStart = false;

                    pMsgStart = new popMessageBox(
                        String.Format("LOT ID : {0}\r\n {1}",GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID, FormTextLangMgr.FindKey("등록하신 정보로 자동 운전을 시작하시겠습니까?")),
                        "경고!!", MessageBoxButtons.OKCancel);

                    pMsgStart.TopMost = true;
                    if (pMsgStart.ShowDialog(this) == DialogResult.Cancel) return false;
                }

                string strITSId = "";
                string strLotId = "";
                int nQty = 0;
                if (GbVar.dbITSInfo.isExsitInfo(ref strITSId, ref strLotId, ref nQty) == false)
                {
                    pMsgStart = new popMessageBox(
                    FormTextLangMgr.FindKey("등록된 ITS정보가 존재하지 않습니다. 재등록하십시오."),
                    "경고!!",
                    MessageBoxButtons.OK);

                    pMsgStart.TopMost = true;
                    if (pMsgStart.ShowDialog(this) == DialogResult.OK) return false;
                }
                else
                {
                    if (strLotId.Trim() != GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID)
                    {
                        pMsgStart = new popMessageBox(
                        FormTextLangMgr.FindKey("등록된 ITS ID 또는 LOT ID와 백업 데이터가\r\n일치하지 않습니다. 재등록 하십시오."),
                        "경고!!",
                        MessageBoxButtons.OK);

                        pMsgStart.TopMost = true;
                        if (pMsgStart.ShowDialog(this) == DialogResult.OK) return false;
                    }
                }
            }

            return bRet;
        }
    }
}
