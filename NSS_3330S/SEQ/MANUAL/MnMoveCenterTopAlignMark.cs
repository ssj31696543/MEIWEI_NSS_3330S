using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class MnMoveCenterTopAlignMark : SeqBase
    {
        int m_nMapTableNo = 0;

        tagRetAlignData[] _stAlignData = null;

        double[] _dCurrentPos = new double[2];

        public MnMoveCenterTopAlignMark()
        {
        }

        public void SetParam(int nTableNo)
        {
            m_nMapTableNo = nTableNo;

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
                    {
                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                        IFMgr.Inst.VISION.SetMapInspCountReset(false);
                        IFMgr.Inst.VISION.SetMapInspCountComplete(false);
                        IFMgr.Inst.VISION.SetMapStageNo(MCDF.eUNIT.NO1 + m_nMapTableNo);
                    }
                    break;
                case 2:
                    {
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);
                    }
                    break;
                case 4:
                    {
                        nFuncResult = MovePosMapVisionZ(POSDF.MAP_VISION_FOCUS_T1 + m_nMapTableNo);
                    }
                    break;
                case 6:
                    {
                        IFMgr.Inst.VISION.SetTopAlignCountReset(true);
                    }
                    break;
                case 8:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                                   IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_RESET_ON);
                            return;
                        }

                        if (!IFMgr.Inst.VISION.IsTopAlignCountReset) return;
                    }
                    break;
                case 10:
                    {
                        IFMgr.Inst.VISION.TrgMapOneShot();
                    }
                    break;
                case 12:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                               IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_ON);
                            return;
                        }
                        if (!IFMgr.Inst.VISION.IsTopAlignCompleted) return;

                        IFMgr.Inst.VISION.SetTopAlignCountComplete(true);
                    }
                    break;
                case 14:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                               IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_TOP_ALIGN_COMP_OFF);
                        }
                        if (IFMgr.Inst.VISION.IsTopAlignCompleted) return;

                        // [2022.04.13.kmlee] Complete 주기 전 Reset OFF
                        IFMgr.Inst.VISION.SetTopAlignCountReset(false);
                        IFMgr.Inst.VISION.SetTopAlignCountComplete(false);
                    }
                    break;
                case 16:
                    {
                        _stAlignData = IFMgr.Inst.VISION.GetTopAlignResult();
                    }
                    break;
                case 18:
                    {
                        if (!_stAlignData[0].bJudge)
                        {
                            SetError((int)ERDF.E_MAP_STAGE_1_TOP_AIGN_NG);
                            return;
                        }

                        _dCurrentPos[0] = MOTION.MotionMgr.Inst[SVDF.AXES.MAP_PK_X].GetRealPos();
                        _dCurrentPos[1] = MOTION.MotionMgr.Inst[SVDF.AXES.MAP_STG_1_Y + m_nMapTableNo].GetRealPos();
                    }
                    break;
                case 20:
                    {
                        nFuncResult = MoveCenterPos();
                    }
                    break;
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

        int MoveCenterPos(long lDelay = 0)
        {
            int[] nPosArray = new int[2];
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            nPosArray[0] = POSDF.MAP_PICKER_MAP_VISION_ALIGN_T1 + m_nMapTableNo;
            nPosArray[1] = POSDF.MAP_STAGE_TOP_ALIGN_MARK1;

            nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
            nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y + m_nMapTableNo;

            for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
            {
                dPosArray[nAxisCnt] = _dCurrentPos[nAxisCnt];
                dSpeedArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dVel[nPosArray[nAxisCnt]];
                dAccArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dAcc[nPosArray[nAxisCnt]];
                dDecArray[nAxisCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nAxisCnt]].dDec[nPosArray[nAxisCnt]];
            }

            dPosArray[0] += _stAlignData[0].dX;
            dPosArray[1] += _stAlignData[0].dY;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nAxisCnt]), nPosArray[nAxisCnt]),
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }
    }
}
