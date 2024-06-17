using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSSU_3400.SEQ.CYCLE;

namespace NSSU_3400.SEQ.AUTO
{
    class procCleanFirst : SeqBase
    {
        //수세기 전용 [2022.05.05 작성자 : 홍은표]
        int m_nCleanUltraDryNum = 0;

        cycCleaning cycleFunction = null;
        public procCleanFirst(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_nCleanUltraDryNum = nSeqID - (int)SEQ_ID.CLEANING_FIRST;
            m_seqInfo = GbVar.Seq.sCleaning[m_nCleanUltraDryNum];

            cycleFunction = new cycCleaning(nSeqID);
            cycleFunction.SetErrorFunc(SetError);
            cycleFunction.SetAddMsgFunc(SetProcMsgEvent);
            cycleFunction.SetAutoManualMode(true);

            m_cycleInfo = cycleFunction;
        }
        void NextSeq(seqCleaning.SEQ_CLEANING_UNIT_PICKER_1 seqNo)
        {
            NextSeq((int)seqNo);
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            cycleFunction.InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            cycleFunction.InitSeq();
        }


        public override void Run()
        {
            if (!IsAcceptRun()) return;
            if (m_seqInfo != null)
            {
                if (m_seqInfo != GbVar.Seq.sCleaning[m_nCleanUltraDryNum])
                {
                    m_seqInfo = GbVar.Seq.sCleaning[m_nCleanUltraDryNum];
                    m_nSeqNo = GbVar.Seq.sCleaning[m_nCleanUltraDryNum].nCurrentSeqNo;
                }
                if (m_seqInfo.nCurrentSeqNo != m_nSeqNo)
                    m_seqInfo.nCurrentSeqNo = m_nSeqNo;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
                if (CheckCycle() == false) return;
            }
            m_nPreSeqNo = m_nSeqNo;
            System.Threading.Thread.Sleep(10);

            nFuncResult = FNC.SUCCESS;
            switch ((seqCleaning.SEQ_CLEANING_UNIT_PICKER_1)m_nSeqNo)
            {
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.NONE:
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstInitMove = START");
                        }
                        nFuncResult = cycleFunction.FirstInitMove();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //procCleanFirst 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] = false;
                                //-----------------------------------------------------------------------

                                //Unit Picker 대기 인터락 플래그 종료------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] = false;
                                //-----------------------------------------------------------------------
                            }

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstInitMove = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT:
                    {
                        if(m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstInitAndWait = START");
                        }
                        nFuncResult = cycleFunction.FirstInitAndWait();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstInitAndWait = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INTERLOCK_WAIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("------------Clean Fisrt = START------------");
                        }

                        #region 인터락 체크 구간
                        //--------- Unit Picker 대기 인터락 플래그 
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] == false)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        //--------- procCleanFirst 시작
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] == false)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] = true;
                        #endregion
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.CHECK_BY_PASS:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse)
                        //{
                        //    NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.UNLOAD);
                        //    return;
                        //}
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.SCRAP_1:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("UnitPickerScrap1 = START");
                        }
                        nFuncResult = cycleFunction.UnitPickerScrap1();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("UnitPickerScrap1 = FINISH");
                        }
                    }
                    break;

                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.SCRAP_2:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("UnitPickerScrap2 = START");
                        }
                        nFuncResult = cycleFunction.UnitPickerScrap2();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("UnitPickerScrap2 = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.BEFORE_ULTRASONIC_CLEAN_PICKER_INTERLOCK_NEW_ADD:
                    {
                        // 옵션 사용 안하면 기존 처럼 마지막에 1차 초음파 써야하므로 인터락체크 하지않음
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.FIRST_ULTRASONIC_MODE].bOptionUse) break;

                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("1차 초음파 인터락 체크 구간 시작");
                        }
                        //클리너 피커 인터락 체크
                        #region 인터락 체크 구간
                        //유닛 피커 초음파 진행 하면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        // 2022. 05. 30 작성자 : 홍은표 
                        // 택타임 개선을 위해 주석
                        //if (GbVar.Seq.sCleaning[0].Info.IsStrip())
                        //{
                        //    if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        //    {
                        //        GbVar.mcState.isCycleRun[m_nSeqID] = false;
                        //        return;
                        //    }
                        //    return;
                        //}
                        #endregion

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("1차 초음파 인터락 체크 구간 종료");
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.SPONGE_CLEAN:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.FIRST_ULTRASONIC_MODE].bOptionUse)
                        {
                            #region STRIP CHECK
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //1차 초음파 인터락 클리어
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = false;

                                NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                                return;
                            }
                            #endregion

                            if (m_bFirstSeqStep)
                            {
                                //클리너 피커 인터락 진행 못하게 인터락 발생
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == false)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = true;
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstUltrasonic = START");
                            }
                            nFuncResult = cycleFunction.FirstUltrasonic();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                //1차 초음파 인터락 클리어
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = false;

                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstUltrasonic = FINISH");
                            }
                        }
                        else
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SWAP_SPONGE_FIRSTCLEAN_USE].bOptionUse)
                            {
                                // 세척 먼저
                                if (m_bFirstSeqStep)
                                {
                                    m_bFirstSeqStep = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("FirstClean = START");
                                }
                                nFuncResult = cycleFunction.FirstClean();
                                if (nFuncResult == FNC.SUCCESS)
                                {
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("FirstClean = FINISH");
                                }
                            }
                            else
                            {
                                // 스펀지 먼저
                                if (m_bFirstSeqStep)
                                {
                                    m_bFirstSeqStep = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("SpongeClean = START");
                                }
                                nFuncResult = cycleFunction.SpongeClean();
                                if (nFuncResult == FNC.SUCCESS)
                                {
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_MODE_USE].bOptionUse)
                                    {
                                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nUnloadingTableNo] = false;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.CLEAN:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion
                        
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SWAP_SPONGE_FIRSTCLEAN_USE].bOptionUse)
                        {
                            // 스펀지 먼저
                            if (m_bFirstSeqStep)
                            {
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("SpongeClean = START");
                            }
                            nFuncResult = cycleFunction.SpongeClean();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_MODE_USE].bOptionUse)
                                {
                                    GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nUnloadingTableNo] = false;
                                }
                            }
                        }
                        else
                        {
                            // 세척 먼저
                            if (m_bFirstSeqStep)
                            {
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstClean = START");
                            }
                            nFuncResult = cycleFunction.FirstClean();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstClean = FINISH");
                            }
                        }
                        
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.BEFORE_ULTRASONIC_CLEAN_PICKER_INTERLOCK:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.FIRST_ULTRASONIC_MODE].bOptionUse) break;

                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("1차 초음파 인터락 체크 구간 시작");
                        }
                        //클리너 피커 인터락 체크
                        #region 인터락 체크 구간
                        //유닛 피커 초음파 진행 하면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        // 2022. 05. 30 작성자 : 홍은표 
                        // 택타임 개선을 위해 주석
                        //if (GbVar.Seq.sCleaning[0].Info.IsStrip())
                        //{
                        //    if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        //    {
                        //        GbVar.mcState.isCycleRun[m_nSeqID] = false;
                        //        return;
                        //    }
                        //    return;
                        //}
                        #endregion

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("1차 초음파 인터락 체크 구간 종료");
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.ULTRASONIC:
                    {
                        // 1차 초음파 먼저 하는 모드 사용하지 않으면 
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.FIRST_ULTRASONIC_MODE].bOptionUse)
                        {
                            #region STRIP CHECK
                            if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                            {
                                //1차 초음파 인터락 클리어
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = false;

                                NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                                return;
                            }
                            #endregion

                            if (m_bFirstSeqStep)
                            {
                                //클리너 피커 인터락 진행 못하게 인터락 발생
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == false)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = true;
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstUltrasonic = START");
                            }
                            nFuncResult = cycleFunction.FirstUltrasonic();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                //1차 초음파 인터락 클리어
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] = false;

                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("FirstUltrasonic = FINISH");
                            }
                        }
                        else
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SWAP_SPONGE_FIRSTCLEAN_USE].bOptionUse)
                            {
                                // 세척 먼저
                                if (m_bFirstSeqStep)
                                {
                                    m_bFirstSeqStep = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("FirstClean = START");
                                }
                                nFuncResult = cycleFunction.FirstClean();
                                if (nFuncResult == FNC.SUCCESS)
                                {
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("FirstClean = FINISH");
                                }
                            }
                            else
                            {
                                // 스펀지 먼저
                                if (m_bFirstSeqStep)
                                {
                                    m_bFirstSeqStep = false;
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                        SeqHistory("SpongeClean = START");
                                }
                                nFuncResult = cycleFunction.SpongeClean();
                                if (nFuncResult == FNC.SUCCESS)
                                {
                                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SCRAP_MODE_USE].bOptionUse)
                                    {
                                        GbVar.Seq.sUnitTransfer.bSeqIfVar[seqUnitTransfer.LOAD_INTERLOCK_T1 + GbVar.Seq.sUnitTransfer.nUnloadingTableNo] = false;
                                    }
                                }
                            }
                        }

                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.BEFORE_UNLOAD_CHECK_INTERLOCK:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion

                        //드라이 플라즈마 구간 인터락 체크
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] == true || //2차초음파 픽업후 드라이 중에 2차 세척구역에 내려놓을수 없음
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true) //2차 세척 픽업 후 2차 초음파에 안착 전까지 2차 세척구역에 내려놓을수 없음
                        {
                                // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        //2차세척 체크  작성자 : 홍은표 2022.05.30
                        if (GbVar.Seq.sCleaning[0].Info.IsStrip()) return;

                        //2022.05.26 해당 구간이 있으면 오래걸림 작성자 : 홍은표
                        /*
                        // 자재 유무 체크
                        if (GbVar.Seq.sCleaning[1].Info.IsStrip()) return;
                        if (GbVar.Seq.sCleaning[2].Info.IsStrip()) return;
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY]) return;
                        */
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.UNLOAD:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sUnitTransfer.Info.IsStrip())
                        {
                            NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT_MOVE);
                            return;
                        }
                        #endregion
                        
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.SECOND_CLEAN_PLACE_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }
                            m_bFirstSeqStep = false;
                            //2차 세척 구역 언로드 인터락 시작---------------------------------------------
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START] = true;
                            //-----------------------------------------------------------------------

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstUnload = START");
                        }
                        nFuncResult = cycleFunction.FirstUnload();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sUnitTransfer.Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[0].Info.IsStrip() == false)
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT);

                                //procCleanFirst 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] = false;
                                //-----------------------------------------------------------------------

                                //Unit Picker 대기 인터락 플래그 종료------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] = false;
                                //-----------------------------------------------------------------------

                                return;
                            }
                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sUnitTransfer.DataShiftTransferToCleanFlip1();

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstUnload = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.FINISH:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("------------Clean Fisrt = FINISH------------");
                        }

                        #region 인터락 해제 구간
                        //procCleanSecond가 동작중이면 다음스텝으로 넘기면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN])
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        //procCleanSecond 시퀀스 시작--------------------------------------------
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == false)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = true;
                        //-----------------------------------------------------------------------

                        //procCleanFirst 시퀀스 종료---------------------------------------------
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] == true)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_CLEANING] = false;
                        //-----------------------------------------------------------------------

                        //Unit Picker 대기 인터락 플래그 종료------------------------------------
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] == true)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CUTTING_TO_UNIT] = false;
                        //-----------------------------------------------------------------------

                        //2차 세척 구역 언로드 인터락 종료---------------------------------------------
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START] == true)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START] = false;
                        //-----------------------------------------------------------------------
                        #endregion

                        NextSeq(seqCleaning.SEQ_CLEANING_UNIT_PICKER_1.INIT);
                        return;
                    }
                    break;
                default:
                    break;
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
    }
}
