using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStMapVisionTable  : SeqBase
    {
        int m_nTableNo = 0;
        int m_nUnitAirDryCount = 0;
        int m_nMapTbAirDryCount = 0;

        public procStMapVisionTable(int nSeqID)
        {
            m_nSeqID = nSeqID;
            
            m_nTableNo = nSeqID - (int)SEQ_ID.MAP_VISION_TABLE_1;
            m_seqInfo = GbVar.Seq.sMapVisionTable[m_nTableNo];
        }

        void NextSeq(seqPkgPickNPlace.SEQ_SOTER_UNIT_PICK_N_PLACE seqNo)
        {
            NextSeq((int)seqNo);
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
            if (!IsAcceptRun()) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_seqInfo != null)
            {
                if (m_seqInfo != GbVar.Seq.sMapVisionTable[m_nTableNo])
                {
                    m_seqInfo = GbVar.Seq.sMapVisionTable[m_nTableNo];
                    m_nSeqNo = GbVar.Seq.sMapVisionTable[m_nTableNo].nCurrentSeqNo;
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

            nFuncResult = FNC.SUCCESS;
            m_bFirstSeqStep = false;

            switch (m_nSeqNo)
            {
                case 0:
                    // Init
                    if (GbVar.LOADER_TEST)
                        return;

                    m_nUnitAirDryCount = 0;
                    m_nMapTbAirDryCount = 0;
                    // Check Map Transfer Z Axis, Chip Picker Z Axes
                    break;

                case 10:
                    // Loading Pos Move
                    nFuncResult = MovePosMapStgY(m_nTableNo, POSDF.MAP_STAGE_STRIP_LOADING);
                    break;
                case 11:
                    // Loading Pos Move
                    nFuncResult = MovePosMapStgT(m_nTableNo, POSDF.MAP_STAGE_STRIP_LOADING);
                    break;


                //SKIP UNIT존재 여부 확인
                case 12:
                    if (GbVar.Seq.sMapVisionTable[m_nTableNo].IsExistSkipUnit == true &&
                        ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.PICKUP_UNIT_SKIP_USAGE].bOptionUse)
                    {
                        GbVar.Seq.sMapVisionTable[m_nTableNo].IsExistSkipUnit = false;
                        nFuncResult = (int)ERDF.E_MAP_STAGE_1_EXIST_SKIP_UNIT + m_nTableNo;
                    }
                    break;

                case 13:
                    GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] = true;
                    break;

                case 14:
                    if (!GbVar.mcState.IsCycleRun(SEQ_ID.MAP_TRANSFER))
                    {
                        if (!LeaveCycle()) return;
                    }

                    if (GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] == true) return;
                    break;

                case 20:
                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] == false) return;

                    SeqHistory("-----------", "Map Stage", "Start !!");
                    break;

                // Align
                case 30:
                    {
                        // 스크랩
                        if (!GbVar.Seq.sMapVisionTable[m_nTableNo].Info.IsStrip())
                        {
                            GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] = false;

                            SeqHistory("Wait inspection finish", "Map Stage", "No Strip Data Detected. Go to 60");
                            NextSeq(60);
                            return;
                        }
                    }
                    break;

                case 32:
                    
                    break;

                case 34:
                    
                    break;

                case 40:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move vision pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosMapStgY(m_nTableNo, POSDF.MAP_STAGE_MAP_VISION_START);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move vision pos", "Map Stage", "Done");

                        GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] = false;
                    }
                    
                    break;

                case 42:
                    // Wait MapVision Inspection Done
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Wait inspection finish", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_INSPECTION_END] == false) return;

                    GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_INSPECTION_END] = false;
                    SeqHistory("Wait inspection finish", "Map Stage", "Done");

                    // 스크랩
                    if (!GbVar.Seq.sMapVisionTable[m_nTableNo].Info.IsStrip())
                    {
                        SeqHistory("Wait inspection finish", "Map Stage", "No Strip Data Detected. Go to 60");
                        NextSeq(60);
                        return;
                    }
                    break;

                //AIR KNIFE 동작 - jy.yang
                case 43:
                    if(!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_AIRKNIFE_USE].bOptionUse)
                    {
                        NextSeq(50);
                        return;
                    }

                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Start pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosMapStgY(m_nTableNo, POSDF.MAP_STAGE_AIR_KNIFE_START);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Start pos.", "Map Stage", "Done");
                    }
                    break;

                case 44:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Blow ON", "Map Stage", "Start");

                        m_nMapTbAirDryCount = 0;
                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MapStageAirKnife(m_nTableNo, true);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Blow ON.", "Map Stage", "Done");
                    }
                    break;

                case 45:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife End pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosMapStgY(m_nTableNo, POSDF.MAP_STAGE_AIR_KNIFE_END);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife End pos.", "Map Stage", "Done");
                    }
                    break;

                case 46:
                    if (m_nMapTbAirDryCount + 1 >= ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_AIRKNIFE_COUNT].lValue)
                    {
                        SeqHistory("Move to Air Knife Work Done.", "Map Stage", "Done");
                        break;
                    }

                    m_nMapTbAirDryCount++;
                    NextSeq(44);
                    return;

                case 47:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to Air Knife Blow OFF", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MapStageAirKnife(m_nTableNo, false);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to Air Knife Blow OFF", "Map Stage", "Done");
                    }
                    break;

                // 다른 테이블 픽업 중인지 확인하여 대기
                case 50:
                    int nCheckTable = m_nTableNo == 0 ? 1 : 0;

                    if (!LeaveCycle()) return;

                    if (GbVar.Seq.sMapVisionTable[nCheckTable].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_START] == true) return;

                    break;

                case 52:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Move to unload pos", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }
                    // Unloading Pos Move (Pnp Start Pos)
                    nFuncResult = MovePosMapStgY(m_nTableNo, POSDF.MAP_STAGE_UNIT_UNLOADING_P1);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory("Move to unload pos", "Map Stage", "Done");

                        GbVar.Seq.sMapVisionTable[m_nTableNo].nMapTablePickUpCount = 0;
                    }
                    break;

                case 54:
                    nFuncResult = MapStageBlow(m_nTableNo, true,0, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse);

                    break;

                case 56:
                    //20211115 CHOH : 배큠 딜레이 추후 수정할 것!!
                    nFuncResult = MapStageBlow(m_nTableNo, false,0, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        SeqHistory(string.Format("ELAPSED, {0}, {1}", "MAP STAGE VACUUM OFF COMPLETE", STEP_ELAPSED));
                    }
                    break;

                //case 54:
                //    //20211130 choh : ODD EVEN BLOW ON
                //    {
                //        // [2022.05.26.kmlee] 2초로 변경
                //        nFuncResult = MapStageVac(m_nTableNo, false, 2000);

                //        if (nFuncResult == FNC.SUCCESS)
                //        {
                //            SeqHistory("Stage Vac Off", "Map Stage", "Done");
                //        }
                //    }
                //    break;

                //case 56:
                //    //20211130 choh : ODD EVEN BLOW OFF
                //    {
                //        nFuncResult = MapStageBlow(m_nTableNo, false, 200);

                //        if (nFuncResult == FNC.SUCCESS)
                //        {
                //            SeqHistory("Stage Blow Off", "Map Stage", "Done");
                //        }
                //    }
                //    break;

                case 58:
                    // Table Vac Off
                    // MapVisionStagePnpReady = true;
                    GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] = true;

                    //STRIP_PICK_AND_PLACE_LEFT_TABLE 싸이클타임 측정시작
                    //TTDF.SetTact(TTDF.STRIP_PICK_AND_PLACE_LEFT_TABLE + m_nTableNo, true);
                    break;

                case 59:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Wait PnP done", "Map Stage", "Start");

                        m_bFirstSeqStep = false;
                    }

                    // 안봐야함
                    //if (!GbVar.mcState.IsCycleRun(SEQ_ID.PICK_N_PLACE_1) && !GbVar.mcState.IsCycleRun(SEQ_ID.PICK_N_PLACE_2))
                    {
                        if (!LeaveCycle()) return;
                    }

                    // 첫번째 Chip 이 후에 VACUUM을 켠 상태로 진행한다
                    //if (GbVar.Seq.sMapVisionTable[m_nTableNo].nMapTablePickUpCount > 0)
                    //{
                    //    int[] OnOut = new int[2];
                    //    OnOut[0] = (int)IODF.OUTPUT.MAP_ST_1_WORK_VAC_PUMP + (6 * m_nTableNo);
                    //    OnOut[1] = (int)IODF.OUTPUT.MAP_ST_1_WORK_VAC + (6 * m_nTableNo);

                    //    if (OnOut[0] == 0 || OnOut[1] == 0)
                    //    {
                    //        MotionMgr.Inst.SetOutput(OnOut[0], true);
                    //        MotionMgr.Inst.SetOutput(OnOut[1], true);
                    //    }
                    //}

                    // Wait Pnp Done
                    if (GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_COMPLETE] == false) return;

                    SeqHistory("Wait PnP done", "Map Stage", "Done");

                    GbVar.Seq.sMapVisionTable[m_nTableNo].Info.Clear();
                    GbVar.Seq.sMapVisionTable[m_nTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_COMPLETE] = false;

                    //STRIP_PICK_AND_PLACE_LEFT_TABLE 싸이클타임 측정종료
                    //TTDF.SetTact(TTDF.STRIP_PICK_AND_PLACE_LEFT_TABLE + m_nTableNo, false);
                    break;
                case 60:
                    {
                        // VACUUM을 끈다
                        nFuncResult = MapStageVac(m_nTableNo, false, 0, !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.DRY_RUN_MODE].bOptionUse);

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Stage Vac Off", "Map Stage", "Done");
                        }
                    }
                    break;

                case 70:
                    // ProcessStop
                    SeqHistory("Seq finish", "Map Stage", "Finish");
                    NextSeq(0);
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
                GbVar.Seq.sMapVisionTable[m_nTableNo].Info.bIsError = true;

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
