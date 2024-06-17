using DionesTool.Motion;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnMachineAllHome : SeqBase
    {
        mnLoaderHome LdHome = new mnLoaderHome();
        mnSawHome SawHome = new mnSawHome();
        mnSorterHome SorterHome = new mnSorterHome();

        bool[] bHommingStartCmd = new bool[3];
        public static bool bIsCmdAllOn = false;

        bool[] bHomeCmdEachAxis = new bool[100];
        bool bHomeCmdEachAxisExcute = false;

        bool bCmdHomeFlag = false;
        int nHomeErrAxisOffset = 0;
        double dMovePos = 0.0;
        int nAxisNo = -1;

        Stopwatch swTableLevelCheck = new Stopwatch();

        #region SEQUENCE ENUM
        enum SEQ_NO_INIT
        {
            RESET_AXIS_ALARM,
            MACHINE_LOADER_HOME,
            MACHINE_SORTER_HOME,
            MACHINE_SAW_HOME,
            WAIT_HOME_COMPLETE,
            VACUUM_CHECK, //DEVICE CHECK
            HOME_COMPLETE,

            ALL_HOME_INIT,
            ALL_HOME_STEP1_SV_RESET,
            ALL_HOME_STEP2_WAIT_RESET_DELAY,
            ALL_HOME_STEP3_CYLINDER_INTERLOCK_CHECK,
            ALL_HOME_STEP4_CYLINDER_INIT,
            ALL_HOME_STEP5_CYLINDER_INIT_CHECK,
            ALL_HOME_STEP6_AXIS_ARR_HOME_1,
            ALL_HOME_STEP7_AXIS_ARR_HOME_2,
            ALL_HOME_STEP8_AXIS_ARR_HOME_3,
            ALL_HOME_STEP9_ALL_HOME_FINISH,

        }
        #endregion

        Stopwatch timeStampInit = new Stopwatch();

        // [2022.04.22.kmlee] 전체 홈 동시를 위한 Task 추가
        Task<int>[] _taskAllHome = new Task<int>[MCDF.MACHINE_MAX];

        // [2022.04.22.kmlee] Task 완료 플래그
        bool[] _flagTaskCompleteArray = new bool[MCDF.MACHINE_MAX];

        // [2022.04.22.kmlee] 동시 Home(true)을 할지 순차적 Home을 할지(false)
        bool _flagHomeAsyncMode = true;

        public mnMachineAllHome()
        {
            _taskAllHome[MCDF.LOADER] = new Task<int>(delegate { return StartHomeUnit(MCDF.LOADER); });
            _taskAllHome[MCDF.SAW] = new Task<int>(delegate { return StartHomeUnit(MCDF.SAW); });
            _taskAllHome[MCDF.SORTER] = new Task<int>(delegate { return StartHomeUnit(MCDF.SORTER); });
        }

        int StartHomeUnit(int nUnitNo)
        {
            // 사용하지 않으면 바로 return FNC.SUCCESS;
            if (!bHommingStartCmd[nUnitNo])
            {
                return FNC.SUCCESS;
            }

            int nReturn = FNC.BUSY;
            while (nReturn == FNC.BUSY)
            {
                if (_flagTaskCompleteArray[nUnitNo])
                    break;

                System.Threading.Thread.Sleep(100);

                switch (nUnitNo)
                {
                    case MCDF.LOADER:
                        {
                            nReturn = LdHome.Init();
                        }
                        break;
                    case MCDF.SAW:
                        {
                            nReturn = SawHome.Init();
                        }
                        break;
                    case MCDF.SORTER:
                        {
                            nReturn = SorterHome.Init();
                        }
                        break;
                    default:
                        break;
                }
            }
            return nReturn;
        }

        public void CancelHome()
        {
            // 기존 모드라면 사용 안함
            if (!_flagHomeAsyncMode) return;

            TaskNewObject();
        }

        void TaskNewObject()
        {
            if (_taskAllHome[MCDF.LOADER].Status == TaskStatus.Running)
            {
                _flagTaskCompleteArray[MCDF.LOADER] = true;

                _taskAllHome[MCDF.LOADER].Wait(1000);
            }
            if (_taskAllHome[MCDF.SAW].Status == TaskStatus.Running)
            {
                _flagTaskCompleteArray[MCDF.SAW] = true;

                _taskAllHome[MCDF.SAW].Wait(1000);
            }
            if (_taskAllHome[MCDF.SORTER].Status == TaskStatus.Running)
            {
                _flagTaskCompleteArray[MCDF.SORTER] = true;

                _taskAllHome[MCDF.SORTER].Wait(1000);
            }

            if (_taskAllHome[MCDF.LOADER].Status == TaskStatus.RanToCompletion)
            {
                _flagTaskCompleteArray[MCDF.LOADER] = false;
                _taskAllHome[MCDF.LOADER].Dispose();
            }
            if (_taskAllHome[MCDF.LOADER].Status != TaskStatus.Created)
            {
                _taskAllHome[MCDF.LOADER] = new Task<int>(delegate { return StartHomeUnit(MCDF.LOADER); });
            }

            if (_taskAllHome[MCDF.SAW].Status == TaskStatus.RanToCompletion)
            {
                _flagTaskCompleteArray[MCDF.SAW] = false;
                _taskAllHome[MCDF.SAW].Dispose();
            }
            if (_taskAllHome[MCDF.SAW].Status != TaskStatus.Created)
            {
                _taskAllHome[MCDF.SAW] = new Task<int>(delegate { return StartHomeUnit(MCDF.SAW); });
            }

            if (_taskAllHome[MCDF.SORTER].Status == TaskStatus.RanToCompletion)
            {
                _flagTaskCompleteArray[MCDF.SORTER] = false;
                _taskAllHome[MCDF.SORTER].Dispose();
            }
            if (_taskAllHome[MCDF.SORTER].Status != TaskStatus.Created)
            {
                _taskAllHome[MCDF.SORTER] = new Task<int>(delegate { return StartHomeUnit(MCDF.SORTER); });
            }
        }

        public void SetHommingPara(bool bLoaderHome, bool bSawHome, bool bSorterHome)
        {
            bHommingStartCmd[MCDF.LOADER] = bLoaderHome;
            bHommingStartCmd[MCDF.SAW] = bSawHome;
            bHommingStartCmd[MCDF.SORTER] = bSorterHome;
        }

        void NextSeq(SEQ_NO_INIT seq)
        {
            base.NextSeq((int)seq);
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

            switch ((SEQ_NO_INIT)m_nSeqNo)
            {
                case SEQ_NO_INIT.RESET_AXIS_ALARM:
                    ResetAxisAlarm();

                    //nFuncResult = IsAllAxisHomeCompleted();
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //    NextSeq(SEQ_NO_INIT.VACUUM_CHECK);
                    //    return;
                    //}

                    if (bHommingStartCmd[MCDF.LOADER]) LdHome.InitSeq();
                    if (bHommingStartCmd[MCDF.SAW]) SawHome.InitSeq();
                    if (bHommingStartCmd[MCDF.SORTER]) SorterHome.InitSeq();

                    // [2022.04.22.kmlee] 동시 홈 추가
                    if (_flagHomeAsyncMode)
                    {
                        TaskNewObject();
                    }

                    //if (bHommingStartCmd[MCDF.LOADER] && bHommingStartCmd[MCDF.SAW] && bHommingStartCmd[MCDF.SORTER])
                    //{
                    //    NextSeq(SEQ_NO_INIT.ALL_HOME_INIT);
                    //    return;
                    //}
                    //else
                    //{
                    //    NextSeq(SEQ_NO_INIT.MACHINE_LOADER_HOME);
                    //    return;
                    //}
                    break;

                case SEQ_NO_INIT.MACHINE_LOADER_HOME:
                    {
                        // [2022.04.22.kmlee] 동시 Home 사용 시 무조건 호출해준다. 내부에서 bHommingStartCmd를 확인함
                        if (_flagHomeAsyncMode)
                        {
                            _taskAllHome[MCDF.LOADER].Start();
                        }
                        else
                        {
                            if (bHommingStartCmd[MCDF.LOADER])
                            {
                                nFuncResult = LdHome.Init();
                            }
                        }
                    }
                    break;


                case SEQ_NO_INIT.MACHINE_SORTER_HOME:
                    {
                        // [2022.04.22.kmlee] 동시 Home 사용 시 무조건 호출해준다. 내부에서 bHommingStartCmd를 확인함
                        if (_flagHomeAsyncMode)
                        {
                            _taskAllHome[MCDF.SORTER].Start();
                        }
                        else
                        {
                            if (bHommingStartCmd[MCDF.SORTER])
                            {
                                nFuncResult = SorterHome.Init();
                            }
                        }
                    }
                    break;

                case SEQ_NO_INIT.MACHINE_SAW_HOME:
                    {
                        // [2022.04.22.kmlee] 동시 Home 사용 시 무조건 호출해준다. 내부에서 bHommingStartCmd를 확인함
                        if (_flagHomeAsyncMode)
                        {
                            _taskAllHome[MCDF.SAW].Start();
                        }
                        else
                        {
                            if (bHommingStartCmd[MCDF.SAW])
                            {
                                nFuncResult = SawHome.Init();
                            }
                        }
                    }
                    break;
                case SEQ_NO_INIT.WAIT_HOME_COMPLETE:
                    {
                        // [2022.04.22.kmlee] 홈 완료를 기다림
                        if (_flagHomeAsyncMode)
                        {
                            if (_taskAllHome[MCDF.LOADER].Wait(10) == false ||
                                _taskAllHome[MCDF.SAW].Wait(10) == false ||
                                _taskAllHome[MCDF.SORTER].Wait(10) == false)
                            {
                                nFuncResult = FNC.BUSY;
                            }
                            else
                            {
                                if (bHommingStartCmd[MCDF.LOADER] == true)
                                {
                                    if (_taskAllHome[MCDF.LOADER].Result > 0)
                                    {
                                        nFuncResult = _taskAllHome[MCDF.LOADER].Result;
                                    }
                                }
                                if (bHommingStartCmd[MCDF.SAW] == true)
                                {
                                    if (_taskAllHome[MCDF.SAW].Result > 0)
                                    {
                                        nFuncResult = _taskAllHome[MCDF.SAW].Result;
                                    }
                                }
                                if (bHommingStartCmd[MCDF.SORTER] == true)
                                {
                                    if (_taskAllHome[MCDF.SORTER].Result > 0)
                                    {
                                        nFuncResult = _taskAllHome[MCDF.SORTER].Result;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SEQ_NO_INIT.VACUUM_CHECK:
                    break;

                case SEQ_NO_INIT.HOME_COMPLETE:
                    FINISH = true;
                    return;

                case SEQ_NO_INIT.ALL_HOME_INIT:
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            GbVar.g_timeStampInit[i].Restart();
                            GbVar.mcState.isInitialized[i] = false;
                            GbVar.mcState.isInitializing[i] = true;
                        }
                        swTableLevelCheck.Reset();
                        bCmdHomeFlag = false;
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP1_SV_RESET:
                    {
                        // 보류
                        //// 상태가 이상해서 끈 후 켬
                        //MotionMgr.Inst[SVDF.AXES.LD_RAIL_T].SetServoOnOff(false);
                        //Thread.Sleep(100);

                        for (int nSvCnt = 0; nSvCnt < SVDF.SV_CNT; nSvCnt++)
                        {
                            if (MotionMgr.Inst[nSvCnt].IsAlarm())
                            {
                                MotionMgr.Inst[nSvCnt].ResetAlarm();
                                Thread.Sleep(100);
                            }

                            MotionMgr.Inst[nSvCnt].SetServoOnOff(true);
                        }
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP2_WAIT_RESET_DELAY:
                    {
                        if (WaitDelay(5000)) return;
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP3_CYLINDER_INTERLOCK_CHECK:
                    {
                        nFuncResult = CmdCylinderInterlockCheck();
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP4_CYLINDER_INIT:
                    {
                        nFuncResult = CmdCylinderInit();
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP5_CYLINDER_INIT_CHECK:
                    {
                        nFuncResult = CmdCylinderInitCheck();
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP6_AXIS_ARR_HOME_1:
                    {
                        int[] nAxisArrHome1 = new int[]
                        {
                            (int)SVDF.AXES.MAP_PK_Z,
                            (int)SVDF.AXES.MAP_VISION_Z,
                            (int)SVDF.AXES.BALL_VISION_Z,
                            (int)SVDF.AXES.TRAY_PK_Z,
                            (int)SVDF.AXES.GD_TRAY_1_ELV_Z,
                            (int)SVDF.AXES.GD_TRAY_2_ELV_Z,
                            (int)SVDF.AXES.RW_TRAY_ELV_Z,
                            (int)SVDF.AXES.EMTY_TRAY_1_ELV_Z,
                            (int)SVDF.AXES.EMTY_TRAY_2_ELV_Z,
                            (int)SVDF.AXES.LD_VISION_X,
                            (int)SVDF.AXES.STRIP_PK_Z,
                            (int)SVDF.AXES.UNIT_PK_Z,
                            
                            (int)SVDF.AXES.CHIP_PK_1_Z_1,
                            (int)SVDF.AXES.CHIP_PK_1_Z_2,
                            (int)SVDF.AXES.CHIP_PK_1_Z_3,
                            (int)SVDF.AXES.CHIP_PK_1_Z_4,
                            (int)SVDF.AXES.CHIP_PK_1_Z_5,
                            (int)SVDF.AXES.CHIP_PK_1_Z_6,
                            (int)SVDF.AXES.CHIP_PK_1_Z_7,
                            (int)SVDF.AXES.CHIP_PK_1_Z_8,

                            (int)SVDF.AXES.CHIP_PK_1_T_1,
                            (int)SVDF.AXES.CHIP_PK_1_T_2,
                            (int)SVDF.AXES.CHIP_PK_1_T_3,
                            (int)SVDF.AXES.CHIP_PK_1_T_4,
                            (int)SVDF.AXES.CHIP_PK_1_T_5,
                            (int)SVDF.AXES.CHIP_PK_1_T_6,
                            (int)SVDF.AXES.CHIP_PK_1_T_7,
                            (int)SVDF.AXES.CHIP_PK_1_T_8,

                            (int)SVDF.AXES.CHIP_PK_2_Z_1,
                            (int)SVDF.AXES.CHIP_PK_2_Z_2,
                            (int)SVDF.AXES.CHIP_PK_2_Z_3,
                            (int)SVDF.AXES.CHIP_PK_2_Z_4,
                            (int)SVDF.AXES.CHIP_PK_2_Z_5,
                            (int)SVDF.AXES.CHIP_PK_2_Z_6,
                            (int)SVDF.AXES.CHIP_PK_2_Z_7,
                            (int)SVDF.AXES.CHIP_PK_2_Z_8,

                            (int)SVDF.AXES.CHIP_PK_2_T_1,
                            (int)SVDF.AXES.CHIP_PK_2_T_2,
                            (int)SVDF.AXES.CHIP_PK_2_T_3,
                            (int)SVDF.AXES.CHIP_PK_2_T_4,
                            (int)SVDF.AXES.CHIP_PK_2_T_5,
                            (int)SVDF.AXES.CHIP_PK_2_T_6,
                            (int)SVDF.AXES.CHIP_PK_2_T_7,
                            (int)SVDF.AXES.CHIP_PK_2_T_8,
                        };
                        nFuncResult = AxisArrHome(nAxisArrHome1);
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP7_AXIS_ARR_HOME_2:
                    {
                        int[] nAxisArrHome2 = new int[]
                        {
                            (int)SVDF.AXES.DRY_BLOCK_STG_X,
                            (int)SVDF.AXES.MAP_STG_1_Y,
                            (int)SVDF.AXES.MAP_STG_2_Y,
                            //(int)SVDF.AXES.CHIP_PK_1_X,
                            (int)SVDF.AXES.CHIP_PK_2_X,
                            (int)SVDF.AXES.BALL_VISION_Y,
                            (int)SVDF.AXES.MAP_PK_X,
                            (int)SVDF.AXES.TRAY_PK_X,
                            (int)SVDF.AXES.TRAY_PK_Y,
                            (int)SVDF.AXES.GD_TRAY_STG_1_Y,
                            (int)SVDF.AXES.GD_TRAY_STG_2_Y,
                            (int)SVDF.AXES.RW_TRAY_STG_Y,
                            (int)SVDF.AXES.LD_RAIL_T,
                            (int)SVDF.AXES.LD_RAIL_Y_FRONT,
                            (int)SVDF.AXES.STRIP_PK_X,
                            (int)SVDF.AXES.UNIT_PK_X,
                        };

                        nFuncResult = AxisArrHome(nAxisArrHome2);
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP8_AXIS_ARR_HOME_3:
                    {
                        int[] nAxisArrHome3 = new int[]
                        {
                            (int)SVDF.AXES.MAGAZINE_ELV_Z
                        };

                        nFuncResult = AxisArrHome(nAxisArrHome3);
                    }
                    break;
                case SEQ_NO_INIT.ALL_HOME_STEP9_ALL_HOME_FINISH:
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            GbVar.mcState.isInitialized[i] = true;
                            GbVar.mcState.isInitializing[i] = false;
                            GbVar.g_timeStampInit[i].Stop();
                        }
                        bCmdHomeFlag = false;

                        FINISH = true;
                        return;
                    }
                default:
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                swTableLevelCheck.Reset();
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

        int ResetAxisAlarm()
        {
#if _AJIN
            for (int nMotCount = 0; nMotCount < SVDF.SV_CNT; nMotCount++)
            {
                if (nMotCount == (int)SVDF.AXES.NONE || nMotCount == (int)SVDF.AXES.MAX) continue;

                if (MotionMgr.Inst[nMotCount].IsAlarm() == true) MotionMgr.Inst[nMotCount].ResetAlarm();

                if (MotionMgr.Inst[nMotCount].IsAlarm() == true)
                {
                    return (int)ERDF.E_SV_AMP + nMotCount;
                }

                if (MotionMgr.Inst[nMotCount].GetDeviceType() == DionesTool.Motion.DeviceType.DEVICE_AJIN)
                {
                    if (MotionMgr.Inst[nMotCount].GetServoOnOff() == false)
                        MotionMgr.Inst[nMotCount].SetServoOnOff(true);
                }
            }
#endif
            return FNC.SUCCESS;
        }

        int IsAllAxisHomeCompleted()
        {

#if !_AJIN
            return FNC.SUCCESS;
#endif

            DionesTool.Motion.HomeResult homeResult = DionesTool.Motion.HomeResult.HR_Fail;
            foreach (SVDF.AXES axis in Enum.GetValues(typeof(SVDF.AXES)))
            {
                switch (axis)
                {
                    case SVDF.AXES.NONE:
                    case SVDF.AXES.MAX:
                        continue;
                }

                if (MotionMgr.Inst[axis] == null) continue;

                homeResult = MotionMgr.Inst[axis].GetHomeResult();

                if (homeResult == DionesTool.Motion.HomeResult.HR_Fail)
                    return (int)ERDF.E_SV_NOT_HOME + (int)axis;
                if (homeResult == DionesTool.Motion.HomeResult.HR_Process)
                    return FNC.BUSY;

                if (!MotionMgr.Inst[axis].GetServoOnOff()) return FNC.BUSY;
                if (MotionMgr.Inst[axis].IsBusy()) return FNC.BUSY;
            }

            //if (MotionMgr.Inst.IsAllHomeCompleted() == false)
            //{
            //    return FNC.BUSY;
            //}

            return FNC.SUCCESS;
        }

        public int CmdCylinderInterlockCheck()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif

            if (!GbFunc.IsGdTrayTableYMoveSafe())
            {
                //간섭 축 홈 상태 확인
                nFuncResult = CheckServoOnHome(SVDF.AXES.GD_TRAY_STG_1_Y, SVDF.AXES.GD_TRAY_STG_2_Y);
                if (FNC.IsSuccess(nFuncResult))
                {
                    double dTable1Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();
                    double dTable2Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

                    if (dTable1Pos - dTable2Pos > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                    {
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_UP, true);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_1_STG_DOWN, false);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_UP, false);
                        MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.GD_TRAY_2_STG_DOWN, true);
                        if (!swTableLevelCheck.IsRunning) swTableLevelCheck.Restart();
                        if (swTableLevelCheck.Elapsed.TotalMilliseconds > 3000)
                        {
                            swTableLevelCheck.Reset();
                            return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        }
                        else
                        {
                            return FNC.BUSY;
                        }
                    }
                    else
                    {
                        swTableLevelCheck.Reset();
                        return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    }
                }
                else
                {
                    swTableLevelCheck.Reset();
                    return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                }
            }

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


        public int CmdCylinderInit()
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nOnOutputArr;
            int[] nOffOutputArr;

            nOnOutputArr = new int[] {
                (int)IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL,
                (int)IODF.OUTPUT.LD_X_GRIP_UP,
                (int)IODF.OUTPUT.BTM_VISION_ALIGN_BWD,
                (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP,
                (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP,
                (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP,
                (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP,
                (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP
            };

            nOffOutputArr = new int[] {
                (int)IODF.OUTPUT.MZ_DOOR_CLOSE_SOL,
                (int)IODF.OUTPUT.LD_X_GRIP_DOWN,
                (int)IODF.OUTPUT.LD_X_GRIP,
                (int)IODF.OUTPUT.BTM_VISION_ALIGN_FWD,
                (int)IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN,
                (int)IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN,
                (int)IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN,
                (int)IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN,
                (int)IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN
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
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int[] nOnInputArr = new int[] {
                (int)IODF.INPUT.SPARE_3118,
                (int)IODF.INPUT.BALL_VISION_ALIGN_BWD,
                (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_UP,
                (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_UP,
                (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_UP,
                (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_UP,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_UP,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_UP,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_UP,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_UP,
            };

            int[] nOffInputArr = new int[] {
                (int)IODF.INPUT.SPARE_3117,
                (int)IODF.INPUT.BALL_VISION_ALIGN_FWD,
                (int)IODF.INPUT.GD_1_TRAY_ELV_STACKER_LF_DOWN,
                (int)IODF.INPUT.GD_2_TRAY_ELV_STACKER_LF_DOWN,
                (int)IODF.INPUT.RW_TRAY_ELV_STACKER_LF_DOWN,
                (int)IODF.INPUT.RW_TRAY_ELV_STACKER_RT_DOWN,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_LF_DOWN,
                (int)IODF.INPUT.EMTY_1_TRAY_ELV_STACKER_RT_DOWN,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_LF_DOWN,
                (int)IODF.INPUT.EMTY_2_TRAY_ELV_STACKER_RT_DOWN,
            };

            if (!GbFunc.IsGdTrayTableYMoveSafe())
            {
                return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
            }

            for (int nOnInput = 0; nOnInput < nOnInputArr.Length; nOnInput++)
            {
                if (GbVar.GB_INPUT[nOnInputArr[nOnInput]] != 1)
                    return FNC.BUSY;
            }

            for (int nOffInput = 0; nOffInput < nOffInputArr.Length; nOffInput++)
            {
                if (GbVar.GB_INPUT[nOffInputArr[nOffInput]] != 0)
                    return FNC.BUSY;
            }

            return FNC.SUCCESS;
        }

        public int AxisHome(SVDF.AXES Axis)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
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

        public int AxisArrHome(int[] nAxisArr)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            if (!bCmdHomeFlag)
            {
                timeStampInit.Restart();

                // Interlock Check
                for (int nCnt = 0; nCnt < nAxisArr.Length; nCnt++)
                {
                    nFuncResult = SafetyMgr.Inst.GetAxisSafetyBeforeHome(nAxisArr[nCnt]);
                    if (FNC.IsErr(nFuncResult))
                    {
                        return nFuncResult;
                    }
                }

                for (int nAxisNo = 0; nAxisNo < nAxisArr.Length; nAxisNo++)
                {
                    GbVar.mcState.isHomeComplete[nAxisArr[nAxisNo]] = false;
                    //MotionMgr.Inst[nAxisArr[nAxisNo]].HomeStart();
                }

                bCmdHomeFlag = true;

                bHomeCmdEachAxisExcute = false;
                bHomeCmdEachAxis = new bool[100];
                return FNC.BUSY;
            }
            else if (!bHomeCmdEachAxisExcute)
            {
                //각 축마다 홈 명령시 20msec 딜레이 추가하도록 함

                if (timeStampInit.ElapsedMilliseconds < 20)
                {
                    return FNC.BUSY;
                }

                for (int nAxisNo = 0; nAxisNo < nAxisArr.Length; nAxisNo++)
                {
                    if (!bHomeCmdEachAxis[nAxisNo])
                    {
                        MotionMgr.Inst[nAxisArr[nAxisNo]].HomeStart();
                        bHomeCmdEachAxis[nAxisNo] = true;
                        timeStampInit.Restart();
                        return FNC.BUSY;
                    }
                }

                bHomeCmdEachAxisExcute = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    for (int nAxisNo = 0; nAxisNo < nAxisArr.Length; nAxisNo++)
                    {
                        GbVar.mcState.isHomeComplete[nAxisArr[nAxisNo]] = false;
                        MotionMgr.Inst[nAxisArr[nAxisNo]].MoveStop();
                    }

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    for (int nAxisNo = 0; nAxisNo < nAxisArr.Length; nAxisNo++)
                    {
                        if (MotionMgr.Inst[nAxisArr[nAxisNo]].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                        {
                            GbVar.mcState.isHomeComplete[nAxisArr[nAxisNo]] = false;
                            nHomeErrAxisOffset = nAxisNo;
                            return FNC.BUSY;
                        }
                    }
                }
            }

            bCmdHomeFlag = false;
            bHomeCmdEachAxisExcute = false;

            for (int nAxisNo = 0; nAxisNo < nAxisArr.Length; nAxisNo++)
            {
                GbVar.mcState.isHomeComplete[nAxisArr[nAxisNo]] = true;
            }
            return FNC.SUCCESS;
        }

        public int CheckServoOnHome(params SVDF.AXES[] axes)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
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
    }
}
