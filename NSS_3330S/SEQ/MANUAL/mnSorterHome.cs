using DionesTool.Motion;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    class mnSorterHome : SeqBase
    {
        bool bCmdHomeFlag = false;
        int nHomeErrAxisOffset = 0;
        double dMovePos = 0.0;
        int nAxisNo = -1;


        Stopwatch swInputCheck = new Stopwatch();

        enum SEQ_NO_INIT
        {
            HOME_INIT,

            SV_OFF,
            SV_RESET,
            WAIT_RESET_DELAY,
            SV_ON,
            WAIT_ON_DELAY,
            CYLINDER_INIT, // ELEV CLAMP...
            CYLINDER_INIT_CHECK,

            ALL_Z_AXIS_HOME, // MAP_PICKER, MAP_VISION, BALL VISION, CELL PICKER, ELEV Z, TRAY PICKER
            ALL_Z_AXIS_HOME_1,
            DRY_STAGE_INTERLOCK,
			
            ALL_XY_AXIS_HOME, // DRY STAGE X, MAP_PICKER, MAP STAGE, TRAY STAGE, PICKER HEAD, BALL VISION Y, TRAY PICKER
			
            MAP_VIS_TABLE_T_HOME,         

            READY_POS_MOVE,
            HOME_ALL_COMPLETE,
        }

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

        public int Init()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                //if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            switch ((SEQ_NO_INIT)m_nSeqNo)
            {
                case SEQ_NO_INIT.HOME_INIT:
#if _NOTEBOOK
                    NextSeq(SEQ_NO_INIT.HOME_ALL_COMPLETE);
                    return FNC.BUSY;
#endif
                    GbVar.g_timeStampInit[MCDF.SORTER].Restart();
                    swInputCheck.Reset();
                    GbVar.mcState.isInitialized[MCDF.SORTER] = false;
                    GbVar.mcState.isInitializing[MCDF.SORTER] = true;
                    bCmdHomeFlag = false;
                    break;

                case SEQ_NO_INIT.SV_OFF:
                    SevoOffPickerZ();
                    NextSeq(SEQ_NO_INIT.SV_RESET);
                    return FNC.BUSY;

                case SEQ_NO_INIT.SV_RESET:
                    ResetServo();
                    NextSeq(SEQ_NO_INIT.WAIT_RESET_DELAY);
                    return FNC.BUSY;

                case SEQ_NO_INIT.WAIT_RESET_DELAY:
                    if (WaitDelay(3000)) return FNC.BUSY;
                    break;

                case SEQ_NO_INIT.SV_ON:
                    ServoOn();
                    NextSeq(SEQ_NO_INIT.WAIT_ON_DELAY);
                    return FNC.BUSY;

                case SEQ_NO_INIT.WAIT_ON_DELAY:
                    if (WaitDelay(3000)) return FNC.BUSY;
                    break;

                case SEQ_NO_INIT.CYLINDER_INIT:
                    nFuncResult = CmdCylinderInit();
                    break;

                case SEQ_NO_INIT.CYLINDER_INIT_CHECK:
                    nFuncResult = CmdCylinderInitCheck();
                    break;

                case SEQ_NO_INIT.ALL_Z_AXIS_HOME:
                    nFuncResult = ChipPickerMoveDownBeforeHome();
                    break;

                case SEQ_NO_INIT.ALL_Z_AXIS_HOME_1:
                    nFuncResult = SorterAllZAxisHome();
                    break;

                case SEQ_NO_INIT.DRY_STAGE_INTERLOCK:
                    nFuncResult = InterlockCheck();
                    break;

                case SEQ_NO_INIT.ALL_XY_AXIS_HOME:
                    nFuncResult = SorterAllXYAxisHome();
                    break;

                case SEQ_NO_INIT.MAP_VIS_TABLE_T_HOME:
                    nFuncResult = SorterMapTableTAxisHome();
                    break;

                case SEQ_NO_INIT.READY_POS_MOVE:
                    nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_READY);
                    break;

                case SEQ_NO_INIT.HOME_ALL_COMPLETE:
                    GbVar.mcState.isInitialized[MCDF.SORTER] = true;
                    GbVar.mcState.isInitializing[MCDF.SORTER] = false;

                    GbVar.g_timeStampInit[MCDF.SORTER].Stop();
                    return FNC.SUCCESS;

                default:
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                swInputCheck.Stop();
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

        public void ResetServo()
        {
            for (int nSvCnt = 0; nSvCnt < SVDF.SV_CNT; nSvCnt++)
            {
                if (MotionMgr.Inst[nSvCnt].IsAlarm())
                {
                    MotionMgr.Inst[nSvCnt].ResetAlarm();
                }
            }
        }

        public void ServoOn()
        {
            for (int nSvCnt = 0; nSvCnt < SVDF.SV_CNT; nSvCnt++)
            {
                MotionMgr.Inst[nSvCnt].SetServoOnOff(true);
            }
        }

        public void SevoOffPickerZ()
        {
            for (int nSvCnt = (int)SVDF.AXES.CHIP_PK_1_Z_1; nSvCnt < (int)SVDF.AXES.CHIP_PK_2_T_8; nSvCnt += 2)
            {
                MotionMgr.Inst[nSvCnt].SetServoOnOff(false);
            }
        }

        public int CmdCylinderInit()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            GbVar.bInputWaitMsg[2] = false;

            int[] nOnOutputArr;
            int[] nOffOutputArr;

            if (!GbFunc.IsGdTrayTableYMoveSafe())
            {
                if ((GbVar.IO[IODF.INPUT.GD_TRAY_1_STG_UP] == 1 && GbVar.IO[IODF.INPUT.GD_TRAY_2_STG_UP] == 1) ||
                        (GbVar.IO[IODF.INPUT.GD_TRAY_1_STG_DOWN] == 1 && GbVar.IO[IODF.INPUT.GD_TRAY_2_STG_DOWN] == 1))
                {
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_UP, true);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_UP, false);
                    MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN, true);
                }
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 0)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_GRIP, false);
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 0)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_GRIP, false);
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 0)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.RW_TRAY_STG_GRIP, false);
            }

            nOnOutputArr = new int[] {
                                             (int)IODF.OUTPUT.BTM_VISION_ALIGN_BWD,
                                             (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP,
                                             (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP,
                                             (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP,
                                             (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP,
                                             (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP,

                                             (int)IODF.OUTPUT.GD_1_ELV_STOPPER_BWD,
                                             (int)IODF.OUTPUT.GD_2_ELV_STOPPER_BWD,
                                             (int)IODF.OUTPUT.RW_ELV_STOPPER_BWD,
                                             (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD,
                                             (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD
            };

            nOffOutputArr = new int[] {
                                            (int)IODF.OUTPUT.BTM_VISION_ALIGN_FWD,
                                            (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN,
                                            (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN,
                                            (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN,
                                            (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN,
                                            (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN,

                                            (int)IODF.OUTPUT.GD_1_ELV_STOPPER_FWD,
                                            (int)IODF.OUTPUT.GD_2_ELV_STOPPER_FWD,
                                            (int)IODF.OUTPUT.RW_ELV_STOPPER_FWD,
                                            (int)IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD,
                                            (int)IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD
            };
            
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
            if (!swInputCheck.IsRunning) swInputCheck.Restart();

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nOnInputArr = new int[] {  //(int)IODF.INPUT.REJECT_BOX_DOOR_CLOSE,
                                             (int)IODF.INPUT.BALL_VISION_ALIGN_BWD,
                                             (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_UP,
                                             (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_UP,
                                             (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_UP,
                                             (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_UP,
                                             (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_UP,
                                             (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_UP,
                                             (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_UP,
                                             (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_UP,
                                             (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_UP,
                                             (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_UP,

                                             (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_BWD,
                                             (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_BWD,
                                             (int)IODF.INPUT.RW_TRAY_STG_ALIGN_BWD,

                                             (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_OPEN,
                                             (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_OPEN,
                                             (int)IODF.INPUT.RW_TRAY_STG_GRIP_OPEN,

                                            };
            int[] nOffInputArr = new int[] { //(int)IODF.INPUT.REJECT_BOX_DOOR_OPEN,
                                             (int)IODF.INPUT.BALL_VISION_ALIGN_FWD,
                                             (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_DOWN,
                                             (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_RT_DOWN,
                                             (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_DOWN,
                                             (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_RT_DOWN,
                                             (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_DOWN,
                                             (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_DOWN,
                                             (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_DOWN,
                                             (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_DOWN,
                                             (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_DOWN,
                                             (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_DOWN,

                                             (int)IODF.INPUT.GD_1_TRAY_ELV_GUIDE_FWD,
                                             (int)IODF.INPUT.GD_2_TRAY_ELV_GUIDE_FWD,
                                             (int)IODF.INPUT.RW_TRAY_ELV_GUIDE_FWD,
                                             (int)IODF.INPUT.EMTY_1_TRAY_ELV_GUIDE_FWD,
                                             (int)IODF.INPUT.EMTY_2_TRAY_ELV_GUIDE_FWD,

                                             (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_FWD,
                                             (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_FWD,
                                             (int)IODF.INPUT.RW_TRAY_STG_ALIGN_FWD,
                                             
                                             (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_CLOSE,
                                             (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_CLOSE,
                                             (int)IODF.INPUT.RW_TRAY_STG_GRIP_CLOSE,

                                             };

            int[] nOnErrArr = new int[] {  //(int)IODF.INPUT.REJECT_BOX_DOOR_CLOSE,
                                            (int)ERDF.E_BALL_VISION_ALIGN_BWD,
                                            (int)ERDF.E_GD_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_RW_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_RW_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL,

                                            (int)ERDF.E_GD_TRAY_STAGE_1_ALIGN_BWD_FAIL,
                                            (int)ERDF.E_GD_TRAY_STAGE_2_ALIGN_BWD_FAIL,
                                            (int)ERDF.E_RW_TRAY_STAGE_ALIGN_BWD_FAIL,

                                            (int)ERDF.E_GD_TRAY_STAGE_1_UNGRIP_FAIL,
                                            (int)ERDF.E_GD_TRAY_STAGE_2_UNGRIP_FAIL,
                                            (int)ERDF.E_RW_TRAY_STAGE_UNGRIP_FAIL,
                                            };

            int[] nOffErrArr = new int[] { //(int)IODF.INPUT.REJECT_BOX_DOOR_OPEN,
                                            (int)ERDF.E_BALL_VISION_ALIGN_BWD,
                                            (int)ERDF.E_GD_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_RW_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_RW_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL,
                                            (int)ERDF.E_EMPTY_2_TRAY_ELV_STACKER_UP_FAIL,

                                            (int)ERDF.E_GD_1_TRAY_ELV_GUIDE_BWD_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_ELV_GUIDE_BWD_FAIL,
                                            (int)ERDF.E_RW_TRAY_ELV_GUIDE_BWD_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_GUIDE_BWD_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_ELV_GUIDE_BWD_FAIL,

                                            (int)ERDF.E_GD_1_TRAY_CONV_STOPPER_BWD_FAIL,
                                            (int)ERDF.E_GD_2_TRAY_CONV_STOPPER_BWD_FAIL,
                                            (int)ERDF.E_RW_TRAY_CONV_STOPPER_BWD_FAIL,
                                            (int)ERDF.E_EMPTY_1_TRAY_CONV_STOPPER_BWD_FAIL,
                                            (int)ERDF.E_EMPTY_2_TRAY_CONV_STOPPER_BWD_FAIL,

                                            (int)ERDF.E_GD_TRAY_STAGE_1_ALIGN_BWD_FAIL,
                                            (int)ERDF.E_GD_TRAY_STAGE_2_ALIGN_BWD_FAIL,
                                            (int)ERDF.E_RW_TRAY_STAGE_ALIGN_BWD_FAIL,

                                            (int)ERDF.E_GD_TRAY_STAGE_1_UNGRIP_FAIL,
                                            (int)ERDF.E_GD_TRAY_STAGE_2_UNGRIP_FAIL,
                                            (int)ERDF.E_RW_TRAY_STAGE_UNGRIP_FAIL,
                                             };




            if (!GbFunc.IsGdTrayTableYMoveSafe())
            {
                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue < swInputCheck.ElapsedMilliseconds)
                {
                    return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                }
                else
                {
                    return FNC.BUSY;
                }
            }

            for (int nOnInput = 0; nOnInput < nOnInputArr.Length; nOnInput++)
            {
                if (GbVar.GB_INPUT[nOnInputArr[nOnInput]] != 1)
                {
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_BWD && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_BWD && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.RW_TRAY_STG_ALIGN_BWD && GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_OPEN && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_OPEN && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOnInputArr[nOnInput] == (int)IODF.INPUT.RW_TRAY_STG_GRIP_OPEN && GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1)
                    {
                        continue;
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue < swInputCheck.ElapsedMilliseconds)
                    {
                        return nOnErrArr[nOnInput];
                    }
                    else
                    {
                        return FNC.BUSY;
                    }
                }
            }

            for (int nOffInput = 0; nOffInput < nOffInputArr.Length; nOffInput++)
            {
                if (GbVar.GB_INPUT[nOffInputArr[nOffInput]] != 0)
                {
                    if(nOffInputArr[nOffInput] == (int)IODF.INPUT.GD_TRAY_1_STG_ALIGN_FWD && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOffInputArr[nOffInput] == (int)IODF.INPUT.GD_TRAY_2_STG_ALIGN_FWD && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOffInputArr[nOffInput] == (int)IODF.INPUT.RW_TRAY_STG_ALIGN_FWD && GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOffInputArr[nOffInput] == (int)IODF.INPUT.GD_TRAY_1_STG_GRIP_CLOSE && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_1_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOffInputArr[nOffInput] == (int)IODF.INPUT.GD_TRAY_2_STG_GRIP_CLOSE && GbVar.GB_INPUT[(int)IODF.INPUT.GD_TRAY_2_CHECK] == 1)
                    {
                        continue;
                    }
                    if (nOffInputArr[nOffInput] == (int)IODF.INPUT.RW_TRAY_STG_GRIP_CLOSE && GbVar.GB_INPUT[(int)IODF.INPUT.RW_TRAY_CHECK] == 1)
                    {
                        continue;
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].lValue < swInputCheck.ElapsedMilliseconds)
                    {
                        return nOffErrArr[nOffInput];
                    }
                    else
                    {
                        return FNC.BUSY;
                    }
                }
            }
            swInputCheck.Stop();
            return FNC.SUCCESS;
        }


        public int UldConvAllStop()
        {
            return FNC.SUCCESS;
        }

        public int AxisHome(SVDF.AXES Axis)
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(Axis)] = false;
                MotionMgr.Inst[SVDF.Get(Axis)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(Axis)] = false;
                    MotionMgr.Inst[SVDF.Get(Axis)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(Axis)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(Axis)] = false;
                        nHomeErrAxisOffset = (int)Axis;
                        return FNC.BUSY;
                    }
                }
            }

            bCmdHomeFlag = false;
            GbVar.mcState.isHomeComplete[SVDF.Get(Axis)] = true;

            return FNC.SUCCESS;
        }

        //231206 jy.yang 추가 홈 구동 전, 3mm 하강 필요
        double[] HomePos1 = new double[CFG_DF.MAX_PICKER_PAD_CNT];
        double[] HomePos2 = new double[CFG_DF.MAX_PICKER_PAD_CNT];
        public int ChipPickerMoveDownBeforeHome()
        {

            bool isDone = false;

            //구동할 축들에 모션정보 배열 할당
            MOTINFO[] mArray = new MOTINFO[CFG_DF.MAX_PICKER_PAD_CNT * 2];

            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                int i2 = CFG_DF.MAX_PICKER_PAD_CNT + i;

                mArray[i] = new MOTINFO();
                mArray[i].nAxis = SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2));
                mArray[i].dPos = 5.0; //+ MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].GetRealPos();
                mArray[i].dVel = 50;
                mArray[i].dAcc = 500;
                mArray[i].dDec = 500;

                mArray[i2] = new MOTINFO();
                mArray[i2].nAxis = SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2));
                mArray[i2].dPos = 5.0; // + MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].GetRealPos();
                mArray[i2].dVel = 50;
                mArray[i2].dAcc = 500;
                mArray[i2].dDec = 500;
            }



            return RunLib.SvMove(mArray, 0);


            //if (!bCmdMoveBeforeHome)
            //{
            //    timeStampInit.Restart();

            //    for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            //    {
            //        Debug.WriteLine(MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].GetRealPos());
            //        Debug.WriteLine(MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].GetRealPos());

            //        HomePos1[i] = MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].GetRealPos() + 3.0;
            //        HomePos2[i] = MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].GetRealPos() + 3.0;


            //        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].MoveAbs(HomePos1[i],50,500,500);
            //        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].MoveAbs(HomePos2[i],50,500,500);
            //    }
            //    Debug.WriteLine("Move Send");

            //    bCmdMoveBeforeHome = true;
            //    return FNC.BUSY;
            //}
            //else
            //{
            //    if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
            //    {
            //        for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            //        {
            //            MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].MoveStop();
            //            MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].MoveStop();
            //        }

            //        bCmdMoveBeforeHome = false;
            //        timeStampInit.Stop();
            //        return (int)ERDF.E_SV_MOVE + nHomeErrAxisOffset;
            //    }
            //    else
            //    {

            //        for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            //        {
            //            int nAxis = (int)(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2));
            //            uint uRef = 0;
            //            CAXM.AxmStatusReadInMotion(nAxis, ref uRef); //uRef 1일때 구동

            //            //sj.shin 모션 구동 완료 확인 변수
            //            isDone &= (!MotionMgr.Inst[nAxis].IsBusy() &&
            //                        MotionMgr.Inst
            //                        [nAxis].IsInPosition(HomePos1[i]) &&
            //                        uRef != 1);

            //            isDone &= (!MotionMgr.Inst[nAxis].IsBusy() &&
            //                       MotionMgr.Inst
            //                       [nAxis].IsInPosition(HomePos2[i]) &&
            //                       uRef != 1);
            //        }

            //        if (isDone == false)
            //        {
            //            return FNC.BUSY;
            //        }
            //        //for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            //        //{
            //        //    if (!MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].IsInPosition()
            //        //        || !MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].IsInPosition())
            //        //    {
            //        //        return FNC.BUSY;
            //        //    }
            //        //}
            //        //if (WaitDelay(5000)) return FNC.BUSY;

            //        for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            //        {
            //            Debug.WriteLine(MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].GetRealPos());
            //            Debug.WriteLine(MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].GetRealPos());
            //        }
            //        bCmdMoveBeforeHome = false;
            //        return FNC.SUCCESS;
            //    }

            //}
        }

        public int SorterAllZAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_VISION_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_Z)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Z)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)] = false;
                

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_VISION_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_Z)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Z)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)].HomeStart();

                ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                //MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].HomeStart();

                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))] = false;
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))].HomeStart();

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))] = false;
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].HomeStart();

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))] = false;
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))].HomeStart();

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))] = false;
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].HomeStart();
                }

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_VISION_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_Z)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Z)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)] = false;
                    ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                    //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                    //MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_VISION_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Z)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)].MoveStop();

                    for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))] = false;
                        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))].MoveStop();

                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))] = false;
                        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].MoveStop();

                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))] = false;
                        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))].MoveStop();

                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))] = false;
                        MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].MoveStop();
                    }

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                    //if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    //{
                    //    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                    //    nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_Z;
                    //    return FNC.BUSY;
                    //}

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_VISION_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_VISION_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_VISION_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_PK_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.BALL_VISION_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.TRAY_PK_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.GD_TRAY_1_ELV_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.GD_TRAY_2_ELV_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.RW_TRAY_ELV_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z;
                        return FNC.BUSY;
                    }

                    for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                    {
                        if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                        {
                            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))] = false;
                            nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_1_T_1 + (i * 2);
                            return FNC.BUSY;
                        }

                        if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                        {
                            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))] = false;
                            nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2);
                            return FNC.BUSY;
                        }

                        if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                        {
                            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))] = false;
                            nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_2_T_1 + (i * 2);
                            return FNC.BUSY;
                        }

                        if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                        {
                            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))] = false;
                            nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2);
                            return FNC.BUSY;
                        }
                    }
                }
            }

            bCmdHomeFlag = false;
            ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
            //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_VISION_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_Z)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Z)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_1_ELV_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_2_ELV_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_ELV_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_1_ELV_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.EMTY_TRAY_2_ELV_Z)] = true;

            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
            {
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_T_1 + (i * 2))] = true;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_Z_1 + (i * 2))] = true;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_T_1 + (i * 2))] = true;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_Z_1 + (i * 2))] = true;
            }

            return FNC.SUCCESS;
        }

        public int SorterAllXYAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
          
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_X)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Y)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Y)] = false;

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)] = false;

                //임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                //MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_X)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Y)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Y)].HomeStart();

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_X)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Y)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Y)] = false;

                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)] = false;

                    //임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                    //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                    //MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_X)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Y)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_X)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Y)].MoveStop();

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
                    //if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    //{
                    //    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                    //    nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_X;
                    //    return FNC.BUSY;
                    //}

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_PK_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_STG_1_Y;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_STG_2_Y;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_1_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.CHIP_PK_2_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.BALL_VISION_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.BALL_VISION_Y;
                        return FNC.BUSY;
                    }

                    //
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.TRAY_PK_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.TRAY_PK_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.TRAY_PK_Y;
                        return FNC.BUSY;
                    }

                    //
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.GD_TRAY_STG_1_Y;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.GD_TRAY_STG_2_Y;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.RW_TRAY_STG_Y;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.DRY_BLOCK_STG_X;
                        return FNC.BUSY;
                    }
                }
            }

            bCmdHomeFlag = false;

            ////임시로 넣음 스트립 피커 해체 (2022.07.29 홍은표)
            //GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_X)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_Y)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_Y)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_1_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.CHIP_PK_2_X)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Y)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_Y)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_1_Y)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.GD_TRAY_STG_2_Y)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.RW_TRAY_STG_Y)] = true;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.DRY_BLOCK_STG_X)] = true;

            return FNC.SUCCESS;
        }

        public int SorterMapTableTAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();

                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_T)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_T)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_T)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_T)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_T)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_T)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_T)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_T)].MoveStop();
                    
                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_1_T)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_T)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_STG_1_T;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAP_STG_2_T)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_T)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAP_STG_2_T;
                        return FNC.BUSY;
                    }
                }
            }

            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_1_T)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_STG_2_T)] = true;
            
            return FNC.SUCCESS;
        }

        public int CheckServoOnHome(params SVDF.AXES[] axes)
        {
            for (int nCnt = 0; nCnt < axes.Length; nCnt++)
            {
                int nAxisNo = (int)axes[nCnt];

                if (MotionMgr.Inst[nAxisNo].GetServoOnOff() == false)
                    return (int)ERDF.E_SV_SERVO_ON + nAxisNo;

                if (MotionMgr.Inst[nAxisNo].GetHomeResult() != HomeResult.HR_Success)
                    return (int)ERDF.E_SV_NOT_HOME + nAxisNo;
            }

            return FNC.SUCCESS;
        }

        public int InterlockCheck()
        {
            //CLEAN PK Z <-> DRY BLOCK X
            //if (GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] == false ||
            //    Math.Abs(MotionMgr.Inst[(int)SVDF.AXES.UNIT_PK_Z].GetRealPos()) > 5)
            //{
            //    return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_MOVE_UNIT_PK_DOWN;
            //}
            return FNC.SUCCESS;
        }
    }
}
