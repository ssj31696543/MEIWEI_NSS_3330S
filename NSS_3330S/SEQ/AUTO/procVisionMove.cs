using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    class procVisionMove : SeqBase
    {
        VisionMoveCmd _visionMoveCmd = new VisionMoveCmd();
        double _dCurrentPos = 0.0;

        public procVisionMove(int nSeqID)
        {
            m_nSeqID = nSeqID;
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
            //if (!IsAcceptRun()) return;

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
                        if (GbVar.LOADER_TEST)
                            return;
                        if (GbVar.g_queueVisionMoveCmd.Count == 0)
                            return;

                        if (!GbVar.g_queueVisionMoveCmd.TryDequeue(out _visionMoveCmd))
                            return;
                    }
                    break;
                case 1:
                    {
                        // 작동 중 등 Interlock 체크
                        // 설비가 Run 중이거나 등등
                        if (GbVar.mcState.IsRun())
                            return;

                        if (GbVar.mcState.isManualRun)
                            return;
                    }
                    break;
                case 2:
                    {
                        GetCurrentPos();
                    }
                    break;
                case 3:
                    {
                        // 이동
                        nFuncResult = MoveVisionPos1();
                        //nFuncResult = (int)ERDF.E_SV_SERVO_ON;

                        if (FNC.IsErr(nFuncResult))
                        {
                            GbVar.g_bResponseJogError = true;
                            NextSeq(6);
                            return;
                        }
                    }
                    break;
                case 4:
                    {
                        // 이동
                        nFuncResult = MoveVisionPos2();
                        //nFuncResult = (int)ERDF.E_SV_SERVO_ON;

                        if (FNC.IsErr(nFuncResult))
                        {
                            GbVar.g_bResponseJogError = true;
                            NextSeq(6);
                            return;
                        }
                    }
                    break;
                case 5:
                    {
                        // 이동
                        nFuncResult = MoveVisionPos3();
                        //nFuncResult = (int)ERDF.E_SV_SERVO_ON;

                        if (FNC.IsErr(nFuncResult))
                        {
                            GbVar.g_bResponseJogError = true;
                            NextSeq(6);
                            return;
                        }
                    }
                    break;
                case 6:
                    {
                        NextSeq(0);
                        return;
                    }
                    //break;
                case 7:
                    {
                        // Error 시 Queue에 있는 것을 모두 삭제
                        VisionMoveCmd cmd;
                        while (GbVar.g_queueVisionMoveCmd.TryDequeue(out cmd))
                        {
                            // 삭제용이므로 아무것도 안함
                        }
                    }
                    break;
                case 8:
                    {
                        NextSeq(0);
                        return;
                    }
                //break;
                default:
                    {
                        NextSeq(0);
                        return;
                    }
            }

            m_bFirstSeqStep = false;

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
        }

        protected void GetCurrentPos()
        {
            int nAxis;
            switch (_visionMoveCmd._cmdRecv)
            {
                case IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                    {
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                    {
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.X_PITCH:
                    {
                        nAxis = (int)SVDF.AXES.MAP_PK_X;

                        _dCurrentPos = MOTION.MotionMgr.Inst[nAxis].GetRealPos();
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Y1_PITCH:
                    {
                        nAxis = (int)SVDF.AXES.MAP_STG_1_Y;

                        _dCurrentPos = MOTION.MotionMgr.Inst[nAxis].GetRealPos();
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Y2_PITCH:
                    {
                        nAxis = (int)SVDF.AXES.MAP_STG_2_Y;

                        _dCurrentPos = MOTION.MotionMgr.Inst[nAxis].GetRealPos();
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Z_PITCH:
                    {
                        nAxis = (int)SVDF.AXES.MAP_VISION_Z;

                        _dCurrentPos = MOTION.MotionMgr.Inst[nAxis].GetRealPos();
                    }
                    break;
                default:
                    break;
            }
        }

        protected int MoveVisionPos1(long lDelay = 0, bool bSkipMapPkInterlock = false)
        {
            int nMoveAxisCnt = 1;
            int nPosNo = 0;

            switch (_visionMoveCmd._cmdRecv)
            {
                case IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                case IFVision.E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                    {
                        nMoveAxisCnt = 2;
                    }
                    break;
            }

            int[] nAxisArray = new int[nMoveAxisCnt];
            double[] dPosArray = new double[nMoveAxisCnt];
            double[] dSpeedArray = new double[nMoveAxisCnt];
            double[] dAccArray = new double[nMoveAxisCnt];
            double[] dDecArray = new double[nMoveAxisCnt];

            switch (_visionMoveCmd._cmdRecv)
            {
                case IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_PK_Z;
                        nAxisArray[1] = (int)SVDF.AXES.MAP_VISION_Z;

                        dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                        dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[nPosNo];
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_PK_Z;
                        nAxisArray[1] = (int)SVDF.AXES.MAP_VISION_Z;

                        dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                        dPosArray[1] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[1]].dPos[nPosNo];
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.X_PITCH:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;

                        dPosArray[0] = _dCurrentPos + _visionMoveCmd._dPos;
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Y1_PITCH:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_STG_1_Y;

                        dPosArray[0] = _dCurrentPos + _visionMoveCmd._dPos;
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Y2_PITCH:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_STG_2_Y;

                        dPosArray[0] = _dCurrentPos + _visionMoveCmd._dPos;
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.Z_PITCH:
                    {
                        nAxisArray[0] = (int)SVDF.AXES.MAP_VISION_Z;

                        dPosArray[0] = _dCurrentPos + _visionMoveCmd._dPos;
                    }
                    break;
                default:
                    return FNC.SUCCESS;
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                //dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNo];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNo];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNo];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNo];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                string.Format("Vision Move : {0}", _visionMoveCmd._cmdRecv),
                                                dPosArray[nCnt]);
                }
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, true, true, bSkipMapPkInterlock);
        }

        protected int MoveVisionPos2(long lDelay = 0, bool bSkipMapPkInterlock = false)
        {
            int nMoveAxisCnt = 3;
            int[] nPosNoArray = new int[nMoveAxisCnt];

            int[] nAxisArray = new int[nMoveAxisCnt];
            double[] dPosArray = new double[nMoveAxisCnt];
            double[] dSpeedArray = new double[nMoveAxisCnt];
            double[] dAccArray = new double[nMoveAxisCnt];
            double[] dDecArray = new double[nMoveAxisCnt];

            // MAP_PK_Z
            // MAP_PK_X
            // MAP_STG_1_Y
            // MAP_STG_2_Y
            // MAP_STG_1_T
            // MAP_STG_2_T
            // MAP_VISION_Z
            switch (_visionMoveCmd._cmdRecv)
            {
                case IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                    {
                        nPosNoArray[0] = POSDF.MAP_PICKER_MAP_VISION_START_T1;
                        nPosNoArray[1] = POSDF.MAP_STAGE_MAP_VISION_START;
                        nPosNoArray[2] = POSDF.MAP_STAGE_MAP_VISION_START;

                        nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
                        nAxisArray[1] = (int)SVDF.AXES.MAP_STG_1_Y;
                        nAxisArray[2] = (int)SVDF.AXES.MAP_STG_1_T;
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                    {
                        nPosNoArray[0] = POSDF.MAP_PICKER_MAP_VISION_START_T2;
                        nPosNoArray[1] = POSDF.MAP_STAGE_MAP_VISION_START;
                        nPosNoArray[2] = POSDF.MAP_STAGE_MAP_VISION_START;

                        nAxisArray[0] = (int)SVDF.AXES.MAP_PK_X;
                        nAxisArray[1] = (int)SVDF.AXES.MAP_STG_2_Y;
                        nAxisArray[2] = (int)SVDF.AXES.MAP_STG_2_T;
                    }
                    break;
                default:
                    return FNC.SUCCESS;
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNoArray[nCnt]];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNoArray[nCnt]];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNoArray[nCnt]];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNoArray[nCnt]];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                string.Format("Vision Move : {0}", _visionMoveCmd._cmdRecv),
                                                dPosArray[nCnt]);
                }
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, true, true, bSkipMapPkInterlock);
        }

        protected int MoveVisionPos3(long lDelay = 0, bool bSkipMapPkInterlock = false)
        {
            int nMoveAxisCnt = 1;
            int[] nPosNoArray = new int[nMoveAxisCnt];

            int[] nAxisArray = new int[nMoveAxisCnt];
            double[] dPosArray = new double[nMoveAxisCnt];
            double[] dSpeedArray = new double[nMoveAxisCnt];
            double[] dAccArray = new double[nMoveAxisCnt];
            double[] dDecArray = new double[nMoveAxisCnt];

            // MAP_PK_Z
            // MAP_PK_X
            // MAP_STG_1_Y
            // MAP_STG_2_Y
            // MAP_STG_1_T
            // MAP_STG_2_T
            // MAP_VISION_Z
            switch (_visionMoveCmd._cmdRecv)
            {
                case IFVision.E_VS_RECV_CMD.MAPBLOCK1_TEACHING:
                    {
                        nPosNoArray[0] = POSDF.MAP_PICKER_MAP_VISION_START_T1;

                        nAxisArray[0] = (int)SVDF.AXES.MAP_VISION_Z;
                    }
                    break;
                case IFVision.E_VS_RECV_CMD.MAPBLOCK2_TEACHING:
                    {
                        nPosNoArray[0] = POSDF.MAP_PICKER_MAP_VISION_START_T2;

                        nAxisArray[0] = (int)SVDF.AXES.MAP_VISION_Z;
                    }
                    break;
                default:
                    return FNC.SUCCESS;
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dPos[nPosNoArray[nCnt]];
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosNoArray[nCnt]];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosNoArray[nCnt]];
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosNoArray[nCnt]];
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                string.Format("Vision Move : {0}", _visionMoveCmd._cmdRecv),
                                                dPosArray[nCnt]);
                }
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay, true, true, bSkipMapPkInterlock);
        }
    }
}
