using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procLoader_LdConv : SeqBase
    {
        private int m_nConvNo = 0;
        cycLoader_LdConv cycFunc = null;
        public procLoader_LdConv(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sLoaderLdConv;

            cycFunc = new cycLoader_LdConv(nSeqID);
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
                return GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ];
            }
            set
            {
                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_LOADING_REQ] = value;
            }
        }

        private bool IF_REQ_UNLOAD
        {
            get
            {
                return GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_UNLOADING_REQ];
            }
            set
            {
                GbVar.Seq.sLdMzLoading.bSeqIfVar[seqLdMzLoading.MGZ_LOAD_CONV_UNLOADING_REQ] = value;
            }
        }

        /// <summary>
        /// 컨베이어 강제 투입 S/W가 눌러졌을 때 FLAG
        /// </summary>
        private bool MGZ_LOAD_PUSH_SW
        {
            get
            {
                return GbVar.Seq.sLdMzLoading.bLdConvMgzLoadPushSW;
            }

            set
            {
                GbVar.Seq.sLdMzLoading.bLdConvMgzLoadPushSW = value;
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
                GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_COMPLETED] = !value;
                GbVar.Seq.sLoaderLdConv.bSeqIfVar[seqLoader_LdConv.MGZ_CONV_RUN] = value;
            }
        }


        private int CONV_MAG_EXIST
        {
            get
            {
                if (GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_3] == 1
                    && IF_REQ_LOAD)
                {
                    return 3;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_2] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_3] == 0
                    && !IF_REQ_LOAD)
                {
                    return 2;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_1] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_2] == 0
                    && !IF_REQ_LOAD)
                {
                    return 1;
                }

                if (GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_INPUT_CHECK] == 1
                    && GbVar.GB_INPUT[(int)IODF.INPUT._1F_MZ_CHECK_1] == 0
                    && !IF_REQ_LOAD)
                {
                    return 0;
                }

                return -1;
            }
        }

        #endregion



        void NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV seqNo)
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

            switch ((seqLoader_LdConv.SEQ_LOADER_LOAD_CONV)m_nSeqNo)
            {
                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INIT:
                    {
                        // 초기화
                        IF_STATUS_RUN = false;
                    }
                    break;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.CHECK_CONV_SENSOR:
                    {
                        if(MGZ_LOAD_PUSH_SW)
                        {
                            //매거진 강제 투입
                            //#3 센서 위치까지, 강제로 투입
                            SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                            NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2);
                            return;
                        }
                        else if(MGZ_UNLOAD_PUSH_SW)
                        {
                            //매거진 강제 배출 (1F CONV)
                            SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                            NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.MAGAZINE_UNLOAD);
                            return;
                        }

                        switch (CONV_MAG_EXIST)
                        {
                            case 0:
                                SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK INPUT SENSOR ", "Input Magazine", "Start");
                                NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INPUT_MAGAZINE);
                                return;

                            case 1:
                                SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 1 SENSOR ", "Loading Magazine 1", "Start");
                                NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_1);
                                return;

                            case 2:
                                SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2);
                                return;

                            case 3:
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.USE_MAGAZINE_BARCODE].bOptionUse)
                                {
                                    SeqHistory("MAGAZINE_READING_BCR", "Load Conveyor", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.READING_MAGAZINE_BCR);
                                }
                                else
                                {
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 3 SENSOR ", "Waiting Req", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ);
                                }
                                return;
                        }
                    }

                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();

                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.CHECK_CONV_SENSOR);
                    return;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INPUT_MAGAZINE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        IF_STATUS_RUN = true;
                        nFuncResult = cycFunc.Mz_Input_Conveyor();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            IF_STATUS_RUN = false;

                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");

                            switch (CONV_MAG_EXIST)
                            {
                                case 0:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK INPUT SENSOR ", "Input Magazine", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INPUT_MAGAZINE);
                                    return;

                                case 1:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 1 SENSOR ", "Loading Magazine 1", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_1);
                                    return;

                                case 2:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2);
                                    return;

                                case 3:
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.USE_MAGAZINE_BARCODE].bOptionUse)
                                    {
                                        SeqHistory("MAGAZINE_READING_BCR", "Load Conveyor", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.READING_MAGAZINE_BCR);
                                    }
                                    else
                                    {
                                        SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 3 SENSOR ", "Waiting Req", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ);
                                    }
                                    return;
                            }
                        }
                    }
                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();

                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.CHECK_CONV_SENSOR);
                    return;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_1:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        IF_STATUS_RUN = true;
                        nFuncResult = cycFunc.Mz_Loading_Conveyor_1();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            IF_STATUS_RUN = false;
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");

                            switch (CONV_MAG_EXIST)
                            {
                                case 0:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK INPUT SENSOR ", "Input Magazine", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INPUT_MAGAZINE);
                                    return;

                                case 1:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 1 SENSOR ", "Loading Magazine 1", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_1);
                                    return;

                                case 2:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2);
                                    return;

                                case 3:
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.USE_MAGAZINE_BARCODE].bOptionUse)
                                    {
                                        SeqHistory("MAGAZINE_READING_BCR", "Load Conveyor", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.READING_MAGAZINE_BCR);
                                    }
                                    else
                                    {
                                        SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 3 SENSOR ", "Waiting Req", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ);
                                    }
                                    return;
                            }
                        }
                    }

                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();

                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.CHECK_CONV_SENSOR);
                    return;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        IF_STATUS_RUN = true;
                        nFuncResult = cycFunc.Mz_Loading_Conveyor_2();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            IF_STATUS_RUN = false;
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");

                            if (MGZ_LOAD_PUSH_SW)
                            {
                                NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.FINISH);
                                return;
                            }

                            switch (CONV_MAG_EXIST)
                            {
                                case 0:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK INPUT SENSOR ", "Input Magazine", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INPUT_MAGAZINE);
                                    return;

                                case 1:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 1 SENSOR ", "Loading Magazine 1", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_1);
                                    return;

                                case 2:
                                    SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 2 SENSOR ", "Loading Magazine 2", "Start");
                                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.LOADING_MAGAZINE_2);
                                    return;

                                case 3:
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.USE_MAGAZINE_BARCODE].bOptionUse)
                                    {
                                        SeqHistory("MAGAZINE_READING_BCR", "Load Conveyor", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.READING_MAGAZINE_BCR);
                                    }
                                    else
                                    {
                                        SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 3 SENSOR ", "Waiting Req", "Start");
                                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ);
                                    }
                                    return;

                            }
                        }
                    }

                    if (nFuncResult != FNC.BUSY)
                        LeaveCycle();

                    NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.CHECK_CONV_SENSOR);
                    return;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.READING_MAGAZINE_BCR:
                    {
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");

                            SeqHistory("CHECK_CONV_SENSOR - DETECTED MZ CHECK 3 SENSOR ", "Waiting Req", "Start");
                            NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ);
                            return;
                        }
                    }
                    return;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.WAITING_REQ:
                    {
                        if (IF_REQ_LOAD)
                        {
                            // 투입 요청이 있을 경우 처리
                            SeqHistory("WAITING_REQ - IF_REQ_LOAD ", "Load Conveyor", "Start");
                            NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.MAGAZINE_LOADING_WAIT);
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
                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.MAGAZINE_LOADING_WAIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                            m_bFirstSeqStep = false;
                        }

                        nFuncResult = cycFunc.MgzLdConvLoading_Wait();

                        if(nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");
                        }
                    }
                    break;

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.MAGAZINE_LOADING_END:
                    {
                        // 신호 해제
                        IF_STATUS_RUN = false;
                        IF_REQ_LOAD = false;
                        SeqHistory("MAGAZINE_LOADING_END", "Load Conveyor", "End");
                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.FINISH);
                        return;
                    }
                    return;
                #endregion

                #region 매거진 언로딩

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.MAGAZINE_UNLOAD:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "Start");
                        m_bFirstSeqStep = false;
                    }

                    IF_STATUS_RUN = true;
                    nFuncResult = cycFunc.Mz_Unloading_Conveyor();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        IF_STATUS_RUN = false;
                        SeqHistory("MAGAZINE_LOADING_START", "Load Conveyor", "End");

                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.FINISH);
                        return;
                    }
                    break;

                #endregion

                case seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.FINISH:
                    {
                        // 강제 투입 배출 모드 완료 시 해당 FLAG 초기화 및 시퀀스 정지 해야 함
                        if (MGZ_LOAD_PUSH_SW)
                        {
                            // 시퀀스 정지
                            MGZ_LOAD_PUSH_SW = false;
                            GbVar.mcState.isCycleRunReq[m_nSeqID] = false;
                            LeaveCycle();
                            SeqHistory("FINISH - MGZ_UNLOAD_PUSH_SW", "Load Conveyor", "End");
                        }

                        if (MGZ_UNLOAD_PUSH_SW)
                        {
                            // 시퀀스 정지
                            MGZ_UNLOAD_PUSH_SW = false;
                            GbVar.mcState.isCycleRunReq[m_nSeqID] = false;
                            LeaveCycle();
                            SeqHistory("FINISH - MGZ_UNLOAD_PUSH_SW", "Load Conveyor", "End");
                        }

                        // 초기로 되돌림
                        SeqHistory("FINISH", "Load Conveyor", "End");
                        NextSeq(seqLoader_LdConv.SEQ_LOADER_LOAD_CONV.INIT);
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
