using DionesTool.Motion;
using NSS_3330S;
using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S
{
    class SafetyMgr
    {
        private static volatile SafetyMgr instance;
        private static object syncRoot = new object();

        //SAFTEY
        public static int nSaftey_Z_Down = 8;
        public static int nSaftey_X = 350;

        //HEP 추가 에어리어 센서 튈까봐 넣음
        int m_nLdElvY_InterlockCount = 0;
        int m_nLdElvZ_InterlockCount = 0;
        public static SafetyMgr Inst
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SafetyMgr();
                    }
                }
                return instance;
            }
        }


        public SafetyMgr()
        {

        }
        ~SafetyMgr()
        {

        }

        public void Dispose()
        {

        }

        public void SetDoorLock()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] != 1 || GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] != 1)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_MONITOR_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_LF_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_CENTER_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_RT_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.REAR_CENTER_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.REAR_RT_DOOR] != 1)
                    //GbVar.GB_INPUT[(int)IODF.INPUT.OHT_FENCE_FRONT_DOOR] != 1 ||
                    //GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_RT_DOOR_SAW] != 1 ||
                    //GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_LF_DOOR_SAW] != 1 ||
                    //GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_MONITOR_DOOR_SAW] != 1)
                {
                    return;
                }

                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL, true);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, true);
            }
        }

        public void SetDoorLockLoader()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD] != 1)
            {
                if (
                    GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_LF_DOOR_SAW] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_RT_DOOR_SAW] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FRONT_MONITOR_DOOR_SAW] != 1)
                {
                    return;
                }

                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, true);
            }
        }

        public void SetDoorLockSorter()
        {
            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.DOOR_LOCK_SIGNAL] != 1)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_MONITOR_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_LF_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_CENTER_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.RT_SIDE_RT_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.REAR_CENTER_DOOR] != 1 ||
                    GbVar.GB_INPUT[(int)IODF.INPUT.REAR_RT_DOOR] != 1 /*||
                    GbVar.GB_INPUT[(int)IODF.INPUT.OHT_FENCE_FRONT_DOOR] != 1*/
                    )
                {
                    return;
                }

                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL, true);
            }
        }

        public void ResetDoorLock()
        {
            if (GbFunc.IsDoorOpenOrPressEmo(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse) == FNC.SUCCESS)
            {
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL, false);
                MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, false);
            }
        }

        public void ResetDoorLockLoader()
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD, false);
        }

        public void ResetDoorLockSorter()
        {
            MotionMgr.Inst.SetOutput((int)IODF.OUTPUT.DOOR_LOCK_SIGNAL, false);
        }

        /// <summary>
        /// 220505 pjh
        /// SAW CUTTING TABLE <-> Strip Picker Z 인터락 비트 On Off
        /// </summary>
        public void CuttingTableInterlockToSaw()
        {
            bool isDangerL = false;

            //Z축 안전 위치아님
            if (IsMoreThanPosNoZ(SVDF.AXES.STRIP_PK_Z, POSDF.STRIP_PICKER_READY, 10))
            {
                //다운 되어있고 Cutting Table간섭 위치
                if (!IsLessThanPosNo(SVDF.AXES.STRIP_PK_X,
                    POSDF.STRIP_PICKER_UNLOADING_TABLE_1,
                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].nValue))
                {
                    isDangerL = true;
                }
            }

            //Z축 안전 위치아님
            if (IsMoreThanPosNoZ(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_SPONGE_START, 10))
            {
                //다운 되어있고 Cutting Table간섭 위치
                if (!IsLessThanPosNo(SVDF.AXES.UNIT_PK_X,
                    POSDF.UNIT_PICKER_LOADING_TABLE_1,
                    ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].nValue))
                {
                    isDangerL = true;
                }
            }

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_PK_INTERLOCK] != Convert.ToInt32(isDangerL))
                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SORTER_PK_INTERLOCK, isDangerL);

            if (GbVar.GB_OUTPUT[(int)IODF.OUTPUT.SORTER_PK_INTERLOCK_R] != Convert.ToInt32(isDangerL))
                MotionMgr.Inst.SetOutput(IODF.OUTPUT.SORTER_PK_INTERLOCK_R, isDangerL);
            //만약 양쪽 테이블 사이에서 다운되어 있다면....둘다 켜야하는데 추후 생각해 보기
        }


        public int GetSafetyDoor(bool bOptionIgnore = false)
        {
            //DOOR
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)
            {
                GbVar.MCDOORSTATE = true;

                int[] nDoor = new int[] {
                    (int)IODF.INPUT.MC_31,
                    (int)IODF.INPUT.FORNT_MONITOR_DOOR,
                    (int)IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR,
                    (int)IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR,
                    (int)IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR,
                    (int)IODF.INPUT.REAR_CENTER_DOOR,
                    (int)IODF.INPUT.REAR_LF_DOOR,
                    (int)IODF.INPUT.REAR_RT_DOOR,
                    (int)IODF.INPUT.RT_SIDE_CENTER_DOOR,
                    (int)IODF.INPUT.RT_SIDE_LF_DOOR,
                    (int)IODF.INPUT.RT_SIDE_RT_DOOR,
                    (int)IODF.INPUT.FRONT_MONITOR_DOOR_SAW,
                    (int)IODF.INPUT.FRONT_RT_DOOR_SAW,
                    (int)IODF.INPUT.FRONT_LF_DOOR_SAW};

                bool bOpen = false;
                int nErrNo = FNC.SUCCESS;
                for (int i = 0; i < nDoor.Length; i++)
                {
                    bOpen |= GbVar.GB_INPUT[(nDoor[i])] == 0;
                    if (GbVar.GB_INPUT[(nDoor[i])] == 0)
                    {
                        GbVar.MCDOORSTATE = false;
                        nErrNo = (int)ERDF.E_DOOR_OPEN_1 + i;
                    }
                }

                if (nErrNo != FNC.SUCCESS)
                {
                    return nErrNo;
                }
            }
            return FNC.SUCCESS;
        }

        public int GetSafetyDoorLoader()
        {

            //DOOR
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)
            {
                GbVar.MCDOORSTATE = true;

                //TODO 대응하는 SENSOR찾기
                int[] nDoor = new int[] {
                                          };

                bool bOpen = false;
                int nErrNo = FNC.SUCCESS;
                for (int i = 0; i < nDoor.Length; i++)
                {
                    bOpen |= GbVar.GB_INPUT[(nDoor[i])] == 0;
                    if (GbVar.GB_INPUT[(nDoor[i])] == 0)
                    {
                        GbVar.MCDOORSTATE = false;
                        nErrNo = (int)ERDF.E_DOOR_OPEN_1 + i;
                    }
                }

                if (nErrNo != FNC.SUCCESS)
                {
                    return nErrNo;
                }
            }
            return FNC.SUCCESS;
        }
        public int GetSafetyDoorSorter()
        {

            //DOOR
            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DOOR_SAFETY_CHECK_USE].bOptionUse)
            {
                GbVar.MCDOORSTATE = true;

                int[] nDoor = new int[] {
                                        (int)IODF.INPUT.FORNT_MONITOR_DOOR,
                                        (int)IODF.INPUT.FORNT_MONITOR_RT_SID_DOOR,
                                        (int)IODF.INPUT.FORNT_TRAY_TOP_LF_DOOR,
                                        (int)IODF.INPUT.FORNT_TRAY_TOP_RT_DOOR, 
                                        //(int)IODF.INPUT.LF_SIDE_LF_DOOR,
                                        //(int)IODF.INPUT.LF_SIDE_MID_DOOR,
                                        //(int)IODF.INPUT.LF_SIDE_RT_DOOR,
                                        (int)IODF.INPUT.REAR_CENTER_DOOR,
                                        (int)IODF.INPUT.REAR_LF_DOOR,
                                        (int)IODF.INPUT.REAR_RT_DOOR,
                                        (int)IODF.INPUT.RT_SIDE_CENTER_DOOR,
                                        (int)IODF.INPUT.RT_SIDE_LF_DOOR,
                                        (int)IODF.INPUT.RT_SIDE_RT_DOOR
                                        };

                bool bOpen = false;
                int nErrNo = FNC.SUCCESS;
                for (int i = 0; i < nDoor.Length; i++)
                {
                    bOpen |= GbVar.GB_INPUT[(nDoor[i])] == 0;
                    if (GbVar.GB_INPUT[(nDoor[i])] == 0)
                    {
                        GbVar.MCDOORSTATE = false;
                        nErrNo = (int)ERDF.E_DOOR_OPEN_1 + i;
                    }
                }

                if (nErrNo != FNC.SUCCESS)
                {
                    return nErrNo;
                }
            }
            return FNC.SUCCESS;
        }

        public int GetAlwaysCheckStatus()
        {
            int nErrNo = FNC.SUCCESS;

#if !_NOTEBOOK
            #region EMO알람
            int[] nEMO = new int[] {
                (int)IODF.INPUT.EMO_FRONT_SWITCH_SAW,
                (int)IODF.INPUT.EMO_REAR_SWITCH_SAW,
                (int)IODF.INPUT.EMO_SWITCH_1,
                (int)IODF.INPUT.EMO_SWITCH_2,
                (int)IODF.INPUT.EMO_SWITCH_3,
            };

            for (int i = 0; i < nEMO.Length; i++)
            {
                if (GbVar.GB_INPUT[(nEMO[i])] == 0)
                {
                    bool bIsOn = MotionMgr.Inst.GetInput(nEMO[i]);
                    if (!bIsOn)
                    {
                        GbFunc.WriteEventLog("EMO ALARM OCCURED LOG", string.Format("EMO BIT {0} IS {1}", nEMO[i], MotionMgr.Inst.GetInput(nEMO[i]), bIsOn));
                        nErrNo = (int)ERDF.E_EMO_1 + i;
                        GbVar.MCDOORSTATE = false;
                    }
                    else
                    {
                        GbFunc.WriteEventLog("EMO ALARM UNEXPECTED LOG", string.Format("EMO BIT {0} IS {1}", nEMO[i], MotionMgr.Inst.GetInput(nEMO[i]), bIsOn));
                    }
                }
            }
            if (nErrNo != FNC.SUCCESS)
            {
                for (int nCntCycle = 0; nCntCycle < GbVar.mcState.isCycleRunReq.Length; nCntCycle++)
                {
                    if (GbVar.mcState.IsRun() || !MotionMgr.Inst.IsAllStop())
                    {
                        GbVar.mcState.AllCycleRunStop();
                        MotionMgr.Inst.AllStop();

                        // 전원 다운 후 돌릴 수 있게 임시로 막음
                        // TODO : 전원 다운 후 다시 돌릴 수 있도록 조치 방안 후 해제
                        //GbVar.mcState.isInitialized[MCDF.LOADER] = false;
                        //GbVar.mcState.isInitialized[MCDF.SAW] = false;
                        //GbVar.mcState.isInitialized[MCDF.SORTER] = false;
                    }

                }

                return nErrNo;
            }
            #endregion

            #region 간섭 센서 알람
            //MAP PK VS TRAY PK SAFETY SENSOR CHECK
            if (GbVar.GB_INPUT[(int)IODF.INPUT.MAP_PK_N_TRAY_PK_COLLISION_CHECK] != 1)
            {
                if (MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_X].IsBusy() || MotionMgr.Inst[(int)SVDF.AXES.MAP_PK_X].IsBusy())
                {
                    MotionMgr.Inst[(int)SVDF.AXES.TRAY_PK_X].MoveEStop();
                    MotionMgr.Inst[(int)SVDF.AXES.MAP_PK_X].MoveEStop();
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.TRAY_PK_X)] = false;
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAP_PK_X)] = false;

                    return (int)ERDF.E_INTL_MAP_PK_X_CAN_NOT_MOVE_TRAY_PK_X_NOT_SAFETY_POS;
                }
            }
            #endregion

            #region TRIP 알람
            int[] nMC = new int[2]{ (int)IODF.INPUT.DICING_SERVO_1_MC_TRIP, 
                                     (int)IODF.INPUT.DICING_SERVO_2_MC_TRIP};
            for (int i = 0; i < nMC.Length - 1; i++)
            {
                if (GbVar.GB_INPUT[(nMC[i])] != 1)
                {
                    nErrNo = (int)ERDF.E_DICING_SERVO_MC_1_TRIP + i;
                }
            }
            if (nErrNo != FNC.SUCCESS)
            {
                return nErrNo;
            }
            #endregion

            #region CDA 알람
            int[] nCDA = new int[]{
                (int)IODF.A_INPUT.DRIVE_AIR,
                (int)IODF.A_INPUT.BLOW_AIR,
                (int)IODF.A_INPUT.CHIP_PK_AIR,
                (int)IODF.A_INPUT.MAP_PK_AIR,
                (int)IODF.A_INPUT.MAP_STG_AIR,
                (int)IODF.A_INPUT.DRY_STG_AIR,
                (int)IODF.A_INPUT.PUMP_AIR_1,
                (int)IODF.A_INPUT.PUMP_AIR_2,
            };
            for (int i = 0; i < nCDA.Length; i++)
            {
                if ((GbVar.GB_AINPUT[nCDA[i]] - ConfigMgr.Inst.Cfg.Vac[nCDA[i]].dDefaultVoltage) * ConfigMgr.Inst.Cfg.Vac[nCDA[i]].dRatio < ConfigMgr.Inst.Cfg.Vac[nCDA[i]].dVacLevelLow)
                {
                    nErrNo = (int)ERDF.E_DRIVE_AIR_LOW + (i * 2);
                }
                else if ((GbVar.GB_AINPUT[nCDA[i]] - ConfigMgr.Inst.Cfg.Vac[nCDA[i]].dDefaultVoltage) > ConfigMgr.Inst.Cfg.Vac[nCDA[i]].dVacLevelHigh)
                {
                    nErrNo = (int)ERDF.E_DRIVE_AIR_HIGH + (i * 2);
                }
                if (nErrNo != FNC.SUCCESS)
                {
                    return nErrNo;
                }
            }
            #endregion

            #region 이오나이저 알람
            //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IONIZER_MONITOR_ALARM].bOptionUse)
            //{
            //        for (int nIdx = 0; nIdx < 4; nIdx++)
            //        {
            //            if (GbDev.IonizerMoniroing[nIdx].ChannelMeasureValue.Count > 0)
            //            {
            //                int nIonValue = 0;
            //                int.TryParse(GbDev.IonizerMoniroing[nIdx].IonBalanceValue, out nIonValue);

            //                if (nIonValue > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IONIZER_MAX_VALUE].nValue ||
            //                    nIonValue < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.IONIZER_MIN_VALUE].nValue)
            //                {
            //                    if (!GbVar.mcState.IsError())
            //                    {
            //                        nErrNo = (int)ERDF.E_IONIZER_VALUE_ERROR_1 + nIdx;
            //                    }
            //                }
            //            }

            //        }
            //}
            //if (nErrNo != FNC.SUCCESS)
            //{
            //    return nErrNo;
            //}

            //int[] nIon = new int[] {
            //    (int)IODF.INPUT.ION_BLOWER_FAN_ALARM_1,
            //    (int)IODF.INPUT.ION_BLOWER_FAN_ALARM_2,
            //    (int)IODF.INPUT.ION_BAR_HV_ABNOMAL_ALARM_1,
            //};
            //for (int i = 0; i < nIon.Length; i++)
            //{
            //    if (GbVar.GB_INPUT[(nMC[i])] == 1)
            //    {
            //        nErrNo = (int)ERDF.E_ION_BLOWER_FAN_ALARM_1 + i;
            //    }
            //}
            //if (nErrNo != FNC.SUCCESS)
            //{
            //    return nErrNo;
            //}
            #endregion
#endif
            return FNC.SUCCESS;
        }

        public int GetCylinderSafetyBeforeOn(int nOutput)
        {
            IODF.OUTPUT output = (IODF.OUTPUT)nOutput;

            switch (output)
            {
                case IODF.OUTPUT.NONE:
                    break;
                case IODF.OUTPUT.TEN_KEY_1:
                    break;
                case IODF.OUTPUT.TEN_KEY_2:
                    break;
                case IODF.OUTPUT.TEN_KEY_3:
                    break;
                case IODF.OUTPUT.TEN_KEY_4:
                    break;
                case IODF.OUTPUT.TEN_KEY_5:
                    break;
                case IODF.OUTPUT.TEN_KEY_6:
                    break;
                case IODF.OUTPUT.TEN_KEY_7:
                    break;
                case IODF.OUTPUT.TEN_KEY_8:
                    break;
                case IODF.OUTPUT.TEN_KEY_9:
                    break;
                case IODF.OUTPUT.HPC_POWER_SWITCH_LED:
                    break;
                case IODF.OUTPUT.VPC_POWER_SWITCH_LED:
                    break;
                case IODF.OUTPUT.START_SWITCH_LED:
                    break;
                case IODF.OUTPUT.STOP_SWITCH_LED:
                    break;
                case IODF.OUTPUT.RESET_SWITCH_LED:
                    break;
                case IODF.OUTPUT.DOOR_LOCK_SIGNAL:
                    break;
                case IODF.OUTPUT.FLUORESCENT_LIGHT_ONOFF:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_RED_LED:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_YELLOW_LED:
                    break;
                case IODF.OUTPUT.TOWER_LAMP_GREEN_LED:
                    break;
                case IODF.OUTPUT.ERROR_BUZZER:
                    break;
                case IODF.OUTPUT.END_BUZZER:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_RUN_1:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_1:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_RUN_2:
                    break;
                case IODF.OUTPUT.ION_BLOWER_FAN_TIP_CLEANING_2:
                    break;
                case IODF.OUTPUT.ION_BAR_RUN_1:
                    break;
                case IODF.OUTPUT.SERVO_POWER_OFF:
                    break;
                case IODF.OUTPUT.SPARE_026:
                    break;
                case IODF.OUTPUT.IONIZER_AIR:
                    break;
                case IODF.OUTPUT.BTM_VISION_ALIGN_FWD:
                    break;
                case IODF.OUTPUT.BTM_VISION_ALIGN_BWD:
                    if (Math.Abs(TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.BALL_VISION_Z].dPos[POSDF.BALL_VISION_READY] - MotionMgr.Inst[(int)SVDF.AXES.BALL_VISION_Z].GetRealPos()) > 0.5f ||
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.BALL_VISION_Z)] != true)
                    {
                        return (int)ERDF.E_INTL_BALL_VISION_CYL_FWD_BWD;
                    }
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_UP:
                    if(GbFunc.IsGdTrayTableUpDownSafe(0, true) == false)
                    {
                        return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    }
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_DOWN:
                    if (GbFunc.IsGdTrayTableUpDownSafe(0, false) == false)
                    {
                        return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    }
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_UP:
                    if (GbFunc.IsGdTrayTableUpDownSafe(1, true) == false)
                    {
                        return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    }
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_DOWN:
                    if (GbFunc.IsGdTrayTableUpDownSafe(1, false) == false)
                    {
                        return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    }
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_ALIGN_CLAMP:
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_ALIGN_CLAMP:
                    break;
                case IODF.OUTPUT.RW_TRAY_STG_ALIGN_CLAMP:
                    break;
                case IODF.OUTPUT.GD_TRAY_1_STG_GRIP:
                    break;
                case IODF.OUTPUT.GD_TRAY_2_STG_GRIP:
                    break;
                case IODF.OUTPUT.RW_TRAY_STG_GRIP:
                    break;
                case IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_UP:
                    break;
                case IODF.OUTPUT.GD_1_ELV_TRAY_STACKER_DOWN:
                    break;
                case IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_UP:
                    break;
                case IODF.OUTPUT.GD_2_ELV_TRAY_STACKER_DOWN:
                    break;
                case IODF.OUTPUT.RW_ELV_TRAY_STACKER_UP:
                    break;
                case IODF.OUTPUT.RW_ELV_TRAY_STACKER_DOWN:
                    break;
                case IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_UP:
                    break;
                case IODF.OUTPUT.EMTY_1_ELV_TRAY_STACKER_DOWN:
                    break;
                case IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_UP:
                    break;
                case IODF.OUTPUT.EMTY_2_ELV_TRAY_STACKER_DOWN:
                    break;
                case IODF.OUTPUT.SPARE_119:
                    break;
                case IODF.OUTPUT.SPARE_120:
                    break;
                case IODF.OUTPUT.SPARE_121:
                    break;
                case IODF.OUTPUT.TRAY_PK_ATC_CLAMP:
                    break;
                case IODF.OUTPUT.SPARE_123:
                    break;
                case IODF.OUTPUT.SPARE_124:
                    break;
                case IODF.OUTPUT.SPARE_125:
                    break;
                case IODF.OUTPUT.SPARE_126:
                    break;
                case IODF.OUTPUT.SPARE_127:
                    break;
                case IODF.OUTPUT.SPARE_128:
                    break;
                case IODF.OUTPUT.DRY_ST_AIR_KNIFE:
                    break;
                case IODF.OUTPUT.DRY_ST_AIR_PIPE_FRONT:
                    break;
                case IODF.OUTPUT.SPARE_131:
                    break;
                case IODF.OUTPUT.GD1_CONV_STOP:
                    break;
                case IODF.OUTPUT.GD1_CONV_RESET:
                    break;
                case IODF.OUTPUT.GD2_CONV_STOP:
                    break;
                case IODF.OUTPUT.GD2_CONV_RESET:
                    break;
                case IODF.OUTPUT.RW_CONV_STOP:
                    break;
                case IODF.OUTPUT.RW_CONV_RESET:
                    break;
                case IODF.OUTPUT.EMPTY_CONV_STOP:
                    break;
                case IODF.OUTPUT.EMPTY_CONV_RESET:
                    break;
                case IODF.OUTPUT.SPARE_208:
                    break;
                case IODF.OUTPUT.MARK_VISION_BLOW:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_PK_BLOW:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_VAC_PUMP:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_VAC_ON:
                    break;
                case IODF.OUTPUT.SPARE_216:
                    break;
                case IODF.OUTPUT.DRY_BLOCK_BLOW:
                    break;
                case IODF.OUTPUT.MAP_ST_1_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_ST_1_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_ST_1_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_221:
                    break;
                case IODF.OUTPUT.SPARE_222:
                    break;
                case IODF.OUTPUT.SPARE_223:
                    break;
                case IODF.OUTPUT.MAP_ST_2_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.MAP_ST_2_VAC_ON:
                    break;
                case IODF.OUTPUT.MAP_ST_2_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_227:
                    break;
                case IODF.OUTPUT.SPARE_228:
                    break;
                case IODF.OUTPUT.SPARE_229:
                    break;
                case IODF.OUTPUT.SPARE_230:
                    break;
                case IODF.OUTPUT.SPARE_231:
                    break;
                case IODF.OUTPUT.GD_1_ELV_STOPPER_FWD:
                    break;
                case IODF.OUTPUT.GD_1_ELV_STOPPER_BWD:
                    break;
                case IODF.OUTPUT.GD_2_ELV_STOPPER_FWD:
                    break;
                case IODF.OUTPUT.GD_2_ELV_STOPPER_BWD:
                    break;
                case IODF.OUTPUT.RW_ELV_STOPPER_FWD:
                    break;
                case IODF.OUTPUT.RW_ELV_STOPPER_BWD:
                    break;
                case IODF.OUTPUT.EMPT_1_ELV_STOPPER_FWD:
                    break;
                case IODF.OUTPUT.EMPT_1_ELV_STOPPER_BWD:
                    break;
                case IODF.OUTPUT.EMPT_2_ELV_STOPPER_FWD:
                    break;
                case IODF.OUTPUT.EMPT_2_ELV_STOPPER_BWD:
                    break;
                case IODF.OUTPUT.GD2_CONV_CCW:
                    break;
                case IODF.OUTPUT.RW_CONV_CW:
                    break;
                case IODF.OUTPUT.RW_CONV_CCW:
                    break;
                case IODF.OUTPUT.SPARE_313:
                    break;
                case IODF.OUTPUT.SPARE_314:
                    break;
                case IODF.OUTPUT.SPARE_315:
                    break;
                case IODF.OUTPUT.SPARE_316:
                    break;
                case IODF.OUTPUT.SPARE_317:
                    break;
                case IODF.OUTPUT.SPARE_318:
                    break;
                case IODF.OUTPUT.SPARE_319:
                    break;
                case IODF.OUTPUT.GD1_CONV_CW:
                    break;
                case IODF.OUTPUT.GD1_CONV_CCW:
                    break;
                case IODF.OUTPUT.GD2_CONV_CW:
                    break;
                case IODF.OUTPUT.MAP_ST_AIR_KNIFE_1:
                    break;
                case IODF.OUTPUT.MAP_ST_AIR_KNIFE_2:
                    break;
                case IODF.OUTPUT.MAP_PK_AIR_KNIFE:
                    break;
                case IODF.OUTPUT.EMPTY1_CONV_CW:
                    break;
                case IODF.OUTPUT.EMPTY1_CONV_CCW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_CW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_CCW:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_STOP:
                    break;
                case IODF.OUTPUT.EMPTY2_CONV_RESET:
                    break;
                case IODF.OUTPUT.MAP_STAGE_NO:
                    break;
                case IODF.OUTPUT.PRE_INSP_COMP:
                    break;
                case IODF.OUTPUT.PRE_INSP_GRAB_REQ:
                    break;
                case IODF.OUTPUT.TOP_ALIGN_CNT_RESET:
                    break;
                case IODF.OUTPUT.TOP_ALIGN_COMP:
                    break;
                case IODF.OUTPUT.MAP_CNT_RESET:
                    break;
                case IODF.OUTPUT.MAP_INSP_COMP:
                    break;
                case IODF.OUTPUT.BALL_HEAD_NO:
                    break;
                case IODF.OUTPUT.BALL_CNT_RESET:
                    break;
                case IODF.OUTPUT.BALL_INSP_COMP:
                    break;
                case IODF.OUTPUT.PICKER_CAL_RESET:
                    break;
                case IODF.OUTPUT.PICKER_CAL_COMP:
                    break;
                case IODF.OUTPUT.PICKER_CAL_LED_ON:
                    break;
                case IODF.OUTPUT.PICKER_CAL_LED_OFF:
                    break;
                case IODF.OUTPUT.SPARE_414:
                    break;
                case IODF.OUTPUT.SPARE_415:
                    break;
                case IODF.OUTPUT.SPARE_416:
                    break;
                case IODF.OUTPUT.SPARE_417:
                    break;
                case IODF.OUTPUT.SPARE_418:
                    break;
                case IODF.OUTPUT.SPARE_419:
                    break;
                case IODF.OUTPUT.SPARE_420:
                    break;
                case IODF.OUTPUT.SPARE_421:
                    break;
                case IODF.OUTPUT.SPARE_422:
                    break;
                case IODF.OUTPUT.SPARE_423:
                    break;
                case IODF.OUTPUT.SPARE_424:
                    break;
                case IODF.OUTPUT.SPARE_425:
                    break;
                case IODF.OUTPUT.SPARE_426:
                    break;
                case IODF.OUTPUT.SPARE_427:
                    break;
                case IODF.OUTPUT.SPARE_428:
                    break;
                case IODF.OUTPUT.SPARE_429:
                    break;
                case IODF.OUTPUT.SPARE_430:
                    break;
                case IODF.OUTPUT.SPARE_431:
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_UP:
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_DOWN:
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_VAC:
                    break;
                case IODF.OUTPUT.IN_LET_TABLE_BLOW:
                    break;
                case IODF.OUTPUT.STRIP_PK_AIR_BLOW:
                    break;
                case IODF.OUTPUT.LD_X_GRIP_UP:
                    break;
                case IODF.OUTPUT.LD_X_GRIP_DOWN:
                    break;
                case IODF.OUTPUT.LD_X_UNGRIP:
                    break;
                case IODF.OUTPUT.LD_X_GRIP:
                    break;
                case IODF.OUTPUT.INLET_TABLE_MATERIAL_IN_STOPPER_UP:
                    break;
                case IODF.OUTPUT.INLET_TABLE_MATERIAL_IN_STOPPER_DOWN:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_SOL_QUAD:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.STRIP_PK_BLOW:
                    break;
                case IODF.OUTPUT.SPARE_2015:
                    break;
                case IODF.OUTPUT.SPARE_2016:
                    break;
                case IODF.OUTPUT.SPARE_2017:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_BLOW:
                    break;
                case IODF.OUTPUT.STRIP_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_BLOW:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_1_VAC_ON:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_OFF_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON_PUMP:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_BLOW:
                    break;
                case IODF.OUTPUT.UNIT_PK_SCRAP_2_VAC_ON:
                    break;
                case IODF.OUTPUT.SPARE_2031:
                    break;
                case IODF.OUTPUT.SPARE_2100:
                    break;
                case IODF.OUTPUT.SPARE_2101:
                    break;
                case IODF.OUTPUT.SPARE_2102:
                    break;
                case IODF.OUTPUT.SPARE_2103:
                    break;
                case IODF.OUTPUT.SPARE_2104:
                    break;
                case IODF.OUTPUT.SPARE_2105:
                    break;
                case IODF.OUTPUT.SPARE_2106:
                    break;
                case IODF.OUTPUT.SPARE_2107:
                    break;
                case IODF.OUTPUT.SORTER_PROGRAM_ON:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_X:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_R_Z:
                    break;
                case IODF.OUTPUT.SORTER_LD_OK_R:
                    break;
                case IODF.OUTPUT.SORTER_BLOW_OFF_R:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_X:
                    break;
                case IODF.OUTPUT.SORTER_STRIP_PK_PICK_POS_Z:
                    break;
                case IODF.OUTPUT.SORTER_LD_OK:
                    break;
                case IODF.OUTPUT.SORTER_BLOW_OFF:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_X:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_R_Z:
                    break;
                case IODF.OUTPUT.SORTER_ULD_OK_R:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_X:
                    break;
                case IODF.OUTPUT.SORTER_UNIT_PK_PICK_POS_Z:
                    break;
                case IODF.OUTPUT.SORTER_ULD_OK:
                    break;
                case IODF.OUTPUT.DICING_COMM_8:
                    break;
                case IODF.OUTPUT.DICING_COMM_9:
                    break;
                case IODF.OUTPUT.DICING_COMM_10:
                    break;
                case IODF.OUTPUT.SORTER_HBC_MDL_MOVE_PERMIT:
                    break;
                case IODF.OUTPUT.DICING_COMM_12:
                    break;
                case IODF.OUTPUT.SORTER_RCP_CHANGE:
                    break;
                case IODF.OUTPUT.SORTER_ERROR:
                    break;
                case IODF.OUTPUT.SORTER_PK_INTERLOCK_R:
                    break;
                case IODF.OUTPUT.SORTER_PK_INTERLOCK:
                    break;
                case IODF.OUTPUT._1F_CONVEYOR_INPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._1F_CONVEYOR_OUTPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._2F_CONVEYOR_INPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT._2F_CONVEYOR_OUTPUT_SWITCH_LED:
                    break;
                case IODF.OUTPUT.DOOR_LOCK_SIGNAL_LD:
                    break;
                case IODF.OUTPUT.SERVO_CONTROL_POWER_OFF_SIGNAL:
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_FORWARD_SIGNAL:
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_BACKWARD_SIGNAL:
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_STOP_SIGNAL:
                    break;
                case IODF.OUTPUT._1F_MZ_CONVEYOR_RESET_SIGNAL:
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_FORWARD_SIGNAL:
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_BACKWARD_SIGNAL:
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_STOP_SIGNAL:
                    break;
                case IODF.OUTPUT._2F_MZ_CONVEYOR_RESET_SIGNAL:
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_FORWARD_SIGNAL:
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_BACKWARD_SIGNAL:
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_STOP_SIGNAL:
                    break;
                case IODF.OUTPUT.ME_CONVEYOR_RESET_SIGNAL:
                    break;
                case IODF.OUTPUT.ME_MZ_CLAMP_SOL:
                    break;
                case IODF.OUTPUT.ME_MZ_UNCLAMP_SOL:
                    break;
                case IODF.OUTPUT.MZ_DOOR_OPEN_SOL:
                    break;
                case IODF.OUTPUT.MZ_DOOR_CLOSE_SOL:
                    break;
                case IODF.OUTPUT._1F_MZ_STOPPER_UP_SOL:
                    break;
                case IODF.OUTPUT._1F_MZ_STOPPER_DOWN_SOL:
                    break;
                case IODF.OUTPUT._2F_MZ_STOPPER_UP_SOL:
                    break;
                case IODF.OUTPUT._2F_MZ_STOPPER_DOWN_SOL:
                    break;
                case IODF.OUTPUT._1F_INPUT_STOPPER_UP_SOL:
                    break;
                case IODF.OUTPUT._1F_INPUT_STOPPER_DOWN_SOL:
                    break;
                case IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL:
                    break;
                case IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL:
                    break;
                case IODF.OUTPUT.SPARE_3030:
                    break;
                case IODF.OUTPUT.SPARE_3031:
                    break;
                case IODF.OUTPUT.SPARE_3100:
                    break;
                case IODF.OUTPUT.SPARE_3101:
                    break;
                case IODF.OUTPUT.SPARE_3102:
                    break;
                case IODF.OUTPUT.SPARE_3103:
                    break;
                case IODF.OUTPUT.SPARE_3104:
                    break;
                case IODF.OUTPUT.SPARE_3105:
                    break;
                case IODF.OUTPUT.SPARE_3106:
                    break;
                case IODF.OUTPUT.SPARE_3107:
                    break;
                case IODF.OUTPUT.SPARE_3108:
                    break;
                case IODF.OUTPUT.SPARE_3109:
                    break;
                case IODF.OUTPUT.SPARE_3110:
                    break;
                case IODF.OUTPUT.SPARE_3111:
                    break;
                case IODF.OUTPUT.SPARE_3112:
                    break;
                case IODF.OUTPUT.SPARE_3113:
                    break;
                case IODF.OUTPUT.SPARE_3114:
                    break;
                case IODF.OUTPUT.SPARE_3115:
                    break;
                case IODF.OUTPUT.SPARE_3116:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_1_WATER:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_1_AIR:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_2_WATER:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_2_AIR:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_3_WATER:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_3_AIR:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_4_WATER:
                    break;
                case IODF.OUTPUT.CLEAN_WATER_4_AIR:
                    break;
                case IODF.OUTPUT.CLEAN_BRUSH_WATER:
                    break;
                case IODF.OUTPUT.CLEAN_AIR_KNIFE_1:
                    break;
                case IODF.OUTPUT.CLEAN_AIR_KNIFE_2:
                    break;
                case IODF.OUTPUT.CLEAN_AIR_BLOW_1_PIPE:
                    break;
                case IODF.OUTPUT.CLEAN_AIR_BLOW_2_PIPE:
                    break;
                case IODF.OUTPUT.CLEAN_SWING_FWD:
                    break;
                case IODF.OUTPUT.CLEAN_SWING_BWD:
                    break;
                case IODF.OUTPUT.MAX:
                    break;
                default:
                    break;
            }

            return FNC.SUCCESS;
        }
        /// <summary>
        /// Software Limit만 확인
        /// </summary>
        /// <param name="nAxis"></param>
        /// <param name="dPos"></param>
        /// <returns></returns>
        public int GetAxisSafetySoftLimit(int nAxis, double dPos)
        {
            // TODO : 
            if (ConfigMgr.Inst.Cfg.MotData[nAxis].dSwLimitN > dPos)
            {
                return (int)ERDF.E_SV_SW_LIMIT_N + nAxis;
            }

            if (ConfigMgr.Inst.Cfg.MotData[nAxis].dSwLimitP < dPos)
            {
                return (int)ERDF.E_SV_SW_LIMIT_P + nAxis;
            }
            return FNC.SUCCESS;
        }


        public int GetAxisSafetyBeforePosNo(int nAxis, int nPosNo, bool bSkipPkHeadInterlock = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            if (nAxis < 0) return FNC.SUCCESS;
            SVDF.AXES axes = (SVDF.AXES)nAxis;
            int nTableNo = 0;
            int nFuncResult = 0;

            switch (axes)
            {
                case SVDF.AXES.DRY_BLOCK_STG_X:
                    {
                        if (nPosNo == POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING)
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                            {
                                if (IsLessThanPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1))
                                {
                                    return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                                }
                            }
                        }

                        if (nPosNo == POSDF.DRY_BLOCK_STAGE_STRIP_LOADING)
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_READY))
                            {
                                if (IsLessThanPosNo(SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_SPONGE_END))
                                {
                                    return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_MOVE_UNIT_PK_DOWN;
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_PK_X:
                    {
                        if (nPosNo == POSDF.MAP_PICKER_STRIP_LOADING)
                        {
                            //2022 08. 01 안전함 괜찮음 홍은표
                            //if (IsInPosNo(SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_STRIP_UNLOADING))
                            //{
                            //    // 유닛피커가 언로딩 위치에 있다.
                            //    return (int)ERDF.E_INTL_MAP_PK_X_CAN_NOT_MOVE_UNIT_PK_X_NOT_SAFETY_POS;
                            //}
                        }
                    }
                    break;
                case SVDF.AXES.MAP_PK_Z:
                    {
                        if (nPosNo == POSDF.MAP_PICKER_STRIP_LOADING)
                        {
                            if (!IsInPosNo(SVDF.AXES.DRY_BLOCK_STG_X, POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING))
                            {
                                return (int)ERDF.E_INTL_MAP_PK_Z_CAN_NOT_MOVE_DRY_BLOCK_STG_X_NOT_UNLOADING_POS;
                            }
                        }

                        if (nPosNo == POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1)
                        {
                            if (!IsInPosNo(SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_STRIP_LOADING))
                            {
                                return (int)ERDF.E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_1_Y_NOT_LOADING_POS;
                            }
                        }

                        if (nPosNo == POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2)
                        {
                            if (!IsInPosNo(SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_STRIP_LOADING))
                            {
                                return (int)ERDF.E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_2_Y_NOT_LOADING_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_STG_1_Y:
                    {
                        if (nPosNo == POSDF.MAP_STAGE_STRIP_LOADING)
                        {
                            if (IsInPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1) &&
                                IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_1_Y_NOT_LOADING_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_STG_2_Y:
                    if (nPosNo == POSDF.MAP_STAGE_STRIP_LOADING)
                    {
                        if (IsInPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2) &&
                            IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                        {
                            return (int)ERDF.E_INTL_MAP_PK_Z_CAN_NOT_MOVE_MAP_STG_2_Y_NOT_LOADING_POS;
                        }
                    }
                    break;
                case SVDF.AXES.CHIP_PK_1_X:
                    {
                        if (!bSkipPkHeadInterlock)
                        {
                            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                            {
                                if (i == 0)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                                else
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.CHIP_PK_2_X:
                    {
                        if (!bSkipPkHeadInterlock)
                        {
                            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                            {
                                if (i == 0)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                                else
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.BALL_VISION_Y:
                    break;
                case SVDF.AXES.BALL_VISION_Z:
                    break;
                case SVDF.AXES.TRAY_PK_X:
                    {
                        if (nPosNo == POSDF.TRAY_PICKER_STAGE_GOOD_1)
                        {
                            if (!IsInPosNo(SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                            {
                                return (int)ERDF.E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_GD_TRAY_STG_1_Y_NOT_UNLOADING_POS;
                            }
                        }

                        if (nPosNo == POSDF.TRAY_PICKER_STAGE_GOOD_2)
                        {
                            if (!IsInPosNo(SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                            {
                                return (int)ERDF.E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_GD_TRAY_STG_2_Y_NOT_UNLOADING_POS;
                            }
                        }

                        if (nPosNo == POSDF.TRAY_PICKER_STAGE_REWORK)
                        {
                            if (!IsInPosNo(SVDF.AXES.RW_TRAY_STG_Y, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                            {
                                return (int)ERDF.E_INTL_TRAY_PK_Z_CAN_NOT_MOVE_RW_TRAY_STG_Y_NOT_UNLOADING_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.TRAY_PK_Z:
                    break;
                case SVDF.AXES.TRAY_PK_Y:
                    break;
                case SVDF.AXES.GD_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.GD_TRAY_2_ELV_Z:
                    break;
                case SVDF.AXES.RW_TRAY_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                    break;
                //case SVDF.AXES.COVER_TRAY_ELV_Z:
                   // break;
                case SVDF.AXES.GD_TRAY_STG_1_Y:
                    {
                        // GD_TRAY_1_STG TABLE LEVEL이 같으면
                        // 잠시 보류
                        // 
                        //if (!GbFunc.IsGdTrayTableYMoveSafe())
                        //{
                        //    //테이블간 충돌 위험
                        //    return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        //}

                        nFuncResult = CheckServoOnHome(SVDF.AXES.GD_TRAY_STG_2_Y);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        if (!GbFunc.IsGdTrayTableYMoveSafe(CFG_DF.TRAY_STG_GD1, nPosNo))
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        }
                    }
                    break;
                case SVDF.AXES.GD_TRAY_STG_2_Y:
                    {
                        // GD_TRAY_1_STG TABLE LEVEL이 같으면
                        //if (!GbFunc.IsGdTrayTableYMoveSafe())
                        //{
                        //    //테이블간 충돌 위험
                        //    return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        //}

                        nFuncResult = CheckServoOnHome(SVDF.AXES.GD_TRAY_STG_1_Y);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        if (!GbFunc.IsGdTrayTableYMoveSafe(CFG_DF.TRAY_STG_GD2, nPosNo))
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        }
                    }
                    break;
                case SVDF.AXES.RW_TRAY_STG_Y:
                    break;
                case SVDF.AXES.MAP_STG_1_T:
                    break;
                case SVDF.AXES.MAP_STG_2_T:
                    break;
                case SVDF.AXES.MAP_VISION_Z:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_1:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_1:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_2:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_2:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_3:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_3:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_4:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_4:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_5:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_5:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_6:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_6:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_7:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_7:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_8:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_8:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_1:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_1:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_2:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_2:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_3:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_3:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_4:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_4:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_5:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_5:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_6:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_6:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_7:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_7:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_8:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_8:
                    break;
                case SVDF.AXES.MAGAZINE_ELV_Z:
                    {
                        // NONE 전진상태이면 MZ Z축 이동불가
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_FORWARD_SENSOR] == 1
                         || GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_BACKWARD_SENSOR] != 1)
                        {
                            return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_PUSHER_FWD;
                        }

                        if (GbVar.GB_INPUT[(int)IODF.INPUT.MATERIAL_PROTRUSION_CHECK_SENSOR] == 0)
                        {
                            return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_MOVE_MATERIAL_DETECTED;
                        }
                    }
                    break;
                case SVDF.AXES.LD_RAIL_T:
                    break;
                case SVDF.AXES.LD_RAIL_Y_FRONT:
                    break;
                case SVDF.AXES.LD_RAIL_Y_REAR:
                    break;
                case SVDF.AXES.BARCODE_Y:
                    break;
                case SVDF.AXES.LD_VISION_X:
                    break;
                case SVDF.AXES.STRIP_PK_X:
                    {
                        double dPosStripPkX = 0.0;
                        double dPosUnitPkX = 0.0;

                        dPosStripPkX = TeachMgr.Inst.Tch.dMotPos[nAxis].dPos[nPosNo];
                        dPosUnitPkX = MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].GetTargetMoveAbsPos();

                        double dOffsetX = dPosUnitPkX + dPosStripPkX;

                        if (dOffsetX > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP].dValue)
                        {
                            MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].MoveStop();
                            MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].MoveStop();

                            return (int)ERDF.E_STRIP_PK_n_UNIT_PK_INTERLOCK;
                        }
                        if (nPosNo != POSDF.STRIP_PICKER_STRIP_GRIP &&
                            nPosNo != POSDF.STRIP_PICKER_STRIP_UNGRIP &&
                            nPosNo != POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY &&
                            nPosNo != POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY)
                        {
                            if (!IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_GRIP) &&
                                !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_UNGRIP) &&
                                !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY) &&
                                !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY))
                            {
                                if (GbVar.IO[IODF.INPUT.LD_X_GRIP_UP] == 0 ||
                                    GbVar.IO[IODF.INPUT.LD_X_GRIP_DOWN] == 1)
                                {
                                    return (int)ERDF.E_INTL_STRIP_PK_X_CAN_NOT_MOVE_GRIPPER_DOWN;
                                }
                            }
                        }

                    }
                    break;
                case SVDF.AXES.STRIP_PK_Z:
                    {

                    }
                    break;
                case SVDF.AXES.UNIT_PK_X:
                    {
                        if (nPosNo == POSDF.UNIT_PICKER_SPONGE_START)
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_SPONGE_START))
                            {
                                return (int)ERDF.E_INTL_UNIT_PK_Z_IS_NOT_SPONGE_POSITION;
                            }
                        }
                        else if (nPosNo == POSDF.UNIT_PICKER_SPONGE_END)
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_SPONGE_END))
                            {
                                return (int)ERDF.E_INTL_UNIT_PK_Z_IS_NOT_SPONGE_POSITION;
                            }
                        }
                        else
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_UNIT_PK_X_CAN_NOT_MOVE_UNIT_PK_Z_NOT_READY_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.UNIT_PK_Z:
                    {
                        if (nPosNo == POSDF.UNIT_PICKER_SPONGE_START || nPosNo == POSDF.UNIT_PICKER_SPONGE_END)
                        {
                            if (!IsInPosNo((int)SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_SPONGE_START) &&
                                !IsInPosNo((int)SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_SPONGE_END))
                            {
                                // 20211111 CHOH : X가 스펀지 위치가 아닌데 Z가 스펀지 위치로 내려가려함
                                return (int)ERDF.E_INTL_UNIT_PK_X_IS_NOT_SPONGE_POSITION;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAX:
                    break;
                default:
                    break;
            }
            return FNC.SUCCESS;
        }

        /// <summary>
        /// 해당축의 이동안전 상태 (소프트웨어 리미트, 구동인터록), 구동 전 1회 감시
        /// 구동중 호출하면 안됨!!
        /// </summary>
        /// <param name="nAxis"></param>
        /// <param name="dPos"></param>
        /// <returns></returns>
        public int GetAxisSafetyBeforePosMove(int nAxis, double dPos, bool bSkipPkHeadInterlock = false, bool bSkipMapPkInterlock = false)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            if (nAxis < 0) return FNC.SUCCESS;
                
            int nFuncResult = 0;
            double dGap = 0.1;

            if (ConfigMgr.Inst.Cfg.MotData[nAxis].dSwLimitN > dPos)
            {
                return (int)ERDF.E_SV_SW_LIMIT_N + nAxis;
            }

            if (ConfigMgr.Inst.Cfg.MotData[nAxis].dSwLimitP < dPos)
            {
                return (int)ERDF.E_SV_SW_LIMIT_P + nAxis;
            }

            SVDF.AXES axes = (SVDF.AXES)nAxis;
            switch (axes)
            {
                case SVDF.AXES.DRY_BLOCK_STG_X:
                    {
                        if (IsInPos(SVDF.AXES.UNIT_PK_Z, POSDF.UNIT_PICKER_STRIP_UNLOADING))
                        {
                            if (IsLessThanPosNo(SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_SCRAP_1))
                            {
                                return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_MOVE_UNIT_PK_DOWN;
                            }                       
                        }

                        if (IsInPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_LOADING))
                        {
                            // MAP PK Z축이 READY POS 보다 하강상태이다
                            if (IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_PK_X:
                    {
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // MAP PK Z축이 READY POS 보다 하강상태이다
                        if (IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                        {
                            return (int)ERDF.E_INTL_MAP_PK_X_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS;
                        }
                    }
                    break;
                case SVDF.AXES.MAP_PK_Z:
                    break;
                case SVDF.AXES.MAP_STG_1_Y:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // MAP PK Z 가 DOWN 상태이다.
                        if (IsInPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_1))
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS;
                            }
                        }

                        //이동하려는 위치가 칩피커 간섭 위치 보다 작다면  인터락 체크 안함
                        if (dPos < (TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_1_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_HEIGHT].dValue)) break;
                        

                        nFuncResult = CheckServoOnHome(SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8,
                                                       SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // 테이블 위치가 Unloading 위치보다 클 때
                        if (IsMoreThanPosNo(SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1))
                        {
                            //칩 피커 1번이 픽업 위치라면
                            if (IsMoreThanPosNo(SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].dValue) == false)
                            {
                                // 패드 z축이 하나라도 0.1 이하이면 PK X 이동불가
                                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                        }

                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                        }
                                    }
                                    else
                                    {
                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                        }

                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                        }
                                    }
                                }
                            }
                        }

                        // 테이블 위치가 Unloading 위치보다 클 때
                        if (IsMoreThanPosNo(SVDF.AXES.MAP_STG_1_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P2))
                        {
                            //칩 피커 2번이 픽업 위치 라면 
                            if (IsMoreThanPosNo(SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T1, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].dValue) == false)
                            {
                                // 패드 z축이 하나라도 0.1 이하이면 PK X 이동불가
                                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                        }

                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                        }
                                    }
                                    else
                                    {
                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                        }

                                        if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.10f))
                                        {
                                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                        }
                                    }
                                }
                            }
                        }

                            
                        
                    }
                    break;
                case SVDF.AXES.MAP_STG_2_Y:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // MAP PK Z 가 DOWN 상태이다.
                        if (IsInPosNo(SVDF.AXES.MAP_PK_X, POSDF.MAP_PICKER_STRIP_UNLOADING_STAGE_2))
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.MAP_PK_Z, POSDF.MAP_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_MOVE_MAP_PK_Z_DOWN_POS;
                            }
                        }

                        //이동하려는 위치가 칩피커 간섭 위치 보다 작다면  인터락 체크 안함
                        if (dPos < (TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.MAP_STG_2_Y].dPos[POSDF.MAP_STAGE_UNIT_UNLOADING_P1] - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_HEIGHT].dValue)) break;


                        nFuncResult = CheckServoOnHome(SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8,
                                                        SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;


                        // 테이블 위치가 Unloading 위치보다 클 때
                        if (IsMoreThanPosNo(SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1))
                        {
                            //칩 피커 1번이 픽업 위치 위치라면 
                            if (IsMoreThanPosNo(SVDF.AXES.CHIP_PK_1_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].dValue) == false)
                            {
                                // 패드 z축이 하나라도 0.1 이하이면 PK X 이동불가
                                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2], 0.10f))
                                    {
                                        return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                    }

                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2], 0.10f))
                                    {
                                        return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                    }
                                }
                            }
                        }

                        // 테이블 위치가 Unloading 위치보다 클 때
                        if (IsMoreThanPosNo(SVDF.AXES.MAP_STG_2_Y, POSDF.MAP_STAGE_UNIT_UNLOADING_P1))
                        {
                            //칩 피커 2번이 픽업 위치 위치라면 
                            if (IsMoreThanPosNo(SVDF.AXES.CHIP_PK_2_X, POSDF.CHIP_PICKER_CHIP_LOADING_REF_T2, ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_SIZE_WIDTH].dValue) == false)
                            {
                                // 패드 z축이 하나라도 0.1 이하이면 PK X 이동불가
                                for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2], 0.10f))
                                    {
                                        return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                                    }

                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.TempCfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_2], 0.10f))
                                    {
                                        return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                                    }
                                }
                            }
                        }
                        
                    }
                    break;
                case SVDF.AXES.CHIP_PK_1_X:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        if (!bSkipPkHeadInterlock)
                        {
                            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                            {
                                if (i == 0)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                                else
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_1_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_1].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.1f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_1_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.CHIP_PK_2_X:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        if (!bSkipPkHeadInterlock)
                        {
                            for (int i = 0; i < CFG_DF.MAX_PICKER_PAD_CNT; i++)
                            {
                                if (i == 0)
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i + 1].dPickUpWait[CFG_DF.MAP_STG_1], 0.5f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                                else
                                {
                                    if (IsMoreThanPosZ(SVDF.AXES.CHIP_PK_2_Z_1 + (2 * i), ConfigMgr.Inst.Cfg.OffsetPnP[CFG_DF.HEAD_2].dPnpOffsetZ[i].dPickUpWait[CFG_DF.MAP_STG_1], 0.1f))
                                    {
                                        return (int)ERDF.E_INTL_MULTI_PK_2_X_CAN_NOT_MOVE_PAD_1_IS_DOWN_POS + i;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.BALL_VISION_Y:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.BALL_VISION_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        if (GbVar.GB_INPUT[(int)IODF.INPUT.BALL_VISION_ALIGN_FWD] == 1 || GbVar.GB_INPUT[(int)IODF.INPUT.BALL_VISION_ALIGN_BWD] == 0)
                            return (int)ERDF.E_INTL_VISION_Y_CAN_NOT_MOVE_BALL_VISION_ALIGN_FWD;
                    }
                    break;
                case SVDF.AXES.BALL_VISION_Z:
                    {
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.BALL_VISION_ALIGN_FWD] == 1 || GbVar.GB_INPUT[(int)IODF.INPUT.BALL_VISION_ALIGN_BWD] == 0)
                            return (int)ERDF.E_INTL_VISION_Z_CAN_NOT_MOVE_BALL_VISION_ALIGN_FWD;
                    }
                    break;
                case SVDF.AXES.TRAY_PK_X:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // TRAY PK Z축이 READY POS 보다 하강상태이다
                        if (IsMoreThanPosNoZ(SVDF.AXES.TRAY_PK_Z, POSDF.TRAY_PICKER_READY))
                        {
                            return (int)ERDF.E_INTL_TRAY_PK_X_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                        }
                    }
                    break;
                case SVDF.AXES.TRAY_PK_Z:
                    break;
                case SVDF.AXES.TRAY_PK_Y:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // TRAY PK Z축이 READY POS 보다 하강상태이다
                        if (IsMoreThanPosNoZ(SVDF.AXES.TRAY_PK_Z, POSDF.TRAY_PICKER_READY))
                        {
                            return (int)ERDF.E_INTL_TRAY_PK_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                        }
                    }
                    break;
                case SVDF.AXES.GD_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.GD_TRAY_2_ELV_Z:
                    break;
                case SVDF.AXES.RW_TRAY_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                    break;
                case SVDF.AXES.GD_TRAY_STG_1_Y:
                    {
                        // GD_TRAY_1_STG TABLE LEVEL이 같으면
                        if (!GbFunc.IsGdTrayTableYMoveSafe())
                        {
                            //간섭 축 홈 상태 확인
                            nFuncResult = CheckServoOnHome(SVDF.AXES.GD_TRAY_STG_2_Y);
                            if (FNC.IsErr(nFuncResult)) return nFuncResult;

                            double dTable1Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();
                            double dTable2Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

                            // 1번 테이블이 아래에 있는 경우
                            if (dTable1Pos <= dTable2Pos)
                            {
                                // 1번 테이블이 위로 올라가는 방향인 경우
                                if (dPos > dTable1Pos)
                                {
                                    // 2번 테이블 현재위치 - 테이블 길이값 보다 높이 올라갈 수 없다.
                                    if (dPos > dTable2Pos - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                                    {
                                        return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                                    }
                                }
                            }
                            // 1번 테이블이 위에 있는 경우
                            else
                            {
                                // 1번 테이블이 내려가는 방향인 경우
                                if (dPos < dTable1Pos)
                                {
                                    // 2번 테이블 현재위치 + 테이블 길이값 보다 내려갈 수 없다.
                                    if (dPos < dTable2Pos + ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                                    {
                                        return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                                    }
                                }
                            }
                        }

                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // TRAY PK Z 가 DOWN 상태이다.
                        if (IsInPosNo(SVDF.AXES.GD_TRAY_STG_1_Y, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.TRAY_PK_Z, POSDF.TRAY_PICKER_READY, 5.0))
                            {
                                if (IsInPosNo(SVDF.AXES.TRAY_PK_X, POSDF.TRAY_PICKER_STAGE_GOOD_1))
                                {
                                    return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.GD_TRAY_STG_2_Y:
                    {
                        // GD_TRAY_1_STG TABLE LEVEL이 같으면
                        if (!GbFunc.IsGdTrayTableYMoveSafe())
                        {
                            //간섭 축 홈 상태 확인
                            nFuncResult = CheckServoOnHome(SVDF.AXES.GD_TRAY_STG_1_Y);
                            if (FNC.IsErr(nFuncResult)) return nFuncResult;

                            double dTable1Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos();
                            double dTable2Pos = MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos();

                            // 2번 테이블이 아래에 있는 경우
                            if (dTable2Pos <= dTable1Pos)
                            {
                                // 2번 테이블이 위로 올라가는 방향인 경우
                                if (dPos > dTable2Pos)
                                {
                                    // 1번 테이블 현재위치 - 테이블 길이값 보다 높이 올라갈 수 없다.
                                    if (dPos > dTable1Pos - ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                                    {
                                        return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                                    }
                                }
                            }
                            // 2번 테이블이 위에 있는 경우
                            else
                            {
                                // 2번 테이블이 내려가는 방향인 경우
                                if (dPos < dTable2Pos)
                                {
                                    // 1번 테이블 현재위치 + 테이블 길이값 보다 내려갈 수 없다.
                                    if (dPos < dTable1Pos + ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                                    {
                                        return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                                    }
                                }
                            }
                        }

                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // TRAY PK Z 가 DOWN 상태이다.
                        if (IsInPosNo(SVDF.AXES.GD_TRAY_STG_2_Y, POSDF.TRAY_STAGE_TRAY_UNLOADING))
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.TRAY_PK_Z, POSDF.TRAY_PICKER_READY, 5.0))
                            {
                                if (IsInPosNo(SVDF.AXES.TRAY_PK_X, POSDF.TRAY_PICKER_STAGE_GOOD_2))
                                {
                                    return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.RW_TRAY_STG_Y:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        // TRAY PK Z 가 DOWN 상태이다.
                        if (IsInPosNo(SVDF.AXES.TRAY_PK_X, POSDF.TRAY_PICKER_STAGE_REWORK))
                        {
                            if (IsMoreThanPosNoZ(SVDF.AXES.TRAY_PK_Z, POSDF.TRAY_PICKER_READY))
                            {
                                return (int)ERDF.E_INTL_RW_TRAY_STG_Y_CAN_NOT_MOVE_TRAY_PK_Z_DOWN_POS;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_STG_1_T:
                    break;
                case SVDF.AXES.MAP_STG_2_T:
                    break;
                case SVDF.AXES.MAP_VISION_Z:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_1:
                case SVDF.AXES.CHIP_PK_1_T_2:
                case SVDF.AXES.CHIP_PK_1_T_3:
                case SVDF.AXES.CHIP_PK_1_T_4:
                case SVDF.AXES.CHIP_PK_1_T_5:
                case SVDF.AXES.CHIP_PK_1_T_6:
                case SVDF.AXES.CHIP_PK_1_T_7:
                case SVDF.AXES.CHIP_PK_1_T_8:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;
                    }
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_1:
                case SVDF.AXES.CHIP_PK_1_Z_2:
                case SVDF.AXES.CHIP_PK_1_Z_3:
                case SVDF.AXES.CHIP_PK_1_Z_4:
                case SVDF.AXES.CHIP_PK_1_Z_5:
                case SVDF.AXES.CHIP_PK_1_Z_6:
                case SVDF.AXES.CHIP_PK_1_Z_7:
                case SVDF.AXES.CHIP_PK_1_Z_8:
                    {

                    }
                    break;
                case SVDF.AXES.CHIP_PK_2_T_1:
                case SVDF.AXES.CHIP_PK_2_T_2:
                case SVDF.AXES.CHIP_PK_2_T_3:
                case SVDF.AXES.CHIP_PK_2_T_4:
                case SVDF.AXES.CHIP_PK_2_T_5:
                case SVDF.AXES.CHIP_PK_2_T_6:
                case SVDF.AXES.CHIP_PK_2_T_7:
                case SVDF.AXES.CHIP_PK_2_T_8:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                            SVDF.AXES.CHIP_PK_2_Z_6);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;
                    }
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_1:
                case SVDF.AXES.CHIP_PK_2_Z_2:
                case SVDF.AXES.CHIP_PK_2_Z_3:
                case SVDF.AXES.CHIP_PK_2_Z_4:
                case SVDF.AXES.CHIP_PK_2_Z_5:
                case SVDF.AXES.CHIP_PK_2_Z_6:
                case SVDF.AXES.CHIP_PK_2_Z_7:
                case SVDF.AXES.CHIP_PK_2_Z_8:
                    {

                    }
                    break;
                case SVDF.AXES.MAGAZINE_ELV_Z:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                    {
                        // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                        if (GbVar.GB_INPUT[(int)IODF.INPUT.LD_SAFETY_SENSOR_LT_SIDE] == 0)
                        {
                            m_nLdElvZ_InterlockCount++;
                            if (m_nLdElvZ_InterlockCount > 10)
                            {
                                if (MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].IsBusy())
                                {
                                    m_nLdElvZ_InterlockCount = 0;
                                    MotionMgr.Inst[(int)SVDF.AXES.MAGAZINE_ELV_Z].MoveStop();
                                    return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_MOVE_NOT_SAFETY_Z_POS;
                                }
                            }
                        }
                        else
                        {
                            m_nLdElvZ_InterlockCount = 0;
                        }
                    }

                    // NONE 전진상태이면 Y축 이동불가
                    if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_FORWARD_SENSOR] == 1
                        || GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_BACKWARD_SENSOR] != 1)
                    {
                        return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_PUSHER_FWD;
                    }
                    break;
                case SVDF.AXES.LD_RAIL_T:
                    break;
                case SVDF.AXES.LD_RAIL_Y_FRONT:
                    break;
                case SVDF.AXES.LD_RAIL_Y_REAR:
                    break;
                case SVDF.AXES.BARCODE_Y:
                    break;
                case SVDF.AXES.LD_VISION_X:
                    break;
                case SVDF.AXES.STRIP_PK_X:
                    {
                        //Z축 홈상태 체크
                        nFuncResult = CheckServoOnHome(SVDF.AXES.STRIP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        double dPosStripPkX = 0.0;
                        double dPosUnitPkX = 0.0;

                        dPosStripPkX = dPos;
                        dPosUnitPkX = MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].GetTargetMoveAbsPos();

                        double dOffsetX = dPosUnitPkX + dPosStripPkX;

                        if (dOffsetX > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP].dValue)
                        {
                            MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].MoveStop();
                            MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].MoveStop();

                            return (int)ERDF.E_STRIP_PK_n_UNIT_PK_INTERLOCK;
                        }
                        
                        if (!IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_GRIP) &&
                            !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_UNGRIP) &&
                            !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_GRIP_SAFETY) &&
                            !IsInPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_STRIP_UNGRIP_SAFETY))
                        {
                            if (GbVar.IO[IODF.INPUT.LD_X_GRIP_UP] == 0 ||
                                GbVar.IO[IODF.INPUT.LD_X_GRIP_DOWN] == 1)
                            {
                                return (int)ERDF.E_INTL_STRIP_PK_X_CAN_NOT_MOVE_GRIPPER_DOWN;
                            }
                        }
                    }
                    break;
                case SVDF.AXES.STRIP_PK_Z:
                    {
                    }
                    break;
                case SVDF.AXES.UNIT_PK_X:
                    {
                        // 간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.UNIT_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;
                        //유닛피커가 스크랩 위치보다 더 큰값으로 위치하면 충돌 위치
                        if (IsMoreThanPosNo(SVDF.AXES.UNIT_PK_X, POSDF.UNIT_PICKER_SCRAP_1))
                        {
                            // STRIP_PK_X 위치가 READY POS 보다 크다
                            if (IsMoreThanPosNo(SVDF.AXES.STRIP_PK_X, POSDF.STRIP_PICKER_READY))
                            {
                                // 20211111 choh :스트립 피커 X 인터락 알람
                                return (int)ERDF.E_STRIP_PK_n_UNIT_PK_INTERLOCK;
                            }
                        }


                        double dPosStripPkX = 0.0;
                        double dPosUnitPkX = 0.0;

                        dPosStripPkX = MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].GetTargetMoveAbsPos();
                        dPosUnitPkX = dPos;

                        double dOffsetX = dPosUnitPkX + dPosStripPkX;

                        if (dOffsetX > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP].dValue)
                        {
                            MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].MoveStop();
                            MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].MoveStop();

                            return (int)ERDF.E_STRIP_PK_n_UNIT_PK_INTERLOCK;
                        }

                    }
                    break;
                case SVDF.AXES.UNIT_PK_Z:
                    break;            
                case SVDF.AXES.NONE:
                //case SVDF.AXES.SPARE_1:
                //case SVDF.AXES.SPARE_2:
                //case SVDF.AXES.SPARE_3:
                case SVDF.AXES.MAX:
                default:
                    break;
            }
            return FNC.SUCCESS;
        }

        /// <summary>
        /// 해당축의 기본 안전정보 (알람, 원점완료상태, 서보온 상태, 리미트센서 감지상태) 구동중 계속감시
        /// </summary>
        /// <param name="nAxis"></param>
        /// <param name="dPos"></param>
        /// <returns></returns>
        public int GetAxisSafetyMoving(int nAxis)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            if (nAxis < 0) return FNC.SUCCESS;
         
            if (MotionMgr.Inst[nAxis].IsAlarm())
            {
                GbVar.mcState.isHomeComplete[nAxis] = false;
                return (int)ERDF.E_SV_AMP + nAxis;
            }

            if (MotionMgr.Inst[nAxis].IsMinusLimit())
            {
                return (int)ERDF.E_SV_HW_LIMIT_N + nAxis;
            }

            if (MotionMgr.Inst[nAxis].IsPlusLimit())
            {
                return (int)ERDF.E_SV_HW_LIMIT_P + nAxis;
            }

            if (!GbVar.mcState.isHomeComplete[nAxis] || !MotionMgr.Inst[nAxis].GetServoOnOff())
            {
                return (int)ERDF.E_SV_NOT_HOME + nAxis;
            }

            //UNIT PK vs MAP PK SAFETY SENSOR CHECK
            if (nAxis == (int)SVDF.AXES.UNIT_PK_X || nAxis == (int)SVDF.AXES.MAP_PK_X)
            {
                
            }

            if (nAxis == (int)SVDF.AXES.STRIP_PK_X || nAxis == (int)SVDF.AXES.UNIT_PK_X)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.STRIP_PK_N_UNIT_PK_COLLISION_CHECK] == 0)
                {
                    MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].MoveStop();
                    MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].MoveStop();

                    return (int)ERDF.E_STRIP_PK_n_UNIT_PK_COLLISION;
                }

                double dPosStripPkX = 0.0;
                double dPosUnitPkX = 0.0;

                dPosStripPkX = MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].GetTargetMoveAbsPos();
                dPosUnitPkX = MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].GetTargetMoveAbsPos();

                double dOffsetX = dPosUnitPkX + dPosStripPkX;

                if(dOffsetX > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.STRIP_PK_n_UNIT_PK_INTERLOCK_GAP].dValue)
                {
                    MotionMgr.Inst[SVDF.AXES.STRIP_PK_X].MoveStop();
                    MotionMgr.Inst[SVDF.AXES.UNIT_PK_X].MoveStop();

                    return (int)ERDF.E_STRIP_PK_n_UNIT_PK_INTERLOCK;
                }
            }

            if (nAxis == (int)SVDF.AXES.GD_TRAY_STG_1_Y || nAxis == (int)SVDF.AXES.GD_TRAY_STG_2_Y)
            {
                // GD_TRAY_1_STG TABLE LEVEL이 같으면
                if (!GbFunc.IsGdTrayTableYMoveSafe())
                {
                    //if (Math.Abs(MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].GetRealPos() - MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].GetRealPos())
                    //    < ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GOOD_TRAY_STAGE_SIZE].dValue)
                    //{
                    //    //테이블간 충돌 위험
                    //    MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_1_Y].MoveEStop();
                    //    MotionMgr.Inst[(int)SVDF.AXES.GD_TRAY_STG_2_Y].MoveEStop();
                    //    return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                    //}

                }
            }
            return FNC.SUCCESS;
        }


        /// <summary>
        /// 원점검색 전 인터록 확인
        /// </summary>
        /// <param name="nAxis"></param>
        /// <returns></returns>
        public int GetAxisSafetyBeforeHome(int nAxis)
        {
#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            int nFuncResult = 0;
            SVDF.AXES axes = (SVDF.AXES)nAxis;

            switch (axes)
            {
                case SVDF.AXES.NONE:
                    break;
                case SVDF.AXES.LD_RAIL_T:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.STRIP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.STRIP_PK_Z))
                        {
                            return (int)ERDF.E_INTL_LD_RAIL_T_CAN_NOT_HOME_STRIP_PK_Z_NOT_READY_POS;
                        }
                    }
                    break;
                case SVDF.AXES.DRY_BLOCK_STG_X:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.MAP_PK_Z))
                        {
                            return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                        }
                    }
                    break;
                case SVDF.AXES.MAP_PK_X:
                    {
                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.MAP_PK_Z))
                        {
                            return (int)ERDF.E_INTL_DRY_BLOCK_STG_X_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                        }

                        ////유닛 피커 X 가 유닛 클리닝 위치 이하로 가깝다
                        //{
                        //    if (MotionMgr.Inst[(int)SVDF.AXES.UNIT_PK_X].GetHomeResult() == HomeResult.HR_Success &&
                        //        MotionMgr.Inst[(int)SVDF.AXES.UNIT_PK_X].GetRealPos() <= TeachMgr.Inst.Tch.dMotPos[(int)SVDF.AXES.UNIT_PK_X].dPos[POSDF.UNIT_PICKER_STRIP_CLEAN] - 1.0)
                        //    {
                        //        return (int)ERDF.E_INTL_UNIT_PK_X_VS_MAP_PK_X_SAFETY_SENSOR_DETECTED;
                        //    }
                        //}

                        ////클리너 Z가 너무 높아서 피커와 충돌 위험이 있다
                        //{
                        //    if (MotionMgr.Inst[(int)SVDF.AXES.NONE].GetRealPos() > 50)
                        //    {
                        //        return (int)ERDF.E_INTL_UNIT_PK_X_CAN_NOT_HOME_CELAN_Z_POS_TOO_HIGH;
                        //    }
                        //}
                    }
                    break;
                case SVDF.AXES.MAP_PK_Z:
                    break;
                case SVDF.AXES.MAP_STG_1_Y:
                    {
                        // TODO : 간섭 축 확인에 CHIP_PK_1_Z_1 ~ 10 추가해야함(?)
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z,
                            SVDF.AXES.CHIP_PK_1_Z_1,SVDF.AXES.CHIP_PK_1_Z_2,SVDF.AXES.CHIP_PK_1_Z_3,SVDF.AXES.CHIP_PK_1_Z_4,SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8,
                            SVDF.AXES.CHIP_PK_2_Z_1,SVDF.AXES.CHIP_PK_2_Z_2,SVDF.AXES.CHIP_PK_2_Z_3,SVDF.AXES.CHIP_PK_2_Z_4,SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8
                            );
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.MAP_PK_Z))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                        }

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.CHIP_PK_1_Z_1))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                        }

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.CHIP_PK_2_Z_1))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                        }

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                                SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8};
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_MAP_STG_1_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                                SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8};
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.MAP_STG_2_Y:
                    {
                        // TODO : 간섭 축 확인에 CHIP_PK_1_Z_1 ~ 10 추가해야함(?)
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.MAP_PK_Z,
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8,
                            SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.MAP_PK_Z))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_MAP_PK_Z_NOT_READY_POS;
                        }

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.CHIP_PK_1_Z_1))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_1_X_NOT_READY_POS;
                        }

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.CHIP_PK_2_Z_1))
                        {
                            return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_2_X_NOT_READY_POS;
                        }

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5, SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8};
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                            SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5, SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8};
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_MAP_STG_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }
                    }
                    break;
                case SVDF.AXES.CHIP_PK_1_X:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6, SVDF.AXES.CHIP_PK_1_Z_7, SVDF.AXES.CHIP_PK_1_Z_8
                            );
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;
                    }
                    break;
                case SVDF.AXES.CHIP_PK_2_X:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                            SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;
                    }
                    break;
                case SVDF.AXES.BALL_VISION_Y:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.BALL_VISION_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.BALL_VISION_Z))
                        {
                            return (int)ERDF.E_INTL_BALL_VISION_Y_CAN_NOT_HOME_BALL_VISION_Z_NOT_READY_POS;
                        }
                    }
                    break;
                case SVDF.AXES.BALL_VISION_Z:
                    break;
                case SVDF.AXES.TRAY_PK_X:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.TRAY_PK_Z))
                        {
                            return (int)ERDF.E_INTL_TRAY_PK_X_CAN_NOT_HOME_TRAY_PK_Z_DOWN_POS;
                        }
                    }
                    break;
                case SVDF.AXES.TRAY_PK_Z:
                    break;
                case SVDF.AXES.TRAY_PK_Y:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(SVDF.AXES.TRAY_PK_Z);
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        //Z축이 dSafetyPos 보다 낮은 위치이다.
                        if (!IsUpReadyPos(SVDF.AXES.TRAY_PK_Z))
                        {
                            return (int)ERDF.E_INTL_TRAY_PK_Y_CAN_NOT_HOME_TRAY_PK_Z_DOWN_POS;
                        }
                    }
                    break;
                case SVDF.AXES.GD_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.GD_TRAY_2_ELV_Z:
                    break;
                case SVDF.AXES.RW_TRAY_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_1_ELV_Z:
                    break;
                case SVDF.AXES.EMTY_TRAY_2_ELV_Z:
                    break;
                //case SVDF.AXES.COVER_TRAY_ELV_Z:
                //    break;
                case SVDF.AXES.GD_TRAY_STG_1_Y:
                    {
                        // GOOD TRAY STAGE 1번과 2번이 모두 UP 또는 DOWN 이다.
                        if (!GbFunc.IsGdTrayTableYMoveSafe())
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STG_1_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        }

                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            //SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                            //SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                            );
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            };
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_GD_TRAY_STAGE_1_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }
                        //{
                        //    SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                        //    SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                        //    SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                        //    };
                        //    for (int i = 0; i < AxisArr.Length; i++)
                        //    {
                        //        if (!IsUpReadyPos(AxisArr[i]))
                        //        {
                        //            return (int)ERDF.E_INTL_GD_TRAY_STAGE_1_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case SVDF.AXES.GD_TRAY_STG_2_Y:
                    {
                        // GOOD TRAY STAGE 1번과 2번이 모두 UP 또는 DOWN 이다.
                        if (!GbFunc.IsGdTrayTableYMoveSafe())
                        {
                            return (int)ERDF.E_INTL_GD_TRAY_STG_2_Y_CAN_NOT_MOVE_STAGE_UP_DOWN_DANGER;
                        }

                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            //SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                            //SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                            );
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            };
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_GD_TRAY_STAGE_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }
                        //{
                        //    SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                        //    SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                        //    SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                        //    };
                        //    for (int i = 0; i < AxisArr.Length; i++)
                        //    {
                        //        if (!IsUpReadyPos(AxisArr[i]))
                        //        {
                        //            return (int)ERDF.E_INTL_GD_TRAY_STAGE_2_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case SVDF.AXES.RW_TRAY_STG_Y:
                    {
                        //간섭 축 홈 상태 확인
                        nFuncResult = CheckServoOnHome(
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            //SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                            //SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                            );
                        if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        {
                            SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                            SVDF.AXES.CHIP_PK_1_Z_1, SVDF.AXES.CHIP_PK_1_Z_2, SVDF.AXES.CHIP_PK_1_Z_3, SVDF.AXES.CHIP_PK_1_Z_4, SVDF.AXES.CHIP_PK_1_Z_5,
                            SVDF.AXES.CHIP_PK_1_Z_6
                            };
                            for (int i = 0; i < AxisArr.Length; i++)
                            {
                                if (!IsUpReadyPos(AxisArr[i]))
                                {
                                    return (int)ERDF.E_INTL_RW_TRAY_STAGE_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                                }
                            }
                        }
                        //{
                        //    SVDF.AXES[] AxisArr = new SVDF.AXES[] {
                        //    SVDF.AXES.CHIP_PK_2_Z_1, SVDF.AXES.CHIP_PK_2_Z_2, SVDF.AXES.CHIP_PK_2_Z_3, SVDF.AXES.CHIP_PK_2_Z_4, SVDF.AXES.CHIP_PK_2_Z_5,
                        //    SVDF.AXES.CHIP_PK_2_Z_6, SVDF.AXES.CHIP_PK_2_Z_7, SVDF.AXES.CHIP_PK_2_Z_8, SVDF.AXES.CHIP_PK_2_Z_9, SVDF.AXES.CHIP_PK_2_Z_10
                        //    };
                        //    for (int i = 0; i < AxisArr.Length; i++)
                        //    {
                        //        if (!IsUpReadyPos(AxisArr[i]))
                        //        {
                        //            return (int)ERDF.E_INTL_RW_TRAY_STAGE_Y_CAN_NOT_HOME_CHIP_PK_PAD_NOT_SAFETY_POS;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case SVDF.AXES.MAP_VISION_Z:
                    {
                    }
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_1:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_1:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_2:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_2:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_3:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_3:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_4:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_4:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_5:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_5:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_6:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_6:
                    break;
                case SVDF.AXES.CHIP_PK_1_Z_7:
                    break;
                case SVDF.AXES.CHIP_PK_1_T_8:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_1:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_1:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_2:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_2:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_3:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_3:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_4:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_4:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_5:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_5:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_6:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_6:
                    break;
                case SVDF.AXES.CHIP_PK_2_Z_7:
                    break;
                case SVDF.AXES.CHIP_PK_2_T_8:
                    break;
                case SVDF.AXES.MAGAZINE_ELV_Z:
                    {
                        //간섭 축 홈 상태 확인
                        //nFuncResult = CheckServoOnHome(SVDF.AXES.NONE);
                        //if (FNC.IsErr(nFuncResult)) return nFuncResult;

                        ////Z축이 dSafetyPos 보다 낮은 위치이다.
                        //if (!IsUpReadyPos(SVDF.AXES.NONE))
                        //{
                        //    return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_PUSHER_FWD;
                        //}

                        //// MGZ LD ELV 에 MGZ 감지되면 Z축 홈잡다가 충돌남 (기구적으로 수정해야 함)
                        //// IODF.INPUT.MGZ_LD_Z_MGZ_TOP_CHECK : B접점, NC
                        //if (GbVar.GB_INPUT[(int)IODF.INPUT.MGZ_LD_Z_MGZ_TOP_CHECK] != 1 || GbVar.GB_INPUT[(int)IODF.INPUT.MGZ_LD_Z_MGZ_BTM_CHECK] != 1)
                        //{
                        //    return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_MGZ_EXIST;
                        //}

                        // Z축 원점 시 Y축 위치를 확인해야 함
                        // Y축이 컨베이어쪽에 있을 떄 원점 잡으면 충돌 발생 함
                        // 추가 해야 함

                        if (GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3117] == 1
                            || GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3118] != 1)
                        {
                            return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_PUSHER_FWD;
                        }
                    }
                    break;
                case SVDF.AXES.LD_VISION_X:
                    {

                    }
                    break;
                case SVDF.AXES.STRIP_PK_X:
                    {

                    }
                    break;
                case SVDF.AXES.STRIP_PK_Z:
                    {

                    }
                    break;
                case SVDF.AXES.UNIT_PK_X:
                    {
                    }
                    break;
                case SVDF.AXES.UNIT_PK_Z:
                    {

                    }
                    break;
                case SVDF.AXES.MAX:
                    break;
                default:
                    break;
            }
            return FNC.SUCCESS;
        }

        /// <summary>
        /// 원점검색 중 모터상태 체크
        /// </summary>
        /// <param name="nAxis"></param>
        /// <returns></returns>
        public int GetAxisSafetyHomming(int nAxis)
        {
            return FNC.SUCCESS;
        }

        public bool IsWithInRangePosZ(double RecipedPos, double dPos)
        {
            if (RecipedPos >= dPos) return false;
            return true;
        }
        public bool IsWithInRangePoschk(double dPos1, double dPos2, double dRange)
        {
            double dP1 = Math.Abs(dPos1);
            double dP2 = Math.Abs(dPos2);
            if (Math.Abs(dP1 - dP2) < dRange) return false;
            return true;
        }
        public bool IsWithInRangePos(double dPos1, double dPos2, double dRange)
        {
            double dP1 = Math.Abs(dPos1);
            double dP2 = Math.Abs(dPos2);
            if (Math.Abs(dP1 - dP2) > dRange) return false;
            return true;
        }

        public bool IsPlus(double dSetPos, double dEncPos)
        {
            if (dSetPos + 0.1 < dEncPos)
                return true;
            return false;
        }
        public bool IsMinus(double dSetPos, double dEncPos)
        {
            if (dSetPos - 0.1 > dEncPos)
                return true;
            return false;
        }

        public int CheckPosErr()
        {
            return FNC.SUCCESS;
        }
        /// <summary>
        /// 반대 IO 확인
        /// </summary>
        /// <param name="Output">이 IO의 반대 IO를 찾습니다.</param>
        /// <param name="bIsOn"></param>
        /// <returns></returns>
        public int CheckOppositeIO(IODF.OUTPUT Output, bool bIsOn)
        {
            switch (Output)
            {
                
                default:
                    break;
            }

            return FNC.SUCCESS;
        }

        public int GetDioBeforeOnOff(IODF.OUTPUT output, bool bOn)
        {
            return FNC.SUCCESS;
        }

        public int DioOffReverse(IODF.OUTPUT output, bool bOn)
        {
            if (bOn == false) return FNC.SUCCESS;

            return FNC.SUCCESS;
        }

        public bool IsInPosNo(SVDF.AXES axis, int nPosNo)
        {
        //    if (axis == SVDF.AXES.DRY_BLOCK_STG_X && nPosNo == POSDF.DRY_BLOCK_STAGE_STRIP_UNLOADING)
        //    {
        //        System.Diagnostics.Debug.Write("Busy : ");
        //        System.Diagnostics.Debug.WriteLine(!MotionMgr.Inst.MOTION[(int)axis].IsBusy());
        //        System.Diagnostics.Debug.Write("POS : ");
        //     System.Diagnostics.Debug.WriteLine(MotionMgr.Inst.MOTION[(int)axis].IsInPosition(TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo]));

        //    }
            return (!MotionMgr.Inst.MOTION[(int)axis].IsBusy()
                && MotionMgr.Inst.MOTION[(int)axis].IsInPosition(TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo]));
        }

        public bool IsInPosNo(int nAxis, int nPosNo)
        {
            return MotionMgr.Inst.MOTION[nAxis].IsInPosition(TeachMgr.Inst.Tch.dMotPos[nAxis].dPos[nPosNo]);
        }
        public bool IsInPosXY(SVDF.AXES axis, int nPosNo, double dCapCheck = 0.1)
        {
            double dRecipePos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPosXY = MotionMgr.Inst[axis].GetRealPos();

            if (Math.Abs(dRecipePos - dCurrentPosXY) < dCapCheck) return true;

            return false;
        }

        public bool IsInPos(SVDF.AXES axis, int nPosNo, double dGapCheck = 0.1)
        {
            double dRecipePos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPosXY = MotionMgr.Inst[axis].GetRealPos();

            if (Math.Abs(dRecipePos - dCurrentPosXY) < dGapCheck) return true;

            return false;
        }

        public bool IsUpReadyPos(SVDF.AXES axis, double dGapCheck = 0.1)
        {
            double dReadyPos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[0];
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos < (dReadyPos + dGapCheck))
                return true;

            System.Diagnostics.Debug.WriteLine(string.Format("dReadyPos : {0} dCurrentPos : {1}", dReadyPos, dCurrentPos));

            return false;
        }

        /// <summary>
        /// 축의 현재 값이 nPosNo의 포지션 값보다 크면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="nPosNo"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsMoreThanPosNoZ(SVDF.AXES axis, int nPosNo, double dGapCheck = 0.05)
        {
            double dRefPos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos > dRefPos + dGapCheck) return true;
            else return false;
        }

        /// <summary>
        /// 축의 현재 값이 dPos 값보다 크면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="dPos"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsMoreThanPosZ(SVDF.AXES axis, double dPos, double dGapCheck = 0.05)
        {
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos > dPos + dGapCheck) return true;
            else return false;
        }

        /// <summary>
        /// 축의 현재 값이 nPosNo의 포지션 값보다 작으면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="nPosNo"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsLessThanPosNoZ(SVDF.AXES axis, int nPosNo, double dGapCheck = 0.05)
        {
            double dRefPos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos < dRefPos - dGapCheck) return true;
            else return false;
        }

        /// <summary>
        /// 축의 현재 값이 dPos 값보다 작으면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="dPos"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsLessThanPosZ(SVDF.AXES axis, double dPos, double dGapCheck = 0.05)
        {
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos < dPos - dGapCheck) return true;
            else return false;
        }


        /// <summary>
        /// 축의 현재 값이 nPosNo의 포지션 값보다 크면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="nPosNo"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsMoreThanPosNo(SVDF.AXES axis, int nPosNo, double dGapCheck = 0.05)
        {
            double dRefPos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos > dRefPos + dGapCheck) return true;
            else return false;
        }


        /// <summary>
        /// 축의 현재 값이 nPosNo의 포지션 값보다 작으면 true
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="nPosNo"></param>
        /// <param name="dGapCheck"></param>
        /// <returns></returns>
        public bool IsLessThanPosNo(SVDF.AXES axis, int nPosNo, double dGapCheck = 0.05)
        {
            double dRefPos = TeachMgr.Inst.Tch.dMotPos[(int)axis].dPos[nPosNo];
            double dCurrentPos = MotionMgr.Inst[axis].GetRealPos();

            if (dCurrentPos < dRefPos - dGapCheck) return true;
            else return false;
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

        public bool CheckSamePos(SVDF.AXES axis1, SVDF.AXES axis2, double dGap = 10)
        {
            double dDiff = 0.0;

            double dAxis1Pos = MotionMgr.Inst[(int)axis1].GetRealPos();
            double dAxis2Pos = MotionMgr.Inst[(int)axis2].GetRealPos();

            dDiff = Math.Abs(dAxis1Pos - dAxis2Pos);

            if (dDiff < dGap)
            {
                return true;
            }

            return false;
        }
    }
}
