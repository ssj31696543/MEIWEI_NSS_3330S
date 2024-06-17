using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;

namespace NSS_3330S.UC
{
    [DefaultProperty("AXIS")]
    public partial class ucSvManualJog : UserControl
    {
        SVDF.AXES _Axis = SVDF.AXES.MAGAZINE_ELV_Z;
        int nPadNo = 0;
        ButtonExJog[] m_btnJog;
        Color OnRed = Color.Red;
        Color OffRed = Color.FromArgb(64, 0, 0);
        Color OnBlue = Color.Blue;
        Color OffBule = Color.FromArgb(0, 0, 64);
        const int MOVE_MODE_JOG = 0;
        const int MOVE_MODE_INC = 1;
        const int MOVE_MODE_ABS = 2;

        bool m_bTmrRefreshFlag = false;

        public ucSvManualJog()
        {
            InitializeComponent();
            SetToolTip();
            m_btnJog = new ButtonExJog[] { btnExJogXn, btnExJogXp, btnExJogYn, btnExJogYp, btnExJogZn, btnExJogZp, btnExJogCCW, btnExJogCW };

            foreach (ButtonExJog btn in m_btnJog)
            {
                btn.MouseDown += OnManualJogButtonDown;
                btn.MouseUp += OnManualJogButtonUp;
            }

            cbPadNo.SelectedIndex = 0;
            tmrRefresh.Enabled = true;
        }

        public SVDF.AXES AXIS
        {
            get
            {
                return _Axis;
            }
            set
            {
                _Axis = SVDF.GetAxisByName(value.ToString());

                if ((SVDF.AXES)value >= SVDF.AXES.CHIP_PK_1_T_1 && (SVDF.AXES)value <= SVDF.AXES.CHIP_PK_2_Z_6)
                {
                    cbPadNo.Visible = true;
                }
                else
                {
                    cbPadNo.Visible = false;
                }

                //switch (value)
                //{
                //    case SVDF.AXES.DRY_BLOCK_STG_X:
                //    case SVDF.AXES.TRAY_PK_X:
                //    case SVDF.AXES.UNIT_PK_X:
                //        {
                //            //역방향
                //            tabJog.SelectedIndex = 0;
                //            btnExJogXn.JOG_DIR_REVERSE = false;
                //            btnExJogXp.JOG_DIR_REVERSE = true;
                //        }
                //        break;
                //    case SVDF.AXES.MAP_PK_X:
                //    case SVDF.AXES.CHIP_PK_1_X:
                //    case SVDF.AXES.CHIP_PK_2_X:
                //    case SVDF.AXES.LD_VISION_X:
                //    case SVDF.AXES.STRIP_PK_X:
                //        {
                //            //정방향
                //            tabJog.SelectedIndex = 0;
                //            btnExJogXn.JOG_DIR_REVERSE = true;
                //            btnExJogXp.JOG_DIR_REVERSE = false;
                //        }
                //        break;
                //    case SVDF.AXES.MAP_STG_1_Y:
                //    case SVDF.AXES.MAP_STG_2_Y:
                //    case SVDF.AXES.BALL_VISION_Y:
                //    case SVDF.AXES.TRAY_PK_Y:
                //    case SVDF.AXES.GD_TRAY_STG_1_Y:
                //    case SVDF.AXES.GD_TRAY_STG_2_Y:
                //    case SVDF.AXES.RW_TRAY_STG_Y:
                //    case SVDF.AXES.REJECT_BOX_Y:
                //    case SVDF.AXES.MGZ_CLAMP_ELV_Y:
                //    case SVDF.AXES.NONE:
                //    case SVDF.AXES.NONE:
                //    case SVDF.AXES.LD_RAIL_Y_FRONT:
                //        {
                //            //정방향
                //            tabJog.SelectedIndex = 1;
                //            btnExJogYn.JOG_DIR_REVERSE = true;
                //            btnExJogYp.JOG_DIR_REVERSE = false;
                //        }
                //        break;
                //    case SVDF.AXES.MAP_PK_Z:
                //    case SVDF.AXES.TRAY_PK_Z:
                //    case SVDF.AXES.CHIP_PK_1_Z_1:
                //    case SVDF.AXES.CHIP_PK_1_Z_2:
                //    case SVDF.AXES.CHIP_PK_1_Z_3:
                //    case SVDF.AXES.CHIP_PK_1_Z_4:
                //    case SVDF.AXES.CHIP_PK_1_Z_5:
                //    case SVDF.AXES.CHIP_PK_1_Z_6:
                //    case SVDF.AXES.CHIP_PK_1_Z_7:
                //    case SVDF.AXES.CHIP_PK_1_Z_8:
                //    case SVDF.AXES.CHIP_PK_1_Z_9:
                //    case SVDF.AXES.CHIP_PK_1_Z_10:
                //    case SVDF.AXES.CHIP_PK_2_Z_1:
                //    case SVDF.AXES.CHIP_PK_2_Z_2:
                //    case SVDF.AXES.CHIP_PK_2_Z_3:
                //    case SVDF.AXES.CHIP_PK_2_Z_4:
                //    case SVDF.AXES.CHIP_PK_2_Z_5:
                //    case SVDF.AXES.CHIP_PK_2_Z_6:
                //    case SVDF.AXES.CHIP_PK_2_Z_7:
                //    case SVDF.AXES.CHIP_PK_2_Z_8:
                //    case SVDF.AXES.CHIP_PK_2_Z_9:
                //    case SVDF.AXES.CHIP_PK_2_Z_10:
                //    case SVDF.AXES.MGZ_CLAMP_ELV_Z:
                //    case SVDF.AXES.NONE:
                //    case SVDF.AXES.STRIP_PK_Z:
                //    case SVDF.AXES.UNIT_PK_Z:
                //        {
                //            //역방향
                //            tabJog.SelectedIndex = 2;
                //            btnExJogZn.JOG_DIR_REVERSE = false;
                //            btnExJogZp.JOG_DIR_REVERSE = true;
                //        }
                //        break;
                //    case SVDF.AXES.BALL_VISION_Z:
                //    case SVDF.AXES.GD_TRAY_ELV_Z:
                //    case SVDF.AXES.RW_TRAY_ELV_Z:
                //    case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                //    case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                //    case SVDF.AXES.COVER_TRAY_ELV_Z:
                //    case SVDF.AXES.NONE:
                //        {
                //            //정방향
                //            tabJog.SelectedIndex = 2;
                //            btnExJogZn.JOG_DIR_REVERSE = true;
                //            btnExJogZp.JOG_DIR_REVERSE = false;
                //        }
                //        break;
                //    case SVDF.AXES.NONE:
                //        {
                //            //역방향
                //            tabJog.SelectedIndex = 3;
                //            btnExJogCCW.JOG_DIR_REVERSE = false;
                //            btnExJogCW.JOG_DIR_REVERSE = true;
                //        }
                //        break;
                //    case SVDF.AXES.CHIP_PK_1_T_1:
                //    case SVDF.AXES.CHIP_PK_1_T_2:
                //    case SVDF.AXES.CHIP_PK_1_T_3:
                //    case SVDF.AXES.CHIP_PK_1_T_4:
                //    case SVDF.AXES.CHIP_PK_1_T_5:
                //    case SVDF.AXES.CHIP_PK_1_T_6:
                //    case SVDF.AXES.CHIP_PK_1_T_7:
                //    case SVDF.AXES.CHIP_PK_1_T_8:
                //    case SVDF.AXES.CHIP_PK_1_T_9:
                //    case SVDF.AXES.CHIP_PK_1_T_10:
                //    case SVDF.AXES.CHIP_PK_2_T_1:
                //    case SVDF.AXES.CHIP_PK_2_T_2:
                //    case SVDF.AXES.CHIP_PK_2_T_3:
                //    case SVDF.AXES.CHIP_PK_2_T_4:
                //    case SVDF.AXES.CHIP_PK_2_T_5:
                //    case SVDF.AXES.CHIP_PK_2_T_6:
                //    case SVDF.AXES.CHIP_PK_2_T_7:
                //    case SVDF.AXES.CHIP_PK_2_T_8:
                //    case SVDF.AXES.CHIP_PK_2_T_9:
                //    case SVDF.AXES.CHIP_PK_2_T_10:
                //    case SVDF.AXES.LD_RAIL_T:
                //        {
                //            //정방향
                //            tabJog.SelectedIndex = 3;
                //            btnExJogCCW.JOG_DIR_REVERSE = true;
                //            btnExJogCW.JOG_DIR_REVERSE = false;
                //        }
                //        break;
                //    case SVDF.AXES.MAX:
                //    case SVDF.AXES.NONE:
                //    default:
                //        break;
                //}
            }
        }

        private void ucSvManualJog_Load(object sender, EventArgs e)
        {
            tabJog.ItemSize = new Size(0, 1);
            cbPadNo.SelectedIndex = (nPadNo / 2);
        }

        private void cbPadNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            nPadNo = (cbPadNo.SelectedIndex * 2);
        }
        private void OnManualJogButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            ButtonExJog btn = sender as ButtonExJog;
            Moving(btn);
        }

        void MoveJog(int nAxisNo, double dPos)
        {
            double dSpeed = GetSpeed(nAxisNo);

            if (MotionMgr.Inst[nAxisNo].GetDeviceType() == DionesTool.Motion.DeviceType.DEVICE_AJIN)
            {
                if (dPos > 0) MotionMgr.Inst[nAxisNo].JogPlus(dSpeed, dSpeed * 4.0, dSpeed * 4.0);
                else MotionMgr.Inst[nAxisNo].JogMinus(dSpeed, dSpeed * 4.0, dSpeed * 4.0);
            }
            else
            {
                if (dPos > 0) MotionMgr.Inst[nAxisNo].JogPlus(dSpeed, 200, 200);
                else MotionMgr.Inst[nAxisNo].JogMinus(dSpeed, 200, 200);
            }
        }

        void MoveInc(int nAxisNo, double dPos)
        {
            double dMovePos = 0.0;
            dMovePos = MotionMgr.Inst[nAxisNo].GetRealPos() + dPos;

            if (dMovePos < ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN)
            {
                MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Cannot move. Position less than S/W Minus Limit."));
                return;
            }

            if (dMovePos > ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP)
            {
                MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Cannot move. Position more than S/W Plus Limit."));
                return;
            }

            double dSpeed = GetSpeed(nAxisNo);
            MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dSpeed, dSpeed * 5, dSpeed * 5);
        }

        void MoveAbs(int nAxisNo, double dPos)
        {
            double dMovePos = 0.0;
            dMovePos = dPos;
            double dSpeed = GetAbsSpeed(nAxisNo);
            MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dSpeed, dSpeed * 5, dSpeed * 5);
        }

        void Moving(ButtonExJog btn)
        {
            if (GbVar.mcState.IsRun()) return;

            int nAxisNo = (int)_Axis;
            int nDir = btn.JOG_DIR_REVERSE ? -1 : 1;
            double dPos = 0.0;

            if (nAxisNo == (int)SVDF.AXES.CHIP_PK_1_T_1 || nAxisNo == (int)SVDF.AXES.CHIP_PK_1_Z_1)
            {
                nAxisNo += (cbPadNo.SelectedIndex * 2);
            }
            else if (nAxisNo == (int)SVDF.AXES.CHIP_PK_2_T_1 || nAxisNo == (int)SVDF.AXES.CHIP_PK_2_Z_1)
            {
                nAxisNo += (cbPadNo.SelectedIndex * 2);
            }

            if (!double.TryParse(tbxIncMove.Text, out dPos))
            {
                MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Distance value is invalidate."));
                return;
            }

            switch (tabJogMode.SelectedIndex)
            {
                case MOVE_MODE_JOG:
                    MoveJog(nAxisNo, nDir);
                    break;
                case MOVE_MODE_INC:
                    if (!double.TryParse(tbxIncMove.Text, out dPos))
                    {
                        MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Distance value is invalidate."));
                        return;
                    }
                    MoveInc(nAxisNo, dPos * nDir);
                    break;
                case MOVE_MODE_ABS:
                    if (!double.TryParse(tbxAbsMove.Text, out dPos))
                    {
                        MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Moving position is invalidate."));
                        return;
                    }
                    MoveAbs(nAxisNo, dPos);
                    break;
                default:
                    break;
            }
        }

        double GetSpeed(int nAxisNo)
        {
            double dSpeed = 0.0;
            switch (GetJogSpeedIndex())
            {
                case 0:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                    break;
                case 1:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                    break;
                case 2:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            return dSpeed;
        }

        double GetAbsSpeed(int nAxisNo)
        {
            double dSpeed = 0.0;
            switch (GetAbsSpeedIndex())
            {
                case 0:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
                    break;
                case 1:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
                    break;
                case 2:
                    dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
                    break;
                default:
                    System.Diagnostics.Debugger.Break();
                    break;
            }

            return dSpeed;
        }

        int GetJogSpeedIndex()
        {
            if (rbJogSpeedLow.Checked) return 0;
            else if (rbJogSpeedMiddle.Checked) return 1;
            else if (rbJogSpeedHigh.Checked) return 2;

            return -1;
        }
        int GetAbsSpeedIndex()
        {
            if (rdbAbsVelLow.Checked) return 0;
            else if (rdbAbsVelMid.Checked) return 1;
            else if (rdbAbsVelHigh.Checked) return 2;

            return -1;
        }

        private void OnIncValChangeButtonClick(object sender, EventArgs e)
        {
            Button button = sender as Button;

            int tag = 0;
            if (button == null || !int.TryParse(button.Tag as string, out tag))
            {
                MessageBox.Show(FormTextLangMgr.FindKey("Invalid tag."));
                return;
            }

            int nCurJogPage = tabJog.SelectedIndex;
            switch (tag)
            {
                case 0:
                    tbxIncMove.Text = "0.01";
                    break;
                case 1:
                    tbxIncMove.Text = "0.1";
                    break;
                case 2:
                    tbxIncMove.Text = "1";
                    break;
                default:
                    break;
            }
        }

        private void OnManualJogButtonUp(object sender, MouseEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            ButtonExJog btn = sender as ButtonExJog;
            int nAxisNo = (int)_Axis;

            if (tabJogMode.SelectedIndex == MOVE_MODE_JOG)
                MotionMgr.Inst[nAxisNo].MoveStop();

            btn.BackColor = Color.FromArgb(240, 240, 240);
        }

        private void SetToolTip()
        {
            this.toolTip1.ToolTipTitle = "라벨 설명";
            this.toolTip1.IsBalloon = true;
            this.toolTip1.SetToolTip(lbError, "에러 상태");
            this.toolTip1.SetToolTip(lbHomeComplete, "홈 완료 상태");
            this.toolTip1.SetToolTip(lbBusy, "BUSY 신호 상태");
            this.toolTip1.SetToolTip(lbHome, "홈센서 감지 상태");
            this.toolTip1.SetToolTip(lbMinus, "-리밋 감지 상태");
            this.toolTip1.SetToolTip(lbPlus, "+리밋 감지 상태");
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
#if _NOTEBOOK
            return;
#endif
            if (m_bTmrRefreshFlag) return;
            m_bTmrRefreshFlag = true;

            try
            {
                return; // 임시로 막아둠 220328
                if (GetJogSpeedIndex() == 0 && lbIncSpeed.Text != "저속") lbIncSpeed.Text = "저속";
                else if (GetJogSpeedIndex() == 1 && lbIncSpeed.Text != "중속") lbIncSpeed.Text = "중속";
                else if (GetJogSpeedIndex() == 2 && lbIncSpeed.Text != "고속") lbIncSpeed.Text = "고속"; 

                lbError.BackColor = MotionMgr.Inst[(int)_Axis].IsAlarm() ? OnRed : OffRed;
                lbHomeComplete.BackColor = MotionMgr.Inst[(int)_Axis].GetHomeResult() == 0 ? OnBlue : OffBule;
                lbBusy.BackColor = MotionMgr.Inst[(int)_Axis].IsBusy() ? OnBlue : OffBule;
                lbHome.BackColor = MotionMgr.Inst[(int)_Axis].IsHome() ? OnBlue : OffBule;
                lbMinus.BackColor = MotionMgr.Inst[(int)_Axis].IsMinusLimit() ? OnRed : OffRed;
                lbPlus.BackColor = MotionMgr.Inst[(int)_Axis].IsPlusLimit() ? OnRed : OffRed;

                lbCurPos.Text = MotionMgr.Inst[(int)_Axis].GetRealPos().ToString("0.000");

                foreach (Control item in tabJog.TabPages[tabJog.SelectedIndex].Controls)
                {
                    if (item.GetType() == typeof(ButtonExJog))
                    {
                        ButtonExJog btn = item as ButtonExJog;
                        int nAxis = (int)btn.AXIS;

                        if (MotionMgr.Inst.MOTION != null)
                        {
                            // + 방향
                            if (btn.JOG_DIR_REVERSE == false)
                            {
                                #region + 상태 표시
                                if (MotionMgr.Inst[nAxis].IsAlarm())
                                {
                                    if (item.BackColor != Color.Maroon)
                                        item.BackColor = Color.Maroon;
                                }
                                else if (MotionMgr.Inst[nAxis].IsPlusLimit())
                                {
                                    if (item.BackColor != Color.Red)
                                        item.BackColor = Color.Red;
                                }
                                else
                                {
                                    if (item.BackColor != Color.FromArgb(240, 240, 240))
                                        item.BackColor = Color.FromArgb(240, 240, 240);
                                }
                                #endregion
                            }
                            // - 방향
                            else
                            {
                                #region - 상태 표시
                                if (MotionMgr.Inst[nAxis].IsAlarm())
                                {
                                    if (item.BackColor != Color.Maroon)
                                        item.BackColor = Color.Maroon;
                                }
                                else if (MotionMgr.Inst[nAxis].IsMinusLimit())
                                {
                                    if (item.BackColor != Color.Red)
                                        item.BackColor = Color.Red;
                                }
                                else
                                {
                                    if (item.BackColor != Color.FromArgb(240, 240, 240))
                                        item.BackColor = Color.FromArgb(240, 240, 240);
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            m_bTmrRefreshFlag = false;
        }
    }
}
