using NSS_3330S.MOTION;
using NSS_3330S.POP;
using NSS_3330S.UC.CONFIG;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class BaseCfgPosControl : Form
    {
        protected CfgMotPosEditButtonEx[,] m_arrMotPosBtn = new CfgMotPosEditButtonEx[SVDF.SV_CNT, SVDF.POS_CNT];
        List<CfgMotPosGetButtonEx> m_listMotPosGetBtn = new List<CfgMotPosGetButtonEx>();
        List<CfgMotPosMoveButtonEx> m_listMotPosMoveBtn = new List<CfgMotPosMoveButtonEx>();

        protected POP.popManualRun m_popManualRun = null;
        protected bool m_bIsUpdate = false;

        public BaseCfgPosControl()
        {
            //
        }

        ~BaseCfgPosControl()
        {
            #region 이벤트 해제
            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Click -= BaseRecipePosControl_Click;
                        m_arrMotPosBtn[nCntSv, nCntPos].TextChanged -= BaseRecipePosControl_TextChanged;
                    }
                }
            }

            foreach (CfgMotPosGetButtonEx btn in m_listMotPosGetBtn)
            {
                btn.Click -= MotPosGetButton_Click;
            }

            foreach (CfgMotPosMoveButtonEx btn in m_listMotPosMoveBtn)
            {
                btn.Click -= MotPosMoveButton_Click;
            } 
            #endregion

            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    m_arrMotPosBtn[nCntSv, nCntPos] = null;
                }
            }

            m_listMotPosGetBtn.Clear();
            m_listMotPosMoveBtn.Clear();
        }

        public void LinkRecipeControl()
        {
            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    m_arrMotPosBtn[nCntSv, nCntPos] = null;
                }
            }

            FindMotPosTbx(this.Controls);
            FindMotPosGetBtn(this.Controls);
            FindMotPosMoveBtn(this.Controls);

            #region 이벤트 등록
            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Click += BaseRecipePosControl_Click;
                        m_arrMotPosBtn[nCntSv, nCntPos].TextChanged += BaseRecipePosControl_TextChanged;
                    }
                }
            }

            foreach (CfgMotPosGetButtonEx btn in m_listMotPosGetBtn)
            {
                btn.Click += MotPosGetButton_Click;
            }

            foreach (CfgMotPosMoveButtonEx btn in m_listMotPosMoveBtn)
            {
                btn.Click += MotPosMoveButton_Click;
            }
            #endregion
        }

        public void UpdateData(bool bSaveAndValidate = false)
        {
            m_bIsUpdate = true;

            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn != null && m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Text = ConfigMgr.Inst.TempCfg.dMotPos[nCntSv].dPos[nCntPos].ToString("F3");
                    }
                }
            }

            m_bIsUpdate = false;
        }

        #region 이벤트 처리
        void BaseRecipePosControl_Click(object sender, EventArgs e)
        {
            if (m_bIsUpdate) return;

            CfgMotPosEditButtonEx btn = sender as CfgMotPosEditButtonEx;

            popKeypad key = new popKeypad(btn.Text);
            if (key.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            btn.Text = key.GET_VALUE.ToString();

            double dTmp = 0.0;
            if (!double.TryParse(btn.Text, out dTmp)) return;

            int nAxis = (int)btn.Axis;

            ConfigMgr.Inst.TempCfg.dMotPos[nAxis].dPos[btn.PosNo] = dTmp;
        }

        void BaseRecipePosControl_TextChanged(object sender, EventArgs e)
        {
            if (m_bIsUpdate) return;

            CfgMotPosEditButtonEx btn = sender as CfgMotPosEditButtonEx;

            double dTmp = 0.0;
            if (!double.TryParse(btn.Text, out dTmp)) return;

            int nAxis = (int)btn.Axis;
            ConfigMgr.Inst.TempCfg.dMotPos[nAxis].dPos[btn.PosNo] = dTmp;
        }

        void MotPosGetButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Do you want to get current position?"),
                                                                                 "Confirm!",
                                                                                 MessageBoxButtons.YesNo,
                                                                                 MessageBoxIcon.Question,
                                                                                 MessageBoxDefaultButton.Button2);
            if (result != System.Windows.Forms.DialogResult.Yes)
            {
                return;
            }

            CfgMotPosGetButtonEx btn = sender as CfgMotPosGetButtonEx;

            if (MotionMgr.Inst[(int)btn.Axis].GetServoOnOff() == false)
            {
                MessageBox.Show(btn.Axis + " Servo is Off Status");
                return;
            }
            if (MotionMgr.Inst[(int)btn.Axis].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
            {
                MessageBox.Show(btn.Axis + " Servo is not home");
                return;
            }

            string strOrgPos = "";
            double dPos = MotionMgr.Inst[(int)btn.Axis].GetRealPos();

            if (m_arrMotPosBtn[(int)btn.Axis, btn.PosNo] != null)
            {
                strOrgPos = m_arrMotPosBtn[(int)btn.Axis, btn.PosNo].Text;
                m_arrMotPosBtn[(int)btn.Axis, btn.PosNo].Text = dPos.ToString("F3");
                ConfigMgr.Inst.TempCfg.dMotPos[(int)btn.Axis].dPos[btn.PosNo] = dPos;
            }

            GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("OnGetMotorCurrentPosButtonClick AXIS[{0}] POS[{1}] {2} -> {3}", btn.Axis, btn.PosName, strOrgPos, dPos));
        }

        void MotPosMoveButton_Click(object sender, EventArgs e)
        {
            //DialogResult result = MessageBox.Show(new Form() { TopMost = true }, "Do you want to go position?",
            //                                                                     "Confirm!",
            //                                                                     MessageBoxButtons.YesNo,
            //                                                                     MessageBoxIcon.Question,
            //                                                                     MessageBoxDefaultButton.Button2);
            //if (result != System.Windows.Forms.DialogResult.Yes)
            //{
            //    return;
            //}

            //CfgMotPosMoveButtonEx btn = sender as CfgMotPosMoveButtonEx;

            //int nAxis = 0;
            //int nPosIdx = 0;

            //nAxis = (int)btn.Axis;
            //nPosIdx = btn.PosNo;

            //GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("OnTeachPosMoveButtonClick MOVE AXIS[{0}] POS[{1}]", SVDF.GetAxisName(nAxis), nPosIdx));

            ////GbSeq.manualRun.SetMoveCfgPos(nAxis, nPosIdx);
            //GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_CFG_POS);
            //GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            //ShowDlgPopManualRunBlock();
            CfgMotPosMoveButtonEx btn = sender as CfgMotPosMoveButtonEx;
            int nAxisNo = (int)btn.Axis;
            int nPosNo = (int)btn.PosNo;
            double dTargetPos = ConfigMgr.Inst.TempCfg.dMotPos[nAxisNo].dPos[nPosNo];
            double dSpeed = ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dVel;
            double dAcc = ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dVel * 10.0f;
            double dDec = ConfigMgr.Inst.TempCfg.MotData[nAxisNo].dVel * 10.0f;

            string strTitle = FormTextLangMgr.FindKey("축 이동");
            string strMsg = string.Format("{0}, {1} {2}\n {3}{4} {5}{6} {7}{8} ",
                FormTextLangMgr.FindKey(SVDF.GetAxisName(nAxisNo))
                , dTargetPos.ToString("F3")
                , FormTextLangMgr.FindKey("으로 이동하시겠습니까?")
                , FormTextLangMgr.FindKey("속도:")
                , dSpeed.ToString("0.0")
                , FormTextLangMgr.FindKey("가속도:")
                , dAcc.ToString("0.0")
                , FormTextLangMgr.FindKey("감속도:")
                , dDec.ToString("0.0")
                );
            popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
            if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            int[] m_nAxisArray = null;
            double[] m_dPosArray = null;
            uint[] m_nOrderArray = null;
            double[] m_dSpeedArray = null;
            double[] m_dAccArray = null;
            double[] m_dDecArray = null;

            m_nAxisArray = new int[] { nAxisNo };
            m_dPosArray = new double[] { dTargetPos };
            m_nOrderArray = new uint[] { 0 };
            m_dSpeedArray = new double[] { dSpeed };
            m_dAccArray = new double[] { dAcc };
            m_dDecArray = new double[] { dDec };

            GbSeq.manualRun.SetMoveCfgPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_CFG_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        #endregion

        #region 컨트롤 찾기
        void FindMotPosTbx(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(CfgMotPosEditButtonEx))
                {
                    CfgMotPosEditButtonEx btn = (CfgMotPosEditButtonEx)ctrl;
                    if (btn.PosNo >= 0 && btn.PosNo < SVDF.POS_CNT)
                    {
                        // 이미 중복된 컨트롤이 존재함
                        if (m_arrMotPosBtn[(int)btn.Axis, btn.PosNo] != null)
                            System.Diagnostics.Debugger.Break();

                        m_arrMotPosBtn[(int)btn.Axis, btn.PosNo] = btn;
                    }
                }

                if (ctrl.Controls.Count > 0)
                    FindMotPosTbx(ctrl.Controls);
            }
        }

        void FindMotPosGetBtn(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(CfgMotPosGetButtonEx))
                {
                    CfgMotPosGetButtonEx btn = (CfgMotPosGetButtonEx)ctrl;
                    if (btn.PosNo >= 0 && btn.PosNo < SVDF.POS_CNT)
                    {
                        m_listMotPosGetBtn.Add(btn);
                    }
                }

                if (ctrl.Controls.Count > 0)
                    FindMotPosGetBtn(ctrl.Controls);
            }
        }

        void FindMotPosMoveBtn(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(CfgMotPosMoveButtonEx))
                {
                    CfgMotPosMoveButtonEx btn = (CfgMotPosMoveButtonEx)ctrl;
                    if (btn.PosNo >= 0 && btn.PosNo < SVDF.POS_CNT)
                    {
                        m_listMotPosMoveBtn.Add(btn);
                    }
                }

                if (ctrl.Controls.Count > 0)
                    FindMotPosMoveBtn(ctrl.Controls);
            }
        }
        #endregion

        protected void OnFinishSeq(int nResult)
        {
            //System.Diagnostics.Debug.WriteLine("Finished");

            if (this.InvokeRequired)
            {
                BeginInvoke(new CommonEvent.FinishSeqEvent(OnFinishSeq), nResult);
                return;
            }

            if (m_popManualRun != null && !m_popManualRun.IsDisposed)
            {
                if (nResult == 0) m_popManualRun.DialogResult = System.Windows.Forms.DialogResult.OK;
                m_popManualRun.Close();
            }
        }

        protected void popManual_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_popManualRun.FormClosed -= popManual_FormClosed;
            if (m_popManualRun != null)
            {
                m_popManualRun.Dispose();
            }
            m_popManualRun = null;
        }

        protected void ShowDlgPopManualRunBlock()
        {
            m_popManualRun = new POP.popManualRun();
            m_popManualRun.FormClosed += popManual_FormClosed;
            if (m_popManualRun.ShowDialog(Application.OpenForms[0]) == System.Windows.Forms.DialogResult.Cancel)
                GbSeq.manualRun.LeaveCycle(-1);
        }
    }
}
