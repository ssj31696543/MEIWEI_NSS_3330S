using DionesTool.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnMapTableVisionView : SeqBase
    {
        int m_nMapTableNo = 0;

        int m_nPickerHeadNo = 0;
        int m_nCurrViewCount = 0; 

        object[] m_args = null;

        public mnMapTableVisionView()
        {
        }

        public void SetParam(int nTableNo, int nHeadNo, params object[] args)
        {
            m_nMapTableNo = nTableNo;
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
                    if (m_nPickerHeadNo == 1 || m_nPickerHeadNo == 2)
                    {
                        nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_READY, m_nPickerHeadNo - 1);
                    }
                    
                    break;

                case 20:
                    if ((RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX * RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountY) <= m_nCurrViewCount)
                    {
                        NextSeq(30);
                        return;
                    }
                    break;

                case 22:
                    // View Pos move
                    nFuncResult = MovePosMapTableViewXYNext(m_nCurrViewCount, m_nMapTableNo, m_nPickerHeadNo);

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

        public int MovePosMapTableViewXYNext(int nViewCount, int nTableNo, int nHeadNo, long lDelay = 0, bool bCheckInterlock = true)
        {
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            double dVisionViewStartPosX;
            double dVisionViewStartPosY;

            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + nTableNo;

            if (nHeadNo == 0)
            {
                nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;

                dPosArray[0] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[0]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV + (3 * nTableNo)];
                dSpeedArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dVel[0];
                dAccArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dAcc[0];
                dDecArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dDec[0];

                dPosArray[1] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[1]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_MV + (3 * nTableNo)];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[0];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[0];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[0];
            }
            else if (nHeadNo == 1)
            {
                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X;

                dPosArray[0] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[0]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (3 * nTableNo)];
                dSpeedArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dVel[0];
                dAccArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dAcc[0];
                dDecArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dDec[0];

                dPosArray[1] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[1]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (3 * nTableNo)];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P1 + (3 * nTableNo)];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[0];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[0];
            }
            else
            {
                nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_2_X;

                dPosArray[0] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[0]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P2 + (3 * nTableNo)];
                dSpeedArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dVel[0];
                dAccArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dAcc[0];
                dDecArray[0] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[0]].dDec[0];

                dPosArray[1] = ConfigMgr.Inst.Cfg.dMotPos[nAxisArray[1]].dPos[POSDF.CFG_MAP_TABLE_1_OFFSET_START_P2 + (3 * nTableNo)];
                dSpeedArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dVel[0];
                dAccArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dAcc[0];
                dDecArray[1] = RecipeMgr.Inst.Rcp.dMotPos[nAxisArray[1]].dDec[0];
            }
            

            int nRow, nCol;
            double dStepOffsetX, dStepOffsetY;

            dVisionViewStartPosX = dPosArray[0];
            dVisionViewStartPosY = dPosArray[1];

            nRow = (nViewCount / RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);
            nCol = (nViewCount % RecipeMgr.Inst.Rcp.MapTbInfo.nUnitCountX);

            dStepOffsetX = nCol * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchX;
            dStepOffsetY = nRow * RecipeMgr.Inst.Rcp.MapTbInfo.dUnitPitchY;

            dPosArray[0] += dStepOffsetX;
            dPosArray[1] += dStepOffsetY;

            dxy dpMovePos;
            dpMovePos = ConfigMgr.Inst.GetMapStageInspCountAngleOffset(nTableNo, dVisionViewStartPosX, dVisionViewStartPosY, dPosArray[0], dPosArray[1]);

            dPosArray[0] = dpMovePos.x;
            dPosArray[1] = dpMovePos.y;

            int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[0], POSDF.MAP_PICKER_MAP_VISION_START_T1 + (2 * nTableNo));
            if (FNC.IsErr(nSafety)) return nSafety;

            nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[1], POSDF.MAP_STAGE_MAP_VISION_START);
            if (FNC.IsErr(nSafety)) return nSafety;

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }
    }
}
