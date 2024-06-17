using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.MOTION;

namespace NSS_3330S.SEQ.MANUAL
{
    public class MnPickerCalibration : SeqBase
    {
        int m_nPickerNo = 0;
        int m_nAngleIndex = 0;
        int m_nAngleOffset = 0;
        double m_dPickerPitchX = 1.0;

        int m_nPickerPadNo = 0;

        tagRetAlignData[] _stAlignData = null;
        double[] m_dCenterX = null;
        double[] m_dCenterY = null;

        int m_nMaxRetryCount = 5;
        int m_nRetryCount = 0;
        double m_dTolerence = 0.005;
        bool m_bMovePitch = false;

        bool m_bPnpPos = false;

        // 데이터 접근 시에만 사용하는 인덱스
        public int DATA_PICKER_PAD_NO
        {
            get
            {
                if(m_bPnpPos)
                {
                    return m_nPickerPadNo + CFG_DF.MAX_PICKER_PAD_CNT;
                }

                return m_nPickerPadNo;
            }
        }

        public MnPickerCalibration()
        {
            _stAlignData = new tagRetAlignData[CFG_DF.MAX_PICKER_PAD_CNT * 2];
            m_dCenterX = new double[CFG_DF.MAX_PICKER_PAD_CNT * 2];
            m_dCenterY = new double[CFG_DF.MAX_PICKER_PAD_CNT * 2];
        }

        public void SetParam(int nPickerNo, int nAngleIndex, double dPickerPitch, int nMaxRetryCount, double dTolerence, bool bMovePitch)
        {
            m_nPickerNo = nPickerNo;
            m_nAngleIndex = nAngleIndex;
            m_dPickerPitchX = dPickerPitch;
            m_nMaxRetryCount = nMaxRetryCount;
            m_dTolerence = dTolerence;
            m_bMovePitch = bMovePitch;

            m_nAngleOffset = (90 * m_nAngleIndex);
            if(m_nAngleIndex >= 4)
            {
                m_nAngleOffset = -90 * (m_nAngleIndex - 3);
            }
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
#if !_NOTEBOOK
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].bOptionUse &&
                            IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VISION_INTERFACE_TIMEOUT].lValue))
                        {
                            SetError((int)ERDF.E_VISION_PROGRAM_INTERFACE_TIMEOUT_READY);
                            return;
                        }
                        if (!IFMgr.Inst.VISION.IsVisionReady) return;
#endif

                        IFMgr.Inst.VISION.SetPickerCalLedOff(false);
                        IFMgr.Inst.VISION.SetPickerCalLedOn(true);
                        IFMgr.Inst.VISION.SetPickerCalReset(false);
                        IFMgr.Inst.VISION.SetPickerCalComplete(false);

                        m_nPickerPadNo = 0;
                        m_bPnpPos = false;
                    }
                    break;
                case 1:
                    {
                        if (!IsDelayOver(1000)) return;

                        IFMgr.Inst.VISION.SetPickerCalLedOn(false);
                    }
                    break;
                case 2:
                    {
                        nFuncResult = MovePosChipPkAllZR(POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF, m_nPickerNo, m_nAngleOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "STRIP PICKER ZT AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                // 피치 위치 또는 저장된 위치 이동 (시작 위치)
                case 4:
                    {
                        if (m_bFirstSeqStep)
                        {
                        }

                        nFuncResult = MovePickerCalPosXYZ(m_nPickerPadNo, 200);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            m_nRetryCount = 0;
                        }
                    }
                    break;
                // Interface 시작
                case 6:
                    {
                        IFMgr.Inst.VISION.SetPickerCalReset(true);
                    }
                    break;
                // Z축 INSPECTION 위치로 이동
                case 7:
                    {
                        if (m_bFirstSeqStep)
                        {
                        }

                        if (m_bPnpPos)
                        {
                            nFuncResult = MovePickerCalPnpPosZ(m_nPickerPadNo);
                        }
                        else
                        {
                            nFuncResult = MovePickerCalInspPosZ(m_nPickerPadNo);
                        }

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                // RESET 응답 대기
                case 8:
                    {
#if !_NOTEBOOK
                        if (!IFMgr.Inst.VISION.IsPickerCalReset) return;
#endif
                    }
                    break;
                // 트리거
                case 10:
                    {
                        IFMgr.Inst.VISION.TrgBallOneShot();
                    }
                    break;
                // COMPLETE ON 응답 대기
                case 12:
                    {
#if !_NOTEBOOK
                        if (!IFMgr.Inst.VISION.IsPickerCalCompleted) return;
#endif

                        IFMgr.Inst.VISION.SetPickerCalReset(false);
                        IFMgr.Inst.VISION.SetPickerCalComplete(true);
                    }
                    break;
                // 딜레이
                case 13:
                    {
                        if (!IsDelayOver(100)) return;
                    }
                    break;
                // COMPLETE OFF 응답 대기
                case 14:
                    {
#if !_NOTEBOOK
                        if (IFMgr.Inst.VISION.IsTopAlignCompleted) return;
#endif

                        IFMgr.Inst.VISION.SetPickerCalComplete(false);
                    }
                    break;
                // Z축 올리기 (X 위치 이동 위해)
                case 15:
                    {
                        if (m_bFirstSeqStep)
                        {
                        }

                        nFuncResult = MovePickerResetPosZ(m_nPickerPadNo);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                // 결과 값 가져오기
                case 16:
                    {
                        _stAlignData[DATA_PICKER_PAD_NO] = IFMgr.Inst.VISION.GetPickerCalResult();

                        m_dCenterX[DATA_PICKER_PAD_NO] = MotionMgr.Inst[SVDF.AXES.CHIP_PK_1_X + m_nPickerNo].GetRealPos() - _stAlignData[DATA_PICKER_PAD_NO].dX;
                        m_dCenterY[DATA_PICKER_PAD_NO] = MotionMgr.Inst[SVDF.AXES.BALL_VISION_Y].GetRealPos() - _stAlignData[DATA_PICKER_PAD_NO].dY;
#if !_NOTEBOOK
                        if (!_stAlignData[DATA_PICKER_PAD_NO].bJudge)
                        {
                            SetError((int)ERDF.E_PICKER_CAL_PATTERN_NOT_FOUND);
                            return;
                        }

                        if (Math.Abs(_stAlignData[DATA_PICKER_PAD_NO].dX) <= m_dTolerence && Math.Abs(_stAlignData[DATA_PICKER_PAD_NO].dY) <= m_dTolerence)
                        {
                            NextSeq(25);
                            return;
                        }
                        else
                        {
                            if (m_nRetryCount >= m_nMaxRetryCount)
                            {
                                SetError((int)ERDF.E_PICKER_CAL_RETRY_OVER);
                                return;
                            }
                            m_nRetryCount++;
                        }
#else
                        NextSeq(25);
                        return;
#endif
                    }
                    break;
                // 찾은 Vision Offset 적용하여 이동
                case 18:
                    {
                        if (m_bFirstSeqStep)
                        {
                        }
                        nFuncResult = MovePickerCalCenterPos(200);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                        }
                    }
                    break;
                // Picker Cal 재시도 위치 이동
                case 20:
                    {
                        NextSeq(6);
                        return;
                    }
                //break;

                // Inspection Pos 후 PnP Pos 변환
                case 25:
                    {
                        if (m_bPnpPos == false)
                        {
                            m_bPnpPos = true;
                            NextSeq(4);
                            return;
                        }

                        m_bPnpPos = false;
                    }
                    break;
                // Picker Pad Count 증가
                case 26:
                    {
                        m_nPickerPadNo++;

                        if (m_nPickerPadNo < CFG_DF.MAX_PICKER_PAD_CNT)
                        {
                            NextSeq(4);
                            return;
                        }
                    }
                    break;

                // 값 갱신
                case 28:
                    {
                        UpdateDataToTempCfg();

                        IFMgr.Inst.VISION.SetPickerCalLedOff(true);
                    }
                    break;
                case 29:
                    {
                        if (!IsDelayOver(1000)) return;
                    }
                    break;
                case 30:
                    IFMgr.Inst.VISION.SetPickerCalLedOff(false);

                    FINISH = true;
                    return;

                default:
                    {
                        //System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            m_bFirstSeqStep = false;

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

        protected int MovePosChipPkAllZR(int nPosNo, int nHeadNo, int nAngleOffset, long lDelay = 0)
        {
            int[] nAxisArray = new int[16];
            double[] dPosArray = new double[16];
            double[] dSpeedArray = new double[16];
            double[] dAccArray = new double[16];
            double[] dDecArray = new double[16];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * nHeadNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset;
            nAxisArray[1] = (int)SVDF.AXES.CHIP_PK_1_Z_2 + nHeadOffset;
            nAxisArray[2] = (int)SVDF.AXES.CHIP_PK_1_Z_3 + nHeadOffset;
            nAxisArray[3] = (int)SVDF.AXES.CHIP_PK_1_Z_4 + nHeadOffset;
            nAxisArray[4] = (int)SVDF.AXES.CHIP_PK_1_Z_5 + nHeadOffset;
            nAxisArray[5] = (int)SVDF.AXES.CHIP_PK_1_Z_6 + nHeadOffset;
            nAxisArray[6] = (int)SVDF.AXES.CHIP_PK_1_Z_7 + nHeadOffset;
            nAxisArray[7] = (int)SVDF.AXES.CHIP_PK_1_Z_8 + nHeadOffset;

            nAxisArray[8] = (int)SVDF.AXES.CHIP_PK_1_T_1 + nHeadOffset;
            nAxisArray[9] = (int)SVDF.AXES.CHIP_PK_1_T_2 + nHeadOffset;
            nAxisArray[10] = (int)SVDF.AXES.CHIP_PK_1_T_3 + nHeadOffset;
            nAxisArray[11] = (int)SVDF.AXES.CHIP_PK_1_T_4 + nHeadOffset;
            nAxisArray[12] = (int)SVDF.AXES.CHIP_PK_1_T_5 + nHeadOffset;
            nAxisArray[13] = (int)SVDF.AXES.CHIP_PK_1_T_6 + nHeadOffset;
            nAxisArray[14] = (int)SVDF.AXES.CHIP_PK_1_T_7 + nHeadOffset;
            nAxisArray[15] = (int)SVDF.AXES.CHIP_PK_1_T_8 + nHeadOffset;


            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                // T축
                if (nCnt >= CFG_DF.MAX_PICKER_PAD_CNT)
                {
                    if (nPosNo == POSDF.CHIP_PICKER_READY || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 || nPosNo == POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2)
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo];// +ConfigMgr.Inst.GetPlaceAngleOffset(0, nHeadNo, nCnt - CFG_DF.MAX_PICKER_PAD_CNT);
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                    else
                    {
                        dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dPos[nPosNo] + nAngleOffset;//ConfigMgr.Inst.GetPlaceAngleOffset();
                        dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dVel[nPosNo];
                        dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dAcc[nPosNo];
                        dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[8]].dDec[nPosNo];
                    }
                }
                // Z축
                else
                {
                    dPosArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[nPosNo];
                    dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dVel[nPosNo];
                    dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dAcc[nPosNo];
                    dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dDec[nPosNo];
                }
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";
                for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                                                SVDF.GetAxisName(nAxisArray[nCnt]),
                                                POSDF.GetPosName(POSDF.GetTeachPosModeAxis(nAxisArray[nCnt]), nPosNo),
                                                dPosArray[nCnt].ToString("F3"));
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosNo(nAxisArray[nCnt], nPosNo);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// 피치 위치 또는 저장된 위치 이동
        /// </summary>
        /// <param name="nPickerPadNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        int MovePickerCalPosXYZ(int nPickerPadNo, long lDelay = 0)
        {
            int[] nPosArray = new int[3];
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * m_nPickerNo;

            nPosArray[0] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;
            nPosArray[1] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;
            nPosArray[2] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + m_nPickerNo;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[2] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
            {
                if (m_bMovePitch)
                {
                    // X는 패드 횟수 확인 필요
                    if (nAxisCnt == 0)
                    {
                        dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]] - (m_dPickerPitchX * nPickerPadNo);
                    }
                    else
                    {
                        dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];
                    }
                }
                else
                {
                    switch (nAxisCnt)
                    {
                        case 0:
                            {
                                // X
                                if (m_bPnpPos)
                                {
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchAbsRef[m_nAngleIndex][nPickerPadNo].x;
                                }
                                else
                                {
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchInspAbsRef[m_nAngleIndex][nPickerPadNo].x;
                                }
                            }
                            break;
                        case 1:
                            {
                                // Y
                                if (m_bPnpPos)
                                {
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchAbsRef[m_nAngleIndex][nPickerPadNo].y;
                                }
                                else
                                {
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchInspAbsRef[m_nAngleIndex][nPickerPadNo].y;
                                }
                            }
                            break;
                        case 2:
                            {
                                // Z
                                if (m_bPnpPos)
                                {
                                    //dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]] -
                                    //    ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dPnpOffsetZ[nPickerPadNo].dPlace[0];
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];

                                    if (dPosArray[nAxisCnt] < 0.0)
                                        dPosArray[nAxisCnt] = 0;
                                }
                                else
                                {
                                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                dSpeedArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dVel;
                dAccArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dAcc;
                dDecArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dDec;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            POSDF.GetPosName(POSDF.CFG_POS_MODE.PICKER_HEAD_OFFSET, nPosArray[nAxisCnt]),
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        int MovePickerCalPnpPosZ(int nPickerPadNo, long lDelay = 0)
        {
            int[] nPosArray = new int[1];
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * m_nPickerNo;

            nPosArray[0] = 0;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (nPickerPadNo * 2);

            // Picker Z
            dPosArray[0] = ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dPnpOffsetZ[nPickerPadNo].dPlace[0];
            dSpeedArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dVel;
            dAccArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dAcc;
            dDecArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dDec;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker Cal Z",
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// 피커 하나 인스펙션 용으로 이동
        /// </summary>
        /// <param name="nPickerPadNo"></param>
        /// <param name="lDelay"></param>
        /// <returns></returns>
        int MovePickerCalInspPosZ(int nPickerPadNo, long lDelay = 0)
        {
            int[] nPosArray = new int[1];
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * m_nPickerNo;

            nPosArray[0] = 0;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (nPickerPadNo * 2);

            // Picker Z
            dPosArray[0] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_BGA_VISION_REF];
            dSpeedArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dVel;
            dAccArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dAcc;
            dDecArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dDec;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker Cal Z",
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        int MovePickerResetPosZ(int nPickerPadNo, long lDelay = 0)
        {
            int[] nPosArray = new int[1];
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            int nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * m_nPickerNo;

            nPosArray[0] = 0;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + nHeadOffset + (nPickerPadNo * 2);

            // Picker Z
            dPosArray[0] = 0;
            dSpeedArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dVel;
            dAccArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dAcc;
            dDecArray[0] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[0]].dDec;

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker Cal Z",
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        int MovePickerCalCenterPos(long lDelay = 0)
        {
            int[] nPosArray = new int[3];
            int[] nAxisArray = new int[3];
            double[] dPosArray = new double[3];
            double[] dSpeedArray = new double[3];
            double[] dAccArray = new double[3];
            double[] dDecArray = new double[3];

            nPosArray[0] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;
            nPosArray[1] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;
            nPosArray[2] = POSDF.CFG_PICKER_HEAD_1_PITCH_OFFSET_READY + m_nPickerNo;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + m_nPickerNo;
            nAxisArray[1] = (int)SVDF.AXES.BALL_VISION_Y;
            nAxisArray[2] = (int)SVDF.AXES.BALL_VISION_Z;

            for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
            {
                // X는 패드 횟수 확인 필요
                if (nAxisCnt == 0)
                {
                    dPosArray[nAxisCnt] = m_dCenterX[DATA_PICKER_PAD_NO];
                }
                else if(nAxisCnt == 1)
                {
                    dPosArray[nAxisCnt] = m_dCenterY[DATA_PICKER_PAD_NO];
                }
                else
                {
                    dPosArray[nAxisCnt] = ConfigMgr.Inst.TempCfg.dMotPos[nAxisArray[nAxisCnt]].dPos[nPosArray[nAxisCnt]];
                }

                dSpeedArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dVel;
                dAccArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dAcc;
                dDecArray[nAxisCnt] = ConfigMgr.Inst.Cfg.MotData[nAxisArray[nAxisCnt]].dDec;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            POSDF.GetPosName(POSDF.CFG_POS_MODE.PICKER_HEAD_OFFSET, nPosArray[nAxisCnt]),
                            dPosArray[nAxisCnt]);
                }
            }

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                int nSafety = SafetyMgr.Inst.GetAxisSafetyBeforePosMove(nAxisArray[nCnt], dPosArray[nCnt]);
                if (FNC.IsErr(nSafety)) return nSafety;
            }

            return AxisMultiMovePos(nAxisArray, dPosArray, dSpeedArray, dAccArray, dDecArray, lDelay);
        }

        /// <summary>
        /// [2022.05.31.kmlee] 수정
        /// </summary>
        void UpdateDataToTempCfg()
        {
            double[] dPosArray = new double[2];
            double[] dInspPosArray = new double[2];

            for (int nPickerPadCnt = 0; nPickerPadCnt < CFG_DF.MAX_PICKER_PAD_CNT; nPickerPadCnt++)
            {
                // Insp X
                dInspPosArray[0] = m_dCenterX[nPickerPadCnt];
                // Insp Y
                dInspPosArray[1] = m_dCenterY[nPickerPadCnt];

                // Insp X
                dPosArray[0] = m_dCenterX[nPickerPadCnt + CFG_DF.MAX_PICKER_PAD_CNT];
                // Insp Y
                dPosArray[1] = m_dCenterY[nPickerPadCnt + CFG_DF.MAX_PICKER_PAD_CNT];

                // Temp에 입력
                ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchAbsRef[m_nAngleIndex][nPickerPadCnt].x = dPosArray[0];
                ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchAbsRef[m_nAngleIndex][nPickerPadCnt].y = dPosArray[1];

                ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchInspAbsRef[m_nAngleIndex][nPickerPadCnt].x = dInspPosArray[0];
                ConfigMgr.Inst.TempCfg.OffsetPnP[m_nPickerNo].dpPickerPitchInspAbsRef[m_nAngleIndex][nPickerPadCnt].y = dInspPosArray[1];
            }

        }
    }
}
