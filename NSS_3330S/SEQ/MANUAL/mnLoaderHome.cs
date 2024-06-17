using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnLoaderHome : SeqBase
    {
        bool bCmdHomeFlag = false;
        int nHomeErrAxisOffset = 0;

        #region SEQUENCE ENUM
        enum SEQ_NO_INIT
        {
            HOME_INIT,
            CONVEYOR_STOP,
            //INTERLOCK_CHECK,
            CYLINDER_INIT,
            CYLINDER_INIT_CHECK,

            //MZ_ELV_Y_HOME, 
            MZ_ELV_Z_HOME,

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
                bCmdHomeFlag = false;
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
                    GbVar.g_timeStampInit[MCDF.LOADER].Restart();
                    GbVar.mcState.isInitialized[MCDF.LOADER] = false;
                    GbVar.mcState.isInitializing[MCDF.LOADER] = true;
                    bCmdHomeFlag = false;
                    break;

                case SEQ_NO_INIT.CONVEYOR_STOP:
                    nFuncResult = LoaderConveyorAllStop();
                    break;
                //case SEQ_NO_INIT.INTERLOCK_CHECK:
                //    nFuncResult = InterlockCheck_Ex();
                //    break;
                case SEQ_NO_INIT.CYLINDER_INIT:
                    nFuncResult = CmdCylinderInit();
                    break;
                case SEQ_NO_INIT.CYLINDER_INIT_CHECK:
                    nFuncResult = CmdCylinderInitCheck();
                    break;
                //case SEQ_NO_INIT.MZ_ELV_Y_HOME:
                //    nFuncResult = LdMgzElvYAxisHome();
                //    break;
                case SEQ_NO_INIT.MZ_ELV_Z_HOME:
                    nFuncResult = LdMgzElvZAxisHome();
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALREADY_ALL:
                    break;
                case SEQ_NO_INIT.HOME_CHECK_ALL:
                    break;
                case SEQ_NO_INIT.HOME_ALL_COMPLETE:
                    GbVar.mcState.isInitialized[MCDF.LOADER] = true;
                    GbVar.mcState.isInitializing[MCDF.LOADER] = false;

                    GbVar.g_timeStampInit[MCDF.LOADER].Stop();
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

        public int LoaderConveyorAllStop()
        {
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SPARE_3101, false);
            MotionMgr.Inst.SetOutput(IODF.OUTPUT.SPARE_3100, false);

            return FNC.SUCCESS;
        }

        /// <summary>
        /// 기존 코드
        /// 매거진 엘베 원점 복귀 전 매거진 클램프 상태 확인
        /// </summary>
        /// <returns></returns>
        public int InterlockCheck()
        {
            //클램프가 되어있으면 알람
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3114] == 1 ||
               GbVar.IO[IODF.OUTPUT.ME_MZ_UNCLAMP_SOL] == 1)
            {
                return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_IS_NOT_UNCLAMP;
            }

            return FNC.SUCCESS;
        }

        /// <summary>
        /// 신규 코드 
        /// 매거진 엘베 원점 복귀 전 매거진 클램프 상태 및 Y축 전진 위치 확인
        /// </summary>
        /// <returns></returns>
        //public int InterlockCheck_Ex()
        //{
        //    if (GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3112] == 1
        //         || GbVar.GB_INPUT[(int)IODF.INPUT.SPARE_3113] == 1)
        //    {
        //        if (RecipeMgr.Inst.Rcp.MapTbInfo.isStripMat == false)
        //        {
        //            // 매거진 센서 둘 중 하나라도 들어오면 매거진 존재한다고 판단하고
        //            // 매거진이 존재하는 경우 Y축이 공급 위치보다 전진해 있다면 에러 처리 함
        //            // !!! 혹시 매거진이 클램프 상태이지만 매거진 유/무 감지 센서가 들어오지 않는다면 예외 상황 발생 할 수 있음 !!!
        //            if (SafetyMgr.Inst.IsMoreThanPosNo(SVDF.AXES.SPARE_63, POSDF.MGZ_LD_ELEV_STRIP_SUPPLY, 10.0))
        //            {
        //                return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_IS_NOT_UNCLAMP;
        //            }
        //        }
        //        else
        //        {
        //            // 매거진 센서 둘 중 하나라도 들어오면 매거진 존재한다고 판단하고
        //            // 매거진이 존재하는 경우 Y축이 공급 위치보다 전진해 있다면 에러 처리 함
        //            // !!! 혹시 매거진이 클램프 상태이지만 매거진 유/무 감지 센서가 들어오지 않는다면 예외 상황 발생 할 수 있음 !!!
        //            if (SafetyMgr.Inst.IsMoreThanPosNo(SVDF.AXES.SPARE_63, POSDF.MGZ_LD_ELEV_STRIP_SUPPLY_STRIP_MODE, 10.0))
        //            {
        //                return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_HOME_IS_NOT_UNCLAMP;
        //            }
        //        }
        //    }

        //    return FNC.SUCCESS;
        //}
        public int CmdCylinderInit()
        {
            GbVar.bInputWaitMsg[0] = false;

            int[] nOnOutputArr = new int[] { (int)IODF.OUTPUT.LOADING_PUSHER_BACKWARD_SOL};

            int[] nOffOutputArr = new int[] {(int)IODF.OUTPUT.LOADING_PUSHER_FORWARD_SOL};

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
            int[] nOnInputArr = new int[] {(int)IODF.INPUT.PUSHER_BACKWARD_SENSOR};

            int[] nOffInputArr = new int[] {
                                             (int)IODF.INPUT.PUSHER_FORWARD_SENSOR
                                             };

            ERDF[] errOnInputArr = new ERDF[] { ERDF.E_MAP_PK_PUSHER_BWD_FAIL };
            ERDF[] errOffInputArr = new ERDF[] { ERDF.E_MAP_PK_PUSHER_FWD_FAIL };

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

        //public int LdMgzElvYAxisHome()
        //{
        //    if (!bCmdHomeFlag)
        //    {
        //        timeStampInit.Restart();
        //        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.SPARE_63)] = false;
        //        MotionMgr.Inst[SVDF.Get(SVDF.AXES.SPARE_63)].HomeStart();

        //        bCmdHomeFlag = true;
        //        return FNC.BUSY;
        //    }
        //    else
        //    {
        //        if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
        //        {
        //            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.SPARE_63)] = false;
        //            MotionMgr.Inst[SVDF.Get(SVDF.AXES.SPARE_63)].MoveStop();

        //            bCmdHomeFlag = false;
        //            timeStampInit.Stop();
        //            return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
        //        }
        //        else
        //        {
        //            if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.SPARE_63)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
        //            {
        //                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.SPARE_63)] = false;
        //                nHomeErrAxisOffset = (int)SVDF.AXES.SPARE_63;
        //                return FNC.BUSY;
        //            }
        //        }
        //    }


        //    bCmdHomeFlag = false;

        //    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.SPARE_63)] = true;


        //    return FNC.SUCCESS;
        //}

        public int LdMgzElvZAxisHome()
        {
            if (!bCmdHomeFlag)
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_FORWARD_SENSOR] == 1
                    || GbVar.GB_INPUT[(int)IODF.INPUT.PUSHER_BACKWARD_SENSOR] != 1)
                {
                    return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Y_CAN_NOT_MOVE_PUSHER_FWD;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT.MATERIAL_PROTRUSION_CHECK_SENSOR] == 0)
                {
                    return (int)ERDF.E_INTL_MGZ_CLAMP_ELV_Z_CAN_NOT_MOVE_MATERIAL_DETECTED;
                }

                timeStampInit.Restart();
                GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)] = false;
                MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)].HomeStart();

                bCmdHomeFlag = true;
                return FNC.BUSY;
            }
            else
            {
                if (timeStampInit.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.EQP_INIT_TIME].lValue)
                {
                    GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)] = false;
                    MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)].MoveStop();

                    bCmdHomeFlag = false;
                    timeStampInit.Stop();
                    return (int)ERDF.E_SV_NOT_HOME + nHomeErrAxisOffset;
                }
                else
                {
                    if (MotionMgr.Inst[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)].GetHomeResult() != DionesTool.Motion.HomeResult.HR_Success)
                    {
                        GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)] = false;
                        nHomeErrAxisOffset = (int)SVDF.AXES.MAGAZINE_ELV_Z;
                        return FNC.BUSY;
                    }
                }
            }


            bCmdHomeFlag = false;
            GbVar.mcState.isHomeComplete[SVDF.Get(SVDF.AXES.MAGAZINE_ELV_Z)] = true;

            return FNC.SUCCESS;
        }
    }
}
