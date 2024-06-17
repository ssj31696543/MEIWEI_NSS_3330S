using DionesTool.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnTrayVisionView : SeqBase
    {
        int m_nTrayTableNo = 0;
        int m_nPickerHeadNo = 0;
        int m_nCurrViewCount = 0;

        object[] m_args = null;

        public mnTrayVisionView()
        {
            //
        }

        public void SetParam(int nTableNo, int nHeadNo, params object[] args)
        {
            m_nTrayTableNo = nTableNo;
            m_nPickerHeadNo = nHeadNo;

            m_args = args;
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        public override void ResetCmd()
        {
            base.ResetCmd();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;
            if (FINISH) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    m_nCurrViewCount = 0;
                    break;

                case 10:
                    // Picker Z Axis All Ready pos move
                    nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_READY, m_nPickerHeadNo);

                    break;

                case 20:
                    if ((RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX * RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountY) <= m_nCurrViewCount)
                    {
                        NextSeq(30);
                        return;
                    }
                    break;

                case 22:
                    // View Pos move
                    nFuncResult = MovePosTrayViewXYNext(m_nCurrViewCount, m_nTrayTableNo, m_nPickerHeadNo);

                    break;

                case 24:
                    // Delay
                    if (FNC.IsBusy(RunLib.msecDelay(1000))) return;
                    break;

                case 26:
                    // Count++
                    m_nCurrViewCount++;
                    NextSeq(20);
                    return;

                case 30:
                    FINISH = true;
                    return;

                default:
                    {
                        //System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
            m_nSeqNo++;
        }

        public int MovePosTrayViewXYNext(int nViewCount, int nTableNo, int nHeadNo, long lDelay = 0, bool bCheckInterlock = true)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            double dVisionViewStartPosX;
            double dVisionViewStartPosY;

            
            if (nTableNo == 0)
            {
                nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y;

                dPosArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            }
            else if (nTableNo == 1)
            {
                nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_2_Y;

                dPosArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];

            }
            else
            {
                nAxisArray[1] = (int)SVDF.AXES.RW_TRAY_STG_Y;

                dPosArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dPos[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[POSDF.TRAY_STAGE_TRAY_WORKING_P1 + nHeadNo];
            }

            if (nHeadNo == 0)
            {
                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;

                dPosArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dSpeedArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dAccArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dDecArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            }
            else
            {
                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;

                dPosArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dSpeedArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dVel[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dAccArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dAcc[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
                dDecArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dDec[POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1 + nTableNo];
            }


            int nRow, nCol;
            double dStepOffsetX, dStepOffsetY;

            dPosArray[0] += ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1 + nHeadNo, true).x;
            dPosArray[1] += ConfigMgr.Inst.GetCamToPickerOffset(CFG_DF.HEAD_1 + nHeadNo, true).y;

            dVisionViewStartPosX = dPosArray[0];
            dVisionViewStartPosY = dPosArray[1];

            nRow = (nViewCount / RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);
            nCol = (nViewCount % RecipeMgr.Inst.Rcp.TrayInfo.nTrayCountX);

            dStepOffsetX = nCol * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchX;
            dStepOffsetY = nRow * RecipeMgr.Inst.Rcp.TrayInfo.dTrayPitchY;

            dPosArray[0] += dStepOffsetX;
            dPosArray[1] -= dStepOffsetY;

            dxy dpMovePos;
            dpMovePos = ConfigMgr.Inst.GetTrayTablePlaceCountAngleOffset(nTableNo, nHeadNo, dVisionViewStartPosX, dVisionViewStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[0], dSpeedArray[0]);
            if (FNC.IsErr(nSafety)) return nSafety;

            nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[1], dSpeedArray[1]);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }
    }
}
