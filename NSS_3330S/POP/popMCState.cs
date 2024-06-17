using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.UC;
using Owf.Controls;
using System.Runtime.InteropServices;

namespace NSS_3330S.POP
{
    public partial class popMCState : Form
    {

        #region Move Window Form Without Tilte Bar (드래그로 창 움직이기)
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd,
                         int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        private void pnlTitleBar_MouseDown(object sender,
        System.Windows.Forms.MouseEventArgs e)
        {
#if NOTEBOOK
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
#else
			return;
#endif
        }
        #endregion

        List<InputLabel> m_lbInput = new List<InputLabel>();

        bool m_bFlagTmrShake = false;
        bool m_bStartShake = false;
        int m_nShakeCnt = 0;
        int m_nPosOrg = 0;
        bool b_Update = false;

        private Stopwatch swTimeStamp = Stopwatch.StartNew();
        Owf.Controls.DigitalDisplayControl lblTime;

        public delegate void ResetDelegate();
        public event ResetDelegate OnResetEvent;

        public popMCState()
        {
            InitializeComponent();

            lblTime = new DigitalDisplayControl();
            lblTime.Dock = DockStyle.Fill;
            lblTime.DigitColor = Color.Red;
            tableLayoutPanel4.Controls.Add(lblTime);
            tableLayoutPanel4.Refresh();
            m_lbInput.Clear();
            FindInputLabel(pnlMain.Controls);
            UpdateLabelStatus();
        }

        void FindInputLabel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(InputLabel))
                    {
                        m_lbInput.Add((InputLabel)ctr);
                    }

                    if (ctr.Controls.Count > 0)
                        FindInputLabel(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                throw ex;
            }
        }

        void UpdateLabelStatus()
        {
            if (b_Update) return;
            b_Update = true;

            foreach (InputLabel item in m_lbInput)
            {
                switch (item.Input)
                {
                    //A접점 EMO
                    case IODF.INPUT.EMO_SWITCH_1:
                    case IODF.INPUT.EMO_SWITCH_2:
                    case IODF.INPUT.EMO_SWITCH_3:
                    case IODF.INPUT.EMO_FRONT_SWITCH_SAW:
                    case IODF.INPUT.EMO_REAR_SWITCH_SAW:
                        {
                            if (GbVar.GB_INPUT[(int)item.Input] == 1)
                            {
                                item.On = true;
                            }
                            else
                            {
                                item.On = false;
                            }
                        }
                        break;
                    //A접점 DOOR
                    case IODF.INPUT.FORNT_MONITOR_DOOR:
                    case IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR:
                    case IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR:
                    case IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR:
                    case IODF.INPUT.RT_SIDE_LF_DOOR:
                    case IODF.INPUT.RT_SIDE_CENTER_DOOR:
                    case IODF.INPUT.RT_SIDE_RT_DOOR:
                    case IODF.INPUT.REAR_LF_DOOR:
                    case IODF.INPUT.REAR_CENTER_DOOR:
                    case IODF.INPUT.REAR_RT_DOOR:
                    case IODF.INPUT.MC_31:

                        {
                            if (GbVar.GB_INPUT[(int)item.Input] == 1)
                            {
                                item.On = true;
                            }
                            else
                            {
                                item.On = false;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            b_Update = false;
        }

        private void tmrErr_Tick(object sender, EventArgs e)
        {
            string strElapsedTime = String.Format("{0:00}", swTimeStamp.Elapsed.Hours) + ":";
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Minutes) + ":");
            strElapsedTime += (String.Format("{0:00}", swTimeStamp.Elapsed.Seconds));
            lblTime.DigitText = strElapsedTime;
            UpdateLabelStatus();
            UpdateButton();
        }

        private void popMCState_Load(object sender, EventArgs e)
        {
            m_nPosOrg = txtPass.Left;
        }

        private void popMCState_FormClosed(object sender, FormClosedEventArgs e)
        {
            GbVar.gbPopMCState = false;
        }

        private void popMCState_VisibleChanged(object sender, EventArgs e)
        {
            tmrErr.Enabled = this.Visible;
            tmrShake.Enabled = this.Visible;

            if (this.Visible)
            {
                swTimeStamp.Restart();
            }
        }

        private void btnWindowStateMinimum_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnPasswordOK_Click(object sender, EventArgs e)
        {
            if (FNC.IsSuccess(GbFunc.IsDoorOpenOrPressEmo(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)))
            {
                popCheckPassword chk = new popCheckPassword();
                chk.TopMost = true;
                if (chk.ShowDialog(this) != DialogResult.OK) return;

                //if (txtPass.Text == "1234")
                //{
                if (OnResetEvent != null)
                    OnResetEvent();

                ResetError();

                //}
                //else
                //{
                //    txtPass.Focus();
                //    m_nShakeCnt = 0;
                //    m_bStartShake = true;
                //}
            }
            else MessageBox.Show("Check the state of door ");
        }

        public void ResetError()
        {
            tmrShake.Stop();
            tmrErr.Stop();

            swTimeStamp.Stop();

            this.Invoke((MethodInvoker)delegate()
            {
                Hide();
            });
        }

        private void tmrShake_Tick(object sender, EventArgs e)
        {
            if (m_bFlagTmrShake) return;
            m_bFlagTmrShake = true;

            try
            {
                if (m_bStartShake)
                {
                    if (m_nShakeCnt % 2 == 0)
                        txtPass.Left = m_nPosOrg - 1;
                    else
                        txtPass.Left = m_nPosOrg + 1;

                    m_nShakeCnt++;

                    if (m_nShakeCnt > 10)
                    {
                        m_bStartShake = false;
                        m_nShakeCnt = 0;

                        txtPass.Left = m_nPosOrg;
                    }
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
            finally
            {
                m_bFlagTmrShake = false;
            }
        }

        private void lblTitle_DoubleClick(object sender, EventArgs e)
        {
            //컨트롤 누르고 더블클릭시 창 최소화
            if (GbVar.mcState.IsRun()) return;
            //if (GbVar.nLoggingUserLevel != MCDF.LEVEL_MAK) return;
            if (!ModifierKeys.HasFlag(Keys.Control)) return;

            this.WindowState = FormWindowState.Minimized;
        }

        private void btnLoaderDoorLock_Click(object sender, EventArgs e)
        {
            // MC RUN 확인
            if (GbVar.mcState.IsRun()/* || GbThread.mnThread.m_bManualRun*/)
            {
                popMessageBox msg = new popMessageBox("장비가 가동중입니다.", "경고");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;
                return;
            }

            // SAW RUN 확인

            // DOOR CLOSE 확인
            if (SafetyMgr.Inst.GetSafetyDoorLoader() != FNC.SUCCESS)
            {
                if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0)
                {
                    popMessageBox msg = new popMessageBox("닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                    msg.TopMost = true;
                    if (msg.ShowDialog(this) != DialogResult.OK) return;
                    return;
                }
            }

            // KEY 확인

            // DOOR LOCK or UNLOCK
            bool bIsOn = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0;
            GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, bIsOn);
        }

        private void btnSorterDoorLock_Click(object sender, EventArgs e)
        {
            // MC RUN 확인
            if (GbVar.mcState.IsRun()/* || GbThread.mnThread.m_bManualRun*/)
            {
                popMessageBox msg = new popMessageBox("장비가 가동중입니다.", "경고");
                msg.TopMost = true;
                if (msg.ShowDialog(this) != DialogResult.OK) return;
                return;
            }

            // SAW RUN 확인

            // DOOR CLOSE 확인
            if (SafetyMgr.Inst.GetSafetyDoorSorter() != FNC.SUCCESS)
            {
                if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0)
                {
                    popMessageBox msg = new popMessageBox("닫히지 않은 도어가 있습니다 확인해주세요.", "경고");
                    msg.TopMost = true;
                    if (msg.ShowDialog(this) != DialogResult.OK) return;
                    return;
                }
            }

            // KEY 확인

            // DOOR LOCK or UNLOCK
            bool bIsOn = GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0;
            GbFunc.SetOut(IODF.OUTPUT.DOOR_LOCK_SIGNAL, bIsOn);
        }

        private void pnlMain_DoubleClick(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            //ConfigMgr.Inst.Cfg.General.isDoorAlarmSkip = true;
        }

        void UpdateButton()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 1 && btnSorterDoorLock.BackColor != Color.Lime)
                btnSorterDoorLock.BackColor = Color.Lime;
            else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] == 0 && btnSorterDoorLock.BackColor != SystemColors.Control)
                btnSorterDoorLock.BackColor = SystemColors.Control;

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 1 && btnLoaderDoorLock.BackColor != Color.Lime)
                btnLoaderDoorLock.BackColor = Color.Lime;
            else if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] == 0 && btnLoaderDoorLock.BackColor != SystemColors.Control)
                btnLoaderDoorLock.BackColor = SystemColors.Control;
        }


    }
}
