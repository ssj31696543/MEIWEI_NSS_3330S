using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    class mnSawHome : SeqBase
    {
        bool bCmdHomeFlag = false;
        int nHomeErrAxisOffset = 0;
        double dMovePos = 0.0;
        int nAxisNo = -1;

        #region SEQUENCE ENUM
        enum SEQ_NO_INIT
        {
            HOME_INIT,
            INTERLOCK_CHECK,
            CYLINDER_INIT,
            CYLINDER_INIT_CHECK,
            SORTER_AXIS_CHECK,
            TRSF_Z_HOME,

            UNIT_PK_X_INC_SAFETY_POS_MOVE,

            CLEAN_Z_SIG_P_SEARCH,
            CLEAN_Z_OFFSET_MOVE,

            CLEAN_R_HOME,

            CLEAN_Z_HOME,
            CLEAN_Z_READY_POS_MOVE,
            
            ALL_HOME_EXCEPT_Z_AXIS, // ALIGN CAM Y AXIS 포함
            CLEANER_Z_DOWN,
            UNIT_PK_X_SAFETY_200_MOVE,
            UNIT_PK_MOVE_CHECK,
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
                    GbVar.g_timeStampInit[MCDF.SAW].Restart();
                    GbVar.mcState.isInitialized[MCDF.SAW] = false;
                    GbVar.mcState.isInitializing[MCDF.SAW] = true;
                    bCmdHomeFlag = false;
                    break;
                case SEQ_NO_INIT.INTERLOCK_CHECK:
                    nFuncResult = InterlockCheck();
                    break;
                case SEQ_NO_INIT.CYLINDER_INIT:
                    nFuncResult = CmdCylinderInit();
                    break;
                case SEQ_NO_INIT.CYLINDER_INIT_CHECK:
                    nFuncResult = CmdCylinderInitCheck();
                    break;
                case SEQ_NO_INIT.TRSF_Z_HOME:
                    nFuncResult = TransferZAxisHome();
                    break;
                case SEQ_NO_INIT.ALL_HOME_EXCEPT_Z_AXIS:
                    nFuncResult = XYTAxisHome();
                    break;
                case SEQ_NO_INIT.UNIT_PK_X_SAFETY_200_MOVE:
                    break;
                case SEQ_NO_INIT.UNIT_PK_MOVE_CHECK:
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALREADY_ALL:
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALL:
                    break;
                case SEQ_NO_INIT.HOME_ALL_COMPLETE:
                    GbVar.mcState.isInitialized[MCDF.SAW] = true;
                    GbVar.mcState.isInitializing[MCDF.SAW] = false;
                    
                    GbVar.g_timeStampInit[MCDF.SAW].Stop();
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

        public int CmdCylinderInit()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            GbVar.bInputWaitMsg[1] = false;

            int[] nOnOutputArr = new int[] { (int)IODF.OUTPUT.LD_X_UNGRIP, (int)IODF.OUTPUT.IN_LET_TABLE_DOWN, (int)IODF.OUTPUT.LD_X_GRIP_UP };

            int[] nOffOutputArr = new int[] { (int)IODF.OUTPUT.LD_X_GRIP, (int)IODF.OUTPUT.IN_LET_TABLE_UP, (int)IODF.OUTPUT.LD_X_GRIP_DOWN };

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
            int[] nOnInputArr = new int[] { (int)IODF.INPUT.LD_X_UNGRIP, (int)IODF.INPUT.IN_LET_TABLE_DOWN, (int)IODF.INPUT.LD_X_GRIP_UP };

            int[] nOffInputArr = new int[] {(int)IODF.INPUT.LD_X_GRIP, (int)IODF.INPUT.IN_LET_TABLE_UP, (int)IODF.INPUT.LD_X_GRIP_DOWN };

            ERDF[] errOnInputArr = new ERDF[] { ERDF.E_LD_VISION_GRIPPER_UNGRIP_FAIL, ERDF.E_IN_LET_TABLE_DOWN_FAIL, ERDF.E_LD_VISION_GRIPPER_UP_FAIL };
            ERDF[] errOffInputArr = new ERDF[] { ERDF.E_LD_VISION_GRIPPER_GRIP_FAIL, ERDF.E_IN_LET_TABLE_UP_FAIL, ERDF.E_LD_VISION_GRIPPER_DOWN_FAIL };

            for (int nOnInput = 0; nOnInput < nOnInputArr.Length; nOnInput++)
            {
                if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue))
                    return (int)errOnInputArr[nOnInput];

                if (GbVar.GB_INPUT[nOnInputArr[nOnInput]] != 1)
                    return FNC.BUSY;
            }

            for (int nOffInput = 0; nOffInput < nOffInputArr.Length; nOffInput++)
            {
                if (IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CYL_MOVE_TIME].nValue))
                    return (int)errOffInputArr[nOffInput];

                if (GbVar.GB_INPUT[nOffInputArr[nOffInput]] != 0)
                    return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }
        public int InterlockCheck()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            //자재가 투입되어 있으면 알람
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_RAIL_TOP_STRIP_DETECT_SENSOR_USE].bOptionUse)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3111] == 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
                {
                    // 매거진 상단 센서 A접으로 변경

                    return (int)ERDF.E_INTL_LD_GRIP_MATERIAL_DETECT_ALARM;
                }
            }
            else
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_FRONT_MATERIAL_CHECK] == 1 ||
                     GbVar.GB_INPUT[(int)IODF.INPUT.LD_RAIL_Y_REAR_MATERIAL_CHECK] == 1 ||
                     GbVar.GB_INPUT[(int)IODF.INPUT.GRIP_MATERIAL_CHECK] == 1)
                {
                    return (int)ERDF.E_INTL_LD_GRIP_MATERIAL_DETECT_ALARM;
                }
            } 

            return FNC.SUCCESS;
        }
        public int SorterAxisCheck()
        {
            return FNC.SUCCESS;
        }

        public int TransferZAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_Z)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_Z)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_Z)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_Z)].MoveStop();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.STRIP_PK_Z;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_Z;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_Z)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_Z)] = true;

            return FNC.SUCCESS;
        }

        public int XYTAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_T)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_VISION_X)] = false;
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_T)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_VISION_X)].HomeStart();
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_T)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_VISION_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = false;


                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_T)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_X)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_VISION_X)].HomeStart();
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].HomeStart();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_T)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_T)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.LD_RAIL_T;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.LD_RAIL_Y_FRONT;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.LD_RAIL_Y_REAR;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.LD_VISION_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_VISION_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.LD_VISION_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.STRIP_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.STRIP_PK_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.UNIT_PK_X)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.UNIT_PK_X;
                        return FNC.BUSY;
                    }

                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.BARCODE_Y;
                        return FNC.BUSY;
                    }
                }
            }

            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_T)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_FRONT)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_RAIL_Y_REAR)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.STRIP_PK_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.UNIT_PK_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.LD_VISION_X)] = true;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = true;

            return FNC.SUCCESS;
        }

        // 신규 Load Gripper X 축 , Barcode Y 축 동시 원점
        // 따로 쓰는 경우에만 사용하면 됨
        public int RailGripperXAndBarcodeYAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = false;

                MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = false;

                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.BARCODE_Y)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.BARCODE_Y;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;

            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BARCODE_Y)] = true;

            return FNC.SUCCESS;
        }

        public int CheckServoOnHome(params SVDF.AXES[] axes)
        {
            for (int nCnt = 0; nCnt < axes.Length; nCnt++)
            {
                int nAxisNo = (int)axes[nCnt];

                if (MotionMgr.Inst[nAxisNo].GetServoOnOff() == false)
                    return (int)ERDF.E_SV_SERVO_ON + nAxisNo;

                if (MotionMgr.Inst[nAxisNo].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    return (int)ERDF.E_SV_NOT_HOME + nAxisNo;
            }

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

        public int CleanerRInterlockCheck()
        {
            ////Cleane R축의 홈 시그널이 꺼져 있거나 서보오프 상태라면 에러
            //if (CheckServoOnHome(SVDF.AXES.NONE) != FNC.SUCCESS)
            //{
            //    return CheckServoOnHome(SVDF.AXES.NONE);
            //}

            //if (!SafetyMgr.Inst.IsInPos(SVDF.AXES.NONE, POSDF.CLEANER_FLIP_STRIP_FLIP_FRONT, 10) || !SafetyMgr.Inst.IsInPos(SVDF.AXES.NONE, POSDF.CLEANER_FLIP_STRIP_FLIP_REAR, 10))
            //{
            //    return (int)ERDF.E_INTL_CLEANER_Z_CAN_NOT_HOME_CELAN_R_IS_NOT_SAFE;
            //}
        
            return FNC.SUCCESS;
        }
    }
}
