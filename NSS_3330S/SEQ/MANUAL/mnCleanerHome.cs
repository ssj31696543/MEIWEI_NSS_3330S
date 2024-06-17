using NSSU_3400.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSSU_3400.SEQ.MANUAL
{
    class mnCleanerHome : SeqBase
    {
        bool bCmdHomeFlag = false;
        int nHomeErrAxisOffset = 0;
        double dMovePos = 0.0;
        int nAxisNo = -1;

        #region SEQUENCE ENUM
        enum SEQ_NO_INIT
        {
            HOME_INIT,
            CYLINDER_INIT,
            CYLINDER_INIT_CHECK,
            
            Z_AXIS_HOME_START,
            DRY_Y_HOME_START,
            X_AXIS_HOME_START,
            UNIT_PICKER_POSITION_CHECK,
            HOME_UNIT_PICKER,
            MOVE_UNIT_PICKER,
            CHECK_UNIT_PICKER,
            PLUS_SIGNAL_SEARCH_ELV,
            CLEANER_FLIP_T_HOME_START,
            CLEANER_FLIP_ELV_HOME_START,
            CLEANER_FLIP_ELV_LOAD_POS_MOVE,
            CLEANER_FLIP_1_LOAD_POS_MOVE,
            CLEANER_FLIP_2_LOAD_POS_MOVE,
            CLEANER_PICKER_READY_POS_MOVE,
            CLEANER_PICKER_R_READY_POS_MOVE,

            HOME_CHECK_ALREADY_ALL,
            HOME_CHECK_ALL,
            HOME_ALL_COMPLETE,
        }
        #endregion

        Stopwatch timeStampInit = new Stopwatch();

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);
        }

        protected override void SetError(int nErrNo)
        {
            OnlyStopEvent(nErrNo);
        }

        void NextSeq(SEQ_NO_INIT seq)
        {
            base.NextSeq((int)seq);
        }

        public int Init(bool bSawClenaer, bool bCleanerSorter)
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;
            switch ((SEQ_NO_INIT)m_nSeqNo)
            {
                case SEQ_NO_INIT.HOME_INIT:
                    GbVar.g_timeStampInit[MCDF.CLEANER].Restart();
                    GbVar.mcState.isInitialized[MCDF.CLEANER] = false;
                    GbVar.mcState.isInitializing[MCDF.CLEANER] = true;
                    bCmdHomeFlag = false;
                    break;
                case SEQ_NO_INIT.CYLINDER_INIT:
                    nFuncResult = CmdCylinderInit();
                    break;
                case SEQ_NO_INIT.CYLINDER_INIT_CHECK:
                    nFuncResult = CmdCylinderInitCheck();
                    break;
                case SEQ_NO_INIT.Z_AXIS_HOME_START:
                    nFuncResult = CleanerZAxisHome();
                    break;
                case SEQ_NO_INIT.DRY_Y_HOME_START:
                    nFuncResult = DryYAxisHome();
                    break;
                case SEQ_NO_INIT.X_AXIS_HOME_START:
                    nFuncResult = CleanerAxisHome();
                    break;
                case SEQ_NO_INIT.UNIT_PICKER_POSITION_CHECK:
                    if(GbVar.IO[IODF.INPUT.UPX_SAFE_POSITION_CHECK] == 1)
                    {
                        NextSeq((int)SEQ_NO_INIT.PLUS_SIGNAL_SEARCH_ELV);
                        return FNC.BUSY;
                    }
                    if(GbVar.mcState.isHomeComplete[(int)SVDF.AXES.UNIT_PK_X])
                    {
                        NextSeq((int)SEQ_NO_INIT.MOVE_UNIT_PICKER);
                        return FNC.BUSY;
                    }
                    break;
                case SEQ_NO_INIT.HOME_UNIT_PICKER:
                    nFuncResult = UnitPickerXAxisHome();
                    break;
                case SEQ_NO_INIT.MOVE_UNIT_PICKER:
                    //nAxisNo = (int)SVDF.AXES.UNIT_PK_X;
                    //dMovePos = 400;
                    //double dVel = 50;
                    //MotionMgr.Inst[nAxisNo].MoveAbs(dMovePos, dVel, dVel * 5, dVel * 5);
                    //클린 1위치가 안전함 READY 위치는 0이어서 클리너피커와 충돌 가능성 있음
                    nFuncResult = MovePosUnitPkX(POSDF.UNIT_PICKER_CLEAN_1);
                    break;
                case SEQ_NO_INIT.CHECK_UNIT_PICKER:
                    //if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MOT_MOVE_TIME].nValue))
                    //    return (int)ERDF.E_SV_MOVE + (int)SVDF.AXES.UNIT_PK_X;
                    //if (!MotionMgr.Inst[nAxisNo].IsInPosition(dMovePos)) return FNC.BUSY;
                    break;
                case SEQ_NO_INIT.PLUS_SIGNAL_SEARCH_ELV:
                    nFuncResult = AxisPlusSinalSearch((int)SVDF.AXES.CL_PICKER_FLIP_ELEVATOR);
                    break;
                case SEQ_NO_INIT.CLEANER_FLIP_T_HOME_START:
                    nFuncResult = CleanerFlipRAxisHome();
                    break;
                case SEQ_NO_INIT.CLEANER_FLIP_ELV_HOME_START:
                    nFuncResult = CleanerFlipElvAxisHome();
                    break;
                case SEQ_NO_INIT.CLEANER_FLIP_ELV_LOAD_POS_MOVE:
                    nFuncResult = MovePosFlipElv(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;
                case SEQ_NO_INIT.CLEANER_FLIP_1_LOAD_POS_MOVE:
                    nFuncResult = MovePosFlip1(POSDF.CLEANER_SECOND_CLEAN_LOAD);
                    break;
                case SEQ_NO_INIT.CLEANER_FLIP_2_LOAD_POS_MOVE:
                    nFuncResult = MovePosFlip2(POSDF.SECOND_ULTRASONIC_SECOND_ULTRASONIC_LOAD);
                    break;
                case SEQ_NO_INIT.CLEANER_PICKER_READY_POS_MOVE:
                    nFuncResult = MovePosCleanerX(POSDF.CLEANER_PICKER_READY);
                    break;
                case SEQ_NO_INIT.CLEANER_PICKER_R_READY_POS_MOVE:
                    nFuncResult = MovePosCleanerR(POSDF.CLEANER_PICKER_SECOND_ULTRASONIC, 100);
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALREADY_ALL:
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALL:
                    break;
                case SEQ_NO_INIT.HOME_ALL_COMPLETE:
                    GbVar.mcState.isInitialized[MCDF.CLEANER] = true;
                    GbVar.mcState.isInitializing[MCDF.CLEANER] = false;

                    GbVar.g_timeStampInit[MCDF.CLEANER].Stop();
                    return FNC.SUCCESS;
                default:
                    break;
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
        }
        public int CleanerAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_X)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_X)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_X)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_X)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_X)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_DRY_X;
                        return FNC.BUSY;
                    }
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_X;
                        return FNC.BUSY;
                    }
                }
            }

            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_X)] = true;

            return FNC.SUCCESS;
        }
        public int UnitPickerXAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_X;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = true;

            return FNC.SUCCESS;
        }
        public int DryYAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Y)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Y)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Y)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Y)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_DRY_Y;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Y)] = true;

            return FNC.SUCCESS;
        }

        public int CleanerZAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_Z)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_Z)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_Z)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_Z)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_DRY_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_DRY_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_Z;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_DRY_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_Z)] = true;

            return FNC.SUCCESS;
        }
        public int CleanerFlipRAxisHome()
        {

            //임시로....
            if (MotionMgr.Inst.MOTION[(int)SVDF.AXES.DRY_BLOCK_STG_X].GetEncPos() > 
                TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.DRY_BLOCK_STG_X].dPos[POSDF.DRY_BLOCK_STAGE_READY] + 5/*GAP*/)
            {
                return (int)ERDF.E_INTL_CLEANER_R_CAN_NOT_MOVE_DRY_STAGE_X_NOT_READY_POS;
            }



            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_R)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_R)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_R)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_R)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_FLIP_1;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_FLIP_2;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_R)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_R)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_R;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_1)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_2)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_R)] = true;

            return FNC.SUCCESS;
        }
        public int CleanerFlipElvAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CL_PICKER_FLIP_ELEVATOR;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CL_PICKER_FLIP_ELEVATOR)] = true;

            return FNC.SUCCESS;
        }
        public int InterlockCheck()
        {
            //클램프가 되어있으면 알람
            if (GbVar.GB_INPUT[(int)IODF.INPUT.MLZ_AXIS_MZ_CLAMP] == 1 ||
               GbVar.IO[IODF.OUTPUT.MGZ_LD_Z_MGZ_CLAMP] == 1)
            {
                return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_IS_NOT_UNCLAMP;
            }

            return FNC.SUCCESS;
        }
        public int CmdCylinderInit()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            GbVar.bInputWaitMsg[0] = false;

            int[] nOnOutputArr = new int[] { (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_OPEN_SOL, (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SOL };

            int[] nOffOutputArr = new int[] { (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SOL, (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SOL, (int)IODF.OUTPUT.ULTRASONIC_1_RUN, (int)IODF.OUTPUT.ULTRASONIC_2_RUN, (int)IODF.OUTPUT.ULTRASONIC_3_RUN, (int)IODF.OUTPUT.ULTRASONIC_4_RUN, (int)IODF.OUTPUT.PICKER_AIR_KNIFE_UP_DOWN_SOL };


#if _MC2
            nOnOutputArr = new int[] { (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_OPEN_SOL, (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SOL };

            nOffOutputArr = new int[] { (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SOL, (int)IODF.OUTPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SOL };
#endif


            for (int nOffCnt = 0; nOffCnt < nOffOutputArr.Length; nOffCnt++)
            {
                MotionMgr.Inst.SetOutput(nOffOutputArr[nOffCnt], false);
            }

            for (int nOnCnt = 0; nOnCnt < nOnOutputArr.Length; nOnCnt++)
            {
                MotionMgr.Inst.SetOutput(nOnOutputArr[nOnCnt], true);
            }

            return FNC.SUCCESS;
        }

        public int CmdCylinderInitCheck()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nOnInputArr = new int[] { (int)IODF.INPUT.SECOND_CLEANING_ZONE_COVER_OPEN_SENSOR_1, (int)IODF.INPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SENSOR };

            int[] nOffInputArr = new int[] {(int)IODF.INPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SENSOR_1, (int)IODF.INPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SENSOR };

#if _MC2
            nOnInputArr = new int[] { (int)IODF.INPUT.SECOND_CLEANING_ZONE_COVER_OPEN_SENSOR_1, (int)IODF.INPUT.SECOND_CLEANING_ZONE_WJ_FORWARD_SENSOR };

            nOffInputArr = new int[] {
                                             (int)IODF.INPUT.SECOND_CLEANING_ZONE_COVER_CLOSE_SENSOR_1, (int)IODF.INPUT.SECOND_CLEANING_ZONE_WJ_BACKWARD_SENSOR
                                             };
#endif
            ERDF[] errOnInputArr = new ERDF[] { ERDF.E_SECOND_CLEANING_ZONE_COVER_OPEN_FAIL, ERDF.E_SECOND_CLEANING_ZONE_WJ_BACKWARD_FAIL };
            ERDF[] errOffInputArr = new ERDF[] { ERDF.E_SECOND_CLEANING_ZONE_COVER_CLOSE_FAIL, ERDF.E_SECOND_CLEANING_ZONE_WJ_FORWARD_FAIL };

            //for (int nOnInput = 0; nOnInput < nOnInputArr.Length; nOnInput++) //org
            for (int nOnInput = 1; nOnInput < nOnInputArr.Length; nOnInput++)//220513 기구물 해체로 인해 임시로 센서 안보게 수정
            {
                if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue))
                    return (int)errOnInputArr[nOnInput];

                if (GbVar.GB_INPUT[nOnInputArr[nOnInput]] != 1)
                    return FNC.BUSY;
            }

            //for (int nOffInput = 0; nOffInput < nOffInputArr.Length; nOffInput++)// org
            for (int nOffInput = 1; nOffInput < nOffInputArr.Length; nOffInput++)//220513 기구물 해체로 인해 임시로 센서 안보게 수정
            {
                if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue))
                    return (int)errOffInputArr[nOffInput];

                if (GbVar.GB_INPUT[nOffInputArr[nOffInput]] != 0)
                    return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }

    }
}
