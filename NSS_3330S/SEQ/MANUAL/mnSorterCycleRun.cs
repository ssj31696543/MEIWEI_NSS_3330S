using NSS_3330S.MOTION;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSS_3330S.SEQ.CYCLE;

namespace NSS_3330S.SEQ.MANUAL
{
    public class mnSorterCycleRun : SeqBase
    {
        int m_nStartSeqNo = 0;
        SEQ_ID m_seqID = SEQ_ID.STRIP_TRANSFER;

        bool m_bVacSkip = true;
        object[] m_args = null;

        public cycUnitDry m_cycDryStage = null;
        public cycMapTransfer m_cycMapTransfer = null;
        public cycPkgPickNPlace[] m_cycPnP = new cycPkgPickNPlace[2];
        public cycTrayTransfer m_cycTrayTransfer = null;
        public cycUldElevator[] m_cycUldElevator = new cycUldElevator[5];
        public cycUldTrayTable[] m_cycUldTrayTable = new cycUldTrayTable[2];

        double[] dSearchedCurrPos = new double[5];
        double[] dWorkingReadyPos = new double[5];

        bool IS_VACUUM_CHECK
        {
            get
            {
                if (m_bVacSkip) return false;

                return true;
            }
        }

        public SEQ_ID CURRENT_SEQ_ID
        {
            get { return m_seqID; }
        }

        public mnSorterCycleRun()
        {
            m_cycDryStage = new cycUnitDry();
            m_cycMapTransfer = new cycMapTransfer((int)SEQ_ID.MAP_TRANSFER, 0);
            m_cycPnP[0] = new cycPkgPickNPlace((int)SEQ_ID.PICK_N_PLACE_1, 0);
            m_cycPnP[1] = new cycPkgPickNPlace((int)SEQ_ID.PICK_N_PLACE_2, 1);
            m_cycTrayTransfer = new cycTrayTransfer((int)SEQ_ID.TRAY_TRANSFER);

            m_cycUldElevator[0] = new cycUldElevator((int)SEQ_ID.ULD_ELV_GOOD_1);
            m_cycUldElevator[1] = new cycUldElevator((int)SEQ_ID.ULD_ELV_GOOD_2);
            m_cycUldElevator[2] = new cycUldElevator((int)SEQ_ID.ULD_ELV_REWORK);
            m_cycUldElevator[3] = new cycUldElevator((int)SEQ_ID.ULD_ELV_EMPTY_1);
            m_cycUldElevator[4] = new cycUldElevator((int)SEQ_ID.ULD_ELV_EMPTY_2);

            m_cycUldTrayTable[0] = new cycUldTrayTable((int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE, 0);
            m_cycUldTrayTable[1] = new cycUldTrayTable((int)SEQ_ID.UNLOAD_GD1_TRAY_TABLE, 1);

            m_cycDryStage.SetAutoManualMode(false);
            m_cycMapTransfer.SetAutoManualMode(false);
            m_cycPnP[0].SetAutoManualMode(false);
            m_cycPnP[1].SetAutoManualMode(false);
            m_cycTrayTransfer.SetAutoManualMode(false);
            m_cycUldElevator[0].SetAutoManualMode(false);
            m_cycUldElevator[1].SetAutoManualMode(false);
            m_cycUldElevator[2].SetAutoManualMode(false);
            m_cycUldElevator[3].SetAutoManualMode(false);
            m_cycUldElevator[4].SetAutoManualMode(false);

            m_cycUldTrayTable[0].SetAutoManualMode(false);
            m_cycUldTrayTable[1].SetAutoManualMode(false);

            m_cycDryStage.SetAddMsgFunc(SetProcMsgEvent);
            m_cycMapTransfer.SetAddMsgFunc(SetProcMsgEvent);
            m_cycPnP[0].SetAddMsgFunc(SetProcMsgEvent);
            m_cycPnP[1].SetAddMsgFunc(SetProcMsgEvent);
            m_cycTrayTransfer.SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldElevator[0].SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldElevator[1].SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldElevator[2].SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldElevator[3].SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldElevator[4].SetAddMsgFunc(SetProcMsgEvent);

            m_cycUldTrayTable[0].SetAddMsgFunc(SetProcMsgEvent);
            m_cycUldTrayTable[1].SetAddMsgFunc(SetProcMsgEvent);
        }

        public void SetParam(int nStartSeqNo, params object[] args)
        {
            m_nStartSeqNo = nStartSeqNo;
            m_bVacSkip = !ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.VAC_SENSOR_USE].bOptionUse;
            m_args = args;

            #region Set Parameter
            switch (m_nStartSeqNo)
            {
                case 80:
                case 81:
                    m_cycDryStage.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.DRY_UNIT;
                    break;

                case 90:
                case 91:
                    m_cycDryStage.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.DRY_UNIT;
                    break;

                // UNIT LOADING
                case 100:
                case 101:
                case 102:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // UNIT UNLOADING
                case 110:
                case 111:
                case 112:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // MAP INSPECTION
                case 120:
                case 121:
                case 122:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // MAP INSPECTION
                case 130:
                case 131:
                case 132:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // TOP ALIGN
                case 140:
                case 141:
                case 142:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // TOP ALIGN
                case 163:
                case 164:
                case 165:
                    m_cycMapTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.MAP_TRANSFER;
                    break;

                // PnP1
                case 200:
                case 201:
                case 202:
                    m_cycPnP[0].SetManualModePickUpParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2
                case 210:
                case 211:
                case 212:
                    m_cycPnP[1].SetManualModePickUpParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 220:
                case 221:
                case 222:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 230:
                case 231:
                case 232:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 240:
                case 241:
                case 242:
                    //m_cycPnP[1].SetManualModeParam(m_args);
                    m_cycPnP[0].SetManualModeInspectionParam(m_args);
                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 250:
                case 251:
                case 252:
                    m_cycPnP[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1 - PICK UP sj.shin
                case 260:
                case 261:
                case 262:
                    m_cycPnP[0].SetManualModePickUpParamNew(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2 - PICK UP sj.shin
                case 270:
                case 271:
                case 272:
                    m_cycPnP[1].SetManualModePickUpParamNew(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 300:
                case 301:
                case 302:
                    m_cycPnP[0].SetManualModeInspectionParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2
                case 310:
                case 311:
                case 312:
                    m_cycPnP[1].SetManualModeInspectionParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 320:
                case 321:
                case 322:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2
                case 330:
                case 331:
                case 332:
                    m_cycPnP[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 400:
                case 401:
                case 402:
                    m_cycPnP[0].SetManualModePlaceParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2
                case 410:
                case 411:
                case 412:
                    m_cycPnP[1].SetManualModePlaceParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_2;
                    break;

                // PnP1
                case 420:
                case 421:
                case 422:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 430:
                case 431:
                case 432:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 440:
                case 441:
                case 442:
                    m_cycPnP[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // PnP2
                case 450:
                case 451:
                case 452:
                    m_cycPnP[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 460:
                case 461:
                case 462:
                    m_cycPnP[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                case 470:
                case 471:
                case 472:
                    m_cycPnP[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.PICK_N_PLACE_1;
                    break;

                // TRAY LOADING
                case 500:
                case 501:
                case 502:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                // TRAY UNLOADING
                case 510:
                case 511:
                case 512:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                // TRAY LOADING
                case 600:
                case 601:
                case 602:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                // TRAY UNLOADING
                case 610:
                case 611:
                case 612:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                // TRAY LOADING
                case 700:
                case 701:
                case 702:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                // TRAY UNLOADING
                case 710:
                case 711:
                case 712:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                case 720:
                case 721:
                case 722:
                    m_cycTrayTransfer.SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.TRAY_TRANSFER;
                    break;

                case 800:
                case 900:
                case 1000:
                case 1100:
                case 1200:
                    m_cycUldElevator[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.ULD_ELV_GOOD_1;
                    break;

                case 810:
                case 910:
                case 1010:
                case 1110:
                case 1210:
                    m_cycUldElevator[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.ULD_ELV_GOOD_2;
                    break;

                case 820:
                case 920:
                case 1020:
                case 1120:
                case 1220:
                    m_cycUldElevator[2].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.ULD_ELV_REWORK;
                    break;

                case 830:
                case 930:
                case 1030:
                case 1130:
                case 1230:
                    m_cycUldElevator[3].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.ULD_ELV_EMPTY_1;
                    break;

                case 840:
                case 940:
                case 1040:
                case 1140:
                case 1240:
                    m_cycUldElevator[4].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.ULD_ELV_EMPTY_2;
                    break;

                case 1400:
                    m_cycUldTrayTable[0].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNLOAD_GD1_TRAY_TABLE;
                    break;
                case 1410:
                    m_cycUldTrayTable[1].SetManualModeParam(m_args);

                    m_seqID = SEQ_ID.UNLOAD_GD2_TRAY_TABLE;
                    break;
                case 1500:
                    m_cycUldTrayTable[0].SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.UNLOAD_GD1_TRAY_TABLE;
                    break;
                case 1510:
                    m_cycUldTrayTable[1].SetManualModeParam(m_args);
                    m_seqID = SEQ_ID.UNLOAD_GD1_TRAY_TABLE;
                    break;
            }
            #endregion
        }

        int nHeadNo;
        bool bFDir = true;
        int nPickerNo;
        int nPickerColCount;
        int nTableNo;
        int nTableCount;
        bool PickCamMove = false;
        public void SetParam_ChipPickerMove(int _nHeadNo, int _nPickerNo, int _nPickerColCount, int _nTableNo, int _nTableCount)
        {
            nHeadNo = _nHeadNo;
            nPickerNo = _nPickerNo;
            nPickerColCount = _nPickerColCount;
            nTableNo = _nTableNo;
            nTableCount = _nTableCount;
            PickCamMove = true;
        }

        public override void InitSeq(int nSeq = 0)
        {
            base.InitSeq(nSeq);

            m_cycDryStage.InitSeq();
            m_cycMapTransfer.InitSeq();
            m_cycPnP[0].InitSeq();
            m_cycPnP[1].InitSeq();
            m_cycTrayTransfer.InitSeq();
            m_cycUldElevator[0].InitSeq();
            m_cycUldElevator[1].InitSeq();
            m_cycUldElevator[2].InitSeq();
            m_cycUldElevator[3].InitSeq();
            m_cycUldElevator[4].InitSeq();
        }

        public override void ResetCmd()
        {
            base.ResetCmd();

            m_cycDryStage.InitSeq();
            m_cycMapTransfer.InitSeq();
            m_cycPnP[0].InitSeq();
            m_cycPnP[1].InitSeq();
            m_cycTrayTransfer.InitSeq();
            m_cycUldElevator[0].InitSeq();
            m_cycUldElevator[1].InitSeq();
            m_cycUldElevator[2].InitSeq();
            m_cycUldElevator[3].InitSeq();
            m_cycUldElevator[4].InitSeq();
        }

        public override void Run()
        {
            if (!IsAcceptRun()) return;
            if (FINISH) return;

            nFuncResult = IsCheckAlwaysMonitoring();
            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }

            if (m_nSeqNo != m_nPreSeqNo)
            {
                ResetCmd();
            }
            m_nPreSeqNo = m_nSeqNo;

            nFuncResult = FNC.SUCCESS;

            switch (m_nSeqNo)
            {
                case 0:
                    {
                        NextSeq(m_nStartSeqNo);
                        return;
                    }
                case 32:
                    break;

                // UNIT BOTTOM DRY 
                case 80:
                    m_cycDryStage.InitSeq();
                    NextSeq(81);
                    return;

                case 81:
                    nFuncResult = m_cycDryStage.BtmAirDry();
                    break;

                // UNIT TOP DRY 
                case 90:
                    m_cycDryStage.InitSeq();
                    NextSeq(91);
                    return;

                case 91:
                    nFuncResult = m_cycDryStage.TopAirDry();
                    break;

                // UNIT LOADING
                case 100:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(101);
                    return;

                case 101:
                    nFuncResult = m_cycMapTransfer.LoadingDryBlock();
                    break;
                case 102:
                    break;


                // UNIT UNLOADING
                case 110:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(111);
                    return;

                case 111:
                    //nFuncResult = m_cycMapTransfer.();
                    break;
                case 112:
                    break;

                // MAP UNLOADING
                case 120:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(121);
                    return;

                case 121:
                    nFuncResult = m_cycMapTransfer.UnloadingMapTable();
                    break;
                case 122:
                    break;

                // MAP INSPECTION
                case 130:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(131);
                    return;

                case 131:
                    nFuncResult = m_cycMapTransfer.MapVisionInspection();
                    break;
                case 132:
                    break;

                // TOP ALIGN
                case 140:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(141);
                    return;

                case 141:
                    nFuncResult = m_cycMapTransfer.TopAlign();
                    break;
                case 142:
                    break;


                // AIR DRY
                case 160:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(161);
                    return;

                case 161:
                    nFuncResult = m_cycMapTransfer.AirDry();
                    break;
                case 162:
                    break;

                // AIR KNIFE
                case 163:
                    m_cycMapTransfer.InitSeq();
                    NextSeq(164);
                    return;

                case 164:
                    nFuncResult = m_cycMapTransfer.AirKnife();
                    break;
                case 165:
                    break;

                // Pnp1 PickUp
                //case 200:
                //    m_cycPnP[0].InitSeq();
                //    NextSeq(201);
                //    return;

                //case 201:
                //    nFuncResult = m_cycPnP[0].UnitPickUpOneCycle();
                //    break;
                //case 202:
                //    break;

                //// Pnp2 PickUp
                //case 210:
                //    m_cycPnP[1].InitSeq();
                //    NextSeq(211);
                //    return;

                //case 211:
                //    nFuncResult = m_cycPnP[1].UnitPickUpOneCycle();
                //    break;
                //case 212:
                //    break;
                case 200:
                    m_cycPnP[0].InitSeq();
                    m_cycPnP[1].InitSeq();

                    m_cycPnP[0].CHIP_PICKER_PICK_UP_TABLE_COUNT = 0;
                    m_cycPnP[1].CHIP_PICKER_PICK_UP_TABLE_COUNT = 0;
                    NextSeq(201);
                    return;

                case 201:
                    nFuncResult = m_cycPnP[0].UnitPickUpOneCycle();
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        NextSeq(202);
                        return;
                    }
                    break;

                case 202:
                    nFuncResult = m_cycPnP[1].UnitPickUpOneCycle();
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        NextSeq(210);
                        return;
                    }
                    break;

                case 210:
                    m_cycPnP[0].InitSeq();
                    m_cycPnP[1].InitSeq();

                    m_cycPnP[0].CHIP_PICKER_PLACE_GOOD_TABLE_COUNT = 0;
                    m_cycPnP[1].CHIP_PICKER_PLACE_GOOD_TABLE_COUNT = 0;

                    NextSeq(211);
                    return;

                case 211:
                    nFuncResult = m_cycPnP[0].UnitPlaceOneCycle();
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        NextSeq(212);
                        return;
                    }
                    break;

                case 212:
                    nFuncResult = m_cycPnP[1].UnitPlaceOneCycle();
                    if (FNC.IsSuccess(nFuncResult))
                    {
                        NextSeq(200);
                        return;
                    }
                    break;

                // Pnp1 PickUp
                case 220:
                case 260: //<< sj.shin 추가
                    m_cycPnP[0].InitSeq();
                    NextSeq(221);
                    return;

                case 221:
                    nFuncResult = m_cycPnP[0].FDirPickUp();
                    break;
                case 222:
                    break;

                case 230:
                    m_cycPnP[0].InitSeq();

                   
                    NextSeq(231);
                    return;

                case 231:
                    nFuncResult = m_cycPnP[0].RDirPickUp();
                    break;
                case 232:
                    break;

                // Pnp1 PickUp
                case 240:
                case 270: //<< sj.shin 추가
                    m_cycPnP[1].InitSeq();
                    NextSeq(241);
                    return;

                case 241:
                    nFuncResult = m_cycPnP[1].FDirPickUp();
                    break;
                case 242:
                    break;

                case 250:     
                    m_cycPnP[1].InitSeq();
                    NextSeq(251);
                    return;

                case 251:
                    nFuncResult = m_cycPnP[1].RDirPickUp();
                    break;
                case 252:
                    break;

                // Pnp1 Inspection
                case 300:
                    m_cycPnP[0].InitSeq();
                    NextSeq(301);
                    return;

                case 301:
                    nFuncResult = m_cycPnP[0].InspBallVisionOneCycle();
                    break;
                case 302:
                    break;

                // Pnp2 Inspection
                case 310:
                    m_cycPnP[1].InitSeq();
                    NextSeq(311);
                    return;

                case 311:
                    nFuncResult = m_cycPnP[1].InspBallVisionOneCycle();
                    break;
                case 312:
                    break;

                // Pnp1 Inspection
                case 320:
                    m_cycPnP[0].InitSeq();
                    NextSeq(321);
                    return;

                case 321:
                    nFuncResult = m_cycPnP[0].BtmInspection();
                    break;
                case 322:
                    break;

                // Pnp2 Inspection
                case 330:
                    m_cycPnP[1].InitSeq();
                    NextSeq(331);
                    return;

                case 331:
                    nFuncResult = m_cycPnP[1].BtmInspection();
                    break;
                case 332:
                    break;

                // Pnp1 Inspection
                case 400:
                    m_cycPnP[0].InitSeq();
                    NextSeq(401);
                    return;

                case 401:
                    nFuncResult = m_cycPnP[0].UnitPlaceOneCycle();
                    break;
                case 402:
                    break;

                // Pnp2 Inspection
                case 410:
                    m_cycPnP[1].InitSeq();
                    NextSeq(411);
                    return;

                case 411:
                    nFuncResult = m_cycPnP[1].UnitPlaceOneCycle();
                    break;
                case 412:
                    break;

                // Pnp1 Inspection
                case 420:
                    m_cycPnP[0].InitSeq();
                    NextSeq(421);
                    return;

                case 421:
                    nFuncResult = m_cycPnP[0].PlaceToGoodTrayF();
                    break;
                case 422:
                    break;

                case 430:
                    m_cycPnP[0].InitSeq();
                    NextSeq(431);
                    return;

                case 431:
                    nFuncResult = m_cycPnP[0].PlaceToReworkTray();
                    break;
                case 432:
                    break;

                case 440:
                    m_cycPnP[0].InitSeq();
                    NextSeq(441);
                    return;

                case 441:
                    nFuncResult = m_cycPnP[0].PlaceToRejectBox();
                    break;
                case 442:
                    break;

                // Pnp2 Inspection
                case 450:
                    m_cycPnP[1].InitSeq();
                    NextSeq(451);
                    return;

                case 451:
                    nFuncResult = m_cycPnP[1].PlaceToGoodTrayF();
                    break;
                case 452:
                    break;

                case 460:
                    m_cycPnP[1].InitSeq();
                    NextSeq(461);
                    return;

                case 461:
                    nFuncResult = m_cycPnP[1].PlaceToReworkTray();
                    break;
                case 462:
                    break;

                case 470:
                    m_cycPnP[1].InitSeq();
                    NextSeq(471);
                    return;

                case 471:
                    nFuncResult = m_cycPnP[1].PlaceToRejectBox();
                    break;
                case 472:
                    break;

                // TRAY LOADING
                case 500:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(501);
                    return;

                case 501:
                    nFuncResult = m_cycTrayTransfer.TrayLoadingFromTable();
                    break;
                case 502:
                    break;

                // TRAY UNLOADING
                case 510:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(511);
                    return;

                case 511:
                    nFuncResult = m_cycTrayTransfer.TrayUnloadingToTable();
                    break;
                case 512:
                    break;

                // TRAY LOADING
                case 600:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(601);
                    return;

                case 601:
                    nFuncResult = m_cycTrayTransfer.TrayLoadingFromElv();
                    break;
                case 602:
                    break;

                // TRAY UNLOADING
                case 610:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(611);
                    return;

                case 611:
                    nFuncResult = m_cycTrayTransfer.TrayUnloadingToElv();
                    break;
                case 612:
                    break;

                // TRAY LOADING
                case 700:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(701);
                    return;

                case 701:
                    nFuncResult = m_cycTrayTransfer.TrayLoadingFromElv();
                    break;
                case 702:
                    break;

                // TRAY UNLOADING
                case 710:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(711);
                    return;

                case 711:
                    //nFuncResult = m_cycTrayTransfer.TrayUnloadingToCoverElv();
                    break;
                case 712:
                    break;

                // TRAY UNLOADING
                case 720:
                    m_cycTrayTransfer.InitSeq();
                    NextSeq(721);
                    return;

                case 721:
                    nFuncResult = m_cycTrayTransfer.TrayUnloadingToElv();
                    break;
                case 722:
                    break;



                case 800:
                    m_cycUldElevator[0].InitSeq();
                    NextSeq(801); return;
                case 801:
                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.EMTY_TRAY_1_ELV_Z);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos[0] = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos();
                        //220615 로그 표시
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis Signal Search Pos{1}", 1, dSearchedCurrPos), "Complete");
                        NextSeq(802);
                    }
                    return;
                case 802:
                    // Tray 공급 요청
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT].dValue < dSearchedCurrPos[0])
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 1), "Not Exist");

                        GbVar.Seq.sUldTrayElvEmpty[0].IS_TRAY = false;
                        break;
                    }
                    else
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 1), "Exist");
                        NextSeq(803);
                    }
                    return;
                case 803:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_EMPTY_1, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("EMPTY{0} TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", m_nElvPorNum + 1), STEP_ELAPSED));
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 1), "Complete");
                        NextSeq(804);
                    }
                    return;
                case 804:
                    nFuncResult = m_cycUldElevator[0].TrayIn();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("EMPTY{0} TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", m_nElvPorNum + 1), STEP_ELAPSED));
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 1), "Complete");
                        NextSeq(805);
                    }
                    return;
                case 805:
                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.EMTY_TRAY_1_ELV_Z);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos[0] = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_1_ELV_Z].GetRealPos();
                        //220615 로그 표시
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis Signal Search Pos{1}", 1, dSearchedCurrPos[0]), "Complete");
                        NextSeq(806);
                    }
                    return;

                case 806:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY1_TRAY_ELV_EXIST_CHECK_POS_LIMIT].dValue < dSearchedCurrPos[0])
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 1), "Not Exist");

                        GbVar.Seq.sUldTrayElvEmpty[0].IS_TRAY = false;
                    }
                    else
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 1), "Exist");
                    }
                    break;

                case 810:
                    m_cycUldElevator[1].InitSeq();
                    NextSeq(811); return;
                case 811:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis", 2), "Start");

                        m_bFirstSeqStep = false;
                    }
                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.EMTY_TRAY_2_ELV_Z);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos[1] = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos();
                        //220615 로그 표시
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis Signal Search Pos{1}", 2, dSearchedCurrPos[1]), "Complete");
                        NextSeq(812);
                    }
                    return;
                case 812:
                    // Tray 공급 요청
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT].dValue < dSearchedCurrPos[1])
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 2), "Not Exist");

                        GbVar.Seq.sUldTrayElvEmpty[1].IS_TRAY = false;
                        break;
                    }
                    else
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 2), "Exist");
                        NextSeq(813);
                    }
                    return;
                case 813:
                    if (m_bFirstSeqStep)
                    {
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 1), "Start");

                        m_bFirstSeqStep = false;
                    }

                    nFuncResult = MovePosElvZAxis(MCDF.ELV_TRAY_EMPTY_2, POSDF.TRAY_ELEV_READY, 0.0f);

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("EMPTY{0} TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", m_nElvPorNum + 1), STEP_ELAPSED));
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 2), "Complete");
                        NextSeq(814);
                    }
                    return;

                case 814:
                    nFuncResult = m_cycUldElevator[1].TrayIn();

                    if (nFuncResult == FNC.SUCCESS)
                    {
                        //SeqHistory(string.Format("ELAPSED, {0}, {1}", string.Format("EMPTY{0} TRAY ELEVATOR MOVE TO READY POSITION COMPLETE", m_nElvPorNum + 1), STEP_ELAPSED));
                        SeqHistory("Ready Move", string.Format("Empty Tray Elevator {0}", 1), "Complete");
                        NextSeq(805);
                    }
                    return;

                case 815:
                    nFuncResult = AxisSinalSearch((int)SVDF.AXES.EMTY_TRAY_1_ELV_Z);

                    if (FNC.IsSuccess(nFuncResult))
                    {
                        dSearchedCurrPos[0] = MotionMgr.Inst[(int)SVDF.AXES.EMTY_TRAY_2_ELV_Z].GetRealPos();
                        //220615 로그 표시
                        SeqHistory("Signal Search", string.Format("Empty Tray Elevator {0} Z Axis Signal Search Pos{1}", 2, dSearchedCurrPos[1]), "Complete");
                        NextSeq(816);
                    }
                    return;

                case 816:
                    if (ConfigMgr.Inst.Cfg.itemOptions[(int)OPTNUM.ULD_EMPTY2_TRAY_ELV_EXIST_CHECK_POS_LIMIT].dValue < dSearchedCurrPos[1])
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 2), "Not Exist");

                        GbVar.Seq.sUldTrayElvEmpty[1].IS_TRAY = false;
                    }
                    else
                    {
                        SeqHistory("Check Exist Tray", string.Format("Empty Tray Elevator {0}", 2), "Exist");
                    }
                    break;

                case 820:
                    m_cycUldElevator[2].InitSeq();
                    NextSeq(821); return;
                    break;
                case 821:
                    nFuncResult = m_cycUldElevator[2].TrayIn();
                    break;

                case 830:
                    m_cycUldElevator[3].InitSeq();
                    NextSeq(831); return;
                    break;
                case 831:
                    nFuncResult = m_cycUldElevator[3].TrayIn();
                    break;

                case 840:
                    m_cycUldElevator[4].InitSeq();
                    NextSeq(841); return;
                    break;
                case 841:
                    nFuncResult = m_cycUldElevator[4].TrayIn();
                    break;

                case 900:
                    m_cycUldElevator[0].InitSeq();
                    NextSeq(901); return;
                    break;
                case 901:
                    nFuncResult = m_cycUldElevator[0].TrayOut();
                    break;

                case 910:
                    m_cycUldElevator[1].InitSeq();
                    NextSeq(911); return;
                    break;
                case 911:
                    nFuncResult = m_cycUldElevator[1].TrayOut();
                    break;

                case 920:
                    m_cycUldElevator[2].InitSeq(); 
                    NextSeq(921); return;
                    break;
                case 921:
                    nFuncResult = m_cycUldElevator[2].TrayOut();
                    break;

                case 930:
                    m_cycUldElevator[3].InitSeq();
                    NextSeq(931); return;
                    break;
                case 931:
                    nFuncResult = m_cycUldElevator[3].TrayOut();
                    break;

                case 940:
                    m_cycUldElevator[4].InitSeq();
                    NextSeq(941); return;
                    break;
                case 941:
                    nFuncResult = m_cycUldElevator[4].TrayOut();
                    break;


                case 1000:
                    m_cycUldElevator[0].InitSeq();
                    NextSeq(1001); return;
                    break;
                case 1001:
                    nFuncResult = m_cycUldElevator[0].TrayLoadReady(ref dSearchedCurrPos[0], ref dWorkingReadyPos[0]);
                    break;

                case 1010:
                    m_cycUldElevator[1].InitSeq();
                    NextSeq(1011); return;
                    break;
                case 1011:
                    nFuncResult = m_cycUldElevator[1].TrayLoadReady(ref dSearchedCurrPos[1], ref dWorkingReadyPos[1]);
                    break;

                case 1020:
                    m_cycUldElevator[2].InitSeq();
                    NextSeq(1021); return;
                    break;
                case 1021:
                    nFuncResult = m_cycUldElevator[2].TrayLoadReady(ref dSearchedCurrPos[2], ref dWorkingReadyPos[2]);
                    break;

                case 1030:
                    m_cycUldElevator[3].InitSeq();
                    NextSeq(1031); return;
                    break;
                case 1031:
                    nFuncResult = m_cycUldElevator[3].TrayLoadReady(ref dSearchedCurrPos[3], ref dWorkingReadyPos[3]);
                    break;

                case 1040:
                    m_cycUldElevator[4].InitSeq();
                    NextSeq(1041); return;
                    break;
                case 1041:
                    nFuncResult = m_cycUldElevator[4].TrayLoadReady(ref dSearchedCurrPos[4], ref dWorkingReadyPos[4]);
                    break;


                case 1100:
                    m_cycUldElevator[0].InitSeq();
                    NextSeq(1101); return;
                    break;
                case 1101:
                    nFuncResult = m_cycUldElevator[0].TrayLoad(ref dSearchedCurrPos[0], ref dWorkingReadyPos[0]);
                    break;

                case 1110:
                    m_cycUldElevator[1].InitSeq();
                    NextSeq(1111); return;
                    break;
                case 1111:
                    nFuncResult = m_cycUldElevator[1].TrayLoad(ref dSearchedCurrPos[1], ref dWorkingReadyPos[1]);
                    break;

                case 1120:
                    m_cycUldElevator[2].InitSeq();
                    NextSeq(1121); return;
                    break;
                case 1121:
                    nFuncResult = m_cycUldElevator[2].TrayLoad(ref dSearchedCurrPos[2], ref dWorkingReadyPos[2]);
                    break;

                case 1130:
                    m_cycUldElevator[3].InitSeq();
                    NextSeq(1131); return;
                    break;
                case 1131:
                    nFuncResult = m_cycUldElevator[3].TrayLoad(ref dSearchedCurrPos[3], ref dWorkingReadyPos[3]);
                    break;

                case 1140:
                    m_cycUldElevator[4].InitSeq();
                    NextSeq(1141); return;
                    break;
                case 1141:
                    nFuncResult = m_cycUldElevator[4].TrayLoad(ref dSearchedCurrPos[4], ref dWorkingReadyPos[4]);
                    break;

                case 1200:
                    m_cycUldElevator[0].InitSeq();
                    NextSeq(1201); return;
                    break;
                case 1201:
                    nFuncResult = m_cycUldElevator[0].TrayUnload(ref dSearchedCurrPos[0], ref dWorkingReadyPos[0]);
                    break;

                case 1210:
                    m_cycUldElevator[1].InitSeq();
                    NextSeq(1211); return;
                    break;
                case 1211:
                    nFuncResult = m_cycUldElevator[1].TrayUnload(ref dSearchedCurrPos[1], ref dWorkingReadyPos[1]);
                    break;

                case 1220:
                    m_cycUldElevator[2].InitSeq();
                    NextSeq(1221); return;
                    break;
                case 1221:
                    nFuncResult = m_cycUldElevator[2].TrayUnload(ref dSearchedCurrPos[2], ref dWorkingReadyPos[2]);
                    break;

                case 1230:
                    m_cycUldElevator[3].InitSeq();
                    NextSeq(1231); return;
                    break;
                case 1231:
                    nFuncResult = m_cycUldElevator[3].TrayUnload(ref dSearchedCurrPos[3], ref dWorkingReadyPos[3]);
                    break;

                case 1240:
                    m_cycUldElevator[4].InitSeq();
                    NextSeq(1241); return;
                    break;
                case 1241:
                    nFuncResult = m_cycUldElevator[4].TrayUnload(ref dSearchedCurrPos[4], ref dWorkingReadyPos[4]);
                    break;

                case 1400:
                    m_cycUldTrayTable[0].InitSeq();
                    NextSeq(1401);
                    return;

                case 1401:
                    nFuncResult = m_cycUldTrayTable[0].TrayTalbeChipMeasure();
                    break;
                case 1410:
                    m_cycUldTrayTable[1].InitSeq();
                    NextSeq(1411);
                    return;

                case 1411:
                    nFuncResult = m_cycUldTrayTable[1].TrayTalbeChipMeasure();
                    break;


                case 1500:
                    m_cycUldTrayTable[0].InitSeq();
                    NextSeq(1501);
                    return;
                case 1501:
                    nFuncResult = m_cycUldTrayTable[0].GoodTrayEvasionMove();
                    break;
                case 1510:
                    m_cycUldTrayTable[1].InitSeq();
                    NextSeq(1511);
                    return;
                case 1511:
                    nFuncResult = m_cycUldTrayTable[1].GoodTrayEvasionMove();
                    break;
                default:
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                    break;
            }

            if (FNC.IsErr(nFuncResult))
            {
                SetError(nFuncResult);
                return;
            }
            else if (FNC.IsBusy(nFuncResult)) return;

            // Cycle이 끝나면 종료
            FINISH = true;

            if (m_nSeqNo > 10000)
            {
                System.Diagnostics.Debugger.Break();
                FINISH = true;
                return;
            }
            m_nSeqNo++;
        }
    }
}
