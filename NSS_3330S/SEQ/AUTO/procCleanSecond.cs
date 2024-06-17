using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSSU_3400.SEQ.CYCLE;

namespace NSSU_3400.SEQ.AUTO
{
    class procCleanSecond : SeqBase
    {
        //수세기 전용 [2022.05.05 작성자 : 홍은표]
        int m_nCleanUltraDryNum = 0;

        cycCleaning cycleFunction = null;
        public procCleanSecond(int nSeqID)
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

            switch ((seqCleaning.SEQ_CLEANING_CLEANER_PICKER)m_nSeqNo)
            {
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.NONE:
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondVacCheck = START");
                        }
                        nFuncResult = cycleFunction.SecondVacCheck();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondVacCheck = END");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.INIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondInitAndWait = START");
                        }
                        nFuncResult = cycleFunction.SecondInitAndWait();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondInitAndWait = END");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.INTERLOCK_WAIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("------------Clean Second = START------------");
                        }

                        #region 인터락 체크 구간
                        //유닛 피커 2차 세척존 진입하면 안됨
                        //FLAG 비트는 한곳에서 0번째 배열에서만 확인 ..ex) sCleaning[0] 
                        //UNIT_TO_SECOND_CLEAN procCleanFirst가 완료되면 true가 됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == false)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        #endregion
                    }
                    break;

                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.CHECK_BY_PASS:
                    //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse)
                    //{
                    //    NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.CHECK_BY_PASS_MOVE);
                    //    return;
                    //}
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.CLEAN:
                    {
                        //작성자 홍은표 2022.05.30
                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.CLEANING_CLEAN_PICKER))
                        {
                            if (!LeaveCycle()) return;
                        }
                        if (m_bFirstSeqStep)
                        {
                            //2차세척 인터락 발생
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] = true;

                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondClean = START");
                        }
                        nFuncResult = cycleFunction.SecondClean();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondClean = FINISH");
                            //2차세척 인터락 발생
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING] = false;

                            #region STRIP CHECK
                            if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                            {
                                //procCleanSecond 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                //-----------------------------------------------------------------------

                                NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK);
                                return;
                            }
                            #endregion


                            //1차 초음파 진행 못하게 인터락 발생
                            //2022.05.27 1차 초음파 막는 인터락 시점 변경, 작성자 : 홍은표
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = true;
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.UNIT_PICKER_INTERLOCK_CHECK:
                    {
                        #region STRIP CHECK
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            //procCleanSecond 시퀀스 종료---------------------------------------------
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                            //-----------------------------------------------------------------------

                            NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK);
                            return;
                        }
                        #endregion

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("유닛피커 인터락 시작");
                        }
                        #region 인터락 체크 구간
                        //1차 초음파 진행 중 클리너 피커 진행 못하게 인터락 체크
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START] == true)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        //procCleanDry 시퀀스, 드라이 플라즈마 구간 인터락 체크
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        #endregion

                        
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("유닛피커 인터락 종료");

                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.DRY_CLEAN_TABLE:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SECOND_CLEAN_DRY_MODE].bOptionUse)
                        {
                            if (m_bFirstSeqStep)
                            {
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("SecondCleanDry = START");
                            }
                            nFuncResult = cycleFunction.SecondCleanDry();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("SecondCleanDry = FINISH");

                                #region STRIP CHECK
                                if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                                {
                                    //procCleanSecond 시퀀스 종료---------------------------------------------
                                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                    //-----------------------------------------------------------------------

                                    NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK);
                                    return;
                                }
                                #endregion

                                
                            }
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.CHECK_BEFORE_PICK_UP:
                    {
                        // [2022.05.17] 작성자 홍은표
                        // 1차 초음파 진행중이면 픽업하러 가면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.FIRST_ULTRASONIC_START])
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                        // [2022.05.17] 작성자 홍은표
                        // 2차 세척존에 스트립 정보가 없으면 가면 안됨. //혹시나 해서 넣어놓은 인터락
                        if (!GbVar.Seq.sCleaning[0].Info.IsStrip())
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.SECOND_CLEAN_PICK:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.SECOND_CLEAN_PICK_UP_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPickUp = START");
                        }
                        nFuncResult = cycleFunction.SecondCleanPickUp();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[0].Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[2].Info.IsStrip() == false)
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.INIT);
                                //procCleanSecond 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                //-----------------------------------------------------------------------

                                //2차 세척 클린 픽업 인터락 해제
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = false;
                                return;
                            }

                            ///내려 놓을떄 쉬프트
                            //220507 pjh 데이터 쉬프트 함수 추가
                            //GbVar.Seq.sCleaning[0].DataShiftCleanFlip1ToCleanFlip2();
                            //2차 세정에서 클리너 피커로 데이터 쉬프트
                            GbVar.Seq.sCleaning[0].DataShiftCleanFlip1ToTransfer();
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPickUp = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.DRY_PICK_UP_SECOND_CLEAN:
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SECOND_CLEAN_PICKUP_DRY_MODE].bOptionUse)
                        {
                            if (m_bFirstSeqStep)
                            {
                                m_bFirstSeqStep = false;
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("SecondCleanPickUpDry = START");
                            }
                            nFuncResult = cycleFunction.SecondCleanPickUpDry();
                            if (nFuncResult == FNC.SUCCESS)
                            {
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                    SeqHistory("SecondCleanPickUpDry = FINISH");

                                #region 스트립 체크 
                                if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                                {
                                    //procCleanSecond 시퀀스 종료---------------------------------------------
                                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                    //-----------------------------------------------------------------------

                                    //2차 세척 클린 픽업 인터락 해제
                                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = false;

                                    NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK);
                                    return;
                                }
                                #endregion
                            }
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.SECOND_ULTRA_PLACE:
                    {
                        if (GbVar.Seq.sCleaning[1].Info.IsStrip())
                        {
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.SECOND_ULTRA_PLACE_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPlace = START");
                        }
                        nFuncResult = cycleFunction.SecondUltraPlace();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[1].Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[2].Info.IsStrip() == false )
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.INIT);
                                //procCleanSecond 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                //-----------------------------------------------------------------------

                                //2차 세척 클린 픽업 인터락 해제
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = false;
                                return;
                            }

                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sCleaning[2].DataShiftTransferToCleanFlip2();
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPlace = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.UNIT_PICKER_INTERLOCK_CLEAR:
                    {
                        //클리너피커가 2차 초음파 & 클리너피커 플라즈마 진행 하면 인터락 풀어도됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = false;
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.START_CLEANER_PICKER_PLASMA:
                    {
                        // cycCleaning.cs의 CleanerPickerPlasma 함수 진행
                        //피커 플라즈마 하고 있을때 플래그 살려주면 안됨
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_PICKER_PLASMA_MODE].bOptionUse)
                        {
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = true;
                        }
                        else
                        {
                            
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.ULTRASONIC:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltrasonic = START");
                        }
                        //CLEANER_PICKER_PLASMA 인터락 ON 발생
                        nFuncResult = cycleFunction.SecondUltrasonic();
                        //완료 후 CLEANER_PICKER_PLASMA 인터락 OFF 발생
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltrasonic = FINISH");
                            #region STRIP CHECK
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                //procCleanSecond 시퀀스 종료---------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                                //-----------------------------------------------------------------------

                                NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.VAC_CHECK);
                                if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_PICKER_PLASMA_MODE].bOptionUse)
                                {
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = false;
                                }
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.WAIT_CLEANER_PICKER_PLASMA:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("클리너 피커 인터락 시작");
                        }

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse) break;

                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.CLEANING_CLEAN_PICKER))
                        {
                            if (!LeaveCycle()) return;
                        }

                        // cycCleaning.cs의 CleanerPickerPlasma 함수 완료 시 false됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] == true) return;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("클리너 피커 인터락 종료");                        
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.CHECK_BY_PASS_MOVE:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse)
                        //{
                        //    nFuncResult = cycleFunction.SecondPickUpByPassMove();
                        //}
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_CLEANER_PICKER.FINISH:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("------------Clean Second = FINISH------------");
                    }

                    //procCleanDry가 동작중이면 다음스텝으로 넘기면 안됨
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY])
                    {
                        if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                        {
                            GbVar.mcState.isCycleRun[m_nSeqID] = false;
                            return;
                        }
                        return;
                    }
                    #region 인터락 해제 구간
                    //procCleanDry 시퀀스 시작-----------------------------------------------
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == false)
                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = true;
                    //-----------------------------------------------------------------------

                    //procCleanSecond 시퀀스 종료---------------------------------------------
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] == true)
                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNIT_TO_SECOND_CLEAN] = false;
                    //-----------------------------------------------------------------------

                    #endregion
                    NextSeq((int)seqCleaning.SEQ_CLEANING_CLEANER_PICKER.INIT);
                    return;
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
