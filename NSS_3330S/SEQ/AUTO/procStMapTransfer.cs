using NSS_3330S.SEQ.CYCLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procStMapTransfer : SeqBase
    {
        cycMapTransfer cycFunc = null;

        public procStMapTransfer(int nSeqID)
        {
            m_nSeqID = nSeqID;
            m_seqInfo = GbVar.Seq.sMapTransfer;

            cycFunc = new cycMapTransfer(nSeqID, 0);
            cycFunc.SetErrorFunc(SetError);
            cycFunc.SetAddMsgFunc(SetProcMsgEvent);
            cycFunc.SetAutoManualMode(true);

            m_cycleInfo = cycFunc;
        }

        void NextSeq(seqMapTransfer.SEQ_SOTER_MAP_TRANSER seqNo)
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

            if (m_seqInfo != null)
            {
                if (m_seqInfo != GbVar.Seq.sMapTransfer)
                {
                    m_seqInfo = GbVar.Seq.sMapTransfer;
                    m_nSeqNo = GbVar.Seq.sMapTransfer.nCurrentSeqNo;
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

            switch ((seqMapTransfer.SEQ_SOTER_MAP_TRANSER)m_nSeqNo)
            {
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.INIT:
                    if (GbVar.LOADER_TEST)
                        return;

                    break;

                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.Z_AXIS_READY_POS_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkZ(POSDF.MAP_PICKER_READY))
                            {
                                break;
                            }
                        }
                        nFuncResult = MovePosMapPkZ(POSDF.MAP_PICKER_READY);
                    }
                    break;

                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.X_AXIS_READY_POS_MOVE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (IsInPosMapPkX(POSDF.MAP_PICKER_READY))
                            {
                                break;
                            }
                        }
                        // [2022.05.24.kmlee] 레디 위치는 클린 피커와의 안전 위치로 설정하고 AIR DRY 위치에서 대기
                        //nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_READY);
                        nFuncResult = MovePosMapPkX(POSDF.MAP_PICKER_READY);
                    }
                    break;

                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.WAIT_DRY_TABLE_LOAD_REQ:
                    break;

                // 드라이 블럭 언로딩 플래그 대기
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.WAIT_UNIT_LOAD_CONFIRM:
                    {
                        if (!LeaveCycle()) return;

                        // 자재 정보가 없을 경우
                        if (!GbVar.Seq.sUnitDry.Info.IsStrip())
                        {
                            GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_RUN] = false;

                            NextSeq(seqMapTransfer.SEQ_SOTER_MAP_TRANSER.INIT);
                            return;
                        }

                        if (GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_UNLOADING_READY] == false ||
                            GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_TOP_DRY_RUN] == true) return;
                        
                        GbVar.Seq.sUnitDry.bSeqIfVar[seqUnitDry.UNIT_UNLOADING_READY] = false;

                        GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_RUN] = true;
                    }
                    break;
                     
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.CYC_UNIT_PICK_UP_FROM_DRY_BLOCK:
                    {
                        if (m_bFirstSeqStep)
                        {
                            if (GbVar.MAP_PICKER_PICK_UP_PAUSE)
                            {
                                //시퀀스 진행 전에만 체크 하기 위해 m_bFirstSeqStep 조건 안에 넣었음
                                // break 안했기 때문에 return 해도 계속 m_bFirstSeqStep은 True
                                LeaveCycle();
                                return;
                            }

                            SeqHistory("Load strip from dryblock", "Map Picker", "Start");
                        }

                        nFuncResult = cycFunc.LoadingDryBlock();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory("Load strip from dryblock", "Map Picker", "Done");

                            GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_RUN] = false;
                            GbVar.Seq.sMapTransfer.bSeqIfVar[seqMapTransfer.MAP_UNIT_LOAD_COMPLETE] = true;

                            GbVar.Seq.sUnitDry.SetProcCycleOutTime((int)MCC.DRY_BLOCK);

                            if (GbVar.Seq.sUnitDry.Info.IsStrip())
                            {
                                SeqHistory("Load strip from dryblock", "Map Picker", "Data Shift");
                                GbVar.Seq.sMapTransfer.DataShiftDryTableToTransfer();
                                GbVar.Seq.sMapTransfer.SetProcCycleInTime((int)MCC.MAP_PICKER);
                            }
                            else
                            {
                                SeqHistory("Load strip from dryblock", "Map Picker", "No Strip. Move Init");
                                NextSeq(seqMapTransfer.SEQ_SOTER_MAP_TRANSER.INIT);
                                return;
                            }
                        }
                    }
                    break;

                // 언로딩할 맵 테이블 확인
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.WAIT_MAP_TABLE_LOADING_READY:
                    {
                        //220505 phj TableSkip 작업
                        if (!LeaveCycle()) return;

                        if (GbVar.Seq.sMapTransfer.Info.IsStrip() == false)
                        {
                            NextSeq(seqMapTransfer.SEQ_SOTER_MAP_TRANSER.INIT);
                            return;
                        }

                        if (GbVar.MAP_PICKER_PLACE_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }

                        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_1_USE].bOptionUse && ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_2_USE].bOptionUse)
                        {
                            if (GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nNextUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] == true)
                            {
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nNextUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] = false;

                                GbVar.Seq.sMapTransfer.nUnloadingTableNo = GbVar.Seq.sMapTransfer.nNextUnloadingTableNo;

                                GbVar.Seq.sMapTransfer.nNextUnloadingTableNo = GbVar.Seq.sMapTransfer.nNextUnloadingTableNo == 0 ? 1 : 0;

                                break;
                            }
                        }
                        else
                        {
                            if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_1_USE].bOptionUse &&
                                !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_2_USE].bOptionUse)
                            {
                                if (GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] == true)
                                {
                                    GbVar.Seq.sMapVisionTable[0].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] = false;
                                    GbVar.Seq.sMapTransfer.nNextUnloadingTableNo = 0;
                                    GbVar.Seq.sMapTransfer.nUnloadingTableNo = GbVar.Seq.sMapTransfer.nNextUnloadingTableNo;

                                    break;
                                }
                            }
                            else if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_1_USE].bOptionUse &&
                                ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MAP_STAGE_2_USE].bOptionUse)
                            {
                                if (GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] == true)
                                {
                                    GbVar.Seq.sMapVisionTable[1].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_READY] = false;
                                    GbVar.Seq.sMapTransfer.nNextUnloadingTableNo = 1;
                                    GbVar.Seq.sMapTransfer.nUnloadingTableNo = GbVar.Seq.sMapTransfer.nNextUnloadingTableNo;

                                    break;
                                }
                            }
                        }
                    }
                    return;

                // 맵 테이블에 언로딩
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.CYC_UNIT_PLACE_TO_MAP_TABLE:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory(string.Format("Unload strip to map table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Start");

                            if (GbVar.Seq.sMapTransfer.nUnloadingTableNo == 1)
                            {
                                if (GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.MAP_TRANSFER_INTERLOCK_START])
                                {
                                    return;
                                }
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] = true;
                            }
                        }

                        nFuncResult = cycFunc.UnloadingMapTable();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            SeqHistory(string.Format("Unload strip to map table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Done");

                            GbVar.Seq.sMapTransfer.SetProcCycleOutTime((int)MCC.MAP_PICKER);

                            if (GbVar.Seq.sMapTransfer.Info.IsStrip())
                            {
                                GbVar.Seq.sMapTransfer.DataShiftTransferToMapStage(GbVar.Seq.sMapTransfer.nUnloadingTableNo);
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].SetProcCycleInTime((int)MCC.VISION_TABLE1 + GbVar.Seq.sMapTransfer.nUnloadingTableNo);

                                IFMgr.Inst.VISION.UpdateHostCodeToMapMainDB(GbVar.Seq.sMapTransfer.nUnloadingTableNo, GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].Info.UnitArr);

                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] = true;
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] = false;

                            }
                            else
                            {
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] = true;
                                GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_START] = false;

                                NextSeq(seqMapTransfer.SEQ_SOTER_MAP_TRANSER.INIT);
                                return;
                            }
                        }
                    }
                    break;

                // 인스펙션 준비 확인
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.WAIT_MAP_TABLE_INSPECTION_READY:
                    {
                        if (!LeaveCycle()) return;

                        if (GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_LOAD_COMPLETE] == true) return;

                        //STRIP_MARK_VISION_LEFT_TABLE 싸이클타임 측정시작
                        //TTDF.SetTact(TTDF.STRIP_MARK_VISION_LEFT_TABLE + GbVar.Seq.sMapTransfer.nUnloadingTableNo, true);

                        // 인스펙션 완료 여부 초기화
                        GbVar.Seq.sMapTransfer.bMapInspDone = false;
                        GbVar.Seq.sMapTransfer.bMapAlignDone = false;
                    }
                    break;

                // 보고
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.REPORT_SUB_UNLOAD:
                    {
                        if (GbVar.TOP_INSP_PAUSE)
                        {
                            LeaveCycle();
                            return;
                        }
                        SeqHistory(string.Format("MES - Sub Unload({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Requset");
                    }
                    break;

                // 상부 얼라인
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.CYC_TOP_ALIGN:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory(string.Format("Top Align table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Start");
                            m_bFirstSeqStep = false;
                        }

                        nFuncResult = cycFunc.TopAlign();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            GbVar.Seq.sMapTransfer.bMapAlignDone = false;

                            SeqHistory(string.Format("Top Align table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Done");
                        }
                    }
                    break;

                // 상부 인스펙션
                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.CYC_MAP_TABLE_INSPECTION:
                    {
                        if (m_bFirstSeqStep)
                        {
                            SeqHistory(string.Format("Inspection map table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Start");
                            m_bFirstSeqStep = false;
                        }

                         nFuncResult = cycFunc.MapVisionInspection();

                        if (nFuncResult == FNC.SUCCESS)
                        {
                            GbVar.Seq.sMapTransfer.bMapInspDone = false;

                            SeqHistory(string.Format("Inspection map table({0})", GbVar.Seq.sMapTransfer.nUnloadingTableNo == 0 ? "Left" : "Right"), "Map Picker", "Done");

                            GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].ProcCycleStart(0);
                            GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_INSPECTION_END] = true;

                            //STRIP_MARK_VISION_LEFT_TABLE 싸이클타임 측정종료
                            //TTDF.SetTact(TTDF.STRIP_MARK_VISION_LEFT_TABLE + GbVar.Seq.sMapTransfer.nUnloadingTableNo, false);
                        }
                    }
                    break;

                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.MAP_TABLE_UNLOAD_SET:
                    {
                        //if (GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nNextUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_UNLOAD_READY] == true) return;
                        if (GbVar.Seq.sMapVisionTable[GbVar.Seq.sMapTransfer.nUnloadingTableNo].bSeqIfVar[seqMapVisionTable.MAP_TABLE_INSPECTION_END] == true) return;
                    }
                    break;

                case seqMapTransfer.SEQ_SOTER_MAP_TRANSER.FINISH:
                    {
                        SeqHistory("Seq finish", "Map Picker", "Finish");
                        NextSeq((int)seqMapTransfer.SEQ_SOTER_MAP_TRANSER.NONE);
                        return;
                    }
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
                GbVar.Seq.sMapTransfer.Info.bIsError = true;

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
