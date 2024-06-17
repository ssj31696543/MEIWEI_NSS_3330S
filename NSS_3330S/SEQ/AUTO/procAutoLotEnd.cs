using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSS_3330S.SEQ.AUTO
{
    public class procAutoLotEnd : SeqBase
    {
        int m_nCheckLotEndCount = 0;

        public procAutoLotEnd(int nSeqID)
        {
            m_nSeqID = nSeqID;
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

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            // 중간에 취소하면 초기화
            if (GbVar.Seq.bIsLotEndRun)
            {
                if (GbVar.Seq.bLotEndReserve == false)
                {
                    NextSeq(0);
                    GbVar.Seq.bIsLotEndRun = false;
                    return;
                }
            }
            switch (m_nSeqNo)
            {
                case 0:
                    {
                        m_nCheckLotEndCount = 0;
                    }
                    break;
                //case 2:
                //    {
                //        // MES 사용 시 대기. 나중에 시나리오에 맞게 처리해야함
                //        if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                //            return;

                //        if (!GbVar.mcState.IsRun() || GbFunc.IsLotEndCondition(false) == false)
                //        {
                //            GbVar.Seq.bLotEndReserve = false;
                //            m_nCheckLotEndCount = 0;
                //            return;
                //        }

                //        m_nCheckLotEndCount++;
                //    }
                //    break;
                //case 4:
                //    {
                //        if (WaitDelay(100)) return;

                //        if (m_nCheckLotEndCount < 20)
                //        {
                //            NextSeq(2);
                //            return;
                //        }

                //        m_nCheckLotEndCount = 0;
                //    }
                //    break;
                //case 6:
                //    {
                //        GbVar.Seq.bLotEndReserve = true;
                //    }
                //    break;
                case 10:
                    {
                        if (GbVar.Seq.bLotEndReserve == false) return;
                        //GbVar.Seq.bLotEndReserve = false;
                        GbVar.bLotEndComp = false;
                        m_nCheckLotEndCount = 0;
                    }
                    break;

                case 11:
                    {
#if _NOTEBOOK
                        if (!GbVar.mcState.IsRun() || GbFunc.IsLotEndCondition(false, false, true) == false)
                        {
                            return;
                        }
                        NextSeq(20);
                        return;
#endif
                        if (!GbVar.mcState.IsRun() || GbFunc.IsLotEndCondition(false) == false)
                        {
                            m_nCheckLotEndCount = 0;
                            return;
                        }
                    }
                    break;
                case 12:
                    {
                        //if (!GbVar.mcState.IsRun() || GbFunc.IsLotEndCondition(false) == false)
                        //{
                        //    m_nCheckLotEndCount = 0;
                        //    GbVar.Seq.bLotEndReserve = false;
                        //    NextSeq(2);
                        //    return;
                        //}

                        m_nCheckLotEndCount++;
                    }
                    break;

                case 14:
                    {
                        if (WaitDelay(100)) return;

                        if (m_nCheckLotEndCount < 20)
                        {
                            NextSeq(12);
                            return;
                        }

                        m_nCheckLotEndCount = 0;
                    }
                    break;
                case 20:
                    {
                        GbVar.Seq.bIsLotEndRun = true;
                    }
                    break;

                // 트레이 배출
                case 21:
                    {
                        // [2022.05.10.kmlee] Tray Table 시퀀스에서 자동으로 처리
                        //// Table Tray load stop
                        //// table tray All unload start
                        //if (MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_CHECK) && GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] == false)
                        //{
                        //    GbVar.Seq.sUldGDTrayTable[0].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                        //}

                        //if (MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_CHECK) && GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] == false)
                        //{
                        //    GbVar.Seq.sUldGDTrayTable[1].bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                        //}

                        //if (MotionMgr.Inst.GetInput(IODF.INPUT.RW_TRAY_CHECK) && GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] == false)
                        //{
                        //    GbVar.Seq.sUldRWTrayTable.bSeqIfVar[seqUldGoodTrayTable.TRAY_UNIT_FULL] = true;
                        //}
                    }
                    break;

                // 트레이 배출 대기 (트레이 테이블 및 트레이 피커)
                case 22:
                    // Table에 있는 트레이 모두 제거하자...
                    // Table에 있는 트레이에 자재가 있으면 Good or Rework, 없으면 
                    // 준비된 Empty port에 배출... 이 때 준비되어 있는 Empty 트레이가 없으며 Stopper Close 후 1번 Port에 놓기
                    
                    // Sensor 뿐 아니라 Seq 변수도 추가하여 비정상 동작을 막자...
                    if (MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_1_CHECK) || 
                        MotionMgr.Inst.GetInput(IODF.INPUT.GD_TRAY_2_CHECK) ||
                        MotionMgr.Inst.GetInput(IODF.INPUT.RW_TRAY_CHECK) ||
                        MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_FRONT_TRAY_CHECK) ||
                        MotionMgr.Inst.GetInput(IODF.INPUT.TRAY_PK_REAR_TRAY_CHECK) ||
                        GbVar.Seq.sUldTrayTransfer.bSeqIfVar[seqUldTrayTransfer.TRAY_UNLOAD_RUN] == true)
                    {
                        return;
                    }

                    // 테이블의 트레이가 모두 배출 될 때까지 대기....
                    break;

                case 24:
                    // Rework Tray 배출 수량이 3개 미만이면 채워 넣어야 한다.
                    // 만약 Bin Sort mode가 아닐 경우 Rework Tray이는 배출하지 않으며 수량도 상관이 없다.
                    break;

                case 30:
                    GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] = true;
                    GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] = true;
                    GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] = true;
                    GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] = true;
                    GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] = true;
                    break;

                case 32:
                    //nFuncResult = MovePosEmptyElvZAxis(POSDF.TRAY_ELEV_READY);
                    //if (nFuncResult == FNC.SUCCESS)
                    //{
                    //}

                    // 엘레베이터 배출까지 대기
                    if (GbVar.Seq.sUldTrayElvGood[0].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] ||
                        GbVar.Seq.sUldTrayElvGood[1].bSeqIfVar[seqUldTrayElvGood.LOT_END_CHECKED] ||
                        GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.LOT_END_CHECKED] ||
                        GbVar.Seq.sUldTrayElvEmpty[0].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED] ||
                        GbVar.Seq.sUldTrayElvEmpty[1].bSeqIfVar[seqUldTrayElvEmpty.LOT_END_CHECKED])
                    {
                        return;
                    }
                    break;

                //case 34:
                //    // OHT 사용 유무에 따라 OHT 배출완료할 때까지 대기할지 사용자에 의해 Tray 배출신호를 기다릴지 정해야 한다.
                //    if (!GbVar.Seq.sUldTrayElvGood.bSeqIfVar[seqUldTrayElvGood.OHT_TRAY_OUT_REQ] || 
                //        !GbVar.Seq.sUldTrayElvRework.bSeqIfVar[seqUldTrayElvRework.OHT_TRAY_OUT_REQ]) return;
                //    break;

                case 40:
                    //IFMgr.Inst.VISION.UpdateLotEnd();
                    break;

                case 42:
                    // [2022.04.06. kmlee] 삭제
                    //IFMgr.Inst.VISION.SetLotStartEndInfo(true);
                    break;
                case 44:
                    // [2022.04.06. kmlee] 삭제
                    //if (IFMgr.Inst.VISION.IsLotStartEndSet != true) return;

                    // [2022.04.06. kmlee] 삭제
                    //IFMgr.Inst.VISION.SetLotStartEndInfo(false);
                    break;
                case 46:
                    // [2022.04.06. kmlee] 삭제
                    //if (IFMgr.Inst.VISION.IsLotStartEndSet != false) return;
                    break;

                case 50:
                    //REPORT_LOT_END
                    //if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.GEM_USE].bOptionUse)
                    //{
                    //    GbVar.Seq.bIsLotEndRun = false;
                    //    NextSeq(80);
                    //    return;
                    //}

                    //IFMgr.Inst.MES.LotEnd.ReportLotEnd(GbVar.lstBinding_HostLot[0].LOT_ID);
                    //IFMgr.Inst.MES.LotEnd.swTimeStamp.Restart();

                    //트레이 언로드 응답이 여기 있는 이유 생각해보기
                    //IFMgr.Inst.MES.CarrierUldRep[(int)MESDF.eTU_PORT.TU01].ReplyTrayUldComp();
                    
                    break;
                        
                case 52:
                    //WAIT_LOT_END_CONFIRM
                    //if (IFMgr.Inst.MES.LotEnd.SEQSTART == true)
                    //{
                    //    if(IFMgr.Inst.MES.LotEnd.swTimeStamp.ElapsedMilliseconds > ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.MES_REPLY_TIME].lValue)
                    //    {
                    //        IFMgr.Inst.MES.LotEnd.swTimeStamp.Stop();

                    //        SetError((int)ERDF.E_MES_LOT_END_AUTO_CONFIRM_RECV_TIMEOUT);
                    //        NextSeq(40);
                    //        return;
                    //    }
                    //    return;
                    //}
                    //IFMgr.Inst.MES.LotEnd.swTimeStamp.Stop();
                    break;

                case 54:
                    //CHECK_LOT_END_CONFIRM
                    //if (IFMgr.Inst.MES.LotEnd.LOT_END_CONFIRM_VALUES[(int)MESDF.eRCMD_LOT_END.CONFIRM_FLAG] == MESDF.CONFIRM_FAIL)
                    //{
                    //    SetError((int)ERDF.E_MES_LOT_END_AUTO_CONFIRM_FAIL);
                    //    NextSeq(0);
                    //    return;
                    //}
                    break;


                case 56:
                    //if (!ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.OHT_USE].bOptionUse)
                    //{
                    //    GbVar.Seq.bIsLotEndRun = false;
                    //    NextSeq(80);
                    //    return;
                    //}
                    break;

                case 60:
                    
                    break;

                case 80:
                    //GbVar.Seq.sUldRjtBoxTable.Clear();
                    //GbVar.Seq.sUldTrayElvGood.Clear();
                    //GbVar.Seq.sUldTrayElvRework.Clear();

                    //GbVar.Seq.sUldGDTrayTable[0].Init();
                    //GbVar.Seq.sUldGDTrayTable[1].Init();
                    //GbVar.Seq.sUldRWTrayTable.Init();

                    //GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE);
                    //GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.UNLOAD_GD2_TRAY_TABLE);
                    //GbSeq.autoRun.SelectInitSeq((int)SEQ_ID.UNLOAD_RW_TRAY_TABLE);

                    
                    //GbVar.Seq.Init();
                    //GbSeq.autoRun.AllInitSeq();

                    GbVar.Seq.sMapVisionTable[0].nMapTablePickUpCount = 0;
                    GbVar.Seq.sMapVisionTable[0].Info.Clear();
                    GbVar.Seq.sMapVisionTable[0].Info.GOOD_UNIT = 0;
                    GbVar.Seq.sMapVisionTable[0].Info.REWORK_UNIT= 0;
                    GbVar.Seq.sMapVisionTable[0].Info.X_MARK_UNIT = 0;

                    GbVar.Seq.sMapVisionTable[1].nMapTablePickUpCount = 0;
                    GbVar.Seq.sMapVisionTable[1].Info.Clear();
                    GbVar.Seq.sMapVisionTable[1].Info.GOOD_UNIT = 0;
                    GbVar.Seq.sMapVisionTable[1].Info.REWORK_UNIT = 0;
                    GbVar.Seq.sMapVisionTable[1].Info.X_MARK_UNIT = 0;

                    GbVar.Seq.sPkgPickNPlace.pInfo[0].ResetPickerHeadInfo();
                    GbVar.Seq.sPkgPickNPlace.pInfo[1].ResetPickerHeadInfo();

                    GbVar.Seq.sUldRWTrayTable.Info.Clear();
                    GbVar.Seq.sUldRWTrayTable.nUnitPlaceCount = 0;

                    GbVar.Seq.sUldGDTrayTable[0].Info.Clear();
                    GbVar.Seq.sUldGDTrayTable[1].Info.Clear();

                    GbVar.Seq.sUldGDTrayTable[0].nUnitPlaceCount = 0;
                    GbVar.Seq.sUldGDTrayTable[1].nUnitPlaceCount = 0;

                    GbVar.Seq.sUldRjtBoxTable.Clear();
                    GbVar.Seq.TrayInit();

                    // 전용 팝업 창
                    SetError((int)ERDF.E_LOT_END_STOP);
                    if (GbVar.lstBinding_HostLot.Count > 0)
                    {
                        GbFunc.WriteEventLog("LOT", String.Format("[LOT] 랏 엔드 LOT ID : {0}-----------------", GbVar.lstBinding_HostLot[MCDF.CURRLOT].LOT_ID));
                    }
                    else
                    {
                        GbFunc.WriteEventLog("LOT", String.Format("[LOT] 랏 엔드-----------------"));
                    }

                    // 전용 팝업 창이 있어서 Cycle Stop으로
                    GbVar.mcState.AllCycleRunReqStop();

                    break;
                case 81:
                    {
                        if (GbVar.mcState.IsRun())
                            return;

                        // 순서 변경
                        GbVar.Seq.bLotEndReserve = false;
                        GbVar.Seq.bIsLotEndRun = false;
                        GbVar.bLotInsert = false;
                        GbVar.bLotEndComp = true;
                    }
                    break;
                case 82:
                    NextSeq(0);
                    return;
            }

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;
            //else if (FNC.IsCycleCheck(nFuncResult))
            //{
            //    LeaveCycle();

            //    Type type = m_seqInfo.GetType();
            //    GbVar.SeqMgr.UpdateSeqObj(type, m_seqInfo);
            //    return;
            //}

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
