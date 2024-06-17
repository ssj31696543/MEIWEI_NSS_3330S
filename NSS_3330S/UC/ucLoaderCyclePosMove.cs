using NSS_3330S.POP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static NSS_3330S.MCDF;

namespace NSS_3330S.UC
{

    public partial class ucLoaderCyclePosMove : UserControl
    {
        protected POP.popManualRun m_popManualRun = null;
        public ucLoaderCyclePosMove()
        {
            InitializeComponent();
        }
        #region ready이동 이벤트함수(변경예정)
        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            int ntag = btn.GetTag();
            int pos_no = 0;
            int axis_no = 0;
            switch (ntag)
            {
                #region 준비위치
                case 1://  매거진 엘리베이터 Y 준비 위치
                    pos_no = (int)POSDF.MGZ_LD_ELEV_READY;
                    axis_no = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                    break;
                case 2://  매거진 엘리베이터 Z 준비 위치
                    pos_no = (int)POSDF.MGZ_LD_ELEV_READY;
                    axis_no = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                    break;
                case 3://  로드 레일 Y 전면 준비 위치
                    pos_no = (int)POSDF.LD_RAIL_T_READY;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                    break;
                case 4://  로드 레일 Y 후면 준비 위치
                    pos_no = (int)POSDF.LD_RAIL_T_READY;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                    break;
                case 5://  로드 레일 T 준비 위치
                    pos_no = (int)POSDF.LD_RAIL_T_READY;
                    axis_no = (int)SVDF.AXES.LD_RAIL_T;
                    break;
                case 7://  바코드 Y 준비 위치
                    pos_no = (int)POSDF.BOT_BARCDOE_READY;
                    axis_no = (int)SVDF.AXES.BARCODE_Y;
                    break;
                case 8://  로드 비전 Y 준비 위치
                    pos_no = (int)POSDF.LD_VISION_X_READY;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                case 9://  스트립 피커 X 준비 위치
                    pos_no = (int)POSDF.STRIP_PICKER_READY;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 10://  스트립 피커 Z 준비 위치
                    pos_no = (int)POSDF.STRIP_PICKER_READY;
                    axis_no = (int)SVDF.AXES.STRIP_PK_Z;
                    break;
                #endregion
                #region 메뉴얼 동작 위치
                #region 매거진 엘리베이터 Y
                //case 16://  매거진 엘리베이터 Y 좌측 로드 위치
                //    pos_no = (int)POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1;
                //    axis_no = (int)SVDF.AXES.SPARE_63;
                //    break;
                //case 18://  매거진 엘리베이터 Y 좌측 언로드 위치
                //    pos_no = (int)POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1;
                //    axis_no = (int)SVDF.AXES.SPARE_63;
                //    break;
                //case 20://  매거진 엘리베이터 Y 매거진 공급 위치
                //    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                //    {
                //        pos_no = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                //    }
                //    else
                //    {
                //        pos_no = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE;
                //    }
                //    axis_no = (int)SVDF.AXES.SPARE_63;
                //    break;
                #endregion
                #region 매거진 엘리베이터 Z
                case 21://  매거진 엘리베이터 Z 로드 위치
                    pos_no = (int)POSDF.MGZ_LD_ELEV_POSITION_LD_1F_1;
                    axis_no = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                    break;
                case 22://  매거진 엘리베이터 Z 언로드 위치
                    pos_no = (int)POSDF.MGZ_LD_ELEV_POSITION_ULD_2F_1;
                    axis_no = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                    break;
                case 23://  매거진 엘리베이터 Z 매거진 공급 위치
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        pos_no = (int)POSDF.MGZ_LD_ELEV_STRIP_SUPPLY;
                    }
                    else
                    {
                        pos_no = (int)POSDF.MGZ_LD_ELEV_QUAD_SUPPLY;
                    }
                    axis_no = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                    break;
                #endregion
                #region 로드 레일 Y 전면
                case 24://  로드 레일 Y 전면 로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_LOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                    break;
                case 25://  로드 레일 Y 전면 언로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_UNLOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                    break;
                case 26://  로드 레일 Y 전면 얼라인 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_ALIGN;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                    break;
                case 27://  로드 레일 Y 전면 메거진 클립 열림 확인 위치
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK;
                    }
                    else
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE;
                    }
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                    break;
                #endregion
                #region 로드 레일 Y 후면
                case 28://  로드 레일 Y 후면 로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_LOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                    break;
                case 29://  로드 레일 Y 후면 언로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_UNLOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                    break;
                case 30://  로드 레일 Y 후면 얼라인 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_ALIGN;
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                    break;
                case 31://  로드 레일 Y 후면 메거진 클립 열림 확인 위치
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK;
                    }
                    else
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE;
                    }
                    axis_no = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                    break;
                #endregion
                #region 로드 레일 T
                case 32://  로드 레일 T 로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_LOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_T;
                    break;
                case 33://  로드 레일 T 언로드 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_UNLOADING;
                    axis_no = (int)SVDF.AXES.LD_RAIL_T;
                    break;
                case 34://  로드 레일 T 얼라인 위치
                    pos_no = (int)POSDF.LD_RAIL_T_STRIP_ALIGN;
                    axis_no = (int)SVDF.AXES.LD_RAIL_T;
                    break;
                case 35://  로드 레일 T 메거진 클립 열림 확인 위치
                    if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK;
                    }
                    else
                    {
                        pos_no = (int)POSDF.LD_RAIL_MAGAZINE_CLIP_OPEN_CHECK_STRIP_MODE;
                    }
                    axis_no = (int)SVDF.AXES.LD_RAIL_T;
                    break;
                #endregion
                #region 바코드 Y
                case 41://  바코드 Y 바코딩 위치
                    pos_no = (int)POSDF.BOT_BARCDOE_SCAN;
                    axis_no = (int)SVDF.AXES.BARCODE_Y;
                    break;
                #endregion
                #region 로드 비전 Y
                case 42://  로드 비전 Y 스트립 그립 위치
                    pos_no = (int)POSDF.LD_VISION_X_STRIP_GRIP;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                case 43://  로드 비전 Y 1차 프리 얼라인 위치
                    pos_no = (int)POSDF.LD_VISION_X_STRIP_ALIGN_1;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                case 44://  로드 비전 Y 2차 프리 얼라인 위치
                    pos_no = (int)POSDF.LD_VISION_X_STRIP_ALIGN_2;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                case 45://  로드 비전 Y 오리엔테이션 위치
                    pos_no = (int)POSDF.LD_VISION_X_STRIP_ORIENT_CHECK;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                case 46://  로드 비전 Y 바코딩 위치
                    pos_no = (int)POSDF.LD_VISION_X_STRIP_BARCODE;
                    axis_no = (int)SVDF.AXES.LD_VISION_X;
                    break;
                #endregion
                #region 스트립 피커 X
                case 47://  스트립 피커 프리얼라인 1 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_1;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 48://  스트립 피커 프리얼라인 2 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_2;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 49://  스트립 피커 스트립 오리엔테이션 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 50://  스트립 피커 바코딩 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_2D_CODE;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 51://  스트립 피커 스트립 로딩 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_LOADING;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 52://  스트립 피커 언로딩 테이블 1 위치
                    pos_no = (int)POSDF.STRIP_PICKER_UNLOADING_TABLE_1;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 54://  스트립 피커 안전그립 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 55://  스트립 피커 그립 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_GRIP;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 56://  스트립 피커 언그립 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_UNGRIP;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 57://  스트립 피커 안전 언그립 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                case 58://  스트립 피커 스트립 푸시 다운 위치
                    pos_no = (int)POSDF.STRIP_PICKER_STRIP_PUSH_DOWN;
                    axis_no = (int)SVDF.AXES.STRIP_PK_X;
                    break;
                #endregion
                #region 스트립 피커 Z
                case 59:
                    axis_no = (int)SVDF.AXES.STRIP_PK_Z;
                    int nAxisNoX = (int)SVDF.AXES.STRIP_PK_X;
                    if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_1))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_1;
                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_2))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_PREALIGN_2;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_ORIENT_CHECK;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_2D_CODE))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_2D_CODE;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_LOADING))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_LOADING;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_UNLOADING_TABLE_1))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_UNLOADING_TABLE_1;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_GRIP))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_GRIP;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_UNGRIP))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_UNGRIP;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY;

                    }
                    else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.STRIP_PICKER_STRIP_PUSH_DOWN))
                    {
                        pos_no = (int)POSDF.STRIP_PICKER_STRIP_PUSH_DOWN;

                    }
                    else
                    {
                        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                        pos_no = (int)POSDF.STRIP_PICKER_READY;
                    }
                    break;
                #endregion
                #endregion
                default:
                    break;
            }
            MovePos(axis_no, pos_no);

        }
        #endregion

        void MovePos(int nAxisNo, int nPosNo)
        {
            // MOVE
            double dTargetPos = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dPos[nPosNo];
            double dSpeed = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dVel[nPosNo];
            double dAcc = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dAcc[nPosNo];
            double dDec = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dDec[nPosNo];
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

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
        void MovePos(int nAxisNo, double dTargetPos, double dSpeed = 40, double dAcc = 400, double dDec = 400)
        {
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

            GbSeq.manualRun.SetMoveTeachPos(m_nAxisArray, m_dPosArray, m_nOrderArray, true, true, m_dSpeedArray, m_dAccArray, m_dDecArray);
            GbSeq.manualRun.SetRunModule(MODULE_TYPE.MOVE_TEACH_POS);
            GbSeq.manualRun.StartManualProcRun(THREAD.ManualRunThread.MANUAL_SEQ.RUN_MODULE, OnFinishSeq);

            ShowDlgPopManualRunBlock();
        }
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
