using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSS_3330S.MOTION;
using NSS_3330S.UC;
using static NSS_3330S.SVDF;

namespace NSS_3330S.POP
{
    public partial class popNeonJog : Form
    {
        Point mouseLocation;
        List<ButtonExJog> m_listBtnJog = new List<ButtonExJog>();
        List<LabelExJogPos> m_listLabelPosLatch = new List<LabelExJogPos>();
        List<double> m_dLatchPos = new List<double>();

        const int MOVE_MODE_JOG = 0;
        const int MOVE_MODE_INC = 1;
        const int MOVE_MODE_ABS = 2;

        bool m_bTmrRefreshFlag = false;

        bool m_bIsLatch = false;

        int nSelectedAxisNo = 0;

        Color OnRed = Color.Red;
        Color OffRed = Color.FromArgb(64, 0, 0);
        Color OnBlue = Color.Blue;
        Color OffBule = Color.FromArgb(0, 0, 64);

        /// <summary>
        /// LOADER, 쏘우, 클리너, 쏘터 축 번호 디파인
        /// </summary>
        /// 
        int[] arrLoaderMotor = { (int)SVDF.AXES.MAGAZINE_ELV_Z };
        int[] arrSawMotor = { (int)SVDF.AXES.LD_RAIL_T, (int)SVDF.AXES.LD_RAIL_Y_FRONT, (int)SVDF.AXES.LD_RAIL_Y_REAR, (int)SVDF.AXES.BARCODE_Y,
                              (int)SVDF.AXES.STRIP_PK_X, (int)SVDF.AXES.STRIP_PK_Z, (int)SVDF.AXES.LD_VISION_X,
                              (int)SVDF.AXES.UNIT_PK_X, (int)SVDF.AXES.UNIT_PK_Z};
        int[] arrSorterMotor = { (int)SVDF.AXES.DRY_BLOCK_STG_X, (int)SVDF.AXES.MAP_PK_X, (int)SVDF.AXES.MAP_PK_Z, (int)SVDF.AXES.MAP_VISION_Z,
                                 (int)SVDF.AXES.MAP_STG_1_Y, (int)SVDF.AXES.MAP_STG_1_T, (int)SVDF.AXES.MAP_STG_2_Y, (int)SVDF.AXES.MAP_STG_2_T,
                                 (int)SVDF.AXES.BALL_VISION_Y, (int)SVDF.AXES.BALL_VISION_Z,
                                 (int)SVDF.AXES.TRAY_PK_X,(int)SVDF.AXES.TRAY_PK_Y,(int)SVDF.AXES.TRAY_PK_Z,
                                 (int)SVDF.AXES.GD_TRAY_STG_1_Y,(int)SVDF.AXES.GD_TRAY_STG_2_Y,(int)SVDF.AXES.RW_TRAY_STG_Y,
                                 
                                 (int)SVDF.AXES.GD_TRAY_1_ELV_Z,(int)SVDF.AXES.GD_TRAY_2_ELV_Z,(int)SVDF.AXES.RW_TRAY_ELV_Z,(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z,(int)SVDF.AXES.EMTY_TRAY_2_ELV_Z};

        int[] arrChipPicker1 = { (int)SVDF.AXES.CHIP_PK_1_X, (int)SVDF.AXES.CHIP_PK_1_Z_1, (int)SVDF.AXES.CHIP_PK_1_T_1,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_2, (int)SVDF.AXES.CHIP_PK_1_T_2,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_3, (int)SVDF.AXES.CHIP_PK_1_T_3,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_4, (int)SVDF.AXES.CHIP_PK_1_T_4,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_5, (int)SVDF.AXES.CHIP_PK_1_T_5,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_6, (int)SVDF.AXES.CHIP_PK_1_T_6,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_7, (int)SVDF.AXES.CHIP_PK_1_T_7,
                                 (int)SVDF.AXES.CHIP_PK_1_Z_8, (int)SVDF.AXES.CHIP_PK_1_T_8};

        int[] arrChipPicker2 = {  (int)SVDF.AXES.CHIP_PK_2_X, (int)SVDF.AXES.CHIP_PK_2_Z_1, (int)SVDF.AXES.CHIP_PK_2_T_1,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_2, (int)SVDF.AXES.CHIP_PK_2_T_2,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_3, (int)SVDF.AXES.CHIP_PK_2_T_3,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_4, (int)SVDF.AXES.CHIP_PK_2_T_4,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_5, (int)SVDF.AXES.CHIP_PK_2_T_5,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_6, (int)SVDF.AXES.CHIP_PK_2_T_6,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_7, (int)SVDF.AXES.CHIP_PK_2_T_7,
                                 (int)SVDF.AXES.CHIP_PK_2_Z_8, (int)SVDF.AXES.CHIP_PK_2_T_8};

        bool bTimeFlag = false;
        int nTimeStack = 0;
        public enum AXES_KOREAN
        {
            NONE = -1,

            #region SORTER
            VISION_TRIGGER_BALL,
            VISION_TRIGGER_BALL_2,
            VISION_TRIGGER_MAP,
            VISION_TRIGGER_MAP_2,

            드라이_스테이지_X = 4,
            맵_피커_X,
            맵_피커_Z,
            맵_테이블_Y1,
            맵_테이블_Y2,
            칩피커_X1,
            칩피커_X2,
            볼_비전_Y,
            볼_비전_Z,
            트레이_피커_X,
            트레이_피커_Z,
            트레이_피커_Y,
            양품_트레이1_엘리베이터_Z,
            양품_트레이2_엘리베이터_Z,
            리워크_트레이_엘리베이터_Z,
            빈_트레이1_엘리베이터_Z,
            빈_트레이2_엘리베이터_Z,
            양품_트레이1_테이블_Y,
            양품_트레이2_테이블_Y,
            리워크_트레이_테이블_Y,
            맵_테이블_T1,
            맵_테이블_T2,
            맵_비전_Z,
            #endregion

            #region HEAD
            칩_피커1_Z1,
            칩_피커1_T1,
            칩_피커1_Z2,
            칩_피커1_T2,
            칩_피커1_Z3,
            칩_피커1_T3,
            칩_피커1_Z4,
            칩_피커1_T4,
            칩_피커1_Z5,
            칩_피커1_T5,
            칩_피커1_Z6,
            칩_피커1_T6,
            칩_피커1_Z7,
            칩_피커1_T7,
            칩_피커1_Z8,
            칩_피커1_T8,

            칩_피커2_Z1,
            칩_피커2_T1,
            칩_피커2_Z2,
            칩_피커2_T2,
            칩_피커2_Z3,
            칩_피커2_T3,
            칩_피커2_Z4,
            칩_피커2_T4,
            칩_피커2_Z5,
            칩_피커2_T5,
            칩_피커2_Z6,
            칩_피커2_T6,
            칩_피커2_Z7,
            칩_피커2_T7,
            칩_피커2_Z8,
            칩_피커2_T8,
            #endregion

            #region LOADER
            로드_레일_T,
            로드_레일_Y_전면,
            로드_레일_Y_후면,
            바코드_Y,
            로드_비전_Y,
            스트립_피커_X,
            스트립_피커_Z,
            유닛_피커_X,
            유닛_피커_Z,
            매거진_엘리베이터_Z,
            #endregion

            MAX,
        }
        public enum AXES_CHINA
        {
            NONE = -1,

            #region SORTER
            VISION_TRIGGER_BALL,
            VISION_TRIGGER_BALL_1,
            VISION_TRIGGER_MAP,
            VISION_TRIGGER_MAP_2,

            干燥块_X = 4,
            检查手臂_X,
            检查手臂_Z,
            检查台_Y1,
            检查台_Y2,
            分拣手臂_X1,
            分拣手臂_X2,
            球视图_Y,
            球视图_Z,
            托盘_拣选机_X,
            托盘_拣选机_Z,
            托盘_拣选机_Y,
            良品托盘_升降台1_Z,
            良品托盘_升降台2_Z,
            返工托盘升降台_Z,
            空_升降台投入1_Z,
            空_升降台投入2_Z,
            良品托盘_台1_Y,
            良品托盘_台2_Y,
            返工托盘_台_Y,
            检查台_T1,
            检查台_T2,
            检查_视图_Z,
            #endregion

            #region HEAD
            分拣手臂1_Z1,
            分拣手臂1_T1,
            分拣手臂1_Z2,
            分拣手臂1_T2,
            分拣手臂1_Z3,
            分拣手臂1_T3,
            分拣手臂1_Z4,
            分拣手臂1_T4,
            分拣手臂1_Z5,
            分拣手臂1_T5,
            分拣手臂1_Z6,
            分拣手臂1_T6,
            分拣手臂1_Z7,
            分拣手臂1_T7,
            分拣手臂1_Z8,
            分拣手臂1_T8,

            分拣手臂2_Z1,
            分拣手臂2_T1,
            分拣手臂2_Z2,
            分拣手臂2_T2,
            分拣手臂2_Z3,
            分拣手臂2_T3,
            分拣手臂2_Z4,
            分拣手臂2_T4,
            分拣手臂2_Z5,
            分拣手臂2_T5,
            分拣手臂2_Z6,
            分拣手臂2_T6,
            分拣手臂2_Z7,
            分拣手臂2_T7,
            分拣手臂2_Z8,
            分拣手臂2_T8,
            #endregion

            #region LOADER
            装载导轨_T,
            装载导轨_Y_正面,
            装载导轨_Y_背面,
            条形码_Y,
            装载_视图_Y,
            基板手臂_X,
            基板手臂_Z,
            单元手臂_X,
            单元手臂_Z,
            料盒升降台_Z,
            #endregion

            MAX,
        }
        public popNeonJog()
        {
            InitializeComponent();

            this.CenterToScreen();

            m_dLatchPos = new List<double>();
            for (int i = 0; i < (int)SVDF.AXES.MAX; i++)
            {
                m_dLatchPos.Add(0.0);
            }

            nSelectedAxisNo = 0;
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

            SetMotorList(nTag);

            string strValue = gridSelect.Rows[0].Cells[0].Value.ToString();

            string[] strAxisValue = strValue.Split('.');

            int.TryParse(strAxisValue[0], out nSelectedAxisNo);
        }


        private void OnManualJogButtonDown(object sender, MouseEventArgs e)
        {
            //if (e.Button != System.Windows.Forms.MouseButtons.Left)
            //    return;

            //ButtonExJog btn = sender as ButtonExJog;
            //Moving(btn);
        }

        private void OnManualJogButtonUp(object sender, MouseEventArgs e)
        {
            //if (GbVar.mcState.IsRun()) return;

            //if (e.Button != System.Windows.Forms.MouseButtons.Left)
            //    return;

            //ButtonExJog btn = sender as ButtonExJog;
            //int nAxisNo = (int)btn.AXIS;

            //if (tabJogMode.SelectedIndex == MOVE_MODE_JOG)
            //    MotionMgr.Inst[nAxisNo].MoveStop();

            //btn.BackColor = Color.FromArgb(240, 240, 240);
        }

        void MoveJog(int nAxisNo, double dPos)
        {
            string strErr = "";
            //double dRealPos = MotionMgr.Inst[nAxisNo].GetRealPos();

            //if (dRealPos < ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN)
            //{
            //    MotionMgr.Inst[nAxisNo].MoveEStop();

            //    strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W - Limit 보다 큰 위치 구동 불가");
            //    popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
            //    if (messageBox.ShowDialog() == DialogResult.OK)
            //    {
            //        return;
            //    }
            //    return;
            //}

            //if (dRealPos > ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP)
            //{
            //    MotionMgr.Inst[nAxisNo].MoveEStop();

            //    strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W + Limit 보다 큰 위치 구동 불가");
            //    popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
            //    if (messageBox.ShowDialog() == DialogResult.OK)
            //    {
            //        return;
            //    }
            //    return;
            //}
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
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W - Limit 보다 큰 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
                if (messageBox.ShowDialog() == DialogResult.OK)
                {
                    return;
                }
                return;
            }

            if (dMovePos > ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP)
            {
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W + Limit 보다 큰 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
                if (messageBox.ShowDialog() == DialogResult.OK)
                {
                    return;
                }
                return;
            }
            if (MotionMgr.Inst[nAxisNo].IsBusy())
            {
                return;
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

                popMessageBox messageBox = new popMessageBox(FormTextLangMgr.FindKey("구동 인터락!!!") + strErr, "ALARM");
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
            string strErr = "구동 에러";

            if (rdbVelLow.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
            else if (rdbVelMid.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
            else if (rdbVelHigh.Checked) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
            else return;

            if (dMovePos < ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitN)
            {
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W - Limit 보다 큰 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
                if (messageBox.ShowDialog() == DialogResult.OK)
                {
                    return;
                }
                return;
            }

            if (dMovePos > ConfigMgr.Inst.Cfg.MotData[nAxisNo].dSwLimitP)
            {
                strErr = FormTextLangMgr.FindKey("구동 인터락!!!") + FormTextLangMgr.FindKey(" S /W + Limit 보다 큰 위치 구동 불가");
                popMessageBox messageBox = new popMessageBox(strErr, "알람", MessageBoxButtons.OK);
                if (messageBox.ShowDialog() == DialogResult.OK)
                {
                    return;
                }
                return;
            }

            if (MotionMgr.Inst[nAxisNo].IsBusy())
            {
                return;
            }

            //if (GetJogSpeedIndex() == 0) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogLowVel;
            //else if (GetJogSpeedIndex() == 1) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogMdlVel;
            //else if (GetJogSpeedIndex() == 2) dVel = ConfigMgr.Inst.Cfg.MotData[nAxisNo].dJogHighVel;
            //else return;

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisNo, dMovePos);
            if (FNC.IsErr(nSafety))
            {
                strErr = "구동 에러";

                if (Enum.IsDefined(typeof(ERDF), nSafety)) strErr = ((ERDF)nSafety).ToString();

                popMessageBox messageBox = new popMessageBox(FormTextLangMgr.FindKey("구동 인터락!!!") + strErr, "ALARM");
                messageBox.ShowDialog();
                return;
            }

            MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dVel, dVel * 4, dVel * 4);
        }

        void Moving(int nDir)
        {
            if (GbVar.mcState.IsRun()) return;

            double dPos = 0.0;

            if (!double.TryParse(tbxIncMove.Text, out dPos))
            {
                MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Distance value is invalidate."));
                return;
            }

            switch (tabJogMode.SelectedIndex)
            {
                case MOVE_MODE_JOG:
                    MoveJog(nSelectedAxisNo, nDir);
                    break;
                case MOVE_MODE_INC:
                    if (!double.TryParse(tbxIncMove.Text, out dPos))
                    {
                        MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Distance value is invalidate."));
                        return;
                    }
                    MoveInc(nSelectedAxisNo, dPos * nDir);
                    break;
                case MOVE_MODE_ABS:
                    if (!double.TryParse(tbxAbsMove.Text, out dPos))
                    {
                        MessageBox.Show(new Form() { TopMost = true }, FormTextLangMgr.FindKey("Moving position is invalidate."));
                        return;
                    }
                    if (Chk_Q.Checked)// 220406 17:00 KTH
                    {
                        string str_msg = string.Format("{0}, {1}{2}\n ",
                            FormTextLangMgr.FindKey(SVDF.GetAxisName(nSelectedAxisNo)),
                            tbxAbsMove.Text,
                            FormTextLangMgr.FindKey(" 위치로 이동하시겠습니까?"));
                        string str_title = "주의";
                        popMessageBox messageBox = new popMessageBox(str_msg, str_title);
                        if(messageBox.ShowDialog() == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                    MoveAbs(nSelectedAxisNo, dPos);
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
            if (bTimeFlag)
            {
                nTimeStack++;
                int nCountDown = 3 - (nTimeStack / 20);

                //if (btnServoOn.Enabled == true) btnServoOn.Enabled = false;
                //if (btnServoReset.Enabled == true) btnServoReset.Enabled = false;
                //if (btnHome.Enabled == true) btnHome.Enabled = false;

                if (btnServoOn.Text != nCountDown.ToString()) btnServoOn.Text = string.Format("{0} 초",nCountDown);
                if (btnServoReset.Text != nCountDown.ToString()) btnServoReset.Text = string.Format("{0} 초", nCountDown);
                if (btnHome.Text != nCountDown.ToString()) btnHome.Text = string.Format("{0} 초", nCountDown);

                if (nCountDown == 0)
                    bTimeFlag = false;
            }
            else
            {
                if (nTimeStack != 0)
                    nTimeStack = 0;
                //if (btnServoOn.Enabled == false) btnServoOn.Enabled = true;
                //if (btnServoReset.Enabled == false) btnServoReset.Enabled = true;
                //if (btnHome.Enabled == false) btnHome.Enabled = true;


                if (btnServoOn.Text != "SERVO ON") btnServoOn.Text = "SERVO ON";
                if (btnServoReset.Text != "RESET") btnServoReset.Text = "RESET";
                if (btnHome.Text != "HOME") btnHome.Text = "HOME";
            }
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

                // 위치 값 표시
                if (labelExJogPos != null)
                {
                    if (labelExJogPos.AXIS != SVDF.AXES.NONE)
                    {
                        labelExJogPos.RefreshUI();
                    }
                    if (labelExJogPos.AXIS != (SVDF.AXES)nSelectedAxisNo)
                    {
                        labelExJogPos.AXIS = (SVDF.AXES)nSelectedAxisNo;
                    }
                }

                //LATCH 값 표시
                if (labelExJogPosLatch != null)
                {
                    if (labelExJogPosLatch.AXIS != SVDF.AXES.NONE)
                    {
                        if (labelExJogPosLatch.AXIS != (SVDF.AXES)nSelectedAxisNo)
                        {
                            labelExJogPosLatch.AXIS = (SVDF.AXES)nSelectedAxisNo;
                        }

                        double dValue = 0.0;
                        // 래치 값 표시
                        if (labelExJogPosLatch != null)
                        {
                            dValue = MotionMgr.Inst.MOTION[(int)labelExJogPosLatch.AXIS].GetRealPos() - m_dLatchPos[(int)labelExJogPosLatch.AXIS];
                            labelExJogPosLatch.Text = dValue.ToString("F3");
                        }
                    }
                }

                //cmd 값 표시
                if (labelExJogPosCmd != null)
                {
                    if (labelExJogPosCmd.AXIS != SVDF.AXES.NONE)
                    {
                        if (labelExJogPosCmd.AXIS != (SVDF.AXES)nSelectedAxisNo)
                        {
                            labelExJogPosCmd.AXIS = (SVDF.AXES)nSelectedAxisNo;
                        }
                        double dValue = 0.0;
                        // 래치 값 표시
                        if (labelExJogPosCmd != null)
                        {
                            dValue = MotionMgr.Inst.MOTION[(int)labelExJogPosCmd.AXIS].GetCmdPos();
                            labelExJogPosCmd.Text = dValue.ToString("F3");
                        }
                    }
                }
                //VEL 값 표시
                if (labelExJogPosSpeed != null)
                {
                    if (labelExJogPosSpeed.AXIS != SVDF.AXES.NONE)
                    {
                        if (labelExJogPosSpeed.AXIS != (SVDF.AXES)nSelectedAxisNo)
                        {
                            labelExJogPosSpeed.AXIS = (SVDF.AXES)nSelectedAxisNo;
                        }
                        double dValue = 0.0;
                        // 래치 값 표시
                        if (labelExJogPosSpeed != null)
                        {
                            dValue = MotionMgr.Inst.MOTION[(int)labelExJogPosSpeed.AXIS].GetVelocity();
                            labelExJogPosSpeed.Text = dValue.ToString("F3");
                        }
                    }
                }

                lbError.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].IsAlarm() ? OnRed : OffRed;
                lbHomeComplete.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].GetHomeResult() == 0 ? OnBlue : OffBule;
                btnHome.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].GetHomeResult() == 0 ? Color.FromArgb(153, 255, 54) : Color.FromArgb(24, 24, 24);
                btnHome.ForeColor = MotionMgr.Inst[(int)nSelectedAxisNo].GetHomeResult() == 0 ? Color.Black : Color.White;
                lbBusy.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].IsBusy() ? OnBlue : OffBule;
                lbHome.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].IsHome() ? OnBlue : OffBule;
                lbMinus.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].IsMinusLimit() ? OnRed : OffRed;
                lbPlus.BackColor = MotionMgr.Inst[(int)nSelectedAxisNo].IsPlusLimit() ? OnRed : OffRed;

                if (btnServoOn != null)
                {
                    if (MotionMgr.Inst[nSelectedAxisNo].GetServoOnOff())
                    {
                        if (btnServoOn.BackColor != Color.FromArgb(153, 255, 54))
                            btnServoOn.BackColor = Color.FromArgb(153, 255, 54);

                        if (btnServoOn.ForeColor != Color.Black)
                            btnServoOn.ForeColor = Color.Black;
                    }
                    else
                    {
                        if (btnServoOn.BackColor != Color.FromArgb(24, 24, 24))
                            btnServoOn.BackColor = Color.FromArgb(24, 24, 24);

                        if (btnServoOn.ForeColor != Color.White)
                            btnServoOn.ForeColor = Color.White;
                    }
                }

                if (btnServoReset != null)
                {
                    if (MotionMgr.Inst[nSelectedAxisNo].IsAlarm())
                    {
                        if (btnServoReset.BackColor != Color.Red)
                            btnServoReset.BackColor = Color.Red;
                    }
                    else
                    {
                        if (btnServoReset.BackColor != Color.FromArgb(24, 24, 24))
                            btnServoReset.BackColor = Color.FromArgb(24, 24, 24);
                    }
                }
                if (bTimeFlag)
                {
                    nTimeStack++;
                    int nCountDown = 3 - (nTimeStack / 20);

                    if (btnServoOn.Enabled == true) btnServoOn.Enabled = false;
                    if (btnServoReset.Enabled == true) btnServoReset.Enabled = false;
                    if (btnHome.Enabled == true) btnHome.Enabled = false;

                    if (btnServoOn.Text != nCountDown.ToString()) btnServoOn.Text = nCountDown.ToString();
                    if (btnServoReset.Text != nCountDown.ToString()) btnServoReset.Text = nCountDown.ToString();
                    if (btnHome.Text != nCountDown.ToString()) btnHome.Text = nCountDown.ToString();

                    if (nCountDown == 0)
                        bTimeFlag = false;
                }
                else
                {
                    if (nTimeStack != 0)
                        nTimeStack = 0;
                    if (btnServoOn.Enabled == false) btnServoOn.Enabled = true;
                    if (btnServoReset.Enabled == false) btnServoReset.Enabled = true;
                    if (btnHome.Enabled == false) btnHome.Enabled = true;


                    if (btnServoOn.Text != "SERVO ON") btnServoOn.Text = "SERVO ON";
                    if (btnServoReset.Text != "RESET") btnServoReset.Text = "RESET";
                    if (btnHome.Text != "HOME") btnHome.Text = "HOME";
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
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
            {
                if (GetJogSpeedIndex() == 0 && lbSpeedChk.Text != "저속") lbSpeedChk.Text = "저속";
                else if (GetJogSpeedIndex() == 1 && lbSpeedChk.Text != "중속") lbSpeedChk.Text = "중속";
                else if (GetJogSpeedIndex() == 2 && lbSpeedChk.Text != "고속") lbSpeedChk.Text = "고속";
            }
            else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                if (GetJogSpeedIndex() == 0 && lbSpeedChk.Text != "低速") lbSpeedChk.Text = "低速";
                else if (GetJogSpeedIndex() == 1 && lbSpeedChk.Text != "中速") lbSpeedChk.Text = "中速";
                else if (GetJogSpeedIndex() == 2 && lbSpeedChk.Text != "高速") lbSpeedChk.Text = "高速";
            }
            else
            {
                if (GetJogSpeedIndex() == 0 && lbSpeedChk.Text != "LOW") lbSpeedChk.Text = "LOW";
                else if (GetJogSpeedIndex() == 1 && lbSpeedChk.Text != "MID") lbSpeedChk.Text = "MID";
                else if (GetJogSpeedIndex() == 2 && lbSpeedChk.Text != "HIGH") lbSpeedChk.Text = "HIGH";
            }


        }


        public IEnumerable<Control> GetAll(Control control, Type type)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => GetAll(ctrl, type)).Concat(controls).Where(c => c.GetType() == type);
        }

        private void popNeonJog_VisibleChanged(object sender, EventArgs e)
        {
            tmrRefresh.Enabled = this.Visible;
            if (this.Visible)
            {
                FormTextLangMgr.Load(this, PathMgr.Inst.FILE_PATH_LANGUAGE_UI);
            }
        }

        private void btnLatch_Click(object sender, EventArgs e)
        {
            m_bIsLatch = !m_bIsLatch;

            if (m_bIsLatch) btnLatch.BackColor = Color.Blue;
            else btnLatch.BackColor = Color.LightGray;

            if (m_bIsLatch) btnLatch.ForeColor = Color.White;
            else btnLatch.ForeColor = Color.Black;
            double dPos = 0;

            //m_listLabelPosLatch[nSelectedAxisNo].Visible = m_bIsLatch;
            double.TryParse(labelExJogPos.Text.ToString(), out dPos);
            m_dLatchPos[(int)labelExJogPos.AXIS] = dPos;
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
            
        }
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rdo = sender as RadioButton;
            if (!rdo.Checked) return;

            int nTag;
            if (!int.TryParse(rdo.Tag.ToString(), out nTag)) return;

            //tabJog.SelectedIndex = nTag;
        }

        private void gridSelect_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string strValue = gridSelect.Rows[e.RowIndex].Cells[0].Value.ToString();

            string[] strAxisValue = strValue.Split('.');

            int.TryParse(strAxisValue[0], out nSelectedAxisNo);


        }
        void SetSelectAxis(int nAxis)
        {

        }
        void SetMotorList(int nTag)
        {
            gridSelect.Rows.Clear();
            int m_nMotorNo = 0;
            if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.KOREAN)
            {
                switch (nTag)
                {
                    //로더
                    case 0:
                        {
                            gridSelect.RowCount = arrLoaderMotor.Length;
                            for (int i = 0; i < arrLoaderMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrLoaderMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //쏘우
                    case 1:
                        {
                            gridSelect.RowCount = arrSawMotor.Length;
                            for (int i = 0; i < arrSawMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSawMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //클리너
                    case 2:
                        {
                            //gridSelect.RowCount = arrCleanerMotor.Length;
                            //for (int i = 0; i < arrCleanerMotor.Length; i++)
                            //{
                            //    gridSelect.Rows[i].Height = 50;
                            //    m_nMotorNo = arrCleanerMotor[i];
                            //    gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                            //    gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            //}
                        }
                        break;
                    //소터
                    case 3:
                        {
                            gridSelect.RowCount = arrSorterMotor.Length;
                            for (int i = 0; i < arrSorterMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSorterMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 1
                    case 4:
                        {
                            gridSelect.RowCount = arrChipPicker1.Length;
                            for (int i = 0; i < arrChipPicker1.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker1[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 2
                    case 5:
                        {
                            gridSelect.RowCount = arrChipPicker2.Length;
                            for (int i = 0; i < arrChipPicker2.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker2[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (ConfigMgr.Inst.Cfg.General.nLanguage == (int)Language.CHINA)
            {
                switch (nTag)
                {
                    //로더
                    case 0:
                        {
                            gridSelect.RowCount = arrLoaderMotor.Length;
                            for (int i = 0; i < arrLoaderMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrLoaderMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_CHINA)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //쏘우
                    case 1:
                        {
                            gridSelect.RowCount = arrSawMotor.Length;
                            for (int i = 0; i < arrSawMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSawMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_CHINA)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //클리너
                    case 2:
                        {
                            //gridSelect.RowCount = arrCleanerMotor.Length;
                            //for (int i = 0; i < arrCleanerMotor.Length; i++)
                            //{
                            //    gridSelect.Rows[i].Height = 50;
                            //    m_nMotorNo = arrCleanerMotor[i];
                            //    gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                            //    gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            //}
                        }
                        break;
                    //소터
                    case 3:
                        {
                            gridSelect.RowCount = arrSorterMotor.Length;
                            for (int i = 0; i < arrSorterMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSorterMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_CHINA)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 1
                    case 4:
                        {
                            gridSelect.RowCount = arrChipPicker1.Length;
                            for (int i = 0; i < arrChipPicker1.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker1[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_CHINA)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 2
                    case 5:
                        {
                            gridSelect.RowCount = arrChipPicker2.Length;
                            for (int i = 0; i < arrChipPicker2.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker2[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_CHINA)m_nMotorNo + ")";
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (nTag)
                {
                    //로더
                    case 0:
                        {
                            gridSelect.RowCount = arrLoaderMotor.Length;
                            for (int i = 0; i < arrLoaderMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrLoaderMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo;
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //쏘우
                    case 1:
                        {
                            gridSelect.RowCount = arrSawMotor.Length;
                            for (int i = 0; i < arrSawMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSawMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo;
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //클리너
                    case 2:
                        {
                            //gridSelect.RowCount = arrCleanerMotor.Length;
                            //for (int i = 0; i < arrCleanerMotor.Length; i++)
                            //{
                            //    gridSelect.Rows[i].Height = 50;
                            //    m_nMotorNo = arrCleanerMotor[i];
                            //    gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo + "(" + (AXES_KOREAN)m_nMotorNo + ")";
                            //    gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            //}
                        }
                        break;
                    //소터
                    case 3:
                        {
                            gridSelect.RowCount = arrSorterMotor.Length;
                            for (int i = 0; i < arrSorterMotor.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrSorterMotor[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo;
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 1
                    case 4:
                        {
                            gridSelect.RowCount = arrChipPicker1.Length;
                            for (int i = 0; i < arrChipPicker1.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker1[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo;
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    //칩피커 2
                    case 5:
                        {
                            gridSelect.RowCount = arrChipPicker2.Length;
                            for (int i = 0; i < arrChipPicker2.Length; i++)
                            {
                                gridSelect.Rows[i].Height = 50;
                                m_nMotorNo = arrChipPicker2[i];
                                gridSelect.Rows[i].Cells[0].Value = (m_nMotorNo).ToString() + "." + (SVDF.AXES)m_nMotorNo;
                                gridSelect.Rows[i].Cells[0].Style.BackColor = Color.White;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            
        }

        private void btnJogCW_MouseDown(object sender, MouseEventArgs e)
        {
            Moving(1);
        }

        private void btnJogCW_MouseUp(object sender, MouseEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if (tabJogMode.SelectedIndex == MOVE_MODE_JOG)
                MotionMgr.Inst[nSelectedAxisNo].MoveStop();
        }

        private void btnJogCCW_MouseDown(object sender, MouseEventArgs e)
        {
            Moving(-1);
        }

        private void btnJogCCW_MouseUp(object sender, MouseEventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;

            if (e.Button != System.Windows.Forms.MouseButtons.Left)
                return;

            if (tabJogMode.SelectedIndex == MOVE_MODE_JOG)
                MotionMgr.Inst[nSelectedAxisNo].MoveStop();
        }

        private void btnServoOn_Click(object sender, EventArgs e)
        {
            if (bTimeFlag) return;

            bool bSetOnOff = !MotionMgr.Inst[nSelectedAxisNo].GetServoOnOff();

            MotionMgr.Inst[nSelectedAxisNo].SetServoOnOff(bSetOnOff);
            if (nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_1_Z_1 ||
                nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_2_T_6)
            {
                bTimeFlag = true;
            }
        }

        private void btnServoReset_Click(object sender, EventArgs e)
        {
            if (bTimeFlag) return;

            MotionMgr.Inst[nSelectedAxisNo].ResetAlarm();
            if (nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_1_Z_1 ||
                nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_2_T_6)
            {
                bTimeFlag = true;
            }
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            if (GbVar.mcState.IsRun()) return;
            if (bTimeFlag) return;
			string strTitle = FormTextLangMgr.FindKey("축 원점검색 (홈동작)");
            string strMsg = string.Format("{0} {1}", FormTextLangMgr.FindKey(SVDF.GetAxisName(nSelectedAxisNo)), FormTextLangMgr.FindKey("축의 원점검색을 하시겠습니까?"));
            POP.popMessageBox pMsgBox = new POP.popMessageBox(strMsg, strTitle);

            if (nSelectedAxisNo == (int)SVDF.AXES.MAGAZINE_ELV_Z)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_FORWARD_SENSOR] == 1
                    || GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_BACKWARD_SENSOR] != 1)
                {
                    popMessageBox pInterlockMsgBox = new popMessageBox("Strip Pusher Backward 상태가 아니면 Magazine ELV Z Home 구동을 할 수 없습니다.", "Interlock Check");
                    return;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT.MATERIAL_PROTRUSION_CHECK_SENSOR] == 0)
                {
                    popMessageBox pInterlockMsgBox = new popMessageBox("Magazine 에서 돌출된 Strip 이 감지되면 Magazine ELV Z Home 구동을 할 수 없습니다.", "Interlock Check");
                    return;
                }
            }

            if (nSelectedAxisNo == (int)SVDF.AXES.GD_TRAY_STG_1_Y
                || nSelectedAxisNo == (int)SVDF.AXES.GD_TRAY_STG_2_Y)
            {
                if (!GbFunc.IsGdTrayTableYMoveSafe())
                {
                    popMessageBox pInterlockMsgBox = new popMessageBox("GD Tray #1 / #2가 같은 높이에서 Home 구동을 할 수 없습니다.", "Interlock Check");
                    return;
                }
            }

            if (pMsgBox.ShowDialog() != DialogResult.OK) return;

            MotionMgr.Inst[nSelectedAxisNo].HomeStart();

            if (nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_1_Z_1 ||
                nSelectedAxisNo >= (int)SVDF.AXES.CHIP_PK_2_T_6)
            {
                bTimeFlag = true;
                GbVar.mcState.isHomeComplete[nSelectedAxisNo] = true;
            }
        }

        private void popNeonJog_Load(object sender, EventArgs e)
        {
            SetMotorList(0);

            string strValue = gridSelect.Rows[0].Cells[0].Value.ToString();

            string[] strAxisValue = strValue.Split('.');

            int.TryParse(strAxisValue[0], out nSelectedAxisNo);
        }

        private void btnJogCW_Click(object sender, EventArgs e)
        {

        }
    }
}
