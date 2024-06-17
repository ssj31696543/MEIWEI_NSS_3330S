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
    public partial class popStripExist : Form
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

        public popStripExist()
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

        public void SetErrorText(int nErrNo, STRIP_MDL eMdl)
        {
            try
            {
                m_nErrNo = nErrNo;
                m_eMdl = eMdl;
                if (nErrNo < 0)
                {
                    tmrErr.Enabled = true;
                    swTimeStamp.Restart();
                }
                else
                {
                    
                    btnYes.Text = "YES(RETRY)";
                    btnNo.Text = "NO(SCRAP)";

                    lblErrNo.Text = String.Format("{0} VAC ALARM OCCURRED!", m_eMdl.ToString().Replace('_',' '));
                    lblErrNo.Text += "\r\nDO YOU WANT TO RETRY PICKUP OR PLACE?";

                    switch (ConfigMgr.Inst.Cfg.General.nLanguage)
                    {
                        case (int)Language.CHINA:
                            {
                                if (btnYes.Text != "是的（重试)") btnYes.Text = "是的（重试)";
                                if (btnNo.Text != "关闭（移除)") btnNo.Text = "关闭（移除)";

                                lblErrNo.Text = String.Format("{0} 真空异常报警", ((STRIP_MDL_CHINA)m_eMdl).ToString());
                                lblErrNo.Text += "\r\n您要重新拾取或放置吗？?";
                                if (btnBuzzerStop.Text != "关闭报警") btnBuzzerStop.Text = "关闭报警";
                            }
                            break;
                        case (int)Language.KOREAN:
                            {
                                if (btnYes.Text != "재검사") btnYes.Text = "재검사";
                                if (btnNo.Text != "진행") btnNo.Text = "진행";

                                lblErrNo.Text = String.Format("{0} VAC ALARM OCCURRED!", m_eMdl.ToString().Replace('_', ' '));
                                lblErrNo.Text += "\r\n다시 픽업하거나 안착하시겠습니까?";
                                if (btnBuzzerStop.Text != "부저스탑") btnBuzzerStop.Text = "부저스탑";
                            }
                            break;
                        case (int)Language.ENGLISH:
                            {
                                if (btnYes.Text != "RETRY") btnYes.Text = "RETRY";
                                if (btnNo.Text != "IGNORE") btnNo.Text = "IGNORE";

                                lblErrNo.Text = String.Format("{0} VAC ALARM OCCURRED!", m_eMdl.ToString().Replace('_', ' '));
                                lblErrNo.Text += "\r\nDO YOU WANT TO RETRY PICKUP OR PLACE?";
                                if (btnBuzzerStop.Text != "BUZZER STOP") btnBuzzerStop.Text = "BUZZER STOP";
                            }
                            break;
                        default:
                            break;
                    }

                    ErrStruct err = new ErrStruct(nErrNo, 1);
                    ErrMgr.Inst.ErrStatus.Add(err);

                    ErrorInfo eInfo = ErrMgr.Inst.errlist[nErrNo];

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
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            //자재 정보 스크랩
            //GbFunc.StripScrap(m_eMdl);

            #region 수세기 스크랩 작업 
            switch (m_eMdl)
            {
                case STRIP_MDL.NONE:
                    break;
                case STRIP_MDL.STRIP_RAIL:
                    GbFunc.StripScrap(STRIP_MDL.STRIP_RAIL);
                    break;
                case STRIP_MDL.STRIP_TRANSFER:
                    GbFunc.StripScrap(STRIP_MDL.STRIP_TRANSFER);
                    break;
                case STRIP_MDL.CUTTING_TABLE:
                    GbFunc.StripScrap(STRIP_MDL.CUTTING_TABLE);
                    break;
                case STRIP_MDL.UNIT_TRANSFER:
                    GbFunc.StripScrap(STRIP_MDL.UNIT_TRANSFER);
                    break;
                case STRIP_MDL.UNIT_DRY:
                    GbFunc.StripScrap(STRIP_MDL.UNIT_DRY);
                    break;
                case STRIP_MDL.MAP_TRANSFER:
                    GbFunc.StripScrap(STRIP_MDL.MAP_TRANSFER);
                    break;
                case STRIP_MDL.MAP_VISION_TABLE_1:
                    GbFunc.StripScrap(STRIP_MDL.MAP_TRANSFER);
                    break;
                case STRIP_MDL.MAP_VISION_TABLE_2:
                    GbFunc.StripScrap(STRIP_MDL.MAP_TRANSFER);
                    break;
                case STRIP_MDL.MAX:
                    break;
                default:
                    break;
            }
            #endregion
            if (OnResetEvent != null)
                OnResetEvent();
            ResetError();

            popMessageBox msg = new popMessageBox(ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA ? "请取出设备内素材，并关闭真空" : "eturn off vacuum.", "INFO", MessageBoxButtons.OK);
            msg.ShowDialog();
        }
    }
}
