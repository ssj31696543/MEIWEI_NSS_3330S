using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;
using NSS_3330S.UC;

namespace NSS_3330S.POP
{
    public partial class popJog : Form
    {
        Point mouseLocation;
        List<ButtonExJog> m_listBtnJog = new List<ButtonExJog>();
        List<LabelExJogPos> m_listLabelPos = new List<LabelExJogPos>();
        List<LabelExJogPos> m_listLabelPosLatch = new List<LabelExJogPos>();
        List<double> m_dLatchPos = new List<double>();

        const int MOVE_MODE_JOG = 0;
        const int MOVE_MODE_INC = 1;
        const int MOVE_MODE_ABS = 2;

        bool m_bTmrRefreshFlag = false;

        bool m_bIsLatch = false;

        public popJog()
        {
            InitializeComponent();

            this.CenterToScreen();
            tabJog.ItemSize = new System.Drawing.Size(0, 1);

            FindJogButton(tabJog.Controls);
            FindPositionLabel(tabJog.Controls);

            tabPicker1.ItemSize = new Size(0, 1);
            tabPk2Pad.ItemSize = new Size(0, 1);

            foreach (ButtonExJog btn in m_listBtnJog)
            {
                btn.MouseDown += OnManualJogButtonDown;
                btn.MouseUp += OnManualJogButtonUp;
            }

            foreach (LabelExJogPos item in m_listLabelPosLatch)
            {
                item.Visible = false;
            }

            m_dLatchPos = new List<double>();
            for (int i = 0; i < (int)SVDF.AXES.MAX; i++)
            {
                m_dLatchPos.Add(0.0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Hide();
            //Close();
        }

        private void lbJogTitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void lbJogTitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePos;
            }
        }

        private void rdoLoader_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = sender as RadioButton;
            if (!rdo.Checked) return;

            int nTag;
            if (!int.TryParse(rdo.Tag.ToString(), out nTag)) return;

            tabJog.SelectedIndex = nTag;
        }

        void FindPositionLabel(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(LabelExJogPos))
                    {
                        LabelExJogPos lbl = ctr as LabelExJogPos;
                        if (lbl.Name.Contains("Latch")) m_listLabelPosLatch.Add(lbl);
                        else  m_listLabelPos.Add(lbl);
                       
                    }

                    if (ctr.Controls.Count > 0)
                        FindPositionLabel(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
            }
        }

        void FindJogButton(Control.ControlCollection collection)
        {
            try
            {
                foreach (Control ctr in collection)
                {
                    if (ctr.GetType() == typeof(ButtonExJog))
                    {
                        m_listBtnJog.Add((ButtonExJog)ctr);
                    }

                    if (ctr.Controls.Count > 0)
                        FindJogButton(ctr.Controls);
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                throw ex;
            }
        }

        private void OnManualJogButtonDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            ButtonExJog btn = sender as ButtonExJog;
            Moving(btn);
        }

        private void OnManualJogButtonUp(object sender, MouseEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            ButtonExJog btn = sender as ButtonExJog;
            int nAxisNo = (int)btn.AXIS;

            if (tabJogMode.SelectedIndex == MOVE_MODE_JOG)
                MotionMgr.Inst[nAxisNo].MoveStop();

            btn.BackColor = Color.FromArgb(240, 240, 240);
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

        /// <summary>
        /// 220428 pjh 알람명 수정 및 기본 메시지 팝업에서 popMessageBox로 수정 
        /// </summary>
        /// <param name="nAxisNo"></param>
        /// <param name="dPos"></param>
        void MoveInc(int nAxisNo, double dPos)
        {
            string strErr = "구동 에러";

            double dMovePos = 0.0;
            dMovePos = MotionMgr.Inst[nAxisNo].GetRealPos() + dPos;

            if (dMovePos < ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN)
            {
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W - Limit 보다 작은 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람");
                if (messageBox.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (dMovePos > ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP)
            {
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W + Limit 보다 큰 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람");
                if (messageBox.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
            }

            double dVel = 0.0;

            if (GetJogSpeedIndex() == 0) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
            else if (GetJogSpeedIndex() == 1) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
            else if (GetJogSpeedIndex() == 2) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
            else return;

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisNo, dMovePos);
            if (FNC.IsErr(nSafety))
            {
                strErr = "구동 에러";

                if (Enum.IsDefined(typeof(ERDF), nSafety)) strErr = ((ERDF)nSafety).ToString();

                popMessageBox messageBox = new popMessageBox(FormTextLangMgr.FindKey("구동 인터락!!!") + strErr, "알람");
                messageBox.ShowDialog();
                return;
            }

            MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dVel, dVel * 4, dVel * 4);
        }

        void MoveAbs(int nAxisNo, double dPos)
        {
            double dMovePos = 0.0;
            dMovePos = dPos;

            double dVel = 0.0;

            if (rdbVelLow.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
            else if (rdbVelMid.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
            else if (rdbVelHigh.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
            else return;

            MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dVel, dVel * 4, dVel * 4);
        }

        void Moving(ButtonExJog btn)
        {
            if (GbVar.mcState.IsRun()) return;

            int nAxisNo = (int)btn.AXIS;
            int nDir = btn.JOG_DIR_REVERSE ? -1 : 1;
            double dPos = 0.0;

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
                    if (Chk_Q.Checked)// 220406 17:00 KTH
                    {
                        string str_msg = string.Format("{0} 축을 {1} 위치로 이동하시겠습니까?\n ",
                            SVDF.GetAxisName(nAxisNo),tbxAbsMove.Text);
                        string str_title = "주의";
                        popMessageBox messageBox = new popMessageBox(str_msg, str_title);
                        if(messageBox.ShowDialog() == DialogResult.Cancel)
                        {
                            return;
                        }
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

        int GetJogSpeedIndex()
        {
            if (rbJogSpeedLow.Checked) return 0;
            else if (rbJogSpeedMiddle.Checked) return 1;
            else if (rbJogSpeedHigh.Checked) return 2;

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
                    tbxIncMove.Text = "0.001";
                    break;
                case 1:
                    tbxIncMove.Text = "0.01";
                    break;
                case 2:
                    tbxIncMove.Text = "0.1";
                    break;
                case 3:
                    tbxIncMove.Text = "1";
                    break;
                default:
                    break;
            }
        }

        private void btnMotionMovingStop_MouseDown(object sender, MouseEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            btnMotionMovingStop.BackColor = Color.Chocolate;
            MotionMgr.Inst.AllStop();
        }

        private void btnMotionMovingStop_MouseUp(object sender, MouseEventArgs e)
        {
            btnMotionMovingStop.BackColor = Color.White;
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            ////JOG 열어놓고 5분동안 조작 없으면 창 닫힘
            //if (tmrElapsedTime.Elapsed.TotalMinutes >= 5)
            //{
            //    this.Close();
            //}
#if _NOTEBOOK
            return;
#endif
            if (m_bTmrRefreshFlag) return;
            m_bTmrRefreshFlag = true;

            try
            {
                //// CCLINK I/O 표시
                //foreach (UcManualOutputButtonCcl item in m_listOuputButton)
                //{
                //    if (item.Output >= 0 && item.Output < IODF_CCL.OUTPUT.MAX)
                //    {
                //        item.On = GbVar.GB_CCLINK_OUTPUT[(int)item.Output] == 1;
                //    }
                //}

                // 위치 값 표시
                foreach (LabelExJogPos lbl in m_listLabelPos)
                {
                    if (lbl != null)
                    {
                        if (lbl.AXIS != SVDF.AXES.NONE)
                        {
                            lbl.RefreshUI();
                        }
                        
                    }
                }

                foreach (var item in GetAll(tabJog.TabPages[tabJog.SelectedIndex], typeof(ButtonExJog)))
                {
                    //if (item.GetType() == ButtonExJog)
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
                                    if (item.BackColor != Color.Red)
                                        item.BackColor = Color.Red;
                                }
                                else if (MotionMgr.Inst[nAxis].IsPlusLimit())
                                {
                                    if (item.BackColor != Color.OrangeRed)
                                        item.BackColor = Color.OrangeRed;
                                }
                                else
                                {
                                    if (item.BackColor != SystemColors.ButtonFace)
                                        item.BackColor = SystemColors.ButtonFace;
                                }
                                #endregion
                            }
                            // - 방향
                            else
                            {
                                #region - 상태 표시
                                if (MotionMgr.Inst[nAxis].IsAlarm())
                                {
                                    if (item.BackColor != Color.Red)
                                        item.BackColor = Color.Red;
                                }
                                else if (MotionMgr.Inst[nAxis].IsMinusLimit())
                                {
                                    if (item.BackColor != Color.OrangeRed)
                                        item.BackColor = Color.OrangeRed;
                                }
                                else
                                {
                                    if (item.BackColor != SystemColors.ButtonFace)
                                        item.BackColor = SystemColors.ButtonFace;
                                }
                                #endregion
                            }
                        }
                    }
                }
                foreach (LabelExJogPos lbl in m_listLabelPosLatch)
                {
                    if (lbl != null)
                    {
                        if (lbl.AXIS != SVDF.AXES.NONE)
                        {
                            double dValue = 0.0;
                            // 래치 값 표시
                            if (lbl != null)
                            {
                                dValue = MotionMgr.Inst.MOTION[(int)lbl.AXIS].GetRealPos() - m_dLatchPos[(int)lbl.AXIS];
                                lbl.Text = dValue.ToString("F3");
                            }
                        }

                    }
                    
                }
            }
            catch (Exception ex)
            {
                GbFunc.WriteExeptionLog(ex.ToString());
                throw ex;
            }
            finally
            {
                m_bTmrRefreshFlag = false;
            }

            if (GetJogSpeedIndex() == 0 && lbSpeedChk.Text != "저속") lbSpeedChk.Text = "저속";
            else if (GetJogSpeedIndex() == 1 && lbSpeedChk.Text != "중속") lbSpeedChk.Text = "중속";
            else if (GetJogSpeedIndex() == 2 && lbSpeedChk.Text != "고속") lbSpeedChk.Text = "고속";
        }


        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        private void popJog_VisibleChanged(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = this.Visible;
        }

        private void btnLatch_Click(object sender, EventArgs e)
        {
            m_bIsLatch = !m_bIsLatch;

            if (m_bIsLatch) btnLatch.BackColor = Color.Blue;
            else btnLatch.BackColor = Color.LightGray;

            if (m_bIsLatch) btnLatch.ForeColor = Color.White;
            else btnLatch.ForeColor = Color.Black;
            double dPos = 0;
            for (int i = 0; i < m_listLabelPos.Count; i++)
            {
                m_listLabelPosLatch[i].Visible = m_bIsLatch;
                double.TryParse(m_listLabelPos[i].Text.ToString(), out dPos);
                m_dLatchPos[(int)m_listLabelPos[i].AXIS] = dPos;
            }
        }

        private void OnTrayElvSigSearchButtonClick(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int nAxisOffset = 0;
            int nAxisNo = 0;

            if (int.TryParse(btn.Tag.ToString(), out nAxisOffset))
            {
                nAxisNo = (int)SVDF.AXES.GD_TRAY_1_ELV_Z + nAxisOffset;

                // 이미 시그널 감지상태이면 무브 하지 못하도록 처리함
                uint nBit = 0;
                if (CAXM.AxmSignalReadInputBit(nAxisNo, 2, ref nBit) != (uint)AXT_FUNC_RESULT.AXT_RT_SUCCESS)
                {
                    // 시그널 읽기 실패시 알람
                    //return mInfo[i].nMoveErrNo;
                    return;
                }
                else
                {
                    // 이미 시그널 감지상태면 SUCCESS
                    if (nBit == 1) return;
                }

                MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_1_ELV_Z + nAxisOffset].MoveSignalSearch();
            }
        }

        private void btnPlusSigSearchTest_Click(object sender, EventArgs e)
        {
            MotionMgr.Inst[(int)SVDF.AXES.NONE].MovePlusSignalSearch();
        }

        private void cbPicker1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            if(combo == cbPicker1)
            {
                tabPicker1.SelectedIndex = combo.SelectedIndex;
            }
            else if(combo == cbPicker2)
            {
                tabPk2Pad.SelectedIndex = combo.SelectedIndex;
            }
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = sender as RadioButton;
            if (!rdo.Checked) return;

            int nTag;
            if (!int.TryParse(rdo.Tag.ToString(), out nTag)) return;

            tabJog.SelectedIndex = nTag;
        }
    }
}
