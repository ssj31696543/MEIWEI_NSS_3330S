using NSSU_3400.MOTION;
using NSSU_3400.POP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSSU_3400.UC
{
    public partial class ucCleanerCyclePosMove : UserControl
    {
        protected POP.popManualRun m_popManualRun = null;

        public ucCleanerCyclePosMove()
        {
            InitializeComponent();
        }

        private void btnUnitPickerX_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.UNIT_PK_X;
            int nPosNo = 0;
            switch (nTag)
            {
                case 0:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_LOADING_TABLE_1;
                    }
                    break;
                case 1:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_LOADING_TABLE_2;
                    }
                    break;
                case 2:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_SCRAP_1;
                    }
                    break;
                case 3:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_SCRAP_2;
                    }
                    break;
                case 4:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_SPONGE_SWING_START;
                    }
                    break;
                case 5:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_SPONGE_SWING_END;
                    }
                    break;
                case 6:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_CLEAN_1;
                    }
                    break;
                case 7:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_US_1;
                    }
                    break;
                case 8:
                    {
                        nPosNo = (int)POSDF.UNIT_PICKER_CLEAN_2_UNLOAD;
                    }
                    break;
                default:
                    break;
            }
            if (!SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.UNIT_PK_Z, (int)POSDF.UNIT_PICKER_READY))
            {
                popMessageBox pMsg = new popMessageBox("클리너피커 Z축 또는 T축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            {
                MovePos(nAxisNo, nPosNo);
            }
        }

        private void btnUnitPickerZ_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.UNIT_PK_Z;
            int nAxisNoX = (int)SVDF.AXES.UNIT_PK_X;

            int nPosNo = (int)POSDF.UNIT_PICKER_READY;
            if (nTag == 0)
            {
                nPosNo = (int)POSDF.UNIT_PICKER_READY;
            }
            else
            {
                if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_LOADING_TABLE_1))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_LOADING_TABLE_1;
                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_LOADING_TABLE_2))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_LOADING_TABLE_2;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_SCRAP_1))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_SCRAP_1;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_SCRAP_2))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_SCRAP_2;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_SPONGE_SWING_START))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_SPONGE_SWING_START;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_SPONGE_SWING_END))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_SPONGE_SWING_START;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_CLEAN_1))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_CLEAN_1;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_US_1))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_US_1;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.UNIT_PICKER_CLEAN_2_UNLOAD))
                {
                    nPosNo = (int)POSDF.UNIT_PICKER_CLEAN_2_UNLOAD;

                }
                else
                {
                    popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                    if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                    nPosNo = (int)POSDF.UNIT_PICKER_READY;
                }
            }
            
            MovePos(nAxisNo, nPosNo);
        }

        private void btnCleanerPickerX_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_X;
            int nPosNo = 0;
            switch (nTag)
            {
                case 0:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_READY;
                    }
                    break;
                case 1:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_CLEAN;
                    }
                    break;
                case 2:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING;
                    }
                    break;
                case 3:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING;
                    }
                    break;
                case 4:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM;
                    }
                    break;
                case 5:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_END_BOTTOM;
                    }
                    break;
                case 6:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_ULTRASONIC;
                    }
                    break;
                case 7:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC;
                    }
                    break;
                case 8:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC;
                    }
                    break;
                //case 9:
                //case 10:
                //    {
                //        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM;
                //    }
                //    break;
                case 11:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_PLASMA; 
                    }
                    break;
                case 12:
                    {
                        nPosNo = (int)POSDF.CLEANER_PICKER_DRY_TABLE;
                    }
                    break;
                default:
                    break;
            }
            if ( SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_R, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA) &&
                !SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_Z, (int)POSDF.CLEANER_PICKER_READY) &&
                !SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_Z, (int)POSDF.CLEANER_PICKER_SAFETY_MOVE))
            {
                popMessageBox pMsg = new popMessageBox("클리너피커 Z축 또는 T축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            {
                MovePos(nAxisNo, nPosNo);
            }
        }

        private void btnCleanerPickerZ_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_Z;
            int nAxisNoX = (int)SVDF.AXES.CL_PICKER_X;
            int nPosNo = 0;

            if (nTag == 0)
            {
                nPosNo = (int)POSDF.CLEANER_PICKER_READY;
            }
            else
            {
                if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_READY))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_READY;
                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_SECOND_CLEAN))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_CLEAN;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_END_ON_CLEANING))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_END_BOTTOM))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_SECOND_ULTRASONIC))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_ULTRASONIC;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC;

                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_END_ON_ULTRASONIC))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC;
                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_SECOND_PLASMA;
                }
                else if (SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_TABLE))
                {
                    nPosNo = (int)POSDF.CLEANER_PICKER_DRY_TABLE;
                }
                else
                {
                    popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
                    if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                    nPosNo = (int)POSDF.CLEANER_PICKER_READY;
                }
            }

            MovePos(nAxisNo, nPosNo);
        }

        private void btnCleanerPickerT_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_R;
            int nAxisNoX = (int)SVDF.AXES.CL_PICKER_X;
            int nPosNo = 0;


            switch (nTag)
            {
                case 0:
                case 2:
                    {
                        if (!SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_READY) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_DRY_TABLE) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNoX, (int)POSDF.CLEANER_PICKER_SAFETY_MOVE))
                        {
                            popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다?", "알림", MessageBoxButtons.OK);
                            pMsg.ShowDialog();
                            return;
                        }
                        else
                        {
                            if (nTag == 0)
                            {
                                MovePos(nAxisNo, (int)POSDF.CLEANER_PICKER_READY);
                            }
                            else
                            {
                                MovePos(nAxisNo, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA);
                            }
                        }
                    }
                    break;
                case 1:
                    {
                            //뒤집혀 있으면 움직이면 안됨
                        if ( SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA) &&
                            //아래 주석들은 조금씩 차이 나므로 움직여도 됨
                            !SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_SECOND_CLEAN) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_SECOND_ULTRASONIC) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_DRY_START_BOTTOM) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_DRY_START_ON_CLEANING) &&
                            !SafetyMgr.Inst.IsInPosNo(nAxisNo, (int)POSDF.CLEANER_PICKER_DRY_START_ON_ULTRASONIC)
                            )
                        {
                            popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다?", "알림", MessageBoxButtons.OK);
                            pMsg.ShowDialog();
                            return;
                        }
                        else
                        {
                            double dPosCleanerPickerX = MotionMgr.Inst[SVDF.AXES.CL_PICKER_R].GetRealPos();
                            double dTargetPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.CL_PICKER_R].dPos[nPosNo];
                            if (Math.Abs(dPosCleanerPickerX - dTargetPos) > 5)
                            {
                                popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다? 조그 이동 하십시오.", "알림", MessageBoxButtons.OK);
                                pMsg.ShowDialog();
                                return;
                            }
                            else
                            {
                                MovePos(nAxisNo, nPosNo);
                            }
                        }
                    }
                    break;
               
                default:
                    break;
            }
            
        }

        private void btnDryZ_Move_Click(object sender, EventArgs e)
        {
            //Button btn = (Button)sender;
            //int nTag = btn.GetTag();
            //int nAxisNo = (int)SVDF.AXES.CL_DRY_Z;
            //int nAxisNoX = (int)SVDF.AXES.CL_DRY_X;
            //int nAxisNoY = (int)SVDF.AXES.CL_DRY_Y;

            //double dTargetPos = 0;
            //double dSpeed = 0;
            //double dAcc = 0;
            //double dDcc = 0;


            //if (nTag == 0)
            //{
            //    dTargetPos = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dPos[(int)POSDF.PLASMA_DRY_READY];
            //    dSpeed = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dVel[(int)POSDF.PLASMA_DRY_READY];
            //    dAcc = dSpeed * 10;
            //    dDcc = dSpeed * 10;
            //}
            //else
            //{
            //    double dCurPosX = MotionMgr.Inst[nAxisNoX].GetRealPos();
            //    double dPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
            //    double dCurPosY = MotionMgr.Inst[nAxisNoY].GetRealPos();
            //    double dPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
            //    if (SafetyMgr.Inst.IsWithInRangePos(dCurPosX, dPosX, 0.1))
            //    {
            //        if (SafetyMgr.Inst.IsWithInRangePos(dCurPosY, dPosY, 0.1))
            //        {
            //            dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaZPos;
            //            dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
            //            dAcc = dSpeed * 10;
            //            dDcc = dSpeed * 10;
            //        }
            //    }
            //    dCurPosX = MotionMgr.Inst[nAxisNoX].GetRealPos();
            //    dPosX = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos;
            //    dCurPosY = MotionMgr.Inst[nAxisNoY].GetRealPos();
            //    dPosY = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos;
            //    if (SafetyMgr.Inst.IsWithInRangePos(dCurPosX, dPosX, 0.1))
            //    {
            //        if (SafetyMgr.Inst.IsWithInRangePos(dCurPosY, dPosY, 0.1))
            //        {
            //            dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaZPos;
            //            dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
            //            dAcc = dSpeed * 10;
            //            dDcc = dSpeed * 10;
            //        }
            //    }

            //    dCurPosX = MotionMgr.Inst[nAxisNoX].GetRealPos();
            //    dPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
            //    dCurPosY = MotionMgr.Inst[nAxisNoY].GetRealPos();
            //    dPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
            //    if (SafetyMgr.Inst.IsWithInRangePos(dCurPosX, dPosX, 0.1))
            //    {
            //        if (SafetyMgr.Inst.IsWithInRangePos(dCurPosY, dPosY, 0.1))
            //        {
            //            dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaZPos;
            //            dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
            //            dAcc = dSpeed * 10;
            //            dDcc = dSpeed * 10;
            //        }
            //    }

            //    dCurPosX = MotionMgr.Inst[nAxisNoX].GetRealPos();
            //    dPosX = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos;
            //    dCurPosY = MotionMgr.Inst[nAxisNoY].GetRealPos();
            //    dPosY = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos;
            //    if (SafetyMgr.Inst.IsWithInRangePos(dCurPosX, dPosX, 0.1))
            //    {
            //        if (SafetyMgr.Inst.IsWithInRangePos(dCurPosY, dPosY, 0.1))
            //        {
            //            dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaZPos;
            //            dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
            //            dAcc = dSpeed * 10;
            //            dDcc = dSpeed * 10;
            //        }
            //    }

            //    if (dSpeed == 0)
            //    {
            //        popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
            //        if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            //        dTargetPos = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dPos[(int)POSDF.PLASMA_DRY_READY];
            //        dSpeed = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dVel[(int)POSDF.PLASMA_DRY_READY];
            //        dAcc = dSpeed * 10;
            //        dDcc = dSpeed * 10;
            //    }

            //    //else
            //    //{
            //    //    popMessageBox pMsg = new popMessageBox("위치가 올바르지 않습니다. 대기 위치로 보내겠습니까?", "알림");
            //    //    if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            //    //    nPosNo = (int)POSDF.CLEANER_PICKER_READY;
            //    //}
            //}

            MovePos(nAxisNo, dTargetPos, dSpeed, dAcc, dDcc);
        }

        private void btnDryX_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_DRY_X;

            double dTargetPos = 0;
            double dSpeed = 0;
            double dAcc = 0;
            double dDcc = 0;

            switch (nTag)
            {
                case 0:
                    {
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dPos[(int)POSDF.PLASMA_DRY_READY];
                        dSpeed = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dVel[(int)POSDF.PLASMA_DRY_READY];
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 1:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartXPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 2:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndXPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 3:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartXPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 4:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndXPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                
                default:
                    break;
            }
            if (SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_R, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA))
            {
                popMessageBox pMsg = new popMessageBox("클리너피커 T축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            { 
                MovePos(nAxisNo, dTargetPos, dSpeed, dAcc, dDcc);
            }
        }

        private void btnDryY_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_DRY_Y;

            double dTargetPos = 0;
            double dSpeed = 0;
            double dAcc = 0;
            double dDcc = 0;

            switch (nTag)
            {
                case 0:
                    {
                        dTargetPos = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dPos[(int)POSDF.PLASMA_DRY_READY];
                        dSpeed = TeachMgr.Inst.Tch.dMotPos[nAxisNo].dVel[(int)POSDF.PLASMA_DRY_READY];
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 1:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaStartYPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 2:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaEndYPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dFirstPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 3:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaStartYPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;
                case 4:
                    {
                        dTargetPos = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaEndYPos;
                        dSpeed = RecipeMgr.Inst.Rcp.cleaning.dSecondPlasmaMoveVel;
                        dAcc = dSpeed * 10;
                        dDcc = dSpeed * 10;
                    }
                    break;

                default:
                    break;
            }
            if (SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_R, (int)POSDF.CLEANER_PICKER_SECOND_PLASMA))
            {
                popMessageBox pMsg = new popMessageBox("클리너피커 T축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            {

                MovePos(nAxisNo, dTargetPos, dSpeed, dAcc, dDcc);
            }
        }

        private void btnFlipElv_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_FLIP_ELEVATOR;
            int nPosNo = 0;
            switch (nTag)
            {
                case 0:
                    {
                        nPosNo = (int)POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD;
                    }
                    break;
                case 1:
                    {
                        nPosNo = (int)POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK;
                    }
                    break;
                default:
                    break;
            }
            if (!SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_FLIP_2, (int)POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD) &&
                !SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_FLIP_2, (int)POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK))
            {
                popMessageBox pMsg = new popMessageBox("클리너 플립2 T축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            {
                MovePos(nAxisNo, nPosNo);
            }
        }

        private void btnFlip1T_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_FLIP_1;
            int nPosNo = 0;
            switch (nTag)
            {
                case 0:
                    {
                        nPosNo = (int)POSDF.CLEANER_SECOND_CLEAN_LOAD;
                    }
                    break;
                case 1:
                    {
                        nPosNo = (int)POSDF.CLEANER_SECOND_CLEAN_WORK;
                    }
                    break;
                default:
                    break;
            }
            MovePos(nAxisNo, nPosNo);
        }

        private void btnFlip2T_Move_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int nTag = btn.GetTag();
            int nAxisNo = (int)SVDF.AXES.CL_PICKER_FLIP_2;
            int nPosNo = 0;
            switch (nTag)
            {
                case 2:
                    {
                        nPosNo = (int)POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD;
                    }
                    break;
                case 3:
                    {
                        nPosNo = (int)POSDF.SECOND_ULTRASONIC_ULTRASONIC_WORK;
                    }
                    break;
                default:
                    break;
            }
            if (!SafetyMgr.Inst.IsInPosNo((int)SVDF.AXES.CL_PICKER_FLIP_ELEVATOR, (int)POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD))
            {
                popMessageBox pMsg = new popMessageBox("플립 엘리베이터 Z축 상태가 올바르지 않습니다", "알림", MessageBoxButtons.OK);
                pMsg.ShowDialog();
                return;
            }
            else
            {
                MovePos(nAxisNo, nPosNo);
            }
        }
        void MovePos(int nAxisNo, int nPosNo)
        {
            // MOVE
            double dTargetPos = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dPos[nPosNo];
            double dSpeed = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dVel[nPosNo];
            double dAcc = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dAcc[nPosNo];
            double dDec = TeachMgr.Inst.TempTch.dMotPos[nAxisNo].dDec[nPosNo];

            if (!GbVar.MANUAL_MOVE_CHECK_SKIP)
            {
                string strTitle = "축 이동";
                string strMsg = string.Format("{0} 축을 {1} 으로 이동하시겠습니까?\n 속도:{2} 가속도:{3} 감속도:{4} ",
                    SVDF.GetAxisName(nAxisNo), dTargetPos.ToString("F3"), dSpeed.ToString("0.0"), dAcc.ToString("0.0"), dDec.ToString("0.0"));
                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            }

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
            if (!GbVar.MANUAL_MOVE_CHECK_SKIP)
            {
                string strTitle = "축 이동";
                string strMsg = string.Format("{0} 축을 {1} 으로 이동하시겠습니까?\n 속도:{2} 가속도:{3} 감속도:{4} ",
                    SVDF.GetAxisName(nAxisNo), dTargetPos.ToString("F3"), dSpeed.ToString("0.0"), dAcc.ToString("0.0"), dDec.ToString("0.0"));
                popMessageBox pMsg = new popMessageBox(strMsg, strTitle);
                if (pMsg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;
            }

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
