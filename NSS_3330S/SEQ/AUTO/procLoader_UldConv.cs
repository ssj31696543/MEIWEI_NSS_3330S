using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procLoader_UldConv : SeqBase
    {
        private int m_nConvNo = 0;
        private bool m_bUnloadSensorSkip = false;
        cycLoader_UldConv cycFunc = null;

        public procLoader_UldConv(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLoaderUldConv;

            cycFunc = new cycLoader_UldConv(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        #region RPOPERTY
        /// <summary>
        /// 매거진 트랜스퍼가 요청하는 인터페이스 신호 
        /// 프로퍼티지만 내부용으로 사용
        /// </summary>
        /// 
        private bool IF_REQ_LOAD
        {
            get
            {
                return GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzUnloading.MGZ_LOAD_CONV_LOADING_REQ];
            }
            set
            {
                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzUnloading.MGZ_LOAD_CONV_LOADING_REQ] = value;
            }
        }

        private bool IF_REQ_UNLOAD
        {
            get
            {
                return GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzUnloading.MGZ_LOAD_CONV_UNLOADING_REQ];
            }
            set
            {
                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzUnloading.MGZ_LOAD_CONV_UNLOADING_REQ] = value;
            }
        }

        /// <summary>
        /// 컨베이어 강제 배출 S/W가 눌러졌을 때 FLAG
        /// </summary>
        private bool MGZ_UNLOAD_PUSH_SW
        {
            get
            {
                return GbVar.Seq.sLdMzLoading.bLdConvMgzUnloadPushSW;
            }

            set
            {
                GbVar.Seq.sLdMzLoading.bLdConvMgzUnloadPushSW = value;
            }
        }


        /// <summary>
        /// 컨베이어 런 상태 설정
        /// </summary>
        private bool IF_STATUS_RUN
        {
            set
            {
                GbVar.Seq.sLoaderUldConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_COMPLETED] = !value;
                GbVar.Seq.sLoaderUldConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN] = value;
            }
        }

        private int CONV_MAG_EMPTY
        {
            get
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_1] == 0
                    && IF_REQ_UNLOAD)
                {
                    return 0;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_1] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_2] == 0
                    && !IF_REQ_UNLOAD)
                {
                    return 1;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_2] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_3] == 0
                    && !IF_REQ_UNLOAD)
                {
                    return 2;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_1] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_2] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._2F_MZ_CHECK_3] == 1)
                {
                    return 3;
                }

                return -1;
            }
        }

        #endregion



        void NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV seqNo)
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

            // 테스트
            //if (!LeaveCycle()) return;

            //return;

            switch ((seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV)m_nSeqNo)
            {
                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.INIT:
                    {
                        // 초기화
                        IF_STATUS_RUN = false;
                    }
                    break;

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.CHECK_CONV_SENSOR:
                    {
                        IF_STATUS_RUN = false;
                        if (MGZ_UNLOAD_PUSH_SW)
                        {
                            //매거진 강제 배출 (2F CONV)
                            SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                            NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                            return;
                        }

                        switch (CONV_MAG_EMPTY)
                        {
                            case 0:
                                SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 1 SENSOR EMPTY - WAIT ELV UNLOADING", "Unload Conveyor", "Start");
                                NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.WAITING_REQ);
                                return;

                            case 1:
                                SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 2 SENSOR EMPTY - UNLOAD MAGAZINE TO 2 SENSOR", "Unload Conveyor", "Start");
                                NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_1);
                                return;

                            case 2:
                                SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 3 SENSOR EMPTY - UNLOAD MAGAZINE TO 3 SENSOR", "Unload Conveyor", "Start");
                                NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                                return;

                            case 3:
                                if (!IF_REQ_UNLOAD)
                                {
                                    nFuncResult = (int)ERDF.E_MGZ_ULD_CONV_UNLOAD_MGZ_FULL;
                                    SetError((int)ERDF.E_MGZ_ULD_CONV_UNLOAD_MGZ_FULL);
                                }
                                return;
                        }
                    }

                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();
                    return;

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.WAITING_REQ:
                    {
                        IF_STATUS_RUN = false;

                        // 요청 대기 구간 , 언로드 우선 처리
                        if (IF_REQ_UNLOAD)
                        {
                            // 배출 요청이 있을 경우 처리

                            m_bUnloadSensorSkip = MGZ_UNLOAD_PUSH_SW; // 배출 S/W 동작시에는 매거진 센서 체크 하지 않기 위함
                            SeqHistory("WAITING_REQ - IF_REQ_UNLOAD || MGZ_UNLOAD_PUSH_SW ", "Load Conveyor", "Start");
                            NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_WAIT);
                            return;
                        }
                        else
                        {
                            //Cycle Stop Check
                            LeaveCycle();
                            return;
                        }
                    }
                    return;

                #region 매거진 로딩
                //case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_LOADING_START:
                //    {
                //        if (m_bFirstSeqStep)
                //        {
                //            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                //            m_bFirstSeqStep = false;
                //        }

                //        nFuncResult = cycFunc.MzLoadingMove();

                //        if(nFuncResult == FNC.SUCCESS)
                //        {
                //            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");
                //        }
                //    }
                //    break;

                //case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_LOADING_END:
                //    {
                //        // 신호 해제
                //        IF_STATUS_RUN = false;
                //        IF_REQ_LOAD = false;
                //        SeqHistory("MAGAZINE_LOADING_END", "Load Conveyor", "End");
                //        NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.FINISH);
                //        return;
                //    }
                //    return;
                #endregion

                #region 매거진 언로딩

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_1:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_UNLOADING_RUN", "Unload Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        IF_STATUS_RUN = true;
                        nFuncResult = cycFunc.MzUnloadingRun_1();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (MGZ_UNLOAD_PUSH_SW)
                            {
                                //매거진 강제 배출 (2F CONV)
                                SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                                return;
                            }

                            switch (CONV_MAG_EMPTY)
                            {
                                case -1:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 1 SENSOR EMPTY - WAIT ELV UNLOADING", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.CHECK_CONV_SENSOR);
                                    return;

                                case 0:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 1 SENSOR EMPTY - WAIT ELV UNLOADING", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.WAITING_REQ);
                                    return;

                                case 1:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 2 SENSOR EMPTY - UNLOAD MAGAZINE TO 2 SENSOR", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_1);
                                    return;

                                case 2:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 3 SENSOR EMPTY - UNLOAD MAGAZINE TO 3 SENSOR", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                                    return;

                                case 3:
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.CHECK_CONV_SENSOR);
                                    return;
                            }
                        }
                    }
                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();
                    return;

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_UNLOADING_RUN", "Unload Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        IF_STATUS_RUN = true;
                        nFuncResult = cycFunc.MzUnloadingRun_2();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (MGZ_UNLOAD_PUSH_SW)
                            {
                                //매거진 강제 배출 (2F CONV)
                                SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                                return;
                            }

                            switch (CONV_MAG_EMPTY)
                            {
                                case -1:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 1 SENSOR EMPTY - WAIT ELV UNLOADING", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.CHECK_CONV_SENSOR);
                                    return;

                                case 0:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 1 SENSOR EMPTY - WAIT ELV UNLOADING", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.WAITING_REQ);
                                    return;

                                case 1:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 2 SENSOR EMPTY - UNLOAD MAGAZINE TO 2 SENSOR", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_1);
                                    return;

                                case 2:
                                    SeqHistory("CHECK_CONV_SENSOR - 2F MAGAZINE 3 SENSOR EMPTY - UNLOAD MAGAZINE TO 3 SENSOR", "Unload Conveyor", "Start");
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_RUN_2);
                                    return;

                                default:
                                    NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.CHECK_CONV_SENSOR);
                                    return;
                            }
                        }
                    }
                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();
                    return;

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_WAIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_UNLOADING_WAIT", "Unload Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        nFuncResult = cycFunc.MgzUldConvUloading_Wait();


                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("MAGAZINE_UNLOADING_WAIT", "Unload Conveyor", "End");
                            break;
                        }
                    }
                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();
                    return;

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.MAGAZINE_UNLOADING_END:
                    {
                        // 신호 해제
                        IF_STATUS_RUN = false;
                        IF_REQ_UNLOAD = false;
                        NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.FINISH);
                    }
                    return;

                #endregion

                case seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.FINISH:
                    {
                        // 강제 배출 모드 완료 시 해당 FLAG 초기화 및 시퀀스 정지 해야 함
                        if(MGZ_UNLOAD_PUSH_SW)
                        {
                            // 시퀀스 정지
                            MGZ_UNLOAD_PUSH_SW = false;
                            GbVar.mcState.isCycleRunReq[m_nSeqID] = false;
                            LeaveCycle();
                            SeqHistory("FINISH - MGZ_UNLOAD_PUSH_SW", "Load Conveyor", "End");
                        }
                        // 초기로 되돌림
                        SeqHistory("FINISH", "Load Conveyor", "End");
                        NextSeq(seqLoader_UldConv.SEQ_LOADER_UNLOAD_CONV.INIT);
                        return;
                    }
                    break;
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
