using NSS_3330S.MOTION;
using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSS_3330S.UC
{
    public class BaseRecipePosControl : Form
    {
        protected RcpMotPosEditButtonEx[,] m_arrMotPosBtn = new RcpMotPosEditButtonEx[SVDF.SV_CNT, SVDF.POS_CNT];
        List<RcpMotPosGetButtonEx> m_listMotPosGetBtn = new List<RcpMotPosGetButtonEx>();
        List<RcpMotPosMoveButtonEx> m_listMotPosMoveBtn = new List<RcpMotPosMoveButtonEx>();

        protected POP.popManualRun m_popManualRun = null;
        protected POP.popRmsRun m_popRmsRun = null;

        protected bool m_bIsUpdate = false;
        
        public BaseRecipePosControl()
        {
            // InitializeComponent보다 먼저 호출되므로 여기서는 컨트롤에 대한 작업을 하면 안됨
        }

        ~BaseRecipePosControl()
        {
            #region 이벤트 해제
            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Click -= BaseRecipePosControl_Click;
                        m_arrMotPosBtn[nCntSv, nCntPos].TextChanged += BaseRecipePosControl_TextChanged;
                    }
                }
            }

            foreach (RcpMotPosGetButtonEx btn in m_listMotPosGetBtn)
            {
                btn.Click -= MotPosGetButton_Click;
            }

            foreach (RcpMotPosMoveButtonEx btn in m_listMotPosMoveBtn)
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

            foreach (RcpMotPosGetButtonEx btn in m_listMotPosGetBtn)
            {
                btn.Click += MotPosGetButton_Click;
            }

            foreach (RcpMotPosMoveButtonEx btn in m_listMotPosMoveBtn)
            {
                btn.Click += MotPosMoveButton_Click;
            }
            #endregion
        }

        public void UpdateRcpMotData(bool bSaveAndValidate = false)
        {
            m_bIsUpdate = true;

            for (int nCntSv = 0; nCntSv < SVDF.SV_CNT; nCntSv++)
            {
                for (int nCntPos = 0; nCntPos < SVDF.POS_CNT; nCntPos++)
                {
                    if (m_arrMotPosBtn != null && m_arrMotPosBtn[nCntSv, nCntPos] != null)
                    {
                        m_arrMotPosBtn[nCntSv, nCntPos].Text = RecipeMgr.Inst.Rcp.dMotPos[nCntSv].dPos[nCntPos].ToString("F3");
                    }
                }
            }

            m_bIsUpdate = false;
        }

        #region 이벤트 처리
        void BaseRecipePosControl_Click(object sender, EventArgs e)
        {
            if (m_bIsUpdate) return;

            RcpMotPosEditButtonEx btn = sender as RcpMotPosEditButtonEx;

            popKeypad key = new popKeypad(btn.Text);
            if (key.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            btn.Text = key.GET_VALUE.ToString();

            double dTmp = 0.0;
            if (!double.TryParse(btn.Text, out dTmp)) return;

            int nAxis = (int)btn.Axis;
            RecipeMgr.Inst.TempRcp.dMotPos[nAxis].dPos[btn.PosNo] = dTmp;
        }

        void BaseRecipePosControl_TextChanged(object sender, EventArgs e)
        {
            if (m_bIsUpdate) return;

            RcpMotPosEditButtonEx btn = sender as RcpMotPosEditButtonEx;

            double dTmp = 0.0;
            if (!double.TryParse(btn.Text, out dTmp)) return;

            int nAxis = (int)btn.Axis;
            RecipeMgr.Inst.TempRcp.dMotPos[nAxis].dPos[btn.PosNo] = dTmp;
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

            RcpMotPosGetButtonEx btn = sender as RcpMotPosGetButtonEx;

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
                RecipeMgr.Inst.TempRcp.dMotPos[(int)btn.Axis].dPos[btn.PosNo] = dPos;
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

            //RcpMotPosMoveButtonEx btn = sender as RcpMotPosMoveButtonEx;

            //int nAxis = 0;
            //int nPosIdx = 0;

            //nAxis = (int)btn.Axis;
            //nPosIdx = btn.PosNo;

            //GbFunc.WriteEventLog(this.GetType().Name.ToString(), string.Format("OnTeachPosMoveButtonClick MOVE AXIS[{0}] POS[{1}]", SVDF.GetAxisName(nAxis), nPosIdx));

            //GbSeq.manualRun.SetMoveRcpPos(nAxis, nPosIdx);
            //GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_RCP_POS);
            //GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            //ShowDlgPopManualRunBlock();
            // MOVE
            RcpMotPosMoveButtonEx btn = sender as RcpMotPosMoveButtonEx;
            int nAxisNo = (int)btn.Axis;
            int nPosNo = (int)btn.PosNo;
            double dTargetPos = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dPos[nPosNo];
            //double dSpeed = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dVel[nPosNo];
            //double dAcc = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dAcc[nPosNo];
            //double dDec = RecipeMgr.Inst.TempRcp.dMotPos[nAxisNo].dDec[nPosNo];
            // [2022.05.23.kmlee] Recipe의 MOVE 속도는 Config를 따라간다
            double dSpeed = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dVel;
            double dAcc = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dAcc;
            double dDec = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dDec;


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

            //MotionMgr.Inst[nAxisNo].MoveAbs(dTargetPos);

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

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }

        #endregion

        #region 컨트롤 찾기
        void FindMotPosTbx(Control.ControlCollection collection)
        {
            foreach (Control ctrl in collection)
            {
                if (ctrl.GetType() == typeof(RcpMotPosEditButtonEx))
                {
                    RcpMotPosEditButtonEx btn = (RcpMotPosEditButtonEx)ctrl;
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
                if (ctrl.GetType() == typeof(RcpMotPosGetButtonEx))
                {
                    RcpMotPosGetButtonEx btn = (RcpMotPosGetButtonEx)ctrl;
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
                if (ctrl.GetType() == typeof(RcpMotPosMoveButtonEx))
                {
                    RcpMotPosMoveButtonEx btn = (RcpMotPosMoveButtonEx)ctrl;
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

        protected void popRms_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_popRmsRun.FormClosing -= popRms_FormClosing;
            if (m_popRmsRun != null)
            {
                m_popRmsRun.Dispose();
            }
            m_popRmsRun = null;
        }

        protected void ShowDlgPopManualRunBlock()
        {
            m_popManualRun = new POP.popManualRun();
            m_popManualRun.FormClosed += popManual_FormClosed;
            if (m_popManualRun.ShowDialog(Application.OpenForms[0]) == System.Windows.Forms.DialogResult.Cancel)
                GbSeq.manualRun.LeaveCycle(-1);
        }


        protected void ShowDigPopRmsRunBlock(string strTitle)
        {
            m_popRmsRun = new POP.popRmsRun();
            m_popRmsRun.FormClosing += popRms_FormClosing;
            m_popRmsRun.SetLabelTextChange(strTitle);
            m_popRmsRun.TopMost = true;
            m_popRmsRun.DialogResult = System.Windows.Forms.DialogResult.OK;

            if(m_popRmsRun.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                if (m_popRmsRun != null)
                {
                    m_popRmsRun.Close();
                }
            }
        }

        protected void CloseDigPopRmsRun()
        {
            if (m_popRmsRun != null)
            {
                m_popRmsRun.Close();
            }
        }
    }
}
