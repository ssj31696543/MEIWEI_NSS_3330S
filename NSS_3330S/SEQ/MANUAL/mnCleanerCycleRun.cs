using NSSU_3400.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSU_3400.SEQ.MANUAL
{
    public class mnCleanerCycleRun : SeqBase
    {
        int m_nStartSeqNo = 0;
        SEQ_ID m_seqID = SEQ_ID.CLEANING_FIRST;

        bool m_bVacSkip = true;
        object[] m_args = null;

        public cycCleaning m_cycCleaning = null;
        
        bool IS_VACUUM_CHECK
        {
            get
            {
                if (m_bVacSkip) return false;

                return true;
            }
        }

        public SEQ_ID CURRENT_SEQ_ID
        {
            get { return m_seqID; }
        }

        public mnCleanerCycleRun()
        {
            m_cycCleaning = new cycCleaning((int)SEQ_ID.CLEANING_FIRST);

            m_cycCleaning.SetAutoManualMode(false);

            m_cycCleaning.SetAddMsgFunc(SetProcMsgEvent);
        }

        public void SetParam(int nStartSeqNo, params object[] args)
        {
            m_nStartSeqNo = nStartSeqNo;
            m_bVacSkip = !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse;
            m_args = args;

            #region Set Parameter
            switch (m_nStartSeqNo)
            {
                case 10:
                case 11:
                case 20:
                case 21:
                case 30:
                case 31:
                    m_cycCleaning.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.CLEANING_FIRST;
                    break;
                case 40:
                case 41:
                case 50:
                case 51:
                case 60:
                case 61:
                case 70:
                case 71:
                    m_cycCleaning.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.CLEANING_SECOND;
                    break;
                case 80:
                case 81:
                case 90:
                case 91:
                case 100:
                case 101:
                case 110:
                case 111:
                case 120:
                case 121:
                case 130:
                case 131:
                case 140:
                case 141:
                case 150:
                case 151:
                case 160:
                case 161:
                case 170:
                case 171:
                    m_cycCleaning.SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.CLEANING_CLEAN_PICKER;
                    break;
                default:
                    break;
            }

            #endregion
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            m_cycCleaning.InitSeq();
            //m_cycStripTransfer.InitSeq();
            //m_cycUnitTransfer.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            m_cycCleaning.InitSeq();
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
                        NextSeq(m_nStartSeqNo);
                        return;
                    }
                // 1차 세정 스폰지
                case 5:
                    m_cycCleaning.InitSeq();
                    NextSeq(6);
                    return;
                case 6:
                    nFuncResult = m_cycCleaning.SpongeClean();
                    break;
                // 1차 세정
                case 10:
                    m_cycCleaning.InitSeq();
                    NextSeq(11);
                    return;
                case 11:
                    nFuncResult = m_cycCleaning.FirstClean();
                    break;
                
                // 1차 초음파
                case 20:
                    m_cycCleaning.InitSeq();
                    NextSeq(21);
                    return;
                case 21:
                    nFuncResult = m_cycCleaning.FirstUltrasonic();
                    break;
                // UNLOAD 
                case 30:
                    m_cycCleaning.InitSeq();
                    NextSeq(31);
                    return;
                case 31:
                    nFuncResult = m_cycCleaning.FirstUnload();
                    break;

                // 2차 세정 
                case 40:
                    m_cycCleaning.InitSeq();
                    NextSeq(41);
                    return;
                case 41:
                    nFuncResult = m_cycCleaning.SecondClean();
                    break;
                // 2차 초음파 안착
                case 50:
                    m_cycCleaning.InitSeq();
                    NextSeq(51);
                    return;
                case 51:
                    nFuncResult = m_cycCleaning.SecondCleanPickUp();
                    break;
                // 2차 초음파
                case 60:
                    m_cycCleaning.InitSeq();
                    NextSeq(61);
                    return;
                case 61:
                    nFuncResult = m_cycCleaning.SecondUltraPlace();
                    break;
                // 2차 초음파
                case 70:
                    m_cycCleaning.InitSeq();
                    NextSeq(71);
                    return;
                case 71:
                    nFuncResult = m_cycCleaning.SecondUltrasonic();
                    break;
                // 피커 1차 드라이 이동
                case 80:
                    m_cycCleaning.InitSeq();
                    NextSeq(81);
                    return;
                case 81:
                    nFuncResult = m_cycCleaning.DryPickerFlipAndPlasmaMove();
                    break;
                // 피커 1차 드라이
                case 90:
                    m_cycCleaning.InitSeq();
                    NextSeq(91);
                    return;
                case 91:
                    nFuncResult = m_cycCleaning.SecondUltraDry();
                    break;
                // 클리너 피커 1차플라즈마 안전 위치 이동
                case 100:
                    m_cycCleaning.InitSeq();
                    NextSeq(101);
                    return;
                case 101:
                    nFuncResult = m_cycCleaning.CleanerPickerPlasmaSafetyMove();
                    break;
                // 1차 플라즈마 시작 위치 이동
                case 110:
                    m_cycCleaning.InitSeq();
                    NextSeq(111);
                    return;
                case 111:
                    nFuncResult = m_cycCleaning.FirstPlasmaStartPosMove();
                    break;
                //1차 플라즈마 시작
                case 120:
                    m_cycCleaning.InitSeq();
                    NextSeq(121);
                    return;
                case 121:
                    //nFuncResult = m_cycCleaning.FirstPlasma();
                    nFuncResult = m_cycCleaning.FirstPlasma_ContinousMode();
                    break;

                ////2차 플라즈마 안전 대기 이동
                //case 130:
                //    m_cycCleaning.InitSeq();
                //    NextSeq(131);
                //    return;
                //case 131:
                //    nFuncResult = m_cycCleaning.SecondPlasmaSafetyMove();
                //    break;

                //2차 초음파 구역 자재 픽업
                case 140:
                    m_cycCleaning.InitSeq();
                    NextSeq(141);
                    return;
                case 141:
                    nFuncResult = m_cycCleaning.SecondUltraPickUp();
                    break;
                //2차 초음파 구역 자재 픽업
                case 150:
                    m_cycCleaning.InitSeq();
                    NextSeq(151);
                    return;
                case 151:
                    nFuncResult = m_cycCleaning.SecondUltraPickUp();
                    break;
                //2차 플라즈마 진행
                case 160:
                    m_cycCleaning.InitSeq();
                    NextSeq(161);
                    return;
                case 161:
                    //nFuncResult = m_cycCleaning.SecondPlasma();
                    nFuncResult = m_cycCleaning.SecondPlasma_Continuous();
                    break;
                //2차 언로드 드라이테이블 
                case 170:
                    m_cycCleaning.InitSeq();
                    NextSeq(171);
                    return;
                case 171:
                    nFuncResult = m_cycCleaning.LoadDryTable();
                    break;
                case 180:
                    m_cycCleaning.InitSeq();
                    NextSeq(181);
                    return;
                case 181:
                    nFuncResult = m_cycCleaning.UnitPickerScrap1();
                    break;
                case 190:
                    m_cycCleaning.InitSeq();
                    NextSeq(191);
                    return;
                case 191:
                    nFuncResult = m_cycCleaning.UnitPickerScrap2();
                    break;
                case 200:
                    m_cycCleaning.InitSeq();
                    NextSeq(201);
                    return;
                case 201:
                    nFuncResult = m_cycCleaning.CleanerPickerPlasma();
                    break;
                case 210:
                    m_cycCleaning.InitSeq();
                    NextSeq(211);
                    return;
                case 211:
                    nFuncResult = m_cycCleaning.SecondCleanDry();
                    break;
                case 300:
                    m_cycCleaning.InitSeq();
                    NextSeq(301);
                    return;
                case 301:
                    nFuncResult = m_cycCleaning.FirstInitAndWait();
                    break;
                case 310:
                    m_cycCleaning.InitSeq();
                    NextSeq(311);
                    return;
                case 311:
                    nFuncResult = m_cycCleaning.SecondInitAndWait();
                    break;
                case 320:
                    m_cycCleaning.InitSeq();
                    NextSeq(321);
                    return;
                case 321:
                    nFuncResult = m_cycCleaning.SecondUltraPickUpDry();
                    break;
                case 330:
                    m_cycCleaning.InitSeq();
                    NextSeq(331);
                    return;
                case 331:
                    nFuncResult = m_cycCleaning.SecondCleanPickUpDry();
                    break;
                default:
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            // Cycle이 끝나면 종료
            FINISH = true;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
            m_nSeqNo++;
        }
    }
}
