using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procLoader_MzTransfer : SeqBase
    {
        cycLoader_MzTransfer cycFunc = null;

        // 데이터 연결 해야 함
        System.Diagnostics.Stopwatch m_swCheckTime = new System.Diagnostics.Stopwatch();


        public procLoader_MzTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLdMzLoading;

            cycFunc = new cycLoader_MzTransfer(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }



        void NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING seqNo)
        {
            NextSeq((int)seqNo);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            cycFunc.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            cycFunc.InitSeq();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                m_bFirstSeqStep = true;
                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            if (m_seqInfo != null)
            {
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }

            //if (cycFunc.CheckFullMgzUnloading())
            //{
            //    if (m_swCheckTime.IsRunning == false ||
            //        m_swCheckTime.ElapsedMilliseconds >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_MGZ_FULL_CHECK_TIME].nValue)
            //    {
            //        // DryRun 모드가 아닌 경우에 매거진이 FULL이라면 에러 발생
            //        nFuncResult = (int)ERDF.E_MGZ_ULD_CONV_UNLOAD_MGZ_FULL;
            //        SetError(nFuncResult);
            //        m_swCheckTime.Restart();
            //    }
            //}
            //else
            //{
            //    if (m_swCheckTime.IsRunning)
            //    {
            //        m_swCheckTime.Reset();
            //    }
            //}
            switch ((seqLdMzLoading.SEQ_LD_MZ_LOADING)m_nSeqNo)
            {
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT:
                    {
                        if (m_swCheckTime == null)
                        {
                            m_swCheckTime = new System.Diagnostics.Stopwatch();
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CHECK_INTERLOCK:
                    {
                        // 동작 가능 상태인지 인터락 체크

                        // 이미 매거진 픽업 상태일 경우 어떻게 처리할지 ?
                    }
                    break;
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.CHECK_LOAD_STOP:
                    {
                        //220617 LoadStop, LotEnd일 경우 매거진 공급 안함
                        if (GbVar.Seq.sLdMzLoading.bIsLoadStop || GbVar.Seq.bIsLotEndRun || GbVar.Seq.bLotEndReserve)
                        {
                            // Cycle Stop Check
                            LeaveCycle();
                            return;
                        }
                    }
                    break;

                #region 매거진 로딩 조건 반복 확인

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_LOAD_STATUS:
                    {
                        // LOT END 이거나 매거진 언로드 신호가 들어오면 언로드 처리해야 함
                        //if(LOT_START || IF_REQ_UNLOAD)
                        //{
                        //    // 언로드 동작으로 보냄
                        //    NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_UNLOAD_STATUS);
                        //    return;
                        //}

                        // 트랜스퍼가 작업 가능한 컨베이어 확인
                        // 투입 된 매거진이 있는지 확인 후 해당 포트가 소진 될 때까지 사용하고 
                        // 해당 포트에 매거진이 없을 경우 다음 포트로 넘어 감
                        nFuncResult = cycFunc.CheckStatusLoading();
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            // 다음 동작 시작
                            string strLog = string.Format("Check Status Loading");
                            SeqHistory(strLog, "Magazine Transfer", "Start");
                            break;
                        }
                        //else
                        //{
                        //    //2022 09 27 HEP
                        //    //로딩하기전에 매거진 없는 경우 경알람
                        //    //if (!GbVar.Seq.bLotEndReserve)
                        //    //{
                        //    //    SetError((int)ERDF.E_MGZ_LD_ELV_ALL_EXHAUSTED);
                        //    //}
                        //}


                        // Cycle Stop Check
                        LeaveCycle();
                        return;
                    }


                #endregion

                #region 매거진 로딩 
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.LOADING_MAGAIZNE_CONV_REQ:
                    {
                        // 트랜스퍼가 픽업 전 컨베이어에 센서 체크 후 매거진이 없으면 투입 요청  
                        // 이미 매거진이 존재하면 완료 후 넘어 감 

                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("LOADING_MAGAIZNE_CONV_REQ", "Magazine Transfer", "Start");
                            m_bFirstSeqStep = false;
                        }

                        // 기존 코드
                        //nFuncResult = cycFunc.MgzLdConvLoading(Working_LdConv);
                        nFuncResult = cycFunc.MgzLdConvLoading_Req();
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("LOADING_MAGAIZNE_CONV_REQ", "Magazine Transfer", "End");
                        }                    
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.LOADING_MAGAIZNE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("LOADING_MAGAIZNE_CONV_WAIT", "Magazine Transfer", "Start");
                            m_bFirstSeqStep = false;
                        }

                        //// 매거진 투입 동작
                        nFuncResult = cycFunc.MzLoadingMove();

                        if(nFuncResult == FNC.CYCLE_CHECK)
                        {
                            SeqHistory("LOADING_MAGAIZNE", "Magazine Transfer", "CYCLE_CHECK");
                            NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.LOADING_MAGAIZNE_CONV_REQ);
                            return;
                        }
                        else if(FNC.IsErr(nFuncResult))
                        {
                            SetError(nFuncResult);
                            SeqHistory("LOADING_MAGAIZNE", "Magazine Transfer", "SET ERROR");
                            NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.LOADING_MAGAIZNE_CONV_REQ);
                            return;
                        }
                        
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("LOADING_MAGAIZNE", "Magazine Transfer", "End");
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.LOADING_MAGAZINE_TRANSFER_END:
                    {
                        //// 컨베이어에서 미리 다음 매거진을 투입 해놓기 위함
                        //// 투입 요청 후 대기하지 않고 바로 빠져 나옴
                        //if (IsLdConv_Detect())
                        //{
                        //    // 컨베이어에 매거진이 있을 경우만 미리 요청 함
                        //    nFuncResult = cycFunc.MgzLdConvLoading_Req();
                        //}
                        if(GbVar.LOADER_TEST)
                        {
                            NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_UNLOAD_STATUS);
                            return;
                        }
                    }
                    break;
                #endregion

                #region MES 보고
                //MES 관련 추가 하면 됨
                case seqLdMzLoading.SEQ_LD_MZ_LOADING.MES_REPORT_SEND:
                    {

                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.MES_REPORT_WAITING:
                    {

                    }
                    break;
                #endregion

                #region  매거진 클립 오픈 확인 & 매거진 작업 위치로 이동 ( 매거진 공급 )

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.SUPPLY_MAGAZINE_CLIP_CHECK:
                    {
                        // 매거진 클립 체크 사용 시 
                        // 매거진 공급 전 클립 오픈 상태 확인 동작
                        // 그리퍼 & 레일 위치도 맞춰야 함

                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("SUPPLY_MAGAZINE_CLIP_CHECK", "Magazine Transfer", "Start");
                        }

                        nFuncResult = cycFunc.MgzTransferMzClipCheckPos(); 
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("SUPPLY_MAGAZINE_CLIP_CHECK", "Magazine Transfer", "End");
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.SUPPLY_MAGAZINE_START:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("SUPPLY_MAGAZINE_START", "Magazine Transfer", "Start");
                        }

                        // 매거진 공급/작업 위치 이동
                        nFuncResult = cycFunc.MgzTransferSupplyPos();
#if _NOTEBOOK
                        nFuncResult = FNC.SUCCESS;
#endif
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            // 매거진 공급 완료 후 스트립 트랜스퍼에 신호 알림
                            //GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = true;
                            SeqHistory("SUPPLY_MAGAZINE_START", "Magazine Transfer", "End");
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.SUPPLY_MAGAZINE_END:
                    {
                        #region 기존 코드
                        // 매거진 슬롯 작업 완료 시 까지 대기 함
                        //if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END])
                        //{
                        //    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = false;

                        //    if(GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE])
                        //    {
                        //        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE] = false;

                        //        // 클램프하고있는 매거진이 없다면 다시 로드하기 위해서 초기로 되돌림
                        //        NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT);
                        //        return;
                        //    }
                        //    else
                        //    { 
                        //        // 클램프 중인 매거진이 있다면 언로드 
                        //        // 해당 매거진 작업이 완료 됐으니 언로딩 시작
                        //        NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_UNLOAD_STATUS);
                        //        return;
                        //    }
                        //}
                        //else
                        //{
                        //    // Cycle Stop Check
                        //    LeaveCycle();
                        //    return;
                        //}
                        #endregion


                        // 신규 시퀀스 변경
                        // 매거진 로딩 후 자재를 다 소진할때 까지 대기
                        // 대기 중 매거진 클램프 상태 확인 및 에러 발생
                        // 강제 매거진 배출 시그널 확인 후 푸셔 동작이 아닐 경우에만 매거진 배출 시퀀스 진행

                        #region 신규코드 확인 후 사용    
                        if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_STRIP_PUSH_END] == true)
                        {
                            // Pusher 동작 중
                        }
                        else
                        {
                            // Push 동작이 아닐 경우 매거진 에러 체크

                            // 매거진 작업 완료 후 처리
                            if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END])
                            {
                                GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = false;
                                if (GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE])
                                {
                                    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE] = false;
                                    NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT); 
                                    return; // 클램프하고있는 매거진이 없다면 다시 로드하기 위해서 초기로 되돌림
                                }
                                else
                                {
                                    // 클램프 중인 매거진이 있다면 언로드,  해당 매거진 작업이 완료 됐으니 언로딩 시작
                                    NextSeq(seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_UNLOAD_STATUS);
                                    return;
                                }
                            }
                            else
                            {
                                // 매거진 강제 배츨 요청 또는 매거진 알람팝업에서 신규 매거진 투입 요청 시 처리
                                if (GbVar.Seq.sLdMzLoading.bMgzUnloadReq || GbVar.Seq.sLdMzLoading.bMgzPopErrSelectNewMgz) 
                                {
                                    if(GbVar.Seq.sLdMzLoading.bMgzUnloadReq) GbVar.Seq.sLdMzLoading.bMgzUnloadReq = false;
                                    if (GbVar.Seq.sLdMzLoading.bMgzPopErrSelectNewMgz) GbVar.Seq.sLdMzLoading.bMgzPopErrSelectNewMgz = false;

                                    // 매거진 감지 센서는 안들어와있지만 클램프 중이라면 에러 
                                    if(!IsLdElvCheckUnClamp_Output() && IsLdElvCheckSensor_MgzEmpty())
                                    {
                                        //클램프 상태라면 에러
                                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                                        nFuncResult = (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST_POPUP;
                                        SetError(nFuncResult);
                                        return;
                                    }

                                    // 매거진 클램프 및 감지 센서 확인 후 매거진이 있다면 언로드 동작, 없다면 로드 동작 진행
                                    if (IsLdElvCheckUnClamp_Output() && IsLdElvCheckSensor_MgzEmpty())
                                    {
                                        // 매거진 없을 경우 다시 매거진 로딩 시퀀스 진행
                                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE] = true;
                                    }
                                    else // 매거진 있다면
                                    {
                                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE] = false;
                                    }
                                    GbVar.Seq.sStripTransfer.nMgzLoadSlotNum = 0;
                                    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                                    GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_WORK_END] = true;
                                }
                                else
                                {
                                    // 정상적인 경우 매거진 에러 처리

                                    // Dry Run이 아닌 경우에만 센서 체크
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse)
                                    {
                                        // dryRun인 경우
                                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = true;
                                    }
                                    else
                                    {
                                        // 매거진 있는 경우는 언로드 진행 하지 않음
                                        GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_MGZ_DELETE] = false;

                                        if (IsLdElvCheckSensorOn())
                                        {
                                            // 매거진 공급 완료 후 스트립 트랜스퍼에 신호 알림
                                            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = true;
                                        }
                                        else
                                        {
                                            // 매거진 클램프 상태에서 매거진 미감지 에러
                                            GbVar.Seq.sLdMzLotStart.bSeqIfVar[seqLdMzLotStart.MGZ_LOT_START_READY] = false;
                                            nFuncResult = (int)ERDF.E_MGZ_LD_ELEVATOR_MGZ_NOT_EXIST_POPUP;
                                            SetError(nFuncResult);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        // Cycle Stop Check
                        LeaveCycle();
                        return;
                    }
                    return;

                #endregion

                #region 매거진 언로딩 조건 반복 확인

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.WAITING_UNLOAD_STATUS:
                    {
                        // insert 위치만 볼게 아니라 
                        // arrival 및 전체 센서 확인 후 하나라도 남아 있으면 더 진행 해야 함
                        // 테스트 시 투입쪽만 들어와도 다음 포트로 변경해버림
                        nFuncResult = cycFunc.CheckStatusUnloading();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            // 다음 동작 시작
                            string strLog = string.Format("Check Status Unloading" );
                            SeqHistory(strLog, "Magazine Transfer", "Start");
                            break;
                        }
                        //else
                        //{
                        //    if(cycFunc.CheckFullMgzUnloading() && !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse
                        //        &&
                        //       m_swCheckTime.ElapsedMilliseconds >= 60000)
                        //    {
                        //        // DryRun 모드가 아닌 경우에 매거진이 FULL이라면 에러 발생
                        //        nFuncResult = (int)ERDF.E_MGZ_ULD_CONV_UNLOAD_MGZ_FULL;
                        //        SetError(nFuncResult);
                        //        m_swCheckTime.Restart();
                        //        return;
                        //    }
                        //}

                        //Cycle Stop Check
                        LeaveCycle();
                        return;
                    }
                    return;

                #endregion

                #region 매거진 언로딩 

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.UNLOADING_MAGAIZNE_CONV_REQ:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("UNLOADING_MAGAZINE_REQ", "Magazine Transfer", "Start");
                        }

                        // 매거진 언로드
                        nFuncResult = cycFunc.MgzUldConvUnLoading_Req();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("UNLOADING_MAGAZINE_REQ", "Magazine Transfer", "End");
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.UNLOADING_MAGAIZNE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("UNLOADING_MAGAIZNE", "Magazine Transfer", "Start");
                        }

                        // 매거진 언로드
                        nFuncResult = cycFunc.MzUnloadingMove();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_UNLOADING_REQ] = false;
                            SeqHistory("UNLOADING_MAGAIZNE", "Magazine Transfer", "End");
                        }
                    }
                    break;

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.UNLOADING_MAGAZINE_END:
                    {
                        //if (!GbVar.Seq.bLotEndReserve)
                        //{
                        //    SetError((int)ERDF.E_MGZ_LD_ELV_ALL_EXHAUSTED);
                        //}
                    }
                    break;

                #endregion

                case seqLdMzLoading.SEQ_LD_MZ_LOADING.FINISH:
                    SeqHistory("Move to Init", "Uld Mgz Elv", "Finish");
                    NextSeq((int)seqLdMzLoading.SEQ_LD_MZ_LOADING.INIT);
                    return;
            }

            GbVar.mcState.nSeqStep[m_nSeqID] = m_nSeqNo;

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
    }

}
