using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnPickerZeroSet_Place : SeqBase
    {
        int m_nHeadNo = 0;
        int m_nStageNo = 0;

        bool[] m_isUsePad;

        double m_dStartZ;
        double m_dStepZ;

        double m_dStartX;
        double m_dStartY;

        int m_nPadNo = 0;
        double m_dCurrentPosZ = 0;

        int m_nHeadOffset = 0;

        double[] m_dResultPosZ;

        public mnPickerZeroSet_Place()
        {
            m_dResultPosZ = new double[CFG_DF.MAX_PICKER_PAD_CNT];
        }

        public void SetParam(int nHeadNo, int nStageNo, bool[] isUsePad, double dStartZ, double dStepZ, double dStartX, double dStartY)
        {
            m_nHeadNo = nHeadNo;
            m_nStageNo = nStageNo;

            m_isUsePad = new bool[isUsePad.Length];
            Array.Copy(isUsePad, m_isUsePad, isUsePad.Length);

            m_dStartZ = dStartZ;
            m_dStepZ = dStepZ;
            m_dStartX = dStartX;
            m_dStartY = dStartY;

            m_nHeadOffset = ((int)SVDF.AXES.CHIP_PK_2_Z_1 - (int)SVDF.AXES.CHIP_PK_1_Z_1) * m_nHeadNo;
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
                        m_nPadNo = 0;

                        for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
                        {
                            BlowOff(nPadNo);
                        }
                    }
                    break;
                case 2:
                    {
                        nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_READY, m_nHeadNo);
                    }
                    break;
                case 4:
                    {
                        nFuncResult = MovePickerZeroSetPosXY();
                    }
                    break;
                case 5:
                    {
                        if (m_nPadNo >= m_isUsePad.Length)
                        {
                            NextSeq(20);
                            return;
                        }

                        if (m_isUsePad[m_nPadNo] == false)
                        {
                            m_nPadNo++;
                            return;
                        }

                        if (m_isUsePad[m_nPadNo])
                        {
                            nFuncResult = MovePickerZeroSetPosX(m_nPadNo);
                        }
                    }
                    break;
                case 6:
                    {
                        if (m_nPadNo >= m_isUsePad.Length)
                        {
                            NextSeq(20);
                            return;
                        }

                        m_dCurrentPosZ = m_dStartZ;
                    }
                    break;
                // m_nPadNo의 패드 Vacuum On
                case 8:
                    {
                        VacuumOn(m_nPadNo);
                    }
                    break;
                // m_dCurrentPosZ로 m_nPadNo의 패드 다운
                case 10:
                    {
                        nFuncResult = MovePickerZ(m_dCurrentPosZ, m_nPadNo, 500);
                    }
                    break;
                // Vacuum 감지 여부 확인
                // 감지 안되면 m_dCurrentPosZ에 m_dStepZ 더하고 10으로 이동
                // 감지되면 현재 위치 저장 후 Vacuum 끄고 진행
                case 12:
                    {
                        if (!IsVacOn(m_nHeadNo, m_nPadNo))
                        {
                            m_dCurrentPosZ += m_dStepZ;
                            NextSeq(10);
                            return;
                        }

                        m_dResultPosZ[m_nPadNo] = m_dCurrentPosZ;

                        BlowOn(m_nPadNo);
                    }
                    break;
                // Z축 올리고
                case 14:
                    {
                        nFuncResult = MovePosChipPkAllZ(POSDF.CHIP_PICKER_READY, m_nHeadNo);
                    }
                    break;
                // Blow OFF
                case 16:
                    {
                        BlowOff(m_nPadNo);
                    }
                    break;
                case 18:
                    {
                        m_nPadNo++;

                        NextSeq(5);
                        return;
                    }
                //break;
                case 20:
                    // 데이터를 갱신한다
                    UpdateTempData();
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
        int MovePickerZ(double dPos, int nPadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            int[] nPosArray = new int[1];
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            nPosArray[0] = POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (m_nHeadOffset) + (nPadNo * 2);

            dPosArray[0] = dPos;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosArray[nCnt]];
                dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosArray[nCnt]] * 10;
                dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosArray[nCnt]] * 10;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker Z",
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
        int MovePickerZeroSetPosX(int nPadNo, long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nPosArray = new int[1];
            int[] nAxisArray = new int[1];
            double[] dPosArray = new double[1];
            double[] dSpeedArray = new double[1];
            double[] dAccArray = new double[1];
            double[] dDecArray = new double[1];

            //2022 09 25 HEP 기존 0으로하면 문제 발생
            //속도 참고용  POSDF 설정
            nPosArray[0] = POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + m_nHeadNo;
            //m_dStartX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + m_nStageNo];
            dPosArray[0] = m_dStartX - (nPadNo * ConfigMgr.Inst.Cfg.OffsetPnP[m_nHeadNo].dDefPickerPitchX);

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                //dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosArray[nCnt]];
                //dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosArray[nCnt]];
                //dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosArray[nCnt]];

                //20221230 CHOH : 속도 너무 빨라서 고정속도 사용하도록 함
                dSpeedArray[nCnt] = 100;
                dAccArray[nCnt] = 500;
                dDecArray[nCnt] = 500;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker ZeroSet X",
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
        int MovePickerZeroSetPosXY(long lDelay = 0)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nPosArray = new int[2];
            int[] nAxisArray = new int[2];
            double[] dPosArray = new double[2];
            double[] dSpeedArray = new double[2];
            double[] dAccArray = new double[2];
            double[] dDecArray = new double[2];

            //2022 09 25 HEP 기존 0으로하면 문제 발생
            //속도 참고용  POSDF 설정
            nPosArray[0] = POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1;
            nPosArray[1] = POSDF.CHIP_PICKER_CHIP_UNLOADING_GOOD_REF_T1;

            nAxisArray[0] = (int)SVDF.AXES.CHIP_PK_1_X + m_nHeadNo;
            nAxisArray[1] = (int)SVDF.AXES.GD_TRAY_STG_1_Y + m_nStageNo;

            //m_dStartX = TeachMgr.Inst.Tch.dMotPos[nAxisArray[0]].dPos[POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1 + m_nStageNo];

            dPosArray[0] = m_dStartX;
            dPosArray[1] = m_dStartY;

            for (int nCnt = 0; nCnt < nAxisArray.Length; nCnt++)
            {
                //dSpeedArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dVel[nPosArray[nCnt]];
                //dAccArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dAcc[nPosArray[nCnt]];
                //dDecArray[nCnt] = TeachMgr.Inst.Tch.dMotPos[nAxisArray[nCnt]].dDec[nPosArray[nCnt]];

                //20221230 CHOH : 속도 너무 빨라서 고정속도 사용하도록 함
                dSpeedArray[nCnt] = 100;
                dAccArray[nCnt] = 500;
                dDecArray[nCnt] = 500;
            }

            if (RunLib.IsCurRunCmdStart())
            {
                m_strMotPos = "MOVE POSITION";

                for (int nAxisCnt = 0; nAxisCnt < nAxisArray.Length; nAxisCnt++)
                {
                    m_strMotPos = string.Format("{0}[{1} - {2} : {3}]", m_strMotPos,
                            SVDF.GetAxisName(nAxisArray[nAxisCnt]),
                            "Picker ZeroSet XY",
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

        void VacuumOn(int nPadNo)
        {
            // BLOW BIT OFF
            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * nPadNo), 3, false);
            // VAC BIT ON
            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * nPadNo), 2, true);
        }

        void BlowOn(int nPadNo)
        {
            // VAC BIT OFF
            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * nPadNo), 2, false);
            // BLOW BIT ON
            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * nPadNo), 3, true);
        }

        void BlowOff(int nPadNo)
        {
            // BLOW BIT ON
            MotionMgr.Inst.SetSigOutput((int)SVDF.AXES.CHIP_PK_1_Z_1 + m_nHeadOffset + (2 * nPadNo), 3, false);
        }

        void UpdateTempData()
        {
            for (int nPadNo = 0; nPadNo < CFG_DF.MAX_PICKER_PAD_CNT; nPadNo++)
            {
                if (m_isUsePad[nPadNo])
                {
                    //칩 두께만큼 더해준다.
                    ConfigMgr.Inst.TempCfg.OffsetPnP[m_nHeadNo].dPnpOffsetZ[nPadNo].dPlace[m_nStageNo] = Math.Round(m_dResultPosZ[nPadNo], 2); // + RecipeMgr.Inst.TempRcp.MapTbInfo.dChipThickness;
                }
            }
        }
    }
}
