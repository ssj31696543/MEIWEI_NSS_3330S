using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycUnitDry : SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bIsStrip = true;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nAirDryCnt = 0;

        #region PROPERTY
        public seqUnitDry SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sUnitDry; }
        }

        public bool IS_STRIP
        {
            get
            {
                if (m_bAutoMode)
                {
                    return SEQ_INFO_CURRENT.Info.IsStrip();
                }

                return m_bIsStrip;
            }
        }
        #endregion

        public void CmdInspectionDBRead()
        {
        }

        public cycUnitDry()
        {
            SetCycleMode(true);

            m_nSeqID = (int)SEQ_ID.DRY_UNIT;
            m_seqInfo = GbVar.Seq.sMapTransfer;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];
        }

        public void SetAutoManualMode(bool bAuto)
        {
            m_bAutoMode = bAuto;
        }

        public void SetManualModeParam(params object[] args)
        {
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }


        public int BtmAirDry()
        {
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
                        if (m_bFirstSeqStep)
                        {
                            m_nAirDryCnt = 0;
                            m_bFirstSeqStep = false;
                        }
                    }
                    break;

                case 2:
                    {
                    }
                    break;
                // 자재 정보가 없을 경우
                case 3:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.Seq.sUnitTransfer.Info.IsStrip() == false)
                            {
                                return FNC.SUCCESS;
                            }
                        }

                    }
                    break;

                case 5:
                    if (RecipeMgr.Inst.Rcp.cleaning.nBtmDryCnt == 0)
                        return FNC.SUCCESS;

                    //Air On
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, true);

                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, true);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, true);
                    
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE AIR KNIFE, PIFE ON", STEP_ELAPSED));
                    break;

                case 8://시작 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nBtmDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_BTM_DRY_START, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS AIR DRY START POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if( FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);

                            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, true); //항시온
                        }
                    }
                    break;

                //case 9:
                //    if(!IsDelayOver(RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay))
                //    {
                //        return FNC.BUSY;
                //    }
                //    break;

                case 10://끝 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nBtmDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_BTM_DRY_END, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lBtmDryEndDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP PICKER X AXIS AIR DRY END POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);
                            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, GbVar.bDryBlockAlways); // 항시온
                        }
                    }
                    break;

                case 12://반복 횟수 확인
                    {
                        if (m_nAirDryCnt + 1 >= RecipeMgr.Inst.Rcp.cleaning.nBtmDryCnt)
                        {
                            break;
                        }

                        m_nAirDryCnt++;

                        NextSeq(8);
                        return FNC.BUSY;
                    }
                //
                //2022 08 12 HEP
                //다시 시작 위치로 보내려고 그래야 왕복 1회가 된다.
                case 14://시작 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nBtmDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_BTM_DRY_START, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lBtmDryStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS AIR DRY START POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                            MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);
                            MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false); // 항시온
                        }
                    }
                    break;

                case 16:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                    MotionMgr.Inst.SetOutput(IODF.OUTPUT.DRY_BLOCK_BLOW, false);

                    MOTION.MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false); //항시온
                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE AIR KNIFE, PIFE OFF", STEP_ELAPSED));

                    return FNC.SUCCESS;

                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int TopAirDry()
        {
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
                        if (m_bFirstSeqStep)
                        {
                            m_nAirDryCnt = 0;
                            m_bFirstSeqStep = false;
                        }
                    }
                    break;
                case 2:
                    {
                    }
                    break;

                // 자재 정보가 없을 경우
                case 3:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.Seq.sUnitDry.Info.IsStrip() == false)
                            {
                                return FNC.SUCCESS;
                            }
                        }

                    }
                    break;

                case 5:
                    if (RecipeMgr.Inst.Rcp.cleaning.nTopDryCnt == 0)
                        return FNC.SUCCESS;

                    //Air On
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, true);

                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, true);



                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEAN KNIFE, PIPE ON", STEP_ELAPSED));
                    break;

                case 8://시작 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nTopDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_TOP_DRY_START, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS TOP DRY START POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);

                        }
                    }
                    break;

                //case 9:
                //    if (!IsDelayOver(RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay))
                //    {
                //        return FNC.BUSY;
                //    }
                //    break;

                case 10://끝 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nTopDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_TOP_DRY_END, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lTopDryEndDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS TOP DRY END POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);
                        }
                    }
                    break;

                case 12://반복 횟수 확인
                    {
                        if (m_nAirDryCnt + 1 >= RecipeMgr.Inst.Rcp.cleaning.nTopDryCnt)
                        {
                            break;
                        }

                        m_nAirDryCnt++;

                        NextSeq(8);
                        return FNC.BUSY;
                    }
                //
                //2022 08 12 HEP
                //다시 시작 위치로 보내려고 그래야 왕복 1회가 된다.
                case 14://시작 위치 
                    {
                        double dSpeed = RecipeMgr.Inst.Rcp.cleaning.nTopDryVel;
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_TOP_DRY_START, true, dSpeed, RecipeMgr.Inst.Rcp.cleaning.lTopDryStartDelay);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS TOP DRY START POS MOVE COMPLETE", STEP_ELAPSED));
                        }
                        else if (FNC.IsErr(nFuncResult))
                        {
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);

                        }
                    }
                    break;
                case 16:
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_1, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_KNIFE_2, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE, false);

                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_KNIFE, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT, false);

                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "CLEAN KNIFE, PIPE OFF", STEP_ELAPSED));
                    return FNC.SUCCESS;

                default:
                    break;
            }

            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }

        public int LoadingToDryStage()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;
            switch (m_nSeqNo)
            {
                case 0:
                    break;

                case 1:
                    if (m_bAutoMode)
                    {
                        
                    }
                    break;

                case 2:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkZ(POSDF.UNIT_PICKER_READY)) break;
                        }
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 4:
                    {
                      
                    }
                    break;
                case 5:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosUnitPkX(POSDF.UNIT_PICKER_STRIP_UNLOADING)) break;
                        }
                        nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_STRIP_UNLOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER X AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 6:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosNo(SVDF.AXES.DRY_BLOCK_STG_X, POSDF.DRY_BLOCK_STAGE_STRIP_LOADING)) break;
                            double dTchPos = TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.DRY_BLOCK_STG_X].dPos[POSDF.DRY_BLOCK_STAGE_STRIP_LOADING];
                            double dPosDry = MotionMgr.Inst[SVDF.AXES.DRY_BLOCK_STG_X].GetTargetMoveAbsPos();
                            if (SafetyMgr.Inst.IsWithInRangePos(dTchPos, dPosDry, 0.01))
                            {
                                break;
                            }
                        }
                        nFuncResult = MovePosDryBlockStgX(POSDF.DRY_BLOCK_STAGE_STRIP_LOADING);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY STAGE X AXIS LOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 8:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, -dOffset);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 10:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, 0, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 11:
                    {
                        nFuncResult = UnitPickerWorkVac(true, false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER VAC ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 13:
                    {
                        nFuncResult = DryBlockWorkBlow(true, 0, false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "DRY BLOCK BLOW ON COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse == false) break;

                        nFuncResult = VacBlowCheck((int)IODF.A_INPUT.UNIT_PK_VAC, true, (int)ERDF.E_UNIT_PK_VAC_NOT_ON, 50);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "CHECK! UNIT PICKER VAC ON", STEP_ELAPSED));
                        }
                    }
                    break;


                case 17:
                    {
                        nFuncResult = DryBlockWorkBlow(false);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER BLOW OFF COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 18:
                    // UNIT TRANSFER Z AXIS UP POS  MOVE
                    {
                        double dOffset = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.UNIT_PICKER_Z_SLOW_DOWN_OFFSET].dValue;

                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_STRIP_UNLOADING, -dOffset, true);

                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS UNLOADING POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;
                case 19:
                    {
                        nFuncResult = MovePosUnitPkZ(POSDF.UNIT_PICKER_READY);
                        if (FNC.IsSuccess(nFuncResult))
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "UNIT PICKER Z AXIS READY POSITION COMPLETE", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    return FNC.SUCCESS;

                    break;
                default:
                    break;
            }
            #region AFTER SWITCH

            if (m_bFirstSeqStep)
            {
                // Position Log
                if (string.IsNullOrEmpty(m_strMotPos) == false)
                {
                    SeqHistory(m_strMotPos);
                }

                m_bFirstSeqStep = false;
            }

            if (FNC.IsErr(nFuncResult))
            {
                return nFuncResult;
            }
            else if (FNC.IsBusy(nFuncResult)) return FNC.BUSY;

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return (int)ERDF.E_WRONG_SEQUENCE_NUMBER;
            }

            return FNC.BUSY;
            #endregion
        }
    }
}
