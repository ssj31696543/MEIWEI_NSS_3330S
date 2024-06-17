using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace NSS_3330S.POP
{
    public partial class popMesErrorMsg : Form
    {
        public int nErrNo = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        bool Expand = false;

        public popMesErrorMsg()
        {
            InitializeComponent();

            swTimeStamp.Stop();
            this.CenterToScreen();
        }

        public void SetErrorText(int nErrNo)
        {
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

                lblTitle.Text = "E." + nErrNo + " " + eInfo.NAME;
                lblCause.Text = eInfo.CAUSE;
                lblAction.Text = eInfo.SOLUTION;
                tmrErr.Enabled = true;
                swTimeStamp.Restart();

                strAlarmText = eInfo.NAME;
                if (addAlarmEvent != null)
                    addAlarmEvent(System.DateTime.Now.ToString("yyyyMMddHHmmssfff"), nErrNo, eInfo.NAME);

                string strnow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                GbFunc.WriteErrorLog(eInfo.NAME, strnow, nErrNo);

                int nBuzzer = 0;
                if(int.TryParse(eInfo.BUZZER.Substring(eInfo.BUZZER.Length -1), out nBuzzer))
                { 
                    //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.MELODY_BUZZER_1 + (nBuzzer - 1), true); 
                }
            }
        }

        public void SetErrorText(int nErrAxis, int nErrCode)
        {
            if (nErrNo < 0)
            {
                tmrErr.Enabled = true;
                swTimeStamp.Restart();
            }
            else
            {
                //GbFunc.WriteErrorLog(SVDF.GetAxisName(nErrAxis) + " ALARM", nErrCode);

                lblTitle.Text = "E." + nErrCode + " " + SVDF.GetAxisName(nErrAxis) + " ALARM";
                
                tmrErr.Enabled = true;
                swTimeStamp.Restart();

                strAlarmText = lblTitle.Text;
                if (addAlarmEvent != null)
                    addAlarmEvent( System.DateTime.Now.ToString("hh:mm:ss tt"), nErrNo,lblTitle.Text);
            }
 
            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.O_TOWER_LAMP_ERR_BZ, true);
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

            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.O_TOWER_LAMP_ERR_BZ, false);
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
        }

        private void buttonClose_EMsg_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            //MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.O_TOWER_LAMP_ERR_BZ, false);
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
                Size = new Size(601, 445);
                lblAction.Visible = true;
                buttonClose_EMsg.Location = new System.Drawing.Point(87, 342);
                btnBuzzerStop.Location = new System.Drawing.Point(368, 342);
                btnExpComp.Location = new System.Drawing.Point(26, 350);
                btnExpComp.ImageIndex = 0;
            }
            else
            {
                Size = new Size(601, 266);
                lblAction.Visible = false;
                buttonClose_EMsg.Location = new System.Drawing.Point(87, 167);
                btnBuzzerStop.Location = new System.Drawing.Point(368, 167);
                btnExpComp.Location = new System.Drawing.Point(26, 175);
                btnExpComp.ImageIndex = 1;
            }
        }
    }
}
