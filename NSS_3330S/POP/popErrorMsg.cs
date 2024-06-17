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
    public partial class popErrorMsg : Form
    {
        public int nErrNo = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        bool Expand = false;

        public popErrorMsg()
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
                    Expand = false;
                    ExpandCompress(Expand);

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
                    //트레이 만재 알람, EMPTY 트레이 소진 알람 에러 소리 변경
                    if (nErrNo == (int)ERDF.E_MGZ_LD_ELV_ALL_EXHAUSTED ||
                        nErrNo == (int)ERDF.E_THERE_IS_NO_TRAY ||
                        ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_GD_1_ELV_RESIDUAL_DETECT_FAIL ||
                        ErrMgr.Inst.ErrStatus[ErrMgr.Inst.ErrStatus.Count - 1].No == (int)ERDF.E_GD_2_ELV_RESIDUAL_DETECT_FAIL)
                    {
                        nBuzzer = 2;
                    }
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
                    addAlarmEvent( System.DateTime.Now.ToString("hh:mm:ss tt"), nErrNo,lblTitle.Text);
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
            this.Invoke((MethodInvoker)delegate()
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
        }

        private void buttonClose_EMsg_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
        }

        private void btnExpComp_Click(object sender, EventArgs e)
        {
            Expand = !Expand; 
            ExpandCompress(Expand);
        }

        private void ExpandCompress(bool beExpand)
        {
            if (beExpand == true)
            {
                Size = new Size(1085, 654);
                buttonClose_EMsg.Location = new System.Drawing.Point(278, 352);
                btnBuzzerStop.Location = new System.Drawing.Point(544, 352);
                lblTitle.Size = new System.Drawing.Size(922, 44);
                lblTime.Location = new System.Drawing.Point(443, 88);

                lblCause.Visible = true;
                lblAction.Visible = true;
                pnlCause.Visible = true;
                pnlSolution.Visible = true;

                btnExpComp.Location = new System.Drawing.Point(1026, 396);
                btnExpComp.ImageIndex = 1;
            }
            else
            {
                Size = new Size(580, 350);
                buttonClose_EMsg.Location = new System.Drawing.Point(20, 14);
                btnBuzzerStop.Location = new System.Drawing.Point(286, 14);
                lblTitle.Size = new System.Drawing.Size(417, 44);
                lblTime.Location = new System.Drawing.Point(199, 88);

                lblCause.Visible = false;
                lblAction.Visible = false;
                pnlCause.Visible = false;
                pnlSolution.Visible = false;

                btnExpComp.Location = new System.Drawing.Point(519, 86);
                btnExpComp.ImageIndex = 0;

            }
        }
    }
}
