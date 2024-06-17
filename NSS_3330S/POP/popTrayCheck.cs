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
    public partial class popTrayCheck : Form
    {
        Point mouseLocation;
        public int m_nErrNo = 0;
        public string strAlarmText = "";
        private Stopwatch swTimeStamp = Stopwatch.StartNew();

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;
        public delegate void addCurAlarmEvent(string strDateTime, int nErr, string strAlarm);
        public event addCurAlarmEvent addAlarmEvent;

        public popTrayCheck()
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

        public void ResetError()
        {
            for (int i = 0; i < ErrMgr.Inst.ErrStatus.Count; i++)
            {
                ErrMgr.Inst.ErrStatus[i].Set = 0;
            }

            Thread.Sleep(200);

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
        }

        private void btnBuzzerStop_Click(object sender, EventArgs e)
        {
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.END_BUZZER, false);
            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.ERROR_BUZZER, false);
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();

            popMessageBox msg = new popMessageBox("Please put the tray in the tray table yourself.", "INFO", MessageBoxButtons.OK);
            msg.ShowDialog();
            GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray = false;
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();

            GbVar.Seq.sUldTrayTransfer.bTrayPopErrSelectNewTray = true;
        }
    }
}
