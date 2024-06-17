using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NSSU_3400.SEQ.CYCLE;

namespace NSSU_3400.SEQ.AUTO
{
    class procCleanDry : SeqBase
    {
        //수세기 전용 [2022.05.05 작성자 : 홍은표]
        int m_nCleanUltraDryNum = 0;

        cycCleaning cycFunc = null;
        public procCleanDry(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_nCleanUltraDryNum = nSeqID - (int)SEQ_ID.CLEANING_FIRST;
            m_seqInfo = GbVar.Seq.sCleaning[m_nCleanUltraDryNum];

            cycFunc = new cycCleaning(nSeqID);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }
        void NextSeq(seqCleaning.SEQ_CLEANING_UNIT_PICKER_1 seqNo)
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
            //LeaveCycle();
            //return;
            switch ((seqCleaning.SEQ_CLEANING_DRY_PICKER)m_nSeqNo)
            {
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.NONE:
                    {
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("------------DRY PLASMA = START------------");
                        }
                        nFuncResult = cycFunc.DryPlasmaInit();
                    }
                    break;

                case seqCleaning.SEQ_CLEANING_DRY_PICKER.WAIT_PICKER_PLASMA:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("클리너 피커 플라즈마 인터락 시작");
                    }
                    //만일 클리너피커 플라즈마 옵션을 사용하지 않으면 인터락 체크를 할 필요 없다. (하단에 SECOND_CLEAN_TO_DRY(2차초음파완료)으로 인터락 확인)
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_PICKER_PLASMA_MODE].bOptionUse == false)
                    {
                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = false;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("CleanerPickerPlasma = SKIP");
                        break;
                    }
                    //FLAG 비트는 한곳에서 0번째 배열에서만 확인 
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] == false)
                    {
                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.CLEANING_SECOND))
                        {
                            if (!LeaveCycle()) return;
                        }

                        return;
                    }

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                        SeqHistory("클리너 피커 플라즈마 인터락 종료");
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.CHECK_BY_PASS:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse) break;
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.PICKER_PLASMA:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse) break;

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_PICKER_PLASMA_MODE].bOptionUse == false)
                        {
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("CleanerPickerPlasma = SKIP");
                            break;
                        }

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("CleanerPickerPlasma = START");
                        }
                        nFuncResult = cycFunc.CleanerPickerPlasma();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("CleanerPickerPlasma = FINISH");
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_PLASMA] = false;
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.INTERLOCK_CHECK:
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("드라이 플라즈마 인터락 시작");
                    }

                    #region 인터락 체크 구간
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == false)
                    {
                        if (!GbVar.mcState.IsCycleRun(SEQ_ID.CLEANING_SECOND))
                        {
                            LeaveCycle();
                            return;
                        }

                        return;
                    }

                    #endregion

                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                        SeqHistory("드라이 플라즈마 인터락 종료");
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_ULTRASONIC_DRY:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse) break;

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraDry = START");

                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] = true;
                        }
                        nFuncResult = cycFunc.SecondUltraDry();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraDry = FINISH");

                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion

                        }
                    }
                    break;
               
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.WAIT_SECOND_CLEAN:
                    {
                        //2022.05.17 작성자 : 홍은표
                        // 2차 세척 중이면 2차 초음파 픽업하면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING])
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        //2022.05.26 작성자 : 홍은표
                        //2차 세척에 언로드 중이어도 내려 픽업하러 가면 안됨
                        //해당 비트 체크하면 1차 플라즈마 이후에 2차 세척구역 언로딩 중일때 충돌을 방지 할수 있음, 해당 비트는 2차 세척 언로드 완료되어야 false가 됨

                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START])
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        // 2차 초음파에 스트립 정보가 없으면 가면 안됨. //혹시나 해서 넣어놓은 인터락
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
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
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_ULTRASONIC_PICK_UP:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.SECOND_ULTRA_PICK_UP_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }

                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraPickUp = START");
                        }
                        nFuncResult = cycFunc.SecondUltraPickUp();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[1].Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[2].Info.IsStrip() == false )
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------

                                return;
                            }

                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sCleaning[1].DataShiftCleanFlip2ToTransfer();
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraPickUp = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.BOTTOM_DRY:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("PlasmaBottomDry = START");
                     
                        }
                        nFuncResult = cycFunc.PlasmaBottomDry();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("PlasmaBottomDry = FINISH");
                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_ULTRASONIC_PLACE:
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
                        nFuncResult = cycFunc.SecondUltraPlace();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[1].Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[2].Info.IsStrip() == false)
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sCleaning[2].DataShiftTransferToCleanFlip2();
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPlace = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_ULTRASONIC_DRY_2:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse) break;

                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraDry = START");
                        }
                        nFuncResult = cycFunc.SecondUltraDry();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraDry = FINISH");

                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion

                            // 2022 05 23 시퀀스 변경으로 인한 인터락시점 변경
                            // [2022.05.26.kmlee] 위치 변경
                            // 다시 위로 올림 왜냐하면 플라즈마 끝나고 다시 인터락 설정 추가함 [2022. 05. 26] 작성자 홍은표
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] = false;
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.FIRST_PLASMA:
                    {
                        //if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_BY_PASS_USE].bOptionUse) break;

                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.FIRST_PLASMA_MODE].bOptionUse)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstPlasma_ContinousMode = SKIP");
                            break;
                        }
                        //연속 이동으로 변경 [2022.05.05 작성자: 홍은표]
                        //nFuncResult = cycFunc.FirstPlasma();
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstPlasma_ContinousMode = START");
                        }

                        nFuncResult = cycFunc.FirstPlasma_ContinousMode();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstPlasma_ContinousMode = FINISH");

                            // 다시 위로 올림 왜냐하면 플라즈마 끝나고 다시 인터락 설정 추가함 [2022. 05. 26] 작성자 홍은표
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] == false)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] = true;

                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.WAIT_SECOND_CLEAN_2:
                    {
                        //2022.05.17 작성자 : 홍은표
                        // 2차 세척 중이면 2차 초음파 픽업하면 안됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEANING])
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        //2022.05.26 작성자 : 홍은표
                        //2차 세척에 언로드 중이어도 내려 픽업하러 가면 안됨
                        //해당 비트 체크하면 1차 플라즈마 이후에 2차 세척구역 언로딩 중일때 충돌을 방지 할수 있음, 해당 비트는 2차 세척 언로드 완료되어야 false가 됨
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.UNLOAD_SECOND_CLEAN_START])
                        {
                            // 중지 요청 확인
                            if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                            {
                                GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                return;
                            }
                            return;
                        }

                        // 2차 초음파에 스트립 정보가 없으면 가면 안됨. //혹시나 해서 넣어놓은 인터락
                        if (!GbVar.Seq.sCleaning[1].Info.IsStrip())
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
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_ULTRASONIC_PICK_UP_2:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.SECOND_ULTRA_PICK_UP_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraPickUp = START");
                        }
                        nFuncResult = cycFunc.SecondUltraPickUp();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[1].Info.IsStrip() == false &&
                                GbVar.Seq.sCleaning[2].Info.IsStrip() == false)
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------

                                return;
                            }

                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sCleaning[1].DataShiftCleanFlip2ToTransfer();
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondUltraPickUp = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.PICKER_FLIP_AND_DRY_POS_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("DryPickerFlipAndDryMove = START");
                        }
                        nFuncResult = cycFunc.DryPickerFlipAndPlasmaMove();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            //인터락 위치 변경 220508 작성자 : 홍은표
                            //택타임 개선 작업 중
                            ////1차 초음파 진행 인터락 해제
                            //if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] == true)
                            //    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.CLEANER_PICKER_SECOND_CLEAN_PICKUP_START] = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("DryPickerFlipAndDryMove = FINISH");

                            // 2022 05 23 시퀀스 변경으로 인한 인터락시점 변경
                            // [2022.05.26.kmlee] 위치 변경
                            // 유닛피커 클리너 피커 인터락 해제 [2022. 05. 26] 작성자 홍은표
                            // 드라이 구간 인터락 -> 1차 플라즈마 인터락 해제 -> 픽업 드라이, 플레이스 드라이 ->
                            if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] == true)
                                GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_ULTRA_DRY_INTERLOCK_START] = false;

                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
            
             
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SECOND_PLASMA:
                    {
                        if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.SECOND_PLASMA_MODE].bOptionUse)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("FirstPlasma_ContinousMode = SKIP");
                            break;
                        }
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPlasma_Continuous = START");
                        }
                        nFuncResult = cycFunc.SecondPlasma_Continuous();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SecondPlasma_Continuous = FINISH");

                            #region 스트립 체크
                            if (!GbVar.Seq.sCleaning[2].Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.LOAD_DRY_TABLE:
                    {
                        // 아직 Dry Table에 있을 경우 대기
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.DRY_TO_UNLOAD_SORTER] == true) return;
                        // Map Picker가 Loading 중일 경우 대기
                        if (GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_RUN] == true) return;

                        //드라이런 모드 또는 클리너 엔드 모드를 사용하지 않으면 인터락 체크를 해야한다.
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse  == false ||
                            ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_END_USE].bOptionUse == false )
                        {
                            //드라이블럭에 자재가 있으면 내려놓으면 안됨
                            if (GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                if (GbVar.mcState.isCycleRunReq[m_nSeqID] == false)
                                {
                                    GbVar.mcState.isCycleRun[m_nSeqID] = false;
                                    return;
                                }
                                return;
                            }
                        }
                        

                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.DRY_BLOCK_PLACE_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                return;
                            }
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("LoadDryTable = START");
                        }
                        nFuncResult = cycFunc.LoadDryTable();
                        if (nFuncResult == FNC.SUCCESS)
                        {
                            if (GbVar.Seq.sCleaning[2].Info.IsStrip() == false &&
                                GbVar.Seq.sUnitDry.Info.IsStrip() == false)
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }

                            //220507 pjh 데이터 쉬프트 함수 추가
                            GbVar.Seq.sCleaning[2].DataShiftTransferToDryStage();

                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("LoadDryTable = FINISH");
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.SORTER_UNLOAD:
                    {
                        if (m_bFirstSeqStep)
                        {
                            m_bFirstSeqStep = false;
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SorterUnload = START");
                        }
                        nFuncResult = cycFunc.SorterUnload();
                        if(nFuncResult == FNC.SUCCESS)
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                                SeqHistory("SorterUnload = FINISH");
                            #region 스트립 체크
                            if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                                //procCleanDry 시퀀스 종료 -----------------------------------------------
                                if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                                    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;
                                // -----------------------------------------------------------------------
                                return;
                            }
                            #endregion
                        }
                    }
                    break;
                case seqCleaning.SEQ_CLEANING_DRY_PICKER.FINISH:
                    #region 인터락 해제구간
                    
                    if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] == true)
                        GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.SECOND_CLEAN_TO_DRY] = false;

                    //픽업후 초기화로변경
                    //if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.DRY_PLASMA_START] == true)
                    //    GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.DRY_PLASMA_START] = false;
                    
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_END_USE].bOptionUse)
                    {
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_END_USE].bOptionUse)
                        {
                            GbFunc.StripScrap(STRIP_MDL.UNIT_DRY);
                        }

                        NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                        //nFuncResult = (int)ERDF.E_CLEAN_END;
                        return;
                    }
                    else
                    {
                        if (GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.DRY_TO_UNLOAD_SORTER] == false)
                            GbVar.Seq.sCleaning[0].bSeqIfVar[seqCleaning.DRY_TO_UNLOAD_SORTER] = true;
                    }

                    #endregion
                    if (m_bFirstSeqStep)
                    {
                        m_bFirstSeqStep = false;
                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.CLEANER_LOG_ANALYSIS].bOptionUse)
                            SeqHistory("------------DRY PLASMA = FINISH------------");
                    }
                    NextSeq((int)seqCleaning.SEQ_CLEANING_DRY_PICKER.INIT);
                    return;
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
