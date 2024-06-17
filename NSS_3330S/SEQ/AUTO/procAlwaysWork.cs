using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSS_3330S.MOTION;

namespace NSS_3330S.SEQ.AUTO
{
    class procAlwaysWork : SeqBase
    {
        double[] m_dPosPrev = new double[SVDF.SV_CNT];
        double[] m_dPosCurrent = new double[SVDF.SV_CNT];

        Stopwatch m_swStopElapsed = new Stopwatch();

        bool[] isAlreadyOccured = new bool[4];

        const int SCRAP_BOX = 0;
        const int SCRAP_FULL = 1;
        const int REJECT_BOX = 2;
        const int REJECT_FULL = 3;

        Stopwatch[] m_swWarningTime = new Stopwatch[4];
        Stopwatch[] m_swBox = new Stopwatch[5];
        public procAlwaysWork(int nSeqID)
        {
            m_nSeqID = nSeqID;
            for (int nSwCnt = 0; nSwCnt < m_swWarningTime.Length; nSwCnt++)
            {
                m_swWarningTime[nSwCnt] = new Stopwatch();
            }
            for (int nSwCnt = 0; nSwCnt < m_swBox.Length; nSwCnt++)
            {
                m_swBox[nSwCnt] = new Stopwatch();
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
            //if (!IsAcceptRun()) return;
//#if _NOTEBOOK
//            return;
//#endif
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
                        nFuncResult = SafetyMgr.Inst.GetAlwaysCheckStatus();
                    }
                    break;
                case 1:
                    {
                        // 무언정지 체크
                        nFuncResult = CheckNoMsgStop();
                    }
                    break;
                case 2:
                    {
                        // 도어 알람 체크 (Start나 Initial 시)
                        if (FNC.IsErr(GbVar.g_nDoorAlarmResult))
                        {
                            nFuncResult = GbVar.g_nDoorAlarmResult;
                            GbVar.g_nDoorAlarmResult = FNC.SUCCESS;
                        }
                    }
                    break;
                case 3:
                    {
                        nFuncResult = CheckBoxAlarm();
                    }
                    break;
                default:
                    {
                        NextSeq(0);
                        return;
                    }
            }
            m_bFirstSeqStep = false;

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;
            else if (FNC.IsCycleCheck(nFuncResult))
            {
                LeaveCycle();

                Type type = m_seqInfo.GetType();
                GbVar.SeqMgr.UpdateSeqObj(type, m_seqInfo);

                return;
            }

            m_nSeqNo++;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
        }

        /// <summary>
        /// 무언정지 확인
        /// </summary>
        /// <returns></returns>
        int CheckNoMsgStop()
        {
            if (!GbVar.mcState.IsRunReq() || ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.NO_MSG_STOP_TIME].dValue < 0.1)
            {
                if (m_swStopElapsed.IsRunning)
                    m_swStopElapsed.Reset();

                return FNC.SUCCESS;
            }
            else
            {
                if (!m_swStopElapsed.IsRunning)
                    m_swStopElapsed.Restart();
            }

            // 현재 위치 저장
            for (int nAxisNo = 0; nAxisNo < SVDF.SV_CNT; nAxisNo++)
            {
                if (MotionMgr.Inst[nAxisNo] == null)
                    continue;

                m_dPosCurrent[nAxisNo] = MotionMgr.Inst[nAxisNo].GetRealPos();
            }

            // 자재가 없으면 리턴
            if (GbFunc.IsLotEndCondition(true))
            {
                m_swStopElapsed.Restart();
                return FNC.SUCCESS;
            }

            // PAUSE 켜져있으면 리턴
            if (GbVar.INSERT_STRIP_PAUSE ||
                GbVar.SAW_PLACE_PAUSE ||
                GbVar.SAW_PICK_UP_PAUSE ||
                GbVar.DRY_BLOCK_PLACE_PAUSE ||
                GbVar.MAP_PICKER_PICK_UP_PAUSE ||
                GbVar.MAP_PICKER_PLACE_PAUSE ||
                GbVar.TOP_INSP_PAUSE ||
                GbVar.BTM_INSP_PASE ||
                GbVar.CHIP_PICKER_PICK_UP_PAUSE ||
                GbVar.CHIP_PICKER_PLACE_PAUSE)
            {
                m_swStopElapsed.Restart();
                return FNC.SUCCESS;
            }

            // 원 스트립 적재가 켜져있으면 로더 제외하고 자재 유무 체크
            if (GbVar.Seq.bOneStripLoad)
            {
                // 자재가 없으면 리턴
                if (GbFunc.IsLotEndCondition(true, false, true))
                {
                    m_swStopElapsed.Restart();
                    return FNC.SUCCESS;
                }
            }

            // 이전 위치와 비교하여 타이머 리셋
            for (int nAxisNo = 0; nAxisNo < SVDF.SV_CNT; nAxisNo++)
            {
                if (MotionMgr.Inst[nAxisNo] == null)
                    continue;

                if (Math.Abs(m_dPosPrev[nAxisNo] - m_dPosCurrent[nAxisNo]) > 1.0)
                {
                    m_swStopElapsed.Restart();
                    break;
                }
            }

            // 이전 위치 저장
            for (int nAxisNo = 0; nAxisNo < SVDF.SV_CNT; nAxisNo++)
            {
                if (MotionMgr.Inst[nAxisNo] == null)
                    continue;

                m_dPosPrev[nAxisNo] = m_dPosCurrent[nAxisNo];
            }

            // 시간 체크하여 알람 발생
            if (m_swStopElapsed.IsRunning)
            {
                if (m_swStopElapsed.Elapsed.TotalMinutes > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.NO_MSG_STOP_TIME].dValue)
                {
                    return (int)ERDF.E_NO_MSG_STOP;
                }
            }

            return FNC.SUCCESS;
        }

        int CheckBoxAlarm()
        {
            if (!GbVar.mcState.IsAllRun()) return FNC.SUCCESS;

#if _NOTEBOOK
            return FNC.SUCCESS;
#endif
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SCRAP_BOX_CHECK] == 0)//0 = exsit, 1 = not exsit
            {
                if (isAlreadyOccured[SCRAP_BOX] == false)
                {
                    if (!m_swBox[SCRAP_BOX].IsRunning) m_swBox[SCRAP_BOX].Restart();
                    if(m_swBox[SCRAP_BOX].ElapsedMilliseconds > 5000)
                    {
                        isAlreadyOccured[SCRAP_BOX] = true;
                        m_swBox[SCRAP_BOX].Reset();
                        return (int)ERDF.E_SCRAP_BOX_IS_NOT_EXIST;
                    }
                }
                else
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue <= 0)
                    {
                        if (m_swWarningTime[SCRAP_BOX].IsRunning)
                            m_swWarningTime[SCRAP_BOX].Stop();
                    }
                    else
                    {
                        if (m_swWarningTime[SCRAP_BOX].IsRunning)
                        {
                            if (m_swWarningTime[SCRAP_BOX].ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue)
                            {
                                m_swWarningTime[SCRAP_BOX].Stop();
                                isAlreadyOccured[SCRAP_BOX] = false;
                            }
                        }
                        else
                        {
                            m_swWarningTime[SCRAP_BOX].Restart();
                        }
                    }         
                }
            }
            else
            {
                if (isAlreadyOccured[SCRAP_BOX] == true)
                    isAlreadyOccured[SCRAP_BOX] = false;

                if (m_swWarningTime[SCRAP_BOX].IsRunning)
                    m_swWarningTime[SCRAP_BOX].Stop();

                if (m_swBox[SCRAP_BOX].IsRunning)
                    m_swBox[SCRAP_BOX].Reset();
            }

            //221024 HEP ADD
            //스크랩 박스 딜레이 추가
            if (GbVar.GB_INPUT[(int)IODF.INPUT.SCRAP_BOX_FULL_CHECK] == 1) // jy.yang 설비 상황에 따라 
            {
                if (isAlreadyOccured[SCRAP_FULL] == false)
                {
                    if (!m_swBox[SCRAP_FULL].IsRunning) m_swBox[SCRAP_FULL].Restart();
                    if (m_swBox[SCRAP_FULL].ElapsedMilliseconds > 5000)
                    {
                        isAlreadyOccured[SCRAP_FULL] = true;
                        m_swBox[SCRAP_FULL].Reset();
                        return (int)ERDF.E_SCRAP_BOX_IS_FULL;
                    }
                }
                else
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue <= 0)
                    {
                        if (m_swWarningTime[SCRAP_FULL].IsRunning)
                            m_swWarningTime[SCRAP_FULL].Stop();
                    }
                    else
                    {
                        if (m_swWarningTime[SCRAP_FULL].IsRunning)
                        {
                            if (m_swWarningTime[SCRAP_FULL].ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue)
                            {
                                m_swWarningTime[SCRAP_FULL].Stop();
                                isAlreadyOccured[SCRAP_FULL] = false;
                            }
                        }
                        else
                        {
                            m_swWarningTime[SCRAP_FULL].Restart();
                        }
                    }
                }
            }
            else
            {
                if (isAlreadyOccured[SCRAP_FULL] == true)
                    isAlreadyOccured[SCRAP_FULL] = false;

                if (m_swWarningTime[SCRAP_FULL].IsRunning)
                    m_swWarningTime[SCRAP_FULL].Stop();
                if (m_swBox[SCRAP_FULL].IsRunning)
                    m_swBox[SCRAP_FULL].Reset();
            }


            if (GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_CHECK] == 0)
            {
                if (isAlreadyOccured[REJECT_BOX] == false)
                {
                    if (!m_swBox[REJECT_BOX].IsRunning) m_swBox[REJECT_BOX].Restart();
                    if (m_swBox[REJECT_BOX].ElapsedMilliseconds > 5000)
                    {
                        isAlreadyOccured[REJECT_BOX] = true;
                        m_swBox[REJECT_BOX].Reset();
                        return (int)ERDF.E_REJECT_BOX_IS_NOT_EXIST;
                    }
                }
                else
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue <= 0)
                    {
                        if (m_swWarningTime[REJECT_BOX].IsRunning)
                            m_swWarningTime[REJECT_BOX].Stop();
                    }
                    else
                    {
                        if (m_swWarningTime[REJECT_BOX].IsRunning)
                        {
                            if (m_swWarningTime[REJECT_BOX].ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue)
                            {
                                m_swWarningTime[REJECT_BOX].Stop();
                                isAlreadyOccured[REJECT_BOX] = false;
                            }
                        }//
                        else
                        {
                            m_swWarningTime[REJECT_BOX].Restart();
                        }
                    }
                }
            }
            else
            {
                if (isAlreadyOccured[REJECT_BOX] == true)
                    isAlreadyOccured[REJECT_BOX] = false;

                if (m_swWarningTime[REJECT_BOX].IsRunning)
                    m_swWarningTime[REJECT_BOX].Stop();
                if (m_swBox[REJECT_BOX].IsRunning)
                    m_swBox[REJECT_BOX].Reset();
            }

            if (GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_FULL1] == 0 || 
                GbVar.GB_INPUT[(int)IODF.INPUT.REJECT_BOX_FULL2] == 0) //0 == full, 1 == not full
            {
                if (isAlreadyOccured[REJECT_FULL] == false)
                {
                    if (!m_swBox[REJECT_FULL].IsRunning) m_swBox[REJECT_FULL].Restart();
                    if (m_swBox[REJECT_FULL].ElapsedMilliseconds > 5000)
                    {
                        isAlreadyOccured[REJECT_FULL] = true;
                        m_swBox[REJECT_FULL].Reset();
                        return (int)ERDF.E_REJECT_BOX_IS_FULL;
                    }
                }
                else
                {
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue <= 0)
                    {
                        if (m_swWarningTime[REJECT_FULL].IsRunning)
                            m_swWarningTime[REJECT_FULL].Stop();
                    }
                    else
                    {
                        if (m_swWarningTime[REJECT_FULL].IsRunning)
                        {
                            if (m_swWarningTime[REJECT_FULL].ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_BOX_ALARM_CLEAR_TIME_OUT].lValue)
                            {
                                m_swWarningTime[REJECT_FULL].Stop();
                                isAlreadyOccured[REJECT_FULL] = false;
                            }
                        }
                        else
                        {
                            m_swWarningTime[REJECT_FULL].Restart();
                        }
                    }
                }
            }
            else
            {
                if (isAlreadyOccured[REJECT_FULL] == true)
                    isAlreadyOccured[REJECT_FULL] = false;

                if (m_swWarningTime[REJECT_FULL].IsRunning)
                    m_swWarningTime[REJECT_FULL].Stop();

                if (m_swBox[REJECT_FULL].IsRunning)
                    m_swBox[REJECT_FULL].Reset();
            }

            return FNC.SUCCESS;
        }
    }
}
