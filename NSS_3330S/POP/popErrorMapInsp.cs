using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.POP
{
    public partial class popErrorMapInsp : Form
    {
        public int nErrNo = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        bool Expand = false;

        public popErrorMapInsp()
        {
            InitializeComponent();

            swTimeStamp.Stop();
            this.CenterToScreen();
        }

        public void SetErrorText(int nErrNo)
        {
            try
            {
                lblErrNo.Text = "E." + nErrNo.ToString();
                if (nErrNo < 0)
                {
                    tmrErr.Enabled = true;
                    swTimeStamp.Restart();
                }
                else
                {
                    // 매거진 전용 알람시에만 디폴트로 확장 사용
                    //Expand = false; 
                    //ExpandCompress(Expand);

                    ErrStruct err = new ErrStruct(nErrNo, 1);
                    ErrMgr.Inst.ErrStatus.Add(err);

                    ErrorInfo eInfo = ErrMgr.Inst.errlist[nErrNo];

                    lblTitle.Text = eInfo.NAME;
                    lblCause.Text = eInfo.CAUSE;
                    lblAction.Text = eInfo.SOLUTION;
                    tmrErr.Enabled = true;
                    swTimeStamp.Restart();

                    strAlarmText = eInfo.NAME;
                    if (addAlarmEvent != null)
                        addAlarmEvent(System.DateTime.Now.ToString("yyyyMMddHHmmssfff"), nErrNo, eInfo.NAME);

                    string strnow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    GbFunc.WriteErrorLog(eInfo.NAME, strnow, nErrNo);

                    int nBuzzer = 1;
                    //if (int.TryParse(eInfo.BUZZER.Substring(eInfo.BUZZER.Length - 1), out nBuzzer))
                    {
                        MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER + (nBuzzer - 1), true);
                    }

                    //string strImgName = GbVar.dbAlarmImg.LoadImageName(nErrNo);
                    //if (strImgName != "")
                    //{
                    //    picErrorImg.ImageLocation = "D:\\WORK\\Sk hynix\\NSK8000\\Source\\Handler\\NSK_8000S_20220225_1748_MC\\NSS_3330S\\NSS_3330S\\Resources\\Alarm\\" + strImgName;
                    //}
                }
            }
            catch (Exception)
            {

            }
        }

        public void SetErrorText(int nErrAxis, int nErrCode)
        {
            lblErrNo.Text = "E." + nErrCode.ToString();
            if (nErrNo < 0)
            {
                tmrErr.Enabled = true;
                swTimeStamp.Restart();
            }
            else
            {
                //GbFunc.WriteErrorLog(SVDF.GetAxisName(nErrAxis) + " ALARM", nErrCode);

                lblTitle.Text = SVDF.GetAxisName(nErrAxis) + " ALARM";

                tmrErr.Enabled = true;
                swTimeStamp.Restart();

                strAlarmText = lblTitle.Text;
                if (addAlarmEvent != null)
                    addAlarmEvent(System.DateTime.Now.ToString("hh:mm:ss tt"), nErrNo, lblTitle.Text);
            }

            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, true);
        }

        public void ResetError()
        {
            for (int i = 0; i < ErrMgr.Inst.ErrStatus.Count; i++)
            {
                ErrMgr.Inst.ErrStatus[i].Set = 0;
            }

            Thread.Sleep(500);

            //GbVar.mcState.isErr = false;

            strAlarmText = "";
            tmrErr.Enabled = false;
            swTimeStamp.Stop();

            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
            this.Invoke((MethodInvoker)delegate ()
            {
                Hide();
            });
        }

        private void tmrErr_Tick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", swTimeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Seconds));
            lblTime.DigitText = strElapsedTime;
            if (lblErrNo.ForeColor == Color.OrangeRed) { lblErrNo.ForeColor = Color.FromArgb(50, 50, 50); }
            else { lblErrNo.ForeColor = Color.OrangeRed; }

            switch (ConfigMgr.Inst.Cfg.General.nLanguage)
            {
                case (int)Language.CHINA:
                    {
                        if (btnRetryMapInsp.Text != "再检查") btnRetryMapInsp.Text = "再检查";
                        if (btnIgnoreError.Text != "继续") btnIgnoreError.Text = "继续";
                        if (btnScrap.Text != "移除") btnScrap.Text = "移除";
                        if (btnBuzzerStop.Text != "关闭报警") btnBuzzerStop.Text = "关闭报警";
                    }
                    break;
                case (int)Language.KOREAN:
                    {
                        if (btnRetryMapInsp.Text != "재검사") btnRetryMapInsp.Text = "재검사";
                        if (btnIgnoreError.Text != "진행") btnIgnoreError.Text = "진행";
                        if (btnScrap.Text != "스크랩") btnScrap.Text = "스크랩";
                        if (btnBuzzerStop.Text != "부저스탑") btnBuzzerStop.Text = "부저스탑";
                    }
                    break;
                case (int)Language.ENGLISH:
                    {
                        if (btnRetryMapInsp.Text != "RETRY") btnRetryMapInsp.Text = "RETRY";
                        if (btnIgnoreError.Text != "IGNORE") btnIgnoreError.Text = "IGNORE";
                        if (btnScrap.Text != "SCRAP") btnScrap.Text = "SCRAP";
                        if (btnBuzzerStop.Text != "BUZZER STOP") btnBuzzerStop.Text = "BUZZER STOP";
                    }
                    break;
                default:
                    break;
            }
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
        }

        private void btnRetryMapInsp_Click(object sender, EventArgs e)
        {
            // 재검사
            GbVar.Seq.sMapTransfer.bRetryMapInsp = true;

            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void btnIgnoreError_Click(object sender, EventArgs e)
        {
            // 강제 진행
            GbVar.Seq.sMapTransfer.bRetryMapInsp = false;

            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void btnScrap_Click(object sender, EventArgs e)
        {
            // 스크랩
            GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.Clear();

            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }
    }
}
