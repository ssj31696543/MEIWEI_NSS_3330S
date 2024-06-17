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
using System.Windows.Forms.DataVisualization.Charting;

namespace NSS_3330S.POP
{
    public partial class popLotEnd : Form
    {
        Point mouseLocation;
        public int m_nErrNo = 0;
        public STRIP_MDL m_eMdl = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        string sPnlCnt = "0";
        string sPnlQty = "0";
        string sLotId = "";
        string sLotType = "";

        public popLotEnd()
        {
            InitializeComponent();

            swTimeStamp.Stop();
            this.CenterToScreen();
        }
        private void lbTitle_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void lbTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePos;
            }
        }

        public void SetErrorText(int nErrNo)
        {
            try
            {
                switch ((Language)ConfigMgr.Inst.Cfg.General.nLanguage)
                {
                    case Language.ENGLISH:
                        lbTitle.Text = "LOT END";
                        btnBuzzerStop.Text = "BUZZER STOP";
                        break;
                    case Language.CHINA:
                        lbTitle.Text = "批次结束";
                        btnBuzzerStop.Text = "关闭提示";
                        break;
                    case Language.KOREAN:
                        lbTitle.Text = "랏 엔드";
                        btnBuzzerStop.Text = "부저끄기";
                        break;
                }

                m_nErrNo = nErrNo;

                ErrStruct err = new ErrStruct(nErrNo, 1);
                ErrMgr.Inst.ErrStatus.Add(err);


                ErrorInfo eInfo = ErrMgr.Inst.errlist[nErrNo];

                tmrWaitTime.Enabled = true;
                swTimeStamp.Restart();

                strAlarmText = eInfo.NAME;
                if (addAlarmEvent != null)
                    addAlarmEvent(System.DateTime.Now.ToString("yyyyMMddHHmmssfff"), nErrNo, eInfo.NAME);

                lblLotEnd.Text = "LOT END!";
                lbLotId.Text = "";
                if (GbVar.lstBinding_HostLot != null)
                {
                    if (GbVar.lstBinding_HostLot.Count > 0)
                    {
                        lbLotId.Text = string.Format("LOT ID : {0}", GbVar.lstBinding_HostLot[0].LOT_ID);
                    }
                }

                string strnow = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                GbFunc.WriteErrorLog(eInfo.NAME, strnow, nErrNo);

                MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, true);

                //상위 보고 및 생산 수량 로그 저장
                if (GbVar.lstBinding_EqpProc != null && GbVar.lstBinding_EqpProc.Count > 0)
                {
                    GbVar.lstBinding_EqpProc[MCDF.CURRLOT].EQP_OUT_TIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    sPnlQty = GbVar.lstBinding_EqpProc[MCDF.CURRLOT].STRIP_OUTPUT_COUNT.ToString();
                    GbVar.LotReportLog.AddStripLog(GbVar.lstBinding_HostLot[0].LOT_ID, true);

                    GbVar.LotMgr.InsertProcQtyLog(GbVar.lstBinding_EqpProc[MCDF.CURRLOT]);
                }
                if (GbVar.lstBinding_HostLot != null && GbVar.lstBinding_HostLot.Count > 0)
                {
                    sPnlCnt = GbVar.lstBinding_HostLot[MCDF.CURRLOT].SUB_QTY;
                    sLotId = GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID;
                    sLotType = GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_TYPE;
                }
           
            }
            catch (Exception)
            {

            }
        }

        public void ResetError()
        {
            for (int i = 0; i < ErrMgr.Inst.ErrStatus.Count; i++)
            {
                ErrMgr.Inst.ErrStatus[i].Set = 0;
            }

            strAlarmText = "";
            tmrWaitTime.Enabled = false;
            swTimeStamp.Stop();

            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            this.Invoke((MethodInvoker)delegate ()
            {
                Hide();
            });
        }

        private void tmrWaitTimeTick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", swTimeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Seconds));
            lblTime.DigitText = strElapsedTime;
            //if (lblLotEnd.ForeColor == Color.OrangeRed) { lblLotEnd.ForeColor = Color.FromArgb(50, 50, 50); }
            //else { lblLotEnd.ForeColor = Color.OrangeRed; }
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();
        }
    }
}
