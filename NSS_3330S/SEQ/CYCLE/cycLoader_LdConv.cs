using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.CYCLE
{
    public class cycLoader_LdConv : SeqBase
    {
        bool m_bAutoMode = true;
        bool m_bQurdSize = true;

        int[] m_nInitAxisNoArray = null;
        int[] m_nInitOrderArray = null;

        int m_nCurrentOrder = 0;

        bool m_bSafetyConvStart = false;
        bool m_bSafetyConvStop = false;

        #region PROPERTY
        public seqLoader_LdConv SEQ_INFO_CURRENT
        {
            get { return GbVar.Seq.sLoaderLdConv; }
        }

        #endregion

        /// <summary>
        /// OP 방향에서 봤을 때 LOAD 1번이 우측 , 2번이 좌측 컨베이어 ( 인덱스 기준으로 0,1 )
        /// </summary>
        /// <param name="nSeqID"></param>
        /// <param name="nConvNo"></param>
        public cycLoader_LdConv(int nSeqID)
        {
            SetCycleMode(true);

            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLoaderLdConv;

            m_nInitAxisNoArray = new int[4];
            m_nInitOrderArray = new int[4];

            m_bSafetyConvStart = false;
            m_bSafetyConvStop = false;

            //m_bQurdSize = 매거진 전역 변수 연결;
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


        // 컨베이어 동작    
        // 컨베이어 동작 후 센서 체크 되면 정지 
        // 작업자가 매거진을 전부 투입하고 마지막 투입 완료 시점을 알아야 함
        public int Mz_Input_Conveyor()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;


            #region SATETY 조건 시 컨베이어 정지 구간

            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
            {
                // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                if (m_bSafetyConvStart)
                {
                    m_bSafetyConvStart = false;
                    LdConv_Stop(); // 리턴은 안봐도 됨
                    m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                }

                // SAFETY 센서 감지 되면 리턴
                return FNC.BUSY;
            }
            else
            {
                // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                if (m_bSafetyConvStop)
                {
                    m_bSafetyConvStop = false;
                    NextSeq(8); // CASE번호 주의
                    return FNC.BUSY;
                }
            }

            #endregion


            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_bSafetyConvStart = false;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?
                        if (IsLdConv_Full())
                        {
                            NextSeq(18);
                        }
                        //if(!IsLdConv_Detect(m_ConvNo))
                        //{
                        //    return (int)ERDF.E_MGZ_LD_1_CONV_LD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}
                    }
                    break;

                case 8:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        m_bSafetyConvStart = true;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 9:
                    nFuncResult = LdStopper_Up(true);

                    break;

                case 10:
                    {
                        // 컨베이어 동작
                        nFuncResult = LdConv_Run(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) 
                                nFuncResult = FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(1))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    nFuncResult = LdConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                nFuncResult = FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 16:
                    {
                        // 로더는 센서 감지 후 추가 동작 시간을 따로 사용 함
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(1000)) 
                            nFuncResult =  FNC.BUSY;
                    }
                    break;

                case 18:
                    {
                        // 컨베이어 정지
                        nFuncResult = LdConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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

        public int Mz_Loading_Conveyor_1()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;


            #region SATETY 조건 시 컨베이어 정지 구간

            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
            {
                // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                if (m_bSafetyConvStart)
                {
                    m_bSafetyConvStart = false;
                    LdConv_Stop(); // 리턴은 안봐도 됨
                    m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                }

                // SAFETY 센서 감지 되면 리턴
                return FNC.BUSY;
            }
            else
            {
                // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                if (m_bSafetyConvStop)
                {
                    m_bSafetyConvStop = false;
                    NextSeq(8); // CASE번호 주의
                    return FNC.BUSY;
                }
            }

            #endregion


            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_bSafetyConvStart = false;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?
                        if (IsLdConv_Full())
                        {
                            NextSeq(18);
                        }
                        //if(!IsLdConv_Detect(m_ConvNo))
                        //{
                        //    return (int)ERDF.E_MGZ_LD_1_CONV_LD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}
                    }
                    break;

                case 8:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        m_bSafetyConvStart = true;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 9:
                    nFuncResult = LdStopper_Up(true);

                    break;

                case 10:
                    {
                        // 컨베이어 동작
                        nFuncResult = LdConv_Run(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) 
                                nFuncResult = FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(2))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    nFuncResult = LdConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                nFuncResult = FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 16:
                    {
                        // 로더는 센서 감지 후 추가 동작 시간을 따로 사용 함
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(1000)) 
                            nFuncResult = FNC.BUSY;
                    }
                    break;

                case 18:
                    {
                        // 컨베이어 정지
                        nFuncResult = LdConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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

        public int Mz_Loading_Conveyor_2()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;


            #region SATETY 조건 시 컨베이어 정지 구간

            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
            {
                // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                if (m_bSafetyConvStart)
                {
                    m_bSafetyConvStart = false;
                    LdConv_Stop(); // 리턴은 안봐도 됨
                    m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                }

                // SAFETY 센서 감지 되면 리턴
                return FNC.BUSY;
            }
            else
            {
                // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                if (m_bSafetyConvStop)
                {
                    m_bSafetyConvStop = false;
                    NextSeq(8); // CASE번호 주의
                    return FNC.BUSY;
                }
            }

            #endregion


            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_bSafetyConvStart = false;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?
                        if (IsLdConv_Full())
                        {
                            NextSeq(18);
                        }
                        //if(!IsLdConv_Detect(m_ConvNo))
                        //{
                        //    return (int)ERDF.E_MGZ_LD_1_CONV_LD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}
                    }
                    break;

                case 8:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        m_bSafetyConvStart = true;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 9:
                    nFuncResult = LdStopper_Up(true);

                    break;

                case 10:
                    {
                        // 컨베이어 동작
                        nFuncResult = LdConv_Run(true);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000)) 
                                nFuncResult = FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(3))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    nFuncResult = LdConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                nFuncResult = FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 16:
                    {
                        // 로더는 센서 감지 후 추가 동작 시간을 따로 사용 함
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(1000)) 
                            nFuncResult = FNC.BUSY;
                    }
                    break;

                case 18:
                    {
                        // 컨베이어 정지
                        nFuncResult = LdConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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

        // 컨베이어 동작 조건
        // MZ TRANSFER가 해당 컨베이어 위치가 아닌경우 가능
        // 투입 된 매거진이 FULL이 아니라면 가능
        public int Mz_Unloading_Conveyor()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;


            #region SATETY 조건 시 컨베이어 정지 구간

            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
            {
                // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                if (m_bSafetyConvStart)
                {
                    m_bSafetyConvStart = false;
                    LdConv_Stop(); // 리턴은 안봐도 됨
                    m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                }

                // SAFETY 센서 감지 되면 리턴
                return FNC.BUSY;
            }
            else
            {
                // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                if (m_bSafetyConvStop)
                {
                    m_bSafetyConvStop = false;
                    NextSeq(8); // CASE번호 주의
                    return FNC.BUSY;
                }
            }

            #endregion


            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_bSafetyConvStart = false;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_UNLOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?
                        if (IsLdConv_Full())
                        {
                            NextSeq(18);
                        }
                        //if(!IsLdConv_Detect(m_ConvNo))
                        //{
                        //    return (int)ERDF.E_MGZ_LD_1_CONV_LD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}
                    }
                    break;

                case 8:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        m_bSafetyConvStart = true;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 9:
                    nFuncResult = LdInputStopper_Up(true);

                    break;

                case 10:
                    {
                        // 컨베이어 동작
                        nFuncResult = LdConv_Run(false);
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN START", STEP_ELAPSED));
                    }
                    break;

                case 15:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                        {
                            if (WaitDelay(3000))
                                nFuncResult = FNC.BUSY;
                            break;
                        }
                        else
                        {
                            if (!IsLdConv_Arrival(0))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    // 컨베이어 정지
                                    nFuncResult = LdConv_Stop();
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }
                                nFuncResult = FNC.BUSY;
                            }
                        }
                    }
                    break;

                case 16:
                    {
                        // 로더는 센서 감지 후 추가 동작 시간을 따로 사용 함
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        //long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(1000))
                            nFuncResult = FNC.BUSY;
                    }
                    break;

                case 18:
                    {
                        // 컨베이어 정지
                        nFuncResult = LdConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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

        public int Mz_Reading_BCR()
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;
            string BCRresult = "";

            #region SATETY 조건 시 컨베이어 정지 구간

            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
            {
                // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                if (m_bSafetyConvStart)
                {
                    m_bSafetyConvStart = false;
                    LdConv_Stop(); // 리턴은 안봐도 됨
                    m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                }

                // SAFETY 센서 감지 되면 리턴
                return FNC.BUSY;
            }
            else
            {
                // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                if (m_bSafetyConvStop)
                {
                    m_bSafetyConvStop = false;
                    NextSeq(8); // CASE번호 주의
                    return FNC.BUSY;
                }
            }

            #endregion


            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_bSafetyConvStart = false;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;
                    }
                    break;

                case 8:
                    {
                        if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                        {
                            // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                            return FNC.CYCLE_CHECK;
                        }

                        m_bSafetyConvStart = true;
                        m_bSafetyConvStop = false;
                    }
                    break;

                case 9:
                    {
                        if (!IsLdConv_Arrival(3))
                        {
                            return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                        }
                    }
                    break;

                case 10:
                    {
                        if(!GbDev.MGZBarcodeReader.IsConnect)
                        {

                        }
                    }
                    break;

                case 11:
                    
                    if (GbDev.MGZBarcodeReader.Send_ReadStart() == "")
                    {
                        if (WaitDelay(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAGAZINE_BARCODE_READING_TIME].lValue))
                            return FNC.BUSY;

                        BCRresult = GbDev.MGZBarcodeReader.Send_ReadStart();
                    }
                    break;

                case 12:
                    if(BCRresult == "")
                    {
                        return (int)ERDF.E_MGZ_BCR_READING_TIME_OUT;
                    }

                    if(BCRresult == "FAIL")
                    {
                        return (int)ERDF.E_MGZ_BCR_READING_FAIL;
                    }

                    GbVar.Seq.sLoaderLdConv.MGZ_ID = BCRresult;
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_LOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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

        public int MgzLdConvLoading_Wait()
        {
            try
            {
                if (m_nSeqNo != m_nPreSeqNo)
                {
                    ResetCmd();

                    if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
                }

                m_nPreSeqNo = m_nSeqNo;
                nFuncResult = FNC.SUCCESS;

                #region SATETY 조건 시 컨베이어 정지 구간

                // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                {
                    // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                    // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                    if (m_bSafetyConvStart)
                    {
                        m_bSafetyConvStart = false;
                        LdConv_Stop(); // 리턴은 안봐도 됨
                        m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                    }

                    // SAFETY 센서 감지 되면 리턴
                    return FNC.BUSY;
                }
                else
                {
                    // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                    if (m_bSafetyConvStop)
                    {
                        m_bSafetyConvStop = false;
                        NextSeq(8); // CASE번호 주의
                        return FNC.BUSY;
                    }
                }

                #endregion

                switch (m_nSeqNo)
                {
                    case 0:
                        {
                            if (m_bFirstSeqStep)
                            {
                                m_bSafetyConvStart = false;
                                m_bSafetyConvStop = false;
                                m_bFirstSeqStep = false;
                            }
                        }
                        break;

                    case 4:
                        {
                            if (m_bAutoMode)
                            {
                                if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                                {
                                    return FNC.CYCLE_CHECK;
                                }
                            }
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_WAIT, true);
                        }
                        break;

                    case 6:
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        // 해당 컨베이어가 동작 중이라면 대기
                        // 추후 타임아웃 필요 할 수도..
                        // 컨베이어 매거진 투입 대기 
                        if (GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN]
                            || GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ])
                        {
                            m_bSafetyConvStart = true;
                            m_bSafetyConvStop = false;
                            // 컨베이어 동작 상태면 대기
                            return FNC.CYCLE_CHECK;
                        }

                        break;


                    case 12:
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                            // 한번 더 매거진 감지 확인
                            if (!IsLdConv_Arrival(4) || !IsLdConv_Arrival(5))
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT_USE].bOptionUse &&
                                    IsDelayOver(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_MAX_ROLLING_TIMEOUT].lValue))
                                {
                                    SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR LOADING ERROR STOP", STEP_ELAPSED));
                                    return (int)ERDF.E_MGZ_LD_CONV_1_LD_MGZ_ARRIVAL_FAIL;
                                }

                                nFuncResult = FNC.BUSY;
                            }

                        }
                        break;

                    case 20:
                        {
                            TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LOADING_WAIT, false);

                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            // 완료
                            return FNC.SUCCESS;
                        }

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
            catch (Exception)
            {
                return (int)ERDF.E_NONE;//에러 번호....할당 해주기
            }
        }

        /// <summary>
        /// 투입 된 매거진을 컨베이어 구동 후 매거진 감지 센서가 들어오면 정지
        /// </summary>
        /// <returns></returns>
        


        /// <summary>
        /// 매거진 컨베이어 배출 방향으로 구동 후 매거진 체크
        /// </summary>
        /// <param name="bCheckSensor"></param>
        /// <returns></returns>
        public int MzUnloadingMove(bool bSensorSkip = false)    
        {
            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();

                if (GbVar.mcState.isCyclePause[m_nSeqID]) return FNC.BUSY;
            }

            m_nPreSeqNo = m_nSeqNo;
            nFuncResult = FNC.SUCCESS;

            #region SATETY 조건 시 컨베이어 정지 구간

            if(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
            {
                // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                {
                    // SAFETY가 감지 됐을 때 컨베이어도 동작 중이라면 정지
                    // 만약 해당 FLAG가 켜지지 않으면 컨베이어는 동작중이 아님
                    if (m_bSafetyConvStart)
                    {
                        m_bSafetyConvStart = false;
                        UldConv_Stop(); // 리턴은 안봐도 됨
                        m_bSafetyConvStop = true; // 컨베이어 정지 FLAG ON
                    }

                    // SAFETY 센서 감지 되면 리턴
                    return FNC.BUSY;
                }
                else
                {
                    // 감지가 풀렸을 때 컨베이어가 정지 중이라면 다시 동작시키기 위함
                    if (m_bSafetyConvStop)
                    {
                        m_bSafetyConvStop = false;
                        NextSeq(8); // CASE번호 주의
                        return FNC.BUSY;
                    }
                }
            }


            #endregion

            switch (m_nSeqNo)
            {
                case 0:
                    {

                    }
                    break;

                case 4:
                    {
                        if (m_bAutoMode)
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                return FNC.CYCLE_CHECK;
                            }
                        }
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_UNLOADING_MOVE, true);

                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "CYCLE START", STEP_ELAPSED));
                    }
                    break;

                case 5:
                    {
                        // MZ TRANSFER 인터락 확인                    
                        // 해당 포트에 매거진이 FULL인지 어떻게 확인 ?

                        //Dry Run 모드 
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse ) break;

                        // 언로딩은 투입 역순이기 떄문에
                        // 먼저 ARRIVAL 센서를 체크 해야 함
                        //if (!IsLdConv_Arrival(m_ConvNo))
                        //{
                        //    // 매거진이 감지 되지 않으면 에러 처리

                        //    // 컨베이어 정지
                        //    nFuncResult = LdConv_Stop();
                        //    return (int)ERDF.E_MGZ_LD_CONV_1_ULD_MGZ_DETECT_FAIL + m_ConvNo;
                        //}          
                    }
                    break;

                case 8:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_SAFETY_SENSOR_USE].bOptionUse)
                        {
                            if (IsConvSafetyHandDetect(ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse))
                            {
                                // Safety 센서가 감지 된 경우 컨베이어 정지 하고 대기
                                return FNC.CYCLE_CHECK;
                            }
                            m_bSafetyConvStart = true;
                            m_bSafetyConvStop = false;
                        }
                    }
                    break;

                case 9:
                    nFuncResult = UldStopper_Up(true);
                    break;

                case 10:
                    {
                        // 일단 초기 컨셉은 일정 시간 까지 컨베이어 동작 후 정지 하도록 함

                        // 컨베이어 동작
                        nFuncResult = UldConv_Run(false);
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR UNLOADING RUN START", STEP_ELAPSED));
                        }
                    }
                    break;

                case 15:
                    {
                        if (WaitDelay(10000)) return FNC.BUSY;
                    }
                    break;

                case 16:
                    {
                        // 센서 감지 후 추가 컨베이어 동작 시간 
                        long lDelay = ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CONV_ROLLING_TIME].lValue;
                        if (WaitDelay(lDelay)) return FNC.BUSY;
                    }
                    break;

                case 18:
                    {
                        // 컨베이어 정지
                        nFuncResult = UldConv_Stop();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            m_bSafetyConvStart = false;
                            m_bSafetyConvStop = false;
                            SeqHistory(string.Format("ELAPSED, {0}, {1}", "LOAD CONVEYOR UNLOADING RUN STOP", STEP_ELAPSED));
                        }
                    }
                    break;

                case 20:
                    {
                        TTDF.SetTact((int)TTDF.CYCLE_NAME.MGZ_LD_CONV_UNLOADING_MOVE, false);

                        return FNC.SUCCESS;
                    }

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
